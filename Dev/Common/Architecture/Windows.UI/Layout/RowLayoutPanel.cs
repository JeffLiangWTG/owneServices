using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using System.Windows.Forms.VisualStyles;

namespace CargoWise.Windows.UI.Layout
{
	/// <summary>
	/// A panel that hosts controls that are organized into fixed height rows.
	/// </summary>
	[ProvideProperty("Row", typeof(Control))]
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class RowLayoutPanel : Panel, IExtenderProvider
	{
		#region Properties

		/// <summary>
		/// Get or set the fixed height for each row.
		/// </summary>
		[DpiState(DpiState.ScaleY)]
		[AttributeProvider("CargoWise.Windows.UI.Layout.RowLayout", nameof(RowHeight))]
		public int RowHeight
		{
			get { return RowLayoutEngine.RowHeight; }
			set
			{
				RowLayoutEngine.RowHeight = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Get or set whether to automatically update the tab order at design time.
		/// </summary>
		[AttributeProvider("CargoWise.Windows.UI.Layout.RowLayout", nameof(AutoTabOrder))]
		public bool AutoTabOrder
		{
			get { return RowLayoutEngine.AutoTabOrder; }
			set { RowLayoutEngine.AutoTabOrder = value; }
		}

		[AttributeProvider("CargoWise.Windows.UI.Layout.RowLayout", nameof(Alignment))]
		public VerticalAlignment Alignment
		{
			get { return RowLayoutEngine.Alignment; }
			set { RowLayoutEngine.Alignment = value; }
		}

		[AttributeProvider("CargoWise.Windows.UI.Layout.RowLayout", nameof(FixedRows))]
		public bool FixedRows
		{
			get { return RowLayoutEngine.FixedRows; }
			set { RowLayoutEngine.FixedRows = value; }
		}

		#endregion

		#region Row Management

		/// <summary>
		/// Get the row a control belongs to.
		/// </summary>
		[Browsable(false)]
		public int GetRow(Control control)
		{
			return RowLayoutEngine.GetRow(control);
		}

		/// <summary>
		/// Set the row a control belongs to.
		/// </summary>
		public void SetRow(Control control, int value)
		{
			RowLayoutEngine.SetRow(control, value);
		}

		#endregion

		#region Design Time Painting

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (IsDesigning)
			{
				for (int i = RowHeight; i < Height; i += RowHeight)
				{
					e.Graphics.DrawLine(LinePen, ControlDpiScalingHelper.NewScaledPoint(0, i, false), ControlDpiScalingHelper.NewScaledPoint(Width, i, false));
				}
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "linePen", Justification = "Just because you don't think I'm disposing it doesn't mean I'm not...")]
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				linePen?.Dispose();
#if !WINZOR
				scrollRepaintTimer?.Dispose();
#endif
			}
		}

		Pen LinePen
		{
			get
			{
				if (linePen == null)
				{
					linePen = new Pen(SystemColors.ControlDarkDark);
					linePen.DashStyle = DashStyle.Dash;
				}
				return linePen;
			}
		}
		Pen linePen;

		bool IsDesigning
		{
			get { return Site != null && Site.DesignMode; }
		}

		#endregion

		#region IExtenderProvider Members

		bool IExtenderProvider.CanExtend(object extendee)
		{
			return CanExtend(extendee);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "extendee")]
		protected bool CanExtend(object extendee)
		{
			Control control = extendee as Control;
			return control != null && control.Parent == this;
		}

		#endregion

		#region Implementation

		public override LayoutEngine LayoutEngine
		{
			get { return RowLayoutEngine; }
		}

		RowLayout RowLayoutEngine
		{
			get { return rowLayoutEngine ?? (rowLayoutEngine = new RowLayout(this, base.LayoutEngine)); }
		}
		RowLayout rowLayoutEngine;

		protected override void OnVisibleChanged(EventArgs e)
		{
			IsPanelVisiblityChanging = true;
			try
			{
				base.OnVisibleChanged(e);
			}
			finally
			{
				IsPanelVisiblityChanging = false;
			}
		}

		internal bool IsPanelVisiblityChanging
		{
			get;
			private set;
		}

		#endregion

		#region OnScroll

#if !WINZOR

		[SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges", Justification = "We turn it off immediately. It's basically just a way to perform an action once on a delay.")]
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
		Timer scrollRepaintTimer;

		protected override void OnScroll(ScrollEventArgs se)
		{
			ScrollRepaintTimer.Start();
			base.OnScroll(se);
		}

#endif

		#endregion

		public override Size GetPreferredSize(Size proposedSize)
		{
			int width;
			if (proposedSize.Width <= 0 || proposedSize.Width >= Int16.MaxValue)
			{
				width = Width;
			}
			else
			{
				width = proposedSize.Width;
			}
			return ControlDpiScalingHelper.NewScaledSize(width, RowLayoutEngine.VisibleRowCount * RowHeight + Margin.Vertical, false);
		}
	}
}
