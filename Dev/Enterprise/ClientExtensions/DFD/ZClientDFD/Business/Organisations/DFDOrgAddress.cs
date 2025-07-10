using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.DFD.Business.Organisations
{
	class DFDOrgAddress : OrgAddress
	{
		public DFDOrgAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public override OrgAddressCapabilityWrapperCollection AddressCapability
		{
			get
			{
				if (addressCapability == null)
				{
					addressCapability = new DFDOrgAddressCapabilityWrapperCollection(this);
					RegisterEditableChildObject(addressCapability);
				}
				return addressCapability;
			}
		}

		DFDOrgAddressCapabilityWrapperCollection addressCapability;

		protected override bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			string propertyName = property.Name;
			if (propertyName == "OA_CompanyNameOverride" ||
				propertyName == "OA_Address1" ||
				propertyName == "OA_Address2" ||
				propertyName == "OA_City" ||
				propertyName == "OA_PostCode" ||
				propertyName == "OA_State" ||
				propertyName == "OA_Phone" ||
				propertyName == "OA_Phone_Formatted" ||
				propertyName == "OA_RL_NKRelatedPortCode" ||
				propertyName == "OA_Mobile" ||
				propertyName == "OA_Mobile_Formatted" ||
				propertyName == "OA_Fax" ||
				propertyName == "OA_Fax_Formatted" ||
				propertyName == "OA_Email" ||
				propertyName == "OA_Language")
			{
				if (AddressCapability.GetCapabilityEnabled(OrgAddressType.Office.Code) && Header != null && (((DFDOrgHeader)Header).IsARAPAccount))
				{
					shouldBeReadOnly = !DFDSecurityCheckpoints.OrgModifyMainAddressARAP.IsAllowed;
				}
				else if (AddressCapability.GetCapabilityEnabled(OrgAddressType.Postal.Code))
				{
					shouldBeReadOnly = !DFDSecurityCheckpoints.OrgModifyPostalAddress.IsAllowed;
				}
				else if (AddressCapability.GetCapabilityEnabled(OrgAddressType.Payables.Code) || AddressCapability.GetCapabilityEnabled(OrgAddressType.Receivables.Code))
				{
					shouldBeReadOnly = !DFDSecurityCheckpoints.OrgModifyARAPAddress.IsAllowed;
				}
			}
			return shouldBeReadOnly || base.GetReadOnlySecurity(property);
		}
	}
}
