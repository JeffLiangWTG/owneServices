using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusStatementLineGroupFinancialDetail))]
	sealed class CusStatementLineGroupFinancialDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestChargeTypeDescription()
		{
			var statementLineGroupFinancialDetail = Factory.New<CusStatementLineGroupFinancialDetail>();

			statementLineGroupFinancialDetail.B11_Type = string.Empty;
			AssertEquals(string.Empty, statementLineGroupFinancialDetail.ChargeTypeDescription);

			statementLineGroupFinancialDetail.B11_Type = "XXX";
			AssertEquals("XXX", statementLineGroupFinancialDetail.ChargeTypeDescription);

			statementLineGroupFinancialDetail.B11_Type = PostingJournalTypeList.Codes.TotalCredits;
			AssertEquals(PostingJournalTypeList.Descriptions.TotalCredits, statementLineGroupFinancialDetail.ChargeTypeDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusStatementHeader>();
			var group = header.LineGroupCollection.AddNew();

			return group.FinancialDetailCollection.AddNew();
		}
	}
}
