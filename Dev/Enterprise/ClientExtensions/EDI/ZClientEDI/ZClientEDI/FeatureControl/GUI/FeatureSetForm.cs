using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.FeatureControl.GUI
{
	public partial class FeatureSetForm : ZTemplateForm, ICanAttachWithoutSecurity
	{
		public FeatureSetForm(FeatureControlSet featureSet)
			: base(featureSet)
		{
			this.ControllerID = Modules.ClientControllerRegistration.FeatureSet;
		}

		protected override bool SupportsEDocs => false;
		override protected bool ShowAuditTab => true;
		protected override bool ShowNotesTab => false;

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				var featureSetUpdateConfirmationMessage = DatabaseGrid.GetFeatureSetUpdateConfirmationMessage();
				if (!string.IsNullOrEmpty(featureSetUpdateConfirmationMessage))
				{
					if (Globals.Message.Show(featureSetUpdateConfirmationMessage, "Confirm Database Feature Set Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) != DialogResult.Yes)
					{
						return ContinueWithSave.No;
					}
				}
			}
			return result;
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			base.Save(factories);
			DatabaseGrid.ClearFeatureSetUpdateConfirmationMessage();
		}

		public FeatureSetLicenceDatabaseModuleButtonGrid DatabaseGridForTest => this.DatabaseGrid;
	}
}
