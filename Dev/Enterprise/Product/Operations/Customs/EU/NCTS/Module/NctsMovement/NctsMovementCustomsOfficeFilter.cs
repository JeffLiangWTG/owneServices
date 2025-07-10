using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Module
{
	public class NctsMovementCustomsOfficeFilter
	{
		public NctsMovementCustomsOfficeFilter(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}
		readonly BusinessObjectFactory factory;

		public void AddFilters(ModuleFilterCollection filters)
		{
			Argument.NotNull(filters, nameof(filters));

			AddDepartureOfficeFilter(filters);
			AddDestinationOfficeFilter(filters);
			AddTransitOfficeFilter(filters);
			AddDestinationOfficeForArrivalFilter(filters);
		}

		#region Implementation

		void AddDepartureOfficeFilter(ModuleFilterCollection filters)
		{
			var officeOfDepartureFilter = AddCustomsOfficeFilter(filters, NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfDeparture, new NCTSDepartureOfficesCollectionProvider(factory), OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			officeOfDepartureFilter.MultilingualDescription = ResString.GetMultilingualString("D321EC94-870B-45F9-A7C0-DDAE323550EB", NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfDeparture);
		}

		void AddDestinationOfficeFilter(ModuleFilterCollection filters)
		{
			var officeOfDestinationFilter = AddCustomsOfficeFilter(filters, NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfDestination, new NCTSDestinationOfficesCollectionProvider(factory), OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			officeOfDestinationFilter.MultilingualDescription = ResString.GetMultilingualString("3F3F9262-19A3-4626-B9AC-01F7C779B89C", NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfDestination);
		}

		void AddDestinationOfficeForArrivalFilter(ModuleFilterCollection filters)
		{
			var officeOfDestinationFilter = AddCustomsOfficeFilter(filters, NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfDestinationForArrival, new NCTSDestinationOfficesForArrivalCollectionProvider(factory), OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
			officeOfDestinationFilter.MultilingualDescription = ResString.GetMultilingualString("5576867E-9473-45D7-AE20-522A4B72B2E8", NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfDestinationForArrival);
		}

		void AddTransitOfficeFilter(ModuleFilterCollection filters)
		{
			var officeOfTransitFilter = AddCustomsOfficeFilter(filters, NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfTransit, new NCTSTransitOfficesCollectionProvider(factory), OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			officeOfTransitFilter.MultilingualDescription = ResString.GetMultilingualString("5F7EBEAB-08E5-4129-A884-344FAC6F8E48", NctsMovementFilterStripBusinessObject.FilterConstants.CustomsOfficeOfTransit);
		}

		ModuleNkFilter AddCustomsOfficeFilter(ModuleFilterCollection filters, ZString description, CollectionProviderWithCodeSupport officeProvider, ZString officeCode)
		{
			var customsOfficeFilter = filters.AddNkFilter(description, GetCustomsOfficeFilterByCode, officeProvider.ModuleID, officeProvider.CollectionForFindbox);
			customsOfficeFilter.Category = CustomsOfficeCategory;
			customsOfficeFilter.MaxLength = officeProvider.MaxLength;

			return customsOfficeFilter;

			ZQuery GetCustomsOfficeFilterByCode(SQLComparisonOperator comparisonOperator, ZString value) => GetCustomsOfficeFilter(comparisonOperator, value, officeCode);
		}

		ZQuery GetCustomsOfficeFilter(SQLComparisonOperator comparisonOperator, ZString value, ZString code)
		{
			var query = new ZDBOnlyQuery(typeof(NctsHeader));
			var customsOfficeSubQuery = new ZDBOnlySubQuery(typeof(NctsEuOfficeCode), CusCodeDataSchema.CY_ParentID);

			customsOfficeSubQuery.AddToFilter(CusCodeDataSchema.CY_Type, EU.Business.CusCodeDataTypeList.Codes.OfficeCode);
			customsOfficeSubQuery.AddToFilter(CusCodeDataSchema.CY_Code, code);
			customsOfficeSubQuery.AddToFilter(CusCodeDataSchema.CY_Data, comparisonOperator, value);

			var phase5DepartureHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			phase5DepartureHeaderQuery.AddSubQuery(customsOfficeSubQuery, JoinCondition.And);

			query.AddSubQuery(customsOfficeSubQuery, JoinCondition.Or);
			query.AddSubQuery(phase5DepartureHeaderQuery, JoinCondition.Or);
			return query;
		}

		FilterCategory CustomsOfficeCategory => customsOfficeCategory ?? (customsOfficeCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("74E19053-7C92-4CF0-8A54-735B3167D4E5", "Customs Offices")));
		FilterCategory customsOfficeCategory;

		#endregion
	}
}
