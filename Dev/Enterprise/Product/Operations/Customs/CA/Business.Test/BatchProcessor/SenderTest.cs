using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.BatchProcessor
{
	sealed class SenderTest : BaseInterchangeSenderTest
	{
		public void TestEnvironmentData()
		{
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			string directoryNotSetup = "Data Loading Module output directory hasn't been setup. Please set it up in " + RegistryLocation(CACustomsDataRegistry.Instance.DataLoadingModuleOutputDirectory);
			string directoryNotExists = "Data Loading Module Output directory does not exist : DummyDirectory";
			CACustomsDataRegistry.Instance.DataLoadingModuleOutputDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "");
			Sender.ExecuteBatch();
			string logMessage = new StringCollectionX(Sender.Logger.UserLogStrings).ToString();
			AssertContains("directoryNotSetup", directoryNotSetup, logMessage);
			AssertNotContains("directoryNotExists", directoryNotExists, logMessage);

			CACustomsDataRegistry.Instance.ExportDeclarationActive.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Sender.ExecuteBatch();
			logMessage = new StringCollectionX(Sender.Logger.UserLogStrings).ToString();
			AssertNotContains("directoryNotSetup", directoryNotSetup, logMessage);
			AssertNotContains("directoryNotExists", directoryNotExists, logMessage);

			CACustomsDataRegistry.Instance.ExportDeclarationActive.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Sender.ExecuteBatch();
			logMessage = new StringCollectionX(Sender.Logger.UserLogStrings).ToString();
			AssertNotContains("directoryNotSetup", directoryNotSetup, logMessage);
			AssertNotContains("directoryNotExists", directoryNotExists, logMessage);

			CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Enterprise.Messaging.Business.EDIMessage message = GetNewEDIMessageReadyToSend();
			Factory.Save();
			Sender.ExecuteBatch();

			logMessage = new StringCollectionX(Sender.Logger.UserLogStrings).ToString();
			message.Reload();
			AssertEquals("Message not processed", EDIMessage.Status.Queued, message.EM_Status);
			AssertContains("directoryNotSetup", directoryNotSetup, logMessage);
			AssertNotContains("directoryNotExists", directoryNotExists, logMessage);

			CACustomsDataRegistry.Instance.DataLoadingModuleOutputDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DummyDirectory");
			Sender.ExecuteBatch();

			message.Reload();
			logMessage = new StringCollectionX(Sender.Logger.UserLogStrings).ToString();
			AssertEquals("Message not processed", EDIMessage.Status.Queued, message.EM_Status);
			AssertNotContains("directoryNotSetup", directoryNotSetup, logMessage);
			AssertContains("directoryNotExists", directoryNotExists, logMessage);

			CACustomsDataRegistry.Instance.DataLoadingModuleOutputDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, outputDirectory.DirectoryName);
			Sender.ExecuteBatch();

			message.Reload();
			logMessage = new StringCollectionX(Sender.Logger.UserLogStrings).ToString();
			AssertEquals("Message processed", EDIMessage.Status.Sent, message.EM_Status);
			AssertNotContains("directoryNotSetup", directoryNotSetup, logMessage);
			AssertNotContains("directoryNotExists", directoryNotExists, logMessage);
		}

		[TestDate(2008, 2, 15, 10, 30, 25)]
		public void TestDataNoIsWrittenWhenError()
		{
			EDIInterchange interchange = GetNewEDIInterchangeReadyToSend();
			interchange.EI_InterchangeNum = "123456790";
			Factory.Save();
			string fileName = Path.Combine(outputDirectory.DirectoryName, "123456790_20080215103025.TXT");
			File.WriteAllText(fileName, "Dummy Data");
			using (FileStream fs = File.Open(fileName, FileMode.Append))
			{
				Sender.ExecuteBatch();
			}
			AssertContains("Cannot write to file: " + fileName + "\r\nError: ", new StringCollectionX(Sender.Logger.UserLogStrings).ToString());
			AssertASCIIFileSameAsString(fileName, "Dummy Data");
			interchange.Reload();
			AssertEquals("enterchangeState", EDIInterchange.Status.Failed, interchange.EI_Status);
		}

		[TestDate(2008, 2, 15, 10, 30, 25)]
		public void TestDataIsWrittenToAFileInTheOutputDirectory()
		{
			EDIInterchange interchange = GetNewEDIInterchangeReadyToSend();
			interchange.EI_InterchangeNum = "123456790";
			interchange.EI_BodyText = "PPermit\r\nPPermit2";
			Factory.Save();
			Sender.ExecuteBatch();
			AssertASCIIFileSameAsString(Path.Combine(outputDirectory.DirectoryName, "123456790_20080215103025.TXT"), "PPermit\r\nPPermit2");
		}

		[TestDate(2008, 8, 28, 16, 53, 25)]
		public void TestSendACIandG7Interchanges_Production()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			AssertSendACIandG7Interchanges("UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+080828:1653+2'UNG+GSMCAR+TSITE+SRP+080828:1653+2+UN+D:00A:SUPRPT'ACI Message 1 text 1ACI Message 2 text 2UNE+2+2'UNZ+1+2'",
				"UNA:+.? 'UNB+UNOA:3+DECLIENTID+RCCECECPW+080828:1653+1'UNG+GSMCAR+TSITE+SRP+080828:1653+1+UN+D:00A:SUPRPT'ACI Message 5 text 3UNE+1+1'UNZ+1+1'",
				"UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+080828:1653+1'UNG+GSIMEX+TSITE+EP+080828:1653+1+CC+D:00A:EX1STP'G7 Message 1 text 1G7 Message 2 text 2UNE+2+1'UNZ+1+1'");
		}

		[TestDate(2008, 8, 28, 16, 53, 25)]
		public void TestSendACIandG7Interchanges_Test()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
			AssertSendACIandG7Interchanges("UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSATID+080828:1653+2'UNG+GSMCAR+TSITE+SRT+080828:1653+2+UN+D:00A:SUPRPT'ACI Message 1 text 1ACI Message 2 text 2UNE+2+2'UNZ+1+2'",
				"UNA:+.? 'UNB+UNOA:3+DECLIENTID+RCCECECPW+080828:1653+1'UNG+GSMCAR+TSITE+SRT+080828:1653+1+UN+D:00A:SUPRPT'ACI Message 5 text 3UNE+1+1'UNZ+1+1'",
				"UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSATID+080828:1653+1'UNG+GSIMEX+TSITE+ET+080828:1653+1+CC+D:00A:EX1STP'G7 Message 1 text 1G7 Message 2 text 2UNE+2+1'UNZ+1+1'");
		}

		void AssertSendACIandG7Interchanges(string expectedCurrentBranchACI, string expectedDEBranchACI, string expectedEXP)
		{
			BatchProcessorUtilities.ResetValidACIBranchesForTesting();
			GlbCompany dECompany = Factory.New<GlbCompany>();
			dECompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			GlbBranch dEBranch = dECompany.Branches.AddNew();
			dEBranch.GB_Code = "FRA";
			dEBranch.GB_RL_NKHomePort = "DEFRA";
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(dECompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "DECLIENTID");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");
			SUPRPTMessage message1 = Factory.New<SUPRPTMessage>();
			message1.EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			message1.EM_MessageText = "ACI Message 1 text " + EDIMessage.MessageNumberPlaceHolder;

			SUPRPTMessage message2 = Factory.New<SUPRPTMessage>();
			message2.EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageText = "ACI Message 2 text " + EDIMessage.MessageNumberPlaceHolder;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;

			EX1STPMessage message3 = Factory.New<EX1STPMessage>();
			message3.EM_MessageType = MessageTypeList.Codes.G7Export;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAEXP;
			message3.EM_MessageText = "G7 Message 1 text " + EDIMessage.MessageNumberPlaceHolder;

			EX1STPMessage message4 = Factory.New<EX1STPMessage>();
			message4.EM_MessageType = MessageTypeList.Codes.G7Export;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message4.EM_MessageText = "G7 Message 2 text " + EDIMessage.MessageNumberPlaceHolder;
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAEXP;

			SUPRPTMessage message5 = Factory.New<SUPRPTMessage>();
			message5.EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
			message5.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			message5.EM_MessageText = "ACI Message 5 text " + EDIMessage.MessageNumberPlaceHolder;
			message5.EM_GB = dEBranch.PK;

			Factory.Save();
			Sender.ExecuteBatch();

			ZQuery filter = new ZQuery();
			filter.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.eHubQueued);
			filter.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			filter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			filter.IncludeBlob(EDIInterchange.AllBlobFields);
			EDIInterchange[] sendableInterchanges = Factory.Load<EDIInterchange>(filter);
			AssertEquals("3 interchanges created", 3, sendableInterchanges.Length);

			int eXPInterchange = -1;
			int aCIInterchange1 = -1;
			int aCIInterchange2 = -1;
			for (int i = 0; i < 3; i++)
			{
				if (sendableInterchanges[i].EI_ApplicationCode == EDIMessage.ApplicationCodes.CAEXP)
				{
					eXPInterchange = i;
				}
				else if (sendableInterchanges[i].EI_ApplicationCode == EDIMessage.ApplicationCodes.CAACI && sendableInterchanges[i].EI_InterchangeText.Contains("ACI Message 5 text"))
				{
					aCIInterchange2 = i;
				}
				else
				{
					aCIInterchange1 = i;
				}
			}
			AssertEquals(expectedCurrentBranchACI, sendableInterchanges[aCIInterchange1].EI_InterchangeText);
			AssertEquals(expectedDEBranchACI, sendableInterchanges[aCIInterchange2].EI_InterchangeText);
			AssertEquals(expectedEXP, sendableInterchanges[eXPInterchange].EI_InterchangeText);
		}

		[TestDate(2008, 8, 28, 16, 53, 25)]
		public void TestSendIMPInterchanges()
		{
			BatchProcessorUtilities.ResetValidACIBranchesForTesting();
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.TransmissionSite.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "12345");
			CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PASSWORD");
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);

			GlbCompany otherCACompany = Factory.New<GlbCompany>();
			otherCACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			GlbBranch otherCABranch = otherCACompany.Branches.AddNew();
			otherCABranch.GB_Code = "OTT";
			otherCABranch.GB_RL_NKHomePort = "CAOTT";
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(otherCACompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "OTTCLIENT");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(otherCACompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "67890");
			CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetTemporaryValue(otherCACompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "OTTPW");

			EDIReleaseMessage message1 = Factory.New<EDIReleaseMessage>();
			message1.EM_IsTestMessage = true;
			message1.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message1.EM_MessageText = "REL Message 1 text " + EDIMessage.MessageNumberPlaceHolder;

			EDIReleaseMessage message2 = Factory.New<EDIReleaseMessage>();
			message2.EM_IsTestMessage = true;
			message2.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageText = "REL Message 2 text " + EDIMessage.MessageNumberPlaceHolder;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;

			EDIReleaseMessage message3 = Factory.New<EDIReleaseMessage>();
			message3.EM_IsTestMessage = true;
			message3.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_MessageText = "REL Message 3 text " + EDIMessage.MessageNumberPlaceHolder;
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message3.EM_GB = otherCABranch.PK;

			Factory.Save();
			Sender.ExecuteBatch();

			ZQuery filter = new ZQuery();
			filter.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.eHubQueued);
			filter.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			filter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			filter.IncludeBlob(EDIInterchange.AllBlobFields);
			var sendableInterchanges = Factory.Load<EDIInterchange>(filter);
			AssertEquals("2 interchanges created", 2, sendableInterchanges.Length);

			int rELInterchange = -1;
			int otherRELInterchange = -1;
			// other import messages types will be added here
			for (int i = 0; i < 2; i++)
			{
				if (sendableInterchanges[i].EI_ApplicationCode == EDIMessage.ApplicationCodes.CAIMP && sendableInterchanges[i].EI_InterchangeText.Contains("REL Message 1 text"))
				{
					rELInterchange = i;
				}
				else if (sendableInterchanges[i].EI_ApplicationCode == EDIMessage.ApplicationCodes.CAIMP && sendableInterchanges[i].EI_InterchangeText.Contains("REL Message 3 text"))
				{
					otherRELInterchange = i;
				}
			}
			AssertEquals(expectedRELInterchange, sendableInterchanges[rELInterchange].EI_InterchangeText);
			AssertEquals(expectedOtherRELInterchange, sendableInterchanges[otherRELInterchange].EI_InterchangeText);
		}

		readonly string expectedRELInterchange = "UNA:+.? 'UNB+UNOA:1+CLIENTID+CBSATID+080828:1653+1'UNG+CUSDEC+TSITE+RT+080828:1653+1+UN+D:96A+12345PASSWORD'REL Message 1 text 1REL Message 2 text 2UNE+2+1'UNZ+1+1'";
		readonly string expectedOtherRELInterchange = "UNA:+.? 'UNB+UNOA:1+OTTCLIENT+RCCECECPW+080828:1653+1'UNG+CUSDEC+TSITE+RT+080828:1653+1+UN+D:96A+67890OTTPW'REL Message 3 text 3UNE+1+1'UNZ+1+1'";

		public override void TestMessagesWithEM_HeldUntilDateNotSentUntilTheRightDate()
		{
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "12345");
			eHubMessagingRegistry.Instance.SendCAViaEHub.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);

			var testMessage1 = Factory.NewWithValidTestData<EDIMessage>();
			testMessage1.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CAIMP;
			testMessage1.EM_Status = EDIMessage.Status.Queued;
			testMessage1.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			testMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			var testMessage2 = Factory.NewWithValidTestData<EDIMessage>();
			testMessage2.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CAIMP;
			testMessage2.EM_Status = EDIMessage.Status.Queued;
			testMessage2.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			testMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage2.EM_HeldUntilDate = ZDateTime.UtcNow;
			var testMessage3 = Factory.NewWithValidTestData<EDIMessage>();
			testMessage3.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CAIMP;
			testMessage3.EM_Status = EDIMessage.Status.Queued;
			testMessage3.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			testMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage3.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(5);

			CombineAssertions("PreCheck", () =>
			{
				AssertEquals("Message Count", 3, Factory.Load<EDIMessage>(new ZQuery()).Length);
				AssertEquals("Interchange Count", 0, Factory.Load<EDIInterchange>(new ZQuery()).Length);
			});

			Factory.Save();
			Sender.ExecuteBatch();

			CombineAssertions("Assertion", () =>
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedMessages = newFactory.Load<EDIMessage>(new ZQuery());
				var reloadedInterchanges = newFactory.Load<EDIInterchange>(new ZQuery());

				AssertEquals("Message Count", 3, reloadedMessages.Length);
				AssertEquals("Interchange Count", 1, reloadedInterchanges.Length);
				var reloadedInterchange = reloadedInterchanges[0];
				var reloadedMessage1 = reloadedMessages.FirstOrDefault(message => message.PK == testMessage1.PK);
				var reloadedMessage2 = reloadedMessages.FirstOrDefault(message => message.PK == testMessage2.PK);
				var reloadedMessage3 = reloadedMessages.FirstOrDefault(message => message.PK == testMessage3.PK);

				AssertEquals("Interchange Status", EDIInterchange.Status.eHubQueued, reloadedInterchange.EI_Status);
				AssertEquals("Interchange TransportType", EDIInterchange.TransportType.eHub, reloadedInterchange.EI_TransportType);
				AssertEquals("Message 1 Status", EDIMessage.Status.Sent, reloadedMessage1.EM_Status);
				AssertEquals("Message 2 Status", EDIMessage.Status.Sent, reloadedMessage2.EM_Status);
				AssertEquals("Message 3 Status", EDIMessage.Status.Queued, reloadedMessage3.EM_Status);
				AssertEquals("Message 1 Status", reloadedInterchange.PK, reloadedMessage1.EM_EI);
				AssertEquals("Message 2 Status", reloadedInterchange.PK, reloadedMessage2.EM_EI);
				AssertEquals("Message 3 Status", ZGuid.Empty, reloadedMessage3.EM_EI);
			});
		}

		public void TestSplitPerMessage()
		{
			BatchProcessorUtilities.ResetValidACIBranchesForTesting();
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.TransmissionSite.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "12345");
			CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PASSWORD");
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var branch1_1 = GlbBranch.CurrentBranch;

			var collection = new NonDependentEDIMessageCollection(Factory);
			var message1 = collection.AddNew();
			message1.EM_MessageType = MessageTypeList.Codes.IntegratedImportDeclaration;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message1.EM_GB = branch1_1.PK;
			message1.EM_MessageText = @"<root>Message 1 text</root>";

			var message2 = collection.AddNew();
			message2.EM_MessageType = MessageTypeList.Codes.IntegratedImportDeclaration;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message2.EM_GB = branch1_1.PK;
			message2.EM_MessageText = @"<root>Message 2 text</root>";

			var message3 = collection.AddNew();
			message3.EM_IsTestMessage = false;
			message3.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message3.EM_GB = branch1_1.PK;
			message3.EM_MessageText = @"<root>Message 3 text</root>";

			var message4 = collection.AddNew();
			message4.EM_IsTestMessage = false;
			message4.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message4.EM_GB = branch1_1.PK;
			message4.EM_MessageText = @"<root>Message 4 text</root>";

			var message5 = collection.AddNew();
			message5.EM_IsTestMessage = false;
			message5.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			message5.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message5.EM_GB = branch1_1.PK;
			message5.EM_MessageText = @"<root>Message 5 text</root>";

			new Sender_Exposed().PackageMessagesIntoInterchanges_Exposed(collection);

			AssertNotEquals(message1.EM_EI, message2.EM_EI);
			AssertNotEquals(message3.EM_EI, message4.EM_EI);

			AssertEquals("UDM", message1.Interchange.EI_ApplicationCode);
			AssertEquals("CAI", message3.Interchange.EI_ApplicationCode);
			AssertEquals("CAI", message5.Interchange.EI_ApplicationCode);
		}

		#region Implementation
		string RegistryLocation(IRegistryItemInternals registryItem)
		{
			return "Admin -> System -> Registry -> " + registryItem.Location;
		}

		protected override EDIInterchange GetNewEDIInterchangeReadyToSend()
		{
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_From = GlbCompany.CurrentCompany.GC_Code;
			interchange.EI_To = "CAC";
			Enterprise.Messaging.Business.EDIMessage message = GetNewEDIMessageReadyToSend();
			interchange.EI_BodyText = message.EM_MessageText;
			interchange.ContainedMessages.Add(message);
			return interchange;
		}

		protected override Enterprise.Messaging.Business.EDIMessage GetNewEDIMessageReadyToSend()
		{
			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DataLoadingModule;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = "PPermit";
			message.EM_HeldUntilDate = ZDateTime.Now.AddDays(-1);
			return message;
		}

		protected override BaseInterchangeSender GetNewSender()
		{
			return new Sender();
		}

		TempDirectory outputDirectory;
		TempDirectory outputCIGDirectory;

		protected override void SetUp()
		{
			base.SetUp();
			outputDirectory = new TempDirectory();
			outputCIGDirectory = new TempDirectory();
			CACustomsDataRegistry.Instance.DataLoadingModuleOutputDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, outputDirectory.DirectoryName);
			CACustomsDataRegistry.Instance.MessageOutputDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, outputCIGDirectory.DirectoryName);
		}

		protected override void TearDown()
		{
			base.TearDown();
			outputDirectory.Dispose();
			outputCIGDirectory.Dispose();
		}
		#endregion

		class Sender_Exposed : Sender
		{
			internal void PackageMessagesIntoInterchanges_Exposed(NonDependentEDIMessageCollection messages)
			{
				base.PackageMessagesIntoInterchanges(messages);
			}
		}
	}
}
