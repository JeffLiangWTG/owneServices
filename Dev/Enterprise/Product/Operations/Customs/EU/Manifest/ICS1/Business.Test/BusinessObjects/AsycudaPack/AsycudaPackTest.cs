using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var container = manifestHeader.Containers.AddNew();
			pack.ContainerPK = container.PK;
			return pack;
		}

		public void TestIsBulk()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

			Factory.Save();

			var pack = Factory.New<AsycudaPack>();
			CombineAssertions(() =>
			{
				pack.APA_PackUQ = "NE";
				AssertEquals(false, pack.IsBulk);

				pack.APA_PackUQ = "VG";
				AssertEquals(true, pack.IsBulk);
			});
		}

		public void TestIsUnpacked()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

			Factory.Save();

			var pack = Factory.New<AsycudaPack>();
			CombineAssertions(() =>
			{
				pack.APA_PackUQ = "VG";
				AssertEquals(false, pack.IsUnpacked);

				pack.APA_PackUQ = "NE";
				AssertEquals(true, pack.IsUnpacked);
			});
		}
	}
}
