using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class CPQAsForDrawbackControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (var control = new CPQAsForDrawbackControl())
			{
				AssertEquals("Drawback", control.ManditoryQuestionsGrid.ColumnLayoutContext);
			}
		}
	}
}
