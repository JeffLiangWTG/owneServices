using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	public abstract class ImportDeclarationSenderTest<T> : TestCaseWithFactory
		where T : ImportDeclarationSender
	{
		public void TestSendMessageATLASVersion10_2()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			if (!ExpectedMessageTypeATLASVersion10_2.IsEmpty)
			{
				var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._102 } };
				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					AssertEDIMessageAndLogbookDetails(ExpectedMessageTypeATLASVersion10_2);
				}
			}
			else
			{
				Assert("ATLAS Version 10.2 Sending not implemented yet for Sending Type", true);
			}
		}

		public void TestSendMessageATLASVersion10_1()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			if (!ExpectedMessageTypeATLASVersion10_1.IsEmpty)
			{
				var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AtlasSystemCode, VersionNumber = ATLASVersionNumberList.Codes._101 } };
				using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
				{
					AssertEDIMessageAndLogbookDetails(ExpectedMessageTypeATLASVersion10_1);
				}
			}
			else
			{
				Assert("ATLAS Version 10.1 Sending not implemented yet for Sending Type", true);
			}
		}

		public void TestAtLeastOneATLASVersionMustBeSpecified()
		{
			Assert("ExpectedMessageTypeATLASVersion10_1 and/or ExpectedMessageTypeATLASVersion10_2 must be specified", !ExpectedMessageTypeATLASVersion10_1.IsEmpty || !ExpectedMessageTypeATLASVersion10_2.IsEmpty);
		}

		protected ImportDeclarationSender ImportDeclarationSender => importDeclarationSender ?? (importDeclarationSender = GetImportDeclarationSender());
		ImportDeclarationSender importDeclarationSender;

		protected abstract T GetImportDeclarationSender();

		protected virtual ZString ExpectedMessageTypeATLASVersion10_2 => ZString.Empty;

		protected virtual ZString ExpectedMessageTypeATLASVersion10_1 => ZString.Empty;

		protected abstract ZString ExpectedMessageSubType { get; }

		protected abstract ZString DeclarationType { get; }

		protected abstract ZString SubStyle { get; }

		protected virtual ZBool ShouldCreateReconEntry => false;

		protected virtual ZBool ShouldResetEntryStatus => false;

		protected virtual ZString ExpectedLogbookRegistrationNumber => ZString.Empty;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OwnerRef = "WTG1234";
			SetupDeclarantForLogbookEORIBranchSuffix();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = DeclarationType;
			entryInstruction.CEI_SubStyle = SubStyle;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			SetupEntryWithCustomsStatus();

			void SetupDeclarantForLogbookEORIBranchSuffix()
			{
				var declarant = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("OHTEST", "EOR1", Core.Constants.CountryCodes.Germany, "EBS1");
				declarant.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "9877700310012345123456000", Core.Constants.CountryCodes.Germany);
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			}

			void SetupEntryWithCustomsStatus()
			{
				entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.REJ;
				entryHeader.MovementReferenceNumberSetter("ATC123456789");

				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL2;
				var entryLine2 = entryHeader.AllEntryLines.AddNew();
				entryLine2.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL3;

				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine2.PK;
			}
		}
		protected CusEntryHeader entryHeader;

		void AssertEDIMessageAndLogbookDetails(ZString expectedMessageType)
		{
			ImportDeclarationSender.Send();

			CombineAssertions(() =>
			{
				AssertEquals("Message count", 1, entryHeader.Messages.Count);
				var message = entryHeader.Messages[0];
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAtlasSystem, message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", Messaging.EDIMessageTypeList.Codes.Import, message.EM_MessageType);
				AssertEquals("EM_ApplicationReference", expectedMessageType, message.EM_ApplicationReference);
				AssertEquals("EM_MessageSubType", ExpectedMessageSubType, message.EM_MessageSubType);
				AssertEquals("LinkedObject", entryHeader.PK, message.EM_LinkedObject.PK);
				AssertEquals("EntryHeader Status", Common.Shared.MessageStatusList.Codes.Sent, entryHeader.CH_Status);
				AssertEquals("LogbookEORIBranchSuffix exists", "EBS1", message.GetLogbookEORIBranchSuffix());
				AssertEquals("LogbookLocalReferenceNumber exists", "WTG1234", message.GetLogbookLocalReferenceNumber());
				AssertEquals("LogbookRegistrationNumber exists", ExpectedLogbookRegistrationNumber, message.GetLogbookRegistrationNumber());
				AssertEquals("LockNumberOfEntryLines", expected: true, entryHeader.LockNumberOfEntryLines);

				AssertEquals("ReconEntry correct for message type", ShouldCreateReconEntry, entryHeader.GetCusReconEntry() != null);

				AssertEntryStatusWasReset();
			});
		}

		void AssertEntryStatusWasReset()
		{
			AssertEquals("Header.CH_EntryStatus", ShouldResetEntryStatus, entryHeader.CH_EntryStatus.IsEmpty);
			AssertEquals("MovementReferenceCusEntryNumber", ShouldResetEntryStatus, CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany) == null);
			AssertEquals("Line.ZG_CustomsStatus", ShouldResetEntryStatus, entryHeader.AllEntryLines.Cast<CusEntryLine>().All(x => x.ZG_CustomsStatus.IsEmpty));
		}
	}
}
