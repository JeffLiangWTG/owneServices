using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.DFD.Business.Organisations
{
	class DFDOrgAddressCapabilityWrapperCollection : OrgAddressCapabilityWrapperCollection
	{
		public DFDOrgAddressCapabilityWrapperCollection(OrgAddress parentAddressCapability)
			: base(parentAddressCapability)
		{ }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DFDOrgAddressCapabilityWrapper(Master);
		}
	}
}
