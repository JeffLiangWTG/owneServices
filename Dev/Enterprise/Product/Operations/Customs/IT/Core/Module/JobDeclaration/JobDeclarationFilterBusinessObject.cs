using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Module;

sealed class JobDeclarationFilterBusinessObject : EU.Module.JobDeclarationFilterBusinessObject
{
	public static class DeclarationFilterConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant declaration")]
		public const string MessageVersion = "Message Version";
	}

	#region Lookups

	public new JobDeclarationFilterLookups Lookups => (JobDeclarationFilterLookups)base.Lookups;

	protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
	{
		return new JobDeclarationFilterLookups(this);
	}

	#endregion

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = base.GetModuleFiltersCore();
		AddMessageVersionFilters(filters);
		return filters;
	}

	void AddMessageVersionFilters(ModuleFilterCollection filters)
	{
		var filter = filters.AddTextFilter(DeclarationFilterConstants.MessageVersion, GetMessageVersionQuery, Lookups.MessageVersionList);
		filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
		filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
		filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
		filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
		filter.Category = FilterCategories.ModesAndTypes;
		filter.MultilingualDescription = ResString.GetMultilingualString("3D985D11-A8A2-4955-BE10-74B33E2951C8", DeclarationFilterConstants.MessageVersion);
		filter.MaxLength = JobDeclaration.Schema.MessageVersionMaxLength;
	}

	ZQuery GetMessageVersionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		=> QueryHelper.GetQueryHandlingBlanks(JobDeclaration.GenAddOnColumnConstants.MessageVersionColumnName, comparisonOperator, value);

	GenAddOnColumnQueryHelper QueryHelper => queryHelper ?? (queryHelper = new GenAddOnColumnQueryHelper(typeof(JobDeclaration)));
	GenAddOnColumnQueryHelper queryHelper;
}
