using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(Classification))]
	class ClassificationTest : DataObjectTestCase<Classification>
	{
		protected override bool ShouldBeFlattenedIntoAttributes
		{
			get { return false; }
		}
	}
}

