using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class NctsMessageProviderHelperTest : TestCase
	{
		public void TestConvertBooleanToN1String()
		{
			CombineAssertions(() =>
			{
				AssertNull("Empty", NctsMessageProviderHelper.ConvertStringToNullableInt(ZString.Empty));
				AssertEquals("Valid", 1, NctsMessageProviderHelper.ConvertStringToNullableInt("1"));
				AssertNull("Invalid", NctsMessageProviderHelper.ConvertStringToNullableInt("X"));
			});
		}
	}
}
