using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZCurrentDateOffsetFilterControlBuilder : ZFilterStrip.CustomFilterControlsBuilder
	{
		public override bool Handles(ModuleFilter moduleFilter)
		{
			return moduleFilter is ModuleCurrentDateOffsetFilter;
		}

		public override Control[] GetFilterControls(ZFilterStrip parentStrip, ModuleFilter moduleFilter, ZBindingSource bindingSource)
		{
			var calcEditOffset = new ZCalcEdit();
			ControlDpiScalingHelper.SetTop(ref calcEditOffset, parentStrip.FilterControlTop, false);
			ControlDpiScalingHelper.SetWidth(ref calcEditOffset, 50, true);
			ControlDpiScalingHelper.SetLeft(ref calcEditOffset, parentStrip.FilterComparisonOperatorBoxStart, false);

			calcEditOffset.DecimalPlaces = 0;
			calcEditOffset.TabIndex = 1;
			calcEditOffset.ShowGroupSeparators = false;
			calcEditOffset.BindTo = "Offset";
			bindingSource.SetBindingMember(calcEditOffset, calcEditOffset.BindTo);

			var labelComparisonOperator = new ZLabel();
			labelComparisonOperator.Text = Res.GetString("79e9ff1c-9390-4142-b954-2b6e769c15bc", "Between");
			ControlDpiScalingHelper.SetTop(ref labelComparisonOperator, parentStrip.FilterControlTop, false);
			ControlDpiScalingHelper.SetWidth(ref labelComparisonOperator, 100, true);
			ControlDpiScalingHelper.SetLeft(ref labelComparisonOperator, parentStrip.FilterComparisonOperatorBoxStart + ControlDpiScalingHelper.ScaleToCurrentDpiX(55), false);

			var dateEditFromCompareDate = new ZFilterStripDateEdit();
			dateEditFromCompareDate.AllowDrop = true;
			dateEditFromCompareDate.Name = "dateEditFromCompareDate";
			dateEditFromCompareDate.ReadOnly = true;
			ControlDpiScalingHelper.SetTop(ref dateEditFromCompareDate, parentStrip.FilterControlTop, false);
			ControlDpiScalingHelper.SetWidth(ref dateEditFromCompareDate, 70, true);
			ControlDpiScalingHelper.SetLeft(ref dateEditFromCompareDate, labelComparisonOperator.Left + labelComparisonOperator.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
			dateEditFromCompareDate.DateTimeFormat = ZDateTimePickerFormat.Short;
			dateEditFromCompareDate.BindTo = "FromCompareDate";
			bindingSource.SetBindingMember(dateEditFromCompareDate, dateEditFromCompareDate.BindTo);

			var dateEditToCompareDate = new ZFilterStripDateEdit();
			dateEditToCompareDate.AllowDrop = true;
			dateEditToCompareDate.Name = "dateEditToCompareDate";
			dateEditToCompareDate.ReadOnly = true;
			ControlDpiScalingHelper.SetTop(ref dateEditToCompareDate, parentStrip.FilterControlTop, false);
			ControlDpiScalingHelper.SetWidth(ref dateEditToCompareDate, 70, true);
			ControlDpiScalingHelper.SetLeft(ref dateEditToCompareDate, dateEditFromCompareDate.Left + dateEditFromCompareDate.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
			dateEditToCompareDate.DateTimeFormat = ZDateTimePickerFormat.Short;
			dateEditToCompareDate.BindTo = "ToCompareDate";
			bindingSource.SetBindingMember(dateEditToCompareDate, dateEditToCompareDate.BindTo);

			return new Control[] { calcEditOffset, labelComparisonOperator, dateEditFromCompareDate, dateEditToCompareDate };
		}
	}
}

