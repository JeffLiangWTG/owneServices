using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	public class AsycudaPackedItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPI_GoodsDescription()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var manifestBill = manifestHeader.Bills.AddNew();
			var packedItem = manifestBill.PackedItems.AddNew();
			packedItem.API_GoodsDescription = "";
			AssertHasMessageErrorContaining(packedItem.API_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			packedItem.API_GoodsDescription = "abcd";
			AssertNoMessageErrorContaining(packedItem.API_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			packedItem.API_GoodsDescription = new string('A', 257);
			AssertEquals(packedItem.API_GoodsDescription.Length, 256);
		}

		public void TestCheckAPI_GrossWeight()
		{
			const string expectedMessage = "The entered value must be greater than 0.";
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var manifestBill = manifestHeader.Bills.AddNew();
			var packedItem = manifestBill.PackedItems.AddNew();
			packedItem.API_GrossWeight = 0;
			AssertHasMessageError(packedItem.API_GrossWeightInfo, expectedMessage);
			packedItem.API_GrossWeight = -1;
			AssertHasMessageError(packedItem.API_GrossWeightInfo, expectedMessage);
			packedItem.API_GrossWeight = 1;
			AssertNoMessageErrors(packedItem.API_GrossWeightInfo);
		}

		public void TestCheckAPI_GrossWeightUQ()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var manifestBill = manifestHeader.Bills.AddNew();
			var packedItem = manifestBill.PackedItems.AddNew();
			ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(packedItem.API_GrossWeightUQInfo, packedItem.API_GrossWeightInfo);
		}

		public void TestCheckAPI_PackStatus_RoleCC_BR1_WCO_024()
		{
			const string expectedMessage = "DG Substance must be provided when cargo status = 'D'";
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var manifestBill = manifestHeader.Bills.AddNew();
			var packedItem = manifestBill.PackedItems.AddNew();

			var info = packedItem.API_PackStatusInfo;
			packedItem.API_PackStatus = "N";
			AssertNoMessageError("When status=N", info, expectedMessage);

			packedItem.API_PackStatus = "D";
			AssertHasMessageError("When status=D, UNDG list is empty", info, expectedMessage);

			CreateValidDG(packedItem);

			packedItem.Validation.ValidateAPI_PackStatus();
			AssertNoMessageError("When status=D, all UNDG items are valid", info, expectedMessage);

			packedItem.UNDGs.Single().DI_DG = ZGuid.Empty;

			packedItem.Validation.ValidateAPI_PackStatus();
			AssertHasMessageError("When status=D, there is at least one DG without UNDGSubstance", info, expectedMessage);
		}

		public void TestCheckAPI_PackStatus_RoleCC_BR1_WCO_026()
		{
			const string expectedMessage = "UNDG Contact must be provided when cargo status = 'D'";
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var manifestBill = manifestHeader.Bills.AddNew();
			var packedItem = manifestBill.PackedItems.AddNew();

			var info = packedItem.API_PackStatusInfo;
			packedItem.API_PackStatus = "N";
			AssertNoMessageError("When status=N", info, expectedMessage);

			packedItem.API_PackStatus = "D";
			AssertHasMessageError("When status=D, UNDG list is empty", info, expectedMessage);

			CreateValidDG(packedItem);

			packedItem.Validation.ValidateAPI_PackStatus();
			AssertNoMessageError("When status=D, all UNDG items are valid", info, expectedMessage);

			packedItem.UNDGs.Single().DI_OC_DGContact = ZGuid.Empty;

			packedItem.Validation.ValidateAPI_PackStatus();
			AssertHasMessageError("When status=D, there is at least one DG without Contact", info, expectedMessage);
		}

		public void TestValidateBillItemsLinkedToPackage()
		{
			const string expectedMessageError = "All bill items must be linked to a package.";
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var manifestBill = manifestHeader.Bills.AddNew();
			var packedItem = manifestBill.PackedItems.AddNew();
			packedItem.Validation.ValidateAll();
			AssertHasRowMessageError("When AsycudaLinkPackages.Count == 0", packedItem, expectedMessageError);

			var linkPackage = packedItem.AsycudaLinkPackages.AddNew();
			var pack = manifestBill.Packs.AddNew();
			pack.APA_PackQty = 10;
			pack.APA_MarksAndNumbers = "mark";
			linkPackage.Package = pack;

			linkPackage.IsLinked = true;
			packedItem.Validation.ValidateAll();
			AssertNoRowMessageError("When we have one Package that IsLinked", packedItem, expectedMessageError);

			var pack2 = manifestBill.Packs.AddNew();
			var linkPackage2 = packedItem.AsycudaLinkPackages.AddNew();
			pack2.APA_PackQty = 10;
			pack2.APA_MarksAndNumbers = "mark";
			linkPackage2.Package = pack2;
			linkPackage2.IsLinked = false;
			packedItem.Validation.ValidateAll();
			AssertNoRowMessageError("When we have at least one Package that IsLinked", packedItem, expectedMessageError);

			linkPackage.IsLinked = false;
			packedItem.Validation.ValidateAll();
			AssertHasRowMessageError("When we have not Any Package that IsLinked", packedItem, expectedMessageError);
		}

		public void TestCheckAPI_Tariff()
		{
			const string expectedHarmonizedCodeMessage = "Harmonized Code is less than 4 digits.";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			header.AMA_ManifestType = "785";
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItems.AddNewPackedItem();

			CombineAssertions("Tariff code with less than 4 digits should trigger an error", () =>
			{
				packedItem.API_Tariff = "123";
				AssertHasMessageError(packedItem.API_TariffInfo, expectedHarmonizedCodeMessage);
				AssertNoMessageErrorContaining(packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("Tariff code with 4 digits should not trigger an error", () =>
			{
				packedItem.API_Tariff = "1234";
				AssertNoMessageError(packedItem.API_TariffInfo, expectedHarmonizedCodeMessage);
				AssertNoMessageErrorContaining(packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			});

			CombineAssertions("Empty tariff code should trigger an error", () =>
			{
				packedItem.API_Tariff = ZString.Empty;
				AssertNoMessageError(packedItem.API_TariffInfo, expectedHarmonizedCodeMessage);
				AssertHasMessageErrorContaining(packedItem.API_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestTariffListValidationCore()
		{
			const string expectedTariffCodeInListMessage = "The code you have selected is not in the list.";
			var countryCode = Core.Constants.CountryCodes.Israel;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(countryCode, Universal.Constants.TariffTypes.Import);
			helper.LoadOrCreateNewTariff(countryCode, tariffType.PK, "0101", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));

			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			header.AMA_ManifestType = "785";
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItems.AddNewPackedItem();

			packedItem.API_Tariff = "010";
			AssertNoMessageError("Tariff code with less than 4 digits should not trigger list error", packedItem.API_TariffInfo, expectedTariffCodeInListMessage);

			packedItem.API_Tariff = "01022";
			AssertHasMessageError("Tariff code with invalid first 4 digits should trigger list error", packedItem.API_TariffInfo, expectedTariffCodeInListMessage);

			packedItem.API_Tariff = "01013";
			AssertNoMessageError("Tariff code with valid first 4 digits should not trigger list error", packedItem.API_TariffInfo, expectedTariffCodeInListMessage);
		}

		void CreateValidDG(AsycudaPackedItem packedItem)
		{
			var factory = Factory;
			var subs3267B = factory.New<UNDGSubstance>();
			subs3267B.DG_UNNO = "3267";
			subs3267B.DG_PG = "I";

			var org = factory.NewWithValidTestData<OrgHeader>();
			var anotherOrg = factory.NewWithValidTestData<OrgHeader>();

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = "Name";

			var firstUNDG = packedItem.UNDGs.AddNew();
			firstUNDG.DI_DG = subs3267B.PK;
			firstUNDG.DI_OC_DGContact = contact.PK;
		}
	}
}
