using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CodeDescriptionPairLists.Testing
{
	public class DeltaGFallbackStatusListTest : TestCase
	{
		public void TestIsStatusInFallbackAndNotRegularised()
		{
			AssertEquals(false, DeltaGFallbackStatusList.IsStatusInFallbackAndNotRegularised(DeltaGFallbackStatusList.Codes.ERR));
			AssertEquals(true, DeltaGFallbackStatusList.IsStatusInFallbackAndNotRegularised(DeltaGFallbackStatusList.Codes.PDS));
			AssertEquals(true, DeltaGFallbackStatusList.IsStatusInFallbackAndNotRegularised(DeltaGFallbackStatusList.Codes.PPS));
			AssertEquals(true, DeltaGFallbackStatusList.IsStatusInFallbackAndNotRegularised(DeltaGFallbackStatusList.Codes.PPW));
			AssertEquals(false, DeltaGFallbackStatusList.IsStatusInFallbackAndNotRegularised(DeltaGFallbackStatusList.Codes.RGA));
			AssertEquals(false, DeltaGFallbackStatusList.IsStatusInFallbackAndNotRegularised(DeltaGFallbackStatusList.Codes.RGM));
			AssertEquals(false, DeltaGFallbackStatusList.IsStatusInFallbackAndNotRegularised(DeltaGFallbackStatusList.Codes.Unset));
			AssertEquals(false, DeltaGFallbackStatusList.IsStatusInFallbackAndNotRegularised(ZString.Empty));
			AssertEquals(false, DeltaGFallbackStatusList.IsStatusInFallbackAndNotRegularised("XXX"));
		}
	}
}
