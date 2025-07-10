using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrgAddressCapability : DocumentWrapper
	{
		DocOrgAddressCapability(OrgAddressCapabilityWrapper orgAddressCapability, BusinessObjectFactory factory)
			: base(orgAddressCapability, factory)
		{
		}

		public static DocOrgAddressCapability New(OrgAddressCapabilityWrapper orgAddressCapability, BusinessObjectFactory factory)
		{
			return (orgAddressCapability != null) ? new DocOrgAddressCapability(orgAddressCapability, factory) : null;
		}

		OrgAddressCapabilityWrapper OrgAddressCapability
		{
			get { return (OrgAddressCapabilityWrapper)WrappedObject; }
		}

		public ZString AddressType
		{
			get { return OrgAddressCapability.AddressCapabilityType; }
		}

		public ZBool Enabled
		{
			get { return OrgAddressCapability.Enabled; }
		}

		public ZBool IsMain
		{
			get { return OrgAddressCapability.Main; }
		}

		public override string ToString()
		{
			return OrgAddressCapability.AddressCapabilityType;
		}
	}
}
