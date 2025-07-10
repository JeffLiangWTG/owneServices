using System;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	internal static class EventContextTypeCode
	{
		// 🚩🚩🚩
		// All changes to XUE structure must be compatible with XUE wiki pages.
		// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13092/XUE-Reference
		// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13099/XUE-Message-Many-Transactions
		// If you need to extend the functionality of the XUE message, make changes on the wiki and ask Murray for a review!
		// 🚩🚩🚩

		[Obsolete("Please use AIB_GovernmentAllocatedNumber_Explicit (EINV_GovtAllocatedBatchRefNumber) or AH_GovernmentAllocatedID (EINV_GovtAllocatedTransactionRefNumber) to avoid ambiguity between batch and transaction level reference numbers.")]
		public const string Legacy_GovernmentAllocatedNumber = "EINV_GovtAllocatedRefNumber";  
		public const string AIB_GovernmentAllocatedNumber_Explicit = "EINV_GovtAllocatedBatchRefNumber";
		public const string AIB_EHubAllocatedNumber = "EINV_MiddlewareAllocatedNum";

		public const string AH_GovernmentAllocatedID = "EINV_GovtAllocatedTransactionRefNumber";
		public const string AH_TransactionReference = "EINV_ComplianceNumber";
		public const string AH_ComplianceDocumentDate = "EINV_ComplianceDate";
		public const string AH_ComplianceSubType = "EINV_ComplianceSubType";
		public const string AH1_ComplianceDocumentStatus = "EINV_ComplianceDocumentStatus";
		public const string AH1_CN_VoidedAndCreditedAmount = "EINV_CN_VoidedAndCreditedAmount";

		public const string AHF_AuthorisationData = "EINV_AuthorisationData";
		public const string AHF_Counter = "EINV_Counter";
		public const string AHF_DateTime = "EINV_DateTime";
		public const string AHF_IDNumber = "EINV_IDNumber";
		public const string AHF_IDType = "EINV_IDType";
		public const string AHF_ITransactionHash = "EINV_ITransactionHash";
		public const string AHF_Number = "EINV_Number";
		public const string AHF_PublicKey = "EINV_PublicKey";
		public const string AHF_VerificationUrl = "EINV_VerificationURL";

		public const string AIP_Status = "EINV_PivotStatus";

		/// <summary>
		/// Note. This key is used for both AR and AP.
		/// Please keep original name for compatibility purpose.
		/// </summary>
		public const string BatchResponseObject = "EINV_ARBatchResponseObject";
		public const string StatusCode = "EINV_StatusCode";

		public const string AHF_IssuerCertificateIdentifier = "EINV_IssuerCertificateIdentifier";
		public const string AHF_IssuerAuthorizationData = "EINV_IssuerAuthorisationData";
		public const string AHF_DebtorNumber = "EINV_DebtorNumber";
		public const string AHF_PlaceOfIssue = "EINV_PlaceOfIssue";

		// 🚩🚩🚩
		// All changes to XUE structure must be compatible with XUE wiki pages.
		// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13092/XUE-Reference
		// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13099/XUE-Message-Many-Transactions
		// If you need to extend the functionality of the XUE message, make changes on the wiki and ask Murray for a review!
		// 🚩🚩🚩
	}
}
