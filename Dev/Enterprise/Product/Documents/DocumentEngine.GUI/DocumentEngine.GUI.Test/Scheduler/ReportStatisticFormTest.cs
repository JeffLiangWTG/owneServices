using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.MasterFiles.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Scheduler.Testing
{
	[TestedType(typeof(ReportStatisticsForm))]
	class ReportStatisticFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ReportStatisticsForm(Factory.New<StmReportRun>());
		}

		public void TestEDocsPlugInShouldBeAvailable()
		{
			using (var form = (ReportStatisticsForm)GetFormToBash())
			{
				form.Show();

				var eDocPlugin = form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				AssertNotNull("eDocsPlugIn", eDocPlugin);

				form.PlugIns.SelectPlugInTabPage(ControllerIDs.eDocsPlugIn);
				AssertEquals("eDocsPlugIn should be available", true, eDocPlugin.TabPage.Controls.Contains(eDocPlugin.UserControl));
			}
		}

		void TestReportSourceShowingCorrectValue(bool isSystemDefined)
		{
			var stmReportRun = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun.RRI_IsSystemDefined = isSystemDefined;
			Factory.Save();

			using (var form = new ReportStatisticsForm(stmReportRun))
			{
				form.Show();

				var reportSourceTextBox = form.Controls.Find("reportSourceTextBox", true).FirstOrDefault();

				AssertNotNull(reportSourceTextBox);
				AssertEquals(isSystemDefined ? "Y" : "N", reportSourceTextBox.Text);
				AssertEquals(true, reportSourceTextBox.GetReadOnly());
			}
		}

		public void TestReportSourceShowingCorrectValue_Y()
		{
			TestReportSourceShowingCorrectValue(true);
		}
		public void TestReportSourceShowingCorrectValue_N()
		{
			TestReportSourceShowingCorrectValue(false);
		}

		public void TestFormAvailableAndEDocsEditable()
		{
			var stmReportRun = Factory.NewWithValidTestData<StmReportRun>();
			Factory.Save();

			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var docFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var storageMain = (BusinessObject)docFactory.New<IStorageMain>();
			storageMain[StorageMainSchema.Constants.SM_ParentFK] = stmReportRun.PK;
			storageMain[StorageMainSchema.Constants.SM_DB] = 1;
			storageMain[StorageMainSchema.Constants.SM_Type] = "RTS";
			((IStorageMain)storageMain).AddFileOrDocument(new byte[] { 1, 2, 3, 4, 5 }, "normal.pdf", "SREP");
			docFactory.Save();

			bool NeedCheckedControls(Control control) => control.Visible && (control is ZTextBox || control is ZDropEdit || control is ZGuidFindBox || control is ZButton || control is ZCheckBox);

			using (var form = new ReportStatisticsForm(stmReportRun))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = (ZTemplateTabControl)form.FindSingle<ZTabControl>("MainTabControl");
				var amnestyControls = new string[] { "openScheduledReportButton" };
				for (var i = 0; i < tabControl.Controls.Count; i++)
				{
					tabControl.SelectedIndex = i;
					Application.DoEvents();
					if (tabControl.Controls[i] is ZTabPagePlugIn tabPagePlugIn && tabPagePlugIn.PlugIn is eDocsPlugIn plugIn)
					{
						AssertEDocsTab(plugIn);
					}
					else
					{
						var allControls = tabControl.SelectedTab.FindAll<Control>().Where(NeedCheckedControls);
						allControls.ForEach(control =>
						{
							if (amnestyControls.Contains(control.Name))
							{
								AssertEquals($"control[{control.Name}] button should be available", true, !control.GetReadOnly());
							}
							else
							{
								AssertEquals($"control[{control.Name}] should be readonly", true, control.GetReadOnly());
							}
						});
					}
				}
			}
		}

		void AssertEDocsTab(eDocsPlugIn plugIn)
		{
			var userControl = plugIn.UserControl as eDocsUserControl;
			var addEDocsButton = userControl.FindSingle<ZButton>("AddEDocsButton");
			var refreshButton = userControl.FindSingle<ZButton>("RefreshButton");
			AssertEquals("Add eDocs button should be readonly", false, addEDocsButton.Enabled);
			refreshButton.PerformClick();
			Application.DoEvents();
			AssertEquals("Add eDocs button should be readonly", false, addEDocsButton.Enabled);

			var storageDocsGrid = userControl.FindSingle<DocumentsZGrid>("StorageDocsGrid");
			storageDocsGrid.RebuildContextMenu();
			var pasteMenuItem = storageDocsGrid.ContextMenu.MenuItems.OfType<PasteMenuItem>().FirstOrDefault();
			AssertEquals("Paste menu should be hidden", null, pasteMenuItem);
			storageDocsGrid.LastClickPoint = new System.Drawing.Point(10, 30);
			storageDocsGrid.UpdateContextMenuElements((StorageDocsBase)storageDocsGrid.ListManager.GetCurrent());
			storageDocsGrid.DragDropTarget = plugIn;
			storageDocsGrid.OnDragDropForRemote(null);
			AssertEquals("Manually uploading eDocs is not allowed.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

#if !WINZOR
			KeySender.PostKeyDown(storageDocsGrid, storageDocsGrid.Handle, Keys.Control | Keys.V);
			Application.DoEvents();
			AssertEquals("Manually uploading eDocs is not allowed.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();
#endif
		}
	}
}
