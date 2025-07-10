using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="KTabPage"/></summary>
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KTabPage : TabPage, IVariableLengthCaptionRenderer
	{
		public KTabPage()
		{
			CaptionRenderer = new ControlTextVariableLengthCaptionRenderer(this);
		}

		public KTabPage(string text) : base(text)
		{
			CaptionRenderer = new ControlTextVariableLengthCaptionRenderer(this);
		}

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
			set { CaptionRenderer.Captions = value; }
		}

		bool IVariableLengthCaptionRenderer.IsCaptionOverridden
		{
			get { return IsCaptionOverridden; }
			set { IsCaptionOverridden = value; }
		}

		protected bool IsCaptionOverridden
		{
			get { return CaptionRenderer.IsCaptionOverridden; }
			set { CaptionRenderer.IsCaptionOverridden = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required or ShouldSerializeText() will fail")]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		protected bool ShouldSerializeText() // this will only work if the Text property is overridden in this class
		{
			// has the developer set the text property manually
			return CaptionRenderer.IsCaptionOverridden && !string.IsNullOrEmpty(Text);
		}

		readonly ControlTextVariableLengthCaptionRenderer CaptionRenderer;

		#endregion
	}
}
