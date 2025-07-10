using CargoWise.Customs.BR.MessageDefinitions.Export.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessageProcessors;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ExportErrorMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestGetFormattedMessageText()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			declaration.JE_DeclarationReference = "JOBTEST";

			var entry = declaration.CustomsEntryHeaders.AddNew();

			var error = XmlObjectSerializer.Deserialize<error>(BRCSecondReturnErrorMessageProcessorTest.ErrorReturnXML);

			var actualHtml = new ExportErrorMessagePrettyFormatter(entry, error).GetFormattedMessageText();
			var expectedHtml = GetExpectedHtml(EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference));

			AssertMultilineASCIIEquals("EM_MessageInterpretation must be equal to HTML", expectedHtml, actualHtml);
		}

		public void TestGetFormattedMessageTextWithEmptyDetail()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			declaration.JE_DeclarationReference = "JOBTEST";

			var entry = declaration.CustomsEntryHeaders.AddNew();

			var error = XmlObjectSerializer.Deserialize<error>(BRCSecondReturnErrorMessageProcessorTest.ErrorReturnXML3);

			var actualHtml = new ExportErrorMessagePrettyFormatter(entry, error).GetFormattedMessageText();
			var expectedHtml = GetExpectedHtmlEmptyDetail(EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference));

			AssertMultilineASCIIEquals("EM_MessageInterpretation must be equal to HTML", expectedHtml, actualHtml);
		}

		string GetExpectedHtmlEmptyDetail(string link) => $@"<br />
<strong><br/>Job Number: {link}</strong><br />
<br />
" +
@"A response message has been received from Brazilian Customs.<br/>The message sent for the above mentioned job has received the following update(s):<br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>code</th><th>message</th><th>tag</th><th>date</th><th>status</th><th>severity</th></tr></thead><tr><td>&nbsp;</td><td>Arquivo XML não atende as especificações definidas no XSD. cvc-complex-type.2.4.a: Invalid content was found starting with element<br>Commodity<br>. One of<br>{""urn:wco:datamodel:WCO:GoodsDeclaration:1"":Destination}<br> is expected.</td><td>[DUEX-IJGFDZ2715]</td><td>2022-11-30T11:06:00</td><td>422</td><td>ERROR</td></tr></table><br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>ambiente</th><th>mnemonico</th><th>sistema</th><th>trackerId</th><th>url</th><th>visao</th></tr></thead><tr><td>TRE</td><td>DUEX</td><td>Declaração Única de Exportação</td><td>Iy8ZTg8Uxe</td><td>/due/api/ext/due</td><td>PRIV</td></tr></table><br/><hr/><br/>
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
<br />
";

		string GetExpectedHtml(string link) => $@"<br />
<strong><br/>Job Number: {link}</strong><br />
<br />
" +
@"A response message has been received from Brazilian Customs.<br/>The message sent for the above mentioned job has received the following update(s):<br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>code</th><th>message</th><th>tag</th><th>date</th><th>status</th><th>severity</th></tr></thead><tr><td>DUEX-ER0027</td><td>Arquivo XML não atende as especificações definidas no XSD. cvc-complex-type.2.4.b: The content of element ''Destination'' is not complete. One of ''{""urn:wco:datamodel:WCO:GoodsDeclaration:1"":GoodsMeasure}'' is expected.</td><td>[DUEX-AUJDTS2715]</td><td>2021-12-29T09:56:58</td><td>422</td><td>ERROR</td></tr><tr><td>DUEX-ER0027</td><td>Arquivo XML não atende as especificações definidas no XSD. cvc-complex-type.2.4.b: The content of element ''GovernmentAgencyGoodsItem'' is not complete. One of ''{""urn:wco:datamodel:WCO:GoodsDeclaration:1"":GovernmentProcedure}'' is expected.</td><td>[DUEX-RFWJER2715]</td><td>2021-12-29T09:56:58</td><td>422</td><td>ERROR</td></tr></table><br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>ambiente</th><th>mnemonico</th><th>sistema</th><th>trackerId</th><th>url</th><th>visao</th></tr></thead><tr><td>TRE</td><td>DUEX</td><td>Declaração Única de Exportação</td><td>YR02FIHrks</td><td>/due/api/ext/due</td><td>PRIV</td></tr></table><br/><hr/><br/>
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
<br />
";
	}
}
