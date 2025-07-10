using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	[TestsSubclassesOf(typeof(CusStatementLine))]
	public abstract class CusStatementLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertType<CusStatementLineTypeDecider>(CusStatementLine.TypeDecider);
		}
	}
}
