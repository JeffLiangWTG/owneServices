using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	public enum OffsetRangeMeasure
	{
		Hours,
		Days
	}

	public class ZOffsetRangeFilterControl : ZCompositeWebControl
	{
		public ZOffsetRangeFilterControl(OffsetRangeMeasure measure)
		{
			Measure = measure;
			BuildControl();
		}

		void BuildControl()
		{
			CssClass = "OffsetRangeFilter";
			var rangeFromLabel = new Label();
			rangeFromLabel.Text = Res.GetString("657abac8-8797-4526-a969-a9a208b4d5c3", "At least");
			var rangeFromControl = GetRangeControl(1);

			var rangeToLabel = new Label();
			rangeToLabel.Text = Res.GetString("dc5aa01e-471e-4362-a539-b30de2d23d6e", "At most");
			var rangeToControl = GetRangeControl(2);

			var rangeDirectionLabel = new Label();
			rangeDirectionLabel.Text = GetMeasureDescription();

			var rangeDirectionControl = new ZDropDownList();
			rangeDirectionControl.BindTo = "FilterOption";
			rangeDirectionControl.DisplayStyle = Core.OComboBoxDropDownStyle.CodeOnly;

			Controls.Add(rangeFromLabel);
			Controls.Add(rangeFromControl);
			Controls.Add(rangeToLabel);
			Controls.Add(rangeToControl);
			Controls.Add(rangeDirectionLabel);
			Controls.Add(rangeDirectionControl);
		}

		string GetMeasureDescription() => Measure == OffsetRangeMeasure.Days ? Res.GetString("613a7cc0-043a-4903-b253-18832f89633d", "Days in the") : Res.GetString("e2591877-22c6-4652-8662-1f8741d25387", "Hours in the");

		WebControl GetRangeControl(int index) => Measure == OffsetRangeMeasure.Days ? GetDaysRangeControl(index) : GetHoursRangeControl(index);

		WebControl GetDaysRangeControl(int index)
		{
			var control = new ZNumericTextBox();
			control.BindTo = $"PropertyDecimal{index}";
			control.Decimals = 2;
			control.MaxLength = 4;

			return control;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanTypeForDuration", Justification = "Baseline")]
		WebControl GetHoursRangeControl(int index)
		{
			var control = new ZTimeFilterControl();
			control.BindTo = $"Property{index}";

			return control;
		}

		OffsetRangeMeasure Measure { get; }
	}
}
