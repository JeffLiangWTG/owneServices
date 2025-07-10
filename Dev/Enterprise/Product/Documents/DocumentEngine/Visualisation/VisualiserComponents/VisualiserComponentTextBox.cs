using System;
using System.Drawing;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.Visualisation
{
	public class VisualiserComponentTextBox : VisualiserComponent
	{
		public VisualiserComponentTextBox(Point location, Size size, VisualiserDataSet visualiserDataSet, string bindToName, CellFormat cellFormat)
			: base(AdjustLocation(location), AdjustSize(size))
		{
			this.VisualiserDataSet = visualiserDataSet;
			this.BindToName = bindToName;
			this.CellFormat = cellFormat;
		}

		static Point AdjustLocation(Point location)
		{
			return location + ControlDpiScalingHelper.NewScaledSize(2, 2);
		}

		static Size AdjustSize(Size size)
		{
			return size - ControlDpiScalingHelper.NewScaledSize(2 + 3, 3);
		}

		public readonly VisualiserDataSet VisualiserDataSet;
		public readonly string BindToName;
		public readonly CellFormat CellFormat;

		void RenderedTextBox_Leave(object sender, EventArgs e)
		{
			VisualiserDataSet.MainTable.AcceptChanges();
		}

#if DEBUG
		public override string GetControlDescriptionForTesting()
		{
			return base.GetControlDescriptionForTesting()
					+ "TextBox Bound To: " + BindToName + "\r\n";
		}

		public override object GetRenderedControlValueForTesting()
		{
			return VisualiserDataSet.MainRow[BindToName];
		}

		public override bool IsModifiableForTesting
		{
			get { return true; }
		}

		public override void SimulateUserSettingValueForTesting(object value)
		{
			VisualiserDataSet.MainRow[BindToName] = value;
		}
#endif
	}
}
