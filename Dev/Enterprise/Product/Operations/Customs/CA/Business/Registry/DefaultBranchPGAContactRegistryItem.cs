using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Registry
{
	public class DefaultBranchPGAContactRegistryItem : GuidRegistryItem
	{
		public DefaultBranchPGAContactRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, GuidRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, Guid defaultValue)
			: base(new DefaultBranchPGAContactImpl(name, category, caption, hint, editorInfo, storage, options, defaultValue))
		{
		}

		class DefaultBranchPGAContactImpl : RegistryItemImpl
		{
			public DefaultBranchPGAContactImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, object defaultValue)
				: base(name, category, caption, hint, new DefaultBranchPGAContactRegistryDataType(), editorInfo, storage, options, defaultValue)
			{
			}
		}
	}
}
