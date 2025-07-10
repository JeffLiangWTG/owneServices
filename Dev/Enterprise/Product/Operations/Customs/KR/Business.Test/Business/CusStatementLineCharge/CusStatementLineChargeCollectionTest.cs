using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusStatementLineChargeCollection))]
	sealed class CusStatementLineChargeCollectionTest : ActiveBusinessObjectCollectionTestCase<CusStatementLineChargeCollection>
	{
		protected override CusStatementLineChargeCollection GetCollectionToTest()
		{
			return new CusStatementLineChargeCollection(Factory.New<CusStatementHeader>().StatementLines.AddNew());
		}

		public void TestStringIndexer()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			var statementLine = statementHeader.StatementLines.AddNew();
			var charg1 = statementLine.Charges.AddNew();
			charg1.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;
			charg1.B4_ChargeAmount = 100m;

			AssertEquals(100m, statementLine.Charges["VFV"].B4_ChargeAmount);
			AssertNull(statementLine.Charges["AAA"]);
		}

		public void TestGetChargeAmount()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			var statementLine = statementHeader.StatementLines.AddNew();
			var chargeVFV = statementLine.Charges.AddNew();
			chargeVFV.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;
			chargeVFV.B4_ChargeAmount = 100m;

			var chargePMT = statementLine.Charges.AddNew();
			chargePMT.B4_ChargeType = ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration;
			chargePMT.B4_ChargeAmount = 200m;

			AssertEquals(100m, statementLine.Charges.GetChargeAmount(ChargeTypeList.Codes.ValueForVAT));
			AssertEquals(200m, statementLine.Charges.GetChargeAmount(ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration));
			AssertEquals(0m, statementLine.Charges.GetChargeAmount(ChargeTypeList.Codes.AgricultureTax));
		}
	}
}
