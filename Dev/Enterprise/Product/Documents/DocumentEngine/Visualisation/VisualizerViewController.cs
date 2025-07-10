using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Visualisation
{
	using System;
	using System.Collections.Generic;

	public sealed class VisualizerViewController
	{
		public VisualizerViewController(
			IVisualizerManager visualizerManager,
			IVisualizerView view)
		{
			this.visualizerManager = visualizerManager;
			this.view = view;

			InitializeController();
		}

		readonly IVisualizerManager visualizerManager;
		readonly IVisualizerView view;

		void InitializeController()
		{
			view.Load += new EventHandler(HandleViewLoad);
			view.SaveButtonClicked += new EventHandler(HandleViewSaveButtonClicked);
			view.DiscardButtonClicked += new EventHandler(HandleViewDiscardButtonClicked);
			view.ResetButtonClicked += new EventHandler(HandleViewResetButtonClicked);
			view.ViewClosing += new EventHandler(HandleViewClosing);
		}

		void HandleViewLoad(object sender, EventArgs e)
		{
			foreach (var reportView in ReportViewList)
			{
				view.AddReportView(reportView);
			}
		}

		void HandleViewSaveButtonClicked(object sender, EventArgs e)
		{
			try
			{
				visualizerManager.SaveData();
				view.Close();
			}
			catch (ExceptionHandlingAbortedAfterErrorMessageException) { }
		}

		void HandleViewDiscardButtonClicked(object sender, EventArgs e)
		{
			try
			{
				view.Close();
			}
			catch (ExceptionHandlingAbortedAfterErrorMessageException) { }
		}

		void HandleViewResetButtonClicked(object sender, EventArgs e)
		{
			var message = Res.GetString("88dad9dc-8140-4e63-94f6-da6046239561", "You are about to clear the overriding data for this menu.");
			var caption = Res.GetString("449e3bfa-70e2-4a78-8667-5da0240e55d9", "Are you sure?");

			if (view.ShowConfirmation(message, caption))
			{
				try
				{
					visualizerManager.ClearData();
					view.Close();
				}
				catch (ExceptionHandlingAbortedAfterErrorMessageException) { }
			}
		}

		void HandleViewClosing(object sender, EventArgs e)
		{
			visualizerManager.RevertData();
		}

		public bool ShouldShowVisualizerView() => ReportViewList.Count > 0;

		List<IVisualizedReportView> reportViewList;
		List<IVisualizedReportView> ReportViewList
		{
			get
			{
				if (reportViewList == null)
				{
					reportViewList = new List<IVisualizedReportView>();
					foreach (var report in visualizerManager.Reports)
					{
						var controller = new VisualizedReportViewController(report, report.OverridingDataSet);
						if (controller.AllowVisualizeReportRegardlessOfWarnings())
						{
							var reportView = view.GetNewReportView();
							reportView.ShouldBeReadOnly = report.VisualizerContentNote == null;
							controller.View = reportView;
							reportViewList.Add(reportView);
						}
					}
				}
				return reportViewList;
			}
		}
	}
}
