using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(JobDocAddressesFilterBusinessObject))]
	sealed class JobDocAddressesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestAddressDescriptionFilter()
		{
			var address1 = Factory.NewWithValidTestData<JobDocAddress>();
			address1.E2_AddressType = "MAN";
			var address2 = Factory.NewWithValidTestData<JobDocAddress>();
			address2.E2_AddressType = "SUD";
			Factory.Save();

			var filter = new JobDocAddressesFilterBusinessObject();
			((ModuleTextFilter)filter[JobDocAddressesFilterBusinessObject.Schema.AddressDescription]).Property = "MAN";
			((ModuleTextFilter)filter[JobDocAddressesFilterBusinessObject.Schema.AddressDescription]).IsActive = true;

			var collection = new JobDocAddressCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(address1, collection);
			AssertCollectionNotContains(address2, collection);
		}

		public void TestOrganisationFilter()
		{
			var filter = new JobDocAddressesFilterBusinessObject();
			var organisationFilter = (ModuleGuidFilter)filter[JobDocAddressesFilterBusinessObject.Schema.Organization];
			AssertEquals(FilterCategories.Organisations, organisationFilter.Category);
			AssertEquals(ModuleIDs.Organisation, organisationFilter.ModuleId);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobDocAddressesFilterBusinessObject();
	}
}
