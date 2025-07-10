using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class LocalTransportCompanyBrandingCollectionRegistryItem : StronglyTypedRegistryItem<LocalTransportCompanyBrandingCollection>
	{
		public LocalTransportCompanyBrandingCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new LocalTransportCompanyBrandingRegistryDataType(), storage, options))
		{
		}

		#region LocalTransportCompanyBrandingRegistryDataType

		[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.LocalTransportCompanyBrandingRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
		#if DEBUG
		public
		#endif
		class LocalTransportCompanyBrandingRegistryDataType : FallbackMergedRegistryBusinessObjectCollectionDataType<LocalTransportCompanyBrandingCollection>
		{
		}

		#endregion

		#region FindBrandingForLocalTransportCompany

		public LocalTransportCompanyBranding FindBrandingForLocalTransportCompany(ZGuid localTransportCompanyPK)
		{
			return Value.Cast<LocalTransportCompanyBranding>().FirstOrDefault(binding => binding.LocalTransportCompanyPK == localTransportCompanyPK);
		}

		#endregion
	}
}
