using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public class ZLinkLabel : KLinkLabel,
		IExtendedControl,
		IGridControl,
		IResCaptionedControl
	{
		#region Bare

		[WTG.StaticAnalysis.Annotation.CodeAlive("There are future possible usages.")]
		[ToolboxItem(false)]
		public class Bare : ZLinkLabel
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		public ZLinkLabel()
		{
			Font = OFont.GetFont();
			TextAlign = ContentAlignment.MiddleLeft;
			Extensions = NewExtensionCollection();
			translationFeedbackManager = new TranslationFeedbackManager(this);
			devInfoPopupManager = new DevInfoPopupManager(this);
		}

		readonly internal TranslationFeedbackManager translationFeedbackManager;
		readonly internal DevInfoPopupManager devInfoPopupManager;

		protected virtual IControlExtensionCollection NewExtensionCollection()
		{
			return new ControlExtensionCollection(this) { new ZLabelCaptionRenderer() };
		}

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

		public bool IsFontBold
		{
			get { return (Font == OFont.GetFontBold()); }
			set { Font = (value) ? OFont.GetFontBold() : OFont.GetFont(); }
		}

		/// <summary>
		/// This is here to fix a .NET bug. CargoWise.Windows.UI.KLabel hides the TabStop property, but
		/// CargoWise.Windows.UI.KLinkLabel (which extends Label), does not readvertise the property.
		/// </summary>
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), EditorBrowsable(EditorBrowsableState.Always)]
		[DefaultValue(true)]
		public new bool TabStop
		{
			get { return base.TabStop; }
			set { base.TabStop = value; }
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
				Extensions.Dispose();
				DesignTimeTextChecker.UnSubscribe(Site, this);
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

		#region IGridControl Members

		int IGridControl.SelectionStart
		{
			get { return 0; }
			set { }
		}

		int IGridControl.SelectionLength
		{
			get { return 0; }
			set { }
		}

		int IGridControl.ButtonWidth
		{
			get { return 0; }
		}

		event KeyEventHandler IGridControl.KeyDown
		{
			add { }
			remove { }
		}

		int IGridControl.MaxLength
		{
			get { return 0; }
			set { }
		}

		void IGridControl.ActivateEditControl()
		{
			Focus();
		}

		bool IGridControl.ShouldHandleKey(Keys keyData)
		{
			return false;
		}

		bool IGridControl.ShownForReadOnly
		{
			get { return true; }
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

		protected override void OnLinkClicked(LinkLabelLinkClickedEventArgs e)
		{
			if (!TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				var form = this.FindForm();
				using (PerformanceStatisticsCollector.StartMonitoring("LinkClicked:" + Name, form != null ? form.GetType().FullName : null))  // Used internally only
				{
					base.OnLinkClicked(e);
				}
			}
		}

#if !WINZOR

		protected override void OnMouseMove(MouseEventArgs e)
		{
			try
			{
				base.OnMouseMove(e);
			}
			catch (ArgumentException ex)
			{
				ProcessException(ex);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				base.OnPaint(e);
			}
			catch (ArgumentException ex)
			{
				ProcessException(ex);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This message is only used for developer team and not for the user.")]
		void ProcessException(ArgumentException ex)
		{
			var errorMessageBuilder = new StringBuilder(string.Format(CultureInfo.InvariantCulture, @"ZLinkLabel information:
Text: {0};
LinkArea Start: {1};
LinkArea Length: {2};
AutoSize: {3};
", Text, LinkArea.Start, LinkArea.Length, AutoSize));

			errorMessageBuilder.AppendLine("Links:");
			foreach (Link link in Links)
			{
				errorMessageBuilder.AppendLine(FormattableString.Invariant($"Start: {link.Start}, Length: {link.Length};"));
			}

			throw new ArgumentException(errorMessageBuilder.ToString(), ex);
		}

#endif

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZLinkLabel>().Result;
		}

#if DEBUG

		public void OnLinkClicked_Exposed(LinkLabelLinkClickedEventArgs e)
		{
			OnLinkClicked(e);
		}

#endif
	}
}
