namespace Enterprise.DocumentEngine.Visualisation
{
	using System;
	using System.Drawing;

	public interface IVisualizedReportView
	{
		event EventHandler FirstShown;

		Size ClientSize { get; set; }
		string Text { get; set; }

		bool ShouldBeReadOnly { get; set; }

		IVisualiserDrawer ControlDrawer { get; }
		IVisualiserDrawer BorderDrawer { get; }
	}
}
