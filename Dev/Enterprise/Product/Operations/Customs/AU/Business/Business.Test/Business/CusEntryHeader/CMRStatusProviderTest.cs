using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRStatusProviderTest : TestCaseWithFactory
	{
		public void TestMessageStatusDescription()
		{
			AssertEquals("Message Status", CustomsEntryStatus.NotSent.Description, statusProvider.MessageStatusDescription);

			entryHeader.CH_Status = CustomsEntryStatus.AwaitingFormalLodge.Code;
			AssertEquals("Message Status", CustomsEntryStatus.AwaitingFormalLodge.Description, statusProvider.MessageStatusDescription);
		}

		public void TestCargoStatusDescription()
		{
			ZString response = CMRImportDeclarationTestData.IMRHeld;
			EDIMessage message1 = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			message1.EM_MessageText = response;
			BatchProcessor.LoggingInformation logger = new BatchProcessor.LoggingInformation();
			new IMDRMessageProcessor(logger).ProcessMessage(message1);
			AssertEquals("Cargo status", CMRImportEntryAdvice.Held.Description, statusProvider.CargoStatusDescription);
		}

		public void TestPaymentStatusDescription()
		{
			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = ZString.Empty;
			AssertEquals("Payment Status", CMREntryPaymentStatusList.Descriptions.NotPaid, statusProvider.PaymentStatusDescription);

			entryHeader.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
			AssertEquals("Payment Status", CMREntryPaymentStatusList.Descriptions.Paid, statusProvider.PaymentStatusDescription);
		}

		public void TestATDSecurityCode()
		{
			ZString response =
				"UNH+000001+CUSRES:D:99B:UN'" +
				"BGM+961:::ATD+13B9 DAI3 I355:1+11'DTM+58:20050204:102'DTM+138:20050204:102'FTX+AHN+++FINALISED:FINALISED'TDT+20++6'" +
				"LOC+12+AUSYD::6'EQD+AH+N123::95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LTD'" +
				"NAD+IM++EAGLE DATAMATION NB'RFF+ABO:B00122382/1/1::1'RFF+ABT:AAAANNPR6::1'RFF+ABQ:SIMPLE MAIL'RFF+AIA:AAAANNPTY'" +
				"RFF+AAE:N10'DOC+1+1'PAC+150+1'CST+1'TAX+1'MOA+68:0.0000'MOA+40:15.0000'MEA+AAA+::WAR+NO:100.00000'RFF+ABD:96092000'RFF+AED:17'" +
				"CNT+5:1'CNT+2:1'CNT+3:0'UNT+30+000001'UNZ+1+00000000273283'";

			CMRATDMessage message1 = Factory.New<CMRATDMessage>();
			message1.EM_ReceiveTransmit = "RCV";
			message1.EM_MessageText = response;
			message1.EM_MessageType = CMRMessage.CMRMessageTypes.ATD;
			entryHeader.Messages.Add(message1);

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			Factory.Save();
			System.Threading.Thread.Sleep(1000);

			AssertEquals("ATD Code", "AAAANNPTY", statusProvider.ATDSecurityCode);
		}

		public void TestATDSecurityCode_ConsolidatedDeclaration()
		{
			ZString atdResponse =
				"UNH+000001+CUSRES:D:99B:UN'" +
				"BGM+961:::ATD+13B9 DAI3 I355:1+11'DTM+58:20050204:102'DTM+138:20050204:102'FTX+AHN+++FINALISED:FINALISED'TDT+20++6'" +
				"LOC+12+AUSYD::6'EQD+AH+N123::95'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+EAGLE DATAMATION INTERNATIONAL PTY:LTD'" +
				"NAD+IM++EAGLE DATAMATION NB'RFF+ABO:B00122382/1/1::1'RFF+ABT:AAAANNPR6::1'RFF+ABQ:SIMPLE MAIL'RFF+AIA:AAAANNPTY'" +
				"RFF+AAE:N10'DOC+1+1'PAC+150+1'CST+1'TAX+1'MOA+68:0.0000'MOA+40:15.0000'MEA+AAA+::WAR+NO:100.00000'RFF+ABD:96092000'RFF+AED:17'" +
				"CNT+5:1'CNT+2:1'CNT+3:0'UNT+30+000001'UNZ+1+00000000273283'";

			var factory = new BusinessObjectFactory();
			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(factory);
			var leadDec = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			leadDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var memberDeclaration = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			memberDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			memberDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = leadDec.EntryHeader;
			factory.Save();

			CMRATDMessage atdMessage = factory.New<CMRATDMessage>();
			atdMessage.EM_ReceiveTransmit = "RCV";
			atdMessage.EM_MessageText = atdResponse;
			atdMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ATD;
			consolidatedDeclaration.Messages.Add(atdMessage);

			factory.Save();
			AssertEquals("Lead entry reads ATD Code from consolidated entry messages", "AAAANNPTY", entryHeader.ATDSecurityCode);
			AssertEquals("Other entry reads ATD Code from consolidated entry messages", "AAAANNPTY", memberDeclaration.EntryHeader.ATDSecurityCode);
		}

		JobDeclaration testDec;
		CusEntryHeader entryHeader;
		CMRStatusProvider statusProvider;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			testDec.JE_DeclarationReference = "B00122382";
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "B00122382/1";
			statusProvider = new CMRStatusProvider(entryHeader);
		}
	}
}
