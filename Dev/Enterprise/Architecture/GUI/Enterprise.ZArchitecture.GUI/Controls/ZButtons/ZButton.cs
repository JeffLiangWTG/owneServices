using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public class ZButton : KButton, IIsVisibleForBindingControl, IIsEnabledForBindingControl, IExtendedControl, IResCaptionedControl, IButton, IReadOnlyAutomationOptional, IEditableInViewMode
	{
		#region Bare

		[ToolboxItem(false)]
		public class Bare : ZButton
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		const int DefaultButtonHeight = 23;

#if WINZOR
		protected override Cursor DefaultCursor => Cursors.Default;
#endif

		public ZButton()
		{
			UserEventTracker.Instance.AddUserEventToControl(this);
			DisposableLeakListener.Instance.RegisterDisposable(this);

			if (!DesignModeFinder.IsDesigning)
			{
				var themeColor = ObjectFactory.Get<ISystemDataRegistry>().ColorTheme.ButtonColor;
				if (themeColor != SystemColors.Control)
				{
					BackColor = themeColor;
				}
			}
			translationFeedbackManager = new TranslationFeedbackManager(this, TranslationFeedbackManager.ClickMode.None);
			devInfoPopupManager = new DevInfoPopupManager(this, TranslationFeedbackManager.ClickMode.None);
			Extensions = NewExtensionCollection();

			// Set the default height being DPI-aware
			ControlDpiScalingHelper.SetHeight(this, DefaultButtonHeight, true);
		}

		readonly internal TranslationFeedbackManager translationFeedbackManager;
		readonly internal DevInfoPopupManager devInfoPopupManager;

		#region Extensions

		protected virtual IControlExtensionCollection NewExtensionCollection()
		{
			var extensions = new ControlExtensionCollection(this) { new ZLabelCaptionRenderer() };
			if (!DesignModeFinder.IsDesigning)
			{
				extensions.Add(new HintExtension());
				extensions.Add(new StatusbarExtension());
				extensions.Add(new NotificationExtension());
			}
			return extensions;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				disposingStarted = true;
				if (translationFeedbackManager != null)
				{
					translationFeedbackManager.Dispose();
				}
				if (devInfoPopupManager != null)
				{
					devInfoPopupManager.Dispose();
				}
				if (ImageList != null)
				{
					ImageList = null;
				}
				Extensions.Dispose();
				DesignTimeTextChecker.UnSubscribe(Site, this);
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}

			base.Dispose(disposing);
		}

		bool disposingStarted;

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			if (disposingStarted)
			{
				return null;
			}
			return base.CreateAccessibilityInstance();
		}

		#endregion

		#region Style

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(FlatStyle.Standard)]
		public new FlatStyle FlatStyle
		{
			get { return base.FlatStyle; }
			set
			{
				if (value != FlatStyle.System)
				{
					base.FlatStyle = value;
				}
			}
		}

		#endregion

		#region MouseUp

		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			if (!IsDisposed)
			{
				base.OnMouseUp(mevent);
			}
		}

		#endregion

		#region Testing for Click is Being Hooked
#if DEBUG

		public bool ClickHasBeenHooked;

		public new event EventHandler Click
		{
			add
			{
				base.Click += value;
				ClickHasBeenHooked = true;
			}
			remove
			{
				base.Click -= value;
			}
		}

#endif
		#endregion

		#region Click

		protected override void OnClick(EventArgs e)
		{
			if (!translationFeedbackManager.HandleClick() && !devInfoPopupManager.HandleClick() && !isInClick)
			{
				isInClick = true;
				try
				{
					if (!Focused)
					{
						Focus();
					}
					var form = this.FindForm();
					using (PerformanceStatisticsCollector.StartMonitoring("ButtonClick:" + Text, form != null ? form.GetType().FullName : null))  // Used internally only
					{
						base.OnClick(e);
					}
				}
				finally
				{
					isInClick = false;
				}
			}
		}

		bool isInClick;

		#endregion

		#region ReadOnly Members

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(false)]
		public bool ReadOnly
		{
			get { return !Enabled; }
			set { Enabled = !value; }
		}

		#endregion

		#region WndProc

#if !WINZOR

		[Conditional("DEBUG")]
		internal void WndProcTesting(ref Message m)
		{
			WndProc(ref m);
		}

		protected override void WndProc(ref Message m)
		{
			try
			{
				base.WndProc(ref m);
			}
			catch (ObjectDisposedException)
			{
				//do nothing - object was disposed by rebinding while we were clicking it, for example.
			}
		}

#endif

#if DEBUG
		public static readonly Overridable<bool> OnPaintShouldThrowException_ForTest = new Overridable<bool>(defaultValue: false);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant exception message")]
#endif
		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				base.OnPaint(e);
#if DEBUG
				if (OnPaintShouldThrowException_ForTest.Value)
				{
					throw new ExternalException("A generic error occurred in GDI+");
				}
#endif
			}
			catch (ExternalException ex) when (ex.Message.Contains("A generic error occurred in GDI+"))
			{
				ZUserControl.ReportGraphicsDisplayFailure(ex);
			}
			catch (InvalidOperationException ex) when (ex.Message.Contains("Visual Style"))
			{
				ZUserControl.ReportGraphicsDisplayFailure(ex);
			}
		}

		protected virtual string GetStatisticsDescription()
		{
			return ControlStatisticsDescription.GetDescription(this);
		}

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				base.Site = value;
				DesignTimeTextChecker.Subscribe(value, this);
			}
		}

		#endregion

		#region IsVisibleForBinding

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool IsVisibleForBinding
		{
			get { return Visible; }
			set
			{
				if (!inVisibleForBinding)
				{
					inVisibleForBinding = true;
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
						inVisibleForBinding = false;
					}
				}
			}
		}

		public event EventHandler IsVisibleForBindingChanged;

		protected virtual void OnIsVisibleForBindingChanged()
		{
			if (IsVisibleForBindingChanged != null)
			{
				IsVisibleForBindingChanged(this, EventArgs.Empty);
			}
		}

		bool inVisibleForBinding;

		#endregion

		#region IsEnabledForBinding

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool IsEnabledForBinding
		{
			get { return Enabled; }
			set
			{
				if (!inEnabledForBinding)
				{
					inEnabledForBinding = true;
					try
					{
						if (Enabled != value)
						{
							Enabled = value;
							OnIsEnabledForBindingChanged();
						}
					}
					finally
					{
						inEnabledForBinding = false;
					}
				}
			}
		}

		public event EventHandler IsEnabledForBindingChanged;

		protected virtual void OnIsEnabledForBindingChanged()
		{
			if (IsEnabledForBindingChanged != null)
			{
				IsEnabledForBindingChanged(this, EventArgs.Empty);
			}
		}

		bool inEnabledForBinding;

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZButton>()
				.Property("Text", "") // Property name
				.Property("ReadOnly", true, false) // Property name
				.Property("IsVisibleForBinding", ZBool.True) // Property name
				.Property("IsEnabledForBinding", ZBool.True) // Property name
				.Result;
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region Resource Strings

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

		#endregion

		#region IButton Members

		bool IButton.ShouldSetImage
		{
			get { return false; }
		}

		[DefaultValue(false)]
		public bool EditableInViewMode { get; set; }

		[DefaultValue(false)]
		public bool DoNoOverrideMyEditableMode { get; set; }

		#endregion

		#region IReadOnlyAutomationOptional Members

		[DefaultValue(false)]
		public bool ShouldSetReadOnlyWhenSettingIncludingChildren { get; set; }

		#endregion
	}
}
