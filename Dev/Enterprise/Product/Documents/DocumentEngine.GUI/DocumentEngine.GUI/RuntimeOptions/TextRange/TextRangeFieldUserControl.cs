using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class TextRangeFieldUserControl : RuntimeOptionUserControl
	{
		public TextRangeFieldUserControl()
		{
			InitializeComponent();
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(filter.DisplayNameLocalized);
			textRangeEditControl.FromTextBox.DataBindings.Add(new KBinding("Text", filter, "From"));
			textRangeEditControl.ToTextBox.DataBindings.Add(new KBinding("Text", filter, "To"));
		}

		public override Type ExpectedFilterType()
		{
			return typeof(TextRangeField);
		}
	}
}
