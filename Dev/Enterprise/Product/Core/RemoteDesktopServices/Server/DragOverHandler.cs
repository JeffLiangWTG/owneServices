using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.RemoteDesktopServices.Server
{
	class DragOverHandler : IMessageHandler
	{
		public void Handle(IEnterpriseChannel channel, Stream messageData)
		{
			ApplicationDispatcher.Current?.BeginInvoke(new DragOverHandlerDelegate(DoHandle), messageData);
		}

		void DoHandle(Stream message)
		{
			TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Getting active form");
			var activeForm = GetActiveForm();

			if (activeForm != null)
			{
				try
				{
					var pointLong = Convert.ToInt64(new StreamReader(message, Encoding.UTF8).ReadToEnd(), CultureInfo.InvariantCulture);
					var byteArray = BitConverter.GetBytes(pointLong);
					var pointX = BitConverter.ToInt32(byteArray, 0);
					var pointY = BitConverter.ToInt32(byteArray, 4);

					TrackingInfoLogger.Instance.NewLog(() => $"Dragging over the form [{activeForm.Text}]");
					typeof(Control).GetMethod("OnDragOver", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(activeForm, new object[] { new DragEventArgs(new DataObject(DataFormats.FileDrop, null), 0, pointX, pointY, DragDropEffects.Copy, DragDropEffects.Copy) });
					TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Dragged over the form");
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Error in dragging over: {ex}");
					ExceptionDispatchInfo.Capture(ex).Throw();
				}
			}
		}

		delegate void DragOverHandlerDelegate(Stream message);

		Form GetActiveForm()
		{
			var form = Form.ActiveForm;

			return form;
		}
	}
}
