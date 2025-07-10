using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class InvoiceHeaderAdditionalInfoValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
{
	public void TestCheckCSI_SubType_MandatoryValidation()
	{
		invoiceHeaderAdditionalInfo.CSI_SubType = "";
		AssertHasMessageErrorContaining(invoiceHeaderAdditionalInfo.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceHeaderAdditionalInfo.CSI_SubType = "123";
		AssertNoMessageErrorContaining(invoiceHeaderAdditionalInfo.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_SubType_ListValidation()
	{
		invoiceHeaderAdditionalInfo.CSI_SubType = "XXX";
		AssertHasMessageErrorContaining(invoiceHeaderAdditionalInfo.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError);

		invoiceHeaderAdditionalInfo.CSI_SubType = "INF";
		AssertNoMessageErrorContaining(invoiceHeaderAdditionalInfo.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckCSI_Code_MandatoryValidation()
	{
		invoiceHeaderAdditionalInfo.CSI_Code = "";
		AssertHasMessageErrorContaining(invoiceHeaderAdditionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceHeaderAdditionalInfo.CSI_Code = "123";
		AssertNoMessageErrorContaining(invoiceHeaderAdditionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_Code_ListValidation()
	{
		SetUpReferenceData();
		Factory.Save();

		invoiceHeaderAdditionalInfo.CSI_SubType = "INF";
		invoiceHeaderAdditionalInfo.CSI_Code = "XXX";
		AssertHasMessageErrorContaining(invoiceHeaderAdditionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

		invoiceHeaderAdditionalInfo.CSI_Code = "AI";
		AssertNoMessageErrorContaining(invoiceHeaderAdditionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckCSI_ReferenceNumber_MandatoryValidation()
	{
		invoiceHeaderAdditionalInfo.CSI_SubType = "REF";
		invoiceHeaderAdditionalInfo.CSI_ReferenceNumber = "";
		AssertHasMessageErrorContaining(invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceHeaderAdditionalInfo.CSI_ReferenceNumber = "REFNO";
		AssertNoMessageErrorContaining(invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		invoiceHeaderAdditionalInfo.CSI_SubType = "INF";
		invoiceHeaderAdditionalInfo.CSI_ReferenceNumber = "";
		AssertNoMessageErrorContaining(invoiceHeaderAdditionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
	}

	void SetUpReferenceData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping("IT", "Italy");

		helper.CreateNewOrGetExistingCusCodeType("AI44E", "Additional Information Code Type");
		helper.CreateNewOrGetExistingCusCodeList("IT", "AI44E", "AI", "Additional Information Code List", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoice = declaration.Invoices.AddNew();
		invoiceHeaderAdditionalInfo = invoice.AdditionalInfos.AddNew();
	}

	InvoiceHeaderAdditionalInfo invoiceHeaderAdditionalInfo;
}
