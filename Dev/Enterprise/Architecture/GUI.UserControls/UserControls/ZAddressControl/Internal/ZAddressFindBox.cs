using System.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[ToolboxItem(false)]
	[SuppressFormDesignerAnalysis]
	[CompositeFieldControl]
	public class ZAddressFindBox : ZGuidFindBox
	{
		[ToolboxItem(false)]
		public new class Bare : ZAddressFindBox
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		protected override void SetControlSize(int charLength)
		{
			base.SetControlSize(charLength);
			ZAddressControl parentAddressControl = Parent as ZAddressControl;
			if (parentAddressControl != null)
			{
				parentAddressControl.SetControlSize();
			}
		}
	}
}
