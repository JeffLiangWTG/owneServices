using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI
{
	// It is inherited from Component, not Control
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ComponentDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class ZToolStripButton : ToolStripButton, IResCaptionedControl, IConvertedFromMenuItem, IButton, IClickableNestedMenuItem
	{
		public ZToolStripButton()
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

		public bool IgnoreMnemonic { get; set; }

		protected override bool ProcessMnemonic(char charCode)
		{
			if (IgnoreMnemonic)
			{
				return false;
			}
			return base.ProcessMnemonic(charCode);
		}

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
				var zToolStrip = Parent as ZToolStrip;
				if (zToolStrip != null && zToolStrip.FocusOnClick && !zToolStrip.Focused)
				{
					zToolStrip.Focus();
				}

				var form = Parent != null ? Parent.FindForm() : null;
				using (PerformanceStatisticsCollector.StartMonitoring("ButtonClick:" + Text, form != null ? form.GetType().FullName : null)) // Used internally only
				{
					base.OnClick(e);
				}
			}
		}

		MenuItem IConvertedFromMenuItem.SourceMenuItem { get; set; }

		readonly ToolStripItemTranslationFeedbackManager translationFeedbackManager;
		readonly internal ToolStripItemDevInfoPopupManager devInfoPopupManager;

		#region IButton Members

		Control IButton.Parent
		{
			get
			{
				var toolStrip = GetCurrentParent();
				return toolStrip != null ? toolStrip.Parent : null;
			}
		}

		bool IButton.Focus()
		{
			var toolStrip = GetCurrentParent();
			if (toolStrip != null)
			{
				toolStrip.Focus();
			}

			Select();
			return true;
		}

		bool IButton.ShouldSetImage
		{
			get { return true; }
		}

		#endregion

		#region IButtonControl Members

		/// <summary>
		/// Needed to allow Form.AcceptButton and Form.CancelButton to work correctly
		/// </summary>
		DialogResult IButtonControl.DialogResult
		{
			get { return DialogResult.OK; }
			set { }
		}

		void IButtonControl.NotifyDefault(bool value)
		{
			// Do nothing - we don't need to update the visual style to indicate default button for a form
		}

		#endregion

		#region IClickableNestedMenuItem Members

		void IClickableNestedMenuItem.AddChildItem(IClickableNestedMenuItem child)
		{
		}

		#endregion
	}
}
