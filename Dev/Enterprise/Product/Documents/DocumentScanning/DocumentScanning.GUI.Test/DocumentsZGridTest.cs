using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Interop.DataObjects;
using CargoWise.IO;
using CargoWise.PdfiumWrapper;
using CargoWise.Types;
using Enterprise.Dash.Integration.Services;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Constants = Enterprise.DocumentScanning.Business.Constants;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class DocumentsZGridTest : TestCaseWithDocumentFactory
	{
		public void TestGetDataObjectThrowExternalStorageException()
		{
			var originalTarget = grid.DocumentManipulationTarget;
			try
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();

				var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
				var document = StorageDocs.NewWithParent_DEBUG(masterFactory);
				masterFactory.Save();
				((UnitTestUserNotification)Globals.Message).AddAnswer(DialogResult.Yes);

				using (var plugin = new eDocsPlugInForTesting(org))
				using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "S3"))
				{
					grid.DocumentManipulationTarget = plugin;
					grid.GetDataObject(new StorageDocs[] { document });

					AssertEquals("Unable to access S3 storage, please contact your system administrator to check the configuration of the eDocs storage. Error message: Service URL can not be empty.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				grid.DocumentManipulationTarget = originalTarget;
			}
		}

		void DocumentsZGridTest_threadExceptionHappened(object sender, Exception ex)
		{
			var file = sender as SlowStorageFile;
			if (file != null)
			{
				file.ThreadException = ex;
				file.EndEvent.Set();
			}
		}

		public void TestDocumentTypeSecurity()
		{
			grid.CurrentRowIndex = 0;

			//Grid.CurrentElement.DocType.RT_SE_NKDocumentReceivedEvent = "ECM";

			/*var dummy = Factory.NewWithValidTestData<OrgHeader>();
			dummy.OH_Code = "ZYZ";

			Grid.CurrentElement.ParentMain.SM_Type = "ORG";
			Grid.CurrentElement.ParentMain.SM_ParentFK = dummy.PK;

			Factory.Save();
			Grid.CurrentElement.Factory.Save();*/

			grid.CurrentElement.SC_DocType = "AGI";
			grid.EditProperties(grid.CurrentElement);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

			Env.Security.GetDocumentTypeUploadCheckPoint("ACV").IsAllowed = false;
			grid.CurrentElement.SC_DocType = "ACV";
			grid.EditProperties(grid.CurrentElement);

			AssertContains("You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDoubleClickOnHeaderRow()
		{
			AssertEquals(0, grid.DoubleClickCounter);
			grid.LastClickPoint = new Point(10, 5);//header row
			grid.FireDoubleClickedIfApplicable();
			AssertEquals("should not process for header double click", 0, grid.DoubleClickCounter);
		}

		[ExpectNoExceptions]
		public void TestViewWithNull()
		{
			grid.View(null);
		}

		public void TestViewDeletedEDoc()
		{
			grid.CurrentRowIndex = 0;
			grid.CurrentElement.SC_IsDeleted = true;

			grid.StorageDocViewed += doc =>
			{
				AssertEquals(true, doc.ReadOnly);
			};

			grid.View(grid.CurrentElement);
		}

		public void TestGetElementAtPosition()
		{
			AssertEquals("should return null if not on a valid row", null, grid.GetElementAtPosition(new Point(0, 0)));
			AssertEquals("Should return a proper bizo if a valid row", documentAtMouse, grid.GetElementAtPosition(startingClickPoint));
		}

		public void TestSelectAll()
		{
			AssertEquals("Precondition: three elements in grid's list", 3, grid.ListManager.List.Count);

			for (var i = 0; i < grid.ListManager.Count; i++)
			{
				grid.UnSelect(i);
			}

			grid.SelectAll();

			for (var i = 0; i < grid.ListManager.Count; i++)
			{
				AssertEquals("Grid's element should be selected", true, grid.IsSelected(i));
			}
		}

		public void TestNewEventCreatedIfDocTypeChanged()
		{
			grid.CurrentRowIndex = 0;
			grid.CurrentElement.SC_DocType = "ACV";
			grid.CurrentElement.DocType.RT_SE_NKDocumentReceivedEvent = "ECM";
			grid.OldDocType = grid.CurrentElement.DocType;
			grid.CurrentElement.SC_DocType = "PUB";
			grid.CurrentElement.DocType.RT_SE_NKDocumentReceivedEvent = "AID";

			var dummy = Factory.NewWithValidTestData<OrgHeader>();
			dummy.OH_Code = "ZYZ";

			grid.CurrentElement.ParentMain.SM_Type = "ORG";
			grid.CurrentElement.ParentMain.SM_ParentFK = dummy.PK;

			Factory.Save();
			grid.CurrentElement.Factory.Save();
			var reference1 = grid.CurrentElement.CreateReference();

			var logs = ((EnterpriseBusinessObject)grid.CurrentElement.ParentMain.DocumentOwner).Logs;
			ZString pkString = grid.CurrentElement.PK.ToString();

			Factory.Save();
			grid.CurrentElement.Factory.Save();

			grid.DocumentsZGrid_CloseOfForm(null, new DocumentEventArgs(grid.CurrentElement));

			AssertEquals("Should not log new DDI event.", 0, logs.Find(new ZQuery(StmALogSchema.SL_Reference, reference1)).Length);
			AssertEquals("Should log new AID event.", 1, logs.Find(new ZQuery(StmALogSchema.SL_Reference, pkString).AddToFilter(StmALogSchema.SL_SE_NKEvent, "AID")).Length);
		}

		public void TestNewEventCreatedIfDocSourceChanged()
		{
			using (SystemDataRegistry.Instance.DDIDocumentSource.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SystemDataRegistry.Instance.DDIDocumentSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var source1 = Factory.NewWithValidTestData<RefDocSource>();
				source1.RDS_Code = "ABZ";
				var source2 = Factory.NewWithValidTestData<RefDocSource>();
				source2.RDS_Code = "DEX";
				Factory.Save();

				grid.CurrentRowIndex = 0;
				grid.CurrentElement.SC_DocType = "ACV";
				grid.CurrentElement.SC_RDS_NKDocSource = "ABZ";
				grid.CurrentElement.DocType.RT_SE_NKDocumentReceivedEvent = "ECM";
				grid.OldDocSource = grid.CurrentElement.DocSource;
				grid.CurrentElement.SC_RDS_NKDocSource = "DEX";
				grid.CurrentElement.DocType.RT_SE_NKDocumentReceivedEvent = "AID";

				var dummy = Factory.NewWithValidTestData<OrgHeader>();
				dummy.OH_Code = "ZYZ";

				grid.CurrentElement.ParentMain.SM_Type = "ORG";
				grid.CurrentElement.ParentMain.SM_ParentFK = dummy.PK;

				Factory.Save();
				grid.CurrentElement.Factory.Save();
				var reference1 = grid.CurrentElement.CreateReference();

				var logs = ((EnterpriseBusinessObject)grid.CurrentElement.ParentMain.DocumentOwner).Logs;
				ZString pkString = grid.CurrentElement.PK.ToString();

				Factory.Save();
				grid.CurrentElement.Factory.Save();

				grid.DocumentsZGrid_CloseOfForm(null, new DocumentEventArgs(grid.CurrentElement));

				AssertEquals("Should not log new DDI event.", 0, logs.Find(new ZQuery(StmALogSchema.SL_Reference, reference1)).Length);
				AssertEquals("Should log new AID event.", 1, logs.Find(new ZQuery(StmALogSchema.SL_Reference, pkString).AddToFilter(StmALogSchema.SL_SE_NKEvent, "AID")).Length);
			}
		}

		[DeveloperOnlyTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcess", Justification = "Testing")]
		public void TestOpenImageUsingDefaultProgram()
		{
			grid.CurrentRowIndex = 0;
			grid.CurrentElement.SC_ImageData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

			var dummy = Factory.NewWithValidTestData<OrgHeader>();
			dummy.OH_Code = "ZYZ";

			grid.CurrentElement.ParentMain.SM_Type = "ORG";
			grid.CurrentElement.ParentMain.SM_ParentFK = dummy.PK;

			Factory.Save();
			grid.CurrentElement.Factory.Save();

			SystemDataRegistry.Instance.UseDefaultWindowsImageViewer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Globals.SetIsUnitTestingProductionFunctionality(true);
			grid.FireDoubleClickedIfApplicable();
			Thread.Sleep(500);

			var tempFileName = Path.GetFileName(grid.CurrentElement.TempFileName);
			Process openProcess = null;
			var processOpened = false;
			var processes = Process.GetProcesses();
			foreach (var process in processes)
			{
				if (process.MainWindowTitle.Contains(tempFileName))
				{
					openProcess = process;
					processOpened = true;
					break;
				}
			}

			if (openProcess != null)
			{
				openProcess.CloseMainWindow();
			}

			File.SetAttributes(grid.CurrentElement.TempFileName, FileAttributes.Normal);
			grid.CurrentElement.Dispose();
			Globals.SetIsUnitTestingProductionFunctionality(false);

			Assert("Should have spawned a process opening the image file", processOpened);
		}

		public void TestDocTypeChanged_NewDocTypeIsNull()
		{
			grid.CurrentRowIndex = 0;
			grid.CurrentElement.SC_DocType = "ACV";
			grid.CurrentElement.DocType.RT_SE_NKDocumentReceivedEvent = "";
			grid.OldDocType = grid.CurrentElement.DocType;
			grid.CurrentElement.SC_DocType = "PPP";

			var dummy = Factory.NewWithValidTestData<OrgHeader>();
			dummy.OH_Code = "ZYZ";

			grid.CurrentElement.ParentMain.SM_Type = "ORG";
			grid.CurrentElement.ParentMain.SM_ParentFK = dummy.PK;

			Factory.Save();
			grid.CurrentElement.Factory.Save();

			var logs = ((EnterpriseBusinessObject)grid.CurrentElement.ParentMain.DocumentOwner).Logs;
			ZString pkString = grid.CurrentElement.PK.ToString();

			//simulate logs having been added
			logs.AddNew(
				Enterprise.ZArchitecture.Business.Events.DocumentImported, string.Concat("ACV|", pkString));
			logs.AddNew(
				Enterprise.ZArchitecture.Business.Events.All["ECM"], pkString);

			Factory.Save();
			grid.CurrentElement.Factory.Save();

			AssertNoExceptionThrown(() => grid.DocumentsZGrid_CloseOfForm(null, new DocumentEventArgs(grid.CurrentElement)));
		}

		public void TestDocumentsZGrid_CloseOfForm_ClearsNotificationsOnDialogResultCancel()
		{
			var main = MasterFactory.New<StorageMain>();
			var storageDoc = MasterFactory.New<StorageFile>();
			storageDoc.SC_SM = main.PK;
			storageDoc.SC_FileNameForRenaming = "somecrapwithoutextension";
			Assert("Should have Notifications", storageDoc.SC_FileNameForRenamingInfo.HasNotifications());

			grid.FindForm().DialogResult = DialogResult.Cancel;
			storageDoc.SetNewFileName("zayden.doc");
			Assert("Should have Notifications", storageDoc.SC_FileNameForRenamingInfo.HasNotifications());

			grid.DocumentsZGrid_CloseOfForm(grid.FindForm(), new DocumentEventArgs(storageDoc));
			AssertEquals("Should have Notifications", false, storageDoc.SC_FileNameForRenamingInfo.HasNotifications());
		}

		public void TestDocumentsZGrid_CloseOfForm_DoesNotClearNotificationsOnDialogResultOK()
		{
			var main = MasterFactory.New<StorageMain>();
			var storageDoc = MasterFactory.New<StorageFile>();
			storageDoc.SC_SM = main.PK;
			storageDoc.SC_FileNameForRenaming = "somecrapwithoutextension";
			Assert("Should have Notifications", storageDoc.SC_FileNameForRenamingInfo.HasNotifications());

			grid.FindForm().DialogResult = DialogResult.OK;
			storageDoc.SetNewFileName("zayden.doc");
			Assert("Should have Notifications", storageDoc.SC_FileNameForRenamingInfo.HasNotifications());

			grid.DocumentsZGrid_CloseOfForm(grid.FindForm(), new DocumentEventArgs(storageDoc));
			AssertEquals("Should still have Notifications", true, storageDoc.SC_FileNameForRenamingInfo.HasNotifications());
		}

		public void TestCurrentElementAtMousePosition()
		{
			grid.LastClickPoint = new Point(0, 0);
			AssertNull("Should return null - there is nothing at point 0,0", grid.CurrentElementAtMousePosition);

			grid.LastClickPoint = startingClickPoint;
			AssertEquals("Should return a valid bizO", documentAtMouse, grid.CurrentElementAtMousePosition);
		}

		public void TestCurrentElement()
		{
			grid.CurrentRowIndex = 0;
			AssertEquals("Should return a valid bizO", documentAtMouse, grid.CurrentElement);

			grid.CurrentRowIndex = 1;
			AssertEquals("Should return a valid bizo", documentInListButNotAtMouse, grid.CurrentElement);

			grid.CurrentRowIndex = 2;
			AssertEquals("Should return a valid bizo", fileInListButNotAtMouse, grid.CurrentElement);
		}

		public void TestIsEditable()
		{
			AssertEquals("Grid should be editable by default", true, grid.IsEditable);

			grid.ReadOnly = true;
			AssertEquals("Grid is not editable if the control is readonly", false, grid.IsEditable);

			grid.ReadOnly = false;
			AssertEquals("Grid is editable again when the control is not readonly", true, grid.IsEditable);

			grid.IsEditable = false;
			AssertEquals("Grid is not editable if you set the editable property to be false automatically", false, grid.IsEditable);

			grid.IsEditable = true;
			AssertEquals("Grid is editable again", true, grid.IsEditable);

			try
			{
				Env.Security.eDocsModify.IsAllowed = false;
				AssertEquals("Grid is not editable if you dn't have modify security rights", false, grid.IsEditable);
			}
			finally
			{
				Env.Security.eDocsModify.IsAllowed = true;
			}

			AssertEquals("Grid is editable again", true, grid.IsEditable);
		}

		public void TestSelectedElements()
		{
			grid.CurrentRowIndex = 0;
			grid.Select(0);
			AssertEquals("Selected elements count should include anything selected", 1, grid.SelectedElements.Length);

			grid.CurrentRowIndex = 1;
			AssertEquals("Selected elements should have those selected, plus the current row index", 2, grid.SelectedElements.Length);
		}

		#region Context Menu tests

		public void TestContextMenuPopup()
		{
			grid.LastClickPoint = new Point(0, 0);
			grid.ContextMenu_Popup(this, EventArgs.Empty);
			Assert("When context menu pops up, it should set the LastClickPoint in the grid to current mouse position", grid.LastClickPoint != new Point(0, 0));
		}

		public void TestSetupContextMenu()
		{
			grid.SetupContextMenu();
			AssertNotNull("Select All menu option should be available in base grid", FindMenuItemByName(grid.ContextMenu, Constants.SelectAllMenuText));
			AssertNotNull("Save menu option should be available in base grid", FindMenuItemByName(grid.ContextMenu, Constants.SaveFileAsMenuText));

			AssertNull("View menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText));
			AssertNull("EditProperties menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText));
			AssertNull("Copy menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText));
			AssertNull("Copy link menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText));
			AssertNull("Cut menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText));
			AssertNull("Paste menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText));
			AssertNull("Delete menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText));
			AssertNull("DeletePermanently menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText));
			AssertNull("Restore menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText));
			AssertNull("Allocate menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText));
			AssertNull("Unallocate menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText));
			AssertNull("DeliverDocument menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText));
			AssertNull("ReviewParsedResults menu option should not be available in base grid by default (false in designer)", FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText));
		}

		public void TestSetupContextMenuShowView()
		{
			grid.ShowViewMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("View menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText));

			grid.ShowViewMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("View menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText));
		}

		public void TestSetupContextMenuShowEditProperties()
		{
			grid.ShowEditPropertiesMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("EditProperties menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText));

			grid.ShowEditPropertiesMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("EditProperties menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText));
		}

		public void TestSetupContextMenuShowCopy()
		{
			grid.ShowCopyMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("Copy menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText));

			grid.ShowCopyMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("Copy menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText));
		}

		public void TestSetupContextMenuShowCopyLink()
		{
			grid.ShowCopyLinkMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("Copy link menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText));

			grid.ShowCopyLinkMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("Copy link menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText));
		}

		public void TestSetupContextMenuShowPaste()
		{
			grid.ShowPasteMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("Paste menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText));

			grid.ShowPasteMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("Paste menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText));
		}

		public void TestSetupContextMenuShowCut()
		{
			grid.ShowCutMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("Cut menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText));

			grid.ShowCutMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("Cut menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText));
		}

		public void TestSetupContextMenuShowDelete()
		{
			grid.ShowDeleteMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("Delete menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText));

			grid.ShowDeleteMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("Delete menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText));
		}

		public void TestSetupContextMenuShowDeletePermanently()
		{
			grid.ShowDeletePermanentlyMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("DeletePermanently menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText));

			grid.ShowDeletePermanentlyMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("DeletePermanently menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText));

			try
			{
				Env.Security.eDocsPermanentDelete.IsAllowed = false;
				FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText).PerformClick();

				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> DocManager -> eDocs Permanent Delete", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				Env.Security.eDocsPermanentDelete.IsAllowed = true;
			}
		}

		public void TestSetupContextMenuShowRestore()
		{
			grid.ShowRestoreMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("Restore menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText));

			grid.ShowRestoreMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("Restore menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText));
		}

		public void TestSetupContextMenuShowAllocate()
		{
			grid.ShowAllocateMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("Allocate menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText));

			grid.ShowAllocateMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("Allocate menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText));
		}

		public void TestSetupContextMenuShowUnallocate()
		{
			grid.ShowUnallocateMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("Unallocate menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText));

			grid.ShowUnallocateMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("Unallocate menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText));
		}

		public void TestSetupContextMenuShowDeliverDocument()
		{
			grid.ShowDeliverDocumentMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("DeliverDocument menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText));

			grid.ShowDeliverDocumentMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("DeliverDocument menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText));
		}

		public void TestSetupContextMenuShowShowParseDocument()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				grid.ShowParseDocumentMenuItem = false;
				grid.SetupContextMenu();
				AssertNull("ParseDocument menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText));

				grid.ShowParseDocumentMenuItem = true;
				grid.SetupContextMenu();
				AssertNotNull("ParseDocument menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText));
			}
		}

		public void TestSetupContextMenuShowReviewParsedResults()
		{
			grid.ShowReviewParsedResultsMenuItem = false;
			grid.SetupContextMenu();
			AssertNull("Review Parsed Results menu item shouldn't be available any more", FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText));

			grid.ShowReviewParsedResultsMenuItem = true;
			grid.SetupContextMenu();
			AssertNotNull("Review Parsed Results menu item should exist now", FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText));
		}

		public void TestRebuildContextMenu()
		{
			grid.ShowAllocateMenuItem = false;
			grid.ShowCopyMenuItem = false;
			grid.ShowCopyLinkMenuItem = false;
			grid.ShowCutMenuItem = false;
			grid.ShowDeleteMenuItem = false;
			grid.ShowSplitDocumentMenuItem = false;
			grid.ShowDeletePermanentlyMenuItem = false;
			grid.ShowDeliverDocumentMenuItem = false;
			grid.ShowEditPropertiesMenuItem = false;
			grid.ShowPasteMenuItem = false;
			grid.ShowRestoreMenuItem = false;
			grid.ShowUnallocateMenuItem = false;
			grid.ShowViewMenuItem = false;
			grid.ShowParseDocumentMenuItem = false;
			grid.ShowReviewParsedResultsMenuItem = false;

			grid.RebuildContextMenu();

			foreach (MenuItem item in grid.ContextMenu.MenuItems)
			{
				Assert("Allocate menu item should not be in the list", item.Text != Constants.AllocateMenuText);
				Assert("Copy menu item should not be in the list", item.Text != Constants.CopyMenuText);
				Assert("Copy Link menu item should not be in the list", item.Text != Constants.CopyLinkMenuText);
				Assert("Cut menu item should not be in the list", item.Text != Constants.CutMenuText);
				Assert("DeletePermanently menu item should not be in the list", item.Text != Constants.DeletePermanentlyMenuText);
				Assert("SplitDocument menu item should not be in the list", item.Text != Constants.SplitDocumentMenuText);
				Assert("Deliver menu item should not be in the list", item.Text != Constants.DeliverDocumentMenuText);
				Assert("Edit Properties menu item should not be in the list", item.Text != Constants.EditPropertiesMenuText);
				Assert("Paste menu item should not be in the list", item.Text != Constants.PasteMenuText);
				Assert("Restore menu item should not be in the list", item.Text != Constants.RestoreMenuText);
				Assert("Unallocate menu item should not be in the list", item.Text != Constants.UnallocateMenuText);
				Assert("View menu item should not be in the list", item.Text != Constants.ViewMenuText);
				Assert("Parse menu item should not be in the list", item.Text != Constants.ParseDocumentMenuText);
				Assert("Review Parsed Results menu item should not be in the list", item.Text != Constants.ReviewParsedResultsMenuText);
			}

			grid.ShowAllocateMenuItem = true;
			grid.RebuildContextMenu();

			var allocateFound = false;
			var copyFound = false;
			foreach (MenuItem item in grid.ContextMenu.MenuItems)
			{
				if (item.Text == Constants.AllocateMenuText)
				{
					allocateFound = true;
				}

				if (item.Text == Constants.CopyMenuText)
				{
					copyFound = true;
				}
			}

			Assert("Allocate menu item should be in the list", allocateFound);
			Assert("Meanwhile copy (still not shown in menu) should not be in the list", !copyFound);
		}

		public void TestUpdateContextMenuMultipleSelectedNotReadonly()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);

			grid.ShowAllocateMenuItem = true;
			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowSplitDocumentMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowRestoreMenuItem = true;
			grid.ShowUnallocateMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = false;

			grid.RebuildContextMenu();
			grid.Select(0);
			grid.Select(1);
			grid.UpdateContextMenuElements(document);

			Assert("Allocate is enabled", FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText).Enabled);
			Assert("Copy is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is disabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Delete is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
			Assert("SplitDocument is disabled", !FindMenuItemByName(grid.ContextMenu, Constants.SplitDocumentMenuText).Enabled);
			Assert("Delete permanently is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText).Enabled);
			Assert("deliver is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Edit Properties is not enabled (multiple elements can't be edited)", !FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("Paste is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("restore is enabled", FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			Assert("Unallocate is enabled", FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText).Enabled);
			Assert("ParseDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);

			gridCollection[0].SC_IsSystemGenerated = false;
			gridCollection[1].SC_IsSystemGenerated = true;
			grid.UpdateContextMenuElements(document);
			Assert("Cut is not enabled when selected documents contains system generated ones.", !FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
		}

		public void TestUpdateContextMenuMultipleSelectedReadOnly()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);

			grid.ShowAllocateMenuItem = true;
			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowRestoreMenuItem = true;
			grid.ShowUnallocateMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = true;

			grid.RebuildContextMenu();

			grid.Select(0);
			grid.Select(1);
			grid.UpdateContextMenuElements(document);
			Assert("Allocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText).Enabled);
			Assert("Copy is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Delete is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
			Assert("Delete permanently is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText).Enabled);
			Assert("Deliver is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Edit Properties is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("Paste is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("restore is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			Assert("Unallocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText).Enabled);
			Assert("Parse Document is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);
		}

		public void TestUpdateContextMenuSingleDocSelectedNotReadonly()
		{
			var document = MasterFactory.New<StorageDocsUnallocated>();

			grid.ShowAllocateMenuItem = true;
			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowRestoreMenuItem = true;
			grid.ShowUnallocateMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = false;

			grid.RebuildContextMenu();

			grid.UpdateContextMenuElements(document);
			Assert("Allocate is enabled", FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText).Enabled);
			Assert("Copy is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			// delete permanently not used on normal grid
			Assert("deliver is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Edit Properties is enabled", FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("Paste is enabled", FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("restore is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			Assert("Unallocate is not enabled - doc's not currently allocated", !FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText).Enabled);
			Assert("ParseDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			AssertNotNull("Review Parsed Results is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);
		}

		public void TestUpdateContextMenuSingleDocSelectedNotReadonlyDeleted()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_IsDeleted = true;

			grid.ShowAllocateMenuItem = true;
			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowSplitDocumentMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowRestoreMenuItem = true;
			grid.ShowUnallocateMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = false;

			grid.RebuildContextMenu();

			grid.UpdateContextMenuElements(document);
			Assert("Allocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText).Enabled);
			Assert("Copy is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Delete is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
			Assert("SplitDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.SplitDocumentMenuText).Enabled);
			Assert("Delete permanently is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText).Enabled);
			Assert("deliver is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Edit Properties is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("Paste is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("restore is enabled", FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			Assert("Unallocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText).Enabled);
			Assert("Parse Document is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);
		}

		public void TestUpdateContextMenuSingleDocSelectedReadOnly()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);

			grid.ShowAllocateMenuItem = true;
			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowRestoreMenuItem = true;
			grid.ShowUnallocateMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = true;

			grid.RebuildContextMenu();

			grid.UpdateContextMenuElements(document);
			Assert("Allocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText).Enabled);
			Assert("Copy is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Delete is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
			Assert("Delete permanently is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText).Enabled);
			Assert("deliver is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Edit Properties is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("Paste is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("restore is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			Assert("Unallocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText).Enabled);
			Assert("Parse Document is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);
		}

		public void TestUpdateContextMenuSingleDocSelectedReadOnlyDeleted()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_IsDeleted = true;

			grid.ShowAllocateMenuItem = true;
			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowSplitDocumentMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowRestoreMenuItem = true;
			grid.ShowUnallocateMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = true;

			grid.RebuildContextMenu();

			grid.UpdateContextMenuElements(document);
			Assert("Allocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText).Enabled);
			Assert("Copy is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Delete is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
			Assert("SplitDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.SplitDocumentMenuText).Enabled);
			Assert("Delete permanently is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText).Enabled);
			Assert("deliver is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Edit Properties is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("Paste is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("restore is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			Assert("Unallocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText).Enabled);
			Assert("Parse Document is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);
		}

		public void TestUpdateContextMenuSingleDocSystemGeneratedSelectedNotReadonly()
		{
			var document = MasterFactory.New<StorageDocsUnallocated>();
			document.SC_IsSystemGenerated = true;

			grid.ShowAllocateMenuItem = true;
			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowRestoreMenuItem = true;
			grid.ShowUnallocateMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = false;

			grid.RebuildContextMenu();

			grid.UpdateContextMenuElements(document);
			Assert("Allocate is enabled", FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText).Enabled);
			Assert("Copy is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Delete is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
			// delete permanently not used on normal grid
			Assert("deliver is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Edit Properties is enabled for system generated", FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("Paste is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("restore is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			Assert("Unallocate is not enabled - doc's not currently allocated", !FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText).Enabled);
			Assert("ParseDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);
		}

		public void TestUpdateContextMenuSingleDocSystemGeneratedSelectedNotReadonlyDeleted()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_IsSystemGenerated = true;
			document.SC_IsDeleted = true;

			grid.ShowAllocateMenuItem = true;
			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowSplitDocumentMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowRestoreMenuItem = true;
			grid.ShowUnallocateMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = false;

			grid.RebuildContextMenu();

			grid.UpdateContextMenuElements(document);
			Assert("Allocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText).Enabled);
			Assert("Copy is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Delete is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
			Assert("SplitDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.SplitDocumentMenuText).Enabled);
			Assert("Delete permanently is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText).Enabled);
			Assert("deliver is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Edit Properties is not enabled for system generated doc", !FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("Paste is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("restore is enabled", FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			Assert("Unallocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText).Enabled);
			Assert("ParseDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);
		}

		public void TestUpdateContextMenuSingleDocSystemGeneratedSelectedReadOnly()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_IsSystemGenerated = true;

			grid.ShowAllocateMenuItem = true;
			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowRestoreMenuItem = true;
			grid.ShowUnallocateMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = true;

			grid.RebuildContextMenu();

			grid.UpdateContextMenuElements(document);
			Assert("Allocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText).Enabled);
			Assert("Copy is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Delete is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
			Assert("Delete permanently is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText).Enabled);
			Assert("deliver is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Edit Properties is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("Paste is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("restore is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			Assert("Unallocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText).Enabled);
			Assert("ParseDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);
		}

		public void TestUpdateContextMenuSingleDocSystemGeneratedSelectedReadOnlyDeleted()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_IsSystemGenerated = true;
			document.SC_IsDeleted = true;

			grid.ShowAllocateMenuItem = true;
			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowSplitDocumentMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowRestoreMenuItem = true;
			grid.ShowUnallocateMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = true;

			grid.RebuildContextMenu();

			grid.UpdateContextMenuElements(document);
			Assert("Allocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText).Enabled);
			Assert("Copy is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Delete is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
			Assert("SplitDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.SplitDocumentMenuText).Enabled);
			Assert("Delete permanently is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText).Enabled);
			Assert("deliver is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Edit Properties is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("Paste is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("restore is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			Assert("Unallocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText).Enabled);
			Assert("ParseDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);
		}

		public void TestUpdateContextMenuNoDocumentNotReadonly()
		{
			grid.ShowAllocateMenuItem = true;
			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowRestoreMenuItem = true;
			grid.ShowUnallocateMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = false;

			grid.RebuildContextMenu();

			grid.UpdateContextMenuElements(null);
			Assert("Allocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText).Enabled);
			Assert("Copy is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Delete is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
			Assert("Delete permanently is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText).Enabled);
			Assert("deliver is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Edit Properties is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("Paste is enabled", FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("restore is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			Assert("Unallocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText).Enabled);
			Assert("ParseDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);
		}

		public void TestUpdateContextMenuNoDocumentReadOnly()
		{
			grid.ShowAllocateMenuItem = true;
			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowRestoreMenuItem = true;
			grid.ShowUnallocateMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = true;

			grid.RebuildContextMenu();

			grid.UpdateContextMenuElements(null);
			Assert("Allocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.AllocateMenuText).Enabled);
			Assert("Copy is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Delete is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeleteMenuText).Enabled);
			Assert("Delete permanently is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeletePermanentlyMenuText).Enabled);
			Assert("Deliver is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Edit Properties is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("Paste is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("restore is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.RestoreMenuText).Enabled);
			Assert("Unallocate is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.UnallocateMenuText).Enabled);
			Assert("ParseDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);
		}

		public void TestUpdateContextMenuSingleDocSelectedNotReadonlyViewable()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var document = StorageDocs.NewWithParent_DEBUG(masterFactory);
			document.SC_DataType = Core.Constants.FileFormats.PDF;
			masterFactory.Save();

			AssemblyDataLookup.AddAssemblyDataForTesting(DeliveryAuthorizingDocumentOwner.DocManagerCode, new DeliveryAuthorizingDocumentOwnerAssemblyData());

			var owner = masterFactory.New<DeliveryAuthorizingDocumentOwner>();
			owner.DocumentViewEnabled = false;
			owner.DocumentDeliveryDisclaimerRequired = false;

			var storageMain = document.ParentMain;
			storageMain.SM_ParentFK = owner.PK;
			storageMain.SM_Type = DeliveryAuthorizingDocumentOwner.DocManagerCode;

			grid.ShowCopyMenuItem = true;
			grid.ShowCopyLinkMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowDeleteMenuItem = true;
			grid.ShowDeletePermanentlyMenuItem = true;
			grid.ShowDeliverDocumentMenuItem = true;
			grid.ShowEditPropertiesMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.ShowViewMenuItem = true;
			grid.ShowSplitDocumentMenuItem = true;
			grid.ShowParseDocumentMenuItem = true;
			grid.ShowReviewParsedResultsMenuItem = true;
			grid.ReadOnly = false;

			grid.RebuildContextMenu();
			grid.UpdateContextMenuElements(document);
			Assert("deliver is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Paste is enabled", FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("Copy is disabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is disabled", !FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is disabled", !FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Edit Properties is disabled", !FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("View is disabled", !FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText).Enabled);
			Assert("SplitDocument is disabled", !FindMenuItemByName(grid.ContextMenu, Constants.SplitDocumentMenuText).Enabled);
			Assert("SaveFileAs is disabled", !FindMenuItemByName(grid.ContextMenu, Constants.SaveFileAsMenuText).Enabled);
			Assert("ParseDocument is disabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is disabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);

			owner.DocumentViewEnabled = true;
			grid.RebuildContextMenu();
			grid.UpdateContextMenuElements(document);
			Assert("deliver is enabled", FindMenuItemByName(grid.ContextMenu, Constants.DeliverDocumentMenuText).Enabled);
			Assert("Paste is enabled", FindMenuItemByName(grid.ContextMenu, Constants.PasteMenuText).Enabled);
			Assert("Copy is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CopyMenuText).Enabled);
			Assert("Copy link is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CopyLinkMenuText).Enabled);
			Assert("Cut is enabled", FindMenuItemByName(grid.ContextMenu, Constants.CutMenuText).Enabled);
			Assert("Edit Properties is enabled", FindMenuItemByName(grid.ContextMenu, Constants.EditPropertiesMenuText).Enabled);
			Assert("View is enabled", FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText).Enabled);
			Assert("SplitDocument is enabled", FindMenuItemByName(grid.ContextMenu, Constants.SplitDocumentMenuText).Enabled);
			Assert("SaveFileAs is enabled", FindMenuItemByName(grid.ContextMenu, Constants.SaveFileAsMenuText).Enabled);
			Assert("ParseDocument is not enabled", !FindMenuItemByName(grid.ContextMenu, Constants.ParseDocumentMenuText).Enabled);
			Assert("Review Parsed Results is disabled", !FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText).Enabled);
		}

		public void TestMenuItemEnabledStatusSingleDocSelected()
		{
			var editMenuItem = grid.FindParentFormEditMenuItem();
			var cutMenuItem = editMenuItem.MenuItems.FindByText(Constants.CutMenuText);
			var copyMenuItem = editMenuItem.MenuItems.FindByText(Constants.CopyMenuText);
			var pasteMenuItem = editMenuItem.MenuItems.FindByText(Constants.PasteMenuText);

			grid.ShowCopyMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.AddCutCopyPasteSelectMenuItems(grid.ContextMenu);
			grid.ListManager.Position = 0;
			grid.Select(0);
			grid.Focus();

			grid.ReadOnly = false;
			gridCollection[0].SC_IsSystemGenerated = false;
			grid.UpdateEditMenuItemEnableStatus();
			Assert("Cut is enabled", cutMenuItem.Enabled);
			Assert("Copy is enabled", copyMenuItem.Enabled);
			Assert("Paste is enabled", pasteMenuItem.Enabled);

			gridCollection[0].SC_IsSystemGenerated = true;
			grid.UpdateEditMenuItemEnableStatus();
			Assert("Cut is not enabled when document is system generated.", !cutMenuItem.Enabled);
			Assert("Copy is enabled", copyMenuItem.Enabled);
			Assert("Paste is not enabled", !pasteMenuItem.Enabled);

			gridCollection[0].SC_IsSystemGenerated = false;
			grid.ReadOnly = true;
			grid.UpdateEditMenuItemEnableStatus();
			Assert("Cut is not enabled when grid is read only.", !cutMenuItem.Enabled);
			Assert("Copy is enabled", copyMenuItem.Enabled);
			Assert("Paste is not enabled when grid is read only.", !pasteMenuItem.Enabled);

			var text = new ZTextBox();
			grid.FindForm().Controls.Add(text);
			text.Focus();
			Assert("Lost focus", !grid.Focused);
			grid.UpdateEditMenuItemEnableStatus();
			Assert("When focus is lost, Cut is enabled", cutMenuItem.Enabled);
			Assert("When focus is lost, Copy is enabled", copyMenuItem.Enabled);
			Assert("When focus is lost, Paste is enabled", pasteMenuItem.Enabled);
		}

		public void TestMenuItemEnabledStatusMultipleDocsSelected()
		{
			var editMenuItem = grid.FindParentFormEditMenuItem();
			var cutMenuItem = editMenuItem.MenuItems.FindByText(Constants.CutMenuText);
			var copyMenuItem = editMenuItem.MenuItems.FindByText(Constants.CopyMenuText);
			var pasteMenuItem = editMenuItem.MenuItems.FindByText(Constants.PasteMenuText);

			grid.ShowCopyMenuItem = true;
			grid.ShowCutMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.AddCutCopyPasteSelectMenuItems(grid.ContextMenu);
			grid.ListManager.Position = 0;
			grid.Select(0);
			grid.Select(1);
			grid.Focus();

			grid.ReadOnly = false;
			gridCollection[0].SC_IsSystemGenerated = false;
			gridCollection[1].SC_IsSystemGenerated = false;
			grid.UpdateEditMenuItemEnableStatus();
			Assert("Cut is enabled", cutMenuItem.Enabled);
			Assert("Copy is enabled", copyMenuItem.Enabled);
			Assert("Paste is not enabled", !pasteMenuItem.Enabled);

			gridCollection[0].SC_IsSystemGenerated = true;
			grid.UpdateEditMenuItemEnableStatus();
			Assert("Cut is not enabled when selected documents contain system generated ones.", !cutMenuItem.Enabled);
			Assert("Copy is enabled", copyMenuItem.Enabled);
			Assert("Paste is not enabled", !pasteMenuItem.Enabled);

			gridCollection[0].SC_IsSystemGenerated = false;
			grid.ReadOnly = true;
			grid.UpdateEditMenuItemEnableStatus();
			Assert("Cut is not enabled when grid is read only.", !cutMenuItem.Enabled);
			Assert("Copy is enabled", copyMenuItem.Enabled);
			Assert("Paste is not enabled when grid is read only.", !pasteMenuItem.Enabled);

			var text = new ZTextBox();
			grid.FindForm().Controls.Add(text);
			text.Focus();
			Assert("Lost focus", !grid.Focused);
			grid.UpdateEditMenuItemEnableStatus();
			Assert("When focus is lost, Cut is enabled", cutMenuItem.Enabled);
			Assert("When focus is lost, Copy is enabled", copyMenuItem.Enabled);
			Assert("When focus is lost, Paste is enabled", pasteMenuItem.Enabled);
		}

		[DeveloperOnlyTest]
		public void TestCopyFromMenuItem()
		{
			var editMenuItem = grid.FindParentFormEditMenuItem();
			var copyMenuItem = editMenuItem.MenuItems.FindByText(Constants.CopyMenuText);
			var pasteMenuItem = editMenuItem.MenuItems.FindByText(Constants.PasteMenuText);

			grid.ShowCopyMenuItem = true;
			grid.ShowPasteMenuItem = true;
			grid.AddCutCopyPasteSelectMenuItems(grid.ContextMenu);
			grid.ListManager.Position = 0;

			helperClass.ResetCounts();
			AssertEquals("Pre-condition: HelperClass.AddCount is reset to 0", 0, helperClass.AddCount);

			grid.Select(0);
			copyMenuItem.PerformClick();
			AssertNoExceptionThrown("There will be InvalidOperationException thrown if it tries to copy the text of selected row instead of document.", Application.DoEvents);
			grid.LastClickPoint = new Point(0, 0);
			pasteMenuItem.PerformClick();
			Application.DoEvents();
			AssertEquals("Document is copied and pasted.", 1, helperClass.AddCount);
		}

		[DeveloperOnlyTest]
		public void TestCopyLinkFromMenuItem()
		{
			var fileName = "Test FileName";
			gridCollection[0].SC_FileName = fileName;
			gridCollection[0].SC_Desc = "Unit Test";
			grid.ShowCopyLinkMenuItem = true;
			grid.AddCopyLinkMenuItem(grid.ContextMenu);
			grid.ListManager.Position = 0;
			grid.Select(0);
			grid.Grid_CopyDocumentLink(grid, EventArgs.Empty);

			var fullFileName = gridCollection[0].SC_FileNameWithExtension;
			AssertEquals("Test FileName.tif", fullFileName);
			Assert("Document link is copied to clipboard.", SafeClipboard.GetText().Contains(fullFileName + " (" + gridCollection[0].SC_Desc + ")"));

			var url = ShowStorageDocUrlHandler.Instance.Create(documentAtMouse);
			AssertContains("Document link is copied to clipboard.", url, SafeClipboard.GetData(DataFormats.Rtf).ToString());
		}

		[DeveloperOnlyTest]
		public void TestCopyLinkFromMenuItemWithCurrentElementParentMainIsNull()
		{
			gridCollection[0].SC_FileName = "Test FileName";
			gridCollection[0].SC_SM = ZGuid.Empty;
			SafeClipboard.Clear();

			grid.ShowCopyLinkMenuItem = true;
			grid.AddCopyLinkMenuItem(grid.ContextMenu);
			grid.ListManager.Position = 0;
			grid.Select(0);
			grid.Grid_CopyDocumentLink(grid, EventArgs.Empty);

			AssertEquals("Document link was not copied to clipboard when ParentMain is Null.", string.Empty, SafeClipboard.GetText());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSplitDocumentWithDigitalSignedDocument()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var document = StorageFile.NewWithParent_DEBUG(masterFactory);
			document.SC_DataType = Core.Constants.FileFormats.PDF;
			document.SC_ImageData = new ZBlob(File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\DigitalSignature\ExpectedSignedPDFFromXls.PDF")));

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(document);
			grid.Select(0);
			grid.Grid_SplitDocument(grid, EventArgs.Empty);

			AssertEquals("Error Message on MessageBox.", "This document has been digitally signed and cannot be split into multiple documents.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSplitDocumentWithoutDigitalSignedDocument()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var document = StorageDocs.NewWithParent_DEBUG(masterFactory);
			document.SC_DataType = Core.Constants.FileFormats.PDF;
			document.SC_ImageData = new ZBlob(File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\DigitalSignature\ExpectedUnsignedPDFFromXls.PDF")));

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(document);
			grid.Select(0);
			grid.Grid_SplitDocument(grid, EventArgs.Empty);

			AssertEquals("SplitDocumentForm should be shown.", typeof(SplitDocumentForm), ZFormModaliser.LastFormShownForTest.GetType());
		}

		public void TestSplitDocumentWithIncompatibleDocument()
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var moqDocument = new Mock<StorageDocsBase>(masterFactory, new DataTable().NewRow());
			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(moqDocument.Object);
			grid.Select(0);

			foreach (var errorType in new PdfiumError[] { PdfiumError.FileCorrupt, PdfiumError.IncorrectPassword, PdfiumError.UnsupportedEncryption, PdfiumError.LicensingError })
			{
				UnitTestUserNotification.Instance.ClearMessages();
				moqDocument.Setup(doc => doc.EDocFormat).Returns(() => Core.Constants.FileFormats.PDF);
				moqDocument.Setup(doc => doc.GetSC_ImageDataReader()).Returns(() => throw new PdfiumException(errorType));
				grid.Grid_SplitDocument(grid, EventArgs.Empty);
				AssertEquals("Failed to split the document; the file may be corrupted. Adobe Reader may be able to repair the document for splitting. Open the file in Adobe Reader, save a copy to your PC, then attach that copy to eDocs.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSplitDocumentWithCorruptedPdfFile_ShouldNotThrowException()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile("Enterprise.DocumentScanning.GUI.Testing.CORRUPTED_File.PDF");
				AssertSplitDocumentWithCorruptedPdfFile(testFilePath);
			}
		}

		void AssertSplitDocumentWithCorruptedPdfFile(string corruptedFilePath)
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var document = StorageDocs.NewWithParent_DEBUG(masterFactory);
			document.SC_DataType = Core.Constants.FileFormats.PDF;
			document.SC_ImageData = new ZBlob(File.ReadAllBytes(corruptedFilePath));

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(document);
			grid.Select(0);
			grid.Grid_SplitDocument(grid, EventArgs.Empty);

			AssertEquals("Error Message on MessageBox.", "Failed to split the document; the file may be corrupted. Adobe Reader may be able to repair the document for splitting. Open the file in Adobe Reader, save a copy to your PC, then attach that copy to eDocs.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSplitDocument_CurrentElementAtMousePositionIsNull()
		{
			grid.LastClickPoint = Point.Empty;
			grid.MousePositionForTesting = Point.Empty;
			grid.Grid_SplitDocument(grid, EventArgs.Empty);
			AssertEquals("Please select an eDoc to split.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		public void TestFileValidation()
		{
			grid.CurrentRowIndex = 2;
			AssertEquals("Should return a valid bizo", fileInListButNotAtMouse, grid.CurrentElement);

			fileInListButNotAtMouse.SetNewFileName("SANGO.exe");
			Factory.Save();

			grid.Select(2);
			grid.FireDoubleClickedIfApplicable();

			Assert(UnitTestUserNotification.Instance.LastMessage.Contains("The following file(s) [SANGO.exe] have been considered as dangerous and therefore could not be opened."));
		}

		[ExpectNoExceptions]
		public void TestInvalidFileName()
		{
			grid.CurrentRowIndex = 2;
			AssertEquals("Should return a valid bizo", fileInListButNotAtMouse, grid.CurrentElement);

			fileInListButNotAtMouse.SC_FileName = ":Kalos";
			fileInListButNotAtMouse.SC_DataType = "xls";
			Factory.Save();

			grid.Select(2);
			grid.FireDoubleClickedIfApplicable();

			Assert(UnitTestUserNotification.Instance.LastMessage.Contains("The following file name(s) [:Kalos.xls] contain invalid character(s), please edit it to be Windows compatible."));
		}

		public void TestDeliverDocument_AndEditingInSameTimeDoesNotThrowException()
		{
			NUnit.Framework.TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			NUnit.Framework.TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			grid.ListManager.Position = 0;
			grid.Select(0);

			var filename = string.Empty;

			try
			{
				using (documentAtMouse.OpenForEdit())
				{
					var org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());
					documentAtMouse.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
					documentAtMouse.ParentMain.SM_ParentFK = org.PK;
					documentAtMouse.SC_ImageData = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.xls");

					File.SetAttributes(documentAtMouse.TempFileName, FileAttributes.ReadOnly); // trick to stop the delivery from deleting the file 

					filename = documentAtMouse.TempFileName;

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					grid.DocumentsGrid_DeliverDocument(this, EventArgs.Empty);

					File.SetAttributes(documentAtMouse.TempFileName, FileAttributes.Normal);

					using (var wr = File.AppendText(filename))
					{
						wr.Write("SangoTest");
						wr.Flush();
					}

					AssertNoExceptionThrown("System.ArgumentException not thrown", Application.DoEvents);

					Assert("HasChanges should be true on the object", documentAtMouse.HasChanges);
				}
			}
			finally
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}
			}
		}

		public void TestDeliverDocument_ExternalExceptionIsHandled()
		{
			UnitTestUserNotification.Instance.ClearMessages();

			grid.ListManager.Position = 0;
			grid.Select(0);

			var org = MasterFactory.NewWithValidTestData<OrgHeader>();
			documentAtMouse.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			documentAtMouse.ParentMain.SM_ParentFK = org.PK;

			var propertyInfo = typeof(StorageDocsWithS3Support).GetProperty("IsMovingToExternalStorage", BindingFlags.Instance | BindingFlags.Public);

			AssertNotNull("Property IsMovingToExternalStorage still exists", propertyInfo);

			propertyInfo.SetValue(documentAtMouse, true);
			documentAtMouse.SC_ImageData = ZBlob.Empty;
			MasterFactory.Save();

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "S3"))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				AssertNoExceptionThrown(() => grid.DocumentsGrid_DeliverDocument(this, EventArgs.Empty));
				AssertStartsWith("The last message should be the warning message from S3 exception", "Unable to access S3 storage, please contact your system administrator to check the configuration of the eDocs storage", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliverDocument()
		{
			NUnit.Framework.TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			NUnit.Framework.TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			grid.ListManager.Position = 0;
			grid.Select(0);
			grid.Select(1);
			grid.Select(2);

			var imagePath = Enterprise.DocumentScanning.Business.Test.TestUtils.TestRepositoryPath + "small_2pages.tif";
			var documentBlob = DocumentUtilities.GetFileAsBytes(imagePath);

			var org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());
			documentAtMouse.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			documentAtMouse.ParentMain.SM_ParentFK = org.PK;
			documentAtMouse.SC_ImageData = documentBlob;
			documentInListButNotAtMouse.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			documentInListButNotAtMouse.ParentMain.SM_ParentFK = org.PK;
			documentInListButNotAtMouse.SC_ImageData = documentBlob;
			fileInListButNotAtMouse.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			fileInListButNotAtMouse.ParentMain.SM_ParentFK = org.PK;
			fileInListButNotAtMouse.SC_ImageData = documentBlob;

			AssertEquals("Document collection count", 3, gridCollection.Count);
			AssertEquals("ListManager count", 3, grid.ListManager.List.Count);
			AssertEquals("Selected documents count", 3, grid.SelectedElements.Length);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			grid.DocumentsGrid_DeliverDocument(this, EventArgs.Empty);

			AssertEquals("Allow AutoDelivery set to false", false, grid.PartialInstructionsForTest.AllowAutoDelivery);
			AssertEquals("Delivery Options set to AllExceptPreview", AllowedDeliveryOptions.AllExceptPreview, grid.PartialInstructionsForTest.DeliveryOptions);

			AssertEquals("At least one recipient in delivery instructions (should have been setup in test grid class)", 1, grid.PartialInstructionsForTest.Recipients.Count);
			AssertEquals("Contacts have no element available in Email Delivery types", 0, grid.PartialInstructionsForTest.Recipients[0].AttachmentTypes.Count);
			Assert("Contacts have attachment type disabled in Email Delivery types", grid.PartialInstructionsForTest.Recipients[0].AttachmentTypeDisabled);

			var printJobs = MasterFactory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Three print jobs created", 3, printJobs.Length);

			AssertEquals("Print jobs should be processed", true, printJobs[0].DeliveryGroup.SB_IsProcessed);
			AssertEquals("Print jobs should be processed", true, printJobs[1].DeliveryGroup.SB_IsProcessed);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliverStoresPrinter()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";

			Factory.Save();

			NUnit.Framework.TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			NUnit.Framework.TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			grid.ListManager.Position = 0;
			grid.Select(0);

			var imagePath = Enterprise.DocumentScanning.Business.Test.TestUtils.TestRepositoryPath + "small_2pages.tif";
			var documentBlob = DocumentUtilities.GetFileAsBytes(imagePath);

			var org = MasterFactory.LoadTop1<OrgHeader>(new ZQuery());
			documentAtMouse.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			documentAtMouse.ParentMain.SM_ParentFK = org.PK;
			documentAtMouse.SC_ImageData = documentBlob;
			grid.PrinterPkForTesting = printer.PK;

			var staff = new BusinessObjectFactory().NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = Guid.NewGuid().ToString().Replace("-", "");
			staff.Factory.Save();

			var ctx = new TemporaryUserContext();
			ctx.StaffLoginName = staff.GS_LoginName;

			using (ctx.Set())
			{
				var stmMenuItem = Factory.New<StmMenuItem>();
				stmMenuItem.SU_MenuName = "Some Document";
				Factory.Save();

				AssertNull("Precondition", StmDefaultPrinter.LoadDefaultPrinter(Factory, GlbStaff.CurrentUser, stmMenuItem));

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				grid.DocumentsGrid_DeliverDocument(this, EventArgs.Empty);

				var defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, GlbStaff.CurrentUser, stmMenuItem);
				defaultPrinter.SDP_SQ_Printer = printer.PK;
				defaultPrinter.SDP_NumberOfCopies = 5;
				Factory.Save();

				AssertNotNull("Should add default Printer.", defaultPrinter);
				AssertEquals(grid.PrinterPkForTesting, defaultPrinter.SDP_SQ_Printer);
				AssertEquals((byte)5, defaultPrinter.SDP_NumberOfCopies);
			}
		}

		public void TestDeliverDocumentRequiresConfirmation()
		{
			AssemblyDataLookup.AddAssemblyDataForTesting(DeliveryAuthorizingDocumentOwner.DocManagerCode, new DeliveryAuthorizingDocumentOwnerAssemblyData());

			var owner = MasterFactory.New<DeliveryAuthorizingDocumentOwner>();
			owner.DocumentViewEnabled = true;
			owner.DocumentDeliveryDisclaimerRequired = true;

			var storageMain = documentAtMouse.ParentMain;
			storageMain.SM_ParentFK = owner.PK;
			storageMain.SM_Type = DeliveryAuthorizingDocumentOwner.DocManagerCode;

			grid.ReadOnly = false;
			grid.CurrentRowIndex = 0;
			grid.Select(0);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			var pack = grid.CreateDocumentPack();
			AssertEquals("Test Document added to pack when Delivery has been Confirmed", 1, pack.Count);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			pack = grid.CreateDocumentPack();
			AssertEquals("Test Document not added to pack when Delivery has been Denied", 0, pack.Count);
		}

		#region TestCutDocuments

		[RequiresSTA]
		public void TestCutDocuments()
		{
			grid.ListManager.Position = 0;

			var documentAtPosition1Parent = documentAtMouse.ParentMain;
			var documentAtPosition2Parent = documentInListButNotAtMouse.ParentMain;

			AssertEquals("Document collection count", 3, gridCollection.Count);
			AssertEquals("ListManager count", 3, grid.ListManager.List.Count);

			grid.ShowCutMenuItem = true;
			grid.AddCutCopyPasteSelectMenuItems(grid.ContextMenu);
			grid.Grid_CutDocuments(grid, EventArgs.Empty);

			AssertEquals("Document collection count", 2, gridCollection.Count);
			AssertEquals("ListManager count", 2, grid.ListManager.List.Count);

			Assert("Document at mouse (position 1) should be deleted", documentAtMouse.IsDeleted);
			Assert("Parent for the document at mouse should also be deleted", documentAtPosition1Parent.IsDeleted);
			Assert("Document not at mouse should not be deleted", !documentInListButNotAtMouse.IsDeleted);
			Assert("Parent for the document not at mouse should not be deleted", !documentAtPosition2Parent.IsDeleted);
		}

		public void TestCutDocumentWhenCheckPointNotFound()
		{
			var docType = "AAA";
			var checkpoint = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsCutDocumentType" + docType));
			var doc = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc.SC_DocType = docType;

			var parentCheckpoint = Env.Security.FindCheckPoint(Env.Security.CutSpecificEDocTypes.Code);
			parentCheckpoint.IsAllowed = true;

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(doc);
			gridCollection[0].SC_FileName = "a";
			Application.DoEvents();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			grid.ListManager.Position = 0;
			grid.ShowCutMenuItem = true;
			grid.AddCutCopyPasteSelectMenuItems(grid.ContextMenu);

			AssertNull("No checkpoint should be found for doc type not in db", checkpoint);

			grid.Grid_CutDocuments(grid, EventArgs.Empty);
			AssertEquals("One document deleted", true, doc.IsDeleted);
		}

		public void TestCutMultipleDocumentsCompleteFailWithParentIsDenied()
		{
			TestCutMultipleDocumentsCompleteFail(false, @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> DocManager -> eDocs Cut Specific Document Types");
		}

		public void TestCutMultipleDocumentsCompleteFailButParentIsGranted()
		{
			TestCutMultipleDocumentsCompleteFail(true, @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> DocManager -> eDocs Cut Specific Document Types -> BKG
Manage -> DocManager -> eDocs Cut Specific Document Types -> ACV");
		}

		void TestCutMultipleDocumentsCompleteFail(bool isParentGranted, string expectedMessage)
		{
			var testType1 = "BKG";
			var checkpoint1 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsCutDocumentType" + testType1));
			checkpoint1.IsAllowed = false;
			var doc1 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc1.SC_DocType = testType1;

			var testType2 = "ACV";
			var checkpoint2 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsCutDocumentType" + testType2));
			checkpoint2.IsAllowed = false;
			var doc2 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc2.SC_DocType = testType2;

			var parentCheckpoint = Env.Security.FindCheckPoint(Env.Security.CutSpecificEDocTypes.Code);
			parentCheckpoint.IsAllowed = isParentGranted;

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(doc1);
			gridCollection.Add(doc2);
			gridCollection[0].SC_FileName = "a";
			gridCollection[1].SC_FileName = "b";
			Application.DoEvents();

			grid.Select(0);
			grid.Select(1);
			grid.ListManager.Position = 0;
			grid.ShowCutMenuItem = true;
			grid.AddCutCopyPasteSelectMenuItems(grid.ContextMenu);
			grid.Grid_CutDocuments(grid, EventArgs.Empty);
			AssertEquals(expectedMessage, ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
			AssertEquals("Document 1 is not deleted", false, doc1.IsDeleted);
			AssertEquals("Document 2 is not deleted", false, doc2.IsDeleted);
		}

		public void TestCutDocumentsPartialSuccess()
		{
			var testType1 = "BKG";
			var checkpoint1 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsCutDocumentType" + testType1));
			checkpoint1.IsAllowed = false;
			var doc1 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc1.SC_DocType = testType1;

			var testType2 = "ACV";
			var checkpoint2 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsCutDocumentType" + testType2));
			checkpoint2.IsAllowed = true;
			var doc2 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc2.SC_DocType = testType2;

			var testType3 = "BKC";
			var checkpoint3 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsCutDocumentType" + testType3));
			checkpoint3.IsAllowed = false;
			var doc3 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc3.SC_DocType = testType3;

			((UnitTestUserNotification)Globals.Message).AddAnswer(DialogResult.Yes);

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(doc1);
			gridCollection.Add(doc2);
			gridCollection.Add(doc3);
			gridCollection[0].SC_FileName = "a";
			gridCollection[1].SC_FileName = "b";
			gridCollection[2].SC_FileName = "c";
			Application.DoEvents();

			grid.Select(0);
			grid.Select(1);
			grid.Select(2);
			grid.ListManager.Position = 0;
			grid.ShowCutMenuItem = true;
			grid.AddCutCopyPasteSelectMenuItems(grid.ContextMenu);
			grid.Grid_CutDocuments(grid, EventArgs.Empty);

			AssertEquals(
@"You do not have the appropriate security rights to cut the below document(s):

a - Document Type BKG
c - Document Type BKC

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> DocManager -> eDocs Cut Specific Document Types -> BKG
Manage -> DocManager -> eDocs Cut Specific Document Types -> BKC

Are you sure you wish to cut the remaining selected items?", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
			AssertEquals("Document 1 is not deleted", false, doc1.IsDeleted);
			AssertEquals("Document 2 is deleted", true, doc2.IsDeleted);
			AssertEquals("Document 3 is not deleted", false, doc3.IsDeleted);
		}

		[RequiresSTA]
		public void TestCutDocumentAddingDDPEvent()
		{
			AssertCutDocumentAddingDDPEvent(true);
			AssertCutDocumentAddingDDPEvent(false);
		}

		void AssertCutDocumentAddingDDPEvent(bool isDocumentSaved)
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var document = StorageDocs.NewWithParent_DEBUG(masterFactory);
			if (isDocumentSaved)
			{
				masterFactory.Save();
			}

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(document);

			grid.ShowCutMenuItem = true;
			grid.AddCutCopyPasteSelectMenuItems(grid.ContextMenu);
			grid.Select(0);
			grid.CurrentElement.ParentMain.SM_Type = "ORG";

			var orgHeader = (OrgHeader)document.ParentMain.DocumentOwner;

			grid.Grid_CutDocuments(grid, EventArgs.Empty);

			var documentDeleteEvent = orgHeader.GetLogs().MostRecentLogByEventTime(AutoEvents.DocumentDeletedPermanently);
			if (isDocumentSaved)
			{
				AssertNotNull("Events.DocumentDeletedPermanently", documentDeleteEvent);
			}
			else
			{
				AssertNull("Events.DocumentDeletedPermanently", documentDeleteEvent);
			}
		}

		public void TestCutShortcut()
		{
			grid.ListManager.Position = 0;
			grid.Select(0);
			grid.ShowCutMenuItem = true;
			grid.AddCutCopyPasteSelectMenuItems(grid.ContextMenu);

			AssertEquals("Pre-condition: Document collection count is 3", 3, gridCollection.Count);
			AssertEquals("Pre-condition: ListManager count is 3", 3, grid.ListManager.List.Count);

			KeySender.PostKeyDown(grid, grid.Handle, Keys.Control | Keys.X);
			Application.DoEvents();

			AssertEquals("Selected document is cut.", 2, gridCollection.Count);
			AssertEquals("Selected document is cut.", 2, grid.ListManager.List.Count);

			gridCollection[0].SC_IsSystemGenerated = true;
			grid.Select(0);
			KeySender.PostKeyDown(grid, grid.Handle, Keys.Control | Keys.X);
			Application.DoEvents();
			AssertEquals("Selected document is not cut because selected document is system generated.", 2, gridCollection.Count);
			AssertEquals("Selected document is not cut because selected document is system generated.", 2, grid.ListManager.List.Count);

			gridCollection[1].SC_IsSystemGenerated = false;
			grid.Select(0);
			grid.Select(1);
			KeySender.PostKeyDown(grid, grid.Handle, Keys.Control | Keys.X);
			Application.DoEvents();
			AssertEquals("Selected documents are not cut when there is system generated document.", 2, gridCollection.Count);
			AssertEquals("Selected documents are not cut when there is system generated document.", 2, grid.ListManager.List.Count);

			gridCollection[0].SC_IsSystemGenerated = false;
			gridCollection[1].SC_IsSystemGenerated = false;
			grid.Select(0);
			grid.Select(1);
			grid.ReadOnly = true;
			KeySender.PostKeyDown(grid, grid.Handle, Keys.Control | Keys.X);
			Application.DoEvents();
			AssertEquals("Selected documents are not cut when tgrid is readonly,", 2, gridCollection.Count);
			AssertEquals("Selected documents are not cut when tgrid is readonly,", 2, grid.ListManager.List.Count);

			grid.ReadOnly = false;
			KeySender.PostKeyDown(grid, grid.Handle, Keys.Control | Keys.X);
			Application.DoEvents();
			AssertEquals("Selected documents are cut.", 0, gridCollection.Count);
			AssertEquals("Selected documents are cut.", 0, grid.ListManager.List.Count);
		}

		#endregion

		#region Tests for InsertData()

		public void TestInsertFromDataFromDataWithMaximumLimitSizeInMB_FileDrop()
		{
			using (SystemDataRegistry.Instance.eDocsMaximumFilesize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (CreateTempFileForTest(out string tempBigFileName, 1 * 1024 * 1024))
			using (CreateTempFileForTest(out string tempNormalFileName, 10))
			{
				helperClass.AddCount = 0;
				var dataObject = new DataObject(DataFormats.FileDrop, new[] { tempBigFileName, tempNormalFileName });
				grid.InsertFromData(dataObject);

				var expectedMessage = $@"The following files are larger than the maximum file size (1MB) specified in the registry 'System -> DocManager -> eDocs Maximum File Size'
{Path.GetFileName(tempBigFileName)}

";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should add normal file", 1, helperClass.AddCount);
			}
		}

		public void TestInsertFromData_ShowInfoNeedsToBeNotified()
		{
			using (SystemDataRegistry.Instance.eDocsMaximumFilesize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (CreateTempFileForTest(out string tempBigFileName, 1 * 1024 * 1024))
			using (CreateTempFileForTest(out string tempNormalFileName, 10))
			{
				helperClass.AddCount = 0;
				grid.GetZDataObjectFromDataForTest = (d) =>
				{
					var dataObject = new DataObject(DataFormats.FileDrop, new[] { tempBigFileName, tempNormalFileName });
					return ZDataObject.FromDataWithMaximumLimitSizeInMB(dataObject, 1, StorageDocsHelper.GetMaximumLimitSizeNotifications(SystemDataRegistry.Instance.eDocsMaximumFilesize));
				};
				grid.InsertFromData(new DataObject());

				var expectedMessage = $@"The following files are larger than the maximum file size (1MB) specified in the registry 'System -> DocManager -> eDocs Maximum File Size'
{Path.GetFileName(tempBigFileName)}";

				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should add normal file", 1, helperClass.AddCount);
			}
		}

		public void TestGetZDataObjectFromData()
		{
			using (SystemDataRegistry.Instance.eDocsMaximumFilesize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (CreateTempFileForTest(out string tempBigFileName, 1 * 1024 * 1024))
			{
				var dataObject = new DataObject(DataFormats.FileDrop, new[] { tempBigFileName });
				var result = grid.GetZDataObjectFromData_Exposed(dataObject);

				var expectedMessage = $@"The following files are larger than the maximum file size (1MB) specified in the registry 'System -> DocManager -> eDocs Maximum File Size'
{Path.GetFileName(tempBigFileName)}";
				AssertEquals(expectedMessage, result.InfoNeedsToBeNotified);
			}
		}

		IDisposable CreateTempFileForTest(out string fileName, int limitSize)
		{
			var tempFile = TempFile.NewWithExtension("dat");
			fileName = tempFile.Filename;
			using (var stream = new FileStream(fileName, FileMode.Create))
			using (var writer = new StreamWriter(stream))
			{
				while (stream.Position < limitSize)
				{
					writer.WriteLine("Create a file for test");
				}
			}

			return new DisposableAction(tempFile.Dispose);
		}

		public void TestInsertFromData_WhenGetDataReturnsNothing()
		{
			// Arrange
			var dataObjectMock = new Mock<IDataObject>();
			dataObjectMock.Setup(x => x.GetDataPresent(It.IsAny<string>())).Returns(true);
			dataObjectMock.Setup(x => x.GetData(It.IsAny<Type>())).Returns(null);
			grid.GetZDataObjectFromDataForTest = d => throw new NotSupportedException(@"Invalid Path: c:\doomed\doomed.xxx");
			UnitTestUserNotification.Instance.ClearMessages();

			//Assert
			AssertNoExceptionThrown(() => grid.InsertFromData(dataObjectMock.Object));
			AssertEquals("Error Invalid Path: c:\\doomed\\doomed.xxx", UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestInsertFromData_NotSupportedException()
		{
			grid.GetZDataObjectFromDataForTest = (d) => throw new NotSupportedException(@"Invalid Path: c:\doomed\doomed.xxx"); // this is a mock path in the exception
			grid.InsertFromData(new DataObject());
			AssertEquals(@"Error Invalid Path: c:\doomed\doomed.xxx", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestInsertFromDataForNoBizOUnderMouse()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);

			var dataToInsert = new DocManagerDataObject(null, new BusinessObject[] { document });
			grid.InsertFromData(dataToInsert);
			AssertEquals("number of calls to Add on interface", 1, helperClass.AddCount);
			AssertEquals("number of calls to AppendToExisting on interface", 0, helperClass.AppendToExistingCount);

			helperClass.ResetCounts();
			var dataToInsert2 = new DataObject();
			var filearray = new string[] { FivePagesTifPath, SmallGifPath };
			dataToInsert2.SetData(DataFormats.FileDrop, filearray);
			grid.InsertFromData(dataToInsert2);
			AssertEquals("number of calls to Add on interface", 1, helperClass.AddCount);
			AssertEquals("number of calls to AppendToExisting on interface", 0, helperClass.AppendToExistingCount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInsertFromDataCorruptedDocument()
		{
			var dataToInsert = new DataObject();
			var testCorruptedPdfPath = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test Corrupted.pdf";
			var filearray = new string[] { testCorruptedPdfPath };

			var dragdroptarget = new Mock<IDragDropSupport>();
			dragdroptarget.Setup(m => m.Add(filearray)).Throws(new CorruptedDocumentException("Boom", null));
			grid.DragDropTarget = dragdroptarget.Object;

			dataToInsert.SetData(DataFormats.FileDrop, filearray);
			grid.InsertFromData(dataToInsert);

			dragdroptarget.VerifyAll();
			AssertEquals("Error A file could not be opened due to unsupported format or corrupted file.\r\nBoom", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestInsertFromDataForNoBizOUnderMouseForReadonlyGrid()
		{
			grid.ReadOnly = true;

			// From here is same as text from above except for assertions
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			var dataToInsert = new DocManagerDataObject(null, new BusinessObject[] { document });
			grid.InsertFromData(dataToInsert);
			AssertEquals("number of calls to Add on interface - readonly", 0, helperClass.AddCount);
			AssertEquals("number of calls to AppendToExisting on interface - readonly", 0, helperClass.AppendToExistingCount);

			helperClass.ResetCounts();
			var dataToInsert2 = new DataObject();
			var filearray = new string[] { FivePagesTifPath, SmallGifPath };
			dataToInsert2.SetData(DataFormats.FileDrop, filearray);
			grid.InsertFromData(dataToInsert2);
			AssertEquals("number of calls to Add on interface - readonly", 0, helperClass.AddCount);
			AssertEquals("number of calls to AppendToExisting on interface - readonly", 0, helperClass.AppendToExistingCount);
		}

		public void TestInsertFromDataForNonSerializableEDocCollectionObject()
		{
			var dataToInsert = new Mock<IDataObject>();
			dataToInsert.Setup(m => m.GetDataPresent(grid.GetDataFormatType())).Returns(true);
			dataToInsert.Setup(m => m.GetFormats(true)).Returns(new string[] { grid.GetDataFormatType() });
			dataToInsert.Setup(m => m.GetData(grid.GetDataFormatType())).Returns(new MemoryStream());
			AssertNoExceptionThrown(() => grid.InsertFromData(dataToInsert.Object));
		}

		public void TestFileWasUsedByOtherProcessThrowIOException()
		{
			var mockDragDropTarget = new Mock<IDragDropSupport>();
			mockDragDropTarget.Setup(m => m.Add(It.IsAny<string[]>())).Throws(new IOException());
			grid.DragDropTarget = mockDragDropTarget.Object;

			using (CreateTempFileForTest(out string tempNormalFileName, 10))
			{
				ErrorReporter.Clear();
				grid.GetZDataObjectFromDataForTest = (d) =>
				{
					var dataObject = new DataObject(DataFormats.FileDrop, new[] { tempNormalFileName });
					return ZDataObject.FromDataWithMaximumLimitSizeInMB(dataObject, 1, StorageDocsHelper.GetMaximumLimitSizeNotifications(SystemDataRegistry.Instance.eDocsMaximumFilesize));
				};
				var dataToInsert = new DataObject(DataFormats.Bitmap, null);
				AssertNoExceptionThrown(() => grid.InsertFromData(new DataObject()));
				AssertEquals("IOExceptionThrownWhenPastingSnippingToolImageToEDocs", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
			mockDragDropTarget.VerifyAll();
		}
		#endregion

		#region Tests for OnDragOver()
		public void TestOnDragOverBarredForReadonlyGrid()
		{
			var documentToInsert = StorageDocs.NewWithParent_DEBUG(MasterFactory);

			var dataToInsert = new DocManagerDataObject(null, new BusinessObject[] { documentToInsert });

			var args = new DragEventArgs(dataToInsert, 0, 10, 60, DragDropEffects.Move, DragDropEffects.None);

			grid.ReadOnly = true;
			grid.OnDragOver(args);
			AssertEquals("Not allowed to drag drop, readonly", DragDropEffects.None, args.Effect);

			grid.ReadOnly = false;
			grid.OnDragOver(args);
			AssertEquals("allowed to drag drop, not readonly", DragDropEffects.Move, args.Effect);
		}

		public void TestOnDragOverBarredForSystemGeneratedDocument()
		{
			var documentToDrag = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			documentToDrag.SC_IsSystemGenerated = true;

			var dataToInsert = new DocManagerDataObject(null, new BusinessObject[] { documentToDrag });

			var args = new DragEventArgs(dataToInsert, 0, 10, 60, DragDropEffects.Move, DragDropEffects.None);
			grid.OnDragOver(args);
			AssertEquals("Not allowed to drag drop, document being dragged is system generated", DragDropEffects.None, args.Effect);

			documentToDrag.SC_IsSystemGenerated = false;
			dataToInsert = new DocManagerDataObject(null, new BusinessObject[] { documentToDrag });

			args = new DragEventArgs(dataToInsert, 0, 10, 60, DragDropEffects.Move, DragDropEffects.None);
			grid.OnDragOver(args);
			AssertEquals("Now allowed to drag drop, document is no longer system generated", DragDropEffects.Move, args.Effect);
		}

		public void TestOnDragOverForMultipleEffects()
		{
			var testFile = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
			var dataToDrop = new DataObject();
			dataToDrop.SetData(DataFormats.FileDrop.GetType(), new string[] { testFile });

			var args = new DragEventArgs(dataToDrop, 0, 10, 60, DragDropEffects.Move | DragDropEffects.Copy, DragDropEffects.None);

			grid.OnDragOver(args);
			AssertEquals("Drag over effect should be copy if both copy and move are allowed", DragDropEffects.Copy, args.Effect);
		}

		public void TestGetApplicableDragEffectForNewElement()
		{
			var documentToInsert = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			var collection = SerializableEDocCollection.New(new BusinessObject[] { documentToInsert });

			var result = grid.GetApplicableDragEffect(documentAtMouse, collection);
			AssertEquals("Allowed to drag and drop, not on the same element", DragDropEffects.Move, result);
		}

		public void TestGetApplicableDragEffectForExistingDocumentInList()
		{
			var collection = SerializableEDocCollection.New(new BusinessObject[] { documentInListButNotAtMouse });

			var result = grid.GetApplicableDragEffect(documentAtMouse, collection);
			AssertEquals("allowed to drag drop, not the same element", DragDropEffects.Move, result);
		}

		public void TestGetApplicableDragEffectForSystemGeneratedDocument()
		{
			var documentToInsert = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			var collection = SerializableEDocCollection.New(new BusinessObject[] { documentToInsert });

			documentAtMouse.SC_IsSystemGenerated = true;
			var result = grid.GetApplicableDragEffect(documentAtMouse, collection);
			AssertEquals("Not allowed to drag drop on a system generated doc", DragDropEffects.None, result);

			documentAtMouse.SC_IsSystemGenerated = false;
			result = grid.GetApplicableDragEffect(documentAtMouse, collection);
			AssertEquals("Allowed to drag drop now - no longer system generated", DragDropEffects.Move, result);
		}

		public void TestGetApplicableDragEffectForSameElementInList()
		{
			var collection = SerializableEDocCollection.New(new BusinessObject[] { documentAtMouse });

			var result = grid.GetApplicableDragEffect(documentAtMouse, collection);
			AssertEquals("NOT allowed to drag drop, trying to drag the same element", DragDropEffects.None, result);
		}

		public void TestGetApplicableDragEffectForNonTifFiles()
		{
			var documentToInsert = StorageFile.NewWithParent_DEBUG(MasterFactory);
			documentToInsert.SC_DataType = "PDF";
			var collection = SerializableEDocCollection.New(new BusinessObject[] { documentToInsert });

			var result = grid.GetApplicableDragEffect(documentAtMouse, collection);
			AssertEquals("This should not be allowed - Non-TIF can't be dragged onto a TIF file", DragDropEffects.None, result);

			result = grid.GetApplicableDragEffect(null, collection);
			AssertEquals("This is allowed - Non-TIF can be dragged over empty space in the grid to make a new file", DragDropEffects.Move, result);
		}

		#endregion

		#region Tests for OnDragDrop()

		public void TestOnDragDropForReadonlyGrid()
		{
			var documentToInsert = StorageDocs.NewWithParent_DEBUG(MasterFactory);

			var dataToInsert = new DocManagerDataObject(null, new BusinessObject[] { documentToInsert });
			var args = new DragEventArgs(dataToInsert, 0, 10, 70, DragDropEffects.Move, DragDropEffects.None);

			grid.ReadOnly = true;
			grid.OnDragDrop(args);
			AssertEquals("Not allowed to drag drop, readonly - no methods on interface called 1", 0, helperClass.AppendToExistingCount);
			AssertEquals("Not allowed to drag drop, readonly - no methods on interface called 2", 0, helperClass.AddCount);
			AssertEquals("Not allowed to drag drop, readonly - no methods on interface called 3", 0, helperClass.AppendToExistingCount);
			AssertEquals("Not allowed to drag drop, readonly - no methods on interface called 4", 0, helperClass.AddCount);
			Assert("Grid click point has changed", startingClickPoint != grid.LastClickPoint);
		}

#if !WINZOR

		public void TestDragDropHasLeftGrid()
		{
			var documentToInsert = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			var dataToInsert = new DocManagerDataObject(null, new BusinessObject[] { documentToInsert });
			var args = new DragEventArgs(dataToInsert, 0, 10, 70, DragDropEffects.Move | DragDropEffects.Copy, DragDropEffects.None);
			grid.OnDragLeave(args);
			Assert("Expected DragDropHasLeftGrid to be true", grid.DragDropHasLeftGrid);

			grid.OnDragEnter(args);
			Assert("Expected DragDropHasLeftGrid to be false", !grid.DragDropHasLeftGrid);
		}

#endif

		#endregion

		#region Virus Scanning

		public void TestOpenVirusDetectedFile()
		{
			grid.CurrentRowIndex = 2;
			AssertEquals("Should return a valid bizo", fileInListButNotAtMouse, grid.CurrentElement);

			fileInListButNotAtMouse.SC_FileName = "virus";
			fileInListButNotAtMouse.SC_DataType = "xls";
			fileInListButNotAtMouse.LastVirusScanResult = Constants.VirusScanResult.Detected;
			Factory.Save();

			grid.Select(2);

			AssertNoExceptionThrown("There will be VirusDetectedException thrown.", () => { grid.FireDoubleClickedIfApplicable(); });
			AssertEquals("The following file(s) [virus.xls] have been detected with virus and therefore could not be opened.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestViewVirusDetectedFile()
		{
			grid.CurrentRowIndex = 0;
			grid.CurrentElement.SC_FileName = "virus";
			grid.CurrentElement.SC_DataType = "xls";
			grid.CurrentElement.LastVirusScanResult = Constants.VirusScanResult.Detected;

			var count = 0;
			grid.StorageDocViewed += doc =>
			{
				count = 1;
			};

			grid.View(grid.CurrentElement);
			AssertEquals(count, 0);
		}

		public void TestColourDeciding()
		{
			fileInListButNotAtMouse.LastVirusScanResult = Constants.VirusScanResult.Detected;
			Factory.Save();

			AssertEquals("e.Colour", Color.FromArgb(235, 155, 155), GetColour(grid, fileInListButNotAtMouse));
		}

		Color GetColour(DocumentsZGridForTesting grid, StorageFile note)
		{
			var e = new ColourDecidingEventArgs(note);
			grid.Grid_ColourDeciding(grid, e);
			return e.Colour;
		}

		#endregion

		#region Shipamax

		void SetupReviewParsedResultsEvent_ForDocument(string launchUrl)
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var document = StorageDocs.NewWithParent_DEBUG(masterFactory.GetFactory(1));
			document.SC_DataType = Core.Constants.FileFormats.PDF;

			document.SC_DocType = "CIV";
			document.SC_IsDeleted = false;

			var shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			document.ParentMain.SM_ParentFK = shipment.PK;
			document.ParentMain.SM_Type = "SHP";

			var dashUtilsMock = new Mock<IDashUtils>();
			dashUtilsMock.Setup(x => x.GetCorrectionToolUrl(It.IsAny<string>())).Returns(launchUrl);
			ObjectFactory.Substitute(dashUtilsMock.Object);

			masterFactory.Save();

			document.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(document);
			grid.Select(0);

			grid.ShowReviewParsedResultsMenuItem = true;
			grid.SetupContextMenu();
		}

		public void TestReviewParsedResultsEvent_ForDocument()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var launchUrl = "https://myserver/Portals";
				SetupReviewParsedResultsEvent_ForDocument(launchUrl);

				AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

				var reviewParsedResultsMenuItem = FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText);
				Assert("Review Parsed Results is enabled", reviewParsedResultsMenuItem.Enabled);
				reviewParsedResultsMenuItem.PerformClick();
				AssertEquals("Launch URL should be launch url", launchUrl, WebUrlLauncher.LastUrlLaunched);
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}

		public void TestReviewParsedResultsEvent_ForDocument_EmptyURL()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetupReviewParsedResultsEvent_ForDocument(string.Empty);

				AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

				var reviewParsedResultsMenuItem = FindMenuItemByName(grid.ContextMenu, Constants.ReviewParsedResultsMenuText);
				Assert("Review Parsed Results is enabled", reviewParsedResultsMenuItem.Enabled);
				reviewParsedResultsMenuItem.PerformClick();
				AssertEquals("Unable to launch the Correction Tool to review the parsed results. Please try again after sometime. If the problem persists, please raise an incident.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		public void TestView_ForDocument()
		{
			grid.ListManager.Position = 0;
			var selectedDocument = (StorageDocs)grid.SelectedElements[0];

			grid.ShowViewMenuItem = true;
			grid.SetupContextMenu();
			grid.StorageDocViewed += OnGrid_StorageDocViewed;

			AssertEquals("Document not marked as 'read' initially", false, selectedDocument.IsReadByUserInThisSession);
			var viewMenuItem = FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText);
			viewMenuItem.PerformClick();
			AssertEquals("StorageDocViewed event should be fired", true, onGridStorageDocViewedCalled);
			AssertEquals("Document should be marked as 'read'", true, selectedDocument.IsReadByUserInThisSession);
		}

		public void TestView_ForDocumentOtherEditor()
		{
			SystemDataRegistry.Instance.UseDefaultWindowsImageViewer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			grid.ListManager.Position = 0;
			var selectedDocument = (StorageDocs)grid.SelectedElements[0];

			try
			{
				grid.ShowViewMenuItem = true;
				grid.SetupContextMenu();
				grid.StorageDocViewed += OnGrid_StorageDocViewed;

				AssertEquals("Document not marked as 'read' initially", false, selectedDocument.IsReadByUserInThisSession);
				var viewMenuItem = FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText);
				viewMenuItem.PerformClick();
				AssertEquals("StorageDocViewed event should be fired", true, onGridStorageDocViewedCalled);
				AssertEquals("Document should be marked as 'read'", true, selectedDocument.IsReadByUserInThisSession);
			}
			finally
			{
				try
				{
					File.SetAttributes(selectedDocument.TempFileName, FileAttributes.Normal);
				}
				finally
				{
					selectedDocument.Dispose();
				}
			}
		}

		public void TestView_WhenIOExceptionThrown_CatchesAndDisplays()
		{
			gridCollection.RemoveAll();
			var mock = gridCollection.Factory.NewMoq<StorageDocs>();
			mock.Object.SC_ImageData = new byte[] { 1, 2, 3, 4, 5 };
			mock.Protected().Setup("GetTempFileNameWithPath").Throws(new PathTooLongException());
			gridCollection.Add(mock.Object);
			SystemDataRegistry.Instance.UseDefaultWindowsImageViewer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			grid.ListManager.Position = 0;
			var selectedDocument = (StorageDocs)grid.SelectedElements[0];

			grid.ShowViewMenuItem = true;
			grid.SetupContextMenu();

			var viewMenuItem = FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText);
			viewMenuItem.PerformClick();

			//now we expect the dialog to have been shown, but no error report to have been made
			AssertContains("Textbox appeared", "The specified path, file name, or both are too long", UnitTestUserNotification.Instance.LastMessage.Text);
			mock.VerifyAll();
		}

		bool onGridStorageDocViewedCalled;
		void OnGrid_StorageDocViewed(StorageDocsBase doc)
		{
			onGridStorageDocViewedCalled = true;
		}

		#region TestViewInSeparateThread

		public void TestViewInSeparateThread()
		{
			var newStorageFile = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(SlowStorageFile));
			newStorageFile.SC_ImageData = new byte[] { 1, 2, 3, 4, 5 };
			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(newStorageFile);
			Application.DoEvents();

			grid.ListManager.Position = grid.ListManager.Count - 1;
			var selectedDocument = (SlowStorageFile)grid.SelectedElements[0];
			AssertEquals("Precondition", false, selectedDocument.Launched);

			selectedDocument.ThreadExceptionHappened += DocumentsZGridTest_threadExceptionHappened;

			grid.ShowViewMenuItem = true;
			grid.SetupContextMenu();
			Assert(!grid.ReadOnly);
			FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText).PerformClick();

			AssertEquals("Should not lock thread", false, selectedDocument.Launched);
			AssertEquals("Should not be readonly", false, selectedDocument.IsReadonly);
			selectedDocument.StartEvent.Set();
			selectedDocument.EndEvent.WaitOne();
			selectedDocument.ThreadExceptionHappened -= DocumentsZGridTest_threadExceptionHappened;
			if (selectedDocument.ThreadException != null)
			{
				Fail("Failed due to " + selectedDocument.ThreadException.Message + "\r\n" + selectedDocument.ThreadException.StackTrace);
			}

			AssertEquals("Should open document in background", true, selectedDocument.Launched);
			AssertNotNull("LaunchProcess should have a SynchronizationContext", selectedDocument.LaunchContext);
		}

		public void TestViewFileInReadonlyModeWhenDocumentGridIsReadOnly()
		{
			var newStorageFile = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(SlowStorageFile));
			newStorageFile.SC_ImageData = new byte[] { 1, 2, 3, 4, 5 };
			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(newStorageFile);
			Application.DoEvents();

			grid.ListManager.Position = grid.ListManager.Count - 1;
			var selectedDocument = (SlowStorageFile)grid.SelectedElements[0];
			grid.ReadOnly = true;

			selectedDocument.ThreadExceptionHappened += DocumentsZGridTest_threadExceptionHappened;

			grid.ShowViewMenuItem = true;
			grid.SetupContextMenu();
			FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText).PerformClick();

			selectedDocument.StartEvent.Set();
			selectedDocument.EndEvent.WaitOne();
			selectedDocument.ThreadExceptionHappened -= DocumentsZGridTest_threadExceptionHappened;
			if (selectedDocument.ThreadException != null)
			{
				Fail("Failed due to " + selectedDocument.ThreadException.Message + "\r\n" + selectedDocument.ThreadException.StackTrace);
			}

			AssertEquals("Should be readonly", true, selectedDocument.IsReadonly);
		}

		public void TestViewInSeparateThreadWhenSynchronizationContextCurrentIsNull()
		{
			var newStorageFile = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(SlowStorageFile));
			newStorageFile.SC_ImageData = new byte[] { 1, 2, 3, 4, 5 };
			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(newStorageFile);
			Application.DoEvents();

			grid.ListManager.Position = grid.ListManager.Count - 1;
			var selectedDocument = (SlowStorageFile)grid.SelectedElements[0];
			AssertEquals("Precondition", false, selectedDocument.Launched);

			grid.ShowViewMenuItem = true;
			grid.SetupContextMenu();
			var syncContext = SynchronizationContext.Current;
			try
			{
				SynchronizationContext.SetSynchronizationContext(null);
				AssertExceptionThrown(typeof(InvalidOperationException), () => { DocumentsZGridTest.FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText).PerformClick(); });
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(syncContext);
			}
		}

		public void TestViewInSeparateThread_Dispose()
		{
			var newStorageFile1 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(SlowStorageFile));
			newStorageFile1.SC_ImageData = new byte[] { 1, 2, 3, 4, 5 };
			var newStorageFile2 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(SlowStorageFile));
			newStorageFile2.SC_ImageData = new byte[] { 1, 2, 3, 4, 5 };
			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(newStorageFile1);
			gridCollection.Add(newStorageFile2);
			Application.DoEvents();

			grid.ListManager.Position = 1;
			var doc2 = (SlowStorageFile)grid.SelectedElements[0];
			grid.ListManager.Position = 0;
			var doc1 = (SlowStorageFile)grid.SelectedElements[0];

			grid.ShowViewMenuItem = true;
			grid.SetupContextMenu();
			var viewMenu = FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText);
			viewMenu.PerformClick();

			// launch doc1 and wait for it to exit
			doc1.StartEvent.Set();
			doc1.EndEvent.WaitOne();
			AssertEquals("Should open document in background", true, doc1.Launched);
			Application.DoEvents();

			// launch doc2, but don't start it
			grid.LastClickPoint = new Point(startingClickPoint.X, startingClickPoint.Y + grid.GetCurrentCellBounds().Height);
			grid.ListManager.Position = 1;
			viewMenu.PerformClick();
			AssertEquals("Should not lock thread", false, doc2.Launched);

			while (doc1.OpenForEditDisposable == null)
			{
				Thread.Sleep(50);
			}
			AssertEquals("doc1 not disposed", false, doc1.OpenForEditDisposable.IsDisposed);

			// start doc2 when doc1 is being disposed
			doc1.OpenForEditDisposable.Disposed += new EventHandler((object sender, EventArgs args) =>
			{
				doc2.StartEvent.Set();
				doc2.EndEvent.WaitOne();
				while (doc2.OpenForEditDisposable == null)
				{
					Thread.Sleep(50);
				}
				// wait for the doc2 view thread to do it's thing with the disposable
				Thread.Sleep(1000);
			});

			grid.Dispose();
			Application.DoEvents();

			AssertEquals("doc1 is disposed", true, doc1.OpenForEditDisposable.IsDisposed);
			AssertEquals("doc2 is disposed", true, doc2.OpenForEditDisposable.IsDisposed);
		}

		public void TestDoubleClickToView()
		{
			AssemblyDataLookup.AddAssemblyDataForTesting(DeliveryAuthorizingDocumentOwner.DocManagerCode, new DeliveryAuthorizingDocumentOwnerAssemblyData());

			var owner = MasterFactory.New<DeliveryAuthorizingDocumentOwner>();
			owner.DocumentViewEnabled = true;
			owner.DocumentDeliveryDisclaimerRequired = false;

			var storageMain = documentAtMouse.ParentMain;
			storageMain.SM_ParentFK = owner.PK;
			storageMain.SM_Type = DeliveryAuthorizingDocumentOwner.DocManagerCode;

			bool docViewedCalled = false;
			grid.ReadOnly = false;
			grid.CurrentRowIndex = 0;
			grid.StorageDocViewed += (StorageDocsBase doc) => docViewedCalled = true;

			grid.FireDoubleClickedIfApplicable();
			AssertEquals("View shown when Menu is Enabled", true, docViewedCalled);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			docViewedCalled = false;
			owner.DocumentViewEnabled = false;
			grid.FireDoubleClickedIfApplicable();
			AssertEquals("Viewing is Not Enabled", false, docViewedCalled);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			owner.DocumentViewEnabled = true;
			owner.DocumentDeliveryDisclaimerRequired = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			grid.FireDoubleClickedIfApplicable();
			Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("To be able to print export documents"));
			AssertEquals("Delivery was not Confirmed", false, docViewedCalled);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			grid.FireDoubleClickedIfApplicable();
			Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("To be able to print export documents"));
			AssertEquals("Delivery was Confirmed", true, docViewedCalled);
		}

		public void TestCheckBeforeView()
		{
			AssemblyDataLookup.AddAssemblyDataForTesting(DeliveryAuthorizingDocumentOwner.DocManagerCode, new DeliveryAuthorizingDocumentOwnerAssemblyData());

			var owner = MasterFactory.New<DeliveryAuthorizingDocumentOwner>();
			owner.DocumentViewEnabled = true;
			owner.DocumentDeliveryDisclaimerRequired = true;

			var storageMain = documentAtMouse.ParentMain;
			storageMain.SM_ParentFK = owner.PK;
			storageMain.SM_Type = DeliveryAuthorizingDocumentOwner.DocManagerCode;

			grid.ShowViewMenuItem = true;
			grid.SetupContextMenu();
			grid.StorageDocViewed += OnGrid_StorageDocViewed;
			var viewMenuItem = FindMenuItemByName(grid.ContextMenu, Constants.ViewMenuText);
			viewMenuItem.PerformClick();

			Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("To be able to print export documents"));
			AssertEquals("StorageDocViewed event should be fired", true, onGridStorageDocViewedCalled);
		}

		class DisposableTracker : Component
		{
			public DisposableTracker(IDisposable track)
			{
				this.track = track;
			}

			IDisposable track;
			public bool IsDisposed => track == null;

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					track.Dispose();
					track = null;
				}
				base.Dispose(disposing);
			}
		}

		class SlowStorageFile : StorageFile
		{
			public delegate void ThreadExceptionEventHandler(object sender, Exception ex);

			public event ThreadExceptionEventHandler ThreadExceptionHappened;

			public Exception ThreadException { get; set; }

			public SlowStorageFile(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public override void ViewFileExceptionExpose(Exception ex)
			{
				ThreadExceptionHappened?.Invoke(this, ex);
			}

			protected override void LaunchProcess()
			{
				StartEvent.WaitOne();
				LaunchContext = SynchronizationContext.Current;
				Launched = true;

				var fileInfo = new FileInfo(TempFileName);
				IsReadonly = fileInfo.IsReadOnly;

				TempFile.Delete(TempFileName);
				EndEvent.Set();
			}

			public override IDisposable OpenForEdit()
			{
				OpenForEditDisposable = new DisposableTracker(base.OpenForEdit());
				return OpenForEditDisposable;
			}

			public AutoResetEvent StartEvent = new AutoResetEvent(false);
			public AutoResetEvent EndEvent = new AutoResetEvent(false);
			public bool Launched { get; private set; }
			public bool IsReadonly { get; private set; }
			public SynchronizationContext LaunchContext { get; private set; }
			public DisposableTracker OpenForEditDisposable;
		}

		#endregion

		public void TestPopulateSaveFileDialogDefaults_StorageDocs()
		{
			var document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			document.SC_FileName = "a filename";
			document.SC_Desc = "description";

			using (var dialog = new ZSaveFileDialog())
			{
				AssertEquals("Precondition: file dialog filename is empty", string.Empty, dialog.UnmappedFileName);

				grid.PopulateSaveFileDialogDefaults(dialog, document);
				AssertEquals("Dialog is set to the description of the BizO", document.SC_FileName.Trim() + '.' + document.SC_DataType.ToLower(), dialog.UnmappedFileName);
			}

			var documentWith2Dots = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			documentWith2Dots.SC_FileName = "a filename.tar.tif";
			documentWith2Dots.SC_Desc = "description2Dots";

			using (var dialog = new ZSaveFileDialog())
			{
				AssertEquals("Precondition: file dialog filename is empty", string.Empty, dialog.UnmappedFileName);

				grid.PopulateSaveFileDialogDefaults(dialog, documentWith2Dots);
				AssertEquals("Dialog is set to the description of the BizO", documentWith2Dots.SC_FileName.Trim() + '.' + documentWith2Dots.SC_DataType.ToLower(), dialog.UnmappedFileName);
			}
		}

		public void TestPopulateSaveFileDialogDefaults_StorageFile()
		{
			var document = StorageFile.NewWithParent_DEBUG(MasterFactory);
			document.SC_FileName = "a filename";

			using (var dialog = new ZSaveFileDialog())
			{
				AssertEquals("Precondition: file dialog filename is empty", string.Empty, dialog.UnmappedFileName);

				grid.PopulateSaveFileDialogDefaults(dialog, document);
				AssertEquals("Dialog is set to the file name with extension of the BizO", document.SC_FileNameWithExtension, dialog.UnmappedFileName);
			}

			var documentWith2Dots = StorageFile.NewWithParent_DEBUG(MasterFactory);
			documentWith2Dots.SC_FileName = "a filename.tar.gz";

			using (var dialog = new ZSaveFileDialog())
			{
				AssertEquals("Precondition: file dialog filename is empty", string.Empty, dialog.UnmappedFileName);

				grid.PopulateSaveFileDialogDefaults(dialog, documentWith2Dots);
				AssertEquals("Dialog is set to the file name with extension of the BizO", documentWith2Dots.SC_FileNameWithExtension, dialog.UnmappedFileName);
			}
		}

		public void TestSetDeliveryMethodOnInstructions()
		{
			var pack = new DocumentPack();
			pack.Add(MasterFactory.New<StorageDocs>());

			var contact = new DocDeliveryContact(Factory);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

			var instructions = new DeliveryInstructions(pack);
			instructions.Recipients.Add(contact);
			grid.SetDeliveryMethodOnInstructions(instructions);
			AssertEquals("DeliveryMethod", Core.Constants.ContactNotifyModes.Print, contact.DeliveryMethod);

			pack.RemoveAll();
			pack.Add(MasterFactory.New<StorageFile>());
			instructions = new DeliveryInstructions(pack);
			instructions.Recipients.Add(contact);
			grid.SetDeliveryMethodOnInstructions(instructions);
			AssertEquals("DeliveryMethod", Core.Constants.ContactNotifyModes.Email, contact.DeliveryMethod);
		}

		public void TestPruneCannotDeleteDocuments()
		{
			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile)));
			gridCollection.Add(((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFileWithCanDelete)));
			gridCollection.Add(((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFileWithCanDelete)));
			gridCollection[0].SC_FileName = "a";
			gridCollection[1].SC_FileName = "b";
			gridCollection[2].SC_FileName = "c";
			Application.DoEvents();

			((UnitTestUserNotification)Globals.Message).AddAnswer(DialogResult.Yes);

			grid.DeleteDocuments(new BusinessObject[] { gridCollection[0], gridCollection[1], gridCollection[2] });

			AssertEquals("The following documents could not be deleted:\r\nb: If you delete this document, the Earth will collapse into a black hole. So don't do that.\r\nc: If you delete this document, the Earth will collapse into a black hole. So don't do that.\r\n\r\nThe remaining 1 document(s) will be deleted.\r\n", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
			AssertEquals("Only one document deleted", gridCollection[0], helperClass.DeleteDocumentsQuietlyValue.First());
			AssertEquals("Only one document deleted", 1, helperClass.DeleteDocumentsQuietlyValue.Count());
		}

		public void TestDeleteDocumentPermanentlyWhenCheckPointNotFound()
		{
			var docType = "AAA";
			var checkpoint = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete" + docType));
			var doc = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc.SC_DocType = docType;

			var parentCheckpoint = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete"));
			parentCheckpoint.IsAllowed = true;

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(doc);
			gridCollection[0].SC_FileName = "a";
			Application.DoEvents();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			AssertNull("No checkpoint should be found for doc type not in db", checkpoint);
			grid.DeleteDocumentsPermanently(new BusinessObject[] { gridCollection[0] });
			AssertEquals("One document deleted", 1, helperClass.DeleteDocumentsPermanentlyValue.Count());
		}

		public void TestDeleteDocumentPermanentlyWithNonDefaultableDialog()
		{
			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile)));
			gridCollection[0].SC_FileName = "a";
			Application.DoEvents();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			grid.DeleteDocumentsPermanently(new BusinessObject[] { gridCollection[0] });

			AssertEquals("Dialog pop up should be non-defaultable.", expected: false, UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
		}

		public void TestDeleteMultipleDocumentsPermanentlyCompleteFail()
		{
			var testType1 = "BKG";
			var checkpoint1 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete" + testType1));
			checkpoint1.IsAllowed = false;
			var doc1 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc1.SC_DocType = testType1;

			var testType2 = "ACV";
			var checkpoint2 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete" + testType2));
			checkpoint2.IsAllowed = false;
			var doc2 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc2.SC_DocType = testType2;

			var parentCheckpoint = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete"));
			parentCheckpoint.IsAllowed = false;

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(doc1);
			gridCollection.Add(doc2);
			gridCollection[0].SC_FileName = "a";
			gridCollection[1].SC_FileName = "b";
			Application.DoEvents();

			grid.DeleteDocumentsPermanently(new BusinessObject[] { gridCollection[0], gridCollection[1] });
			AssertEquals(
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> DocManager -> eDocs Permanent Delete", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
			AssertNull("No documents deleted", helperClass.DeleteDocumentsPermanentlyValue);
		}

		public void TestDeleteMultipleDocumentsPermanentlyCompleteFailButParentIsGranted()
		{
			var testType1 = "BKG";
			var checkpoint1 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete" + testType1));
			checkpoint1.IsAllowed = false;
			var doc1 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc1.SC_DocType = testType1;

			var testType2 = "ACV";
			var checkpoint2 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete" + testType2));
			checkpoint2.IsAllowed = false;
			var doc2 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc2.SC_DocType = testType2;

			var parentCheckpoint = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete"));
			parentCheckpoint.IsAllowed = true;

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(doc1);
			gridCollection.Add(doc2);
			gridCollection[0].SC_FileName = "a";
			gridCollection[1].SC_FileName = "b";
			Application.DoEvents();

			grid.DeleteDocumentsPermanently(new BusinessObject[] { gridCollection[0], gridCollection[1] });
			AssertEquals(
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> DocManager -> eDocs Permanent Delete -> BKG
Manage -> DocManager -> eDocs Permanent Delete -> ACV", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
			AssertNull("No documents deleted", helperClass.DeleteDocumentsPermanentlyValue);
		}

		public void TestDeleteDocumentsPermanentlyPartialSuccess()
		{
			var testType1 = "BKG";
			var checkpoint1 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete" + testType1));
			checkpoint1.IsAllowed = false;
			var doc1 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc1.SC_DocType = testType1;

			var testType2 = "ACV";
			var checkpoint2 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete" + testType2));
			checkpoint2.IsAllowed = true;
			var doc2 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc2.SC_DocType = testType2;

			var testType3 = "BKC";
			var checkpoint3 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete" + testType3));
			checkpoint3.IsAllowed = false;
			var doc3 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc3.SC_DocType = testType3;

			((UnitTestUserNotification)Globals.Message).AddAnswer(DialogResult.Yes);

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(doc1);
			gridCollection.Add(doc2);
			gridCollection.Add(doc3);
			gridCollection[0].SC_FileName = "a";
			gridCollection[1].SC_FileName = "b";
			gridCollection[2].SC_FileName = "c";
			Application.DoEvents();

			grid.DeleteDocumentsPermanently(new BusinessObject[] { gridCollection[0], gridCollection[1], gridCollection[2] });

			AssertEquals(
@"You do not have the appropriate security rights to permanently delete the below document(s):

a - Document Type BKG
c - Document Type BKC

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> DocManager -> eDocs Permanent Delete -> BKG
Manage -> DocManager -> eDocs Permanent Delete -> BKC

Are you sure you wish to permanently delete the remaining selected items? This cannot be undone.", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
			AssertEquals("Only one document deleted", gridCollection[1], helperClass.DeleteDocumentsPermanentlyValue.First());
			AssertEquals("One document deleted", 1, helperClass.DeleteDocumentsPermanentlyValue.Count());
		}

		public void TestDeleteDocumentsPermanentlyWhenAllPermissionsAreGranted()
		{
			var testType1 = "BKG";
			var checkpoint1 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete" + testType1));
			checkpoint1.IsAllowed = true;
			var doc1 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc1.SC_DocType = testType1;

			var testType2 = "ACV";
			var checkpoint2 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete" + testType2));
			checkpoint2.IsAllowed = true;
			var doc2 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc2.SC_DocType = testType2;

			var testType3 = "BKC";
			var checkpoint3 = Env.Security.FindCheckPoint(new CheckpointLookupKey("eDocsPermanentDelete" + testType3));
			checkpoint3.IsAllowed = true;
			var doc3 = ((NumberedBusinessObjectFactory)documentAtMouse.Factory).NewWithParent(typeof(StorageFile));
			doc3.SC_DocType = testType3;

			((UnitTestUserNotification)Globals.Message).AddAnswer(DialogResult.Yes);

			gridCollection.RemoveAndDeleteAll();
			gridCollection.Add(doc1);
			gridCollection.Add(doc2);
			gridCollection.Add(doc3);
			gridCollection[0].SC_FileName = "a";
			gridCollection[1].SC_FileName = "b";
			gridCollection[2].SC_FileName = "c";
			Application.DoEvents();

			grid.DeleteDocumentsPermanently(new BusinessObject[] { gridCollection[0], gridCollection[1], gridCollection[2] });

			AssertEquals("Are you sure you wish to permanently delete the selected items? This cannot be undone. There are 3 selected item(s).", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);

			AssertEquals("All documents deleted", 3, helperClass.DeleteDocumentsPermanentlyValue.Count());
		}

		public void TestParseMenuItem()
		{
			using var civDisposible = DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			const int databaseNumber = 1;
			var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var factory = masterFactory.GetFactory(databaseNumber);
			var doc = factory.NewWithParent(typeof(StorageFile));
			doc.ParentMain.SM_DB = databaseNumber;
			doc.ParentMain.SM_ParentFK = ZGuid.NewZGuid();
			doc.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			doc.SC_DocType = "CIV";
			doc.SC_DataType = "PDF";

			masterFactory.Save();

			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				grid.Parse(doc);
			});

			var oldShipamaxMessage = doc.ActiveShipamaxMessage;
			doc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Cancelled;

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			grid.Parse(doc);
			AssertEquals("Manually attempting to parse a document gives a dialog", "This will trigger the parsing of the document as a Comercial Invoice. Click 'OK' to continue.", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
			AssertEquals("Doc HasChanges", true, doc.HasChanges);

			masterFactory.Save();
			AssertEquals("There should be only 1 active message", 1, masterFactory.GetDatabaseCount(typeof(EDIMessage), new ZQuery(EDIMessageSchema.EM_LinkUniqueID, doc.PK).AddToFilter(EDIMessageSchema.EM_IsActive, true)));
			AssertEquals("The old shipamax message should be deactivated after re-parsing.", false, oldShipamaxMessage.EM_IsActive);
			AssertEquals("The new shipamax message should be active after reparsing", true, doc.ActiveShipamaxMessage.EM_IsActive);
			AssertEquals("EDIMessage should be set to Queue status", "QUE", doc.ActiveShipamaxMessage.EM_Status);

			doc.ActiveShipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			doc.HasChanges = false;
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			grid.Parse(doc);
			AssertEquals("Manually attempting to re-parse a document gives a different dialog", "The document will be re-parsed as a Comercial Invoice. Re-parsing may take a few minutes, and all previously parsed/edited data will be lost. Click 'OK' to re-parse the document.", ((UnitTestUserNotification)Globals.Message).LastMessage.Text);
			AssertEquals("Doc HasChanges", true, doc.HasChanges);
		}

		public void TestOpenMultipleEdocsShouldNotShowWaitCursor()
		{
			try
			{
				gridCollection.RemoveAndDeleteAll();
				using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "DB"))
				{
					var testBytes = new byte[10000];
					foreach (var i in Enumerable.Range(0, testBytes.Length))
					{
						testBytes[i] = 0x1;
					}
					for (var i = 0; i < 50; i++)
					{
						var document = StorageFile.NewWithParent_DEBUG(MasterFactory);
						document.SC_FileName = "Test" + i;
						document.SC_ImageData = new ZBlob(testBytes);
						document.SC_DataType = "TXT";
						gridCollection.Add(document);
					}

					grid.SelectAllElements();
					grid.DoubleClicked();
					Application.DoEvents();

					var form = grid.FindForm();
					AssertEquals("Should be not in wait cursor", false, form.UseWaitCursor);
				}
			}
			finally
			{
				foreach (StorageFile document in gridCollection)
				{
					var tempFileName = document.TempFileName;
					if (File.Exists(tempFileName))
					{
						File.Delete(tempFileName);
					}
				}
			}
		}

		class StorageFileWithCanDelete : StorageFile
		{
			public StorageFileWithCanDelete(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public override bool CanDelete => false;

			public override MultilingualString ReasonForNotAbleToDelete => (NoResString)"If you delete this document, the Earth will collapse into a black hole. So don't do that.";
		}

		#region SetUp/TearDown Implementation

		protected override void SetUp()
		{
			base.SetUp();
			grid = new DocumentsZGridForTesting();
			form = new ZForm();
			RunGridSetup(form, grid);
		}

		void RunGridSetup(ZForm form, DocumentsZGrid grid)
		{
			helperClass = new HelperTestClass();

			form.Controls.Add(grid);
			grid.Dock = DockStyle.Fill;
			form.Location = new Point(0, 0);
			form.StartPosition = FormStartPosition.Manual;

			grid.DragDropTarget = helperClass;
			grid.DocumentManipulationTarget = helperClass;

			documentAtMouse = StorageDocs.NewWithParentWithoutFK_DEBUG(MasterFactory);
			documentInListButNotAtMouse = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			documentAtMouse.SC_ImageData = new byte[] { 1, 2, 3, 4, 5 };
			fileInListButNotAtMouse = StorageFile.NewWithParent_DEBUG(MasterFactory);

			gridCollection = new CollectionForTesting(MasterFactory);
			gridCollection.Add(documentAtMouse);
			gridCollection.Add(documentInListButNotAtMouse);
			gridCollection.Add(fileInListButNotAtMouse);

			var textBoxColumnInfo1 = new ZTextBoxColumnStyleInfo();
			var textBoxColumnInfo2 = new ZTextBoxColumnStyleInfo();
			textBoxColumnInfo1.ColumnName = "SC_DescriptionForWeb";
			textBoxColumnInfo2.ColumnName = "SC_DocType";
			grid.ColumnStyles.Add(textBoxColumnInfo1);
			grid.ColumnStyles.Add(textBoxColumnInfo2);

			form.Show();
			grid.SetDataBinding(gridCollection, "", gridCollection.GetType().Name);

			grid.ListManager.Position = 0;
			AssertEquals("Precondition: First element in grid is the DocumentAtMouse", ((StorageDocs)grid.ListManager.GetCurrent()).PK, documentAtMouse.PK);
			grid.ListManager.Position = 1;
			AssertEquals("Precondition: Second element in grid is the DocumentInListButNotAtMouse", ((StorageDocs)grid.ListManager.GetCurrent()).PK, documentInListButNotAtMouse.PK);
			grid.ListManager.Position = 2;
			AssertEquals("Precondition: third element in grid is the FileInListButNotAtMouse", ((StorageFile)grid.ListManager.GetCurrent()).PK, fileInListButNotAtMouse.PK);

			startingClickPoint = new Point(10, 30); // (10,30) in control coords or (10,70) in screen coords
			grid.LastClickPoint = startingClickPoint;
		}

		class CollectionForTesting : StorageDocsCollectionBase
		{
			public CollectionForTesting(DocumentFactory factory)
				: base(factory)
			{
			}

			protected override bool AllowSort => false;
		}

		protected override void TearDown()
		{
			form.Dispose();
			base.TearDown();
			DeleteTempFiles();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}
		protected override void MasterSetUp()
		{
			base.MasterSetUp();

			if (!dbHelper.DatabaseExists(1))
			{
				dbHelper.CreateDatabase(1);
			}
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();

			if (dbHelper.DatabaseExists(1))
			{
				var dbName = dbHelper.GetDatabaseName(1);
				dbHelper.DropDatabase(dbName);
			}
		}

		readonly DocManagerDBHelperTestClass dbHelper = new();

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(StorageDocsBaseTest).Assembly));

		string SmallGifPath
		{
			get
			{
				if (string.IsNullOrEmpty(smallGifPath))
				{
					smallGifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif");
				}
				return smallGifPath;
			}
		}
		string smallGifPath;

		string FivePagesTifPath
		{
			get
			{
				if (string.IsNullOrEmpty(fivePagesTifPath))
				{
					fivePagesTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.5pages.tif");
				}
				return fivePagesTifPath;
			}
		}
		string fivePagesTifPath;

		static MenuItem FindMenuItemByName(ContextMenu menu, string text)
		{
			foreach (MenuItem item in menu.MenuItems)
			{
				if (item.Text == text)
				{
					return item;
				}
			}
			return null;
		}

		DocumentsZGridForTesting grid;
		HelperTestClass helperClass;
		ZForm form;
		Point startingClickPoint;
		CollectionForTesting gridCollection;
		StorageDocs documentAtMouse;
		StorageDocs documentInListButNotAtMouse;
		StorageFile fileInListButNotAtMouse;

		#endregion
	}
}
