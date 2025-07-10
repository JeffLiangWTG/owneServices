using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class EDIGenericChargeTest : TestCaseWithFactory
	{
		public void TestTranslatable()
		{
			var obj = Factory.New<EDIGenericCharge>();
			obj.VC_Description = "A";
			var resKey = obj.VC_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(obj, "A").ResourceKey;
			AssertEquals("A", obj.VC_DescriptionMultilingual);
			using (Res.TemporarilySwitchLanguage("CHS"))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "B"));
				AssertEquals("B", obj.VC_DescriptionMultilingual);
			}
		}
	}
}