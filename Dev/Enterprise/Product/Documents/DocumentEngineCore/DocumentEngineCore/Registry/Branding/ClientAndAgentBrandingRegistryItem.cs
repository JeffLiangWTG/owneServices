using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public abstract class ClientAndAgentBrandingRegistryItem : StronglyTypedRegistryItem<ClientAndAgentBrandingCollection>
	{
		public ClientAndAgentBrandingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, dataType, storage, RegistryOptions.Default))
		{
		}

		public ClientAndAgentBrandingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, dataType, storage, options))
		{
		}

		#region ClientAndAgentBrandingRegistryDataType

		internal protected abstract class ClientAndAgentBrandingRegistryDataType<T> : FallbackMergedRegistryBusinessObjectCollectionDataType<T>
			where T : ClientAndAgentBrandingCollection
		{
			protected ClientAndAgentBrandingRegistryDataType() { }

			public override bool IsDeserializedDataAlive(object value)
			{
				ClientAndAgentBrandingCollection brandingCollection;

				return base.IsDeserializedDataAlive(value)
					&& (brandingCollection = value as ClientAndAgentBrandingCollection) != null
					&& !brandingCollection.Cast<ClientAndAgentBrandingBusinessObject>().Any(brandingBizo => brandingBizo.Image != null && brandingBizo.Image.IsDisposed());
			}
		}

		#endregion
	}
}
