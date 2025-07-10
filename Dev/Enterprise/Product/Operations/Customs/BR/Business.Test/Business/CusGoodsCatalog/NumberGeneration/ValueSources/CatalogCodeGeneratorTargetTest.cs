using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class CatalogCodeGeneratorTargetTest : TestCaseWithFactory
	{
		public void TestMaxLength()
		{
			AssertEquals(50, target.MaxLength);
		}

		public void TestName()
		{
			AssertEquals("Catalog Code", target.Name);
		}

		protected override void SetUp()
		{
			base.SetUp();
			target = new CatalogCodeGeneratorTarget();
		}

		CatalogCodeGeneratorTarget target;
	}
}
