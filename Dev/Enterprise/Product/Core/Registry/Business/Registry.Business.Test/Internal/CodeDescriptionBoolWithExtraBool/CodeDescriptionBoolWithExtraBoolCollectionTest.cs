using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolWithExtraBoolCollection))]
	sealed class CodeDescriptionBoolWithExtraBoolCollectionTest : CodeDescriptionBoolWithExtraBoolCollectionAbstractTest<CodeDescriptionBoolWithExtraBoolCollection>
	{
		protected override CodeDescriptionBoolWithExtraBoolCollection GetCollectionToTest()
		{
			return new CodeDescriptionBoolWithExtraBoolCollection();
		}
	}
}
