using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCargoStatusListTest : TestCase
	{
		public void TestMapToCWCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Should be Empty", ZString.Empty, BRCargoStatusList.MapToCWCode(""));
				AssertEquals("Should be Empty", ZString.Empty, BRCargoStatusList.MapToCWCode("XX"));
				AssertEquals("Should be 4", BRCargoStatusList.Codes.Unbound, BRCargoStatusList.MapToCWCode("DESVINCULADA"));
				AssertEquals("Should be 5", BRCargoStatusList.Codes.Delivered, BRCargoStatusList.MapToCWCode("ENTREGUE"));
				AssertEquals("Should be 6", BRCargoStatusList.Codes.Moored, BRCargoStatusList.MapToCWCode("ATRACADA"));
				AssertEquals("Should be 7", BRCargoStatusList.Codes.Bound, BRCargoStatusList.MapToCWCode("VINCULADA"));
			});
		}
	}
}
