using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZPanelTest : ZControlBaseTestCase<ZPanel>
	{
		protected override bool IsDragDropHandledByEDocs
		{
			get { return true; }
		}

		public void TestIExtendedControlMembers()
		{
			using (var panel = new ZPanel())
			{
				CombineAssertions(() =>
				{
					var extendedControl = (IExtendedControl)panel;
					AssertType<ControlExtensionCollection>("Extensions", panel.Extensions);
					AssertSame("Host", panel, extendedControl.Host);
				});
			}
		}
	}
}
