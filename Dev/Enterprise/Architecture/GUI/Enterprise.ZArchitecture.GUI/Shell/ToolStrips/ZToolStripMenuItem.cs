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
	public class ZToolStripMenuItem : ToolStripMenuItem, IResCaptionedControl, IConvertedFromMenuItem, IClickableNestedMenuItem
	{
		public ZToolStripMenuItem()
		{
			translationFeedbackManager = new ToolStripItemTranslationFeedbackManager(this);
			devInfoPopupManager = new ToolStripItemDevInfoPopupManager(this);
		}

		public ZToolStripMenuItem(ResourceStringData caption)
			: this()
		{
			this.CaptionResourceString = caption;
		}

		public ZToolStripMenuItem(ResourceStringData caption, EventHandler onClick)
			: this(caption)
		{
			if (onClick != null)
			{
				this.Click += onClick;
			}
		}

		public ZToolStripMenuItem(string text)
			: this()
		{
			this.Text = text;
			this.Name = text; // Saving to the Name property allows the menu item to be found in DropDownItems dictionaries.
		}

		public ZToolStripMenuItem(string text, EventHandler onClick)
			: this(text)
		{
			if (onClick != null)
			{
				this.Click += onClick;
			}
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
			using (new MenuClickPendingTracker())
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
					using (PerformanceStatisticsCollector.StartMonitoring("ToolStripMenuClick", Text))
					{
						base.OnClick(e);
					}
				}
			}
		}

		MenuItem IConvertedFromMenuItem.SourceMenuItem { get; set; }

		readonly ToolStripItemTranslationFeedbackManager translationFeedbackManager;
		readonly internal ToolStripItemDevInfoPopupManager devInfoPopupManager;

		#region IClickableNestedMenuItem Members

		void IClickableNestedMenuItem.AddChildItem(IClickableNestedMenuItem child)
		{
			DropDownItems.Add((ToolStripItem)child);
		}

		#endregion
	}
}
