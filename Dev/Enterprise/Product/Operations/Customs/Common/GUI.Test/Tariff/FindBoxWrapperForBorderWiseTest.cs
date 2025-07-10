using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Moq;
using Moq.Protected;
using Websocket.Client;
using Websocket.Client.Models;

namespace Enterprise.Customs.Common.GUI.Testing
{
	class FindBoxWrapperForBorderWiseTest : FindBoxWrapperWithBorderWiseIntegrationTestCase<FindBoxWrapperForBorderWise>
	{
		WebSocketClientMock webSocketClientMock;

		protected override FindBoxWrapperForBorderWise CreateFindBoxPopup(AdditionalDataForBorderWise filterData) => new FindBoxWrapperForBorderWise(filterData);

		public void TestShowModal_WhenUsingBorderWiseWebWithWebsocket_ShouldShowWaitingForResponseDialogAndCloseDialogOnWebsocketMessageReceivedWithTariffData()
		{
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableMultilineTariffClassification = false;
			var data = new AdditionalDataForBorderWise("Flooble Crank", ZDateTime.BrettsBirthday);
			var findBox = CreateFindBox(null);
			try
			{
				ZGuid returnHookPK = default;
				webSocketClientMock = new WebSocketClientMock();

				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var dialog = (WaitingForResponseFromBorderWiseWebMessageBox)form;
					returnHookPK = dialog.FormBizo.PK;
					try
					{
						AssertNull(dialog.FormBizo.SelectedTariff);
						AssertNull(dialog.FormBizo.SelectedStat);
						AssertNullOrEmpty(findBox.Code);
						AssertEquals("&Cancel", dialog.MessageBoxButton1.Text);
						AssertEquals(false, dialog.IsDisposed);

						webSocketClientMock.MessageReceivedSubject.OnNext(ResponseMessage.TextMessage(CreateWebSocketExchangeMessage(BorderWiseWebSocketMessageStatus.MSG, returnHookPK)));
					}
					catch
					{
						((IDisposable)form).Dispose(); // Ensure that if the above fails we don't leave the dialog open forever.
						throw;
					}
					AssertEquals("2901.24.00", dialog.FormBizo.BorderWiseInvoiceLines[0].TariffCode);
					AssertEquals("05", dialog.FormBizo.BorderWiseInvoiceLines[0].StatCode);
					AssertEquals(true, dialog.IsDisposed);
				});

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (var popupWrapper = CreateFindBoxPopup(data))
				{
					AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

					popupWrapper.BorderWiseLauncher.WebSocketClient = webSocketClientMock;
					popupWrapper.BorderWiseLauncher.Launcher.HttpMessageHandler = CreateHttpMessageHandlerMock();
					popupWrapper.BorderWiseLauncher.HttpMessageHandler = CreateHttpMessageHandlerMock();
					popupWrapper.ShowModal(findBox, null);

					popupWrapper.BorderWiseLauncher.BorderWiseTariffProcessor.BorderWiseWebSocketClient.WebSocketClientDisposeDelayTask.Wait();

					AssertEquals("Should have used the tariff and stat code passed back from BorderWise Web via websocket exchange message", "2901.24.00 05", findBox.Code);
					AssertNotNull(returnHookPK);
					AssertNotContains("Original URL should not include tariff heading since nothing was selected in the find box originally.", "tariff=", WebUrlLauncher.LastUrlLaunched);
				}
			}
			finally
			{
				(findBox as IDisposable)?.Dispose();
			}
		}

		public void TestShowModal_WhenUsingBorderWiseWebWithWebsocketAndExistingValuePresentInTextBox_ShouldPassThroughExistingTariffDetailsToBorderWise()
		{
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableMultilineTariffClassification = false;
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			var findBox = CreateFindBox("1234.56.78 90");
			try
			{
				ZGuid? returnHookPK = null;
				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var dialog = (WaitingForResponseFromBorderWiseWebMessageBox)form;
					returnHookPK = dialog.FormBizo.PK;
					AssertEquals("&Cancel", dialog.MessageBoxButton1.Text);
					dialog.Dispose();
				});

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (var popupWrapper = CreateFindBoxPopup(data))
				{
					webSocketClientMock = new WebSocketClientMock();
					popupWrapper.BorderWiseLauncher.WebSocketClient = webSocketClientMock;
					popupWrapper.BorderWiseLauncher.Launcher.HttpMessageHandler = CreateHttpMessageHandlerMock();
					popupWrapper.BorderWiseLauncher.HttpMessageHandler = CreateHttpMessageHandlerMock();

					popupWrapper.ShowModal(findBox, null);

					var actualUrl = WebUrlLauncher.LastUrlLaunched;
					var indexOfHash = actualUrl.ToLower().IndexOf("%26hash%3d%");
					if (indexOfHash > 0)
					{
						var nextParameterStart = actualUrl.IndexOf("%26", indexOfHash + 1);
						actualUrl = actualUrl.Substring(0, indexOfHash + 10) + "(hash)" + actualUrl.Substring(nextParameterStart);
					}

					popupWrapper.BorderWiseLauncher.BorderWiseTariffProcessor.BorderWiseWebSocketClient.WebSocketClientDisposeDelayTask.Wait();

					AssertNotNull("Should have called form shown delegate", returnHookPK);
					AssertContains("Should include tariff heading, country, import/export flag, and return hyperlink so we can navigate to the proper tariff entry. Complete URL including hash: " + WebUrlLauncher.LastUrlLaunched, "tariff=1234.56.78&stat=90&impexp=I&c=AU&ret=", actualUrl);
					AssertContains("Last Url launched should include WebSocketClientId.", $"&WebSocketClientId={returnHookPK}", WebUrlLauncher.LastUrlLaunched);
				}
			}
			finally
			{
				(findBox as IDisposable)?.Dispose();
			}
		}

		public void TestShowModal_WhenUsingBorderWiseWebAndWebsocketCloseDialogBeforeSelectingTariffLine_ShouldNotThrowException()
		{
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			var findBox = CreateFindBox("1234.56.78 90");
			try
			{
				ZGuid? returnHookPK = null;
				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var dialog = (WaitingForResponseFromBorderWiseWebMessageBox)form;
					returnHookPK = dialog.FormBizo.PK;
					dialog.Dispose();

					AssertEquals(true, dialog.IsDisposed);
				});

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (var popupWrapper = CreateFindBoxPopup(data))
				{
					webSocketClientMock = new WebSocketClientMock();
					popupWrapper.BorderWiseLauncher.WebSocketClient = webSocketClientMock;
					popupWrapper.BorderWiseLauncher.Launcher.HttpMessageHandler = CreateHttpMessageHandlerMock();
					popupWrapper.BorderWiseLauncher.HttpMessageHandler = CreateHttpMessageHandlerMock();

					AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);
					AssertNoExceptionThrown(() => popupWrapper.ShowModal(findBox, null));
					AssertNotNull("Should have called form shown delegate", returnHookPK);
					AssertEquals("Should not have altered the tariff code already present in the find box since the dialog was closed before the edient hyperlink was executed", "1234.56.78 90", findBox.Code);

					popupWrapper.BorderWiseLauncher.BorderWiseTariffProcessor.BorderWiseWebSocketClient.WebSocketClientDisposeDelayTask.Wait();
				}
			}
			finally
			{
				(findBox as IDisposable)?.Dispose();
			}
		}

		public void TestWebSocketReconnectionHappened_WhenUsingBorderWiseWebAndWebsocket_ShouldSendMessageWithSTAStatus()
		{
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			var findBox = CreateFindBox("1234.56.78 90");
			try
			{
				webSocketClientMock = new WebSocketClientMock();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var dialog = (WaitingForResponseFromBorderWiseWebMessageBox)form;
					webSocketClientMock.ReconnectionSubject.OnNext(ReconnectionInfo.Create(ReconnectionType.Initial));

					try
					{
						var webSocketExchangeMessage = Utilities.DeserializeFromJson<BorderWiseExchangeMessage>(webSocketClientMock.Messages[0]);
						AssertEquals("Websocket on reconnection happened should send message", webSocketClientMock.Messages.Count, 1);
						AssertEquals("Websocket on reconnection happened should send message with STA status", webSocketExchangeMessage.Status, BorderWiseWebSocketMessageStatus.STA);
					}
					finally
					{
						dialog.Dispose(); // Ensure that if the above fails we don't leave the dialog open forever.
					}
				});

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (var popupWrapper = CreateFindBoxPopup(data))
				{
					popupWrapper.BorderWiseLauncher.WebSocketClient = webSocketClientMock;
					popupWrapper.BorderWiseLauncher.Launcher.HttpMessageHandler = CreateHttpMessageHandlerMock();
					popupWrapper.BorderWiseLauncher.HttpMessageHandler = CreateHttpMessageHandlerMock();
					popupWrapper.ShowModal(findBox, null);

					popupWrapper.BorderWiseLauncher.BorderWiseTariffProcessor.BorderWiseWebSocketClient.WebSocketClientDisposeDelayTask.Wait();
				}
			}
			finally
			{
				(findBox as IDisposable)?.Dispose();
			}
		}

		public void TestWebSocketMessageReceived_WhenUsingBorderWiseWebAndWebsocketReceivedMessageWithTariffData_ShouldSendMessageWithFINStatusAndDisposeWebSocketClient()
		{
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			var findBox = CreateFindBox("1234.56.78 90");
			try
			{
				webSocketClientMock = new WebSocketClientMock();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var dialog = (WaitingForResponseFromBorderWiseWebMessageBox)form;
					webSocketClientMock.MessageReceivedSubject.OnNext(ResponseMessage.TextMessage(CreateWebSocketExchangeMessage(BorderWiseWebSocketMessageStatus.MSG, dialog.FormBizo.PK)));

					try
					{
						var webSocketExchangeMessage = Utilities.DeserializeFromJson<BorderWiseExchangeMessage>(webSocketClientMock.Messages[0]);
						AssertEquals("Websocket on receiving valid message", webSocketClientMock.Messages.Count, 1);
						AssertEquals("Websocket on receiving valid message should send FIN message", webSocketExchangeMessage.Status, BorderWiseWebSocketMessageStatus.FIN);
					}
					finally
					{
						dialog.Dispose();
					}
				});

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (var popupWrapper = CreateFindBoxPopup(data))
				{
					popupWrapper.BorderWiseLauncher.WebSocketClient = webSocketClientMock;
					popupWrapper.BorderWiseLauncher.Launcher.HttpMessageHandler = CreateHttpMessageHandlerMock();
					popupWrapper.BorderWiseLauncher.HttpMessageHandler = CreateHttpMessageHandlerMock();
					popupWrapper.ShowModal(findBox, null);

					popupWrapper.BorderWiseLauncher.BorderWiseTariffProcessor.BorderWiseWebSocketClient.WebSocketClientDisposeDelayTask.Wait();

					AssertEquals("Websocket on receiving valid message should dispose websocket client", popupWrapper.BorderWiseLauncher.BorderWiseTariffProcessor.BorderWiseWebSocketClient.WebSocketClient, null);
				}
			}
			finally
			{
				(findBox as IDisposable)?.Dispose();
			}
		}

		public void TestWebSocketMessageReceived_WhenUsingBorderWiseWebAndWebsocketReceivedInvalidMessage_ShouldSendMessageWithERRStatusAndDisposeWebSocketClient()
		{
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			var findBox = CreateFindBox("1234.56.78 90");
			try
			{
				webSocketClientMock = new WebSocketClientMock();
				ZFormModaliser.ShowDialogsInTest = true;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (var popupWrapper = CreateFindBoxPopup(data))
				{
					ZFormModaliser.SetDelegateToCallOnFormShown(form =>
					{
						var dialog = (WaitingForResponseFromBorderWiseWebMessageBox)form;
						webSocketClientMock.MessageReceivedSubject.OnNext(ResponseMessage.TextMessage("{\"ClientId\":\"Test\"}"));//passing invalid data

						popupWrapper.BorderWiseLauncher.BorderWiseTariffProcessor.BorderWiseWebSocketClient.WebSocketClientDisposeDelayTask.Wait();

						try
						{
							var webSocketExchangeMessage = Utilities.DeserializeFromJson<BorderWiseExchangeMessage>(webSocketClientMock.Messages[0]);
							AssertEquals("Websocket on receiving valid message", webSocketClientMock.Messages.Count, 1);
							AssertEquals("Websocket on receiving valid message should send ERR message", webSocketExchangeMessage.Status, BorderWiseWebSocketMessageStatus.ERR);
						}
						finally
						{
							dialog.Dispose();
						}
					});

					popupWrapper.BorderWiseLauncher.WebSocketClient = webSocketClientMock;
					popupWrapper.BorderWiseLauncher.Launcher.HttpMessageHandler = CreateHttpMessageHandlerMock();
					popupWrapper.BorderWiseLauncher.HttpMessageHandler = CreateHttpMessageHandlerMock();
					popupWrapper.ShowModal(findBox, null);

					AssertEquals("Websocket on receiving valid message should dispose websocket client", popupWrapper.BorderWiseLauncher.BorderWiseTariffProcessor.BorderWiseWebSocketClient.WebSocketClient, null);
				}
			}
			finally
			{
				(findBox as IDisposable)?.Dispose();
			}
		}

		public void TestMessageBoxCloseEvent_WhenUsingBorderWiseWebAndWebsocket_ShouldSendMessageWithCANStatusAndDisposeWebSocketClient()
		{
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			var findBox = CreateFindBox("1234.56.78 90");
			try
			{
				webSocketClientMock = new WebSocketClientMock();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var dialog = (WaitingForResponseFromBorderWiseWebMessageBox)form;
					dialog.Dispose();
				});
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				using (var popupWrapper = CreateFindBoxPopup(data))
				{
					popupWrapper.BorderWiseLauncher.WebSocketClient = webSocketClientMock;
					popupWrapper.BorderWiseLauncher.Launcher.HttpMessageHandler = CreateHttpMessageHandlerMock();
					popupWrapper.BorderWiseLauncher.HttpMessageHandler = CreateHttpMessageHandlerMock();
					popupWrapper.ShowModal(findBox, null);

					popupWrapper.BorderWiseLauncher.BorderWiseTariffProcessor.BorderWiseWebSocketClient.WebSocketClientDisposeDelayTask.Wait();

					var webSocketExchangeMessage = Utilities.DeserializeFromJson<BorderWiseExchangeMessage>(webSocketClientMock.Messages[0]);
					AssertEquals("Websocket on receiving valid message", webSocketClientMock.Messages.Count, 1);
					AssertEquals("Websocket on receiving valid message should send CAN message", webSocketExchangeMessage.Status, BorderWiseWebSocketMessageStatus.CAN);
				}
			}
			finally
			{
				(findBox as IDisposable)?.Dispose();
			}
		}

		public void TestShowModal_WhenUsingBorderWiseWebWithWebsocketWithBatchTariffMode_ShouldPassVersionDetailToBorderWise()
		{
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			webSocketClientMock = new WebSocketClientMock();
			var declaration = CreateInvoiceLines();
			ZFormModaliser.ShowDialogsInTest = true;

			using (var zForm = new ZForm(declaration))
			using (var zGrid = new ZGrid())
			using (var zGridFindBox = new ZGridFindBox())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (var popupWrapper = CreateFindBoxPopup(data))
			{
				((IFindBox)zGridFindBox).Code = "1234.56.78 90";
				zGridFindBox.Parent = zGrid;
				popupWrapper.BorderWiseLauncher.WebSocketClient = webSocketClientMock;
				popupWrapper.BorderWiseLauncher.Launcher.HttpMessageHandler = CreateHttpMessageHandlerMock();
				popupWrapper.BorderWiseLauncher.HttpMessageHandler = CreateHttpMessageHandlerMock();

				popupWrapper.ShowModal(zGridFindBox, zForm);

				var webSocketClientId = popupWrapper.BorderWiseLauncher.BorderWiseTariffProcessor.BorderWiseWebSocketClient.WebSocketClientId;
				var actualUrl = WebUrlLauncher.LastUrlLaunched;

				AssertContains("Should include tariff heading, country, import/export flag, and return hyperlink so we can navigate to the proper tariff entry. Complete URL including hash: " + WebUrlLauncher.LastUrlLaunched, "tariff=1234.56.78&stat=90&impexp=I&c=AU", actualUrl);
				AssertContains("Last Url launched should include WebSocketClientId and version.", $"&version=2&mode=Batch&WebSocketClientId={webSocketClientId}", WebUrlLauncher.LastUrlLaunched);
			}
		}

		public void TestBatchTariffMode_WhenSingleTariffDataWithoutInvoicelineReceivedUsingBorderWiseWebAndWebsocket_ShouldAssignTariffDataToFindBox()
		{
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			webSocketClientMock = new WebSocketClientMock();
			var declaration = CreateInvoiceLines();
			ZFormModaliser.ShowDialogsInTest = true;

			using (var zForm = new ZForm(declaration))
			using (var zGrid = new ZGrid())
			using (var zGridFindBox = new ZGridFindBox())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (var popupWrapper = CreateFindBoxPopup(data))
			{
				zGridFindBox.Parent = zGrid;
				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var dialog = (WaitingForResponseFromBorderWiseMessageBox)form;
					try
					{
						AssertNullOrEmpty(((IFindBox)zGridFindBox).Code);
						AssertEquals(false, dialog.IsDisposed);
						dialog.Dispose();
					}
					catch
					{
						((IDisposable)form).Dispose();
						throw;
					}
					finally
					{
						dialog.Dispose();
					}
					AssertEquals(true, dialog.IsDisposed);
				});

				popupWrapper.BorderWiseLauncher.WebSocketClient = webSocketClientMock;
				popupWrapper.BorderWiseLauncher.Launcher.HttpMessageHandler = CreateHttpMessageHandlerMock();
				popupWrapper.BorderWiseLauncher.HttpMessageHandler = CreateHttpMessageHandlerMock();
				popupWrapper.ShowModal(zGridFindBox, zForm);

				var borderWiseInvoiceLine1 = new BorderWiseInvoiceLine("2901.24.00", "05", Guid.NewGuid(), 0, Guid.NewGuid(), "test1");
				var borderWiseTariffExchangeModelV2 = new BorderWiseTariffExchangeModelV2()
				{
					JobPk = declaration.PK.ToGuid(),
					JobNumber = declaration.JobNumber
				};
				borderWiseTariffExchangeModelV2.BorderWiseInvoiceLines.Add(borderWiseInvoiceLine1);

				var webSocketExchangeMessage = CreateWebSocketExchangeMessage(BorderWiseWebSocketMessageStatus.MSG, ZGuid.NewZGuid(), borderWiseTariffExchangeModelV2: borderWiseTariffExchangeModelV2, mode: TariffClassificationMode.Batch);

				webSocketClientMock.MessageReceivedSubject.OnNext(ResponseMessage.TextMessage(webSocketExchangeMessage));
				Thread.Sleep(2000);

				AssertEquals("2901.24.00 05", ((IFindBox)zGridFindBox).Code);
			}
		}

		public void TestBatchTariffMode_WhenMultipleTariffDataReceivedUsingBorderWiseWebAndWebsocket_ShouldUpdateInvoicelinesTariffDetails()
		{
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			webSocketClientMock = new WebSocketClientMock();
			var declaration = CreateInvoiceLines();
			var declaration1 = new BusinessObjectFactory().Load<BaseJobDeclaration>(declaration.PK);

			ZFormModaliser.ShowDialogsInTest = true;

			using (var zForm = new ZForm(declaration))
			using (var zGrid = new ZGrid())
			using (var zGridFindBox = new ZGridFindBox())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (var popupWrapper = CreateFindBoxPopup(data))
			{
				zGridFindBox.Parent = zGrid;
				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var dialog = (WaitingForResponseFromBorderWiseMessageBox)form;
					try
					{
						AssertEquals(false, dialog.IsDisposed);
						dialog.Dispose();
					}
					catch
					{
						((IDisposable)form).Dispose();
						throw;
					}

					AssertEquals(true, dialog.IsDisposed);
				});

				popupWrapper.BorderWiseLauncher.WebSocketClient = webSocketClientMock;
				popupWrapper.BorderWiseLauncher.Launcher.HttpMessageHandler = CreateHttpMessageHandlerMock();
				popupWrapper.BorderWiseLauncher.HttpMessageHandler = CreateHttpMessageHandlerMock();
				popupWrapper.ShowModal(zGridFindBox, zForm);

				webSocketClientMock.MessageReceivedSubject.OnNext(ResponseMessage.TextMessage(CreateWebSocketExchangeMessage(BorderWiseWebSocketMessageStatus.MSG, ZGuid.NewZGuid(), baseJobDeclaration: declaration, mode: TariffClassificationMode.Batch)));

				Thread.Sleep(2000);

				AssertEquals("2901.24.00 05", declaration.InvoiceLines[0].JI_Tariff);
				AssertEquals("test1", declaration.InvoiceLines[0].JI_Description);
				AssertEquals("2901.24.00 06", declaration.InvoiceLines[1].JI_Tariff);
				AssertEquals("test2", declaration.InvoiceLines[1].JI_Description);
			}
		}

		public void TestBatchTariffMode_WhenShowModalIsCalledMultipleTime_ShouldUseSameWebsocketClientAndSubscribeAndReconnectOnlyOnce()
		{
			var data = new AdditionalDataForBorderWise("I", ZDateTime.BrettsBirthday);
			webSocketClientMock = new WebSocketClientMock();
			var declaration = CreateInvoiceLines();
			var declaration1 = new BusinessObjectFactory().Load<BaseJobDeclaration>(declaration.PK);

			ZFormModaliser.ShowDialogsInTest = true;

			using (var zForm = new ZForm(declaration))
			using (var zGrid = new ZGrid())
			using (var zGridFindBox = new ZGridFindBox())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (var popupWrapper = CreateFindBoxPopup(data))
			{
				zGridFindBox.Parent = zGrid;
				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var dialog = (WaitingForResponseFromBorderWiseMessageBox)form;
					try
					{
						AssertEquals(false, dialog.IsDisposed);
						dialog.Dispose();
					}
					catch
					{
						((IDisposable)form).Dispose();
						throw;
					}

					AssertEquals(true, dialog.IsDisposed);
				});

				popupWrapper.BorderWiseLauncher.WebSocketClient = webSocketClientMock;
				popupWrapper.BorderWiseLauncher.Launcher.HttpMessageHandler = CreateHttpMessageHandlerMock();
				popupWrapper.BorderWiseLauncher.HttpMessageHandler = CreateHttpMessageHandlerMock();
				popupWrapper.ShowModal(zGridFindBox, zForm);
				popupWrapper.ShowModal(zGridFindBox, zForm);

				webSocketClientMock.ReconnectionSubject.OnNext(ReconnectionInfo.Create(ReconnectionType.Initial));

				Thread.Sleep(2000);

				AssertEquals(1, webSocketClientMock.ReconnectionHappenedCount);
				AssertEquals(1, webSocketClientMock.StartedCount);
			}
		}

		HttpMessageHandler CreateHttpMessageHandlerMock()
		{
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			var autoLoginResponse = Utilities.SerializeToJson(new RedirectUrlResponse()
			{
				Url = "https://app.borderwise.com",
				TokenExpiry = DateTime.UtcNow.AddHours(12)
			});
			var useBatchModeResponse = Utilities.SerializeToJson(new BatchModeSettings() { UseBatchMode = true, ExpiryDateTimeUtc = DateTime.UtcNow.AddHours(12) });

			httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
			"SendAsync",
			ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.Contains($"/api/v1/auth/autoLogin/url?securedQueryString=")),
			ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(autoLoginResponse)
			});

			httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
			"SendAsync",
			ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.Contains($"/api/settings/use-batchmode?countryCode=")),
			ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(useBatchModeResponse)
			});

			return httpMessageHandlerMock.Object;
		}

		BaseJobDeclaration CreateInvoiceLines()
		{
			var factory = new BusinessObjectFactory();
			var declaration = factory.New<BaseJobDeclaration>();

			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1234.56.78 01";
			invoiceLine1.JI_Description = "test1";
			invoiceLine1.JI_Calc_Invoice = "123";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1234.56.78 02";
			invoiceLine2.JI_Description = "test2";
			invoiceLine1.JI_Calc_Invoice = "123";

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1234.56.78 03";
			invoiceLine3.JI_Description = "test3";
			invoiceLine1.JI_Calc_Invoice = "789";

			factory.Save();

			return declaration;
		}

		string CreateWebSocketExchangeMessage(
			BorderWiseWebSocketMessageStatus borderWiseWebSocketMessageStatus,
			ZGuid? clientId,
			string borderWiseWebSocketMessageType = "",
			BorderWiseTariffExchangeModelV2 borderWiseTariffExchangeModelV2 = null,
			BaseJobDeclaration baseJobDeclaration = null,
			string mode = "Single")
		{
			string data;

			if (borderWiseTariffExchangeModelV2 == null)
			{
				var borderWiseInvoiceLine1 = new BorderWiseInvoiceLine("2901.24.00", "05", baseJobDeclaration?.FilteredInvoiceLines[0].PK.ToGuid(), 1, baseJobDeclaration?.Invoices[0].PK.ToGuid(), "123", "test1");
				var borderWiseInvoiceLine2 = baseJobDeclaration?.FilteredInvoiceLines.Count > 2
					? new BorderWiseInvoiceLine("2901.24.00", "06", baseJobDeclaration?.FilteredInvoiceLines[1].PK.ToGuid(), 2, baseJobDeclaration?.Invoices[0].PK.ToGuid(), "456", "test2")
					: new BorderWiseInvoiceLine("2901.24.00", "06", null, 2, baseJobDeclaration?.Invoices[0].PK.ToGuid(), "456", "test2");

				borderWiseTariffExchangeModelV2 = new BorderWiseTariffExchangeModelV2();
				borderWiseTariffExchangeModelV2.BorderWiseInvoiceLines.Add(borderWiseInvoiceLine1);
				borderWiseTariffExchangeModelV2.BorderWiseInvoiceLines.Add(borderWiseInvoiceLine2);

				if (baseJobDeclaration != null)
				{
					borderWiseTariffExchangeModelV2.JobPk = baseJobDeclaration.PK.ToGuid();
				}

				data = Utilities.SerializeToJson(borderWiseTariffExchangeModelV2);
			}
			else
			{
				data = Utilities.SerializeToJson(borderWiseTariffExchangeModelV2);
			}

			var borderWiseExchangeMessage = new BorderWiseExchangeMessage()
			{
				ClientId = clientId.Value.ToGuid(),
				Status = borderWiseWebSocketMessageStatus,
				SystemNameRecipient = "BorderWiseWeb",
				SystemNameSender = "CargoWiseOne",
				Data = data,
				Type = string.IsNullOrEmpty(borderWiseWebSocketMessageType) ? nameof(BorderWiseTariffExchangeModelV2) : borderWiseWebSocketMessageType,
				Mode = mode
			};
			return Utilities.SerializeToJson(borderWiseExchangeMessage);
		}

		void SetupCurrentUserEmail()
		{
			var factory = new BusinessObjectFactory();
			GlbStaff.GetCurrentUser(factory).GS_EmailAddress = "test@Enterprises.com";
			factory.Save();
		}

		readonly IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableWebSocketClient = true;
			ZFormModaliser.ShowDialogsInTest = true;
			SetupCurrentUserEmail();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			webSocketClientMock?.Dispose();
			webSocketClientMock = null;

			base.TearDown();
			OpenedFormCache.GetInstance().CloseAllCachedForms();
			WebUrlLauncher.ClearLastUrlLaunched();
		}
	}
}

