using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ArchiveManager
{
	class AccountingArchiveableBusinessObjectProvider : IArchiveableBusinessObjectProvider
	{
		#region IArchiveableBusinessObjectProvider Members

		public IArchiveableBusinessObject[] LoadArchiveableBusinessObjects(IArchiveItem item, BusinessObjectFactory factory)
		{
			TransactionHeader header = factory.Load<TransactionHeader>(item.PK);
			return new IArchiveableBusinessObject[] { new ArchiveAccTransactionHeader(header) };
		}

		public IEnumerable<string> TableNamesSupported
		{
			get { yield break; }
		}

		public IEnumerable<ReferenceKeyType> ReferenceKeyTypesSupported
		{
			get { yield break; }
		}

		#endregion
	}

	class ArchiveAccTransactionHeader : IArchiveableBusinessObject
	{
		public ArchiveAccTransactionHeader(TransactionHeader header)
		{
			this.header = header;
		}

		public static ReferenceKeyType TransactionNo
		{
			get { return new ReferenceKeyType("TRN", ResString.GetMultilingualString("708f3cf8-bd9d-4489-a404-634479edb42b", "Transaction #")); }
		}

		public static ReferenceKeyType ConsolidatedInvoiceRefNo
		{
			get { return new ReferenceKeyType("INV", ResString.GetMultilingualString("02fc0d3b-c721-4b16-8c38-98f0ac95da8d", "Job Invoice #")); }
		}

		readonly TransactionHeader header;

		#region IArchiveableBusinessObject Members

		public IEnumerable<ArchiveReferenceKey> AdditionalKeys
		{
			get
			{
				if (!string.IsNullOrEmpty(header.JobNumber))
				{
					yield return new ArchiveReferenceKey(ArchiveReferenceKey.CommonTypes.JobNo, header.JobNumber);
				}

				if (!string.IsNullOrEmpty(header.AH_ConsolidatedInvoiceRef))
				{
					yield return new ArchiveReferenceKey(ConsolidatedInvoiceRefNo, header.AH_ConsolidatedInvoiceRef);
				}

				yield break;
			}
		}

		public IEnumerable<ArchiveDocumentDescriptor> ArchiveDocuments
		{
			get
			{
				if (header.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					yield return new ArchiveDocumentDescriptor(JobInvoicingEDocsProviderSupporter.CostConfirmationDocument) { DocTypeToCheck = "INV" };

					if (header.AH_TransactionCategory == Constants.TransactionCategory.Codes.SelfBilling)
					{
						yield return new ArchiveDocumentDescriptor(JobInvoicingEDocsProviderSupporter.SelfBillingInvoice) { DocTypeToCheck = "INV" };
					}
				}
				else
				{
					yield return new ArchiveDocumentDescriptor(header.EnterpriseInvoiceMenuName) { DocTypeToCheck = "INV" };

					if (header.IsGovtTaxInvoice)
					{
						yield return new ArchiveDocumentDescriptor(header.GovernmentInvoiceMenuName) { DocTypeToCheck = "INV" };
					}
				}
			}
		}

		public BusinessObject ArchiveableBusinessObject
		{
			get { return header; }
		}

		public ArchiveReferenceKey NaturalKey
		{
			get { return new ArchiveReferenceKey(TransactionNo, header.AH_TransactionNum); }
		}

		public Guid BranchPK
		{
			get { return header.AH_GB.ToGuid(); }
		}

		#endregion
	}
}
