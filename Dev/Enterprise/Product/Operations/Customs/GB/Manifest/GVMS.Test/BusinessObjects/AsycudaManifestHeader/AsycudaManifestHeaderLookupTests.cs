using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class AsycudaManifestHeaderLookupTests : BusinessObjectLookupsTestCase
	{
		public void TestGVMSEmptyVehicleList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("List Count", 3, header.Lookups.EmptyVehicleList.Count);
			AssertType<GVMSEmptyVehicle>(header.Lookups.EmptyVehicleList);
			AssertSame("Cached List", Factory.GetCachedValue<GVMSEmptyVehicle>(), header.Lookups.EmptyVehicleList);
			AssertPairEquals(new CodeDescriptionPair("", "Vehicle is not empty"), header.Lookups.EmptyVehicleList[0]);
			AssertPairEquals(new CodeDescriptionPair("CON", "Empty vehicle is being moved under a contract of carriage"), header.Lookups.EmptyVehicleList[1]);
			AssertPairEquals(new CodeDescriptionPair("OWN", "Empty vehicle is not being moved via a contract of carriage"), header.Lookups.EmptyVehicleList[2]);
		}

		void AssertPairEquals(ICodeDescription expectedPair, ICodeDescription actualPair) => AssertEquals(expectedPair.Code + expectedPair.Description, actualPair.Code + actualPair.Description);

		public void TestManifestNatureList()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			AssertSame(Factory.GetCachedValue<GVMSManifestNature>(), header.Lookups.Natures);
		}
		public void TestCustomsStatusList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var actual = header.Lookups.RegistrationStatusList;
			var expected = new GVMSCustomsStatus();

			Assert("Checking for: " + expected.CodesAsString, actual.ContainsOnly(expected.GetAllCodes()));
		}

		public void TestCarrierCodeList()
		{
			var carrier1 = Factory.New<ZZRefCarrierCombined>();
			carrier1.ZZ4_Code = "ABC";
			carrier1.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.UnitedKingdom;
			carrier1.ZZ4_Description = "Carrier 1";
			var carrier2 = Factory.New<ZZRefCarrierCombined>();
			carrier2.ZZ4_Code = "ZAQ";
			carrier2.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.SouthAfrica;
			carrier2.ZZ4_Description = "Dra-Jou-Dinge";
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			var list = header.Lookups.CarrierCodeList;

			Assert(list.ContainsOnly("abc"));
		}

		public void TestLoadPortList()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_Nature = GVMSManifestNature.Codes.Import;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-GVMS", () => new UKUnlocoGVMSLocations(Factory)), header.Lookups.LoadingPortList);

			header.AMA_Nature = GVMSManifestNature.Codes.Export;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-UK", () => new UKUnlocoUKLocations(Factory)), header.Lookups.LoadingPortList);

			header.AMA_Nature = GVMSManifestNature.Codes.GBtoNI;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-GB", () => new UKUnlocoGBLocations(Factory)), header.Lookups.LoadingPortList);

			header.AMA_Nature = GVMSManifestNature.Codes.NItoGB;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-NI", () => new UKUnlocoNILocations(Factory)), header.Lookups.LoadingPortList);
		}

		public void TestDischargePortList()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_Nature = GVMSManifestNature.Codes.Import;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-UK", () => new UKUnlocoUKLocations(Factory)), header.Lookups.DischargePortList);

			header.AMA_Nature = GVMSManifestNature.Codes.Export;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-GVMS", () => new UKUnlocoGVMSLocations(Factory)), header.Lookups.DischargePortList);

			header.AMA_Nature = GVMSManifestNature.Codes.GBtoNI;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-NI", () => new UKUnlocoNILocations(Factory)), header.Lookups.DischargePortList);

			header.AMA_Nature = GVMSManifestNature.Codes.NItoGB;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-GB", () => new UKUnlocoGBLocations(Factory)), header.Lookups.DischargePortList);
		}

		public void TestCustomsLoadPortList()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_Nature = GVMSManifestNature.Codes.Import;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-GVMS", () => new UKUnlocoGVMSLocations(Factory)), header.Lookups.CustomsLoadingPortList);

			header.AMA_Nature = GVMSManifestNature.Codes.Export;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-UK", () => new UKUnlocoUKLocations(Factory)), header.Lookups.CustomsLoadingPortList);

			header.AMA_Nature = GVMSManifestNature.Codes.GBtoNI;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-GB", () => new UKUnlocoGBLocations(Factory)), header.Lookups.CustomsLoadingPortList);

			header.AMA_Nature = GVMSManifestNature.Codes.NItoGB;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-NI", () => new UKUnlocoNILocations(Factory)), header.Lookups.CustomsLoadingPortList);
		}

		public void TestCustomsDischargePortList()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_Nature = GVMSManifestNature.Codes.Import;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-UK", () => new UKUnlocoUKLocations(Factory)), header.Lookups.CustomsDischargePortList);

			header.AMA_Nature = GVMSManifestNature.Codes.Export;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-GVMS", () => new UKUnlocoGVMSLocations(Factory)), header.Lookups.CustomsDischargePortList);

			header.AMA_Nature = GVMSManifestNature.Codes.GBtoNI;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-NI", () => new UKUnlocoNILocations(Factory)), header.Lookups.CustomsDischargePortList);

			header.AMA_Nature = GVMSManifestNature.Codes.NItoGB;
			AssertSame(Factory.GetCachedValue<RefUNLOCOCollection>("GVMS-Ports-GB", () => new UKUnlocoGBLocations(Factory)), header.Lookups.CustomsDischargePortList);
		}

		public void TestRoutesList()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var list = header.Lookups.GVMSRoutesList;
			AssertEquals("R1, R2", list.CodesAsString);
		}

		public void TestProfileList()
		{
			var pwd = Factory.New<GlbExternalPassword_GB>();
			pwd.GP_GC = GlbCompany.CurrentCompany.PK;
			pwd.Badge = "WTG";
			pwd.EORI = GlbBranch.CurrentBranch.OrgProxy.GetEuIdentificationNumber();
			pwd.Status = PasswordStatusList.Codes.Valid;
			pwd.GP_ExpiryDate = ZDateTime.Today.AddDays(2);
			pwd.GP_IssueDate = ZDateTime.Today.AddDays(-2);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("WTG", header.Lookups.ProfileList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();

			GVMSTestHelper.SetupPorts(Factory);
			GVMSTestHelper.SetupRoutes(Factory);
		}
	}
}
