using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class GmailOAuth2JsonFileRegistryItem : StronglyTypedRegistryItem<GmailOAuth2JsonFile>
	{
		public GmailOAuth2JsonFileRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new GmailOAuth2JsonFileRegistryDataType(), new GmailOAuth2JsonFileRegistryEditorInfo(), storage, options, null, true))
		{
		}
	}
}
