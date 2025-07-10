using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.MasterFiles
{
	static class UKLocationsLoader
	{
		internal static ZDBOnlyQuery CreateBaseFilter(bool isUKOnly)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(RefUNLOCO));
			result.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, isUKOnly ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, Core.Constants.CountryCodes.UnitedKingdom);
			return result;
		}
	}

	public class UKUnlocoUKLocations : RefUNLOCOCollection
	{
		public UKUnlocoUKLocations(BusinessObjectFactory factory) : base(factory, CreateFilter()) { }

		protected override bool AllowNew => false;

		static ZDBOnlyQuery CreateFilter()
		{
			return UKLocationsLoader.CreateBaseFilter(true);
		}
	}

	public class UKUnlocoGBLocations : RefUNLOCOCollection
	{
		public UKUnlocoGBLocations(BusinessObjectFactory factory) : base(factory, CreateFilter()) { }

		protected override bool AllowNew => false;

		static ZDBOnlyQuery CreateFilter()
		{
			ZDBOnlyQuery result = UKLocationsLoader.CreateBaseFilter(true);
			var subQuery = new ZDBOnlySubQuery(typeof(RefCountryStates), RefCountryStatesSchema.PK, notIn: true);
			subQuery.AddToFilter(RefCountryStatesSchema.RW_RegionName, RefUNLOCO.Regions.NorthernIreland);
			result.AddSubQuery(RefUNLOCOSchema.RL_RW, subQuery, JoinCondition.And);
			return result;
		}
	}

	public class UKUnlocoNILocations : RefUNLOCOCollection
	{
		public UKUnlocoNILocations(BusinessObjectFactory factory) : base(factory, CreateFilter()) { }

		protected override bool AllowNew => false;

		static ZDBOnlyQuery CreateFilter()
		{
			ZDBOnlyQuery result = UKLocationsLoader.CreateBaseFilter(true);
			var subQuery = new ZDBOnlySubQuery(typeof(RefCountryStates), RefCountryStatesSchema.PK);
			subQuery.AddToFilter(RefCountryStatesSchema.RW_RegionName, RefUNLOCO.Regions.NorthernIreland);
			result.AddSubQuery(RefUNLOCOSchema.RL_RW, subQuery, JoinCondition.And);
			return result;
		}
	}

	public class UKUnlocoGVMSLocations : RefUNLOCOCollection
	{
		public UKUnlocoGVMSLocations(BusinessObjectFactory factory) : base(factory, CreateFilter()) { }

		protected override bool AllowNew => false;

		static ZDBOnlyQuery CreateFilter()
		{
			ZDBOnlyQuery result = UKLocationsLoader.CreateBaseFilter(false);
			var subQuery = new ZDBOnlySubQuery(typeof(RefLocoMap), RefLocoMapSchema.RY_RL_NKLocoPort);
			subQuery.AddToFilter(RefLocoMapSchema.RY_SystemUsage, "GVM");

			var subCtrQuery = new ZDBOnlySubQuery(typeof(RefCountry), RefCountrySchema.PK);
			subCtrQuery.AddToFilter(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom);
			subQuery.AddSubQuery(RefLocoMapSchema.RY_RN, subCtrQuery, JoinCondition.And);

			result.AddSubQuery(RefUNLOCOSchema.RL_Code, subQuery, JoinCondition.And);
			return result;
		}
	}
}
