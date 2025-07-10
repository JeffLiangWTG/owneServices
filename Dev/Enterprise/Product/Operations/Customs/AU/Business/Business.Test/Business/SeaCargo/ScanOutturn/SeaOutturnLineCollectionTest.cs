using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaOutturnLineCollection))]
	sealed class SeaOutturnLineCollectionTest : OutturnLineCollectionTest<SeaOutturnLineCollection>
	{
		protected override SeaOutturnLineCollection GetCollectionToTest()
		{
			return new SeaOutturnLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SeaOutturnLine();
		}
	}
}
