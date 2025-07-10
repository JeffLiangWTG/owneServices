using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCImportLicenseStatusMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCImportLicenseStatusMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "LIS" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => Array.Empty<string>();

		public void TestMessageTypesToInclude()
		{
			var processor = new BRCImportLicenseStatusMessageProcessor(new LoggingInformation());
			var messageTypesToInclude = processor.MessageTypesToInclude;
			AssertEquals(1, messageTypesToInclude.Count);
			AssertEquals(MessageTypeList.Codes.LIS, messageTypesToInclude[0]);
		}

		public void TestProcessResponseMessage_EntryNotFound()
		{
			var dateTime = ZDateTime.Now.ToSmallDateTime();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.LIC;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.LIS, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = GetMessageBody("17", dateTime.ToString(Constants.DataFormat), dateTime.ToString(Constants.DataFormat), dateTime.ToString(Constants.DataFormat), dateTime.ToString("HH:mm:ss"));

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertContains("Logger", "Error: \tUnable to find an Entry with Entry Number '2210703648' for LIS message #1\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_LogErrorWithDeserializationFailedAndException()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.LIC;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.LIS, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = "aaaaaaaa";
			AssertExceptionThrown<InvalidOperationException>(() => { ExecuteMessageProcessor(responseMessage); });
		}

		[TestDate(2023, 1, 1)]
		public void TestProcessResponseMessage_TestFindLinkedObject()
		{
			ReferenceTestDataHelper.CreateEntryStatusForLicenseAndExportList(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("2210703648");
			entry.CH_MessageType = MessageTypeList.Codes.LIC;
			Factory.Save();

			var dateTime = new ZDateTime(2022, 5, 6, 9, 10, 11);
			AssertCH_EntryReleaseDate("05", dateTime);
			AssertCH_EntryReleaseDate("06", dateTime.AddMinutes(1));
			AssertCH_EntryReleaseDate("07", dateTime.AddMinutes(2));
			AssertCH_EntryReleaseDate("08", dateTime.AddMinutes(3));
			AssertCH_EntryReleaseDate("10", dateTime.AddMinutes(4));
			AssertCH_EntryReleaseDate("17", null, ZDateTime.Empty, ZDateTime.Now);

			void AssertCH_EntryReleaseDate(string statusCode, ZDateTime? releaseDate, ZDateTime? expectedReleaseDate = null, ZDateTime? expectedEventTime = null)
			{
				entry.CH_EntryStatus = ZString.Empty;
				entry.CH_ValidityILShipmentDate = ZDateTime.Empty;
				entry.CH_ValidityILDispatchDate = ZDateTime.Empty;
				entry.CH_EntryReleaseDate = ZDateTime.Empty;

				var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.LIS, ZString.Empty).ResponseMessage;
				responseMessage.EM_MessageText = GetMessageBody(statusCode, "02/01/2023", "03/01/2023",
					releaseDate?.ToString(Constants.DataFormat) ?? ZString.Empty, releaseDate?.ToString("HH:mm:ss") ?? ZString.Empty);

				ExecuteMessageProcessor(responseMessage);
				CombineAssertions($"Status Code = {statusCode}", () =>
				{
					AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
					AssertEquals("EM_Status must be equal to", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
					AssertEquals("EM_LinkTable must be equal to", "CusEntryHeader", responseMessage.EM_LinkTable);
					AssertEquals("EM_LinkUniqueID must be equal to", entry.PK, responseMessage.EM_LinkUniqueID);

					AssertEquals("CH_EntryStatus must be equal to", $"L{statusCode}", entry.CH_EntryStatus);
					AssertEquals("EM_MessageInterpretation must be equal to", GetExpectedHtml(EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference)), responseMessage.EM_MessageInterpretation);
					AssertEquals("CH_ValidityILShipmentDate must be equal to", new ZDateTime(2023, 1, 2), entry.CH_ValidityILShipmentDate);
					AssertEquals("CH_ValidityILDispatchDate must be equal to", new ZDateTime(2023, 1, 3), entry.CH_ValidityILDispatchDate);
					AssertEquals("CH_EntryReleaseDate must be equal to", expectedReleaseDate ?? releaseDate, entry.CH_EntryReleaseDate);

					var log = entry.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
					AssertEquals("CES Log Reference", $"L{statusCode}", log.SL_Reference);
					AssertEquals("CES Log Event Time", expectedEventTime ?? releaseDate, log.SL_EventTime);
				});
			}
		}

		[TestDate(2023, 1, 1)]
		public void TestProcessResponseMessage_TestEmptyFields()
		{
			var dateTime = ZDateTime.Now.ToSmallDateTime();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("2210703648");
			entry.CH_MessageType = MessageTypeList.Codes.LIC;
			entry.CH_EntryStatus = "L17";
			entry.CH_ValidityILShipmentDate = ZDateTime.Today;
			entry.CH_ValidityILDispatchDate = ZDateTime.Today;
			entry.CH_EntryReleaseDate = dateTime;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.LIS, ZString.Empty).ResponseMessage;
			responseMessage.EM_MessageText = GetMessageBody(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("CH_EntryStatus should not be changed", "L17", entry.CH_EntryStatus);
				AssertEquals("CH_ValidityILShipmentDate should not be changed", ZDateTime.Today, entry.CH_ValidityILShipmentDate);
				AssertEquals("CH_ValidityILDispatchDate should not be changed", ZDateTime.Today, entry.CH_ValidityILDispatchDate);
				AssertEquals("CH_EntryReleaseDate should not be changed", dateTime, entry.CH_EntryReleaseDate);
			});
		}

		[TestDate(2023, 1, 1)]
		public void TestProcessResponseMessage_TestInvalidFields()
		{
			var dateTime = ZDateTime.Now.ToSmallDateTime();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("2210703648");
			entry.CH_MessageType = MessageTypeList.Codes.LIC;
			entry.CH_EntryStatus = "L17";
			entry.CH_ValidityILShipmentDate = ZDateTime.Today;
			entry.CH_ValidityILDispatchDate = ZDateTime.Today;
			entry.CH_EntryReleaseDate = dateTime;
			Factory.Save();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.LIS, ZString.Empty).ResponseMessage;
			responseMessage.EM_MessageText = GetMessageBody("H05", "31/02/2022", "31/02/2022", "31/02/2022", "14:25:40");

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("CH_EntryStatus should not be changed", "L17", entry.CH_EntryStatus);
				AssertEquals("CH_ValidityILShipmentDate should not be changed", ZDateTime.Today, entry.CH_ValidityILShipmentDate);
				AssertEquals("CH_ValidityILDispatchDate should not be changed", ZDateTime.Today, entry.CH_ValidityILDispatchDate);
				AssertEquals("CH_EntryReleaseDate should not be changed", dateTime, entry.CH_EntryReleaseDate);
			});
		}

		#region Implementation

		public static string GetMessageBody(string statusCode, string dtValidityShipment, string dtValidityDispatch, string dtReleaseDate, string dtReleaseHour) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<li-completa-type>
	<Grupo-Dados-Basicos xmlns = ""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html"" >
		<numero-li>22/1070364-8</numero-li>
		<Importador>
			<importador-tipo>1</importador-tipo>
			<importador-identificador > 08.264.406/0001-93</importador-identificador>
			<importador-nome>GARANTE DISTRIBUIDORA E IMPORTADORA DE PRODUTOS ALIMENT</importador-nome>
			<importador-atividade-economica />
			<importador-natureza-juridica>SOCIEDADE POR COTAS DE RESPONS.LIMITADA-EMPRESA PRIVADA</importador-natureza-juridica>
			<importador-endereco-logradouro>EST DO GANCHINHO- LD</importador-endereco-logradouro>
			<importador-endereco-numero>690</importador-endereco-numero>
			<importador-endereco-complemento />
			<importador-endereco-bairro>UMBARA</importador-endereco-bairro>
			<importador-endereco-cidade>CURITIBA</importador-endereco-cidade>
			<importador-endereco-uf>PR</importador-endereco-uf>
			<importador-endereco-cep>81930165</importador-endereco-cep>
			<importador-telefone>41 - 38882000</importador-telefone>
			<importador-pais />
			<importador-pais-nome />
		</Importador>
		<Outras-Informacoes>
			<pais-procedencia-mercadoria>158</pais-procedencia-mercadoria>
			<pais-procedencia-mercadoria-nome>CHILE</pais-procedencia-mercadoria-nome>
			<urf-entrada>1017500</urf-entrada>
			<urf-entrada-nome>ALF - URUGUAIANA</urf-entrada-nome>
			<urf-despacho>0917900</urf-despacho>
			<urf-despacho-nome>ALF - CURITIBA</urf-despacho-nome>
		</Outras-Informacoes>
		<Informacoes-Complementares>
			<texto-informacoes-complementares>DIR00089/22 - INVOICE: 676 - MASTERSUL COMEX LTDA (41) 3024-0100                                                                                                                                                                                             </texto-informacoes-complementares>
		</Informacoes-Complementares>
	</Grupo-Dados-Basicos>
	<Grupo-Fornecedor xmlns = ""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html"" >
		<fornecedor-tipo>1</fornecedor-tipo>
		<pais-aquisicao-mercadoria>158</pais-aquisicao-mercadoria>
		<pais-aquisicao-mercadoria-nome>CHILE</pais-aquisicao-mercadoria-nome>
		<pais-origem-mercadoria>158</pais-origem-mercadoria>
		<pais-origem-mercadoria-nome>CHILE</pais-origem-mercadoria-nome>
		<Fornecedor>
			<fornecedor-estrangeiro-nome> TERRAUSTRAL S.A</fornecedor-estrangeiro-nome>
			<fornecedor-estrangeiro-email/>
			<fornecedor-estrangeiro-responsavel/>
			<fornecedor-estrangeiro-logradouro>HERNANDO DE AGUIRRE</fornecedor-estrangeiro-logradouro>
			<fornecedor-estrangeiro-numero>1915</fornecedor-estrangeiro-numero>
			<fornecedor-estrangeiro-complemento />
			<fornecedor-estrangeiro-cidade>SANTIAGO</fornecedor-estrangeiro-cidade>
			<fornecedor-estrangeiro-uf>EXTERIOR</fornecedor-estrangeiro-uf>
		</Fornecedor>
		<Fabricante>
			<fabricante-nome />
			<fabricante-email />
			<fabricante-responsavel />
			<fabricante-endereco-logradoudo />
			<fabricante-endereco-numero />
			<fabricante-endereco-complemento />
			<fabricante-endereco-cidade />
			<fabricante-endereco-estado />
		</Fabricante>
	</Grupo-Fornecedor>
	<Grupo-Mercadoria xmlns = ""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html"" >
		<Dados-Gerais>
			<subitem-ncm>2204.21.00</subitem-ncm>
			<subitem-ncm-nome>-- Em recipientes de capacidade não superior a 2 l</subitem-ncm-nome>
			<unidade-medida-estatistica>LITRO</unidade-medida-estatistica>
			<mercadoria-naladi>22042110</mercadoria-naladi>
			<mercadoria-naladi-nome />
			<moeda>220</moeda>
			<moeda-nome>DOLAR DOS EUA</moeda-nome>
			<incoterm>FCA</incoterm>
			<incoterm-nome>FCA - FREE CARRIER</incoterm-nome>
		</Dados-Gerais>
		<Condicao-Mercadoria>
			<condicao-mercadoria>N</condicao-mercadoria>
			<condicao-mercadoria-nome>Nenhuma</condicao-mercadoria-nome>
			<tipo-enquadramento-material-usado />
			<tipo-enquadramento-material-usado-nome />
			<tipo-operacao-enquadramento-material-usado />
			<tipo-operacao-enquadramento-material-usado-nome />
		</Condicao-Mercadoria>
		<lista-destaque-ncm />
		<lista-processo-anuente />
		<Informacoes-Drawback>
			<drawback-regime>3</drawback-regime>
			<drawback-numero-ato-isencao />
			<drawback-numero-ato-suspencao />
		</Informacoes-Drawback>
		<lista-detalhe-ncm>
			<detalhe-ncm-item-drawback>
				<numero-sequencial-produto>1</numero-sequencial-produto>
				<nome-unidade-medida-comercializada>CAIXAS</nome-unidade-medida-comercializada>
				<peso-liquido-total>5.625,00000</peso-liquido-total>
				<qtd-mercadoria-unidade-comercializada>1.250,00000</qtd-mercadoria-unidade-comercializada>
				<qtd-mercadoria-unidade-estatistica>5.625,00000</qtd-mercadoria-unidade-estatistica>
				<valor-total-local-embarque>20.000,0000000</valor-total-local-embarque>
				<valor-unitario-condicao-venda>16,0000000</valor-unitario-condicao-venda>
				<valor-total-condicao-venda>20.000,0000000</valor-total-condicao-venda>
				<descricao-produto>1250 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO ROSADO MEIO SECO, ROSE (50% SYRAH - 50% CABERNET SAUVIGNON), MARCA: QUE BELLA RESERVE, SAFRA: 2021, TEOR ALC.: 13,1, LOTE: L-22071, INDICACAO GEOGRAFICA: VALLE CENTRAL</descricao-produto>
				<marca />
				<modelo />
				<numero-serie />
				<ano-fabricacao />
				<item-ac-drawback />
			</detalhe-ncm-item-drawback>
			<detalhe-ncm-item-drawback>
				<numero-sequencial-produto>2</numero-sequencial-produto>
				<nome-unidade-medida-comercializada>CAIXAS</nome-unidade-medida-comercializada>
				<peso-liquido-total>4.410,00000</peso-liquido-total>
				<qtd-mercadoria-unidade-comercializada>980,00000</qtd-mercadoria-unidade-comercializada>
				<qtd-mercadoria-unidade-estatistica>4.410,00000</qtd-mercadoria-unidade-estatistica>
				<valor-total-local-embarque>11.760,0000000</valor-total-local-embarque>
				<valor-unitario-condicao-venda>12,0000000</valor-unitario-condicao-venda>
				<valor-total-condicao-venda>11.760,0000000</valor-total-condicao-venda>
				<descricao-produto>980 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CARMENERE, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,6, LOTE: L-22073, INDICACAO GEOGRAFICA: VALLE CENTRAL</descricao-produto>
				<marca />
				<modelo />
				<numero-serie />
				<ano-fabricacao />
				<item-ac-drawback />
			</detalhe-ncm-item-drawback>
			<detalhe-ncm-item-drawback>
				<numero-sequencial-produto>3</numero-sequencial-produto>
				<nome-unidade-medida-comercializada>CAIXAS</nome-unidade-medida-comercializada>
				<peso-liquido-total>3.780,00000</peso-liquido-total>
				<qtd-mercadoria-unidade-comercializada>840,00000</qtd-mercadoria-unidade-comercializada>
				<qtd-mercadoria-unidade-estatistica>3.780,00000</qtd-mercadoria-unidade-estatistica>
				<valor-total-local-embarque>10.080,0000000</valor-total-local-embarque>
				<valor-unitario-condicao-venda>12,0000000</valor-unitario-condicao-venda>
				<valor-total-condicao-venda>10.080,0000000</valor-total-condicao-venda>
				<descricao-produto>840 CAIXAS CONTENDO 6 GARRAFAS DE 750 ML DE VINHO FINO TINTO MEIO SECO, CABERNET SAUVIGNON, MARCA: SUNTANA, SAFRA: 2021, TEOR ALC.: 12,7, LOTE: L-22074, INDICACAO GEOGRAFICA: VALLE CENTRAL</descricao-produto>
				<marca />
				<modelo />
				<numero-serie />
				<ano-fabricacao />
				<item-ac-drawback />
			</detalhe-ncm-item-drawback>
		</lista-detalhe-ncm>
		<Totalizadores>
			<quantidade-total-medida-estatistica>13.815,00000</quantidade-total-medida-estatistica>
			<peso-liquido-total-kg>13.815,00000</peso-liquido-total-kg>
			<valor-total-local-embarque>41.840,0000000</valor-total-local-embarque>
			<valor-total-condicao-venda>41.840,0000000</valor-total-condicao-venda>
		</Totalizadores>
	</Grupo-Mercadoria>
	<Grupo-Negociacao xmlns = ""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html"" >
		<regime-acordo-tributario>1</regime-acordo-tributario>
		<regime-acordo-tributario-nome> RECOLHIMENTO INTEGRAL</regime-acordo-tributario-nome>
		<fundamento-legal-regime />
		<fundamento-legal-regime-nome />
		<tipo-acordo-tarifario>2</tipo-acordo-tarifario>
		<tipo-acordo-tarifario-nome>ALADI</tipo-acordo-tarifario-nome>
		<codigo-acordo-aladi>335</codigo-acordo-aladi>
		<codigo-acordo-aladi-nome>ACORDO DE COMPLEMENTACAO ECONOMICA N. 35 - MERCOSUL/CHILE</codigo-acordo-aladi-nome>
		<cobertura-cambial>1</cobertura-cambial>
		<cobertura-cambial-nome>COM COBERTURA CAMBIAL E PAGAMENTO FINAL A PRAZO DE ATE
			180</cobertura-cambial-nome>
		<modalidade-pagamento>31</modalidade-pagamento>
		<modalidade-pagamento-nome>FINANCIAMENTO DO FORNECEDOR (SUPPLIER
			S CREDIT) - OUTROS</modalidade-pagamento-nome>
		<numero-dias-limite-pagamento />
		<codigo-orgao-financeiro-internacional />
		<codigo-orgao-financeiro-internacional-nome />
		<codigo-motivo-sem-cobertura />
		<codigo-motivo-sem-cobertura-nome />
	</Grupo-Negociacao>
	<Grupo-LI-Anuencias xmlns = ""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html"" >
		<Informacoes-LI>
			<data-registro>25/04/2022</data-registro>
			<hora-registro>16:22</hora-registro>
			<data-situacao>{dtReleaseDate}</data-situacao>
			<hora-situacao>{dtReleaseHour}</hora-situacao>
			<codigo-situacao>{statusCode}</codigo-situacao>
			<nome-situacao>DESEMBARACADA</nome-situacao>
			<data-restricao-embarque />
			<data-validade-embarque>{dtValidityShipment}</data-validade-embarque>
			<data-validade-despacho>{dtValidityDispatch}</data-validade-despacho>
			<numero-li-substituida />
			<numero-li-substitutiva />
		</Informacoes-LI>
		<Informacoes-LI-Vinculada-DI>
			<declaracao-vinculada>2209102544</declaracao-vinculada>
			<adicao-vinculada>001</adicao-vinculada>
			<retificacao />
		</Informacoes-LI-Vinculada-DI>
		<Informacoes-Cancelamento-Vencimento-LI>
			<motivo />
			<cpf-importador-efetuou-cancelamento />
			<data-cancelamento-vencimento />
			<hora-cancelamento-vencimento />
		</Informacoes-Cancelamento-Vencimento-LI>
		<lista-anuencias>
			<anuencia>
				<orgao-anuente>MAPA</orgao-anuente>
				<codigo-situacao-anuencia>05</codigo-situacao-anuencia>
				<nome-situacao-anuencia>DEFERIDA</nome-situacao-anuencia>
				<data-diagnostico-anuencia>13/05/2022</data-diagnostico-anuencia>
				<hora-diagnostico-anuencia>14:25</hora-diagnostico-anuencia>
				<data-restricao-embarque />
				<data-validade-embarque>11/08/2022</data-validade-embarque>
				<data-validade-despacho>09/11/2022</data-validade-despacho>
				<codigo-tratamento-administrativo>01</codigo-tratamento-administrativo>
				<nome-tratamento-administrativo>MERCADORIA</nome-tratamento-administrativo>
				<texto-anuente>Adotado procedimento simplificado para itens 2 e 3 e completo para item 1, conforme IN 67/2018 MAPA.Despacho autorizado.                                                                            </texto-anuente>
			</anuencia>
		</lista-anuencias>
	</Grupo-LI-Anuencias>
</li-completa-type>";

		public static string GetExpectedHtml(string link) => @"<br />
<strong>Job Number: LINK<br/>Import License Number: 22/1070364-8<br/>Import License Status: DESEMBARACADA<br/></strong><br />
<br />
A response message has been received from Brazilian Customs.<br/>The message sent for the above mentioned job has received the following update(s):<br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>Consenting Body</th><th>Status</th><th>Diagnosis Date</th><th>Shipment Restricted Until</th><th>Shipment Valid Until</th><th>Dispatch Valid Until</th><th>Administrative Status</th><th>Consenting Body Statements</th></tr></thead><tr><td>MAPA</td><td>05 - DEFERIDA</td><td>2022-05-13 14:25</td><td>&nbsp;</td><td>2022-08-11</td><td>2022-11-09</td><td>01 - MERCADORIA</td><td>Adotado procedimento simplificado para itens 2 e 3 e completo para item 1, conforme IN 67/2018 MAPA.Despacho autorizado.                                                                            </td></tr></table><br/>
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
<br />
".Replace("LINK", link);

		#endregion
	}
}

