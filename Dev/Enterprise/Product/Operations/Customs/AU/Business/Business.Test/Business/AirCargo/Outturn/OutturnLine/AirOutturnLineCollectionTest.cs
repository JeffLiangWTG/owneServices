using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirOutturnLineCollection))]
	sealed class AirOutturnLineCollectionTest : OutturnLineCollectionTest<AirOutturnLineCollection>
	{
		protected override AirOutturnLineCollection GetCollectionToTest() => new AirOutturnLineCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AirOutturnLine();
	}
}
