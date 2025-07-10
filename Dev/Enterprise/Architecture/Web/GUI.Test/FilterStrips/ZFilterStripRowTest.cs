using System;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using ZDateTime = CargoWise.Types.ZDateTime;
using ZDecimal = CargoWise.Types.ZDecimal;
using ZGuid = CargoWise.Types.ZGuid;
using ZString = CargoWise.Types.ZString;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	sealed class ZFilterStripRowTest : TestCaseWithDummy
	{
		#region TestCategoryTrianglesAreNotVisibleForAnonymous

		public void TestCategoryTrianglesAreNotVisibleForAnonymous()
		{
			ZFilterStripRowForTest newRow = new ZFilterStripRowForTest();
			using (ZPage testPage = new ZPage())
			{
				testPage.Controls.Add(newRow);
				newRow.OnPreRender();
				Assert("Triangle is not visible if User was not logged in", newRow.AssertTriangleVisibility(false));

				OrgHeader company = Factory.New<OrgHeader>();
				company.OH_Code = "XXXXX";
				Factory.Save();
				testPage.SiteUser.LoginSupportForTest(company.OH_Code);
				newRow.OnPreRender();
				Assert("Triangle is visible if User has logged in", newRow.AssertTriangleVisibility(true));

				testPage.SiteUser.Logout();
				newRow.OnPreRender();
				Assert("Triangle is not visible if User has logged out", newRow.AssertTriangleVisibility(false));
			}
		}

		#endregion

		#region TestRowContent

		public void TestRowContent()
		{
			AssertEquals(4, Row.Cells.Count);

			// cell 1
			AssertEquals(1, Row.Cells[0].Controls.Count);
			Assert(Row.Cells[0].Controls[0] is ZFilterStripDropDownList);

			// cell 2
			AssertEquals(1, Row.Cells[1].Controls.Count);

			// cell 3
			AssertEquals(1, Row.Cells[2].Controls.Count);

			// cell 4
			AssertEquals(2, Row.Cells[3].Controls.Count);
			Assert(Row.Cells[3].Controls[0] is ZButton);
			Assert(Row.Cells[3].Controls[1] is ChangeCategoryPopup);
		}

		#endregion

		#region TestPk

		public void TestPk()
		{
			Row.Bind(FilterStrip);
			AssertEquals(FilterStrip.PK, Row.PK);
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			SelectTextFilter();
			AssertCollectionContains("Precondition", Row, Table.Rows);
			AssertEquals("Precondition", false, FilterStrip.IsDeleted);
			AssertCollectionContains("Precondition", FilterStrip, FilterStrips);
			AssertEquals("Precondition", "text filter", FilterStrip.FilterDescription);

			bool eventFired = false;
			Row.FilterStripDeleted += delegate
			{ eventFired = true; };
			Row.Delete();
			AssertCollectionNotContains(Row, Table.Rows);
			AssertEquals(true, FilterStrip.IsDeleted);
			AssertCollectionNotContains(FilterStrip, FilterStrips);
			AssertEquals(FilterStrip.SelectFilterDescriptionText, FilterStrip.FilterDescription);
			AssertEquals(true, eventFired);
		}

		#endregion

		#region TestClear

		public void TestClear()
		{
			SelectTextFilter();
			int filterControlHash = FilterControls[0].GetHashCode();
			TextFilter.Property = "abc";
			AssertEquals("Precondition", "abc", TextFilter.Property);

			Row.Clear();
			AssertEquals("", TextFilter.Property);
			AssertNotEquals("The filter controls should be rebuilt on clear.", filterControlHash, FilterControls[0].GetHashCode());
		}

		#endregion

		#region TestIsOnlyStrip

		public void TestIsOnlyStrip()
		{
			Row.IsOnlyStrip = true;
			AssertEquals(false, Row.DeleteButton.Enabled);

			Row.IsOnlyStrip = false;
			AssertEquals(true, Row.DeleteButton.Enabled);
		}

		#endregion

		#region TestFilterDescriptionChangedIsFiredOnFilterChange

		public void TestFilterDescriptionChangedIsFiredOnFilterChange()
		{
			bool eventFired = false;
			Row.FilterDescriptionChanged += delegate
			{
				eventFired = true;
			};

			SelectTextFilter();
			int filterControlHash = FilterControls[0].GetHashCode();
			SelectDateFilter();
			AssertNotEquals("The filter controls should be rebuilt on clear.", filterControlHash, FilterControls[0].GetHashCode());
			AssertEquals("Row.FilterDescriptionChanged should fire when the FilterStrip.FilterDescription changes.", true, eventFired);
		}

		#endregion

		#region TestTextFilterWithListAllowsEmptySelection

		public void TestTextFilterWithListAllowsEmptySelection()
		{
			FilterStrip.FilterDescription = "text filter with list";
			TextFilterWithList.Property = "";
			Row.Bind(FilterStrip);

			AssertEquals(true, ((ZDropDownList)FilterControls[0]).ShowEmptyItem);
			AssertEquals("", ((ZDropDownList)FilterControls[0]).Text);
		}

		#endregion

		#region TestControlsToCaptureReturnKey

		void AssertNumberOfControlsToCaptureReturnKey(int expectedControlsCount, ModuleFilter filter)
		{
			Row.Clear();
			FilterStrip.FilterDescription = filter.Description;
			Row.Bind(FilterStrip);

			AssertEquals(string.Format("Number of controls to capture return key for \"{0}\" filter does not match expected value", filter.Description), expectedControlsCount, Row.ControlsToCaptureReturnKey.Count);
		}

		public void TestControlsToCaptureReturnKey()
		{
			AssertNumberOfControlsToCaptureReturnKey(1, TextFilter);
			AssertNumberOfControlsToCaptureReturnKey(1, NkFilter);
			AssertNumberOfControlsToCaptureReturnKey(2, TextAndNkFilter);
			AssertNumberOfControlsToCaptureReturnKey(2, DateFilter);
			AssertNumberOfControlsToCaptureReturnKey(1, GuidFilter);
			AssertNumberOfControlsToCaptureReturnKey(2, GuidsFilter);
			AssertNumberOfControlsToCaptureReturnKey(2, LocationFilter);
			AssertNumberOfControlsToCaptureReturnKey(2, NumberRangeFilter);
		}

		#endregion

		#region TestBind

		public void TestBind()
		{
			SelectTextFilter();

			ZFilterStripRowForTest newRow = new ZFilterStripRowForTest();
			AssertEquals("Precondition", 0, newRow.Cells[2].Controls.Count);
			AssertEquals("Precondition", "", newRow.FilterDescriptionDropList.Text);

			newRow.Bind(FilterStrip);
			AssertEquals(FilterStrip.PK, newRow.PK);
			AssertEquals("Bind() should have built the filter controls.", 1, newRow.Cells[2].Controls.Count);
			AssertEquals("Bind() should have bound the FilterDescriptionDropList.", "text filter", newRow.FilterDescriptionDropList.Text);

			bool eventHooked = false;
			newRow.FilterDescriptionChanged += delegate
			{ eventHooked = true; };
			SelectDateFilter();
			AssertEquals("Bind should hook up the FilterStrip.FilterDescriptionChanged event.", true, eventHooked);
		}

		#endregion

		#region TestBindingEachModuleFilter

		public void TestBindingTextFilterWithList_QueryCommon()
		{
			FilterStrip.FilterDescription = "text filter with list and query";
			TextFilterWithListAndQueryDelegateCommon.Property = "Grouchy";
			Row.Bind(FilterStrip);

			AssertEquals(1, FilterControls.Count);

			var dropDownList = (ZDropDownList)FilterControls[0];
			AssertEquals(true, dropDownList.Enabled);
			AssertEquals(OComboBoxDropDownStyle.DescriptionOnly, dropDownList.DisplayStyle);
			AssertEquals("Grouchy Description", dropDownList.Text);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_TextWithList", FilterControls[0].ID);

			AssertEquals(0, FilterClauseControls.Count);
		}

		public void TestBindingTextFilterWithList_QueryOperator()
		{
			FilterStrip.FilterDescription = "text filter with list and query and operator";
			TextFilterWithListAndQueryDelegateOperator.Property = "Grouchy";
			Row.Bind(FilterStrip);

			AssertEquals(1, FilterControls.Count);

			var dropDownList = (ZDropDownList)FilterControls[0];
			AssertEquals(true, dropDownList.Enabled);
			AssertEquals(OComboBoxDropDownStyle.DescriptionOnly, dropDownList.DisplayStyle);
			AssertEquals("Grouchy Description", dropDownList.Text);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_TextWithList", FilterControls[0].ID);

			AssertEquals(1, FilterClauseControls.Count);
		}

		public void TestBindingTextFilterWithList()
		{
			FilterStrip.FilterDescription = "text filter with list";
			TextFilterWithList.Property = "Grouchy";
			Row.Bind(FilterStrip);

			var dropDownList = (ZDropDownList)FilterControls[0];
			AssertEquals(true, dropDownList.Enabled);
			AssertEquals(OComboBoxDropDownStyle.DescriptionOnly, dropDownList.DisplayStyle);
			AssertEquals("Grouchy Description", dropDownList.Text);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_TextWithList", FilterControls[0].ID);

			AssertEquals(0, FilterClauseControls.Count);
		}

		public void TestBindingNkFilter()
		{
			FilterStrip.FilterDescription = "nk filter";
			NkFilter.Property = "Handy";

			Row.Bind(FilterStrip);
			AssertEquals("Handy", ((ZFindBox)FilterControls[0]).Text);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_NK", FilterControls[0].ID);
		}

		public void TestBindingTextAndNkFilter()
		{
			FilterStrip.FilterDescription = "text & nk filter";
			TextAndNkFilter.NkProperty = "Gargamel";
			TextAndNkFilter.Property = "Azrael";
			Row.Bind(FilterStrip);

			AssertEquals("Gargamel", ((ZFindBox)FilterControls[0].Controls[0].Controls[1].Controls[1]).Text);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_NK", FilterControls[0].Controls[0].Controls[1].Controls[1].ID);
			AssertEquals("Azrael", ((WebControls.ZTextBox)FilterControls[0].Controls[0].Controls[0].Controls[0]).Text);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_Text", FilterControls[0].Controls[0].Controls[0].Controls[0].ID);
		}

		public void TestBindingTextFilter()
		{
			SelectTextFilter();
			TextFilter.Property = "Brainy";
			Row.Bind(FilterStrip);

			AssertEquals("Brainy", ((WebControls.ZTextBox)FilterControls[0]).Text);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_Text", FilterControls[0].ID);
		}

		public void TestTextFilterLocalization()
		{
			const string hao = "好";
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter(delegate(string key)
				{ return new ResourceStringData(key, hao); });

				SelectTextFilter();
				TextFilter.Property = "Hao";
				Row.Bind(FilterStrip);

				var dropDown = (ZDropDownList)Row.Cells[1].Controls[0];
				foreach (ListItem item in dropDown.Items)
				{
					AssertEquals("Text should be localized", hao, item.Text);
					AssertNotContains("Value should not be localized", hao, item.Value);
				}
			}
		}

		public void TestBindingWorkflowTextFilter()
		{
			Assert("Precondition", WorkflowTextFilter.MilestoneEventTypes.Count > 1);

			SelectWorkflowTextFilter();
			ICodeDescription milestoneEventType = WorkflowTextFilter.MilestoneEventTypes[1];
			WorkflowTextFilter.MilestoneEvent = milestoneEventType.Code;
			CodeDescriptionPair pair = new CodeDescriptionPair("FRB", "Fireblade");
			WorkflowTextFilter.List.Add(pair);
			WorkflowTextFilter.Property = pair.Code;
			WorkflowTextFilter.EventReference = "Meh";
			Row.Bind(FilterStrip);

			var milestoneDDL = (ZDropDownList)FilterControls[0];
			AssertEquals(4, FilterControls.Count);
			AssertEquals(true, milestoneDDL.Enabled);
			AssertEquals(OComboBoxDropDownStyle.DescriptionOnly, milestoneDDL.DisplayStyle);
			AssertEquals(milestoneEventType.Description, milestoneDDL.Text);
			AssertEquals(pair.Description, ((ZDropDownList)FilterControls[2]).Text);

			var hiddenField = (HiddenField)FilterControls[1];
			AssertEquals(milestoneDDL.ID + "_SelectedIndex", hiddenField.ID);
			Assert(Page.ZClientScript.IsClientScriptBlockRegistered(typeof(ZDropDownList), ZDropDownList.StoreSelectedIndexScriptKey));

			var duplicateItem = new ListItem("Duplicate", milestoneDDL.Items[0].Value);
			milestoneDDL.Items.Add(duplicateItem);
			milestoneDDL.SelectedValue = duplicateItem.Value;
			var expectedIndex = milestoneDDL.Items.Count - 1;
			hiddenField.Value = expectedIndex.ToString();
			typeof(HiddenField).InvokeMember("RaisePostDataChangedEvent", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null, hiddenField, null);
			AssertEquals(expectedIndex, milestoneDDL.SelectedIndex);

			var container = (HtmlGenericControl)FilterControls[3];
			AssertEquals(3, container.Controls.Count);
			AssertEquals(WorkflowTextFilter.EventReference, ((WebControls.ZTextBox)container.Controls[2]).Text);
			AssertEquals("EventReferenceComparisonOption", ((ZDropDownList)container.Controls[1]).BindTo);
		}

		public void TestWarehouseLocationFilter()
		{
			SelectWarehouseLocationFilter();
			WarehouseLocationFilter.Location = "A-1-1-1";
			Row.Bind(FilterStrip);

			AssertEquals(Warehouse.WW_WarehouseCode, ((ZGuidDropDownList)GetControlFromFilterControls(0, 0, 0)).Items[1].Text);
			AssertEquals(Warehouse.PK.ToString(), ((ZGuidDropDownList)GetControlFromFilterControls(0, 0, 0)).Items[1].Value);
			AssertEquals(WarehouseLocationFilter.Location, ((WebControls.ZTextBox)GetControlFromFilterControls(0, 1, 0)).Text);
		}

		public void TestReferenceNumberFilter()
		{
			SelectReferenceNumberFilter();

			CodeDescriptionPair pair = new CodeDescriptionPair("CCN", "Cargo Control Number");
			ReferenceNumberFilter.Types.Add(pair);
			ReferenceNumberFilter.Type = pair.Code;
			ReferenceNumberFilter.Property = "12345";
			ReferenceNumberFilter.Country = "CA";
			Row.Bind(FilterStrip);

			AssertEquals(ReferenceNumberFilter.Property, ((WebControls.ZTextBox)GetControlFromFilterControls(0, 0, 0)).Text);
			AssertEquals(ReferenceNumberFilter.Country, ((ZFindBox)GetControlFromFilterControls(0, 1, 1)).Text);
			AssertEquals(pair.Code, ((ZDropDownList)GetControlFromFilterControls(0, 1, 3)).Text);

			var countryLabel = (ZTextLabel)GetControlFromFilterControls(0, 1, 0);
			AssertEquals("&nbsp;&nbsp;Country/Region:", countryLabel.Text);
			AssertEquals(false, countryLabel.EnableHtmlEncoding);

			var typeLabel = (ZTextLabel)GetControlFromFilterControls(0, 1, 2);
			AssertEquals("&nbsp;&nbsp;Type:", typeLabel.Text);
			AssertEquals(false, typeLabel.EnableHtmlEncoding);
		}

		public void TestVoyageVesselModuleFilter()
		{
			SelectVoyageVesselModuleFilter();

			VoyageVesselFilter.Property = "Vessel1";
			VoyageVesselFilter.Vessel = "Vessel1";

			Row.Bind(FilterStrip);

			AssertEquals(VoyageVesselFilter.Property, ((WebControls.ZTextBox)GetControlFromFilterControls(0, 0, 0)).Text);
			AssertEquals(VoyageVesselFilter.Vessel, ((ZFindBox)GetControlFromFilterControls(0, 1, 1)).Text);
		}

		public void TestEntryNumberModuleFilter()
		{
			SelectEntryNumberModuleFilter();

			EntryNumberModuleFilter.EntryType = "CAN";
			EntryNumberModuleFilter.Property = "12345";
			Row.Bind(FilterStrip);

			AssertEquals("CAN", ((ZDropDownList)GetControlFromFilterControls(0, 0, 0)).Text);
			AssertEquals(EntryNumberModuleFilter.Property, ((WebControls.ZTextBox)GetControlFromFilterControls(0, 1, 0)).Text);

			EntryNumberModuleFilter.EntryType = "EXDC";
			AssertEquals(ZString.Empty, EntryNumberModuleFilter.Property);
			AssertEquals(true, EntryNumberModuleFilter.PropertyInfo.ReadOnly);

			Row.Bind(FilterStrip);

			AssertEquals("EXDC", ((ZDropDownList)GetControlFromFilterControls(0, 0, 0)).Text);
			AssertEquals(ZString.Empty, ((WebControls.ZTextBox)GetControlFromFilterControls(0, 1, 0)).Text);
		}

		public void TestChargeModuleFilter()
		{
			SelectChargeModuleFilter();

			ChargeModuleFilter.ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			ChargeModuleFilter.UseLowerBound = true;
			ChargeModuleFilter.UseUpperBound = true;
			ChargeModuleFilter.UpperBound = 29M;
			ChargeModuleFilter.LowerBound = 18M;

			Row.Bind(FilterStrip);

			var dropDownList = (ZDropDownList)FilterControls[0];
			AssertEquals(true, dropDownList.Enabled);
			AssertEquals(OComboBoxDropDownStyle.DescriptionOnly, dropDownList.DisplayStyle);
			AssertEquals(ChargeCodeGroupList.Descriptions.Brokerage, dropDownList.Text);

			AssertEquals(true, ((ZCheckBox)GetControlFromFilterControls(1, 0, 0)).Checked);
			AssertEquals(true, ((ZCheckBox)GetControlFromFilterControls(1, 1, 0)).Checked);

			AssertEquals(18M, ZDecimal.Parse(((ZNumericTextBox)GetControlFromFilterControls(1, 0, 1)).Text));
			AssertEquals(29M, ZDecimal.Parse(((ZNumericTextBox)GetControlFromFilterControls(1, 1, 1)).Text));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestBindingWorkflowFilter()
		{
			Assert("Precondition", WorkflowFilter.MilestoneEventTypes.Count > 1);
			Assert("Precondition", WorkflowFilter.PropertySearch_List["Date range"] != null);
			Assert("Precondition", WorkflowFilter.PropertySearch_List["Today"] != null);

			SelectWorkflowFilter();
			var milestoneEventType = WorkflowFilter.MilestoneEventTypes[1];
			WorkflowFilter.MilestoneEvent = milestoneEventType.Code;
			WorkflowFilter.PropertySearch = WorkflowFilter.PropertySearch_List["Date range"].Code;
			WorkflowFilter.Property1 = new ZDateTime(2006, 6, 15);
			WorkflowFilter.Property2 = new ZDateTime(2007, 6, 15);
			WorkflowFilter.EventReference = "Meh";
			Row.Bind(FilterStrip);

			var milestoneDDL = (ZDropDownList)FilterControls[0];
			AssertEquals(4, FilterControls.Count);
			AssertEquals(true, milestoneDDL.Enabled);
			AssertEquals(OComboBoxDropDownStyle.DescriptionOnly, milestoneDDL.DisplayStyle);
			AssertEquals(milestoneEventType.Description, milestoneDDL.Text);
			var dateControl1 = (ZDateEdit)GetControlFromFilterControls(2, 0, 3);
			var dateControl2 = (ZDateEdit)GetControlFromFilterControls(2, 1, 1);
			var dropDownList = (ZFilterStripDropDownList)GetControlFromFilterControls(2, 0, 0);
			AssertEquals("15-Jun-06", dateControl1.Text);
			AssertEquals("15-Jun-07", dateControl2.Text);
			Assert(dropDownList.IsDescriptionsList);
			Assert(dropDownList.ShowEmptyItem);

			var hiddenField = (HiddenField)FilterControls[1];
			AssertEquals(milestoneDDL.ID + "_SelectedIndex", hiddenField.ID);
			Assert(Page.ZClientScript.IsClientScriptBlockRegistered(typeof(ZDropDownList), ZDropDownList.StoreSelectedIndexScriptKey));

			var duplicateItem = new ListItem("Duplicate", milestoneDDL.Items[0].Value);
			milestoneDDL.Items.Add(duplicateItem);
			milestoneDDL.SelectedValue = duplicateItem.Value;
			var expectedIndex = milestoneDDL.Items.Count - 1;
			hiddenField.Value = expectedIndex.ToString();
			typeof(HiddenField).InvokeMember("RaisePostDataChangedEvent", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null, hiddenField, null);
			AssertEquals(expectedIndex, milestoneDDL.SelectedIndex);

			var container = (HtmlGenericControl)FilterControls[3];
			AssertEquals(3, container.Controls.Count);
			AssertEquals(WorkflowFilter.EventReference, ((WebControls.ZTextBox)container.Controls[2]).Text);
			AssertEquals("EventReferenceComparisonOption", ((ZDropDownList)container.Controls[1]).BindTo);

			WorkflowFilter.PropertySearch = WorkflowFilter.PropertySearch_List["Today"].Code;
			Row.Bind(FilterStrip);

			var dateRangeLabel = (ZCodeLookupLabel)GetControlFromFilterControls(2, 0, 2);
			AssertEquals(ZDateTime.Today.ToString("dd-MMM-yy"), dateRangeLabel.Text);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestBindingWorkflowFilter_Localization()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.Spanish))
			{
				Assert("Precondition", WorkflowFilter.MilestoneEventTypes.Count > 1);
				Assert("Precondition", WorkflowFilter.PropertySearch_List["Date range"] != null);
				Assert("Precondition", WorkflowFilter.PropertySearch_List["Today"] != null);

				SelectWorkflowFilter();
				var milestoneEventType = WorkflowFilter.MilestoneEventTypes[1];
				WorkflowFilter.MilestoneEvent = milestoneEventType.Code;
				WorkflowFilter.PropertySearch = WorkflowFilter.PropertySearch_List["Date range"].Code;
				WorkflowFilter.Property1 = new ZDateTime(2006, 6, 15);
				WorkflowFilter.Property2 = new ZDateTime(2007, 6, 15);
				Row.Bind(FilterStrip);

				var milestoneDDL = (ZDropDownList)FilterControls[0];
				AssertEquals(true, milestoneDDL.Enabled);
				AssertEquals(OComboBoxDropDownStyle.DescriptionOnly, milestoneDDL.DisplayStyle);
				AssertEquals(milestoneEventType.Description, milestoneDDL.Text);
				var dateControl1 = (ZDateEdit)GetControlFromFilterControls(2, 0, 3);
				var dateControl2 = (ZDateEdit)GetControlFromFilterControls(2, 1, 1);
				var dropDownList = (ZFilterStripDropDownList)GetControlFromFilterControls(2, 0, 0);
				AssertEquals("15-Jun-06", dateControl1.Text);
				AssertEquals("15-Jun-07", dateControl2.Text);
				Assert(dropDownList.IsDescriptionsList);
				Assert(dropDownList.ShowEmptyItem);

				WorkflowFilter.PropertySearch = WorkflowFilter.PropertySearch_List["Today"].Code;
				Row.Bind(FilterStrip);

				var dateRangeLabel = (ZCodeLookupLabel)GetControlFromFilterControls(2, 0, 2);
				AssertEquals(ZDateTime.Today.ToString("dd-MMM-yy"), dateRangeLabel.Text);
			}
		}

		public void TestBindingWorkflowDaysOffsetRangeFilter()
		{
			SelectWorkflowFilter();
			WorkflowFilter.PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange;
			Row.Bind(FilterStrip);

			var daysOffsetRangeControl = (ZOffsetRangeFilterControl)GetControlFromFilterControls(2, 0, 4);
			var hoursOffsetRangeControl = (ZOffsetRangeFilterControl)GetControlFromFilterControls(2, 0, 5);

			AssertNotNull(daysOffsetRangeControl);
			AssertEquals(true, daysOffsetRangeControl.Visible);

			AssertNotNull(hoursOffsetRangeControl);
			AssertEquals(false, hoursOffsetRangeControl.Visible);
		}

		public void TestBindingWorkflowHoursOffsetRangeFilter()
		{
			SelectWorkflowFilter();
			WorkflowFilter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			Row.Bind(FilterStrip);

			var daysOffsetRangeControl = (ZOffsetRangeFilterControl)GetControlFromFilterControls(2, 0, 4);
			var hoursOffsetRangeControl = (ZOffsetRangeFilterControl)GetControlFromFilterControls(2, 0, 5);

			AssertNotNull(daysOffsetRangeControl);
			AssertEquals(false, daysOffsetRangeControl.Visible);

			AssertNotNull(hoursOffsetRangeControl);
			AssertEquals(true, hoursOffsetRangeControl.Visible);
		}

		public void TestBindingtDateFilter()
		{
			SelectDateFilter();
			DateFilter.PropertySearch = DateFilter.PropertySearch_List["Date range"].Code;
			DateFilter.Property1 = new ZDateTime(2006, 7, 3);
			DateFilter.Property2 = new ZDateTime(2007, 7, 3);
			Row.Bind(FilterStrip);

			ZDateEdit dateControl1 = (ZDateEdit)GetControlFromFilterControls(0, 0, 3);
			ZDateEdit dateControl2 = (ZDateEdit)GetControlFromFilterControls(0, 1, 1);
			AssertEquals("03-Jul-06", dateControl1.Text);
			AssertEquals("03-Jul-07", dateControl2.Text);
			AssertEquals("Control ID should be based on the filter strip's PK and BindTo", FilterStrip.PK.ToString() + "_Property1", dateControl1.ID);
			AssertEquals("Control ID should be based on the filter strip's PK and BindTo", FilterStrip.PK.ToString() + "_Property2", dateControl2.ID);
			var dropDown = (ZFilterStripDropDownList)Row.Cells[1].Controls[0];
			Assert("IsDescription should be true", dropDown.IsDescriptionsList);
			Assert("ShowEmptyItem should be true", dropDown.ShowEmptyItem);
		}

		public void TestBindingDateDaysOffsetRangeFilter()
		{
			SelectDateFilter();
			DateFilter.PropertySearch = ModuleDateFilter.SpecifiedDayOffsetRange;
			Row.Bind(FilterStrip);

			var daysOffsetRangeControl = (ZOffsetRangeFilterControl)GetControlFromFilterControls(0, 0, 4);
			var hoursOffsetRangeControl = (ZOffsetRangeFilterControl)GetControlFromFilterControls(0, 0, 5);

			AssertNotNull(daysOffsetRangeControl);
			AssertEquals(true, daysOffsetRangeControl.Visible);

			AssertNotNull(hoursOffsetRangeControl);
			AssertEquals(false, hoursOffsetRangeControl.Visible);
		}

		public void TestBindingDateHoursOffsetRangeFilter()
		{
			SelectDateFilter();
			DateFilter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			Row.Bind(FilterStrip);

			var daysOffsetRangeControl = (ZOffsetRangeFilterControl)GetControlFromFilterControls(0, 0, 4);
			var hoursOffsetRangeControl = (ZOffsetRangeFilterControl)GetControlFromFilterControls(0, 0, 5);

			AssertNotNull(daysOffsetRangeControl);
			AssertEquals(false, daysOffsetRangeControl.Visible);

			AssertNotNull(hoursOffsetRangeControl);
			AssertEquals(true, hoursOffsetRangeControl.Visible);
		}

		public void TestBindingDateWorkingHoursOffsetRangeFilter()
		{
			SelectDateFilter();
			DateFilter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;
			Row.Bind(FilterStrip);

			var daysOffsetRangeControl = (ZOffsetRangeFilterControl)GetControlFromFilterControls(0, 0, 4);
			var hoursOffsetRangeControl = (ZOffsetRangeFilterControl)GetControlFromFilterControls(0, 0, 5);

			AssertNotNull(daysOffsetRangeControl);
			AssertEquals(false, daysOffsetRangeControl.Visible);

			AssertNotNull(hoursOffsetRangeControl);
			AssertEquals(true, hoursOffsetRangeControl.Visible);
		}

		public void TestBindingDateFilter_Localization()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.Spanish))
			{
				SelectDateFilter();
				DateFilter.PropertySearch = DateFilter.PropertySearch_List["Date range"].Code;
				DateFilter.Property1 = new ZDateTime(2006, 7, 3);
				DateFilter.Property2 = new ZDateTime(2007, 7, 3);
				Row.Bind(FilterStrip);

				ZDateEdit dateControl1 = (ZDateEdit)GetControlFromFilterControls(0, 0, 3);
				ZDateEdit dateControl2 = (ZDateEdit)GetControlFromFilterControls(0, 1, 1);
				AssertEquals("03-Jul-06", dateControl1.Text);
				AssertEquals("03-Jul-07", dateControl2.Text);
				AssertEquals("Control ID should be based on the filter strip's PK and BindTo", FilterStrip.PK.ToString() + "_Property1", dateControl1.ID);
				AssertEquals("Control ID should be based on the filter strip's PK and BindTo", FilterStrip.PK.ToString() + "_Property2", dateControl2.ID);
				var dropDown = (ZFilterStripDropDownList)Row.Cells[1].Controls[0];
				Assert("IsDescription should be true", dropDown.IsDescriptionsList);
				Assert("ShowEmptyItem should be true", dropDown.ShowEmptyItem);
			}
		}

		public void TestBindingGuidFilter()
		{
			Dummy.Z0_Code = "Jokey";
			FilterStrip.FilterDescription = "guid filter";
			GuidFilter.Property = Dummy.PK;
			Row.Bind(FilterStrip);

			AssertEquals("Jokey", ((ZGuidFindBox)FilterControls[0]).Text);
			AssertEquals("Control ID should be based on the filter strip's PK and WebModuleID", FilterStrip.PK.ToString() + "_" + WebModuleIDs.NotAssigned.Name, FilterControls[0].ID);
		}

		public void TestDescriptionSensitiveGuidFilter()
		{
			var filters = new ModuleFilterCollection();
			filters.AddGuidFilter("Carrier", ModuleIDs.Organisation, DummyBizoSchema.Z0_Guid, DummyList);
			var strip = new FilterStrip(filters);
			Row.Bind(strip);

			var control = (ZGuidFindBox)FilterControls[0];
			AssertEquals(WebModuleIDs.OrgCarrierTracking, control.ModuleID);
		}

		public void TestBindingGuidsFilter()
		{
			Dummy.Z0_Code = "Lazy";
			FilterStrip.FilterDescription = "guids filter";
			GuidsFilter.Property1 = Dummy.PK;
			GuidsFilter.Property2 = Dummy.PK;
			Row.Bind(FilterStrip);

			ZGuidFindBox findBox1 = (ZGuidFindBox)GetControlFromFilterControls(0, 0, 0);
			ZGuidFindBox findBox2 = (ZGuidFindBox)GetControlFromFilterControls(0, 1, 1);
			AssertEquals("Lazy", findBox1.Text);
			AssertEquals("Lazy", findBox2.Text);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_" + WebModuleIDs.NotAssigned.Name + "_1", findBox1.ID);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_" + WebModuleIDs.NotAssigned.Name + "_2", findBox2.ID);
		}

		public void TestBindingLocationsFilter()
		{
			Location1.OH_Code = "Vanit";
			Location2.OH_Code = "Hefty";
			FilterStrip.FilterDescription = "location filter";
			LocationFilter.Property1 = "Vanit";
			LocationFilter.Property2 = "Hefty";
			Row.Bind(FilterStrip);

			ZFindBox locationFindBox1 = (ZFindBox)GetControlFromFilterControls(0, 0, 0);
			ZFindBox locationFindBox2 = (ZFindBox)GetControlFromFilterControls(0, 1, 1);
			AssertEquals("Vanit", locationFindBox1.Text);
			AssertEquals("Hefty", locationFindBox2.Text);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_Location1", locationFindBox1.ID);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_Location2", locationFindBox2.ID);
		}

		public void TestBindingFlagsFilter()
		{
			FilterStrip.FilterDescription = "flags filter";
			FlagsFilter.Property0 = true;
			Row.Bind(FilterStrip);

			AssertEquals("flagName", ((ZCheckBox)FilterControls[0]).Text);
			AssertEquals(true, ((ZCheckBox)FilterControls[0]).Checked);
			AssertEquals("Control ID should be based on the filter strip's PK and check box sequental number", FilterStrip.PK.ToString() + "_0", FilterControls[0].ID);
		}

		public void TestBindingNumberRangeFilter()
		{
			FilterStrip.FilterDescription = "number range filter";
			NumberRangeFilter.Property1 = 18M;
			NumberRangeFilter.Property2 = 29M;
			Row.Bind(FilterStrip);

			AssertEquals(18M, ZDecimal.Parse(((ZNumericTextBox)FilterControls[1]).Text));
			AssertEquals(29M, ZDecimal.Parse(((ZNumericTextBox)FilterControls[3]).Text));
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_Condition_0", FilterControls[0].ID);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_1", FilterControls[1].ID);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_Condition_2", FilterControls[2].ID);
			AssertEquals("Control ID should be based on the filter strip's PK", FilterStrip.PK.ToString() + "_2", FilterControls[3].ID);
		}

		public void TestDifferentFiltersUseSameControlIdsFromFilterStripPK()
		{
			ZGuid filterStripPK = FilterStrip.PK;
			TestBindingTextAndNkFilter();
			AssertFilterControlsHaveFilterStripPkAsId();

			foreach (HtmlTableCell cell in Row.Cells)
			{
				cell.Controls.Clear();
				AssertEquals("Cell should have no controls after clearing", 0, cell.Controls.Count);
			}

			// Next test should use the Same IDs from the current FilterStrip
			AssertEquals("Keep using the Same FilterStrip", filterStripPK, FilterStrip.PK);
			TestBindingTextFilter();
			AssertFilterControlsHaveFilterStripPkAsId();
			AssertEquals("Keep using the Same FilterStrip", filterStripPK, FilterStrip.PK);
		}

		public void TestBindingTextFilter_ClauseOnPostBack()
		{
			SelectTextFilter();
			TextFilter.Property = "Brainy";
			Row.Bind(FilterStrip);

			TextFilter.Property = "Changed";
			var list = (ZDropDownList)FilterClauseControls[0];
			list.RaisePostDataChangedEventInternal();

			var textBox = (WebControls.ZTextBox)FilterControls[0];
			AssertEquals("Changed", textBox.Text);
		}

		public void TestBindingTextFilterWithList_ClauseOnPostBack()
		{
			FilterStrip.FilterDescription = "text filter with list and query and operator";
			TextFilterWithListAndQueryDelegateOperator.Property = "Grouchy";
			Row.Bind(FilterStrip);

			TextFilterWithListAndQueryDelegateOperator.Property = "Changed";
			var list = (ZDropDownList)FilterClauseControls[0];
			list.RaisePostDataChangedEventInternal();

			var dropDownList = (ZDropDownList)FilterControls[0];
			AssertEquals("Changed Description", dropDownList.Text);
		}

		#endregion

		#region Implementation

		Control GetControlFromFilterControls(int mainControlIndex, int cellIndex, int controlIndex)
		{
			return ((HtmlTable)FilterControls[mainControlIndex]).Rows[0].Cells[cellIndex].Controls[controlIndex];
		}

		void AssertFilterControlsHaveFilterStripPkAsId()
		{
			foreach (Control control in FilterControls)
			{
				Assert("Controll's ID should start from FilterStrip PK", control.ID.StartsWith(FilterStrip.PK.ToString()));
			}
		}

		#region Module Filters

		void SelectTextFilter()
		{
			FilterStrip.FilterDescription = "text filter";
		}

		void SelectWorkflowTextFilter()
		{
			FilterStrip.FilterDescription = "workflow text filter";
		}

		void SelectWarehouseLocationFilter()
		{
			FilterStrip.FilterDescription = "warehouse location filter";
		}

		void SelectReferenceNumberFilter()
		{
			FilterStrip.FilterDescription = "reference number filter";
		}

		void SelectVoyageVesselModuleFilter()
		{
			FilterStrip.FilterDescription = "Flight/Voyage # and Vessel";
		}

		void SelectEntryNumberModuleFilter()
		{
			FilterStrip.FilterDescription = "entry number module filter";
		}

		void SelectChargeModuleFilter()
		{
			FilterStrip.FilterDescription = "charge module filter";
		}

		void SelectWorkflowFilter()
		{
			FilterStrip.FilterDescription = "workflow filter";
		}

		void SelectDateFilter()
		{
			FilterStrip.FilterDescription = "date filter";
		}

		ModuleFilterCollection ModuleFilters
		{
			get
			{
				if (fModuleFilters == null)
				{
					fModuleFilters = new ModuleFilterCollection();
					fModuleFilters.AddCustomFilter(TextFilterWithList);
					fModuleFilters.AddCustomFilter(NkFilter);
					fModuleFilters.AddCustomFilter(TextAndNkFilter);
					fModuleFilters.AddCustomFilter(TextFilter);
					fModuleFilters.AddCustomFilter(WorkflowTextFilter);
					fModuleFilters.AddCustomFilter(WarehouseLocationFilter);
					fModuleFilters.AddCustomFilter(ReferenceNumberFilter);
					fModuleFilters.AddCustomFilter(VoyageVesselFilter);
					fModuleFilters.AddCustomFilter(WorkflowFilter);
					fModuleFilters.AddCustomFilter(DateFilter);
					fModuleFilters.AddCustomFilter(GuidFilter);
					fModuleFilters.AddCustomFilter(GuidsFilter);
					fModuleFilters.AddCustomFilter(LocationFilter);
					fModuleFilters.AddCustomFilter(FlagsFilter);
					fModuleFilters.AddCustomFilter(NumberRangeFilter);
					fModuleFilters.AddCustomFilter(EntryNumberModuleFilter);
					fModuleFilters.AddCustomFilter(ChargeModuleFilter);
					fModuleFilters.AddCustomFilter(TextFilterWithListAndQueryDelegateCommon);
					fModuleFilters.AddCustomFilter(TextFilterWithListAndQueryDelegateOperator);
				}
				return fModuleFilters;
			}
		}

		ModuleTextFilter TextFilterWithList
		{
			get { return fTextFilterWithList ?? (fTextFilterWithList = new ModuleTextFilter("text filter with list", DummyBizoSchema.Z0_Description, CodeDescList)); }
		}

		ModuleTextFilter TextFilterWithListAndQueryDelegateCommon
		{
			get { return fTextFilterWithListQueryCommon ?? (fTextFilterWithListQueryCommon = new ModuleTextFilter("text filter with list and query", GetQueryCommon, CodeDescList)); }
		}

		ModuleTextFilter TextFilterWithListAndQueryDelegateOperator
		{
			get { return fTextFilterWithListQueryOperator ?? (fTextFilterWithListQueryOperator = new ModuleTextFilter("text filter with list and query and operator", GetQueryWithOperator, CodeDescList)); }
		}

		ZQuery GetQueryCommon(ZString value)
		{
			return new ZQuery();
		}

		ZQuery GetQueryWithOperator(SQLComparisonOperator op, ZString value)
		{
			return new ZQuery();
		}

		ModuleNkFilter NkFilter
		{
			get { return fNkFilter ?? (fNkFilter = new ModuleNkFilter("nk filter", DummyBizoSchema.Z0_FK_Code, DummyModuleIDs.Dummy, DummyList)); }
		}

		ModuleTextAndNkFilter TextAndNkFilter
		{
			get { return fTextAndNkFilter ?? (fTextAndNkFilter = new ModuleTextAndNkFilter("text & nk filter", DummyBizoSchema.Z0_Description, DummyBizoSchema.Z0_FK_Code, DummyModuleIDs.Dummy, DummyList)); }
		}

		ModuleTextFilter TextFilter
		{
			get { return fTextFilter ?? (fTextFilter = new ModuleTextFilter("text filter", DummyBizoSchema.Z0_Description)); }
		}

		WorkflowModuleTextFilter WorkflowTextFilter
		{
			get { return fWorkflowTextFilter ?? (fWorkflowTextFilter = new WorkflowModuleTextFilter("workflow text filter", GetMilestoneTextFilter, new CodeDescriptionPairList(), typeof(DummyBusinessObject))); }
		}

		ModuleWarehouseLocationFilter WarehouseLocationFilter
		{
			get { return fWarehouseLocationFilter ?? (fWarehouseLocationFilter = new ModuleWarehouseLocationFilter("warehouse location filter", GetFilter, WarehouseList)); }
		}

		ReferenceNumberFilter ReferenceNumberFilter
		{
			get { return fReferenceNumberFilter ?? (fReferenceNumberFilter = new ReferenceNumberFilter("reference number filter", delegate { return new ZQuery(); }, CountryList)); }
		}

		VoyageVesselModuleFilter VoyageVesselFilter
		{
			get { return fVoyageVesselModuleFilter ?? (fVoyageVesselModuleFilter = new VoyageVesselModuleFilter("Flight/Voyage # and Vessel", delegate { return new ZQuery(); }, VesselList)); }
		}

		EntryNumberModuleFilter EntryNumberModuleFilter
		{
			get { return fEntryNumberModuleFilter ?? (fEntryNumberModuleFilter = new EntryNumberModuleFilter("entry number module filter", delegate { return null; })); }
		}

		ChargeModuleFilter ChargeModuleFilter
		{
			get { return fChargeModuleFilter ?? (fChargeModuleFilter = new ChargeModuleFilter("charge module filter", delegate { return null; })); }
		}

		ZQuery GetFilter(ZQuery filter)
		{
			return new ZQuery();
		}

		ZQuery GetMilestoneTextFilter(ZString value)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);

			if (value == "rightvalue")
			{
				subQuery.AddToFilter(ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			}
			else if (value == "wrongvalue")
			{
				subQuery.AddToFilter(ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.Equal, ZDateTime.Empty);
			}
			return subQuery;
		}

		WorkflowModuleFilter WorkflowFilter
		{
			get { return fWorkflowFilter ?? (fWorkflowFilter = new WorkflowModuleFilter("workflow filter", typeof(DummyBusinessObject), WorkflowModuleFilterTypes.MilestoneDate)); }
		}

		ModuleDateFilter DateFilter
		{
			get { return fDateFilter ?? (fDateFilter = new ModuleDateFilter("date filter", DummyBizoSchema.Z0_Date)); }
		}

		ModuleGuidFilter GuidFilter
		{
			get { return fGuidFilter ?? (fGuidFilter = new ModuleGuidFilter("guid filter", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, DummyList)); }
		}

		ModuleGuidsFilter GuidsFilter
		{
			get { return fGuidsFilter ?? (fGuidsFilter = new ModuleGuidsFilter("guids filter", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, DummyList, DummyBizoSchema.Z0_Guid, DummyList)); }
		}

		ModuleLocationFilter LocationFilter
		{
			get { return fLocationFilter ?? (fLocationFilter = new ModuleLocationFilter("location filter", DummyBizoSchema.Z0_FK_Code, LocationList, DummyBizoSchema.Z0_FK_Code, LocationList)); }
		}

		ModuleFlagsFilter FlagsFilter
		{
			get { return fFlagsFilter ?? (fFlagsFilter = new ModuleFlagsFilter("flags filter", new string[] { "flagName" }, new SchemaBoolColumn[] { DummyBizoSchema.Z0_Bool })); }
		}

		ModuleNumberRangeFilter NumberRangeFilter
		{
			get { return fNumberRangeFilter ?? (fNumberRangeFilter = new ModuleNumberRangeFilter("number range filter", DummyBizoSchema.Z0_Number)); }
		}

		LocationCollection LocationList
		{
			get
			{
				if (fLocationList == null)
				{
					fLocationList = new LocationCollection(Factory);
					fLocationList.Add(Location1);
					fLocationList.Add(Location2);
				}
				return fLocationList;
			}
		}

		DummyBusinessObjectCollection DummyList
		{
			get
			{
				if (fDummyList == null)
				{
					fDummyList = new DummyBusinessObjectCollection(Factory);
					fDummyList.Add(Dummy);
				}
				return fDummyList;
			}
		}

		CodeDescriptionPairList CodeDescList
		{
			get
			{
				if (fCodeDescList == null)
				{
					fCodeDescList = new CodeDescriptionPairList();
					fCodeDescList.AddPair("Grouchy", "Grouchy Description");
					fCodeDescList.AddPair("Changed", "Changed Description");
				}
				return fCodeDescList;
			}
		}

		IWhsWarehouseCollection WarehouseList
		{
			get
			{
				if (fWarehouseList == null)
				{
					fWarehouseList = ObjectFactory.Get<IWhsWarehouseCollection>("IWhsWarehouseCollection", Factory);
					fWarehouseList.Add(Warehouse);
				}
				return fWarehouseList;
			}
		}

		RefCountryCollection CountryList
		{
			get
			{
				if (fCountryList == null)
				{
					fCountryList = new RefCountryCollection(Factory);
					//fCountryList.Add(Country);
				}
				return fCountryList;
			}
		}

		RefVesselCollection VesselList
		{
			get
			{
				if (fVesselList == null)
				{
					fVesselList = new RefVesselCollection(Factory);
				}
				return fVesselList;
			}
		}

		OrgHeader Location1
		{
			get { return fLocation1 ?? (fLocation1 = Factory.New<OrgHeader>()); }
		}

		OrgHeader Location2
		{
			get { return fLocation2 ?? (fLocation2 = Factory.New<OrgHeader>()); }
		}

		IWhsWarehouse Warehouse
		{
			get { return fWarehouse ?? (fWarehouse = Factory.New<IWhsWarehouse>()); }
		}

		ModuleFilterCollection fModuleFilters;
		ModuleTextFilter fTextFilterWithList;
		ModuleTextFilter fTextFilterWithListQueryCommon;
		ModuleTextFilter fTextFilterWithListQueryOperator;
		ModuleNkFilter fNkFilter;
		ModuleTextAndNkFilter fTextAndNkFilter;
		ModuleTextFilter fTextFilter;
		WorkflowModuleTextFilter fWorkflowTextFilter;
		WorkflowModuleFilter fWorkflowFilter;
		ModuleDateFilter fDateFilter;
		ModuleGuidFilter fGuidFilter;
		ModuleGuidsFilter fGuidsFilter;
		ModuleLocationFilter fLocationFilter;
		ModuleFlagsFilter fFlagsFilter;
		ModuleNumberRangeFilter fNumberRangeFilter;
		ModuleWarehouseLocationFilter fWarehouseLocationFilter;
		ReferenceNumberFilter fReferenceNumberFilter;
		VoyageVesselModuleFilter fVoyageVesselModuleFilter;
		DummyBusinessObjectCollection fDummyList;
		CodeDescriptionPairList fCodeDescList;
		IWhsWarehouseCollection fWarehouseList;
		LocationCollection fLocationList;
		RefCountryCollection fCountryList;
		RefVesselCollection fVesselList;
		OrgHeader fLocation1;
		OrgHeader fLocation2;
		IWhsWarehouse fWarehouse;
		EntryNumberModuleFilter fEntryNumberModuleFilter;
		ChargeModuleFilter fChargeModuleFilter;

		#endregion

		#region ZFilterStripRow and other Controls

		ControlCollection FilterControls
		{
			get { return Row.Cells[2].Controls; }
		}

		ControlCollection FilterClauseControls
		{
			get { return Row.Cells[1].Controls; }
		}

		ZFilterStripRowForTest Row
		{
			get
			{
				if (fRow == null)
				{
					fRow = new ZFilterStripRowForTest();
					Table.Rows.Add(fRow);
					fRow.Bind(FilterStrip);
				}
				return fRow;
			}
		}

		HtmlTable Table
		{
			get
			{
				if (fTable == null)
				{
					fTable = new HtmlTable();
					Page.Controls.Add(fTable);
				}
				return fTable;
			}
		}

		ZPage Page
		{
			get { return fPage ?? (fPage = new ZPage()); }
		}

		FilterStrip FilterStrip
		{
			get
			{
				if (fFilterStrip == null)
				{
					fFilterStrip = new FilterStrip(ModuleFilters);
					FilterStrips.Add(fFilterStrip);
				}
				return fFilterStrip;
			}
		}

		FilterStripCollection FilterStrips
		{
			get { return fFilterStrips ?? (fFilterStrips = new FilterStripCollection(ModuleFilters)); }
		}

		ZFilterStripRowForTest fRow;
		HtmlTable fTable;
		FilterStrip fFilterStrip;
		FilterStripCollection fFilterStrips;
		ZPage fPage;

		#endregion

		#region class ZFilterStripRowForTest

		class ZFilterStripRowForTest : ZFilterStripRow
		{
			public new ZFilterStripDropDownList FilterDescriptionDropList
			{
				get { return base.FilterDescriptionDropList; }
			}

			public void OnPreRender()
			{
				base.OnPreRender(EventArgs.Empty);
			}

			public bool AssertTriangleVisibility(bool visibility)
			{
				return ChangeCategoryPopupButton.Visible == visibility;
			}

			public new ZButton DeleteButton
			{
				get { return base.DeleteButton; }
			}
		}

		#endregion

		#endregion
	}
}
