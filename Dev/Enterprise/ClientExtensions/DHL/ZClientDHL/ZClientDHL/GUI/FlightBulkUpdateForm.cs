using System;
using System.Windows.Forms;
using Enterprise.Client.DHL.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.DHL.GUI
{
	public partial class FlightBulkUpdateForm : ZChildForm
	{
		public FlightBulkUpdateForm(FlightBulkUpdateBusinessObject flightBulkUpdateBizO)
			: base(flightBulkUpdateBizO)
		{
		}

		public override string FormCaption
		{
			get { return "Flight Bulk Update"; }
		}

		protected override DialogResult ShowErrorsDialogCore(bool includeIgnoreOption)
		{
			using (ZMessageBox msgBox = new ZErrorMessageBox(BusinessEntityForValidation, includeIgnoreOption))
			{
				msgBox.Text = "Unable to proceed with update...";
				msgBox.Message = "There are errors that need to be corrected before updating.";
				return ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
			}
		}

		void Update_Click(object sender, EventArgs e)
		{
			FlightBulkUpdateBizO.RunPreSaveValidation();
			if (FlightBulkUpdateBizO.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				if (FlightBulkUpdateBizO.HasMinimumRequirements)
				{
					var processor = NewFlightBulkUpdateProcessor(FlightBulkUpdateBizO);
					try
					{
						using (updateProgressForm = new ProgressForm())
						{
							updateProgressForm.ShowCancelButton = false;
							processor.OnProgress += new DHLProgressEventHandler(FlightBulkUpdateForm_OnProgress);
							updateProgressForm.ShowModalTo(this);

							int committedCount = processor.BulkUpdate();
							if (!string.IsNullOrEmpty(processor.ConcurrencyErrorMessage))
							{
								Globals.Message.ShowError(processor.ConcurrencyErrorMessage);
							}
							else
							{
								Globals.Message.Show(string.Format("Flight Bulk Update Complete.  Total records updated: {0}.", committedCount));
							}

							updateProgressForm.Status = "Updated declaration flight details.";
							updateProgressForm.PercentComplete = 100;
						}
					}
					finally
					{
						processor.OnProgress -= new DHLProgressEventHandler(FlightBulkUpdateForm_OnProgress);
					}
				}
				else
				{
					Globals.Message.Show("You must enter at least one new flight detail to be able to update.");
				}
			}
		}

		protected virtual FlightBulkUpdateProcessor NewFlightBulkUpdateProcessor(FlightBulkUpdateBusinessObject flightBulkUpdateBizO)
		{
			return new FlightBulkUpdateProcessor(FlightBulkUpdateBizO);
		}

		FlightBulkUpdateBusinessObject FlightBulkUpdateBizO
		{
			get { return (FlightBulkUpdateBusinessObject)BusinessEntity; }
		}

		void FlightBulkUpdateForm_OnProgress(object sender, DHLProgressEventArgs e)
		{
			updateProgressForm.Status = e.Message;
			updateProgressForm.PercentComplete = e.PercentComplete;
		}

		ProgressForm updateProgressForm;
	}
}
