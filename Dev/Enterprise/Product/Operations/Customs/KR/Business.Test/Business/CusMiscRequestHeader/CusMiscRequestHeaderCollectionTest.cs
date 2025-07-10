using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusMiscRequestHeaderCollection))]
	sealed class CusMiscRequestHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusMiscRequestHeaderCollection>
	{
		protected override CusMiscRequestHeaderCollection GetCollectionToTest() => new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
	}
}
