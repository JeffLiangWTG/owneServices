using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class ContractNumbersUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new ContractNumbersUserControl())
			{
				AssertNotNull(control.Controls.Find("ContractNumbersTextBox", true).FirstOrDefault());
				AssertNotNull(control.Controls.Find("ContractNumbersEditButton", true).FirstOrDefault());
			}
		}
	}
}
