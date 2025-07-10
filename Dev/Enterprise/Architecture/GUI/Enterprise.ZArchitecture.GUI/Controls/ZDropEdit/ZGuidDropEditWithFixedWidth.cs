using System.ComponentModel;

using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public class ZGuidDropEditWithFixedWidth : ZGuidDropEdit
	{
		[WTG.StaticAnalysis.Annotation.CodeAlive("There are future possible usages.")]
		[ToolboxItem(false)]
		public new class Bare : ZGuidDropEditWithFixedWidth
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
