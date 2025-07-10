using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Business.Xsd;
using Enterprise.DataTransfer.Native.Business.Xsd.Type;
using Enterprise.DataTransfer.Native.Common;

namespace Enterprise.DataTransfer.Native.Business
{
	public static class PropertyDefExtension
	{
		public static XsdDataType GetXsdDataType(this IPropertyDef property)
		{
			var columnDef = property.ColumnDef;
			return XsdDataTypeFactory.Map(columnDef);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public static bool IsAddInfoAddress(this string propertyString)
		{
			return propertyString.Contains("Address") || propertyString.Contains("OA_");
		}

		public static string CondenseKeyValuePairsIntoSortedOneString(this IEnumerable<KeyValuePair<ZString, ZString>> pairs) => AddInfoParser.Serialise(pairs, sortByKey: true);
	}
}
