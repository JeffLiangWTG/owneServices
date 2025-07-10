using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using IESupportingDocument = Enterprise.Customs.IE.Business.Declaration.SupportingDocument;

namespace Enterprise.Customs.IE.DocumentWrappers.Testing
{
	[MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Ireland)]
	sealed class IECompactProducedDocumentsCertificatesBuilderTest : TestCaseWithFactory
	{
		public void TestAppendCountrySpecificFields()
		{
			var document = Factory.New<IESupportingDocument>();
			document.CSI_Code = "A380";
			document.CSI_ReferenceNumber = "3278923";
			document.CSI_SubType = "1";
			document.CSI_RN_NKCountryCode = "IT";
			document.CSI_ReferenceNumber2 = "REF12";
			document.CSI_ItemNumber = 1;

			var builder = new IECompactProducedDocumentsCertificatesBuilderForTest();
			var lineBuilder = new ZStringBuilder();
			builder.AppendCountrySpecificFields_Exposed(lineBuilder, document);

			AssertEquals("AppendSupportingDocument", "0-A380-3278923-1-REF12", lineBuilder.ToStringWithDelimiterBetweenAppends("-"));
		}
	}

	sealed class IECompactProducedDocumentsCertificatesBuilderForTest : IECompactProducedDocumentsCertificatesBuilder
	{
		public void AppendCountrySpecificFields_Exposed(ZStringBuilder lineSb, IESupportingDocument supportingDocument) => base.AppendCountrySpecificFields(lineSb, supportingDocument);
	}
}
