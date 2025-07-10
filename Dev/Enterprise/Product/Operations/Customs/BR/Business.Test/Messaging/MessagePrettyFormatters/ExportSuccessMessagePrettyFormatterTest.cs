using CargoWise.Customs.BR.MessageDefinitions.Export.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessageProcessors;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ExportSuccessMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestGetFormattedMessageText()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "JOBTEST";
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var xml = XmlObjectSerializer.Deserialize<pucomexReturn>(BRCSecondReturnSuccessMessageProcessorTest.ExportSuccessMessageBody);

			var actualHtml = new ExportSuccessMessagePrettyFormatter(entry, xml).GetFormattedMessageText();
			var expectedHtml = GetExpectedExportSuccessHTML(EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference));

			AssertMultilineASCIIEquals("EM_MessageInterpretation must be equal to HTML", expectedHtml, actualHtml);

			var xmlWithoutChaveDeAcesso = XmlObjectSerializer.Deserialize<pucomexReturn>(BRCSecondReturnSuccessMessageProcessorTest.ExportSuccessMessageBody.Replace("<chaveDeAcesso>20RQN000613376</chaveDeAcesso>", ""));

			actualHtml = new ExportSuccessMessagePrettyFormatter(entry, xmlWithoutChaveDeAcesso).GetFormattedMessageText();
			expectedHtml = GetExpectedExportSuccessHTML(EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference));

			AssertMultilineASCIIEquals("EM_MessageInterpretation must be equal to HTML", expectedHtml.Replace("<th>chaveDeAcesso</th>", "").Replace("<td>20RQN000613376</td>", ""), actualHtml);
		}

		public static string GetExpectedExportSuccessHTML(string link) => $@"<br />
<strong>Job Number: {link}<br/><br/></strong><br />
<br />
A response message has been received from Brazilian Customs.<br/>The message sent for the above mentioned job has received the following update(s):<br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>message</th><th>due</th><th>ruc</th><th>chaveDeAcesso</th><th>date</th><th>cpf</th></tr></thead><tr><td>Operação realizada com sucesso.</td><td>20BR0000274180</td><td>0BR00000000200000000000000000020403</td><td>20RQN000613376</td><td>2020-04-07 16:07:06</td><td>01183367708</td></tr></table><br/>
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
<br />
";
	}
}
