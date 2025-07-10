using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(Package))]
	sealed class PackageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			AssertType<PackageLookups>(package.Lookups);
		}

		public void TestValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			AssertType<PackageValidation>(package.Validation);
		}

		public void TestCW_PackType_Caption()
		{
			CombineAssertions("Package CW_PackTypeInfo", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var package = declaration.Packages.AddNew();
				var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(package.CW_PackTypeInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("IMP UCC, Caption", "Pack Type", data.Caption);
				AssertEquals("IMP UCC, Full Description", "[18 06 003 000] Type of Packages", data.FullDescription);

				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(package.CW_PackTypeInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption", "Pack Type", data.Caption);
				AssertEquals("Full Description", string.Empty, data.FullDescription);
			});
		}

		public void TestCW_PackQtyInfo_Caption()
		{
			CombineAssertions("Package CW_PackQtyInfo", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var package = declaration.Packages.AddNew();
				var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(package.CW_PackQtyInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("IMP UCC, Caption", "Pack Qty", data.Caption);
				AssertEquals("IMP UCC, Full Description", "[18 06 004 000] Number of Packages", data.FullDescription);

				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(package.CW_PackQtyInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption", "Pack Qty", data.Caption);
				AssertEquals("Full Description", string.Empty, data.FullDescription);
			});
		}

		public void TestCW_MarksAndNosInfo_Caption()
		{
			CombineAssertions("Package CW_MarksAndNosInfo", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var package = declaration.Packages.AddNew();
				var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(package.CW_MarksAndNosInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("IMP UCC, Caption", "Marks", data.Caption);
				AssertEquals("IMP UCC, Full Description", "[18 06 054 000] Shipping Marks", data.FullDescription);

				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(package.CW_MarksAndNosInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption", "Marks", data.Caption);
				AssertEquals("Full Description", string.Empty, data.FullDescription);
			});
		}

		public void TestShouldDeleteIfPackQtyIsEmptyUnlessTypeIsBulkOrUnpacked()
		{
			SetUpPackageTypes();

			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "123";

			Package package = (Package)declaration.Packages.AddNew();
			package.CW_HouseBill = bill.CU_BillUniqueCode;

			AssertPackNotDeleted(package, UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkLiquidGas);
			AssertPackNotDeleted(package, UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas);
			AssertPackNotDeleted(package, UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkLiquid);
			AssertPackNotDeleted(package, UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkPowders);
			AssertPackNotDeleted(package, UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGrains);
			AssertPackNotDeleted(package, UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkNodules);
			AssertPackNotDeleted(package, UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked);

			package.CW_MarksAndNos = "";
			package.CW_PackQty = 0;
			package.CW_PackType = "BX";
			Factory.Save();
			AssertEquals("Package, deleted", true, package.IsDeleted);
		}

		public void TestIsBulk()
		{
			SetUpPackageTypes();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var package = (Package)declaration.Packages.AddNew();

			CombineAssertions(() =>
			{
				package.CW_PackType = "";
				AssertEquals("When Pack Type is '', IsBulk", false, package.IsBulk);

				package.CW_PackType = "VG";
				AssertEquals("When Pack Type is 'VG' (BULK attribute), IsBulk", true, package.IsBulk);

				package.CW_PackType = "NE";
				AssertEquals("When Pack Type is 'NE' (BREAKBULK attribute), IsBulk", false, package.IsBulk);
			});
		}

		public void TestIsBreakBulk()
		{
			SetUpPackageTypes();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var package = (Package)declaration.Packages.AddNew();

			CombineAssertions(() =>
			{
				package.CW_PackType = "";
				AssertEquals("When Pack Type is '', IsBreakBulk", false, package.IsBreakBulk);

				package.CW_PackType = "VG";
				AssertEquals("When Pack Type is 'VG' (BULK attribute), IsBreakBulk", false, package.IsBreakBulk);

				package.CW_PackType = "NE";
				AssertEquals("When Pack Type is 'NE' (BREAKBULK attribute), IsBreakBulk", true, package.IsBreakBulk);
			});
		}

		public void TestIsEmptyPackTypeAllowed()
		{
			SetUpPackageTypes();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var package = (Package)declaration.Packages.AddNew();

			AssertIsEmptyPackTypeAllowed(package, UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkLiquidGas, true);
			AssertIsEmptyPackTypeAllowed(package, UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas, true);
			AssertIsEmptyPackTypeAllowed(package, UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkLiquid, true);
			AssertIsEmptyPackTypeAllowed(package, UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkPowders, true);
			AssertIsEmptyPackTypeAllowed(package, UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGrains, true);
			AssertIsEmptyPackTypeAllowed(package, UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkNodules, true);
			AssertIsEmptyPackTypeAllowed(package, UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked, true);
			AssertIsEmptyPackTypeAllowed(package, "A1", false);
		}

		void AssertPackNotDeleted(Package package, string type)
		{
			package.CW_PackType = type;

			package.CW_MarksAndNos = "marks";
			package.CW_PackQty = 1;
			Factory.Save();
			AssertEquals("Package not deleted", false, package.IsDeleted);

			package.CW_PackQty = 0;
			Factory.Save();
			AssertEquals("Package not deleted", false, package.IsDeleted);

			package.CW_MarksAndNos = "";
			package.CW_PackQty = 1;
			Factory.Save();
			AssertEquals("Package not deleted", false, package.IsDeleted);

			package.CW_PackQty = 0;
			Factory.Save();
			AssertEquals("Package not deleted", false, package.IsDeleted);
		}

		void AssertIsEmptyPackTypeAllowed(Package package, string type, bool expectedValue)
		{
			package.CW_PackType = type;
			AssertEquals(ZString.Format("for type{0}", type), expectedValue, package.IsEmptyPackTypeAllowed);
		}

		public void TestGetNewInvoiceLinePivotCollection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var pack = declaration.Packages.AddNew();

			Assert("Precondition Pack is EU Package class", pack is Package);
			Assert("Precondition InvoiceLine is EU InvoiceLine class", invoiceLine1 is JobComInvoiceLine);
			Assert("Precondition SupportsChcPivot", invoiceLine1.SupportsChcPivotBetweenInvoiceLineAndPacking);

			var npbos = invoiceLine1.PackagesForInvoiceLinesForBindingOnly;
			var npbo = npbos.Cast<BaseCusLinkPackage>().FirstOrDefault();
			AssertEquals("Precondition IsLinked", false, npbo?.IsLinked);
			npbo.IsLinked = true;

			pack.InvoiceLinePivotCollection.RunPreSaveValidation();
			var pivot = pack.InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().FirstOrDefault();
			AssertHasMessageErrorContaining(pivot.CHC_NumberOfPacksInfo, "have not entered");

			pack.CW_PackType = UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas;

			pack.InvoiceLinePivotCollection.RunPreSaveValidation();
			pivot = pack.InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().FirstOrDefault();
			AssertNoMessageErrorContaining(pivot.CHC_NumberOfPacksInfo, "have not entered");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "MB1";
			declaration.JE_TotalNoOfPacks = 123;
			declaration.JE_TotalNoOfPacksPackType = "AE";
			var pack = declaration.Packages[0];
			return pack;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		void SetUpPackageTypes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VL", "VL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VY", "VY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VR", "VR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VO", "VO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
	}
}
