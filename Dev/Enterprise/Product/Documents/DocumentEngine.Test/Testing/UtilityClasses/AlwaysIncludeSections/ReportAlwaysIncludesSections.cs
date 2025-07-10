using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ExcelTemplates;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	class ReportAlwaysIncludesSections : Report
	{
		public ReportAlwaysIncludesSections(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying)
			: base(pack, template, docDataProvider, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying)
		{
		}

		public ReportAlwaysIncludesSections(DocumentPack pack, ExcelTemplate template, DataProviderList docDataProvider, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying, bool isPasswordProtectedForOpening)
			: base(pack, template, docDataProvider, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying, isPasswordProtectedForOpening)
		{
		}

		internal override ReportAnalyser GetNewReportAnalyser()
		{
			return new ReportAnalyserAlwaysIncludesSections(this);
		}
	}

	[TestedType(typeof(ReportAlwaysIncludesSections))]
	sealed class ReportAlwaysIncludesSectionsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReportAlwaysIncludesSections(new DocumentPack(), null, null, "", new UserControlProviderList(), DocumentDirection.ANY, false);
		}
	}
}
