using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPCSCusTempStorageDec))]
	class CUSPCSCusTempStorageDecValidationTest : CusTempStorageDecValidationTest<CUSPCSCusTempStorageDec>
	{
		public void TestShouldValidateRegistrationNumberLengthAndMrnFormat()
		{
			var validation = GetCusTempStorageDecToTest().Validation;
			AssertEquals(false, validation.ShouldValidateRegistrationNumberLengthAndMrnFormat);
		}

		protected override CUSPCSCusTempStorageDec GetCusTempStorageDecToTest() => Factory.NewWithValidTestData<CUSPCSCusTempStorageDec>();
	}
}
