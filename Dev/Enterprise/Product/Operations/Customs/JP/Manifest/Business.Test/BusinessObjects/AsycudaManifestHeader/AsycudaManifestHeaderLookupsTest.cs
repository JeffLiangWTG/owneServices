using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderLookups))]
	sealed class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGetDischargePortListCore()
		{
			AsycudaManifestHeader.AMA_TransportMode = TransportTypeList.Codes.Air;
			AsycudaManifestHeader.AMA_Nature = JPJobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				var list = ManifestHeaderLookups.DischargePortList;
				list.AssertFilterBusinessObjectDefault("Code:Property", ModuleTextFilter.ComparisonConstants.StartsWith, "JP", true);
				list.AssertFilterBusinessObjectDefault("IATA Code:Property", ModuleTextFilter.ComparisonConstants.StartsWith, "", true);
				list.AssertFilterBusinessObjectDefault("IATA Code:Property:1", ModuleTextFilter.ComparisonConstants.IsNotBlank, null, true);
			});

			AsycudaManifestHeader.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				var list = ManifestHeaderLookups.DischargePortList;
				list.AssertFilterBusinessObjectDefault("Code:Property", ModuleTextFilter.ComparisonConstants.NotStartsWith, "JP", true);
				list.AssertFilterBusinessObjectDefault("IATA Code:Property", ModuleTextFilter.ComparisonConstants.StartsWith, "", true);
				list.AssertFilterBusinessObjectDefault("IATA Code:Property:1", ModuleTextFilter.ComparisonConstants.IsNotBlank, null, true);
			});

			AsycudaManifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			AsycudaManifestHeader.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				var list = ManifestHeaderLookups.DischargePortList;
				list.AssertFilterBusinessObjectDefault("Code:Property", ModuleTextFilter.ComparisonConstants.NotStartsWith, "JP", true);
				list.AssertFilterBusinessObjectDefaultNotContain("IATA Code:Property");
				list.AssertFilterBusinessObjectDefaultNotContain("IATA Code:Property:1");
			});
		}

		public void TestGetLoadingPortListCore()
		{
			AsycudaManifestHeader.AMA_TransportMode = TransportTypeList.Codes.Air;
			AsycudaManifestHeader.AMA_Nature = JPJobMessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				var list = ManifestHeaderLookups.LoadingPortList;
				list.AssertFilterBusinessObjectDefault("Code:Property", ModuleTextFilter.ComparisonConstants.StartsWith, "JP", true);
				list.AssertFilterBusinessObjectDefault("IATA Code:Property", ModuleTextFilter.ComparisonConstants.StartsWith, "", true);
				list.AssertFilterBusinessObjectDefault("IATA Code:Property:1", ModuleTextFilter.ComparisonConstants.IsNotBlank, null, true);
			});

			AsycudaManifestHeader.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				var list = ManifestHeaderLookups.LoadingPortList;
				list.AssertFilterBusinessObjectDefault("Code:Property", ModuleTextFilter.ComparisonConstants.NotStartsWith, "JP", true);
				list.AssertFilterBusinessObjectDefault("IATA Code:Property", ModuleTextFilter.ComparisonConstants.StartsWith, "", true);
				list.AssertFilterBusinessObjectDefault("IATA Code:Property:1", ModuleTextFilter.ComparisonConstants.IsNotBlank, null, true);
			});

			AsycudaManifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			CombineAssertions(() =>
			{
				var list = ManifestHeaderLookups.LoadingPortList;
				list.AssertFilterBusinessObjectDefault("Code:Property", ModuleTextFilter.ComparisonConstants.NotStartsWith, "JP", true);
				list.AssertFilterBusinessObjectDefaultNotContain("IATA Code:Property");
				list.AssertFilterBusinessObjectDefaultNotContain("IATA Code:Property:1");
			});
		}

		public void TestPortOfFinalDepartureCollection()
		{
			AsycudaManifestHeader.AMA_TransportMode = "SEA";
			var list = ManifestHeaderLookups.PortOfFinalDepartureCollection;

			AssertType<ZZRefCusCodeListCombinedCollection>("Type", list);

			var collection = list as ZZRefCusCodeListCombinedCollection;
			CombineAssertions(() =>
			{
				collection.AssertFilterBusinessObjectDefault("Transport Mode:Property", ModuleTextFilter.ComparisonConstants.Exact, Core.Constants.TransportModes.Sea, true);
				collection.AssertFilterBusinessObjectDefault("List Type:Property", string.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, false);
				collection.AssertFilterBusinessObjectDefault("Country/Region or Grouping:Property", string.Empty, Core.Constants.CountryCodes.Japan, false);
			});
		}

		public void TestUNLOCOCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			var collection = header.Lookups.LoadingPortList;
			var filterBusinessObjectDefaults = collection.FilterBusinessObjectDefaults;
			AssertEquals("IATA Code:Property", ZString.Empty, filterBusinessObjectDefaults["IATA Code:Property"].Value);
			Assert("IATA Code:Property", filterBusinessObjectDefaults["IATA Code:Property"].IsRemovable);

			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			collection = header.Lookups.LoadingPortList;
			AssertEquals("FilterBusinessObjectDefaults Count", 1, collection.FilterBusinessObjectDefaults.Count);
		}

		public void TestNatures()
		{
			var natures = ManifestHeaderLookups.Natures;
			AssertType<JPJobMessageTypeList>(natures);
			AssertEquals("Export", natures.GetDescriptionFromCode(JPJobMessageTypeList.Codes.Export));
			AssertEquals("Import", natures.GetDescriptionFromCode(JPJobMessageTypeList.Codes.Import));
		}

		public void TestMessageStatusList()
		{
			AssertType<JPMessageStatusList>(ManifestHeaderLookups.MessageStatusList);
		}

		public void TestRegistrationStatusList()
		{
			AssertType<JPCustomsStatusList>(ManifestHeaderLookups.RegistrationStatusList);
		}

		public void TestMasterBillStatusList()
		{
			AssertType<JPMasterBillStatusList>(ManifestHeaderLookups.MasterBillStatusList);
		}

		AsycudaManifestHeaderLookups ManifestHeaderLookups => manifestHeaderLookups ??= AsycudaManifestHeader.Lookups;
		AsycudaManifestHeaderLookups manifestHeaderLookups;

		AsycudaManifestHeader AsycudaManifestHeader => asycudaManifestHeader ??= Factory.NewWithValidTestData<AsycudaManifestHeader>();
		AsycudaManifestHeader asycudaManifestHeader;
	}
}
