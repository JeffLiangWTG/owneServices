using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test;

[TestedType(typeof(FilterableClientLicencePriceItemCollection))]
public class FilterableClientLicencePriceItemCollectionTest : NonPersistentBusinessObjectTestCase
{
	public void TestApplyFiltersSetsAdditionalFiltersOnParent()
	{
		var parent = ClientLicencePriceItemCollection();
		var filter = DummyBusinessObjectCodePropertyIsNotSchemaColumn.DummyFilter(SQLComparisonOperator.Contains, new ZString("TestValue"));

		var filterable = new FilterableClientLicencePriceItemCollection(parent,
			new MockFilterStripBusinessObject(filter));

		filterable.ApplyFilter();

		AssertEquals(filter, parent.AdditionalFilter);
	}

	public void TestApplyFiltersClearsAdditionalFiltersOnParentWhenFilterHasNoResult()
	{
		var parent = ClientLicencePriceItemCollection();
		var filter = new ZQuery { IsNoResultQuery = true };

		var filterable = new FilterableClientLicencePriceItemCollection(parent, new MockFilterStripBusinessObject(filter));

		filterable.ApplyFilter();

		AssertEquals(new ZQuery(), parent.AdditionalFilter);
	}

	public void TestClearFiltersClearsAdditionalFiltersOnParent()
	{
		var parent = ClientLicencePriceItemCollection();

		var filterable = new FilterableClientLicencePriceItemCollection(parent,
			new MockFilterStripBusinessObject(DummyBusinessObjectCodePropertyIsNotSchemaColumn.DummyFilter(SQLComparisonOperator.Contains, new ZString("TestValue"))));

		filterable.ClearFilter();

		AssertEquals(new ZQuery(), parent.AdditionalFilter);
	}

	ClientLicencePriceItemCollection ClientLicencePriceItemCollection()
	{
		var priceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
		var parent = new ClientLicencePriceItemCollection(priceHeader);
		return parent;
	}

	class MockFilterStripBusinessObject : FilterStripBusinessObject
	{
		public MockFilterStripBusinessObject(ZQuery filter)
		{
			Filter = filter;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			throw new System.NotImplementedException();
		}

		public override ZQuery Filter { get; }
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var parent = ClientLicencePriceItemCollection();
		var filter = new MockFilterStripBusinessObject(DummyBusinessObjectCodePropertyIsNotSchemaColumn.DummyFilter(SQLComparisonOperator.Contains, new ZString("TestValue")));

		var result = new FilterableClientLicencePriceItemCollection(parent,
			filter);
		return result;
	}
}
