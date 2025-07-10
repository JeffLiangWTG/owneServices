using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(true)]
	public class ZLabel : KLabel, IBindTo, IIsVisibleForBindingControl, IExtendedControl, IResCaptionedControl
	{
		#region Bare

		[WTG.StaticAnalysis.Annotation.CodeAlive("There are possible future usages.")]
		[ToolboxItem(false)]
		public class Bare : ZLabel
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		public ZLabel()
		{
			TextAlign = ContentAlignment.MiddleLeft;
			DisposableLeakListener.Instance.RegisterDisposable(this);
			Extensions = NewExtensionCollection();
			translationFeedbackManager = new TranslationFeedbackManager(this);
			devInfoPopupManager = new DevInfoPopupManager(this);
		}

		readonly internal TranslationFeedbackManager translationFeedbackManager;
		readonly internal DevInfoPopupManager devInfoPopupManager;

		protected virtual IControlExtensionCollection NewExtensionCollection()
		{
			return new ControlExtensionCollection(this) { new ZLabelCaptionRenderer(), new NotificationExtension() };
		}

		#region Properties

		[DefaultValue(ContentAlignment.MiddleLeft)]
		public override ContentAlignment TextAlign
		{
			get { return base.TextAlign; }
			set { base.TextAlign = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Font Font
		{
			get { return base.Font; }
			set { base.Font = value; }
		}

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(false)]
		public virtual bool IsFontBold
		{
			get { return (Font == OFont.GetFontBold()); }
			set { Font = value ? OFont.GetFontBold() : OFont.GetFont(); }
		}

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(OFontTypes.Normal)]
		public OFontTypes FontType
		{
			get { return OFont.FindFontType(Font); }
			set { Font = OFont.GetFont(value); }
		}

		[SmartTagVisible(-1)] // show this property even before BindTo
		[BindingOptions(UseTypeConverters = true)]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		#endregion

		#region Reporting BindTos when deleting

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				base.Site = value;
				if (value != null)
				{
					var service = (IComponentChangeService)value.GetService(typeof(IComponentChangeService));
					if (service != null)
					{
						service.ComponentRemoved -= ComponentChangeService_ComponentRemoved;
						service.ComponentRemoved += ComponentChangeService_ComponentRemoved;
					}
				}
				DesignTimeTextChecker.Subscribe(value, this);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This code is run in the Visual Studio process, Developer constant")]
		void ComponentChangeService_ComponentRemoved(object sender, ComponentEventArgs e)
		{
			var label = e.Component as ZLabel;
			if (label == this)
			{
				if (!label.Disposing && !string.IsNullOrEmpty(label.GetBindingMember()))
				{
					DesignTimeUI.ShowMessage(label,
@"
Label with text '".Trim() + label.Text + @"' had a BindingMember set on it. If you are replacing this
label with a rendered resource string caption, make sure you add this binding manually to
LabelCaptionRenderer (see the Wiki for more information).".Trim());
				}
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

		#region IBindTo Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZLabel>()
				.Property("Text", "") // Property name
				.Property("IsVisibleForBinding", ZBool.True)
				.Result;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (translationFeedbackManager != null)
				{
					translationFeedbackManager.Dispose();
				}
				if (devInfoPopupManager != null)
				{
					devInfoPopupManager.Dispose();
				}
				var service = Site == null ? null : (IComponentChangeService)Site.GetService(typeof(IComponentChangeService));
				if (service != null)
				{
					service.ComponentRemoved -= ComponentChangeService_ComponentRemoved;
					DesignTimeTextChecker.UnSubscribe(Site, this);
				}
				Extensions.Dispose();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
			base.Dispose(disposing);
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

		#region Implementation

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

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
	}
}
