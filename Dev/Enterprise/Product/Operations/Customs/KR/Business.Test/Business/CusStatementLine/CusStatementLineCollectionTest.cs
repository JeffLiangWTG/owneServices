using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusStatementLineCollection))]
	sealed class CusStatementLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusStatementLineCollection>
	{
		protected override CusStatementLineCollection GetCollectionToTest()
		{
			return new CusStatementLineCollection(Factory.New<CusStatementHeader>());
		}

		public void TestGetTotalAmount()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_CustomsFeesTotal = 200m;
			var charg1 = statementLine.Charges.AddNew();
			charg1.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;
			charg1.B4_ChargeAmount = 100m;

			var statementLine2 = statementHeader.StatementLines.AddNew();
			statementLine2.B3_CustomsFeesTotal = 2000m;
			var charg2 = statementLine2.Charges.AddNew();
			charg2.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;
			charg2.B4_ChargeAmount = 1000m;

			AssertEquals(1100m, statementHeader.StatementLines.GetTotalChargeAmount(ChargeTypeList.Codes.ValueForVAT));
			AssertEquals(2200m, statementHeader.StatementLines.GetTotalCustomsFeesTotal());
		}
	}
}
