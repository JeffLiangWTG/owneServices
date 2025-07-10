using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class UserDefinedFieldBuilderBuilderTest : TestCase
	{
		public void TestUserDefinedFieldBuilderDocumenters()
		{
			var builder = new UserDefinedFieldBuilderBuilder();
			var documenters = builder.UserDefinedFieldBuilderDocumenters;
			Assert("Builder should have documenters defined.", documenters.Count > 0);
			foreach (var documenter in documenters)
			{
				Assert("Each documenter should have documentation.", documenters != null);
			}
		}
	}
}
