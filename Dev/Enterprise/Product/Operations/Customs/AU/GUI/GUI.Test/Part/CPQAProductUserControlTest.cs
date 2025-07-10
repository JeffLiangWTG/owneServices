using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class CPQAProductUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (var control = new CPQAProductUserControl())
			{
				AssertEquals("Product", control.ManditoryQuestionsGrid.ColumnLayoutContext);
			}
		}
	}
}
