using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class EdiContactSendEmailSetResetPassword : ContactSendEmailSetResetPassword
	{
		public override bool SendPasswordInstructions(OrgContact selectedContact, bool displayMsg = true, PasswordInstructionUrlType type = PasswordInstructionUrlType.Default)
		{
			var ediOrgContact = (EDIOrgContact)selectedContact;

			if (!ediOrgContact.CanSendResetPasswordEmail)
			{
				Globals.Message.Show(Res.GetString("e189023b-9ca1-49a9-85f2-a8d7a46526d3", "This contact is linked to a user account which has been deactivated by the user. Password reset cannot occur from this organization until the user account is reactivated."));
				return false;
			}

			IsClearingPasswordOverride = selectedContact.IsClearingPersonPasswordOverride;

			if (!ediOrgContact.RelationshipPromptRequired)
			{
				var success = base.SendPasswordInstructions(selectedContact, displayMsg);
				IsClearingPasswordOverride = false;
				return success;
			}

			if (ZFormModaliser.ShowDialogAndDispose(new EdiAccountVerificationWarning(ediOrgContact)) == DialogResult.Cancel)
			{
				return false;
			}

			var newFactory = new BusinessObjectFactory();
			var newFactoryHasChanges = false;

			if (selectedContact.Person?.HasPassword ?? false)
			{
				var person = newFactory.Load<GlbPerson>(selectedContact.OC_PER);
				person.RemovePasswordHash();
				selectedContact.IsClearingPersonPasswordOverride = newFactoryHasChanges = IsClearingPasswordOverride = true;
			}

			if (!selectedContact.OC_PasswordHash.IsEmpty)
			{
				var contact = newFactory.Load<OrgContact>(selectedContact.PK);
				contact.RemovePasswordAndHash();
				newFactoryHasChanges = IsClearingPasswordOverride = true;
			}

			foreach (var userAccount in ediOrgContact.AccountVerificationStatusCollection.UserAccounts)
			{
				var userAccountReloaded = newFactory.Load<EdiCustomerUserAccount>(userAccount.PK);
				userAccountReloaded.EUA_ContactRelationshipStatus = ZString.Empty;
				userAccountReloaded.EUA_IsContactRelationshipActive = true;
				newFactoryHasChanges = true;
			}

			if (newFactoryHasChanges)
			{
				newFactory.Save();
			}

			if (selectedContact.IsInDatabase)
			{
				selectedContact.Reload();
				ediOrgContact.AccountVerificationStatusCollection.Reload();
			}

			var emailSendSuccess = base.SendPasswordInstructions(selectedContact, displayMsg);
			IsClearingPasswordOverride = false;
			return emailSendSuccess;
		}
	}
}
