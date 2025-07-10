using System;
using Enterprise.Client.JAS.Business.Matching;
using Enterprise.ClientSharedComponents;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public partial class ExportPreMatchingDataForm : ZChildForm
	{
		public ExportPreMatchingDataForm(PreMatchedDataExporter exporter) : base(exporter)
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return "Export Data For Pre-Matching"; }
		}

		#region Event handlers

		internal void OKBoundButton_Click(object sender, EventArgs e)
		{
			Exporter.RunPreSaveValidation();
			if (Exporter.HasErrors)
			{
				Globals.Message.ShowError("There are errors that need to be fixed before Pre-Matching data can be exported");
			}
			else
			{
				if (Exporter.HasMinimumRequirements)
				{
					try
					{
						using (updateProgressForm = new ProgressForm())
						{
							updateProgressForm.ShowProgressBar = true;
							updateProgressForm.ShowCancelButton = true;
							updateProgressForm.Status = "Exporting Data for Pre-Matching";
							Exporter.OnProgress += new SharedProgressEventHandler(ExportPreMatchingDataForm_OnProgress);
							updateProgressForm.Cancelled += new EventHandler(fProgressForm_Cancelled);
							updateProgressForm.ShowModalTo(this);
							Exporter.Export();
							updateProgressForm.Status = "Export Data For Pre-Matching Complete.";
							updateProgressForm.PercentComplete = 100;
						}
					}
					finally
					{
						Exporter.OnProgress -= new SharedProgressEventHandler(ExportPreMatchingDataForm_OnProgress);
					}
				}
				else
				{
					Globals.Message.Show("No transaction to export.");
				}
			}
			Close();
		}

		internal void CancelBoundButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected virtual void fProgressForm_Cancelled(object sender, EventArgs e)
		{
			Exporter.CancelExport();
		}

		#endregion

		#region Implementation

		internal void ExportPreMatchingDataForm_OnProgress(object sender, SharedProgressEventArgs e)
		{
			updateProgressForm.Status = e.Message;
			updateProgressForm.PercentComplete = e.PercentComplete;
		}

		ProgressForm updateProgressForm;
		protected virtual PreMatchedDataExporter Exporter
		{
			get { return (PreMatchedDataExporter)DataSource; }
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.DumpDirectoryBrowser != null)
				{
					this.DumpDirectoryBrowser.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
