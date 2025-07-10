using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class ImportSADNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Description_WhenRequired()
		{
			CombineAssertions(() =>
			{
				var requiredImportSADNumberMessage = "Import SAD Numbers only required when Origin Type is Import.";
				declaration.ZG_OriginType = EMCSOriginTypeList.Codes.TaxWarehouse;
				importSADNumber.Validation.ValidateAll();
				AssertHasMessageError("Tax Warehouse no SAD Number", importSADNumber.CSI_DescriptionInfo, requiredImportSADNumberMessage);

				declaration.ZG_OriginType = EMCSOriginTypeList.Codes.Import;
				importSADNumber.Validation.ValidateAll();
				AssertNoMessageError("Import allows SAD Number", importSADNumber.CSI_DescriptionInfo, requiredImportSADNumberMessage);
			});
		}

		public void TestCheckCSI_Description_Mandatory()
		{
			CombineAssertions(() =>
			{
				declaration.ZG_OriginType = EMCSOriginTypeList.Codes.TaxWarehouse;
				importSADNumber.Validation.ValidateAll();
				AssertNoMessageErrorContaining("Not Mandatory for non Import", importSADNumber.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ZG_OriginType = EMCSOriginTypeList.Codes.Import;
				importSADNumber.Validation.ValidateAll();
				AssertHasMessageErrorContaining("Mandatory for Import", importSADNumber.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				importSADNumber.CSI_Description = "12AT12120829019";
				AssertNoMessageErrorContaining("Entered", importSADNumber.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCSI_Description_Duplicate()
		{
			CombineAssertions(() =>
			{
				var duplicateImportSADNumberMessage = "Import SAD Number should not be duplicated.";
				declaration.ZG_OriginType = EMCSOriginTypeList.Codes.Import;
				importSADNumber.CSI_Description = "12AT12120829019";
				var importSADNumber2 = declaration.ImportSADNumbers.AddNew();
				importSADNumber2.CSI_Description = "12AT12120829019";
				AssertHasMessageError("Duplicate SAD Numbers", importSADNumber2.CSI_DescriptionInfo, duplicateImportSADNumberMessage);

				importSADNumber2.CSI_Description = "12AT12120829020";
				AssertNoMessageError("Unique SAD Numbers", importSADNumber2.CSI_DescriptionInfo, duplicateImportSADNumberMessage);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			importSADNumber = declaration.ImportSADNumbers.AddNew();
		}
		EMCSJobDeclaration declaration;
		ImportSADNumber importSADNumber;
	}
}
