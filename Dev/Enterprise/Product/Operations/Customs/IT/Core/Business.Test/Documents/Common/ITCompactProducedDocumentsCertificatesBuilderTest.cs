using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using ITSupportingDocument = Enterprise.Customs.IT.Business.Declaration.SupportingDocument;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITCompactProducedDocumentsCertificatesBuilderTest : TestCaseWithFactory
{
	public void TestAppendCountrySpecificFields()
	{
		var document = Factory.New<ITSupportingDocument>();
		document.CSI_Code = "A380";
		document.CSI_ReferenceNumber = "3278923";
		document.CSI_SubType = "1";
		document.CSI_Quantity = 12;
		document.CSI_DateOfIssue = new ZDate(2021, 12, 10);
		document.CSI_RN_NKCountryCode = "IT";

		var builder = new ITCompactProducedDocumentsCertificatesBuilderForTest();
		var lineBuilder = new ZStringBuilder();
		builder.AppendCountrySpecificFields_Exposed(lineBuilder, document);

		AssertEquals("AppendSupportingDocument", "A380-IT-2021-3278923-12", lineBuilder.ToStringWithDelimiterBetweenAppends("-"));
	}
}

sealed class ITCompactProducedDocumentsCertificatesBuilderForTest : ITCompactProducedDocumentsCertificatesBuilder
{
	public void AppendCountrySpecificFields_Exposed(ZStringBuilder lineSb, ITSupportingDocument supportingDocument) => base.AppendCountrySpecificFields(lineSb, supportingDocument);
}
