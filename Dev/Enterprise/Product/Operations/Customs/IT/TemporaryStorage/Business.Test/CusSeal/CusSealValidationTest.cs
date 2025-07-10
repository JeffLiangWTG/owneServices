using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class CusSealValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckBK_SealNumber()
	{
		const string expectedMessage = "The field does not match validation pattern [A-Z0-9]{1,20}: it must contain only uppercase letters and numbers, and have length from 1 to 20 characters";

		seal.BK_SealNumber = "";
		AssertNoMessageError("Valid input, empty string", seal.BK_SealNumberInfo, expectedMessage);

		seal.BK_SealNumber = "A";
		AssertNoMessageError("Valid input, seal contains uppercase letters", seal.BK_SealNumberInfo, expectedMessage);

		seal.BK_SealNumber = "TEST";
		AssertNoMessageError("Valid input, seal contains uppercase letters and numbers", seal.BK_SealNumberInfo, expectedMessage);

		seal.BK_SealNumber = "TEST1234";
		AssertNoMessageError("Valid input, seal contains uppercase letters", seal.BK_SealNumberInfo, expectedMessage);

		seal.BK_SealNumber = "123456";
		AssertNoMessageError("Valid input, seal contains numbers", seal.BK_SealNumberInfo, expectedMessage);

		seal.BK_SealNumber = "test012";
		AssertHasMessageError("Invalid input, seal contains lowercase letters", seal.BK_SealNumberInfo, expectedMessage);

		seal.BK_SealNumber = "TEST@1234";
		AssertHasMessageError("Invalid input, seal contains special characters", seal.BK_SealNumberInfo, expectedMessage);

		seal.BK_SealNumber = "TEST 1234";
		AssertHasMessageError("Invalid input, seal contains special characters", seal.BK_SealNumberInfo, expectedMessage);

		seal.BK_SealNumber = "ABCDEFGHIJKLMNOPQRST";
		AssertNoMessageError("Valid input, seal length is equal to 20 characters", seal.BK_SealNumberInfo, expectedMessage);
	}

	protected override void SetUp()
	{
		base.SetUp();
		seal = Factory.New<CusSeal>();
	}
	CusSeal seal;
}
