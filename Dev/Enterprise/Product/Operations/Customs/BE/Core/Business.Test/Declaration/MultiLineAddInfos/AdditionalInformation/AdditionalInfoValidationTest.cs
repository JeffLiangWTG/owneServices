using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
{
	public void TestCSI_Code_ExportInvoiceHeader_Mandatory()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(GetAdditionalInfo().CSI_CodeInfo);
	}

	public void TestCSI_Code_ExportInvoiceHeader_ListValidation()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(GetAdditionalInfo().CSI_ReferenceNumberInfo);
	}

	public void TestCheckCSI_ReferenceNumber_Mandatory()
	{
		var additionalInfo = GetAdditionalInfo();
		additionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalInformation;
		additionalInfo.CSI_ReferenceNumber = string.Empty;

		CombineAssertions(() =>
		{
			AssertNoMessageError("Reference should not be mandatory when ReadOnly", additionalInfo.CSI_ReferenceNumberInfo, "You have not entered a Reference.");

			additionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalReference;
			additionalInfo.CSI_ReferenceNumber = string.Empty;
			AssertHasMessageError("Reference should not be mandatory when not ReadOnly", additionalInfo.CSI_ReferenceNumberInfo, "You have not entered a Reference.");
		});
	}

	public void TestCheckCSI_SubType_ExportInvoiceHeader()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(GetAdditionalInfo().CSI_SubTypeInfo, "INV", BEAdditionalDocTypeList.Codes.TransportDocuments);
	}

	AdditionalInfo GetAdditionalInfo()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		return invoice.AdditionalInfos.AddNew();
	}
}
