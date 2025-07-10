using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class BaseACAStandAloneUserControl : BaseACAMAWBUserControl
	{
		public BaseACAStandAloneUserControl()
		{
			InitializeComponent();
			ChangeBindingToFilteredList();
			MessagesTabPage.AdditionalText = "Messages";
		}

		protected CusMAWB CusMAWB
		{
			get { return CurrentDataItem as CusMAWB; }
		}

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			MenuItem editItem = new ZMenuItem("Edit", new EventHandler(EditHAWB));
			HouseBillsModuleButtonGrid.ContextMenu.MenuItems.Add(editItem);
		}

		void EditHAWB(object sender, EventArgs args)
		{
			if (SelectedHAWB != null)
			{
				if (SelectedHAWB.IsInDatabase)
				{
					if (SelectedHAWB.Shipment == null)
					{
						lastShownForm = HAWBController.ShowEditForm(SelectedHAWB);
					}
					else
					{
						Globals.Message.ShowWarning("Please edit shipments from the Consol Details tab.");
					}
				}
				else
				{
					Globals.Message.ShowWarning("Please save the form before you edit this HouseBill.");
				}
			}
			else
			{
				Globals.Message.ShowWarning("Please select a Housebill to Edit.");
			}
		}

		protected IZForm lastShownForm;

		ZController HAWBController
		{
			get { return ZControllerFactory.Create(ControllerIDs.Customs.AU.HouseAirCargo); }
		}

		protected CusHAWB SelectedHAWB
		{
			get
			{
				return (CusHAWB)HouseBillsModuleButtonGrid.ListManager.GetCurrent();
			}
		}

		#endregion

		#region Plugins

		public void SetupPlugins()
		{
			if (!pluginsSetup)
			{
				MasterTabControl.PlugIns.Add(ControllerIDs.Customs.AU.AirCargoCusUnderbondPluginController);
				HAWBTabControl.PlugIns.AddCurrentDependentPlugIn(ControllerIDs.Customs.AU.AirCargoDeclarationCusUnderbondController, HouseBillsModuleButtonGrid);
				pluginsSetup = true;
			}
		}

		bool pluginsSetup;

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				UnhookHouseBillsModuleButtonGridEvents();
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Binding and Events

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			HouseBillsModuleButtonGrid.AfterBind -= new EventHandler(InnerGrid_AfterBind);
			if (dataSource != null)
			{
				HouseBillsModuleButtonGrid.AfterBind += new EventHandler(InnerGrid_AfterBind);
			}
			base.SetDataBinding(dataSource, dataMember);

			if (!AirCargoCustomFieldsControl.IsDisposed)
			{
				AirCargoCustomFieldsControl.ForceBindingIncludingParents();
			}
			if (!AirCargoHouseCustomFieldsControl.IsDisposed)
			{
				AirCargoHouseCustomFieldsControl.ForceBindingIncludingParents();
			}
			SetColumnsVisibility();
		}

		void SetColumnsVisibility()
		{
			this.HouseBillsModuleButtonGrid.RemoveFromAvailableColumns("CS_OH_Consignee", "CS_OH_Consignor");
		}

		void InnerGrid_AfterBind(object sender, EventArgs e)
		{
			int indexOfCurrentHouseBill = HouseBillsModuleButtonGrid.List == null ? -1 : HouseBillsModuleButtonGrid.List.IndexOf(CusMAWB.CurrentHouseBill);
			if (indexOfCurrentHouseBill >= 0)
			{
				HouseBillsModuleButtonGrid.Select(indexOfCurrentHouseBill);
				HouseBillsModuleButtonGrid.CurrentRowIndex = indexOfCurrentHouseBill;
			}

			UpdateMenuTextAndBizO();
			HookHouseBillsModuleButtonGridEvents();

			HouseDetailsTabPage.Hook(HouseBillsModuleButtonGrid.ListManager);
			MessagesTabPage.Hook(HouseBillsModuleButtonGrid.ListManager);
			HookCustomFields();
		}

		void HookCustomFields()
		{
			if (!customFieldsHooked && DataSource is CusMAWB mawb)
			{
				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(HouseBillsModuleButtonGrid, mawb.FilteredChildBills, WorkflowDescriptors.CustomsHouseAirCargoCode, true);
				customFieldsHooked = true;
			}
		}
		bool customFieldsHooked;

		protected void HookHouseBillsModuleButtonGridEvents()
		{
			if (HouseBillsModuleButtonGrid != null && HouseBillsModuleButtonGrid.ListManager != null)
			{
				HouseBillsModuleButtonGrid.ListManager.CurrentChanged += new EventHandler(HouseBillsGrid_CurrentChanged);
			}
		}

		void UnhookHouseBillsModuleButtonGridEvents()
		{
			if (HouseBillsModuleButtonGrid != null && HouseBillsModuleButtonGrid.ListManager != null)
			{
				HouseBillsModuleButtonGrid.ListManager.CurrentChanged -= new EventHandler(HouseBillsGrid_CurrentChanged);
			}
		}

		void HouseBillsGrid_CurrentChanged(object sender, EventArgs e)
		{
			UpdateMenuTextAndBizO();
		}

		void UpdateMenuTextAndBizO()
		{
			AirCargoMasterForm masterForm = ParentForm as AirCargoMasterForm;
			if (HouseBillsModuleButtonGrid.List.Count > 0)
			{
				var current = HouseBillsModuleButtonGrid.ListManager.GetCurrent();
				if (masterForm != null)
				{
					masterForm.OnCurrentHouseBillChanged(current as CusHAWB);
				}
				HAWB = current as CusHAWBBase;
			}
		}

		void ChangeBindingToFilteredList()
		{
			BindingSource.SetBindingMember(airCagoHouseBillPartiesUserControl, "FilteredChildBills");
		}

		void OnFilterButtonClick(object sender, EventArgs e)
		{
			CusMAWB.FilteredChildBills.Rebuild();
		}

		void OnClearButtonClick(object sender, EventArgs e)
		{
			CusMAWB.FilteredChildBills.ClearFilter();
			CusMAWB.FilteredChildBills.Rebuild();
		}

		void HouseBillsModuleButtonGrid_DoubleClick(object sender, EventArgs e)
		{
			EditHAWB(sender, e);
		}

		#endregion
	}
}
