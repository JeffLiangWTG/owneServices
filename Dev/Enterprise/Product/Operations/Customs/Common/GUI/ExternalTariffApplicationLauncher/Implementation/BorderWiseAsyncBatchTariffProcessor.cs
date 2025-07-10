using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.GUI.ExternalTariffApplicationLauncher;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Websocket.Client;

namespace Enterprise.Customs.Common.GUI
{
	internal class BorderWiseAsyncBatchTariffProcessor : IBorderWiseTariffProcessor
	{
		[SuppressMessage("CargoWiseOne", "CW1021")]
		static readonly ConcurrentDictionary<string, SemaphoreSlim> webSocketClientLock = new();

		Form parentForm;
		IFindBox findBox;
		BorderWiseFilters filters;
		ICommonInvoiceDataProvider commonInvoiceDataProvider;
		BaseJobComInvoiceLine clickedRowInvoiceLine;
		WaitingForResponseFromBorderWiseMessageBox messageBox;
		SynchronizationContext synchronizationContext = SynchronizationContext.Current;
		IExternalTariffApplicationLauncher borderWiseWebLauncher;

		const string BWWebSocketClient = "BWWebSocketClient";

		public IBorderWiseWebSocketClient BorderWiseWebSocketClient { get; set; }
		public IWebsocketClient WebSocketClient { get; set; }

		public bool Process(Form parentForm, ICommonInvoiceDataProvider commonInvoiceDataProvider, IExternalTariffApplicationLauncher borderWiseWebLauncher)
		{
			synchronizationContext = SynchronizationContext.Current;
			this.commonInvoiceDataProvider = commonInvoiceDataProvider;
			this.parentForm = parentForm;
			this.borderWiseWebLauncher = borderWiseWebLauncher;

			SetupWebSocketClient(CreateTariffExchangeModel());

			return true;
		}

		public bool Process(IFindBox findBox, Form parentForm, IExternalTariffApplicationLauncher borderWiseWebLauncher, BorderWiseFilters filters)
		{
			var result = TryGetInvoiceDataProvider(findBox, parentForm);

			if (result.InvoiceDataProvider == null || !result.IsBatchModeAllowed)
			{
				return false;
			}

			this.commonInvoiceDataProvider = result.InvoiceDataProvider;

			var jobDetails = GetInvoiceDetails();

			this.parentForm = parentForm;

			if (string.IsNullOrEmpty(jobDetails.JobNumber) || jobDetails.JobPk == Guid.Empty)
			{
				ShowMessageBox(
					BorderWiseWebTariffProcessorDescriptions.InformationLabel,
					BorderWiseWebTariffProcessorDescriptions.SaveJobDeclaration,
					BorderWiseWebTariffProcessorDescriptions.OKButtonLabel);

				return false;
			}

			this.findBox = findBox;
			this.filters = filters;
			this.borderWiseWebLauncher = borderWiseWebLauncher;
			clickedRowInvoiceLine = result.ClickedRowInvoiceLine;

			var tariffExchangeModel = CreateTariffExchangeModel(true);
			SetupWebSocketClient(tariffExchangeModel);

			if (BorderWiseWebSocketClient?.JobStatusInBW == TariffClassificationJobStatus.ProcessingInCW1)
			{
				ShowMessageBox(
					BorderWiseWebTariffProcessorDescriptions.InformationLabel,
					BorderWiseWebTariffProcessorDescriptions.ProcessingInCargoWise,
					BorderWiseWebTariffProcessorDescriptions.OKButtonLabel);
			}
			else
			{
				BorderWiseWebSocketClient.BorderWiseTariffExchangeModelV2 = tariffExchangeModel;

				var isSendMessage = BorderWiseWebSocketClient.SendMessage(
					BorderWiseWebSocketMessageStatus.MSG,
					Utilities.SerializeToJson(tariffExchangeModel),
					TariffClassificationMode.Batch);

				borderWiseWebLauncher.LaunchExternalApplication(
				filters,
				BorderWiseWebSocketClient.WebSocketClientId,
				BorderWiseWebSocketClient.WebSocketClientId,
				tariffExchangeModel.JobPk.ToString());

				if (!isSendMessage)
				{
					ShowMessageBox(
						BorderWiseWebTariffProcessorDescriptions.InformationLabel,
						BorderWiseWebTariffProcessorDescriptions.UnableToConnectToBorderWise,
						BorderWiseWebTariffProcessorDescriptions.OKButtonLabel);
				}
			}

			return true;
		}

		public bool IsBatchModeAllowed(IFindBox findBox, Form parentForm)
		{
			var result = TryGetInvoiceDataProvider(findBox, parentForm);
			return result.InvoiceDataProvider != null && result.IsBatchModeAllowed;
		}

		public void SendMessageAndDisposeConnectionIfNeeded(ICommonInvoiceDataProvider commonInvoiceDataProvider, Dictionary<ZGuid, string> invoicePksAction, bool isSaved)
		{
			if (commonInvoiceDataProvider == null)
			{
				return;
			}

			this.commonInvoiceDataProvider = commonInvoiceDataProvider;
			var tariffExchangeModel = CreateTariffExchangeModel();

			if (tariffExchangeModel.JobPk == ZGuid.Empty || string.IsNullOrEmpty(tariffExchangeModel.JobNumber))
			{
				return;
			}

			var isAttachDetachDelete = (invoicePksAction != null && invoicePksAction.Count > 0);

			if (isAttachDetachDelete)
			{
				foreach (var item in invoicePksAction)
				{
					if (!tariffExchangeModel.InvoicePksAction.ContainsKey(item.Key.ToGuid()))
					{
						tariffExchangeModel.InvoicePksAction.Add(item.Key.ToGuid(), item.Value);
					}
				}
			}

			var hasAttached = tariffExchangeModel.InvoicePksAction.ContainsValue(BorderWiseWebTariffProcessorDescriptions.Attach);

			BorderWiseWebSocketClient = TryGetBorderWiseWebSocketClientFromCache(tariffExchangeModel.JobPk.ToString());

			if (BorderWiseWebSocketClient == null)
			{
				return;
			}

			if (BorderWiseWebSocketClient.IsMessageReceived || isAttachDetachDelete)
			{
				var status = BorderWiseWebSocketMessageStatus.CAN;

				if (isSaved)
				{
					status = BorderWiseWebSocketClient.IsMessageReceived
						? (hasAttached ? BorderWiseWebSocketMessageStatus.MSG : BorderWiseWebSocketMessageStatus.FIN)
						: BorderWiseWebSocketMessageStatus.MSG;
				}

				BorderWiseWebSocketClient.SendMessageAndDisposeWebSocketClient(
					status,
					Utilities.SerializeToJson(tariffExchangeModel),
					!isSaved,
					TariffClassificationMode.Batch);

				BorderWiseWebSocketClient.WebSocketClientDisposeDelayTask?.GetAwaiter().GetResult();
			}
			else if (!isSaved)
			{
				BorderWiseWebSocketClient.DisposeWebSocketClient();
			}
		}

		void HandleWebSocketData(object sender, WebSocketDataReceivedEventArgs eventArgs)
		{
			if (commonInvoiceDataProvider?.Factory.ThreadSentry.IsOwner == true || synchronizationContext == null)
			{
				ProcessWebSocketData(eventArgs);
			}
			else
			{
				synchronizationContext.Post(_ => ProcessWebSocketData(eventArgs), null);
			}
		}

		void ProcessWebSocketData(WebSocketDataReceivedEventArgs e)
		{
			if (commonInvoiceDataProvider == null)
			{
				return;
			}

			var jobDetail = GetInvoiceDetails();

			if (e.JobPk == Guid.Empty || e.JobPk != jobDetail.JobPk || (parentForm == null || parentForm.IsDisposed))
			{
				return;
			}

			var tariffSelectionResults = e.BorderWiseInvoiceLines;

			if (tariffSelectionResults?.Count == 1 && tariffSelectionResults[0].InvoiceLineNumber == 0)
			{
				SetFindBoxCode(findBox, filters, $"{tariffSelectionResults[0].TariffCode} {tariffSelectionResults[0].StatCode}");
			}
			else if (new[] { TariffClassificationJobStatus.New, TariffClassificationJobStatus.Working, TariffClassificationJobStatus.Edited }.Contains(e.JobStatus))
			{
				ShowMessageBoxWithNavigation(
					jobDetail.JobPk.ToString(),
					BorderWiseWebTariffProcessorDescriptions.InformationLabel,
					BorderWiseWebTariffProcessorDescriptions.InvoiceClassificationWarning,
					BorderWiseWebTariffProcessorDescriptions.OKButtonLabel,
					BorderWiseWebTariffProcessorDescriptions.NavigateButtonLabel);
			}
			else if (e.JobStatus == TariffClassificationJobStatus.ProcessingInCW1 && tariffSelectionResults == null)
			{
				ShowMessageBox(
					BorderWiseWebTariffProcessorDescriptions.InformationLabel,
					BorderWiseWebTariffProcessorDescriptions.InvoiceProcessingByOtherUserWarning,
					BorderWiseWebTariffProcessorDescriptions.OKButtonLabel);
			}
			else if (tariffSelectionResults != null)
			{
				UpdateInvoiceLinesAndSendMessage(tariffSelectionResults, e.JobStatus);
			}
		}

		void UpdateInvoiceLinesAndSendMessage(List<BorderWiseInvoiceLine> tariffSelectionResults, string status)
		{
			if (status != TariffClassificationJobStatus.ProcessingInCW1)
			{
				var message = CreateTariffExchangeModel();
				message.JobStatus = TariffClassificationJobStatus.ProcessingInCW1;

				BorderWiseWebSocketClient.SendMessage(BorderWiseWebSocketMessageStatus.MSG, Utilities.SerializeToJson(message), TariffClassificationMode.Batch);
			}

			parentForm?.Activate();

			if (UpdateInvoiceLines(tariffSelectionResults))
			{
				BorderWiseWebSocketClient.JobStatusInBW = TariffClassificationJobStatus.ProcessingInCW1;
				ShowMessageBox(
					BorderWiseWebTariffProcessorDescriptions.NewClassificationsNotification,
					BorderWiseWebTariffProcessorDescriptions.TariffClassificationsReceived,
					BorderWiseWebTariffProcessorDescriptions.OKButtonLabel);
			}
			else
			{
				SendMessageAndDisposeConnectionIfNeeded(this.commonInvoiceDataProvider, null, true);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1024:Bad Concurrent Collection Access")]
		bool SetupWebSocketClient(BorderWiseTariffExchangeModelV2 tariffExchangeModel)
		{
			var jobDeclarationPK = tariffExchangeModel.JobPk.ToString();
			var setupWebSocketClientLock = webSocketClientLock.GetOrAdd(jobDeclarationPK, _ => new SemaphoreSlim(1, 1));
			setupWebSocketClientLock.Wait();

			try
			{
				BorderWiseWebSocketClient = TryGetBorderWiseWebSocketClientFromCache(jobDeclarationPK);

				if (BorderWiseWebSocketClient == null)
				{
					var countryCode = string.IsNullOrEmpty(filters?.CountryCodeOverride) ? Utilities.GetCountryCode() : filters.CountryCodeOverride;

					BorderWiseWebSocketClient = new BorderWiseWebSocketClient(
						ZGuid.NewZGuid(),
						Utilities.GetOrgCode(),
						countryCode,
						Utilities.GetCargoWiseClientId(),
						Utilities.GetDatabaseNumber(),
						tariffExchangeModel);

					BorderWiseWebSocketClient.WebSocketDataReceived -= HandleWebSocketData;
					BorderWiseWebSocketClient.WebSocketDataReceived += HandleWebSocketData;

					if (WebSocketClient != null)
					{
						BorderWiseWebSocketClient.WebSocketClient = WebSocketClient;
					}

					BorderWiseWebSocketClient.EnsureWebSocketConnection();

					commonInvoiceDataProvider.Factory.GetCachedValue($"{BWWebSocketClient}:{jobDeclarationPK}", delegate
						{
							return BorderWiseWebSocketClient;
						});

					return true;
				}
				else
				{
					BorderWiseWebSocketClient.EnsureWebSocketConnection();

					BorderWiseWebSocketClient.BorderWiseTariffExchangeModelV2 = tariffExchangeModel;

					BorderWiseWebSocketClient.ClearWebSocketDataReceivedEvent();
					BorderWiseWebSocketClient.WebSocketDataReceived -= HandleWebSocketData;
					BorderWiseWebSocketClient.WebSocketDataReceived += HandleWebSocketData;

					return false;
				}
			}
			finally
			{
				setupWebSocketClientLock.Release();
				webSocketClientLock.TryRemove(jobDeclarationPK, out _);
			}
		}

		(ICommonInvoiceDataProvider InvoiceDataProvider, BaseJobComInvoiceLine ClickedRowInvoiceLine, bool IsBatchModeAllowed) TryGetInvoiceDataProvider(IFindBox findBox, Form parentForm)
		{
			if (findBox is Control control && parentForm is ZForm zForm && control.Parent is ZGrid zGrid)
			{
				var isBatchModeAllowed = true;
				var invoiceDataProvider = zForm.BusinessEntity as ICommonInvoiceDataProvider ?? zGrid.DataSource as ICommonInvoiceDataProvider;
				var clickedRowInvoiceLine = zGrid.CurrentRowIndex >= 0 ? invoiceDataProvider?.FilteredInvoiceLines?.ToList()[zGrid.CurrentRowIndex] : null;

				if (clickedRowInvoiceLine != null && zGrid.CurrentCell.ColumnNumber >= 0)
				{
					var currentColumnName = zGrid.Columns[zGrid.CurrentCell.ColumnNumber].ColumnName;

					if (currentColumnName != AutoJobComInvoiceLine.Schema.JI_Tariff &&
						currentColumnName != BaseJobComInvoiceLine.Schema.JI_FormattedTariff)
					{
						isBatchModeAllowed = false;
					}
				}

				return (invoiceDataProvider, clickedRowInvoiceLine, isBatchModeAllowed);
			}

			return (null, null, false);
		}

		BorderWiseTariffExchangeModelV2 CreateTariffExchangeModel(bool includeInvoiceLine = false)
		{
			var jobDetails = GetInvoiceDetails();
			var tariffExchangeModel = new BorderWiseTariffExchangeModelV2
			{
				JobNumber = jobDetails.JobNumber,
				JobPk = jobDetails.JobPk.ToGuid(),
				TariffType = jobDetails.MessageType,
				JobType = jobDetails.JobType,
				BranchPk = jobDetails.BranchPk,
			};

			if (!includeInvoiceLine)
			{
				return tariffExchangeModel;
			}

			foreach (var invoiceLine in commonInvoiceDataProvider.FilteredInvoiceLines)
			{
				var borderWiseInvoiceLine = new BorderWiseInvoiceLine(
					invoiceLine.JI_FormattedTariff,
					string.Empty,
					invoiceLine.PK.ToGuid(),
					invoiceLine.JI_LineNo,
					invoiceLine.JI_JZ.ToGuid(),
					invoiceLine.JI_Calc_Invoice,
					invoiceLine.JI_Description,
					invoiceLine.JI_PartNo
				);

				if (clickedRowInvoiceLine != null && clickedRowInvoiceLine.PK.IsValid && clickedRowInvoiceLine.PK == invoiceLine.PK)
				{
					borderWiseInvoiceLine.TariffCode = findBox.Code;
					borderWiseInvoiceLine.IsSelected = true;

					var invoice = commonInvoiceDataProvider.Invoices.FirstOrDefault(x => x.PK == clickedRowInvoiceLine.InvoiceHeader.PK);
					var clickedInvoiceLine = invoice.InvoiceLines.FindByPK(clickedRowInvoiceLine.PK);
					if (clickedInvoiceLine == null)
					{
						invoice.InvoiceLines.Add(invoiceLine);
					}
				}

				tariffExchangeModel.BorderWiseInvoiceLines.Add(borderWiseInvoiceLine);
			}

			return tariffExchangeModel;
		}

		bool UpdateInvoiceLines(List<BorderWiseInvoiceLine> borderWiseInvoiceLines)
		{
			if (borderWiseInvoiceLines?.Count == 1 && borderWiseInvoiceLines[0].InvoiceLineNumber == 0)
			{
				return SetFindBoxCode(findBox, filters, $"{borderWiseInvoiceLines[0].TariffCode} {borderWiseInvoiceLines[0].StatCode}");
			}

			var hasUpdates = false;

			foreach (var tariffSelected in borderWiseInvoiceLines)
			{
				ZGuid.TryParse(tariffSelected.InvoiceLinePK, out ZGuid invoiceLinePK);
				var invoice = commonInvoiceDataProvider.Invoices.FirstOrDefault(x => tariffSelected.InvoiceNumberPk == x.PK);

				if (invoice != null)
				{
					var invoiceLine = invoice.InvoiceLines.FirstOrDefault(x => tariffSelected.InvoiceLinePK == x.PK) as BaseJobComInvoiceLine;

					if (invoiceLine != null)
					{
						var tariff = invoiceLine.FormatTariffForSaving($"{tariffSelected.TariffCode} {tariffSelected.StatCode}".Trim());

						if (clickedRowInvoiceLine != null && clickedRowInvoiceLine.PK == invoiceLine.PK)
						{
							SetFindBoxCode(findBox, filters, tariff);
						}

						if (invoiceLine.JI_Tariff != tariff)
						{
							invoiceLine.JI_Tariff = tariff;
							hasUpdates = true;
						}

						if (invoiceLine.JI_Description != tariffSelected.Description)
						{
							invoiceLine.JI_Description = tariffSelected.Description;
							hasUpdates = true;
						}
					}
				}
			}

			return hasUpdates;
		}

		bool SetFindBoxCode(IFindBox findBox, BorderWiseFilters filters, string tariff)
		{
			if (findBox != null && filters.AdditionalData != null)
			{
				var code = filters.AdditionalData.FormatBorderWiseInput(tariff);
				if (findBox.Code != code)
				{
					findBox.Code = code;
					return true;
				}
			}
			return false;
		}

		void ShowMessageBox(string caption, string message, string okButtonText)
		{
			DisposeMessageBox();

			using (messageBox = new WaitingForResponseFromBorderWiseMessageBox(okButtonText, caption, message))
			{
				ZFormModaliser.ShowDialogWithoutDispose(messageBox, parentForm);
			}
		}

		void ShowMessageBoxWithNavigation(string jobPk, string caption, string message, string okButtonText, string navigateButtonText)
		{
			DisposeMessageBox();

			using (messageBox = new WaitingForResponseFromBorderWiseMessageBox(navigateButtonText, okButtonText, caption, message))
			{
				messageBox.MessageBoxButton1.Click += (sender, e) =>
				{
					if (filters == null)
					{
						filters = new BorderWiseFilters() { CountryCodeOverride = Utilities.GetCountryCode() };
					}

					this.borderWiseWebLauncher.LaunchExternalApplication(
						filters,
						BorderWiseWebSocketClient.WebSocketClientId,
						BorderWiseWebSocketClient.WebSocketClientId,
						jobPk);
				};

				ZFormModaliser.ShowDialogWithoutDispose(messageBox, parentForm);

				if (messageBox != null)
				{
					messageBox.MessageBoxButton1.Click -= null;
				}
			}
		}

		IBorderWiseWebSocketClient TryGetBorderWiseWebSocketClientFromCache(string jobPk)
		{
			if (commonInvoiceDataProvider?.Factory == null || string.IsNullOrWhiteSpace(jobPk))
			{
				return null;
			}

			commonInvoiceDataProvider.Factory.TryGetValueFromCacheOnly($"{BWWebSocketClient}:{jobPk}", out IBorderWiseWebSocketClient borderWiseWebSocketClient);
			return borderWiseWebSocketClient;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Job Details")]
		JobDetails GetInvoiceDetails()
		{
			var firstInvoice = commonInvoiceDataProvider?.Invoices.FirstOrDefault();
			var isCommercialInvoiceLine = commonInvoiceDataProvider != null
								  && commonInvoiceDataProvider.Invoices.Count() == 1
								  && firstInvoice != null
								  && !firstInvoice.IsAttachedToPersistentDeclaration;

			if (isCommercialInvoiceLine)
			{
				return new JobDetails
				{
					JobPk = firstInvoice.PK,
					JobNumber = firstInvoice.JZ_InvoiceNumber,
					MessageType = firstInvoice.JZ_MessageType,
					BranchPk = firstInvoice.Branch.PK.ToGuid(),
					JobType = "Commercial Invoice"
				};
			}
			else
			{
				var jobDeclaration = commonInvoiceDataProvider as BaseJobDeclaration;

				return new JobDetails
				{
					JobPk = jobDeclaration.PK,
					JobNumber = jobDeclaration?.JobNumber,
					MessageType = commonInvoiceDataProvider.JE_MessageType,
					BranchPk = jobDeclaration.Branch.PK.ToGuid(),
					JobType = (parentForm != null && parentForm.Name.Contains("Shipment") ? "Shipment" : "Customs Declaration")
				};
			}
		}

		void DisposeMessageBox()
		{
			messageBox?.Close();
			messageBox?.Dispose();
		}
	}

	class JobDetails
	{
		public ZGuid JobPk { get; set; }
		public string JobNumber { get; set; }
		public string MessageType { get; set; }
		public Guid BranchPk { get; set; }
		public string JobType { get; set; }
	}
}
