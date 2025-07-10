using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class CalculateInsuranceNonPersistentBizObjValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckInsurancePercentage()
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(calculateInsuranceBizObj.InsurancePercentageInfo);
		}

		public void TestCheckDutiablePercent()
		{
			const string message = "Percentage value should be between 0 and 100";

			calculateInsuranceBizObj.DutiablePercent = 101;
			AssertHasErrorContaining(calculateInsuranceBizObj.DutiablePercentInfo, message);

			calculateInsuranceBizObj.DutiablePercent = 100;
			AssertNoErrorContaining(calculateInsuranceBizObj.DutiablePercentInfo, message);

			calculateInsuranceBizObj.DutiablePercent = -1;
			AssertHasErrorContaining(calculateInsuranceBizObj.DutiablePercentInfo, message);

			calculateInsuranceBizObj.DutiablePercent = 0;
			AssertNoErrorContaining(calculateInsuranceBizObj.DutiablePercentInfo, message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
			calculateInsuranceBizObj = new CalculateInsuranceBizObj(chargeFactory, invoice);
		}

		CalculateInsuranceBizObj calculateInsuranceBizObj;
	}
}
