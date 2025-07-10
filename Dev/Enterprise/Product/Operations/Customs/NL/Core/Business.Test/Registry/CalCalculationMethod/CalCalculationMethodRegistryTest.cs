using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CalCalculationMethodRegistry))]
sealed class CalCalculationMethodRegistryTest : RegistryBusinessObjectTemplateTestCase<CalCalculationMethodRegistry>
{
	public void TestValidBasedOnDutiesVatDefault()
	{
		AssertEquals(CalCalculationMethodRegistry.BasedOnDutiesVatDefault, true);
	}

	public void TestCalculationMethodNameReadOnly()
	{
		Assert(new CalCalculationMethodRegistry().CalculationMethodNameInfo.ReadOnly);
	}

	public void TestCalculationMethodValueReadOnly()
	{
		var registry = new CalCalculationMethodRegistry();
		CombineAssertions(() =>
		{
			registry.CalculationMethodName = CalculationMethodList.Codes.DUT;
			Assert("BasedOnDutiesVat", registry.CalculationMethodValueInfo.ReadOnly);
			registry.CalculationMethodName = CalculationMethodList.Codes.WGT;
			Assert("BasedOnWeight", registry.CalculationMethodValueInfo.ReadOnly);
			registry.CalculationMethodName = CalculationMethodList.Codes.DEF;
			Assert("DefaultAmountPerEntry", !registry.CalculationMethodValueInfo.ReadOnly);
		});
	}

	public void TestValidateCalculationMethodValue()
	{
		var registry = new CalCalculationMethodRegistry();

		CombineAssertions(() =>
		{
			registry.CalculationMethodName = CalculationMethodList.Codes.DUT;
			registry.CalculationMethodValue = 0;
			AssertNoMessageErrors("No error: " + registry.CalculationMethodName, registry.CalculationMethodValueInfo);

			registry.CalculationMethodName = CalculationMethodList.Codes.WGT;
			registry.CalculationMethodValue = 0;
			AssertNoMessageErrors("No error: " + registry.CalculationMethodName, registry.CalculationMethodValueInfo);

			registry.CalculationMethodName = CalculationMethodList.Codes.DEF;
			registry.CalculationMethodValue = 0;
			AssertHasMessageErrorContaining("Error: " + registry.CalculationMethodName, registry.CalculationMethodValueInfo, "You have not entered a Value");

			registry.CalculationMethodValue = 1;
			AssertNoMessageErrors("No error: " + registry.CalculationMethodName, registry.CalculationMethodValueInfo);
		});
	}

	public void TestCalculationMethodDescription()
	{
		var registry = new CalCalculationMethodRegistry();
		registry.CalculationMethodName = CalculationMethodList.Codes.DUT;
		AssertEquals(CalculationMethodList.Descriptions.DUT, registry.CalculationMethodDescription);
	}

	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;

	protected override CalCalculationMethodRegistry GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

	protected override CalCalculationMethodRegistry GetBusinessObjectToSerialise()
	{
		BizObj.CalculationMethodName = CalculationMethodList.Codes.DUT;
		BizObj.CalculationMethodValue = CalCalculationMethodRegistry.BasedOnDutiesVatValue;
		BizObj.CalculationMethodDefault = CalCalculationMethodRegistry.BasedOnDutiesVatDefault;
		return BizObj;
	}
}
