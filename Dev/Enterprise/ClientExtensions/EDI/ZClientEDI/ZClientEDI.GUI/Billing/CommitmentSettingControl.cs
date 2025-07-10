using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class CommitmentSettingControl : ZUserControl
	{
		public CommitmentSettingControl()
		{
			InitializeComponent();
		}

		void InfoLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			var msg =
@"Enter/Select a name to share a commitment with other databases.
All databases with a commitment with the same name will aggregate
their discount units to reach the level of commitment.
The name can be any text, but beginning with the enterprise code is recommended.
To create a new shared commitment, simply enter a new name.
The new name will then become available in the drop list for other databases.
It is up to the user to ensure the number of units matches on other databases.

The filter 'STL Setting Commitment Group Name' can be used to find shared commitments.
The filter is in the Licence Database module and the Organization module.";
			Globals.Message.ShowInformation(msg);
		}
	}
}
