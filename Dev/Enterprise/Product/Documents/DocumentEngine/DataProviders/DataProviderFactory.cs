using CargoWise.Common;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore;

namespace Enterprise.DocumentEngine
{
	static class DataProviderFactory
	{
		public static IDataProvider GetDataProvider(Report report)
		{
			IDataProvider result;
			switch (report.Style)
			{
				case Report.Styles.Report:
					result = new ReportDataProvider(report);
					break;
				case Report.Styles.Document:
					result = GetBusinessObjectDataProvider(report.DataProviderList, report.OverridingDataSet);
					break;
				default:
					throw new DocumentEngineException("Report style not defined: " + report.Style.ToString());
			}

			return result;
		}

		internal static BusinessObjectDataProvider GetBusinessObjectDataProvider(DataProviderList dataProviderList, Visualisation.VisualiserDataSet visualiserDataSet = null)
		{
			var list = Lazy.Create(() => dataProviderList, false);
			var set = Lazy.Create(() => visualiserDataSet, false);
			return new BusinessObjectDataProvider(list, set);
		}
	}
}
