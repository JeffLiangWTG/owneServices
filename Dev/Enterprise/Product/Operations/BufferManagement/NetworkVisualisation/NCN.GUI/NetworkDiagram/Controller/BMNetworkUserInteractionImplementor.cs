using System;
using System.Windows.Forms;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public class BMNetworkUserInteractionImplementor : IBMNetworkUserInteractionImplementor
	{
		public BMNetworkUserInteractionImplementor(IProgressReporterProvider progressReporterProvider)
		{
			ProgressReporterProvider = progressReporterProvider;
		}

		public IProgressReporterProvider ProgressReporterProvider { get; }

		public bool HasUserConfirmed(string message, string caption, params ConfirmationNotification[] notifications)
		{
			if (notifications.Length > 0)
			{
				return Globals.Message.ShowConfirmationWithNotifications(new ConfirmationDialogDescriptor(message, caption, notifications)) == ZDialogResult.OK;
			}
			else
			{
				return Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, DialogResult.OK) == DialogResult.OK;
			}
		}

		public bool HasUserAnsweredYes(string message, string caption)
		{
			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes;
		}

		public void ShowMessage(string message)
		{
			Globals.Message.Show(message);
		}

		public void ShowMessage(string message, string caption)
		{
			Globals.Message.Show(message, caption, MessageBoxButtons.OK, DialogResult.OK);
		}

		public void ShowError(string message)
		{
			Globals.Message.ShowError(message);
		}

		public void ShowError(string message, string caption)
		{
			Globals.Message.ShowError(message, caption);
		}

		public T ShowMultiOptionDialog<T>(string message, string caption, T defaultValue, ButtonStripAction<T>[] buttonStrips)
			where T : struct, IConvertible
		{
			return new MultiActionButtonDialogWrapper<T>().ShowDialog(message, caption, defaultValue, buttonStrips);
		}
	}
}
