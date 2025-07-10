using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class PrincipalBrandingCollectionRegistryItem : StronglyTypedRegistryItem<PrincipalBrandingCollection>
	{
		public PrincipalBrandingCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new PrincipalBrandingRegistryDataType(), storage))
		{
		}

		public PrincipalBrandingCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new PrincipalBrandingRegistryDataType(), storage, options))
		{
		}

		#region PrincipalBrandingRegistryDataType

		[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.PrincipalBrandingRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
#if DEBUG
		public
#endif
			class PrincipalBrandingRegistryDataType : FallbackMergedRegistryBusinessObjectCollectionDataType<PrincipalBrandingCollection>
		{
		}

		#endregion

		#region FindBrandingForPrincipal

		public PrincipalBranding FindBrandingForPrincipal(ZGuid principalPK)
		{
			foreach (PrincipalBranding binding in Value)
			{
				if (binding.PrincipalPK == principalPK)
				{
					return binding;
				}
			}

			return null;
		}

		#endregion
	}
}
