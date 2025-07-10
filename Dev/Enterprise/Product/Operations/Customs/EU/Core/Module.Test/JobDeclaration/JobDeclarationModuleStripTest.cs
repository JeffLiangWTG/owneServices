using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Module.Testing
{
	sealed class JobDeclarationModuleStripTest : TestCaseWithFactory
	{
		public void TestHandlesCustomsOffice()
		{
			using (var strip = new JobDeclarationModuleStrip())
			{
				MethodInfo info = typeof(JobDeclarationModuleStrip).GetMethod(
				"GetCurrentFilterControls", BindingFlags.Instance | BindingFlags.NonPublic);

				var filter = new CustomsOfficeFilter("CustomsOfficeFilter Test", delegate
				{ return null; }, new CodeDescriptionPairList());

				Control[] controls = (Control[])info.Invoke(strip, new object[] { filter });
				AssertEquals("Should provide controls for " + filter.GetType().Name, true, controls.Length > 0);

				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}
	}
}
