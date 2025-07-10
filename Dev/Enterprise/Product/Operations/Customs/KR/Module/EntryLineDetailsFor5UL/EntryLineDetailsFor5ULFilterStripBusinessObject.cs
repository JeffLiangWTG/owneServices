using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Module
{
	public class EntryLineDetailsFor5ULFilterStripBusinessObject : FilterStripBusinessObject
	{
		public EntryLineDetailsFor5ULFilterStripBusinessObject()
		{
		}

		public static class Schema
		{
			public const string EntryNum = "Entry Number";
			public const string LineNum = "Entry Line No.";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var entryNumFilter = result.AddTextFilter(Schema.EntryNum, KREntryLineDetailsViewSchema.KEL_EntryNum);
			entryNumFilter.MultilingualDescription = ResString.GetMultilingualString("EntryLineDetailsFor5ULFilterStripBusinessObject|EntryNum", Schema.EntryNum);
			entryNumFilter.Category = FilterCategories.NumbersAndReferences;
			entryNumFilter.ComparisonOperator_List.DefaultCode = ModuleNumberFilter.ComparisonConstants.Exact;
			entryNumFilter.ReadOnly = true;
			entryNumFilter.Visibility = FilterVisibility.AlwaysVisible;

			var lineNumFilter = result.AddNumberFilter(Schema.LineNum, GetLineNumQuery);
			lineNumFilter.MultilingualDescription = ResString.GetMultilingualString("EntryLineDetailsFor5ULFilterStripBusinessObject|LineNum", Schema.LineNum);
			lineNumFilter.Category = FilterCategories.NumbersAndReferences;
			lineNumFilter.ComparisonOperator_List.DefaultCode = ModuleNumberFilter.ComparisonConstants.Exact;
			lineNumFilter.Visibility = FilterVisibility.AlwaysVisible;
			return result;
		}

		ZQuery GetLineNumQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();

			ZShort lineNum = ZShort.ParseSafe(value, 0);
			query.AddToFilter(KREntryLineDetailsViewSchema.KEL_LineNumber, SQLComparisonOperator.Equal, lineNum);

			return query;
		}
	}
}
