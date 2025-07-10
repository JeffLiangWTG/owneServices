using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class UPEProcessQueueUserControlTest : TestCaseWithFactory
	{
		public void TestCurrentUserControl()
		{
			using (UPEProcessQueueUserControlForTest uPEProcessQueueUserControl = new UPEProcessQueueUserControlForTest())
			{
				using (Control control = uPEProcessQueueUserControl.GetCurrentQueueUserControl())
				{
					AssertEquals("Wrong type", typeof(UPECurrentQueueUserControl), control.GetType());
				}
			}
		}

		class UPEProcessQueueUserControlForTest : UPEProcessQueueUserControl
		{
			public UPEProcessQueueUserControlForTest() : base()
			{
			}

			public new CurrentQueueUserControl GetCurrentQueueUserControl()
			{
				return new UPECurrentQueueUserControl();
			}
		}
	}
}
