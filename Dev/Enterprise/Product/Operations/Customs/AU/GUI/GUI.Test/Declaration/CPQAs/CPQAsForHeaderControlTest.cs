using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class CPQAsForHeaderControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (CPQAsForHeaderControl control = new CPQAsForHeaderControl())
			{
				AssertEquals("Header", control.ManditoryQuestionsGrid.ColumnLayoutContext);
			}
		}
	}
}
