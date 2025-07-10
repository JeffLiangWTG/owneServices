using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCExportCompleteConsultMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCExportCompleteConsultMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CDE" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "COM" };

		public void TestProcessResponseMessage_EntryNotFound()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var requestInterchange = BRCResponseMessageProcessorTest.CreateInterchange(Factory, ZGuid.NewZGuid(), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Sent, MessageTypeList.Codes.CDE);
			var requestMessage = BRCResponseMessageProcessorTest.CreateMessage(Factory, requestInterchange.PK, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Sent, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);
			var responseInterchange = BRCResponseMessageProcessorTest.CreateInterchange(Factory, requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Received, MessageTypeList.Codes.CDE);
			var responseMessage = BRCResponseMessageProcessorTest.CreateMessage(Factory, responseInterchange.PK, null, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);

			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", ZGuid.Empty, requestMessage.EM_LinkUniqueID);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertContains("Logger", "Error: \tUnable to locate the related Business Object for CDE message #1\r\n", logger.LogMessages.ToString());
			});
		}

		public void TestUpdateRiskChannel()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("23BR0010362485");
			entry.CH_MessageType = MessageTypeList.Codes.CDE;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.CompleteConsult).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkTable", "CusEntryHeader", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.CDE, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);

				AssertEquals("CH_RiskChannel", "1", entry.CH_RiskChannel);
				AssertEquals("CH_AdministrativeStatus", "1", entry.CH_AdministrativeStatus);
				AssertEquals("AdministrativeStatusDescription", "Deferred", entry.AdministrativeStatusDescription);
				AssertEquals("CH_CargoStatus", "3", entry.CH_CargoStatus);
				AssertEquals("CargoStatusDescription", "Cargo Fully Exported", entry.CargoStatusDescription);
			});
		}

		public void TestProcessResponseMessage_UpdateMessageStatus()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("23BR0010362485");
			entry.CH_MessageType = MessageTypeList.Codes.CDE;
			Factory.Save();

			AssertMessageStatus(JsonMessage.Replace("\"dataDeRegistro\": \"2023-06-30T23:22:41.000+0000\",", string.Empty));
			AssertMessageStatus(JsonMessage.Replace("\"chaveDeAcesso\": \"23COZ100034720\",", string.Empty));
			AssertMessageStatus(JsonMessage, BRMessageStatusList.Codes.Accepted);

			void AssertMessageStatus(string message, string expectedMessageStatus = "")
			{
				var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.CompleteConsult).ResponseMessage;
				responseMessage.EM_MessageText = message;
				Factory.Save();

				ExecuteMessageProcessor(responseMessage);
				AssertEquals("CH_Status", expectedMessageStatus, entry.CH_Status);
			}
		}

		public void TestProcessResponseMessage_InvalidAdministrativeStatus()
		{
			ReferenceTestDataHelper.CreateEntryStatusForExport(Factory);

			var jsonMessageUpdated = JsonMessage.Replace("\"situacaoDoTratamentoAdministrativo\": \"DEFERIDO\"", "\"situacaoDoTratamentoAdministrativo\": \"TEST\"");
			AssertInvalidResponse(jsonMessageUpdated, "Unable to find Export Administrative Status Code for situacaoDoTratamentoAdministrativo 'TEST'.");

			jsonMessageUpdated = JsonMessage.Replace("\"situacaoDoTratamentoAdministrativo\": \"DEFERIDO\",", string.Empty);
			AssertInvalidResponse(jsonMessageUpdated, string.Empty);
		}

		public void TestProcessResponseMessage_InvalidCargoStatus()
		{
			ReferenceTestDataHelper.CreateEntryStatusForExport(Factory);

			var jsonMessageUpdated = JsonMessage.Replace("\"codigo\": 3,", "\"codigo\": 8,");
			AssertInvalidResponse(jsonMessageUpdated, "Unable to find Export Cargo Situation Code for situacoesDaCarga '8'.");

			jsonMessageUpdated = JsonMessage.Replace("\"situacoesDaCarga\": [\r\n        {\r\n            \"codigo\": 3,\r\n            \"descricao\": \"Carga Completamente Exportada\",\r\n            \"cargaOperada\": true\r\n        }\r\n    ],", string.Empty);
			AssertInvalidResponse(jsonMessageUpdated, string.Empty);
		}

		public void TestProcessResponseMessage_InvalidRiskChannel()
		{
			ReferenceTestDataHelper.CreateEntryStatusForExport(Factory);

			var jsonMessageUpdated = JsonMessage.Replace("\"canal\": \"VERDE\",", " \"canal\": \"TEST\",");
			AssertInvalidResponse(jsonMessageUpdated, "Unable to find Risk Channel Code for canal 'TEST'.");

			jsonMessageUpdated = JsonMessage.Replace("\"canal\": VERDE,", "\"canal\": ,");
			AssertInvalidResponse(jsonMessageUpdated, string.Empty);
		}

		void AssertInvalidResponse(string jsonMessage, string expectedErrorReportedMessage)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("23BR0010362485");
			entry.CH_MessageType = MessageTypeList.Codes.CDE;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.CompleteConsult).ResponseMessage;
			responseMessage.EM_MessageText = jsonMessage;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			AssertEquals(expectedErrorReportedMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
			ErrorReporter.Clear();
		}

		internal const string JsonMessage = @"{
    ""bloqueio"": false,
    ""canal"": ""VERDE"",
    ""chaveDeAcesso"": ""23COZ100034720"",
    ""dataDeRegistro"": ""2023-06-30T23:22:41.000+0000"",
    ""declarante"": {
        ""numeroDoDocumento"": ""00406859000103"",
        ""tipoDoDocumento"": ""CNPJ"",
        ""nome"": ""AGAPLASTIC INDUSTRIA E COMERCIO LTDA"",
        ""estrangeiro"": false,
        ""nacionalidade"": {
            ""codigo"": 105,
            ""nome"": ""BRASIL"",
            ""nomeResumido"": ""BRA""
        }
    },
    ""embarqueEmRecintoAlfandegado"": true,
    ""despachoEmRecintoAlfandegado"": true,
    ""eventosDoHistorico"": [
        {
            ""dataEHoraDoEvento"": ""2023-06-27T16:56:40.993+0000"",
            ""evento"": ""Registro"",
            ""responsavel"": ""00302993738"",
            ""informacoesAdicionais"": ""Origem: Frontend""
        },
        {
            ""dataEHoraDoEvento"": ""2023-06-28T14:01:06.729+0000"",
            ""evento"": ""Apresentação para despacho"",
            ""responsavel"": ""Automático""
        },
        {
            ""dataEHoraDoEvento"": ""2023-06-28T14:01:09.462+0000"",
            ""evento"": ""Liberação sem conferência aduaneira"",
            ""responsavel"": ""Automático"",
            ""informacoesAdicionais"": ""Canal Verde""
        },
        {
            ""dataEHoraDoEvento"": ""2023-06-28T14:01:09.547+0000"",
            ""evento"": ""Desembaraço"",
            ""responsavel"": ""Automático""
        },
        {
            ""dataEHoraDoEvento"": ""2023-06-29T15:18:33.245+0000"",
            ""evento"": ""Entrega de carga - URF 717700 | RA 7911101"",
            ""responsavel"": ""59961953720""
        },
        {
            ""dataEHoraDoEvento"": ""2023-06-30T23:22:40.910+0000"",
            ""evento"": ""Manifestação de dados de embarque - Veículo PHBQF"",
            ""responsavel"": ""12679707745""
        },
        {
            ""dataEHoraDoEvento"": ""2023-06-30T23:22:41.381+0000"",
            ""evento"": ""Carga Completamente Exportada"",
            ""responsavel"": ""Automático"",
            ""informacoesAdicionais"": ""Embarque da carga para o exterior: 29/06/2023""
        },
        {
            ""dataEHoraDoEvento"": ""2023-06-30T23:22:41.472+0000"",
            ""evento"": ""Averbação"",
            ""responsavel"": ""Automático""
        }
    ],
    ""exigenciasFiscais"": [
    ],
    ""formaDeExportacao"": ""POR_CONTA_PROPRIA"",
    ""impedidoDeEmbarque"": false,
    ""informacoesComplementares"": ""FATURA: 011/2023"",
    ""itens"": [
        {
            ""ncm"": {
                ""codigo"": ""39269040"",
                ""descricao"": ""ARTIGOS DE LABORATÓRIO OU DE FARMÁCIA"",
                ""unidadeMedidaEstatistica"": ""QUILOGRAMA LIQUIDO""
            },
            ""quantidadeNaUnidadeEstatistica"": 336,
            ""numero"": 1,
            ""pesoLiquidoTotal"": 336,
            ""valorDaMercadoriaNaCondicaoDeVenda"": 11014.02,
            ""valorDaMercadoriaNoLocalDeEmbarque"": 9800,
            ""valorDaMercadoriaNoLocalDeEmbarqueEmReais"": 46744.0400,
            ""valorDaMercadoriaNaCondicaoDeVendaEmReais"": 52534.672596,
            ""dataDeConversao"": ""2023-06-27T16:20:00.000+0000"",
            ""itemDaNotaFiscalDeExportacao"": {
                ""numeroDoItem"": 1,
                ""notaFiscal"": {
                    ""chaveDeAcesso"": ""33230600406859000103550010000450351000135951"",
                    ""modelo"": ""55"",
                    ""serie"": 1,
                    ""numeroDoDocumento"": 45035,
                    ""ufDoEmissor"": ""RJ"",
                    ""identificacaoDoEmitente"": {
                        ""numero"": ""00406859000103"",
                        ""cnpj"": true,
                        ""cpf"": false
                    },
                    ""finalidade"": ""NF-e normal"",
                    ""quantidadeDeItens"": 2,
                    ""notaFicalEletronica"": true
                },
                ""cfop"": 7101,
                ""codigoDoProduto"": ""AL103"",
                ""descricao"": ""ABAIXADOR DE LINGUA TIC-TONG ANIMAL"",
                ""quantidadeEstatistica"": 336,
                ""unidadeComercial"": ""PCT"",
                ""valorTotalCalculado"": 52880.43,
                ""ncm"": {
                    ""codigo"": ""39269040"",
                    ""descricao"": ""ARTIGOS DE LABORATÓRIO OU DE FARMÁCIA"",
                    ""unidadeMedidaEstatistica"": ""QUILOGRAMA LIQUIDO""
                },
                ""apresentadaParaDespacho"": true
            },
            ""itensDeNotaComplementar"": [
            ],
            ""itensDaNotaDeRemessa"": [
            ],
            ""descricaoDaMercadoria"": ""ABAIXADOR DE LINGUA TIC-TONG ANIMAL"",
            ""exportador"": {
                ""numeroDoDocumento"": ""00406859000103"",
                ""tipoDoDocumento"": ""CNPJ"",
                ""estrangeiro"": false,
                ""nacionalidade"": {
                    ""codigo"": 105,
                    ""nome"": ""BRASIL"",
                    ""nomeResumido"": ""BRA""
                }
            },
            ""unidadeComercializada"": ""PCT"",
            ""atributos"": [
            ],
            ""tratamentosAdministrativos"": [
            ],
            ""documentosImportacao"": [
            ],
            ""documentosDeTransformacao"": [
            ],
            ""codigoCondicaoVenda"": {
                ""codigo"": ""CIP""
            },
            ""nomeImportador"": ""OTOPLUG OY"",
            ""enderecoImportador"": ""TUOMAALANKATU - 1 - TAMPERE   -  FINLANDIA - Exterior - 33580000 - FINLANDIA"",
            ""listaDeEnquadramentos"": [
                {
                    ""codigo"": 80000,
                    ""dataRegistro"": ""2023-06-30T23:22:41.000+0000""
                }
            ],
            ""listaPaisDestino"": [
                {
                    ""codigo"": 271
                }
            ],
            ""valorTotalCalculadoItem"": 52880.43,
            ""quantidadeNaUnidadeComercializada"": 1400,
            ""calculoTributario"": {
                ""tratamentosTributarios"": [
                ],
                ""quadroDeCalculos"": [
                ]
            },
            ""exportacaoTemporaria"": {
                ""temporaria"": false
            }
        },
        {
            ""ncm"": {
                ""codigo"": ""39269040"",
                ""descricao"": ""ARTIGOS DE LABORATÓRIO OU DE FARMÁCIA"",
                ""unidadeMedidaEstatistica"": ""QUILOGRAMA LIQUIDO""
            },
            ""quantidadeNaUnidadeEstatistica"": 189,
            ""numero"": 2,
            ""pesoLiquidoTotal"": 189,
            ""valorDaMercadoriaNaCondicaoDeVenda"": 7297.86,
            ""valorDaMercadoriaNoLocalDeEmbarque"": 6615,
            ""valorDaMercadoriaNoLocalDeEmbarqueEmReais"": 31552.2270,
            ""valorDaMercadoriaNaCondicaoDeVendaEmReais"": 34809.332628,
            ""dataDeConversao"": ""2023-06-27T16:20:00.000+0000"",
            ""itemDaNotaFiscalDeExportacao"": {
                ""numeroDoItem"": 2,
                ""notaFiscal"": {
                    ""chaveDeAcesso"": ""33230600406859000103550010000450351000135951"",
                    ""modelo"": ""55"",
                    ""serie"": 1,
                    ""numeroDoDocumento"": 45035,
                    ""ufDoEmissor"": ""RJ"",
                    ""identificacaoDoEmitente"": {
                        ""numero"": ""00406859000103"",
                        ""cnpj"": true,
                        ""cpf"": false
                    },
                    ""finalidade"": ""NF-e normal"",
                    ""quantidadeDeItens"": 2,
                    ""notaFicalEletronica"": true
                },
                ""cfop"": 7101,
                ""codigoDoProduto"": ""AL104"",
                ""descricao"": ""ABAIXADOR DE LINGUA TIC-TONG ANIMAL JUNIOR"",
                ""quantidadeEstatistica"": 189,
                ""unidadeComercial"": ""PCT"",
                ""valorTotalCalculado"": 35038.50,
                ""ncm"": {
                    ""codigo"": ""39269040"",
                    ""descricao"": ""ARTIGOS DE LABORATÓRIO OU DE FARMÁCIA"",
                    ""unidadeMedidaEstatistica"": ""QUILOGRAMA LIQUIDO""
                },
                ""apresentadaParaDespacho"": true
            },
            ""itensDeNotaComplementar"": [
            ],
            ""itensDaNotaDeRemessa"": [
            ],
            ""descricaoDaMercadoria"": ""ABAIXADOR DE LINGUA TIC-TONG ANIMAL JUNIOR"",
            ""exportador"": {
                ""numeroDoDocumento"": ""00406859000103"",
                ""tipoDoDocumento"": ""CNPJ"",
                ""estrangeiro"": false,
                ""nacionalidade"": {
                    ""codigo"": 105,
                    ""nome"": ""BRASIL"",
                    ""nomeResumido"": ""BRA""
                }
            },
            ""unidadeComercializada"": ""PCT"",
            ""atributos"": [
            ],
            ""tratamentosAdministrativos"": [
            ],
            ""documentosImportacao"": [
            ],
            ""documentosDeTransformacao"": [
            ],
            ""codigoCondicaoVenda"": {
                ""codigo"": ""CIP""
            },
            ""nomeImportador"": ""OTOPLUG OY"",
            ""enderecoImportador"": ""TUOMAALANKATU - 1 - TAMPERE   -  FINLANDIA - Exterior - 33580000 - FINLANDIA"",
            ""listaDeEnquadramentos"": [
                {
                    ""codigo"": 80000,
                    ""dataRegistro"": ""2023-06-30T23:22:41.000+0000""
                }
            ],
            ""listaPaisDestino"": [
                {
                    ""codigo"": 271
                }
            ],
            ""valorTotalCalculadoItem"": 35038.50,
            ""quantidadeNaUnidadeComercializada"": 1050,
            ""calculoTributario"": {
                ""tratamentosTributarios"": [
                ],
                ""quadroDeCalculos"": [
                ]
            },
            ""exportacaoTemporaria"": {
                ""temporaria"": false
            }
        }
    ],
    ""moeda"": {
        ""codigo"": 220
    },
    ""numero"": ""23BR0010362485"",
    ""paisImportador"": {
        ""codigo"": 271
    },
    ""recintoAduaneiroDeDespacho"": {
        ""codigo"": ""7911101""
    },
    ""recintoAduaneiroDeEmbarque"": {
        ""codigo"": ""7911101""
    },
    ""ruc"": ""3BR00406859200000000000000000693126"",
    ""situacao"": ""AVERBADA_SEM_DIVERGENCIA"",
    ""situacaoDoTratamentoAdministrativo"": ""DEFERIDO"",
    ""situacoesDaCarga"": [
        {
            ""codigo"": 3,
            ""descricao"": ""Carga Completamente Exportada"",
            ""cargaOperada"": true
        }
    ],
    ""solicitacoes"": [
    ],
    ""tipo"": ""NOTA_FISCAL_ELETRONICA"",
    ""tratamentoPrioritario"": false,
    ""unidadeLocalDeDespacho"": {
        ""codigo"": ""0717700""
    },
    ""unidadeLocalDeEmbarque"": {
        ""codigo"": ""0717700""
    },
    ""responsavelPeloACD"": ""RECEPCAO_NO_CCT"",
    ""despachoEmRecintoDomiciliar"": false,
    ""dataDoCCE"": ""2023-06-30T23:22:41.000+0000"",
    ""dataDeCriacao"": ""2023-06-27T16:56:40.000+0000"",
    ""valorTotalMercadoria"": 87918.93000,
    ""inclusaoNotaFiscal"": false,
    ""exigenciaAtiva"": false,
    ""consorciada"": false,
    ""declaracaoTributaria"": {
        ""compensacoes"": [
        ],
        ""contestacoes"": [
        ],
        ""recolhimentos"": [
        ],
        ""divergente"": false
    },
    ""atosConcessoriosIsencao"": {
        ""title"": ""Link para lista de atos concessórios do tipo isenção"",
        ""href"": ""https://portalunico.siscomex.gov.br/due/api/ext/due/23BR0010362485/drawback/isencao/atos-concessorios"",
        ""method"": ""GET"",
        ""type"": ""application/json""
    },
    ""exigenciasFiscaisEstruturadas"": {
        ""title"": ""Link para a lista de exigências fiscais"",
        ""href"": ""https://portalunico.siscomex.gov.br/due/api/ext/due/23BR0010362485/exigencias-fiscais"",
        ""method"": ""GET"",
        ""type"": ""application/json""
    },
    ""dat"": false,
    ""oea"": false
}";
	}
}
