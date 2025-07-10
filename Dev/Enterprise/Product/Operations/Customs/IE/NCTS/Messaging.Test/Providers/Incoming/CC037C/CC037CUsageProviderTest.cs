using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC037CUsageProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("UsageType missing", () => new CC037CUsageProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber", 1, provider.SequenceNumber);
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "19MRNCC055C0123456", provider.MRN);
		}

		public void TestCoveredAmount()
		{
			AssertEquals("CoveredAmount", 10M, provider.CoveredAmount);
		}

		public void TestCurrency()
		{
			AssertEquals("Currency", "QWE", provider.Currency);
		}

		public void TestLockDate()
		{
			AssertEquals("LockDate", new ZDate(2023, 02, 21), provider.LockDate);
		}

		public void TestArrivalDateAndTime()
		{
			AssertEquals("ArrivalDateAndTime", new ZDateTime(2023, 02, 5, 3, 2, 2), provider.ArrivalDateAndTime);
		}

		public void TestReleaseDate()
		{
			AssertEquals("ReleaseDate", new ZDate(2023, 02, 19), provider.ReleaseDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC037CUsageProvider(new UsageType
			{
				SequenceNumber = "1",
				Mrn = "19MRNCC055C0123456",
				CoveredAmount = 10,
				Currency = "QWE",
				LockDate = new DateTime(2023, 02, 21, 1, 2, 7),
				ArrivalDateAndTime = new DateTime(2023, 02, 5, 3, 2, 2),
				ReleaseDate = new DateTime(2023, 02, 19, 2, 3, 6),
			});
		}
		CC037CUsageProvider provider;
	}
}
