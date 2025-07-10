using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	public class NctsPreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestInitializeGridLayout()
		{
			using (var control = new NctsPreviousDocumentsUserControl())
			{
				var previousDocumentsGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				var lineNoColumnStyle = (ZCalcEditColumnStyleInfo)previousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_LineNo);
				CombineAssertions(() =>
				{
					AssertEquals("CSI_LineNo Width", 80, lineNoColumnStyle.Width);
					AssertEquals("CSI_LineNo Decimals", 0, lineNoColumnStyle.Decimals);
				});
			}
		}
	}
}
