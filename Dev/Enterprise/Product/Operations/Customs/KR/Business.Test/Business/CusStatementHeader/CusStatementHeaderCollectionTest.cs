using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusStatementHeaderCollection))]
	sealed class CusStatementHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusStatementHeaderCollection>
	{
		protected override CusStatementHeaderCollection GetCollectionToTest()
		{
			return new CusStatementHeaderCollection(Factory);
		}
	}
}
