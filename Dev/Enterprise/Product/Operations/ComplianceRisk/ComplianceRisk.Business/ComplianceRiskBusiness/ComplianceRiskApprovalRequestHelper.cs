using Enterprise.ComplianceRisk.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;

namespace Enterprise.ComplianceRisk.Business
{
	public static class ComplianceRiskApprovalRequestHelper
	{
		public static MultilingualString GetRestrictedDocumentErrorMessageFor_DeniedPartyOrComplianceWise(ISecurityLoginEventArgsForDocumentApproval docApprovalArgs)
		{
			return IsRestrictedDocumentMessageForComplianceWise(docApprovalArgs)
				? GetRestrictedDocumentErrorMessageForComplianceWise()
				: GetRestrictedDocumentErrorMessageForDeniedParty();
		}

		public static MultilingualString GetRestrictedDocumentWarningMessageFor_DeniedPartyOrComplianceWise(ISecurityLoginEventArgsForDocumentApproval docApprovalArgs)
		{
			return IsRestrictedDocumentMessageForComplianceWise(docApprovalArgs)
				? GetRestrictedDocumentWarningMessageForComplianceWise()
				: GetRestrictedDocumentWarningMessageForDeniedParty();
		}

		public static bool IsRestrictedDocumentMessageForComplianceWise(ISecurityLoginEventArgsForDocumentApproval docApprovalArgs)
		{
			if (docApprovalArgs.ParentBusinessObject is IBaseJobDeclaration parent && parent.JE_JS.IsValid)
			{
				return ComplianceRiskHelper.IsFreightEnabledComplianceWise;
			}

			return docApprovalArgs.ParentBusinessObject is IComplianceItemRiskStatusProvider complianceRiskProvider && complianceRiskProvider.IsEnabledComplianceWise;
		}

		static MultilingualString GetRestrictedDocumentErrorMessageForDeniedParty()
		{
			return ResString.GetMultilingualString("81625a44-c8b2-40b0-b25c-2d5a464af5dc", @"You cannot request approval because delivery of this document is restricted due to uncleared status in denied party screening.

Please enter a username and password to override this restriction instead.");
		}

		static MultilingualString GetRestrictedDocumentErrorMessageForComplianceWise()
		{
			return ResString.GetMultilingualString("a6648e27-fbfe-4519-8db7-76947173ad26", @"You cannot request approval because delivery of this document is restricted due to uncleared status in compliance risk.

Please enter a username and password to override this restriction instead.");
		}

		static MultilingualString GetRestrictedDocumentWarningMessageForDeniedParty()
		{
			return ResString.GetMultilingualString("0501adc2-8ae3-4961-b959-417ff33a7952", @"Warning – Submitting a Customs Declaration to Customs for an Organization which may be on the Denied Party Screening list could incur many severe penalties.
Your company has chosen to require all Organizations included on a Customs Declaration be fully screened before sending to Customs.");
		}

		static MultilingualString GetRestrictedDocumentWarningMessageForComplianceWise()
		{
			return ResString.GetMultilingualString("8114cf10-cd73-4e03-8754-828ba5d0b271", @"Warning - Submitting a Customs Declaration to Customs when the Job Compliance Status is not Clear could incur many severe penalties.
Your company has chosen to require the Job Compliance Status to be Clear before submitting messages to Customs.");
		}
	}
}
