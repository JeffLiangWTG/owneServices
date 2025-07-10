using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Testing
{
	sealed class DocumentGenerationHelperTest : TestCase
	{
		public void TestIsGeneratingDocument()
		{
			Assert(!DocumentGenerationHelper.IsGeneratingDocument);
			using (DocumentGenerationHelper.SetIsGeneratingDocument())
			{
				Assert(DocumentGenerationHelper.IsGeneratingDocument);
			}
			Assert(!DocumentGenerationHelper.IsGeneratingDocument);
		}
	}
}
