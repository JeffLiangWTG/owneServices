using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class LPCOJobNumberGeneratorTargetTest : TestCaseWithFactory
	{
		public void TestMaxLength()
		{
			AssertEquals(35, target.MaxLength);
		}

		public void TestName()
		{
			AssertEquals("LPCO Job Number", target.Name);
		}

		protected override void SetUp()
		{
			base.SetUp();
			target = new LPCOJobNumberGeneratorTarget();
		}

		LPCOJobNumberGeneratorTarget target;
	}
}
