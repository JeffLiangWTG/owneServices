using Enterprise.Customs.IT.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class MessagesTabUserControl : EU.NCTS.GUI.MessagesTabUserControl
{
	public MessagesTabUserControl()
	{
		InitializeComponent();
		SetUpMessagesGridContextMenuItems();
	}

	void SetUpMessagesGridContextMenuItems()
	{
		ResponseFileManualUploaderContextMenu.Initialize();
		EDIMessageExporterContextMenu.Initialize();
		NctsUniqueTransactionIdentifierGridContextMenuItem.Initialize();
	}

	protected override void Dispose(bool disposing)
	{
		ResponseFileManualUploaderContextMenu.Dispose();
		EDIMessageExporterContextMenu.Dispose();
		NctsUniqueTransactionIdentifierGridContextMenuItem.Dispose();

		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	ResponseFileManualUploaderGridContextMenuItemComponent ResponseFileManualUploaderContextMenu => responseFileManualUploaderContextMenu ??= new(MessageGrid);
	ResponseFileManualUploaderGridContextMenuItemComponent responseFileManualUploaderContextMenu;

	EDIMessageExporterGridContextMenuItemComponent EDIMessageExporterContextMenu => ediMessageExporterContextMenu ??= new(MessageGrid, ResString.GetMultilingualString("E38400AF-9D6D-43C8-B625-67F815D08A3A", "Save Message to Disk"));
	EDIMessageExporterGridContextMenuItemComponent ediMessageExporterContextMenu;

	NctsUniqueTransactionIdentifierGridContextMenuItemComponent NctsUniqueTransactionIdentifierGridContextMenuItem => nctsUniqueTransactionIdentifierGridContextMenuItem ??= new(MessageGrid);
	NctsUniqueTransactionIdentifierGridContextMenuItemComponent nctsUniqueTransactionIdentifierGridContextMenuItem;
}
