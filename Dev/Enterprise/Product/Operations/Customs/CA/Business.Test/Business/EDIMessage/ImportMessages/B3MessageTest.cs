using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B3Message))]
	public class B3MessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTransactionNumber()
		{
			message.EM_MessageText = "UNH+2287+CUSDEC:S:99B:UN'BGM+:::AB+454+9'CST++I'LOC+41+497'LOC+11+497'RFF+TN:0001831'RFF+ARA:105759013RM0001'TDT+11++2++9165'DOC+785+803609238364B'DTM+204:20160120:102'MOA+43:4000'UNS+D'DMS+1'NAD+SE++GHJ LTD. INT?'L  ?+?:??@'DOC+935'DTM+129:20160119:102'LOC+27+AU+AU'PAT+1+CONSIGN:::02'MOA+6::CAD'CST+1+POS+1+8544700090+23'MOA+40:400000'MOA+43:400000'MOA+125:400000'RFF+LI:1:0'MOA+38:400000'TAX+7+VAT++5.0'MOA+1:20000'GIR+1+1'MEA+AAR++MTR:2134'TAX+5+++0.00'MOA+155:000'UNS+S'TAX+7+:::K90'MOA+1:20000'TAX+4+:::K90'MOA+176:20000'UNT+37+2287'";
			AssertEquals("TransactionNumber", "000001831", message.TransactionNumber);
		}

		public void TestMessageSubTypeDescription()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = B3EntryStatusList.Codes.Accepted;
			AssertEquals("EM_MessageSubTypeDescription", B3EntryStatusList.Descriptions.Accepted, message.EM_MessageSubTypeDescription);

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			AssertEquals("EM_MessageSubTypeDescription", MessageSubTypeCodes.Descriptions.Original, message.EM_MessageSubTypeDescription);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGetLastAcceptedMessageIsNotNull()
		{
			var x = B3Message.GetLastSentAcceptedB3Message(null);
		}

		public void TestGetLastSentAcceptedB3Message()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			var message1 = AddB3Message(entryHeader, B3EntryStatusList.Codes.Error, EDIMessage.Direction.Receive, ZDateTime.Now.AddDays(-1));
			var message2 = AddB3Message(entryHeader, ZString.Empty, EDIMessage.Direction.Transmit, ZDateTime.Now.AddDays(-2));
			var message3 = AddB3Message(entryHeader, B3EntryStatusList.Codes.Accepted, EDIMessage.Direction.Receive, ZDateTime.Now.AddDays(-5));
			var message4 = AddB3Message(entryHeader, ZString.Empty, EDIMessage.Direction.Transmit, ZDateTime.Now.AddDays(-6));
			AssertEquals(message4, B3Message.GetLastSentAcceptedB3Message(entryHeader));

			var message5 = AddB3Message(entryHeader, B3EntryStatusList.Codes.Confirmed, EDIMessage.Direction.Receive, ZDateTime.Now.AddDays(-3));
			var message6 = AddB3Message(entryHeader, ZString.Empty, EDIMessage.Direction.Transmit, ZDateTime.Now.AddDays(-4));
			AssertEquals(message6, B3Message.GetLastSentAcceptedB3Message(entryHeader));
		}

		static B3Message AddB3Message(CusEntryHeader entryHeader, string subType, string direction, ZDateTime date, string text = "")
		{
			var message = entryHeader.Factory.New<B3Message>();
			message.EM_MessageSubType = subType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = message.IsTransmitMessage ? EDIMessage.Status.Sent : EDIMessage.Status.Received;
			message.EM_SystemCreateTimeUtc = date;
			message.EM_MessageText = text;
			entryHeader.Messages.Add(message);

			return message;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (B3Message)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<B3Message>();
		}

		public virtual void TestDefaultValues()
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.CAIMP, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.B3CUSDEC, message.EM_MessageType);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
		}

		public void TestMessageNumberFilledIn()
		{
			var number = Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", EDIMessage.ApplicationCodes.CAIMP).PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("MessageNumberFilledIn", "Message Number = " + number, message.EM_MessageText);
		}

		public void TestBatchNumberInCUSDEC()
		{
			message.EM_MessageText = "UNH+63+CUSDEC:S:99B:UN'BGM++033+9'UNT+3+63'";
			AssertEquals("BatchNumber", "033", message.BatchNumber);
		}

		public void TestBatchNumberInCUSRES()
		{
			message.EM_MessageText = "UNH+63+CUSRES:S:99B:UN'BGM++034+9'UNT+3+63'";
			AssertEquals("BatchNumber", "034", message.BatchNumber);
		}

		public void TestMessageScheduleDescription()
		{
			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = testCompany.Branches.AddNew();
			testBranch.FillWithValidTestData();
			testBranch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();
			using (DisposableEnvironment.ForBranch(testBranch.PK.ToGuid()))
			{
				var testMessage = Factory.New<B3Message>();
				var testDateTime = new ZDateTime(2015, 05, 01, 00, 00, 00);
				CombineAssertions("TestWithoutHeldUntilDate", () =>
				{
					testMessage.EM_IsActive = true;
					testMessage.EM_Status = EDIMessage.Status.Queued;
					testMessage.EM_HeldUntilDate = ZDateTime.Empty;
					AssertEquals("Active Queued Notime", string.Empty, testMessage.MessageScheduleDescription);

					testMessage.EM_IsActive = false;
					testMessage.EM_Status = EDIMessage.Status.Queued;
					testMessage.EM_HeldUntilDate = ZDateTime.Empty;
					AssertEquals("InActive Queued Notime", string.Empty, testMessage.MessageScheduleDescription);

					testMessage.EM_IsActive = true;
					testMessage.EM_Status = EDIMessage.Status.Cancelled;
					testMessage.EM_HeldUntilDate = ZDateTime.Empty;
					AssertEquals("Active Cancelled Notime", string.Empty, testMessage.MessageScheduleDescription);

					testMessage.EM_IsActive = false;
					testMessage.EM_Status = EDIMessage.Status.Cancelled;
					testMessage.EM_HeldUntilDate = ZDateTime.Empty;
					AssertEquals("InActive Cancelled Notime", string.Empty, testMessage.MessageScheduleDescription);

					testMessage.EM_IsActive = true;
					testMessage.EM_Status = EDIMessage.Status.Sent;
					testMessage.EM_HeldUntilDate = ZDateTime.Empty;
					AssertEquals("Active Sent Notime", string.Empty, testMessage.MessageScheduleDescription);

					testMessage.EM_IsActive = false;
					testMessage.EM_Status = EDIMessage.Status.Sent;
					testMessage.EM_HeldUntilDate = ZDateTime.Empty;
					AssertEquals("InActive Sent Notime", string.Empty, testMessage.MessageScheduleDescription);
				});

				CombineAssertions("TestWithHeldUntilDate", () =>
				{
					testMessage.EM_IsActive = true;
					testMessage.EM_Status = EDIMessage.Status.Queued;
					testMessage.EM_HeldUntilDate = testDateTime;
					AssertEquals("Active Queued Withtime", "Scheduled at 1/05/2015 10:00:00 AM", testMessage.MessageScheduleDescription);

					testMessage.EM_IsActive = false;
					testMessage.EM_Status = EDIMessage.Status.Queued;
					testMessage.EM_HeldUntilDate = testDateTime;
					AssertEquals("InActive Queued Withtime", "Canceled - Scheduled at 1/05/2015 10:00:00 AM", testMessage.MessageScheduleDescription);

					testMessage.EM_IsActive = true;
					testMessage.EM_Status = EDIMessage.Status.Cancelled;
					testMessage.EM_HeldUntilDate = testDateTime;
					AssertEquals("Active Cancelled Withtime", "Canceled - Scheduled at 1/05/2015 10:00:00 AM", testMessage.MessageScheduleDescription);

					testMessage.EM_IsActive = false;
					testMessage.EM_Status = EDIMessage.Status.Cancelled;
					testMessage.EM_HeldUntilDate = testDateTime;
					AssertEquals("InActive Cancelled Withtime", "Canceled - Scheduled at 1/05/2015 10:00:00 AM", testMessage.MessageScheduleDescription);

					testMessage.EM_IsActive = true;
					testMessage.EM_Status = EDIMessage.Status.Sent;
					testMessage.EM_HeldUntilDate = testDateTime;
					AssertEquals("Active Sent Withtime", "Scheduled at 1/05/2015 10:00:00 AM", testMessage.MessageScheduleDescription);

					testMessage.EM_IsActive = false;
					testMessage.EM_Status = EDIMessage.Status.Sent;
					testMessage.EM_HeldUntilDate = testDateTime;
					AssertEquals("InActive Sent Withtime", "Canceled - Scheduled at 1/05/2015 10:00:00 AM", testMessage.MessageScheduleDescription);
				});
			}
		}

		public void TestRNSProcessingDate()
		{
			var testMessage = Factory.New<B3Message>();
			AssertEquals("RNSProcessingDate", ZDateTime.Empty, testMessage.RNSProcessingDate);

			testMessage.EM_MessageText = @"UNH+1+CUSRES:S:99B:UN
BGM++544+9
DTM+137:20160304:102
ERP+:I99
RFF+ABO:70293
ERC+942992
ERP+:I99
DOC+961
CST++2+2+0
UNT+12+1".Replace("\r\n", "'");
			AssertEquals("RNSProcessingDate", new ZDateTime(2016, 3, 4), testMessage.RNSProcessingDate);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			message = (EDIMessage)GetNewBusinessObject();
			message.EM_MessageText = "Message Number = " + EDIMessage.MessageNumberPlaceHolder;
		}

		EDIMessage message;

		#endregion
	}
}
