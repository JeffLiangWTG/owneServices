using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.HK.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.HK.ServiceTasks.Testing
{
	class TraxonMessageProcessorTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		public void TestIEMFMAResponse()
		{
			SetupNotificationRegistry();

			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "010";

			testIEMFMATransmitMessage.EM_LinkUniqueID = consol.PK;
			testIEMFMATransmitMessage.EM_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;

			var iEMFMAMessageProcessor = new TraxonMessageProcessor(logger);
			iEMFMAMessageProcessor.ProcessMessage(testIEMFMAResponseMessage);
			AssertEquals("Link Table has been set in the response", testIEMFMATransmitMessage.EM_LinkTable, testIEMFMAResponseMessage.EM_LinkTable);
			AssertEquals("Link Unique ID has been set in the response", testIEMFMATransmitMessage.EM_LinkUniqueID, testIEMFMAResponseMessage.EM_LinkUniqueID);

			AssertEquals("One email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("test@test.com", email.Recipients[0].Email);
			AssertEquals("test@edi.com.au", email.CCRecipients[0].Email);
			AssertEquals("ISAC Response Consol Number: 010", email.Subject);
			AssertEquals("UNH+2+IEMFMA:D:95A:IA:IATA01'BGM'UCM+2:1+CUSEXP:D:95A:UN'FTX+Z03+++AWB 000-00022911 UPDATED AT 1600 15JAN1995. REPLACE?:2, TOTAL?:2.'ERC+1'UNT+6+2'", email.Body);
		}

		public void TestIEMFNAResponse()
		{
			var iEMFNAMessageProcessor = new TraxonMessageProcessor(logger);
			iEMFNAMessageProcessor.ProcessMessage(testIEMFNAResponseMessage);
			AssertEquals("Link Table has been set in the response", testIEMFNATransmitMessage.EM_LinkTable, testIEMFNAResponseMessage.EM_LinkTable);
			AssertEquals("Link Unique ID has been set in the response", testIEMFNATransmitMessage.EM_LinkUniqueID, testIEMFNAResponseMessage.EM_LinkUniqueID);
		}

		public void TestCIMFNAResponse()
		{
			var cIMFNAMessageProcessor = new TraxonMessageProcessor(logger);
			cIMFNAMessageProcessor.ProcessMessage(testCIMFNAResponseMessage);
			AssertEquals("Link Table has been set in the response", testCIMFNATransmitMessage.EM_LinkTable, testCIMFNAResponseMessage.EM_LinkTable);
			AssertEquals("Link Unique ID has been set in the response", testCIMFNATransmitMessage.EM_LinkUniqueID, testCIMFNAResponseMessage.EM_LinkUniqueID);
		}

		public void TestCIMFMAResponse()
		{
			var cIMFMAMessageProcessor = new TraxonMessageProcessor(logger);
			cIMFMAMessageProcessor.ProcessMessage(testCIMFMAResponseMessage);
			AssertEquals("Link Table has been set in the response", testCIMFMATransmitMessage.EM_LinkTable, testCIMFMAResponseMessage.EM_LinkTable);
			AssertEquals("Link Unique ID has been set in the response", testCIMFMATransmitMessage.EM_LinkUniqueID, testCIMFMAResponseMessage.EM_LinkUniqueID);
		}

		public void TestCIMFNAResponse2()
		{
			var outgoingMessage = CreateMessage("879432", EDIMessage.Direction.Transmit, "1234");
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();
			outgoingMessage.EM_MessageNum = "879432";
			Factory.Save();

			var responseMessage = CreateMessage("", EDIMessage.Direction.Receive, "");
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageText = TestResponseMessage1;

			var processor = new TraxonMessageProcessor(logger);
			processor.ProcessMessage(responseMessage);

			AssertEquals("Link Unique ID has been set in the response", outgoingMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
		}

		public void TestCIMFMAResponse2()
		{
			var outgoingMessage = CreateMessage("500", EDIMessage.Direction.Transmit, "1234");
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();
			outgoingMessage.EM_MessageNum = "500";
			Factory.Save();

			var responseMessage = CreateMessage("", EDIMessage.Direction.Receive, "");
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_MessageText = TestResponseMessage2;

			var processor = new TraxonMessageProcessor(logger);
			processor.ProcessMessage(responseMessage);

			AssertEquals("Link Unique ID has been set in the response", outgoingMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
		}

		public void TestNoMessageNumber()
		{
			SetupNotificationRegistry();

			var responseWithNoMessageNumber = CreateMessage("1", EDIMessage.Direction.Receive, "");
			responseWithNoMessageNumber.EM_Status = EDIMessage.Status.Queued;
			responseWithNoMessageNumber.EM_MessageText = TestNoMessageNum;

			var processor = new TraxonMessageProcessor(logger);
			processor.ProcessMessage(responseWithNoMessageNumber);

			AssertEquals("One email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("test@edi.com.au", email.CCRecipients[0].Email);
			AssertEquals("ISAC Response Error", email.Subject);
			AssertEquals("Segment indicating what the original message number was didn't exist.\nUNH+HMF8903429X057+CIMFMA:0'FMAACK/057-89034293 UPDATED AT 1650 09SEP2011. REPLACE 2  TOTAL 2.AWB057-89034293 TASKID4830576'UNT+3+HMF8903429X057'", email.Body);
		}

		public void TestOriginalMessageNotFound()
		{
			SetupNotificationRegistry();

			var responseNoOriginalMessage = CreateMessage("1", EDIMessage.Direction.Receive, "");
			responseNoOriginalMessage.EM_Status = EDIMessage.Status.Queued;
			responseNoOriginalMessage.EM_MessageText = TestNoOriginalMessage;

			var processor = new TraxonMessageProcessor(logger);
			processor.ProcessMessage(responseNoOriginalMessage);

			AssertEquals("One email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("test@edi.com.au", email.CCRecipients[0].Email);
			AssertEquals("ISAC Response Error", email.Subject);
			AssertEquals("The Original Message was not found for Message Number : 9988\nUNH+HMF8903429X057+CIMFMA:0+9988'FMAACK/057-89034293 UPDATED AT 1650 09SEP2011. REPLACE 2  TOTAL 2.AWB057-89034293 TASKID4830576'UNT+3+HMF8903429X057'", email.Body);
		}

		void SetupNotificationRegistry()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			staff.GS_EmailAddress = "test@test.com";

			staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			staff.GS_EmailAddress = "test@edi.com.au";

			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);

			var link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;
			Factory.Save();

			HKDataRegistry.Instance.GroupToCopyTraxonResponseEmailsTo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		protected LoggingInformation logger;
		protected EDIMessage testCIMFNAResponseMessage;
		protected EDIMessage testCIMFMAResponseMessage;
		protected EDIMessage testIEMFMAResponseMessage;
		protected EDIMessage testIEMFNAResponseMessage;
		protected EDIMessage testIEMFMATransmitMessage;
		protected EDIMessage testIEMFNATransmitMessage;
		protected EDIMessage testCIMFNATransmitMessage;
		protected EDIMessage testCIMFMATransmitMessage;

		protected override void SetUp()
		{
			base.SetUp();

			HKDataRegistry.Instance.HKTraxonSenderID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "1234");

			testIEMFMAResponseMessage = CreateMessage("2", EDIMessage.Direction.Receive, "");
			testIEMFMAResponseMessage.EM_Status = EDIMessage.Status.Queued;
			testIEMFMAResponseMessage.EM_MessageText = TestIEMFMAMessageResponse;

			testIEMFMATransmitMessage = CreateMessage("1", EDIMessage.Direction.Transmit, "1234");

			testIEMFNAResponseMessage = CreateMessage("3", EDIMessage.Direction.Receive, "");
			testIEMFNAResponseMessage.EM_Status = EDIMessage.Status.Queued;
			testIEMFNAResponseMessage.EM_MessageText = TestIEMFNAMessageResponse;

			testIEMFNATransmitMessage = CreateMessage("10", EDIMessage.Direction.Transmit, "1234");

			testCIMFNAResponseMessage = CreateMessage("119813", EDIMessage.Direction.Receive, "");
			testCIMFNAResponseMessage.EM_Status = EDIMessage.Status.Queued;
			testCIMFNAResponseMessage.EM_MessageText = TestCIMFNAMessageResponse;

			testCIMFNATransmitMessage = CreateMessage("119813", EDIMessage.Direction.Transmit, "1234");
			testCIMFNATransmitMessage.EM_Status = EDIMessage.Status.Sent;

			testCIMFMAResponseMessage = CreateMessage("119812", EDIMessage.Direction.Receive, "");
			testCIMFMAResponseMessage.EM_Status = EDIMessage.Status.Queued;
			testCIMFMAResponseMessage.EM_MessageText = TestCIMFMAMessageResponse;

			var testCIMFMATransmitMessage2 = CreateMessage("119812", EDIMessage.Direction.Transmit, "5678");
			testCIMFMATransmitMessage = CreateMessage("119812", EDIMessage.Direction.Transmit, "1234");
			Factory.Save();

			testIEMFMATransmitMessage.EM_MessageNum = "1";
			testIEMFNATransmitMessage.EM_MessageNum = "10";
			testCIMFNATransmitMessage.EM_MessageNum = "119813";

			testCIMFMATransmitMessage.EM_MessageNum = "119812";
			testCIMFMATransmitMessage2.EM_MessageNum = "119812";

			Factory.Save();

			logger = new LoggingInformation();
		}

		EDIMessage CreateMessage(ZString messageNum, ZString direction, ZString interchangeFromForOutgoing)
		{
			var result = Factory.New<TraxonMessage>();
			result.EM_ReceiveTransmit = direction;
			result.EM_MessageNum = messageNum;
			result.EM_Status = EDIMessage.Status.Sent;
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			result.EM_LinkUniqueID = ZGuid.NewZGuid();
			result.EM_LinkTable = TraxonMessage.Schema.TableName;

			if (result.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
			{
				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_From = interchangeFromForOutgoing;
				interchange.EI_To = "TRAXON";
				interchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.Traxon;
				interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
				result.EM_EI = interchange.PK;
			}

			return result;
		}

		const string TestIEMFMAMessageResponse = @"UNH+2+IEMFMA:D:95A:IA:IATA01'BGM'UCM+2:1+CUSEXP:D:95A:UN'FTX+Z03+++AWB 000-00022911 UPDATED AT 1600 15JAN1995. REPLACE?:2, TOTAL?:2.'ERC+1'UNT+6+2'";
		internal const string TestIEMFNAMessageResponse = @"UNH+3+IEMFNA:D:95A:IA:IATA01'BGM'UCM+3:10+CUSEXP:D:95A:UN'FTX+AA0+++DUPLICATE HWB NO'ERC+1'UNT+6+3'";
		internal const string TestCIMFNAMessageResponse = @"UNH+HMF5721896X618+CIMFNA:0+119813'FNAACK/ERROR IN618-57218965,HWB/FLIGHT HAS BEEN FROZEN CIMXHL'UNT+3+119813'UNZ+1+022037120'";
		internal const string TestCIMFMAMessageResponse = @"UNH+HMF6556500X020+CIMFMA:0+119812'FMAACK/020-65565006 UPDATED ON 20040403:0932/1 HWB UPDATED CIMXHL'UNT+3+119812'UNZ+1+03932260'";
		internal const string TestOutgoingMessage1 = "UNH+HMF8903429X057+CUSEXP:D:95A:UN+879432'BGM+85:::EXPRESS CONSIGNMENT MANIFEST+05789034293/C00001120+5'LOC+5+SYD'LOC+8+AKL'CNT+7:4000.0:KGM'CNT+8:3'NAD+PK+RWWER'TDT+13+AF345'DTM+132:110428:101'RFF+MWB:05789034293'CNT+10:2'CNI+1+432879342'CNT+8:1'CNT+1:1'MEA+WT++KGM:2500.0'LOC+5+CHI'LOC+8+IVC'NAD+CN++FX 8621310489+NZAS+TIWAI ROAD:SOUTHERLAND VIA INVERCARGILL, NEW Z+SOUTHERLAND+AUK+A+NZ'NAD+CZ++TE 8522345678+AUSTRALIAN ACADEMY OF MUSIC+C/O PHOENIX INTERNATIONAL:712 N. CENTRAL                     +DALE+IL+60191+US'GDS+12'FTX+AAA+++TEST'MOA+96:0.00:NZD'MOA+95:0.00:NZD'MOA+94:0.00:NZD'FTX+AAA+1++AAA'CNI+2+890332499'CNT+8:2'CNT+1:10'MEA+WT++KGM:1500.0'LOC+5+CHI'LOC+8+IVC'NAD+CN+++NZAS+TIWAI ROAD:SOUTHERLAND VIA INVERCARGILL, NEW Z+SOUTHERLAND+AUK+A+NZ'NAD+CZ+++AUSTRALIAN ACADEMY OF MUSIC+C/O PHOENIX INTERNATIONAL:712 N. CENTRAL                     +DALE+IL+60191+US'GDS+12'FTX+AAA+++TEST 2'MOA+96:0.00:NZD'MOA+95:0.00:NZD'MOA+94:0.00:NZD'DOC+811:::A234567890B234567890C'FTX+AAA+1++VVVV'UNT+40+HMF8903429X057'";
		internal const string TestResponseMessage1 = "UNH+HMF8903429X057+CIMFNA:0+879432'FNAACK/INVALID LICENCE NUMBER HWB 890332499 AWB 05789034293 AWB057-89034293 TASKID 314723'UNT+3+HMF8903429X057'";
		internal const string TestResponseMessage2 = "UNH+HMF8903429X057+CIMFMA:0+500'FMAACK/057-89034293 UPDATED AT 1650 09SEP2011. REPLACE 2  TOTAL 2.AWB057-89034293 TASKID4830576'UNT+3+HMF8903429X057'";
		internal const string TestNoMessageNum = "UNH+HMF8903429X057+CIMFMA:0'FMAACK/057-89034293 UPDATED AT 1650 09SEP2011. REPLACE 2  TOTAL 2.AWB057-89034293 TASKID4830576'UNT+3+HMF8903429X057'";
		internal const string TestNoOriginalMessage = "UNH+HMF8903429X057+CIMFMA:0+9988'FMAACK/057-89034293 UPDATED AT 1650 09SEP2011. REPLACE 2  TOTAL 2.AWB057-89034293 TASKID4830576'UNT+3+HMF8903429X057'";
	}
}
