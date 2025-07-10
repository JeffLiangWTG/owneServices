using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CusEngineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCEG_EngineNumber()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(engine.CEG_EngineNumberInfo);
	}

	public void TestCheckCEG_CapacityCC()
	{
		ValidationTestHelper.AssertValueCannotBeNegativeMessageError(engine.CEG_CapacityCCInfo);
	}

	public void TestCheckCEG_CapacityCCIsValidZDecimal()
	{
		engine.CEG_CapacityCC = 999999.99m;
		AssertHasError(engine.CEG_CapacityCCInfo, @"The number 999,999.99 is too large, the maximum value allowed for Engine Capacity (L) is 9,999.99.");
	}

	protected override void SetUp()
	{
		base.SetUp();
		engine = Factory.New<CusEngine>();
	}

	CusEngine engine;
}
