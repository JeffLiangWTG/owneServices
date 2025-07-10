using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(IsNeutralMaster))]
	class IsNeutralMasterTest : DataObjectTestCase<IsNeutralMaster>
	{
		protected override bool ShouldBeFlattenedIntoAttributes
		{
			get { return true; }
		}
	}
}
