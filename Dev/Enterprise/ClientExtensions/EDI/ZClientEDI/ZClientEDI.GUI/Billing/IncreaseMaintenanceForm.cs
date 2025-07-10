using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class IncreaseMaintenanceForm : ZChildForm
	{
		/// <summary>
		/// Simple system to remember the user's last values.
		/// ThreadStatic is not necessary here since the user won't have multiple forms and they won't be on separate threads even if they did.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
		static decimal LastPercentage = 5;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
		static bool LastIncreaseOld = true;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
		static bool LastIncreaseNew = false;

		public IncreaseMaintenanceForm()
		{
			InitializeComponent();

			Percentage = LastPercentage;
			IncreaseOld = LastIncreaseOld;
			IncreaseNew = LastIncreaseNew;
		}

		public decimal Percentage
		{
			get { return percentBox.Value; }
			set { percentBox.Value = value; }
		}

		public bool IncreaseOld
		{
			get { return oldCheckBox.Checked; }
			set { oldCheckBox.Checked = value; }
		}

		public bool IncreaseNew
		{
			get { return newCheckBox.Checked; }
			set { newCheckBox.Checked = value; }
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			LastPercentage = Percentage;
			LastIncreaseOld = IncreaseOld;
			LastIncreaseNew = IncreaseNew;
		}
	}
}

