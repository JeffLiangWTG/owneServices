using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(Placement.Outer)]
	public partial class PostingJournal : IDataObject
	{
		public PostingJournal()
		{
			PostingJournalDetailCollection = new List<PostingJournalDetail>();
		}

		public PostingJournal(IDataObjectWriterStrategy strategy)
			: this()
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(3)]
		public TransactionType? TransactionType { get; set; }
		public ZInt? Sequence { get; set; }
		public ZInt? BatchSequence { get; set; }
		public ZInt? OriginalBatchNumber { get; set; }
		public ZInt? OriginalBatchSequence { get; set; }

		[MaxLength(1024), AllowLineControlWhiteSpace]
		public ZString? Description { get; set; }

		public Currency LocalCurrency { get; set; }
		public ZDecimal? LocalAmount { get; set; }
		public ZDecimal? LocalGSTVATAmount { get; set; }
		public ZDecimal? LocalWHTAmount { get; set; }
		public ZDecimal? LocalTotalAmount { get; set; }
		public ZDecimal? LocalExtraVATAmount { get; set; }

		public Currency OSCurrency { get; set; }
		public ZDecimal? OSAmount { get; set; }
		public ZDecimal? OSGSTVATAmount { get; set; }
		public ZDecimal? OSWHTAmount { get; set; }
		public ZDecimal? OSTotalAmount { get; set; }
		public ZDecimal? OSExtraVATAmount { get; set; }
		public ZDecimal? ChargeExchangeRate { get; set; }
		public Currency ChargeCurrency { get; set; }
		public ZDecimal? ChargeTotalAmount { get; set; }
		public ZDecimal? ChargeTotalExVATAmount { get; set; }
		public ZDecimal? ChargeTotalVATAmount { get; set; }

		public TaxID VATTaxID { get; set; }
		public TaxMessageID TaxMessageID { get; set; }
		public TaxID WithholdingTaxID { get; set; }
		public ZDate? TaxDate { get; set; }

		public ZDateTime? GLPostDate { get; set; }
		public ZDateTime? JobRecognitionDate { get; set; }
		public ChargeCode ChargeCode { get; set; }
		public GLAccount GLAccount { get; set; }
		[MaxLength(20)]
		public ZString? LocalGLAccount { get; set; }
		public EntityReference Job { get; set; }
		public EntityReference CostSource { get; set; }

		public Branch Branch { get; set; }
		public Department Department { get; set; }
		public OrganizationReference Organization { get; set; }
		public ZBool? IsFinalCharge { get; set; }
		[MaxLength(3)]
		public RevenueRecognitionType? RevenueRecognitionType { get; set; }
		[MaxLength(3)]
		public ZString? TransactionCategory { get; set; }
		public List<PostingJournalDetail> PostingJournalDetailCollection { get; set; }
		public ZDecimal? RecoverableGSTVATPercentage { get; set; }
		public SubAccount SubAccount { get; set; }

		public List<SubAccount> SubAccountCollection { get; private set; }

		public CodeDescriptionPair GSTVATBasis { get; set; }
		[MaxLength(23)]
		public ZString? GovernmentReportingChargeCode { get; set; }
		public List<RatingBasis> RatingBasisCollection { get; private set; }
		public PlaceOfSupply PlaceOfSupply { get; set; }
		public List<TaxLink> TaxTransactionLinkCollection { get; private set; }

		public CodeDescriptionPair SupplyType { get; set; }
		public Branch TaxBranch { get; set; }

		public ZDecimal? CashAdvanceAmount { get; set; }

		public ImportMetaData ImportMetaData { get; set; }
	}
}
