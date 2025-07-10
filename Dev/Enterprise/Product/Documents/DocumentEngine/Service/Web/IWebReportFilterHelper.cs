using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine
{
	public interface IWebReportFilterHelper
	{
		bool HasWebModule(LookupFilterFieldBase filterField);
		bool IsSupportedOnWeb(FilterField filterField);
	}
}
