using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionAddressFieldSupporterTest : TestCaseWithFactory
	{
		public void TestDefaultStrategies()
		{
			OperationalActionAddressFieldSupporter supporter = new OperationalActionAddressFieldSupporter("fieldName", false, AddressType.NoDefault, (f) => new OrganisationsFindBoxCollection(Factory));
			FieldDefaultingStrategyList strategies = supporter.GetDefaultingStrategies();
			IFieldDefaultingStrategy[] actual = new IFieldDefaultingStrategy[strategies.Count];
			strategies.CopyTo(actual, 0);
			AssertEquals(supporter, strategies.FieldSupporter);
			AssertContainsExactElementsInAnyOrder("No supported strategies", Array.Empty<Type>(), Array.ConvertAll(actual, (s) => s.GetType()));
		}

		public void TestAsFilterStringCore()
		{
			OrgAddress address = Factory.New<OrgHeader>().MainAddress;
			OperationalActionAddressFieldSupporter supporter = new OperationalActionAddressFieldSupporter("FieldName", false, AddressType.APM, (f) => new OrganisationsFindBoxCollection(f));
			AssertEquals(null, supporter.AsFilterString(address.PK, Factory));
		}
	}
}
