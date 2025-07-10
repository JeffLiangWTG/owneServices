using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Interop.DataObjects;
using CargoWise.IO;
using CargoWise.PdfiumWrapper;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Dash.Integration.Services;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.DocumentScanning.Business.Constants;

#if WINZOR
using System.Threading.Tasks;
using WinzorFramework.JSInterop;
#endif

namespace Enterprise.DocumentScanning.GUI
{
	/// <summary>
	/// Supports own drag/drop and customised context menu specifically for documents.
	/// Listens for WM_COPY / WM_CUT / WM_PASTE messages which are thrown
	/// by our parent form when the user clicks the relavent items on the
	/// menu bar.
	/// </summary>
	public partial class DocumentsZGrid : ZGrid
	{
		public bool DragDropHasLeftGrid { get; set; }
		internal bool IsInsertAllowed { get; set; } = true;

		public DocumentsZGrid()
		{
			RegisterEventHandlers();

			StorageDocImageViewer = new StorageDocsViewer();
			StorageDocImageViewer.Initialise(this, null);

			DisableImportDataMenuItem = true;
		}

		internal StorageDocsViewer StorageDocImageViewer { get; }

		void RegisterEventHandlers()
		{
			DragLeave += new EventHandler(Grid_DragLeave);
			DragEnter += new DragEventHandler(Grid_DragEnter);
			DoubleClick += new EventHandler(Grid_DoubleClick);
			ColourDeciding += Grid_ColourDeciding;

			Cut += new EventHandler(Grid_CutDocuments);
			Paste += new EventHandler(Grid_PasteDocuments);
			Copy += new EventHandler(Grid_CopyDocuments);

			HandleCreated += new EventHandler(DocumentsZGrid_HandleCreated);
#if !WINZOR
			Scroll += new EventHandler(DocumentsZGrid_Scroll);
#endif
		}

		#region Designer Properties

		#region ShowViewMenuItem

		[Description("Shows the View menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowViewMenuItem { get; set; }

		#endregion

		#region ShowEditPropertiesMenuItem

		[Description("Shows the Edit Properties menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowEditPropertiesMenuItem { get; set; }

		#endregion

		#region ShowCutMenuItem

		[Description("Shows the Cut menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowCutMenuItem { get; set; }

		#endregion

		#region ShowCopyMenuItem

		[Description("Shows the Copy menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowCopyMenuItem { get; set; }

		#endregion

		#region ShowCopyLinkMenuItem

		[Description("Shows the Copy Link menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowCopyLinkMenuItem { get; set; }

		#endregion

		#region ShowSplitDocumentMenuItem

		[Description("Shows the Split Document menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowSplitDocumentMenuItem { get; set; }

		#endregion

		#region ShowPasteMenuItem

		[Description("Shows the Paste menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowPasteMenuItem { get; set; }

		#endregion

		#region ShowDeleteMenuItem

		[Description("Shows the Delete menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowDeleteMenuItem { get; set; }

		#endregion

		#region ShowDeletePermanentlyMenuItem

		[Description("Shows the Delete Permanently menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowDeletePermanentlyMenuItem { get; set; }

		#endregion

		#region ShowRestoreMenuItem

		[Description("Shows the Restore menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowRestoreMenuItem { get; set; }

		#endregion

		#region ShowAllocateMenuItem

		[Description("Shows the Allocate menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowAllocateMenuItem { get; set; }

		#endregion

		#region ShowUnallocateMenuItem

		[Description("Shows the Unallocate menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowUnallocateMenuItem { get; set; }

		#endregion

		#region ShowDeliverDocumentMenuItem

		[Description("Shows the Deliver Document menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowDeliverDocumentMenuItem { get; set; }

		#endregion

		#region ShowParseMenuItem

		[Description("Shows the menu item to manually send the eDoc to be parsed by Document Ingestion"), DefaultValue(true)]
		public bool ShowParseDocumentMenuItem { get; set; }

		#endregion

		#region ReviewParsedResultsMenuItem

		[Description("Shows the Review Parsed Results menu item in the Context Menu"), DefaultValue(false)]
		public bool ShowReviewParsedResultsMenuItem { get; set; }

		#endregion

		#endregion

		#region Properties

		[Browsable(false)]
		public StorageDocsBase CurrentElementAtMousePosition => GetElementAtPosition(LastClickPoint);

#if !WINZOR

		bool isScrolled;
		int lastStartScrollTickCount;

		bool ScrolledInLast2Seconds => System.Environment.TickCount - lastStartScrollTickCount < 2000;

#endif

		public StorageDocsBase GetElementAtPosition(Point p)
		{
			StorageDocsBase bizO = null;
			var info = HitTest(p);

			if (info.Type == DataGrid.HitTestType.Cell || info.Type == DataGrid.HitTestType.RowHeader && info.Row != -1)
			{
				bizO = (StorageDocsBase)ListManager.List[info.Row];
			}

			return bizO;
		}

		[Browsable(false)]
		public StorageDocsBase CurrentElement
		{
			get
			{
				StorageDocsBase bizO = null;

				if (CurrentRowIndex >= 0 && CurrentRowIndex < ListManager.Count)
				{
					bizO = (StorageDocsBase)ListManager.List[CurrentRowIndex];
				}
				return bizO;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsEditable
		{
			get => !ReadOnly && Env.Security.eDocsModify.IsAllowed && isEditable;
			set => isEditable = value;
		}

		bool isEditable = true;

		public override T[] GetSelectedElements<T>()
		{
			var selectedList = new List<T>(base.GetSelectedElements<T>());

			if (CurrentRowIndex >= 0 && !IsSelected(CurrentRowIndex))
			{
				var bo = (T)ListManager.List[CurrentRowIndex];

				if (bo != null)
				{
					selectedList.Add(bo);
				}
			}

			return selectedList.ToArray();
		}

		#endregion

		#region Related Interfaces

		public IDragDropSupport DragDropTarget { get; set; }

		public IDocumentManipulationSupport DocumentManipulationTarget { get; set; }

		#endregion

		#region Context Menu

		protected override void SetupContextMenu()
		{
			base.SetupContextMenu();

			AddSeparator(ContextMenu);
			AddViewMenuItem(ContextMenu);
			AddEditPropertiesMenuItem(ContextMenu);
			AddCutCopyPasteSelectMenuItems(ContextMenu);
			AddCopyLinkMenuItem(ContextMenu);
			AddSplitDocumentMenuItem(ContextMenu);
			AddSaveMenuItem(ContextMenu);

			AddSeparator(ContextMenu);
			AddDeleteAndRestoreMenuItems(ContextMenu);

			AddSeparator(ContextMenu);
			AddAllocateAndUnallocateMenuItems(ContextMenu);

			AddSeparator(ContextMenu);
			AddParseMenuItems(ContextMenu);
			AddReviewParsedResultsMenuItem(ContextMenu);

			AddSeparator(ContextMenu);
			AddDeliverMenuItems(ContextMenu);

			RemoveSeparatorFromEnd(ContextMenu);
		}

		void AddMenuItem(ContextMenu menu, eDocMenuItem menuItem)
		{
			menu.MenuItems.Add(menuItem);
		}

		void AddSeparator(ContextMenu menu)
		{
			if (menu.MenuItems[menu.MenuItems.Count - 1].Text != Constants.SeparatorMenuText)
			{
				AddMenuItem(menu, new SeparatorMenuItem());
			}
		}

		void RemoveSeparatorFromEnd(ContextMenu contextMenu)
		{
			if (contextMenu.MenuItems[contextMenu.MenuItems.Count - 1].Text == Constants.SeparatorMenuText)
			{
				var itemToRemove = contextMenu.MenuItems[contextMenu.MenuItems.Count - 1];
				contextMenu.MenuItems.Remove(itemToRemove);
			}
		}

		void AddViewMenuItem(ContextMenu contextMenu)
		{
			if (ShowViewMenuItem)
			{
				AddMenuItem(contextMenu, new ViewMenuItem(new EventHandler(Grid_ViewFile)));
			}
		}

		void AddEditPropertiesMenuItem(ContextMenu contextMenu)
		{
			if (ShowEditPropertiesMenuItem)
			{
				AddMenuItem(contextMenu, new EditPropertiesMenuItem(new EventHandler(Grid_EditProperties)));
			}
		}

		protected void AddCutCopyPasteSelectMenuItems(ContextMenu contextMenu)
		{
			if (ShowCutMenuItem)
			{
				AddMenuItem(contextMenu, new CutMenuItem(new EventHandler(Grid_CutDocuments)));
			}
			if (ShowCopyMenuItem)
			{
				AddMenuItem(contextMenu, new CopyMenuItem(new EventHandler(Grid_CopyDocuments)));
			}
			if (ShowPasteMenuItem)
			{
				AddMenuItem(contextMenu, new PasteMenuItem(new EventHandler(Grid_PasteDocuments)));
			}

			AddMenuItem(contextMenu, new SelectAllMenuItem(new EventHandler(Grid_SelectAllDocuments)));
		}

		protected void AddCopyLinkMenuItem(ContextMenu contextMenu)
		{
			if (ShowCopyLinkMenuItem)
			{
				AddMenuItem(contextMenu, new CopyLinkMenuItem(new EventHandler(Grid_CopyDocumentLink)));
			}
		}

		void AddSplitDocumentMenuItem(ContextMenu contextMenu)
		{
			if (ShowSplitDocumentMenuItem)
			{
				AddMenuItem(contextMenu, new SplitDocumentMenuItem(new EventHandler(Grid_SplitDocument)));
			}
		}

		void AddSaveMenuItem(ContextMenu contextMenu)
		{
			AddMenuItem(contextMenu, new SaveFileAsMenuItem(new EventHandler(Grid_SaveFileAs)));
		}

		void AddDeleteAndRestoreMenuItems(ContextMenu contextMenu)
		{
			if (ShowDeleteMenuItem)
			{
				AddMenuItem(contextMenu, new DeleteMenuItem(new EventHandler(Grid_DeleteDocuments)));
			}
			if (ShowDeletePermanentlyMenuItem)
			{
				AddMenuItem(contextMenu, new DeletePermanentlyMenuItem(new EventHandler(DocumentsGrid_DeleteDocumentsPermanently)));
			}
			if (ShowRestoreMenuItem)
			{
				AddMenuItem(contextMenu, new RestoreMenuItem(new EventHandler(DocumentsGrid_RestoreDocuments)));
			}
		}

		void AddAllocateAndUnallocateMenuItems(ContextMenu contextMenu)
		{
			if (ShowAllocateMenuItem)
			{
				AddMenuItem(contextMenu, new AllocateMenuItem(new EventHandler(DocumentsGrid_AllocateDocuments)));
			}
			if (ShowUnallocateMenuItem)
			{
				AddMenuItem(contextMenu, new UnallocateMenuItem(new EventHandler(DocumentsGrid_UnallocateDocuments)));
			}
		}

		void AddDeliverMenuItems(ContextMenu contextMenu)
		{
			if (ShowDeliverDocumentMenuItem)
			{
				AddMenuItem(contextMenu, new DeliverDocumentMenuItem(new EventHandler(DocumentsGrid_DeliverDocument)));
			}
		}

		void AddParseMenuItems(ContextMenu contextMenu)
		{
			if (ShowParseDocumentMenuItem)
			{
				AddMenuItem(contextMenu, new ParseDocumentMenuItem(new EventHandler(Grid_Parse)));
			}
		}

		void AddReviewParsedResultsMenuItem(ContextMenu contextMenu)
		{
			if (ShowReviewParsedResultsMenuItem)
			{
				AddMenuItem(contextMenu, new ReviewParsedResultsMenuItem(new EventHandler(Grid_ReviewParsedResultsMenuItem)));
			}
		}

		public void RebuildContextMenu()
		{
			SetupContextMenu();
		}

		protected override void HookContextMenu()
		{
			base.HookContextMenu();
			ContextMenu.Popup += new EventHandler(ContextMenu_Popup);
		}

		protected override void UnHookContextMenu()
		{
			base.UnHookContextMenu();
			ContextMenu.Popup -= new EventHandler(ContextMenu_Popup);
		}

		protected void ContextMenu_Popup(object sender, EventArgs e)
		{
			SaveMouseCoordinates();
			UpdateContextMenuElements(CurrentElementAtMousePosition);
		}

		void SaveMouseCoordinates()
		{
			LastClickPoint = PointToClient(MousePosition);
		}

		public void UpdateContextMenuElements(StorageDocsBase elementAtMouse)
		{
			var isViewable = IsDocumentViewEnabled(elementAtMouse);

			foreach (var item in ContextMenu.MenuItems.OfType<eDocMenuItem>())
			{
				item.UpdateVisibility(elementAtMouse, SelectedElements, IsEditable, isViewable);
			}
		}

		#endregion

		#region Menu Events

		#region Delete

		public bool IsPerformingBulkOperation { get; private set; }

		void Grid_DeleteDocuments(object sender, EventArgs e)
		{
			IsPerformingBulkOperation = true;
			DeleteDocuments(SelectedElements);
			IsPerformingBulkOperation = false;
			BulkOperationComplete?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler BulkOperationComplete;

		protected void DeleteDocuments(BusinessObject[] documentsToDelete)
		{
			if (!documentsToDelete.Any())
			{
				Globals.Message.Show(Res.GetString("7b7f5736-34e8-434c-9b0d-36ebc61ad722", "Please select the eDocs you want to delete."), Res.GetString("bf018cd3-d852-4a90-9b55-64e9fb45b2f0", "Delete Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			else
			{
				var prompt = Res.GetString("9ba24939-cf38-421b-ae1b-15bbf9f04b79", @"Do you really want to delete the selected eDoc(s)?");
				if (Globals.Message.Show(prompt, Res.GetString("bd3367b3-240f-4be7-a2d5-3a1213f418dd", "Confirm Delete"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					var documentsToDeletePruned = PruneAndReportCannotDeleteDocuments(documentsToDelete);
					OnDeletingDocuments(documentsToDeletePruned);
					DocumentManipulationTarget.DeleteDocumentsQuietly(documentsToDeletePruned);
				}
			}
		}

		void DocumentsGrid_DeleteDocumentsPermanently(object sender, EventArgs e)
		{
			DeleteDocumentsPermanently(SelectedElements);
		}

		public void DeleteDocumentsPermanently(ICollection<BusinessObject> documentsToDelete)
		{
			if (documentsToDelete.Count == 0)
			{
				Globals.Message.Show(Res.GetString("4df094cd-41e1-4760-8149-be7c1a1c32bd", "Please select the eDocs you want to delete permanently."), Res.GetString("bf018cd3-d852-4a90-9b55-64e9fb45b2f0", "Delete Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			var docsWithoutPermissions = new List<BusinessObject>();
			var docsWithPermissions = new List<BusinessObject>();

			foreach (StorageDocsBase doc in documentsToDelete)
			{
				var checkpointForDocType = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete" + doc.SC_DocType));
				if (checkpointForDocType == null || checkpointForDocType.IsAllowed)
				{
					docsWithPermissions.Add(doc);
				}
				else
				{
					docsWithoutPermissions.Add(doc);
				}
			}

			var securityRightPath = docsWithoutPermissions.Count > 0 ? GetDocTypeSecurityPaths(docsWithoutPermissions, GetEDocsPermanentDeleteSecurityPath) : string.Empty;

			if (docsWithPermissions.Count == 0)
			{
				if (!Env.Security.eDocsPermanentDelete.IsAllowed)
				{
					Env.Security.eDocsPermanentDelete.ShowError();
				}
				else
				{
					var errorMessage = new StringBuilder()
						.AppendLine(SecurityCore.SecurityErrorMessage)
						.Append(securityRightPath)
						.ToString();
					Globals.Message.ShowError(errorMessage);
				}

				return;
			}

			var confirmationMessage = GetConfirmationMessageForPartialFailure(
				docsWithoutPermissions,
				securityRightPath,
				Res.GetString("1c1a569a-bc5c-408a-9e69-cef1862e81ee", "You do not have the appropriate security rights to permanently delete the below document(s):"),
				ResString.GetMultilingualString("3b173d7d-2f8e-4142-826f-23e5cbc5c2e6", "Are you sure you wish to permanently delete the remaining selected items? This cannot be undone."));

			if (UserDefinitelyWantsToPermanentlyDeleteItems(docsWithPermissions.Count, confirmationMessage))
			{
				OnDeletingDocuments(docsWithPermissions);
				var nonDeletedDocuments = DocumentManipulationTarget.DeleteDocumentsPermanently(docsWithPermissions);

				if (nonDeletedDocuments.Count > 0)
				{
					ReportCannotDeleteDocuments(nonDeletedDocuments, docsWithPermissions.Count, isPermaDelete: true);
				}
			}
		}

		string GetConfirmationMessageForPartialFailure(
			ICollection<BusinessObject> docsWithoutPermissions,
			string securityPath,
			string warningMessage,
			string confirmMessage)
		{
			if (docsWithoutPermissions.Count == 0)
			{
				return null;
			}

			var failureWarning = new StringBuilder().AppendLine(warningMessage);

			foreach (StorageDocsBase doc in docsWithoutPermissions)
			{
				_ = failureWarning
					.AppendLine()
					.Append(ResString.GetMultilingualString("0d15986a-ac2c-49e4-9d2b-aeec5ca1e9a4", "{0} - Document Type {1}", doc.SC_FileName, doc.SC_DocType));
			}

			_ = failureWarning
				.AppendLine(System.Environment.NewLine)
				.AppendLine(ResString.GetMultilingualString("141ab887-a4dc-400a-9c11-abc6ed659cdd", "If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:"))
				.AppendLine(securityPath)
				.AppendLine()
				.Append(confirmMessage);

			return failureWarning.ToString();
		}

		string GetEDocsPermanentDeleteSecurityPath(string docType)
		{
			return ResString.GetMultilingualString("0160524c-59d3-4bf2-8e06-f35548d11d44", "Manage -> DocManager -> eDocs Permanent Delete -> {0}", docType);
		}

		string GetEDocsCutSpecificDocumentTypeSecurityPath(string docType)
		{
			return ResString.GetMultilingualString("8F44B016-B5A4-4C19-A3D2-44E486BA9880", "Manage -> DocManager -> eDocs Cut Specific Document Types -> {0}", docType);
		}

		string GetDocTypeSecurityPaths(ICollection<BusinessObject> documents, Func<string, string> getSecurityPathForDocType)
		{
			var securityPathsMessage = new StringBuilder();

			foreach (var docType in documents.Select(d => ((StorageDocsBase)d).SC_DocType).Distinct())
			{
				securityPathsMessage.AppendLine();
				securityPathsMessage.Append(getSecurityPathForDocType(docType));
			}

			return securityPathsMessage.ToString();
		}

		bool UserDefinitelyWantsToPermanentlyDeleteItems(int docsToDelete, string confirmationMessage = null)
		{
			var message = confirmationMessage ?? Res.GetString("65BED696-25F6-473A-B0FF-5028AC57E542", "Are you sure you wish to permanently delete the selected items? This cannot be undone. There are {0} selected item(s).", docsToDelete);
			var caption = Res.GetString("59fbb18a-87aa-4a96-8e4a-86a91becc8d8", "Confirm Delete");
			return Globals.Message.Show(message, caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Warning) == ZDialogResult.Yes;
		}

		ICollection<BusinessObject> PruneAndReportCannotDeleteDocuments(ICollection<BusinessObject> docsToDelete)
		{
			var sortedDocs = docsToDelete.ToLookup(doc => doc.CanDelete);

			if (sortedDocs[false].Any())
			{
				ReportCannotDeleteDocuments(sortedDocs[false].ToArray(), docsToDelete.Count);
			}

			return sortedDocs[true].ToList();
		}

		void ReportCannotDeleteDocuments(ICollection<BusinessObject> documentsThatCouldntBeDeleted, int totalDocsAttemptedToDelete, bool isPermaDelete = false)
		{
			var errorMessage = new StringBuilder(Res.GetString("ee6968d0-9461-41ab-94e1-584d4e1f8d08", "The following documents could not be deleted:"));
			errorMessage.AppendLine();

			foreach (var bizO in documentsThatCouldntBeDeleted)
			{
				var storageDoc = bizO as AutoStorageDocs;
				var name = storageDoc != null ? storageDoc.SC_FileName : (ZString)"eDoc";

				errorMessage.AppendLine(name + ": " + bizO.ReasonForNotAbleToDelete);
				totalDocsAttemptedToDelete--;
			}

			if (totalDocsAttemptedToDelete > 0)
			{
				errorMessage.AppendLine();
				errorMessage.AppendLine(isPermaDelete
				? Res.GetString("7b3e2f9c-ef3f-46ee-9112-53e8a945c07d", "The remaining {0} document(s) will be deleted permanently.", totalDocsAttemptedToDelete)
				: Res.GetString("a1f2b06c-82bd-4ebc-a197-a4ecfac820fe", "The remaining {0} document(s) will be deleted.", totalDocsAttemptedToDelete));
			}

			Globals.Message.ShowError(errorMessage.ToString(), Res.GetString("247d0b45-3862-45cc-94b7-78a22f4b550e", "Cannot Delete Document"));
		}

		void OnDeletingDocuments(ICollection<BusinessObject> documentsToDelete)
		{
			DeletingDocuments?.Invoke(this, new DeletingDocumentsEventArgs(documentsToDelete));
		}

		public event DeletingDocumentsDelegate DeletingDocuments;

		#endregion

		#region EditMenuItem

		MenuItem editMenuItem;
		MenuItem cutEditMenuItem;
		MenuItem copyEditMenuItem;
		MenuItem pasteEditMenuItem;

		void DocumentsZGrid_HandleCreated(object sender, EventArgs e)
		{
			editMenuItem = FindParentFormEditMenuItem();
			if (editMenuItem != null)
			{
				cutEditMenuItem = FindMenuItemByCaption(editMenuItem.MenuItems, ZEditMenuItem.CutMenuItemCaption);
				copyEditMenuItem = FindMenuItemByCaption(editMenuItem.MenuItems, ZEditMenuItem.CopyMenuItemCaption);
				pasteEditMenuItem = FindMenuItemByCaption(editMenuItem.MenuItems, ZEditMenuItem.PasteMenuItemCaption);
				editMenuItem.Popup += new EventHandler(editMenuItem_Popup);
			}
		}

		void editMenuItem_Popup(object sender, EventArgs e)
		{
			UpdateEditMenuItemEnableStatus();
		}

		ZMenuItem FindMenuItemByCaption(Menu.MenuItemCollection menuItems, MultilingualString caption)
		{
			return menuItems.OfType<ZMenuItem>().FirstOrDefault(menuItem => caption.Equals(menuItem.Caption));
		}

#if DEBUG
		internal
#endif
		MenuItem FindParentFormEditMenuItem()
		{
			for (var parent = Parent; parent != null; parent = parent.Parent)
			{
				var menuProvider = parent as IFileMenuItemsProvider;
				if (menuProvider != null)
				{
					return menuProvider.EditMenuItem;
				}
			}
			return null;
		}

#if DEBUG
		internal
#endif
		void UpdateEditMenuItemEnableStatus()
		{
			if (editMenuItem != null)
			{
				if (copyEditMenuItem != null)
				{
					copyEditMenuItem.Enabled = !this.Focused || IsMenuItemEnabledForSelectedElements(Constants.CopyMenuText);
				}
				if (cutEditMenuItem != null)
				{
					cutEditMenuItem.Enabled = !this.Focused || (IsEditable && IsMenuItemEnabledForSelectedElements(Constants.CutMenuText));
				}
				if (pasteEditMenuItem != null)
				{
					pasteEditMenuItem.Enabled = !this.Focused || (IsEditable && IsMenuItemEnabledForSelectedElements(Constants.PasteMenuText));
				}
			}
		}

		#endregion

		#region View

		void Grid_ViewFile(object sender, EventArgs e)
		{
			View(CurrentElementAtMousePosition);
		}

		void Grid_DoubleClick(object sender, EventArgs e)
		{
			SaveMouseCoordinates();
			FireDoubleClickedIfApplicable();
		}

		protected void FireDoubleClickedIfApplicable()
		{
			if (HitTest(LastClickPoint).Row >= 0)
			{
				DoubleClicked();
			}
		}

		protected virtual void DoubleClicked()
		{
			var fileValidation = new FileTypeValidation();
			var dangerousFiles = new List<ZString>();
			var invalidFileNames = new List<ZString>();
			var virusDetectedFiles = new List<ZString>();

			foreach (StorageDocsBase element in SelectedElements)
			{
				var fileName = element.SC_FileNameWithExtension;
				if (!MakeFilenameSafe.IsSafe(fileName))
				{
					invalidFileNames.Add(fileName);
				}
				else if (fileValidation.IsDangerousFile(fileName))
				{
					dangerousFiles.Add(fileName);
				}
				else if (element.IsVirusDetected)
				{
					virusDetectedFiles.Add(fileName);
				}
				else
				{
					View(element);
				}
			}
			if (dangerousFiles.Count > 0)
			{
				Globals.Message.ShowError(Res.GetString("AB957BB3-59D3-4D94-B25A-48544BC21265", "The following file(s) [{0}] have been considered as dangerous and therefore could not be opened.", string.Join(", ", dangerousFiles.ToArray())));
			}
			if (virusDetectedFiles.Count > 0)
			{
				Globals.Message.ShowError(Res.GetString("BDF15F47-0058-4907-A73E-15CAC7A54D65", "The following file(s) [{0}] have been detected with virus and therefore could not be opened.", string.Join(", ", virusDetectedFiles.ToArray())));
			}
			if (invalidFileNames.Count > 0)
			{
				Globals.Message.ShowError(Res.GetString("C2AFDB7F-F75B-4FA5-9DDD-3E7694542CB0", "The following file name(s) [{0}] contain invalid character(s), please edit it to be Windows compatible.", string.Join(", ", invalidFileNames.ToArray())));
			}
		}

		protected void View(StorageDocsBase doc)
		{
			if (doc != null && IsDocumentViewEnabled(doc) && ConfirmDocumentDelivery(doc))
			{
				StorageDocImageViewer.View(doc, ReadOnly || doc.SC_IsDeleted);
				StorageDocViewed?.Invoke(doc);
			}
		}

		public bool IsDocumentViewEnabled(StorageDocsBase doc)
		{
			var documentDeliveryAuthorizer = doc?.ParentMain?.DocumentOwner as IEDocDeliveryAuthorization;
			return (documentDeliveryAuthorizer?.IsDocumentViewEnabled(doc) ?? true) && !(doc != null && doc.IsVirusDetected);
		}

		bool ConfirmDocumentDelivery(StorageDocsBase doc)
		{
			var deliveryEnabled = true;

			var documentDeliveryAuthorizer = doc?.ParentMain?.DocumentOwner as IEDocDeliveryAuthorization;
			if (documentDeliveryAuthorizer?.IsDocumentDeliveryDisclaimerRequired(doc) ?? false)
			{
				using (var messageBox = new ZMessageBox(documentDeliveryAuthorizer.DeliveryDisclaimerMessage, ZString.Empty, MessageBoxButtons.OKCancel, MessageBoxIcon.Information, Res.GetString("9B894ACB-ECE3-487A-8DA4-EFDB46FECC37", "Agree")))
				{
					deliveryEnabled = ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox) == DialogResult.OK;
				}
			}

			return deliveryEnabled;
		}

		public delegate void StorageDocViewedHandler(StorageDocsBase doc);
		public event StorageDocViewedHandler StorageDocViewed;

		#endregion

		#region Scroll

#if !WINZOR

		void DocumentsZGrid_Scroll(object sender, EventArgs e)
		{
			isScrolled = true;
			lastStartScrollTickCount = System.Environment.TickCount;
		}

#endif

		#endregion

		#region Edit

		void Grid_EditProperties(object sender, EventArgs e)
		{
			EditProperties(CurrentElementAtMousePosition);
		}

		protected internal void EditProperties(StorageDocsBase objectToView)
		{
			if (objectToView != null)
			{
				OldDocType = objectToView.DocType;
				OldDocSource = objectToView.DocSource;
				var checkpoint = OldDocType != null ? Env.Security.GetDocumentTypeUploadCheckPoint(OldDocType.RT_DocType) : null;

				if (checkpoint == null || checkpoint.IsAllowed)
				{
					if (!objectToView.SC_IsDeleted)
					{
						var form = new EditPropertiesFileForm(objectToView);
						((IEditResponse)form).CloseOfForm += new DocumentEventHandler(DocumentsZGrid_CloseOfForm);
						ZFormModaliser.Show(form, FindForm());
					}
					else
					{
						Globals.Message.Show(Res.GetString("59310f48-425b-4586-a9b6-af884c761a8c", "You cannot change the details of a deleted eDoc. To change this eDoc, select 'Restore' and then change the details."), Res.GetString("5e6eb90b-64a9-4680-8df5-cbacc8c250d1", "Cannot Modify Deleted eDoc"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				}
				else
				{
					checkpoint.ShowError();
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("f19e017f-d09c-425a-bb42-7c32517ad00d", "Please select a record to edit."));
			}
		}

		#endregion

		#region Parse

		void Grid_Parse(object sender, EventArgs e)
		{
			Parse(CurrentElementAtMousePosition);
		}

		protected internal void Parse(StorageDocsBase objectToView)
		{
			var message = string.Empty;
			var caption = string.Empty;
			var parseStatus = objectToView?.ActiveShipamaxMessage?.EM_Status ?? ZString.Empty;
			var parseTypeDescription = ParseDocumentMenuItem.GetParseTypeDescription(objectToView);
			switch (parseStatus)
			{
				case EDIMessageStatusList.Codes.Cancelled:
				case EDIMessageStatusList.Codes.Error:
					message = Res.GetString("F9F80CA3-1CF4-45C6-9DEA-53A4F85D1A2D", "This will trigger the parsing of the document as a {0}. Click 'OK' to continue.", parseTypeDescription);
					caption = Res.GetString("A6D9FCEE-1C60-47B9-A627-BAF7341F3B38", "Send for Parsing");
					break;

				case EDIMessageStatusList.Codes.Sent:
				case EDIMessageStatusList.Codes.PreProcessedOK:
				case EDIMessageStatusList.Codes.ProcessedOK:
					message = Res.GetString("25A7E4CA-D504-4AAB-98F1-5D1A551CB0ED", "The document will be re-parsed as a {0}. Re-parsing may take a few minutes, and all previously parsed/edited data will be lost. Click 'OK' to re-parse the document.", parseTypeDescription);
					caption = Res.GetString("F0232A7A-9F47-41D9-9A33-DD02C20C1226", "Send for Re-parsing");
					break;

				default:
					throw new InvalidOperationException($"Should not manually parse the document, as the parse status is {parseStatus}");
			}

			if (Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
			{
				objectToView.DeactivateCurrentShipamaxMessageIfNeeded();
				objectToView.IsParsingEnabled = true;
				objectToView.CreateMessageForShipamaxParsing();
				objectToView.HasChanges = true;
			}
		}

		#endregion

#if DEBUG
		internal
#endif
		RefDocType OldDocType;
		protected RefDocSource OldDocSource;

#if DEBUG
		internal
#endif
		void DocumentsZGrid_CloseOfForm(object sender, DocumentEventArgs e)
		{
			if (e.Document == null)
			{
				throw new ArgumentException("Invalid DocumentEventArgs.Document");
			}

			var editResponse = sender as IEditResponse;
			if (editResponse != null)
			{
				((IEditResponse)sender).CloseOfForm -= new DocumentEventHandler(DocumentsZGrid_CloseOfForm);
			}

			var zFormSender = sender as ZForm;
			if (zFormSender != null && zFormSender.DialogResult == DialogResult.Cancel)
			{
				e.Document.Validation.ValidateAll();
			}
		}

		#endregion

		#region Cut

		protected void Grid_CutDocuments(object sender, EventArgs e)
		{
			if (IsEditable && ShowCutMenuItem)
			{
				if (IsMenuItemEnabledForSelectedElements(Constants.CutMenuText))
				{
					CutDocuments(SelectedElements);
				}
			}
		}

		protected void CutDocuments(BusinessObject[] documentsToCut)
		{
			if (documentsToCut.Length == 0)
			{
				Globals.Message.Show(Res.GetString("97CA25CB-C560-4F85-AE6D-01778F548EED", "Please select the eDocs you want to cut."), Res.GetString("415990E6-E7BC-4AB7-A19C-FE1B18945028", "Cut Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			var documentsWithPermission = new List<StorageDocsBase>();
			var documentsWithoutPermissions = new List<BusinessObject>();

			foreach (var document in documentsToCut.Cast<StorageDocsBase>())
			{
				var checkpointForDocType = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsCutDocumentType" + document.SC_DocType));
				if (checkpointForDocType == null || checkpointForDocType.IsAllowed)
				{
					documentsWithPermission.Add(document);
				}
				else
				{
					documentsWithoutPermissions.Add(document);
				}
			}

			var securityRightPath = documentsWithoutPermissions.Count > 0 ? GetDocTypeSecurityPaths(documentsWithoutPermissions, GetEDocsCutSpecificDocumentTypeSecurityPath) : string.Empty;

			if (documentsWithPermission.Count == 0)
			{
				if (!Env.Security.CutSpecificEDocTypes.IsAllowed)
				{
					Env.Security.CutSpecificEDocTypes.ShowError();
				}
				else
				{
					var errorMessage = new StringBuilder()
						.AppendLine(SecurityCore.SecurityErrorMessage)
						.Append(securityRightPath)
						.ToString();
					Globals.Message.ShowError(errorMessage);
				}

				return;
			}

			if (documentsWithoutPermissions.Count > 0)
			{
				var confirmationMessage = GetConfirmationMessageForPartialFailure(documentsWithoutPermissions, securityRightPath,
					Res.GetString("1449A2F9-BD8E-4620-925B-E56776479A24", "You do not have the appropriate security rights to cut the below document(s):"),
					ResString.GetMultilingualString("6BAA5075-2C46-41CF-B963-73AD2CE199E7", "Are you sure you wish to cut the remaining selected items?"));

				var caption = Res.GetString("47940D95-0F22-46A9-B14D-09BB2882E80C", "Confirm Cut");
				if (Globals.Message.Show(confirmationMessage, caption, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Warning) != ZDialogResult.Yes)
				{
					return;
				}
			}

			CopyDocuments(documentsWithPermission.ToArray());

			foreach (var document in documentsWithPermission)
			{
				var documentOwner = document.ParentMain?.DocumentOwner;
				var isSaved = document.IsInDatabase;
				var reference = DocumentLogReferenceHelper.GetReference(document, AutoEvents.DocumentDeletedPermanently);
				document.DeleteParentIfUnallocated();
				document.Delete();

				if (isSaved &&
					documentOwner is EnterpriseBusinessObject owner)
				{
					_ = owner.Logs.AddNew(AutoEvents.DocumentDeletedPermanently, reference);
				}
			}
		}

		bool IsMenuItemEnabledForSelectedElements(MultilingualString caption)
		{
			if (ContextMenu == null)
			{
				ErrorReporter.ReportOnce("ContextMenuNullDocumentsZGrid", "Context Menu null in IsMenuItemEnabledForSelectedElements.");
				return true;
			}
			var item = FindMenuItemByCaption(ContextMenu.MenuItems, caption) as eDocMenuItem;
			if (item != null)
			{
				if (SelectedElements.Length > 0)
				{
					var multipleSelected = SelectedElements.Length > 1;
					var enabled = item.GetEnabledStatus((StorageDocsBase)SelectedElements[0], multipleSelected);
					if (multipleSelected)
					{
						enabled = enabled && item.GetEnabledStatusWhenMultipleElementsSelected(SelectedElements);
					}
					return enabled;
				}
				else
				{
					return item.GetEnabledStatus(null, false);
				}
			}
			return false;
		}

		#endregion

		#region Copy

		void Grid_CopyDocuments(object sender, EventArgs e)
		{
			if (ShowCopyMenuItem && IsMenuItemEnabledForSelectedElements(Constants.CopyMenuText))
			{
				CopyDocuments(SelectedElements);
			}
		}

		protected virtual void CopyDocuments(BusinessObject[] documentsToCopy)
		{
			if (!SafeClipboard.SetDataObject(GetDataObject(documentsToCopy), false))
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
		}

		#endregion

		#region Copy Link

		protected void Grid_CopyDocumentLink(object sender, EventArgs e)
		{
			if (ShowCopyLinkMenuItem && CurrentElementAtMousePosition != null && CurrentElementAtMousePosition.ParentMain != null)
			{
				var fileDesc = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", CurrentElementAtMousePosition.SC_FileNameWithExtension, CurrentElementAtMousePosition.SC_DescMultilingual);
				var message = Res.GetString("9da6402e-48ab-45dc-8a02-d73468dba3d6", "{0} File {1}", EnvProxy.Instance.CurrentUser.InitialsAndDateTime, fileDesc);

				var url = ShowStorageDocUrlHandler.Instance.Create(CurrentElementAtMousePosition);
				ZMenuStrategyHelper.ShortcutCreator.CopyHyperlinkToClipboard(message, url);
			}
		}

		#endregion

		#region Split Document

		protected void Grid_SplitDocument(object sender, EventArgs e)
		{
			if (CurrentElementAtMousePosition != null)
			{
				try
				{
					if (CurrentElementAtMousePosition.EDocFormat == Core.Constants.FileFormats.PDF)
					{
						using (var pdfDocument = new PdfDocument(CurrentElementAtMousePosition.GetSC_ImageDataReader()))
						{
							if (pdfDocument.SignatureCount > 0)
							{
								Globals.Message.Show(Res.GetString("668CBEBD-B99A-4ADB-AA2D-BAB484CB4B03", "This document has been digitally signed and cannot be split into multiple documents."));
								return;
							}
						}
					}

					var form = new SplitDocumentForm(new DocumentSplitManager(CurrentElementAtMousePosition, (BusinessObjectCollection)ListManager.List));
					ZFormModaliser.Show(form, FindForm());
				}
				catch (VirusDetectedException ex)
				{
					Globals.Message.ShowError(ex.VirusDetectedFriendlyMessage);
				}
				catch (Exception ex) when (IsCausedByIncompatibleDocument(ex))
				{
					Globals.Message.Show(Res.GetString("78f7a24c-aa0e-4778-9583-46caed4e7b0f",
						"Failed to split the document; the file may be corrupted. Adobe Reader may be able to repair the document for splitting. Open the file in Adobe Reader, save a copy to your PC, then attach that copy to eDocs."));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("642cc88b-44e1-4873-910c-a71e4ea89e8a", "Please select an eDoc to split."));
			}
		}

		bool IsCausedByIncompatibleDocument(Exception ex)
		{
			var pdfex = ex as PdfiumException;

			if (pdfex == null && ex is CorruptedDocumentException corruptedDocumentException)
			{
				pdfex = corruptedDocumentException.InnerException as PdfiumException;
			}

			if (pdfex != null)
			{
				switch (pdfex.ErrorType)
				{
					case PdfiumError.FileCorrupt:
					case PdfiumError.IncorrectPassword:
					case PdfiumError.UnsupportedEncryption:
					case PdfiumError.LicensingError:
						return true;

					default:
						return false;
				}
			}
			return false;
		}

		#endregion

		#region Paste

		protected virtual void Grid_PasteDocuments(object sender, EventArgs e)
		{
			if (!IsInsertAllowed)
			{
				Globals.Message.ShowError(Res.GetString("31F7A101-4B18-434D-B0CD-7F7C13C79B9C", "Manually uploading eDocs is not allowed."));
				return;
			}
			PasteDocuments();
		}

		protected void PasteDocuments()
		{
			if (IsEditable)
			{
#if !WINZOR
				var dataObject = SafeClipboard.GetDataObject();
#else
				var dataObject = SafeClipboard.GetServerDataObject();
#endif
				if (dataObject != null)
				{
					InsertFromData(dataObject);
				}
			}
		}

		#endregion

		#region Restore

		void DocumentsGrid_RestoreDocuments(object sender, EventArgs e)
		{
			RestoreDocuments(SelectedElements);
		}

		public void RestoreDocuments(BusinessObject[] documentsToRestore)
		{
			if (documentsToRestore.Length <= 0)
			{
				Globals.Message.Show(Res.GetString("1e754998-e296-466f-8766-5b62f2954112", "Please select the eDocs you want to restore."), Res.GetString("2e800c79-b02c-4459-a60f-1efe98ec00fe", "Restore Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			else
			{
				var prompt = Res.GetString("f3358780-47cb-4613-bd86-72351d7af53a", "Do you really want to restore the selected eDoc(s)?");
				if (Globals.Message.Show(prompt, Res.GetString("ddb62664-9310-49a6-afa1-7c39196e2638", "Confirm Restore"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					DocumentManipulationTarget.RestoreDocuments(documentsToRestore);
				}
			}
		}

		#endregion

		#region Allocate

		void DocumentsGrid_AllocateDocuments(object sender, EventArgs e)
		{
			try
			{
				var documentsNotAllocated = DocumentManipulationTarget.AllocateDocuments(SelectedElements);
				if (documentsNotAllocated.Any())
				{
					Globals.Message.Show(Res.GetString("ee284f00-96f6-4586-904c-2e98ef8df8c7", "Some eDocs were not allocated. Please ensure that the Type, Doc Type, Description and Allocate To fields have been filled in and the eDoc has no errors."), Res.GetString("9bee2897-7fb8-429d-92a3-d0e05890a757", "No eDocs Allocated"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (CanNotCreateSDDatabaseException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		#endregion

		#region Unallocate

		void DocumentsGrid_UnallocateDocuments(object sender, EventArgs e)
		{
			UnallocateDocuments(SelectedElements);
		}

		public void UnallocateDocuments(IEnumerable<BusinessObject> documentsToUnallocate)
		{
			var count = documentsToUnallocate.Take(2).Count();
			if (count <= 0)
			{
				Globals.Message.Show(Res.GetString("f781c9f0-cfba-428e-9b4b-d57d57814dce", "Please select the eDocs you want to unallocate."), Res.GetString("53e0ac6f-2d1f-4650-a2a4-1b3cee236938", "Select eDocs"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			var prompt = Res.GetString("930683ef-5c9d-40b2-82b3-a8ad8f1027c0", "Do you really want to unallocate the selected {0}?", (count > 1) ? "eDocs" : "eDoc");
			if (Globals.Message.Show(prompt, Res.GetString("1194f277-0f35-4b4f-94e9-2025f7c4e766", "Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				var documentsLeftover = DocumentManipulationTarget.UnallocateDocuments(documentsToUnallocate.Cast<StorageDocsBase>());

				if (documentsLeftover != null && documentsLeftover.Any())
				{
					Globals.Message.Show(Res.GetString("09d9c9cb-29e8-46be-a45d-78e2a4e529fb", "You cannot unallocate a deleted document or any files. If this is a deleted document, select 'Restore' and then unallocate it."), Res.GetString("64688656-c291-470f-aa44-14e53416c433", "Cannot Modify Deleted Documents Or Files"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}

		#endregion

		#region Print / Email / Fax

		internal void DocumentsGrid_DeliverDocument(object sender, EventArgs e)
		{
			try
			{
				DeliverDocument();
			}
			catch (ExternalStorageException ex)
			{
				Globals.Message.ShowError(ex.UnableToAccessStorageFriendlyMessage);
			}
			catch (VirusDetectedException ex)
			{
				Globals.Message.ShowError(ex.VirusDetectedFriendlyMessage);
			}
		}

		void DeliverDocument()
		{
			var pack = CreateDocumentPack();
			if (pack.Any())
			{
				var instructions = CreateInstructions(pack);
				SetDeliverySettingsOnInstructions(instructions);
				SetDeliveryMethodOnInstructions(instructions);
				using (var task = new PrintTask())
				{
					task.Add(pack);
					task.DeliveryInstructionsDefaultPK = DeliveryInstructionsDefaultPk;
					task.RunWithPartialInstructions(instructions.DeliveryOptions, instructions, Env.Security.None);
				}
			}
		}

		ZGuid DeliveryInstructionsDefaultPk => new ZGuid("6E9192E4-800B-44FE-9334-B4D5316D192B");

		protected virtual DeliveryInstructions CreateInstructions(DocumentPack pack)
		{
			return DeliveryInstructionsProvider.Get(pack);
		}

		protected void SetDeliveryMethodOnInstructions(DeliveryInstructions instructions)
		{
			var emailModes = new[] { Core.Constants.ContactNotifyModes.Email, Core.Constants.ContactNotifyModes.EPrint };
			var canOnlyEmail = instructions.DeliverablesToBePrinted
				.SelectMany(d => ((IDeliverable)d).GetSupportedDeliveryMethods())
				.All(m => emailModes.Contains(m));

			foreach (DocDeliveryContact recipient in instructions.Recipients)
			{
				using (recipient.GetValidationSuspender())
				{
					if (canOnlyEmail)
					{
						recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					}
					recipient.AttachmentTypeDisabled = true;
				}
			}
		}

		void SetDeliverySettingsOnInstructions(DeliveryInstructions instructions)
		{
			instructions.DeliveryOptions = AllowedDeliveryOptions.AllExceptPreview;
			instructions.AllowAutoDelivery = false;
			instructions.AttachmentTypeDisabled = true;
		}

		protected DocumentPack CreateDocumentPack()
		{
			var result = new DocumentPack();
			foreach (StorageDocsBase element in SelectedElements)
			{
				if (ConfirmDocumentDelivery(element))
				{
					element.IncludedInPrint = true;
					element.ShouldPrintByDefault = true;
					result.Add(element);
				}
			}
			return result;
		}

		#endregion

		#region Save File As

		void Grid_SaveFileAs(object sender, EventArgs e)
		{
			foreach (StorageDocsBase element in SelectedElements)
			{
				using (var dialog = new ZSaveFileDialog())
				{
					PopulateSaveFileDialogDefaults(dialog, element);

					if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
					{
						try
						{
							using (var stream = dialog.OpenFile())
							{
								element.SaveToStream(stream);
							}
						}
						catch (IOException ex)
						{
							Globals.Message.ShowError(ex.Message);
						}
						catch (ExternalStorageException ex)
						{
							Globals.Message.ShowError(ex.Message);
						}
						catch (VirusDetectedException ex)
						{
							Globals.Message.ShowError(ex.VirusDetectedFriendlyMessage);
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Extension Filter")]
		protected void PopulateSaveFileDialogDefaults(ZSaveFileDialog dialog, StorageDocsBase bizOToSave)
		{
			string extensionType = bizOToSave.EDocFormat;
			dialog.AddExtension = true;
			dialog.Filter = extensionType + " files (*." + extensionType + ")|*." + extensionType;
			dialog.DefaultExt = extensionType;
			dialog.FileName = bizOToSave.GetFileNameOnlyWithExtension();
		}

		#endregion

		#region SelectAll

		void Grid_SelectAllDocuments(object sender, EventArgs e)
		{
			SelectAll();
		}

		public void SelectAll()
		{
			for (var i = 0; i < ListManager.Count; i++)
			{
				if (!IsSelected(i))
				{
					Select(i);
				}
			}
		}

		#endregion

		#region DataObjects, Drag & Drop

		protected void Grid_DragLeave(object sender, EventArgs e)
		{
			DragDropHasLeftGrid = true;
		}

		protected void Grid_DragEnter(object sender, DragEventArgs e)
		{
			DragDropHasLeftGrid = false;
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			var mouseHoverPoint = PointToClient(ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y)));
			var bizOAtMouse = GetElementAtPosition(mouseHoverPoint);

			if (IsEditable)
			{
				if (e.Data.GetDataPresent(GetDataFormatType())) // dragging from another eDocs grid
				{
					var documentsToDrop = (SerializableEDocCollection)e.Data.GetData(GetDataFormatType());

					if (!documentsToDrop.ContainsSystemGeneratedDocuments)
					{
						e.Effect = GetApplicableDragEffect(bizOAtMouse, documentsToDrop);
					}
					else
					{
						e.Effect = DragDropEffects.None;
					}
				}
				else
				{
					e.Effect = ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) ? DragDropEffects.Copy : DragDropEffects.Move;
				}
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

#if WINZOR
		protected override void DoDragDrop()
		{
		}
#endif

		protected override void OnDragDrop(DragEventArgs e)
		{
			OnDragDropCore(e);
		}

		public void OnDragDropForRemote(DragEventArgs e)
		{
			OnDragDropCore(e);
		}

		void OnDragDropCore(DragEventArgs e)
		{
			base.OnDragDrop(e);

			if (!IsInsertAllowed)
			{
				Globals.Message.ShowError(Res.GetString("31F7A101-4B18-434D-B0CD-7F7C13C79B9C", "Manually uploading eDocs is not allowed."));
				return;
			}

			LastClickPoint = PointToClient(ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y)));

			if (IsEditable)
			{
				if (e.Data.GetDataPresent(DocManagerDataObject.DataFormatType))
				{
					var data = (SerializableEDocCollection)e.Data.GetData(DocManagerDataObject.DataFormatType);

					// semi-hack to solve weird bug. 
					// drag drop from unallocated grid to plugin grid.
					// click on any row back on the unallocated grid and it 
					// would try and paste the document back on itself.
					// so, check to see if you are trying to paste on yourself
					// before actually pasting!
					InsertData(data);
				}
				else
				{
					InsertFromData(e.Data);
				}
			}
		}

		internal DragDropEffects GetApplicableDragEffect(StorageDocsBase bizOAtMouse, SerializableEDocCollection documentsBeingDragged)
		{
			if (bizOAtMouse != null)
			{
				var isDraggable = !(bizOAtMouse.SC_IsSystemGenerated) && !documentsBeingDragged.ContainsPK(bizOAtMouse.PK) && IsAbleToInsertIntoDocument(bizOAtMouse, documentsBeingDragged);
				return isDraggable ? DragDropEffects.Move : DragDropEffects.None;
			}
			else
			{
				foreach (StorageDocsBase element in ListManager.List)
				{
					if (documentsBeingDragged.ContainsPK(element.PK))
					{
						return DragDropEffects.None;
					}
				}
				return DragDropEffects.Move;
			}
		}

		bool IsAbleToInsertIntoDocument(StorageDocsBase bizOAtMouse, SerializableEDocCollection collection)
		{
			if (bizOAtMouse != null)
			{
				return bizOAtMouse is StorageDocs && bizOAtMouse.IsImageFile && collection.ContainsOnlyImageFiles;
			}
			else
			{
				return false;
			}
		}

		protected override void OnDragDropCompleted(DragDropCompletedEventArgs dragDropArgs)
		{
			if (dragDropArgs.DragDropEffect == DragDropEffects.Move && !DragDropHasLeftGrid)
			{
				foreach (SerializableEDoc serializableEDoc in dragDropArgs.Data.Elements)
				{
					serializableEDoc.BaseBusinessObject.Delete();
				}
			}
		}

		protected string GetDataFormatType()
		{
			return DocManagerDataObject.DataFormatType;
		}

		protected override GridRowsDataObject GetDataObject(ICollection<BusinessObject> documentsToSerialize)
		{
			try
			{
				return new DocManagerDataObject(this, documentsToSerialize);
			}
			catch (Exception ex) when (ex is ZBlobReadException || ex is SqlStreamReaderRowNotFoundException)
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("ba258a39-956c-487d-b9a7-73e12ade1ddb", "While you were working, another user has modified this document. Please reload this form."));
				return new GridRowsDataObject(this, Array.Empty<BusinessObject>());
			}
			catch (ExternalStorageException ex)
			{
				Globals.Message.ShowError(ex.UnableToAccessStorageFriendlyMessage);
				return new GridRowsDataObject(this, Array.Empty<BusinessObject>());
			}
		}

		#endregion

		#region Review Parsed Results

		protected void Grid_ReviewParsedResultsMenuItem(object sender, EventArgs e)
		{
			if (ShowReviewParsedResultsMenuItem && CurrentElementAtMousePosition != null && CurrentElementAtMousePosition.ParentMain != null)
			{
				var dashUtils = ObjectFactory.Get<IDashUtils>();
				var correctionToolUrl = dashUtils.GetCorrectionToolUrl(CurrentElementAtMousePosition.PK.ToString());
				if (!string.IsNullOrEmpty(correctionToolUrl))
				{
					WebUrlLauncher.Launch(correctionToolUrl);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("8D2AC5D9-A5DA-4DE2-97BA-D6906A88E1CF", "Unable to launch the Correction Tool to review the parsed results. Please try again after sometime. If the problem persists, please raise an incident."));
				}
			}
		}

		#endregion

		#region Insert/Copy Data (private methods)

		protected virtual void InsertFromData(IDataObject dataToInsert)
		{
			if (IsEditable)
			{
				if (dataToInsert.GetDataPresent(GetDataFormatType()) && dataToInsert.GetData(GetDataFormatType()) is SerializableEDocCollection collection)
				{
					InsertData(collection);
				}
				else if (dataToInsert.GetDataPresent(DataFormats.FileDrop) && dataToInsert.GetData(DataFormats.FileDrop) is string[] values)
				{
					InsertData(values);
				}
				else
				{
					try
					{
						using (var insertableData = GetZDataObjectFromData(dataToInsert))
						using (ShouldReportIOException())
						{
							if (!insertableData.InfoNeedsToBeNotified.IsNullOrEmpty())
							{
								Globals.Message.ShowWarning(insertableData.InfoNeedsToBeNotified);
							}

							if (insertableData.FileDropCount > 0 && insertableData.GetData(DataFormats.FileDrop) is string[] names)
							{
								InsertData(names);
							}
						}
					}
					catch (NotSupportedException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
			}
		}

		protected virtual ZDataObject GetZDataObjectFromData(IDataObject dataToInsert)
		{
			return ZDataObject.FromDataWithMaximumLimitSizeInMB(dataToInsert, SystemDataRegistry.Instance.eDocsMaximumFilesize.Value, StorageDocsHelper.GetMaximumLimitSizeNotifications(SystemDataRegistry.Instance.eDocsMaximumFilesize));
		}

		void InsertData(SerializableEDocCollection eDocs)
		{
			if (DragDropTarget != null)
			{
				try
				{
					DragDropTarget.Add(eDocs);
				}
				catch (IOException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		void InsertData(string[] filenames)
		{
			using (new ZWaitCursorChanger())
			{
				if (DragDropTarget != null)
				{
					try
					{
						var fileValidator = new AcceptableFileValidator(false, filenames);

						var filesToInsert = fileValidator.GetValidFiles();
						string[] incompatibleFiles = null;

						incompatibleFiles = DragDropTarget.Add(filesToInsert);

						ShowWarningMessageIfNotEmpty(fileValidator.GetInvalidFilesMessage() + GetMessageIfIncompatibleTifFilesExist(incompatibleFiles));
					}
					catch (IOException ex)
					{
						Globals.Message.ShowError(ex.Message);

						if (shouldReportIOException)
						{
							ErrorReporter.ReportOnce("IOExceptionThrownWhenPastingSnippingToolImageToEDocs", ex);
						}
					}
					catch (CorruptedDocumentException corruptedDocEx)
					{
						Globals.Message.ShowError(Res.GetString("77e2e299-5fc6-421a-a8b3-44b2ee1b2698", "A file could not be opened due to unsupported format or corrupted file.") +
							System.Environment.NewLine + corruptedDocEx.Message);
					}
				}
			}
		}

		bool shouldReportIOException;

		IDisposable ShouldReportIOException()
		{
			shouldReportIOException = true;
			return new DisposableAction(() => shouldReportIOException = false);
		}

		#endregion

		#region Virus Scanning

		protected void Grid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var eDocsFile = (StorageDocsBase)e.ObjectAtRow;
			if (eDocsFile != null)
			{
				e.Colour = eDocsFile.IsVirusDetected
					? Color.FromArgb(235, 155, 155)
					: Color.Empty;
			}
		}

		#endregion

		#region Notification methods

		string GetMessageIfIncompatibleTifFilesExist(string[] filenames)
		{
			if (filenames != null && filenames.Length > 0)
			{
				var mainText = Res.GetString("b1243b5e-9d4b-4124-8e6f-895ad75c295f", @"The following files were not added because they are not in a recognized image file format.
Please add them directly to the eDocs tab of the appropriate form:");
				return System.Environment.NewLine + mainText + new FileListFormatter().FormatListToString(filenames, false);
			}
			else
			{
				return string.Empty;
			}
		}

		void ShowWarningMessageIfNotEmpty(ZString message)
		{
			if (!message.IsEmpty)
			{
				Globals.Message.Show(message, Res.GetString("95dfeefd-bfda-42d4-a630-b4fb3ef494b7", "Warning"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		#endregion

#if WINZOR
		protected override void OnAfterSetVisibleRowCount()
		{
			if (CopySelectedRowsAllowed && AllowBeginDrag && (AllowDragDropWithChanges || !IsDataSourceChanged) && IsValidSource(ListManager))
			{
				var files = new Dictionary<int, string>();

				foreach (var rowNumber in VisibleRowDataRowNumbers)
				{
					if (rowNumber < 0 || rowNumber >= ListManager.Count)
					{
						continue;
					}

					var row = ListManager.List[rowNumber];
					if (row is StorageDocsBase storageDoc)
					{
						var bizPK = string.Empty;
						if (storageDoc.ParentMain != null)
						{
							bizPK = storageDoc.ParentMain.SM_ParentFK.ToString();
						}
						files[rowNumber] = string.Join("|", (string)storageDoc.SC_FileNameWithExtension, storageDoc.PK.ToString(), bizPK);
					}
				}

				RegisterAfterRenderAction(async () =>
				{
					await (GetJSInterop<IGridJSInterop>()?.SetRowDragDataAsync(ElementReference, files) ?? Task.CompletedTask);
				});
			}
		}
#endif

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StorageDocImageViewer.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

#if !WINZOR

		/// <summary>
		/// Allows user to use established shortcuts for Cut/Paste to cut and paste
		/// </summary>
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case WindowsMessage.WM_COPY:
					if (!isScrolled || !ScrolledInLast2Seconds)// if drag scroll bar, it will send Control-C message, may be Microsoft's bug.
					{
						Copy?.Invoke(this, EventArgs.Empty);
					}
					isScrolled = false;
					return;//don't go to base.WdnProc, otherwise it will copy the text of selected row instead of the document.

				case WindowsMessage.WM_CUT:
					Cut?.Invoke(this, EventArgs.Empty);
					break;

				case WindowsMessage.WM_PASTE:
					Paste?.Invoke(this, EventArgs.Empty);
					break;

				default:
					break;
			}

			base.WndProc(ref m);
		}

#else

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.X | Keys.Control))
			{
				Cut?.Invoke(this, EventArgs.Empty);
			}

			if (keyData == (Keys.C | Keys.Control))
			{
				Copy?.Invoke(this, EventArgs.Empty);
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}
#endif
		/// <summary>
		/// Ctrl + V message cannot be captured in WndProc so handle it here.
		/// </summary>
		protected override void OnKeyDown(KeyEventArgs key)
		{
#if !WINZOR
			if (key.Modifiers == Keys.Control && key.KeyCode == Keys.V)
			{
				Paste?.Invoke(this, EventArgs.Empty);
			}
#endif
			base.OnKeyDown(key);
		}

#if DEBUG
		public
#else
		internal
#endif
			Point LastClickPoint;

		internal EventHandler Cut;
		internal EventHandler Copy;
		internal EventHandler Paste;
	}

	#region DeletingDocuments event classes

	public class DeletingDocumentsEventArgs : EventArgs
	{
		public DeletingDocumentsEventArgs(IEnumerable<BusinessObject> documentsToDelete)
		{
			DocumentsToDelete = documentsToDelete;
		}

		public IEnumerable<BusinessObject> DocumentsToDelete { get; private set; }
	}

	public delegate void DeletingDocumentsDelegate(object sender, DeletingDocumentsEventArgs e);

	#endregion
}
