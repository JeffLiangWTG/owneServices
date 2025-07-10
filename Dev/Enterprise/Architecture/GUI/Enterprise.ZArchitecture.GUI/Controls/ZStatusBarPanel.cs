using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Common.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	// It is inherited from Component, not Control
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ComponentDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class ZStatusBarPanel : StatusBarPanel
	{
		public ZStatusBarPanel()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
			base.Dispose(disposing);
		}
	}
}
