using System;
using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.MarketingManager.Module;
using Enterprise.MarketingManager.Module.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.Module
{
	[TestedType(typeof(CRMGlbCompanyCampaignModule))]
	class EDICRMGlbCompanyCampaignModuleTest : CRMGlbCompanyCampaignModule_Test
	{
		public void TestFilterBusinessObject()
		{
			using (EDIGlbCompanyCampaignModuleForTest module = new EDIGlbCompanyCampaignModuleForTest())
			{
				AssertEquals(typeof(EDIGlbCompanyCampaignFilterBusinessObject), module.FilterBusinessObject.GetType());
			}
		}

		class EDIGlbCompanyCampaignModuleForTest : EDICRMGlbCompanyCampaignModule
		{
			public new FilterBusinessObject FilterBusinessObject
			{
				get
				{
					return base.FilterBusinessObject;
				}
			}
		}

		protected override Type ExpectedCampaignTypeListType
		{
			get
			{
				return typeof(EDICampaignTypeList);
			}
		}
	}
}
