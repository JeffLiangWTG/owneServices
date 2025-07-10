using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusMiscRequestLineCollection))]
	sealed class CusMiscRequestLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusMiscRequestLineCollection>
	{
		protected override CusMiscRequestLineCollection GetCollectionToTest() => new CusMiscRequestLineCollection(Header);
		CusMiscRequestHeader Header => header ?? (header = Factory.New<CusMiscRequestHeader>());
		CusMiscRequestHeader header;
	}
}
