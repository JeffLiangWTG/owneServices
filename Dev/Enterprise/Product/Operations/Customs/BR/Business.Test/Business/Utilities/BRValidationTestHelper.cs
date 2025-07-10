using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Testing
{
	public static class BRValidationTestHelper
	{
		public static void CheckPercentageProperties(ZPropertyInfo propertyInfo)
		{
			propertyInfo.Value = ConvertValueToIZType(50m);
			TestCaseWithFactory.AssertNoError(propertyInfo, "Percentage should be a value between 0 and 100.");

			propertyInfo.Value = ConvertValueToIZType(150m);
			TestCaseWithFactory.AssertHasError(propertyInfo, "Percentage should be a value between 0 and 100.");

			propertyInfo.Value = ConvertValueToIZType(-10m);
			TestCaseWithFactory.AssertHasError(propertyInfo, "Percentage should be a value between 0 and 100.");
		}

		static IZType ConvertValueToIZType(ZDecimal value) => value;
	}
}
