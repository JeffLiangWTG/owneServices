using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.Utilities;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[HttpContextEnabledTest]
	[TestsSubclassesOf(typeof(ZFilterGridModule))]
	public abstract class ZFilterGridModuleTest : ZWebModule_Test
	{
		#region Test Cases

		public void TestCollectionLoadDBHitsWithDBOnlyQueryNoResults()
		{
			using (var filterGridModule = ZWebModuleFactory.Create(TestID, new BusinessObjectFactory(), TestPage))
			{
				if (GetShoudTestLoadDBHitsWithDBOnlyQuery(filterGridModule.GridCollection))
				{
					SetupForCollectionLoadDBHitsWithDBOnlyQueryTests();
					AssertNotNull("Module has a valid grid collection", filterGridModule.GridCollection);

					var filter = new ZDBOnlyQuery(GetCollectionElementType());
					SetupDBOnlyQuery(filter);
					Factory.Save();
					filterGridModule.LoadCollection(filterGridModule.CreateNewFilterBusinessObject(), filter);
					AssertEquals("Collection should have 0 results", 0, filterGridModule.GridCollection.Count);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestCollectionLoadDBHitsWithDBOnlyQuery()
		{
			using (var filterGridModule = ZWebModuleFactory.Create(TestID, new BusinessObjectFactory(), TestPage))
			{
				if (GetShoudTestLoadDBHitsWithDBOnlyQuery(filterGridModule.GridCollection))
				{
					SetupForCollectionLoadDBHitsWithDBOnlyQueryTests();
					AssertNotNull("Module has a valid grid collection", filterGridModule.GridCollection);

					var expectedObjects = GetNewBusinessObjectsExpectedFromFilter();
					var unexpectedObjects = GetNewBusinessObjectsUnexpectedFromFilter();

					Assert("Should have objects expected in the collection after loading with filter", expectedObjects.Count > 1);
					Assert("Should have objects unexpected in the collection after loading with filter", unexpectedObjects.Count > 1);
					Factory.Save();

					var filter = new ZDBOnlyQuery(GetCollectionElementType());
					var filterObject = filterGridModule.CreateNewFilterBusinessObject();
					string testTableName = GetTableName();
					SetupDBOnlyQuery(filter);
					Assert("Collection must have non-empty filter for grid collection", !filter.IsEmpty);
					filterGridModule.GridCollection.Factory.ResetDatabaseLoadCount();
					filterGridModule.LoadCollection(filterObject, filter);
					AssertEquals(string.Format("Collection should have {0} result", GetExpectedCollectionResults()), GetExpectedCollectionResults(), filterGridModule.GridCollection.Count);
					if (ExpectDBHits)
					{
						AssertEquals("Should hit database only once for " + testTableName, 1, filterGridModule.GridCollection.Factory.GetTableHitCount(testTableName));
					}
				}
				else
				{
					Assert(true);
				}
			}
		}

		protected virtual bool ExpectDBHits => true;

		protected virtual int GetExpectedCollectionResults() => 9;

		protected virtual string GetTableName() => FilterGridModule.GridCollection.TableName;

		protected virtual Type GetCollectionElementType() => FilterGridModule.GridCollection.TypeOfElements;

		protected virtual void SetupForCollectionLoadDBHitsWithDBOnlyQueryTests()
		{
		}

		protected OrgContactWebUser SiteUser => (OrgContactWebUser)WebEnv.AppInstance.SiteUser;

		protected virtual bool GetShoudTestLoadDBHitsWithDBOnlyQuery(IBusinessObjectCollection collection) { return true; }

		protected virtual List<BusinessObject> GetNewBusinessObjectsExpectedFromFilter() => new List<BusinessObject>();

		protected virtual List<BusinessObject> GetNewBusinessObjectsUnexpectedFromFilter() => new List<BusinessObject>();

		protected virtual void SetupDBOnlyQuery(ZDBOnlyQuery filter)
		{
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			Globals.IsWeb = true;
			TestHelper = GetNewHelper();
			TestPage = GetNewTestPage();

			Factory.Save();
			base.SetUp();
			TestHelper.TestSiteUser.Login(TestHelper.TestOrg.OH_Code, TestHelper.TestContact.OC_Email, TestHelper.TestContact.PasswordForTesting);
		}

		protected virtual ZPage GetNewTestPage()
		{
			var page = new ZTestPage();
			page.SetSiteUser(TestHelper.TestSiteUser);

			return page;
		}

		protected virtual ZWebTestHelper GetNewHelper() => new ZWebTestHelper(Factory);

		protected ZWebTestHelper TestHelper;

		protected override void TearDown()
		{
			if (TestPage != null)
			{
				TestPage.Dispose();
			}
			Globals.IsWeb = false;
			base.TearDown();
		}

		protected ZFilterGridModule FilterGridModule => TestZWebModule as ZFilterGridModule;

		protected override ZWebModule GetNewZWebModule() => ZWebModuleFactory.Create(TestID, Factory, TestPage);

		protected ZPage TestPage;

		public abstract FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault();

		protected FilterBusinessObjectDefault[] ArrExpectedFilterBusinessObjectDefault => arrExpectedFilterBusinessObjectDefault ?? (arrExpectedFilterBusinessObjectDefault = GetArrExpectedFilterBusinessObjectDefault());

		FilterBusinessObjectDefault[] arrExpectedFilterBusinessObjectDefault;

		#endregion Setup

		#region BusinessObject/Filter/Collection Tests

		#region TestActiveStatusFilter

		public void TestActiveStatusFilter()
		{
			if (AllowActiveStatusFilterTest())
			{
				SetupForActiveStatusFilterTest();

				var expectedActiveElements = GetNewFilterGridCollection();
				var expectedInactiveElements = GetNewFilterGridCollection();

				SetupAndGetExpectedElementsWithActiveStatus(expectedActiveElements, FilterGridModule.GetCancellableCollectionElementTypeInternal());
				SetupAndGetExpectedElementsWithInactiveStatus(expectedInactiveElements, FilterGridModule.GetCancellableCollectionElementTypeInternal());
				Factory.Save();

				BeforeCollectionLoadForActiveStatusTest();
				var testFilterBusinessObject = FilterGridModule.CreateNewFilterBusinessObject();
				var filter = testFilterBusinessObject.Filter;
				AddAdditionalFilterForActiveStatusTest(filter);
				FilterGridModule.LoadCollection(FilterGridModule.CreateNewFilterBusinessObject(), filter);
				if (CanHaveInactiveElements(FilterGridModule.GetCancellableCollectionElementTypeInternal()))
				{
					AssertEquals(expectedActiveElements.Count, FilterGridModule.GridCollection.Count);
				}
				else
				{
					AssertEquals(expectedActiveElements.Count + expectedInactiveElements.Count, FilterGridModule.GridCollection.Count);
				}
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual void BeforeCollectionLoadForActiveStatusTest() => FilterGridModule.ResetGridCollection();

		protected virtual void AddAdditionalFilterForActiveStatusTest(ZQuery filter)
		{
		}

		protected virtual IList GetNewFilterGridCollection() => FilterGridModule.GetNewCollection(Factory);

		protected virtual bool AllowActiveStatusFilterTest() => true;

		protected virtual void SetupForActiveStatusFilterTest()
		{
		}

		protected virtual bool CanHaveInactiveElements(Type elementType) => typeof(ICancellable).IsAssignableFrom(elementType);

		protected virtual void SetupAndGetExpectedElementsWithActiveStatus(IList collection, Type collectionElementType)
		{
			collection.Add(GetNewElement(collectionElementType, false));
			collection.Add(GetNewElement(collectionElementType, false));
			collection.Add(GetNewElement(collectionElementType, false));
			collection.Add(GetNewElement(collectionElementType, false));
			collection.Add(GetNewElement(collectionElementType, false));
		}

		protected virtual void SetupAndGetExpectedElementsWithInactiveStatus(IList collection, Type collectionElementType)
		{
			collection.Add(GetNewElement(collectionElementType, true));
			collection.Add(GetNewElement(collectionElementType, true));
			collection.Add(GetNewElement(collectionElementType, true));
			collection.Add(GetNewElement(collectionElementType, true));
			collection.Add(GetNewElement(collectionElementType, true));
		}

		protected virtual BusinessObject GetNewElement(Type elementType, bool isCancelled)
		{
			var element = Factory.NewWithValidTestData(elementType);
			var cancellable = element as ICancellable;
			if (cancellable != null)
			{
				cancellable.IsCancelled = isCancelled;
			}

			return element;
		}

		#endregion

		public void TestZFilterBusinessObjectFactory()
		{
			AssertNotNull(FilterGridModule.FilterFactoryInternal);
		}

		public void TestGridCollectionType()
		{
			Assert(typeof(IBusinessObjectCollection).IsAssignableFrom(FilterGridModule.GridCollectionType));
			AssertEquals(FilterGridModule.GridCollectionType, FilterGridModule.GridCollection.GetType());
		}

		public void TestFilterBusinessObjectType()
		{
			TestFilterBusinessObjectTypeCore();
		}

		protected virtual void TestFilterBusinessObjectTypeCore()
		{
			Assert(FilterGridModule.FilterBusinessObjectType.IsSubclassOf(typeof(FilterBusinessObject)));
			AssertEquals(FilterGridModule.FilterBusinessObjectType, FilterGridModule.CreateNewFilterBusinessObject().GetType());
		}

		[ExpectNoExceptions]
		public void TestGetNewFilterBusinessObjectDefaultsDoesNotThrowExceptionWhenWebUserIsNotLoggedIn()
		{
			TestPage.SiteUser.Logout();
			AssertEquals("SiteUser is not LoggedIn", false, TestPage.SiteUser.IsLoggedIn);
			AssertNotNull(FilterGridModule.GetNewFilterBusinessObjectDefaults_Internal());
		}

		public void TestGridCollection()
		{
			AssertNotNull(FilterGridModule.GridCollection);
		}

		public void TestCollectionCaching()
		{
			ZTestFilterPage page;
			var mock = new Mock<ZTestFilterPage>() { CallBase = true };
			mock.Protected()
				.Setup<ZString>("GetModuleID")
				.Returns(new ZString(TestID.ToString()));
			mock.Protected()
				.Setup<Uri>("RequestUrl")
				.Returns(RequestUrl);
			mock.Protected()
				.Setup<BrowserType>("GetBrowserType")
				.Returns(BrowserType.IE);
			mock.Protected()
				.Setup<SearchControl>("GetNewSearchControl")
				.Returns(GetNewSearchControl());
			var qs = new NameValueCollection();
			qs.Add(ZIFramePage.OKFunctionQuery, "ZTextPopup_SetValueAndHidePopup");
			qs.Add(ZIFramePage.CancelFunctionQuery, "ZTextPopup_HidePopup");
			qs.Add(ZIFramePage.ControlIDQuery, "TestClientID");
			mock.Protected()
				.Setup<NameValueCollection>("RequestQueryString")
				.Returns(qs);
			ZGuid[] cachedPKs;
			using (page = mock.Object)
			{
				using (var tmp = new TempDirectory())
				{
					page.SetServerMappedPathForTest(tmp.DirectoryName);
					TestLoadCollectionInternal(page);
					page.OnInit(EventArgs.Empty);
					page.OnLoad(EventArgs.Empty);
					page.FindButton_Click(this, EventArgs.Empty);

					cachedPKs = page.SearchControl.ViewState["ModuleGridCollectionCachedPKs"] as ZGuid[];
					var filterGridModule = page.SearchControl.Module;
					AssertNotNull("Should be a ZFilterGridModule", filterGridModule);
					if (filterGridModule.CacheCollectionPKs)
					{
						AssertNotNull("CachedPKs", cachedPKs);
						AssertNotNull("FilterGridModule", filterGridModule);
						AssertEquals("SearchControl should have cached results", filterGridModule.GridCollection.Count, cachedPKs.Length);
					}
					else
					{
						AssertNull("CachedPKs should be null as no caching is being done", cachedPKs);
					}
				}
			}

			mock.VerifyAll();
		}

		public void TestShouldCacheCollectionKeys()
		{
			AssertEquals("Module caching differs to expected", ExpectCachingOfCollectionKeys, FilterGridModule.CacheCollectionPKs);
		}

		protected virtual bool ExpectCachingOfCollectionKeys => true;

		[ExpectNoExceptions("Failed to load collection")]
		public void TestLoadCollection()
		{
			ZTestFilterPage page;
			var mock = new Mock<ZTestFilterPage>() { CallBase = true };
			mock.Protected()
				.Setup<ZString>("GetModuleID")
				.Returns(new ZString(TestID.ToString()));
			mock.Protected()
				.Setup<Uri>("RequestUrl")
				.Returns(RequestUrl);
			mock.Protected()
				.Setup<BrowserType>("GetBrowserType")
				.Returns(BrowserType.IE);
			mock.Protected()
				.Setup<SearchControl>("GetNewSearchControl")
				.Returns(GetNewSearchControl());
			var qs = new NameValueCollection();
			qs.Add(ZIFramePage.OKFunctionQuery, "ZTextPopup_SetValueAndHidePopup");
			qs.Add(ZIFramePage.CancelFunctionQuery, "ZTextPopup_HidePopup");
			qs.Add(ZIFramePage.ControlIDQuery, "TestClientID");
			mock.Protected()
				.Setup<NameValueCollection>("RequestQueryString")
				.Returns(qs);
			using (page = mock.Object)
			{
				using (var tmp = new TempDirectory())
				{
					page.SetServerMappedPathForTest(tmp.DirectoryName);
					TestLoadCollectionInternal(page);
					page.OnInit(EventArgs.Empty);
					page.OnLoad(EventArgs.Empty);
					page.FindButton_Click(this, EventArgs.Empty);
				}
			}

			mock.VerifyAll();
		}

		protected virtual void TestLoadCollectionInternal(ZFilterPage page)
		{
		}

		protected virtual Uri RequestUrl => new Uri("http://www.test.com/ediWeb/ZTestFilterPage.aspx");

		protected TestSearchControl GetNewSearchControl()
		{
			var search = new TestSearchControl();
#if DEBUG
			search.SetModuleForTest((ZFilterGridModule)TestZWebModule);
#endif

			return search;
		}

		public void TestFilterBusinessObject() => TestFilterBusinessObjectCore();

		protected virtual void TestFilterBusinessObjectCore()
		{
			var filterBizO = FilterGridModule.CreateNewFilterBusinessObject();
			AssertNotNull(filterBizO);
			var expected = FilterGridModule.FilterFactoryInternal.New(FilterGridModule.FilterBusinessObjectType);
			expected.SetExternalDefaults(FilterGridModule.FilterBusinessObjectDefaultsInternal);
			AssertEquals(expected.Filter.LiteralTextADO, filterBizO.Filter.LiteralTextADO);
		}

		public void TestFilterBusinessObjectDefaults() => TestFilterBusinessObjectDefaultsCore();

		protected virtual void TestFilterBusinessObjectDefaultsCore()
		{
			var itemCount = 0;
			var @enum = ((IEnumerable)FilterGridModule.FilterBusinessObjectDefaultsInternal).GetEnumerator();
			while (@enum.MoveNext())
			{
				itemCount++;
			}

			AssertEquals(ArrExpectedFilterBusinessObjectDefault.Length, itemCount);
			foreach (var @default in ArrExpectedFilterBusinessObjectDefault)
			{
				var key = @default.FilterName.IsEmpty ? @default.PropertyName.ToString() : @default.FilterName + ":" + @default.PropertyName;
				var value = @default.Value;
				AssertEquals("Default for " + key, value, FilterGridModule.FilterBusinessObjectDefaultsInternal[key].Value);
			}
		}

		public void TestFilterControlResource()
		{
			AssertNotNull(FilterGridModule.FilterControlResource);

			using (var tmp = new TempDirectory())
			{
				FilterGridModule.FilterControlResource.ServerMappedPathForTesting = tmp.DirectoryName;
				FilterGridModule.FilterControlResource.Extract();
			}
		}

		public virtual void TestGridColumns()
		{
			AssertNotNull(FilterGridModule.GridColumnFields);
			Assert("There should be at least one ", FilterGridModule.GridColumnFields.Length > 0);
		}

		public virtual void TestDefaultGridColumns()
		{
			AssertNotNull(FilterGridModule.DefaultGridColumnFields);
			AssertArrayEqualsByElements("Default Columns", ExpectedDefaultGridColumns, FilterGridModule.DefaultGridColumnFields);
			Assert("There should be at least one default column", FilterGridModule.DefaultGridColumnFields.Length > 0);
			AssertNoGroupMemberColumnsInCollection(FilterGridModule.GridColumnFields, FilterGridModule.DefaultGridColumnFields);
		}

		public virtual void TestRequiredGridColumns()
		{
			AssertNotNull(FilterGridModule.RequiredGridColumnFields);
			AssertArrayEqualsByElements("RequiredColumns", ExpectedRequiredGridColumns, FilterGridModule.RequiredGridColumnFields);
			Assert("There should be at least one required column", FilterGridModule.RequiredGridColumnFields.Length > 0);
			AssertNoGroupMemberColumnsInCollection(FilterGridModule.GridColumnFields, FilterGridModule.RequiredGridColumnFields);
		}

		#region TestBindableProperties

		public virtual void TestAllGridColumnsAreExportableToExcel()
		{
			if (FilterGridModule.GridCollection.AllowNew)
			{
				FillCollectionWithAtLeastOneElement();
				Assert("Precondition", FilterGridModule.GridCollection.Count > 0);
				var log = new ZStringBuilder();
				foreach (var column in FilterGridModule.GridColumnFields)
				{
					var templateColumn = column as ZTemplateColumn;
					if (templateColumn != null && !ExcludeFromExcelExportColumnsBoundTo.Contains(templateColumn.BindTo))
					{
						var collectionOfOne = new DataGridColumnCollection(new ZDataGrid(), new ArrayList { column });
						var helper = new DataGridExcelExportHelper(FilterGridModule.GridCollection, collectionOfOne);
						if ((helper.GetExcelExportColumns().Count != 1 && !(column is ZGroupColumn)) || (column is ZGroupColumn && helper.GetExcelExportColumns().Count != ((ZGroupColumn)column).GroupMembers.Length))
						{
							log.Append(String.Format("Should be an Excel Export column for '{0}' column.", column.HeaderText));
						}
					}
				}
				var msg = (ZString)log.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
				Assert("Failed for next reasons:" + System.Environment.NewLine + msg, msg.IsEmpty);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual List<string> ExcludeFromExcelExportColumnsBoundTo => new List<string>();

		[ExpectNoExceptions]
		protected virtual void FillCollectionWithAtLeastOneElement()
		{
			if (FilterGridModule.GridCollection.AllowNew)
			{
				try
				{
					FilterGridModule.GridCollection.AddNew();
				}
				catch (NoConcreteTypeException)
				{
					Assert(
						string.Format("Type of elements is abstract. Please override 'FillCollectionWithOneElement()' method in {0}",
									  ToString().Substring(0, ToString().LastIndexOf('.'))),
						!FilterGridModule.GridCollection.TypeOfElements.IsAbstract);
					throw;
				}
				catch (NotSupportedException)
				{
					Assert(
						string.Format("Collection doesn't allow adding. Please override 'FillCollectionWithOneElement()' method in {0}",
									  ToString().Substring(0, ToString().LastIndexOf('.'))),
						FilterGridModule.GridCollection.AllowNew);
					throw;
				}
			}
		}

		public virtual void TestAllBindablePropertiesHavePropertyInfo()
		{
			if (FilterGridModule.GridCollection.AllowNew)
			{
				FillCollectionWithAtLeastOneElement();
				Assert("Precondition", FilterGridModule.GridCollection.Count > 0);
				var log = new ZStringBuilder();
				foreach (var column in FilterGridModule.GridColumnFields)
				{
					if (column is ZGroupColumn groupColumn)
					{
						foreach (DataGridColumn innerColumn in groupColumn.GroupMembers)
						{
							AssertBindability(TypeDescriptor.GetProperties(FilterGridModule.GridCollection[0])[((ZTemplateColumn)innerColumn).BindTo], log);
						}
					}
					else
					{
						if (column is ZTemplateColumn templateColumn)
						{
							AssertBindability(TypeDescriptor.GetProperties(FilterGridModule.GridCollection[0])[templateColumn.BindTo], log);
						}
					}
				}

				ZString msg = log.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
				Assert("Failed for next reasons:" + msg, msg.IsEmpty);
			}
			else
			{
				Assert(true);
			}
		}

		void AssertBindability(PropertyDescriptor descriptor, ZStringBuilder log)
		{
			AssertNotNull(descriptor);

			if (descriptor is WrappingPropertyDescriptor propertyDescriptor)
			{
				AssertBindability(propertyDescriptor.Inner, log);
			}
			else
			{
				var bizO = GetNewBizObjOfType(descriptor.ComponentType);
				Assert(string.Format("Cannot get an instance of BusinessObject for the following Type: {0}.\r\nPlease override the GetNewBizObjOfType test's method to provide an instance of this type.", descriptor.ComponentType.FullName), bizO != null);
				var bindTo = descriptor.Name;
				var typeName = descriptor.ComponentType.FullName;

				if (!(bizO.ZPropertyInfoHash.ContainsKey(bindTo) || bizO[descriptor.Name] is IBusinessObjectCollection))
				{
					log.Append(string.Format("Property for {0} does not exist on object of type {1}", bindTo, typeName));
				}
			}
		}

		#endregion

		void AssertNoGroupMemberColumnsInCollection(DataGridColumn[] allDataGridColumns, DataGridColumn[] collectionToSearch)
		{
			foreach (var column in allDataGridColumns)
			{
				if (column is ZGroupColumn groupColumn)
				{
					foreach (var columnToCheck in collectionToSearch)
					{
						if (((IList)groupColumn.GroupMembers).Contains(columnToCheck))
						{
							Fail("Group member columns are not allowed to be Default/Required");
						}
					}
				}
			}
		}

		protected virtual DataGridColumn[] ExpectedDefaultGridColumns => FilterGridModule.GridColumnFields;

		protected virtual DataGridColumn[] ExpectedRequiredGridColumns => new DataGridColumn[] { FilterGridModule.SelectionColumn };

		#endregion BusinessObject/Filter/Collection Tests

		#region Ordering Tests

		protected virtual ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(DummyBizoSchema.Z0_Code.Name, ListSortDirection.Ascending) };

		public virtual void TestGetSortInfos()
		{
			var sortInfos = FilterGridModule.GetSortInfos(FilterGridModule.CreateNewFilterBusinessObject());
			AssertSortInfos(ExpectedSortInfos, sortInfos);
		}

		protected void AssertSortInfos(ColumnAndSortOrder[] expectedSortInfos, ColumnAndSortOrder[] actualSortInfos)
		{
			AssertEquals("The count of sort infos are not equal", expectedSortInfos.Length, actualSortInfos.Length);
			for (var i = 0; i < ExpectedSortInfos.Length; i++)
			{
				AssertEquals("OrderByColumnName", expectedSortInfos[i].OrderByColumnName, actualSortInfos[i].OrderByColumnName);
				AssertEquals("SortDirection", expectedSortInfos[i].SortDirection, actualSortInfos[i].SortDirection);
			}
		}

		protected virtual ListSortDirection ExpectedDefaultSortOrder => ListSortDirection.Ascending;

		public void TestDefaultSortOrder()
		{
			AssertEquals("Default Sort Order", ExpectedDefaultSortOrder, FilterGridModule.DefaultSortOrder);
		}

		#endregion Ordering Tests

		public void TestSiteUserNull()
		{
			//setting AppInstance to Null
			TestPage.IsCreateNewAppInstanceIfNullForTest = false;
			AssertNull(TestPage.AppInstance);
			AssertNotNull("Should not throw exceptions when site user is null", FilterGridModule.GetNewFilterBusinessObjectDefaults_Internal());

			TestPage = null;
			AssertNotNull("Should not throw exceptions when page is null", FilterGridModule.GetNewFilterBusinessObjectDefaults_Internal());
		}

		public virtual void TestSelectionColumnIsZHyperLinkColumn()
		{
			var column = FilterGridModule.SelectionColumn;
			AssertNotNull("SelectionColumn should be not null", column);

			Assert("SelectionColumn should be in All Columns", ((IList)FilterGridModule.GridColumnFields).Contains(column));
			Assert("SelectionColumn should be in Required Columns", ((IList)FilterGridModule.RequiredGridColumnFields).Contains(column));

			if (GridHasHyperLinkColumn)
			{
				Assert("selected column should be ZHyperlinkColumn or ZButtonColumn or ZLinkButtonColumn", column is ZHyperLinkColumn || column is ZButtonColumn || column is ZLinkButtonColumn);
			}

			var buttonColumn = column as ZButtonColumn;
			if (buttonColumn != null)
			{
				AssertEquals("Select command should be specified", "Select", buttonColumn.CommandName);
			}
		}

		public virtual bool GridHasHyperLinkColumn => true;

		public virtual void TestLoadCollectionReturnsRowCount()
		{
			var filterBizO = FilterGridModule.CreateNewFilterBusinessObject();

			AssertEquals("FilterGridModule should limit to 1000 rows by default", 1000, FilterGridModule.MaxRows);
			FilterGridModule.MaxRows = 250;
			FilterGridModule.LoadCollection(filterBizO);
			if (FilterGridModule.GridCollection.TypeOfElements.IsClass && !FilterGridModule.GridCollection.TypeOfElements.IsSubclassOf(typeof(NonPersistentBusinessObject)))
			{
				var expectedCount = Factory.GetDatabaseCount(FilterGridModule.GridCollection.TypeOfElements, GetFilterForExpectedCount(FilterGridModule, filterBizO));
				expectedCount = (expectedCount > FilterGridModule.MaxRows) ? FilterGridModule.MaxRows : expectedCount;
				AssertEquals("LoadCollection should not have returned more than 250 rows", expectedCount, FilterGridModule.GridCollection.Count);
			}
			else
			{
				Assert("LoadCollection should not have returned more than 250 rows", FilterGridModule.GridCollection.Count <= 250);
			}
		}

		protected virtual ZQuery GetFilterForExpectedCount(ZFilterGridModule module, FilterBusinessObject filterBizO) => filterBizO.Filter;

		public void TestLoadExcelCollection()
		{
			SetupForExcelExport();
			using (WebDataRegistry.Instance.MaxFilteredRecords.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			using (WebDataRegistry.Instance.MaxFilteredRecordsForExportToExcel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				var filterBizO = GetFilterObjectForExcelExport();

				CreateNewElementForExcelExport();
				CreateNewElementForExcelExport();
				CreateNewElementForExcelExport();
				CreateNewElementForExcelExport();
				CreateNewElementForExcelExport();
				CreateNewElementForExcelExport();
				Factory.Save();

				AssertEquals("Grid should have a limit of 3 rows", 3, FilterGridModule.MaxRows);
				AssertEquals("Grid Collection should have 3 rows", 3, FilterGridModule.LoadCollection(filterBizO));
				AssertEquals("Collection for Excel export should have 5 rows", 5, FilterGridModule.LoadExcelCollection(filterBizO).Count);
			}
		}

		protected virtual void SetupForExcelExport() { }

		protected virtual FilterBusinessObject GetFilterObjectForExcelExport() => FilterGridModule.CreateNewFilterBusinessObject();

		protected virtual BusinessObject CreateNewElementForExcelExport()
		{
			var elementType = GetElementTypeForExcelExport();
			var element = GetNewElement(elementType, false);
			foreach (FilterBusinessObjectDefault defaultInfo in FilterGridModule.FilterBusinessObjectDefaultsInternal)
			{
				var property = element.FindPropertyInfo(defaultInfo.PropertyName);
				if (property != null)
				{
					property.Value = defaultInfo.Value;
				}
			}

			return element;
		}

		protected virtual Type GetElementTypeForExcelExport() => FilterGridModule.GetCancellableCollectionElementTypeInternal();

		public virtual void TestGetBusinessObjectPKColumn()
		{
			var filterBizO = FilterGridModule.CreateNewFilterBusinessObject();
			var pKColumn = FilterGridModule.GetBusinessObjectPKColumn(filterBizO);
			Assert("GetBusinessObjectPKColumn should be Zguid Type", pKColumn is SchemaGuidColumn || pKColumn is SchemaPKColumn);
		}

		public void TestGetCollectionSorterForSuppressedFields()
		{
			AssertEquals("Should return normal sorter", ExpectedCollectionSorterType, FilterGridModule.GetNewCollectionSorter("OH_Code", ListSortDirection.Ascending).GetType());
			AssertEquals("Should return normal sorter", ExpectedCollectionSorterType, FilterGridModule.GetNewCollectionSorter("*SUPPRESSED*OH_Code", ListSortDirection.Ascending).GetType());
		}

		protected virtual Type ExpectedCollectionSorterType => typeof(WebCollectionSorter);

		protected virtual BusinessObject GetNewBizObjOfType(Type type)
		{
			BusinessObject result = null;
			if (!type.IsSubclassOf(typeof(NonPersistentBusinessObject)))
			{
				result = Factory.New(type);
			}

			return result;
		}

		public virtual void TestTranslatability()
		{
			if (GetType().Namespace.StartsWith("Enterprise.Client") || GetType().Namespace.StartsWith("Enterprise.ZClient"))
			{
				Assert(true);

				return;
			}
			CombineAssertions(delegate
			{
				const string hao = "好";
				using (var mockRes = Res.UseMockData())
				{
					mockRes.SetResourceGetter(delegate(string key) { return new ResourceStringData(key, hao); });
					foreach (var gridColumnField in FilterGridModule.GridColumnFields)
					{
						AssertContains("Grid column field \"" + gridColumnField.HeaderText + "\" is not translatable", hao, gridColumnField.HeaderText);
					}
				}
			});
		}
	}
}
