using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Messaging.Testing
{
	abstract class EDIMessageComparerTest<T> : AutoEDIMessageComparerTest<T> where T : EDIMessage
	{
		public void TestOneWithInterchangeOneWithout()
		{
			MessageX.EM_EI = Factory.New(typeof(EDIInterchange)).PK;
			AssertCompareLessThan("(with Interchange) < (no Interchange)");
		}

		public void TestCompareInterchangeNums()
		{
			MessageX.EM_EI = Factory.New(typeof(EDIInterchange)).PK;
			MessageY.EM_EI = Factory.New(typeof(EDIInterchange)).PK;

			MessageX.Interchange.EI_InterchangeNum = "1";
			MessageY.Interchange.EI_InterchangeNum = "02";
			AssertCompareLessThan("When < than numeric");

			MessageX.Interchange.EI_InterchangeNum = "C01";
			MessageY.Interchange.EI_InterchangeNum = "C02";
			AssertCompareLessThan("When < than alphanumeric");

			MessageX.Interchange.EI_InterchangeNum = "1";
			MessageY.Interchange.EI_InterchangeNum = "01";
			MessageX.EM_MessageNum = "2";
			MessageY.EM_MessageNum = "1";
			AssertCompareGreaterThan("When = numeric, fallback to EM_MessageNum");

			MessageX.Interchange.EI_InterchangeNum = "C01";
			MessageY.Interchange.EI_InterchangeNum = "C01";
			MessageX.EM_MessageNum = "2";
			MessageY.EM_MessageNum = "1";
			AssertCompareGreaterThan("When = alphanumeric, fallback");

			MessageX.Interchange.EI_InterchangeNum = "C01";
			MessageY.Interchange.EI_InterchangeNum = "C02";
			MessageX.EM_MessageNum = "2";
			MessageY.EM_MessageNum = "1";
			AssertCompareLessThan("EI_InterchangeNum takes precedence over EM_MessageNum");
		}

		public void TestCompareInterchangeNums_WithReallyBigNumber()
		{
			MessageX.EM_EI = Factory.New(typeof(EDIInterchange)).PK;
			MessageY.EM_EI = Factory.New(typeof(EDIInterchange)).PK;

			MessageX.Interchange.EI_InterchangeNum = "00111111111111111111";
			MessageY.Interchange.EI_InterchangeNum = "111111111111111112";
			AssertCompareLessThan("When < than numeric for really big numbers");
		}

		public void TestWhenComparingReceivedAndTransmittedCompareByDate()
		{
			MessageX.EM_EI = Factory.New(typeof(EDIInterchange)).PK;
			MessageY.EM_EI = Factory.New(typeof(EDIInterchange)).PK;

			MessageX.Interchange.EI_InterchangeNum = "1";
			MessageY.Interchange.EI_InterchangeNum = "2";
			MessageX.EM_MessageNum = "1";
			MessageY.EM_MessageNum = "2";

			MessageX.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			MessageY.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			MessageX.EM_SystemCreateTimeUtc = new ZDateTime(2005, 2, 2);
			MessageY.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
			AssertCompareGreaterThan("Must sort by DateTime and not interchange/message number when comparing Receive to Transmit");
		}

		public void TestCompareBusinessObjectNotInDbCreateSequence()
		{
			AssertCompareLessThan("When business object not saved, the first Factory.New'd EDIMessage should come first");

			MessageX.EM_ApplicationCode = "AP2";
			MessageY.EM_ApplicationCode = "AP1";
			AssertCompareLessThan("BusinessObjectNotInDbCreateSequence takes precedence over EM_ApplicationCode");

			Factory.Save();

			MessageX.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
			MessageY.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
			MessageX.EM_ApplicationCode = "AP2";
			MessageY.EM_ApplicationCode = "AP1";
			AssertCompareGreaterThan("When = , fallback to EM_ApplicationCode");
		}
	}

	class EDIMessageComparerTest : TestCaseWithFactory
	{
		public void TestGetAscendingSortedMessages()
		{
			EDIMessage messageX = Factory.NewWithValidTestData<EDIMessage>();
			EDIMessage messageY = Factory.NewWithValidTestData<EDIMessage>();

			messageX.EM_MessageNum = "1";
			messageY.EM_MessageNum = "2";

			var messages = new List<EDIMessage>();
			messages.Add(messageX);
			messages.Add(messageY);

			EDIMessage[] sortedMessages = EDIMessageComparer.GetSortedMessages(messages, ListSortDirection.Ascending);
			AssertEquals("Messages should be sorted descendingly", "1", sortedMessages[0].EM_MessageNum);
			AssertEquals("Messages should be sorted descendingly", "2", sortedMessages[1].EM_MessageNum);
		}

		public void TestGetDecendinglySortedMessages()
		{
			EDIMessage messageX = Factory.NewWithValidTestData<EDIMessage>();
			EDIMessage messageY = Factory.NewWithValidTestData<EDIMessage>();

			messageX.EM_MessageNum = "1";
			messageY.EM_MessageNum = "2";

			var messages = new List<EDIMessage>();
			messages.Add(messageX);
			messages.Add(messageY);

			EDIMessage[] sortedMessages = EDIMessageComparer.GetSortedMessages(messages, ListSortDirection.Descending);
			AssertEquals("Messages should be sorted descendingly", "2", sortedMessages[0].EM_MessageNum);
			AssertEquals("Messages should be sorted descendingly", "1", sortedMessages[1].EM_MessageNum);
		}

		public void TestGetDecendingSortedMessages()
		{
			EDIMessage messageX = Factory.NewWithValidTestData<EDIMessage>();
			EDIMessage messageY = Factory.NewWithValidTestData<EDIMessage>();

			messageX.EM_MessageNum = "1";
			messageY.EM_MessageNum = "2";

			var messages = new List<EDIMessage>();
			messages.Add(messageX);
			messages.Add(messageY);

			EDIMessage[] sortedMessages = EDIMessageComparer.GetSortedMessages(messages, ListSortDirection.Descending);
			AssertEquals("Messages should be sorted descendingly", "2", sortedMessages[0].EM_MessageNum);
			AssertEquals("Messages should be sorted descendingly", "1", sortedMessages[1].EM_MessageNum);
		}
	}
}
