using System;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public class CusHAWBGuiEventHandlers : IDisposable
	{
		public CusHAWBGuiEventHandlers(UPECusHAWB cusHAWB)
		{
			this.CusHAWB = cusHAWB;
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public readonly UPECusHAWB CusHAWB;

		virtual
 public void HookEvents()
		{
			if (CusHAWB != null)
			{
				CusHAWB.IsRedirectedChanging += new IsRedirectedChangingEventHandler(OnCusHAWB_IsRedirectedChanging);
				CusHAWB.RebillFlagChanging += new RebillFlagChangingEventHandler(OnCusHAWB_RebillFlagChanging);
				CusHAWB.QueryShipmentHeldLetterDetails += new QueryShipmentHeldLetterDetailsEventHandler(OnCusHAWB_QueryShipmentHeldLetterDetails);
				CusHAWB.PrintBatchItemQueued += new PrintBatchItemQueuedEventHandler(OnCusHAWB_PrintBatchItemQueued);
			}
		}

		public void UnhookEvents()
		{
			if (CusHAWB != null)
			{
				CusHAWB.IsRedirectedChanging -= new IsRedirectedChangingEventHandler(OnCusHAWB_IsRedirectedChanging);
				CusHAWB.RebillFlagChanging -= new RebillFlagChangingEventHandler(OnCusHAWB_RebillFlagChanging);
				CusHAWB.QueryShipmentHeldLetterDetails -= new QueryShipmentHeldLetterDetailsEventHandler(OnCusHAWB_QueryShipmentHeldLetterDetails);
				CusHAWB.PrintBatchItemQueued -= new PrintBatchItemQueuedEventHandler(OnCusHAWB_PrintBatchItemQueued);
			}
		}

		virtual
 public ContinueWithSave RunPreSaveDialogs(IBusiness topLevelEntity)
		{
			ContinueWithSave result = ContinueWithSave.Yes;
			if (CusHAWB != null && CusHAWB.ShipmentHeldLetterDetails.QueueMovementRequiresAutoDelivery())
			{
				topLevelEntity.RunPreSaveValidation();
				if (!topLevelEntity.HasErrors())
				{
					result = ShowShipmentHeldLetterForm();
				}
			}
			return result;
		}

		ContinueWithSave ShowShipmentHeldLetterForm()
		{
			if (ZFormModaliser.ShowDialogAndDispose(NewShipmentHeldLetterForm(CusHAWB.ShipmentHeldLetterDetails, ShipmentHeldLetterRecipient.Unknown)) != DialogResult.OK)
			{
				return ContinueWithSave.No;
			}
			return ContinueWithSave.Yes;
		}

		#region IDisposable Members

		public void Dispose()
		{
			UnhookEvents();
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#endregion

		#region Implementation

		void OnCusHAWB_IsRedirectedChanging(IsRedirectedChangingEventArgs eventArgs)
		{
			if (!eventArgs.IsRedirected)
			{
				string confirmationMessage = "Removing the override will reset your Delivery Address details.\r\nYou will lose changes that you have made to the Delivery Address Redirection.";
				DialogResult dialogResult = Globals.Message.Show(confirmationMessage, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				eventArgs.Cancel = (dialogResult == DialogResult.No);
			}
		}

		void OnCusHAWB_RebillFlagChanging(RebillFlagChangingEventArgs eventArgs)
		{
			if (eventArgs.RebillFlag != RebillFlags.Unflagged)
			{
				using (CustomFlagForm form = CustomFlagForm.New(eventArgs.RebillFlag, CusHAWB))
				{
					if (form != null)
					{
						eventArgs.Cancel = (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.Cancel);
					}
				}
			}
		}

		void OnCusHAWB_QueryShipmentHeldLetterDetails(object sender, QueryShipmentHeldLetterDetailsEventArgs e)
		{
			using (ShipmentHeldLetterForm form = NewShipmentHeldLetterForm(e.BizObj, e.Recipient))
			{
				e.Cancel = (ZFormModaliser.ShowDialogWithoutDispose(form) != DialogResult.OK);
			}
		}

		void OnCusHAWB_PrintBatchItemQueued(object sender, PrintBatchItemQueuedEventArgs e)
		{
			if (e.NotifyUser)
			{
				Globals.Message.ShowInformation(e.PrintItem.PrintItemQueuedMessage);
			}
		}

		protected virtual ShipmentHeldLetterForm NewShipmentHeldLetterForm(ShipmentHeldLetterBusinessObject bizObj, ShipmentHeldLetterRecipient recipient)
		{
			return new ShipmentHeldLetterForm(bizObj, recipient);
		}

		#endregion
	}
}
