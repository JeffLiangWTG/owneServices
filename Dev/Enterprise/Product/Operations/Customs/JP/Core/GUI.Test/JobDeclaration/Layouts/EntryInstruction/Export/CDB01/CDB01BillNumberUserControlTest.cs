using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing;

[TestedType(typeof(CDB01BillNumberUserControl))]
sealed class CDB01BillNumberUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new CDB01BillNumberUserControl())
		{
			TestHelper.AssertControlExists(control, "CDB01BillNumberPanel", "");
			TestHelper.AssertControlExists(control, "BillNumberTextBox", "CEI_BillNumber");
			TestHelper.AssertControlExists(control, "BillNumberTypeDropEdit", "CEI_BillNumberType");
		}
	}
}
