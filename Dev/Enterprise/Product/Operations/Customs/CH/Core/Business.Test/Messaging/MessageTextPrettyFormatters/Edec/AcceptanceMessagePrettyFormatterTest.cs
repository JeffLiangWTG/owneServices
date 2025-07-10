using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

public class AcceptanceMessagePrettyFormatterTest : TestCaseWithFactory
{
	readonly ZString responseMessage = $@"<?xml version=""1.0"" encoding=""UTF - 8""?>
<goodsDeclarationsResponse schemaVersion =""4.0"" xmlns=""http://www.e-dec.ch/xml/schema/edecResponse/v4""
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecResponse/v4 http://www.ezv.admin.ch/pdf_linker.php?doc=edecResponse_v_4_0"">
<goodsDeclarationAcceptance>
<traderDeclarationNumber>0100011210809998</traderDeclarationNumber>
<traderReference>MM_TEST_2009_eBegleit</traderReference>
<customsDeclarationNumber>21CHEI000042027074</customsDeclarationNumber>
<customsDeclarationVersion>1</customsDeclarationVersion>
<accessCode>b6zCR8Vfz2u5yKeU</accessCode>
<acceptanceDate>2021-10-14</acceptanceDate>
<acceptanceTime>11:31:16</acceptanceTime>
<declarant>
<traderIdentificationNumber>CHE293274655</traderIdentificationNumber>
<declarantNumber>72</declarantNumber>
</declarant>
<initiator>1</initiator>
<correctionCode>1</correctionCode>
<valuation>
<duty>230.40</duty>
<VAT>5022.70</VAT>
</valuation>
<goodsItem>
<traderItemID>1</traderItemID>
<customsItemNumber>1</customsItemNumber>
<selectionResult>1</selectionResult>
<valuationRate>4.80</valuationRate>
<valuationDetail>
<duty>115.20</duty>
<VAT>2511.35</VAT>
</valuationDetail>
</goodsItem>
<goodsItem>
<traderItemID>2</traderItemID>
<customsItemNumber>2</customsItemNumber>
<selectionResult>1</selectionResult>
<valuationRate>4.80</valuationRate>
<valuationDetail>
<duty>115.20</duty>
<VAT>2511.35</VAT>
</valuationDetail>
</goodsItem>
</goodsDeclarationAcceptance>
<goodsDeclarationStatus>
<traderDeclarationNumber>0100011210809998</traderDeclarationNumber>
<traderReference>MM_TEST_2009_eBegleit</traderReference>
<customsOfficeNumber>CH001251</customsOfficeNumber>
<customsDeclarationNumber>21CHEI000042027074</customsDeclarationNumber>
<customsDeclarationVersion>1</customsDeclarationVersion>
<statusDate>2021-10-14</statusDate>
<statusTime>11:31:16</statusTime>
<status>" + RefCusCodeTestHelper.EntryStatusCode + @"</status>
<materialCheck>0</materialCheck>
<release>1</release>
</goodsDeclarationStatus>
</goodsDeclarationsResponse>";
	readonly ZString formattedHtml = $@"<table border=""0""><tr><td>
Customs Declaration Number:</td><td>21CHEI000042027074</td></tr><tr><td>
Customs Declaration Version:</td><td>1</td></tr><tr><td>
Acceptance Date:</td><td>14-Oct-21</td></tr><tr><td>
Declarant Number:</td><td>72</td></tr><tr><td>
Duty Total Amount:</td><td>230.40</td></tr><tr><td>
VAT Total Amount:</td><td>5022.70</td></tr><tr><td>
Access Code:</td><td>b6zCR8Vfz2u5yKeU</td></tr><tr><td>
Entry Status:</td><td>123 - english description</td></tr></table>";

	public void TestGetFormattedText_AcceptanceDetails()
	{
		RefCusCodeTestHelper.CreateCustomsStatusCodeListAndFrenchLanguage(Factory);
		var expectedMessageInterpretation = formattedHtml;

		var interpretation = new AcceptanceMessagePrettyFormatter(Factory, (IAcceptanceResponseDetail)MessageSchemaDecider.GetSpecificMessageAnalyzer(responseMessage).MessageDetail).GetFormattedText();
		var expectedText = expectedMessageInterpretation.Replace(System.Environment.NewLine, string.Empty);
		AssertEquals("EM_MessageInterpretation", expectedText, interpretation);
	}
}
