using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusStatementLineChargeCollection))]
	sealed class CusStatementLineChargeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestUpdateLineChargeFor()
		{
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.Charges.UpdateLineChargeFor("AAA", 100.25m);
			AssertEquals("one charge created", 1, statementLine.Charges.Count);
			AssertEquals("one charge created", 100.25m, statementLine.Charges[0].B4_ChargeAmount);

			statementLine.Charges.UpdateLineChargeFor("AAA", -100.25m);
			AssertEquals("one charge created", -100.25m, statementLine.Charges[0].B4_ChargeAmount);

			var charge = statementLine.Charges[0];
			statementLine.Charges.UpdateLineChargeFor("AAA", 0m);
			AssertEquals("AAA charge deleted", 0, statementLine.Charges.Count);
			AssertEquals("IsDeleted", true, charge.IsDeleted);
		}

		public void TestUpdateLineChargeForOtherChargeType()
		{
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.Charges.UpdateLineChargeFor("AAA", 6.95m, null, allowExistedType: true);
			statementLine.Charges.UpdateLineChargeFor("AAA", -6.95m, null, allowExistedType: true);
			AssertEquals("2 charges created", 2, statementLine.Charges.Count);

			var statementLine2 = statement.StatementLines.AddNew();
			statementLine2.Charges.UpdateLineChargeFor("AAA", 8m, null, allowExistedType: false);
			statementLine2.Charges.UpdateLineChargeFor("AAA", 88m, null, allowExistedType: false);
			AssertEquals("1 charges created", 1, statementLine2.Charges.Count);
		}

		public void TestGetAmountFor()
		{
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.Charges.UpdateLineChargeFor("AAA", 100.25m);
			AssertEquals("Amount for valid code", 100.25m, statementLine.Charges.GetAmountFor("AAA"));
			AssertEquals("Amount for invalid code", 0m, statementLine.Charges.GetAmountFor("BBB"));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusStatementLineChargeCollection(Factory.New<CusStatementHeader>().StatementLines.AddNew());
		}
	}
}
