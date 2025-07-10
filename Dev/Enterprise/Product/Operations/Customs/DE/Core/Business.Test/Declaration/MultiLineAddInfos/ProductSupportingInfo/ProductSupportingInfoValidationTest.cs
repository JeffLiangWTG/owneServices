using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	public class ProductSupportingInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Description()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			var product = entryInstruction.Products.AddNew();

			if (entryInstruction.EnabledOutwardProcessing)
			{
				product.Validation.ValidateCSI_Description();
				AssertHasMessageErrorContaining(product.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				product.CSI_Description = "this is desc";
				AssertNoMessageErrorContaining(product.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckCSI_Tariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			var product = entryInstruction.Products.AddNew();

			if (entryInstruction.EnabledOutwardProcessing)
			{
				product.Validation.ValidateCSI_Tariff();
				AssertHasMessageErrorContaining(product.CSI_TariffInfo, MandatoryValidation.YouHaveNotEntered);

				product.CSI_Tariff = "0702001000";
				AssertNoMessageErrorContaining(product.CSI_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			}
			else
			{
				Assert(true);
			}
		}
	}
}
