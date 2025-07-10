using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(ContextType))]
	class ContextTypeTest : DataObjectTestCase<ContextType>
	{
		protected override bool ShouldBeFlattenedIntoAttributes
		{
			get { return true; } // This could possibly end up being a ICodeDataPair but isn't right now.
		}
	}
}

