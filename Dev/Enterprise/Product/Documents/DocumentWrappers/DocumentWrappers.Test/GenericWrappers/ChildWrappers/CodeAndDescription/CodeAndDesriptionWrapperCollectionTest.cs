using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CodeAndDesriptionWrapperCollection))]
	sealed class CodeAndDesriptionWrapperCollectionTest : GenericWrapperCollectionTest<CodeAndDesriptionWrapperCollection>
	{
		protected override CodeAndDesriptionWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new CodeAndDesriptionWrapperCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			var list = new CodeDescriptionPairList();
			return new CodeAndDescriptionWrapper("", list, Factory);
		}
	}
}
