using System;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	abstract class CAUniversalEventMessageProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSetBranchPK()
		{
			var company = Factory.Load<GlbCompany>(declaration.JE_GC);
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BBB";
			branch.GB_BranchName = "Test Branch";
			Factory.Save();

			branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BBB"));
			Message.EM_Status = "QUE";
			Message.EM_GB = branch.PK;
			Factory.Save();
			AssertNotEquals(entryHeader.Branch.PK, Message.EM_GB);

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK)))
			{
				GetMessageProcesser().Process();
				AssertEquals(entryHeader.Branch.PK, Message.EM_GB);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateMessageStatus()
		{
			var processor = GetMessageProcesser();
			SetupRegistryForTest();
			processor.Process();

			var testStatusList = new[] { MessageStatusList.Codes.AwaitingOriginal, MessageStatusList.Codes.AwaitingChange,
				MessageStatusList.Codes.AwaitingReplace, MessageStatusList.Codes.AwaitingDelete };
			AssertEquals(testStatusList.Length, ExpectedStatusList.Length);
			for (var i = 0; i < testStatusList.Length; i++)
			{
				entryHeader.CH_Status = testStatusList[i];
				processor.Process();
				AssertEquals("Expected Message Status", ExpectedStatusList[i], entryHeader.CH_Status);
				AssertEquals("Expected Entry Status", ExpectedEntryStatus, CusEntryNumber.Load(declaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada).CE_EntryStatus);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetEmailRegistryValues_CusEntryHeader()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "DE2";
			newStaff.GS_EmailAddress = "de2@cargowise.com";
			declaration.JE_GS_NKCusAgent = "DE2";
			var (expectedEmail, expectedSubject) = SetupRegistryForTest();

			GetMessageProcesser().Process();

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			var recipient = email.Recipients.Count > 0 ? email.Recipients[0] : email.CCRecipients[0];
			Assert("for system", recipient.IsForSystemCommunication);

			email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == expectedSubject);
			AssertNotNull(email);

			AssertEquals("One recipient", 1, email.Recipients.Count);
			AssertEquals("de2@cargowise.com", email.Recipients[0].Email);
			AssertEquals("One CC recipient", 1, email.CCRecipients.Count);
			AssertEquals(expectedEmail, email.CCRecipients[0].Email);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			ErrorReporter.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEM_MessageInterpretation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;

			helper.CreateNewOrGetExistingCusCodeType(codeType, "CA Notice Reason Code");
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "0001", "Matched.", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "0003", "Cargo Complete.", startDate, endDate);
			Factory.Save();

			GetMessageProcesser().Process();
			var messageInterpretationText = File.ReadAllText(messageInterpretationFilePath + MessageInterpretationFileName);
			AssertMultilineASCIIEquals("MessageInterpretation", RemoveHyperlink(messageInterpretationText), RemoveHyperlink(Message.EM_MessageInterpretation));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAcknowledgementReport()
		{
			declaration.JE_SystemCreateUser = "DE1";
			var (expectedEmail, expectedSubject) = SetupRegistryForTest();

			GetMessageProcesser().Process();
			var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == expectedSubject);
			AssertNotNull(email);
			AssertEquals("One recipient", 1, email.CCRecipients.Count);
			AssertEquals(User.PostMasterUserName, expectedEmail, email.CCRecipients[0].Email);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMailboxDisplayName_EnvironmentBranchShouldBeSetAndComesFromMessageBranch()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var currentDisplayName = Env.Registry.MailboxDisplayName;
			RawDataRegistry.Instance.MailboxDisplayName.SetTemporaryValue(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, currentDisplayName);
			AssertEquals("MailboxDisplayName is from current comapny", currentDisplayName, Env.Registry.MailboxDisplayName);

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DZC";
			company.CompanyName = "DN Test Company";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "DZB";
			branch.GB_BranchName = "DN Test Branch";
			Factory.Save();

			declaration.JE_GB = branch.PK;

			Message.EM_Status = EDIMessageStatusList.Codes.Queued;
			Message.EM_GB = branch.PK;
			Factory.Save();

			var displayNameFromDNCompany = "DN Company Display Name";
			RawDataRegistry.Instance.MailboxDisplayName.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, displayNameFromDNCompany);

			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				AssertEquals("MailboxDisplayName is from DN company", displayNameFromDNCompany, Env.Registry.MailboxDisplayName);
				SetupRegistryForTest();
			}

			GetMessageProcesser().Process();
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email display name should come from DN company", displayNameFromDNCompany, email.FromDisplayName);
		}

		protected virtual (string ExpectedEmail, string ExpectedSubject) SetupRegistryForTest()
		{
			var newGroup2 = Factory.New<GlbGroup>();
			newGroup2.GG_Code = "NG2";

			var newStaff2 = newGroup2.Staff.AddNew();
			newStaff2.GS_Code = "NS2";
			newStaff2.GS_LoginName = "NS2";
			newStaff2.GS_EmailAddress = "ns2@cargowise.com";

			CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgements.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ESG");
			CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newGroup2.PK.ToGuid());
			Factory.Save();
			return ("ns2@cargowise.com", MessageTypeDescription);
		}

		protected virtual string[] ExpectedStatusList => new[] { MessageStatusList.Codes.AwaitingOriginal, MessageStatusList.Codes.AwaitingChange, MessageStatusList.Codes.AwaitingReplace, MessageStatusList.Codes.AwaitingDelete };

		protected virtual string ExpectedEntryStatus => string.Empty;

		protected abstract string UniversalEventFileName { get; }

		protected abstract string MessageInterpretationFileName { get; }

		protected abstract string MessageTypeDescription { get; }

		protected abstract CAUniversalEventMessageProcessor GetMessageProcesser();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected override void SetUp()
		{
			base.SetUp();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "Job001";
			CusEntryNumber.New(declaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada);
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "12345000000011";
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\UniversalEventMessageProcessor\TestFiles\";
			messageInterpretationFilePath = testFilePath + @"MessageInterpretation\";
			universalEventFilePath = testFilePath + @"UniversalEvent\";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "ZZZ", "EDIFACT syntax error", startDate, endDate);
			var code = helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "463", "Transaction Number or Cargo Control Number or Conveyance Reference Number: INVALID STATUS OF REQUEST", startDate, endDate);
			helper.CreateOrGetLanguage("FR", "French");
			helper.CreateNewOrGetExistingCusCodeListLanguage(code, "FR", "Numéro  de transaction ou Numéro de contrôle du fret ou Numéro de référence de moyen de transport.: STATUT DE LA DEMANDE NON VALIDE");
			Factory.Save();
		}

		protected IXmlSessionTracker logger;
		protected JobDeclaration declaration;
		protected CusEntryHeader entryHeader;
		string messageInterpretationFilePath;
		protected string universalEventFilePath;

		UniversalEventMessage message;
		protected UniversalEventMessage Message
		{
			get
			{
				if (message == null)
				{
					message = Factory.New<UniversalEventMessage>();
					message.EM_MessageText = File.ReadAllText(universalEventFilePath + UniversalEventFileName);
				}
				return message;
			}
		}

		protected UniversalEvent UniversalEvent => Message.GetEM_MessageTextReader().Parse<UniversalEvent>();

		string RemoveHyperlink(string inputString) => Regex.Replace(inputString, @"\<a href=.*\>(.*)\</a\>", "$1");
	}
}
