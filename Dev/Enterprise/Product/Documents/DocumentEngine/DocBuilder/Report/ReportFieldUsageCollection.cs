using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class ReportFieldUsageCollection : DocumentMacroUsageCollection
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ReportFieldUsage();
		}
	}
}
