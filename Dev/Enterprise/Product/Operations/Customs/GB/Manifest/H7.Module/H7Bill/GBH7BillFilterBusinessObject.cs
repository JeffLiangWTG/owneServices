using Enterprise.Customs.EU.H7.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.H7.Module
{
	public class GBH7BillFilterBusinessObject : EUH7BillFilterBusinessObject
	{
		protected override void SetMessageStatusFilter(ModuleFilterCollection filters)
		{
			var messageStatusFilter = filters.AddTextFilter(Descriptions.MessageStatus, AsycudaBillSchema.ABL_MessageStatus, Factory.GetCachedValue<Common.Shared.MessageStatusList>());
			messageStatusFilter.Category = FilterCategories.StatusAndFlags;
			messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("GBH7BillFilterBusinessObject|MessageStatusFilter", Descriptions.MessageStatus);
			messageStatusFilter.ComparisonOperator_List.Clear();
			messageStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.Exact);
			messageStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.NotEqual);
			messageStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.IsBlank);
			messageStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.IsNotBlank);
			messageStatusFilter.ComparisonOperator_List.DefaultCode = ModuleNumberFilter.ComparisonConstants.Exact;
		}

		protected override EUH7BillFilterBusinessObjectLookups GetNewLookups() => new GBH7BillFilterBusinessObjectLookups(this);
	}
}
