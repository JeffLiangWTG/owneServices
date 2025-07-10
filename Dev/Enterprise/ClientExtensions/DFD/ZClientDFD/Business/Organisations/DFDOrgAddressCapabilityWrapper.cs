using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.DFD.Business.Organisations
{
	class DFDOrgAddressCapabilityWrapper : OrgAddressCapabilityWrapper
	{
		public DFDOrgAddressCapabilityWrapper(OrgAddress parentAddress)
			: base(parentAddress)
		{ }

		protected override bool GetReadOnlySecurity(System.ComponentModel.PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			if (AddressCapabilityType == OrgAddressType.Payables.Code ||
				AddressCapabilityType == OrgAddressType.Receivables.Code ||
				AddressCapabilityType == OrgAddressType.Office.Code ||
				AddressCapabilityType == OrgAddressType.Postal.Code)
			{
				shouldBeReadOnly = !DFDSecurityCheckpoints.OrgLockAddressARAPOP.IsAllowed;
			}
			return shouldBeReadOnly || base.GetReadOnlySecurity(property);
		}
	}
}
