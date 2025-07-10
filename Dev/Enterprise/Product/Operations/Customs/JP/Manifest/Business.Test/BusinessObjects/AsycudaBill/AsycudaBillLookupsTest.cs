using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillLookups))]
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackageTypeList()
		{
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "BA");
			var collection = AsycudaBillLookups.PackageTypeList;
			AssertEquals(1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "BA" }, collection.GetAllCodes());
		}

		public void TestMessageStatusList()
		{
			AssertType<JPMessageStatusList>(AsycudaBillLookups.MessageStatusList);
		}

		public void TestCargoTypeList()
		{
			AssertType<ManifestCargoTypeList>(AsycudaBillLookups.CargoTypeList);
		}

		public void TestJPCustomsWeightUnitList()
		{
			AssertType<CustomsWeightUnitList>(AsycudaBillLookups.JPCustomsWeightUnitList);
		}

		public void TestJPCustomsVolumeUnitList()
		{
			AssertType<CustomsVolumeUnitList>(AsycudaBillLookups.JPCustomsVolumeUnitList);
		}

		public void TestCustomsStatusList()
		{
			AssertType<JPCustomsStatusList>(AsycudaBillLookups.CustomsStatusList);
		}

		public void TestReasonList()
		{
			AssertEquals("Should be same", AsycudaBill.TemporaryLandingInfo.Lookups.CodeList, AsycudaBillLookups.ReasonList);
		}

		public void TestBondedTransportList()
		{
			AssertEquals("Sorts the result by code", "11, 16, 17, 25, 31, 6", AsycudaBillLookups.BondedTransportList.CodesAsString);
		}

		public void TestSpecialCargoCodes()
		{
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "AOG");
			var collection = (ZZRefCusCodeListCombinedCollection)AsycudaBillLookups.SpecialCargoCodes;
			collection.Load();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 1, collection.Count);
				AssertContainsExactElementsInExactOrder(JPRefCusCodeListTypes.GetSpecialCargoCodes(Factory), collection);
			});
		}

		public void TestTariffCollection()
		{
			Factory.CreateTariffData(Universal.Constants.TariffTypes.Import, "01012100001", "01012100002", "01012100003", "01012100004", "01012100005");
			Factory.CreateTariffData(Universal.Constants.TariffTypes.Export, "02012100001", "04012100002");
			AsycudaManifestHeader.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			var collection = AsycudaBillLookups.TariffCollection;
			collection.Load();
			AssertEquals(5, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "01012100001", "01012100002", "01012100003", "01012100004", "01012100005" }, collection.Select(c => c.ZZ1_TariffCode));

			AsycudaManifestHeader.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			collection = AsycudaBillLookups.TariffCollection;
			collection.Load();
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "02012100001", "04012100002" }, collection.Select(c => c.ZZ1_TariffCode));
		}

		AsycudaManifestHeader AsycudaManifestHeader => asycudaManifestHeader ??= Factory.NewWithValidTestData<AsycudaManifestHeader>();
		AsycudaManifestHeader asycudaManifestHeader;

		AsycudaBill AsycudaBill => asycudaBill ??= AsycudaManifestHeader.Bills.AddNew();
		AsycudaBill asycudaBill;

		AsycudaBillLookups AsycudaBillLookups => asycudaBillLookups ??= AsycudaBill.Lookups;
		AsycudaBillLookups asycudaBillLookups;
	}
}
