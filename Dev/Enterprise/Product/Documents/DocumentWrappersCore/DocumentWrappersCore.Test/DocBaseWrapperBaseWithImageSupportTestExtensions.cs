using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentWrappersCore.Testing
{
	public static class DocBaseWrapperBaseWithImageSupportTestExtensions
	{
		public static void SetReportNameForTesting(this DocBaseWrapperBaseWithImageSupport wrapper, string reportName)
		{
			wrapper.WriteCustomConstant(Constants.TemplateDefined.ReportName, reportName);
		}

		public static void SetDocumentDirectionForTesting(this DocBaseWrapperBaseWithImageSupport wrapper, string documentDirection)
		{
			wrapper.WriteCustomConstant(Constants.TemplateDefined.DocumentDirection, documentDirection);
		}

		public static void WriteCustomConstant(this DocBaseWrapperBaseWithImageSupport wrapper, string key, object value)
		{
			wrapper.SetTemplateConstants(new Dictionary<string, object> { { key, value } });
		}

		public static void SetTemplateConstants(this DocBaseWrapperBaseWithImageSupport wrapper, Dictionary<string, object> constants)
		{
			if (Globals.IsTest)
			{
				wrapper.Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(constants);
			}
		}
	}
}
