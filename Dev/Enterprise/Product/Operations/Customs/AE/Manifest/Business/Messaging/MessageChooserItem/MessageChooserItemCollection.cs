using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class MessageChooserItemCollection : MessageChooserItemCollection<MessageChooserItem>
{
	protected override ASYCUDA.Business.MessageChooserItem AddNewCore(ASYCUDA.Business.MessageChooser messageChooser, ISelectionItem item, bool showStatus) => new MessageChooserItem((MessageChooser)messageChooser, item, showStatus);
}
