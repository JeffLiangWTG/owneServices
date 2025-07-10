using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.Customs.ASYCUDA.Module;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Manifest.Module;

public sealed class AEManifestBillFilterStrip : ASYCUDAManifestBillFilterStrip
{
	public static class AeFilterConstants
	{
		public const string SplitBillNumber = "Split Bill Number";
		public static ResourceString SplitBillNumberFilterMultilingualDescription => ResString.GetMultilingualString("AeManifestBillFilterStrip|SplitBillNumberFilter", AeFilterConstants.SplitBillNumber);
	}

	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	=> new AEManifestBillFilterStrip();

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = base.GetModuleFiltersCore();

		var splitBillNumberFilter = filters.AddTextFilter(AeFilterConstants.SplitBillNumber, GetSplitBillNumberQuery);
		splitBillNumberFilter.Category = FilterCategories.TextSearch;
		splitBillNumberFilter.MultilingualDescription = AeFilterConstants.SplitBillNumberFilterMultilingualDescription;

		return filters;
	}

	ZQuery GetSplitBillNumberQuery(SQLComparisonOperator comparisonOperator, ZString filterText)
	{
		var query = new ZDBOnlyQuery(typeof(AsycudaBill));
		var splitBillNumberQuery = BillSplitNumberGenAddOnColumnHelper.GetQueryHandlingBlanks(AsycudaBill.Schema.ABL_SplitBillNumber, comparisonOperator, filterText);
		query.AddToFilter(splitBillNumberQuery);
		return query;
	}

	GenAddOnColumnQueryHelper BillSplitNumberGenAddOnColumnHelper => billSplitNumberGenAddOnColumnHelper ??= new GenAddOnColumnQueryHelper(typeof(AsycudaBill));

	GenAddOnColumnQueryHelper billSplitNumberGenAddOnColumnHelper;
}
