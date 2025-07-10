using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Messaging
{
	public interface IClassificationLine1
	{
		//Classification Line 1
		ZShort B3LineNumber { get; }
		ZShort SequenceNumber { get; }
		ZString RecordIdentifier { get; }
		ZInt B3SubHeaderNumber { get; }
		ZInt B3SubHeaderNumberForLVX { get; }
		ZString ClassificationNumber { get; }
		ZString ValueForDutyCode { get; }
		ZString TariffCode { get; }
		ZDecimal ValueForCurrency { get; }
		ZDecimal ValueForDuty { get; }
		ZDecimal ValueForTax { get; }
		ZString AuthorityNumber { get; }
		ZString TRSNumber { get; }
		ZString[] PartNumberDescriptions { get; }
		IEnumerable<IInvoiceCrossReference> InvoiceCrossReferences { get; }
		ZDecimal CustomsQuantity { get; }
		ZDecimal CountOfInvoice { get; }
		ZString CustomsUnitQty { get; }
		ZDecimal InvoiceQuantity { get; }
		ZString InvoiceUQ { get; }
		Money TotalLinePrice { get; }
		Money CustomsValue { get; }
		Money FOB { get; }
		ZDecimal SalesTaxAmount { get; }
		ZDecimal CTAAmount { get; }
		(ZDecimal Amount, RefCurrency Currency) DeductionChargeAmountAndCurrency { get; }
		ZString CustomsDutyCode { get; }
		ZDecimal SurtaxAmount { get; }
		ZDecimal SurtaxQuantity { get; }
		ZString SurtaxUnitOfMeasure { get; }
		ZString SurtaxCode { get; }
		ZString SurtaxStatementCode { get; }
		ZBool SurtaxIsOverride { get; }
		ZBool HasSurtax { get; }
		ZDecimal ADDAmount { get; }
		ZDecimal ADDQuantity { get; }
		ZString ADDUnitOfMeasure { get; }
		ZString ADDCode { get; }
		ZBool ADDIsOverride { get; }
		ZBool HasADD { get; }
		ZDecimal CVDAmount { get; }
		ZDecimal CVDQuantity { get; }
		ZString CVDUnitOfMeasure { get; }
		ZString CVDCode { get; }
		ZBool CVDIsOverride { get; }
		ZBool HasCVD { get; }
		ZDecimal SafeguardAmount { get; }
		ZString SafeguardCode { get; }
		ZBool SafeguardIsOverride { get; }
		ZBool HasSafeguard { get; }
		ZString SafeguardStatementCode { get; }

		//Classification Line 3
		ZString SIMACode { get; }
		ZString SIMAStatementCode { get; }
		ZDecimal SIMAAssessment { get; }
		ZDecimal ExciseDutyAmount { get; }
		ZString ExciseExemptionCode { get; }
		ZString ExciseCode { get; }
		ZDecimal ExciseTaxRate { get; }
		ZDecimal ExciseTaxRateToPrint { get; }
		ZString ExciseTaxRateType { get; }
		ZDecimal ExciseTaxAmount { get; }
		bool IsDummyExciseTaxRate { get; }
		ZBool HasExcise { get; }
		ZString GSTExemptionCode { get; }
		ZString GSTCode { get; }
		ZDecimal RateOfGST { get; }
		ZString GSTRateType { get; }
		ZDecimal GSTAmount { get; }
		ZDecimal CUDAmount { get; }
		bool HasGSTDetails { get; }

		IEnumerable<IClassificationLine2> ClassificationLines { get; }

		//Low Value Shipments
		ZInt CountOfConsolidatedLines { get; }

		BusinessObjectFactory Factory { get; }

		// Invoice Line for CARM
		BusinessObject RelevantLine { get; }

		MessageSubTypes MessageSubType { get; }
	}

	public interface IClassificationLine2
	{
		ZInt B3LineNumber { get; }
		ZString UnitOfMeasureCode { get; }
		ZDecimal ClassificationLineQuantity { get; }
		ZDecimal WeightInKGM { get; }
		ZDecimal CustomsDutyRate { get; }
		ZString CustomsDutyRateType { get; }
		ZDecimal CustomsDutyAmount { get; }
		ZString PreviousTransactionNumber { get; }
		ZInt PreviousLineNumber { get; }
		ZString InvoiceUnitOfMeasureCode { get; }
		ZDecimal ClassificationLineInvoiceQuantity { get; }
	}
}
