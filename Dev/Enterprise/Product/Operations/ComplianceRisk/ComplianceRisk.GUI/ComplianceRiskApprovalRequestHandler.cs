using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ComplianceRisk.GUI
{
	public class ComplianceRiskApprovalRequestHandler : IComplianceRiskApprovalRequestHandler
	{
		public void Initialize(ISecurityLogin loginBisObject, Func<LoginDialogResult> showLoginDocumentLoginForm)
		{
			this.loginBisObject = loginBisObject;
			this.showLoginDocumentLoginForm = showLoginDocumentLoginForm;
		}

		ISecurityLogin loginBisObject;
		Func<LoginDialogResult> showLoginDocumentLoginForm;

		public bool IsRestrictedForSingleStep(ISecurityLoginEventArgsForDocumentApproval args)
		{
			return args.IsDPSFreightMovementRestricted;
		}

		// It should be noted that if the CheckpointLookupKeys are modified here, the AddDpsOrComplianceSecurityCheckPoints in DocumentDeliveryCreditControlHelper also needs to be modified accordingly.
		CheckpointLookupKey[] CheckpointLookupKeys => new CheckpointLookupKey[]
		{
			Env.Security.ConsolidationsComplianceAllowOverrideFreightMovementRestrictions.LookupKey,
			Env.Security.ShipmentsComplianceAllowOverrideFreightMovementRestrictions.LookupKey,
			Env.Security.BookingsComplianceAllowOverrideFreightMovementRestrictions.LookupKey,
			Env.Security.CustomsComplianceAllowOverrideFreightMovementRestrictions.LookupKey,
			Env.Security.OrgDeniedPartyScreeningOverrideFreightMvmtRestr.LookupKey
		};

		public bool IsRestrictedForMultiStep(ISecurityLoginEventArgsForDocumentApproval args) => args.IsDPSFreightMovementRestricted
			&& loginBisObject.IsRestrictedByCheckpoint(Env.Security, CheckpointLookupKeys);

		public void HandleApprovalRequestForSingleStep(ISecurityLoginEventArgsForDocumentApproval args)
		{
			bool displayLoginDialogAgain;
			do
			{
				displayLoginDialogAgain = false;

				var loginDialogResult = showLoginDocumentLoginForm();

				if (args != null && loginDialogResult == LoginDialogResult.Yes)
				{
					args.IsAllowedToProceed = true;
					args.AuthorisingStaffLogin = loginBisObject.Login;
				}
				else if (loginDialogResult == LoginDialogResult.Ignore)
				{
					HandleApprovalRequestForMultiStep(args);
					displayLoginDialogAgain = true;
				}
			}
			while (displayLoginDialogAgain);
		}

		public void HandleApprovalRequestForMultiStep(ISecurityLoginEventArgsForDocumentApproval args)
		{
			Globals.Message.Show(ComplianceRiskApprovalRequestHelper.GetRestrictedDocumentErrorMessageFor_DeniedPartyOrComplianceWise(args));
		}

		public LoginDialogResult ShowCustomsConfirmationDialogForDPSRestrictedDocuments(ISecurityLoginEventArgsForDocumentApproval docApprovalArgs)
		{
			var partiesMessage = new ZStringBuilder(ComplianceRiskApprovalRequestHelper.GetRestrictedDocumentWarningMessageFor_DeniedPartyOrComplianceWise(docApprovalArgs));

			var screeningParties = ((ICreditControlledDocumentDelivery)docApprovalArgs.ParentBusinessObject).GetScreeningParties();
			var deniedParties = DeniedPartyScreenerAsync.GetDeniedParties(screeningParties);
			if (deniedParties.Any())
			{
				partiesMessage.AppendLine();
				partiesMessage.AppendLine();
				partiesMessage.AppendLine(Res.GetString("e0979c96-9ffc-47e6-ad96-c1d79aa46474", "Denied Parties:"));

				foreach (var party in deniedParties.Take(5))
				{
					partiesMessage.AppendLine(party.Summary);
				}
			}

			var dialogInfo = GetDialogRestrictedDocumentInfo(docApprovalArgs);

			return Globals.Message.ShowConfirmation(partiesMessage.ToString(),
				dialogInfo.Caption, Res.GetString("cb038436-428f-458c-98e4-adb03510966c", "If you are sure you want to override your company policy please type:"),
				dialogInfo.Confirmation, MessageBoxIcon.Warning,
				ConfirmationMessageLayout.LineBreakAfterConfirmationPrompt) == DialogResult.OK ? LoginDialogResult.Yes : LoginDialogResult.No;
		}

		static (string Caption, string Confirmation) GetDialogRestrictedDocumentInfo(ISecurityLoginEventArgsForDocumentApproval docApprovalArgs)
		{
			var caption = Res.GetString("36cdbc02-c6d1-4630-a78c-be7c0f9ad568", "Warning - Denied Party Screening");
			var confirmation = Res.GetString("260c6878-a712-4d78-b282-9a9114d41a10", "I UNDERSTAND THE CONSEQUENCES OF OVERRIDING THE DENIED PARTY MOVEMENT RESTRICTION");

			if (ComplianceRiskApprovalRequestHelper.IsRestrictedDocumentMessageForComplianceWise(docApprovalArgs))
			{
				caption = Res.GetString("8189b5c8-2fab-4a16-aa46-286e3b3d454c", "Warning - Compliance Risk");
				confirmation = Res.GetString("4bb4f7f7-3a01-495a-a86a-ddfe6e1e84c8", "I UNDERSTAND THE CONSEQUENCES OF OVERRIDING THE COMPLIANCE RISK MOVEMENT RESTRICTION");
			}

			return (caption, confirmation);
		}
	}
}
