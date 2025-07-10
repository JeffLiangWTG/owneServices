using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CusAuthorisationHeaderExtensionsTest : TestCaseWithFactory
{
	public void TestSupportImportSupportingDocumentsRule()
	{
		authorisationHeader.CPH_Type = "";
		AssertEquals("[CPH_Type is Empty] SupportImportSupportingDocumentsRule", false, authorisationHeader.SupportImportSupportingDocumentsRule());

		authorisationHeader.CPH_Type = "EIR";
		AssertEquals("[CPH_Type is EIR] SupportImportSupportingDocumentsRule", false, authorisationHeader.SupportImportSupportingDocumentsRule());

		authorisationHeader.CPH_Type = "CWP";
		AssertEquals("[CPH_Type is CWP] SupportImportSupportingDocumentsRule", true, authorisationHeader.SupportImportSupportingDocumentsRule());

		AssertNoExceptionThrown("No exception thrown if authorisationHeader is null", () => CusAuthorisationHeaderExtensions.SupportImportSupportingDocumentsRule(null));
	}

	public void TestSupportExportSupportingDocumentsRule()
	{
		authorisationHeader.CPH_Type = "";
		AssertEquals("[CPH_Type is Empty] SupportExportSupportingDocumentsRule", false, authorisationHeader.SupportExportSupportingDocumentsRule());

		authorisationHeader.CPH_Type = "CWP";
		AssertEquals("[CPH_Type is CWP] SupportExportSupportingDocumentsRule", false, authorisationHeader.SupportExportSupportingDocumentsRule());

		authorisationHeader.CPH_Type = "OPO";
		AssertEquals("[CPH_Type is OPO SupportExportSupportingDocumentsRule", true, authorisationHeader.SupportExportSupportingDocumentsRule());

		AssertNoExceptionThrown("No exception thrown if authorisationHeader is null", () => CusAuthorisationHeaderExtensions.SupportExportSupportingDocumentsRule(null));
	}

	public void TestSupportNctsSupportingDocumentsRule()
	{
		authorisationHeader.CPH_Type = "";
		AssertEquals("[CPH_Type is Empty] SupportNctsSupportingDocumentsRule", false, authorisationHeader.SupportNctsSupportingDocumentsRule());

		authorisationHeader.CPH_Type = "CWP";
		AssertEquals("[CPH_Type is CWP] SupportNctsSupportingDocumentsRule", false, authorisationHeader.SupportNctsSupportingDocumentsRule());

		authorisationHeader.CPH_Type = "ACR";
		AssertEquals("[CPH_Type is ACR] SupportNctsSupportingDocumentsRule", true, authorisationHeader.SupportNctsSupportingDocumentsRule());

		AssertNoExceptionThrown("No exception thrown if authorisationHeader is null", () => CusAuthorisationHeaderExtensions.SupportNctsSupportingDocumentsRule(null));
	}

	public void TestGetSupportedDocumentRefCusCodeListTypeCode()
	{
		authorisationHeader.CPH_Type = "";
		AssertEquals("[CPH_Type is Empty] GetSupportedDocumentRefCusCodeListTypeCode", "", authorisationHeader.GetSupportedDocumentRefCusCodeListTypeCode());

		authorisationHeader.CPH_Type = "EIR";
		AssertEquals("[CPH_Type is EIR] GetSupportedDocumentRefCusCodeListTypeCode", "", authorisationHeader.GetSupportedDocumentRefCusCodeListTypeCode());

		authorisationHeader.CPH_Type = "CWP";
		AssertEquals("[CPH_Type is CWP] GetSupportedDocumentRefCusCodeListTypeCode", "DC44I", authorisationHeader.GetSupportedDocumentRefCusCodeListTypeCode());

		authorisationHeader.CPH_Type = "OPO";
		AssertEquals("[CPH_Type is CWP] GetSupportedDocumentRefCusCodeListTypeCode", "DC44E", authorisationHeader.GetSupportedDocumentRefCusCodeListTypeCode());

		authorisationHeader.CPH_Type = "ACR";
		AssertEquals("[CPH_Type is ACR] GetSupportedDocumentRefCusCodeListTypeCode", "DC44N", authorisationHeader.GetSupportedDocumentRefCusCodeListTypeCode());

		AssertNoExceptionThrown("No exception thrown if authorisationHeader is null", () => CusAuthorisationHeaderExtensions.GetSupportedDocumentRefCusCodeListTypeCode(null));
	}

	public void TestSupportImportLocationQualifierLB()
	{
		authorisationHeader.CPH_Type = "";
		AssertEquals("[CPH_Type is Empty] SupportImportLocationQualifierLB", false, authorisationHeader.SupportImportLocationQualifierLB());

		authorisationHeader.CPH_Type = "CWP";
		AssertEquals("[CPH_Type is CWP] SupportImportLocationQualifierLB", true, authorisationHeader.SupportImportLocationQualifierLB());

		authorisationHeader.CPH_Type = "CW1";
		AssertEquals("[CPH_Type is CW1] SupportImportLocationQualifierLB", true, authorisationHeader.SupportImportLocationQualifierLB());

		authorisationHeader.CPH_Type = "CW2";
		AssertEquals("[CPH_Type is CW2] SupportImportLocationQualifierLB", true, authorisationHeader.SupportImportLocationQualifierLB());

		authorisationHeader.CPH_Type = "XYZ";
		AssertEquals("[CPH_Type is XYZ] SupportImportLocationQualifierLB", false, authorisationHeader.SupportImportLocationQualifierLB());

		authorisationHeader = null;
		AssertNoExceptionThrown("When AuthorisationHeader is null", () => authorisationHeader.SupportImportLocationQualifierLB());
	}

	public void TestSupportImportLocationQualifierLC()
	{
		authorisationHeader.CPH_Type = "";
		AssertEquals("[CPH_Type is Empty] SupportImportLocationQualifierLC", false, authorisationHeader.SupportImportLocationQualifierLC());

		authorisationHeader.CPH_Type = "ALI";
		AssertEquals("[CPH_Type is ALI] SupportImportLocationQualifierLC", true, authorisationHeader.SupportImportLocationQualifierLC());

		authorisationHeader.CPH_Type = "ALE";
		AssertEquals("[CPH_Type is ALE] SupportImportLocationQualifierLC", true, authorisationHeader.SupportImportLocationQualifierLC());

		authorisationHeader.CPH_Type = "XYZ";
		AssertEquals("[CPH_Type is XYZ] SupportImportLocationQualifierLC", false, authorisationHeader.SupportImportLocationQualifierLC());

		authorisationHeader = null;
		AssertNoExceptionThrown("When AuthorisationHeader is null", () => authorisationHeader.SupportImportLocationQualifierLC());
	}

	protected override void SetUp()
	{
		base.SetUp();

		authorisationHeader = Factory.New<CusAuthorisationHeader>();
	}
	CusAuthorisationHeader authorisationHeader;
}
