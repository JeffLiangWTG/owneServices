using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrgAddressCapapabilityCollection : DocumentWrapperCollection
	{
		public DocOrgAddressCapapabilityCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocOrgAddressCapapabilityCollection(OrgAddressCapabilityWrapperCollection collectionSource, BusinessObjectFactory factory)
			: base(collectionSource, factory)
		{
		}

		public new DocOrgAddressCapability this[int index]
		{
			get { return (DocOrgAddressCapability)base[index]; }
		}

		public ZBool GetCapabilityEnabled(string type)
		{
			foreach (DocOrgAddressCapability oAC in this)
			{
				if (oAC.AddressType == type && oAC.Enabled)
				{
					return ZBool.True;
				}
			}
			return ZBool.False;
		}
	}
}
