using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class DocBuilderUsageCollection : DocumentMacroUsageCollection
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DocBuilderUsage();
		}
	}
}
