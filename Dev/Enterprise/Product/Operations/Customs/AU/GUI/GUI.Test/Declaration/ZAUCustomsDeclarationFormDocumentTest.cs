using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.DocumentScanning.Business.Constants;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class ZAUCustomsDeclarationFormDocumentTest : TestCaseWithDocumentFactory
	{
		public void TestContextMenu()
		{
			using (var form = new ZAUCustomsDeclarationForm(declaration))
			{
				form.Show();
				var mainTabControl = form.CustomsBrokerageUserControl.MainTabControl;
				var eDocsTabPage = mainTabControl.GetTabPage("eDocsTabPage");
				mainTabControl.SelectedTab = eDocsTabPage;

				var plugIn = (eDocsPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var grid = userControl.FindSingle<DocumentsZGrid>("StorageDocsGrid");

				AssertContextMenuItemStates(grid, qrpDoc, false);
				AssertContextMenuItemStates(grid, clearanceDoc, true);
			}
		}

		public void TestContextMenuWithRegistryKeyDisabled()
		{
			var disableQRPViewRegistryItem = (BooleanRegistryItem)RawDataRegistry.Instance.NEXDOCSDisableQRPView;

			using (disableQRPViewRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZAUCustomsDeclarationForm(declaration))
			{
				form.Show();
				var mainTabControl = form.CustomsBrokerageUserControl.MainTabControl;
				var eDocsTabPage = mainTabControl.GetTabPage("eDocsTabPage");
				mainTabControl.SelectedTab = eDocsTabPage;

				var plugIn = (eDocsPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var grid = userControl.FindSingle<DocumentsZGrid>("StorageDocsGrid");

				AssertContextMenuItemStates(grid, qrpDoc, true);
				AssertContextMenuItemStates(grid, clearanceDoc, true);
			}
		}

		void AssertContextMenuItemStates(DocumentsZGrid grid, StorageDocsBase element, bool expectedState)
		{
			grid.RebuildContextMenu();
			grid.UpdateContextMenuElements(element);
			var contextMenuItems = grid.ContextMenu.MenuItems;

			AssertEquals(Constants.ViewMenuText, expectedState, contextMenuItems.FindByText(Constants.ViewMenuText).Enabled);
			AssertEquals(Constants.EditPropertiesMenuText, expectedState, contextMenuItems.FindByText(Constants.EditPropertiesMenuText).Enabled);
			AssertEquals(Constants.CutMenuText, expectedState, contextMenuItems.FindByText(Constants.CutMenuText).Enabled);
			AssertEquals(Constants.CopyMenuText, expectedState, contextMenuItems.FindByText(Constants.CopyMenuText).Enabled);
			AssertEquals(Constants.CopyLinkMenuText, expectedState, contextMenuItems.FindByText(Constants.CopyLinkMenuText).Enabled);
			AssertEquals(Constants.SplitDocumentMenuText, expectedState, contextMenuItems.FindByText(Constants.SplitDocumentMenuText).Enabled);
			AssertEquals(Constants.SaveFileAsMenuText, expectedState, contextMenuItems.FindByText(Constants.SaveFileAsMenuText).Enabled);
		}

		public void TestShowDisclaimerMessageOnDeliverEDoc()
		{
			using (var form = new ZAUCustomsDeclarationForm(declaration))
			{
				form.Show();
				var mainTabControl = form.CustomsBrokerageUserControl.MainTabControl;
				var eDocsTabPage = mainTabControl.GetTabPage("eDocsTabPage");
				mainTabControl.SelectedTab = eDocsTabPage;

				var plugIn = (eDocsPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				var userControl = (eDocsUserControl)plugIn.UserControl;
				var grid = userControl.FindSingle<DocumentsZGrid>("StorageDocsGrid");

				grid.SelectSingleElementByPK(qrpDoc.PK);
				AssertContextMenuClick(grid, Constants.DeliverDocumentMenuText, "To be able to print export documents");

				grid.SelectSingleElementByPK(clearanceDoc.PK);
				AssertContextMenuClick(grid, Constants.DeliverDocumentMenuText, "");
			}
		}

		public void TestShowDisclaimerMessageOnView()
		{
			var filenames = new List<string>(2);
			try
			{
				var disableQRPViewRegistryItem = (BooleanRegistryItem)RawDataRegistry.Instance.NEXDOCSDisableQRPView;

				using (disableQRPViewRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (var form = new ZAUCustomsDeclarationForm(declaration))
				{
					form.Show();
					var mainTabControl = form.CustomsBrokerageUserControl.MainTabControl;
					var eDocsTabPage = mainTabControl.GetTabPage("eDocsTabPage");
					mainTabControl.SelectedTab = eDocsTabPage;

					var plugIn = (eDocsPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
					var userControl = (eDocsUserControl)plugIn.UserControl;
					var grid = userControl.FindSingle<DocumentsZGrid>("StorageDocsGrid");

					var startingClickPoint = new Point(10, 30);
					var offset = grid.GetCurrentCellBounds().Height;

					grid.SelectSingleElementByPK(qrpDoc.PK);
					filenames.Add(grid.CurrentElement.SaveToTempFile());
					grid.LastClickPoint = new Point(startingClickPoint.X, startingClickPoint.Y + (grid.CurrentRowIndex * offset));
					AssertContextMenuClick(grid, Constants.ViewMenuText, "To be able to print export documents");

					grid.SelectSingleElementByPK(clearanceDoc.PK);
					filenames.Add(grid.CurrentElement.SaveToTempFile());
					grid.LastClickPoint = new Point(startingClickPoint.X, startingClickPoint.Y + (grid.CurrentRowIndex * offset));
					AssertContextMenuClick(grid, Constants.ViewMenuText, "");
				}
			}
			finally
			{
				foreach (var tempFileName in filenames)
				{
					if (File.Exists(tempFileName))
					{
						File.SetAttributes(tempFileName, FileAttributes.Normal);
						File.Delete(tempFileName);
					}
				}
			}
		}

		void AssertContextMenuClick(DocumentsZGrid grid, string menuItemName, string expectedLastMessageText)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			grid.ContextMenu.MenuItems.FindByText(menuItemName).PerformClick();
			AssertContains(menuItemName, expectedLastMessageText, UnitTestUserNotification.Instance.LastMessage?.Text ?? string.Empty);
		}

		JobDeclaration declaration;
		StorageDocsBase qrpDoc;
		StorageDocsBase clearanceDoc;

		protected override void SetUp()
		{
			declaration = MasterFactory.New<JobDeclaration>();

			var storageMain = MasterFactory.NewWithValidTestData<StorageMain>();
			storageMain.SM_ParentFK = declaration.PK;
			storageMain.SM_Type = Core.Constants.DocManagerCodes.JobDeclaration;

			qrpDoc = storageMain.Documents.AddNew();
			qrpDoc.SC_FileName = "QRPTest";
			qrpDoc.SC_DataType = Core.Constants.FileFormats.PDF;
			qrpDoc.SC_DocType = Core.Constants.RefDocTypes.QuarantineRemotePrint;
			qrpDoc.SC_ImageData = new byte[] { 1, 2, 3, 4, 5 };

			clearanceDoc = storageMain.Documents.AddNew();
			clearanceDoc.SC_FileName = "ClearanceTest";
			clearanceDoc.SC_DataType = Core.Constants.FileFormats.PDF;
			clearanceDoc.SC_DocType = Core.Constants.RefDocTypes.ClearanceAdvice;
			clearanceDoc.SC_ImageData = new byte[] { 1, 2, 3, 4, 5 };

			MasterFactory.Save();
		}
	}
}
