using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	[TestedType(typeof(DummyFilterStripBusinessObjectGB))]
	class ForwardingShipmentModuleCustomsFiltersObjectGBTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new DummyFilterStripBusinessObjectGB();
	}

	class DummyFilterStripBusinessObjectGB : CountrySpecificDummyFilterStripBusinessObject
	{
		protected override string Country => Core.Constants.CountryCodes.UnitedKingdom;
	}
}
