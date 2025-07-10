using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class ZSplitResultGridTest : Enterprise.DocumentScanning.Business.Test.TestCaseWithDocumentFactory
	{
		public void TestDoubleClickOnHeaderRow()
		{
			AssertEquals(0, Grid.DoubleClickCounter);
			Grid.LastClickPoint = new Point(10, 5);//header row
			Grid.FireDoubleClickedIfApplicableInternal();
			AssertEquals("should not process for header double click", 0, Grid.DoubleClickCounter);
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestViewWithNull()
		{
			Grid.ViewInternal(null);
		}

		public void TestGetElementAtPosition()
		{
			AssertEquals("should return null if not on a valid row", null, Grid.GetElementAtPosition(new Point(0, 0)));
			AssertEquals("Should return a proper bizo if a valid row", DocumentAtMouse, Grid.GetElementAtPosition(StartingClickPoint));
		}

		public void TestCurrentElementAtMousePosition()
		{
			Grid.LastClickPoint = new Point(0, 0);
			AssertNull("Should return null - there is nothing at point 0,0", Grid.CurrentElementAtMousePosition);

			Grid.LastClickPoint = StartingClickPoint;
			AssertEquals("Should return a valid bizO", DocumentAtMouse, Grid.CurrentElementAtMousePosition);
		}

		public void TestCurrentElement()
		{
			Grid.CurrentRowIndex = 0;
			AssertEquals("Should return a valid bizO", DocumentAtMouse, Grid.CurrentElement);

			Grid.CurrentRowIndex = 1;
			AssertEquals("Should return a valid bizo", DocumentInListButNotAtMouse, Grid.CurrentElement);

			Grid.CurrentRowIndex = 2;
			AssertEquals("Should return a valid bizo", FileInListButNotAtMouse, Grid.CurrentElement);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestView_ForDocument()
		{
			Grid.ListManager.Position = 0;
			var file = MasterFactory.NewWithValidTestData<StorageDocs>();
			file.SC_FileName = "Test";
			file.SC_DataType = Core.Constants.FileFormats.TIF;
			file.SC_ImageData = new ZBlob(File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TifFileHavingSixPages.tif")));
			SplitManager.DocumentToSplit = file;

			Grid.Select(0);
			DocumentSplitResultInfo selectedDocument = (DocumentSplitResultInfo)Grid.SelectedElements[0];

			Grid.SetupContextMenuInternal();

			AssertNull("Temp document doesn't exist before click", Grid.CurrentFile);
			MenuItem viewMenuItem = FindMenuItemByName(Grid.ContextMenu, Constants.ViewMenuText);
			viewMenuItem.PerformClick();
			AssertEquals("Document should be marked as 'read'", true, Grid.CurrentFile.IsReadByUserInThisSession);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestView_ForDocumentOtherEditor()
		{
			var file = MasterFactory.NewWithValidTestData<StorageDocs>();
			file.SC_FileName = "Test";
			file.SC_DataType = Core.Constants.FileFormats.PDF;
			file.SC_ImageData = new ZBlob(File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\PdfFileHavingThreePages.pdf")));
			SplitManager.DocumentToSplit = file;

			SystemDataRegistry.Instance.UseDefaultWindowsImageViewer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Grid.Select(0);
			DocumentSplitResultInfo selectedDocument = (DocumentSplitResultInfo)Grid.SelectedElements[0];

			try
			{
				Grid.SetupContextMenuInternal();

				AssertNull("Temp document doesn't exist before click", Grid.CurrentFile);
				MenuItem viewMenuItem = FindMenuItemByName(Grid.ContextMenu, Constants.ViewMenuText);
				viewMenuItem.PerformClick();
				AssertEquals("Document should be marked as 'read'", true, Grid.CurrentFile.IsReadByUserInThisSession);
			}
			finally
			{
				File.SetAttributes(Grid.CurrentFile.TempFileName, FileAttributes.Normal);
				File.Delete(Grid.CurrentFile.TempFileName);
			}
		}

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

		#region SetUp/TearDown Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Grid = new ZSplitResultGridForTesting();
			RunGridSetup(Grid);
		}

		void RunGridSetup(ZSplitResultGridForTesting grid)
		{
			Form = new ZForm();
			Form.Controls.Add(grid);
			grid.Dock = DockStyle.Fill;
			Form.Location = new Point(0, 0);
			Form.StartPosition = FormStartPosition.Manual;

			StorageDocs doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			doc.SC_DataType = Core.Constants.FileFormats.PDF;
			SplitManager = new DocumentSplitManager(doc, doc.ParentMain.eDocs);
			GridCollection = SplitManager.DocumentSplitResultCollection;

			DocumentAtMouse = GridCollection.AddNew();
			DocumentAtMouse.DocumentData = new byte[] { 1, 2, 3, 4, 5 };
			DocumentInListButNotAtMouse = GridCollection.AddNew();
			FileInListButNotAtMouse = GridCollection.AddNew();

			DocumentAtMouse.DocumentName = "Test1";
			DocumentInListButNotAtMouse.DocumentName = "Test2";
			FileInListButNotAtMouse.DocumentName = "Test3";

			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo textBoxColumnInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo calcBoxColumnInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			textBoxColumnInfo1.ColumnName = "DocumentName";
			calcBoxColumnInfo1.ColumnName = "StartPage";
			grid.ColumnStyles.Add(textBoxColumnInfo1);
			grid.ColumnStyles.Add(calcBoxColumnInfo1);

			Form.Show();
			grid.SetDataBinding(GridCollection, "", GridCollection.GetType().Name);

			grid.ListManager.Position = 0;
			AssertEquals("Precondition: First element in grid is the DocumentAtMouse", ((DocumentSplitResultInfo)grid.ListManager.GetCurrent()).PK, DocumentAtMouse.PK);
			grid.ListManager.Position = 1;
			AssertEquals("Precondition: Second element in grid is the DocumentInListButNotAtMouse", ((DocumentSplitResultInfo)grid.ListManager.GetCurrent()).PK, DocumentInListButNotAtMouse.PK);
			grid.ListManager.Position = 2;
			AssertEquals("Precondition: third element in grid is the FileInListButNotAtMouse", ((DocumentSplitResultInfo)grid.ListManager.GetCurrent()).PK, FileInListButNotAtMouse.PK);

			StartingClickPoint = new Point(10, 30); // (10,30) in control coords or (10,70) in screen coords
			grid.LastClickPoint = StartingClickPoint;
		}

		protected override void TearDown()
		{
			Form.Dispose();
			base.TearDown();
			DeleteTempFiles();
		}

		#endregion

		ZSplitResultGridForTesting Grid;
		DocumentSplitManager SplitManager;
		DocumentSplitResultInfoCollection GridCollection;
		ZForm Form;
		DocumentSplitResultInfo DocumentAtMouse;
		DocumentSplitResultInfo DocumentInListButNotAtMouse;
		DocumentSplitResultInfo FileInListButNotAtMouse;
		Point StartingClickPoint;

		#region Test Objects

		internal class ZSplitResultGridForTesting : ZSplitResultGrid
		{
			public int DoubleClickCounter;
			protected override void DoubleClicked()
			{
				DoubleClickCounter++;
				base.DoubleClicked();
			}

			internal void FireDoubleClickedIfApplicableInternal() => base.FireDoubleClickedIfApplicable();

			internal void ViewInternal(DocumentSplitResultInfo file) => base.View(file);

			internal void SetupContextMenuInternal() => base.SetupContextMenu();
		}

		#endregion
	}
}
