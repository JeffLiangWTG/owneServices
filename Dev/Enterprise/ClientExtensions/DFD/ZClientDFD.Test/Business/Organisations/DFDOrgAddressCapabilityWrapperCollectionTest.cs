using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Business.Organisations.Testing
{
	[TestedType(typeof(DFDOrgAddressCapabilityWrapperCollection))]
	class DFDOrgAddressCapabilityWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DFDOrgAddressCapabilityWrapperCollection>
	{
		protected override DFDOrgAddressCapabilityWrapperCollection GetCollectionToTest()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			return new DFDOrgAddressCapabilityWrapperCollection(address);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			return new DFDOrgAddressCapabilityWrapper(address);
		}
	}
}
