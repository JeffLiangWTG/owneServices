using CargoWise.Common;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public partial class ControlledSupportIncidentDialog : ZChildForm
	{
		public ControlledSupportIncidentDialog(IncidentManagementLink incidentLink, Result defaultAnswer = Result.None) : base()
		{
			Argument.NotNull(incidentLink, nameof(IncidentManagementLink));
			this.incidentLink = incidentLink;

			InitializeComponent();

			this.MinimizeBox = false;
			this.MaximizeBox = false;
			this.MainStatusBar?.Hide();
			this.contentsLabel.Text = LabelText;
			openAsViewModeButton.Select();
			DialogResult = defaultAnswer;
		}

		readonly IncidentManagementLink incidentLink;

		public override string FormCaption => ResString.GetMultilingualString("4c70a97e-b133-42f0-b592-c4514c8d9447", "Incident controlled by Incident Group");

		string LabelText
		{
			get
			{
				return ResString.GetMultilingualString("4b23f46b-6789-446f-965c-c0e20eb6c833",
					@"This incident is controlled by Incident Group:
{0} - {1}
It should not be edited whilst control is enabled.
To perform actions on this incident, including releasing control, please go to the parent Incident Group.", incidentLink.IncidentManagementGroup?.Number ?? string.Empty, incidentLink.IncidentManagementGroup?.ING_Description ?? string.Empty);
			}
		}

		protected ZButton openAsViewModeButton;
		protected ZButton openLinkedGroupButton;
		protected ZButton openAsEditModeButton;
		protected ZLabel contentsLabel;

		public new Result DialogResult { get; private set; }

		public new Result ShowDialog()
		{
			ZFormModaliser.ShowDialogAndDispose(this);
			return DialogResult;
		}

		void openAsEditModeButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = Result.Edit;
			this.Close();
		}

		void openLinkedGroupButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = Result.OpenGroup;
			this.Close();
		}

		void openAsViewModeButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = Result.View;
			this.Close();
		}

		public enum Result
		{
			View,
			Edit,
			OpenGroup,
			None
		}
	}
}
