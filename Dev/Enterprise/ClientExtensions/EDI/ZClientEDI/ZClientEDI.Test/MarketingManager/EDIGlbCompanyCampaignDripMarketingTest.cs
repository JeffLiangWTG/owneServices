using System.Collections.Generic;
using Enterprise.Client.EDI.MarketingManager.Business;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.Testing;

[TestedType(typeof(GlbCompanyCampaignDripMarketing))]
public class EDIGlbCompanyCampaignDripMarketingTest : GlbCompanyCampaignDripMarketingTest
{
	protected override ModuleIdentifier ExpectedModuleID
	{
		get
		{
			return ModuleIDs.DripMarketingFilterRuleEDI;
		}
	}
}

[TestedType(typeof(EDIGlbCompanyCampaignDripMarketing))]
public class EDIGlbCompanyCampaignDripMarketingRelatedFilterTest : RelatedModuleFilterSupportableTestCase<EDIGlbCompanyCampaignDripMarketing>
{
	protected override IEnumerable<FilterRuleTestSet> GetFilterRules(EDIGlbCompanyCampaignDripMarketing businessObject)
	{
		return new[] { new FilterRuleTestSet(null, () => businessObject.FilterRule, "EDIGlbCompanyCampaignContactFilterBusinessObject") };
	}

	protected override void ValidateBusinessObject(EDIGlbCompanyCampaignDripMarketing businessObject)
	{
		businessObject.Validation.ValidateAll();
	}

	protected override string FilterDescriptionForValidationTest => "Contact Details Verified By";

	protected override EDIGlbCompanyCampaignDripMarketing GetNewBusinessObject()
	{
		var touch = Factory.NewWithValidTestData<EDIGlbCompanyCampaign>();
		var pivot = Factory.NewWithValidTestData<EDIGlbCompanyCampaignDripMarketing>();
		pivot.GCD_G0_NextTouch = touch.PK;

		return pivot;
	}
}
