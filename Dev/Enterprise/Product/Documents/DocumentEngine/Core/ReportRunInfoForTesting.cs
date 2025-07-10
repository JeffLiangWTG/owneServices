#if DEBUG
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.Testing
{
	public class ReportRunInfoForTesting
	{
		public readonly string PivotTitle;
		public readonly string TemplateName;
		public readonly UserControlProviderList UserDefinedFieldValueList;

		public ReportRunInfoForTesting(string pivotTitle, string templateName, UserControlProviderList userDefinedFieldValueList)
		{
			PivotTitle = pivotTitle;
			TemplateName = templateName;
			UserDefinedFieldValueList = userDefinedFieldValueList;
		}
	}
}
#endif
