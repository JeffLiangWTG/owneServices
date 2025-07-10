using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing.Text;
using System.Windows.Forms;
using CargoWise.ComponentModel.Design;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="Button"/></summary>
	[DefaultBindingProperty("Text")]
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class KButton : Button, IVariableLengthCaptionRenderer
	{
		public KButton()
		{
			CaptionRenderer = new ButtonBaseVariableLengthCaptionRenderer(this);
		}

		#region Designer Smart Tags

		[SmartTagVisible]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		[SmartTagVisible]
		public override AnchorStyles Anchor
		{
			get { return base.Anchor; }
			set { base.Anchor = value; }
		}

		[SmartTagVisible]
		public new event EventHandler Click
		{
			add { base.Click += value; }
			remove { base.Click -= value; }
		}

		#endregion

		#region IVariableLengthCaptionRenderer Members

		string[] IVariableLengthCaptionRenderer.Captions
		{
			get { return Captions; }
			set { Captions = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		protected string[] Captions
		{
			get { return CaptionRenderer.Captions; }
			set
			{
				CaptionRenderer.Captions = value;
				CaptionRenderer.HasAutoSetToolTip = true;
			}
		}

		public MultilingualString ToolTipCaption
		{
			get { return CaptionRenderer.ToolTipCaption; }
			set { CaptionRenderer.ToolTipCaption = value; }
		}

		bool IVariableLengthCaptionRenderer.IsCaptionOverridden
		{
			get { return IsCaptionOverridden; }
			set { IsCaptionOverridden = value; }
		}

		[DefaultValue(false)]
		public bool IsCaptionOverridden
		{
			get { return CaptionRenderer.IsCaptionOverridden; }
			set { CaptionRenderer.IsCaptionOverridden = value; }
		}

		protected bool ShouldSerializeText() // this will only work if the Text property is overridden in this class
		{
			// has the developer set the text property manually
			return CaptionRenderer.IsCaptionOverridden && !string.IsNullOrEmpty(Text);
		}

		readonly ButtonBaseVariableLengthCaptionRenderer CaptionRenderer;

		#endregion

		#region OnPaint
		[DefaultValue(TextRenderingHint.SystemDefault)]
		public TextRenderingHint TextRenderingHint { get; set; } = TextRenderingHint.SystemDefault;

#if !WINZOR

		protected override void OnPaint(PaintEventArgs pevent)
		{
			pevent.Graphics.TextRenderingHint = TextRenderingHint;
			base.OnPaint(pevent);
		}

#endif

		#endregion
	}
}
