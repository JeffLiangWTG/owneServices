using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
	sealed class CompactProducedDocumentsCertificatesBuilderTest : TestCaseWithFactory
	{
		public void TestAppendSupportingDocument_Phase5()
		{
			var document = Factory.New<SupportingDocument>();
			document.CSI_Code = "A380";
			document.CSI_ReferenceNumber = "3278923";
			document.CSI_SubType = "1";
			document.CSI_Quantity = 12;
			document.CSI_DateOfIssue = new ZDate(2021, 12, 10);
			document.CSI_RN_NKCountryCode = "IT";

			var builder = new CompactProducedDocumentsCertificatesBuilderForTest();
			var lineBuilder = new ZStringBuilder();
			builder.AppendSupportingDocument_Exposed(lineBuilder, document, true);

			AssertEquals("AppendSupportingDocument", "A380-3278923", lineBuilder.ToStringWithDelimiterBetweenAppends("-"));
		}

		public void TestAppendCountrySpecificFields()
		{
			var document = Factory.New<SupportingDocument>();
			document.CSI_Code = "A380";
			document.CSI_ReferenceNumber = "3278923";
			document.CSI_SubType = "1";
			document.CSI_Quantity = 0;
			document.CSI_DateOfIssue = new ZDate(2021, 12, 10);
			document.CSI_RN_NKCountryCode = "IT";
			document.CSI_Description = "TST_DECR";
			var builder = new CompactProducedDocumentsCertificatesBuilderForTest();
			var lineBuilder = new ZStringBuilder();
			builder.AppendCountrySpecificFields_Exposed(lineBuilder, document);

			AssertEquals("AppendCountrySpecificFields", "A380-3278923-TST_DECR", lineBuilder.ToStringWithDelimiterBetweenAppends("-"));
		}
	}
}
