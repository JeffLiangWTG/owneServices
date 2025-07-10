using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	internal class MatchEPaymentRecipientsHelper
	{
		internal static void CreateBeneficiaryRequest(MatchEPaymentRecipients matchEPaymentRecipients)
		{
			var request = matchEPaymentRecipients.CreateNewBeneficiaryRequestInNewFactory();
			Globals.Message.ShowInformation(Res.GetString("396D3DE6-C07B-402E-BC40-8FAED6B160F5", "Recipients list requested from third party provider OFX. This may take several minutes to receive."));
			matchEPaymentRecipients.CurrentRequest = request;
			matchEPaymentRecipients.RefreshBindingIncludingChildren();
		}

		internal static bool RunPreSynchronizeCheck(MatchEPaymentRecipients matchEPaymentRecipients) => VerifyUserHasEPaymentAccountAndActiveToken(matchEPaymentRecipients)
																									&& VerifyStatusOfLatestCreatedRequest(matchEPaymentRecipients);

		static bool VerifyUserHasEPaymentAccountAndActiveToken(MatchEPaymentRecipients matchEPaymentRecipients)
		{
			(bool continueProcess, AccEPaymentStaffToken staffToken) = matchEPaymentRecipients.CheckUserHasEPaymentAccountAndToken();
			if (!continueProcess)
			{
				Globals.Message.ShowError(Res.GetString("0C127069-CEE1-45CC-853B-6050F20226A7", "In order to request OFX Recipient List, you must have a registered OFX account. If you already have an account, please ensure that you have configured an OFX E-Payment Account in the Bank Account Maintenance module and that your staff profile is registered and authorized on this Bank Account."));
			}
			else
			{
				if (staffToken.TK_Status != AccEPaymentStaffTokenLookups.StatusCodes.Authorised
					|| staffToken.TK_ExpiryUtc < ZDateTime.UtcNow)
				{
					continueProcess = false;
					if (Globals.Message.Show(Res.GetString("328B0607-7473-468B-BE36-8B23755C04DC", @"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?"),
										Res.GetString("550F9B33-C054-460C-A834-DF2121F862A0", "Confirmation"),
										MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						var authorizeURL = staffToken.PrepareOAuthURL();
						WebUrlLauncher.Launch(authorizeURL);
					}
				}
			}

			return continueProcess;
		}

		static bool VerifyStatusOfLatestCreatedRequest(MatchEPaymentRecipients matchEPaymentRecipients)
		{
			var continueProcess = true;
			var latestRequest = matchEPaymentRecipients.RetrieveLatestCreatedBeneficiaryRequest();
			if (latestRequest != null)
			{
				if (latestRequest.ABR_Status == EPaymentStatusCodes.BeneficiaryRequest.Queued)
				{
					Globals.Message.ShowError(Res.GetString("1B363DFE-AD68-4E8E-80F0-96939E130743", "A request has already been generated for the recipients list"));
					continueProcess = false;
				}
				else if (latestRequest.ABR_Status == EPaymentStatusCodes.BeneficiaryRequest.Requested)
				{
					if (Globals.Message.Show(Res.GetString("65EA5918-F31F-4BA5-A22E-F9D23F532F74", "A request has already been sent to OFX for a recipients list. This may take up to several minutes to receive. Are you sure you wish to generate another request?"),
										Res.GetString("550F9B33-C054-460C-A834-DF2121F862A0", "Confirmation"),
										MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
					{
						continueProcess = false;
					}
				}
			}
			return continueProcess;
		}
	}
}
