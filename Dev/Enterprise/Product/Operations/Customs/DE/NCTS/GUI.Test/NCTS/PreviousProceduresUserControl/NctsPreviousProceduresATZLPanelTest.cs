using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	sealed class NctsPreviousProceduresATZLPanelTest : TestCaseWithFactory
	{
		public void TestBinding()
		{
			using (var control = new NctsPreviousProceduresATZLPanel())
			{
				AssertEquals("LocalReferenceTextBox", "PreviousProcedureMaster.CSI_ReferenceNumber2", control.LocalReferenceTextBox.BindTo);
				AssertEquals("AuthorizationNumberDropEdit", "PreviousProcedureMaster.AuthorizationNumber", control.AuthorizationNumberDropEdit.BindTo);
			}
		}
	}
}
