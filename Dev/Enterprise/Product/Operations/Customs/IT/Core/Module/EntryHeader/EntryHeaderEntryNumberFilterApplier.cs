using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IT.Module.EntryHeaderFilterBusinessObject;

namespace Enterprise.Customs.IT.Module;

public class EntryHeaderEntryNumberFilterApplier
{
	public EntryHeaderEntryNumberFilterApplier(EntryHeaderFilterBusinessObject parentEntryHeaderFilterBusinessObject)
	{
		parent = Argument.NotNull(parentEntryHeaderFilterBusinessObject, nameof(parentEntryHeaderFilterBusinessObject));
	}

	readonly EntryHeaderFilterBusinessObject parent;

	public void AddFilters(ModuleFilterCollection filters)
	{
		Argument.NotNull(filters, nameof(filters));

		AddReleaseCodeFilter(filters);
		AddRegistrationNumberFilter(filters);
		AddExitDateFilter(filters);
		AddExitProcessingDateFilter(filters);
		AddExitOfficeFilter(filters);
		AddExitStatusFilter(filters);
		AddArrivalDateFilter(filters);
		AddArrivalOfficeFilter(filters);
		AddArrivalStatusFilter(filters);
	}

	#region Release Code

	void AddReleaseCodeFilter(ModuleFilterCollection filters)
	{
		var releaseCodeFilter = filters.AddTextFilter(ITFilterConstants.ReleaseCode, GetReleaseCodeFilterQuery).WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
		releaseCodeFilter.Category = FilterCategories.NumbersAndReferences;
		releaseCodeFilter.MultilingualDescription = ResString.GetMultilingualString("5EBC3FA4-2CDC-49CE-876D-43FDF5401CE6", ITFilterConstants.ReleaseCode);

		ZQuery GetReleaseCodeFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetFilterQuery(CusEntryNumberConstants.EntryTypes.ClereanceCode, CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
		}
	}

	#endregion

	#region Registration Number

	void AddRegistrationNumberFilter(ModuleFilterCollection filters)
	{
		var registrationNumberFilter = filters.AddTextFilter(ITFilterConstants.RegistrationNumber, GetRegistrationNumberFilterQuery).WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
		registrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
		registrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("B3372DE6-02AE-486B-A1C8-615FBCFC2DA1", ITFilterConstants.RegistrationNumber);

		ZQuery GetRegistrationNumberFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetFilterQuery(CusEntryNumberConstants.EntryTypes.RegistrationNumber, CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
		}
	}

	#endregion

	#region Exit Date

	void AddExitDateFilter(ModuleFilterCollection filters)
	{
		var exitDateFilter = filters.AddDateFilter(ITFilterConstants.ExitDateFilter, GetExitDateQuery);
		exitDateFilter.Category = FilterCategories.Dates;
		exitDateFilter.MultilingualDescription = ResString.GetMultilingualString("88CDB349-EC95-44DF-9B17-35EA10DBE8E7", ITFilterConstants.ExitDateFilter);

		ZQuery GetExitDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) => GetIssueDateFilterQuery(CusEntryNumberConstants.EntryTypes.Ivisto, comparisonOperator, value1, value2);
	}

	#endregion

	#region Exit Processing Date

	void AddExitProcessingDateFilter(ModuleFilterCollection filters)
	{
		var exitProcessingDateFilter = filters.AddDateFilter(ITFilterConstants.ExitProcessingDateFilter, GetExitProcessingDateQuery, convertFromLocalToUTC: true);
		exitProcessingDateFilter.Category = FilterCategories.Dates;
		exitProcessingDateFilter.MultilingualDescription = ResString.GetMultilingualString("D5C8F32A-A4B3-47F6-9EE6-212A158D0EED", ITFilterConstants.ExitProcessingDateFilter);

		ZQuery GetExitProcessingDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) => GetCreationDateFilterQuery(CusEntryNumberConstants.EntryTypes.Ivisto, comparisonOperator, value1, value2);
	}

	#endregion

	#region Exit Office Filter

	void AddExitOfficeFilter(ModuleFilterCollection filters)
	{
		var exitOfficeFilter = filters.AddNkFilter(ITFilterConstants.ExitOfficeFilter, GetExitOfficeFilterQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, parent.Lookups.CustomsOfficeList);
		exitOfficeFilter.MaxLength = CustomsOfficeMaxLength;
		exitOfficeFilter.Category = parent.CustomsOfficeCategory;
		exitOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("0CEE2800-B6A0-4705-BDFD-FDDBE06286E6", ITFilterConstants.ExitOfficeFilter);

		ZQuery GetExitOfficeFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetFilterQuery(CusEntryNumberConstants.EntryTypes.Ivisto, CusEntryNumSchema.CE_EntryLineReference, comparisonOperator, value);
		}
	}

	#endregion

	#region Exit Status Filter

	void AddExitStatusFilter(ModuleFilterCollection filters)
	{
		var exitStatusFilter = filters.AddTextFilter(ITFilterConstants.ExitStatusFilter, GetExitStatusFilter, parent.Lookups.ExitStatusList);
		exitStatusFilter.Category = FilterCategories.NumbersAndReferences;
		exitStatusFilter.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryStatus);
		exitStatusFilter.MultilingualDescription = ResString.GetMultilingualString("B8ACBF5E-8998-42D9-8A52-81848519C72B", ITFilterConstants.ExitStatusFilter);

		ZQuery GetExitStatusFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetFilterQuery(CusEntryNumberConstants.EntryTypes.Ivisto, CusEntryNumSchema.CE_EntryStatus, comparisonOperator, value);
		}
	}

	#endregion

	#region Arrival Date

	void AddArrivalDateFilter(ModuleFilterCollection filters)
	{
		var arrivalDateFilter = filters.AddDateFilter(ITFilterConstants.ArrivalDateFilter, GetArrivalDateQuery);
		arrivalDateFilter.Category = FilterCategories.Dates;
		arrivalDateFilter.MultilingualDescription = ResString.GetMultilingualString("CDCD7CA2-471D-44BA-BA88-992FA2F7FC53", ITFilterConstants.ArrivalDateFilter);

		ZQuery GetArrivalDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2) => GetIssueDateFilterQuery(CusEntryNumberConstants.EntryTypes.Irildes, comparisonOperator, value1, value2);
	}

	#endregion

	#region Arrival Office Filter

	void AddArrivalOfficeFilter(ModuleFilterCollection filters)
	{
		var arrivalOfficeFilter = filters.AddNkFilter(ITFilterConstants.ArrivalOfficeFilter, GetArrivalOfficeFilterQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, parent.Lookups.CustomsOfficeList);
		arrivalOfficeFilter.MaxLength = CustomsOfficeMaxLength;
		arrivalOfficeFilter.Category = parent.CustomsOfficeCategory;
		arrivalOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("2752C0F3-4326-4164-BBF7-3B78A49A370E", ITFilterConstants.ArrivalOfficeFilter);

		ZQuery GetArrivalOfficeFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetFilterQuery(CusEntryNumberConstants.EntryTypes.Irildes, CusEntryNumSchema.CE_EntryLineReference, comparisonOperator, value);
		}
	}

	#endregion

	#region Arrival Status Filter

	void AddArrivalStatusFilter(ModuleFilterCollection filters)
	{
		var arrivalStatusFilter = filters.AddTextFilter(ITFilterConstants.ArrivalStatusFilter, GetExitStatusFilter, parent.Lookups.ArrivalStatusList);
		arrivalStatusFilter.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryStatus);
		arrivalStatusFilter.Category = FilterCategories.NumbersAndReferences;
		arrivalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("1606D1D5-1722-4FAF-BED6-B2CC4665C41A", ITFilterConstants.ArrivalStatusFilter);

		ZQuery GetExitStatusFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetFilterQuery(CusEntryNumberConstants.EntryTypes.Irildes, CusEntryNumSchema.CE_EntryStatus, comparisonOperator, value);
		}
	}

	#endregion

	ZDBOnlyQuery GetEntryHeaderQuery() => new ZDBOnlyQuery(typeof(CusEntryHeader));

	ZDBOnlySubQuery GetEntryNumberSubQuery(ZString entryType, bool notIn = false)
	{
		var cusEntryNumSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);
		cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryType);
		cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.Italy);

		return cusEntryNumSubQuery;
	}

	ZQuery GetCreationDateFilterQuery(ZString entryType, DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		=> GetDateFilterQuery(entryType, comparisonOperator, value1, value2, CusEntryNumSchema.CE_SystemCreateTimeUtc);

	ZQuery GetIssueDateFilterQuery(ZString entryType, DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		=> GetDateFilterQuery(entryType, comparisonOperator, value1, value2, CusEntryNumSchema.CE_IssueDate);

	ZQuery GetDateFilterQuery(ZString entryType, DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2, SchemaDateTimeColumn dateTimeColumn)
	{
		var query = GetEntryHeaderQuery();

		var entryNumberSubQuery = GetEntryNumberSubQuery(entryType);
		parent.AddDateTimeRange(entryNumberSubQuery, comparisonOperator, JoinCondition.And, dateTimeColumn, value1, value2);

		query.AddSubQuery(entryNumberSubQuery, JoinCondition.And);
		if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
		{
			var noEntryNumSubQuery = GetEntryNumberSubQuery(entryType, notIn: true);
			query.AddSubQuery(noEntryNumSubQuery, JoinCondition.Or);
		}

		return query;
	}

	ZQuery GetFilterQuery(ZString entryType, SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, ZString value)
	{
		var query = GetEntryHeaderQuery();

		var entryNumberQuery = GetEntryNumberSubQuery(entryType);
		entryNumberQuery.AddToFilter(schemaColumn, comparisonOperator, value);
		query.AddSubQuery(entryNumberQuery, JoinCondition.And);

		if (comparisonOperator == SpecialComparisonOperator.IsBlank)
		{
			var noEntryNumSubQuery = GetEntryNumberSubQuery(entryType, notIn: true);
			query.AddSubQuery(noEntryNumSubQuery, JoinCondition.Or);
		}

		return query;
	}

	const int CustomsOfficeMaxLength = 8;
}
