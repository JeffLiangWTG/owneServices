using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class CustomsRejectionMessagePrettyFormatterTest : TestCase
{
	protected ZString BGMReference => "0100011210809998";

	protected ZString customsRejectionType = "correctionRejection";

	#region Message Response

	protected ZString MessageResponse => $@"<goodsDeclarationsResponse schemaVersion=""4.0""
				xmlns=""http://www.e-dec.ch/xml/schema/edecResponse/v4""
				xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
				xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecResponse/v4 http://www.ezv.admin.ch/pdf_linker.php?doc=edecResponse_v_4_0"">
			<goodsDeclarationRejection>
				<rejectionDate>2022-07-28</rejectionDate>
				<rejectionTime>11:36:24</rejectionTime>
				<errors>
					<customsRejection>
						<traderDeclarationNumber>{BGMReference}</traderDeclarationNumber>
						<traderReference>Test_NI_Korrektur</traderReference>
						<declarant>
							<traderIdentificationNumber>CHE375081047</traderIdentificationNumber>
							<declarantNumber>72</declarantNumber>
						</declarant>
						<type>{customsRejectionType}</type>
					</customsRejection>
				</errors>
			</goodsDeclarationRejection>
		</goodsDeclarationsResponse>";

	#endregion

	public void TestRejectedCorrectionMessageFormattedText()
	{
		customsRejectionType = "correctionRejection";
		AssertCustomsRejectedMessageFormattedText(CH.Business.CustomsRejectionMessagePrettyFormatter.ContentCorrectionRejectionText, CH.Business.CustomsRejectionMessagePrettyFormatter.ContentCancellationRejectionText);
	}

	public void TestRejectedCancellationMessageFormattedText()
	{
		customsRejectionType = "cancellationRejection";
		AssertCustomsRejectedMessageFormattedText(CH.Business.CustomsRejectionMessagePrettyFormatter.ContentCancellationRejectionText, CH.Business.CustomsRejectionMessagePrettyFormatter.ContentCorrectionRejectionText);
	}

	void AssertCustomsRejectedMessageFormattedText(string expectedText, string unexpectedText)
	{
		var interpretation = new CustomsRejectionMessagePrettyFormatter((ICustomsRejectionResponseDetail)MessageSchemaDecider.GetSpecificMessageAnalyzer(MessageResponse).MessageDetail).GetFormattedText();

		AssertEquals($"Content Header: {CH.Business.CustomsRejectionMessagePrettyFormatter.HeaderText}", true, interpretation.Contains($"<h2>{CH.Business.CustomsRejectionMessagePrettyFormatter.HeaderText}</h2>"));
		AssertEquals($"Content text expected: {expectedText}", true, interpretation.Contains($"<p>{expectedText}"));
		AssertEquals($"Content text unexpected: {unexpectedText}", false, interpretation.Contains($"<p>{unexpectedText}"));
	}
}
