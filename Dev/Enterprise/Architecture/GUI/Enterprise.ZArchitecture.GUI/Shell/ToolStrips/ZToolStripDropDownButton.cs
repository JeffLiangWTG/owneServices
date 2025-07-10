using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI
{
	// It is inherited from Component, not Control
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ComponentDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class ZToolStripDropDownButton : ToolStripDropDownButton, IConvertedFromMenuItem
	{
		public ZToolStripDropDownButton()
		{
			translationFeedbackManager = new ToolStripItemTranslationFeedbackManager(this);
			devInfoPopupManager = new ToolStripItemDevInfoPopupManager(this);
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new string ToolTipText
		{
			get { return base.ToolTipText; }
			set { base.ToolTipText = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable(true)]
		public ResourceStringData CaptionResourceString
		{
			get
			{
				return captionResourceString ?? ResourceStringData.Empty;
			}
			set
			{
#if DEBUG
				if (DesignMode && value == null)
				{
					return;
				}
				ZLabelCaptionCache.Instance.OnDataHit(captionResourceString, this.Parent);
#endif
				captionResourceString = value;
				this.Text = value.Caption;
				if (!string.IsNullOrEmpty(captionResourceString.FullDescription))
				{
					this.ToolTipText = captionResourceString.FullDescription;
				}
			}
		}
		ResourceStringData captionResourceString;

		bool ShouldSerializeCaptionResourceString()
		{
			return !CaptionResourceString.IsEmpty();
		}

#if DEBUG
		protected override void OnParentChanged(ToolStrip oldParent, ToolStrip newParent)
		{
			base.OnParentChanged(oldParent, newParent);
			ZLabelCaptionCache.Instance.OnDataHit(captionResourceString, this.Parent);
		}
#endif

		protected override void OnClick(EventArgs e)
		{
			if (TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				translationFeedbackManager.OpenFeedbackForm();
			}
			else if (DevInfoPopupManager.InDevelopInformationMode())
			{
				devInfoPopupManager.ShowDevelopInfoForm();
			}
			else
			{
				base.OnClick(e);
			}
		}

		MenuItem IConvertedFromMenuItem.SourceMenuItem
		{
			get;
			set;
		}

		readonly ToolStripItemTranslationFeedbackManager translationFeedbackManager;
		readonly internal ToolStripItemDevInfoPopupManager devInfoPopupManager;
	}
}
