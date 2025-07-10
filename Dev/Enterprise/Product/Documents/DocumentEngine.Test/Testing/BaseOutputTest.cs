using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.Testing
{
	public abstract class BaseOutputTest : TestCaseWithFactory
	{
		protected DocumentPack Pack;

		protected override void SetUp()
		{
			base.SetUp();
			DocumentCommand stmMenuItem = Factory.New<DocumentCommand>();
			Pack = new DocumentPack(stmMenuItem);
		}
	}
}
