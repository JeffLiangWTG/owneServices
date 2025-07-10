using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing;

[TestedType(typeof(TemporaryStorageFilterStripBusinessObject))]
class TemporaryStorageFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
	{
		return new TemporaryStorageFilterStripBusinessObject();
	}

	public void TestGetFilterInflators()
	{
		var filterStrip = new TemporaryStorageFilterStripBusinessObjectForTest();
		var filterInflators = filterStrip.GetFilterInflators_Exposed();
		AssertEquals(0, filterInflators.Count);
	}

	sealed class TemporaryStorageFilterStripBusinessObjectForTest : TemporaryStorageFilterStripBusinessObject
	{
		public List<IFilterInflator> GetFilterInflators_Exposed() => GetFilterInflators();
	}
}
