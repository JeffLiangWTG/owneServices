using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class LicenceEnterpriseKeyCollection : RegistryBusinessObjectCollectionTemplate
	{
		public LicenceEnterpriseKeyCollection()
			: base() { }

		public LicenceEnterpriseKeyCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public LicenceEnterpriseKeyCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public new LicenceEnterpriseKey this[int index]
		{
			get { return (LicenceEnterpriseKey)Elements[index]; }
		}

		public new LicenceEnterpriseKey AddNew()
		{
			return (LicenceEnterpriseKey)base.AddNew();
		}

		public bool ContainsLicenceEnterprise(ZGuid lE_PK)
		{
			foreach (LicenceEnterpriseKey key in this)
			{
				if (key.LE_PK == lE_PK)
				{
					return true;
				}
			}
			return false;
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LicenceEnterpriseKeyCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LicenceEnterpriseKey(CurrentFallbackLevel, CurrentFactory);
		}

		#endregion			
	}
}

