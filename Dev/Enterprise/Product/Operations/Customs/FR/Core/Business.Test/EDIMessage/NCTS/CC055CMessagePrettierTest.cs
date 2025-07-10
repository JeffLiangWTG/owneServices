using System.Text.RegularExpressions;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC055C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC055CMessagePrettierTest : NCTSMessagePrettierTest<Cc055CType, CC055CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => new ZString(Regex.Replace(@"
<style>
	body, p, td {
		font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;
	}
</style>
<p style=""font-size: 120%""><strong>Status: </strong>Guarantee Invalid<br><strong>MRN: </strong>MRN1</p>
<p><strong>Invalid Guarantee Reason: </strong>
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">
	<tr align=""center"">
		<td width=""75px"">GRN</td>
		<td width=""200px"">Invalid Guarantee Reason</td>
	</tr>
	<tr>
		<td>GRN1</td>
		<td>Guarantee does not exist</td>
	</tr>
	<tr>
		<td>GRN2</td>
		<td>Guarantee exists, but not valid</td>
	</tr>
	<tr>
		<td>GRN3</td>
		<td>Access code not valid</td>
	</tr>
</table>
</p>
", "[\r\n]+\t*", string.Empty));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC055CResponseMessage.xml");
	}
}

