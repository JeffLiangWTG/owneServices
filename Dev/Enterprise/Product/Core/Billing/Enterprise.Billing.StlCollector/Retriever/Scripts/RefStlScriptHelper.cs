using System;
using System.Globalization;
using CargoWise.Billing.Collectors;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	public static class RefStlScriptHelper
	{
		public static string DataGrainToCode(StlDataGrain dataGrain)
		{
			switch (dataGrain)
			{
				case StlDataGrain.Transactional:
					return RefStlItemGrain.Transactional;
				case StlDataGrain.MonthlyAllowHistoricalData:
					return RefStlItemGrain.MonthlyAllowHistoricalData;
				case StlDataGrain.MonthlyCurrentDataOnly:
					return RefStlItemGrain.MonthlyCurrentDataOnly;
				case StlDataGrain.Daily:
					return RefStlItemGrain.Daily;
				case StlDataGrain.Snapshot:
					return RefStlItemGrain.Snapshot;
				default:
					throw new ArgumentOutOfRangeException(dataGrain.ToString());
			}
		}

		public static string DateToCode(DateTime collectionStartDateUtc)
		{
			return collectionStartDateUtc == DateTime.MinValue ? null
				: collectionStartDateUtc.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
		}

		public static string DateTypeToCode(StlDateType dateType)
		{
			switch (dateType)
			{
				case StlDateType.DateTime:
					return RefStlDateType.DateTime;
				case StlDateType.DateTimeOffset:
					return RefStlDateType.DateTimeOffset;
				case StlDateType.SmallDateTime:
					return RefStlDateType.SmallDateTime;
				default:
					throw new ArgumentOutOfRangeException(dateType.ToString());
			}
		}
	}
}
