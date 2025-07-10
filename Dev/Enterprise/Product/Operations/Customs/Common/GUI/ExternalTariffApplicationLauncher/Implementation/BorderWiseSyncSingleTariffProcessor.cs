using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Common.GUI.ExternalTariffApplicationLauncher;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Websocket.Client;

namespace Enterprise.Customs.Common.GUI
{
	internal class BorderWiseSyncSingleTariffProcessor : IBorderWiseTariffProcessor
	{
		bool isMessageReceived;
		ZGuid webSocketClientId;

		public IWebsocketClient WebSocketClient { get; set; }

		public IBorderWiseWebSocketClient BorderWiseWebSocketClient { get; set; }

		public BorderWiseInvoiceLine Process(
			IExternalTariffApplicationLauncher borderWiseWebLauncher,
			BorderWiseFilters filters,
			IFindBox findBox = null,
			Form parentForm = null)
		{
			using (var messageBox = new WaitingForResponseFromBorderWiseWebMessageBox(
				BorderWiseWebTariffProcessorDescriptions.CancelButtonLabel,
				BorderWiseWebTariffProcessorDescriptions.WaitingForBorderWiseResponse,
				BorderWiseWebTariffProcessorDescriptions.PendingBorderWiseResponse))
			{
				if (ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableWebSocketClient)
				{
					var countryCode = string.IsNullOrEmpty(filters?.CountryCodeOverride) ? (string)GlbCompany.CurrentCompany?.GC_RN_NKCountryCode : filters.CountryCodeOverride;

					webSocketClientId = messageBox.FormBizo.PK;

					BorderWiseWebSocketClient = new BorderWiseWebSocketClient(webSocketClientId, Utilities.GetOrgCode(), countryCode, Utilities.GetCargoWiseClientId(), Utilities.GetDatabaseNumber());

					if (WebSocketClient != null)
					{
						BorderWiseWebSocketClient.WebSocketClient = WebSocketClient;
					}

					BorderWiseWebSocketClient.WebSocketDataReceived -= HandleWebSocketData;
					BorderWiseWebSocketClient.WebSocketDataReceived += HandleWebSocketData;
					BorderWiseWebSocketClient.EnsureWebSocketConnection();

					messageBox.OnClosedEvent += delegate
					{
						BorderWiseWebSocketClient.SendMessageAndDisposeWebSocketClient(BorderWiseWebSocketMessageStatus.CAN, null, true, TariffClassificationMode.Single, !isMessageReceived);
					};

					borderWiseWebLauncher.LaunchExternalApplication(filters, messageBox.FormBizo.PK, webSocketClientId);
				}
				else
				{
					borderWiseWebLauncher.LaunchExternalApplication(filters, messageBox.FormBizo.PK);
				}

				ZFormModaliser.ShowDialogWithoutDispose(messageBox);

				if (messageBox.FormBizo?.BorderWiseInvoiceLines?.Count > 0 && !string.IsNullOrEmpty(messageBox.FormBizo?.BorderWiseInvoiceLines[0].TariffCode))
				{
					if (findBox != null)
					{
						findBox.Code = filters.AdditionalData.FormatBorderWiseInput($"{messageBox.FormBizo?.BorderWiseInvoiceLines[0].TariffCode} {messageBox.FormBizo?.BorderWiseInvoiceLines[0].StatCode}");
					}

					return messageBox.FormBizo.BorderWiseInvoiceLines[0];
				}
			}

			return null;
		}

		public void DisposeWebSocketClient()
		{
			BorderWiseWebSocketClient.DisposeWebSocketClient();
		}

		public void HandleWebSocketData(object sender, WebSocketDataReceivedEventArgs e)
		{
			isMessageReceived = true;

			var form = (WaitingForResponseFromBorderWiseWebMessageBox)OpenedFormCache.GetInstance()
					   .GetForm(webSocketClientId.ToGuid(), ControllerIDs.BorderWiseWebReturnHook.ToString());

			if (form == null)
			{
				DisposeWebSocketClient();
				return;
			}

			var tariffSelectionResults = e.BorderWiseInvoiceLines;

			if (tariffSelectionResults?.Count > 0)
			{
				form.FormBizo.BorderWiseInvoiceLines = tariffSelectionResults;

				var tariffSelectionResult = tariffSelectionResults[0];

				form.FormBizo.SelectedTariff = tariffSelectionResult.TariffCode;

				if (!string.IsNullOrEmpty(tariffSelectionResult.StatCode))
				{
					form.FormBizo.SelectedStat = tariffSelectionResult.StatCode.Trim();
				}

				BorderWiseWebSocketClient.SendMessage(BorderWiseWebSocketMessageStatus.FIN, null, TariffClassificationMode.Single);
			}

			form.Activate();
			form.Close();
			form.Dispose();
		}
	}
}
