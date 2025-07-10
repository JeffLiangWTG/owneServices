using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	internal static class GVMSTestHelper
	{
		public static void SetupPorts(BusinessObjectFactory factory)
		{
			var chester = new RefUNLOCO.Loader(factory).Load("GBCEG");
			if (chester.CountryStates == null)
			{
				var ni = factory.NewWithValidTestData<RefCountryStates>();
				ni.RW_RegionName = "ENGLAND";
				chester.RL_RW = ni.PK;
			}

			var belfast = new RefUNLOCO.Loader(factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = factory.NewWithValidTestData<RefCountryStates>();
				ni.RW_RegionName = "NORTHERN IRELAND";
				belfast.RL_RW = ni.PK;
			}

			// make dublin GVMS
			var dublin = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "IEDUB"));
			var dublinGvms = dublin.RefLocoMaps.OfType<RefLocoMap>().FirstOrDefault(m => m.RY_SystemUsage == "GVM");
			if (dublinGvms == null)
			{
				var map1 = dublin.RefLocoMaps.AddNew();
				map1.RY_RN = chester.Country.PK;
				map1.RY_SystemUsage = "GVM";
			}
			factory.Save();
		}

		public static void SetupRoutes(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var gvmsRoutes = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbGVMSRoutes;

			helper.CreateNewOrGetExistingCusCodeType(gvmsRoutes, "GVMS Routes", Core.Constants.CountryCodes.UnitedKingdom);

			var dtMIN = ZDateTime.MinSmallDateTimeValue;
			var dtMAX = ZDateTime.MaxSmallDateTimeValue;

			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, gvmsRoutes, "R1", "Route 1", dtMIN, dtMAX);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, gvmsRoutes, "R2", "Route 2", dtMIN, dtMAX);

			factory.Save();
		}

		public static void SetupPortsForRouteCalculation(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Ports", Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("GvmsPortId", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "GB");
			factory.Save();

			var heathrow = new RefUNLOCO.Loader(factory).Load("GBLHR");

			var zebrugee = new RefUNLOCO.Loader(factory).Load("BEZEE");
			var zebrugeeGvms = zebrugee.RefLocoMaps.OfType<RefLocoMap>().FirstOrDefault(m => m.RY_SystemUsage == "GVM");
			if (zebrugeeGvms == null)
			{
				var map1 = zebrugee.RefLocoMaps.AddNew();
				map1.RY_RN = heathrow.Country.PK;
				map1.RY_SystemUsage = "GVM";
				map1.RY_LocalPortCode = "1111";
			}
			factory.Save();

			var southampton = new RefUNLOCO.Loader(factory).Load("GBSTN");
			var stnMap = southampton.RefLocoMaps.AddNew();
			stnMap.RY_RN = heathrow.Country.PK;
			stnMap.RY_SystemUsage = "SEA";
			stnMap.RY_LocalPortCode = "STN";
			var stnRefCusCode = helper.CreateCusCodeList("GB", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "STN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			factory.Save();

			var hull = new RefUNLOCO.Loader(factory).Load("GBHUL");
			var hulRefCusCode = helper.CreateCusCodeList("GB", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "HUL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var hullPortAttribute = hulRefCusCode.Attributes.AddNew("GvmsPortId", "2222");

			factory.Save();

			var portsmouth = new RefUNLOCO.Loader(factory).Load("GBPME");
			var pmeMap = southampton.RefLocoMaps.AddNew();
			pmeMap.RY_RN = heathrow.Country.PK;
			pmeMap.RY_SystemUsage = "SEA";
			pmeMap.RY_LocalPortCode = "PTM";
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Ports", Core.Constants.CountryCodes.UnitedKingdom);
			var ptmRefCusCode = helper.CreateCusCodeList("GB", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PTM", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var ptmPortAttribute = ptmRefCusCode.Attributes.AddNew("GvmsPortId", "3333");

			factory.Save();
		}

		public static void SetupRoutesForRouteCalculation(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var gvmsRoutes = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbGVMSRoutes;

			helper.CreateNewOrGetExistingCusCodeType(gvmsRoutes, "GVMS Routes", Core.Constants.CountryCodes.UnitedKingdom);

			var dtMIN = ZDateTime.MinSmallDateTimeValue;
			var dtMAX = ZDateTime.MaxSmallDateTimeValue;

			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, gvmsRoutes, "R1", "Route #R1 from 1111 to 2222 via ABC", dtMIN, dtMAX);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, gvmsRoutes, "R2", "Route #R2 from 1111 to 3333 via DEF", dtMIN, dtMAX);

			factory.Save();
		}

		public static void SetUpCarriersForRouteCalculation(BusinessObjectFactory factory)
		{
			var carrier1 = factory.New<ZZRefCarrierCombined>();
			carrier1.ZZ4_Code = "ABC";
			carrier1.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.UnitedKingdom;
			carrier1.ZZ4_Description = "Carrier 1";
			var carrier2 = factory.New<ZZRefCarrierCombined>();
			carrier2.ZZ4_Code = "DEF";
			carrier2.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.UnitedKingdom;
			carrier2.ZZ4_Description = "Carrier 2";
			factory.Save();
		}
	}
}
