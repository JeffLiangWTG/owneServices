using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Organisation;
using Enterprise.Customs.GB.CDS.Organisation;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Organisation
{
	public partial class HmrcQueryForm : ZChildForm
	{
		public HmrcQueryForm(NonPersistentOrgHeaderChecker checker)
			: base(checker)
		{
			InitializeComponent();
			this.checker = checker;
		}

		readonly NonPersistentOrgHeaderChecker checker;

		public override string FormHeading => Res.GetString("0256F211-70ED-4490-BD96-DE2EA3585F52", "Verify GB EORI, UK VAT number or XI NOP Waiver");

		void QueryButton_Click(object sender, System.EventArgs e)
		{
			if (!checker.IsValid)
			{
				Globals.Message.ShowError(Res.GetString("1D9D535D-6384-4CDD-B651-74F69DF37083", "Please select at least one number for verification!"));
				return;
			}
			if (checker.OrgHeader.HasChanges && (ShowSaveOrganisationDialog() != DialogResult.Yes || FireSaveButton() != ContinueWithSave.Yes))
			{
				return;
			}
			SendMessages();
		}

		DialogResult ShowSaveOrganisationDialog()
		{
			return Globals.Message.Show(Res.GetString("B4F148E1-82F8-4CF5-AF9A-3FC030B43EAC", "The Organization has not yet been saved. Do you want to save and proceed?"),
				Res.GetString("055AE1F4-0329-4809-8276-6F9E754E5223", "Save Organization"), MessageBoxButtons.YesNo, DialogResult.No);
		}

		void SendMessages()
		{
			var manager = new OrgHeaderCheckerSendingManager(checker);
			manager.Send();
			if (manager.SendingNotifications.Count > 0)
			{
				var firstError = manager.SendingNotifications[0].Message.Split(System.Environment.NewLine)[0];
				Globals.Message.ShowError(firstError, Res.GetString("2490A6EC-D061-4380-8E54-9923747EAF4A", "Failed to send message(s)"));
			}
			else
			{
				Globals.Message.Show(Res.GetString("0420D5B2-789C-41F2-8BB0-404A3D8DD07B", "Message(s) sent successfully!"));
				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
