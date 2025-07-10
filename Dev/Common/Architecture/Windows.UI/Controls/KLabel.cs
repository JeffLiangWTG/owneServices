using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.ComponentModel.Design;

namespace CargoWise.Windows.UI
{
	/// <summary><see cref="KLabel"/></summary>
	[DesignTimeControlNameGenerator("lbl")]
	[DefaultBindingProperty("Text")]
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KLabel : Label, IVariableLengthCaptionRenderer
	{
		public KLabel()
		{
			CaptionRenderer = new LabelVariableLengthCaptionRenderer(this);
			UseMnemonic = false;
		}

		#region AutoSize=true while setting Text in HandleCreated problem fix

		[SmartTagVisible]
		public override string Text
		{
			get { return base.Text; }
			set
			{
				if (inOnHandleCreated && IsHandleCreated && AutoSize)
				{
					base.Text = value;
					BeginInvoke(new MethodInvoker(delegate
					{
						// invokes MeasureTextCache.InvalidateCache() and AdjustSize()
						if (!Disposing && !IsDisposed)
						{
							OnFontChanged(EventArgs.Empty);
						}
					}));
				}
				else
				{
					base.Text = value;
				}
			}
		}
			
		protected override void OnHandleCreated(EventArgs e)
		{
			inOnHandleCreated = true;
			try
			{
				base.OnHandleCreated(e);
			}
			finally
			{
				inOnHandleCreated = false;
			}
		}
		bool inOnHandleCreated;

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
			set { CaptionRenderer.Captions = value; }
		}

		bool IVariableLengthCaptionRenderer.IsCaptionOverridden
		{
			get { return IsCaptionOverridden; }
			set { IsCaptionOverridden = value; }
		}

		protected virtual bool IsCaptionOverridden
		{
			get { return CaptionRenderer.IsCaptionOverridden; }
			set { CaptionRenderer.IsCaptionOverridden = value; }
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
