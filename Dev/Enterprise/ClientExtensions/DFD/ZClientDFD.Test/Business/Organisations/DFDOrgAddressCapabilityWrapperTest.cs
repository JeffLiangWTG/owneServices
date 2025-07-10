using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Business.Organisations.Testing
{
	[TestedType(typeof(DFDOrgAddressCapabilityWrapper))]
	class DFDOrgAddressCapabilityWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var parentOrgAddress = Factory.New<OrgAddress>();
			return new DFDOrgAddressCapabilityWrapper(parentOrgAddress);
		}
	}
}
