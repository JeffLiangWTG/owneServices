using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(FTADetailsUserControl))]
	sealed class FTADetailsUserControlTest : TestCaseWithFactory
	{
		public void TestFTADetailsGroupBox()
		{
			using (var control = new FTADetailsUserControl())
			{
				AssertEquals("LawCodeDropEdit Binding", "CustomsEntryInstructions.CEI_FTARelationArticleCode", control.FindSingle<ZDropEdit>("LawCodeDropEdit").BindTo);
				AssertEquals("CustomsDisbursementBillDropEdit Binding", "CustomsEntryInstructions.CEI_StatementNumber5WN", control.FindSingle<ZDropEdit>("CustomsDisbursementBillDropEdit").BindTo);
			}
		}
	}
}
