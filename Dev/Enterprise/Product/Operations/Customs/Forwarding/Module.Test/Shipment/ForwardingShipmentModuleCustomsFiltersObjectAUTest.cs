using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	[TestedType(typeof(CountrySpecificDummyFilterStripBusinessObjectAU))]
	class ForwardingShipmentModuleCustomsFiltersObjectAUTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CountrySpecificDummyFilterStripBusinessObjectAU();
	}

	class CountrySpecificDummyFilterStripBusinessObjectAU : CountrySpecificDummyFilterStripBusinessObject
	{
		protected override string Country => Core.Constants.CountryCodes.Australia;
	}
}
