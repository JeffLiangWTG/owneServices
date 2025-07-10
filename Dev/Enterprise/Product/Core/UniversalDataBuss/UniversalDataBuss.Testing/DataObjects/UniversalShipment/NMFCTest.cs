using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(NMFC))]
	class NMFCTest : DataObjectTestCase<NMFC>
	{
		protected override bool ShouldBeFlattenedIntoAttributes => false;
	}
}
