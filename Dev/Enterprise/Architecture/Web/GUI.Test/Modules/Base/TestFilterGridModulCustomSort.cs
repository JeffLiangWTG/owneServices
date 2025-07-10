using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules.Testing;

namespace Enterprise.ZArchitecture.Web.Modules.TestFilterGridModulCustomSorter.Testing
{
	[HttpContextEnabledTest]
	public class TestFilterGridModulCustomSort : ZWebModule_Test
	{
		public void TestCostomSorter()
		{
			using (var testModulForCustomSorter = new ZFilterGridModuleForTest(Factory, new ZPage()))
			{
				AssertEquals("Must be created 3 columns", 3, testModulForCustomSorter.GridColumnFields.Length);

				Assert("Should start with CustomSorterSupport keyword", testModulForCustomSorter.GridColumnFields[0].SortExpression.Equals("Z0_Code"));
				var sorter1 = testModulForCustomSorter.GetNewCollectionSorter(testModulForCustomSorter.GridColumnFields[0].SortExpression, ListSortDirection.Ascending);
				AssertEquals("GetNewCollectionSorter must be return constructor " + typeof(WebCollectionSorter) + " type", typeof(WebCollectionSorter), sorter1.GetType());

				Assert("Should start with CustomSorterSupport keyword", testModulForCustomSorter.GridColumnFields[1].SortExpression.StartsWith(CustomSorterSupportConst.IsCustomSorter));
				var sorter2 = testModulForCustomSorter.GetNewCollectionSorter(testModulForCustomSorter.GridColumnFields[1].SortExpression, ListSortDirection.Ascending);
				AssertEquals("GetNewCollectionSorter must be return constructor " + typeof(CustomSorter) + " type", typeof(CustomSorter), sorter2.GetType());
			}
		}

		public void TestCustomSorterThrowException()
		{
			using (var testModulForCustomSorter = new ZFilterGridModuleForTest(Factory, new ZPage()))
			{
				try
				{
					var badSorter = testModulForCustomSorter.GetNewCollectionSorter(testModulForCustomSorter.GridColumnFields[2].SortExpression, ListSortDirection.Ascending);
					Fail("GetNewCollectionSorter wrong work with SortExpression");
				}
				catch (ApplicationException ex)
				{
					AssertEquals("GetNewCollectionSorter return don't support exception or wrong work with SortExpresion", "Illegal using SortExpression of ZTileLineColumn with ISupportCostomSort interface", ex.Message);
				}
				catch (Exception)
				{
					Fail("Unexpected Exception");
				}
			}
		}

		public void TestCustomSorterReportDevError()
		{
			using (var testModulForCustomSorter = new ZFilterGridModuleForTest(Factory, new ZPage()))
			{
				AssertEquals("Must be created 3 columns", 3, testModulForCustomSorter.GridColumnFields.Length);

				ErrorReporter.Clear();
				AssertEquals("No DeveloperError should be reported yet", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

				var testSort = CustomSorterSupportConst.IsCustomSorter + ZGuid.NewZGuid().ToString();
				var badSorter = testModulForCustomSorter.GetNewCollectionSorter(testSort, ListSortDirection.Ascending);

				AssertEquals("DeveloperError should be reported", false, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
				var expected = ZString.Format("Cannot find column for Custom sorting Expression {0} , Module {1}", testSort, testModulForCustomSorter.Description);
				AssertEquals("Error message should be as expected", expected, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestModuleManualSortCollection_Load()
		{
			using (var module = new ZFilterGridModuleManualSortForTest(Factory, new ZPage()))
			{
				var filterBizo = module.CreateNewFilterBusinessObject();
				module.LoadCollection(filterBizo);

				var collection = (DummyBusinessObjectManualSortCollection)module.GridCollection;

				Assert(collection.ModuleManualSortCollectLoadCalled);
			}
		}

		#region Implementation

		protected override ZWebModule GetNewZWebModule() => ZWebModuleFactory.Create(TestID, Factory, new ZTestPage());

		protected override WebModuleID TestID => WebModuleIDs.Dummy;

		#region Helper Classes

		public class TestColumnForSorting : ZTextEditColumn, ISupportCustomSorter
		{
			public TestColumnForSorting(string headerText, string bindTo)
				: base(headerText, bindTo)
			{
			}

			public override string SortExpression => CustomSorterSupportConst.IsCustomSorter + CustomSortedColumnID;

			public ZGuid CustomSortedColumnID
			{
				get
				{
					if (testGuid.IsEmpty)
					{
						testGuid = ZGuid.NewZGuid();
					}

					return testGuid;
				}
			}
			ZGuid testGuid;

			public IComparer GetCustomSorter(ListSortDirection sortDirection) => new CustomSorter(BindTo, sortDirection);
		}

		public class CustomSorter : WebCollectionSorter
		{
			public CustomSorter(string sortProperty)
				: base(sortProperty)
			{
			}

			public CustomSorter(string sortProperty, ListSortDirection sortDirection)
				: base(sortProperty, sortDirection)
			{
			}
		}

		public class DummyBusinessObjectManualSortCollection : DummyBusinessObjectCollection, IModuleManualSortCollection
		{
			public DummyBusinessObjectManualSortCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public bool ModuleManualSortCollectLoadCalled { get; private set; }

			public void Load(ZQuery query, ListSortDescriptionCollection sortInfos)
			{
				base.Load(query);
				((IBindingListView)this).ApplySort(sortInfos);
				ModuleManualSortCollectLoadCalled = true;
			}
		}

		#endregion

		#endregion
	}
}
