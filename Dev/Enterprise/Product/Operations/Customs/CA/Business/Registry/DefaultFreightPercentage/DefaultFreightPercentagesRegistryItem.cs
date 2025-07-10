
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Registry
{
	public class DefaultFreightPercentagesRegistryItem : StronglyTypedRegistryItem<DefaultFreightPercentageCollection>
	{
		public DefaultFreightPercentagesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultFreightPercentagesRegistryDataType(), storage))
		{
		}

		public DefaultFreightPercentagesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultFreightPercentagesRegistryDataType(), storage, options))
		{
		}

		public new DefaultFreightPercentageCollection Value
		{
			get { return base.Value; }
		}
	}
}
