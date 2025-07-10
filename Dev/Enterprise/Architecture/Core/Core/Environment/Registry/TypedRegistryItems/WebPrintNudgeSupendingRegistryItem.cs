using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebPrintNudgeSuspendingRegistryItem : StronglyTypedRegistryItem<WebPrintNudgeSuspending>
	{
		public WebPrintNudgeSuspendingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, WebPrintNudgeSuspending nudgeSupending)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebPrintNudgeSuspendingRegistryDataType(nudgeSupending), new WebPrintNudgeSuspendingEditorInfo(), storage, options, nudgeSupending, true))
		{
		}
	}
}
