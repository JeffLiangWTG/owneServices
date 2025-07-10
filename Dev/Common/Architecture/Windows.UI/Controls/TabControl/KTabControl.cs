using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI.Design;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="KTabControl"/></summary>
	[Designer(DesignerTypes.KTabControlDesigner)]
	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KTabControl : TabControl, ITabOrderExtendedToTabPages, IKControl
	{
		/// <summary>
		/// Get or set whether to the extend the tab order across tab pages.
		/// </summary>
		[Description("Set to true to extend the tab order across tab pages.")]
		[Category(DesignerConstants.Category)]
		[DefaultValue(false)]
		public bool TabOrderExtendedToTabPages { get; set; }

		[SmartTagVisible]
		public new TabPageCollection TabPages
		{ get { return base.TabPages; } }

		protected virtual Type TabPageType
		{ get { return typeof(KTabPage); } }

		protected override void Dispose(bool disposing)
		{
			// this object must be set to null to unhook an event that will otherwise to prevent a memory leak
			if (disposing)
			{
				ImageList = null;
			}
			base.Dispose(disposing);
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#region IKControl implementation

		protected override void CreateHandle()
		{
			if (!IsDisposed)
			{
				base.CreateHandle();
				OnHandleFullyCreated(EventArgs.Empty);
			}
		}

		protected virtual void OnHandleFullyCreated(EventArgs e)
		{
			if (IsHandleCreated && HandleFullyCreated != null)
			{
				HandleFullyCreated(this, e);
			}
		}

		public event EventHandler HandleFullyCreated;

		#endregion
	}
}
