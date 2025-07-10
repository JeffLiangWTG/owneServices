using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.GUI.ZBindingContext;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDisplayGridTest : TestCaseWithDummy
	{
		#region Excel Export Tests

		// #warning add tests once stuff in ZGrid is moved into a helper class (see ZGrid test comment)

		public void TestExportToExcelMenuItemDoesNotExist()
		{
			using (var grid = new ZDisplayGrid())
			{
				var excelExportMenuItemFound = false;

				typeof(ZGrid).InvokeMember("ContextMenu_Popup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { grid, EventArgs.Empty });
				foreach (MenuItem item in grid.ContextMenu.MenuItems)
				{
					if (item.Text == "&Export All Columns To Excel" && item.Visible)
					{
						excelExportMenuItemFound = true;
						break;
					}
				}

				AssertEquals("The 'Export All Columns To Excel' menu item should not exist on the ZDisplayGrid context menu.", false, excelExportMenuItemFound);
			}
		}

		public void TestNoNullReferenceExceptionThrownWhenParentFilterGridModuleIsNull()
		{
			using (var testForm = new ZForm())
			using (var testGrid = new ZDisplayGrid())
			{
				var dummies = new DummyBusinessObjectCollection(Factory);
				var columnInfo = new ZTextBoxColumnStyleInfo();
				columnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Code;
				testGrid.Columns.Add(columnInfo);
				testForm.Controls.Add(testGrid);
				testGrid.SetDataBinding(dummies, "");

				AssertNoExceptionThrown(() => testGrid.ExcelExporter.ExportIntoAndOpenExcel(testGrid.ListManager, testGrid.Columns));
			}
		}

		public void TestNoNotImplementedExceptionThrownAndShowMessageWhenNoRecordsToExport()
		{
			using (var testForm = new ZForm())
			using (var testGrid = new ZDisplayGrid())
			{
				var dummies = new TempList();
				var columnInfo = new ZTextBoxColumnStyleInfo();
				columnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Code;
				testGrid.Columns.Add(columnInfo);
				testForm.Controls.Add(testGrid);
				testGrid.SetDataBinding(dummies, "");

				AssertNoExceptionThrown(() => testGrid.ExportVisibleIntoAndOpenExcel());
				AssertEquals("There are no records to export.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

#if !WINZOR
		class ZDisplayGridWithSqlException_CanContinueWithExport : ZDisplayGrid
		{
			protected override bool CanContinueWithExport
			{
				get
				{
					throw SqlExceptionBuilder.CreateSqlException(8623, "The query processor ran out of internal resources and could not produce a query plan.");
				}
			}
		}

		public void TestExportToExcelNoSqlException_GetCanContinueWithExport()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			using (var testForm = new ZForm())
			using (var testGrid = new ZDisplayGridWithSqlException_CanContinueWithExport())
			using (var module = new DummyFilterGridModule())
			{
				var dummies = new DummyBusinessObjectCollection(Factory);

				var columnInfo = new ZTextBoxColumnStyleInfo();
				columnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Code;

				testGrid.Columns.Add(columnInfo);
				testForm.Controls.Add(testGrid);
				testGrid.SetParentFilterGridModule(module);
				testGrid.SetDataBinding(dummies, "");

				var filter = new ZQuery(DummyBizoSchema.PK, dummy.PK);
				dummies.LoadedFilterMatches(filter);
				module.PerformSearch();

				AssertNoExceptionThrown(() => testGrid.ExportVisibleIntoAndOpenExcel());
				AssertEquals("Your query is too complicated, please simplify your search conditions.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);

				AssertNoExceptionThrown(() => testGrid.ExportIntoAndOpenExcel());
				AssertEquals("Your query is too complicated, please simplify your search conditions.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class BusinessObjectReaderWithSqlException_GetDatabaseCount : BusinessObjectReader
		{
			public BusinessObjectReaderWithSqlException_GetDatabaseCount(BusinessObjectFactoryProvider factoryProvider) : base(factoryProvider)
			{
			}

			public override bool HasRecords => true;

			public override Type BusinessObjectType => throw new NotImplementedException();

			protected override BusinessObject[] LoadNextBatchCore(ZGuid pk)
			{
				throw new NotImplementedException();
			}

			protected override BusinessObject[] LoadNextBatchCore(BusinessObject lastBusinessObjectRead)
			{
				throw new NotImplementedException();
			}

			public override int ApproximateCount
			{
				get { throw SqlExceptionBuilder.CreateSqlException(8623, "The query processor ran out of internal resources and could not produce a query plan."); }
			}
		}

		class ZDisplayGridWithSqlException_GetDatabaseCount : ZDisplayGrid
		{
			internal override ExcelExporter ExcelExporter
			{
				get
				{
					return new ExcelExporter(new BusinessObjectReaderWithSqlException_GetDatabaseCount(new BusinessObjectFactoryProvider()), GetNewColumnsForExport(), GetNewExcelExporterGuiNotifications());
				}
			}
		}

		public void TestExportToExcelNoSqlException_GetDatabaseCount()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			using (var testForm = new ZForm())
			using (var testGrid = new ZDisplayGridWithSqlException_GetDatabaseCount())
			using (var module = new DummyFilterGridModule())
			{
				var dummies = new DummyBusinessObjectCollection(Factory);

				var columnInfo = new ZTextBoxColumnStyleInfo();
				columnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Code;

				testGrid.Columns.Add(columnInfo);
				testForm.Controls.Add(testGrid);
				testGrid.SetParentFilterGridModule(module);
				testGrid.SetDataBinding(dummies, "");

				var filter = new ZQuery(DummyBizoSchema.PK, dummy.PK);
				dummies.LoadedFilterMatches(filter);
				AssertNoExceptionThrown(() => testGrid.ExportVisibleIntoAndOpenExcel());
				AssertEquals("Your query is too complicated, please simplify your search conditions.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);

				AssertNoExceptionThrown(() => testGrid.ExportIntoAndOpenExcel());
				AssertEquals("Your query is too complicated, please simplify your search conditions.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
#endif

		public void TestExportSecurityCheckpoint_MainCheckPointIsNone()
		{
			using (var module = new DummyFilterGridModuleWithNoSecurity())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.Grid.ExportIntoAndOpenExcel();
				AssertEquals("There are no records to export.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestExportWithSubclassedFactory()
		{
			using (var module = new DummyFilterGridModuleWithFactorySubclass())
			{
				AssertExceptionThrown("Subclassed factory should be used", typeof(NotSupportedException), () => module.Grid.LoadTop1MatchingFilterInNewFactory());
			}
		}

		class DummyFilterGridModuleWithNoSecurity : DummyFilterGridModule
		{
			public override SecurityCheckpoint SecurityCheckpoint
			{
				get { return (SecurityCheckpoint)EnvProxy.Instance.Security.None; }
			}
		}

		#region Implementation
		class BusinessObjectFactory2 : BusinessObjectFactory
		{
			public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
			{
				return new BusinessObjectFactory2();
			}

			public override BusinessObject Load(Type bizOType, ZGuid pK)
			{
				throw new NotSupportedException();
			}

			public override BusinessObject[] Load(Type bizOType, ZQuery sqlFilter)
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		class DummyFilterGridModuleWithFactorySubclass : DummyFilterGridModule
		{
			protected internal override BusinessObjectFactory GetNewFactory() => new BusinessObjectFactory2();
		}

		public void TestExportWithoutSecurityPermissionIsDenied()
		{
			using (var module = new DummyFilterGridModuleWithExportSecuritySet())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.SetExportSecurityCheckpoint(false);
				module.Grid.ExportIntoAndOpenExcel();

				var expectedMessage = SecurityCore.SecurityErrorMessage + System.Environment.NewLine + System.Environment.NewLine + "TESTING 1 2 3 -> " + SecurityCore.ExportToExcelAutoGeneratedDisplayText;
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestExportWithSecurityPermissionIsAllowed()
		{
			using (var module = new DummyFilterGridModuleWithExportSecuritySet())
			{
				AssertNull("Precondition - the Module.Grid.List should be null.", module.Grid.List);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.SetExportSecurityCheckpoint(true);
				module.Grid.ExportIntoAndOpenExcel();

				AssertEquals(ExcelExporter.ExportMessages.NoRecordsToExport, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestExportWithTooManyRows_StillFunctions()
		{
			SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			for (var i = 0; i < 11; ++i)
			{
				Factory.NewWithValidTestData<DummyBusinessObject>();
			}
			Factory.Save();

			try
			{
				using (var module = new DummyFilterGridModule())
				{
					module.ModifyMaxRowsToLoad(10);
					AssertNotEquals(null, module.Grid.LoadTop1MatchingFilterInNewFactory());
					module.Grid.ExportIntoAndOpenExcel();

					AssertNotEquals(ExcelExporter.ExportMessages.NoRecordsToExport, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				//clean up temporary file created
				foreach (var path in Directory.GetFiles(Temp.TempPath, "CargoWise Export*"))
				{
					TempFile.TryDeleteHandleAllExceptions(path);
				}
			}
		}

		class DummyFilterGridModuleWithExportSecuritySet : DummyFilterGridModule
		{
			public void SetExportSecurityCheckpoint(bool isAllowed)
			{
				var export = EnvProxy.Instance.Security.FindOrCreateExportCheckPoint(SecurityCheckpoint);
				if (export != null)
				{
					export.IsAllowed = isAllowed;
				}
			}
		}

		#endregion
#if !WINZOR
		#region TestScrollBarsEnabled

		public void TestScrollBarsEnabled()
		{
			using (var testForm = new ZChildForm())
			{
				testForm.Size = new Size(300, 172);

				var dummies = new DummyBusinessObjectCollection(Factory);
				dummies.SetReadOnlyIncludingChildren(true);

				var columnInfo1 = new ZTextBoxColumnStyleInfo();
				columnInfo1.ColumnName = DummyBusinessObject.Schema.Z0_Code;
				columnInfo1.Width = 80;

				var columnInfo2 = new ZTextBoxColumnStyleInfo();
				columnInfo2.ColumnName = DummyBusinessObject.Schema.Z0_Description;
				columnInfo2.Width = 80;

				var testGrid = new ZDisplayGridForTest();
				testGrid.Dock = DockStyle.Fill;
				testGrid.IsWholeRowSelectedOnClick = true;
				testGrid.Columns.Add(columnInfo1);
				testGrid.Columns.Add(columnInfo2);
				testForm.Controls.Add(testGrid);

				testGrid.SetDataBinding(dummies, "");
				testForm.Show();

				AssertEquals("PreCondition: Column Count", 2, testGrid.Columns.Count);
				AssertEquals("PreCondition: Registry should have been reset, Column Width is default", 80, testGrid.Columns[0].ColumnStyle.Width);
				AssertEquals("PreCondition: Registry should have been reset, Column Width is default", 80, testGrid.Columns[1].ColumnStyle.Width);

				AssertEquals("Horizontal ScrollBar should not be visible", false, testGrid.HorizScrollBar.Visible);
				AssertEquals("Vertical ScrollBar should not be visible", false, testGrid.VertScrollBar.Visible);

				for (var i = 0; i < 20; i++)
				{
					dummies.AddNew();
				}

				AssertEquals("Horizontal ScrollBar should not be visible", false, testGrid.HorizScrollBar.Visible);
				AssertEquals("Vertical ScrollBar should be visible", true, testGrid.VertScrollBar.Visible);
				AssertEquals("Vertical ScrollBar should be Enabled", true, testGrid.VertScrollBar.Enabled);

				var modalForm = new ZChildForm();
				ZFormModaliser.Show(modalForm, testForm);

				Application.DoEvents();
				System.Threading.Thread.Sleep(100);

				testGrid.RefreshTableStyles();

				modalForm.Close();
				AssertEquals("Vertical ScrollBar should be Enabled", true, testGrid.VertScrollBar.Enabled);
			}
		}

		#endregion
#endif
		#region TestResizeCreatesHints

		public void TestResizeCreatesHints()
		{
			CreateSavedDummies();
			using (var testForm = GetTestForm())
			{
				testForm.Size = new Size(800, 400);
				testForm.Show();
				TestGrid.FirePaint();
				AssertEquals("Precondition", 1, TestGrid.PreFetchHintsHit);
				testForm.Size = new Size(1024, 700);
				TestGrid.FirePaint();
				AssertEquals("Precondition", 2, TestGrid.PreFetchHintsHit);
			}
		}

		#endregion

		#region TestObjectLoadHintReceivedForVisibleRows

		public void TestObjectLoadHintReceivedForVisibleRows()
		{
			CreateSavedDummies();
			using (var testForm = GetTestForm())
			{
				testForm.Show();
				TestGrid.FirePaint();

				AssertEquals(1, TestGrid.PreFetchHintsHit);

				//SELECT * FROM dbo.DummyDependentBizo  
				//SELECT * FROM dbo.DummyBizo WHERE Z0_PK in ('', '')
				//SELECT * FROM dbo.StmData WHERE SD_Name = 'XXX' and SD_Owner = 'YYY'

				AssertEquals(2, Factory.DatabaseLoadCount);

				AssertEquals(TestGrid.VisibleRowCount, TestGrid.LastPreFetchObjects.Length);
				AssertEquals(Dummies[0].PK, TestGrid.LastPreFetchObjects[0].PK);
				AssertEquals(Dummies[TestGrid.VisibleRowCount - 1].PK, TestGrid.LastPreFetchObjects[TestGrid.VisibleRowCount - 1].PK);
			}
		}

		protected override void SetUp()
		{
			// We want to ensure the ProcessFieldChangeRules are pre-loaded in the UberCache so that HitCounts are accurate
			new BusinessObjectFactory().New<DummyBusinessObject>();
			base.SetUp();
		}

		#endregion

		#region TestCopyColumnCaptionsToBoundFields

		public void TestCopyColumnCaptionsToBoundFields()
		{
			using (var grid = new ZDisplayGrid())
			{
				Assert("CopyColumnCaptionsToBoundFields should be disabled by default on ZDisplayGrid", !grid.CopyColumnCaptionsToBoundFields);

				grid.CopyColumnCaptionsToBoundFields = true;
				Assert(grid.CopyColumnCaptionsToBoundFields);

				grid.CopyColumnCaptionsToBoundFields = false;
				Assert(!grid.CopyColumnCaptionsToBoundFields);
			}
		}

		#endregion

		#region Setup of objects/controls for tests

		class NonDependentDummyDependentCollection : BusinessObjectCollection<DummyDependantBusinessObject>
		{
			public NonDependentDummyDependentCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		void CreateSavedDummies()
		{
			var factory2 = new BusinessObjectFactory();

			for (var i = 0; i < 100; i++)
			{
				var bizO = DummyBusinessObject.New(factory2);
				bizO.Z0_Code = "C" + i.ToString();
				var dep = factory2.New<DummyDependantBusinessObject>();
				dep.ZD1_Z0 = bizO.PK;
			}

			factory2.Save();
		}

		ZChildForm GetTestForm()
		{
			var testForm = new ZChildForm();

			testForm.Size = new Size(1024, 700);

			Dummies = new NonDependentDummyDependentCollection(Factory);
			Dummies.SetReadOnlyIncludingChildren(true);
			Dummies.Load();
			AssertEquals("Precondition - objects should exist", 100, Dummies.Count);

			var columnInfo1 = new ZGuidFindBoxColumnStyleInfo();
			columnInfo1.ColumnName = DummyDependantBusinessObject.Schema.ZD1_Z0;
			columnInfo1.ModuleID = Modules.Testing.DummyModuleIDs.Dummy;
			columnInfo1.BindToList = "PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett";
			columnInfo1.Width = 80;

			TestGrid = new ZDisplayGridForTest();
			TestGrid.Dock = DockStyle.Fill;
			TestGrid.IsWholeRowSelectedOnClick = true;
			TestGrid.Columns.Add(columnInfo1);
			testForm.Controls.Add(TestGrid);

			TestGrid.SetDataBinding(Dummies, "");
			return testForm;
		}

		ZDisplayGridForTest TestGrid;
		NonDependentDummyDependentCollection Dummies;

		public void TestShowExportToExcelMenuItem()
		{
			using (var grid = new ZDisplayGridForTest())
			{
				Assert(!grid.ShowExportToExcelMenuItem_Exposed);
				grid.ForceShowExportToExcelMenuItem = true;
				Assert(grid.ShowExportToExcelMenuItem_Exposed);
			}
		}

		#endregion

		#region class ZTestDisplayGrid

		internal class ZDisplayGridForTest : ZDisplayGrid
		{
			protected override void AddFetchHintHints(BusinessObject[] businessObjects, ZGridColumns columns)
			{
				LastPreFetchObjects = businessObjects;
				PreFetchHintsHit++;
				base.AddFetchHintHints(businessObjects, columns);
			}

			public BusinessObject[] LastPreFetchObjects;
			public int PreFetchHintsHit;

			public new ScrollBar HorizScrollBar
			{
				get { return base.HorizScrollBar; }
			}

			public new ScrollBar VertScrollBar
			{
				get { return base.VertScrollBar; }
			}

			public void FirePaint()
			{
				OnPaint(new PaintEventArgs(CreateGraphics(), new Rectangle()));
			}

			public bool ShowExportToExcelMenuItem_Exposed
			{
				get
				{
					return ShowExportToExcelMenuItem;
				}
			}
		}

		#endregion

		#region Other tests
		public void TestFirstBizObjInList()
		{
			Factory.Save(); // new factory used so we need to save ours

			using (var testForm = new ZForm())
			using (var grid = new ZDisplayGrid())
			using (var module = new DummyFilterGridModule())
			{
				grid.SetParentFilterGridModule(module);
				module.PerformSearch();
				var bizObj = grid.GetFirstBizOInList();
				AssertNotNull("Should have found bizo", bizObj);
				AssertEquals("Should be the saved Dummy.", Dummy.PK, bizObj.PK);

				testForm.Controls.Add(grid);
				var columnInfo = new ZTextBoxColumnStyleInfo();
				columnInfo.ColumnName = CargoWise.EntityFramework.Testing.DummyBusinessObject.Schema.Z0_Code;
				grid.ColumnStyles.Add(columnInfo);
				grid.SetDataBinding(Dummy, "Collection");

				testForm.Show();
				AssertNotNull("PreCondition: Bound correctly.", grid.ListManager);
				AssertNotNull("PreCondition: Bound correctly.", grid.List);
				grid.List.Clear();

				bizObj = grid.GetFirstBizOInList();
				AssertNotNull("Should have found bizo", bizObj);
				AssertEquals("Should still be the saved Dummy, List is empty.", Dummy.PK, bizObj.PK);

				grid.ListManager.AddNew();
				bizObj = grid.GetFirstBizOInList();
				AssertNotNull("Should have found bizo", bizObj);
				AssertEquals("Should be list element 1.", grid.List[0], bizObj);
			}
		}

		public void TestFilterBusinessObjectIsCached()
		{
			using (var testForm = new ZForm())
			using (var grid = new ZDisplayGrid())
			using (var module = new DummyFilterGridModule())
			{
				grid.SetParentFilterGridModule(module);
				module.EmbeddedControl.Controls.Add(grid);
				testForm.Controls.Add(module.EmbeddedControl);
				var columnInfo = new ZTextBoxColumnStyleInfo();
				columnInfo.ColumnName = CargoWise.EntityFramework.Testing.DummyBusinessObject.Schema.Z0_Code;
				grid.ColumnStyles.Add(columnInfo);
				grid.SetDataBinding(Dummy, "Collection");

				testForm.Show();
				AssertNotNull("PreCondition: Bound correctly.", grid.ListManager);

				AssertNotNull("FilterBusinessObject is cached correctly", grid.FilterBusinessObject);
			}
		}

		public void TestGridLayoutWithLegacyGridKey()
		{
			using (var testForm = new ZForm())
			using (var module = new DummyFilterGridModule())
			{
				var control = (ZFilterStripControl)module.EmbeddedControl;

				AssertEquals("PreCondition", true, string.IsNullOrEmpty(control.FilteredGrid.GridId));

				control.FilteredGrid.SetParentFilterGridModule(module);
				testForm.Controls.Add(module.EmbeddedControl);

				control.FilteredGrid.SetDataBinding(Dummy, "Collection");

				testForm.Show();

				var filterBizO = module.DummyFilterBusinessObject;

				var filter = (ModuleTextFilter)filterBizO[DummyBizoSchema.Z0_Description.Name];
				filter.Property = "someUserSavedValue";

				var strip1 = filterBizO.FilterStrips.AddNew();
				strip1.FilterDescription = filter.Description;

				//SaveColumnLayout should be Yes as StmModuleFilter.PK becomes part of Legacy context key
				var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterBizO, "SavedTestFilter", true, false, SaveColumnLayout.Yes);
				control.FilteredGrid.CurrentColumnLayout = savedLayout;

				control.FilteredGrid.FilterBusinessObject.SaveLastUsedLayout(savedLayout.PK);
				control.FilteredGrid.Columns[DummyBizoSchema.Z0_Number.Name].IsVisible = false;
				control.FilteredGrid.Columns.HasLayoutChanged = true;
				control.FilteredGrid.SaveLayoutEvenIfColumnsAreUnchanged();
			}

			using (var testForm = new ZForm())
			using (var module = new DummyFilterGridModule())
			{
				var control = (ZFilterStripControl)module.EmbeddedControl;

				AssertEquals("PreCondition:LastUsedLayoutName", "SavedTestFilter", control.FilterBusinessObject.LastUsedLayout.S9_FilterName);

				control.FilteredGrid.SetParentFilterGridModule(module);
				testForm.Controls.Add(module.EmbeddedControl);

				control.FilteredGrid.SetDataBinding(Dummy, "Collection");

				testForm.Show();

				AssertEquals("Number column should be invisible", false, control.FilteredGrid.Columns[DummyBizoSchema.Z0_Number.Name].IsVisible);
				AssertEquals("Description column should be visible", true, control.FilteredGrid.Columns[DummyBizoSchema.Z0_Description.Name].IsVisible);
			}
		}

		public void TestLeftTopCornerIconDoesNotGetUpdated()
		{
			using (var form = new ZForm())
			using (var module = new DummyFilterGridModule())
			{
				var filter = (ZFilterStripControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertEquals(false, filter.FilteredGrid.ShouldShowNotifications);
				module.GridCollection.ResumeValidation(); // it's suspended
				var bo1 = module.GridCollection.AddNew();
				bo1.AddRowError("error!");
				AssertEquals(null, filter.FilteredGrid.NotificationTypeForTesting);

				module.OverrideModuleDecisionProvider(new Modules.Internal.DirectToFormModuleDecisionProvider(new DummyFindBox()));
				AssertEquals(true, filter.FilteredGrid.ShouldShowNotifications);
				bo1.AddRowError("another error!");
				AssertEquals(null, filter.FilteredGrid.NotificationTypeForTesting);
			}
		}

		public void TestShouldShowErrors()
		{
			using (var grid = new ZDisplayGrid())
			using (var module = new DummyFilterGridModule())
			{
				AssertEquals(false, grid.ShouldShowNotifications);
				grid.SetParentFilterGridModule(module);
				AssertEquals(false, grid.ShouldShowNotifications);
				module.OverrideModuleDecisionProvider(new Modules.Internal.DirectToFormModuleDecisionProvider(new DummyFindBox()));
				AssertEquals(true, grid.ShouldShowNotifications);
				module.OverrideModuleDecisionProvider(new Modules.Internal.PopupModuleDecisionProvider(new DummyFindBox()));
				AssertEquals(true, grid.ShouldShowNotifications);
			}
		}
		#endregion
	}
}
