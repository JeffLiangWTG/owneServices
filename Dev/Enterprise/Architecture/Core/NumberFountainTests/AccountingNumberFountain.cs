using CargoWise.Data;

#region Test
#if DEBUG

namespace Enterprise.NumberFountain.Testing
{
	using Enterprise.ZArchitecture.Environment;
	using NUnit.Framework;

	internal class AccountingNumberFountainPoolerTest : TestCase
	{
		class AccountingNumberFountainPoolerForTest : AccountingNumberFountainPooler
		{
			internal AccountingNumberFountainPoolerForTest(string fountainName, string prefix)
				: this(fountainName, prefix, FountainUtils.MinNumber)
			{
			}

			internal AccountingNumberFountainPoolerForTest(string fountainName, string prefix, long minNumber)
				: base(fountainName, prefix, minNumber)
			{
			}
		}

		public void TestGetNextWithPrefix()
		{
			AccountingNumberFountainPoolerForTest fountain = new AccountingNumberFountainPoolerForTest("TestFountain", "P");

			AssertEquals("Get Next for empty string", "P00000001", fountain.GetTodaysPeriodFountain().GetNextFormatted(Db.Connection));
			AssertEquals("GetNext for 0410", "P0410000001", fountain.GetPeriodFountain("0410", 6).GetNextFormatted(Db.Connection));
			AssertEquals("GetNext for 0410", "P0410000002", fountain.GetPeriodFountain("0410", 6).GetNextFormatted(Db.Connection));
			AssertEquals("GetNext for 0411", "P0411000001", fountain.GetPeriodFountain("0411", 6).GetNextFormatted(Db.Connection));
			AssertEquals("GetNext for 0412", "P0412000001", fountain.GetPeriodFountain("0412", 6).GetNextFormatted(Db.Connection));
			AssertEquals("GetNext for 0410", "P0410000003", fountain.GetPeriodFountain("0410", 6).GetNextFormatted(Db.Connection));
			AssertEquals("GetNext for 0411", "P0411000002", fountain.GetPeriodFountain("0411", 6).GetNextFormatted(Db.Connection));
			AssertEquals("GetNext for 0412", "P0412000002", fountain.GetPeriodFountain("0412", 6).GetNextFormatted(Db.Connection));

			//Another instance of the fountain SHOULD NOT continue the numbering
			AccountingNumberFountainPoolerForTest fountain2 = new AccountingNumberFountainPoolerForTest("TestFountain2", "P");
			AssertEquals("GetNext for 0412 for Fountain2 should not continue the numbering", "P0412000001", fountain2.GetPeriodFountain("0412", 6).GetNextFormatted(Db.Connection));
			AssertEquals("GetNext for 0410 for Fountain2 should not continue the numbering", "P0410000001", fountain2.GetPeriodFountain("0410", 6).GetNextFormatted(Db.Connection));
		}

		public void TestPeekPrelimWithPrefix()
		{
			AccountingNumberFountainPoolerForTest fountain = new AccountingNumberFountainPoolerForTest("TestFountain", "P");
			AssertEquals("PeekPrelim for today's period", "P000001", fountain.GetPeriodFountain(string.Empty, 6).PeekPreliminaryFormatted(Db.Connection));
			AssertEquals("PeekPrelim for 0410", "P0410000001", fountain.GetPeriodFountain("0410", 6).PeekPreliminaryFormatted(Db.Connection));

			fountain.GetPeriodFountain("0410", 6).GetNextFormatted(Db.Connection);
			AssertEquals("PeekPrelim for 0410", "P0410000002", fountain.GetPeriodFountain("0410", 6).PeekPreliminaryFormatted(Db.Connection));
			AssertEquals("PeekPrelim for 0411", "P0411000001", fountain.GetPeriodFountain("0411", 6).PeekPreliminaryFormatted(Db.Connection));
		}

		public void TestGetNextForAustralia()
		{
			AccountingNumberFountainPoolerForTest fountain1 = new AccountingNumberFountainPoolerForTest("TestFountain1", "P");
			AssertEquals("GetNext Fountain1", "P00000001", fountain1.GetPeriodFountain(string.Empty).GetNextFormatted(Db.Connection));
			AssertEquals("GetNext Fountain1", "P00000002", fountain1.GetPeriodFountain(string.Empty).GetNextFormatted(Db.Connection));
			AssertEquals("GetNext Fountain1", "P0410000001", fountain1.GetPeriodFountain("0410", 6).GetNextFormatted(Db.Connection));

			AccountingNumberFountainPoolerForTest fountain2 = new AccountingNumberFountainPoolerForTest("TestFountain2", "P");
			AssertEquals("GetNext Fountain2", "P00000001", fountain2.GetPeriodFountain(string.Empty).GetNextFormatted(Db.Connection));
			AssertEquals("GetNext Fountain2", "P00000002", fountain2.GetPeriodFountain(string.Empty).GetNextFormatted(Db.Connection));
			AssertEquals("GetNext Fountain2", "P0410000001", fountain2.GetPeriodFountain("0410", 6).GetNextFormatted(Db.Connection));
			AssertEquals("GetNext Fountain1", "P0410000002", fountain1.GetPeriodFountain("0410", 6).GetNextFormatted(Db.Connection));
		}

		public void TestPeekPrelimForAustralia()
		{
			AccountingNumberFountainPoolerForTest fountain1 = new AccountingNumberFountainPoolerForTest("TestFountain1", "P");

			AssertEquals("PeekPrelim Fountain1", "P00000001", fountain1.GetPeriodFountain(string.Empty).PeekPreliminaryFormatted(Db.Connection));
			fountain1.GetPeriodFountain("").GetNextFormatted(Db.Connection);
			AssertEquals("PeekPrelim Fountain1", "P00000002", fountain1.GetPeriodFountain(string.Empty).PeekPreliminaryFormatted(Db.Connection));

			//Fountain1.VoucherPeriodPrefixToUse = "200410";
			fountain1.GetPeriodFountain("0410", 6).GetNextFormatted(Db.Connection);
			AssertEquals("PeekPrelim Fountain1", "P00000002", fountain1.GetPeriodFountain(string.Empty).PeekPreliminaryFormatted(Db.Connection));
		}

		public void TestDefaultNumberFountainOwner()
		{
			AccountingNumberFountainFactory testFactory = new AccountingNumberFountainFactory("TestFountain", EnvProxy.Instance.CurrentCompany.PK);
			AssertEquals("Owner should be the current company", EnvProxy.Instance.CurrentCompany.PK, testFactory.OwnerPk);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Db.Connection.BeginTransaction();
		}

		protected override void TearDown()
		{
			Db.Connection.RollbackTransaction();
			base.TearDown();
		}
	}
}

#endif
#endregion
