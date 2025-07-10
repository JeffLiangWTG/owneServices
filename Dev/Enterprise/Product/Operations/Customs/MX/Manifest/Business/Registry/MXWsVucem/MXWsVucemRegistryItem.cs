using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class MXWsVucemRegistryItem : StronglyTypedRegistryItem<MXWsVucem>
	{
		public MXWsVucemRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions registryOptions,
			MXWsVucem defaultValue
		) : base(new RegistryItemImpl(
			name, category, caption, hint, new MXWsVucemRegistryDataType(), storage, registryOptions, defaultValue
		))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.MX.Manifest.GUI.MXWsVucemRegistryItemEditor, Enterprise.Customs.MX.Manifest.GUI")]
	public class MXWsVucemRegistryDataType : NonPersistentBusinessObjectRegistryDataType<MXWsVucem>
	{
		public MXWsVucemRegistryDataType()
		{
		}
	}
}
