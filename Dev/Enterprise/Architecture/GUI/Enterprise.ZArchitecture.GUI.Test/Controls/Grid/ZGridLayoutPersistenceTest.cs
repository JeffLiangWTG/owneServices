using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business.Internal;

#pragma warning disable CW1108 // Do Not Use DataSet

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridLayoutPersistenceTest : TestCaseWithFactory
	{
		public void TestMultipleColumnsSort()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Code = "a";
			dummy1.Z0_Description = "2";
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Code = "a";
			dummy2.Z0_Description = "1";
			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy3.Z0_Code = "c";
			dummy3.Z0_Description = "3";
			var dummy4 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy4.Z0_Code = "c";
			dummy4.Z0_Description = "4";
			var dummy5 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy5.Z0_Code = "b";
			dummy5.Z0_Description = "6";
			var dummy6 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy6.Z0_Code = "b";
			dummy6.Z0_Description = "5";
			Factory.Save();

			AssertMultipleColumnsSortForDifferentCollectionType(typeof(BusinessObjectCollection));
			AssertMultipleColumnsSortForDifferentCollectionType(typeof(ActiveBusinessObjectCollection));

			void AssertMultipleColumnsSortForDifferentCollectionType(Type collectionType)
			{
				IBusinessObjectCollection collection = null;
				if (collectionType == typeof(ActiveBusinessObjectCollection))
				{
					collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
					var isLoaded = collection.IsLoaded;
				}
				else
				{
					collection = new DummyBusinessObjectCollection(Factory);
					collection.Add(dummy1);
					collection.Add(dummy2);
					collection.Add(dummy3);
					collection.Add(dummy4);
					collection.Add(dummy5);
					collection.Add(dummy6);
				}

				var propertyDescriptorCollection = BusinessObjectPropertyDescriptorCollection.FromType(typeof(DummyBusinessObject));
				var descriptor = propertyDescriptorCollection["Z0_Code"];
				var listView = (IBindingListView)collection;
				listView.ApplySort(descriptor, ListSortDirection.Ascending);
				AssertEquals(dummy1, collection[0]);
				AssertEquals(dummy2, collection[1]);
				AssertEquals(dummy5, collection[2]);
				AssertEquals(dummy6, collection[3]);
				AssertEquals(dummy3, collection[4]);
				AssertEquals(dummy4, collection[5]);

				var sorts = ZGrid.AddSort(listView, propertyDescriptorCollection["Z0_Description"]);
				listView.ApplySort(sorts);
				AssertEquals(dummy2, collection[0]);
				AssertEquals(dummy1, collection[1]);
				AssertEquals(dummy6, collection[2]);
				AssertEquals(dummy5, collection[3]);
				AssertEquals(dummy3, collection[4]);
				AssertEquals(dummy4, collection[5]);
			}
		}

		public void TestMultipleColumnsSortWithView()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Code = "a";
			dummy1.Z0_Description = "2";
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Code = "a";
			dummy2.Z0_Description = "1";
			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy3.Z0_Code = "c";
			dummy3.Z0_Description = "3";
			var dummy4 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy4.Z0_Code = "c";
			dummy4.Z0_Description = "4";
			var dummy5 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy5.Z0_Code = "b";
			dummy5.Z0_Description = "6";
			var dummy6 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy6.Z0_Code = "b";
			dummy6.Z0_Description = "5";
			Factory.Save();

			AssertMultipleColumnsSortForDifferentCollectionType(typeof(IBusinessObjectCollectionView));
			AssertMultipleColumnsSortForDifferentCollectionType(typeof(ActiveBusinessObjectCollection));

			void AssertMultipleColumnsSortForDifferentCollectionType(Type collectionType)
			{
				IBusinessObjectCollection collection = null;
				if (collectionType == typeof(ActiveBusinessObjectCollection))
				{
					collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
					var isLoaded = collection.IsLoaded;
				}
				else
				{
					var collectionToFilter = new DummyBusinessObjectCollection(Factory)
					{
						dummy1,
						dummy2,
						dummy3,
						dummy4,
						dummy5,
						dummy6
					};
					collection = new DummyBusinessObjectCollectionView(collectionToFilter);
				}

				var propertyDescriptorCollection = BusinessObjectPropertyDescriptorCollection.FromType(typeof(DummyBusinessObject));
				var descriptor = propertyDescriptorCollection["Z0_Code"];
				var listView = (IBindingListView)collection;
				listView.ApplySort(descriptor, ListSortDirection.Ascending);
				AssertEquals(dummy1, collection[0]);
				AssertEquals(dummy2, collection[1]);
				AssertEquals(dummy5, collection[2]);
				AssertEquals(dummy6, collection[3]);
				AssertEquals(dummy3, collection[4]);
				AssertEquals(dummy4, collection[5]);

				var sorts = ZGrid.AddSort(listView, propertyDescriptorCollection["Z0_Description"]);
				listView.ApplySort(sorts);
				AssertEquals(dummy2, collection[0]);
				AssertEquals(dummy1, collection[1]);
				AssertEquals(dummy6, collection[2]);
				AssertEquals(dummy5, collection[3]);
				AssertEquals(dummy3, collection[4]);
				AssertEquals(dummy4, collection[5]);
			}
		}

		public void TestHasLayoutChangedFromCustomiseColumns()
		{
			var customiseFormClosedMethod = typeof(ZGrid).GetMethod("CustomiseForm_Closed", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertNotNull("CustomiseForm_Closed Method should be found on ZGrid", customiseFormClosedMethod);

			var resultField = typeof(ZGridCustomise).GetField("result", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertNotNull("Result Field should be found on ZGridCustomise", resultField);

			CreateDataSetForLayoutTest();
			using (TestForm = new KForm())
			{
				InitialiseForm(true);
				AssertEquals("Initial HasLayoutChanged", false, TestGrid.Columns.HasLayoutChanged);

				var keyProvider = new LegacyDataGridLayoutContextKeyProvider(TestGrid);
				var customiseBizObj = new ZGridCustomiseBizObj(new string[] { keyProvider.ContextKeyForStmModuleFilter }, new string[] { keyProvider.ContextKeyForStmData }, null, ZGuid.Empty);
				using (var testCustomiseForm = new ZGridCustomise(TestGrid.Columns, TestGrid.Columns, true, customiseBizObj))
				{
					testCustomiseForm.DialogResult = DialogResult.OK;
					resultField.SetValue(testCustomiseForm, TestGrid.Columns);
					customiseFormClosedMethod.Invoke(TestGrid, new object[] { testCustomiseForm, EventArgs.Empty });
					testCustomiseForm.Close();
				}

				AssertEquals("HasLayoutChanged after Column Customise with OK", true, TestGrid.Columns.HasLayoutChanged);

				TestGrid.Columns.HasLayoutChanged = false;
				using (var testCustomiseForm = new ZGridCustomise(TestGrid.Columns, TestGrid.Columns, true, customiseBizObj))
				{
					testCustomiseForm.DialogResult = DialogResult.Cancel;
					customiseFormClosedMethod.Invoke(TestGrid, new object[] { testCustomiseForm, EventArgs.Empty });
					testCustomiseForm.Close();
				}

				AssertEquals("HasLayoutChanged after Column Customise with Cancel", false, TestGrid.Columns.HasLayoutChanged);
			}
		}

		public void TestGridCurrentLayoutNameIsUpdatedOnlyWhenOKIsPressed()
		{
			var customiseFormClosedMethod = typeof(ZGrid).GetMethod("CustomiseForm_Closed", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertNotNull("CustomiseForm_Closed Method should be found on ZGrid", customiseFormClosedMethod);

			var resultField = typeof(ZGridCustomise).GetField("result", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertNotNull("Result Field should be found on ZGridCustomise", resultField);

			CreateDataSetForLayoutTest();
			using (TestForm = new KForm())
			{
				InitialiseForm(true);
				AssertEquals("Initial HasLayoutChanged", false, TestGrid.Columns.HasLayoutChanged);

				var keyProvider = new LegacyDataGridLayoutContextKeyProvider(TestGrid);
				var customiseBizObj = new ZGridCustomiseBizObj(new string[] { keyProvider.ContextKeyForStmModuleFilter }, new string[] { keyProvider.ContextKeyForStmData }, null, ZGuid.Empty);
				TestGrid.SetCustomiseBizObj(customiseBizObj);

				var manageable = new ZColumnsLayoutModification(new List<ICustomizableColumn>(), Factory, null, keyProvider.ContextKeyForStmModuleFilter);
				var blah = new DataGridLayoutManager().SavePreconfiguredLayout(manageable, "Blah", false, false, SaveColumnLayout.Ignore);
				var blahBlah = new DataGridLayoutManager().SavePreconfiguredLayout(manageable, "BlahBlah", false, false, SaveColumnLayout.Ignore);

				TestGrid.CurrentColumnLayout = blahBlah;

				using (var testCustomiseForm = new ZGridCustomise(TestGrid.Columns, TestGrid.Columns, true, customiseBizObj))
				{
					testCustomiseForm.DialogResult = DialogResult.Cancel;
					resultField.SetValue(testCustomiseForm, TestGrid.Columns);
					customiseBizObj.CurrentLayout = blah;
					customiseFormClosedMethod.Invoke(TestGrid, new object[] { testCustomiseForm, EventArgs.Empty });
					testCustomiseForm.Close();
				}

				AssertEquals("HasLayoutChanged after Column Customise with Cancel", false, TestGrid.Columns.HasLayoutChanged);
				AssertEquals("Therefore ZGrid.CurrentColumnLayoutName should NOT change if cancel is clicked", "BlahBlah", TestGrid.CurrentColumnLayout.ColumnLayoutName);

				TestGrid.SetCustomiseBizObj(customiseBizObj);
				using (var testCustomiseForm = new ZGridCustomise(TestGrid.Columns, TestGrid.Columns, true, customiseBizObj))
				{
					testCustomiseForm.DialogResult = DialogResult.OK;
					resultField.SetValue(testCustomiseForm, TestGrid.Columns);
					customiseBizObj.CurrentLayout = blah;
					customiseFormClosedMethod.Invoke(TestGrid, new object[] { testCustomiseForm, EventArgs.Empty });
					testCustomiseForm.Close();
				}

				AssertEquals("HasLayoutChanged after Column Customise with Cancel", true, TestGrid.Columns.HasLayoutChanged);
				AssertEquals("Therefore ZGrid.CurrentColumnLayoutName should be changed too", "Blah", TestGrid.CurrentColumnLayout.ColumnLayoutName);
			}
		}

		public void TestHasLayoutChangedAndSaveLastSelectedLayoutFromSort()
		{
			CreateDataSetForLayoutTest();
			using (TestForm = new KForm())
			{
				InitialiseForm(true);
				AssertEquals("Initial HasLayoutChanged", false, TestGrid.Columns.HasLayoutChanged);
				AssertEquals("Initial SaveLastSelectedLayout", false, TestGrid.SaveLastSelectedLayout);

				var args = new MouseEventArgs(MouseButtons.Left, 1, 73, 10, 0);
				AssertEquals("Should get Header Click from Args", DataGrid.HitTestType.ColumnHeader, TestGrid.HitTest(args.X, args.Y).Type);

				TestGrid.OnMouseUp(args);
				AssertEquals("HasLayoutChanged after clicking column header to sort", true, TestGrid.Columns.HasLayoutChanged);
				AssertEquals("SaveLastSelectedLayout after clicking column header to sort", true, TestGrid.SaveLastSelectedLayout);
			}
		}

		public void TestSaveLoadColumnsLayout_LayoutChanged()
		{
			CreateDataSetForLayoutTest();

			using (TestForm = new KForm())
			{
				InitialiseForm(true);

				var list = (IBindingList)TestGrid.ListManager.List;
				TestGrid.Columns[0].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				TestGrid.Columns[1].IsVisible = true;

				var testColumn2Descriptor = TestGrid.ListManager.GetItemProperties()["TestColumn2"];
				list.ApplySort(testColumn2Descriptor, ListSortDirection.Descending);
				AssertEquals("First Row", Row3, ((DataRowView)list[0]).Row);

				TestGrid.Columns.HasLayoutChanged = true; // force save
				TestGrid.SaveUserLayoutSettings();

				TestGrid.Columns[0].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
				TestGrid.Columns[1].IsVisible = false;
				var testColumn1Descriptor = TestGrid.ListManager.GetItemProperties()["TestColumn1"];
				list.ApplySort(testColumn1Descriptor, ListSortDirection.Ascending);
				AssertEquals("First Row", Row1, ((DataRowView)list[0]).Row);

				TestGrid.LoadUserLayoutSettings();

				AssertEquals("Column width", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), TestGrid.Columns[0].ColumnStyle.Width);
				AssertEquals("Column visible", true, TestGrid.Columns[1].IsVisible);
				AssertEquals("SortProperty.Name", "TestColumn2", list.SortProperty.Name);
				AssertEquals("SortDirection", ListSortDirection.Descending, list.SortDirection);
				AssertEquals("First Row", Row3, ((DataRowView)list[0]).Row);
			}
		}

		public void TestLoadColumnsLayoutFromBinding()
		{
			CreateDataSetForLayoutTest();

			using (TestForm = new KForm())
			{
				InitialiseForm(true);
				TestGrid.SetDataBinding(Data, "Table1");

				var list = (IBindingList)TestGrid.ListManager.List;
				var testColumn2Descriptor = TestGrid.ListManager.GetItemProperties()["TestColumn2"];
				list.ApplySort(testColumn2Descriptor, ListSortDirection.Descending);

				TestGrid.Columns.HasLayoutChanged = true; // force save
			} // should call SaveUserLayoutSettings when disposing

			using (TestForm = new KForm())
			{
				InitialiseForm(false);
				TestGrid.SetDataBinding(Data, "Table1");

				var list = (IBindingList)TestGrid.ListManager.List;
				AssertNotNull("List should not be null after binding", list);
				AssertNotNull("SortProperty should be set", list.SortProperty);
				AssertEquals("SortProperty.Name", "TestColumn2", list.SortProperty.Name);
				AssertEquals("SortDirection", ListSortDirection.Descending, list.SortDirection);
			}
		}

		public void TestSaveLoadColumnsLayout_LayoutNotChanged()
		{
			CreateDataSetForLayoutTest();

			using (TestForm = new KForm())
			{
				InitialiseForm(true);

				TestGrid.Columns[0].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				TestGrid.Columns.HasLayoutChanged = true; // force save
				TestGrid.SaveUserLayoutSettings();

				TestGrid.Columns[0].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
				TestGrid.Columns.HasLayoutChanged = false; // force no save
				TestGrid.SaveUserLayoutSettings();

				TestGrid.Columns[0].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
				TestGrid.LoadUserLayoutSettings();

				AssertEquals("Column width should be from first save", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), TestGrid.Columns[0].ColumnStyle.Width);
			}
		}

		#region TestLoadUserLayoutSetFrozenSort

		public void TestLoadUserLayoutSetFrozenSort()
		{
			var dummies = new ActiveBusinessObjectCollection<DummyBusinessObject>(new BusinessObjectFactory());

			using (var testForm = new ZForm(dummies))
			{
				var grid = new TestZGrid();
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Number", 100));
				grid.SetBindingMember(".");
				testForm.Controls.Add(grid);

				testForm.Show();
				Application.DoEvents();

				var list = (IBindingList)grid.ListManager.List;
				var propertyDescriptor = grid.ListManager.GetItemProperties()["Z0_Description"];
				list.ApplySort(propertyDescriptor, ListSortDirection.Descending);

				grid.Columns.HasLayoutChanged = true; // force save
				grid.SaveUserLayoutSettings();
			}

			dummies.ApplySort((IComparer)null);
			AssertNull(dummies.SortComparer);

			using (var testForm = new ZForm(dummies))
			{
				var grid = new TestZGrid();
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Code", 100));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Number", 100));
				grid.SetBindingMember(".");
				testForm.Controls.Add(grid);

				testForm.Show();
				Application.DoEvents();

				AssertNotNull(dummies.SortComparer);
				Assert(dummies.SortComparer is IFreezeSortOnElementModifyComparer);

				grid.LoadUserLayoutSettings();

				Assert(dummies.SortComparer is IFreezeSortOnElementModifyComparer);
			}
		}

		#endregion

		#region Implementation

		TestZGrid TestGrid;
		KForm TestForm;
		DataSet Data; // for testing only
		DataTable Table1;
		DataRow Row1;
		DataRow Row2;
		DataRow Row3;

		void CreateDataSetForLayoutTest()
		{
			Data = new DataSet(); // for testing only
			Table1 = new DataTable("Table1");
			Data.Tables.Add(Table1);

			var testColumn1 = new DataColumn("TestColumn1");
			var testColumn2 = new DataColumn("TestColumn2");
			var testColumn3 = new DataColumn("TestColumn3");
			Table1.Columns.Add(testColumn1);
			Table1.Columns.Add(testColumn2);
			Table1.Columns.Add(testColumn3);

			Row1 = Table1.NewRow();
			Row1[testColumn1] = 1;
			Row1[testColumn2] = 1;
			Table1.Rows.Add(Row1);

			Row2 = Table1.NewRow();
			Row2[testColumn1] = 2;
			Row2[testColumn2] = 2;
			Table1.Rows.Add(Row2);

			Row3 = Table1.NewRow();
			Row3[testColumn1] = 3;
			Row3[testColumn2] = 3;
			Table1.Rows.Add(Row3);
		}

		void InitialiseForm(bool shouldBind)
		{
			TestGrid = new TestZGrid();
			TestGrid.Columns.AddTextColumn("TestColumn1", ControlDpiScalingHelper.ScaleToCurrentDpiX(100));
			TestGrid.Columns.AddTextColumn("TestColumn2", ControlDpiScalingHelper.ScaleToCurrentDpiX(100));
			TestGrid.Columns.AddTextColumn("TestColumn3", ControlDpiScalingHelper.ScaleToCurrentDpiX(100));
			TestForm.Controls.Add(TestGrid);

			if (shouldBind)
			{
				TestGrid.SetDataBinding(Data, "Table1");
				TestGrid.RefreshTableStyles();
			}

			TestForm.Show();
		}

		#endregion
	}
}
