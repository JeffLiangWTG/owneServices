namespace Enterprise.DocumentEngine.Visualisation
{
	using System;

	public interface IVisualizerView
	{
		event EventHandler Load;
		event EventHandler SaveButtonClicked;
		event EventHandler ResetButtonClicked;
		event EventHandler DiscardButtonClicked;
		event EventHandler ViewClosing;

		IVisualizedReportView GetNewReportView();
		void AddReportView(IVisualizedReportView view);
		bool ShowConfirmation(string message, string caption);
		void Close();
	}
}
