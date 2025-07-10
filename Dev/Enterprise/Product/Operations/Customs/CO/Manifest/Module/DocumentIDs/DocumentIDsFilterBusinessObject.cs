using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CO.Manifest.Module
{
	public class DocumentIDsFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		public static class FilterConstants
		{
			public const string TransactionReference = "Transaction Reference";
			public const string IsUsed = "Is Used";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var codeFilter = result.AddTextFilter(FilterConstants.TransactionReference, CusTransactionNumberSchema.TN_TransactionReference);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("E09FCC2C-1BD6-4AB8-B6A6-DCE77C48478B", "Number");
			codeFilter.Category = FilterCategories.NumbersAndReferences;

			var codeFilter2 = result.AddFlagFilter(FilterConstants.IsUsed, FilterConstants.IsUsed, CusTransactionNumberSchema.TN_IsUsed, ModuleFilterSubGroup.Default);
			codeFilter2.MultilingualDescription = ResString.GetMultilingualString("C03DFE90-EC93-4F01-9604-11EEDC54C932", "Is Used");
			codeFilter2.Category = FilterCategories.NumbersAndReferences;

			return result;
		}
	}
}
