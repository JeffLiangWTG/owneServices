using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.Testing;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.Customs.GB.Chief.CusDec.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using static Enterprise.Customs.GB.CDS.Constants;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSEntryDeclarationMessageProcessor))]
	public class CDSEntryDeclarationMessageProcessorTest : GBAutoSendCustomsMessageProcessorTest
	{
		protected override void SetEntryClearedStatus(CusEntryHeader entry)
		{
			entry.EntryNumber = "123";
			entry.CH_EntryStatus = Constants.ThreeCharFunctionCodes.DeclarationCleared;
		}

		protected override void PrepareDeclaration(BaseJobDeclaration declaration)
		{
			RequestMessageTests.CreateProcedures(declaration.Factory, new ZString[] { "4012123", "4012456", "4012ABC" });

			var dec = (JobDeclaration)declaration;
			dec.JE_MessageType = "IMP";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_CustomsProfile = "DSK";
			var inst = dec.CustomsEntryInstructions.AddNew();
			inst.CEI_Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;  // H1 for CDS (EntryInstruction)
			inst.CEI_SubStyle = "Y";  // H1 for CDS (EntryInstruction)

			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = dec.CompanyPK;
			password.Badge = dec.JE_CustomsProfile;
			password.EORI = dec.DeclarantTraderId;
			password.StatusMessage = "Status Message";
			password.Status = PasswordStatusList.Codes.Valid;
			password.GP_ExpiryDate = new ZDate(2015, 9, 1);
			password.GP_IssueDate = new ZDate(2014, 3, 20);
		}

		protected override void PrepareInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			var line = (JobComInvoiceLine)invoiceLine;
			var dec = line.Declaration;
			line.JI_CEI = dec.CustomsEntryInstructions[0].PK;
			line.JI_Procedure = "4012123";
		}

		protected override void AssertEntryAndMessageResultForEndToEndTest(CusEntryHeader entry)
		{
			CombineAssertions(() =>
			{
				AssertEquals(1, entry.Messages.Count);
				AssertEquals("AWO", entry.CH_Status);
				var originalMessage = entry.Messages[0];
				AssertEquals("NEW", originalMessage.EM_MessageType);
				AssertEquals("", originalMessage.EM_MessageSubType);
				AssertEquals("QUE", originalMessage.EM_Status);
			});
		}

		protected override ZString ExpectedMessageDescription => Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

		protected override void SetUp()
		{
			base.SetUp();
			GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
			{
				new BadgeCodeSetting
				{
					BadgeCode = "DSK",
					RL_PortCode = "GBLBA",
					Direction = "IMP",
					CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW,
					MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk,
					ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services,
				}
			});
			GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new CredentialsSettingCollection
			{
				new CredentialsSetting
				{
					BadgeCode = "DSK",
					Printer = "Location",
					Company = "Role",
					Username = "Username",
					Password = "Password"
				}
			});
		}

		[TestDate(2015, 8, 22)]
		public void TestAutoSendAmendmentMessageByWorkflow()
		{
			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.ZG_Gateway = GatewayList.Codes.CDS;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;
			declaration.JE_MessageSubType = "IM";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "H1";

			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = declaration.CompanyPK;
			password.Badge = declaration.JE_CustomsProfile;
			password.EORI = declaration.DeclarantTraderId;
			password.StatusMessage = "Status Message";
			password.Status = PasswordStatusList.Codes.Valid;
			password.GP_ExpiryDate = new ZDate(2015, 9, 1);
			password.GP_IssueDate = new ZDate(2014, 3, 20);
			password.IsTokenForCDS = true;

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cei.PK;
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			var sender = new CDSMessageSender(decWrapper);
			sender.Send(shutUp);
			AssertEquals(1, entryHeader.Messages.Count);
			AssertEquals(CDSEDIMessageTypeList.Codes.NewDeclaration, entryHeader.Messages[0].EM_MessageType);
			Factory.Save();

			// Make changes for amendment
			declaration.PreviousDocuments.AddNew().CSI_Code = "123";
			declaration.JE_TotalNoOfPacks = 10;
			entryHeader.MovementReferenceNumberSetter("mrn123", ZDateTime.BrettsBirthday);
			entryHeader.CH_EntryStatus = ThreeCharFunctionCodes.DeclarationAccepted;
			entryHeader.Messages[0].EM_Status = EDIMessage.Status.Received;
			Factory.Save();

			IProcessor processor = new CDSEntryDeclarationMessageProcessor(declaration);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);

			var informationNotifications = notifications.GetEventsByType(NotificationType.Information);
			AssertEquals(1, informationNotifications.Length);
			AssertContains("message has been sent to customs for Job", informationNotifications[0].Message);

			AssertEquals(3, entryHeader.Messages.Count);
			AssertEquals(CDSEDIMessageTypeList.Codes.NewAmendment, entryHeader.Messages[1].EM_MessageType);
			AssertEquals(EDIMessage.Status.Acknowledged, entryHeader.Messages[1].EM_Status);
			AssertEquals(CDSEDIMessageTypeList.Codes.AmendDeclaration, entryHeader.Messages[2].EM_MessageType);
			AssertEquals(EDIMessage.Status.Queued, entryHeader.Messages[2].EM_Status);
			AssertXMLContains("<StatementDescription>Automatic amendment</StatementDescription>", entryHeader.Messages[2].EM_MessageText);
			AssertXMLContains("<ChangeReasonCode>32</ChangeReasonCode>", entryHeader.Messages[2].EM_MessageText);
		}
	}
}
