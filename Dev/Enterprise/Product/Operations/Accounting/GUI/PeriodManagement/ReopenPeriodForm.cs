using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class ReopenPeriodForm : ZChildForm
	{
		public ReopenPeriodForm(AccPeriodManagement period)
			: base()
		{
			if (period != null)
			{
				this.period = period;
				Text = Res.GetString("17B03190-CD0E-4a3f-8B9A-41608AD2507C", "Reopen Period {0}", this.period.AM_Period.ToString());
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Implementation

		protected Business.PeriodManagement.NewYearPeriodSettings NewYearSettings;
		ZGroupBox ReSetPeriodsGroupBox;
		ZButton ReopenButton;
		ZCheckBox ReopenSubLedgerPeriodCheckBox;
		ZCheckBox ReopenForAdjustmentsCheckBox;
		ZCheckBox ReopenGeneralLedgerPeriodCheckBox;
		ZButton CloseButton;
		readonly System.ComponentModel.Container components;

		#region System stuff
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#endregion

		readonly AccPeriodManagement period;

		void ReopenButton_Click(object sender, EventArgs e)
		{
			var message = ZString.Empty;
			if (ReopenSubLedgerPeriodCheckBox.Checked && period.AM_IsSubLedgerClosed)
			{
				period.AM_IsSubLedgerClosed = false;
				message = Res.GetString("32de5539-112f-49a7-a5ea-ed84bfbb10a4", "Sub ledger,") + " ";
			}

			if (ReopenGeneralLedgerPeriodCheckBox.Checked && period.AM_IsGeneralLedgerClosed)
			{
				period.AM_IsGeneralLedgerClosed = false;
				message += Res.GetString("a71cfb4b-726b-4e49-a720-7117ee4bd285", "General ledger,") + " ";
			}

			if (ReopenForAdjustmentsCheckBox.Checked && period.AM_IsSubledgerClosedForAdjustments)
			{
				period.AM_IsSubledgerClosedForAdjustments = false;
				message += Res.GetString("ecec7e90-a4fd-44df-b719-39ca71fe1c9b", "Adjustments,") + " ";
			}
			if (!message.IsEmpty)
			{
				new PeriodReopenedEmail(period).Send();
				message = message.TrimEnd(", ".ToCharArray()) + '.';
				Globals.Message.Show(Res.GetString("ac4aa47a-2d25-45ed-baaf-561f4e810318", "Period {0} is Reopened for: {1}", period.AM_Period.ToString(), message));
				period.Factory.Save();
				this.Close();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("b31849e7-48b1-47fc-95b0-c81276e2084e", "Nothing to reopen!"));
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}

