using System;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;

namespace Enterprise.DocumentEngine.Testing
{
	public class ReportForTestAllowingCacheReset : Report
	{
		public ReportForTestAllowingCacheReset(DocumentPack pack, ExcelTemplate template, Guid mainPK, Core.Constants.DataContext dataContext)
			: base(pack, template, mainPK, dataContext)
		{ }

		public ReportForTestAllowingCacheReset(DocumentPack pack, ExcelTemplate template, IBODocDataProvider docDataProvider, string reportName, UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying)
			: base(pack, template, docDataProvider, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying)
		{ }

		public void ResetCachedExcelFileForTesting()
		{
			base.ResetCachedExcelFile();
		}
	}
}
