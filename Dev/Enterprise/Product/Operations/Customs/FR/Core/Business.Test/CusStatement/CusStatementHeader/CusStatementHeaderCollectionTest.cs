using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	[TestedType(typeof(CusStatementHeaderCollection))]
	class CusStatementHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusStatementHeaderCollection>
	{
		protected override CusStatementHeaderCollection GetCollectionToTest()
		{
			return new CusStatementHeaderCollection(Factory);
		}
	}
}
