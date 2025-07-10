using System;
using System.ComponentModel;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;
using Component = Enterprise.Customs.CA.Business.Component;

namespace Enterprise.Customs.CA.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[DefaultBindingProperty("CNSCPGAHeader")]
	public partial class CNSCUserControl : ZUserControl
	{
		public CNSCUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			InitializeLazyCreate(isOnInvoiceLine);
		}

		protected void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			CategoryDropEdit.SelectedIndexChanged += new EventHandler(CategoryDropEdit_SelectedIndexChanged);

			LPCOGridUserControl.RemoveExceptAvailableColumns(CNSCPGAHeader.AvailableLPCOFields);

			if (!isOnInvoiceLine)
			{
				DetailsGroupBox.Controls.Remove(DeliveredPartyAddressControl);
				DetailsGroupBox.Controls.Remove(UNDGGuidFindBox);
				DetailsGroupBox.Controls.Remove(TotalQuantityCalcDropEdit);

				BottomSplitContainer.Panel2.Controls.Remove(LPCOGroupBox);
				CNSCSplitContainer.Panel2.Controls.Add(LPCOGroupBox);
				BottomSplitContainer.Dispose();

				CountIntEdit.Dispose();
				PackQtyCalcEdit.Dispose();

				NNIECRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(525, 19, true);
				PackMarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 45, true);
			}
		}

		public new CNSCPGAHeader CurrentDataItem => base.CurrentDataItem as CNSCPGAHeader;

		void CategoryDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetComponentFieldsCaptions();
		}

		void SetComponentFieldsCaptions()
		{
			var currentDataItem = CurrentDataItem;
			if (currentDataItem == null)
			{
				return;
			}

			NNIECRTextBox.Visible = currentDataItem.IsEquipment;
			ComponentGrid.ReadOnly = currentDataItem.IsEquipment;

			if (currentDataItem.IsRadiationDevice)
			{
				ComponentGrid.SetColumnCaption(Component.Schema.CA_Qty, "Activity");
				ComponentGrid.SetColumnCaption(Component.Schema.CA_Name, "Name");
			}
			else if (currentDataItem.IsSubstance)
			{
				ComponentGrid.SetColumnCaption(Component.Schema.CA_Qty, "Activity Concentration");
				ComponentGrid.SetColumnCaption(Component.Schema.CA_Name, "Name");
			}
			else if (currentDataItem.IsControlledSubstance)
			{
				ComponentGrid.SetColumnCaption(Component.Schema.CA_Qty, "Total Mass");
				ComponentGrid.SetColumnCaption(Component.Schema.CA_Name, "Specification");
			}
		}
	}
}
