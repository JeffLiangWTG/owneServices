using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	[TestedType(typeof(CusIntrastatHeaderCollection))]
	sealed class CusIntrastatHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusIntrastatHeaderCollection>
	{
	}
}
