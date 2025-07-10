using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EMCSAddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_ExciseProductCode_NoWarningRequiresToBeMappedForCorrectTariff()
		{
			const string message = "The Excise Product code AX is only mapped for these tariffs.\r\n20180801, 20180802";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes);

			var cusCodeList = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "AX", ZDateTime.BrettsBirthday, ZDateTime.Now.AddYears(1));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(nameof(ExciseProductCodeAttribute.RelTrf), "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCodeList.Attributes.AddNew(nameof(ExciseProductCodeAttribute.RelTrf), "20180802");
			cusCodeList.Attributes.AddNew(nameof(ExciseProductCodeAttribute.RelTrf), "20180801");

			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "BY", ZDateTime.BrettsBirthday, ZDateTime.Now.AddYears(1));

			Factory.Save();

			invoiceLine.JI_Tariff = "20180803";
			invoiceLine.ZG_ExciseProductCode = "AX";

			AssertEquals(false, invoiceLine.ZG_ExciseProductCodeInfo.HasWarning(message));
		}

		public void TestCheckZG_ExciseProductCodeW200_NotAllowedForDomesticEMCSMovements()
		{
			const string msg = "An Excise Product Code of 'W200' is not allowed for domestic EMCS movements.";
			var info = invoiceLine.ZG_ExciseProductCodeInfo;

			var supplier = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			supplier.OH_Code = "ABC123";

			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			importer.OH_Code = "123ABC";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, EMCSJobComInvoiceLine.ExciseProductCode_W200, ZDateTime.BrettsBirthday, ZDateTime.Now.AddYears(1));

			Factory.Save();

			supplier.OH_RL_NKClosestPort = "DEFRA";
			importer.OH_RL_NKClosestPort = "DEFRA";
			invoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
			validation.ValidateZG_ExciseProductCode();
			AssertHasMessageError(info, msg);

			supplier.OH_RL_NKClosestPort = "USLAX";
			importer.OH_RL_NKClosestPort = "DEFRA";
			invoiceLine.AddInfoValidation.ValidateZG_ExciseProductCode();
			AssertNoMessageError(info, msg);

			supplier.OH_RL_NKClosestPort = "DEFRA";
			importer.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.AddInfoValidation.ValidateZG_ExciseProductCode();
			AssertNoMessageError(info, msg);

			supplier.OH_RL_NKClosestPort = "DEFRA";
			importer.OH_RL_NKClosestPort = "DEFRA";
			invoiceLine.ZG_ExciseProductCode = "E200";
			AssertNoMessageError(info, msg);
		}

		public void TestCheckZG_ExciseProductCodeIfDeclarationIsConsolidatedDocument()
		{
			const string messageError = "The Excise Code 'W200' and all 'E'-Codes are invalid for Consolidated Documents.";
			CombineAssertions(() =>
			{
				validation.ValidateZG_ExciseProductCode();
				AssertNoMessageError("No ConsolidatedDocument and empty ExciseProductCode", invoiceLine.ZG_ExciseProductCodeInfo, messageError);
				invoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
				AssertNoMessageError("No ConsolidatedDocument and ExciseProductCode == W200", invoiceLine.ZG_ExciseProductCodeInfo, messageError);
				invoiceLine.ZG_ExciseProductCode = "E200";
				AssertNoMessageError("No ConsolidatedDocument and ExciseProductCode starts with 'E'", invoiceLine.ZG_ExciseProductCodeInfo, messageError);
				declaration.SetConsolidatedDocument();
				validation.ValidateZG_ExciseProductCode();
				AssertHasMessageError("Is ConsolidatedDocument and ExciseProductCode starts with 'E'", invoiceLine.ZG_ExciseProductCodeInfo, messageError);
				invoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
				AssertHasMessageError("Is ConsolidatedDocument and ExciseProductCode == W200", invoiceLine.ZG_ExciseProductCodeInfo, messageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			invoiceLine = declaration.InvoiceHeader.InvoiceLines.AddNew();
			validation = new EMCSAddInfoJobComInvoiceLineValidation((EMCSAddInfoJobComInvoiceLine)((Customs.Business.IAddInfoManagerWithSchema)invoiceLine).AddInfo);
		}
		EMCSJobComInvoiceLine invoiceLine;
		EMCSJobDeclaration declaration;
		EMCSAddInfoJobComInvoiceLineValidation validation;
	}
}
