using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Windows.Forms;
using CargoWise.Windows.UI.Controls;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="KPanel"/></summary>
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class KPanel : Panel, IDisposeStackProvider
	{
		#region ControlCollection

		protected override Control.ControlCollection CreateControlsInstance()
		{ return new ControlCollection(this); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public new class ControlCollection : AutoDisposeControlCollection
		{
			public ControlCollection(KPanel owner)
				: base(owner)
			{
			}
		}

		#endregion

		#region OnScroll

#if !WINZOR

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges", Justification = "We turn it off immediately. It's basically just a way to perform an action once on a delay.")]
		Timer ScrollRepaintTimer
		{
			get
			{
				if (scrollRepaintTimer == null)
				{
					scrollRepaintTimer = new Timer();
					scrollRepaintTimer.Interval = 50;
					scrollRepaintTimer.Tick += (o, e) => { scrollRepaintTimer.Stop(); Invalidate(true); };
				}
				return scrollRepaintTimer;
			}
		}

#endif

		#region Handle

		/// <summary>
		/// Overrides base CreateHandle method with adding information about dispose stack.
		/// </summary>
		protected override void CreateHandle()
		{
			try
			{
				if (!IsDisposed)
				{
					base.CreateHandle();
				}
			}
			catch (ObjectDisposedException ex)
			{
				throw new ObjectDisposedException(this.BuildDisposeInformation(), ex);
			}
		}

		#endregion

		#region Disposed Access Tracking

		[DefaultValue(false)]
		[Browsable(false)]
		public bool TrackDisposedAccess { get; set; }

		public StackTrace DisposeStack => disposeStack;
		StackTrace disposeStack;

		public string DisposeControlPath => disposeControlPath;
		string disposeControlPath;

		#endregion

#if !WINZOR

		Timer scrollRepaintTimer;

		protected override void OnScroll(ScrollEventArgs se)
		{
			ScrollRepaintTimer.Start();
			base.OnScroll(se);
		}

#endif

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (TrackDisposedAccess)
			{
				disposeStack = new StackTrace();
				disposeControlPath = ControlDescription.GetControlPath(this);
			}

			base.Dispose(disposing);
#if !WINZOR
			if (disposing)
			{
				scrollRepaintTimer?.Dispose();
			}
#endif
		}

		#endregion
	}
}
