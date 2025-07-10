using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	[TestedType(typeof(ArchiveEDocsForm))]
	sealed class ArchiveeDocsFormTest : ZFormBasherTest
	{
		protected override bool AllowSaveOnFormForTestHasChanges
		{
			get { return false; }
		}

		protected override Form GetFormToBashCore()
		{
			return new ArchiveEDocsForm(new ArchiveEDocsManager(new DocumentFactoryProvider().GetFactory(Factory)));
		}

		public void TestRemoveActionIsRemoveOnly()
		{
			using (ArchiveEDocsForm form = new ArchiveEDocsForm(new ArchiveEDocsManager(new DocumentFactoryProvider().GetFactory(Factory))))
			{
				AssertEquals("Remove action must be Remove Only", RemoveAction.Remove, form.ListToArchiveGrid.RemoveAction);
				AssertEquals("Remove action must be Remove Only", RemoveAction.Remove, form.ArchivedDocumentsGrid.RemoveAction);
			}
		}

		public void TestCaptionOnRemoveOption()
		{
			using (ArchiveEDocsForm form = new ArchiveEDocsForm(new ArchiveEDocsManager(new DocumentFactoryProvider().GetFactory(Factory))))
			{
				AssertMenuItemStatus(form.ListToArchiveGrid, "&Remove", false);
				AssertMenuItemStatus(form.ListToArchiveGrid, "Remove from CD", true);

				AssertMenuItemStatus(form.ArchivedDocumentsGrid, "&Remove", false);
				AssertMenuItemStatus(form.ListToArchiveGrid, "Remove from CD", true);
			}
		}

		void AssertMenuItemStatus(ZGrid grid, string menuName, bool shouldExist)
		{
			bool itemFound = false;
			foreach (MenuItem item in grid.ContextMenu.MenuItems)
			{
				if (item.Text == menuName)
				{
					itemFound = true;
				}
			}
			AssertEquals("Menu item " + menuName + (shouldExist ? " should" : " shouldn't") + " exist on grid " + grid.Name, shouldExist, itemFound);
		}

		public void TestCloseFormWithChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";
			org.OH_FullName = "ABC Orginazation";
			var doc = Factory.New<JobRequiredDocument>();
			doc.EQ_DocType = Core.Constants.TransportParentTypes.CommercialInvoice;
			org.RequiredDocuments.Add(doc);

			var job = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			job.JE_OH_Importer = org.PK;

			Factory.Save();

			var masterFactory = new DbBackendDocumentFactory(Factory);
			var storageMain = masterFactory.NewWithValidTestData<StorageMain>();
			storageMain.SM_ParentFK = job.PK;
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			var storageDoc = storageMain.Documents.AddNew();
			storageDoc.SC_DocType = Core.Constants.TransportParentTypes.CommercialInvoice;
			storageDoc.SC_IsPublished = true;
			storageDoc.SC_ImageData = new byte[] { 1, 2, 3 };

			var storageDoc2 = storageMain.Documents.AddNew();
			storageDoc2.SC_DocType = Core.Constants.TransportParentTypes.CommercialInvoice;
			storageDoc2.SC_IsPublished = true;
			storageDoc2.SC_ImageData = new byte[] { 1, 2, 3 };

			masterFactory.Save();

			using (var form = (ArchiveEDocsForm)GetFormToBashCore())
			{
				form.Show();

				form.Manager.SA_Organisation = org.PK;
				form.Manager.SA_IncludeConsignee = true;
				form.Manager.SA_IncludeConsignor = true;

				Type formType = (typeof(ArchiveEDocsForm));
				FieldInfo addToCDButtonField = formType.GetField("AddToCDButton", BindingFlags.NonPublic | BindingFlags.Instance);
				ZButton addToCDButton = (ZButton)addToCDButtonField.GetValue(form);
				AssertNotNull(addToCDButton);
				addToCDButton.PerformClick();

				AssertEquals(form.ArchivedDocumentsGrid.ListManager.List.Count, 2);

				var test = form.ArchivedDocumentsGrid.ListManager.List[0] as StorageDocs;
				form.Refresh();
				form.ArchivedDocumentsGrid.SelectSingleElement(test);

				AssertRemoveFromCDMenuItemEnabled(form.ArchivedDocumentsGrid);
				AssertRemoveFromCDMenuItemEnabled(form.ListToArchiveGrid);

				var contextMenu = form.ArchivedDocumentsGrid.ContextMenu;
				contextMenu.MenuItems.FindByText("Remove from CD").PerformClick();

				form.DisplayMode = ODisplayMode.NewSaved;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				FieldInfo closeButtonField = formType.GetField("CloseButton", BindingFlags.NonPublic | BindingFlags.Instance);
				ZButton closeButton = (ZButton)closeButtonField.GetValue(form);
				AssertNotNull(closeButton);
				AssertNoExceptionThrown(closeButton.PerformClick);
			}
		}

		void AssertRemoveFromCDMenuItemEnabled(ZGrid grid)
		{
			grid.SetCurrentHitTestForTest(0, 0);
			grid.OnPopup_CallForTesting();
			Assert("the menu item 'Remove from CD' of " + grid.Name + " should be enabled", grid.DeleteMenuItem.Enabled);
		}
	}
}
