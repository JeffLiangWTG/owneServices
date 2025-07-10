using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore;

namespace Enterprise.DocumentEngine.ValueProviders
{
	public static class MacroErrorReporter
	{
		public static void ReportFieldNotFound(string macro, Report report)
		{
			DataProviderList topLevelDataSource = null;
			if (report != null)
			{
				DataProviders.BusinessObjectDataProvider boDataProvider = report.DataProvider as DataProviders.BusinessObjectDataProvider;

				if (boDataProvider != null)
				{
					topLevelDataSource = boDataProvider.TopLevelDataSources;
				}
			}
			FieldNotFoundException.ReportFieldNotFound(macro, topLevelDataSource);
		}
	}
}