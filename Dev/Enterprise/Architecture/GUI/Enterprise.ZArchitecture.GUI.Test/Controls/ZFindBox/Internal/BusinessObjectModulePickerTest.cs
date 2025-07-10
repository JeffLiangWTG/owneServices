using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	public class BusinessObjectModulePickerTest : TestCaseWithFactory
	{
		public void TestExplodesIfCollectionInvalid()
		{
			var mock = new Mock<IBusinessObjectCollection>();
			AssertExceptionThrown<InvalidOperationException>(() => BusinessObjectModulePicker.PickFromModuleScreen<BusinessObject>(mock.Object, ModuleIDs.GlbStaff));
		}

		public void TestPickStaff()
		{
			var staff1 = (BusinessObject)Factory.New<IGlbStaff>();
			var staff2 = (BusinessObject)Factory.New<IGlbStaff>();
			var staff3 = (BusinessObject)Factory.New<IGlbStaff>();

			using (SetActionOnDialogShown(popup => SelectRecords(new[] { staff1, staff2 }, popup)))
			{
				var collection = (IActiveBusinessObjectCollection)ObjectFactory.Get<IGlbStaffCollection>("IGlbStaffCollection", Factory);
				var results = BusinessObjectModulePicker.PickFromModuleScreen<BusinessObject>(collection, ModuleIDs.GlbStaff);

				AssertEquals(2, results.Length);
				AssertCollectionContains(staff1, results);
				AssertCollectionContains(staff2, results);
			}
		}

		public void TestPickOneStaff()
		{
			var staff1 = (BusinessObject)Factory.New<IGlbStaff>();
			var staff2 = (BusinessObject)Factory.New<IGlbStaff>();

			var selectionCount = 0;

			using (SetActionOnDialogShown(popup =>
			{
				if (selectionCount == 0)
				{
					SelectRecords(new[] { staff1, staff2 }, popup);
					selectionCount++;
				}
				else
				{
					SelectRecords(new[] { staff1 }, popup);
				}
			}))
			{
				var collection = (IActiveBusinessObjectCollection)ObjectFactory.Get<IGlbStaffCollection>("IGlbStaffCollection", Factory);
				var result = BusinessObjectModulePicker.PickOneRecordFromModuleScreen<BusinessObject>(collection, ModuleIDs.GlbStaff);

				AssertEquals(staff1, result);
				AssertEquals(1, selectionCount);
			}
		}

		public void TestDoNotLoadLastLayoutIfNotRequired()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			using (var module1 = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module1.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Z0_Description");
				var filter = (ModuleTextFilter)strip.CurrentModuleFilter;
				filter.Property = "Test 1";
				filterBizo.SaveLayout("Test1");

				filter.Property = "Test 2";
				var lastLayout = filterBizo.SaveLayout("Test2");
				filterBizo.SaveLastUsedLayout(lastLayout.PK);

				Factory.Save();

				using (module1.ShowPopup())
				{
					var filterControl = module1.DisplayGrid.GetParentFilterControl();
					AssertEquals("Find (Test2)", filterControl.ToolStripFindDropButton_ForTest.Text);
				}
			}

			using (var module2 = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var popup = new EmbeddedModulePopup(module2))
			{
				popup.Show();

				var filterControl = module2.DisplayGrid.GetParentFilterControl();
				AssertEquals("Find (Test2)", filterControl.ToolStripFindDropButton_ForTest.Text);
				popup.Close();
			}

			var emptyLayout = Factory.New<StmModuleFilter>();
			using (var module3 = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var popup = new EmbeddedModulePopup(module3, emptyLayout, true))
			{
				popup.Show();

				var filterControl = module3.DisplayGrid.GetParentFilterControl();
				AssertEquals("Find", filterControl.ToolStripFindDropButton_ForTest.Text);
				popup.Close();
			}
		}

		public void TestSelectInvalidRow()
		{
			var staff1 = (BusinessObject)Factory.New<IGlbStaff>();
			var staff2 = (BusinessObject)Factory.New<IGlbStaff>();
			staff1.FillWithValidTestData();
			staff2.FillWithValidTestData();
			((IGlbStaff)staff1).GS_Code = "ST1";
			((IGlbStaff)staff2).GS_Code = "ST2";
			Factory.Save();

			var collection = (IActiveBusinessObjectCollection)ObjectFactory.Get<IGlbStaffCollection>("IGlbStaffCollection", Factory);
			collection.AdditionalFilter = new ZQuery(Schema.GlbStaffSchema.GS_Code, "ST1");

			var searchResultCount = 0;

			using (SetActionOnDialogShown(popup =>
				{
					popup.Module.PerformSearch();
					searchResultCount = popup.Module.GridCollection.Count;
					popup.Module.ModuleDecisionProvider.HandleDefaultAction(new[] { staff2 });
				}))
			{
				var results = BusinessObjectModulePicker.PickFromModuleScreen<BusinessObject>(collection, ModuleIDs.GlbStaff);

				AssertEquals("PRE", 3, searchResultCount);
				AssertEquals("can't select a row that doesn't match AdditionalFilter", 0, results.Length);
				var expectedMessage = collection.GetAllNotificationsWhenAdditionalFilterNotMet(staff2);
				Assert(!expectedMessage.IsEmpty);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public static IDisposable SelectRecordsOnDialogShown(params BusinessObject[] bizos)
		{
			return SetActionOnDialogShown(dialog => SelectRecords(bizos, dialog));
		}

		static IDisposable SetActionOnDialogShown(Action<EmbeddedModulePopup> action)
		{
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				var modulePopup = (EmbeddedModulePopup)dialog;
				action(modulePopup);
			});

			return new DisposableAction(ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs);
		}

		static void SelectRecords(BusinessObject[] bizos, EmbeddedModulePopup popup)
		{
			var method = typeof(EmbeddedModulePopup).GetMethod("HandleSelection", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			method.Invoke(popup, new object[] { bizos });
		}
	}
}
