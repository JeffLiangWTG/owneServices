using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.GUI
{
	public class UPEProcessQueueGuiEventHandlers : IDisposable
	{
		public UPEProcessQueueGuiEventHandlers(UPEProcessQueue uPEProcessQueue)
		{
			this.UPEProcessQueue = uPEProcessQueue;
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public readonly UPEProcessQueue UPEProcessQueue;

		public void HookEvents()
		{
			if (UPEProcessQueue != null)
			{
				UPEProcessQueue.AskHasEIRBeenRaised += new CancelEventHandler(OnUPEProcessQueue_AskHasEIRBeenRaised);
			}
		}

		public void UnhookEvents()
		{
			if (UPEProcessQueue != null)
			{
				UPEProcessQueue.AskHasEIRBeenRaised -= new CancelEventHandler(OnUPEProcessQueue_AskHasEIRBeenRaised);
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			UnhookEvents();
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#endregion

		#region Implementation

		void OnUPEProcessQueue_AskHasEIRBeenRaised(object sender, CancelEventArgs eventArgs)
		{
			string confirmationMessage = "Has EIR been raised.";
			DialogResult dialogResult = Globals.Message.Show(confirmationMessage, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			eventArgs.Cancel = (dialogResult == DialogResult.No);
		}

		#endregion
	}
}
