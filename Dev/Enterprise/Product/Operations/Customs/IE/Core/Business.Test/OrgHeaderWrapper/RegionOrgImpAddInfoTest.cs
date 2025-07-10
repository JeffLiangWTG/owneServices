using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(RegionOrgImpAddInfo))]
	class RegionOrgImpAddInfoTest : EU.Business.Testing.EUOrgImpAddInfoAbstractTest
	{
		public void TestLookup()
		{
			AssertType<RegionOrgImpAddInfoLookups>(((RegionOrgImpAddInfo)GetNewBusinessObject()).Lookups);
		}

		public void TestZO_OtherDeferTypeCaption()
		{
			var businessObject = (RegionOrgImpAddInfo)GetNewBusinessObject();
			AssertEquals("Payment Method", businessObject.ZO_OtherDeferTypeInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		protected override BusinessObject GetNewBusinessObject() => new RegionOrgImpAddInfo(Factory);
	}
}
