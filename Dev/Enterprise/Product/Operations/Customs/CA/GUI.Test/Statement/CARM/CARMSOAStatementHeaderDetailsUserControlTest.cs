using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CARMSOAStatementHeaderDetailsUserControl))]
	sealed class CARMSOAStatementHeaderDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var userControl = new CARMSOAStatementHeaderDetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("MessageTypeTextBox", true, userControl.Controls.Find("MessageTypeTextBox", true).First().Visible);
					AssertEquals("StatementTypeDropEdit", true, userControl.Controls.Find("StatementTypeDropEdit", true).First().Visible);
					AssertEquals("StatementNumberZTextBox", true, userControl.Controls.Find("StatementNumberZTextBox", true).First().Visible);
					AssertEquals("ImporterCustomsIDZTextBox", true, userControl.Controls.Find("ImporterCustomsIDZTextBox", true).First().Visible);
					AssertEquals("ImporterGuidFindBox", true, userControl.Controls.Find("ImporterGuidFindBox", true).First().Visible);
					AssertEquals("PrintDateDateEdit", true, userControl.Controls.Find("PrintDateDateEdit", true).First().Visible);
					AssertEquals("StatementAmountCalcEdit", true, userControl.Controls.Find("StatementAmountCalcEdit", true).First().Visible);
					AssertEquals("DueDateDateEdit", true, userControl.Controls.Find("DueDateDateEdit", true).First().Visible);
				});
			}
		}
	}
}
