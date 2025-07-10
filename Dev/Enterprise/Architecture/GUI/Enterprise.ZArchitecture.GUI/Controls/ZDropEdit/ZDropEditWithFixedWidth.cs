using System.ComponentModel;

using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public class ZDropEditWithFixedWidth : ZDropEdit
	{
		[ToolboxItem(false)]
		public new class Bare : ZDropEditWithFixedWidth
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		protected internal override void SetControlSize(int maxLength)
		{
			base.SetControlSize(PreBoundMaxLength);
		}
	}
}
