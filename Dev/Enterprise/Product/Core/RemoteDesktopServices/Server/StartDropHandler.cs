using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class StartDropHandler : IMessageHandler
	{
		public void Handle(IEnterpriseChannel channel, Stream messageData)
		{
			ApplicationDispatcher.Current?.Invoke(() =>
			{
				var message = new StreamReader(messageData, Encoding.UTF8).ReadToEnd();
				waitingActiveForm = GetDragDropHelperInstance().HandleRemoteWindowTitleAndHandleMessage(message, true);
			});
		}

		public virtual DragDropHelper GetDragDropHelperInstance()
		{
			return DragDropHelper.Instance;
		}

		public static Form UseWaitingActiveForm()
		{
			var form = waitingActiveForm;
			if (form != null)
			{
				Application.UseWaitCursor = false;
				form.Focus();
				waitingActiveForm = null;
			}
			return form;
		}
		static Form waitingActiveForm;
	}
}
