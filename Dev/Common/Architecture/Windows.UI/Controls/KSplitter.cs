using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Windows.UI.Interop;
using CargoWise.Windows.UI.Layout;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class KSplitter : Control, ISplitterLayoutSaveProvider
	{
		public KSplitter() : base()
		{
			this.minSize = 0x19;
			this.minExtra = 0x19;
			this.anchor = Point.Empty;
			this.splitSize = -1;
			this.splitterThickness = 3;
			this.lastDrawSplit = -1;
			base.SetStyle(ControlStyles.Selectable, false);
			this.TabStop = false;
			this.minSize = 0x19;
			this.minExtra = 0x19;
			this.Dock = DockStyle.Left;
		}

		// Fields
		Point anchor;
		BorderStyle borderStyle;
		static readonly object EVENT_MOVED = new object();
		static readonly object EVENT_MOVING = new object();
		int initTargetSize;
		int lastDrawSplit;
		int maxSize;
		int minExtra;
		int minSize;
		int splitSize;
		Control splitTarget;
		SplitterMessageFilter splitterMessageFilter;
		int splitterThickness;
		int parentControlWidth;
		int parentControlHeight;

		public event SplitterEventHandler SplitterMoved
		{
			add
			{
				base.Events.AddHandler(EVENT_MOVED, value);
			}
			remove
			{
				base.Events.RemoveHandler(EVENT_MOVED, value);
			}
		}

		public event SplitterEventHandler SplitterMoving
		{
			add
			{
				base.Events.AddHandler(EVENT_MOVING, value);
			}
			remove
			{
				base.Events.RemoveHandler(EVENT_MOVING, value);
			}
		}

		//Methods re-created using Reflection
		internal virtual Control ParentInternal
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				var parentField = typeof(Control).GetField("parent", BindingFlags.Instance | BindingFlags.NonPublic) ?? typeof(Control).GetField("_parent", BindingFlags.Instance | BindingFlags.NonPublic);
				return (Control)parentField.GetValue(this);
			}
			set
			{
				var parentField = typeof(Control).GetField("parent", BindingFlags.Instance | BindingFlags.NonPublic) ?? typeof(Control).GetField("_parent", BindingFlags.Instance | BindingFlags.NonPublic);
				if ((Control)parentField.GetValue(this) != value)
				{
					if (value != null)
					{
						value.Controls.Add(this);
					}
					else
					{
						((Control)parentField.GetValue(this)).Controls.Remove(this);
					}
				}
			}
		}

#if !WINZOR
		internal bool CaptureInternal
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				return (this.IsHandleCreated && (UnsafeNativeMethods.GetCapture() == this.Handle));
			}
			set
			{
				if (this.CaptureInternal != value)
				{
					if (value)
					{
						UnsafeNativeMethods.SetCapture(new HandleRef(this, this.Handle));
					}
					else
					{
						SafeNativeMethods.ReleaseCapture();
					}
				}
			}
		}
#endif

		//Methods
		void ApplySplitPosition()
		{
			this.SplitPosition = this.splitSize;
		}

		SplitData CalcSplitBounds()
		{
			SplitData data = new SplitData();
			Control control = this.FindTarget();
			data.target = control;
			if (control != null)
			{
				switch (control.Dock)
				{
					case DockStyle.Top:
					case DockStyle.Bottom:
						this.initTargetSize = control.Bounds.Height;
						break;

					case DockStyle.Left:
					case DockStyle.Right:
						this.initTargetSize = control.Bounds.Width;
						break;
				}
				Control parentInternal = this.ParentInternal;
				ControlCollection controls = parentInternal.Controls;
				int count = controls.Count;
				int num2 = 0;
				int num3 = 0;
				for (int i = 0; i < count; i++)
				{
					Control control3 = controls[i];
					if (control3 != control)
					{
						switch (control3.Dock)
						{
							case DockStyle.Top:
							case DockStyle.Bottom:
								num3 += control3.Height;
								break;

							case DockStyle.Left:
							case DockStyle.Right:
								num2 += control3.Width;
								break;
						}
					}
				}
				Size clientSize = parentInternal.ClientSize;
				if (this.Horizontal)
				{
					this.maxSize = (clientSize.Width - num2) - this.minExtra;
				}
				else
				{
					this.maxSize = (clientSize.Height - num3) - this.minExtra;
				}
				data.dockWidth = num2;
				data.dockHeight = num3;
			}
			return data;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Ported code directly from System.Windows.Forms. DPI awareness can come later if desired")]
		Rectangle CalcSplitLine(int splitSize, int minWeight)
		{
			Rectangle bounds = base.Bounds;
			Rectangle rectangle2 = this.splitTarget.Bounds;
			switch (this.Dock)
			{
				case DockStyle.Top:
					if (bounds.Height < minWeight)
					{
						bounds.Height = minWeight;
					}
					bounds.Y = rectangle2.Y + splitSize;
					return bounds;

				case DockStyle.Bottom:
					if (bounds.Height < minWeight)
					{
						bounds.Height = minWeight;
					}
					bounds.Y = ((rectangle2.Y + rectangle2.Height) - splitSize) - bounds.Height;
					return bounds;

				case DockStyle.Left:
					if (bounds.Width < minWeight)
					{
						bounds.Width = minWeight;
					}
					bounds.X = rectangle2.X + splitSize;
					return bounds;

				case DockStyle.Right:
					if (bounds.Width < minWeight)
					{
						bounds.Width = minWeight;
					}
					bounds.X = ((rectangle2.X + rectangle2.Width) - splitSize) - bounds.Width;
					return bounds;
			}
			return bounds;
		}

		int CalcSplitSize()
		{
			Control control = this.FindTarget();
			if (control != null)
			{
				Rectangle bounds = control.Bounds;
				switch (this.Dock)
				{
					case DockStyle.Top:
					case DockStyle.Bottom:
						return bounds.Height;

					case DockStyle.Left:
					case DockStyle.Right:
						return bounds.Width;
				}
			}
			return -1;
		}

		void DrawSplitBar(int mode)
		{
#if !WINZOR
			if ((mode != 1) && (this.lastDrawSplit != -1))
			{
				this.DrawSplitHelper(this.lastDrawSplit);
				this.lastDrawSplit = -1;
			}
			else if ((mode != 1) && (this.lastDrawSplit == -1))
			{
				return;
			}
			if (mode != 3)
			{
				this.DrawSplitHelper(this.splitSize);
				this.lastDrawSplit = this.splitSize;
			}
			else
			{
				if (this.lastDrawSplit != -1)
				{
					this.DrawSplitHelper(this.lastDrawSplit);
				}
				this.lastDrawSplit = -1;
			}
#endif
		}

#if !WINZOR
		void DrawSplitHelper(int splitSize)
		{
			if (this.splitTarget != null)
			{
				Rectangle rectangle = this.CalcSplitLine(splitSize, 3);

				using (Graphics g = this.ParentInternal.CreateGraphics())
				using (HatchBrush hatchBrush = new HatchBrush(HatchStyle.Percent50, Color.Black, Color.White))
				{
					g.FillRectangle(hatchBrush, rectangle);
				}
			}
		}

#endif

		Control FindTarget()
		{
			Control parentInternal = this.ParentInternal;
			if (parentInternal != null)
			{
				ControlCollection controls = parentInternal.Controls;
				int count = controls.Count;
				DockStyle dock = this.Dock;
				for (int i = 0; i < count; i++)
				{
					Control control2 = controls[i];
					if (control2 != this)
					{
						switch (dock)
						{
							case DockStyle.Top:
								if (control2.Bottom != base.Top)
								{
									break;
								}
								return control2;

							case DockStyle.Bottom:
								if (control2.Top != base.Bottom)
								{
									break;
								}
								return control2;

							case DockStyle.Left:
								if (control2.Right != base.Left)
								{
									break;
								}
								return control2;

							case DockStyle.Right:
								if (control2.Left != base.Right)
								{
									break;
								}
								return control2;
						}
					}
				}
			}
			return null;
		}

		int GetSplitSize(int x, int y)
		{
			int num;
			if (this.Horizontal)
			{
				num = x - this.anchor.X;
			}
			else
			{
				num = y - this.anchor.Y;
			}
			int num2 = 0;
			switch (this.Dock)
			{
				case DockStyle.Top:
					num2 = this.splitTarget.Height + num;
					break;

				case DockStyle.Bottom:
					num2 = this.splitTarget.Height - num;
					break;

				case DockStyle.Left:
					num2 = this.splitTarget.Width + num;
					break;

				case DockStyle.Right:
					num2 = this.splitTarget.Width - num;
					break;
			}
			return Math.Max(Math.Min(num2, this.maxSize), this.minSize);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if ((this.splitTarget != null) && (e.KeyCode == Keys.Escape))
			{
				this.SplitEnd(false);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if ((e.Button == MouseButtons.Left) && (e.Clicks == 1))
			{
				this.SplitBegin(e.X, e.Y);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (this.splitTarget != null)
			{
				int x = e.X + base.Left;
				int y = e.Y + base.Top;
				Rectangle rectangle = this.CalcSplitLine(this.GetSplitSize(e.X, e.Y), 0);
				int splitX = rectangle.X;
				int splitY = rectangle.Y;
				this.OnSplitterMoving(new SplitterEventArgs(x, y, splitX, splitY));
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (this.splitTarget != null)
			{
				int x = e.X;
				int left = base.Left;
				int y = e.Y;
				int top = base.Top;
				Rectangle rectangle = this.CalcSplitLine(this.GetSplitSize(e.X, e.Y), 0);
				int num5 = rectangle.X;
				int num6 = rectangle.Y;
				this.SplitEnd(true);
			}
		}

		Color originalBackColor;

		protected override void OnMouseEnter(EventArgs e)
		{
			originalBackColor = BackColor;
			BackColor = Color.DarkGray;

			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if (BackColor != originalBackColor)
			{
				BackColor = originalBackColor;
			}

			base.OnMouseLeave(e);
		}

		protected virtual void OnSplitterMoved(SplitterEventArgs sevent)
		{
			SplitterEventHandler handler = (SplitterEventHandler)base.Events[EVENT_MOVED];
			if (handler != null)
			{
				handler(this, sevent);
			}
			if (this.splitTarget != null)
			{
				this.SplitMove(sevent.SplitX, sevent.SplitY);
			}
		}

		protected virtual void OnSplitterMoving(SplitterEventArgs sevent)
		{
			SplitterEventHandler handler = (SplitterEventHandler)base.Events[EVENT_MOVING];
			if (handler != null)
			{
				handler(this, sevent);
			}
			if (this.splitTarget != null)
			{
				this.SplitMove(sevent.SplitX, sevent.SplitY);
			}
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			if (Parent != null)
			{
				Parent.SizeChanged -= ParentControl_Resized;
				Parent.SizeChanged += ParentControl_Resized;
			}
		}

		void ParentControl_Resized(object sender, EventArgs e)
		{
			if (FindForm()?.WindowState == FormWindowState.Minimized || Parent == null)
			{
				return;
			}

			var isParentControlSizeShrinked = Horizontal && Parent.Width < parentControlWidth || !Horizontal && Parent.Height < parentControlHeight;
			if (isParentControlSizeShrinked)
			{
				SplitPosition = CalcSplitSize();
			}
			parentControlWidth = Parent.Width;
			parentControlHeight = Parent.Height;
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (this.Horizontal)
			{
				if (width < 1)
				{
					width = 3;
				}
				this.splitterThickness = width;
			}
			else
			{
				if (height < 1)
				{
					height = 3;
				}
				this.splitterThickness = height;
			}
			base.SetBoundsCore(x, y, width, height, specified);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Ported code directly from System.Windows.Forms. DPI awareness can come later if desired")]
		void SplitBegin(int x, int y)
		{
#if !WINZOR
			SplitData data = this.CalcSplitBounds();
			if ((data.target != null) && (this.minSize < this.maxSize))
			{
				this.anchor = new Point(x, y);
				this.splitTarget = data.target;
				this.splitSize = this.GetSplitSize(x, y);
				//IntSecurity.UnmanagedCode.Assert();
				try
				{
					if (this.splitterMessageFilter != null)
					{
						this.splitterMessageFilter = new SplitterMessageFilter(this);
					}
					System.Windows.Forms.Application.AddMessageFilter(this.splitterMessageFilter);
				}
				finally
				{
					//CodeAccessPermission.RevertAssert();
				}
				CaptureInternal = true;
				this.DrawSplitBar(1);
			}
#endif
		}

		void SplitEnd(bool accept)
		{
#if !WINZOR
			this.DrawSplitBar(3);
			this.splitTarget = null;
			CaptureInternal = false;
			if (this.splitterMessageFilter != null)
			{
				System.Windows.Forms.Application.RemoveMessageFilter(this.splitterMessageFilter);
				this.splitterMessageFilter = null;
			}
			if (accept)
			{
				this.ApplySplitPosition();
			}
			else if (this.splitSize != this.initTargetSize)
			{
				this.SplitPosition = this.initTargetSize;
			}
			this.anchor = Point.Empty;
#endif
		}

		void SplitMove(int x, int y)
		{
			int splitSize = this.GetSplitSize((x - base.Left) + this.anchor.X, (y - base.Top) + this.anchor.Y);
			if (this.splitSize != splitSize)
			{
				this.splitSize = splitSize;
				this.DrawSplitBar(2);
			}
		}

		public override string ToString()
		{
			string str = base.ToString();
			return (str + (NoResString)", MinExtra: " + this.MinExtra.ToString(CultureInfo.CurrentCulture) + (NoResString)", MinSize: " + this.MinSize.ToString(CultureInfo.CurrentCulture));
		}

		// Properties
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public override bool AllowDrop
		{
			get
			{
				return base.AllowDrop;
			}
			set
			{
				base.AllowDrop = value;
			}
		}

		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DefaultValue(0)]
		public override AnchorStyles Anchor
		{
			get
			{
				return AnchorStyles.None;
			}
			set
			{
			}
		}

		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage
		{
			get
			{
				return base.BackgroundImage;
			}
			set
			{
				base.BackgroundImage = value;
			}
		}

		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout
		{
			get
			{
				return base.BackgroundImageLayout;
			}
			set
			{
				base.BackgroundImageLayout = value;
			}
		}

		[DispId(-504), DefaultValue(0)]
		public BorderStyle BorderStyle
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.borderStyle;
			}
			set
			{
				if (!((int)value >= 0 && (int)value <= 2))
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(BorderStyle));
				}
				if (this.borderStyle != value)
				{
					this.borderStyle = value;
					base.UpdateStyles();
				}
			}
		}

#if !WINZOR
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle &= -513;
				createParams.Style &= -8388609;
				switch (this.borderStyle)
				{
					case BorderStyle.FixedSingle:
						createParams.Style |= 0x800000;
						return createParams;

					case BorderStyle.Fixed3D:
						createParams.ExStyle |= 0x200;
						return createParams;
				}
				return createParams;
			}
		}

		protected override Cursor DefaultCursor
		{
			get
			{
				switch (this.Dock)
				{
					case DockStyle.Top:
					case DockStyle.Bottom:
						return Cursors.HSplit;

					case DockStyle.Left:
					case DockStyle.Right:
						return Cursors.VSplit;
				}
				return base.DefaultCursor;
			}
		}

		protected override ImeMode DefaultImeMode
		{
			get
			{
				return ImeMode.Disable;
			}
		}

#endif

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Ported code directly from System.Windows.Forms. DPI awareness can come later if desired")]
		protected override Size DefaultSize
		{
			get
			{
				return new Size(3, 3);
			}
		}

		[Localizable(true), DefaultValue(3), SuppressMessage("CargoWiseOne", "CW1017", Justification = "Ported code directly from System.Windows.Forms. DPI awareness can come later if desired")]
		public override DockStyle Dock
		{
			get
			{
				return base.Dock;
			}
			set
			{
				if (((value != DockStyle.Top) && (value != DockStyle.Bottom)) && ((value != DockStyle.Left) && (value != DockStyle.Right)))
				{
					throw new ArgumentException(string.Format("Invalid Doc Enum. Value = {0}", (int)Dock));
				}
				int splitterThickness = this.splitterThickness;
				base.Dock = value;
				switch (this.Dock)
				{
					case DockStyle.Top:
					case DockStyle.Bottom:
						if (this.splitterThickness == -1)
						{
							break;
						}
						base.Height = splitterThickness;
						return;

					case DockStyle.Left:
					case DockStyle.Right:
						if (this.splitterThickness != -1)
						{
							base.Width = splitterThickness;
						}
						break;

					default:
						return;
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		public override Font Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		public override Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		bool Horizontal
		{
			get
			{
				DockStyle dock = this.Dock;
				if (dock != DockStyle.Left)
				{
					return (dock == DockStyle.Right);
				}
				return true;
			}
		}

		[DefaultValue(0x19), Localizable(true)]
		public int MinExtra
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.minExtra;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				this.minExtra = value;
			}
		}

		[Localizable(true), DefaultValue(0x19)]
		public int MinSize
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.minSize;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				this.minSize = value;
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), SuppressMessage("CargoWiseOne", "CW1017", Justification = "Ported code directly from System.Windows.Forms. DPI awareness can come later if desired")]
		public int SplitPosition
		{
			get
			{
				if (this.splitSize == -1)
				{
					this.splitSize = this.CalcSplitSize();
				}
				return this.splitSize;
			}
			set
			{
				SplitData data = this.CalcSplitBounds();
				if (value > this.maxSize)
				{
					value = this.maxSize;
				}
				if (value < this.minSize)
				{
					value = this.minSize;
				}
				this.splitSize = value;
				this.DrawSplitBar(3);
				if (data.target == null)
				{
					this.splitSize = -1;
				}
				else
				{
					Rectangle bounds = data.target.Bounds;
					switch (this.Dock)
					{
						case DockStyle.Top:
							bounds.Height = value;
							break;

						case DockStyle.Bottom:
							bounds.Y += bounds.Height - this.splitSize;
							bounds.Height = value;
							break;

						case DockStyle.Left:
							bounds.Width = value;
							break;

						case DockStyle.Right:
							bounds.X += bounds.Width - this.splitSize;
							bounds.Width = value;
							break;
					}
					data.target.Bounds = bounds;
					//Application.DoEvents(); //GET #REKT APPLICATION.DOEVENTS()
					this.OnSplitterMoved(new SplitterEventArgs(base.Left, base.Top, base.Left + (bounds.Width / 2), base.Top + (bounds.Height / 2)));
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Bindable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		int ISplitterLayoutSaveProvider.SplitterPosition
		{
			get => SplitPosition;
			set => SplitPosition = value;
		}

		int ISplitterLayoutSaveProvider.ContainerSize
		{
			get
			{
				var result = 0;
				if (Parent != null)
				{
					if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
					{
						result = Parent.Height;
					}
					else
					{
						result = Parent.Width;
					}
				}
				return result;
			}
		}

		bool ISplitterLayoutSaveProvider.IsSplitterFixed
		{
			get => DoNotSaveSplitterLayout;
		}

		bool ISplitterLayoutSaveProvider.IsLayoutRestored
		{
			get;
			set;
		}

		public bool DoNotSaveSplitterLayout { get; set; }

		// Nested Types
		class SplitData
		{
			// Fields
			public int dockHeight;
			public int dockWidth;
			internal Control target;

			// Methods
			public SplitData()
			{
				this.dockWidth = -1;
				this.dockHeight = -1;
			}
		}

		class SplitterMessageFilter : IMessageFilter
		{
			public SplitterMessageFilter(KSplitter splitter)
			{
				this.owner = splitter;
			}

			// Fields
			readonly KSplitter owner;

			// Methods
			public bool PreFilterMessage(ref Message m)
			{
				if ((m.Msg < 0x100) || (m.Msg > 0x108))
				{
					return false;
				}
#pragma warning disable WFDEV001 // 'Message.LParam' is obsolete: 'Casting to/from IntPtr is unsafe, use LParamInternal.'
				if ((m.Msg == 0x100) && (((int)((long)m.WParam)) == 0x1b))
#pragma warning restore WFDEV001
				{
					this.owner.SplitEnd(false);
				}
				return true;
			}
		}
	}
}
