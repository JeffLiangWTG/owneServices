using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	[SuppressFormDesignerAnalysis]
	class ZPopupFindBoxTest : BaseFindBoxTest
	{
		public void TestNewModuleFromModuleIDDoesNotReturnDisposedModule()
		{
			CreateControls(Form, "");
			using (var dummy = (DummyFilterGridModule)ZPopupFindBox.NewModuleFromModuleID())
			{
				Assert("Should not return a disposed module", !dummy.IsDisposed);
			}
		}

		public void TestShowNewForm()
		{
			CreateControls(Form, "");
			FindBox.CodeBox.Text = "NEWCODE";
			Form.Show();

			SendCommandKeyToCodeBox(Keys.F3);
			var popup = ZPopupFindBox.DummyModule.LastController.LastFormCreated;

			try
			{
				AssertNotNull("'Form for New' Created", popup);
				//brett todo :)	AssertEquals("New Dummy has Default Code", "NCODE", ((DummyBusinessObject)Popup.BusinessEntity).Z0_Code);
				AssertEquals("DisplayMode", ODisplayMode.New, popup.DisplayMode);
			}
			finally
			{
				popup.Close();
			}
		}

		public void TestShouldShowNewFormWhenEmpty()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			DummyFilterGridModule.SetAllowNew(true);

			using (var form = new ZForm(dummy))
			{
				var grid = new ZGrid { BindTo = "Collection" };
				var columnStyleInfo = new ZGuidFindBoxColumnStyleInfo
				{
					BindToList = "Lookups+DummyList",
					ModuleID = ModuleIDs.NotAssigned,
					ColumnName = "Z0_Guid",
					ShowNewFormWhenEmpty = false
				};

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;

				form.Controls.Add(grid);
				form.Show();
				grid.Columns[0].ColumnStyle.ReadOnly = true;

				grid.FindBoxColumnModuleShowing += (sender, e) =>
				{
					AssertEquals(grid.Columns[0].ColumnStyle, e.ColumnStyle);
					e.ModuleID = DummyModuleIDs.Dummy;
				};

				((IFindBoxUserControl)((ZCodeFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl).ShowEditOrViewForm();
				AssertNull("Should not have shown anything", ZFormModaliser.ActiveForm);

				((ZGridFindBox)(((ZCodeFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl)).ShowNewFormWhenEmpty = true;
				((IFindBoxUserControl)((ZCodeFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl).ShowEditOrViewForm();
				AssertNotNull("Should have shown", ZFormModaliser.ActiveForm);
			}
		}

		#region Testing AllowNewForm

		public void AllowNewForm()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();

			using (var form = new ZForm(dummy))
			{
				var grid = new ZGrid();
				grid.BindTo = "Collection";
				var columnStyleInfo = new ZGuidFindBoxColumnStyleInfo();
				columnStyleInfo.BindToList = "Lookups+DummyList";
				columnStyleInfo.ModuleID = ModuleIDs.NotAssigned;
				columnStyleInfo.ColumnName = "Z0_Guid";
				columnStyleInfo.AllowNewForm = false;

				grid.ColumnStyles.Add(columnStyleInfo);
				grid.Dock = DockStyle.Fill;

				form.Controls.Add(grid);
				form.Show();
				grid.Columns[0].ColumnStyle.ReadOnly = true;

				grid.FindBoxColumnModuleShowing += (sender, e) =>
				{
					AssertEquals(grid.Columns[0].ColumnStyle, e.ColumnStyle);
					e.ModuleID = DummyModuleIDs.Dummy;
				};

				((IFindBoxUserControl)((ZCodeFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl).ShowEditOrViewForm();
				AssertNull("Should not have shown anything", ZFormModaliser.ActiveForm);

				((ZGridFindBox)(((ZCodeFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl)).AllowNewForm = true;
				((IFindBoxUserControl)((ZCodeFindBoxColumnStyle)grid.Columns[0].ColumnStyle).EditControl).ShowEditOrViewForm();
				AssertNotNull("Should have shown", ZFormModaliser.ActiveForm);
			}
		}

		public void TestOverrideSecurityMessage()
		{
			using (var findBox = new TestPopupFindBox())
			{
				using (var module = new DummyFilterGridModule())
				{
					findBox.Code = "Sango_Test";
					DummyFilterGridModule.SetAllowNew(false);
					module.DefaultMessageOverridingSecurityRightMessage = "Dummy Message";
					findBox.ShowEditForm(module);
					AssertEquals("Dummy Message", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}
		#endregion

		public void TestRunPopupSecurityCheck()
		{
			CreateControls(Form, "");
			ZPopupFindBox.ModuleID = DummyModuleIDs.Dummy;

			try
			{
				DummyFilterGridModule.PopupsAreAllowed = false;
				AssertEquals(false, ZPopupFindBox.RunPopupSecurityCheck());

				DummyFilterGridModule.PopupsAreAllowed = true;
				AssertEquals(true, ZPopupFindBox.RunPopupSecurityCheck());
			}
			finally
			{
				DummyFilterGridModule.ResetPopupsAreAllowedToDefault();
			}
		}

		public void TestRunPopupSecurityCheckWithCountryOverride()
		{
			CreateControls(Form, "");
			ZPopupFindBox.ModuleID = DummyModuleIDs.Dummy3;

			try
			{
				DummyFilterGridModule.PopupsAreAllowed = true;

				ZPopupFindBox.GetCountryCode = () => string.Empty;
				AssertExceptionThrown<ZException>("ZModuleFactory did not return a module for ID : Dummy3", () => ZPopupFindBox.RunPopupSecurityCheck());

				ZPopupFindBox.GetCountryCode = () => Enterprise.Core.Constants.CountryCodes._TemplateCountryName_;
				AssertNoExceptionThrown(() => ZPopupFindBox.RunPopupSecurityCheck());
			}
			finally
			{
				DummyFilterGridModule.ResetPopupsAreAllowedToDefault();
			}
		}

		public void TestShowNewPopupModuleFormIfListOnlyExistedBriefly()
		{
			CreateControls(Form, "");
			ZPopupFindBox.ModuleID = ModuleIDs.NotAssigned;
			ZPopupFindBox.List = new FilteredDummyCollection(Factory);
			ZPopupFindBox.List = null;

			Assert(ZPopupFindBox.PopupForm is EmbeddedModulePopup);
		}

		public void TestShowNewPopupModuleFormIfNoSecurityRight()
		{
			CreateControls(Form, "");

			DummyFilterGridModule.PopupsAreAllowed = false;
			ZPopupFindBox.ModuleID = DummyModuleIDs.Dummy;
			ZPopupFindBox.List = new FilteredDummyCollection(Factory);

			var popupForm = (EmbeddedModulePopup)ZPopupFindBox.PopupForm;
			Assert(!ZPopupFindBox.RunPopupSecurityCheck());
			AssertNotNull(popupForm);
			AssertEquals("Z0_Code", (popupForm.Module_ForTest.LimitedColumns as ZLimitedColumnsProvider).CodeColumnName);
			AssertEquals("Z0_Description", (popupForm.Module_ForTest.LimitedColumns as ZLimitedColumnsProvider).DescriptionColumnName);
		}

		public void TestRunPopupSecurityCheckWithListSetOnly()
		{
			CreateControls(Form, "");
			ZPopupFindBox.ModuleID = ModuleIDs.NotAssigned;
			ZPopupFindBox.List = new List1();

			try
			{
				DummyFilterGridModule.PopupsAreAllowed = false;
				AssertEquals(false, ZPopupFindBox.RunPopupSecurityCheck());

				DummyFilterGridModule.PopupsAreAllowed = true;
				AssertEquals(true, ZPopupFindBox.RunPopupSecurityCheck());
			}
			finally
			{
				DummyFilterGridModule.ResetPopupsAreAllowedToDefault();
			}
		}

		public void TestShowEditForm()
		{
			var newDummy = Factory.New<DummyBusinessObject>();
			newDummy.Z0_Code = "ABCD";
			newDummy.Factory.Save();

			CreateControls(Form, "");
			Form.Show();

			ZPopupFindBox.CodeBox.Text = "ABCD";

			SendCommandKeyToCodeBox(Keys.F3);
			var popup = ZPopupFindBox.DummyModule.LastController.LastFormCreated;

			try
			{
				AssertNotNull("'Form for Edit' Created", popup);
				AssertEquals("Editing Dummy Code", newDummy.Z0_Code, ((DummyBusinessObject)popup.BusinessEntity).Z0_Code);
				AssertEquals("DisplayMode", ODisplayMode.Browse, popup.DisplayMode);
			}
			finally
			{
				popup.Close();
			}
		}

		public void TestShowEditFormWithAllowNewFalse()
		{
			try
			{
				DummyFilterGridModule.SetAllowNew(false);
				var expectedMessage = SecurityCore.SecurityErrorMessage + System.Environment.NewLine + System.Environment.NewLine + "TESTING 1 2 3";

				CreateControls(Form, "");
				Form.Show();

				ZPopupFindBox.CodeBox.Text = "";
				SendCommandKeyToCodeBox(Keys.F3);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNull("No new permissions, the controller should not have been created.", ZPopupFindBox.DummyModule.LastController);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				ZPopupFindBox.CodeBox.Text = "BAD";
				SendCommandKeyToCodeBox(Keys.F3);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNull("No new permissions, the controller should not have been created.", ZPopupFindBox.DummyModule.LastController);
			}
			finally
			{
				DummyFilterGridModule.ResetAllowNewToDefault();
			}
		}

		public void TestShowEditFormWithAllowNewTrue()
		{
			CreateControls(Form, "");
			Form.Show();

			ZPopupFindBox.CodeBox.Text = "";
			SendCommandKeyToCodeBox(Keys.F3);

			var popup = ZPopupFindBox.DummyModule.LastController.LastFormCreated;
			try
			{
				AssertNotNull("'Form for New' created", popup);
				AssertEquals("DisplayMode", ODisplayMode.New, popup.DisplayMode);
			}
			finally
			{
				popup.Close();
			}
		}

		public void TestShowEditFormWithReadOnlyTrue()
		{
			try
			{
				DummyFilterGridModule.SetAllowNew(true);
				CreateControls(Form, "");

				var dummy1 = Factory.New<DummyBusinessObject>();
				dummy1.Z0_Code = "ABCDE";
				Factory.Save();

				Dummy.SS_Dummy = dummy1.Z0_Code;

				Dummy.SS_Dummy_ReadOnly = true;
				Form.Show();

				SendCommandKeyToCodeBox(Keys.F3);
				AssertEquals(ODisplayMode.Browse, ZPopupFindBox.DummyModule.LastController.LastFormCreated.DisplayMode);
				ZPopupFindBox.DummyModule.LastController.LastFormCreated.Close();
			}
			finally
			{
				DummyFilterGridModule.ResetAllowNewToDefault();
			}
		}

		public void TestShowEditFormWithReadOnlyFalse()
		{
			try
			{
				DummyFilterGridModule.SetAllowNew(true);
				CreateControls(Form, "");

				var dummy1 = Factory.New<DummyBusinessObject>();
				dummy1.Z0_Code = "ABCDE";
				Factory.Save();

				Dummy.SS_Dummy = dummy1.Z0_Code;

				Dummy.SS_Dummy_ReadOnly = false;
				Form.Show();

				SendCommandKeyToCodeBox(Keys.F3);
				AssertEquals(ODisplayMode.Browse, ZPopupFindBox.DummyModule.LastController.LastFormCreated.DisplayMode);
				ZPopupFindBox.DummyModule.LastController.LastFormCreated.Close();
			}
			finally
			{
				DummyFilterGridModule.ResetAllowNewToDefault();
			}
		}

		public void TestShowEditFormWithListIsNull()
		{
			using (var findBox = new TestPopupFindBox())
			{
				using (var module = new DummyFilterGridModule())
				{
					findBox.Code = "AAA";
					findBox.List = null;
					AssertNoExceptionThrown(() => findBox.ShowEditForm(module)); //There was an exception.
				}
			}
		}

		public void TestShowNewFormWithReadOnlyTrue()
		{
			try
			{
				DummyFilterGridModule.SetAllowNew(true);
				CreateControls(Form, "");

				Dummy.SS_Dummy_ReadOnly = true;
				Form.Show();

				ZPopupFindBox.CodeBox.Text = "";
				SendCommandKeyToCodeBox(Keys.F3);
				AssertNull(ZPopupFindBox.DummyModule.LastController);
			}
			finally
			{
				DummyFilterGridModule.ResetAllowNewToDefault();
			}
		}

		public void TestShowNewFormWithReadOnlyFalse()
		{
			try
			{
				DummyFilterGridModule.SetAllowNew(true);
				CreateControls(Form, "");

				Dummy.SS_Dummy_ReadOnly = false;
				Form.Show();

				ZPopupFindBox.CodeBox.Text = "";
				SendCommandKeyToCodeBox(Keys.F3);
				AssertEquals(ODisplayMode.New, ZPopupFindBox.DummyModule.LastController.LastFormCreated.DisplayMode);
				ZPopupFindBox.DummyModule.LastController.LastFormCreated.Close();
			}
			finally
			{
				DummyFilterGridModule.ResetAllowNewToDefault();
			}
		}

		public void TestShowViewForm()
		{
			using (var filterModule = new DummyFilterGridModule())
			{
				filterModule.SetAllowEdit(false);
				filterModule.SetAllowView(true);

				var newDummy = Factory.New<DummyBusinessObject>();
				newDummy.Z0_Code = "ABCD";
				Factory.Save();

				CreateControls(Form, "");
				Form.Show();

				ZPopupFindBox.SetFilterModuleForTesting(filterModule);
				ZPopupFindBox.CodeBox.Text = "ABCD";

				SendCommandKeyToCodeBox(Keys.F3);
				var popup = ZPopupFindBox.DummyModule.LastController.LastFormCreated;

				try
				{
					AssertNotNull("'Form for View' Created", popup);
					AssertEquals("View form", "View ZDummyForm", popup.Text);
					AssertEquals("Editing Dummy Code", newDummy.Z0_Code, ((DummyBusinessObject)popup.BusinessEntity).Z0_Code);
					AssertEquals("DisplayMode", ODisplayMode.ReadOnly, popup.DisplayMode);
				}
				finally
				{
					popup.Close();
				}
			}
		}

		public void TestShowEditFormMultipleItemsSelected()
		{
			var newDummy = Factory.New<DummyBusinessObject>();
			newDummy.Z0_Code = "ABCD";
			newDummy.Z0_Description = "First Description";

			var newDummy2 = Factory.New<DummyBusinessObject>();
			newDummy2.Z0_Code = "ABCD";
			newDummy2.Z0_Description = "Second Description";
			Factory.Save();

			CreateControls(Form, "");
			Form.Show();

			ZPopupFindBox.CodeBox.Text = "ABCD";

			SendCommandKeyToCodeBox(Keys.F3);

			AssertMultilineASCIIEquals("More than one entity has this code. Select and edit the one you're after.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			var popupForm = (EmbeddedModulePopup)ZPopupFindBox.PopupForm;

			try
			{
				AssertNotNull("'Form for Selection' Created", popupForm);
				AssertEquals(popupForm.Text, "Test Module");

				var elements = ((IDummyFilterModule)popupForm.Module).Collection.ToArray();
				AssertEquals("Number of Elements", 2, elements.Length);
				AssertEquals("First Element", "First Description", ((DummyBusinessObject)elements[0]).Z0_Description);
				AssertEquals("Second Element", "Second Description", ((DummyBusinessObject)elements[1]).Z0_Description);
			}
			finally
			{
				popupForm.Close();
			}
		}

		public void TestPressingF3OnAModuleThatDoesNotAllowViewDoesNotThrowException()
		{
			var newDummy = Factory.New<DummyBusinessObject>();
			newDummy.Z0_Code = "ABCD";
			newDummy.Factory.Save();

			CreateControls(Form, "");
			Form.Show();

			ZPopupFindBox.ReadOnly = true;
			ZPopupFindBox.CodeBox.Text = "ABCD";
			using (var filterModule = new DummyFilterGridModule())
			{
				filterModule.SetAllowEdit(false);
				filterModule.SetAllowView(false);
				ZPopupFindBox.SetFilterModuleForTesting(filterModule);
				AssertNoExceptionThrown(delegate { SendCommandKeyToCodeBox(Keys.F3); });
				AssertEquals(filterModule, ZPopupFindBox.DummyModule);
				AssertNull("'No Form for View' Created", filterModule.LastController);
			}
		}

		public void TestPopupFormCanHandleInvalidModuleId()
		{
			CreateControls(Form, "");
			ZPopupFindBox.ModuleID = ModuleIDs.NotAssigned;
			ZPopupFindBox.PopupCaption = null;

			var popupForm = (Form)ZPopupFindBox.PopupForm;

			AssertNotNull(popupForm);
			AssertEquals(String.Format("A valid ModuleID could not be found for: {0}\r\nList.GetType() is {1}\r\nDataSource is {2}\r\nCurrentItem is {3}\r\nDataMember is {4}\r\nDataMember is {5}", "ZForm :  (TestPopupFindBox)", "null", "null", "null", String.Empty, String.Empty), ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestPopupShowAndClose()
		{
			CreateControls(Form, "");
			Form.Show();

			SendCommandKeyToCodeBox(Keys.F4);
			var popupForm = ((Form)ZPopupFindBox.PopupForm);
			AssertEquals("Popup Form Visible", true, popupForm.Visible);

			KeySender.PostKeyDown((Form)ZPopupFindBox.PopupForm, Keys.Escape);
			Application.DoEvents();

			AssertEquals("Test Form Visible", true, Form.Visible);
			AssertEquals("Popup Form Visible", false, popupForm.Visible);
			AssertEquals("Popup form IsDisposed", true, popupForm.IsDisposed);
		}

		public void TestSetCodeForNewForm()
		{
			var dummy = Factory.New<DummyBusinessObjectWithCalculatedCode>();
			CreateControls(Form, "");

			ZPopupFindBox.SetCodeProperty(dummy, "a");
			AssertEquals("Code should be set", "a", dummy.CalculatedCode);

			ZPopupFindBox.SetCodeProperty(dummy, "".PadLeft(dummy.CalculatedCodeInfo.MaxLength + 5, 'a'));
			AssertEquals("Code should be truncated to fit maxlength", "".PadLeft(dummy.CalculatedCodeInfo.MaxLength, 'a'), dummy.CalculatedCode);
		}

		public void TestSetCodeForNewForm_ReadOnlyCode()
		{
			var dummy = Factory.New<DummyBizoWithReadOnlyCode>();
			CreateControls(Form, "");

			ZPopupFindBox.SetCodeProperty(dummy, "a");
			AssertEquals("Code should not change", "ImReadOnly", dummy.CalculatedCode);
		}

		public void TestSetCodeForNewForm_NoInfo()
		{
			var dummy = Factory.New<DummyBizoWithNoInfoCode>();
			CreateControls(Form, "");

			ZPopupFindBox.SetCodeProperty(dummy, "a");
			AssertEquals("Code should change", "a", dummy.CalculatedCode);
		}

		public void TestSetCodeForNewForm_NotSettingDefaultValueAttribute()
		{
			var dummy = Factory.New<DummyBusinessObjectWithNumberFountainCode>();
			CreateControls(Form, "");

			ZPopupFindBox.SetCodeProperty(dummy, "a");
			AssertEquals("Code should not be set for a number fountain settable property", "", dummy.Code);
		}

		public void TestGetCountryCode()
		{
			CreateControls(Form, "");
			ZPopupFindBox.GetCountryCode = () => EnvProxy.Instance.CurrentCompany.Country.Code;

			using (var dummy = (DummyFilterGridModule)ZPopupFindBox.NewModuleFromModuleID())
			{
				AssertNotNull("Module", dummy);
			}
		}

		public void TestNewModuleFromModuleID()
		{
			using (var findBox = new TestPopupFindBox())
			{
				var moduleID = DummyModuleIDs.Dummy;
				findBox.ModuleShowing += (sender, e) => e.ModuleID = moduleID;
				using (var module = findBox.NewModuleFromModuleID())
				{
					AssertEquals(findBox.DummyModule, module);
				}

				moduleID = ModuleIDs.GlbStaff;
				using (var module = findBox.NewModuleFromModuleID())
				{
					AssertEquals("Enterprise.MasterFiles.Module.GlbStaffModule", module.GetType().FullName);
				}
			}
		}

		public void TestPrefixExistsInModule()
		{
			using (var findBox = new TestPopupFindBox())
			{
				findBox.ModuleID = ModuleIDs.JobConsol;
				Assert("Prefix 'H' should exist", findBox.PrefixExistsInModule("H"));
				Assert("Prefix 'M' should exist", findBox.PrefixExistsInModule("M"));
				Assert("Prefix 'L' should exist", findBox.PrefixExistsInModule("L"));
				Assert("Prefix 'APPLE ' should not exist", !findBox.PrefixExistsInModule("APPLE "));
			}
		}

		public void TestModuleID()
		{
			try
			{
				using (var findBox = new TestPopupFindBoxNoList())
				{
					var newDummy = Factory.New<DummyBusinessObject>();
					newDummy.Factory.Save();
					findBox.SetDataBinding(newDummy, "Collection");
					AssertEquals(ModuleIDs.NotAssigned, findBox.ModuleID);
					AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#region TestGetBizObjsToEditOrView

		public void TestGetBizObjsToEditOrView()
		{
			Factory.New<DummyBusinessObject>().Z0_Code = "AAA";
			Factory.New<DummyBusinessObject>().Z0_Code = "BBB";

			var filteredCollection = new FilteredDummyCollection(Factory);
			filteredCollection.FilterCode = "B";

			var restrictedFilteredCollection = new RestrictedFilteredDummyCollection(Factory);
			restrictedFilteredCollection.FilterCode = "B";

			using (var findBox = new TestPopupFindBox())
			{
				findBox.Code = "AAA";

				findBox.List = filteredCollection;
				AssertNotNull(findBox.GetBizObjsToEditOrView().FirstOrDefault());

				findBox.List = restrictedFilteredCollection;
				AssertNull("Should not find filtered item with restricted option enabled", findBox.GetBizObjsToEditOrView().FirstOrDefault());

				findBox.Code = "BBB";

				findBox.List = filteredCollection;
				AssertNotNull(findBox.GetBizObjsToEditOrView().FirstOrDefault());

				findBox.List = restrictedFilteredCollection;
				AssertNotNull(findBox.GetBizObjsToEditOrView().FirstOrDefault());

				Factory.New<DummyBusinessObject>().Z0_Code = "BBB";

				AssertEquals(findBox.GetBizObjsToEditOrView().Count(), 2);
			}
		}

		[ModuleID(ModuleId.Dummy)]
		class FilteredDummyCollection : DummyBusinessObjectCollection
		{
			public FilteredDummyCollection(BusinessObjectFactory factory) : base(factory) { }

			protected override ZQuery CreateRelationshipFilter()
			{
				return new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, FilterCode);
			}

			public ZString FilterCode { get; set; }
		}

		[RestrictedFilteredItem]
		class RestrictedDummy : DummyBusinessObject
		{
			public RestrictedDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class RestrictedFilteredDummyCollection : FilteredDummyCollection
		{
			public RestrictedFilteredDummyCollection(BusinessObjectFactory factory) : base(factory) { }

			public new RestrictedDummy this[int i]
			{
				get { return (RestrictedDummy)base[i]; }
			}
		}

		#endregion

		#region TestSelectFromPopupFormWithoutDisplaying

		public void TestSelectFromPopupWithoutDisplaying()
		{
			using (var findbox = new ZPopupFindBoxForSelectTest())
			{
				AssertEquals(SilentSelectResult.None, findbox.SelectFromPopupFormWithoutDisplaying());

				((IFindBox)findbox).Code = "XYZ";

				AssertEquals(SilentSelectResult.FoundNothing, findbox.SelectFromPopupFormWithoutDisplaying());
				AssertEquals("Code should not be changed", "XYZ", ((IFindBox)findbox).Code);

				var dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Description = "Q";
				dummy.Z0_Code = "ABC";
				Factory.Save();

				AssertEquals(SilentSelectResult.FoundOne, findbox.SelectFromPopupFormWithoutDisplaying());
				AssertEquals("Selected code should be set", "ABC", ((IFindBox)findbox).Code);

				((IFindBox)findbox).Code = "XYZ";
				dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Description = "Q";
				Factory.Save();

				AssertEquals(SilentSelectResult.MultipleResults, findbox.SelectFromPopupFormWithoutDisplaying());
				AssertEquals("Code should not be changed", "XYZ", ((IFindBox)findbox).Code);
			}
		}

		class ZPopupFindBoxForSelectTest : TestPopupFindBox
		{
			protected override IFindBoxPopup GetNewPopupForm()
			{
				var module = (ZFilterModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy);
				var codeFilter = module.FilterBusinessObject.ModuleFilters.AddTextFilter("Q Description", DummyBizoSchema.Z0_Description);
				codeFilter.Property = "Q";
				codeFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

				return new EmbeddedModulePopup(module);
			}
		}

		#endregion

		#region TestChangingListChangesModuleId

		public void TestChangingListChangesModuleId()
		{
			using (var findBox = new TestPopupFindBox())
			{
				AssertEquals(ModuleIDs.NotAssigned, findBox.ModuleID);

				findBox.List = new List1();
				AssertEquals(DummyModuleIDs.Dummy, findBox.ModuleID);

				findBox.List = new List2();
				AssertEquals(DummyModuleIDs.Dummy2, findBox.ModuleID);

				findBox.List = null;
				AssertEquals(DummyModuleIDs.Dummy2, findBox.ModuleID);

				findBox.List = new List1();
				AssertEquals(DummyModuleIDs.Dummy, findBox.ModuleID);
			}
		}

		[ModuleID(ModuleId.Dummy)]
		class List1 : List<object> { }

		[ModuleID(ModuleId.Dummy2)]
		class List2 : List<object> { }

		#endregion

		#region TestSelectFromPopupForm

		public void TestSelectFromPopupForm()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "Q";
			dummy1.Z0_Code = "ABC";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Description = "Q";
			dummy2.Z0_Code = "ACD";
			Factory.Save();

			using (var form = new ZForm())
			using (var findbox = new ZPopupFindBoxForSelectTest())
			{
				form.Controls.Add(findbox);
				EnvProxy.Instance.Registry.AutoRunSearchFromFindBox = true;
				((IFindBox)findbox).Code = "AB";
				findbox.SelectFromPopupForm(false);
				var popup = findbox.PopupForm as EmbeddedModulePopup;
				Assert(popup.Visible);
				AssertEquals("The first item should be selected", 1, popup.Module.DisplayGrid.SelectedElements.Length);
				AssertEquals("Selected code should not be set", "AB", ((IFindBox)findbox).Code);
				AssertEquals("A EmbeddedModulePopup should be shown when autoSelect is false", popup, ZFormModaliser.LastFormShownForTest);
				popup.Close();

				ZFormModaliser.LastFormShownForTest = null;
				findbox.SelectFromPopupForm(true);
				AssertNull("A EmbeddedModulePopup should not be shown when only one candidate", ZFormModaliser.LastFormShownForTest);
				AssertEquals("Selected code should be set", "ABC", ((IFindBox)findbox).Code);

				ZFormModaliser.LastFormShownForTest = null;
				findbox.SelectFromPopupForm(true);
				popup = findbox.PopupForm as EmbeddedModulePopup;
				Assert(popup.Visible);
				AssertEquals("A EmbeddedModulePopup should be shown when only one candidate but equals to the old code", popup, ZFormModaliser.LastFormShownForTest);
				popup.Close();

				EnvProxy.Instance.Registry.AutoRunSearchFromFindBox = false;
				((IFindBox)findbox).Code = "AB";
				findbox.SelectFromPopupForm(true);
				popup = findbox.PopupForm as EmbeddedModulePopup;
				Assert(popup.Visible);
				AssertEquals("Selected code should not be set", "AB", ((IFindBox)findbox).Code);
				AssertEquals("A EmbeddedModulePopup should be shown when registry AutoRunSearchFromFindBox is off", popup, ZFormModaliser.LastFormShownForTest);
				popup.Close();

				ZFormModaliser.LastFormShownForTest = null;
				EnvProxy.Instance.Registry.AutoRunSearchFromFindBox = true;
				((IFindBox)findbox).Code = "A";
				findbox.SelectFromPopupForm(true);
				popup = findbox.PopupForm as EmbeddedModulePopup;
				Assert(popup.Visible);
				AssertEquals("The first item should be selected", 1, popup.Module.DisplayGrid.SelectedElements.Length);
				AssertEquals("Selected code should not be set", "A", ((IFindBox)findbox).Code);
				AssertEquals("A EmbeddedModulePopup should be shown when more than one candidate", popup, ZFormModaliser.LastFormShownForTest);
				popup.Close();
			}
		}

		public void TestSelectFromPopupFormTwice()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "Q";
			dummy1.Z0_Code = "ABC";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Description = "Q";
			dummy2.Z0_Code = "ACD";
			Factory.Save();

			using (var form = new ZForm())
			using (var findbox = new ZPopupFindBoxForSelectTest())
			{
				form.Controls.Add(findbox);
				EnvProxy.Instance.Registry.AutoRunSearchFromFindBox = true;
				((IFindBox)findbox).Code = "AB";
				findbox.SelectFromPopupForm(false);
				AssertNoExceptionThrown(() => findbox.SelectFromPopupForm(false));
				var popup = findbox.PopupForm as EmbeddedModulePopup;
				popup.Close();
			}
		}

		#endregion

		#region Test Classes

		class TestPopupFindBoxNoList : ZPopupFindBox
		{
			protected internal override bool RequiresList
			{
				get { return false; }
			}
		}

		class TestPopupFindBox : ZPopupFindBox
		{
			public new IFindBoxPopup PopupForm
			{
				get { return base.PopupForm; }
			}

			public new void SetCodeProperty(BusinessObject bizO, string code)
			{
				base.SetCodeProperty(bizO, code);
			}

			public override void SetDataBinding(object dataSource, string dataMember)
			{
				if (dataSource != null)
				{
					IFindBox.Description = DescriptionFromCode("");
				}
				base.SetDataBinding(dataSource, dataMember);
				if (dataSource != null && !ComponentExtensions.IsDesignMode(this))
				{
					PullList();
					var codeBinding = new KBinding("Text", dataSource, dataMember, true);
					codeBinding.Format += new ConvertEventHandler(CodeBoxBinding_Format);
					codeBinding.Parse += new ConvertEventHandler(CodeBoxBinding_Parse);
					CodeBox.DataBindings.Add(codeBinding);
				}
			}

			public DummyFilterGridModule DummyModule;

			public void SetFilterModuleForTesting(ZFilterModule module)
			{
				filterModuleForTesting = module;
				ModuleID = module.ID;
			}
			ZFilterModule filterModuleForTesting;

			public void SetParentModuleIDForTest(ModuleIdentifier parentModuleID)
			{
				ParentModuleID = parentModuleID;
			}

			protected internal override ZFilterModule NewModuleFromModuleID()
			{
				var result = filterModuleForTesting ?? base.NewModuleFromModuleID();
				DummyModule = result as DummyFilterGridModule;
				return result;
			}

			void CodeBoxBinding_Format(object sender, ConvertEventArgs e)
			{
				IFindBox.Description = DescriptionFromCode(e.Value.ToString());
				e.Value = e.Value.ToString();
			}

			void CodeBoxBinding_Parse(object sender, ConvertEventArgs e)
			{
				e.Value = new ZString(e.Value);
			}

			protected string DescriptionFromCode(string code)
				=> IFindBox.ListProvider.DescriptionFromCode(code)
					?? InvalidDescription(code);

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (DummyModule != null)
					{
						DummyModule.Dispose();
					}
				}
				base.Dispose(disposing);
			}
		}

		[CodeProperty("CalculatedCode")]
		class DummyBusinessObjectWithCalculatedCode : DummyBusinessObject
		{
			public DummyBusinessObjectWithCalculatedCode(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[CargoWise.ComponentModel.MaxLength(10)]
			public ZString CalculatedCode { get; set; }

			public ZPropertyInfo CalculatedCodeInfo
			{
				get { return GetZPropertyInfo(nameof(CalculatedCode)); }
			}
		}

		[CodeProperty("CalculatedCode")]
		class DummyBizoWithReadOnlyCode : DummyBusinessObject
		{
			public DummyBizoWithReadOnlyCode(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString CalculatedCode => "ImReadOnly";
		}

		[CodeProperty("CalculatedCode")]
		class DummyBizoWithNoInfoCode : DummyBusinessObject
		{
			public DummyBizoWithNoInfoCode(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString CalculatedCode { get; set; }
		}

		[CodeProperty("Code")]
		class DummyBusinessObjectWithNumberFountainCode : DummyBusinessObject
		{
			public DummyBusinessObjectWithNumberFountainCode(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[NotDefaultingPropertyValue]
			[CargoWise.ComponentModel.MaxLength(10)]
			public ZString Code { get; set; }

			public ZPropertyInfo CodeInfo
			{
				get { return GetZPropertyInfo(nameof(Code)); }
			}
		}

		#endregion

		#region Implementation

		ZChildForm Form
		{
			get { return form ?? (form = new ZChildForm()); }
		}
		ZChildForm form;

		protected override void CreateControls(ZChildForm testForm, string acceptableBindForTextBox)
		{
			base.CreateControls(testForm, "");
			ZPopupFindBox.ModuleID = DummyModuleIDs.Dummy;
			ZPopupFindBox.ShowNewFormWhenEmpty = true;
			ZPopupFindBox.AllowNewForm = true;
		}

		protected override ZFindBoxUserControl NewFindBoxTester
		{
			get { return new TestPopupFindBox(); }
		}

		TestPopupFindBox ZPopupFindBox
		{
			get { return (TestPopupFindBox)FindBox; }
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
