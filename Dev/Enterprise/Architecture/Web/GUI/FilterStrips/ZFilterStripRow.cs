using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using ZGuid = CargoWise.Types.ZGuid;
using ZString = CargoWise.Types.ZString;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	#region SuppressResourceStringsCheckRegion

	public class ZFilterStripRow : HtmlTableRow
	{
		#region Construction

		public ZFilterStripRow()
		{
			EnableViewState = false;
			SetupCells();
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			ChangeCategoryPopupButton.Visible = ((ZPage)Page).SiteUser.IsLoggedIn;
		}

		protected void SetupCells()
		{
			Cells.Add(FilterDescriptionCell);
			Cells.Add(FilterClauseCell);
			Cells.Add(FilterControlsCell);
			Cells.Add(DeleteButtonCell);
		}

		void SetFilterControlID(WebControl control, string suffix)
		{
			control.ID = ControlIdPrefix + (!string.IsNullOrEmpty(suffix) ? "_" + suffix : string.Empty);
		}

		#endregion

		#region PK & BizO

		public ZGuid PK
		{
			get { return BizO.PK; }
		}

		FilterStrip BizO
		{
			get { return bizO; }
		}

		string ControlIdPrefix
		{
			get { return BizO.PK.ToString(); }
		}

		FilterStrip bizO;

		#endregion

		#region Cells

		#region Filter Description Cell

		HtmlTableCell FilterDescriptionCell
		{
			get
			{
				if (filterDescriptionCell == null)
				{
					filterDescriptionCell = new HtmlTableCell();
					filterDescriptionCell.Controls.Add(FilterDescriptionDropList);
				}

				return filterDescriptionCell;
			}
		}

		HtmlTableCell filterDescriptionCell;

		protected ZFilterStripDropDownList FilterDescriptionDropList
		{
			get
			{
				if (filterDescriptionDropList == null)
				{
					filterDescriptionDropList = new ZFilterStripDropDownList();
					filterDescriptionDropList.AutoPostBack = true;
					filterDescriptionDropList.BindTo = FilterStrip.Schema.FilterDescription;
					filterDescriptionDropList.BindToList = FilterStrip.Schema.FilterDescriptionList;
					filterDescriptionDropList.Width = ZFilterStripConstants.Controls.FilterDescriptionDropListWidth;
					filterDescriptionDropList.IsDescriptionsList = true;
					filterDescriptionDropList.CssClass = "ZFilterStripDropDownList";
				}
				return filterDescriptionDropList;
			}
		}

		ZFilterStripDropDownList filterDescriptionDropList;

		#endregion

		#region Filter Clause Cell

		HtmlTableCell FilterClauseCell
		{
			get { return filterClauseCell ?? (filterClauseCell = new HtmlTableCell()); }
		}

		ZDropDownList FilterClauseDropList
		{
			get { return (FilterClauseCell.Controls.Count > 0) ? FilterClauseCell.Controls[0] as ZDropDownList : null; }
		}

		HtmlTableCell filterClauseCell;

		#endregion

		#region Filter Controls Cell

		HtmlTableCell FilterControlsCell
		{
			get { return filterControlsCell ?? (filterControlsCell = new HtmlTableCell()); }
		}

		HtmlTableCell filterControlsCell;

		#endregion

		#region Delete Button Cell

		HtmlTableCell DeleteButtonCell
		{
			get
			{
				if (deleteButtonCell == null)
				{
					deleteButtonCell = new HtmlTableCell();
					deleteButtonCell.Width = ZFilterStripConstants.Cells.FilterAddButtonWidth.ToString();
					deleteButtonCell.Controls.Add(DeleteButton);
					deleteButtonCell.Controls.Add(ChangeCategoryPopupButton);
					deleteButtonCell.Style["padding-left"] = "10px";
				}
				return deleteButtonCell;
			}
		}

		protected ZButton DeleteButton
		{
			get
			{
				if (deleteButton == null)
				{
					deleteButton = new ZButton();
					deleteButton.Click += new EventHandler(DeleteButton_Click);
					deleteButton.Font.Bold = true;
					deleteButton.ForeColor = Color.Red;
					deleteButton.Height = ZFilterStripConstants.Controls.ButtonHeight;
					deleteButton.Text = "x";
					deleteButton.Width = ZFilterStripConstants.Controls.AddButtonWidth;
					deleteButton.Attributes[nameof(HtmlTextWriterAttribute.Class)] = "Button";
				}
				return deleteButton;
			}
		}

		void DeleteButton_Click(object sender, EventArgs e)
		{
			OnDeleteButtonClick();
			Delete();
		}

		void OnDeleteButtonClick()
		{
			if (DeleteButtonClick != null)
			{
				DeleteButtonClick(this, EventArgs.Empty);
			}
		}

		HtmlTableCell deleteButtonCell;
		ZButton deleteButton;

		public ChangeCategoryPopup ChangeCategoryPopupButton
		{
			get
			{
				if (changeCategoryPopup == null)
				{
					changeCategoryPopup = new ChangeCategoryPopup();
					changeCategoryPopup.RenderContentsOnly = true;
					changeCategoryPopup.TextChanged += new EventHandler(changeCategoryPopup_TextChanged);
				}
				return changeCategoryPopup;
			}
		}

		void changeCategoryPopup_TextChanged(object sender, EventArgs e)
		{
			if (OrCategoryChanged != null)
			{
				OrCategoryChanged(this, EventArgs.Empty);
			}
		}

		ChangeCategoryPopup changeCategoryPopup;

		#endregion

		#endregion

		#region Delete

		public void Delete()
		{
			HtmlTable table = Parent as HtmlTable;

			if (table != null)
			{
				table.Rows.Remove(this);
			}

			BizO.FilterDescriptionInfo.ValueChanged -= new EventHandler(FilterDescriptionInfo_ValueChanged);
			BizO.FilterDescription = ""; // ensures IsActive state is correct

			if (BizO.ParentCollections.Count > 0)
			{
				var collection = BizO.ParentCollections.First();
				collection.CountChanged -= new CollectionCountChangedEventHandler(ZFilterStripRow_CountChanged);
				collection.RemoveAndDelete(BizO);
			}
			else
			{
				BizO.Delete();
			}

			OnFilterStripDeleted();
		}

		void ZFilterStripRow_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemRemoved && e.BizObject.PK == PK)
			{
				Delete();
			}
		}

		void OnFilterStripDeleted()
		{
			if (FilterStripDeleted != null)
			{
				FilterStripDeleted(this, EventArgs.Empty);
			}
		}

		public event EventHandler DeleteButtonClick;
		public event EventHandler OrCategoryChanged;
		public event EventHandler FilterStripDeleted;

		#endregion

		#region Clear

		public void Clear()
		{
			if (BizO.CurrentModuleFilter != null)
			{
				BizO.CurrentModuleFilter.Clear();
			}
			BindFilterControls();
		}

		#endregion

		#region IsOnlyStrip

		public bool IsOnlyStrip
		{
			set { DeleteButton.Enabled = !value; }
		}

		#endregion

		#region Changing Filter

		void FilterDescriptionInfo_ValueChanged(object sender, EventArgs e)
		{
			BindFilterControls();
			OnFilterDescriptionChanged();
		}

		void OnFilterDescriptionChanged()
		{
			if (FilterDescriptionChanged != null)
			{
				FilterDescriptionChanged(this, EventArgs.Empty);
			}
		}

		public event EventHandler FilterDescriptionChanged;

		#endregion

		#region Bind()

		public void Bind(FilterStrip dataSource)
		{
			UnhookDataSourceHandlers(bizO);

			bizO = dataSource;
			bizO.FilterDescriptionInfo.ValueChanged += new EventHandler(FilterDescriptionInfo_ValueChanged);

			if (bizO.ParentCollections.Count > 0)
			{
				bizO.ParentCollections.First().CountChanged += new CollectionCountChangedEventHandler(ZFilterStripRow_CountChanged);
			}

			FilterDescriptionDropList.Bind(bizO);
			BindFilterControls();
			BindDeleteButtonCellControls();
		}

		void UnhookDataSourceHandlers(FilterStrip dataSource)
		{
			if (dataSource != null)
			{
				dataSource.FilterDescriptionInfo.ValueChanged -= FilterDescriptionInfo_ValueChanged;

				if (dataSource.ParentCollections.Count > 0)
				{
					dataSource.ParentCollections.First().CountChanged -= ZFilterStripRow_CountChanged;
				}
			}
		}

		void BindDeleteButtonCellControls()
		{
			ChangeCategoryPopupButton.Bind(bizO);
		}

		void BindFilterControls()
		{
			ClearFilterControls();
			BindFilterControlsCore();
			AssignControlIDs();
		}

		void ClearFilterControls()
		{
			FilterClauseCell.Controls.Clear();
			FilterControlsCell.Controls.Clear();
			RemoveSecondRowControls();
			ControlsToCaptureReturnKey.Clear();
		}

		protected virtual void BindFilterControlsCore()
		{
			if (BizO.CurrentModuleFilter is WorkflowModuleTextFilter)
			{
				BindWorkflowTextFilterControls();
			}
			else if (BizO.CurrentModuleFilter is ReferenceNumberFilter)
			{
				BindReferenceNumberFilterControls();
			}
			else if (BizO.CurrentModuleFilter is EntryNumberModuleFilter)
			{
				BindEntryNumberModuleFilterControls();
			}
			else if (BizO.CurrentModuleFilter is ChargeModuleFilter)
			{
				BindChargeModuleFilterControls();
			}
			else if (BizO.IsTextFilterWithList)
			{
				BindTextWithListFilterControls();
			}
			else if (BizO.IsNkFilter)
			{
				BindNkFilterControls();
			}
			else if (BizO.IsTextAndNkFilter)
			{
				BindTextAndNkFilterControls();
			}
			else if (BizO.CurrentModuleFilter is VoyageVesselModuleFilter)
			{
				BindVoyageVesselFilterControls();
			}
			else if (BizO.IsNumberFilter || BizO.IsTextFilter)
			{
				BindTextOrNumberFilterControls();
			}
			else if (BizO.CurrentModuleFilter is WorkflowModuleFilter)
			{
				BindWorkflowFilterControls();
			}
			else if (BizO.IsDateFilter)
			{
				BindDateFilterControls();
			}
			else if (BizO.IsGuidFilter)
			{
				BuildGuidFilterControls();
			}
			else if (BizO.IsGuidsFilter)
			{
				BuildGuidsFilterControls();
			}
			else if (BizO.IsLocationFilter)
			{
				BuildLocationsFilterControls();
			}
			else if (BizO.IsFlagsFilter)
			{
				BuildFlagsFilterControls();
			}
			else if (BizO.IsNumberRangeFilter)
			{
				BuildNumberRangeControls();
			}
			else if (BizO.CurrentModuleFilter is ModuleWarehouseLocationFilter)
			{
				BindWarehouseListFilterControls();
			}
		}

		void AssignControlIDs()
		{
			// if we let .NET assign its own IDs, .NET gets confused when dynamically-created
			// controls are removed and it sends post back events to the wrong control.

			// cell 1
			FilterDescriptionDropList.ID = ControlIdPrefix + "_FilterDescriptionDropList";

			// cell 2
			if (FilterClauseDropList != null)
			{
				FilterClauseDropList.ID = ControlIdPrefix + "_FilterClauseDropList";
			}

			// cell 3
			for (int i = 0; i < FilterControlsCell.Controls.Count; i++)
			{
				if (string.IsNullOrEmpty(FilterControlsCell.Controls[i].ID))
				{
					FilterControlsCell.Controls[i].ID = ControlIdPrefix + "_Condition_" + i;
				}
			}

			// cell 4
			DeleteButton.ID = ControlIdPrefix + "_DeleteButton";
			changeCategoryPopup.ID = ControlIdPrefix + "_Category";
		}

		public List<Control> ControlsToCaptureReturnKey
		{
			get
			{
				return controlsToCaptureReturnKey ?? (controlsToCaptureReturnKey = new List<Control>());
			}
		}
		List<Control> controlsToCaptureReturnKey;

		#endregion

		#region Filter Controls

		#region Reference Number FilterControls

		void BindReferenceNumberFilterControls()
		{
			WebControls.ZTextBox textBox = new WebControls.ZTextBox();
			SetFilterControlID(textBox, "Text");
			textBox.BindTo = "Property";
			textBox.Style["width"] = "100px";

			BindFilterClauseControls(textBox);

			var findBoxLabel = new ZTextLabel();
			findBoxLabel.Font.Size = 8;
			findBoxLabel.EnableHtmlEncoding = false;
			findBoxLabel.Text = "&nbsp;&nbsp;" + Res.GetString("0b5b007d-e08c-4f76-afcb-fe038a553b9a", "Country/Region:");

			var findBox = new ZFindBox();
			SetFilterControlID(findBox, "Country");
			findBox.BindTo = "Country";
			findBox.ModuleID = WebModuleIDs.GetWebModuleIDFromModuleID(ModuleIDs.RefCountry);
			findBox.TextChanged += ReferenceNumberCountryFilterTextChanged;
			findBox.Width = Unit.Pixel(100);
			findBox.AutoPostBack = true;

			var dropListLabel = new ZTextLabel();
			dropListLabel.Font.Size = 8;
			dropListLabel.EnableHtmlEncoding = false;
			dropListLabel.Text = "&nbsp;&nbsp;" + Res.GetString("5df6264d-6079-4ef8-88fd-fe7dc17ceb36", "Type:");

			SetFilterControlID(ReferenceNumberTypeDropList, "TypeList");
			ReferenceNumberTypeDropList.BindTo = "Type";
			ReferenceNumberTypeDropList.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
			ReferenceNumberTypeDropList.Style["width"] = "60px";
			ReferenceNumberTypeDropList.ShowEmptyItem = true;

			var table = GetNewTableWithFilterControls(
				new Control[] { textBox },
				new Control[] { findBoxLabel, findBox, dropListLabel, ReferenceNumberTypeDropList });

			FilterControlsCell.Controls.Add(table);
			textBox.Bind(BizO.CurrentModuleFilter);
			findBox.Bind(BizO.CurrentModuleFilter);
			ReferenceNumberTypeDropList.Bind(BizO.CurrentModuleFilter);
		}

		void ReferenceNumberCountryFilterTextChanged(object source, EventArgs e)
		{
			ModuleFilterTextChanged(source, e);
			ReferenceNumberTypeDropList.Bind(BizO.CurrentModuleFilter);
		}

		ZDropDownList ReferenceNumberTypeDropList
		{
			get { return referenceNumberTypeDropList ?? (referenceNumberTypeDropList = new ZDropDownList()); }
		}

		ZDropDownList referenceNumberTypeDropList;

		#endregion

		#region EntryNumberModule FilterControls

		void BindEntryNumberModuleFilterControls()
		{
			WebControls.ZTextBox textBox = new WebControls.ZTextBox();
			SetFilterControlID(textBox, "entryNumberText");
			textBox.BindTo = "Property";
			textBox.Style["width"] = "100%";

			BindFilterClauseControls(textBox);

			SetFilterControlID(EntryTypeDropList, "EntryTypeList");
			EntryTypeDropList.BindTo = "EntryType";
			EntryTypeDropList.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
			EntryTypeDropList.Style["width"] = "60px";
			EntryTypeDropList.ShowEmptyItem = true;
			EntryTypeDropList.AutoPostBack = true;
			EntryTypeDropList.SelectedIndexChanged += new EventHandler((sender, e) => EntryTypeDropList_SelectedIndexChanged(sender, e, textBox));

			var table = GetNewTableWithFilterControls(
				new Control[] { EntryTypeDropList },
				new Control[] { textBox });

			FilterControlsCell.Controls.Add(table);
			EntryTypeDropList.Bind(BizO.CurrentModuleFilter);
			textBox.Bind(BizO.CurrentModuleFilter);
		}

		void EntryTypeDropList_SelectedIndexChanged(object sender, EventArgs e, WebControls.ZTextBox textBox)
		{
			ZDropDownList senderDropList = (ZDropDownList)sender;
			senderDropList.Bind(BizO.CurrentModuleFilter);
			textBox.Bind(BizO.CurrentModuleFilter);
		}

		ZDropDownList EntryTypeDropList
		{
			get { return entryTypeDropList ?? (entryTypeDropList = new ZDropDownList()); }
		}

		ZDropDownList entryTypeDropList;

		#endregion

		#region Charge Module FilterControls

		void BindChargeModuleFilterControls()
		{
			AddSecondRowControls();

			ZDropDownList dropList = new ZDropDownList();
			SetFilterControlID(dropList, "chargeGroupDropList");
			dropList.BindTo = ChargeModuleFilter.Schema.ChargeGroup;
			dropList.Style["width"] = "100%";
			dropList.ShowEmptyItem = true;

			FilterControlsCell.Controls.Add(dropList);
			dropList.Bind(BizO.CurrentModuleFilter);

			ZNumericTextBox fromCalcEdit = new ZNumericTextBox();
			ZNumericTextBox toCalcEdit = new ZNumericTextBox();

			ZCheckBox useLowerBoundCheckBox = new ZCheckBox();
			SetFilterControlID(useLowerBoundCheckBox, "useLowerBoundCheckBox");
			useLowerBoundCheckBox.Font.Size = 8;
			useLowerBoundCheckBox.Text = Res.GetString("ed722f81-2f92-47b0-9a7a-1c37347eb01c", "From:");
			useLowerBoundCheckBox.AutoPostBack = true;
			useLowerBoundCheckBox.CheckedChanged += new EventHandler((sender, e) => useLowerBoundCheckBox_CheckedChanged(sender, e, fromCalcEdit));
			useLowerBoundCheckBox.BindTo = ChargeModuleFilter.Schema.UseLowerBound;

			ZCheckBox useUpperBoundCheckBox = new ZCheckBox();
			SetFilterControlID(useUpperBoundCheckBox, "useUpperBoundCheckBox");
			useUpperBoundCheckBox.Font.Size = 8;
			useUpperBoundCheckBox.Text = Res.GetString("b5e5339a-9eef-43a9-af09-d07218bd2d44", "To:");
			useUpperBoundCheckBox.AutoPostBack = true;
			useUpperBoundCheckBox.CheckedChanged += new EventHandler((sender, e) => useUpperBoundCheckBox_CheckedChanged(sender, e, toCalcEdit));
			useUpperBoundCheckBox.BindTo = ChargeModuleFilter.Schema.UseUpperBound;

			SetFilterControlID(fromCalcEdit, "lowerBoundCalcEdit");
			SetFilterControlID(toCalcEdit, "upperBoundCalcEdit");

			fromCalcEdit.Style["width"] = "80px";
			toCalcEdit.Style["width"] = "80px";

			fromCalcEdit.BindTo = ChargeModuleFilter.Schema.LowerBound;
			toCalcEdit.BindTo = ChargeModuleFilter.Schema.UpperBound;

			HtmlTable table = GetNewTableWithFilterControls(
				new Control[] { useLowerBoundCheckBox, fromCalcEdit },
				new Control[] { useUpperBoundCheckBox, toCalcEdit });
			FilterControlsCell.Controls.Add(table);

			useLowerBoundCheckBox.Bind(BizO.CurrentModuleFilter);
			fromCalcEdit.Bind(BizO.CurrentModuleFilter);
			useUpperBoundCheckBox.Bind(BizO.CurrentModuleFilter);
			toCalcEdit.Bind(BizO.CurrentModuleFilter);
		}

		void useUpperBoundCheckBox_CheckedChanged(object sender, EventArgs e, ZNumericTextBox toCalcEdit)
		{
			ZCheckBox senderDropList = (ZCheckBox)sender;
			((ChargeModuleFilter)BizO.CurrentModuleFilter).UseUpperBound = senderDropList.Checked;

			senderDropList.Bind(BizO.CurrentModuleFilter);
			toCalcEdit.Bind(BizO.CurrentModuleFilter);
		}

		void useLowerBoundCheckBox_CheckedChanged(object sender, EventArgs e, ZNumericTextBox fromCalcEdit)
		{
			ZCheckBox senderDropList = (ZCheckBox)sender;
			((ChargeModuleFilter)BizO.CurrentModuleFilter).UseLowerBound = senderDropList.Checked;

			senderDropList.Bind(BizO.CurrentModuleFilter);
			fromCalcEdit.Bind(BizO.CurrentModuleFilter);
		}

		#endregion

		#region Warehouse List FilterControls

		void BindWarehouseListFilterControls()
		{
			var dropList = new ZGuidDropDownList();
			SetFilterControlID(dropList, "WarehouseList");
			dropList.BindTo = "Warehouse";
			dropList.Style["width"] = "200px";
			dropList.ShowEmptyItem = true;
			dropList.DataValueField = "PK";
			dropList.DataTextField = "WW_WarehouseCode";

			var textBox = new WebControls.ZTextBox();
			SetFilterControlID(textBox, "Text");
			textBox.BindTo = "Location";
			textBox.Width = 80;

			var table = GetNewTableWithFilterControls(
				new Control[] { dropList },
				new Control[] { textBox });

			FilterControlsCell.Controls.Add(table);
			dropList.Bind(BizO.CurrentModuleFilter);
			textBox.Bind(BizO.CurrentModuleFilter);
		}

		#endregion

		#region Text with List FilterControls

		void BindTextWithListFilterControls()
		{
			ZDropDownList dropList = new ZDropDownList();
			SetFilterControlID(dropList, "TextWithList");
			dropList.BindTo = "Property";
			dropList.BindToList = "List";
			dropList.Style["width"] = "100%";
			dropList.ShowEmptyItem = true;

			if (BizO.CurrentModuleFilter.QueryDelegate is GetTextQueryWithOperator)
			{
				BindFilterClauseControls(dropList);
			}

			FilterControlsCell.Controls.Add(dropList);
			dropList.Bind(BizO.CurrentModuleFilter);
		}

		#endregion

		#region NK Filter Controls

		void BindNkFilterControls()
		{
			ModuleNkFilter moduleFilter = (ModuleNkFilter)BizO.CurrentModuleFilter;
			ZFindBox findBox = new ZFindBox();
			SetFilterControlID(findBox, "NK");
			findBox.BindTo = "Property";
			findBox.BindToList = "List";
			findBox.ModuleID = WebModuleIDs.GetWebModuleIDFromModuleID(moduleFilter.ModuleId);
			findBox.TextChanged += ModuleFilterTextChanged;
			findBox.Style["width"] = "100%";

			FilterControlsCell.Controls.Add(findBox);
			findBox.Bind(BizO.CurrentModuleFilter);

			ControlsToCaptureReturnKey.Add(findBox.TextBoxControl);
		}

		#endregion

		#region Text and NK Filter Controls

		void BindTextAndNkFilterControls()
		{
			WebControls.ZTextBox textBox = new WebControls.ZTextBox();
			SetFilterControlID(textBox, "Text");
			textBox.BindTo = "Property";
			textBox.Width = 80;

			BindFilterClauseControls(textBox);

			ZTextLabel findBoxLabel = new ZTextLabel();
			findBoxLabel.Font.Size = 8;
			ModuleTextAndNkFilter moduleFilter = (ModuleTextAndNkFilter)BizO.CurrentModuleFilter;
			ZFindBox findBox = new ZFindBox();
			SetFilterControlID(findBox, "NK");
			findBox.BindTo = "NkProperty";
			findBox.BindToList = "List";
			findBox.ModuleID = WebModuleIDs.GetWebModuleIDFromModuleID(moduleFilter.ModuleId);
			findBox.TextChanged += ModuleFilterTextChanged;

			HtmlTable table = GetNewTableWithFilterControls(
				new Control[] { textBox },
				new Control[] { findBoxLabel, findBox });

			FilterControlsCell.Controls.Add(table);
			textBox.Bind(BizO.CurrentModuleFilter);
			findBox.Bind(BizO.CurrentModuleFilter);

			ControlsToCaptureReturnKey.Add(textBox);
			ControlsToCaptureReturnKey.Add(findBox.TextBoxControl);
		}

		#endregion

		#region Voyage Vessel Filter Controls

		void BindVoyageVesselFilterControls()
		{
			WebControls.ZTextBox textBox = new WebControls.ZTextBox();
			SetFilterControlID(textBox, "Text");
			textBox.BindTo = "Property";
			textBox.Width = 80;

			BindFilterClauseControls(textBox);

			ZTextLabel findBoxLabel = new ZTextLabel();
			findBoxLabel.Font.Size = 8;
			findBoxLabel.Text = Res.GetString("d1e35c25-ca03-4c43-9a97-964c90baecea", "Vessel:");
			var moduleFilter = (VoyageVesselModuleFilter)BizO.CurrentModuleFilter;
			ZFindBox findBox = new ZFindBox();
			SetFilterControlID(findBox, "Vessel");
			findBox.BindTo = "Vessel";
			findBox.BindToList = "List";
			findBox.ModuleID = WebModuleIDs.GetWebModuleIDFromModuleID(moduleFilter.ID);
			findBox.TextChanged += ModuleFilterTextChanged;

			HtmlTable table = GetNewTableWithFilterControls(
				new Control[] { textBox },
				new Control[] { findBoxLabel, findBox });

			FilterControlsCell.Controls.Add(table);
			textBox.Bind(BizO.CurrentModuleFilter);
			findBox.Bind(BizO.CurrentModuleFilter);

			ControlsToCaptureReturnKey.Add(textBox);
			ControlsToCaptureReturnKey.Add(findBox.TextBoxControl);
		}

		#endregion

		#region Text / Number Filter Controls

		void BindTextOrNumberFilterControls()
		{
			WebControls.ZTextBox textBox = new WebControls.ZTextBox();
			SetFilterControlID(textBox, "Text");
			textBox.BindTo = "Property";
			textBox.Style["width"] = "100%";

			BindFilterClauseControls(textBox);

			FilterControlsCell.Controls.Add(textBox);
			textBox.Bind(BizO.CurrentModuleFilter);

			ControlsToCaptureReturnKey.Add(textBox);
		}

		#endregion

		#region Workflow Text Filter Controls

		void BindWorkflowTextFilterControls()
		{
			AddSecondRowControls();

			ZFilterStripDropDownList eventTypeDropList = new ZFilterStripDropDownList();
			SetFilterControlID(eventTypeDropList, "WorkflowFilter_EventType");
			eventTypeDropList.BindTo = "MilestoneEvent";
			eventTypeDropList.BindToList = "MilestoneEventTypes";
			eventTypeDropList.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			eventTypeDropList.Style["width"] = "100%";

			FilterControlsCell.Controls.Add(eventTypeDropList);
			eventTypeDropList.Bind(BizO.CurrentModuleFilter);
			var selectedValueControl = new HiddenField();
			FilterControlsCell.Controls.Add(selectedValueControl);
			eventTypeDropList.EnableDuplicateValues(selectedValueControl);

			ZDropDownList statusDropList = new ZDropDownList();
			SetFilterControlID(statusDropList, "WorkflowFilter_Status");
			statusDropList.BindTo = "Property";
			statusDropList.BindToList = "List";
			statusDropList.Style["width"] = "100%";

			FilterControlsCell.Controls.Add(statusDropList);
			statusDropList.Bind(BizO.CurrentModuleFilter);

			FilterControlsCell.Controls.Add(GetEventReferenceControls(nameof(WorkflowModuleTextFilter.ComparisonOperator_List)));
		}

		#region Second Row implementation

		void AddSecondRowControls()
		{
			if (FilterDescriptionCell.Controls.Count == 1)
			{
				FilterDescriptionCell.Controls.Add(BrControl);
				FilterDescriptionCell.Controls.Add(EmptyTextLabel);
			}
			if (FilterClauseCell.Controls.Count == 0)
			{
				FilterClauseCell.Controls.Add(BrControl);
			}
			if (DeleteButtonCell.Controls.Count == 4)
			{
				DeleteButtonCell.Controls.Add(BrControl);
				DeleteButtonCell.Controls.Add(EmptyTextLabel);
			}
		}

		void RemoveSecondRowControls()
		{
			if (FilterDescriptionCell.Controls.Count == 3)
			{
				FilterDescriptionCell.Controls.RemoveAt(2);
				FilterDescriptionCell.Controls.RemoveAt(1);
			}
			if (FilterClauseCell.Controls.Count == 2)
			{
				FilterClauseCell.Controls.RemoveAt(0);
			}
			if (DeleteButtonCell.Controls.Count == 6)
			{
				DeleteButtonCell.Controls.RemoveAt(5);
				DeleteButtonCell.Controls.RemoveAt(4);
			}
		}

		#endregion

		#endregion

		#region Workflow Filter Controls

		void BindWorkflowFilterControls()
		{
			AddSecondRowControls();

			BindWorkflowFilterClauseControls();

			FilterControlsCell.Controls.Add(WorkflowFilter_EventTypeDropList);
			WorkflowFilter_EventTypeDropList.Bind(BizO.CurrentModuleFilter);
			var selectedValueControl = new HiddenField();
			FilterControlsCell.Controls.Add(selectedValueControl);
			WorkflowFilter_EventTypeDropList.EnableDuplicateValues(selectedValueControl);

			var table = GetNewTableWithFilterControls(
				new Control[] { WorkflowFilter_DropList, WorkflowFilter_SpaceBeforeDateRangeLabel, WorkflowFilter_DateRangeLabel, WorkflowFilter_FromDateEdit, WorkflowFilter_DaysOffsetRange, WorkflowFilter_HoursOffsetRange },
				new Control[] { WorkflowFilter_ToLabel, WorkflowFilter_ToDateEdit });
			FilterControlsCell.Controls.Add(table);

			WorkflowFilter_DropList.Bind(BizO.CurrentModuleFilter);
			WorkflowFilter_DropList.IsDescriptionsList = true;
			WorkflowFilter_DropList.ShowEmptyItem = true;
			UpdateWorkflowFilterControls(WorkflowFilter_DropList.SelectedValue);

			FilterControlsCell.Controls.Add(GetEventReferenceControls(nameof(WorkflowModuleFilter.EventReferenceComparisonOptions)));
		}

		void BindWorkflowFilterClauseControls()
		{
			FilterClauseCell.Controls.Add(WorkflowFilter_DropListInClauseCell);
			WorkflowFilter_DropListInClauseCell.Bind(BizO.CurrentModuleFilter);
		}

		void UpdateWorkflowFilterControls(string propertySearch) => UpdateDateFilterControls(propertySearch,
			WorkflowFilter_DropListInClauseCell,
			WorkflowFilter_DropList,
			WorkflowFilter_SpaceBeforeDateRangeLabel,
			WorkflowFilter_DateRangeLabel,
			WorkflowFilter_FromDateEdit,
			WorkflowFilter_ToLabel,
			WorkflowFilter_ToDateEdit,
			WorkflowFilter_DaysOffsetRange,
			WorkflowFilter_HoursOffsetRange);

		void workflowEventType_SelectedIndexChanged(object sender, EventArgs e)
		{
			ZFilterStripDropDownList senderDropList = (ZFilterStripDropDownList)sender;

			senderDropList.Bind(BizO.CurrentModuleFilter);
		}

		void workflowDropList_SelectedIndexChanged(object sender, EventArgs e)
		{
			ZFilterStripDropDownList senderDropList = (ZFilterStripDropDownList)sender;

			senderDropList.Bind(BizO.CurrentModuleFilter);

			WorkflowFilter_DropListInClauseCell.SelectedIndex = senderDropList.SelectedIndex;
			WorkflowFilter_DropList.SelectedIndex = senderDropList.SelectedIndex;

			UpdateWorkflowFilterControls(senderDropList.Text);
		}

		void WorkflowDateEdit_TextChanged(object sender, EventArgs e)
		{
			ZDateEdit senderDateEdit = (ZDateEdit)sender;

			senderDateEdit.Bind(BizO.CurrentModuleFilter);
		}

		#region Controls

		ZFilterStripDropDownList WorkflowFilter_DropListInClauseCell
		{
			get
			{
				if (worflowFilter_DropListInClauseCell == null)
				{
					worflowFilter_DropListInClauseCell = GetNewWorkflowFilterDropList("Clause");
					worflowFilter_DropListInClauseCell.Width = ZFilterStripConstants.Controls.FilterClauseDropListWidth;
				}

				return worflowFilter_DropListInClauseCell;
			}
		}

		ZFilterStripDropDownList GetNewWorkflowFilterDropList(string additionalSuffix)
		{
			var dropList = new ZFilterStripDropDownList();
			SetFilterControlID(dropList, "WorkflowFilter_DateFilter" + "_" + additionalSuffix);
			dropList.AutoPostBack = true;
			dropList.BindTo = "PropertySearch";
			dropList.BindToList = "PropertySearch_List";
			dropList.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
			dropList.Font.Italic = true;
			dropList.SelectedIndexChanged += new EventHandler(workflowDropList_SelectedIndexChanged);

			return dropList;
		}

		ZFilterStripDropDownList WorkflowFilter_EventTypeDropList => workflowFilter_EventTypeDropList ?? (workflowFilter_EventTypeDropList = GetNewWorkflowFilter_EventTypeDropList());

		ZFilterStripDropDownList GetNewWorkflowFilter_EventTypeDropList()
		{
			var dropList = new ZFilterStripDropDownList();
			SetFilterControlID(dropList, "WorkflowFilter_EventType");
			dropList.BindTo = "MilestoneEvent";
			dropList.BindToList = "MilestoneEventTypes";
			dropList.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			dropList.Style["width"] = "100%";
			dropList.SelectedIndexChanged += new EventHandler(workflowEventType_SelectedIndexChanged);

			return dropList;
		}

		ZFilterStripDropDownList WorkflowFilter_DropList => workflowFilter_DropList ?? (workflowFilter_DropList = GetNewWorkflowFilterDropList("Filter"));

		ZTextLabelWhiteSpace WorkflowFilter_SpaceBeforeDateRangeLabel => workflowFilter_SpaceBeforeDateRangeLabel ?? (workflowFilter_SpaceBeforeDateRangeLabel = new ZTextLabelWhiteSpace(ZFilterStripConstants.Spacing.LeftOrRightCellPadding));

		ZCodeLookupLabel WorkflowFilter_DateRangeLabel
		{
			get
			{
				if (workflowFilter_DateRangeLabel == null)
				{
					workflowFilter_DateRangeLabel = new ZCodeLookupLabel();
					workflowFilter_DateRangeLabel.BindTo = "PropertySearch";
					workflowFilter_DateRangeLabel.BindToList = "PropertySearch_List";
					workflowFilter_DateRangeLabel.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
					workflowFilter_DateRangeLabel.Font.Size = 8;
				}

				return workflowFilter_DateRangeLabel;
			}
		}

		ZDateEdit WorkflowFilter_FromDateEdit => workflowFilter_FromDateEdit ?? (workflowFilter_FromDateEdit = WorkflowFilter_GetNewDateEdit("Property1"));

		ZDateEdit WorkflowFilter_ToDateEdit => workflowFilter_ToDateEdit ?? (workflowFilter_ToDateEdit = WorkflowFilter_GetNewDateEdit("Property2"));

		ZDateEdit WorkflowFilter_GetNewDateEdit(ZString bindTo)
		{
			var result = new ZDateEdit();
			SetFilterControlID(result, bindTo);
			result.BindTo = bindTo;
			result.TextChanged += new EventHandler(WorkflowDateEdit_TextChanged);
			result.CssClass = "FilterStripRowDateEdit";

			return result;
		}

		ZTextLabel WorkflowFilter_ToLabel
		{
			get
			{
				if (workflowFilter_ToLabel == null)
				{
					workflowFilter_ToLabel = new ZTextLabel();
					workflowFilter_ToLabel.Font.Size = 8;
					workflowFilter_ToLabel.Text = Res.GetString("b5e5339a-9eef-43a9-af09-d07218bd2d44", "To:");
				}

				return workflowFilter_ToLabel;
			}
		}

		ZOffsetRangeFilterControl WorkflowFilter_DaysOffsetRange => workflowFilter_DaysOffsetRange ?? (workflowFilter_DaysOffsetRange = new ZOffsetRangeFilterControl(OffsetRangeMeasure.Days));

		ZOffsetRangeFilterControl WorkflowFilter_HoursOffsetRange => workflowFilter_HoursOffsetRange ?? (workflowFilter_HoursOffsetRange = new ZOffsetRangeFilterControl(OffsetRangeMeasure.Hours));

		ZFilterStripDropDownList worflowFilter_DropListInClauseCell;
		ZFilterStripDropDownList workflowFilter_EventTypeDropList;
		ZFilterStripDropDownList workflowFilter_DropList;
		ZTextLabelWhiteSpace workflowFilter_SpaceBeforeDateRangeLabel;
		ZCodeLookupLabel workflowFilter_DateRangeLabel;
		ZDateEdit workflowFilter_FromDateEdit;
		ZDateEdit workflowFilter_ToDateEdit;
		ZTextLabel workflowFilter_ToLabel;
		ZOffsetRangeFilterControl workflowFilter_DaysOffsetRange;
		ZOffsetRangeFilterControl workflowFilter_HoursOffsetRange;

		#endregion

		#region Additional Controls

		LiteralControl BrControl
		{
			get
			{
				return new LiteralControl("<br/>");
			}
		}

		ZTextLabel EmptyTextLabel
		{
			get
			{
				ZTextLabel emptyTextLabel = new ZTextLabel("");
				emptyTextLabel.Style["display"] = "inline-block";
				emptyTextLabel.Style["height"] = ZFilterStripConstants.Controls.ButtonHeight.ToString();

				return emptyTextLabel;
			}
		}

		#endregion

		#endregion

		#region Date Filter Controls

		void BindDateFilterControls()
		{
			BindDateFilterClauseControls();

			var table = GetNewTableWithFilterControls(
				new Control[] { DateFilter_DropList, DateFilter_SpaceBeforeDateRangeLabel, DateFilter_DateRangeLabel, DateFilter_FromDateEdit, DateFilter_DaysOffsetRange, DateFilter_HoursOffsetRange },
				new Control[] { DateFilter_ToLabel, DateFilter_ToDateEdit });

			FilterControlsCell.Controls.Add(table);
			DateFilter_DropList.Bind(BizO.CurrentModuleFilter);
			UpdateDateFilterControls(DateFilter_DropList.SelectedValue);

			ControlsToCaptureReturnKey.Add(DateFilter_FromDateEdit.TextBoxControl);
			ControlsToCaptureReturnKey.Add(DateFilter_ToDateEdit.TextBoxControl);
		}

		void BindDateFilterClauseControls()
		{
			FilterClauseCell.Controls.Add(DateFilter_DropListInClauseCell);
			DateFilter_DropListInClauseCell.Bind(BizO.CurrentModuleFilter);
		}

		void UpdateDateFilterControls(string propertySearch) => UpdateDateFilterControls(propertySearch,
			DateFilter_DropListInClauseCell,
			DateFilter_DropList,
			DateFilter_SpaceBeforeDateRangeLabel,
			DateFilter_DateRangeLabel,
			DateFilter_FromDateEdit,
			DateFilter_ToLabel,
			DateFilter_ToDateEdit,
			DateFilter_DaysOffsetRange,
			DateFilter_HoursOffsetRange);

		void UpdateDateFilterControls(string propertySearch,
			ZFilterStripDropDownList inClauseCell,
			ZFilterStripDropDownList listControl,
			ZTextLabelWhiteSpace spaceLabel,
			ZCodeLookupLabel rangeLabel, ZDateEdit fromDateControl,
			ZTextLabel toLabel,
			ZDateEdit toDateControl,
			ZOffsetRangeFilterControl daysOffsetRangeControl,
			ZOffsetRangeFilterControl hoursOffsetRangeControl)
		{
			var showDaysOffsetRangeControl = propertySearch == ModuleDateFilter.SpecifiedDayOffsetRange;
			var showHoursOffsetRangeControl = propertySearch == ModuleDateFilter.SpecifiedHourOffsetRange || propertySearch == ModuleDateFilter.SpecifiedWorkHourOffsetRange;
			var showOffsetRange = showDaysOffsetRangeControl || showHoursOffsetRangeControl;
			var showDateTimeControls = propertySearch == ModuleDateFilter.SpecifiedDateTimeRange;
			var showDateControls = showDateTimeControls || propertySearch == ModuleDateFilter.SpecifiedDateRange;

			inClauseCell.Visible = showDateControls;
			listControl.Visible = !showDateControls || showOffsetRange;
			spaceLabel.Visible = !showDateControls && !showOffsetRange;
			rangeLabel.Visible = !showDateControls && !showOffsetRange;
			fromDateControl.Visible = showDateControls;
			toLabel.Visible = showDateControls;
			toDateControl.Visible = showDateControls;
			daysOffsetRangeControl.Visible = showDaysOffsetRangeControl;
			hoursOffsetRangeControl.Visible = showHoursOffsetRangeControl;

			var filter = BizO.CurrentModuleFilter;
			if (showDateControls)
			{
				inClauseCell.Bind(filter);
				if (showDateTimeControls)
				{
					fromDateControl.DateTimeFormat = ZDateTimePickerFormat.Long;
					toDateControl.DateTimeFormat = ZDateTimePickerFormat.Long;
				}
				else
				{
					fromDateControl.DateTimeFormat = ZDateTimePickerFormat.Short;
					toDateControl.DateTimeFormat = ZDateTimePickerFormat.Short;
				}
				fromDateControl.Bind(filter);
				toDateControl.Bind(filter);
			}
			else if (showOffsetRange)
			{
				DateFilter_DropList.Bind(filter);
				if (showDaysOffsetRangeControl)
				{
					daysOffsetRangeControl.Bind(filter);
				}
				else
				{
					hoursOffsetRangeControl.Bind(filter);
				}
			}
			else
			{
				listControl.Bind(filter);
				rangeLabel.Bind(filter);
			}
		}

		void dateDropList_SelectedIndexChanged(object sender, EventArgs e)
		{
			var senderDropList = (ZFilterStripDropDownList)sender;

			senderDropList.Bind(BizO.CurrentModuleFilter);

			DateFilter_DropListInClauseCell.SelectedIndex = senderDropList.SelectedIndex;
			DateFilter_DropList.SelectedIndex = senderDropList.SelectedIndex;

			UpdateDateFilterControls(senderDropList.SelectedValue);
		}

		void DateEdit_TextChanged(object sender, EventArgs e)
		{
			var senderDateEdit = (ZDateEdit)sender;
			senderDateEdit.Bind(BizO.CurrentModuleFilter);
		}

		#region Controls

		ZFilterStripDropDownList DateFilter_DropListInClauseCell
		{
			get
			{
				if (dateFilter_DropListInClauseCell == null)
				{
					dateFilter_DropListInClauseCell = GetNewDateRangeFilterClauseDropList();
					dateFilter_DropListInClauseCell.Width = ZFilterStripConstants.Controls.FilterClauseDropListWidth;
				}
				return dateFilter_DropListInClauseCell;
			}
		}

		ZFilterStripDropDownList DateFilter_DropList => dateFilter_DropList ?? (dateFilter_DropList = GetNewDateRangeFilterClauseDropList());

		ZFilterStripDropDownList GetNewDateRangeFilterClauseDropList()
		{
			var dropList = new ZFilterStripDropDownList();
			SetFilterControlID(dropList, "DateFilter");
			dropList.AutoPostBack = true;
			dropList.BindTo = "PropertySearch";
			dropList.BindToList = "PropertySearch_List";
			dropList.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
			dropList.Font.Italic = true;
			dropList.SelectedIndexChanged += new EventHandler(dateDropList_SelectedIndexChanged);
			dropList.IsDescriptionsList = true;
			dropList.ShowEmptyItem = true;

			return dropList;
		}

		ZTextLabelWhiteSpace DateFilter_SpaceBeforeDateRangeLabel => dateFilter_SpaceBeforeDateRangeLabel ?? (dateFilter_SpaceBeforeDateRangeLabel = new ZTextLabelWhiteSpace(ZFilterStripConstants.Spacing.LeftOrRightCellPadding));

		ZCodeLookupLabel DateFilter_DateRangeLabel
		{
			get
			{
				if (fDateFilter_DateRangeLabel == null)
				{
					fDateFilter_DateRangeLabel = new ZCodeLookupLabel();
					fDateFilter_DateRangeLabel.BindTo = "PropertySearch";
					fDateFilter_DateRangeLabel.BindToList = "PropertySearch_List";
					fDateFilter_DateRangeLabel.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
					fDateFilter_DateRangeLabel.Font.Size = 8;
				}
				return fDateFilter_DateRangeLabel;
			}
		}

		ZDateEdit DateFilter_FromDateEdit
		{
			get { return fDateFilter_FromDateEdit ?? (fDateFilter_FromDateEdit = DateFilter_GetNewDateEdit("Property1")); }
		}

		ZDateEdit DateFilter_ToDateEdit
		{
			get { return fDateFilter_ToDateEdit ?? (fDateFilter_ToDateEdit = DateFilter_GetNewDateEdit("Property2")); }
		}

		ZDateEdit DateFilter_GetNewDateEdit(ZString bindTo)
		{
			var result = new ZDateEdit();
			SetFilterControlID(result, bindTo);
			result.BindTo = bindTo;
			result.TextChanged += new EventHandler(DateEdit_TextChanged);

			return result;
		}

		ZOffsetRangeFilterControl DateFilter_DaysOffsetRange => dateFilter_DaysOffsetRange ?? (dateFilter_DaysOffsetRange = new ZOffsetRangeFilterControl(OffsetRangeMeasure.Days));

		ZOffsetRangeFilterControl DateFilter_HoursOffsetRange => dateFilter_HoursOffsetRange ?? (dateFilter_HoursOffsetRange = new ZOffsetRangeFilterControl(OffsetRangeMeasure.Hours));

		ZTextLabel DateFilter_ToLabel
		{
			get
			{
				if (fDateFilter_ToLabel == null)
				{
					fDateFilter_ToLabel = new ZTextLabel();
					fDateFilter_ToLabel.Font.Size = 8;
					fDateFilter_ToLabel.Text = Res.GetString("b5e5339a-9eef-43a9-af09-d07218bd2d44", "To:");
					//fDateFilter_ToLabel.CssClass = ZFilterStripConstants.CssClasses.Clear;
				}
				return fDateFilter_ToLabel;
			}
		}

		ZFilterStripDropDownList dateFilter_DropListInClauseCell;
		ZFilterStripDropDownList dateFilter_DropList;
		ZCodeLookupLabel fDateFilter_DateRangeLabel;
		ZDateEdit fDateFilter_FromDateEdit;
		ZDateEdit fDateFilter_ToDateEdit;
		ZTextLabel fDateFilter_ToLabel;
		ZTextLabelWhiteSpace dateFilter_SpaceBeforeDateRangeLabel;
		ZOffsetRangeFilterControl dateFilter_DaysOffsetRange;
		ZOffsetRangeFilterControl dateFilter_HoursOffsetRange;

		#endregion

		#endregion

		#region Guid Filter Controls

		void BuildGuidFilterControls()
		{
			var moduleFilter = (ModuleGuidFilter)BizO.CurrentModuleFilter;

			var findBox = CreateGuidFindBox(moduleFilter.ModuleId, moduleFilter.Description);
			findBox.BindTo = "Property";
			findBox.BindToList = "List";
			findBox.Style["width"] = "100%";

			FilterControlsCell.Controls.Add(findBox);
			findBox.Bind(BizO.CurrentModuleFilter);

			ControlsToCaptureReturnKey.Add(findBox.TextBoxControl);
		}

		void BuildGuidsFilterControls()
		{
			ModuleGuidsFilter moduleFilter = (ModuleGuidsFilter)BizO.CurrentModuleFilter;

			string text1 = moduleFilter.ItemDescription1 == null ? "#1:" : moduleFilter.ItemDescription1.Caption + ":";
			string text2 = moduleFilter.ItemDescription2 == null ? "#2:" : moduleFilter.ItemDescription2.Caption + ":";

			ZTextLabel label1 = new ZTextLabel(text1);
			ZTextLabel label2 = new ZTextLabel(text2);

			label1.Font.Size = 8;
			label2.Font.Size = 8; // replace with css

			ZGuidFindBox findBox1 = CreateGuidFindBox(moduleFilter.ModuleId);
			findBox1.ID += "_1";
			ZGuidFindBox findBox2 = CreateGuidFindBox(moduleFilter.ModuleId);
			findBox2.ID += "_2";

			findBox1.BindTo = "Property1";
			findBox1.BindToList = "List1";
			findBox2.BindTo = "Property2";
			findBox2.BindToList = "List2";
			findBox1.TextChanged += ModuleFilterTextChanged;
			findBox2.TextChanged += ModuleFilterTextChanged;

			FilterClauseCell.Align = "right";
			FilterClauseCell.Controls.Add(label1);
			FilterClauseCell.Controls.Add(new ZTextLabelWhiteSpace(ZFilterStripConstants.Spacing.AfterLabelInFilterClauseCell));

			HtmlTable table = GetNewTableWithFilterControls(new Control[] { findBox1 }, new Control[] { label2, findBox2 });
			FilterControlsCell.Controls.Add(table);

			findBox1.Bind(moduleFilter);
			findBox2.Bind(moduleFilter);

			ControlsToCaptureReturnKey.Add(findBox1.TextBoxControl);
			ControlsToCaptureReturnKey.Add(findBox2.TextBoxControl);
		}

		ZGuidFindBox CreateGuidFindBox(ModuleIdentifier moduleID, string description = null)
		{
			var result = new ZGuidFindBox();
			result.ModuleID = WebModuleIDs.GetWebModuleIDFromModuleID(moduleID, description);
			SetFilterControlID(result, result.ModuleID.Name);
			result.TextChanged += ModuleFilterTextChanged;
			return result;
		}

		void ModuleFilterTextChanged(object source, EventArgs e)
		{
			((ISelfBindingWebControl)source).Bind(BizO.CurrentModuleFilter);
		}

		#endregion

		#region Filter Clause Controls

		void BindFilterClauseControls(ISelfBindingWebControl predicateControl)
		{
			var dropList = GetFilterClauseDropDownList(predicateControl);
			FilterClauseCell.Controls.Add(dropList);
		}

		ZDropDownList GetFilterClauseDropDownList(ISelfBindingWebControl predicateControl, string bindTo = "ComparisonOperator", string bindToList = "ComparisonOperator_List")
		{
			var dropList = new ZDropDownList()
			{
				BindTo = bindTo,
				BindToList = bindToList,
				DisplayStyle = OComboBoxDropDownStyle.CodeOnly,
				Width = ZFilterStripConstants.Controls.FilterClauseDropListWidth,
				AutoPostBack = true
			};

			SetFilterControlID(dropList, bindTo);
			dropList.Font.Italic = true;
			dropList.SelectedIndexChanged += FilterClauseChanged;
			dropList.Bind(BizO.CurrentModuleFilter);

			void FilterClauseChanged(object sender, EventArgs e)
			{
				dropList.Bind(BizO.CurrentModuleFilter);
				predicateControl.Bind(BizO.CurrentModuleFilter);
			}

			return dropList;
		}

		#endregion

		#region Location Filter Controls

		void BuildLocationsFilterControls()
		{
			ModuleFilterWithSubDescriptions moduleFilter = (ModuleFilterWithSubDescriptions)BizO.CurrentModuleFilter;

			string text1 = moduleFilter.ItemDescription1 == null ? Res.GetString("fec799e8-607b-4354-8964-b0d54ff5b1e3", "Location #1:") : moduleFilter.ItemDescription1.Caption + ":";
			string text2 = moduleFilter.ItemDescription2 == null ? Res.GetString("d17fa25c-333b-4bce-a121-77c650a0efe1", "Location #2:") : moduleFilter.ItemDescription2.Caption + ":";

			ZTextLabel location1Label = new ZTextLabel(text1);
			ZFindBox location1FindBox = new ZFindBox();

			ZTextLabel location2Label = new ZTextLabel(text2);
			ZFindBox location2FindBox = new ZFindBox();

			location1Label.Font.Size = 8;
			location2Label.Font.Size = 8; // replace with css

			SetFilterControlID(location1FindBox, "Location1");
			location1FindBox.ModuleID = WebModuleIDs.GetWebModuleIDFromModuleID(ModuleIDs.Location);
			location1FindBox.BindTo = "Property1";
			location1FindBox.BindToList = "List1";
			location1FindBox.TextChanged += ModuleFilterTextChanged;

			SetFilterControlID(location2FindBox, "Location2");
			location2FindBox.ModuleID = WebModuleIDs.GetWebModuleIDFromModuleID(ModuleIDs.Location);
			location2FindBox.BindTo = "Property2";
			location2FindBox.BindToList = "List2";
			location2FindBox.TextChanged += ModuleFilterTextChanged;

			FilterClauseCell.Align = "right";
			FilterClauseCell.Controls.Add(location1Label);
			FilterClauseCell.Controls.Add(new ZTextLabelWhiteSpace(ZFilterStripConstants.Spacing.AfterLabelInFilterClauseCell));

			HtmlTable table = GetNewTableWithFilterControls(new Control[] { location1FindBox }, new Control[] { location2Label, location2FindBox });
			FilterControlsCell.Controls.Add(table);

			location1FindBox.Bind(moduleFilter);
			location2FindBox.Bind(moduleFilter);

			ControlsToCaptureReturnKey.Add(location1FindBox.TextBoxControl);
			ControlsToCaptureReturnKey.Add(location2FindBox.TextBoxControl);
		}

		#endregion

		#region Flags Filter Controls

		void BuildFlagsFilterControls()
		{
			ModuleFlagsFilter moduleFilter = BizO.CurrentModuleFilter as ModuleFlagsFilter;
			if (moduleFilter != null)
			{
				ZCheckBox[] checkBoxes = new ZCheckBox[moduleFilter.FlagNames.Length];

				for (int i = 0; i < checkBoxes.Length; i++)
				{
					checkBoxes[i] = new ZCheckBox();
					SetFilterControlID(checkBoxes[i], i.ToString());
					checkBoxes[i].BindTo = "Property" + i;
					checkBoxes[i].Font.Size = 8;
					checkBoxes[i].Text = moduleFilter.FlagNames[i];

					FilterControlsCell.Controls.Add(checkBoxes[i]);
					checkBoxes[i].Bind(moduleFilter);
				}
			}
		}

		#endregion

		#region Number Range Controls

		void BuildNumberRangeControls()
		{
			ZTextLabel fromLabel = new ZTextLabel(Res.GetString("ed722f81-2f92-47b0-9a7a-1c37347eb01c", "From:"));
			ZTextLabel toLabel = new ZTextLabel(Res.GetString("b5e5339a-9eef-43a9-af09-d07218bd2d44", "To:"));
			fromLabel.Font.Size = 8;
			toLabel.Font.Size = 8;
			toLabel.Style[HtmlTextWriterStyle.PaddingLeft] = "8px";

			ZNumericTextBox fromCalcEdit = new ZNumericTextBox();
			ZNumericTextBox toCalcEdit = new ZNumericTextBox();

			SetFilterControlID(fromCalcEdit, "1");
			SetFilterControlID(toCalcEdit, "2");
			fromCalcEdit.BindTo = "Property1";
			toCalcEdit.BindTo = "Property2";

			FilterControlsCell.Controls.Add(fromLabel);
			FilterControlsCell.Controls.Add(fromCalcEdit);
			FilterControlsCell.Controls.Add(toLabel);
			FilterControlsCell.Controls.Add(toCalcEdit);

			fromCalcEdit.Bind(BizO.CurrentModuleFilter);
			toCalcEdit.Bind(BizO.CurrentModuleFilter);

			ControlsToCaptureReturnKey.Add(fromCalcEdit);
			ControlsToCaptureReturnKey.Add(toCalcEdit);
		}

		#endregion

		HtmlTable GetNewTableWithFilterControls(Control[] leftAlignedControls, Control[] rightAlignedControls)
		{
			HtmlTable table = new HtmlTable();
			HtmlTableRow row = new HtmlTableRow();
			HtmlTableCell leftCell = new HtmlTableCell();
			HtmlTableCell rightCell = new HtmlTableCell();

			table.CellPadding = 0;
			table.CellSpacing = 0;
			table.Width = "100%";

			leftCell.NoWrap = true;
			rightCell.NoWrap = true;
			rightCell.Align = "right";

			foreach (Control control in leftAlignedControls)
			{
				leftCell.Controls.Add(control);
			}
			foreach (Control control in rightAlignedControls)
			{
				rightCell.Controls.Add(control);
			}

			row.Cells.Add(leftCell);
			row.Cells.Add(rightCell);
			table.Rows.Add(row);

			return table;
		}

		HtmlGenericControl GetEventReferenceControls(string bindToList)
		{
			var container = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
			container.Attributes["class"] = "EventReferenceFilterStripRow";

			var eventRefLabel = new ZTextLabel();
			eventRefLabel.Text = Res.GetString("8da296f0-dfa2-430e-b866-385685a8c1a1", "Event Reference:");

			var eventRefTextBox = new WebControls.ZTextBox();
			SetFilterControlID(eventRefTextBox, "EventReference");
			eventRefTextBox.BindTo = "EventReference";

			var dropList = GetFilterClauseDropDownList(eventRefTextBox, "EventReferenceComparisonOption", bindToList);

			eventRefTextBox.Bind(BizO.CurrentModuleFilter);

			container.Controls.Add(eventRefLabel);
			container.Controls.Add(dropList);
			container.Controls.Add(eventRefTextBox);

			return container;
		}

		#endregion

		#region Dispose

		public override void Dispose()
		{
			UnhookDataSourceHandlers(bizO);
			base.Dispose();
		}

		#endregion
	}

	#endregion
}
