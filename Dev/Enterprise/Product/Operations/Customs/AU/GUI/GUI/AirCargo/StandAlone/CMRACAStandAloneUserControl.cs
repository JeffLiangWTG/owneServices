using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class CMRACAStandAloneUserControl : BaseACAStandAloneUserControl
	{
		public CMRACAStandAloneUserControl()
		{
			InitializeComponent();
			BindingSource.SetBindingMember(houseDetails, "FilteredChildBills");
		}

		#region User Friendly Statuses

		protected internal override AirCargoHAWBProviderContainerControl ChildControl => houseDetails;

		#endregion

		#region Load

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			MenuItem exportItem = new ZMenuItem("Create Contingency Data", new EventHandler(ExportHouseToCSV));
			HouseBillsModuleButtonGrid.ContextMenu.MenuItems.Add(exportItem);
			deferredScheduleDateTextBox.BringToFront();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (CusMAWB != null)
			{
				CusMAWB.DeferredScheduledMessagesEventChanged += ChangeControlsVisibility;
			}

			ChangeControlsVisibility();
		}

		#endregion

		#region Export Contingency Data

		void ExportHouseToCSV(object sender, EventArgs args)
		{
			if (SelectedHAWB != null)
			{
				ExportContingencyData(SelectedHAWB);
			}
			else
			{
				Globals.Message.ShowWarning("Please select a Housebill before attempting to create Contingency Data.");
			}
		}

		public virtual void ExportContingencyData(CusHAWB hAWB)
		{
			new CMRExportForm((ZForm)ParentForm, new AirReportHAWBExporter(hAWB)).Export();
		}

		#endregion

		#region visibility

		void ChangeControlsVisibility()
		{
			if (CusMAWB != null)
			{
				CusMAWB.RefreshBinding();
				deferredScheduleDateTextBox.Visible = !CusMAWB.DeferredScheduledDateForDisplayInCanberraTime.IsEmpty;
				AltPartShipModelCheckBox.Visible = CusMAWB.IsAltPartShipModelActive;
			}
		}

		#endregion
	}
}
