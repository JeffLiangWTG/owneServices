using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderChargesValidation))]
	sealed class CusEntryHeaderChargesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckC1_ChargeType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var charge1 = entry.Charges.AddNew("AAA", 10m);
			var charge2 = entry.Charges.AddNew();
			charge2.C1_ChargeType = "AAA";
			AssertNoMessageErrors(charge2.C1_ChargeTypeInfo);
			charge2.C1_ChargeType = "BBB";
			AssertNoMessageErrors(charge2.C1_ChargeTypeInfo);
		}
	}
}
