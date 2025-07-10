using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ComplianceRisk.Integration
{
	public interface IComplianceRiskApprovalRequestHandler : IDocumentApprovalRequestHandler
	{
		LoginDialogResult ShowCustomsConfirmationDialogForDPSRestrictedDocuments(ISecurityLoginEventArgsForDocumentApproval docApprovalArgs);
	}
}
