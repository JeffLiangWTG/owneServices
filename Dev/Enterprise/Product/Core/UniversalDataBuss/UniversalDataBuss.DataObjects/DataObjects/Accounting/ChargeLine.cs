using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public partial class ChargeLine : IDataObject
	{
		public ChargeLine()
		{
		}

		public ChargeLine(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public ChargeCode ChargeCode { get; set; }
		[MaxLength(1024), AllowLineControlWhiteSpace]
		public ZString? Description { get; set; }
		public CodeDescriptionPair ChargeCodeGroup { get; set; }
		public Branch Branch { get; set; }
		public Department Department { get; set; }
		public Currency CostOSCurrency { get; set; }
		public ZDecimal? CostOSAmount { get; set; }
		public ZDecimal? CostLocalAmount { get; set; }
		public ZBool? CostIsPosted { get; set; }
		[MaxLength(38)]
		public ZString? CostAPInvoiceNumber { get; set; }
		public ZDateTime? CostInvoiceDate { get; set; }
		public ZDateTime? CostDueDate { get; set; }
		public TaxID CostGSTVATID { get; set; }
		public ZDecimal? CostOSGSTVATAmount { get; set; }
		public EntityReference CostApportionmentConsolNumber { get; set; }
		public Currency SellOSCurrency { get; set; }
		public ZDecimal? SellOSAmount { get; set; }
		public ZDecimal? SellLocalAmount { get; set; }
		public ZBool? SellIsPosted { get; set; }
		[MaxLength(3)]
		public ZString? SellPostedTransactionType { get; set; }
		[MaxLength(38)]
		public ZString? SellPostedTransactionNumber { get; set; }
		public TransactionInfo SellPostedTransaction { get; set; }
		[MaxLength(3)]
		public ZString? SellInvoiceType { get; set; }
		public TaxID SellGSTVATID { get; set; }
		public ZDecimal? SellOSGSTVATAmount { get; set; }

		public OrganizationReference Debtor { get; set; }

		public OrganizationReference Creditor { get; set; }

		[MaxLength(25)]
		public ZString? ExternalDebtorCode { get; set; }

		[MaxLength(25)]
		public ZString? ExternalCreditorCode { get; set; }

		public ZShort? DisplaySequence { get; set; }

		[MaxLength(38)]
		public ZString? SupplierReference { get; set; }

		[MaxLength(35)]
		public ZString? SellReference { get; set; }

		[MaxLength(23)]
		public ZString? GovernmentReportingSellChargeCode { get; set; }
		[MaxLength(23)]
		public ZString? GovernmentReportingCostChargeCode { get; set; }

		public ZDecimal? CostExchangeRate { get; set; }
		public ZDecimal? SellExchangeRate { get; set; }

		public CodeDescriptionPair CostRatingBehaviour { get; set; }

		public CodeDescriptionPair SellRatingBehaviour { get; set; }

		public ImportMetaData ImportMetaData { get; set; }

		public List<RatingBasis> CostRatingBasisCollection { get; private set; }

		public List<RatingBasis> SellRatingBasisCollection { get; private set; }

		public PlaceOfSupply CostPlaceOfSupply { get; set; }

		public PlaceOfSupply SellPlaceOfSupply { get; set; }

		public CodeDescriptionPair CostSupplyType { get; set; }

		public CodeDescriptionPair SellSupplyType { get; set; }

		public Branch CostTaxBranch { get; set; }

		public Branch SellTaxBranch { get; set; }

		public ZBool? ARCashAdvanceRequired { get; set; }
		public ZBool? APCashAdvanceRequired { get; set; }
		public CashAdvanceRequestLine ARCashAdvanceRequestLine { get; set; }
		public CashAdvanceRequestLine APCashAdvanceRequestLine { get; set; }
	}
}
