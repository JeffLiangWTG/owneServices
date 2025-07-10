using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ExportJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public override void TestCheckJE_CustomsOffice()
		{
			base.TestCheckJE_CustomsOffice();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_CustomsOfficeInfo);
		}

		public override void TestCheckJE_LocationOfGoods()
		{
			base.TestCheckJE_LocationOfGoods();

			declaration.ClearanceOfficeIsCustomsEnclosure = false;
			declaration.JE_LocationOfGoods = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ClearanceOfficeIsCustomsEnclosure = true;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_LocationOfGoodsInfo);
		}

		public void TestCheckJE_DeclarantType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_DeclarantTypeInfo, "XX", TypeOfOperationExportList.Codes._1001);
		}

		public void TestCheckBoardingOfficeCode()
		{
			declaration.BoardingOfficeCode = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.BoardingOfficeCodeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.BoardingOfficeCode = "8655439";
			AssertHasMessageErrorContaining(declaration.BoardingOfficeCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBoardingEnclosureCode()
		{
			AssertNoMessageErrorContaining(declaration.BoardingEnclosureCodeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.BoardingOfficeIsCustomsEnclosure = true;
			declaration.BoardingEnclosureCode = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.BoardingEnclosureCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.BoardingEnclosureCode = "8655439";
			AssertHasMessageErrorContaining(declaration.BoardingEnclosureCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJE_OA_DeclarantAddress()
		{
			var orgHeaderCnpj = Factory.New<OrgHeader>();
			orgHeaderCnpj.OH_Code = "TCJN";
			orgHeaderCnpj.PrimaryRegistrationNumber.NumberTypeForDisplay = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			orgHeaderCnpj.PrimaryRegistrationNumber.Number = "01001001000101";
			var orgAddressCnpj = orgHeaderCnpj.Addresses.AddNew();
			orgAddressCnpj.OA_Address1 = "TEST1";

			var orgHeaderCpf = Factory.New<OrgHeader>();
			orgHeaderCpf.OH_Code = "TCPF";
			orgHeaderCpf.PrimaryRegistrationNumber.NumberTypeForDisplay = BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration;
			orgHeaderCpf.PrimaryRegistrationNumber.Number = "02764703384";
			var orgAddressCpf = orgHeaderCpf.Addresses.AddNew();
			orgAddressCpf.OA_Address1 = "TEST1";

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1002;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1003;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1002;
			declaration.JE_OA_DeclarantAddress = orgAddressCnpj.PK;
			AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, "The Declarant Registration Number differs from CJN - Business Taxpayer Registration.");
			AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, "Declarant Registration Number not found.");

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1002;
			declaration.JE_OA_DeclarantAddress = orgAddressCpf.PK;
			AssertHasMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, "The Declarant Registration Number differs from CJN - Business Taxpayer Registration.");
			AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, "Declarant Registration Number not found.");

			orgHeaderCnpj.PrimaryRegistrationNumber.Number = ZString.Empty;
			declaration.JE_OA_DeclarantAddress = orgAddressCnpj.PK;
			AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, "The Declarant Registration Number differs from CJN - Business Taxpayer Registration.");
			AssertHasMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, "Declarant Registration Number not found.");

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			declaration.JE_OA_DeclarantAddress = orgAddressCpf.PK;
			AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, "The Declarant Registration Number differs from CJN - Business Taxpayer Registration.");
			AssertNoMessageErrorContaining(declaration.JE_OA_DeclarantAddressInfo, "Declarant Registration Number not found.");
		}

		public void TestCheckJE_OH_Supplier()
		{
			var orgHeaderCnpj = Factory.New<OrgHeader>();
			orgHeaderCnpj.OH_Code = "TCJN";
			orgHeaderCnpj.PrimaryRegistrationNumber.NumberTypeForDisplay = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			orgHeaderCnpj.PrimaryRegistrationNumber.Number = "01001001000101";

			var orgHeaderCpf = Factory.New<OrgHeader>();
			orgHeaderCpf.OH_Code = "TCPF";
			orgHeaderCpf.PrimaryRegistrationNumber.NumberTypeForDisplay = BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration;
			orgHeaderCpf.PrimaryRegistrationNumber.Number = "02764703384";

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1002;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertNoMessageErrorContaining(declaration.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1003;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertNoMessageErrorContaining(declaration.JE_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			declaration.JE_OH_Supplier = orgHeaderCnpj.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_SupplierInfo, "The Main Supplier Registration Number differs from CJN - Business Taxpayer Registration.");
			AssertNoMessageErrorContaining(declaration.JE_OH_SupplierInfo, "Main Supplier Registration Number not found.");

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			declaration.JE_OH_Supplier = orgHeaderCpf.PK;
			AssertHasMessageErrorContaining(declaration.JE_OH_SupplierInfo, "The Main Supplier Registration Number differs from CJN - Business Taxpayer Registration.");
			AssertNoMessageErrorContaining(declaration.JE_OH_SupplierInfo, "Main Supplier Registration Number not found.");

			orgHeaderCnpj.PrimaryRegistrationNumber.Number = ZString.Empty;
			declaration.JE_OH_Supplier = orgHeaderCnpj.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_SupplierInfo, "The Main Supplier Registration Number differs from CJN - Business Taxpayer Registration.");
			AssertHasMessageErrorContaining(declaration.JE_OH_SupplierInfo, "Main Supplier Registration Number not found.");
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.Export;
	}
}
