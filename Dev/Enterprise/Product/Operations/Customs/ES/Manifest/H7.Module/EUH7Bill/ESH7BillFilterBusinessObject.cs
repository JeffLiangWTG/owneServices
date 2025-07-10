using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Manifest.H7.Module
{
	public class ESH7BillFilterBusinessObject : EUH7BillFilterBusinessObject
	{
		public static class ESDescriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string H7MRN = "MRN (H7)";
			public const string G3LRN = "LRN (G3)";
			public const string G3MRN = "MRN (G3)";

			#endregion
		}

		protected override void SetLRNAndMRNFilter(ModuleFilterCollection result)
		{
			var g3LRNFilter = result.AddTextFilter(ESDescriptions.G3LRN, CusEntryNumSchema.CE_EntryNum)
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			g3LRNFilter.Category = FilterCategories.NumbersAndReferences;
			g3LRNFilter.SubGroup = new BillCusEntryNumSubGroup { EntryType = CusEntryNumberTypes.EU.LocalReferenceNumber, EntryLineReference = AsycudaBill.G3DeclarationType };
			g3LRNFilter.MultilingualDescription = ResString.GetMultilingualString("ESH7BillFilterBusinessObject|G3LocalReferenceNumber", ESDescriptions.G3LRN);

			var h7MRNFilter = result.AddTextFilter(ESDescriptions.H7MRN, CusEntryNumSchema.CE_EntryNum)
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			h7MRNFilter.Category = FilterCategories.NumbersAndReferences;
			h7MRNFilter.SubGroup = new BillCusEntryNumSubGroup { EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber, EntryLineReference = AsycudaBill.H7MessageType };
			h7MRNFilter.MultilingualDescription = ResString.GetMultilingualString("ESH7BillFilterBusinessObject|H7MovementReferenceNumber", ESDescriptions.H7MRN);

			var g3MRNFilter = result.AddTextFilter(ESDescriptions.G3MRN, CusEntryNumSchema.CE_EntryNum)
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			g3MRNFilter.Category = FilterCategories.NumbersAndReferences;
			g3MRNFilter.SubGroup = new BillCusEntryNumSubGroup { EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber, EntryLineReference = AsycudaBill.G3DeclarationType };
			g3MRNFilter.MultilingualDescription = ResString.GetMultilingualString("ESH7BillFilterBusinessObject|G3MovementReferenceNumber", ESDescriptions.G3MRN);
		}

		protected override void SetCustomStastusFilter(ModuleFilterCollection filters)
		{
			var customsStatus = Factory.GetCachedValue<ESH7AISEntryStatusList>();
			customsStatus.Sort();
			var customsStatusFilter = filters.AddTextFilter(Descriptions.CustomsStatus, AsycudaBillSchema.ABL_BillStatus, customsStatus);
			customsStatusFilter.Category = FilterCategories.StatusAndFlags;
			customsStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ESH7BillFilterBusinessObject|CustomsStatusFilter", Descriptions.CustomsStatus);
			customsStatusFilter.ComparisonOperator_List.Clear();
			customsStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.Exact);
			customsStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.NotEqual);
			customsStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.IsBlank);
			customsStatusFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.IsNotBlank);
			customsStatusFilter.ComparisonOperator_List.DefaultCode = ModuleNumberFilter.ComparisonConstants.Exact;
		}
	}
}
