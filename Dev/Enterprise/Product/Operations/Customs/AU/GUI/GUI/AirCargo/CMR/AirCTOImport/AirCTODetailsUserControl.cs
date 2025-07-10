using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCTOUserDetailsControl : BaseACAMAWBUserControl
	{
		public AirCTOUserDetailsControl()
		{
			InitializeComponent();
			HAWBTabControl.TabPages.Insert(this.messagesTabPage, 1);
			BindingSource.SetBindingMember(messagesUserControl, "ChildBills");
			BindingSource.SetBindingMember(ctoHouseDetailsUserControl, "ChildBills");
			messagesTabPage.AdditionalText = "Messages";
			masterBillsGrid.DoubleClick -= MasterBillsGrid_DoubleClick;
			masterBillsGrid.DoubleClick += MasterBillsGrid_DoubleClick;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (BindingSource.DataSource != null)
			{
				HouseDetailsTabPage.Unhook();
				messagesTabPage.Unhook();
				SetCurrentMasterBillFromGrid();
				ChildBillsListManager.CurrentChanged -= ChildBillsListManager_CurrentChanged;
			}
			base.SetDataBinding(dataSource, dataMember);
			if (BindingSource.DataSource != null)
			{
				HouseDetailsTabPage.Hook(masterBillsGrid.ListManager);
				messagesTabPage.Hook(masterBillsGrid.ListManager);
				ChildBillsListManager.CurrentChanged += ChildBillsListManager_CurrentChanged;
				SetCurrentMasterBillFromGrid();
			}
		}

		#region Export Contingency Data

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			MenuItem exportItem = new ZMenuItem("Create Contingency Data", new EventHandler(ExportHouseToCSV));
			masterBillsGrid.ContextMenu.MenuItems.Add(exportItem);
		}

		void ExportHouseToCSV(object sender, EventArgs args)
		{
			if (masterBillsGrid.CurrentRowIndex >= 0)
			{
				CTOCusHAWB selectedHAWB = masterBillsGrid.ListManager.GetCurrent() as CTOCusHAWB;
				if (selectedHAWB != null)
				{
					ExportContingencyData(selectedHAWB);
				}
			}
			else
			{
				Globals.Message.ShowWarning("Please select a row before attempting to create Contingency Data.");
			}
		}

		protected virtual void ExportContingencyData(CTOCusHAWB hAWB)
		{
			new CMRExportForm((ZForm)ParentForm, new AirCTOHAWBExporter(hAWB)).Export();
		}

		#endregion

		#region ShowHawbDetailsForm

		void MasterBillsGrid_DoubleClick(object sender, EventArgs e)
		{
			ShowHawbDetailsForm();
		}

		void ShowHawbDetailsForm()
		{
			if (masterBillsGrid.ListManager.Position >= 0)
			{
				if (((IBusiness)CurrentDataItem).HasChanges)
				{
					Globals.Message.ShowError("The current form has changes, you will need to save before opening this HAWB.");
				}
				else
				{
					CTOCusHAWB hawb = (CTOCusHAWB)masterBillsGrid.ListManager.GetCurrent();
					ZController controller = ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCTOHawbImport);

#if DEBUG
					lastHawbController = controller;
#endif

					controller.ShowEditForm(hawb);
				}
			}
		}

#if DEBUG
		internal ZController lastHawbController;
#endif

		#endregion

		#region UserFriendlyStatuses

		void ChildBillsListManager_CurrentChanged(object sender, EventArgs e)
		{
			SetCurrentMasterBillFromGrid();
		}

		void SetCurrentMasterBillFromGrid()
		{
			if (ChildBillsListManager != null)
			{
				HAWB = ChildBillsListManager.GetCurrent() as CusHAWBBase;
			}
		}

		CurrencyManager ChildBillsListManager
		{
			get { return (CurrencyManager)GetBindingManager("ChildBills"); }
		}

		protected internal override AirCargoHAWBProviderContainerControl ChildControl => ctoHouseDetailsUserControl;

		#endregion
	}
}
