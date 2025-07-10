using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.CH.Business.CusEntryHeader;

namespace Enterprise.Customs.CH.Module;

public class EntryHeaderFilterBusinessObject : Customs.Module.EntryHeaderFilterBusinessObject
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constraint name")]
	public static class FilterConstants
	{
		public const string PhaseStatus = "Phase Status";
		public const string AcceptanceDate = "Acceptance Date";
		public const string ActivationDeadline = "Activation Deadline";
		public const string LastEComStatus = "eCom Status";
		public const string SelectionResult = "Selection Result";
	}

	public new EntryHeaderFilterLookups Lookups => (EntryHeaderFilterLookups)base.Lookups;

	protected override Customs.Module.EntryHeaderFilterLookups GetNewLookups() => new EntryHeaderFilterLookups(this);

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = base.GetModuleFiltersCore();
		AddPhaseStatusFilter(filters);
		AddAcceptanceDateFilter(filters);
		AddActivationDeadlineFilter(filters);
		AddLastEComStatusFilter(filters);
		AddSelectionResultFilter(filters);
		return filters;
	}

	void AddPhaseStatusFilter(ModuleFilterCollection filters)
	{
		var phaseStatus = filters.AddTextFilter(FilterConstants.PhaseStatus, CusEntryHeaderSchema.CH_PhaseStatus, Lookups.PhaseStatusList).WithMaxLengthOf<ModuleTextFilter>(CusEntryHeaderSchema.CH_PhaseStatus);
		phaseStatus.Category = FilterCategories.StatusAndFlags;
		phaseStatus.MultilingualDescription = ResString.GetMultilingualString("E1F113CD-DBE9-4D2C-9180-43E3BE63AAD0", FilterConstants.PhaseStatus);
	}

	void AddSelectionResultFilter(ModuleFilterCollection filters)
	{
		var selectionResultFilter = filters.AddTextFilter(FilterConstants.SelectionResult, CusEntryNumSchema.CE_EntryStatus, Lookups.SelectionResultList).WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryStatus);
		selectionResultFilter.Category = FilterCategories.StatusAndFlags;
		selectionResultFilter.MultilingualDescription = ResString.GetMultilingualString("43B4BBBC-EA70-44B8-8F87-575E9A5E2154", FilterConstants.SelectionResult);
		selectionResultFilter.SubGroup = MRNSubGroup;
	}

	void AddAcceptanceDateFilter(ModuleFilterCollection filters)
	{
		var acceptanceDateFilter = filters.AddDateFilter(FilterConstants.AcceptanceDate, CusEntryNumSchema.CE_IssueDate);
		acceptanceDateFilter.Category = FilterCategories.Dates;
		acceptanceDateFilter.MultilingualDescription = ResString.GetMultilingualString("3E7D5D4C-5605-4794-ABAA-79F98560CC08", FilterConstants.AcceptanceDate);
		acceptanceDateFilter.SubGroup = MRNSubGroup;
	}

	void AddActivationDeadlineFilter(ModuleFilterCollection filters)
	{
		var activationDeadlineFilter = filters.AddDateFilter(FilterConstants.ActivationDeadline, CusEntryNumSchema.CE_ExpiryDate);
		activationDeadlineFilter.Category = FilterCategories.Dates;
		activationDeadlineFilter.MultilingualDescription = ResString.GetMultilingualString("ED9E8A13-D5F3-4F6B-9459-316DEF0EB891", FilterConstants.ActivationDeadline);
		activationDeadlineFilter.SubGroup = MRNSubGroup;
	}

	void AddLastEComStatusFilter(ModuleFilterCollection filters)
	{
		var lastEComStatus = filters.AddTextFilter(FilterConstants.LastEComStatus, GetCHCusEntryHeaderLastEComStatusFilterQuery, Lookups.EComplaintStatusList);
		lastEComStatus.MaxLength = CusEntryHeader.Schema.CH_LastEComplaintStatusMaxLength;
		lastEComStatus.Category = FilterCategories.StatusAndFlags;
		lastEComStatus.MultilingualDescription = ResString.GetMultilingualString("F34736B2-56D6-4BE5-B966-E6FB84C92021", FilterConstants.LastEComStatus);
	}

	ZQuery GetCHCusEntryHeaderLastEComStatusFilterQuery(SQLComparisonOperator filterOperator, ZString value)
	{
		return EntryHeaderModelViewColumnHelper.GetModuleFilterQuery(CusEntryHeader.Schema.CH_ClusterKey, ModelViewConstants.CHCusEntryHeader.ClusterKey, ModelViewConstants.CHCusEntryHeader.Name, ModelViewConstants.CHCusEntryHeader.LastEComplaintStatus, filterOperator, value);
	}

	ModelViewColumnQueryHelper<CusEntryHeader> EntryHeaderModelViewColumnHelper => entryHeaderModelViewColumnHelper ??= new ModelViewColumnQueryHelper<CusEntryHeader>();
	ModelViewColumnQueryHelper<CusEntryHeader> entryHeaderModelViewColumnHelper;

	ModuleFilterSubGroup MRNSubGroup => mrnSubGroup ??= new MRNFilterSubGroup();
	ModuleFilterSubGroup mrnSubGroup;

	class MRNFilterSubGroup : ModuleFilterSubGroup
	{
		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var mrnQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			mrnQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
			mrnQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			mrnQuery.AddToFilter(filter);
			query.AddSubQuery(mrnQuery, JoinCondition.And);
			return query;
		}
	}
}
