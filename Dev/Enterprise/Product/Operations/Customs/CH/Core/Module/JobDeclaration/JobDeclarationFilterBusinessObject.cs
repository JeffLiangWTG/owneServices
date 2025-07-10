using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Module;

public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class CHFilterConstants
	{
		public const string PhaseStatus = "Phase Status";
		public const string SelectionResult = "Selection Result";
	}

	public new JobDeclarationFilterLookups Lookups => (JobDeclarationFilterLookups)base.Lookups;

	protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups() => new JobDeclarationFilterLookups(this);

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = base.GetModuleFiltersCore();
		AddPhaseStatusFilter(filters);
		AddSelectionResultFilter(filters);
		return filters;
	}

	void AddPhaseStatusFilter(ModuleFilterCollection filters)
	{
		var phaseStatus = filters.AddTextFilter(CHFilterConstants.PhaseStatus, PhaseStatusQuery, Lookups.PhaseStatusList).WithMaxLengthOf<ModuleTextFilter>(CusEntryHeaderSchema.CH_PhaseStatus);
		phaseStatus.Category = FilterCategories.StatusAndFlags;
		phaseStatus.MultilingualDescription = ResString.GetMultilingualString("BE155569-5BDB-46AD-BB2A-E85924B2134E", CHFilterConstants.PhaseStatus);
	}

	ZQuery PhaseStatusQuery(ZString value)
	{
		var entryHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
		entryHeaderSubQuery.AddToFilter(CusEntryHeaderSchema.CH_PhaseStatus, value);

		var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
		declarationQuery.AddSubQuery(entryHeaderSubQuery, JoinCondition.And);

		return declarationQuery;
	}

	void AddSelectionResultFilter(ModuleFilterCollection filters)
	{
		var shouldCombineEntryStatusFromHeaders = ShouldCombineEntryStatusFromHeaders;
		var selectionResultFilter = new EntryStatusFilter(CHFilterConstants.SelectionResult, new JobDeclarationEntryStatusFilterHelper(SelectionResultQueryAny, SelectionResultQueryAll).GetEntryStatusFilter, Lookups.SelectionResultList).WithMaxLengthOf<EntryStatusFilter>(CusEntryNumSchema.CE_EntryStatus);
		selectionResultFilter.ShowFilterType = shouldCombineEntryStatusFromHeaders;
		selectionResultFilter.MultilingualDescription = ResString.GetMultilingualString("AEFC7DD1-7B1D-4977-8F30-07F807E4A4A9", CHFilterConstants.SelectionResult);
		filters.AddCustomFilter(selectionResultFilter);
	}

	ZQuery SelectionResultQueryAny(SQLComparisonOperator comparisonOperator, ZString value)
	{
		if (value == DeclarationFilterConstants.EntryStatus.NotSentForFilter)
		{
			value = ZString.Empty;
		}

		var query = new ZQuery();

		var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
		var entryHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
		var entryNumberSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
		entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, comparisonOperator, value);
		entryHeaderSubQuery.AddSubQuery(entryNumberSubQuery, JoinCondition.And);
		jobDeclarationQuery.AddSubQuery(entryHeaderSubQuery, JoinCondition.And);
		query.AddToFilter(jobDeclarationQuery, JoinCondition.And);

		return query;
	}

	ZQuery SelectionResultQueryAll(SQLComparisonOperator comparisonOperator, ZString value)
	{
		if (value == DeclarationFilterConstants.EntryStatus.NotSentForFilter)
		{
			value = ZString.Empty;
		}

		var text = FormattableString.Invariant($"[JE_PK] IN (SELECT [JE_PK] FROM [JobDeclaration], [CusEntryHeader], [CusEntryNum] WHERE [JE_PK] = [CH_JE] AND [CH_PK] = [CE_ParentID] AND [CE_EntryType] = '{CusEntryNumberTypes.Standard.MovementReferenceNumber}' GROUP BY [JE_PK] HAVING COUNT(*) = SUM(CASE WHEN [CE_EntryStatus] {comparisonOperator.ComparisonText(value)} '{value}' THEN 1 ELSE 0 END))");

		var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
		jobDeclarationQuery.AddFilterAndZSQLParameterCollection(text, null);

		return jobDeclarationQuery;
	}

	protected override bool ShouldCombineEntryStatusFromHeaders => true;
}
