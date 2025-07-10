using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	public class FRStatementNumberGeneratorTargetTest : TestCaseWithFactory
	{
		public void TestMaxLength()
		{
			AssertEquals(25, target.MaxLength);
		}

		public void TestName()
		{
			AssertEquals("Statement Number", target.Name);
		}

		protected override void SetUp()
		{
			base.SetUp();
			target = new FRStatementNumberGeneratorTarget();
		}

		FRStatementNumberGeneratorTarget target;
	}
}
