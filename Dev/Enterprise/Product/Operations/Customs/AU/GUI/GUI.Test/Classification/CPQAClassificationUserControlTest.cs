using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class CPQAClassificationUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (CPQAClassificationUserControl control = new CPQAClassificationUserControl())
			{
				AssertEquals("Classification", control.ManditoryQuestionsGrid.ColumnLayoutContext);
			}
		}
	}
}
