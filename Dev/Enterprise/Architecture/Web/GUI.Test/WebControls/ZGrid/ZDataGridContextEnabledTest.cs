using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Moq;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[HttpContextEnabledTest]
	public class ZDataGridContextEnabledTest : TestCaseWithFactory
	{
		#region Implementation

		void CreateLotsOfChildren(DummyBusinessObject parent, int numToCreate)
		{
			for (int i = 0; i < numToCreate; i++)
			{
				DummyChildBusinessObject child = Factory.NewWithValidTestData<DummyChildBusinessObject>();
				child.Z0_Number = i;
				child.Z0_Guid = parent.PK;
			}
			Factory.Save();
		}

		DummyBusinessObject fTestBizO;
		protected DummyBusinessObject TestBizO
		{
			get
			{
				if (fTestBizO == null)
				{
					fTestBizO = GetNewDataSource();
				}
				return fTestBizO;
			}
		}

		protected virtual DummyBusinessObject GetNewDataSource()
		{
			DummyBusinessObject result = Factory.NewWithValidTestData<DummyBusinessObject>();
			result.Z0_VarCharMax = "ABC";
			return result;
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected BusinessObjectFactory WebFactory
		{
			get { return Factory; }
		}

		protected ZDataGrid TestGrid
		{
			get
			{
				if (fTestGrid == null)
				{
					fTestGrid = new ZDataGrid();
					fTestGrid.Page = new ZTestPage();
				}
				return fTestGrid;
			}
		}
		ZDataGrid fTestGrid;
		#endregion

		#region Paging

		public void TestPaging()
		{
			TestGrid.AllowPaging = true;
			TestGrid.MaxRows = 2;
			SetupGridForSaveDiscardTests();
			TestGrid.AllowEdit = true;
			AssertEquals(10, TestGrid.Collection.Count);
			AssertEquals(1, TestGrid.CachedCurrentPage);
			AssertEquals(0, TestGrid.CurrentPageIndex);

			TestGrid.OnPageIndexChangedInternal(new DataGridPageChangedEventArgs(this, 2));
			AssertEquals(3, TestGrid.CachedCurrentPage);
			AssertEquals(2, TestGrid.CurrentPageIndex);
		}

		public void TestBindCoreOnlyGetsTriggeredOnceOnPageIndexChanged()
		{
			var testGrid = new TestDataGrid();
			AssertEquals(0, testGrid.OnDataBindingCallCount);

			testGrid.BindTo = "Collection";
			testGrid.Bind(TestBizO);
			AssertEquals(1, testGrid.OnDataBindingCallCount);

			testGrid.BindCore();
			AssertEquals(2, testGrid.OnDataBindingCallCount);

			testGrid.OnPageIndexChangedInternal(new DataGridPageChangedEventArgs(this, 0));
			AssertEquals(3, testGrid.OnDataBindingCallCount);

			testGrid.PageIndexChanged += (e, _) => testGrid.BindCore();
			testGrid.OnPageIndexChangedInternal(new DataGridPageChangedEventArgs(this, 0));
			AssertEquals(4, testGrid.OnDataBindingCallCount);
		}

		class TestDataGrid : ZDataGrid
		{
			public int OnDataBindingCallCount;

			protected override void OnDataBinding(EventArgs e)
			{
				base.OnDataBinding(e);
				OnDataBindingCallCount++;
			}
		}

		public void TestAddNewRowWhenPagingAllowed()
		{
			TestGrid.AllowAdd = true;
			TestGrid.AllowPaging = true;
			TestGrid.PageSize = 2;
			TestGrid.Columns.Add(new ZTextEditColumn("Test", DummyBizoSchema.Z0_VarCharMax.Name));
			TestGrid.BindTo = "Collection";
			TestGrid.Bind(TestBizO);

			AssertEquals(0, TestGrid.Collection.Count);
			AssertEquals(1, TestGrid.PageCount);
			AssertEquals(0, TestGrid.CurrentPageIndex);
			AssertEquals(true, TestGrid.ShowFooter);

			TestGrid.AddNewRow();
			AssertEquals(1, TestGrid.Collection.Count);
			AssertEquals(1, TestGrid.PageCount);
			AssertEquals(0, TestGrid.CurrentPageIndex);
			AssertEquals(true, TestGrid.ShowFooter);

			TestGrid.AddNewRow();
			AssertEquals(2, TestGrid.Collection.Count);
			AssertEquals(1, TestGrid.PageCount);
			AssertEquals(0, TestGrid.CurrentPageIndex);
			AssertEquals(true, TestGrid.ShowFooter);

			TestGrid.AddNewRow();
			AssertEquals(3, TestGrid.Collection.Count);
			AssertEquals(2, TestGrid.PageCount);
			AssertEquals(1, TestGrid.CurrentPageIndex);
			AssertEquals(true, TestGrid.ShowFooter);
		}

		#endregion

		#region TestOnDeleteSavesChanges

		public void TestOnDeleteSavesChanges()
		{
			SetupGridForSaveDiscardTests();
			PlaceDataGridInEditMode(2, "Splatty");

			AssertEquals("The datagrid should be editing row 2", 2, TestGrid.EditItemIndex);
			AssertEquals("The 4th row should contain Child number 4", 4, TestBizO.Collection[4].Z0_Number);
			TestGrid.OnDeleteCommandInternal(new DataGridCommandEventArgs(TestGrid.Items[4], null, new CommandEventArgs("Delete", null)));
			AssertEquals("The 4th row should have been deleted", 5, TestBizO.Collection[4].Z0_Number);
			AssertEquals("Grid should no longer be editable", -1, TestGrid.EditItemIndex);
			AssertEquals("Data collection should have been updated for row 2", "Splatty", TestBizO.Collection[2].Z0_VarCharMax);

			PlaceDataGridInEditMode(4, "Splatty");

			AssertEquals("The datagrid should be editing row 4", 4, TestGrid.EditItemIndex);
			AssertEquals("The 4th row should contain Child number 5", 5, TestBizO.Collection[4].Z0_Number);
			TestGrid.OnDeleteCommandInternal(new DataGridCommandEventArgs(TestGrid.Items[4], null, new CommandEventArgs("Delete", null)));
			AssertEquals("The 4th row should have been deleted", 6, TestBizO.Collection[4].Z0_Number);
			AssertEquals("Grid should no longer be editable", -1, TestGrid.EditItemIndex);
		}
		#endregion

		#region TestOnEditSavesChanges

		public void TestOnEditSavesChanges()
		{
			SetupGridForSaveDiscardTests();
			PlaceDataGridInEditMode(2, "Splatty");

			AssertEquals("The datagrid should be editing row 2", 2, TestGrid.EditItemIndex);
			AssertEquals("The 4th row should contain Child number 4", 4, TestBizO.Collection[4].Z0_Number);

			PlaceDataGridInEditMode(4, "Another Splatty");
			AssertEquals("Grid should now be editing row 4", 4, TestGrid.EditItemIndex);
			AssertEquals("Data collection should have been updated for row 2", "Splatty", TestBizO.Collection[2].Z0_VarCharMax);

			TestGrid.OnEditCommandInternal(new DataGridCommandEventArgs(TestGrid.Items[4], null, new CommandEventArgs("Edit", null)));

			ZTextBox textBox = TestGrid.Items[4].Controls[0].Controls[0] as ZTextBox;
			AssertNotNull("Cell should contain TextBox", textBox);
			AssertEquals("Previous value should have been discarded when we select edit on an row that is in edit mode", "", textBox.Text);
			AssertEquals("Should still be editing row 4", 4, TestGrid.EditItemIndex);
		}
		#endregion

		#region TestGetEDocs

		public void TestGetEDocs()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Collection.AddNew();
			dummy.Collection.AddNew();
			TestGrid.Columns.Add(new ZTextEditColumn("Code", DummyBusinessObject.Schema.Z0_Code));
			TestGrid.BindTo = "Collection";
			TestGrid.Bind(dummy);

			var helperMock = new Mock<IEDocsWebHelper>();
			ObjectFactory.Substitute(helperMock.Object);

			var moduleMock = new Mock<ISupportEDocsBulkDownload>();
			var module = moduleMock.Object;

			var relatedPKs = new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() };
			moduleMock.Setup(m => m.GetEDocsBulkDownloadRelevantAndRelatedPKs(TestGrid, 0)).Returns(relatedPKs);

			var eDoc1Mock = new Mock<IeDocBase>();
			var eDoc2Mock = new Mock<IeDocBase>();
			var expectedEDocs = new[] { eDoc1Mock.Object, eDoc2Mock.Object };
			helperMock.Setup(h => h.GetEDocs(relatedPKs)).Returns(expectedEDocs);

			var actualEDocs = TestGrid.GetEDocs(0, module);
			AssertEquals(expectedEDocs, actualEDocs);
			moduleMock.Verify(m => m.GetEDocsBulkDownloadRelevantAndRelatedPKs(TestGrid, 0), Times.Once);
			helperMock.Verify(h => h.GetEDocs(relatedPKs), Times.Once);

			actualEDocs = TestGrid.GetEDocs(1, module);
			AssertNotEquals(expectedEDocs, actualEDocs);
			moduleMock.Verify(m => m.GetEDocsBulkDownloadRelevantAndRelatedPKs(TestGrid, 0), Times.Once);
			helperMock.Verify(h => h.GetEDocs(relatedPKs), Times.Once);

			actualEDocs = TestGrid.GetEDocs(0, module);
			AssertEquals(expectedEDocs, actualEDocs);
			moduleMock.Verify(m => m.GetEDocsBulkDownloadRelevantAndRelatedPKs(TestGrid, 0), Times.Once);
			helperMock.Verify(h => h.GetEDocs(relatedPKs), Times.Once);
		}

		#endregion

		#region TestSaveDiscardChanges

		public void TestSaveDiscardChanges()
		{
			SetupGridForSaveDiscardTests();

			PlaceDataGridInEditMode(2, "Splatty");
			BusinessObject rollbackBizO = TestGrid.BusinessObjectForRollback;
			ZGuid rollbackBizOIndexer = TestGrid.BusinessObjectForRollbackIndexer;
			AssertEquals("Indexer should be not empty", false, rollbackBizOIndexer.IsEmpty);
			AssertNotNull("RollbackBizO should be stored in Session", TestGrid.Page.Session[rollbackBizOIndexer.ToString()]);

			TestGrid.SaveChanges(null);
			AssertEquals("Grid should no longer be editable", -1, TestGrid.EditItemIndex);
			AssertEquals("Data collection should have been updated", "Splatty", TestBizO.Collection[2].Z0_VarCharMax);
			AssertNull("BusinessObjectForRollback should be null", TestGrid.BusinessObjectForRollback);
			AssertEquals("Grid should no longer be editable", -1, TestGrid.EditItemIndex);
			AssertEquals("RollbackBizO was deleted", true, rollbackBizO.IsDeleted);
			AssertEquals("Indexer to store BusinessObjectToRollback in Session should be empty", true, TestGrid.BusinessObjectForRollbackIndexer.IsEmpty);
			AssertNull("RollbackBizO should be removed from Session", TestGrid.Page.Session[rollbackBizOIndexer.ToString()]);

			PlaceDataGridInEditMode(4, "Splatty");

			TestGrid.DiscardChanges(null);
			AssertEquals("Grid should no longer be editable", -1, TestGrid.EditItemIndex);
			AssertEquals("Data collection should not have been updated", "", TestBizO.Collection[4].Z0_VarCharMax);
			AssertNull("BusinessObjectForRollback should be null", TestGrid.BusinessObjectForRollback);
		}

		public void TestRollbackBusinessObjectChanges()
		{
			SetupGridForSaveDiscardTests();

			PlaceDataGridInEditMode(2, "Splatty");
			BusinessObject rollbackBizO = TestGrid.BusinessObjectForRollback;
			ZGuid rollbackBizOIndexer = TestGrid.BusinessObjectForRollbackIndexer;
			AssertEquals("Indexer should be not empty", false, rollbackBizOIndexer.IsEmpty);
			AssertNotNull("RollbackBizO should be stored in Session", TestGrid.Page.Session[rollbackBizOIndexer.ToString()]);

			TestBizO.Collection[2].Z0_VarCharMax = "Splatty";
			AssertEquals("Data should be changed", "Splatty", TestBizO.Collection[2].Z0_VarCharMax);

			TestGrid.DiscardChanges(null);
			AssertEquals("Grid should no longer be editable", -1, TestGrid.EditItemIndex);
			AssertEquals("Data collection should be rolled back", "", TestBizO.Collection[2].Z0_VarCharMax);
			AssertNull("BusinessObjectForRollback should be null", TestGrid.BusinessObjectForRollback);
			AssertEquals("RollbackBizO was deleted", true, rollbackBizO.IsDeleted);
			AssertEquals("Indexer to store BusinessObjectToRollback in Session should be empty", true, TestGrid.BusinessObjectForRollbackIndexer.IsEmpty);
			AssertNull("RollbackBizO should be removed from Session", TestGrid.Page.Session[rollbackBizOIndexer.ToString()]);
		}

		void SetupGridForSaveDiscardTests()
		{
			//Clear the text property as it causes intermittent failing tests if TestBizO is third in the collection
			TestBizO.Z0_VarCharMax = "";
			TestGrid.AllowPaging = false;
			CreateLotsOfChildren(TestBizO, 10);
			ZQuery childrenOnlyFilter = new ZQuery(DummyBizoSchema.Z0_Guid, TestBizO.PK);
			childrenOnlyFilter.OrderBy = DummyBizoSchema.Z0_Number.Name;
			TestBizO.Collection.Load(childrenOnlyFilter);
			AssertEquals("Collection.Count should be 10", 10, TestBizO.Collection.Count);

			TestGrid.Columns.Add(new ZTextEditColumn("Text", DummyBizoSchema.Z0_VarCharMax.Name));
			TestGrid.BindTo = "Collection";
			TestGrid.Bind(TestBizO);
			AssertEquals("TestGrid should have 10 Items", 10, TestGrid.Items.Count);
		}

		void PlaceDataGridInEditMode(int dataGridRowToPlaceInEditMode, string editText)
		{
			TestGrid.AllowEdit = true;
			TestGrid.OnEditCommandInternal(new DataGridCommandEventArgs(TestGrid.Items[dataGridRowToPlaceInEditMode], null, new CommandEventArgs("Edit", null)));
			AssertEquals("EditItemIndex", dataGridRowToPlaceInEditMode, TestGrid.EditItemIndex);
			AssertEquals("Editable Cell", 1, TestGrid.Items[dataGridRowToPlaceInEditMode].Controls.Count);
			AssertEquals("Editable Control", 1, TestGrid.Items[dataGridRowToPlaceInEditMode].Controls[0].Controls.Count);
			ZTextBox textBox = TestGrid.Items[dataGridRowToPlaceInEditMode].Controls[0].Controls[0] as ZTextBox;
			AssertNotNull("Cell should contain TextBox", textBox);
			AssertEquals("TextBox should initially be blank", "", textBox.Text);
			AssertEquals("DummyBizO should have blank text", "", TestBizO.Collection[dataGridRowToPlaceInEditMode].Z0_VarCharMax);
			AssertNotNull("BusinessObjectForRollback should be not null", TestGrid.BusinessObjectForRollback);
			DummyChildBusinessObject savedBizO = TestGrid.BusinessObjectForRollback as DummyChildBusinessObject;
			AssertNotNull("BusinessObjectForRollback should be a DummyChildBusinessObject", savedBizO);
			AssertEquals("BusinessObjectForRollback should save editing BusinessObject", TestBizO.Collection[dataGridRowToPlaceInEditMode].Z0_Number, savedBizO.Z0_Number);

			textBox.Text = editText;
			textBox.HasChanges = true;
		}
		#endregion

		#region TestNoPhantomWhileRollingBack

		class DummyActiveCollection : ActiveBusinessObjectCollection<SupportingCloneBizO>
		{
			public DummyActiveCollection(DummyBusinessObject parent) : base(parent) { }
		}

		class SupportingCloneBizO : DummyDependantBusinessObject
		{
			public SupportingCloneBizO(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override bool SupportsCloneCore()
			{
				return true;
			}
		}

		public void TestNoPhantomWhileRollingBack()
		{
			TestBizO.CollectionWithPublicSetter = new DummyActiveCollection(TestBizO);

			var testCollection = (DummyActiveCollection)TestBizO.CollectionWithPublicSetter;
			testCollection.AddNew().ZD1_Code = "code";
			testCollection.AddNew().ZD1_Code = "code2";

			TestGrid.Columns.Add(new ZTextEditColumn("Code", DummyDependentBizoSchema.ZD1_Code.Name));
			TestGrid.BindTo = "CollectionWithPublicSetter";
			TestGrid.Bind(TestBizO);
			AssertEquals("testGrid should have 2 Items", 2, TestGrid.Items.Count);

			TestGrid.OnEditCommandInternal(new DataGridCommandEventArgs(TestGrid.Items[1], null, new CommandEventArgs("Edit", null)));
			var rollbackBizO = TestGrid.BusinessObjectForRollback as DummyDependantBusinessObject;
			var rollbackBizOIndexer = TestGrid.BusinessObjectForRollbackIndexer;

			AssertEquals("Collection Count should stay unchanged", 2, testCollection.Count);
			AssertNotEquals("Objects' factories", TestBizO.Factory, rollbackBizO.Factory);

			var originalCode = testCollection[1].ZD1_Code;
			testCollection[1].ZD1_Code = "new";
			TestGrid.DiscardChanges(null);
			AssertEquals("Data should be rolled back", originalCode, testCollection[1].ZD1_Code);

			AssertNull("RollbackBizO should be null", TestGrid.BusinessObjectForRollback);
			AssertNull("RollbackBizO should be removed from Session", TestGrid.Page.Session[rollbackBizOIndexer.ToString()]);
			Assert("RollbackBizO was deleted", rollbackBizO.IsDeleted);
			Assert("RollbackBizOIndexer should be empty", TestGrid.BusinessObjectForRollbackIndexer.IsEmpty);
		}

		#endregion

		#region TestEditingItemControlChanged

		public void TestEditingItemControlChanged()
		{
			SetupGridForSaveDiscardTests();
			PlaceDataGridInEditMode(2, "Splatty");

			AssertEquals("The datagrid should be editing row 2", 2, TestGrid.EditItemIndex);

			ZTextBox textBox = TestGrid.Items[2].Controls[0].Controls[0] as ZTextBox;
			AssertNotNull("Cell should contain TextBox", textBox);
			AssertEquals("TextBox should initially be assigned", "Splatty", textBox.Text);
			AssertEquals("DummyBizO should have blank text", "", TestBizO.Collection[TestGrid.EditItemIndex].Z0_VarCharMax);

			TestGrid.BindControlChangesToDataSource(textBox);

			AssertEquals("The datagrid should be editing row 2", 2, TestGrid.EditItemIndex);
			AssertEquals("DummyBizO should be updated", "Splatty", TestBizO.Collection[TestGrid.EditItemIndex].Z0_VarCharMax);
		}
		#endregion

		#region TestEdittingItemWithNonPersistentObjectSupportClone

		class DummyNonPersistentBusinessObjectForTestCollection : NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObjectSupportClone>
		{
			public DummyNonPersistentBusinessObjectForTestCollection(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new DummyNonPersistentBusinessObjectSupportClone();
			}
		}

		class DummyNonPersistentBusinessObjectSupportClone : NonPersistentBusinessObject
		{
			public ZString Z0_Code
			{
				get; set;
			}

			public ZPropertyInfo Z0_CodeInfo
			{
				get { return GetZPropertyInfo(nameof(Z0_Code)); }
			}

			protected override bool SupportsCloneCore()
			{
				return true;
			}

			protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			{
				var result = new DummyNonPersistentBusinessObjectSupportClone();
				result.Z0_Code = Z0_Code;
				return result;
			}
		}

		public void TestEdittingItemWithNonPersistentObjectSupportClone()
		{
			TestGrid.AllowPaging = false;

			var dataSource = new DummyNonPersistentBusinessObjectForTestCollection(Factory)
				{
					new DummyNonPersistentBusinessObjectSupportClone() { Z0_Code = "0" },
					new DummyNonPersistentBusinessObjectSupportClone() { Z0_Code = "1" },
					new DummyNonPersistentBusinessObjectSupportClone() { Z0_Code = "2" },
					new DummyNonPersistentBusinessObjectSupportClone() { Z0_Code = "3" },
					new DummyNonPersistentBusinessObjectSupportClone() { Z0_Code = "4" },
					new DummyNonPersistentBusinessObjectSupportClone() { Z0_Code = "5" },
					new DummyNonPersistentBusinessObjectSupportClone() { Z0_Code = "6" },
					new DummyNonPersistentBusinessObjectSupportClone() { Z0_Code = "7" },
					new DummyNonPersistentBusinessObjectSupportClone() { Z0_Code = "8" },
					new DummyNonPersistentBusinessObjectSupportClone() { Z0_Code = "9" }
				};

			AssertEquals("Collection.Count should be 10", 10, dataSource.Count);

			TestGrid.Columns.Add(new ZTextEditColumn("Text", DummyBizoSchema.Z0_Code.Name));
			TestGrid.BindTo = "";
			TestGrid.Bind(dataSource);
			AssertEquals("TestGrid should have 10 Items", 10, TestGrid.Items.Count);

			EditGridWithNonPersistentBusinessObject(2, dataSource, "2", "Not a Number");

			AssertEquals("The datagrid should be editing row2", 2, TestGrid.EditItemIndex);
			AssertEquals("The row4 should be 4", "4", dataSource[4].Z0_Code);

			EditGridWithNonPersistentBusinessObject(4, dataSource, "4", "Not a Number plus");

			AssertEquals("Grid should now be editing row4", 4, TestGrid.EditItemIndex);
			AssertEquals("Data collection should have been updated for row2", "Not a Number", dataSource[2].Z0_Code);

			TestGrid.OnEditCommandInternal(new DataGridCommandEventArgs(TestGrid.Items[4], null, new CommandEventArgs("Edit", null)));

			var textBox = TestGrid.Items[4].Controls[0].Controls[0] as ZTextBox;
			AssertNotNull("Cell should contain TextBox", textBox);
			AssertEquals("Previous value should have been discarded when we select edit on an row that is in edit mode", "4", textBox.Text);
			AssertEquals("Should still be editing row4", 4, TestGrid.EditItemIndex);
		}

		void EditGridWithNonPersistentBusinessObject(int dataGridRowToPlaceInEditMode, DummyNonPersistentBusinessObjectForTestCollection dataSource, string initText, string editText)
		{
			TestGrid.AllowEdit = true;
			TestGrid.OnEditCommandInternal(new DataGridCommandEventArgs(TestGrid.Items[dataGridRowToPlaceInEditMode], null, new CommandEventArgs("Edit", null)));

			AssertEquals("EditItemIndex", dataGridRowToPlaceInEditMode, TestGrid.EditItemIndex);
			AssertEquals("Editable Cell", 1, TestGrid.Items[dataGridRowToPlaceInEditMode].Controls.Count);
			AssertEquals("Editable Control", 1, TestGrid.Items[dataGridRowToPlaceInEditMode].Controls[0].Controls.Count);

			var textBox = TestGrid.Items[dataGridRowToPlaceInEditMode].Controls[0].Controls[0] as ZTextBox;

			AssertNotNull("Cell should contain TextBox", textBox);
			AssertEquals("TextBox should initially be " + initText, initText, textBox.Text);
			AssertEquals("DummyBizO should have value " + initText, initText, dataSource[dataGridRowToPlaceInEditMode].Z0_Code);
			AssertNotNull("BusinessObjectForRollback should be not null", TestGrid.BusinessObjectForRollback);

			var savedBizO = TestGrid.BusinessObjectForRollback as DummyNonPersistentBusinessObjectSupportClone;

			AssertNotNull("BusinessObjectForRollback should be a DummyNonPersistentBusinessObjectSupportClone", savedBizO);
			AssertEquals("BusinessObjectForRollback should be a correct clone", dataSource[dataGridRowToPlaceInEditMode].Z0_Code, savedBizO.Z0_Code);

			textBox.Text = editText;
			textBox.HasChanges = true;
		}

		#endregion

		#region TestAddFetchHints

		ZDataGrid PrepareForFetchHintsTest(OrgHeaderCollection orgs, bool allowPaging)
		{
			ZDataGrid testGrid = new ZDataGrid { PageSize = 5, AllowPaging = allowPaging };

			testGrid.Columns.Add(new ZCodeFindBoxColumn("Port", OrgHeaderSchema.OH_RL_NKClosestPort.Name, "Lookups+ClosestPorts"));
			testGrid.Columns.Add(new ZTextEditColumn("MainAddress", "MainAddress+OA_Address1"));

			ZQuery maxFilter = new ZQuery { MaximumRows = 10 };
			orgs.Load(maxFilter);

			AssertEquals("PreCondition: LoadedFetchHintCount for UNLOCO should be zero", 0, Factory.GetLoadedFetchHintCountForTable(RefUNLOCOSchema.Constants.TableName));
			AssertEquals("PreCondition: LoadedFetchHintCount for OrgAddress should be zero", 0, Factory.GetLoadedFetchHintCountForTable(OrgAddressSchema.Constants.TableName));
			return testGrid;
		}

		public void TestAddFetchHints()
		{
			ReleaseFactory();
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var orgs = new OrgHeaderCollection(Factory);
				var testGrid = PrepareForFetchHintsTest(orgs, true);
				RowFactory.ResetCacheAfterDbUpgrade();
				testGrid.Bind(orgs);

				AssertEquals("Only FetchHints for current page should be loaded", GetUNLOCOsAmountToBeFetched(orgs, 5), Factory.GetLoadedFetchHintCountForTable(RefUNLOCOSchema.Constants.TableName));
				AssertEquals("Only FetchHints for current page should be loaded", 5, Factory.GetLoadedFetchHintCountForTable(OrgAddressSchema.Constants.TableName));
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
				Factory.ClearLoadedFetchHintCountForTable(RefUNLOCOSchema.Constants.TableName);
				Factory.ClearLoadedFetchHintCountForTable(OrgAddressSchema.Constants.TableName);
			}
		}

		public void TestAddFetchHints_NoPaging()
		{
			ReleaseFactory();
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var orgs = new OrgHeaderCollection(Factory);
				var testGrid = PrepareForFetchHintsTest(orgs, false);
				RowFactory.ResetCacheAfterDbUpgrade();
				testGrid.Bind(orgs);

				AssertEquals("All FetchHints for collection should be loaded", GetUNLOCOsAmountToBeFetched(orgs, 10), Factory.GetLoadedFetchHintCountForTable(RefUNLOCOSchema.Constants.TableName));
				AssertEquals("All FetchHints for collection should be loaded", 10, Factory.GetLoadedFetchHintCountForTable(OrgAddressSchema.Constants.TableName));
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
				Factory.ClearLoadedFetchHintCountForTable(RefUNLOCOSchema.Constants.TableName);
				Factory.ClearLoadedFetchHintCountForTable(OrgAddressSchema.Constants.TableName);
			}
		}

		public void TestAddFetchHints_LastPageEmpty()
		{
			ReleaseFactory();
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var orgs = new OrgHeaderCollection(Factory);
				var testGrid = PrepareForFetchHintsTest(orgs, true);
				testGrid.SetCurrentPageIndex(2);

				RowFactory.ResetCacheAfterDbUpgrade();
				testGrid.Bind(orgs);

				AssertEquals("Should set current page to 0", 0, testGrid.CurrentPageIndex);
				AssertEquals("Only FetchHints for current page should be loaded", GetUNLOCOsAmountToBeFetched(orgs, 5), Factory.GetLoadedFetchHintCountForTable(RefUNLOCOSchema.Constants.TableName));
				AssertEquals("Only FetchHints for current page should be loaded", 5, Factory.GetLoadedFetchHintCountForTable(OrgAddressSchema.Constants.TableName));
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
				Factory.ClearLoadedFetchHintCountForTable(RefUNLOCOSchema.Constants.TableName);
				Factory.ClearLoadedFetchHintCountForTable(OrgAddressSchema.Constants.TableName);
			}
		}

		public void TestAddFetchHintsForExportingToExcel()
		{
			using (RowFactory.SetCachedTables())
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				ReleaseFactory();
				OrgHeaderCollection orgs = new OrgHeaderCollection(Factory);
				ZDataGrid testGrid = PrepareForFetchHintsTest(orgs, true);
				try
				{
					testGrid.DataSource = orgs;
					testGrid.ExportIntoExcel(orgs);
					testGrid.DataBind();

					AssertEquals("All FetchHints should be loaded", GetUNLOCOsAmountToBeFetched(orgs), Factory.GetLoadedFetchHintCountForTable(RefUNLOCOSchema.Constants.TableName));
					AssertEquals("All FetchHints should be loaded", 10, Factory.GetLoadedFetchHintCountForTable(OrgAddressSchema.Constants.TableName));
				}
				finally
				{
					RowFactory.LoadedFetchHintRecordingEnabled = false;
					Factory.ClearLoadedFetchHintCountForTable(RefUNLOCOSchema.Constants.TableName);
					Factory.ClearLoadedFetchHintCountForTable(OrgAddressSchema.Constants.TableName);
				}
			}
		}

		public void TestExportIntoExcelWithCouldNotGetValueForExportException()
		{
			ReleaseFactory();
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var bizObj = Factory.New<DummyBusinessObject>();
				var exportColumn = ExcelExportColumn.New(new SchemaStringColumn(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(bizObj.TableName), "I_Dont_Exist", 99, System.Data.SqlDbType.VarChar, ZString.Empty, true, 100));
				var exportColumns = new List<ExcelExportColumnBase> { exportColumn };
				var collection = new DummyBusinessObjectCollection(Factory) { bizObj };
				var orgs = new OrgHeaderCollection(Factory);
				var testGrid = PrepareForFetchHintsTest(orgs, true);

				AssertNoExceptionThrown(() => testGrid.ExportIntoExcel(collection, exportColumns));
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		public void TestExportIntoExcelWithOverride()
		{
			ReleaseFactory();
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var orgs = new OrgHeaderCollection(Factory);
				var testGrid = PrepareForFetchHintsTest(orgs, true);
				testGrid.DataSource = orgs;

				testGrid.ExportIntoExcel();
				AssertEquals("Loads collection", 10, Factory.GetLoadedFetchHintCountForTable(OrgAddressSchema.Constants.TableName));

				testGrid.GetCollectionToExportOverride = () =>
				{
					var maxFilter = new ZQuery { MaximumRows = 20 };
					orgs.Load(maxFilter);

					return orgs;
				};

				testGrid.ExportIntoExcel();
				AssertEquals("Loads collection to export (when defined)", 20, Factory.GetLoadedFetchHintCountForTable(OrgAddressSchema.Constants.TableName));
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		int GetUNLOCOsAmountToBeFetched(OrgHeaderCollection orgs)
		{
			return GetUNLOCOsAmountToBeFetched(orgs, orgs.Count);
		}

		int GetUNLOCOsAmountToBeFetched(OrgHeaderCollection orgs, int elementsOnPage)
		{
			int result = 0;
			int elementCounter = 0;

			foreach (OrgHeader org in orgs)
			{
				if (!string.IsNullOrEmpty(org.OH_RL_NKClosestPort) && elementCounter < elementsOnPage)
				{
					result++;
				}
				elementCounter++;
			}
			return result;
		}

		#endregion

		#region DummyBusinessObjectWithRefCountryLookups

		public class DummyBusinessObjectWithRefCountryLookup : DummyBusinessObject
		{
			public DummyBusinessObjectWithRefCountryLookup(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyBusinessObjectRefCountryLookup Lookups
			{
				get
				{
					if (fLookups == null)
					{
						fLookups = new DummyBusinessObjectRefCountryLookup(this);
					}
					return fLookups;
				}
			}
			DummyBusinessObjectRefCountryLookup fLookups;
		}

		public class DummyBusinessObjectRefCountryLookup : DummyLookups
		{
			public DummyBusinessObjectRefCountryLookup(DummyBusinessObjectWithRefCountryLookup parent)
				: base(parent)
			{
			}

			public RefCountryCollection Countries
			{
				get
				{
					if (fCountries == null)
					{
						fCountries = new RefCountryCollection(Factory);
					}
					return fCountries;
				}
			}
			RefCountryCollection fCountries;
		}
		#endregion

		#region Test Apply Sort

		public void TestApplySort()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			PopulateCollection(collection);

			TestGrid.OrderBy = "Z0_Date DESC";
			TestGrid.Bind(collection);

			AssertEquals("Collection not sorted by Z0_Date Descending", new ZDateTime(2011, 6, 7), collection[0].Z0_Date);
			AssertEquals("Collection not sorted by Z0_Date Descending", new ZDateTime(2011, 5, 15), collection[1].Z0_Date);
			AssertEquals("Collection not sorted by Z0_Date Descending", new ZDateTime(2011, 3, 12), collection[2].Z0_Date);
			AssertEquals("Collection not sorted by Z0_Date Descending", new ZDateTime(2011, 2, 4), collection[3].Z0_Date);
			AssertEquals("Collection not sorted by Z0_Date Descending", new ZDateTime(2011, 1, 1), collection[4].Z0_Date);

			TestGrid.OrderBy = "Z0_Date ASC";
			TestGrid.Bind(collection);

			AssertEquals("Collection not sorted by Z0_Date Ascending", new ZDateTime(2011, 1, 1), collection[0].Z0_Date);
			AssertEquals("Collection not sorted by Z0_Date Ascending", new ZDateTime(2011, 2, 4), collection[1].Z0_Date);
			AssertEquals("Collection not sorted by Z0_Date Ascending", new ZDateTime(2011, 3, 12), collection[2].Z0_Date);
			AssertEquals("Collection not sorted by Z0_Date Ascending", new ZDateTime(2011, 5, 15), collection[3].Z0_Date);
			AssertEquals("Collection not sorted by Z0_Date Ascending", new ZDateTime(2011, 6, 7), collection[4].Z0_Date);
		}

		public class DummyBusinessObjectCollection : BusinessObjectCollection<DummyBusinessObject>
		{
			public DummyBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		void PopulateCollection(DummyBusinessObjectCollection collection)
		{
			DummyBusinessObject dummy1 = collection.AddNew();
			dummy1.Z0_Code = "2A4";
			dummy1.Z0_Date = new ZDateTime(2011, 1, 1);

			DummyBusinessObject dummy2 = collection.AddNew();
			dummy2.Z0_Code = "06";
			dummy2.Z0_Date = new ZDateTime(2011, 5, 15);

			DummyBusinessObject dummy3 = collection.AddNew();
			dummy3.Z0_Code = "2A5";
			dummy3.Z0_Date = new ZDateTime(2011, 3, 12);

			DummyBusinessObject dummy4 = collection.AddNew();
			dummy4.Z0_Code = "05";
			dummy4.Z0_Date = new ZDateTime(2011, 2, 4);

			DummyBusinessObject dummy5 = collection.AddNew();
			dummy5.Z0_Code = "13";
			dummy5.Z0_Date = new ZDateTime(2011, 6, 7);
		}

		#endregion
	}
}
