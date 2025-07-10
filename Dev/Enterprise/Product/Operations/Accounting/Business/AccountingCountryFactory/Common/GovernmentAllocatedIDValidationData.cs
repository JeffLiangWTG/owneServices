using CargoWise.Common;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public record GovernmentAllocatedIDValidationData : IGovernmentAllocatedIDValidationData
	{
		public GovernmentAllocatedIDValidationData(InvoicingBase invoicingBase)
		{
			Argument.NotNull(invoicingBase, nameof(invoicingBase));

			var eInvoicingEligibilityLiteTransaction = (IEInvoicingEligibilityLiteTransaction)invoicingBase;
			var orgCountryCode = eInvoicingEligibilityLiteTransaction.InvoiceOrgAddressOverride.CountryCode;
			if (string.IsNullOrEmpty(orgCountryCode))
			{
				orgCountryCode = eInvoicingEligibilityLiteTransaction.OrgHeader.MainAddress.CountryCode;
			}

			EReportingStatus = invoicingBase.EInvoicingStatus;
			GovernmentAllocatedID = invoicingBase.AH_GovernmentAllocatedID;
			Ledger = invoicingBase.AH_Ledger;
			OrgCountryCode = orgCountryCode;
		}

		public string EReportingStatus { get; }

		public string GovernmentAllocatedID { get; }

		public string Ledger { get; }

		public string OrgCountryCode { get; }
	}
}
