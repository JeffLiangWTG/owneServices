using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	static class DocumentEngineTestHelperMethodExtensions
	{
		internal static string ToContents(this StmTemplate template)
		{
			return DocumentEngineTestHelper.GetTemplateContents(template);
		}
	}
}
