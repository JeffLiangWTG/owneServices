using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	[TestedType(typeof(DummyFilterStripBusinessObjectUS))]
	class ForwardingShipmentModuleCustomsFiltersObjectUSTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new DummyFilterStripBusinessObjectUS();
	}

	class DummyFilterStripBusinessObjectUS : CountrySpecificDummyFilterStripBusinessObject
	{
		protected override string Country => Core.Constants.CountryCodes.UnitedStates;
	}
}
