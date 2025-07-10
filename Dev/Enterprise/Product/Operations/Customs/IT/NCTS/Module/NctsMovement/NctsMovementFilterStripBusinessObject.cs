using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.NCTS.Module;

public class NctsMovementFilterStripBusinessObject : EU.NCTS.Module.NctsMovementFilterStripBusinessObject
{
	public static class ITFilterConstants
	{
		#region SuppressResourceStringsCheckRegion Reason = Filter Constants

		public const string ArrivalDate = "IRILDES Arrival Date";
		public const string ArrivalOfficeCode = "IRILDES Arrival Office Code";
		public const string ArrivalOfficeDescription = "IRILDES Arrival Office Description";
		public const string ArrivalStatus = "IRILDES Arrival Status";
		public const string RepresentationType = "Representation Type";
		public const string Declarant = "Declarant";
		public const string DefermentAccountNumber = "Approval Defer No.";
		public const string RegistrationDate = "Registration Date";
		public const string ReleaseDate = "Release Date";

		#endregion
	}

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = base.GetModuleFiltersCore();

		AddIrildesFilters(filters);
		AddRepresentationFilter(filters);
		AddDeclarantFilter(filters);
		AddDefermentAccountNumberFilter(filters);
		AddRegistrationDateFilter(filters);
		AddReleaseDateFilter(filters);

		return filters;
	}

	void AddIrildesFilters(ModuleFilterCollection filters)
	{
		var notificationsCategory = new FilterCategory(ResString.GetMultilingualString("F14BB13D-A990-47D0-AF41-B5B5E381481E", "Notifications"));

		var arrivalDateFilter = filters.AddDateFilter(ITFilterConstants.ArrivalDate, GetArrivalDateQuery);
		arrivalDateFilter.Category = notificationsCategory;
		arrivalDateFilter.MultilingualDescription = ResString.GetMultilingualString("B42E6672-4ADC-44F4-AEA6-BBD5DAEBF6EB", ITFilterConstants.ArrivalDate);

		var arrivalOfficeCodeFilter = filters.AddTextFilter(ITFilterConstants.ArrivalOfficeCode, GetArrivalOfficeCodeQuery).WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryLineReference);
		arrivalOfficeCodeFilter.Category = notificationsCategory;
		arrivalOfficeCodeFilter.MultilingualDescription = ResString.GetMultilingualString("D46E1E94-96E6-42FB-B7C1-9DF999C17F44", ITFilterConstants.ArrivalOfficeCode);

		var arrivalOfficeDescriptionFilter = filters.AddTextFilter(ITFilterConstants.ArrivalOfficeDescription, GetArrivalOfficeDescriptionQuery).WithMaxLengthOf<ModuleTextFilter>(ZZRefCusCodeListCombinedSchema.ZZD_Description);
		arrivalOfficeDescriptionFilter.Category = notificationsCategory;
		arrivalOfficeDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("ED836581-B2CA-400A-B416-78BB7D611AF6", ITFilterConstants.ArrivalOfficeDescription);

		var arrivalStatusFilter = filters.AddTextFilter(ITFilterConstants.ArrivalStatus, GetArrivalStatusQuery).WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryStatus);
		arrivalStatusFilter.Category = notificationsCategory;
		arrivalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("38371F6B-87D3-45C5-9D31-5FDE402BBFC3", ITFilterConstants.ArrivalStatus);

		ZQuery GetArrivalDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) => GetIssueDateQuery(comparisonOperator, value1, value2, CusEntryNumberConstants.EntryTypes.Irildes);
	}

	#region Arrival Status Query

	ZQuery GetArrivalStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var query = GetNctsHeaderZOnlyQuery();

		var irildesSubQuery = GetIrildesSubQuery();
		irildesSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, comparisonOperator, value);
		query.AddSubQuery(irildesSubQuery, JoinCondition.And);

		AddHasNotIrildesSubQueryIfNeeded(comparisonOperator, query);

		return query;
	}

	#endregion

	#region Arrival Office Code Query

	ZQuery GetArrivalOfficeCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var query = GetNctsHeaderZOnlyQuery();

		var irildesQuery = GetIrildesSubQuery();
		irildesQuery.AddToFilter(CusEntryNumSchema.CE_EntryLineReference, comparisonOperator, value);
		query.AddSubQuery(irildesQuery, JoinCondition.And);

		AddHasNotIrildesSubQueryIfNeeded(comparisonOperator, query);

		return query;
	}

	#endregion

	#region Arrival Office Description Query

	ZQuery GetArrivalOfficeDescriptionQuery(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var query = GetNctsHeaderZOnlyQuery();
		var irildesQuery = GetIrildesSubQuery();

		var zzRefCusCodeListCombinedSubQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListCombined), ZZRefCusCodeListCombinedSchema.ZZD_Code, CusEntryNumSchema.CE_EntryLineReference);
		zzRefCusCodeListCombinedSubQuery.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Description, comparisonOperator, value);
		irildesQuery.AddSubQuery(zzRefCusCodeListCombinedSubQuery, JoinCondition.And);
		query.AddSubQuery(irildesQuery, JoinCondition.And);

		AddHasNotIrildesSubQueryIfNeeded(comparisonOperator, query);

		return query;
	}

	#endregion

	#region Representation Type

	void AddRepresentationFilter(ModuleFilterCollection filters)
	{
		var representationTypeFilter = filters.AddTextFilter(ITFilterConstants.RepresentationType, GetRepresentationTypeQuery, GetRepresentationTypes);
		representationTypeFilter.Category = FilterCategories.ModesAndTypes;
		representationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ABD564FA-6D6B-4B03-A581-48E937864A10", ITFilterConstants.RepresentationType);
	}

	ZQuery GetRepresentationTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var query = GetNctsHeaderZOnlyQuery();

		var genAddOnColumnSubquery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
		genAddOnColumnSubquery.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, NctsHeader.Schema.RepresentationType);
		genAddOnColumnSubquery.AddToFilter(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);

		query.AddSubQuery(genAddOnColumnSubquery, JoinCondition.And);

		return query;
	}

	IList GetRepresentationTypes() => Factory.GetCachedValue<RepresentationTypeList>();

	#endregion

	#region Declarant 

	void AddDeclarantFilter(ModuleFilterCollection filters)
	{
		var declarantFilter = filters.AddGuidFilter(ITFilterConstants.Declarant, ModuleIDs.Organisation, GetDeclarantGuidQuery, new OrganisationsFindBoxCollection(Factory));
		declarantFilter.Category = FilterCategories.Organisations;
		declarantFilter.MaxLength = OrgHeaderSchema.OH_Code.MaxLength;
		declarantFilter.MultilingualDescription = ResString.GetMultilingualString("A9B9C8A4-7F10-4436-87E9-BF28C5D83783", ITFilterConstants.Declarant);
	}

	public ZQuery GetDeclarantGuidQuery(ZGuid value)
	{
		var query = GetNctsHeaderZOnlyQuery();

		var genAddOnColumnSubquery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
		genAddOnColumnSubquery.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, NctsHeader.Schema.DeclarantAddressPK);

		var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
		orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, value);

		genAddOnColumnSubquery.AddSubQuery(GenAddOnColumnSchema.XA_Data, orgAddressSubQuery, JoinCondition.And);

		query.AddSubQuery(genAddOnColumnSubquery, JoinCondition.And);
		return query;
	}

	#endregion

	#region DefermentAccountNumber

	void AddDefermentAccountNumberFilter(ModuleFilterCollection filters)
	{
		var defermentAccountNumberFilter = filters.AddTextFilter(ITFilterConstants.DefermentAccountNumber, GetDefermentAccountNumber);
		defermentAccountNumberFilter.Category = FilterCategories.TextSearch;
		defermentAccountNumberFilter.MaxLength = NctsDepartureMovementHeader.Schema.DefermentAccountNumberMaxLength;
		defermentAccountNumberFilter.MultilingualDescription = ResString.GetMultilingualString("7ADCC4C0-466F-408F-9081-8FD6A8B23635", ITFilterConstants.DefermentAccountNumber);
	}

	ZQuery GetDefermentAccountNumber(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var query = GetNctsHeaderZOnlyQuery();

		var departureMovementSubQuery = new ZDBOnlySubQuery(typeof(NctsDepartureMovementHeader), CusInBondMoveHeaderSchema.BM_BH);

		var genAddOnColumnSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
		genAddOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, SQLComparisonOperator.Equal, NctsDepartureMovementHeader.Schema.DefermentAccountNumber);
		genAddOnColumnSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);

		departureMovementSubQuery.AddSubQuery(genAddOnColumnSubQuery, JoinCondition.And);

		query.AddSubQuery(departureMovementSubQuery, JoinCondition.And);

		return query;
	}

	#endregion

	#region RegistrationDate

	void AddRegistrationDateFilter(ModuleFilterCollection filters)
	{
		var registrationDateFilter = filters.AddDateFilter(ITFilterConstants.RegistrationDate, GetRegistrationDateQuery);
		registrationDateFilter.Category = FilterCategories.Dates;
		registrationDateFilter.MultilingualDescription = ResString.GetMultilingualString("4F697A61-8F7F-4558-9551-7D791D05B72A", ITFilterConstants.RegistrationDate);

		ZQuery GetRegistrationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) => GetIssueDateQuery(comparisonOperator, value1, value2, CusEntryNumberConstants.EntryTypes.RegistrationNumber);
	}

	#endregion

	#region ReleaseDate

	void AddReleaseDateFilter(ModuleFilterCollection filters)
	{
		var registrationDateFilter = filters.AddDateFilter(ITFilterConstants.ReleaseDate, GetReleaseDateQuery);
		registrationDateFilter.Category = FilterCategories.Dates;
		registrationDateFilter.MultilingualDescription = ResString.GetMultilingualString("708B9C6E-EE00-4DA2-9D9E-343B3FB4C72D", ITFilterConstants.ReleaseDate);

		ZQuery GetReleaseDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) => GetIssueDateQuery(comparisonOperator, value1, value2, CusEntryNumberConstants.EntryTypes.ClereanceCode);
	}

	protected override MultilingualString MRNReleaseDateFilterDescription => ResString.GetMultilingualString("BDAF04AB-A61B-4154-9369-EBF5EC11F892", "MRN Release Date");

	#endregion

	#region Implementation

	ZQuery GetIssueDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2, ZString entryType)
	{
		var query = GetNctsHeaderZOnlyQuery();

		var releaseQuery = GetEntryNumSubQuery(entryType);

		AddDateTimeRange(releaseQuery, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, value1, value2);
		query.AddSubQuery(releaseQuery, JoinCondition.And);

		if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
		{
			AddHasNotInSubQuery(query, entryType);
		}

		return query;
	}

	ZDBOnlySubQuery GetIrildesSubQuery() => GetEntryNumSubQuery(CusEntryNumberConstants.EntryTypes.Irildes);

	ZDBOnlySubQuery GetEntryNumSubQuery(ZString entryType, bool notIn = false)
	{
		var cusEntryNumSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);
		cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryType);
		cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.Italy);

		return cusEntryNumSubQuery;
	}

	void AddHasNotIrildesSubQueryIfNeeded(SQLComparisonOperator comparisonOperator, ZDBOnlyQuery query) => AddHasNotEntryNumSubQueryIfNeeded(comparisonOperator, query, CusEntryNumberConstants.EntryTypes.Irildes);

	void AddHasNotEntryNumSubQueryIfNeeded(SQLComparisonOperator comparisonOperator, ZDBOnlyQuery query, ZString entryType)
	{
		if (comparisonOperator == SpecialComparisonOperator.IsBlank)
		{
			var noIrildesSubQuery = GetEntryNumSubQuery(entryType, notIn: true);
			query.AddSubQuery(noIrildesSubQuery, JoinCondition.Or);
		}
	}

	void AddHasNotInSubQuery(ZDBOnlyQuery query, ZString entryType)
	{
		var noRegistrationSubQuery = GetEntryNumSubQuery(entryType, notIn: true);
		query.AddSubQuery(noRegistrationSubQuery, JoinCondition.Or);
	}

	ZDBOnlyQuery GetNctsHeaderZOnlyQuery() => new ZDBOnlyQuery(typeof(NctsHeader));

	#endregion
}
