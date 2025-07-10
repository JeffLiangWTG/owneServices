using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ProductConditionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestProductConditions()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.EXDOCSProductCondition, "EXDPC");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, RefCusCodeListTypes.Codes.EXDOCSProductCondition, "7", "DEBARKED", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, RefCusCodeListTypes.Codes.EXDOCSProductCondition, "8", "BARK-FREE", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, RefCusCodeListTypes.Codes.EXDOCSProductCondition, "9", "DRIED", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, RefCusCodeListTypes.Codes.EXDOCSProductCondition, "10", "CHOPPED", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, RefCusCodeListTypes.Codes.EXDOCSProductCondition, "11", "PEELED", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var eXDOCHeader = invoiceHeader.QuarantineExDocHeader;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var eXDOCLine = invoiceLine.QuarantineExDocLine;

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			var productConditions = eXDOCLine.ProductConditions.AddNew();
			productConditions.CY_Code = "7";
			AssertHasMessageError(productConditions.CY_CodeInfo, "Product Conditions may only be present when Produce Type is Horticulture or Grains and Plants.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			productConditions = eXDOCLine.ProductConditions.AddNew();
			productConditions.CY_Code = "7";
			AssertNoMessageError(productConditions.CY_CodeInfo, "Product Conditions may only be present when Produce Type is Horticulture or Grains and Plants.");
			AssertHasError(productConditions.CY_CodeInfo, "This Product Condition has already been entered.");

			productConditions.CY_Code = "8";
			AssertNoError(productConditions.CY_CodeInfo, "This Product Condition has already been entered.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			productConditions = eXDOCLine.ProductConditions.AddNew();
			productConditions.CY_Code = "9";
			AssertNoMessageError(productConditions.CY_CodeInfo, "Product Conditions may only be present when Produce Type is Horticulture or Grains and Plants.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			productConditions = eXDOCLine.ProductConditions.AddNew();
			productConditions.CY_Code = "10";
			AssertHasMessageError(productConditions.CY_CodeInfo, "Product Conditions may only be present when Produce Type is Horticulture or Grains and Plants.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			productConditions = eXDOCLine.ProductConditions.AddNew();
			productConditions.CY_Code = "11";
			AssertNoMessageErrorContaining(productConditions.CY_CodeInfo, ListValidation.InvalidCodeMessageError);

			productConditions.CY_Code = "X";
			AssertHasMessageErrorContaining(productConditions.CY_CodeInfo, ListValidation.InvalidCodeMessageError);

			productConditions.CY_Code = "";
			AssertNoMessageErrorContaining(productConditions.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
