using System.Linq;
using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCImportLicenseAcceptInboundMessageCreatorTest : BRCInboundMessageCreatorAbstractTest
	{
		protected override ZString InterchangeType => MessageTypeList.Codes.LIC;

		protected override ZString TransportType => ZString.Empty;

		public void TestGenerateMessageFromInterchange()
		{
			var liMessageText1 = @"<li>
												<dtRegistro>07/04/2022</dtRegistro>
												<idSolicitacao>BXI000010021</idSolicitacao>
												<importador>
													<numero>58500398000105</numero>
													<tipoImportador>J</tipoImportador>
												</importador>
												<mensagemDiagnostico>
													<mensagemDiagnostico>ERRO NA COMUNICAÇÃO COM O DRAWBACK ISENÇÃO</mensagemDiagnostico>
												</mensagemDiagnostico>
												<numeroLI/>
											</li>";

			var liMessageText2 = @"<li>
												<dtRegistro>07/04/2022</dtRegistro>
												<idSolicitacao>BXI000010022</idSolicitacao>
												<importador>
													<numero>58500398000105</numero>
													<tipoImportador>J</tipoImportador>
												</importador>
												<mensagemDiagnostico>
													<mensagemDiagnostico>ERRO NA COMUNICAÇÃO COM O DRAWBACK ISENÇÃO</mensagemDiagnostico>
												</mensagemDiagnostico>
												<numeroLI/>
											</li>";

			var messageBody = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
									<lote-li>
										<cpfUsuario>000.000.000-01</cpfUsuario>
										<dataHoraEnvioFormatada>07/04/2022 09:00:00</dataHoraEnvioFormatada>
										<idLote>XXXXXXXX000001</idLote>
										<listaLIVORetorno>
											{liMessageText1}
											{liMessageText2}
										</listaLIVORetorno>
										<versao/>
										<versaoValida>true</versaoValida>
									</lote-li>";

			var messageBody1 = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
									<lote-li>
										<cpfUsuario>000.000.000-01</cpfUsuario>
										<dataHoraEnvioFormatada>07/04/2022 09:00:00</dataHoraEnvioFormatada>
										<idLote>000001</idLote>
										<listaLIVORetorno>
											{liMessageText1}
										</listaLIVORetorno>
										<versao/>
										<versaoValida>true</versaoValida>
									</lote-li>";

			var messageBody2 = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
									<lote-li>
										<cpfUsuario>000.000.000-01</cpfUsuario>
										<dataHoraEnvioFormatada>07/04/2022 09:00:00</dataHoraEnvioFormatada>
										<idLote>000001</idLote>
										<listaLIVORetorno>
											{liMessageText2}
										</listaLIVORetorno>
										<versao/>
										<versaoValida>true</versaoValida>
									</lote-li>";

			var expectedMessageText1 = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(XmlObjectSerializer.Deserialize<loteli>(messageBody1));
			var expectedMessageText2 = XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(XmlObjectSerializer.Deserialize<loteli>(messageBody2));

			var interchange = ProcessEDIInterchange(messageBody);

			AssertEquals("Interchange status should be set to Received", EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			AssertEquals("Should have been 2 message extracted from interchange", 2, interchange.ContainedMessages.Count);

			AssertEDIMessageCreated(interchange.ContainedMessages[0], expectedMessageText1, messageNum: "1");
			AssertEDIMessageCreated(interchange.ContainedMessages[1], expectedMessageText2, messageNum: "2");
		}

		public void TestProcessEDIInterchangeInvalidMessage()
		{
			var interchange = ProcessEDIInterchange("aaaaaaaa");
			AssertEquals("Interchange status should be set to Failed", EDIInterchangeStatusList.Codes.Failed, interchange.EI_Status);
			AssertEquals("Logger", "Error - There is an error in XML document (1, 1).", logger.Logs.ElementAt(0).ToString());
		}
	}
}
