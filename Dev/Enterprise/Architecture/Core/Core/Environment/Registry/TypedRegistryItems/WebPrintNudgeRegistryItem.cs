using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebPrintNudgeRegistryItem : StronglyTypedRegistryItem<WebPrintNudge>
	{
		public WebPrintNudgeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, WebPrintNudge nudge)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebPrintNudgeRegistryDataType(nudge), new WebPrintNudgeEditorInfo(), storage, options, nudge, true))
		{
		}
	}
}
