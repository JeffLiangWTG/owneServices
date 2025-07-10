using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class EComRuleErrorPrettyFormatterTest : TestCase
{
	#region Message Response
	ZString ResponseText => $@"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<edecComplaintResponse xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xmlns=""http://www.e-dec.ch/xml/schema/edecComplaintResponse/v1""
xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecComplaintResponse/v1 edecComplaintResponse_v_1_0.xsd"" schemaVersion=""1.0"" >
<requestorTraderIdentificationNumber>CHE123456789</requestorTraderIdentificationNumber>
<requestorCorrelationID>1</requestorCorrelationID>
<edecComplaintRejection>
<rejectionDate>2015-06-08</rejectionDate>
<rejectionTime>15:39:09</rejectionTime>
<errors>
 <ruleErrors>
<customsDeclarationNumber>15CHEI000000481462</customsDeclarationNumber>
<declarant>
 <traderIdentificationNumber>1000031</traderIdentificationNumber>
 <declarantNumber>1</declarantNumber>
</declarant>
<error>
 <ruleName>C010</ruleName>
 <checkType>Complaint Check</checkType>
<descriptions>
 <description language = ""de"" > Der Feldname ist in den Stammdaten nicht vorhanden.</description>
 <description language = ""fr"" > Le nom de champ n'est pas disponible dans les données fixes.</description>
 <description language = ""it"" > Il nome del campo non è compreso nei dati di base.</description>
</descriptions>
</error>
 </ruleErrors>
</errors>
</edecComplaintRejection>
</edecComplaintResponse>";

	ZString FormattedText => $@"<h2>eCom Request has been rejected</h2>
<p>Rejection Date/Time: 2015-06-08 15:39:09</p>
<p>Request Number: 1</p>
<h3>Errors</h3>
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
<thead><tr class=""tableheadings""><th>Rule Name</th><th>Description</th></tr></thead>
<tr><td>C010</td><td> Der Feldname ist in den Stammdaten nicht vorhanden.</td></tr>
</table>";

	ZString MessageIsEmpty => "<h2>Message is empty</h2>";

	#endregion

	public void TestGetFormattedText_SchemaErrorDetails()
	{
		var expectedMessageInterpretation = FormattedText;

		var interpretation = new EComRuleErrorPrettyFormatter(null).GetFormattedText();
		AssertEquals($"When MessageDetail is empty, EM_MessageInterpretation should return '{MessageIsEmpty}'", MessageIsEmpty, interpretation);

		interpretation = new EComRuleErrorPrettyFormatter((IEComRuleErrorsResponseDetail)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail).GetFormattedText();
		var expectedText = expectedMessageInterpretation.Replace(System.Environment.NewLine, string.Empty);
		AssertEquals("EM_MessageInterpretation", expectedText, interpretation);
	}

	public void TestGetFormattedText_Language()
	{
		CombineAssertions(() =>
		{
			var interpretedMessage = new EComRuleErrorPrettyFormatter((IEComRuleErrorsResponseDetail)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail).GetFormattedText();
			Assert("default language is german", interpretedMessage.Contains("Der Feldname ist"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "FR-CH";
			interpretedMessage = new EComRuleErrorPrettyFormatter((IEComRuleErrorsResponseDetail)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail).GetFormattedText();
			Assert("French", interpretedMessage.Contains("Le nom de champ"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "IT-CH";
			interpretedMessage = new EComRuleErrorPrettyFormatter((IEComRuleErrorsResponseDetail)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail).GetFormattedText();
			Assert("Italian", interpretedMessage.Contains("Il nome del campo"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "DE-CH";
			interpretedMessage = new EComRuleErrorPrettyFormatter((IEComRuleErrorsResponseDetail)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail).GetFormattedText();
			Assert("German", interpretedMessage.Contains("Der Feldname ist"));
		});
	}
}
