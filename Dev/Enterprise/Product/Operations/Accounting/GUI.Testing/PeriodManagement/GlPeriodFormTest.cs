using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	[TestedType(typeof(GlPeriodForm))]
	public class GlPeriodFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new GlPeriodForm(Factory.New<AccPeriodManagement>());
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (GlPeriodForm)GetFormToBashCore())
			{
				Assert("Alternate Chart form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}
	}
}
