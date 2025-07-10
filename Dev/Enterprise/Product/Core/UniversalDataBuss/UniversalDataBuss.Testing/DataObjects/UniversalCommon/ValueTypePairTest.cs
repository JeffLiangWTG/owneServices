using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[TestedType(typeof(ValueTypePair))]
	class ValueTypePairTest : DataObjectTestCase<ValueTypePair>
	{
		protected override bool ShouldBeFlattenedIntoAttributes => true;
	}
}
