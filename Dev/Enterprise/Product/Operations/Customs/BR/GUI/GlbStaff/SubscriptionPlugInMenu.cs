using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public class SubscriptionPlugInMenu : KMenuItem
	{
		public SubscriptionPlugInMenu(BRGlbStaffWrapper wrapper)
		{
			Text = Res.GetString("C5803C1E-8636-4F58-88DA-E68345C6CB4D", "Subscription");
			InitialiseMenu();
			Wrapper = wrapper;
		}

		internal BRGlbStaffWrapper Wrapper;
		internal MenuItem SendSubscription;

		void InitialiseMenu()
		{
			SendSubscription = new ZMenuItem(Res.GetString("1D0ECC76-6746-4B1A-AA86-936AC9D86DE3", "Send Subscription"), SendSubscription_Click);
			MenuItems.Add(SendSubscription);
		}

		ZForm Form => (ZForm)(GetMainMenu()?.GetForm());

		void SendSubscription_Click(object sender, EventArgs e)
		{
			if (CustomsPlugIn.FormPreSaved(Wrapper.Staff, Form) && ValidateCertificate(Wrapper.CCTPassword))
			{
				var messageSendingObjectParent = new SubscriptionMessageSendingObjectParent(Wrapper);
				using (var form = new SubscriptionMessageSendingForm(messageSendingObjectParent))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						var countOfMessages = messageSendingObjectParent.SendMessagesAndSave();
						Globals.Message.Show(Res.GetString("56acc64c-7aff-42eb-9f8d-c4ba032847e7", "{0} message(s) have been sent.", countOfMessages));
					}
				}
			}
		}

		bool ValidateCertificate(GlbExternalPassword_CCT password)
		{
			var isValid = true;
			if (password.GP_ExpiryDate < ZDateTime.Now)
			{
				isValid = false;
				Globals.Message.ShowError(Res.GetString("12788CB2-E9BA-4AB3-8A93-A118F89CD3DF", "The certificate has expired. Please create a new certificate."));
			}
			else if (password.GP_PasswordStatus != BRPasswordStatusList.Codes.Valid)
			{
				isValid = false;
				Globals.Message.ShowError(Res.GetString("F823A9BA-7030-470C-B992-45B08FC69D87", "Certificate not found or invalid."));
			}

			return isValid;
		}
	}
}
