using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.Test
{
	[TestedType(typeof(MENTAgedScoreQueryCollection))]
	class MENTAgedScoreQueryCollectionTest : ActiveBusinessObjectCollectionTestCase<MENTAgedScoreQueryCollection>
	{
		protected override MENTAgedScoreQueryCollection GetCollectionToTest()
		{
			return new MENTAgedScoreQueryCollection(Factory, new ZQuery());
		}
	}
}
