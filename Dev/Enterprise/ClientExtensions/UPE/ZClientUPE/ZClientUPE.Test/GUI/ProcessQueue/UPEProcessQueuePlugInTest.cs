using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class UPEProcessQueuePlugInTest : TestCaseWithFactory
	{
		public void TestGetNewUserControl()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			UPEProcessQueuePluginForTest plugin = new UPEProcessQueuePluginForTest(Factory.New<UPECusHAWB>());
			try
			{
				using (Control control = plugin.GetNewUserControl())
				{
					AssertEquals("Wrong type", typeof(UPEProcessQueueUserControl), control.GetType());
					AssertEquals("Wrong DockStyle", DockStyle.Fill, control.Dock);
				}
			}
			finally
			{
				plugin.Dispose();
			}
		}

		class UPEProcessQueuePluginForTest : UPEProcessQueuePlugin
		{
			public UPEProcessQueuePluginForTest(IProcessQueueParent processQueueParent) : base(processQueueParent)
			{
			}

			public new Control GetNewUserControl()
			{
				return base.GetNewUserControl();
			}
		}
	}
}
