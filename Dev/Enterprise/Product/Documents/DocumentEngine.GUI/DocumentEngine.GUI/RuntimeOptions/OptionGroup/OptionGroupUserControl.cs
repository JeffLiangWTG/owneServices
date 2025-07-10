using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class OptionGroupUserControl : RuntimeOptionUserControl
	{
		const int PixelsPerLineInCheckedListBox = 15;
		int DescriptionCodePairListCount;

		public OptionGroupUserControl()
		{
			InitializeComponent();
		}

		public override Type ExpectedFilterType()
		{
			return typeof(OptionGroup);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);

			FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(filter.DisplayNameLocalized);
			OptionGroup optionGroup = (OptionGroup)filter;
			DescriptionCodePairListCount = optionGroup.DescriptionCodePairList?.Count ?? 0;

			CheckedListBox.SingleCheckMode = optionGroup.IsRadioButton;
			CheckedListBox.SizeChanged += new EventHandler(CheckedListBox_SizeChanged);
			FieldLabel.SizeChanged += new EventHandler(FieldLabel_SizeChanged);
			ChangeCheckedListBoxHeightToFit();
			CheckedListBox.BindTo = "DescriptionCodePairList";
			SetDataBinding(filter, "");
		}

		void FieldLabel_SizeChanged(object sender, EventArgs e)
		{
			FieldLabel.Font = Font;
			FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption = $"{FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption}"; // Set Caption so that MeasureBestFitCaption can be actived
			ChangeCheckedListBoxHeightToFit();
		}

		void ChangeCheckedListBoxHeightToFit()
		{
			var textSize = TextRenderer.MeasureText(FieldLabel.GetExtension<ILabelCaptionRenderer>().Caption, FieldLabel.Font);
			var textNumberOfLines = (int)Math.Ceiling((float)(textSize.Width - 1.0f) / (FieldLabel.Width - FieldLabel.Padding.Horizontal));
			if (textNumberOfLines >= 1 && DescriptionCodePairListCount <= textNumberOfLines)
			{
				ControlDpiScalingHelper.SetHeight(ref CheckedListBox, (PixelsPerLineInCheckedListBox * textNumberOfLines) + 4, true);
			}
			else
			{
				ControlDpiScalingHelper.SetHeight(ref CheckedListBox, (DescriptionCodePairListCount > 10 ? 10 : DescriptionCodePairListCount) * PixelsPerLineInCheckedListBox + 4, true);
			}
		}

		void CheckedListBox_SizeChanged(object sender, EventArgs e)
		{
			ControlDpiScalingHelper.SetHeight(ref FieldLabel, CheckedListBox.Height, false);
			ControlDpiScalingHelper.SetHeight(this, FieldLabel.Top + FieldLabel.Height, false);
		}
	}
}
