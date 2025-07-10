using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGuidSearchEditTest : TestCaseWithDummy
	{
		protected override void SetUp()
		{
			TestGuidSearchEditForm.SuppressBindings = false;

			DummyWithLookups = Factory.New<DummyWithLookups>();

			var allDependents = new DummyDependentWithCodeBusinessObjectCollection(DummyWithLookups, Factory);
			var dependent = allDependents.AddNew();
			dependent.ZD1_Code = "ONE";

			dependent = allDependents.AddNew();
			dependent.ZD1_Code = "TWO";

			dependent = allDependents.AddNew();
			dependent.ZD1_Code = "THREE";

			dependent = allDependents.AddNew();
			dependent.ZD1_Code = "FOUR";

			dependent = allDependents.AddNew();
			dependent.ZD1_Code = "FIVE";

			dependent = allDependents.AddNew();
			dependent.ZD1_Code = "SIX";

			dependent = allDependents.AddNew();
			dependent.ZD1_Code = "SEVEN";

			dependent = allDependents.AddNew();
			dependent.ZD1_Code = "EIGHT";

			dependent = allDependents.AddNew();
			dependent.ZD1_Code = "NINE";

			dependent = allDependents.AddNew();
			dependent.ZD1_Code = "TEN";

			allDependents.Factory.Save();

			base.SetUp();
		}

		public void TestTextChange_WithDropdownOnlyAtEnd()
		{
			using (TestForm = new TestGuidSearchEditForm(DummyWithLookups))
			{
				TestForm.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				AssertNotNull("PreCondition: ZGuidSearchEdit should be instantiated.", GuidSearchEditForTest);
				TestForm.GuidSearchEditForTest.Focus();

				AssertEquals("Focused DropEdit - focus on CodeBox", true, GuidSearchEditForTest.CodeBox.Focused);

				SendKeyToGuidSearchEdit(Keys.T);
				AssertEquals("GuidSearchEditForTest Text", "T", GuidSearchEditForTest.CodeBox.Text);

				SendKeyToGuidSearchEdit(Keys.E);
				AssertEquals("GuidSearchEditForTest Text", "TE", GuidSearchEditForTest.CodeBox.Text);

				SendKeyToGuidSearchEdit(Keys.N);
				AssertEquals("GuidSearchEditForTest Text", "TEN", GuidSearchEditForTest.CodeBox.Text);

				SendKeyToGuidSearchEdit(Keys.F4);
				AssertEquals("DropDown Visible", true, GuidSearchEditForTest.IsDroppedDown);

				AssertEquals("GuidSearchEditForTest List Count", 1, GuidSearchEditForTest.List.Count);
			}
		}

		public void TestTextChange_WithDropdown()
		{
			using (TestForm = new TestGuidSearchEditForm(DummyWithLookups))
			{
				TestForm.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				AssertNotNull("PreCondition: ZGuidSearchEdit should be instantiated.", GuidSearchEditForTest);
				TestForm.GuidSearchEditForTest.Focus();

				AssertEquals("Focused DropEdit - focus on CodeBox", true, GuidSearchEditForTest.CodeBox.Focused);
				SendKeyToGuidSearchEdit(Keys.F4);

				Application.DoEvents();
				UserIdleWorker.Flush();

				AssertEquals("DropDown Visible", true, GuidSearchEditForTest.IsDroppedDown);

				SendKeyToGuidSearchEdit(Keys.T);

				Application.DoEvents();
				UserIdleWorker.Flush();

				AssertEquals("GuidSearchEditForTest List Count", 4, GuidSearchEditForTest.List.Count);

				SendKeyToGuidSearchEdit(Keys.E);
				AssertEquals("GuidSearchEditForTest List Count", 1, GuidSearchEditForTest.List.Count);

				SendKeyToGuidSearchEdit(Keys.N);
				AssertEquals("GuidSearchEditForTest Text", "TEN", GuidSearchEditForTest.CodeBox.Text);

				var previousValue = DummyWithLookups.Z0_Guid;

				SendKeyToGuidSearchEdit(Keys.Tab);

				AssertNotEquals("Bound Property should have changed", previousValue, DummyWithLookups.Z0_Guid);
			}
		}

		public void TestReadOnlyControl()
		{
			using (TestForm = new TestGuidSearchEditForm(DummyWithLookups))
			{
				TestForm.GuidSearchEditForTest.ShowDropDown();
				TestForm.GuidSearchEditForTest.ReadOnly = true;
				TestForm.Show();

				AssertEquals("Code Box Read Only", true, TestForm.GuidSearchEditForTest.CodeBox.ReadOnly);
				AssertEquals("No Dropdown", false, TestForm.GuidSearchEditForTest.IsDroppedDown);

				TestForm.GuidSearchEditForTest.ReadOnly = false;
				TestForm.GuidSearchEditForTest.ShowDropDown();

				AssertEquals("Code Box not Read Only", false, TestForm.GuidSearchEditForTest.CodeBox.ReadOnly);
				AssertEquals("Is a Dropdown", true, TestForm.GuidSearchEditForTest.IsDroppedDown);
			}
		}

		public void TestBusinessObjectBindingReadOnly()
		{
			using (TestForm = new TestGuidSearchEditForm(DummyWithLookups))
			{
				TestForm.Show();
				AssertNotNull("PreCondition: ZDropEdit should be instantiated.", GuidSearchEditForTest);
				UserIdleWorker.Flush();
				AssertEquals("Control is not read only", false, GuidSearchEditForTest.ReadOnly);
				DummyWithLookups.Z0_Guid_ReadOnly = true;
				UserIdleWorker.Flush();
				AssertEquals("Control is read only", true, GuidSearchEditForTest.ReadOnly);
			}
		}

		public void TestUnBoundedControl()
		{
			TestGuidSearchEditForm.SuppressBindings = true;
			using (TestForm = new TestGuidSearchEditForm(DummyWithLookups))
			{
				TestForm.Show();

				Application.DoEvents();
				UserIdleWorker.Flush();

				AssertNotNull("PreCondition: ZGuidSearchEdit should be instantiated.", GuidSearchEditForTest);
				TestForm.GuidSearchEditForTest.Focus();

				AssertNull("GuidSearchEditForTest List is null", GuidSearchEditForTest.List);

				AssertEquals("Focused DropEdit - focus on CodeBox", true, GuidSearchEditForTest.CodeBox.Focused);
				SendKeyToGuidSearchEdit(Keys.F4);
				AssertEquals("DropDown Visible", true, GuidSearchEditForTest.IsDroppedDown);

				SendKeyToGuidSearchEdit(Keys.T);
				AssertNull("GuidSearchEditForTest List is null", GuidSearchEditForTest.List);

				SendKeyToGuidSearchEdit(Keys.F4);
				AssertEquals("DropDown not Visible", false, GuidSearchEditForTest.IsDroppedDown);
			}
		}

		public void TestShowEditForm()
		{
			using (var filterModule = new DummyDependentWithCodeModule())
			{
				filterModule.SetAllowEdit(true);
				filterModule.SetAllowView(true);

				using (TestForm = new TestGuidSearchEditForm(DummyWithLookups))
				{
					TestForm.Show();

					GuidSearchEditForTest.SetFilterModuleForTesting(filterModule);

					Application.DoEvents();
					UserIdleWorker.Flush();

					GuidSearchEditForTest.CodeBox.Text = "TEN";

					SendKeyToGuidSearchEdit(Keys.F3);

					using (var popup = GuidSearchEditForTest.DummyModule.LastController.LastFormCreated)
					{
						AssertNotNull("'Form for Edit' Created", popup);
						AssertEquals("Editing Dummy Code", "TEN", ((DummyDependentWithCodeBusinessObject)popup.BusinessEntity).ZD1_Code);
						AssertEquals("DisplayMode", ODisplayMode.Browse, popup.DisplayMode);
					}
				}
			}
		}

		public void TestShowViewForm()
		{
			using (var filterModule = new DummyDependentWithCodeModule())
			{
				filterModule.SetAllowEdit(false);
				filterModule.SetAllowView(true);

				using (TestForm = new TestGuidSearchEditForm(DummyWithLookups))
				{
					TestForm.Show();

					GuidSearchEditForTest.SetFilterModuleForTesting(filterModule);

					Application.DoEvents();
					UserIdleWorker.Flush();

					GuidSearchEditForTest.CodeBox.Text = "TEN";

					SendKeyToGuidSearchEdit(Keys.F3);

					using (var popup = GuidSearchEditForTest.DummyModule.LastController.LastFormCreated)
					{
						AssertNotNull("'Form for View' Created", popup);
						AssertEquals("View form", "View ZDummyDependentWithCodeForm", popup.Text);
						AssertEquals("Editing Dummy Code", "TEN", ((DummyDependentWithCodeBusinessObject)popup.BusinessEntity).ZD1_Code);
						AssertEquals("DisplayMode", ODisplayMode.ReadOnly, popup.DisplayMode);
					}
				}
			}
		}

		public void TestShowEditFormMultipleItemsSelected()
		{
			using (var filterModule = new DummyDependentWithCodeModule())
			{
				filterModule.SetAllowEdit(true);
				filterModule.SetAllowView(true);
				using (TestForm = new TestGuidSearchEditForm(DummyWithLookups))
				{
					TestForm.Show();
					Application.DoEvents();
					UserIdleWorker.Flush();

					AssertNotNull("PreCondition: ZGuidSearchEdit should be instantiated.", GuidSearchEditForTest);
					TestForm.GuidSearchEditForTest.Focus();

					AssertEquals("Focused DropEdit - focus on CodeBox", true, GuidSearchEditForTest.CodeBox.Focused);

					Application.DoEvents();
					UserIdleWorker.Flush();

					SendKeyToGuidSearchEdit(Keys.F4);
					AssertEquals("DropDown Visible", true, GuidSearchEditForTest.IsDroppedDown);

					SendKeyToGuidSearchEdit(Keys.T);
					AssertEquals("GuidSearchEditForTest List Count", 4, GuidSearchEditForTest.List.Count);

					GuidSearchEditForTest.SetFilterModuleForTesting(filterModule);

					Application.DoEvents();
					UserIdleWorker.Flush();

					SendKeyToGuidSearchEdit(Keys.F3);

					var popup = GuidSearchEditForTest.DummyModule?.LastController?.LastFormCreated;

					AssertEquals("New form shown", ODisplayMode.New, popup.DisplayMode);
				}
			}
		}

		public void TestGetBizObjsToEditOrView_NoErrorForActiveCollection()
		{
			var dummyWithLookups = Factory.New<DummyWithLookups>();
			var allDependents = new DummyDependentWithCodeBusinessObjectCollection(dummyWithLookups, Factory);
			var dependent = allDependents.AddNew();
			dependent.ZD1_Code = "ONE";
			allDependents.Factory.Save();

			using (TestForm = new TestGuidSearchEditForm(dummyWithLookups))
			{
				TestForm.Show();

				GuidSearchEditForTest.BindToList = "Lookups.ActiveDependents";

				AssertEquals("Precondition: No errors", 0, ExceptionReporterTestListener.Instance.Count);
				var collection = GuidSearchEditForTest.GetBizObjsToEditOrView();
				AssertEquals("No developer errors should be reported", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Collection contains value", 1, collection.Count());
				var item = (DummyDependentWithCodeBusinessObject)collection.FirstOrDefault();
				AssertEquals("The ZD1_Code of item should be ONE", "ONE", item.ZD1_Code);
			}
		}

		void SendKeyToGuidSearchEdit(Keys key)
		{
			KeySender.PostKeyDown(TestForm.GuidSearchEditForTest.CodeBox, key);
			Application.DoEvents();
			UserIdleWorker.Flush();
		}

		DummyWithLookups DummyWithLookups;

		TestGuidSearchEditForm TestForm;
		TestZGuidSearchEditForTest GuidSearchEditForTest
		{
			get { return TestForm.GuidSearchEditForTest; }
		}
	}
}
