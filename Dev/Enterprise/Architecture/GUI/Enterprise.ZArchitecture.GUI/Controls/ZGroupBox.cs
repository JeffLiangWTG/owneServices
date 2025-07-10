using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(true)]
	public class ZGroupBox : KGroupBox, IIsVisibleForBindingControl, IExtendedControl, IResCaptionedControl
	{
		#region Bare

		[WTG.StaticAnalysis.Annotation.CodeAlive("There are future possible usages.")]
		[ToolboxItem(false)]
		public class Bare : ZGroupBox
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		public ZGroupBox()
		{
			UserEventTracker.Instance.AddUserEventToControl(this);
			DisposableLeakListener.Instance.RegisterDisposable(this);
			Extensions = NewExtensionCollection();

			this.DoubleBuffered = true; // Caption Labels don't flicker.
			devInfoPopupManager = new DevInfoPopupManager(this);
		}

#if WINZOR
		protected override Cursor DefaultCursor => Cursors.Default;
#endif

		protected virtual IControlExtensionCollection NewExtensionCollection()
		{
			return new ControlExtensionCollection(this) { new ZLabelCaptionRenderer() };
		}

		#region IsVisibleForBinding

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool IsVisibleForBinding
		{
			get { return Visible; }
			set
			{
				if (!fInVisibleForBinding)
				{
					fInVisibleForBinding = true;
					try
					{
						if (Visible != value)
						{
							Visible = value;
							OnIsVisibleForBindingChanged();
						}
					}
					finally
					{
						fInVisibleForBinding = false;
					}
				}
			}
		}

		bool fInVisibleForBinding;

		public event EventHandler IsVisibleForBindingChanged;

		protected virtual void OnIsVisibleForBindingChanged()
		{
			if (IsVisibleForBindingChanged != null)
			{
				IsVisibleForBindingChanged(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Drag and Drop

		[DefaultValue(true)]
		public override bool AllowDrop
		{
			get { return true; }
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			DragDropManager.HandleDragDrop(this, drgevent);
			base.OnDragDrop(drgevent);
		}

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			DragDropManager.HandleDragOver(this, drgevent);
			base.OnDragOver(drgevent);
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZGroupBox>()
				.Property("Text", "") // Property name
				.Property("IsVisibleForBinding", ZBool.True)
				.Result;
		}

		#endregion

		#region Painting Bold Font

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Font Font
		{
			get { return overriddenFont ?? base.Font; }
			set { base.Font = value; }
		}
#pragma warning disable IDE0044 //Object overriddenFont is getting modified, conflicting readonly property
		Font overriddenFont;
#pragma warning restore IDE0044

		[DefaultValue(FlatStyle.Standard)]
		public new FlatStyle FlatStyle
		{
			get { return base.FlatStyle; }
			set { base.FlatStyle = value == FlatStyle.System ? FlatStyle.Standard : value; }
		}

#if !WINZOR

		protected override void OnPaint(PaintEventArgs e)
		{
			overriddenFont = OFont.GetFontBold();
			try
			{
				base.OnPaint(e);
				if (OnPainted != null)
				{
					OnPainted(e);
				}
			}
			catch (System.Runtime.InteropServices.ExternalException)
			{
				// Do nothing, probably graphics context is not avaiable at the moment because of locked/sleeping/etc remote session.
				// Especially do not try to repaint immediatelly, as it will fail again.
				// Will be repainted when possible (when graphics context is available).
			}
			finally
			{
				overriddenFont = null;
			}
		}

#else

		protected override bool BoldLegend => true;

#endif

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
				DesignTimeTextChecker.UnSubscribe(Site, this);
				if (devInfoPopupManager != null)
				{
					devInfoPopupManager.Dispose();
				}
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Colorful border

		public Action<PaintEventArgs> OnPainted;

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region Implementation

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				base.Site = value;
				DesignTimeTextChecker.Subscribe(value, this);
			}
		}

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

		#endregion

		#region Translation Feedback

		Rectangle TextArea
		{
			get
			{
				if (textArea == null)
				{
					using (var graphics = CreateGraphics())
					{
						textArea = ControlDpiScalingHelper.NewScaledRectangle(
							ControlDpiScalingHelper.NewScaledPoint(ClientRectangle.X + ControlDpiScalingHelper.ScaleToCurrentDpiX(8), ClientRectangle.Y, false),
							graphics.MeasureString(Text, OFont.GetFontBold()).ToSize(),
							false);
					}
				}
				return textArea.Value;
			}
		}
		Rectangle? textArea;

		protected override void OnSizeChanged(EventArgs e)
		{
			textArea = null;
			base.OnSizeChanged(e);
		}

		protected override void OnLocationChanged(EventArgs e)
		{
			textArea = null;
			base.OnLocationChanged(e);
		}

		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				textArea = null;
				base.Text = value;
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (TranslationFeedbackManager.InTranslationFeedbackMode() && TextArea.Contains(e.Location))
			{
				TranslationFeedbackManager.PaintCaptionHighlight(this, TextArea);
				highlightForTranslationFeedbackMode = true;
			}
			else
			{
				OnMouseLeave(e);
			}

			base.OnMouseMove(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if (highlightForTranslationFeedbackMode)
			{
				highlightForTranslationFeedbackMode = false;
				Invalidate(TextArea);
				Update();
			}
			else
			{
				base.OnMouseLeave(e);
			}
		}

		protected override void OnClick(EventArgs e)
		{
			if (highlightForTranslationFeedbackMode)
			{
				TranslationFeedbackManager.OpenFeedbackForm(this);
			}
			base.OnClick(e);
		}

		bool highlightForTranslationFeedbackMode;

		#endregion

		[Browsable(true)]
		public ResourceStringData CaptionResourceString
		{
			get { return captionResourceString ?? ResourceStringData.Empty; }
			set
			{
#if DEBUG
				if (DesignMode && value == null)
				{
					return;
				}
#endif
				captionResourceString = value;
				this.RefreshCaptionLabel();
			}
		}
		ResourceStringData captionResourceString;

		bool ShouldSerializeCaptionResourceString()
		{
			return !CaptionResourceString.IsEmpty();
		}

		readonly internal DevInfoPopupManager devInfoPopupManager;
	}
}
