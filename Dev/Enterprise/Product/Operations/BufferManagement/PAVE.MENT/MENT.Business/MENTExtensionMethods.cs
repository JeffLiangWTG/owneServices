using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.PAVE.MENT.Business
{
	public static class MENTExtensionMethods
	{
		public static IEnumerable<ColumnsToCategoryIndex> CreateCategoryIndexMap(this IEnumerable<MENTDataRow> set)
		{
#if NETFRAMEWORK
			return set
				.DistinctBy(s => s.XCategoryString)
#else
			return IEnumerableExtensions.DistinctBy(set, s => s.XCategoryString)
#endif
				.OrderBy(x => x.XCategory, new RowSegmentDefinitionAndDataComparer())
				.Select((category, index) => new ColumnsToCategoryIndex([category.XCategoryString], index, category.XCategoryString));
		}

		public static string ToStringWithFormatForMENT(this IZType value)
		{
			string result;

			if (value is ZDecimal)
			{
				result = ((ZDecimal)value).ToString("G29", CultureInfo.InvariantCulture);
			}
			else if (value is ZInt)
			{
				result = ((ZInt)value).ToString();
			}
			else if (value is ZString)
			{
				result = ((ZString)value).ToString();
			}
			else if (value is ZDateTime)
			{
				result = ((ZDateTime)value).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
			}
			else
			{
				ErrorReporter.ReportOnce("MENTToStringUnhandled", string.Format(CultureInfo.InvariantCulture, "DataType was: {0}", value.BaseDataType.ToString()));
				result = value.ToString();
			}

			return result;
		}
	}
}
