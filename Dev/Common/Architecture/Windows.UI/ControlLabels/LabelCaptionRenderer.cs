using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Renders captions on one or more controls. Implement IVariableLengthCaptionRenderer on your controls to handle the
	/// label caption rendering manually.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renderer")]
	public interface ILabelCaptionRenderer : IAutomaticLabelExtension, IVariableLengthCaptionRenderer
	{
		string Caption { get; set; }
		LabelCaptionAlignment Alignment { get; set; }
		int LabelTop { get; set; }
		bool Visible { get; set; }
		bool IsCaptionTruncated { get; set; }
		bool InHotCaptionFeedbackMode();
		StringRenderingOptions Options { get; set; }
		Font Font { get; }
		void ManuallySetCaptionToolTip(string tooltip);
	}

	/// <summary>
	/// Renders captions on one or more controls. Implement IVariableLengthCaptionRenderer on your controls to handle the
	/// label caption rendering manually.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renderer")]
	[ToolboxItem(false)]
	public partial class LabelCaptionRenderer :
		ILabelCaptionRenderer,
		IVariableLengthCaptionRenderer,
		IBindableComponent,
		IDisposable
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public LabelCaptionRenderer()
		{
			painter = new LabelCaptionPainter(this);
			Visible = true;
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public LabelCaptionRenderer(Control control)
			: this()
		{
			Argument.NotNull(control, "control");
			Control = control;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renderer")]
		public LabelCaptionRenderer(IVariableLengthCaptionRenderer captionRenderer)
			: this()
		{
			Argument.NotNull(captionRenderer, "captionRenderer");
			CustomRenderer.Renderer = captionRenderer;
		}

		~LabelCaptionRenderer()
		{
			Dispose(false);
		}

		public static Control GetRenderSurface(Control control, LabelCaptionAlignment alignment)
		{
			if ((alignment == LabelCaptionAlignment.Left && control.Left < ControlDpiScalingHelper.ScaleToCurrentDpiX(DistanceBetweenLabelAndControl)) ||
				(alignment == LabelCaptionAlignment.Top && control.Top < ControlDpiScalingHelper.ScaleToCurrentDpiY(DistanceBetweenLabelAndControl)))
			{
				return control.Parent?.Parent;
			}
			return control.Parent;
		}

		protected virtual Font GetDefaultFont() => RenderSurface == null ? null : RenderSurface.Font;

		public Font Font
		{
			get { return font ?? GetDefaultFont(); }
			set
			{
				if (font != value)
				{
					font = value;
					FontChanged();
					Invalidate();
				}
			}
		}

		Font font;
		int? fontHeight;

		int FontHeight => fontHeight ?? (fontHeight = Font.Height).Value;

		void renderSurface_FontChanged(object sender, EventArgs e) => FontChanged();

		void FontChanged() => fontHeight = null;

		public string Caption
		{
			get
			{
				string result = null;

				string[] allCaptions = Captions;
				if (allCaptions != null)
				{
					foreach (string caption in allCaptions)
					{
						if (result == null || caption != null && caption.Length > result.Length)
						{
							result = caption;
						}
					}
				}

				return result;
			}
			set { Captions = value == null ? Array.Empty<string>() : new[] { value }; }
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public virtual string[] Captions
		{
			get { return captions; }
			set
			{
				if (CustomRenderer.Renderer != null)
				{
					captions = value ?? Array.Empty<string>();
					CustomRenderer.Captions = value;
				}
				else
				{
					if (captions != value)
					{
						captions = value ?? Array.Empty<string>();
						Invalidate();
					}
				}
				OnCaptionChanged(EventArgs.Empty);
			}
		}
		string[] captions = Array.Empty<string>();

		public event EventHandler CaptionChanged;

		void OnCaptionChanged(EventArgs e)
		{
			if (CaptionChanged != null)
			{
				CaptionChanged(this, e);
			}
		}

		public bool Visible
		{
			get { return visible; }
			set
			{
				if (visible != value)
				{
					visible = value;
					if (!value)
					{
						if (RenderSurface != null && !lastPaintedRectangle.IsEmpty)
						{
							RenderSurface.Invalidate(lastPaintedRectangle);
						}
					}
					Invalidate();
				}
			}
		}
		bool visible;

		public LabelCaptionAlignment Alignment
		{
			get { return alignment; }
			set
			{
				if (alignment != value)
				{
					alignment = value;
					Invalidate();
				}
			}
		}
		LabelCaptionAlignment alignment = LabelCaptionAlignment.Default;

		public StringRenderingOptions Options
		{
			get { return options; }
			set
			{
				if (options != value)
				{
					options = value;
					Invalidate();
				}
			}
		}
		StringRenderingOptions options = StringRenderingOptions.Truncate;

		public virtual string LabelSeparator
		{
			get { return labelSeparator; }
			set
			{
				if (labelSeparator != value)
				{
					labelSeparator = value;
					Invalidate();
				}
			}
		}
		string labelSeparator;

		[DefaultValue(-1)]
		public int LabelTop
		{
			get { return labelTop; }
			set
			{
				if (labelTop != value)
				{
					labelTop = value;
					Invalidate();
				}
			}
		}
		int labelTop = -1;

		public bool IsThisCaptionTruncated()
		{
			return RenderSurface != null && MeasureCaption().Truncated;
		}

		public bool IsCaptionTruncated
		{
			get => isCaptionTruncated || IsThisCaptionTruncated();
			set => isCaptionTruncated = value;
		}
		bool isCaptionTruncated;

		public Color ForeColor
		{
			get => foreColor;
			set
			{
				if (foreColor != value)
				{
					foreColor = value;
					Invalidate();
				}
			}
		}
		Color foreColor;

		public virtual void Refresh()
		{
			Invalidate();
		}

		public bool FailureToDrawTraceEnabled { get; set; }

		#region Control property

		protected virtual Control Control
		{
			get { return control; }
			set
			{
				if (value != Control)
				{
					IsPaintingActive = false;
					if (Control != null)
					{
						Control.ParentChanged -= UpdateIsPaintingActiveOrCustomRenderer;
						Control.HandleCreated -= UpdateIsPaintingActiveOrCustomRenderer;
						Control.VisibleChanged -= UpdateIsPaintingActiveOrCustomRenderer;
						Control.Disposed -= Control_Disposed;
					}
					control = value;
					TabPageForVisibilityMonitor = Control as TabPage;
					CustomRenderer.Control = value;
					if (Control != null)
					{
						Control.ParentChanged += UpdateIsPaintingActiveOrCustomRenderer;
						Control.HandleCreated += UpdateIsPaintingActiveOrCustomRenderer;
						Control.VisibleChanged += UpdateIsPaintingActiveOrCustomRenderer;
						Control.Disposed += Control_Disposed;
					}
					UpdateIsPaintingActive();
				}
			}
		}
		Control control;

		TabPage TabPageForVisibilityMonitor
		{
			get { return tabPageForVisibilityMonitor; }
			set
			{
				if (tabPageForVisibilityMonitor != null)
				{
					tabPageForVisibilityMonitor.ParentChanged -= TabPageForVisibilityMonitor_ParentChanged;
				}
				tabPageForVisibilityMonitor = value;
				TabControlForVisibilityMonitor = value == null ? null : value.Parent as TabControl;
				if (tabPageForVisibilityMonitor != null)
				{
					tabPageForVisibilityMonitor.ParentChanged += TabPageForVisibilityMonitor_ParentChanged;
				}
			}
		}
		TabPage tabPageForVisibilityMonitor;

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		TabControl TabControlForVisibilityMonitor
		{
			set
			{
				if (tabControlForVisibilityMonitor != null)
				{
					tabControlForVisibilityMonitor.ParentChanged -= UpdateIsPaintingActiveOrCustomRenderer;
					tabControlForVisibilityMonitor.VisibleChanged -= UpdateIsPaintingActiveOrCustomRenderer;
#if WINZOR
					tabControlForVisibilityMonitor.Paint -= UpdateIsPaintingActiveOrCustomRenderer;
#endif
				}
				tabControlForVisibilityMonitor = value;
				if (tabControlForVisibilityMonitor != null)
				{
					tabControlForVisibilityMonitor.ParentChanged += UpdateIsPaintingActiveOrCustomRenderer;
					tabControlForVisibilityMonitor.VisibleChanged += UpdateIsPaintingActiveOrCustomRenderer;
#if WINZOR
					tabControlForVisibilityMonitor.Paint += UpdateIsPaintingActiveOrCustomRenderer;
#endif
				}
			}
		}
		TabControl tabControlForVisibilityMonitor;

		void TabPageForVisibilityMonitor_ParentChanged(object sender, EventArgs e)
		{
			TabControlForVisibilityMonitor = TabPageForVisibilityMonitor.Parent as TabControl;
		}

		void UpdateIsPaintingActiveOrCustomRenderer(object sender, EventArgs e)
		{
			UpdateIsPaintingActiveOrCustomRenderer();
		}

		void UpdateIsPaintingActiveOrCustomRenderer()
		{
			if (CustomRenderer.Renderer != null)
			{
				if (!CustomRenderer.IsCaptionOverridden || Control.IsDesignMode())
				{
					CustomRenderer.Captions = Captions;
				}
			}
			else
			{
				UpdateIsPaintingActive();
			}
		}

		#endregion

		#region FindAndMeasureCaption

		protected LabelCaptionMeasurement MeasureCaption()
		{
			if (captionMeasurement == null)
			{
				using (PerformanceStatisticsCollector.StartMonitoring("LabelCaptionRenderer.MeasureCaption", ""))
				{
					captionMeasurement = MeasureCaptionCore();
				}
			}
			return captionMeasurement;
		}
#if DEBUG
		public
#endif
		LabelCaptionMeasurement captionMeasurement;

		protected virtual LabelCaptionMeasurement MeasureCaptionCore()
		{
			LabelCaptionMeasurement result;
			switch (Alignment)
			{
				case LabelCaptionAlignment.Left:
					result = MeasureCaption_ForLeft();
					break;

				case LabelCaptionAlignment.Top:
					result = MeasureCaption_ForTop();
					break;

				case LabelCaptionAlignment.Auto:
					result = MeasureCaption_ForLeft();
					if (result.Caption == null)
					{
						result = MeasureCaption_ForTop();
					}
					break;

				default:
					throw new InvalidOperationException();
			}
			return result;
		}

		LabelCaptionMeasurement MeasureCaption_ForTop()
		{
			SizeF size;
			bool truncated;
			int width = RenderSurface.DisplayRectangle.Width - GetControlLocationOnRenderSurface(Control).X;
			string caption = StringRenderingHelper.MeasureBestFit(Captions, width, LabelSeparator, Font, FontHeight, Options, out size, out truncated);

			var bounds = ControlDpiScalingHelper.NewScaledRectangle(Control.Left, Control.Top - (int)size.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(DistanceBetweenLabelAndControl), (int)size.Width, (int)size.Height, false);

			var renderSurfaceContainsBounds = RenderSurface.DisplayRectangle.Contains(bounds);
#if WINZOR
			if (RenderSurface is ScrollableControl)
			{
				renderSurfaceContainsBounds = true;
			}
#endif
			if (size.Width > width || !renderSurfaceContainsBounds)
			{
				bounds = Rectangle.Empty;
				caption = null;
			}

			return new LabelCaptionMeasurement(bounds, caption, truncated);
		}

		LabelCaptionMeasurement MeasureCaption_ForLeft()
		{
			var leftControlDist = NearestLeftControl?.Right > 0 ? NearestLeftControl?.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(DistanceBetweenLabelAndControl) : null;
			var left = Math.Max(leftControlDist ?? RenderSurface.DisplayRectangle.X, 0);

			var width = Math.Max(GetControlLocationOnRenderSurface(Control).X - left, 0);

			SizeF size;
			bool truncated;

			string caption = StringRenderingHelper.MeasureBestFit(Captions, width - ControlDpiScalingHelper.ScaleToCurrentDpiX(DistanceBetweenLabelAndControl), LabelSeparator, Font, FontHeight, Options, out size, out truncated);
			float x = GetControlLocationOnRenderSurface(Control).X - size.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(DistanceBetweenLabelAndControl);
			float effectiveLabelTop = LabelTop == -1 ? ((float)Control.Height / 2) - (size.Height / 2) : LabelTop;
			float y = GetControlLocationOnRenderSurface(Control).Y + effectiveLabelTop;

			var bounds = ControlDpiScalingHelper.NewScaledRectangle((int)x, (int)y, (int)size.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(1), (int)size.Height, false);

			var renderSurfaceContainsBounds = ControlDpiScalingHelper.NewScaledRectangle(ControlDpiScalingHelper.NewScaledPoint(0, 0), RenderSurface.Size).Contains(bounds);
#if WINZOR
			if (RenderSurface is ScrollableControl)
			{
				renderSurfaceContainsBounds = true;
			}
#endif
			if (size.Width > width || !renderSurfaceContainsBounds)
			{
				bounds = Rectangle.Empty;
				caption = null;
				truncated = false;
			}

			return new LabelCaptionMeasurement(bounds, caption, truncated);
		}

		#endregion

		#region IsPaintingActive

		void UpdateIsPaintingActive()
		{
			bool value = false;
			if (Visible && Control != null && (IsPaintingActive || Control.Created) && Control.Visible)
			{
				string[] result = Captions;
				value = result != null && result.Length > 0 && CustomRenderer.Renderer == null;
			}
			IsPaintingActive = value;
		}

		internal bool IsPaintingActive
		{
			get { return isPaintingActive; }
			set
			{
				if (IsPaintingActive != value)
				{
					if (IsPaintingActive && Control != null)
					{
						Control.ParentChanged -= UpdateRenderSurface;
						Control.HandleCreated -= UpdateRenderSurface;
						Control.VisibleChanged -= UpdateRenderSurface;
						Control.LocationChanged -= Control_LocationChanged;
						Control.SizeChanged -= Invalidate;
					}
					isPaintingActive = value;
					if (IsPaintingActive && Control != null)
					{
						Control.ParentChanged += UpdateRenderSurface;
						Control.HandleCreated += UpdateRenderSurface;
						Control.VisibleChanged += UpdateRenderSurface;
						Control.LocationChanged += Control_LocationChanged;
						Control.SizeChanged += Invalidate;
					}
					if (!IsPaintingActive && renderSurface != null)
					{
						renderSurface.Invalidate(lastPaintedRectangle);
					}
					UpdateRenderSurface();
				}
			}
		}
		bool isPaintingActive;

		#endregion

		#region IControlExtension

		IExtendedControl IControlExtension.Owner
		{
			get { return Control as IExtendedControl; }
		}

		public void Initialize(IExtendedControl owner)
		{
			if (Control != null)
			{
				throw new ArgumentException("The extension has already been initialized");
			}
			Argument.NotNull(owner, "owner");
			Control = owner as Control;
		}

		#endregion

		#region IVariableLengthCaptionRenderer

		string[] IVariableLengthCaptionRenderer.Captions
		{
			get { return Captions; }
			set { Captions = value; }
		}

		public virtual bool IsCaptionOverridden
		{
			get { return CustomRenderer.IsCaptionOverridden; }
			set { CustomRenderer.IsCaptionOverridden = value; }
		}

		#endregion

		#region IBindableComponent Members

		BindingContext IBindableComponent.BindingContext
		{
			get { return BindingContext; }
			set { throw new NotImplementedException(); }
		}

		protected BindingContext BindingContext
		{
			get { return Control.BindingContext; }
		}

		public ControlBindingsCollection DataBindings
		{
			get { return dataBindings ?? (dataBindings = new ControlBindingsCollection(this)); }
		}
		ControlBindingsCollection dataBindings;

		#endregion

		#region IComponent Members

		public event EventHandler Disposed;

		ISite IComponent.Site { get; set; }

		#endregion

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !isDisposed)
			{
				if (!lastPaintedRectangle.IsEmpty && renderSurface != null)
				{
					renderSurface.Invalidate(lastPaintedRectangle);
				}

				InvalidateLeftControl();
				RenderSurface = null;
				Control = null;
				IsPaintingActive = false;
				painter.Dispose();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);

				if (Disposed != null)
				{
					Disposed(this, EventArgs.Empty);
				}
				isDisposed = true;
			}
		}

		#endregion

		#region Implementation

		const int DistanceBetweenLabelAndControl = 2;
		Rectangle lastPaintedRectangle = Rectangle.Empty;
		bool isDisposed;

		ControlCustomVariableLengthCaptionRenderer CustomRenderer
		{
			get { return customRenderer ?? (customRenderer = new ControlCustomVariableLengthCaptionRenderer(() => Captions)); }
		}
		ControlCustomVariableLengthCaptionRenderer customRenderer;

		void Invalidate(object sender, EventArgs e)
		{
			Invalidate();
		}

		protected void Invalidate()
		{
			painter.Invalidate();
		}

		protected void InvalidateCore()
		{
			using (PerformanceStatisticsCollector.StartMonitoring("LabelCaptionRenderer.Invalidate", ""))
			{
				UpdateIsPaintingActive();

				captionMeasurement = null;
				lastPaintedRectangle = Rectangle.Empty;

				if (CustomRenderer.Renderer != null)
				{
					CustomRenderer.Captions = Captions;
				}
				else if (
					RenderSurface != null &&
					RenderSurface.Created &&
					RenderSurface.Visible &&
					Control.Created &&
					Control.Visible)
				{
					using (Region region = new Region())
					{
						if (!lastPaintedRectangle.IsEmpty)
						{
							region.Union(lastPaintedRectangle);
						}
						region.Union(MeasureCaption().CaptionBounds);
						RenderSurface.Invalidate(region);
					}
				}
			}
		}

		Control RenderSurface
		{
			get
			{
				UpdateRenderSurface();
				return renderSurface;
			}
			set
			{
				if (renderSurface != value)
				{
					if (renderSurface != null)
					{
#if WINZOR
						var form = renderSurface.FindForm();
						if (form != null)
						{
							form.Shown -= RenderSurface_FormShown;
						}
#endif
						renderSurface.Paint -= RenderSurface_Paint;
						renderSurface.Layout -= RenderSurface_Layout;
						renderSurface.FontChanged -= renderSurface_FontChanged;
						painter.UnhookRenderSurfaceEvents();
					}
					renderSurface = value;
					if (renderSurface != null)
					{
#if WINZOR
						var form = renderSurface.FindForm();
						if (form != null)
						{
							form.Shown += RenderSurface_FormShown;
						}
#endif
						renderSurface.Paint += RenderSurface_Paint;
						renderSurface.Layout += RenderSurface_Layout;
						renderSurface.FontChanged += renderSurface_FontChanged;
						painter.HookRenderSurfaceEvents();
					}
					FontChanged();
					OnRenderSurfaceLayoutChanged();
				}
			}
		}

#if WINZOR
		void RenderSurface_FormShown(object sender, EventArgs e)
		{
			Invalidate();
		}
#endif

		Control renderSurface;

		Control NearestLeftControl
		{
			get
			{
#if DEBUG
				if (!Control.Visible || !Control.Created)
				{
					throw new InvalidOperationException("Control must be visible and created to determine NearestLeftControl");
				}
#endif
				if (nearestLeftControl == null &&
					RenderSurface != null &&
					Control.Parent != null)
				{
					int mostRight = -1;
					foreach (Control sibling in RenderSurface.Controls)
					{
						if (sibling.Visible &&
							Control.Bottom > sibling.Top && sibling.Bottom > Control.Top &&
							sibling.Right < GetControlLocationOnRenderSurface(Control).X)
						{
							if (mostRight == -1 || sibling.Right > mostRight)
							{
								mostRight = sibling.Right;
								nearestLeftControl = sibling;
							}
						}
					}

					if (nearestLeftControl != null)
					{
						nearestLeftControl.LocationChanged += InvalidateLeftControl;
						nearestLeftControl.SizeChanged += InvalidateLeftControl;
					}
				}

				return nearestLeftControl;
			}
		}
		Control nearestLeftControl;

		void InvalidateLeftControl(object sender, EventArgs e)
		{
			InvalidateLeftControl();
		}

		void InvalidateLeftControl()
		{
			if (nearestLeftControl != null)
			{
				nearestLeftControl.LocationChanged -= InvalidateLeftControl;
				nearestLeftControl.SizeChanged -= InvalidateLeftControl;
				nearestLeftControl = null;
			}
		}

		bool HasObstruction
		{
			get
			{
#if DEBUG
				if (!Control.Visible || !Control.Created)
				{
					throw new InvalidOperationException("Control must be visible and created to determine HasObstruction");
				}
#endif
				if (hasObstruction == null && RenderSurface != null)
				{
					hasObstruction = GetObstruction() != null;
				}
				return (bool)hasObstruction;
			}
		}
		bool? hasObstruction;

		Control GetObstruction()
		{
			if (RenderSurface != null)
			{
				Rectangle rectangle = MeasureCaption().CaptionBounds;
				if (RenderSurface == null)
				{
					return null;
				}

				foreach (Control child in RenderSurface.Controls)
				{
					if (child.Created && child.Visible)
					{
						if (rectangle.IntersectsWith(child.Bounds))
						{
							return child;
						}
					}
				}
			}
			return null;
		}

		void UpdateRenderSurface(object sender, EventArgs e)
		{
			UpdateRenderSurface();
		}

		void UpdateRenderSurface()
		{
			RenderSurface = IsPaintingActive ? GetRenderSurface(Control, Alignment) : null;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "control")]
		Point GetControlLocationOnRenderSurface(Control control)
		{
			Point result = Point.Empty;
			if (control.Parent != null)
			{
				Point location = control.Parent.PointToScreen(control.Location);
				result = RenderSurface.PointToClient(location);
			}
			return result;
		}

		void Control_Disposed(object sender, EventArgs e)
		{
			Dispose();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "captionMeasurement")]
		void RenderSurface_Paint(object sender, PaintEventArgs e)
		{
			if (TraceFailureToDraw(Visible, (NoResString)"LabelCaptionRenderer not visible") && // Debug failure message
				TraceFailureToDraw(RenderSurface != null, (NoResString)"RenderSurface null") && // Debug failure message
				TraceFailureToDraw(Control.Created, (NoResString)"Control not created") && // Debug failure message
				TraceFailureToDraw(Control.Visible, (NoResString)"Control not visible") && // Debug failure message
				TraceFailureToDraw(Captions.Length > 0, (NoResString)"No caption available") && // Debug failure message
				TraceFailureToDraw(!HasObstruction, x => (NoResString)"Control with name '" + (GetObstruction() == null ? (NoResString)"<unknown>" : GetObstruction().Name) + (NoResString)"' is obstructing the label"))
			{
				var captionMeasurement = MeasureCaption();

				bool hasCaptionToRender = !string.IsNullOrEmpty(captionMeasurement.Caption);

				var clipRectangleIntersectsWithCaptionBounds = e.ClipRectangle.IntersectsWith(captionMeasurement.CaptionBounds);
#if WINZOR
				if (RenderSurface is ScrollableControl)
				{
					clipRectangleIntersectsWithCaptionBounds = true;
				}
#endif
				if (TraceFailureToDraw(hasCaptionToRender, (NoResString)"Caption of sufficient size not available") &&
					clipRectangleIntersectsWithCaptionBounds)
				{
					Paint(e, captionMeasurement);
					lastPaintedRectangle = captionMeasurement.CaptionBounds;
				}
			}
		}

		bool TraceFailureToDraw(bool condition, string reason)
		{
			return TraceFailureToDraw(condition, x => reason);
		}

		delegate string ReasonDelegate(object notUsed);
		bool TraceFailureToDraw(bool condition, ReasonDelegate reason)
		{
			if (FailureToDrawTraceEnabled && !condition)
			{
				Trace.WriteLine(GetType().Name + (NoResString)": " + control.Name + (NoResString)" failed to render; " + reason(null));
			}
			return condition;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded property names")]
		void RenderSurface_Layout(object sender, LayoutEventArgs e)
		{
			if (e.AffectedControl == null ||
				(e.AffectedControl.Parent == null || e.AffectedControl == RenderSurface || e.AffectedControl.Parent == RenderSurface) &&
				(e.AffectedProperty == "Parent" || e.AffectedProperty == "Visible" || e.AffectedProperty == "Bounds"))
			{
				if (e.AffectedProperty == "Visible")
				{
					OnRenderSurfaceLayoutChanged();
				}
				else if (Control.Created && Control.Visible)
				{
					if (e.AffectedControl != null &&
						(e.AffectedControl.Parent == null || e.AffectedControl.Parent == Control.Parent || e.AffectedControl == renderSurface || e.AffectedControl.Parent == renderSurface))
					{
						OnRenderSurfaceLayoutChanged();
					}
				}
			}
		}

		void Control_LocationChanged(object sender, EventArgs e)
		{
			if (Control.Created && Control.Visible)
			{
				OnRenderSurfaceLayoutChanged();
			}
		}

		void OnRenderSurfaceLayoutChanged()
		{
			hasObstruction = null;
			InvalidateLeftControl();
			captionMeasurement = null;
			Invalidate();
		}

		protected virtual void Paint(PaintEventArgs e, LabelCaptionMeasurement captionMeasurement)
		{
			painter.Paint(e, captionMeasurement);
		}

		protected readonly LabelCaptionPainter painter;

		#endregion

		#region HotCaptionFeedback

		public virtual bool InHotCaptionFeedbackMode() { return false; }
		public virtual void OpenCaptionFeedback() { }
		public virtual void PaintHotCaptionHighlight(Graphics graphics, Rectangle bounds) { }
		[SuppressMessage("Maintainability", "IDE0052: Remove unread private member.", Justification = "It is invoked implicitly")]
		string manuallyToolTip;

		public void ManuallySetCaptionToolTip(string tooltip)
		{
			manuallyToolTip = tooltip;
		}

		#endregion
	}
}
