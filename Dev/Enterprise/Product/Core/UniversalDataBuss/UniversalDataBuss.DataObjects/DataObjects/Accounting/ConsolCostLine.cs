using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(Placement.Inner)]
	public partial class ConsolCostLine : IDataObject
	{
		public ConsolCostLine()
		{
		}

		public ConsolCostLine(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public ChargeCode ChargeCode { get; set; }
		public CodeDescriptionPair ChargeCodeGroup { get; set; }
		public UniversalChargeCode UniversalChargeCode { get; set; }
		public Currency CostOSCurrency { get; set; }
		public ZDecimal? CostOSAmount { get; set; }
		public ZDecimal? CostLocalAmount { get; set; }
		public ZDecimal? CostOSGSTVATAmount { get; set; }
		public TaxID CostGSTVATID { get; set; }
		public ZDecimal? CostExchangeRate { get; set; }
		public ZBool? CostIsPosted { get; set; }
		[MaxLength(38)]
		public ZString? CostAPInvoiceNumber { get; set; }
		public ZDateTime? CostInvoiceDate { get; set; }
		public ZDateTime? CostDueDate { get; set; }
		public Staff CostOwner { get; set; }
		public OrganizationReference Creditor { get; set; }
		[MaxLength(25)]
		public ZString? ExternalCreditorCode { get; set; }
		[MaxLength(38)]
		public ZString? SupplierReference { get; set; }
		[MaxLength(3)]
		public ZString? ApportionmentMethod { get; set; }
		[MaxLength(3)]
		public ZString? PrepaidCollectFilter { get; set; }
		public ZBool? ApportionToSubShipments { get; set; }
		public ZBool? IncludeOnCollectInvoice { get; set; }
		public RatingBehaviour RatingBehaviour { get; set; }

		[MaxLength(23)]
		public ZString? GovernmentReportingSellChargeCode { get; set; }
		[MaxLength(23)]
		public ZString? GovernmentReportingCostChargeCode { get; set; }

		public ImportMetaData ImportMetaData { get; set; }
		public PlaceOfSupply PlaceOfSupply { get; set; }
		public List<RatingBasis> CostRatingBasisCollection { get; private set; }

		public CodeDescriptionPair SupplyType { get; set; }

		public Branch CostTaxBranch { get; set; }
	}
}
