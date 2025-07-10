using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class CPQAsForLineControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (CPQAsForLineControl control = new CPQAsForLineControl())
			{
				AssertEquals("Line", control.ManditoryQuestionsGrid.ColumnLayoutContext);
			}
		}
	}
}
