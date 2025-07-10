using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Accounting.Export.Business
{
	public class TransactionLineRow
	{
		public ZInt? BatchSequence { get; set; }
		public ZInt? OriginalBatchNumber { get; set; }
		public ZInt? OriginalBatchSequence { get; set; }
		public Guid PK { get; set; }
		public Guid CompanyPK { get; set; }
		public Guid BranchPK { get; set; }
		public Guid DepartmentPK { get; set; }
		public Guid TransactionHeader { get; set; }
		public ZString? LineType { get; set; }
		public ZDateTime? PostDate { get; set; }
		public ZDateTime? ReverseDate { get; set; }
		public ZDateTime? JobRecognitionDate { get; set; }
		public ZDate? TaxDate { get; set; }
		public ZDecimal? LineAmount { get; set; }
		public ZInt? Sequence { get; set; }
		public ZString? Description { get; set; }
		public Currency LocalCurrency { get; set; }
		public int LocalCurrencySubUnitRatio { get; set; }
		public int LocalCurrencyDecimals => ExchangeRate.Decimals(LocalCurrencySubUnitRatio);
		public ZDecimal? GSTVAT { get; set; }
		public ZString? GSTVATBasis { get; set; }
		public ZDecimal? WHTTax { get; set; }
		public ZDecimal? OSWHTTax { get; set; }
		public Currency OSCurrency { get; set; }
		public ZDecimal? OSAmount { get; set; }
		public ZDecimal? ChargeExchangeRate { get; set; }
		public Currency ChargeCurrency { get; set; }
		public ZDecimal? ChargeAmount { get; set; }
		public ZDecimal? ChargeGST { get; set; }
		public TaxID ChargeTaxID { get; set; }
		public TaxMessageID ChargeTaxMessageID { get; set; }
		public TaxID ChargeWithholdingTaxID { get; set; }
		public TaxID TaxID { get; set; }
		public TaxMessageID TaxMessageID { get; set; }
		public TaxID WithholdingTaxID { get; set; }
		public ChargeCode ChargeCode { get; set; }
		public GLAccount GLAccount { get; set; }
		public EntityReference Job { get; set; }
		public EntityReference CostSource { get; set; }
		public Branch Branch { get; set; }
		public Department Department { get; set; }
		public OrganizationReference Organization { get; set; }
		public ZBool? IsFinalCharge { get; set; }
		public RevenueRecognitionType? RevRecognitionType { get; set; }
		public ZDecimal? OSExTaxAmount { get; set; }
		public ZDecimal? OSTaxAmount { get; set; }
		public ZString? ExternalCreditorCode { get; set; }
		public ZString? ExternalDebtorCode { get; set; }
		public ZBool IsCashBasisVATOnly { get; set; }
		public ZDecimal InputGSTVATRecoverable { get; set; }
		public ZDecimal? GSTVATRecoverable { get; set; }
		public ZDecimal? GSTVATNotRecoverable { get; set; }
		public SubAccount SubAccount { get; set; }
		public List<SubAccount> SubAccountCollection
		{
			get { return subAccountCollection ?? (subAccountCollection = new List<SubAccount>()); }
		}
		List<SubAccount> subAccountCollection;
		public ZDecimal? OSExtraVATAmount { get; set; }
		public ZDecimal? LocalExtraVATAmount { get; set; }
		public ZString? GovtChargeCode { get; set; }
		public List<RatingBasis> RatingBasisCollection { get; set; }
		public PlaceOfSupply PlaceOfSupply { get; set; }

		public CodeDescriptionPair SupplyType { get; set; }

		public Branch TaxBranch { get; set; }

		public List<CashBasisVATRow> CashBasisVATLines
		{
			get { return cashBasisVATLines ?? (cashBasisVATLines = new List<CashBasisVATRow>()); }
		}
		List<CashBasisVATRow> cashBasisVATLines;

		public ZDecimal? CashAdvanceAmount { get; set; }

		public override string ToString()
		{
			return string.Format((NoResString)"PK={0} TransactionHeader={1} LineType={2} PostDate={3} ReverseDate={4} LineAmount={5}",
						PK, TransactionHeader, LineType, PostDate, ReverseDate, LineAmount);
		}
	}
}
