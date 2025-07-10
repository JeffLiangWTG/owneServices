using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SurplusOutturnLineCollection))]
	sealed class SurplusOutturnLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SurplusOutturnLineCollection>
	{
		protected override SurplusOutturnLineCollection GetCollectionToTest()
		{
			return new SurplusOutturnLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SurplusOutturnLine(null, null, Factory);
		}
	}
}
