using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.BR.Manifest.Business.Test
{
	public class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPA_PackUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "Packs");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Brazil);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, RefCusCodeListTypes.Codes.PackageTypes, "AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var bg = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, RefCusCodeListTypes.Codes.PackageTypes, "BG", "BG DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(bg.PK, RefCusCodeList.Attributes.Bulk, RefCusCodeList.AttributeValues.Bulk);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, RefCusCodeListTypes.Codes.PackageTypes, "BAG", "BBB DESC", new ZDateTime(1994, 3, 3), ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			pack.APA_PackUQ = string.Empty;
			AssertHasMessageErrorContaining(pack.APA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);

			pack.APA_PackUQ = "12";
			AssertHasMessageErrorContaining(pack.APA_PackUQInfo, ListValidation.InvalidCodeMessageError);
			pack.APA_PackUQ = pack.Lookups.PackUQList[0].Code;
			AssertNoNotifications(pack.APA_PackUQInfo);
		}

		public void TestCheckBulkType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			pack.Validation.ValidateAll();
			AssertHasMessageErrorContaining(pack.BulkTypeInfo, "Bulk Type cannot be empty if Container Mode is Bulk");

			pack.BulkType = BRBulkTypeList.Codes._10;
			AssertNoMessageErrorContaining(pack.BulkTypeInfo, "Bulk Type cannot be empty if Container Mode is Bulk");

			ValidationTestHelper.AssertInvalidCodeMessageError(pack.BulkTypeInfo, "X", BRBulkTypeList.Codes._1);
		}
	}
}
