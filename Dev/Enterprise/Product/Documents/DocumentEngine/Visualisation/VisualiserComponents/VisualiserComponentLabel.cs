using System.Drawing;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.Visualisation
{
	public class VisualiserComponentLabel : VisualiserComponent
	{
		public VisualiserComponentLabel(Point location, Size size, ValueGetter getCaption, CellFormat cellFormat)
			: base(AdjustLocation(location), AdjustSize(size))
		{
			this.getCaption = getCaption;
			CellFormat = cellFormat;
		}

		static Point AdjustLocation(Point location)
		{
			return location + ControlDpiScalingHelper.NewScaledSize(2, 2);
		}

		static Size AdjustSize(Size size)
		{
			return size - ControlDpiScalingHelper.NewScaledSize(3, 3);
		}

		readonly ValueGetter getCaption;

		public delegate string ValueGetter();

		public VisualiserComponentLabel(Point location, Size size, string caption, CellFormat cellFormat)
			: base(location, size)
		{
			Caption = caption;
			CellFormat = cellFormat;
		}

		public readonly CellFormat CellFormat;

		public string Caption
		{
			get;
			private set;
		}

		public void ReadCaption()
		{
			Caption = getCaption();
		}

#if DEBUG
		public override string GetControlDescriptionForTesting()
		{
			return base.GetControlDescriptionForTesting()
					+ "Label Captioned: " + Caption + "\r\n";
		}

		public override object GetRenderedControlValueForTesting()
		{
			return Caption;
		}
#endif
	}
}
