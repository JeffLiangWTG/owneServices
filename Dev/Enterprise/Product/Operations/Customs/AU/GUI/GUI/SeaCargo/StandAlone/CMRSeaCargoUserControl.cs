using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class CMRSeaCargoUserControl : ZUserControl
	{
		public CMRSeaCargoUserControl(bool isStandalone)
			: this()
		{
			this.isStandalone = isStandalone;
		}

		public CMRSeaCargoUserControl()
		{
			InitializeComponent();
			MessageUserControl.ReplaceBindingMessagesCollectionWithAnotherCollection("MessagesForBinding");
			MessageUserControl.SetBindPrepend("FilteredHouseBills.");
			OceanBillSpecificsTabControl.SelectedIndexChanged += OceanBillSpecificsTabControl_SelectedIndexChanged;
		}

		readonly bool isStandalone;

		public event EventHandler VisibilityChanged;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			PackingForContainersControl.PackingHouseGrid.AfterBind -= new EventHandler(PackingHouseGrid_AfterBind);
			PackingForContainersControl.PackingHouseGrid.AfterBind += new EventHandler(PackingHouseGrid_AfterBind);
			HouseBillsGrid.AfterBind -= new EventHandler(HouseBillsGrid_AfterBind);
			HouseBillsGrid.AfterBind += new EventHandler(HouseBillsGrid_AfterBind);
			base.SetDataBinding(dataSource, dataMember);

			if (!CustomFieldsControl.IsDisposed)
			{
				CustomFieldsControl.ForceBindingIncludingParents();
			}

			if (!HouseBillCustomFieldsControl.IsDisposed)
			{
				HouseBillCustomFieldsControl.ForceBindingIncludingParents();
			}

			if (OceanBill != null)
			{
				HookCustomFields();
			}
			SetColumnsVisibility();
		}

		void SetColumnsVisibility()
		{
			this.HouseBillsGrid.RemoveFromAvailableColumns("CA_OH_Consignee", "CA_OH_Consignor");
		}

		#region HookCustomFields

		void HookCustomFields()
		{
			if (!customFieldsHooked)
			{
				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(HouseBillsGrid, OceanBill.FilteredHouseBills, WorkflowDescriptors.CusSCAHouseWorkflowDescriptorCode, true);
				customFieldsHooked = true;
			}
		}

		bool customFieldsHooked;

		#endregion

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			var oceanBill = OceanBill;
			if (oceanBill != null)
			{
				oceanBill.FilteredHouseBills.DeletingHouseBillWhenDisallowed -= new EventHandler(OnDeletingHouseBillWhenDisallowed);
				oceanBill.CB_MultiOBLUnpackInfo.ValueChanged -= new EventHandler(CB_MultiOBLUnpackInfo_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var oceanBill = OceanBill;
			if (oceanBill != null)
			{
				oceanBill.FilteredHouseBills.DeletingHouseBillWhenDisallowed += new EventHandler(OnDeletingHouseBillWhenDisallowed);
				oceanBill.CB_MultiOBLUnpackInfo.ValueChanged += new EventHandler(CB_MultiOBLUnpackInfo_ValueChanged);

				ChangeVisibility(oceanBill.CB_MultiOBLUnpack);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SetupPlugIns();
			SetupHouseBillEditing();
			OceanBillDetailsUserControl.SetMessagingModeVisiblity(!isStandalone);
		}

		public CusSCAOceanBill OceanBill
		{
			get { return (CusSCAOceanBill)CurrentDataItem; }
		}

		void OnDeletingHouseBillWhenDisallowed(object sender, EventArgs e)
		{
			Globals.Message.ShowError("You should withdraw the HouseBill prior to delete.");
		}

		public void ChangeVisibility(bool isOceanBillUnpack)
		{
			OceanBillDetailsUserControl.UpdateForOceanBillUnpack();
			HouseBillDetailsUserControl.UpdateForOceanBillUnpack();

			var labelText = isOceanBillUnpack ? OceanBillLabelText : HouseBillLabelText;
			HouseBillsTabPage.Text = labelText + "s";
			ChangeColumnHeader(HouseBillsGrid, Customs.Business.AutoCusSCAHouse.Schema.CA_HouseBill, labelText);
			ChangeColumnHeader(PackingForContainersControl.PackingHouseGrid, CusSCAPivot.Schema.CV_AssociatedHouse, "Associated " + labelText);
			OnVisibilityChanged(EventArgs.Empty);
		}

		internal const string OceanBillLabelText = "Ocean Bill";
		internal const string HouseBillLabelText = "House Bill";

		protected virtual void OnVisibilityChanged(EventArgs e)
		{
			if (VisibilityChanged != null)
			{
				VisibilityChanged(this, e);
			}
		}

		void ChangeColumnHeader(ZGrid grid, string columnName, string text)
		{
			ZGridColumn column = grid.Columns[columnName];
			if (column != null)
			{
				column.ColumnStyle.HeaderText = text;
				grid.RefreshTableStyles();
			}
		}

		public void SetupPlugIns()
		{
			if (this.OceanBillSpecificsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.AU.CusSCAContainerUnderbondPluginController) == null)
			{
				this.OceanBillSpecificsTabControl.PlugIns.Add(ControllerIDs.Customs.AU.CusSCAContainerUnderbondPluginController);
			}
		}

		#region EditHouseBill

		void SetupHouseBillEditing()
		{
			if (CustomsDataRegistry.Instance.IsAUSeaCargoHouseEnabled)
			{
				var editItem = new ZMenuItem("Edit", new EventHandler(EditHouseBill));
				HouseBillsGrid.ContextMenu.MenuItems.Add(editItem);
				HouseBillsGrid.DoubleClick += new EventHandler(HouseBillsGrid_DoubleClick);
			}
		}

		void HouseBillsGrid_DoubleClick(object sender, EventArgs e)
		{
			EditHouseBill(sender, e);
		}

		void EditHouseBill(object sender, EventArgs args)
		{
			var houseBill = SelectedHouseBill;
			if (houseBill == null)
			{
				Globals.Message.ShowWarning("Please select a Housebill to Edit.");
			}
			else if (houseBill.Shipment != null)
			{
				Globals.Message.ShowWarning("Please edit shipments from the Consol Details tab.");
			}
			else if (!houseBill.IsInDatabase)
			{
				Globals.Message.ShowWarning("Please save the form before you edit this HouseBill.");
			}
			else
			{
				lastShownForm = SeaCargoHouseController.ShowEditForm(houseBill);
			}
		}
		protected IZForm lastShownForm; // so the form can be closed during testing

		ZController SeaCargoHouseController => seaCargoHouseController ?? (seaCargoHouseController = ZControllerFactory.Create(ControllerIDs.Customs.AU.SeaCargoHouseController));
		ZController seaCargoHouseController;

		protected CusSCAHouse SelectedHouseBill => HouseBillsGrid.ListManager.GetCurrent() as CusSCAHouse;

		#endregion

		void CB_MultiOBLUnpackInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeVisibility(OceanBill.CB_MultiOBLUnpack);
		}

		void HouseBillsGrid_AfterBind(object sender, EventArgs e)
		{
			ChangeVisibility(OceanBill.CB_MultiOBLUnpack);
		}

		void PackingHouseGrid_AfterBind(object sender, EventArgs e)
		{
			ChangeVisibility(OceanBill.CB_MultiOBLUnpack);
		}

		void OceanBillSpecificsTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			ZTabPage tabPage = OceanBillSpecificsTabControl.SelectedTab;
			if (tabPage != null)
			{
				foreach (Control control in tabPage.Controls)
				{
					CusUnderbondUserControl underbondControl = control as CusUnderbondUserControl;
					if (underbondControl != null)
					{
						underbondControl.OutturnDisabled = true;
					}
				}
			}
		}

		void OnClearButtonClick(object sender, EventArgs e)
		{
			OceanBill.FilteredHouseBills.ClearFilter();
			OceanBill.FilteredHouseBills.Rebuild();
		}

		void OnFilterButtonClick(object sender, EventArgs e)
		{
			OceanBill.FilteredHouseBills.Rebuild();
		}

		void CustomFieldsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.CustomFieldsControl = new SeaCargoCustomFieldsUserControl();
			this.CustomFieldsTabPage.SuspendLayout();
			this.CustomFieldsControl.SuspendLayout();
			this.CustomFieldsTabPage.Controls.Add(this.CustomFieldsControl);
			// 
			// CustomFieldsControl
			// 
			this.CustomFieldsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomFieldsControl, "OceanBill");
			this.CustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CustomFieldsControl.Name = "CustomFieldsControl";
			this.CustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 571, true);
			this.CustomFieldsControl.TabIndex = 1;
			this.CustomFieldsTabPage.PerformLayout();
			this.CustomFieldsControl.ResumeLayout(true);
			this.CustomFieldsControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(true);
		}

		void HouseBillsCustomFieldsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.HouseBillCustomFieldsControl = new SeaCargoHouseBillCustomFieldsUserControl();
			this.HouseBillCustomFieldsTabPage.SuspendLayout();
			this.HouseBillCustomFieldsControl.SuspendLayout();
			this.HouseBillCustomFieldsTabPage.Controls.Add(this.HouseBillCustomFieldsControl);
			// 
			// CustomFieldsControl
			// 
			this.HouseBillCustomFieldsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseBillCustomFieldsControl, "FilteredHouseBills");
			this.HouseBillCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBillCustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HouseBillCustomFieldsControl.Name = "HouseBillCustomFieldsControl";
			this.HouseBillCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 308, true);
			this.HouseBillCustomFieldsControl.TabIndex = 1;
			this.HouseBillCustomFieldsTabPage.PerformLayout();
			this.HouseBillCustomFieldsControl.ResumeLayout(true);
			this.HouseBillCustomFieldsControl.PerformLayout();
			this.HouseBillCustomFieldsTabPage.ResumeLayout(true);
		}
	}
}
