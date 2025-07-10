using System;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.ZArchitecture;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IDocumentApprovalRequestHandler
	{
		void Initialize(ISecurityLogin loginBisObject, Func<LoginDialogResult> showLoginDocumentLoginForm);
		bool IsRestrictedForSingleStep(ISecurityLoginEventArgsForDocumentApproval args);
		bool IsRestrictedForMultiStep(ISecurityLoginEventArgsForDocumentApproval args);
		void HandleApprovalRequestForSingleStep(ISecurityLoginEventArgsForDocumentApproval args);
		void HandleApprovalRequestForMultiStep(ISecurityLoginEventArgsForDocumentApproval args);
	}
}
