using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Interop.DataObjects;
using CargoWise.Windows.UI;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class DragDropHandler : XmlMessageHandler<DragDropMessage>
	{
		protected override void Handle(IEnterpriseChannel channel, DragDropMessage message)
		{
			try
			{
				if (message is null)
				{
					var errMessage = Res.GetString("47222C8D-06E6-4571-857A-1BF4DA003116", "Data received is NULL, please try again in a few minutes.");
					TrackingInfoLogger.Instance.NewLog(() => errMessage);
					throw new IOException(errMessage);
				}
				else if (message.fileDrop is null)
				{
					var errMessage = Res.GetString("994F90BC-8328-471B-B973-A306D9DF83A9", "File Drop field of the data received is NULL, please try again in a few minutes.");
					TrackingInfoLogger.Instance.NewLog(() => errMessage);
					throw new IOException(errMessage);
				}

				// Check if any file data is null first, then we do not care about the file data being null or not afterwards
				if (message.fileDrop.Any(drop => drop is null))
				{
					TrackingInfoLogger.Instance.NewLog(() => $"File data is missing");
					throw new IOException(Res.GetString("B9C19AFA-2F81-4ED4-A84B-6D649B979BA6", "Data was unsuccessfully transferred - one or more file is missing, please try again in a few minutes."));
				}

				var badFile = message.fileDrop.FirstOrDefault(drop => drop.IsDataCorrupted);
				if (badFile is not null)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"File data is corrupted, name: [{badFile.fileName}], length: [{badFile.fileData.Length}]");
					throw new IOException(Res.GetString("10b5ee43-1bac-4220-8c8a-db02eb936ca3", "Data was unsuccessfully transferred - one or more file is corrupted, please try again in a few minutes."));
				}

				ApplicationDispatcher.Current?.BeginInvoke(new DragDropHandlerDelegate(DoHandle), message);
			}
			finally
			{
				Application.UseWaitCursor = false;
			}
		}

		void DoHandle(DragDropMessage message)
		{
			var activeForm = GetActiveForm();
			if (activeForm != null)
			{
				void OnDragDrop()
				{
#if DEBUG
					if (activeForm.InvokeRequired)
					{
						activeForm.Dispose();
						throw new Exception("Cannot do drag drop from a different thread");
					}
#endif
					TrackingInfoLogger.Instance.NewLog(() => $@"Creating data object from files:
{GetDropInfo(message)}");
					var dataObject = message.CreateDataObject();
					try
					{
						TrackingInfoLogger.Instance.NewLog(() => $"Dropping file(s) on the form [{activeForm.Text}]");
						typeof(Control).GetMethod("OnDragDrop", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(activeForm, new object[] { new DragEventArgs(dataObject, 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy) });
						TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Dropped file(s) on the form");
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						TrackingInfoLogger.Instance.NewLog(() => $"Error in dropping file(s): {ex}");
						ExceptionDispatchInfo.Capture(ex).Throw();
					}
					finally
					{
						if (dataObject is ZAutoDeleteFileDropDataObject)
						{
							TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Flagging auto-delete file for data object");
							((ZAutoDeleteFileDropDataObject)dataObject).AutoDelete = true;
						}

						TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Disposing data object");
						dataObject.Dispose();
						TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Disposed data object");
					}

					string GetDropInfo(DragDropMessage message)
					{
						var sb = new StringBuilder();
						foreach (var fileDrop in message.fileDrop)
						{
							sb.AppendLine($"File Name: {fileDrop.fileName}, file Length: {fileDrop.fileData.Length}");
						}
						return sb.ToString();
					}
				}

				activeForm.InvokeSafe(OnDragDrop);
			}
		}

		protected override void ReportError(IEnterpriseChannel channel, Stream messageData, Exception ex)
		{
		}

		delegate void DragDropHandlerDelegate(DragDropMessage message);

		Form GetActiveForm()
		{
#if DEBUG
			if (testForm != null)
			{
				return testForm;
			}
#endif
			return StartDropHandler.UseWaitingActiveForm();
		}

#if DEBUG
		public static Form testForm;
#endif
	}
}
