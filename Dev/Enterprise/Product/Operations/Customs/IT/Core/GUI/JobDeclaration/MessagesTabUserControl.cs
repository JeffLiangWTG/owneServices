namespace Enterprise.Customs.IT.GUI;

public partial class MessagesTabUserControl : EU.GUI.MessagesTabUserControl
{
	public MessagesTabUserControl()
	{
		InitializeComponent();
		SetUpMessagesGridContextMenuItems();
	}

	void SetUpMessagesGridContextMenuItems()
	{
		SearchCustomsMessagesUpdateContextMenu.Initialize();
		ResponseFileManualUploaderContextMenu.Initialize();
		EFUpdatesRequesterContextMenu.Initialize();
		EDIMessageExporterContextMenu.Initialize();
		CertificateOfOriginXmlUploaderContextMenu.Initialize();
		EntryUniqueTransactionIdentifierGridContextMenuItemComponent.Initialize();
	}

	protected override void Dispose(bool disposing)
	{
		SearchCustomsMessagesUpdateContextMenu.Dispose();
		ResponseFileManualUploaderContextMenu.Dispose();
		EFUpdatesRequesterContextMenu.Dispose();
		EDIMessageExporterContextMenu.Dispose();
		CertificateOfOriginXmlUploaderContextMenu.Dispose();
		EntryUniqueTransactionIdentifierGridContextMenuItemComponent.Dispose();
		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	ResponseFileManualUploaderGridContextMenuItemComponent ResponseFileManualUploaderContextMenu => responseFileManualUploaderContextMenu ??= new ResponseFileManualUploaderGridContextMenuItemComponent(MessagesGrid);
	ResponseFileManualUploaderGridContextMenuItemComponent responseFileManualUploaderContextMenu;

	ElectronicFolderUpdateRequesterGridContextMenuItemComponent EFUpdatesRequesterContextMenu => efUpdatesRequesterContextMenu ??= new ElectronicFolderUpdateRequesterGridContextMenuItemComponent(MessagesGrid);
	ElectronicFolderUpdateRequesterGridContextMenuItemComponent efUpdatesRequesterContextMenu;

	EDIMessageExporterGridContextMenuItemComponent EDIMessageExporterContextMenu => ediMessageExporterContextMenu ??= new EDIMessageExporterGridContextMenuItemComponent(MessagesGrid);
	EDIMessageExporterGridContextMenuItemComponent ediMessageExporterContextMenu;

	CertificateOfOriginXmlUploaderGridContextMenuItemComponent CertificateOfOriginXmlUploaderContextMenu => certificateOfOriginXmlUploaderContextMenu ??= new CertificateOfOriginXmlUploaderGridContextMenuItemComponent(MessagesGrid);
	CertificateOfOriginXmlUploaderGridContextMenuItemComponent certificateOfOriginXmlUploaderContextMenu;

	SearchCustomsMessagesUpdateGridContextMenuComponent SearchCustomsMessagesUpdateContextMenu => searchCustomsMessagesUpdateContextMenu ??= new SearchCustomsMessagesUpdateGridContextMenuComponent(MessagesGrid);
	SearchCustomsMessagesUpdateGridContextMenuComponent searchCustomsMessagesUpdateContextMenu;

	EntryUniqueTransactionIdentifierGridContextMenuItemComponent EntryUniqueTransactionIdentifierGridContextMenuItemComponent
		=> entryUniqueTransactionIdentifierGridContextMenuItemComponent ??= new EntryUniqueTransactionIdentifierGridContextMenuItemComponent(MessagesGrid);

	EntryUniqueTransactionIdentifierGridContextMenuItemComponent entryUniqueTransactionIdentifierGridContextMenuItemComponent;
}
