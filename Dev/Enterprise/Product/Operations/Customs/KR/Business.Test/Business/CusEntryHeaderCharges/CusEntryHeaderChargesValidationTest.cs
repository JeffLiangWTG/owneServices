using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusEntryHeaderChargesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckC1_ChargeType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var charge1 = entry.Charges.AddNew("DTY");
			var charge2 = entry.Charges.AddNew();
			charge2.C1_IsLandedCostOnly = false;
			charge2.C1_ChargeType = "VAT";

			var charge3 = entry.Charges.AddNew();
			charge3.C1_ChargeType = "VAT";
			AssertNoErrors(charge3.C1_ChargeTypeInfo);
		}
	}
}
