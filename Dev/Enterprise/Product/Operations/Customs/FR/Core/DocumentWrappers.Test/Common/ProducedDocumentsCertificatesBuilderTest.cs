using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.DocumentWrappers.Common.Testing;

sealed class ProducedDocumentsCertificatesBuilderTest : TestCaseWithFactory
{
	public void TestAppendSupportingDocument_CSI_IsDTP_False()
	{
		var document = Factory.New<SupportingDocument>();
		document.CSI_Code = "9100";
		document.CSI_ReferenceNumber = "3278923";
		document.CSI_SubType = "1";
		document.CSI_Quantity = 12;
		document.CSI_Description = "SIC TRANSIT GLORIA MUNDI";
		document.CSI_DateOfIssue = new ZDate(2021, 12, 10);
		document.CSI_IsDTP = false;

		var result = new ZStringBuilder();
		var builder = new ProducedDocumentsCertificatesBuilderForTest();
		builder.AppendSupportingDocument_Exposed(result, document);

		AssertEquals("CSI_IsDTP True", "9100 3278923 10/12/2021", result.ToString());
	}

	public void TestAppendSupportingDocument_CSI_IsDTP_True()
	{
		var document = Factory.New<SupportingDocument>();
		document.CSI_Code = "9100";
		document.CSI_ReferenceNumber = "3278923";
		document.CSI_SubType = "1";
		document.CSI_Quantity = 12;
		document.CSI_Description = "SIC TRANSIT GLORIA MUNDI";
		document.CSI_DateOfIssue = new ZDate(2021, 12, 10);
		document.CSI_IsDTP = true;

		var result = new ZStringBuilder();
		var builder = new ProducedDocumentsCertificatesBuilderForTest();
		builder.AppendSupportingDocument_Exposed(result, document);

		AssertEquals("CSI_IsDTP False", ZString.Empty, result.ToString());
	}

	public void TestAppendSupportingDocument_D48()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
		helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Enterprise.Customs.FR.Business.UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var document = declaration.SupportingDocuments.AddNew();
		document.CSI_Code = "0001";
		document.CSI_ReferenceNumber = "3278923";
		document.CSI_SubType = "1";
		document.CSI_Quantity = 12;
		document.CSI_Description = "SIC TRANSIT GLORIA MUNDI";
		document.CSI_DateOfIssue = new ZDate(2021, 12, 10);
		document.CSI_IsDTP = false;
		document.CSI_Value = 400.00m;
		document.CSI_Quantity3 = 2.00m;

		var result = new ZStringBuilder();
		var builder = new ProducedDocumentsCertificatesBuilderForTest();
		builder.AppendSupportingDocument_Exposed(result, document);

		AssertEquals("Should include D48 content", "0001 3278923 10/12/2021 (D48 : 400 - 2)", result.ToString());

		document.CSI_Code = "9001";
		result = new ZStringBuilder();
		builder.AppendSupportingDocument_Exposed(result, document);
		AssertEquals("Should not include D48 content when not IsD48AndNotClosed", "9001 3278923 10/12/2021", result.ToString());
	}

	class ProducedDocumentsCertificatesBuilderForTest : ProducedDocumentsCertificatesBuilder
	{
		public void AppendSupportingDocument_Exposed(ZStringBuilder result, SupportingDocument supportingDocument) => base.AppendSupportingDocument(result, supportingDocument);
	}
}
