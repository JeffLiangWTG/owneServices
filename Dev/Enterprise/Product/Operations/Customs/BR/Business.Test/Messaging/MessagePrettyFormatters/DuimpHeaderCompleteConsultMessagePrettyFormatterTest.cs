using System.Text.Json;
using CargoWise.Customs.BR.MessageDefinitions.Duimp;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessageProcessors;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class DuimpHeaderCompleteConsultMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestGetFormattedMessageText()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "JOBTEST";
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var json = JsonSerializer.Deserialize<DuimpConsultaCover>(BRCDuimpHeaderSuccessResponseMessageProcessorTest.JsonMessageCompleteConsult);

			var actualHtml = new DuimpHeaderCompleteConsultMessagePrettyFormatter(entry, json).GetFormattedMessageText();
			var expectedHtml = GetExpectedExportSuccessHTML(EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference));

			AssertMultilineASCIIEquals("EM_MessageInterpretation must be equal to HTML", expectedHtml, actualHtml);
		}

		public void TestGetFormattedMessageText_InvalidXml()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "JOBTEST";
			var entry = declaration.CustomsEntryHeaders.AddNew();

			AssertNoExceptionThrown(() => new DuimpHeaderCompleteConsultMessagePrettyFormatter(entry, new DuimpConsultaCover()).GetFormattedMessageText());
		}

		public static string GetExpectedExportSuccessHTML(string link) => $@"<br />
<strong>Job Number: {link}</strong><br />
<br />
A response message has been received from Brazilian Customs.<br/>The message sent for the above mentioned job has received the following update(s):<br/><br/>identificacao:<br/><br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>numero</th><th>versao</th></tr></thead><tr class=""table"" align=""center""><td>24BR00000002090</td><td>1</td></tr></table><br/>situacao:<br/><br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>situacaoDuimp</th><th>situacaoAnaliseRetificacao</th><th>situacaoLicenciamento</th><th>controleCarga</th></tr></thead><tr class=""table"" align=""center""><td>REGISTRADA_AGUARDANDO_CANAL</td><td>PENDENTE_AGUARDANDO_ANALISE</td><td>DISPENSADO</td><td>VINCULADA</td></tr></table><br/>situacaoConferenciaAduaneira:<br/><br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>siglaOrgao</th><th>situacao</th><th>indicadorAutorizacaoEntrega</th><th>indicadorDesembaracoDecisaoJudicial</th></tr></thead><tr class=""table"" align=""center""><td>RECEITA</td><td>DESEMBARACO_AUTOMATICO</td><td>SIM</td><td>SIM</td></tr><tr class=""table"" align=""center""><td>RECEITA 2</td><td>DESEMBARACO_MANUAL</td><td>NAO</td><td>NAO</td></tr></table><br/>situacaoConferenciaAnuente:<br/><br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>siglaOrgao</th><th>situacao</th><th>indicadorAutorizacaoProsseguimentoConferenciaAnuente</th><th>indicadorConclusaoDecisaoJudicial</th></tr></thead><tr class=""table"" align=""center""><td>ANVISA</td><td>DESEMBARACO_AUTOMATICO</td><td>SIM</td><td>SIM</td></tr><tr class=""table"" align=""center""><td>ANVISA 2</td><td>DESEMBARACO_MANUAL</td><td>SIM</td><td>NAO</td></tr></table><br/>resultadoAnaliseRisco:<br/><br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>canalConsolidado</th></tr></thead><tr class=""table"" align=""center""><td>VERDE</td></tr></table><br/>resultadoRFB:<br/><br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>orgao</th><th>resultado</th></tr></thead><tr class=""table"" align=""center""><td>Receita</td><td>DESEMBARACO_AUTORIZADO</td></tr><tr class=""table"" align=""center""><td>Receita 2</td><td>DESEMBARACO_MANUAL</td></tr></table><br/>resultadoAnuente:<br/><br/><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><thead><tr class=""tableheadings""><th>orgao</th><th>resultado</th></tr></thead><tr class=""table"" align=""center""><td>MAPA</td><td>ANALISE_DOCUMENTAL</td></tr><tr class=""table"" align=""center""><td>MAPA 2 </td><td>ANALISE_XML</td></tr></table><br/>
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
<br />
";
	}
}
