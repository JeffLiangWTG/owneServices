using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Interop.DataObjects;
using CargoWise.Windows.UI;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Graph;
using static CargoWise.Interop.DataObjects.ZDataObjectMicrosoftOffice365;
using Application = System.Windows.Forms.Application;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class MicrosoftOffice365ObjectHandler : XmlMessageHandler<ZDataObjectMicrosoftOffice365>
	{
		protected override void Handle(IEnterpriseChannel channel, ZDataObjectMicrosoftOffice365 message)
		{
			try
			{
				if (message.AccessToken == null)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Access token is null. Client side Microsoft Office 365 login failed.\r\nDrag type: {message.Format}\r\nContent:\r\n{message.Content}");
					return;
				}
				if (message.Format == DragType.Unknown)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Drag source is not supported.\r\nContent:\r\n{message.Content}");
					return;
				}
				var targetForm = GetActiveForm(message.WindowTitleAndHwnd);
				TrackingInfoLogger.Instance.NewLog(() => $"Found target form: {targetForm.Text}");
				if (targetForm != null)
				{
					try
					{
						using (var dataObject = message.CreateDataObject())
						{
							TrackingInfoLogger.Instance.NewLog(() => $"ZDataObjectMicrosoftOffice365 message: Type - {message.Format}, Content - '{message.Content}', Token is null - {string.IsNullOrEmpty(message.AccessToken)}");
							var filePaths = string.Join(", ", (string[])dataObject.GetData(DataFormats.FileDrop));
							TrackingInfoLogger.Instance.NewLog(() => $"Dataobject created from message: File drop count - {dataObject.FileDropCount}, File list - '{filePaths}'");
							ApplicationDispatcher.Current?.BeginInvoke(new MicrosoftOffice365ObjectHandlerDelegate(DoHandle), dataObject, targetForm);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						if (ex is ServiceException serviceException)
						{
							Globals.Message.ShowError(Res.GetString("58495EEA-DA65-426A-A570-71F7FDB1E60F",
								@"Error Code: {0}. Dragging object from MS office 365 failed.
	Please make sure you are dragging from the authorized account.", serviceException.Error.Code));
						}
						TrackingInfoLogger.Instance.NewLog(() => $"Drop MicrosoftOffice365 data failed. Content: {message.Content}. Exception details: {ex}");
					}
				}
			}
			finally
			{
				Application.UseWaitCursor = false;
			}
		}

		void DoHandle(ZDataObject dataObject, Form targetForm)
		{
			void DoDragDrop()
			{
				try
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Dropping file(s) on the form [{targetForm.Text}]");
					typeof(Control).GetMethod("OnDragDrop", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(targetForm, new object[] { new DragEventArgs(dataObject, 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy) });
					TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Dropped file(s) on the form");
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Error in dragging over: {ex}");
					ExceptionDispatchInfo.Capture(ex).Throw();
				}
			}

			targetForm?.InvokeSafe(DoDragDrop);
		}

		delegate void MicrosoftOffice365ObjectHandlerDelegate(ZDataObject dataObject, Form targetForm);

		protected virtual Form GetActiveForm(string message)
		{
			return ApplicationDispatcher.Current?
				.Invoke(() => DragDropHelper.Instance.HandleRemoteWindowTitleAndHandleMessage(message));
		}
	}
}
