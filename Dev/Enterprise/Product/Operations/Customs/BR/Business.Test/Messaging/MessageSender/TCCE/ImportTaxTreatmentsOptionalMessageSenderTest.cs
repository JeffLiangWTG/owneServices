using System;
using System.Linq;
using CargoWise.Customs.BR.MessageDefinitions.TTCE;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ImportTaxTreatmentsOptionalMessageSenderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Declaration is null", () => new ImportTaxTreatmentsOptionalMessageSender(null, null));
			AssertExceptionThrown<ArgumentException>("RespostaObterTratamentosTributariosImportacaoDTO is null", () => new ImportTaxTreatmentsOptionalMessageSender(Factory.New<JobDeclaration>(), null));
		}

		public void TestSendMessage()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BR1";

			var password = BRGlbStaffWrapper.Get(broker).CCTPassword;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = broker.GS_Code;

			var ttce = BRMessageHelper.DeserializeObject<RespostaObterTratamentosTributariosImportacaoDTO>(ResponseMessageMTT);

			var messageSender = new ImportTaxTreatmentsOptionalMessageSender(declaration, ttce);
			messageSender.SendMessage();
			AssertEquals("Messages count should be", 1, declaration.Messages.Count);

			var message = declaration.Messages.Cast<EDIMessage>().FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.BRCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.RTT, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes, message.EM_MessageSubType);
				AssertEquals("EM_LinkUniqueID", declaration.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", JobDeclarationSchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals("EM_ApplicationReference", ZString.Empty, message.EM_ApplicationReference);
				AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
				AssertEquals("EM_MessageText", ExpectedMessageText, message.EM_MessageText);
				AssertEquals("EM_GP", declaration.BrokerCertificate.PK, message.EM_GP);
				AssertEquals("IsInDatabase", false, message.IsInDatabase);
			});
		}

		const string ExpectedMessageText = @"{
  ""ncm"": ""01010101"",
  ""codigoPais"": 105,
  ""dataFatoGerador"": ""2024-12-11"",
  ""tipoOperacao"": ""I"",
  ""fundamentosOpcionais"": [
    {
      ""codigoTributo"": 1,
      ""codigoRegime"": 1,
      ""codigoFundamentoLegal"": 6
    },
    {
      ""codigoTributo"": 6,
      ""codigoRegime"": 5,
      ""codigoFundamentoLegal"": 908
    },
    {
      ""codigoTributo"": 7,
      ""codigoRegime"": 5,
      ""codigoFundamentoLegal"": 908
    },
    {
      ""codigoTributo"": 2,
      ""codigoRegime"": 1,
      ""codigoFundamentoLegal"": 6996
    }
  ]
}";

		const string ResponseMessageMTT = @"{
    ""ncm"": ""01010101"",
    ""codigoPais"": 105,
    ""dataFatoGerador"": ""2024-12-11"",
    ""tipoOperacao"": ""I"",
    ""tratamentosTributarios"": [
        {
            ""tributo"": {
                ""codigo"": ""1"",
                ""nome"": ""Imposto de Importação""
            },
            ""regime"": {
                ""codigo"": ""1"",
                ""nome"": ""RECOLHIMENTO INTEGRAL""
            },
            ""fundamentoLegal"": {
                ""codigo"": ""0006"",
                ""nome"": ""EX-TARIFÁRIOS TEMPORÁRIOS DE II"",
                ""tipo"": ""Opcional""
            },
            ""mercadorias"": [
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_15574"",
                            ""descricaoCodigo"": ""Ex Tarifário II"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0007"",
                            ""descricaoValor"": ""Rotores turbo-fan para bombeamento de ar através de sucção central e descarga em fluxo radial, disposto em pás aerodinâmicas com torção tridimensional, conformado através de injeção de precisão das partes (rotor-turbo + anel flange) e unidos através do processo de solda a laser, executada em atmosfera classificada com controle de partículas em suspensão, controle de humidade e controle de temperatura, para uso em unidades evaporadoras (indoor unit) de sistemas de ar condicionado com expansão direta de alta eficiência.""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_15574"",
                            ""descricaoCodigo"": ""Ex Tarifário II"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0062"",
                            ""descricaoValor"": ""Rotores completos dotados de eixo principal e quatro impelidores, para compressão do ar atmosférico do compressor centrífugo cuja vazão nominal é de 10Nm³/h, pressão de entrada 0,972barA e pressão de saída 7barA.""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_15574"",
                            ""descricaoCodigo"": ""Ex Tarifário II"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0063"",
                            ""descricaoValor"": ""Difusores de 1° estágio para redução da velocidade do ar atmosférico do compressor centrífugo cuja vazão nominal é de 10nm³/h, pressão de entrada 0,972barA e pressão de saída 7barA.""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_15574"",
                            ""descricaoCodigo"": ""Ex Tarifário II"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0064"",
                            ""descricaoValor"": ""Difusores do 1º estádio para redução da velocidade do ar atmosférico do compressor centrifugo cuja vazão nominal é de 350.000Nm3/h, pressão de entrada 0,972bara e pressão de saída 7bara.""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_15574"",
                            ""descricaoCodigo"": ""Ex Tarifário II"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0065"",
                            ""descricaoValor"": ""Rotores completos dotados de um eixo principal e 4 impelidores, para compressão do ar atmosférico do compressor centrifugo cuja vazão nominal é de 350.000Nm3/h, pressão de entrada 0,972bara e pressão de saída 7bara.""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_15574"",
                            ""descricaoCodigo"": ""Ex Tarifário II"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0085"",
                            ""descricaoValor"": ""Cruzetas para acionamento de êmbolos, com no mínimo 2,84m de comprimento e 0,90m de largura, para uso em hiper compressores com máxima pressão de operação de 269MPa, usadas na produção de polietileno de baixa densidade, com estrutura em aço fundido, pino cilíndrico, tirantes, porcas e sapatas deslizantes.""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_15574"",
                            ""descricaoCodigo"": ""Ex Tarifário II"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0086"",
                            ""descricaoValor"": ""Conjuntos rotativos utilizados para compor as carcaças de compressores de hidrogênio de primeiro ou segundo estágio, com a função de compressão do gás de pureza de no mínimo de 99%, dotados de um rotor macho com diâmetro entre 163 e 204mm, com 4 lóbulos; e de um rotor fêmea com diâmetro entre 163 e 204mm, com 6 lóbulos; fabricados em aço forjado de baixa liga Cr-Mo, com tratamento térmico, para uso exclusivo em compressores do tipo parafuso, acionados por motor elétrico com potência nominal de 1.410kW.""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_15574"",
                            ""descricaoCodigo"": ""Ex Tarifário II"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0093"",
                            ""descricaoValor"": ""Carcaças para compressor de grande porte de fluxo axial para altos fornos, fabricadas em aço forjado, contendo sistema com 614 palhetas em aço inox JIS SUS403 aerodinâmicas móveis de ângulo variável, dispostas em 12 estágios, que permitam controle da vazão de ar de até 6.400Nm³/h, à pressão de 4,2kgf/cm², com taxa de compressão de 5,2.""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_15574"",
                            ""descricaoCodigo"": ""Ex Tarifário II"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0094"",
                            ""descricaoValor"": ""Rotores para compressor de grande porte de fluxo axial para altos fornos, fabricado em Aço NiCrMo forjado, composto por eixo, acoplamentos e 638 palhetas em aço inox JIS SUS403 aerodinâmicas, dispostas em 12 estágios, que gira a 3.757rpm, permitindo fornecimento de ar soprado a uma vazão de até 6.400Nm³/h, à pressão de 4,2kgf/cm² com taxa de compressão de 5,2.""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_15574"",
                            ""descricaoCodigo"": ""Ex Tarifário II"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0095"",
                            ""descricaoValor"": ""Rotores dotados de eixo principal e 5 impelidores, para compressão de propeno, para uso exclusivo em compressor centrífugo com vazão nominal de 59.118nm³/h, pressão de sucção nominal de 1,599bara e pressão de descarga nominal de 17,535bara.""
                        }
                    ]
                }
            ]
        },
        {
            ""tributo"": {
                ""codigo"": ""6"",
                ""nome"": ""PIS Importação""
            },
            ""regime"": {
                ""codigo"": ""1"",
                ""nome"": ""RECOLHIMENTO INTEGRAL""
            },
            ""fundamentoLegal"": {
                ""codigo"": ""1100"",
                ""nome"": ""PIS-IMPORTAÇÃO e COFINS-IMPORTAÇÃO"",
                ""tipo"": ""Normal""
            },
            ""mercadorias"": [
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_13741"",
                            ""descricaoCodigo"": ""Uso autopeças"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0002"",
                            ""descricaoValor"": ""Importador NÃO é pessoa jurídica fabricante de máquinas e veículos relacionados no Anexo II do Art. 1º da Lei 10.485/2002. (§9º, Art. 8º. Lei 10865/2004)\n""
                        },
                        {
                            ""codigo"": ""ATT_13715"",
                            ""descricaoCodigo"": ""Anexo II da Lei 10.485/2002"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0001"",
                            ""descricaoValor"": ""Se enquadra na descrição do Anexo II da Lei 10.485/2002""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_13741"",
                            ""descricaoCodigo"": ""Uso autopeças"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0002"",
                            ""descricaoValor"": ""Importador NÃO é pessoa jurídica fabricante de máquinas e veículos relacionados no Anexo II do Art. 1º da Lei 10.485/2002. (§9º, Art. 8º. Lei 10865/2004)\n""
                        },
                        {
                            ""codigo"": ""ATT_13715"",
                            ""descricaoCodigo"": ""Anexo II da Lei 10.485/2002"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""XXXX"",
                            ""descricaoValor"": ""Não se enquadra em outra opção""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_13741"",
                            ""descricaoCodigo"": ""Uso autopeças"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""XXXX"",
                            ""descricaoValor"": ""Não se enquadra em outra opção""
                        }
                    ]
                }
            ]
        },
        {
            ""tributo"": {
                ""codigo"": ""7"",
                ""nome"": ""Cofins Importação""
            },
            ""regime"": {
                ""codigo"": ""1"",
                ""nome"": ""RECOLHIMENTO INTEGRAL""
            },
            ""fundamentoLegal"": {
                ""codigo"": ""1100"",
                ""nome"": ""PIS-IMPORTAÇÃO e COFINS-IMPORTAÇÃO"",
                ""tipo"": ""Normal""
            },
            ""mercadorias"": [
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_13741"",
                            ""descricaoCodigo"": ""Uso autopeças"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0002"",
                            ""descricaoValor"": ""Importador NÃO é pessoa jurídica fabricante de máquinas e veículos relacionados no Anexo II do Art. 1º da Lei 10.485/2002. (§9º, Art. 8º. Lei 10865/2004)\n""
                        },
                        {
                            ""codigo"": ""ATT_13715"",
                            ""descricaoCodigo"": ""Anexo II da Lei 10.485/2002"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0001"",
                            ""descricaoValor"": ""Se enquadra na descrição do Anexo II da Lei 10.485/2002""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_13741"",
                            ""descricaoCodigo"": ""Uso autopeças"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""0002"",
                            ""descricaoValor"": ""Importador NÃO é pessoa jurídica fabricante de máquinas e veículos relacionados no Anexo II do Art. 1º da Lei 10.485/2002. (§9º, Art. 8º. Lei 10865/2004)\n""
                        },
                        {
                            ""codigo"": ""ATT_13715"",
                            ""descricaoCodigo"": ""Anexo II da Lei 10.485/2002"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""XXXX"",
                            ""descricaoValor"": ""Não se enquadra em outra opção""
                        }
                    ]
                },
                {
                    ""atributos"": [
                        {
                            ""codigo"": ""ATT_13741"",
                            ""descricaoCodigo"": ""Uso autopeças"",
                            ""tipoCodigo"": ""Domínio dinâmico"",
                            ""valor"": ""XXXX"",
                            ""descricaoValor"": ""Não se enquadra em outra opção""
                        }
                    ]
                }
            ]
        }
    ],
    ""fundamentosOpcionaisDisponiveis"": [
		{
			""tributo"": {
					""codigo"":""1"",
					""nome"":""Imposto de Importação""
			},
			""regime"":	{
				""codigo"":""1"",
				""nome"":""RECOLHIMENTO INTEGRAL""
			},
			""fundamentoLegal"": {
				""codigo"":""0006"",
				""nome"":""EX-TARIFÁRIOS TEMPORÁRIOS DE II"",
				""tipo"":""Opcional""
			}
		},
		{
			""tributo"": {
				""codigo"":""6"",
				""nome"":""PIS Importação""
			},
			""regime"": {
				""codigo"":""5"",
				""nome"":""SUSPENSAO""
			},
			""fundamentoLegal"": {
				""codigo"":""0908"",
				""nome"":""ADMISSÃO NO GNL-TEMPORÁRIO "",
				""tipo"":""Opcional""
			}
		},
		{
			""tributo"": {
				""codigo"":""7"",
				""nome"":""Cofins Importação""
			},
			""regime"":	{
				""codigo"":""5"",
				""nome"":""SUSPENSAO""
			},
			""fundamentoLegal"": {
				""codigo"":""0908"",
				""nome"":""ADMISSÃO NO GNL-TEMPORÁRIO "",
				""tipo"":""Opcional""
			}
		},
		{
			""tributo"": {
				""codigo"":""2"",
				""nome"":""IPI""
			},
			""regime"":	{
				""codigo"":""1"",
				""nome"":""RECOLHIMENTO INTEGRAL""
			},
			""fundamentoLegal"":
			{
				""codigo"":""6996"",
				""nome"":""NOTA COMPLEMENTAR DA TIPI"",
				""tipo"":""Opcional""
			}
		}
]}";
	}
}

