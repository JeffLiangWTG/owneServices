using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCLPCOSuccessResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCLPCOSuccessResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "LPC" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "SUC" };

		public void TestProcessResponseMessage_LogErrorWithDeserializationFailed()
		{
			var lpcoHeader = Factory.NewWithValidTestData<CusLPCOHeader>();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(lpcoHeader, MessageTypeList.Codes.LPC, null).ResponseMessage;
			responseMessage.EM_MessageText = string.Empty;
			Factory.Save();

			var logger =  ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", lpcoHeader.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", lpcoHeader.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", lpcoHeader.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertEquals("Logger", "Error: \tMessage #1: Message deserialization was failed.\r\n", logger.LogMessages.ToString());
			});
		}

		public void TestProcessResponseMessage_Success()
		{
			var lpcoHeader = Factory.NewWithValidTestData<CusLPCOHeader>();
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(lpcoHeader, MessageTypeList.Codes.LPC, null).ResponseMessage;
			responseMessage.EM_MessageText = LPCOResponseMessageText;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", lpcoHeader.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", lpcoHeader.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", lpcoHeader.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);

				AssertEquals("CPH_Number updated", "E1900000001", lpcoHeader.CPH_Number);
				AssertEquals("CPH_StartDate", new ZDate("2019-09-02"), lpcoHeader.CPH_StartDate);
				AssertEquals("CPH_EndDate", new ZDate("2078-12-31"), lpcoHeader.CPH_EndDate);
				AssertEquals("CPH_MessageStatus", BRMessageStatusList.Codes.Accepted, lpcoHeader.CPH_MessageStatus);
			});
		}

		public void TestProcessResponseMessage_Rejection()
		{
			AssertProcessRejectionResponse(LPCOResponseMessageText.Replace(@"""numero"": ""E1900000001"",", @"""numero"": """","));
			AssertProcessRejectionResponse(LPCOResponseMessageText.Replace(@"""numero"": ""E1900000001"",", ""));

			void AssertProcessRejectionResponse(string messageText)
			{
				var lpcoHeader = Factory.NewWithValidTestData<CusLPCOHeader>();
				lpcoHeader.CPH_Number = "E1900000001";
				var startDate = lpcoHeader.CPH_StartDate;
				var endDate = lpcoHeader.CPH_EndDate;

				var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(lpcoHeader, MessageTypeList.Codes.LPC, null).ResponseMessage;
				responseMessage.EM_MessageText = messageText;
				Factory.Save();

				ExecuteMessageProcessor(responseMessage);
				CombineAssertions(() =>
				{
					AssertEquals("EM_GB", lpcoHeader.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
					AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);

					AssertEquals("CPH_Number NOT cleared", "E1900000001", lpcoHeader.CPH_Number);
					AssertEquals("CPH_StartDate", startDate, lpcoHeader.CPH_StartDate);
					AssertEquals("CPH_EndDate", endDate, lpcoHeader.CPH_EndDate);
					AssertEquals("CPH_MessageStatus", BRMessageStatusList.Codes.Rejected, lpcoHeader.CPH_MessageStatus);
				});
			}
		}

		public void TestProcessResponseMessage_InvalidDataRegistro()
		{
			var lpcoHeader = Factory.NewWithValidTestData<CusLPCOHeader>();
			lpcoHeader.CPH_StartDate = new ZDate(2023, 2, 10);
			lpcoHeader.CPH_EndDate = new ZDate(2079, 2, 10);

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(lpcoHeader, MessageTypeList.Codes.LPC, null).ResponseMessage;
			responseMessage.EM_MessageText = LPCOResponseMessageText.Replace(@"""dataInicioVigencia"": ""2019-09-02T10:04:38.123Z"",", @"""dataRegistro"": ""0001-01-01T00:00:00.000Z"",");
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			AssertEquals("CPH_StartDate NOT updated", new ZDateTime(2023, 2, 10), lpcoHeader.CPH_StartDate);

			responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(lpcoHeader, MessageTypeList.Codes.LPC, null).ResponseMessage;
			responseMessage.EM_MessageText = LPCOResponseMessageText.Replace(@"""dataInicioVigencia"": ""2019-09-02T10:04:38.123Z"",", @"");
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			AssertEquals("CPH_StartDate NOT updated", new ZDateTime(2023, 2, 10), lpcoHeader.CPH_StartDate);
		}

		const string LPCOResponseMessageText = @"{
  ""dataInicioVigencia"": ""2019-09-02T10:04:38.123Z"",
  ""dataFimVigencia"": ""2078-12-31T23:59:59.000Z"",
  ""numero"": ""E1900000001"",
  ""codigoModelo"": ""E00104"",
  ""dataInicioVigenciaModelo"": ""2019-08-29T13:50Z"",
  ""orgao"": ""MAPA"",
  ""situacao"": {
    ""id"": ""DEFERIDO"",
    ""descricao"": ""Deferido""
  },
  ""dataSituacaoAtual"": ""2019-08-29T14:03:52.123Z"",
  ""informacaoAdicional"": ""Texto Livre"",
  ""chaveAcesso"": ""7ae071d708d04808b5d7624fafae57d4"",
  ""prorrogacaoPendente"": true,
  ""retificacaoPendente"": true,
  ""dataRegistro"": ""2019-08-29T14:03:52.123Z"",
  ""listaCamposFormulario"": [
    {
      ""codigo"": ""CPF_CNPJ_EXPORTADOR"",
      ""listaValor"": [""12345678901"", ""12345678901234""],
      ""valorComposto"": {
        ""indicacaoImportacaoTerceiros"": {
          ""codigoIndicador"": 0,
          ""cpfCnpj"": ""03141554900""
        },
        ""exportadorEstrangeiro"": {
          ""codigo"": ""111222333"",
          ""cpfCnpjRaiz"": ""00055555"",
          ""codigoPais"": ""AR"",
          ""versao"": ""1""
        },
        ""exportadorEFabricanteDoProduto"": {
          ""exportadorIgualFabricante"": true,
          ""operadorEstrangeiro"": {
            ""codigo"": ""111222333"",
            ""cpfCnpjRaiz"": ""00055555"",
            ""codigoPais"": ""AR"",
            ""versao"": ""1""
          }
        },
        ""fabricante"": {
          ""codigoPais"": ""AR"",
          ""conhecido"": true,
          ""cpfCnpj"": ""03141554900"",
          ""operadorEstrangeiro"": {
            ""codigo"": ""111222333"",
            ""cpfCnpjRaiz"": ""00055555"",
            ""codigoPais"": ""AR"",
            ""versao"": ""1""
          }
        },
        ""fundamentoLegal"": {
          ""codigoFundamento"": ""00102030004"",
          ""ncm"": ""01012100"",
          ""camposAdicionais"": [
            {
              ""codigoAtributo"": ""ATT_2982"",
              ""valor"": ""32061100""
            }
          ]
        },
        ""listaComposicaoAtributo"": [
          [
            {
              ""atributo"": ""ATT_1"",
              ""valor"": ""12345678901""
            }
          ]
        ]
      },
      ""unidadeMedida"": ""UN"",
      ""intervenientes"": [
        {
          ""id"": ""12345678901"",
          ""nome"": ""Fulano da Silva"",
          ""endereco"": {
            ""logradouro"": ""Rua das Acácias, 123"",
            ""bairro"": ""Centro"",
            ""municipio"": ""Florianópolis"",
            ""cep"": ""99999-999"",
            ""uf"": ""SC""
          }
        }
      ]
    }
  ],
  ""listaNcm"": [
    {
      ""numeroItem"": 1,
      ""ncm"": ""01012100"",
      ""listaCamposNcm"": [
        {
          ""codigo"": ""CPF_CNPJ_EXPORTADOR"",
          ""listaValor"": [""12345678901"", ""12345678901234""],
          ""valorComposto"": {
            ""indicacaoImportacaoTerceiros"": {
              ""codigoIndicador"": 0,
              ""cpfCnpj"": ""03141554900""
            },
            ""exportadorEstrangeiro"": {
              ""codigo"": ""111222333"",
              ""cpfCnpjRaiz"": ""00055555"",
              ""codigoPais"": ""AR"",
              ""versao"": ""1""
            },
            ""exportadorEFabricanteDoProduto"": {
              ""exportadorIgualFabricante"": true,
              ""operadorEstrangeiro"": {
                ""codigo"": ""111222333"",
                ""cpfCnpjRaiz"": ""00055555"",
                ""codigoPais"": ""AR"",
                ""versao"": ""1""
              }
            },
            ""fabricante"": {
              ""codigoPais"": ""AR"",
              ""conhecido"": true,
              ""cpfCnpj"": ""03141554900"",
              ""operadorEstrangeiro"": {
                ""codigo"": ""111222333"",
                ""cpfCnpjRaiz"": ""00055555"",
                ""codigoPais"": ""AR"",
                ""versao"": ""1""
              }
            },
            ""fundamentoLegal"": {
              ""codigoFundamento"": ""00102030004"",
              ""ncm"": ""01012100"",
              ""camposAdicionais"": [
                {
                  ""codigoAtributo"": ""ATT_2982"",
                  ""valor"": ""32061100""
                }
              ]
            },
            ""listaComposicaoAtributo"": [
              [
                {
                  ""atributo"": ""ATT_1"",
                  ""valor"": ""12345678901""
                }
              ]
            ]
          },
          ""unidadeMedida"": ""UN"",
          ""intervenientes"": [
            {
              ""id"": ""12345678901"",
              ""nome"": ""Fulano da Silva"",
              ""endereco"": {
                ""logradouro"": ""Rua das Acácias, 123"",
                ""bairro"": ""Centro"",
                ""municipio"": ""Florianópolis"",
                ""cep"": ""99999-999"",
                ""uf"": ""SC""
              }
            }
          ]
        }
      ],
      ""listaAtributosNcm"": [
        {
          ""codigo"": ""CPF_CNPJ_EXPORTADOR"",
          ""listaValor"": [""12345678901"", ""12345678901234""],
          ""valorComposto"": {
            ""indicacaoImportacaoTerceiros"": {
              ""codigoIndicador"": 0,
              ""cpfCnpj"": ""03141554900""
            },
            ""exportadorEstrangeiro"": {
              ""codigo"": ""111222333"",
              ""cpfCnpjRaiz"": ""00055555"",
              ""codigoPais"": ""AR"",
              ""versao"": ""1""
            },
            ""exportadorEFabricanteDoProduto"": {
              ""exportadorIgualFabricante"": true,
              ""operadorEstrangeiro"": {
                ""codigo"": ""111222333"",
                ""cpfCnpjRaiz"": ""00055555"",
                ""codigoPais"": ""AR"",
                ""versao"": ""1""
              }
            },
            ""fabricante"": {
              ""codigoPais"": ""AR"",
              ""conhecido"": true,
              ""cpfCnpj"": ""03141554900"",
              ""operadorEstrangeiro"": {
                ""codigo"": ""111222333"",
                ""cpfCnpjRaiz"": ""00055555"",
                ""codigoPais"": ""AR"",
                ""versao"": ""1""
              }
            },
            ""fundamentoLegal"": {
              ""codigoFundamento"": ""00102030004"",
              ""ncm"": ""01012100"",
              ""camposAdicionais"": [
                {
                  ""codigoAtributo"": ""ATT_2982"",
                  ""valor"": ""32061100""
                }
              ]
            },
            ""listaComposicaoAtributo"": [
              [
                {
                  ""atributo"": ""ATT_1"",
                  ""valor"": ""12345678901""
                }
              ]
            ]
          },
          ""unidadeMedida"": ""UN"",
          ""intervenientes"": [
            {
              ""id"": ""12345678901"",
              ""nome"": ""Fulano da Silva"",
              ""endereco"": {
                ""logradouro"": ""Rua das Acácias, 123"",
                ""bairro"": ""Centro"",
                ""municipio"": ""Florianópolis"",
                ""cep"": ""99999-999"",
                ""uf"": ""SC""
              }
            }
          ]
        }
      ],
      ""produto"": {
        ""codigo"": 12,
        ""versao"": ""string"",
        ""cnpjRaiz"": ""33683111""
      },
      ""identificadorCota"": ""I00085-01-01"",
      ""criterioDistribuicaoCota"": ""Performance"",
      ""parcelaDistribuicaoCota"": ""Ordem de registro""
    }
  ],
  ""listaVinculos"": [
    {
      ""dataVinculo"": ""25/11/2020"",
      ""numeroDocumento"": ""19BR0000001234"",
      ""numeroDocumentoItem"": 1,
      ""dataACD"": ""25/11/2020"",
      ""dataDesembaraco"": ""25/11/2020"",
      ""dataCCE"": ""25/11/2020"",
      ""dataAverbacao"": ""25/11/2020"",
      ""dataDesvinculacao"": ""25/11/2020"",
      ""quantidadeComercial"": 100.001122,
      ""quantidadeUnidadeEstatistica"": 100.001122,
      ""vmle"": 100.01
    }
  ],
  ""saldos"": [
    {
      ""titulo"": ""Item 1 - NCM 11223344"",
      ""saldoQuantidadeComercial"": 123.12345,
      ""saldoQuantidadeEstatistica"": 123.12345,
      ""saldoVMLE"": 123.12,
      ""saldoPesoLiquido"": 123.12345,
      ""saldoValorFinanciado"": 123.12,
      ""saldoValorCondicaoVenda"": 123.12,
      ""moedaVmle"": ""USD"",
      ""moedaValorFinanciado"": ""USD"",
      ""moedaValorCondicaoVenda"": ""USD""
    }
  ]
}";
	}
}
