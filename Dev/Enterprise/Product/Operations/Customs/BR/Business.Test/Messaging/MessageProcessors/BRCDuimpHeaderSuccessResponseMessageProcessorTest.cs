using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	sealed class BRCDuimpHeaderSuccessResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCDuimpHeaderSuccessResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CIH" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "SUC" };

		public void TestMessageFriendlyName()
		{
			var processor = new BRCDuimpHeaderSuccessResponseMessageProcessor(new LoggingInformation());
			AssertEquals("MessageFriendlyName", "DUIMP Header Success Response Message", processor.MessageFriendlyName);
		}

		public void TestProcessResponseMessageOriginal_Accepted()
		{
			var entry = CreateEntryHeader();
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			entry.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Active;

			var (requestMessage, responseMessage) = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Success, requestMessageSubType: EDIMessageSubTypeList.Codes.Original);
			responseMessage.EM_MessageText = JsonMessageResponse;
			Factory.Save();

			var headerMessages = entry.Messages.Where(w => w.EM_MessageType == MessageTypeList.Codes.CIH && w.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);
			var lineMessages = entry.Messages.Where(w => w.EM_MessageType == MessageTypeList.Codes.CIL && w.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions("Successful response for CIH-ORI with numero", () =>
			{
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("MRN Number updated", "20BR00000012345", entry.MovementReferenceNumber);
				AssertEquals("CH_CustomsPostedStatus updated to Accepted", CustomsPostedStatusList.Codes.Accepted, entry.CH_CustomsPostedStatus);
				AssertEquals("No CIH message sent on message processor", 0, headerMessages.Except(new[] { requestMessage }).Count());
				AssertEquals("CIL message sent on message processor", 1, lineMessages.Count());
				AssertEquals("CIL Message[0].EM_MessageSubType should be", EDIMessageSubTypeList.Codes.Addition, lineMessages.ElementAt(0).EM_MessageSubType);
				AssertEquals("CH_Status updated", BRMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
				AssertEquals("CH_CustomsPostedStatus updated", CustomsPostedStatusList.Codes.Accepted, entry.CH_CustomsPostedStatus);
				AssertEquals("CH_AuthorityVersion updated", "1", entry.CH_AuthorityVersion);
			});

			var cilMessage = lineMessages.ElementAt(0);
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { (BREDIMessage)lineMessages.ElementAt(0) });
			var provider = new BRInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();

			AssertContains("Version number of the interchange created was populated", @"""custom.BR.VersionNumber"":""1""", cilMessage.Interchange.EI_InterchangeText);
		}

		public void TestProcessResponseMessageOriginal_Rejected()
		{
			var entry = CreateEntryHeader();
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			entry.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Active;

			var (requestMessage, responseMessage) = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Success, requestMessageSubType: EDIMessageSubTypeList.Codes.Original);
			responseMessage.EM_MessageText = JsonMessageResponse.Replace(@"""numero"": ""20BR00000012345"",", @"""numero"": """",").Replace(@"""versao"": ""1""", @"""versao"": """"");
			Factory.Save();

			var headerMessages = entry.Messages.Where(w => w.EM_MessageType == MessageTypeList.Codes.CIH && w.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);
			var lineMessages = entry.Messages.Where(w => w.EM_MessageType == MessageTypeList.Codes.CIL && w.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();

			CombineAssertions("Successful response for CIH-ORI with empty numero", () =>
			{
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);

				AssertEquals("MRN Number NOT updated", ZString.Empty, entry.MovementReferenceNumber);
				AssertEquals("CH_Status updated", BRMessageStatusList.Codes.Rejected, entry.CH_Status);
				AssertEquals("CH_CustomsPostedStatus not updated", CustomsPostedStatusList.Codes.Active, entry.CH_CustomsPostedStatus);
				AssertEquals("MessageRejected log added", AutoEvents.MessageRejected.Code, entry.Logs.MostRecentLog.SL_SE_NKEvent);
				AssertEquals("CH_AuthorityVersion not updated", "", entry.CH_AuthorityVersion);

				AssertEquals("No CIH message sent on message processor", 0, headerMessages.Except(new[] { requestMessage, requestMessage }).Count());
				AssertEquals("No CIL message sent on message processor", 0, lineMessages.Count());
			});
		}

		public void TestProcessResponseMessageUpdate_Accepted()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CDI;
			entry.MovementReferenceNumberSetter("38BR15856778945");
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			entry.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
			entry.AllEntryLines.AddNew().CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			entry.AllEntryLines.AddNew().CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			entry.AllEntryLines.AddNew().CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Deleted;

			var (requestMessage, responseMessage) = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Success, requestMessageSubType: EDIMessageSubTypeList.Codes.Update);
			responseMessage.EM_MessageText = JsonMessageResponse;
			CreateLineMessageAndSetAsProcessed(entry, EDIMessageSubTypeList.Codes.Addition);
			CreateLineMessageAndSetAsProcessed(entry, EDIMessageSubTypeList.Codes.Update);
			CreateLineMessageAndSetAsProcessed(entry, EDIMessageSubTypeList.Codes.Deletion);
			Factory.Save();

			var headerMessages = entry.Messages.Where(w => w.EM_MessageType == MessageTypeList.Codes.CIL && w.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);
			var lineMessages = entry.Messages.Where(w => w.EM_MessageType == MessageTypeList.Codes.CIL && w.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();

			CombineAssertions("Successful response for CIH-UPD", () =>
			{
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);

				AssertEquals("MRN Number NOT be updated", "38BR15856778945", entry.MovementReferenceNumber);
				AssertEquals("CH_CustomsPostedStatus update", CustomsPostedStatusList.Codes.Accepted, entry.CH_CustomsPostedStatus);
				AssertEquals("CH_Status update", BRMessageStatusList.Codes.Accepted, entry.CH_Status);
				AssertEquals("No CIH message sent on message processor", 3, headerMessages.Except(new[] { requestMessage }).Count());
				AssertEquals("No CIL message sent on message processor", 3, lineMessages.Count());
			});
		}

		public void TestProcessResponseMessageUpdate_Rejected()
		{
			var entry = CreateEntryHeader("38BR15856778945");

			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			entry.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
			entry.AllEntryLines.AddNew().CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Active;
			entry.AllEntryLines.AddNew().CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			entry.AllEntryLines.AddNew().CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
			entry.AllEntryLines.AddNew().CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Deleted;

			var (requestMessage, responseMessage) = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Success, requestMessageSubType: EDIMessageSubTypeList.Codes.Update);
			responseMessage.EM_MessageText = JsonMessageResponse;
			CreateLineMessageAndSetAsProcessed(entry, EDIMessageSubTypeList.Codes.Addition);
			CreateLineMessageAndSetAsProcessed(entry, EDIMessageSubTypeList.Codes.Update);
			CreateLineMessageAndSetAsProcessed(entry, EDIMessageSubTypeList.Codes.Deletion);
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);

				AssertEquals("MRN Number NOT be updated", "38BR15856778945", entry.MovementReferenceNumber);
				AssertEquals("CH_CustomsPostedStatus not updated", CustomsPostedStatusList.Codes.UpdatePending, entry.CH_CustomsPostedStatus);
				AssertEquals("CH_Status updated", BRMessageStatusList.Codes.Rejected, entry.CH_Status);
				AssertEquals("MessageRejected log added", AutoEvents.MessageRejected.Code, entry.Logs.MostRecentLog.SL_SE_NKEvent);
			});
		}

		public void TestProcessResponseMessageUpdate_NotAllLineMessagesProcessed()
		{
			var entry = CreateEntryHeader("38BR15856778945");
			entry.CH_BGMReference = "B00001000-1";

			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			entry.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
			entry.AllEntryLines.AddNew().CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Active;
			entry.AllEntryLines.AddNew().CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			entry.AllEntryLines.AddNew().CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
			entry.AllEntryLines.AddNew().CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Deleted;
			Factory.Save();

			var (requestMessage, responseMessage) = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Success, requestMessageSubType: EDIMessageSubTypeList.Codes.Update);
			responseMessage.EM_MessageText = JsonMessageResponse;
			CreateLineMessageAndSetAsProcessed(entry, EDIMessageSubTypeList.Codes.Addition);
			CreateLineMessageAndSetAsProcessed(entry, EDIMessageSubTypeList.Codes.Update);
			CreateLineMessage(entry, EDIMessageSubTypeList.Codes.Deletion);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<MessageProcessLockException>("MessageProcessLockException thrown to postpone",
					"Message #1 postponed: Entry Header B00001000-1, has CIL message waiting response.", () =>
					{
						var logger = ExecuteMessageProcessor(responseMessage);
						AssertEquals("Logger", "Warning: \tMessage #1 postponed: Entry Header B00001000-1, has CIL message waiting response.\r\n", logger.LogMessages.ToString());
					});
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);

				AssertEquals("MRN Number NOT be updated", "38BR15856778945", entry.MovementReferenceNumber);
				AssertEquals("CH_Status NOT updated", BRMessageStatusList.Codes.AwaitingResponse, entry.CH_Status);
				AssertEquals("CH_CustomsPostedStatus NOT updated", CustomsPostedStatusList.Codes.UpdatePending, entry.CH_CustomsPostedStatus);
			});
		}

		public void TestProcessResponseMessage_DeserializationFailed()
		{
			var entryHeader = CreateEntryHeader();

			AssertProcessResponseMessage_LogErrorWithDeserializationFailed("");
			AssertProcessResponseMessage_LogErrorWithDeserializationFailed(JsonMessageResponse.Replace(@"""numero"": ""20BR00000012345"",", ""));

			void AssertProcessResponseMessage_LogErrorWithDeserializationFailed(string messageText)
			{
				var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entryHeader, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Success, requestMessageSubType: EDIMessageSubTypeList.Codes.Original).ResponseMessage;
				responseMessage.EM_MessageText = messageText;
				Factory.Save();

				var logger = ExecuteMessageProcessor(responseMessage);
				CombineAssertions(() =>
				{
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
					AssertEquals("Logger", "Error: \tMessage #1: Message deserialization was failed.\r\n", logger.LogMessages.ToString());
					AssertEquals("CH_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryHeader.CH_CustomsPostedStatus);
				});
			}
		}

		public void TestProcessResponseMessageCompleteConsult_Success()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDI;
			entryHeader.MovementReferenceNumberSetter("24BR00000002090");

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entryHeader, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Success, requestMessageSubType: EDIMessageSubTypeList.Codes.CompleteConsult).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageCompleteConsult;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("CH_AdministrativeStatus", "2", entryHeader.CH_AdministrativeStatus);
				AssertEquals("CH_CargoStatus", "7", entryHeader.CH_CargoStatus);
				AssertEquals("CH_RiskChannel", "1", entryHeader.CH_RiskChannel);
				AssertEquals("RiskChannelDescription", "Green", entryHeader.RiskChannelDescription);
				AssertEquals("EntryAccessKey updated", "21ASW000000879", entryHeader.EntryAccessKey);

				AssertEquals("EM_GB", entryHeader.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", entryHeader.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entryHeader.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("EM_MessageInterpretation", DuimpHeaderCompleteConsultMessagePrettyFormatterTest.GetExpectedExportSuccessHTML(EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference)), responseMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessResponseMessageDiagnosis_Success()
		{
			var entry = CreateEntryHeader();
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Success, requestMessageSubType: ImportEntryActionCodeList.Codes.DIA).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageResponse;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("CH_Status", BRMessageStatusList.Codes.Accepted, entry.CH_Status);

				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
			});
		}

		public void TestProcessResponseMessageRegister_Success()
		{
			var entry = CreateEntryHeader();
			entry.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Success, requestMessageSubType: ImportEntryActionCodeList.Codes.REG).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageResponse;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("CH_Status", BRMessageStatusList.Codes.Accepted, entry.CH_Status);

				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
			});
		}

		public void TestProcessResponseMessageCompleteConsult_DeserializationFailed()
		{
			var entryHeader = CreateEntryHeader("24BR00000002090");

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entryHeader, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Success, requestMessageSubType: EDIMessageSubTypeList.Codes.CompleteConsult).ResponseMessage;
			responseMessage.EM_MessageText = "";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertEquals("Logger", "Error: \tMessage #1: Message deserialization was failed.\r\n", logger.LogMessages.ToString());
			});
		}

		public void TestProcessResponseMessageNoMatchingMessageSubType()
		{
			var entryHeader = CreateEntryHeader();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entryHeader, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Success, requestMessageSubType: EDIMessageSubTypeList.Codes.Amend).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageResponse;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Discarded, responseMessage.EM_Status);
				AssertEquals("Logger", "Error: \tMessage #1: Request Message 1 is not original, update or complete consult.\r\n", logger.LogMessages.ToString());
			});
		}

		EDIMessage CreateLineMessage(CusEntryHeader entry, string lineMessageSubType)
		{
			return BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Success,
				requestMessageSubType: lineMessageSubType).ResponseMessage;
		}

		void CreateLineMessageAndSetAsProcessed(CusEntryHeader entry, string lineMessageSubType)
		{
			CreateLineMessage(entry, lineMessageSubType).EM_Status = EDIMessage.Status.ProcessedOK;
		}

		CusEntryHeader CreateEntryHeader(string entryNumber = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CDI;
			if (entryNumber != null)
			{
				entryHeader.MovementReferenceNumberSetter(entryNumber);
			}

			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Active;
			invoiceLine.JI_CL = entryLine.PK;
			return entryHeader;
		}

		const string JsonMessageResponse = @"{
  ""message"": ""Mensagem de exemplo."",
  ""identificacao"": {
    ""numero"": ""20BR00000012345"",
    ""versao"": ""1""
  },
  ""links"": [
    {
      ""_rel"": ""string"",
      ""_href"": ""string"",
      ""_method"": ""GET""
    }
  ]
}";

		internal const string JsonMessageCompleteConsult = @"{
	""identificacao"": {
		""numero"": ""24BR00000002090"",
		""versao"": ""1"",
		""importador"": {
			""ni"": ""00055555000130""
		},
		""dataRegistro"": ""2021-05-25T15:53:18-0300"",
		""responsavelRegistroNumero"": ""60025721046"",
		""informacaoComplementar"": ""Texto complementando informações sobre a Duimp."",
		""chaveAcesso"": ""21ASW000000879""
	},
	""situacao"": {
		""situacaoDuimp"": ""REGISTRADA_AGUARDANDO_CANAL"",
		""situacaoAnaliseRetificacao"": ""PENDENTE_AGUARDANDO_ANALISE"",
		""situacaoLicenciamento"": ""DISPENSADO"",
		""controleCarga"": ""VINCULADA"",
		""situacaoConferenciaAduaneira"": [
			{
				""siglaOrgao"": ""RECEITA"",
				""situacao"": ""DESEMBARACO_AUTOMATICO"",
				""indicadorAutorizacaoEntrega"": ""SIM"",
				""indicadorDesembaracoDecisaoJudicial"": ""SIM""
      },
      {
        ""siglaOrgao"": ""RECEITA 2"",
        ""situacao"": ""DESEMBARACO_MANUAL"",
        ""indicadorAutorizacaoEntrega"": ""NAO"",
        ""indicadorDesembaracoDecisaoJudicial"": ""NAO""
			}
		],
		""situacaoConferenciaAnuente"": [
			{
				""siglaOrgao"": ""ANVISA"",
				""situacao"": ""DESEMBARACO_AUTOMATICO"",
				""indicadorAutorizacaoProsseguimentoConferenciaAnuente"": ""SIM"",
				""indicadorConclusaoDecisaoJudicial"": ""SIM""
      },
      {
        ""siglaOrgao"": ""ANVISA 2"",
        ""situacao"": ""DESEMBARACO_MANUAL"",
        ""indicadorAutorizacaoProsseguimentoConferenciaAnuente"": ""SIM"",
        ""indicadorConclusaoDecisaoJudicial"": ""NAO""
			}
		]
	},
	""equipesTrabalho"": [
		{
			""siglaOrgao"": ""ANVISA"",
			""codigo"": ""07106001"",
			""descricao"": ""Conferência de importação do Porto do Rio""
		}
	],
	""resultadoAnaliseRisco"": {
		""canalConsolidado"": ""VERDE"",
		""resultadoRFB"": [
			{
				""orgao"": ""Receita"",
				""resultado"": ""DESEMBARACO_AUTORIZADO""
      },
      {
        ""orgao"": ""Receita 2"",
        ""resultado"": ""DESEMBARACO_MANUAL""
			}
		],
		""resultadoAnuente"": [
			{
				""orgao"": ""MAPA"",
				""resultado"": ""ANALISE_DOCUMENTAL""
      },
      {
        ""orgao"": ""MAPA 2 "",
        ""resultado"": ""ANALISE_XML""
			}
		]
	},
	""carga"": {
		""unidadeDeclarada"": {
			""codigo"": ""7912001""
		},
		""identificacao"": ""132105000002800"",
		""seguro"": {
			""codigoMoedaNegociada"": ""USD"",
			""valorMoedaNegociada"": 30.22
		},
		""frete"": {
			""codigoMoedaNegociada"": ""USD"",
			""valorMoedaNegociada"": 30.22
		},
		""valorAFRMMDevido"": 153.77,
		""valorAFRMMPago"": 153.77,
		""indicadorAFRMMQuitado"": ""SIM"",
		""motivoSituacaoEspecial"": {
			""codigo"": ""1""
		}
	},
	""documentos"": {
		""documentosInstrucao"": [
			{
				""tipo"": {
					""codigo"": ""99""
				},
				""palavrasChave"": [
					{
						""codigo"": 33,
						""valor"": ""9999.99.99""
					}
				]
			}
		],
		""processos"": [
			{
				""identificacao"": ""15595720034201371"",
				""tipo"": ""ADMINISTRATIVO""
			}
		],
		""declaracoesExportacaoEstrangeira"": [
			{
				""numero"": ""19XY0000001-XYZ"",
				""faixaInicio"": ""A-11"",
				""faixaFim"": ""B-20""
			}
		],
		""dossies"": [
			{
				""numero"": ""201950000000515""
			}
		]
	},
	""adicoes"": [
		{
			""numero"": 1,
			""itens"": 1
		}
	],
	""tributos"": {
		""mercadoria"": {
			""valorTotalLocalEmbarqueBRL"": 20.366,
			""valorTotalLocalEmbarqueUSD"": 20.366
		},
		""tributosCalculados"": [
			{
				""tipo"": ""II"",
				""valoresBRL"": {
					""calculado"": 1598.73,
					""aReduzir"": 135.7,
					""devido"": 201.12,
					""suspenso"": 16.7,
					""aRecolher"": 16.7,
					""recolhido"": 16.7
				}
			}
		]
	},
	""pagamentos"": [
		{
			""versaoOrigem"": ""1"",
			""principal"": {
				""dataPagamento"": ""2021-05-25T15:53:18-0300"",
				""codigoReceita"": ""5602"",
				""banco"": ""001"",
				""agencia"": ""3521"",
				""conta"": ""707070"",
				""tributo"": {
					""tipo"": ""II""
				},
				""valor"": 17.2,
				""juros"": {
					""codigoReceita"": ""5602"",
					""valor"": 100.1,
					""dataPagamentoJuros"": ""2021-05-25T15:53:18-0300"",
					""bancoJuros"": ""001"",
					""agenciaJuros"": ""3521"",
					""contaJuros"": ""707070""
				}
			}
		}
	],
	""tratamentoAdministrativo"": {
		""resultadoProcessamentoTA"": {
			""dataProcessamento"": ""2021-05-25T15:53:18-0300"",
			""resultadoConsolidadoTA"": ""DEFERIDO""
		},
		""itensTratamentoAdministrativo"": [
			{
				""numeroItemDuimp"": 1,
				""tipoTratamento"": ""IMPEDE_REGISTRO"",
				""descricao"": ""ALERTA"",
				""orgao"": ""DECEX"",
				""lpco"": ""21255555555"",
				""observacoes"": ""Texto de observação.""
			}
		]
	},
	""quantidadeItens"": 100,
	""itens"": [
		{
			""indice"": 1,
			""link"": ""ext/duimp/19BR00000004677/0/itens/1""
		}
	]
}
";
	}
}
