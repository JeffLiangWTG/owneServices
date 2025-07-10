using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	/// <summary>
	/// This wrapper must not be used for supplementary details page of a periodic invoice document, as SecondaryLayoutWhenPrintedInPeriodicInvoice is empty
	/// </summary>
	[AllowPublicConstructor]
	public class DocPeriodicInvoiceLineWrapper : DocumentWrapper, IDocARInvoiceLine
	{
		public DocPeriodicInvoiceLineWrapper(IDocARInvoiceLine line, string layout)
		{
			Line = line;
			Layout = layout;
		}
		readonly IDocARInvoiceLine Line;
		readonly string Layout;

		public static DocPeriodicInvoiceLineWrapper New(InvoicingLineBase line, BusinessObjectFactory factoryToWrap)
		{
			DocPeriodicInvoiceLineWrapper result = null;
			if (line != null)
			{
				result = new DocPeriodicInvoiceLineWrapper(DocARInvoiceLine.New(line, factoryToWrap), "NON");
			}
			return result;
		}

		public ZString CreatingUserName => Line.CreatingUserName;

		public ZDateTime CreatedDate => Line.CreatedDate;

		public DocChargeCode ChargeCode => Line.ChargeCode;

		public DocGlobalChargeCodeCollection ARGlobalChargeCodes => Line.ARGlobalChargeCodes;

		public DocGLAccount GLAccount => Line.GLAccount;

		public DocGLAccount PercentOfGLAccount => Line.PercentOfGLAccount;

		public DocARInvoice Invoice => Line.Invoice;

		public DocTaxRate TaxRate => Line.TaxRate;

		public ZDecimal TaxRateAmount_Raw => Line.TaxRateAmount_Raw;

		public ZDecimal TaxExtraRateAmount => Line.TaxExtraRateAmount;

		public DocWithholdingTaxRate WithholdingTaxRate => Line.WithholdingTaxRate;

		public ZString LineDescription
		{
			get
			{
				var description = string.Empty;

				if (Layout == InvoiceTypeLayoutList.Codes.INV || Layout == InvoiceTypeLayoutList.Codes.CHG)
				{
					description = FormattableString.Invariant($"{GovtChargeCodeDesriptionWithHeading}{Line.LineDescription}"); // Segments are translated individually
				}
				else if (Layout == InvoiceTypeLayoutList.Codes.NON)
				{
					description = FormattableString.Invariant($"{GovtChargeCodeDesriptionWithHeading}{InvoiceLineDescriptionForPeriodicInvoice}");
				}
				return description;
			}
		}

		string GovtChargeCodeDesriptionWithHeading
		{
			get
			{
				if (govtChargeCodeDescription == null)
				{
					govtChargeCodeDescription = !string.IsNullOrEmpty(GovernmentReportingCode) && !string.IsNullOrEmpty(GovernmentReportingCodeHeading) ? FormattableString.Invariant($"[{GovernmentReportingCodeHeading}: {GovernmentReportingCode}] ") : string.Empty;
				}

				return govtChargeCodeDescription;
			}
		}
		string govtChargeCodeDescription;

		public ZString InvoiceLineDescriptionForPeriodicInvoice => Line.InvoiceLineDescriptionForPeriodicInvoice;

		public ZString LineDescriptionAndExchangeRate => Line.LineDescriptionAndExchangeRate;

		public ZDecimal ExchangeRate => Line.ExchangeRate;

		public DocBranch Branch => Line.Branch;

		public DocDepartment Department => Line.Department;

		public ZDecimal GSTVAT => Line.GSTVAT;

		public ZString OSTaxDisplay => Line.OSTaxDisplay;

		public ZDecimal OSExTaxAmount => Line.OSExTaxAmount;

		public ZDecimal OSTaxAmount => Line.OSTaxAmount;

		public ZDecimal OSSERAmount => Line.OSSERAmount;

		public ZString JobNumber => Line.JobNumber;

		public DocJobHeader JobHeader => Line.JobHeader;

		public ZString JobTypeForPeriodicInvoice => Line.JobTypeForPeriodicInvoice;

		public FreightWrapper OperationsJob => Line.OperationsJob;

		public ZDecimal LineAmount => Line.LineAmount;

		public ZDecimal ChargeExchangeRate => Line.ChargeExchangeRate;

		public ZDecimal ChargeOSAmount => Line.ChargeOSAmount;

		public ZDecimal ChargeOSAmountForCLC => Line.ChargeOSAmountForCLC;

		public ZString ChargeCurrency => Line.ChargeCurrency;

		public ZString LineType => Line.LineType;

		public DocOrganisation Organisation => Line.Organisation;

		public ZDecimal OSAmount => Line.OSAmount;

		public ZDecimal OSUnitPrice => Line.OSUnitPrice;

		public ZInt PercentageOfPeriod => Line.PercentageOfPeriod;

		public ZDateTime PostDate => Line.PostDate;

		public ZInt PostPeriod => Line.PostPeriod;

		public ZBool PostToGL => Line.PostToGL;

		public ZBool PreventInvoicePrintGrouping => Line.PreventInvoicePrintGrouping;

		public ZDateTime ReverseDate => Line.ReverseDate;

		public ZInt ReversePeriod => Line.ReversePeriod;

		public ZBool ReverseToGL => Line.ReverseToGL;

		public DocCurrency Currency => Line.Currency;

		public DocCurrency DisplayCurrency => Line.DisplayCurrency;

		public ZShort Sequence => Line.Sequence;

		public ZDecimal UnitPrice => Line.UnitPrice;

		public ZInt UnitQty => Line.UnitQty;

		public ZDecimal WithholdingTax => Line.WithholdingTax;

		public DocShipment Shipment => Line.Shipment;

		public ZString FKToShipment => Line.FKToShipment;

		public ZString ExchangeRateAndAmount => Line.ExchangeRateAndAmount;

		public ZString ChargeOSAmountAndCurrency => Line.ChargeOSAmountAndCurrency;

		public DocOrganisation HeaderOrganisation => Line.HeaderOrganisation;

		public DocCurrency HeaderCurrency => Line.HeaderCurrency;

		public ZDecimal LocalAmountAndTax => Line.LocalAmountAndTax;

		public DocJobInvoicingJobCharge Charge => Line.Charge;

		public ZString TaxRateDisplay => Line.TaxRateDisplay;

		public ZString OSAmountDisplay => Line.OSAmountDisplay;

		public ZString TaxAmountDisplay => Line.TaxAmountDisplay;

		public ZString TaxGroupCode => Line.TaxGroupCode;

		public ZString ExchangeRateDisplay => Line.ExchangeRateDisplay;

		public ZBool IsSubTotalLine => Line.IsSubTotalLine;

		public ZBool IsSpacerLine => Line.IsSpacerLine;

		public ZBool IsCommentLine => Line.IsCommentLine;

		public ZBool IsRollUpLine => Line.IsRollUpLine;

		public ZBool HasBeenSubTotalled => Line.HasBeenSubTotalled;

		public ZString ClientReference => Line.ClientReference;

		public ZString SubInvoiceRef => Line.SubInvoiceRef;

		public ZString ShippersReference => Line.ShippersReference;

		public ZBool IsLoadListJob => Line.IsLoadListJob;

		public ZBool IsCFSShipmentJob => Line.IsCFSShipmentJob;

		public ZBool IsCustomJob => Line.IsCustomJob;

		public ZBool IsNCTSJob => Line.IsNCTSJob;

		public ZBool IsLocalCartage => Line.IsLocalCartage;

		public DocLoadListConsol LoadList => Line.LoadList;

		public ZString OtherReference => Line.OtherReference;

		public DocBaseJobDeclaration Customs => Line.Customs;

		public DocCommonCartage LocalTransport => Line.LocalTransport;

		public ZDecimal OSAmountForTotal => Line.OSAmountForTotal;

		public ZDecimal OSExTaxAmountForTotal => Line.OSExTaxAmountForTotal;

		public ZDecimal LineAmountForTotal => Line.LineAmountForTotal;

		public ZDecimal GSTVATForTotal => Line.GSTVATForTotal;

		public ZDecimal LocalAmountAndTaxForTotal => Line.LocalAmountAndTaxForTotal;

		public ZString TaxAmountDisplayForTotal => Line.TaxAmountDisplayForTotal;

		public ZString OSTaxDisplayForTotal => Line.OSTaxDisplayForTotal;

		public ZString TaxRateAsterisks => Line.TaxRateAsterisks;

		public ZString TaxRateAsterisksAsNumbers => Line.TaxRateAsterisksAsNumbers;

		public ZString OSTaxDisplayNoAsterisks => Line.OSTaxDisplayNoAsterisks;

		public ZString OSTaxDisplayNoAsterisksWithRegistryRule => Line.OSTaxDisplayNoAsterisksWithRegistryRule;

		public ZDecimal OSGSTAmount => Line.OSGSTAmount;

		public ZDecimal OSQSTAmount => Line.OSQSTAmount;

		public ZDecimal OSSBCAmount => Line.OSSBCAmount;

		public ZDecimal OSKKCAmount => Line.OSKKCAmount;

		public ZBool IsExtraTaxSBCAndKKC => Line.IsExtraTaxSBCAndKKC;

		public ZBool IsExtraTaxSBCOrKKC => Line.IsExtraTaxSBCOrKKC;

		public ZDecimal OSEDUAmount => Line.OSEDUAmount;

		public ZDecimal OSRETAmount => Line.OSRETAmount;

		public ZDecimal OSSPVAmount => Line.OSSPVAmount;

		public ZBool IncludeTaxAmountInOsTaxDisplay => Line.IncludeTaxAmountInOsTaxDisplay;

		public ZBool ShowPercentInGSTDisplay => Line.ShowPercentInGSTDisplay;

		public ZString ContainerNumbers => Line.ContainerNumbers;

		public DocAmountByChargeCodeCollection AmountSplittedByChargeCode => Line.AmountSplittedByChargeCode;

		public ZString SellRecognition => Line.SellRecognition;

		public ZString CostRecognition => Line.CostRecognition;

		public ZString ProductName => Line.ProductName;

		public ZBool IsApproved => Line.IsApproved;

		public ZString SellAccount => Line.SellAccount;

		public ZString CostAccount => Line.CostAccount;

		public ZBool IsApportioned => Line.IsApportioned;

		public ZBool CostPosted => Line.CostPosted;

		public ZBool SellPosted => Line.SellPosted;

		public ZDecimal CFXJnl => Line.CFXJnl;

		public ZShort ChargeCodePrintSequence => Line.ChargeCodePrintSequence;

		public ZInt DisplaySequence => Line.DisplaySequence;

		public ZString EstimatedCostWithCurrency => Line.EstimatedCostWithCurrency;

		public ZDecimal EstimatedCost => Line.EstimatedCost;

		public ZString EstimatedRevenueWithCurrency => Line.EstimatedRevenueWithCurrency;

		public ZDecimal EstimatedRevenue => Line.EstimatedRevenue;

		public ZString LocalCostAmountWithCurrency => Line.LocalCostAmountWithCurrency;

		public ZDecimal LocalCostAmount => Line.LocalCostAmount;

		public ZString LocalSellAmountWithCurrency => Line.LocalSellAmountWithCurrency;

		public ZDecimal LocalSellAmount => Line.LocalSellAmount;

		public ZString CostOSAmountAndCurrency => Line.CostOSAmountAndCurrency;

		public ZDecimal CostOSAmount => Line.CostOSAmount;

		public ZDecimal CostExchangeRate => Line.CostExchangeRate;

		public ZString CostCurrency => Line.CostCurrency;

		public ZString LayoutWhenPrintedInPeriodicInvoice => Layout;

		public ZString SecondaryLayoutWhenPrintedInPeriodicInvoice => ZString.Empty;

		public ZDecimal OSIntegratedGSTAmount => Line.OSIntegratedGSTAmount;

		public ZDecimal OSCentreGSTAmount => Line.OSCentreGSTAmount;

		public ZDecimal OSStateGSTAmount => Line.OSStateGSTAmount;

		public ZString GovernmentReportingCode => Line.GovernmentReportingCode;

		public ZString GovernmentReportingCodeHeading => Line.GovernmentReportingCodeHeading;

		public ZString Quantity => Line.Quantity;

		public ZBool DisplayTaxGroupCode => Line.DisplayTaxGroupCode;

		public ZDateTime TaxDate => Line.TaxDate;
	}
}
