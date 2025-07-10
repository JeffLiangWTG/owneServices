using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Module
{
	public partial class IncidentApprovalController : ZController, IServiceRequestController
	{
		static void SendERequestDocumentSafe(string referenceId, Form requestRaisingForm, bool shouldAttachScreenshot)
		{
			var factory = new BusinessObjectFactory();
			var eRequestDoc = new Xsd.ERequestDocument();
			eRequestDoc.ReferenceId = referenceId;

			var timestamp = ZDateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
			if (requestRaisingForm != null && !requestRaisingForm.IsDisposed && shouldAttachScreenshot)
			{
				requestRaisingForm.InvokeRenderDispatcher(async () => {
					await requestRaisingForm.CargoWiseClientServices.WindowService.ScreenShotAsync(new ScreenShotSetting()
					{
						Action = ScreenShotAction.CaptureClientWindow,
						Callback = (string data) =>
						{
							return requestRaisingForm.InvokeWinzorDispatcherAsync(() =>
							{
								if (!string.IsNullOrWhiteSpace(data))
								{
									var imageData = Convert.FromBase64String(data);
									var screenShot = CreateAttachment(eRequestDoc, timestamp);
									screenShot.Data = imageData;
								}

								createAndSaveSystemReportWithExceptionHandling(factory, eRequestDoc, timestamp);
							});
						}
					});
				});
			}
			else
			{
				createAndSaveSystemReportWithExceptionHandling(factory, eRequestDoc, timestamp);
			}
		}

		static void createAndSaveSystemReportWithExceptionHandling(BusinessObjectFactory factory, Xsd.ERequestDocument eRequestDoc, string timestamp)
		{
			try
			{
				createAndSaveSystemReport(factory, eRequestDoc, timestamp);
			}
			catch (Exception ex)
			{
				ExceptionReporter.Instance.ReportDeveloperException("dec5abce-d955-4dd4-b94d-b8b1be1f3c87", SendRequestDeveloperExceptionMessage, ex);
			}
		}
	}
}
