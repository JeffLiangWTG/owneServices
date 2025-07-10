using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	[TestedType(typeof(DummyFilterStripBusinessObjectSG))]
	class ForwardingShipmentModuleCustomsFiltersObjectSGTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new DummyFilterStripBusinessObjectSG();
	}

	class DummyFilterStripBusinessObjectSG : CountrySpecificDummyFilterStripBusinessObject
	{
		protected override string Country => Core.Constants.CountryCodes.Singapore;
	}
}
