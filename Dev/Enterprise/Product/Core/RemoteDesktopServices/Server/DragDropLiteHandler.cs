using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Interop.DataObjects;
using CargoWise.Windows.UI;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class DragDropLiteHandler : XmlMessageHandler<DragDropLiteMessage>
	{
		protected override void Handle(IEnterpriseChannel channel, DragDropLiteMessage message)
		{
			TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Getting active form");
			var targetForm = ApplicationDispatcher.Current?.Invoke(() => GetActiveForm(message.windowTitleAndHwnd));
			if (targetForm != null)
			{
				try
				{
					if (message.fileDrop != null && message.fileDrop.Any(fileName => string.IsNullOrEmpty(fileName)))
					{
						var errMessage = (NoResString)"One or more drop file are not accessible";
						TrackingInfoLogger.Instance.NewLog(() => errMessage);
						throw new InvalidOperationException(errMessage);
					}

					ApplicationDispatcher.Current?.BeginInvoke(new DragDropLiteHandlerDelegate(DoHandle), message, targetForm);
				}
				finally
				{
					Application.UseWaitCursor = false;
				}
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		[SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Exception message check")]
		void DoHandle(DragDropLiteMessage message, Form targetForm)
		{
			void DoDragDrop()
			{
#if DEBUG
				if (targetForm.InvokeRequired)
				{
					targetForm.Dispose();
					throw new Exception("Cannot do drag drop from a different thread");
				}
#endif

				TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Getting drive mapping paths");
				var mappedPathList = new List<string>();
				message.fileDrop.ForEach(fileName =>
				{
					try
					{
						var mappedPath = new MappedClientPath().GetMappedPathOfExistingFile(fileName);
						mappedPathList.Add(mappedPath);
					}
					catch (FileNotFoundException ex)
					{
						var terminalService = ObjectFactory.Get<TerminalService>();
						var supportedClientVersion = terminalService.IsCitrixICA ? ClientCitrixVersion.Version : ClientVersion.Version;
						ErrorReporter.ReportOnce(
							"DragDropLiteHandlerEmptyString",
							$"Unable to get mapped path of [{fileName}] due to exception, " +
							$"IsCitrix: {terminalService.IsCitrixICA}, IsRemoteAppSession: {terminalService.IsRemoteAppSession}, " +
							$"IsWTSSession: {terminalService.IsWTSSession}, ClientSessionProtocolType: {terminalService.GetClientSessionProtocolType(numberOfTries: 1)}, " +
							$"TerminalService.LastWin32Error: {terminalService.LastWin32Error}, SupportedClientVersion: {supportedClientVersion}",
							ex);
					}
				});

				message.fileDrop = mappedPathList;

				TrackingInfoLogger.Instance.NewLog(() => $@"Creating data object from files");
				var dataObject = message.CreateDataObject();
				try
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Dropping file(s) on the form [{targetForm.Text}]");
					typeof(Control).GetMethod("OnDragDrop", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(targetForm, new object[] { new DragEventArgs(dataObject, 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy) });
					TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Dropped file(s) on the form");
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Error in dropping file(s): {ex}");
					ExceptionDispatchInfo.Capture(ex).Throw();
				}
				finally
				{
					var autoDeleteFileDropDataObject = dataObject as ZAutoDeleteFileDropDataObject;
					if (autoDeleteFileDropDataObject != null && message.NeedCleanUpTempFile)
					{
						TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Flagging auto-delete file for data object");
						autoDeleteFileDropDataObject.AutoDelete = true;
					}

					TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Disposing data object");
					dataObject.Dispose();
					TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Disposed data object");
				}
			}

			targetForm?.InvokeSafe(DoDragDrop);
		}

		delegate void DragDropLiteHandlerDelegate(DragDropLiteMessage message, Form targetForm);

		Form GetActiveForm(string message)
		{
#if DEBUG
			if (testForm != null)
			{
				return testForm;
			}
#endif
			return DragDropHelper.Instance.HandleRemoteWindowTitleAndHandleMessage(message);
		}

#if DEBUG
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "For test only")]
		public static Form testForm;
#endif
	}
}
