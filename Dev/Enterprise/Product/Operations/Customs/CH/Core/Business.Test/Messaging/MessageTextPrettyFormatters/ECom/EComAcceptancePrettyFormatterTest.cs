using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class EComAcceptancePrettyFormatterTest : TestCase
{
	#region Message Response

	ZString ResponseText => $@"<edecComplaintResponse schemaVersion = ""1.0"" xmlns=""http://www.e-dec.ch/xml/schema/edecComplaintResponse/v1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecComplaintResponse/v1 http://www.ezv.admin.ch/pdf_linker.php?doc=edecComplaintResponse_v_1_0"" >
  <requestorTraderIdentificationNumber>CHE326684996</requestorTraderIdentificationNumber>
  <requestorCorrelationID>cb-1661862989733</requestorCorrelationID>
  <edecComplaintAcceptance>
    <customsDeclarationNumber>22CHEI000043145845</customsDeclarationNumber>
    <acceptanceDate>2022-08-30</acceptanceDate>
    <acceptanceTime>14:36:30</acceptanceTime>
  </edecComplaintAcceptance>
</edecComplaintResponse>";

	ZString FormattedText => $@"<h2>eCom Request has been accepted</h2>
<p>Acceptance Date/Time: 2022-08-30 14:36:30</p>
<p>Request Number: cb-1661862989733</p>";

	ZString MessageIsEmpty => "<h2>Message is empty</h2>";

	#endregion

	public void TestGetFormattedText_AcceptanceDetails()
	{
		var expectedMessageInterpretation = FormattedText;

		var interpretation = new EComAcceptancePrettyFormatter(null).GetFormattedText();
		AssertEquals($"When MessageDetail is empty, EM_MessageInterpretation should return '{MessageIsEmpty}'", MessageIsEmpty, interpretation);

		interpretation = new EComAcceptancePrettyFormatter((IEComAcceptanceResponseDetail)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail).GetFormattedText();
		var expectedText = expectedMessageInterpretation.Replace(System.Environment.NewLine, string.Empty);
		AssertEquals("EM_MessageInterpretation", expectedText, interpretation);
	}
}
