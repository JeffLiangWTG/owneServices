using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business;

public class MessageChooserItemCollection : NonPersistentBusinessObjectCollection<MessageChooserItem>
{
	public MessageChooserItem AddNew(MessageChooser messageChooser, ISelectionItem item, bool showStatus)
	{
		var result = AddNewCore(messageChooser, item, showStatus);
		Add(result);
		return result;
	}

	protected virtual MessageChooserItem AddNewCore(MessageChooser messageChooser, ISelectionItem item, bool showStatus)
	{
		return new MessageChooserItem(messageChooser, item, showStatus);
	}

	protected override bool AllowNewCore => false;

	protected override bool AllowRemoveCore => false;

	protected override BusinessObject CreateNonPersistentBusinessObject() => throw new System.NotImplementedException();
}
