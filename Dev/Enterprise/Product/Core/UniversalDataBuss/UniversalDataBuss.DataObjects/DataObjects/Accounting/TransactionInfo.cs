using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[RootElement("UniversalTransaction")]
	[XsdSchema("UniversalTransaction.xsd")]
	public partial class TransactionInfo : TopLevelDataObject , IMilestoneCollectionParent, IDisposable
	{
		public TransactionInfo()
		{
		}

		public TransactionInfo(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[NamespaceDependent(UniversalXmlInfo.Namespace_2011_11, typeof(Universal._2011_11.DataContext))]
		[NamespaceDependent(UniversalXmlInfo.Namespace_2012_11, typeof(Universal._2012_11.DataContext))]
		[ReferenceProperty]
		public override IDataContextDataObject DataContext { get; set; }

		public OrganizationAddress OrganizationAddress { get; set; }

		public OrganizationAddress BranchAddress { get; set; }

		[MaxLength(2)]
		public ZString? Ledger { get; set; }
		[MaxLength(3)]
		public TransactionType? TransactionType { get; set; }
		[MaxLength(38)]
		public ZString? Number { get; set; }
		[MaxLength(128)]
		public ZString? Description { get; set; }
		public ZDateTime? TransactionDate { get; set; }
		[MaxLength(3)]
		public ZString? Category { get; set; }
		public ZDateTime? DueDate { get; set; }

		public Currency LocalCurrency { get; set; }
		public ZDecimal? LocalExVATAmount { get; set; }
		public ZDecimal? LocalVATAmount { get; set; }
		public ZDecimal? LocalTotal { get; set; }
		public ZDecimal? LocalWHTAmount { get; set; }

		public Currency OSCurrency { get; set; }
		public ZDecimal? OSExGSTVATAmount { get; set; }
		public ZDecimal? OSGSTVATAmount { get; set; }
		public ZDecimal? OSTotal { get; set; }
		public ZDecimal? OSWHTAmount { get; set; }

		public ZDateTime? PostDate { get; set; }
		[MaxLength(38)]
		public ZString? CheckNumberOrPaymentRef { get; set; }
		[MaxLength(3)]
		public PaymentOrReceiptType? PaymentOrReceiptType { get; set; }
		[MaxLength(50)]
		public ZString? CheckDrawer { get; set; }
		[MaxLength(25)]
		public ZString? DrawerBank { get; set; }
		[MaxLength(25)]
		public ZString? DrawerBranch { get; set; }
		[MaxLength(38)]
		public ZString? JobInvoiceNumber { get; set; }
		public ZDateTime? FullyPaidDate { get; set; }
		[MaxLength(50)]
		public ZString? GovernmentAllocatedID { get; set; }
		public ZBool? IsPrinted { get; set; }
		public ZBool? IsCancelled { get; set; }
		public CodeDescriptionPair CancelReason { get; set; }
		[MaxLength(200)]
		public ZString? CancelReasonFreeText { get; set; }
		public ZDateTime? DateClearedInCashBook { get; set; }
		public ZDecimal? OutstandingAmount { get; set; }
		public ZDecimal? ExchangeRate { get; set; }
		[MaxLength(50)]
		public ZString? PlaceOfIssue { get; set; }
		[MaxLength(20)]
		public ZString? ReceiptOrDirectDebitNumber { get; set; }
		public ZBool? IsCreatedByMatchingProcess { get; set; }
		[MaxLength(3)]
		public InvoiceTermType? InvoiceTerm { get; set; }
		public ZInt? InvoiceTermDays { get; set; }

		[MaxLength(10)]
		public ZString? BankAccount { get; set; }
		public EntityReference Job { get; set; }
		public Branch Branch { get; set; }
		public Department Department { get; set; }
		public ZDateTime? RequisitionDate { get; set; }
		[MaxLength(3)]
		public ZString? RequisitionStatus { get; set; }
		[MaxLength(10)]
		public ZString? CheckBookCode { get; set; }
		[MaxLength(120)]
		public ZString? OrganizationsTransactionID { get; set; }
		public ZDateTime? CreateTime { get; set; }
		public StaffUsingAttributes CreateUser { get; set; }
		public ZDateTime? LastEditTime { get; set; }
		public StaffUsingAttributes LastEditUser { get; set; }
		public ZInt? NumberOfSupportingDocuments { get; set; }
		[VerticalPartition]
		public List<PostingJournal> PostingJournalCollection { get; private set; }
		public List<PostingRelatedJournal> PostingJournalApportionmentCollection { get; private set; }
		public List<PostingRelatedJournal> PostingJournalMultipleInstallmentCollection { get; private set; }
		public List<BankAccount> BankAccountCollection { get; private set; }
		public List<BankAccount> SellersBankAccountCollection { get; private set; }

		[MaxLength(25)]
		public ZString? ExternalDebtorCode { get; set; }
		[MaxLength(25)]
		public ZString? ExternalCreditorCode { get; set; }

		public List<Shipment> ShipmentCollection { get; private set; }

		[MaxLength(20)]
		public ZString? TransactionReference { get; set; }
		[MaxLength(3)]
		public ZString? ComplianceSubType { get; set; }
		[MaxLength(3)]
		public ZString? AgreedPaymentMethod { get; set; }

		public OriginalReference OriginalReference { get; set; }

		public ZDateTime? OrderCollectionDate { get; set; }

		public ZDateTime? DocumentReceivedDate { get; set; }

		public CodeDescriptionPair MatchStatus { get; set; }

		public CodeDescriptionPair MatchStatusReason { get; set; }

		public List<AttachedDocument> AttachedDocumentCollection { get; private set; }
		public List<DocumentTracking> OrganizationDocumentTrackingCollection { get; private set; }

		public CodeDescriptionPair APAccountGroup { get; set; }
		public CodeDescriptionPair ARAccountGroup { get; set; }

		public SettlementMethod SettlementMethod { get; set; }

		public List<InvoiceRemittance> InvoiceRemittanceCollection { get; private set; }

		[MaxLength(200)]
		public ZString? DigitalSignature { get; set; }

		public PlaceOfSupply PlaceOfSupply { get; set; }

		public Branch TaxBranch { get; set; }
		public OrganizationAddress TaxBranchAddress { get; set; }

		public List<Milestone> MilestoneCollection { get; private set; }
		public ZDecimal? LocalTaxTransactionsAmount { get; set; }
		public ZDecimal? OSTaxTransactionsAmount { get; set; }
		public List<TaxTransaction> TaxTransactionCollection { get; private set; }
		public List<MatchLine> MatchLineCollection { get; set; }

		public ZDecimal? TotalCashAdvanceAmount { get; set; }

		public List<AuthorizationDetails> AuthorizationDetailCollection { get; private set; }
		public List<TransactionHeaderReference> TransactionHeaderReferenceCollection { get; private set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (AttachedDocumentCollection != null)
				{
					foreach (var attachedDocument in AttachedDocumentCollection)
					{
						attachedDocument?.Dispose();
					}
				}

				if (AuthorizationDetailCollection != null)
				{
					foreach (var authDetails in AuthorizationDetailCollection)
					{
						authDetails?.Dispose();
					}
				}

				OriginalReference?.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
