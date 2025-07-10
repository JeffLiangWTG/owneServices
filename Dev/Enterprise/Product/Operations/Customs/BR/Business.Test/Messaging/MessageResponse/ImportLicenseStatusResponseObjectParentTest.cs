using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseStatusResponseObjectParent))]
	class ImportLicenseStatusResponseObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportLicenseStatusResponseObjectParent(Factory.New<JobDeclaration>());
		}

		#endregion

		public void TestHumanReadableName()
		{
			var importLicenseParent = GetNewBusinessObject() as ImportLicenseStatusResponseObjectParent;
			AssertEquals("Update Import License Status", importLicenseParent.HumanReadableName);
		}

		public void TestResponseHasDiagnosis()
		{
			var importLicenseParent = GetNewBusinessObject() as ImportLicenseStatusResponseObjectParent;
			AssertEquals(false, importLicenseParent.ResponseHasDiagnosis);
		}

		public void TestResponseHasReferenceNumber()
		{
			var importLicenseParent = GetNewBusinessObject() as ImportLicenseStatusResponseObjectParent;
			AssertEquals(false, importLicenseParent.ResponseHasReferenceNumber);
		}

		public void TestResponseHasStatus()
		{
			var importLicenseParent = GetNewBusinessObject() as ImportLicenseStatusResponseObjectParent;
			AssertEquals(true, importLicenseParent.ResponseHasStatus);
		}

		public void TestImportLicenseResponseObjects()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var importLicenseParent = new ImportLicenseStatusResponseObjectParent(declaration);
			AssertNotNull("ImportLicenseResponseObjects should not be null.", importLicenseParent.Collection);
		}

		public void TestLoadValidXML()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "00000000000001";
			declaration.JE_OH_Importer = importer.PK;

			var message = Factory.NewWithValidTestData<BREDIMessage>();
			message.EM_ApplicationCode = ApplicationCodes.BRCustoms;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			message.EM_ApplicationReference = "RLI00000000001000001";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			entry.MovementReferenceNumberSetter("2210703648");

			message.EM_LinkedObject = entry;
			entry.Messages.Add(message);

			Factory.Save();

			using (var xml = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
			{
				var messageBuilder = new ZStringBuilder();
				var importLicenseParent = new ImportLicenseStatusResponseObjectParent(declaration);
				importLicenseParent.AddLog = AppendLog(messageBuilder);
				importLicenseParent.LoadAndValidateXML("Response.xml", xml);

				AssertEquals("Create EDIInterchange result should be", true, importLicenseParent.CreateDataFromXml());
				AssertEquals("(100/100) File loading complete!(0/1) Response Message 1 processed.", messageBuilder.ToString());

				var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_InterchangeType, MessageTypeList.Codes.LIS));

				AssertEquals("EDIMessages created", 1, interchange.ContainedMessages.Count);

				CombineAssertions(() =>
				{
					AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.BRCustoms, interchange.EI_ApplicationCode);
					AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
					AssertEquals("EI_From", BREDIInterchange.BRCustoms, interchange.EI_From);
					AssertEquals("EI_To", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
					AssertEquals("EI_Status", EDIMessageStatusList.Codes.Received, interchange.EI_Status);
					AssertEquals("EI_GB", GlbBranch.CurrentBranch.PK, interchange.EI_GB);
					AssertEquals("EI_BodyText", responseXML, interchange.EI_BodyText);

					var interchangeMessage = interchange.ContainedMessages[0];

					AssertEquals("EM_Status must be equal to", EDIMessageStatusList.Codes.Received, interchangeMessage.EM_Status);
					AssertEquals("EM_LinkTable must be equal to", "CusEntryHeader", interchangeMessage.EM_LinkTable);
					AssertEquals("EM_LinkUniqueID must be equal to", entry.PK, interchangeMessage.EM_LinkUniqueID);
					AssertEquals("CH_EntryStatus must be equal to", "L17", entry.CH_EntryStatus);
					AssertEquals("CH_ValidityILShipmentDate must be equal to", new ZDateTime(2022, 08, 11), entry.CH_ValidityILShipmentDate);
					AssertEquals("CH_ValidityILDispatchDate must be equal to", new ZDateTime(2022, 11, 09), entry.CH_ValidityILDispatchDate);

					var response1 = importLicenseParent.Collection[0];
					AssertEquals("response1.EntryNumber should be", "22/1070364-8", response1.EntryNumber);
					AssertEquals("response1.RegistrationDate should be", "25/04/2022 16:22", response1.RegistrationDate);
					AssertEquals("response1.Status should be", "DESEMBARACADA", response1.Status);
				});
			}
		}

		public void TestLoadInvalidXML()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "00000000000001";
			declaration.JE_OH_Importer = importer.PK;

			var message = Factory.NewWithValidTestData<BREDIMessage>();
			message.EM_ApplicationCode = ApplicationCodes.BRCustoms;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			message.EM_ApplicationReference = "RLI00000000001000001";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			entry.MovementReferenceNumberSetter("BXI000010021");

			message.EM_LinkedObject = entry;

			Factory.Save();

			entry.Messages.Add(message);

			using (var response = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
			{
				var messageBuilder = new ZStringBuilder();
				var importLicenseParent = new ImportLicenseStatusResponseObjectParent(declaration);
				importLicenseParent.AddLog = AppendLog(messageBuilder);
				importLicenseParent.LoadAndValidateXML("Response.xml", response);

				AssertEquals(@"(1/1) The Import License Number 2210703648 does not match any Entry Header.
(100/100) File loading complete!", messageBuilder.ToStringWithNewLineBetweenAppends());
			}
		}

		public void TestLoadWrongXMLFile()
		{
			var messageBuilder = new ZStringBuilder();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			using (var response = new MemoryStream(Encoding.UTF8.GetBytes(wrongResponseXML)))
			{
				var importLicenseParent = new ImportLicenseStatusResponseObjectParent(declaration);
				importLicenseParent.AddLog = AppendLog(messageBuilder);
				importLicenseParent.LoadAndValidateXML("Response.xml", response);
				AssertEquals("(100/100) Unable to read Response.xml", messageBuilder.ToString());
			}
		}

		Action<int, int, string> AppendLog(ZStringBuilder messageBuilder)
		{
			return (completedCount, totalCount, messageText) => messageBuilder.Append($"({completedCount}/{totalCount}) {messageText}");
		}

		readonly string responseXML = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
  <resposta-consulta-li versao = ""1.0"" xmlns=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html ResultadoConsultaLoteLiWeb.xsd"">
    <identificador-consulta>251</identificador-consulta>
    <lista-mensagens-e-erros/>
    <lista-li-completa>
      <li-completa>
        <Grupo-Dados-Basicos>
          <numero-li>22/1070364-8</numero-li>
          <Importador>
            <importador-tipo>1</importador-tipo>
            <importador-identificador>08.264.406/0001-93</importador-identificador>
            <importador-nome>GARANTE DISTRIBUIDORA E IMPORTADORA DE PRODUTOS ALIMENT</importador-nome>
            <importador-atividade-economica/>
            <importador-natureza-juridica>SOCIEDADE POR COTAS DE RESPONS.LIMITADA-EMPRESA PRIVADA</importador-natureza-juridica>
            <importador-endereco-logradouro>EST DO GANCHINHO- LD</importador-endereco-logradouro>
            <importador-endereco-numero>690</importador-endereco-numero>
            <importador-endereco-complemento/>
            <importador-endereco-bairro>UMBARA</importador-endereco-bairro>
            <importador-endereco-cidade>CURITIBA</importador-endereco-cidade>
            <importador-endereco-uf>PR</importador-endereco-uf>
            <importador-endereco-cep>81930165</importador-endereco-cep>
            <importador-telefone>41 - 38882000</importador-telefone>
            <importador-pais/>
            <importador-pais-nome/>
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
            <texto-informacoes-complementares>DIR00089/22 - INVOICE: 676 - MASTERSUL COMEX LTDA (41) 3024-0100</texto-informacoes-complementares>
          </Informacoes-Complementares>
        </Grupo-Dados-Basicos>
        <Grupo-Fornecedor>
          <fornecedor-tipo>1</fornecedor-tipo>
          <pais-aquisicao-mercadoria>158</pais-aquisicao-mercadoria>
          <pais-aquisicao-mercadoria-nome>CHILE</pais-aquisicao-mercadoria-nome>
          <pais-origem-mercadoria>158</pais-origem-mercadoria>
          <pais-origem-mercadoria-nome>CHILE</pais-origem-mercadoria-nome>
          <Fornecedor>
            <fornecedor-estrangeiro-nome>TERRAUSTRAL S.A</fornecedor-estrangeiro-nome>
            <fornecedor-estrangeiro-email/>
            <fornecedor-estrangeiro-responsavel/>
            <fornecedor-estrangeiro-logradouro>HERNANDO DE AGUIRRE</fornecedor-estrangeiro-logradouro>
            <fornecedor-estrangeiro-numero>1915</fornecedor-estrangeiro-numero>
            <fornecedor-estrangeiro-complemento/>
            <fornecedor-estrangeiro-cidade>SANTIAGO</fornecedor-estrangeiro-cidade>
            <fornecedor-estrangeiro-uf>EXTERIOR</fornecedor-estrangeiro-uf>
          </Fornecedor>
          <Fabricante>
            <fabricante-nome/>
            <fabricante-email/>
            <fabricante-responsavel/>
            <fabricante-endereco-logradoudo/>
            <fabricante-endereco-numero/>
            <fabricante-endereco-complemento/>
            <fabricante-endereco-cidade/>
            <fabricante-endereco-estado/>
          </Fabricante>
        </Grupo-Fornecedor>
        <Grupo-Mercadoria>
          <Dados-Gerais>
            <subitem-ncm>2204.21.00</subitem-ncm>
            <subitem-ncm-nome>-- Em recipientes de capacidade não superior a 2 l</subitem-ncm-nome>
            <unidade-medida-estatistica>LITRO</unidade-medida-estatistica>
            <mercadoria-naladi>22042110</mercadoria-naladi>
            <mercadoria-naladi-nome/>
            <moeda>220</moeda>
            <moeda-nome>DOLAR DOS EUA</moeda-nome>
            <incoterm>FCA</incoterm>
            <incoterm-nome>FCA - FREE CARRIER</incoterm-nome>
          </Dados-Gerais>
          <Condicao-Mercadoria>
            <condicao-mercadoria>N</condicao-mercadoria>
            <condicao-mercadoria-nome>Nenhuma</condicao-mercadoria-nome>
            <tipo-enquadramento-material-usado/>
            <tipo-enquadramento-material-usado-nome/>
            <tipo-operacao-enquadramento-material-usado/>
            <tipo-operacao-enquadramento-material-usado-nome/>
          </Condicao-Mercadoria>
          <lista-destaque-ncm/>
          <lista-processo-anuente/>
          <Informacoes-Drawback>
            <drawback-regime>3</drawback-regime>
            <drawback-numero-ato-isencao/>
            <drawback-numero-ato-suspencao/>
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
              <marca/>
              <modelo/>
              <numero-serie/>
              <ano-fabricacao/>
              <item-ac-drawback/>
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
              <marca/>
              <modelo/>
              <numero-serie/>
              <ano-fabricacao/>
              <item-ac-drawback/>
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
              <marca/>
              <modelo/>
              <numero-serie/>
              <ano-fabricacao/>
              <item-ac-drawback/>
            </detalhe-ncm-item-drawback>
          </lista-detalhe-ncm>
          <Totalizadores>
            <quantidade-total-medida-estatistica>13.815,00000</quantidade-total-medida-estatistica>
            <peso-liquido-total-kg>13.815,00000</peso-liquido-total-kg>
            <valor-total-local-embarque>41.840,0000000</valor-total-local-embarque>
            <valor-total-condicao-venda>41.840,0000000</valor-total-condicao-venda>
          </Totalizadores>
        </Grupo-Mercadoria>
        <Grupo-Negociacao>
          <regime-acordo-tributario>1</regime-acordo-tributario>
          <regime-acordo-tributario-nome>RECOLHIMENTO INTEGRAL</regime-acordo-tributario-nome>
          <fundamento-legal-regime/>
          <fundamento-legal-regime-nome/>
          <tipo-acordo-tarifario>2</tipo-acordo-tarifario>
          <tipo-acordo-tarifario-nome>ALADI</tipo-acordo-tarifario-nome>
          <codigo-acordo-aladi>335</codigo-acordo-aladi>
          <codigo-acordo-aladi-nome>ACORDO DE COMPLEMENTACAO ECONOMICA N. 35 - MERCOSUL/CHILE</codigo-acordo-aladi-nome>
          <cobertura-cambial>1</cobertura-cambial>
          <cobertura-cambial-nome>COM COBERTURA CAMBIAL E PAGAMENTO FINAL A PRAZO DE ATE' 180</cobertura-cambial-nome>
          <modalidade-pagamento>31</modalidade-pagamento>
          <modalidade-pagamento-nome>FINANCIAMENTO DO FORNECEDOR (SUPPLIER'S CREDIT) - OUTROS</modalidade-pagamento-nome>
          <numero-dias-limite-pagamento/>
          <codigo-orgao-financeiro-internacional/>
          <codigo-orgao-financeiro-internacional-nome/>
          <codigo-motivo-sem-cobertura/>
          <codigo-motivo-sem-cobertura-nome/>
        </Grupo-Negociacao>
        <Grupo-LI-Anuencias>
          <Informacoes-LI>
            <data-registro>25/04/2022</data-registro>
            <hora-registro>16:22</hora-registro>
            <data-situacao>16/05/2022</data-situacao>
            <hora-situacao>14:25:40</hora-situacao>
            <codigo-situacao>17</codigo-situacao>
            <nome-situacao>DESEMBARACADA</nome-situacao>
            <data-restricao-embarque/>
            <data-validade-embarque>11/08/2022</data-validade-embarque>
            <data-validade-despacho>09/11/2022</data-validade-despacho>
            <numero-li-substituida/>
            <numero-li-substitutiva/>
          </Informacoes-LI>
          <Informacoes-LI-Vinculada-DI>
            <declaracao-vinculada>2209102544</declaracao-vinculada>
            <adicao-vinculada>001</adicao-vinculada>
            <retificacao/>
          </Informacoes-LI-Vinculada-DI>
          <Informacoes-Cancelamento-Vencimento-LI>
            <motivo/>
            <cpf-importador-efetuou-cancelamento/>
            <data-cancelamento-vencimento/>
            <hora-cancelamento-vencimento/>
          </Informacoes-Cancelamento-Vencimento-LI>
          <lista-anuencias>
            <anuencia>
              <orgao-anuente>MAPA</orgao-anuente>
              <codigo-situacao-anuencia>05</codigo-situacao-anuencia>
              <nome-situacao-anuencia>DEFERIDA</nome-situacao-anuencia>
              <data-diagnostico-anuencia>13/05/2022</data-diagnostico-anuencia>
              <hora-diagnostico-anuencia>14:25</hora-diagnostico-anuencia>
              <data-restricao-embarque/>
              <data-validade-embarque>11/08/2022</data-validade-embarque>
              <data-validade-despacho>09/11/2022</data-validade-despacho>
              <codigo-tratamento-administrativo>01</codigo-tratamento-administrativo>
              <nome-tratamento-administrativo>MERCADORIA</nome-tratamento-administrativo>
              <texto-anuente>Adotado procedimento simplificado para itens 2 e 3 e completo para item 1, conforme IN 67/2018 MAPA.Despacho autorizado.</texto-anuente>
            </anuencia>
          </lista-anuencias>
        </Grupo-LI-Anuencias>
      </li-completa>
    </lista-li-completa>
  </resposta-consulta-li>";

		readonly string wrongResponseXML = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
  <resposta-consulta-li versao = ""1.0"" xmlns=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.serpro.gov.br/liweb/schema/ResultadoConsultaLoteLiWeb.html ResultadoConsultaLoteLiWeb.xsd"">
    <identificador-consulta>251</identificador-consulta>
    <lista-mensagens-e-erros/>
      <li-completa>
        <Grupo-Dados-Basicos>
          <numero-li>22/1070364-8</numero-li>
          <Importador>
            <importador-tipo>1</importador-tipo>
            <importador-identificador>08.264.406/0001-93</importador-identificador>
            <importador-nome>GARANTE DISTRIBUIDORA E IMPORTADORA DE PRODUTOS ALIMENT</importador-nome>
            <importador-atividade-economica/>
            <importador-natureza-juridica>SOCIEDADE POR COTAS DE RESPONS.LIMITADA-EMPRESA PRIVADA</importador-natureza-juridica>
            <importador-endereco-logradouro>EST DO GANCHINHO- LD</importador-endereco-logradouro>
            <importador-endereco-numero>690</importador-endereco-numero>
            <importador-endereco-complemento/>
            <importador-endereco-bairro>UMBARA</importador-endereco-bairro>
            <importador-endereco-cidade>CURITIBA</importador-endereco-cidade>
            <importador-endereco-uf>PR</importador-endereco-uf>
            <importador-endereco-cep>81930165</importador-endereco-cep>
            <importador-telefone>41 - 38882000</importador-telefone>
            <importador-pais/>
            <importador-pais-nome/>
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
            <texto-informacoes-complementares>DIR00089/22 - INVOICE: 676 - MASTERSUL COMEX LTDA (41) 3024-0100</texto-informacoes-complementares>
          </Informacoes-Complementares>
        </Grupo-Dados-Basicos>
        <Grupo-Fornecedor>
          <fornecedor-tipo>1</fornecedor-tipo>
          <pais-aquisicao-mercadoria>158</pais-aquisicao-mercadoria>
          <pais-aquisicao-mercadoria-nome>CHILE</pais-aquisicao-mercadoria-nome>
          <pais-origem-mercadoria>158</pais-origem-mercadoria>
          <pais-origem-mercadoria-nome>CHILE</pais-origem-mercadoria-nome>
          <Fornecedor>
            <fornecedor-estrangeiro-nome>TERRAUSTRAL S.A</fornecedor-estrangeiro-nome>
            <fornecedor-estrangeiro-email/>
            <fornecedor-estrangeiro-responsavel/>
            <fornecedor-estrangeiro-logradouro>HERNANDO DE AGUIRRE</fornecedor-estrangeiro-logradouro>
            <fornecedor-estrangeiro-numero>1915</fornecedor-estrangeiro-numero>
            <fornecedor-estrangeiro-complemento/>
            <fornecedor-estrangeiro-cidade>SANTIAGO</fornecedor-estrangeiro-cidade>
            <fornecedor-estrangeiro-uf>EXTERIOR</fornecedor-estrangeiro-uf>
          </Fornecedor>
          <Fabricante>
            <fabricante-nome/>
            <fabricante-email/>
            <fabricante-responsavel/>
            <fabricante-endereco-logradoudo/>
            <fabricante-endereco-numero/>
            <fabricante-endereco-complemento/>
            <fabricante-endereco-cidade/>
            <fabricante-endereco-estado/>
          </Fabricante>
        </Grupo-Fornecedor>
        <Grupo-Mercadoria>
          <Dados-Gerais>
            <subitem-ncm>2204.21.00</subitem-ncm>
            <subitem-ncm-nome>-- Em recipientes de capacidade não superior a 2 l</subitem-ncm-nome>
            <unidade-medida-estatistica>LITRO</unidade-medida-estatistica>
            <mercadoria-naladi>22042110</mercadoria-naladi>
            <mercadoria-naladi-nome/>
            <moeda>220</moeda>
            <moeda-nome>DOLAR DOS EUA</moeda-nome>
            <incoterm>FCA</incoterm>
            <incoterm-nome>FCA - FREE CARRIER</incoterm-nome>
          </Dados-Gerais>
          <Condicao-Mercadoria>
            <condicao-mercadoria>N</condicao-mercadoria>
            <condicao-mercadoria-nome>Nenhuma</condicao-mercadoria-nome>
            <tipo-enquadramento-material-usado/>
            <tipo-enquadramento-material-usado-nome/>
            <tipo-operacao-enquadramento-material-usado/>
            <tipo-operacao-enquadramento-material-usado-nome/>
          </Condicao-Mercadoria>
          <lista-destaque-ncm/>
          <lista-processo-anuente/>
          <Informacoes-Drawback>
            <drawback-regime>3</drawback-regime>
            <drawback-numero-ato-isencao/>
            <drawback-numero-ato-suspencao/>
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
              <marca/>
              <modelo/>
              <numero-serie/>
              <ano-fabricacao/>
              <item-ac-drawback/>
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
              <marca/>
              <modelo/>
              <numero-serie/>
              <ano-fabricacao/>
              <item-ac-drawback/>
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
              <marca/>
              <modelo/>
              <numero-serie/>
              <ano-fabricacao/>
              <item-ac-drawback/>
            </detalhe-ncm-item-drawback>
          </lista-detalhe-ncm>
          <Totalizadores>
            <quantidade-total-medida-estatistica>13.815,00000</quantidade-total-medida-estatistica>
            <peso-liquido-total-kg>13.815,00000</peso-liquido-total-kg>
            <valor-total-local-embarque>41.840,0000000</valor-total-local-embarque>
            <valor-total-condicao-venda>41.840,0000000</valor-total-condicao-venda>
          </Totalizadores>
        </Grupo-Mercadoria>
        <Grupo-Negociacao>
          <regime-acordo-tributario>1</regime-acordo-tributario>
          <regime-acordo-tributario-nome>RECOLHIMENTO INTEGRAL</regime-acordo-tributario-nome>
          <fundamento-legal-regime/>
          <fundamento-legal-regime-nome/>
          <tipo-acordo-tarifario>2</tipo-acordo-tarifario>
          <tipo-acordo-tarifario-nome>ALADI</tipo-acordo-tarifario-nome>
          <codigo-acordo-aladi>335</codigo-acordo-aladi>
          <codigo-acordo-aladi-nome>ACORDO DE COMPLEMENTACAO ECONOMICA N. 35 - MERCOSUL/CHILE</codigo-acordo-aladi-nome>
          <cobertura-cambial>1</cobertura-cambial>
          <cobertura-cambial-nome>COM COBERTURA CAMBIAL E PAGAMENTO FINAL A PRAZO DE ATE' 180</cobertura-cambial-nome>
          <modalidade-pagamento>31</modalidade-pagamento>
          <modalidade-pagamento-nome>FINANCIAMENTO DO FORNECEDOR (SUPPLIER'S CREDIT) - OUTROS</modalidade-pagamento-nome>
          <numero-dias-limite-pagamento/>
          <codigo-orgao-financeiro-internacional/>
          <codigo-orgao-financeiro-internacional-nome/>
          <codigo-motivo-sem-cobertura/>
          <codigo-motivo-sem-cobertura-nome/>
        </Grupo-Negociacao>
        <Grupo-LI-Anuencias>
          <Informacoes-LI>
            <data-registro>25/04/2022</data-registro>
            <hora-registro>16:22</hora-registro>
            <data-situacao>16/05/2022</data-situacao>
            <hora-situacao>14:25:40</hora-situacao>
            <codigo-situacao>17</codigo-situacao>
            <nome-situacao>DESEMBARACADA</nome-situacao>
            <data-restricao-embarque/>
            <data-validade-embarque>11/08/2022</data-validade-embarque>
            <data-validade-despacho>09/11/2022</data-validade-despacho>
            <numero-li-substituida/>
            <numero-li-substitutiva/>
          </Informacoes-LI>
          <Informacoes-LI-Vinculada-DI>
            <declaracao-vinculada>2209102544</declaracao-vinculada>
            <adicao-vinculada>001</adicao-vinculada>
            <retificacao/>
          </Informacoes-LI-Vinculada-DI>
          <Informacoes-Cancelamento-Vencimento-LI>
            <motivo/>
            <cpf-importador-efetuou-cancelamento/>
            <data-cancelamento-vencimento/>
            <hora-cancelamento-vencimento/>
          </Informacoes-Cancelamento-Vencimento-LI>
          <lista-anuencias>
            <anuencia>
              <orgao-anuente>MAPA</orgao-anuente>
              <codigo-situacao-anuencia>05</codigo-situacao-anuencia>
              <nome-situacao-anuencia>DEFERIDA</nome-situacao-anuencia>
              <data-diagnostico-anuencia>13/05/2022</data-diagnostico-anuencia>
              <hora-diagnostico-anuencia>14:25</hora-diagnostico-anuencia>
              <data-restricao-embarque/>
              <data-validade-embarque>11/08/2022</data-validade-embarque>
              <data-validade-despacho>09/11/2022</data-validade-despacho>
              <codigo-tratamento-administrativo>01</codigo-tratamento-administrativo>
              <nome-tratamento-administrativo>MERCADORIA</nome-tratamento-administrativo>
              <texto-anuente>Adotado procedimento simplificado para itens 2 e 3 e completo para item 1, conforme IN 67/2018 MAPA.Despacho autorizado.</texto-anuente>
            </anuencia>
          </lista-anuencias>
        </Grupo-LI-Anuencias>
      </li-completa>
  </resposta-consulta-li>";
	}
}
