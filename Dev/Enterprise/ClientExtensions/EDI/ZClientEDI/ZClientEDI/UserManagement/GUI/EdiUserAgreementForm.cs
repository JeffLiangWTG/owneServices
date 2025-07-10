using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.UserManagement.GUI
{
	public partial class EdiUserAgreementForm : ZTemplateForm
	{
		public EdiUserAgreementForm(EdiUserAgreement ediUserAgreement)
			: base(ediUserAgreement)
		{
			InitializeComponent();
		}

		protected override bool SupportsEDocs => true;
		protected override bool ShowNotesTab => true;

		public override string FormCaption => Res.GetString("fd8d9759-2861-49a7-b484-b49eb08b4b0a", "User Agreement");

		EdiUserAgreement Agreement => BusinessEntity as EdiUserAgreement;

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes && !Agreement.ERA_RN_NKCountryCode.IsEmpty && !Agreement.HasFallback)
			{
				var warningText = Res.GetString("708b6dee-70da-4944-8d79-19a7583cd7a9", "There is no Country/Region Code fallback for this Agreement Type. You can create a fallback by specifying an Agreement with an empty Country/Region Code. The fallback version will be used by the Web API when the requested country-specific Agreement does not exist.");
				var messageResult = Globals.Message.Show(warningText, Res.GetString("d6c928af-94ef-47ea-a8c7-d88238804655", "No Fallback Agreement"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				result = messageResult == DialogResult.OK ? ContinueWithSave.Yes : ContinueWithSave.No;
			}

			return result;
		}
	}
}
