using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.PlugIn.Internal;

namespace Enterprise.Accounting.GUI.DataExportBatch.Testing
{
	internal class DataExportBatchPluginTest : TestCaseWithFactory
	{
		public void TestLicenceCheckPoint()
		{
			using (var plugin = new DataExportBatchPlugin(null))
			{
				AssertEquals(Env.Licence.Core, ((IPlugInInternals)plugin).LicenceCheckPoint);
			}
		}

		public void TestName()
		{
			using (var plugin = new DataExportBatchPlugin(null))
			{
				AssertEquals("Data Export Batch", plugin.Name);
			}
		}

		public void TestUserControl()
		{
			using (var plugin = new DataExportBatchPlugin(null))
			{
				var control = plugin.UserControl;
				AssertType<DataExportBatchUserControl>("UserControl", control);
				AssertEquals("UserControl.Dock", DockStyle.Fill, control.Dock);
			}
		}
	}
}
