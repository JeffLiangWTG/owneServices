using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Messaging.Testing
{
	abstract class AutoEDIMessageComparerTest<T> : TestCaseWithFactory where T : AutoEDIMessage
	{
		public void TestCompareNull()
		{
			AssertEquals("When X is null", LessThan, Comparer.Compare(null, MessageY));
			AssertEquals("When Y is null", GreaterThan, Comparer.Compare(MessageX, null));
			AssertEquals("When X and Y are null they equal", 0, Comparer.Compare(null, null));
		}

		public void TestCompareMessageNums()
		{
			MessageX.EM_MessageNum = "1";
			MessageY.EM_MessageNum = "02";
			AssertCompareLessThan("When < than numeric");

			MessageX.EM_MessageNum = "C01";
			MessageY.EM_MessageNum = "C02";
			AssertCompareLessThan("When < than alphanumeric");

			MessageX.EM_MessageNum = "1";
			MessageY.EM_MessageNum = "01";
			MessageX.EM_SystemCreateTimeUtc = new ZDateTime(2005, 2, 2);
			MessageY.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
			AssertCompareGreaterThan("When = numeric, fallback to EM_SystemCreateTimeUtc");

			MessageX.EM_MessageNum = "C01";
			MessageY.EM_MessageNum = "C01";
			MessageX.EM_SystemCreateTimeUtc = new ZDateTime(2005, 2, 2);
			MessageY.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
			AssertCompareGreaterThan("When = alphanumeric, fallback to EM_SystemCreateTimeUtc");

			MessageX.EM_MessageNum = "C01";
			MessageY.EM_MessageNum = "C02";
			MessageX.EM_SystemCreateTimeUtc = new ZDateTime(2005, 2, 2);
			MessageY.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
			AssertCompareLessThan("EM_MessageNum takes precedence over EM_SystemCreateTimeUtc");
		}

		public void TestCompareMessageNums_WithReallyBigNumber()
		{
			MessageX.EM_MessageNum = "00111111111111111111";
			MessageY.EM_MessageNum = "111111111111111112";
			AssertCompareLessThan("When < than numeric for really big number");
		}

		public void TestCompareSystemCreateTime()
		{
			Factory.Save();

			MessageX.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
			MessageY.EM_SystemCreateTimeUtc = new ZDateTime(2005, 2, 2);
			AssertCompareLessThan("When < than");

			MessageX.EM_SystemCreateTimeUtc = ZDateTime.Empty;
			MessageY.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
			AssertCompareGreaterThan("EDIMessage added > EDIMessage in db with create time");

			MessageX.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
			MessageY.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
			MessageX.EM_ApplicationCode = "AP2";
			MessageY.EM_ApplicationCode = "AP1";
			AssertCompareGreaterThan("When = , fallback to EM_ApplicationCode");

			MessageX.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
			MessageY.EM_SystemCreateTimeUtc = new ZDateTime(2005, 2, 2);
			MessageX.EM_ApplicationCode = "AP2";
			MessageY.EM_ApplicationCode = "AP1";
			AssertCompareLessThan("EM_SystemCreateTimeUtc takes precedence over EM_ApplicationCode");
		}

		public void TestCompareApplicationCodes()
		{
			Factory.Save();

			MessageX.EM_MessageNum = "1";
			MessageY.EM_MessageNum = "1";

			MessageX.EM_ApplicationCode = "AP1";
			MessageY.EM_ApplicationCode = "AP2";
			AssertCompareLessThan("When < than");

			MessageX.EM_ApplicationCode = "AP1";
			MessageY.EM_ApplicationCode = "AP1";
			MessageX.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			MessageY.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertCompareGreaterThan("When = , fallback to EM_ReceiveTransmit");

			MessageX.EM_ApplicationCode = "AP1";
			MessageY.EM_ApplicationCode = "AP2";
			MessageX.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			MessageY.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertCompareLessThan("EM_ApplicationCode takes precedence over EM_ReceiveTransmit");
		}

		public void TestCompareReceiveToTransmit()
		{
			MessageX.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			MessageY.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertCompareLessThan("Receive < Transmit");

			MessageX.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			MessageX.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			AssertEquals("Fallback to something else (EM_PK)", true, Comparer.Compare(MessageX, MessageY) != 0);
		}

		#region Implementation

		protected void AssertCompareGreaterThan(string failureMessage)
		{
			AssertCompare(failureMessage, GreaterThan);
		}

		protected void AssertCompareLessThan(string failureMessage)
		{
			AssertCompare(failureMessage, LessThan);
		}

		void AssertCompare(string failureMessage, int lessThanOrGreaterThan)
		{
			AssertEquals(failureMessage, lessThanOrGreaterThan, Comparer.Compare(MessageX, MessageY));
			AssertEquals(failureMessage, -lessThanOrGreaterThan, Comparer.Compare(MessageY, MessageX));

			AssertEquals(failureMessage, 0, Comparer.Compare(MessageX, MessageX));
			AssertEquals(failureMessage, 0, Comparer.Compare(MessageY, MessageY));
		}

		protected override void SetUp()
		{
			base.SetUp();
			Comparer = GetNewComparer();
			MessageX = Factory.NewWithValidTestData<T>();
			MessageY = Factory.NewWithValidTestData<T>();
			MessageX.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
			MessageY.EM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 1);
		}

		protected IComparer<T> Comparer;
		protected abstract IComparer<T> GetNewComparer();

		protected T MessageX;
		protected T MessageY;

		protected abstract int LessThan { get; }
		protected abstract int GreaterThan { get; }

		#endregion
	}
}
