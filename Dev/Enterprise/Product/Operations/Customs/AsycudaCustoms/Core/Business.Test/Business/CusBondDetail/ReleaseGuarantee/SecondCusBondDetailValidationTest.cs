using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class SecondCusBondDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPW_BondAmount()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(Factory.New<SecondCusBondDetail>().PW_BondAmountInfo);
		}

		public void TestCheckPW_BondNumber2()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(Factory.New<SecondCusBondDetail>().PW_BondNumber2Info);
		}

		public void TestCheckPW_BondEffectiveDate()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(Factory.New<SecondCusBondDetail>().PW_BondEffectiveDateInfo);
		}
	}
}
