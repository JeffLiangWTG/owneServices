using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public class AccountingDocumentApprovalRequestHandler : IAccountingDocumentApprovalRequestHandler
	{
		public void Initialize(ISecurityLogin loginBisObject, Func<LoginDialogResult> showLoginDocumentLoginForm)
		{
			this.loginBisObject = loginBisObject;
			this.showLoginDocumentLoginForm = showLoginDocumentLoginForm;
		}

		ISecurityLogin loginBisObject;
		Func<LoginDialogResult> showLoginDocumentLoginForm;

		public void HandleApprovalRequestForMultiStep(ISecurityLoginEventArgsForDocumentApproval docApprovalArgs)
		{
			var approvalRequest = GetApprovalRequestConditionally(docApprovalArgs);

			if (approvalRequest != null)
			{
				if (docApprovalArgs.IsExternalAccountingSystemUsed)
				{
					approvalRequest.Factory.Save();
					approvalRequest.XP_RequestIDInfo.RefreshBinding();
					Globals.Message.Show(WrapMessageIfRequired(docApprovalArgs, string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
						Res.GetString("DC83A628-BB92-4ABE-A4C1-7160A2D9E786", "Credit Control Approval Request created. Reference No: '{0}'", approvalRequest.XP_RequestID),
						System.Environment.NewLine,
						Res.GetString("FE52ABFF-4603-4E25-BC87-578970B7658D", "The operation you want to perform requires this request to be approved before you can proceed."))));
				}
				else
				{
					ZFormModaliser.ShowDialogAndDispose(
					new CreditControlledDocumentsApprovalForm(
						new CreditControlledDocumentsApprovalBulk(approvalRequest.Factory, approvalRequest),
						CreditControlledDocumentsApprovalFormModes.SetDescription));
				}
			}
		}

		public bool IsRestrictedForSingleStep(ISecurityLoginEventArgsForDocumentApproval args)
		{
			return args.IsAccountingRestricted;
		}

		public bool IsRestrictedForMultiStep(ISecurityLoginEventArgsForDocumentApproval args)
		{
			return args.IsAccountingRestricted;
		}

		public void HandleApprovalRequestForSingleStep(ISecurityLoginEventArgsForDocumentApproval args)
		{
			var loginDialogResult = args.IsExternalAccountingSystemUsed ? LoginDialogResult.Ignore : showLoginDocumentLoginForm();

			if (loginDialogResult == LoginDialogResult.Yes)
			{
				args.IsAllowedToProceed = true;
				args.AuthorisingStaffLogin = loginBisObject.Login;
			}
			else if (loginDialogResult == LoginDialogResult.Ignore)
			{
				HandleApprovalRequestForMultiStep(args);
			}
		}

		static string WrapMessageIfRequired(ISecurityLoginEventArgsForDocumentApproval docApprovalArgs, string messageToBeWrapped)
		{
			if (docApprovalArgs.IsExternalAccountingSystemUsed)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", messageToBeWrapped, System.Environment.NewLine, Res.GetString("87AB58F3-08D4-4AEE-B55C-967043FA03C3", "Please repeat the same operation in a short while to check if the request is approved."));
			}

			return messageToBeWrapped;
		}

		static CreditControlledDocumentsApproval GetApprovalRequestConditionally(ISecurityLoginEventArgsForDocumentApproval docApprovalArgs)
		{
			var continueWithRequest = true;
			var approvalRequest = new BusinessObjectFactory().New<CreditControlledDocumentsApproval>();
			approvalRequest.Initialize(docApprovalArgs.ParentBusinessObject, docApprovalArgs.MenuItemPK, docApprovalArgs.AuthorizationLevel.ToArray(), docApprovalArgs.DefaultApprovalRequestReason);

			if (approvalRequest.RequestAlreadyMade)
			{
				var previousRejectedApprovalRequest = approvalRequest.GetLastRequestIfWasRejected();
				if (previousRejectedApprovalRequest != null)
				{
					var msg = Res.GetString("3FE8379C-FF9C-4312-AC8C-A784EE4BAC21", "A delivery request for this document has previously been rejected.") + "\r\n";
					if (!previousRejectedApprovalRequest.RejectionReason.IsEmpty)
					{
						msg += Res.GetString("B5CBE4D2-F3C0-4BF4-BE62-8F7FA95A432C", "Reason for rejection: '{0}'", previousRejectedApprovalRequest.RejectionReason) + "\r\n";
					}
					msg += Res.GetString("A9B9A19D-6094-45EB-918E-D911572CB1D6", "Do you want to re-submit the request?");
					var result = Globals.Message.Show(msg, Res.GetString("aea5e2d3-90ea-4af2-aed9-6977f99ef0a3", "Resubmit?"), MessageBoxButtons.YesNo, DialogResult.No);
					continueWithRequest = result == DialogResult.Yes;
				}
				else
				{
					Globals.Message.Show(WrapMessageIfRequired(docApprovalArgs, Res.GetString("ce9312b9-d0c9-4636-b47a-3d166f407e55", "A request has already been made to print this document for this job.")));
					continueWithRequest = false;
				}
			}

			if (continueWithRequest)
			{
				return approvalRequest;
			}
			else
			{
				return null;
			}
		}
	}
}
