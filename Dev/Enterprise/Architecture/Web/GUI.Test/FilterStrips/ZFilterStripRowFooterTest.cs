using System;
using System.Reflection;
using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;
using Enterprise.ZArchitecture.Web.Business.FilterStrips.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	sealed class ZFilterStripRowFooterTest : TestCaseWithFactory
	{
		#region TestRowContent

		public void TestRowContent()
		{
			AssertEquals(3, Row.Cells.Count);

			// cell 1
			AssertEquals(6, Row.Cells[0].Controls.Count);
			Assert(Row.Cells[0].Controls[0] is HtmlInputHidden);
			Assert(Row.Cells[0].Controls[1] is ZHyperlink);
			Assert(Row.Cells[0].Controls[2] is ManageLayoutsPopup);
			Assert(Row.Cells[0].Controls[3] is SaveLayoutPopup);
			Assert(Row.Cells[0].Controls[4] is ZButton);
			Assert(Row.Cells[0].Controls[5] is ZButton);
			AssertEquals(Row.Cells[0].Width, (ZFilterStripConstants.Cells.FilterDescriptionWidth + ZFilterStripConstants.Cells.FilterClauseWidth).ToString());

			// cell 3
			AssertEquals(2, Row.Cells[1].Controls.Count);
			Assert(Row.Cells[1].Controls[0] is HtmlInputHidden);
			Assert(Row.Cells[1].Controls[1] is ZFilterLayoutDropDownList);

			// cell 4
			AssertEquals(2, Row.Cells[2].Controls.Count);
			Assert(Row.Cells[2].Controls[0] is ZButton);
			Assert(Row.Cells[2].Controls[1] is ZButton);
		}

		#endregion

		#region TestClientEventsAssignedOnLoad

		public void TestClientEventsAssignedOnLoad()
		{
			AssertEquals("Precondition", true, ((ZString)Row.ResetLayoutButton.OnClientClick).IsEmpty);
			Row.OnLoad();

			AssertEquals("Precondition", false, ((ZString)Row.ResetLayoutButton.OnClientClick).IsEmpty);
		}

		#endregion

		#region TestLayoutRelatedControlsAreShownOnlyIfAuthenticated
		public void TestLayoutLoadingAndSavingControlsAreOnlyVisibleIfAuthenticated()
		{
			using (ZPage testPage = new ZPage())
			{
				try
				{
					testPage.Controls.Add(Row);
					Row.OnPreRender();
					Assert("Controls are not visible if User was not logged in", Row.AssertLayoutControlsVisibility(false));

					OrgHeader company = Factory.New<OrgHeader>();
					company.OH_Code = "XXXXX";
					Factory.Save();
					testPage.SiteUser.LoginSupportForTest(company.OH_Code);
					Row.OnPreRender();
					Assert("Controls are visible if User has logged in", Row.AssertLayoutControlsVisibility(true));

					testPage.SiteUser.Logout();
					Row.OnPreRender();
					Assert("Controls are not visible if User has logged out", Row.AssertLayoutControlsVisibility(false));
				}
				finally
				{
					testPage.Controls.Remove(Row);
				}
			}
		}

		#endregion

		#region TestBind

		public void TestBind()
		{
			ZFilterStripRowFooterForTest newRow = new ZFilterStripRowFooterForTest();
			var scrappy = TestDataHelper.NewLayoutWithUserData("Scrappy", Contact.PK, OrgContactSchema.Constants.Prefix);
			var scooby = TestDataHelper.NewLayoutWithUserData("Scooby", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			FilterStripBizO.LayoutsHelper.CurrentLayout = scrappy;
			AssertEquals("Precondition", "", newRow.FilterLayoutsDropList.Text);

			newRow.Bind(FilterStripBizO);
			AssertEquals("Scrappy", newRow.FilterLayoutsDropList.Text);

			bool eventFired = false;
			newRow.LayoutChanged += delegate
			{ eventFired = true; };
			FilterStripBizO.LayoutsHelper.CurrentLayout = scooby;
			AssertEquals("Binding should hook up the CurrentLayoutNameChanged event.", true, eventFired);
			AssertEquals("Drop list should display unique code", newRow.FilterLayoutsDropList.DisplayStyle, OComboBoxDropDownStyle.CodeOnly);
		}

		#endregion

		#region TestHasUserLoadedFilterLayout

		public void TestHasUserLoadedFilterLayout()
		{
			AssertEquals("Should be false by default", false, Row.HasUserLoadedLayout);

			Row.HiddenUserLoadedFilterLayoutInput.Value = "some crap";
			AssertEquals(false, Row.HasUserLoadedLayout);

			Row.HiddenUserLoadedFilterLayoutInput.Value = Row.FilterIsNotUnsavedFlagText;
			AssertEquals(true, Row.HasUserLoadedLayout);
		}

		#endregion

		#region TestResetUserLoadedFilterLayout

		public void TestResetUserLoadedFilterLayout()
		{
			Row.HiddenUserLoadedFilterLayoutInput.Value = Row.FilterIsNotUnsavedFlagText;
			AssertEquals("Precondition", true, Row.HasUserLoadedLayout);

			Row.ResetHasUserLoadedLayout();
			AssertEquals(false, Row.HasUserLoadedLayout);
		}

		#endregion

		#region TestResetLayoutEvent()

		public void TestResetLayoutEvent()
		{
			bool eventFired = false;

			Row.ResetLayout += delegate
			{
				eventFired = true;
			};

			Row.ClickResetLayoutButton();
			AssertEquals("Clicking the Reset button should fire the ResetLayout event.", true, eventFired);
		}

		#endregion

		#region TestFindButtonClickEvent()

		public void TestFindButtonClickEvent()
		{
			bool eventFired = false;

			Row.FindButtonClick += delegate
			{
				eventFired = true;
			};

			Row.ClickFindButton();
			AssertEquals("Clicking the Find button should fire the FindButtonClick event.", true, eventFired);
		}

		#endregion

		#region TestFindButtonFunctionIsDisabledDuringPostBack

		public void TestFindButtonFunctionIsDisabledDuringPostBack()
		{
			AssertEquals("Strips should be formed before clicking Find button", @"if (typeof(Sys) != ""undefined"") return !Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack(); else return true;", Row.FindButton.OnClientClick);
		}

		#endregion

		#region TestLayoutChangedEvent()

		public void TestLayoutChangedEvent()
		{
			var scrappy = TestDataHelper.NewLayoutWithUserData("Scrappy", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			Row.ResetLayoutButton.OnClientClick = "";
			AssertEquals("Precondition", "", Row.ResetLayoutButton.OnClientClick);

			bool eventFired = false;
			Row.LayoutChanged += delegate(object sender, LayoutEventArgs e)
			{
				AssertEquals("Scrappy", e.Layout.S9_FilterName);
				eventFired = true;
			};

			FilterStripBizO.LayoutsHelper.CurrentLayout = scrappy;
			AssertEquals("Changing the selected layout should fire the LayoutChanged event.", true, eventFired);
		}

		#endregion

		#region TestSaveLayoutButtonTextDidNotChange

		[ExpectNoExceptions]
		public void AssertSaveLayoutButtonTextDidNotChange(string layoutName, bool published)
		{
			using (var resourceStrings = Res.UseMockData())
			{
				var layoutNameForComparison = published ? "[TranslatedName]" : "TranslatedName";
				resourceStrings.SetResourceGetter(key => new ResourceStringData(key, layoutNameForComparison));
				var layout = FilterStripBizO.Layouts.AddNew();
				AssertEquals("Precondition", false, layout.S9_IsPublished);
				layout.S9_IsPublished = published;
				layout.S9_FilterName = layoutName;
				AssertEquals("FilterName should not change", layout.S9_FilterName, layoutName);
				AssertEquals("Bracketed-layout-name should have brackets for published and no-brackets for unpublished", layout.DisplayName, layoutNameForComparison);
				Row.SaveLayoutButton.TextBoxControl.Text = layoutName;
				Row.SaveLayoutButton.IsPublishedTextBoxControl.Text = published ? "Y" : "N";
				Row.fSaveLayoutButton_TextChanged(Row, EventArgs.Empty);
			}
		}

		public void TestSaveLayoutButtonTextChanged()
		{
			CombineAssertions(() =>
			{
				AssertSaveLayoutButtonTextDidNotChange("Layout unpublished", false);
				AssertSaveLayoutButtonTextDidNotChange("Layout published", true);
			});
		}

		#endregion

		#region TestClearFiltersEvent()

		public void TestClearFiltersEvent()
		{
			bool eventFired = false;

			Row.ClearFilters += delegate
			{
				eventFired = true;
			};

			Row.ClickClearButton();
			AssertEquals("Clicking the Clear button should fire the ClearFilters event.", true, eventFired);
		}

		#endregion

		#region TestAddFilterStripButtonClickEvent()

		public void TestAddFilterStripButtonClickEvent()
		{
			bool eventFired = false;

			Row.AddFilterStripButtonClick += delegate
			{
				eventFired = true;
			};

			Row.ClickAddButton();
			AssertEquals("Clicking the [+] button should fire the AddFilterStripButtonClick event.", true, eventFired);
		}

		#endregion

		#region Implementation

		ZFilterStripRowFooterForTest Row
		{
			get
			{
				if (fRow == null)
				{
					fRow = new ZFilterStripRowFooterForTest();
					fRow.Bind(FilterStripBizO);
				}

				return fRow;
			}
		}

		DummyFilterStripBusinessObjectForWeb FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					fFilterStripBizO = new DummyFilterStripBusinessObjectForWeb();
					fFilterStripBizO.LayoutsHelper = new FilterStripLayoutsHelperForWeb(fFilterStripBizO, Contact);
				}
				return fFilterStripBizO;
			}
		}

		LayoutsTestDataHelper TestDataHelper
		{
			get { return fTestDataHelper ?? (fTestDataHelper = new LayoutsTestDataHelper(Factory)); }
		}

		OrgContact Contact
		{
			get { return fContact ?? (fContact = Factory.NewWithValidTestData<OrgContact>()); }
		}

		ZFilterStripRowFooterForTest fRow;
		DummyFilterStripBusinessObjectForWeb fFilterStripBizO;
		LayoutsTestDataHelper fTestDataHelper;
		OrgContact fContact;

		#region class ZFilterStripRowFooterForTest

		class ZFilterStripRowFooterForTest : ZFilterStripRowFooter
		{
			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}

			public void OnPreRender()
			{
				base.OnPreRender(EventArgs.Empty);
			}

			public bool AssertLayoutControlsVisibility(bool visibility)
			{
				return
					HiddenLayoutNameInput.Visible == visibility &&
					HelpHyperlink.Visible == visibility &&
					ManageLayoutsButton.Visible == visibility &&
					SaveLayoutButton.Visible == visibility &&
					FilterLayoutsDropList.Visible == visibility &&
					ResetLayoutButton.Visible == visibility;
			}

			public new HtmlInputHidden HiddenLayoutNameInput
			{
				get { return base.HiddenLayoutNameInput; }
			}

			public new HtmlInputHidden HiddenUserLoadedFilterLayoutInput
			{
				get { return base.HiddenUserLoadedFilterLayoutInput; }
			}

			public new ZButton ResetLayoutButton
			{
				get { return base.ResetLayoutButton; }
			}

			public new string FilterIsNotUnsavedFlagText
			{
				get { return ZFilterStripRowFooter.FilterIsNotUnsavedFlagText; }
			}

			public new SaveLayoutPopup SaveLayoutButton
			{
				get { return base.SaveLayoutButton; }
			}

			#region Clicking Buttons

			public void ClickResetLayoutButton()
			{
				ClickButton(ResetLayoutButton);
			}

			public void ClickFindButton()
			{
				ClickButton(FindButton);
			}

			public void ClickClearButton()
			{
				ClickButton(ClearButton);
			}

			public void ClickAddButton()
			{
				ClickButton(AddButton);
			}

			void ClickButton(ZButton button)
			{
				MethodInfo method = button.GetType().GetMethod("OnClick", BindingFlags.NonPublic | BindingFlags.Instance);
				method.Invoke(button, new object[] { EventArgs.Empty });
			}

			#endregion

		}

		#endregion

		#endregion
	}
}
