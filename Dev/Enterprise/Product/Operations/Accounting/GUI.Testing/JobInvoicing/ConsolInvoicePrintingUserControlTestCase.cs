using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	internal class ConsolInvoicePrintingUserControlTestCase : TestCaseWithFactory
	{
		public void TestSettingSecurityCheckPoint()
		{
			using (ConsolInvoicePrintingUserControl control = new ConsolInvoicePrintingUserControl(Env.Security.MaintainShipmentJobInvoicing))
			{
				AssertEquals(Env.Security.MaintainShipmentJobInvoicing, control.JobInvoicePrintingControl.PluginSecurity);
			}
		}
	}
}
