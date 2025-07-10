using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class BRImportLicenseInterchangeProviderTest : InterchangeProviderTestCase
	{
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new BRImportLicenseInterchangeProvider(collection);
		}

		public void TestAddFileOrDocumentImportLicense()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_DeclarationReference = "TEST_BO";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var message1 = entryHeader.Messages.AddNew(typeof(BREDIMessage)) as BREDIMessage;
			message1.EM_MessageType = MessageTypeList.Codes.LIC;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_MessageText = licXML1;
			var message2 = entryHeader.Messages.AddNew(typeof(BREDIMessage)) as BREDIMessage;
			message2.EM_MessageType = MessageTypeList.Codes.LIC;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageText = licXML2;

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message1, message2 });
			var provider = new BRImportLicenseInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();

			var docs = declaration.DocManagerInfo.AllEDocs;
			AssertEquals(1, docs.Count);
			var doc = docs[0];
			AssertEquals("TEST_BO_000001.xml", doc.FileName);
			AssertEquals(Core.Constants.RefDocTypes.RequestDocument, doc.DocType);

			using (var streamReader = new StreamReader(doc.GetImageDataReader(), Encoding.ASCII))
			{
				AssertXMLEquals(licExpectedXML, streamReader.ReadToEnd());
			}
		}

		public override void TestMessagesPopulateNewInterchange()
		{
			var message1 = CreateAndPopulateMessage(MessageTypeList.Codes.LIC, ImportLicenseActionCodeList.Codes.ORI, licXML1);
			var message2 = CreateAndPopulateMessage(MessageTypeList.Codes.LIC, ImportLicenseActionCodeList.Codes.ORI, licXML2);
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message1, message2 });
			var provider = new BRImportLicenseInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();

			var interchanges = provider.Interchanges;
			CombineAssertions(() =>
			{
				AssertEquals("Number Of Interchanges", 1, interchanges.Length);

				var interchange = interchanges[0];
				AssertNotNull("Interchange created", interchange);
				AssertEquals("Message 1 Status", Constants.EDIMessageStatusCodes.Manual, message1.EM_Status);
				AssertEquals("Message 2 Status", Constants.EDIMessageStatusCodes.Manual, message2.EM_Status);
				AssertEquals("interchange for Message 1", interchange.PK, message1.EM_EI);
				AssertEquals("interchange for Message 2", interchange.PK, message2.EM_EI);

				AssertEquals("To", BREDIInterchange.BRCustoms, interchange.EI_To);
				AssertEquals("From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("Interchange Status", Constants.EDIMessageStatusCodes.Manual, interchange.EI_Status);
				AssertEquals("Interchange Type ", MessageTypeList.Codes.LIC, interchange.EI_InterchangeType);
				AssertEquals("Application Code ", BREDIInterchange.ApplicationCodes.BRCustoms, interchange.EI_ApplicationCode);
				AssertEquals("Transport Type", EDIInterchange.TransportType.tXT, interchange.EI_TransportType);
				AssertEquals("Header Text", @"{""custom.MessageSubType"":""ORI"",""custom.ReferenceNumber"":""21BR0000022649""}", interchange.EI_HeaderText);
				AssertXMLEquals("Body Text", licExpectedXML, interchange.EI_BodyText.ToString());
				AssertEquals("Footer Text", ZString.Empty, interchange.EI_FooterText);
			});
		}

		BREDIMessage CreateAndPopulateMessage(string messageType, string messageSubType, string messageBody)
		{
			var message = Factory.New<BREDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = messageBody;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationReference = "21BR0000022649";
			return message;
		}

		const string licXML1 = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<lote-li xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" idArquivoLote=""{EDIMessage.UniqueBatchNumberPlaceHolderHtml}"" versao=""2.0""
    xmlns=""http://www.serpro.gov.br/liweb/schema/LoteLiWeb.html"">
    <li>
        <identificador-li>B00001003-1</identificador-li>
        <tipo-importador>1</tipo-importador>
        <identificacao-importador>58500398000105</identificacao-importador>
        <codigo-urf-entrada>0000900</codigo-urf-entrada>
        <codigo-urf-despacho>0000500</codigo-urf-despacho>
        <texto-informacoes-complementares>testetestteteste2teste3teste4</texto-informacoes-complementares>
        <tipo-fornecedor>1</tipo-fornecedor>
        <fornecedor-estrangeiro-nome>ATSENT NOME 1</fornecedor-estrangeiro-nome>
        <fornecedor-estrangeiro-logradouro>RUA CONSELHEIRO MOREIRA DE BARROS</fornecedor-estrangeiro-logradouro>
        <fornecedor-estrangeiro-numero />
        <fornecedor-estrangeiro-complemento />
        <fornecedor-estrangeiro-cidade>SAO PAULO</fornecedor-estrangeiro-cidade>
        <fornecedor-estrangeiro-uf>AM</fornecedor-estrangeiro-uf>
        <pais-origem-mercadoria>105</pais-origem-mercadoria>
        <subitem-ncm>02062200</subitem-ncm>
        <codigo-mercadoria-naladi />
        <codigo-moeda>790</codigo-moeda>
        <codigo-incoterms>FOB</codigo-incoterms>
        <condicao-mercadoria>S</condicao-mercadoria>
        <tipo-enquadramento-material-usado>2</tipo-enquadramento-material-usado>
        <tipo-operacao-enquadramento-material-usado>08</tipo-operacao-enquadramento-material-usado>
        <drawback-regime>4</drawback-regime>
        <drawback-numero-ato-isencao>123</drawback-numero-ato-isencao>
        <drawback-numero-ato-suspencao />
        <detalhe-ncm-item-drawback>
            <numero-sequencial-produto>1</numero-sequencial-produto>
            <peso-liquido-total>10.000</peso-liquido-total>
            <qtd-mercadoria-unidade-comercializada>1.00000</qtd-mercadoria-unidade-comercializada>
            <qtd-mercadoria-unidade-estatistica>10.00000</qtd-mercadoria-unidade-estatistica>
            <valor-unitario-condicao-venda>10</valor-unitario-condicao-venda>
            <valor-total-local-embarque>10.0000</valor-total-local-embarque>
            <descricao-produto>TESTE PRODUITO</descricao-produto>
            <marca>BRAND</marca>
            <modelo>MODEL</modelo>
            <numero-serie>SERIALNUMBER</numero-serie>
            <ano-fabricacao>2022</ano-fabricacao>
            <item-ac-drawback>1</item-ac-drawback>
        </detalhe-ncm-item-drawback>
        <cobertura-cambial />
        <modalidade-pagamento />
        <codigo-orgao-financeiro-internacional />
    </li>
</lote-li>";

		const string licXML2 = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<lote-li xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" idArquivoLote=""{EDIMessage.UniqueBatchNumberPlaceHolderHtml}"" versao=""2.0""
    xmlns=""http://www.serpro.gov.br/liweb/schema/LoteLiWeb.html"">
    <li>
        <identificador-li>B00001003-2</identificador-li>
        <tipo-importador>1</tipo-importador>
        <identificacao-importador>58500398000105</identificacao-importador>
        <codigo-urf-entrada>0000900</codigo-urf-entrada>
        <codigo-urf-despacho>0000500</codigo-urf-despacho>
        <texto-informacoes-complementares>Teste2</texto-informacoes-complementares>
        <tipo-fornecedor>1</tipo-fornecedor>
        <fornecedor-estrangeiro-nome>ATSENT NOME 1</fornecedor-estrangeiro-nome>
        <fornecedor-estrangeiro-logradouro>RUA CONSELHEIRO MOREIRA DE BARROS</fornecedor-estrangeiro-logradouro>
        <fornecedor-estrangeiro-numero />
        <fornecedor-estrangeiro-complemento />
        <fornecedor-estrangeiro-cidade>SAO PAULO</fornecedor-estrangeiro-cidade>
        <fornecedor-estrangeiro-uf>AM</fornecedor-estrangeiro-uf>
        <pais-origem-mercadoria />
        <subitem-ncm>02062200</subitem-ncm>
        <codigo-mercadoria-naladi />
        <codigo-moeda>790</codigo-moeda>
        <codigo-incoterms>FOB</codigo-incoterms>
        <condicao-mercadoria>N</condicao-mercadoria>
        <tipo-enquadramento-material-usado />
        <tipo-operacao-enquadramento-material-usado />
        <drawback-regime>3</drawback-regime>
        <drawback-numero-ato-isencao />
        <drawback-numero-ato-suspencao />
        <detalhe-ncm-item-drawback>
            <numero-sequencial-produto>1</numero-sequencial-produto>
            <peso-liquido-total>10.000</peso-liquido-total>
            <qtd-mercadoria-unidade-comercializada>1.00000</qtd-mercadoria-unidade-comercializada>
            <qtd-mercadoria-unidade-estatistica>10.00000</qtd-mercadoria-unidade-estatistica>
            <valor-unitario-condicao-venda>20</valor-unitario-condicao-venda>
            <valor-total-local-embarque>20.0000</valor-total-local-embarque>
            <descricao-produto>TESTE PRODUITO</descricao-produto>
            <marca />
            <modelo />
            <numero-serie />
            <ano-fabricacao />
            <item-ac-drawback />
        </detalhe-ncm-item-drawback>
        <cobertura-cambial />
        <modalidade-pagamento />
        <codigo-orgao-financeiro-internacional />
    </li>
</lote-li>";

		const string licExpectedXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<lote-li xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" idArquivoLote=""000001"" versao=""2.0"" xmlns=""http://www.serpro.gov.br/liweb/schema/LoteLiWeb.html"">
  <li>
    <identificador-li>B00001003-1</identificador-li>
    <tipo-importador>1</tipo-importador>
    <identificacao-importador>58500398000105</identificacao-importador>
    <codigo-urf-entrada>0000900</codigo-urf-entrada>
    <codigo-urf-despacho>0000500</codigo-urf-despacho>
    <texto-informacoes-complementares>testetestteteste2teste3teste4</texto-informacoes-complementares>
    <tipo-fornecedor>1</tipo-fornecedor>
    <fornecedor-estrangeiro-nome>ATSENT NOME 1</fornecedor-estrangeiro-nome>
    <fornecedor-estrangeiro-logradouro>RUA CONSELHEIRO MOREIRA DE BARROS</fornecedor-estrangeiro-logradouro>
    <fornecedor-estrangeiro-numero />
    <fornecedor-estrangeiro-complemento />
    <fornecedor-estrangeiro-cidade>SAO PAULO</fornecedor-estrangeiro-cidade>
    <fornecedor-estrangeiro-uf>AM</fornecedor-estrangeiro-uf>
    <pais-origem-mercadoria>105</pais-origem-mercadoria>
    <subitem-ncm>02062200</subitem-ncm>
    <codigo-mercadoria-naladi />
    <codigo-moeda>790</codigo-moeda>
    <codigo-incoterms>FOB</codigo-incoterms>
    <condicao-mercadoria>S</condicao-mercadoria>
    <tipo-enquadramento-material-usado>2</tipo-enquadramento-material-usado>
    <tipo-operacao-enquadramento-material-usado>08</tipo-operacao-enquadramento-material-usado>
    <drawback-regime>4</drawback-regime>
    <drawback-numero-ato-isencao>123</drawback-numero-ato-isencao>
    <drawback-numero-ato-suspencao />
    <detalhe-ncm-item-drawback>
      <numero-sequencial-produto>1</numero-sequencial-produto>
      <peso-liquido-total>10.000</peso-liquido-total>
      <qtd-mercadoria-unidade-comercializada>1.00000</qtd-mercadoria-unidade-comercializada>
      <qtd-mercadoria-unidade-estatistica>10.00000</qtd-mercadoria-unidade-estatistica>
      <valor-unitario-condicao-venda>10</valor-unitario-condicao-venda>
      <valor-total-local-embarque>10.0000</valor-total-local-embarque>
      <descricao-produto>TESTE PRODUITO</descricao-produto>
      <marca>BRAND</marca>
      <modelo>MODEL</modelo>
      <numero-serie>SERIALNUMBER</numero-serie>
      <ano-fabricacao>2022</ano-fabricacao>
      <item-ac-drawback>1</item-ac-drawback>
    </detalhe-ncm-item-drawback>
    <cobertura-cambial />
    <modalidade-pagamento />
    <codigo-orgao-financeiro-internacional />
  </li>
  <li>
    <identificador-li>B00001003-2</identificador-li>
    <tipo-importador>1</tipo-importador>
    <identificacao-importador>58500398000105</identificacao-importador>
    <codigo-urf-entrada>0000900</codigo-urf-entrada>
    <codigo-urf-despacho>0000500</codigo-urf-despacho>
    <texto-informacoes-complementares>Teste2</texto-informacoes-complementares>
    <tipo-fornecedor>1</tipo-fornecedor>
    <fornecedor-estrangeiro-nome>ATSENT NOME 1</fornecedor-estrangeiro-nome>
    <fornecedor-estrangeiro-logradouro>RUA CONSELHEIRO MOREIRA DE BARROS</fornecedor-estrangeiro-logradouro>
    <fornecedor-estrangeiro-numero />
    <fornecedor-estrangeiro-complemento />
    <fornecedor-estrangeiro-cidade>SAO PAULO</fornecedor-estrangeiro-cidade>
    <fornecedor-estrangeiro-uf>AM</fornecedor-estrangeiro-uf>
    <pais-origem-mercadoria />
    <subitem-ncm>02062200</subitem-ncm>
    <codigo-mercadoria-naladi />
    <codigo-moeda>790</codigo-moeda>
    <codigo-incoterms>FOB</codigo-incoterms>
    <condicao-mercadoria>N</condicao-mercadoria>
    <tipo-enquadramento-material-usado />
    <tipo-operacao-enquadramento-material-usado />
    <drawback-regime>3</drawback-regime>
    <drawback-numero-ato-isencao />
    <drawback-numero-ato-suspencao />
    <detalhe-ncm-item-drawback>
      <numero-sequencial-produto>1</numero-sequencial-produto>
      <peso-liquido-total>10.000</peso-liquido-total>
      <qtd-mercadoria-unidade-comercializada>1.00000</qtd-mercadoria-unidade-comercializada>
      <qtd-mercadoria-unidade-estatistica>10.00000</qtd-mercadoria-unidade-estatistica>
      <valor-unitario-condicao-venda>20</valor-unitario-condicao-venda>
      <valor-total-local-embarque>20.0000</valor-total-local-embarque>
      <descricao-produto>TESTE PRODUITO</descricao-produto>
      <marca />
      <modelo />
      <numero-serie />
      <ano-fabricacao />
      <item-ac-drawback />
    </detalhe-ncm-item-drawback>
    <cobertura-cambial />
    <modalidade-pagamento />
    <codigo-orgao-financeiro-internacional />
  </li>
</lote-li>";
	}
}
