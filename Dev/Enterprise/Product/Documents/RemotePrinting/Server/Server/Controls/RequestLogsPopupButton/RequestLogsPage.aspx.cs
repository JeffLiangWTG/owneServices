using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.RemotePrinting.Server.Business;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.RemotePrinting.Server.Controls
{
	public partial class RequestLogsPage : ZIFramePage
	{
		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			NotificationFlags.DisplayErrors = true;
		}

		#region ClientLogRetrievalInfo data source

		protected override BusinessObject GetNewDataSource()
		{
			return new ClientLogRetrievalInfo
			{
				FromDate = ZDateTime.Today.AddDays(-7),
				ToDate = ZDateTime.Today,
				Logs = true,
				ServiceLogs = true,
			};
		}

		protected ClientLogRetrievalInfo ClientLogRetrievalBizo => (ClientLogRetrievalInfo)DataSource;

		#endregion

		#region OK

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override void HandleOkButtonClick()
		{
			var clientLogRetrievalInfo = ClientLogRetrievalBizo;
			clientLogRetrievalInfo.RunPreSaveValidation();
			if (!clientLogRetrievalInfo.HasErrors)
			{
				if (!GetClientInfoFromParameters(out var clientId, out var serverName, out var webServerAddress, out var hostName))
				{
					ReloadPage("SignalR Client details are not defined, trying to reload this page, if failed, please manually reload this page.");
					return;
				}

				base.HandleOkButtonClick();

				var requestResult = RetrieveLogs(clientLogRetrievalInfo, clientId, webServerAddress, hostName);
				if (string.IsNullOrEmpty(requestResult))
				{
					ScriptNotificationMessage("Request to retrieve logs was sent to WebPrint Client.");
				}
				else
				{
					ScriptNotificationMessage("Request to retrieve logs from WebPrint Client was not successful: " + requestResult);
				}
			}
			else
			{
				NotificationFlags.DisplayErrors = true;
			}
		}

		protected override string[] OKFunctionArguments
		{
			get { return null; }
		}

		string RetrieveLogs(ClientLogRetrievalInfo clientLogRetrievalInfo, string clientId, string webServerAddress, string hostName)
		{
			var types = Types.LogTypes.None;
			if (clientLogRetrievalInfo.Logs)
			{
				types |= Types.LogTypes.Log;
			}
			if (clientLogRetrievalInfo.InstallLogs)
			{
				types |= Types.LogTypes.InstallLog;
			}
			if (clientLogRetrievalInfo.ServiceLogs)
			{
				types |= Types.LogTypes.ServiceLog;
			}
			if (clientLogRetrievalInfo.WindowsEvents)
			{
				types |= Types.LogTypes.WindowsEvent;
			}

			var requestDetails = new ClientLogRequestDetails
			{
				EmailAddress = clientLogRetrievalInfo.EmailAddress,
				FromDate = clientLogRetrievalInfo.FromDate.ToDateTime().Date,
				ToDate = clientLogRetrievalInfo.ToDate.ToDateTime().Date.AddDays(1),
				LogTypes = types,
			};

			string requestResult;
			if (RemoteHub.Controller.HasRegisteredClient(clientId))
			{
				requestResult = RemoteHub.Controller.RequestClientLogs(clientId, requestDetails);
			}
			else
			{
				requestResult = new SupportWebClient().RequestClientLogs(webServerAddress, hostName, clientId, requestDetails);
			}

			return requestResult;
		}

		bool GetClientInfoFromParameters(out string clientId, out string serverName, out string webServerAddress, out string hostName)
		{
			clientId = GetStringFromParameter(RequestLogsPopupButton.ClientIdParam);
			serverName = GetStringFromParameter(RequestLogsPopupButton.ServerNameParam);
			webServerAddress = GetStringFromParameter(RequestLogsPopupButton.WebServerAddressParam);
			hostName = GetStringFromParameter(RequestLogsPopupButton.WebServerHostNameParam);

			return !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(serverName);
		}

		#endregion

		#region Cancel

		protected override void HandleCancelButtonClick()
		{
			NotificationFlags.DisplayErrors = false;
			base.HandleCancelButtonClick();
		}

		protected override string[] CancelFunctionArguments
		{
			get { return null; }
		}

		#endregion

		#region ReloadPage

		const string ReloadParentPageScriptBlockKey = "RequestLogsPage_ReloadParentPage";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void ReloadPage(string errorMessage)
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), ReloadParentPageScriptBlockKey))
			{
				var reloadScript = (this.BrowserType == BrowserType.IE)
					? // ie script
					@"<SCRIPT>
					window.attachEvent('onload', _Reload_Page);

					function _Reload_Page()
					{{
						alert('{0}');
						if (typeof parent[""{1}""] === ""function"")
						{{
							parent[""{1}""]();
						}}
					}}
					</SCRIPT>"
					: // mozilla script
					@"<SCRIPT event='onload'>

					_Reload_Page();

					function _Reload_Page()
					{{
						alert('{0}');
						if (typeof parent[""{1}""] === ""function"")
						{{
							parent[""{1}""]();
						}}
					}}
					</SCRIPT>";
				reloadScript = string.Format(reloadScript, errorMessage, SupportDiagnostics.ReloadSupportDiagnosticsPageScriptFunctionName);
				ZClientScript.RegisterClientScriptBlock(GetType(), ReloadParentPageScriptBlockKey, reloadScript);
			}
		}

		#endregion
	}
}
