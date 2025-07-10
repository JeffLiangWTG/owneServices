using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class DragStatusHandler : MessageHandlerWithReturn<bool>
	{
		protected override bool DoHandle(IEnterpriseChannel channel, Stream messageData)
		{
			var result = ApplicationDispatcher.Current?.Invoke(new DragStatusHandlerDelegate(HandleBody), new StreamReader(messageData, Encoding.UTF8).ReadToEnd());
			return result != null && (bool)result;
		}

		protected bool HandleBody(string message)
		{
			Form waitingActiveForm;
			try
			{
				waitingActiveForm = GetDragDropHelperInstance().HandleRemoteWindowTitleAndHandleMessage(message, false, false);
			}
			catch (ObjectDisposedException)
			{
				return false;
			}

			return waitingActiveForm != null && !waitingActiveForm.IsDisposed;
		}

		public virtual DragDropHelper GetDragDropHelperInstance()
		{
			return DragDropHelper.Instance;
		}

		delegate bool DragStatusHandlerDelegate(string message);
	}
}
