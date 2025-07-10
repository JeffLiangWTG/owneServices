using CargoWise.Common;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.ValueProviders
{
	public class MacroStringReplacer
	{
		public MacroStringReplacer(DataProviderList dataProviders)
		{
			Argument.NotNull(dataProviders, "DataProviderList dataProviders");
			this.dataProviders = dataProviders;
		}

		readonly DataProviderList dataProviders;

		Report report;
		Report Report
		{
			get { return report ?? (report = GetReportWithDocumentPack()); }
		}

		Report GetReportWithDocumentPack()
		{
			DocumentPack documentPack = new DocumentPack();
			var report = new Report(documentPack, null, dataProviders, string.Empty, null, DocumentDirection.ANY, false);
			documentPack.Add(report);
			return report;
		}

		public string ReplaceMacros(string stringWithMacrosInIt)
		{
			Report.ErrorManager.ClearErrors();
			CellContentReplacer cellContentReplacer = new CellContentReplacer(Report, stringWithMacrosInIt);
			cellContentReplacer.ReplaceMacros();
			return cellContentReplacer.ContentAsString;
		}

		internal string GetErrorsFromLastReplace()
		{
			if (Report.ErrorManager.HasErrors)
			{
				return Report.ErrorManager.ToString("{1}", false);
			}

			return string.Empty;
		}
	}
}
