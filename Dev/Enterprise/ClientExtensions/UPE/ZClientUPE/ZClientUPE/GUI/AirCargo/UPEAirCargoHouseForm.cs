using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI
{
	public partial class UPEAirCargoHouseForm : AirCargoHouseForm
	{
		[Obsolete("Design-time only", true)]
		public UPEAirCargoHouseForm()
		{
		}

		public UPEAirCargoHouseForm(UPECusHAWB businessEntity)
			: base(businessEntity)
		{
			PlugIns.Add(ControllerIDs.ProcessQueue);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			HookEvents();
		}

		public new UPECusHAWB BusinessEntity
		{
			get { return (UPECusHAWB)base.BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return base.FormCaption + " - " + BusinessEntity.CS_HAWB; }
		}

		#region Event Handlers

		CusHAWBGuiEventHandlers CusHAWBGuiEventHelper;
		UPEProcessQueueGuiEventHandlers UPEProcessQueueGuiEventHelper;

		void HookEvents()
		{
			CusHAWBGuiEventHelper = GetCusHAWBGuiEventHandlers();
			CusHAWBGuiEventHelper.HookEvents();

			UPEProcessQueueGuiEventHelper = new UPEProcessQueueGuiEventHandlers(BusinessEntity.CurrentQueue);
			UPEProcessQueueGuiEventHelper.HookEvents();
		}

		protected virtual CusHAWBGuiEventHandlers GetCusHAWBGuiEventHandlers()
		{
			return new CusHAWBGuiEventHandlers(BusinessEntity);
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			if (CusHAWB.Messages.Count == 0 && ConfirmPreAlertToCustoms() == DialogResult.Yes)
			{
				CusHAWB.SendAirCargoMessage();
				if (CusHAWB.HasMessageErrors || CusHAWB.HasErrors)
				{
					Globals.Message.ShowError("Could not pre-alert HAWB to customs because of the following errors:\n" + CusHAWB.MessageErrorsString, "Air Cargo Errors");
				}
			}

			ContinueWithSave result = CusHAWBGuiEventHelper.RunPreSaveDialogs(BusinessEntity);
			if (result == ContinueWithSave.Yes)
			{
				result = base.ValidateAndSave();
				Activate();
			}

			return result;
		}
		internal ContinueWithSave InternalValidateAndSave() => ValidateAndSave();

		DialogResult ConfirmPreAlertToCustoms()
		{
			return Globals.Message.Show("Do you wish to pre-alert the HAWB to Customs?", "Pre-Alert HAWB", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		#endregion

		#region Implementation

		internal AlertForm AlertForm;

		UPECusHAWB CusHAWB
		{
			get { return BusinessEntity; }
		}

		protected override void OnLoad(EventArgs e)
		{
			if (!this.IsDesignMode())
			{
				DisableNewAction();
			}
			base.OnLoad(e);
			if (!this.IsDesignMode())
			{
				BusinessEntity.RunScreeningValidation();
				BeginInvoke(new MethodInvoker(ShowAlertForm));
			}
		}

		void ShowAlertForm()
		{
			if (CusHAWB.HasAlerts)
			{
				AlertForm = new AlertForm(new Alert(CusHAWB.AlertsList, BusinessEntity.Factory));
				AlertForm.Show();
			}
		}

#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (CusHAWBGuiEventHelper != null)
				{
					CusHAWBGuiEventHelper.Dispose();
				}
				if (UPEProcessQueueGuiEventHelper != null)
				{
					UPEProcessQueueGuiEventHelper.Dispose();
				}
				if (AlertForm != null)
				{
					AlertForm.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
			}
}
