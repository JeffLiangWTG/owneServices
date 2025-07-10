using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test;

[TestedType(typeof(FilterableClientLicencePriceHeaderCollection))]
public class FilterableClientLicencePriceHeaderCollectionTest : NonPersistentBusinessObjectTestCase
{
	public void TestApplyFiltersSetsAdditionalFiltersOnParent()
	{
		var parent = ClientLicencePriceHeaderCollection();
		var filter = new ZQuery(ClientLicencePriceHeaderSchema.L6_UseStandardDiscount, true);

		var filterable = new FilterableClientLicencePriceHeaderCollection(parent, new MockFilterStripBusinessObject(filter));

		filterable.ApplyFilter();

		AssertEquals(filter, parent.AdditionalFilter);
	}

	public void TestApplyFiltersClearsAdditionalFiltersOnParentWhenFilterHasNoResult()
	{
		var parent = ClientLicencePriceHeaderCollection();
		var filter = new ZQuery { IsNoResultQuery = true };

		var filterable = new FilterableClientLicencePriceHeaderCollection(parent, new MockFilterStripBusinessObject(filter));

		filterable.ApplyFilter();

		AssertEquals(new ZQuery(), parent.AdditionalFilter);
	}

	public void TestClearFiltersClearsAdditionalFiltersOnParent()
	{
		var parent = ClientLicencePriceHeaderCollection();

		var filterable = new FilterableClientLicencePriceHeaderCollection(parent,
			new MockFilterStripBusinessObject(new ZQuery(ClientLicencePriceHeaderSchema.L6_UseStandardDiscount, true)));

		filterable.ClearFilter();

		AssertEquals(new ZQuery(), parent.AdditionalFilter);
	}

	ClientLicencePriceHeaderCollection ClientLicencePriceHeaderCollection()
	{
		var company = Factory.NewWithValidTestData<LicenceCompany>();
		var parent = new ClientLicencePriceHeaderCollection(company);
		return parent;
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var company = Factory.NewWithValidTestData<LicenceCompany>();
		return new ClientLicencePriceHeaderCollection(company).AsFilterable;
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
}
