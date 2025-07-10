using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CO.Manifest.Business.Testing.CodeDescriptionPairLists
{
	sealed class CODeliveryModeListTest : TestCaseWithFactory
	{
		public void TestMapDeliveryMode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CY_CY", CODeliveryModeList.Codes._1, CODeliveryModeList.MapDeliveryMode(Core.Constants.DeliveryModes.Codes.CY_CY));
				AssertEquals("CY_CFS", CODeliveryModeList.Codes._2, CODeliveryModeList.MapDeliveryMode(Core.Constants.DeliveryModes.Codes.CY_CFS));
				AssertEquals("CFS_CY", CODeliveryModeList.Codes._3, CODeliveryModeList.MapDeliveryMode(Core.Constants.DeliveryModes.Codes.CFS_CY));
				AssertEquals("CFS_CFS", CODeliveryModeList.Codes._4, CODeliveryModeList.MapDeliveryMode(Core.Constants.DeliveryModes.Codes.CFS_CFS));
				Assert("When deliveryMode is empty, should keep empty", CODeliveryModeList.MapDeliveryMode(ZString.Empty).IsEmpty);
				Assert("When deliveryMode not found, should be empty", CODeliveryModeList.MapDeliveryMode("X").IsEmpty);
			});
		}
	}
}
