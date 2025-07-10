using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.ELG
{
	internal class SagAPConverter : SagAccountsConverter
	{
		public SagAPConverter(BusinessObjectFactory factory, NotificationBuffer notifications)
			: base(factory, notifications)
		{
		}

		protected override bool IsOkToProcess(Xsd.TxnHeader xmlHeader)
		{
			return (xmlHeader.Ledger == Xsd.TxnLedgerType.AP);
		}

		protected override ZString MapToNominalCode(ZString transportMode, ZString chargeCode)
		{
			return ELGDataRegistry.Instance.TransportAndChargeCodeCollectionItem.Value.FindNominalCostCode(transportMode, chargeCode);
		}

		protected override string NominalCodeType
		{
			get { return "Cost"; }
		}

		protected override void BuildSpecificInvoiceHeaderRow(Xsd.TxnHeader xmlHeader, SagInvoiceHeaderDataRow headerRow)
		{
			SagAPInvoiceHeaderDataRow result = (SagAPInvoiceHeaderDataRow)headerRow;
			result.InvoiceDescription = xmlHeader.Description;
			result.TransactionNumber = xmlHeader.TxnNumber;
		}

		#region Build Denormalised External Invoice
		protected override void AddSpecificInvoiceHeader(SagFlatFileDataRow invoiceHeader, ref SagFlatFileDataRow result)
		{
			SagAPInvoiceHeaderDataRow apInvoiceHeader = (SagAPInvoiceHeaderDataRow)invoiceHeader;
			SagAPExternalInvoiceDataRow apResult = (SagAPExternalInvoiceDataRow)result;
			apResult.AccountNumber = apInvoiceHeader.AccountNumber;
			apResult.DueDate = apInvoiceHeader.SettlementDueDate;
			apResult.GoodsValueInAccountCurrency = apInvoiceHeader.OsGoodsValue;
			apResult.PerControlValueInBaseCurrency = apInvoiceHeader.LocalControlValue;
			apResult.DocumentToBaseCurrencyRate = apInvoiceHeader.ExchangeCurrencyRate;
			apResult.DocumentToAccountCurrencyRate = apInvoiceHeader.ReciprocalExchangeCurrencyRate;
			apResult.TransactionReference = apInvoiceHeader.InvoiceDescription;
			apResult.SecondReference = apInvoiceHeader.TransactionNumber;
			apResult.Source = apInvoiceHeader.LedgerSource;
			apResult.SYSTraderTranType = apInvoiceHeader.TransactionType;
			apResult.TransactionDate = apInvoiceHeader.InvoiceDate;
			apResult.TaxValue = apInvoiceHeader.LocalTaxValue;
		}

		protected override void AddSpecificInvoiceLineAggregateCollection(Dictionary<ZString, SagInvoiceLineAggregateDataRow> invoiceLineAggregateCollection,
			ref SagFlatFileDataRow result)
		{
			SagAPExternalInvoiceDataRow apResult = (SagAPExternalInvoiceDataRow)result;
			int fieldPos = SagAPExternalInvoiceDataRow.Schema.NominalAnalysisTransactionValue_1.Name;
			int step = 15;
			foreach (SagInvoiceLineAggregateDataRow invoiceLineAggregate in invoiceLineAggregateCollection.Values)
			{
				apResult.SetField(fieldPos, invoiceLineAggregate.TransactionValue, 2);
				apResult.SetField(fieldPos + step, invoiceLineAggregate.AccountNumber);
				apResult.SetField(fieldPos + (step * 2), invoiceLineAggregate.CostCentre);
				apResult.SetField(fieldPos + (step * 3), invoiceLineAggregate.Department);
				apResult.SetField(fieldPos + (step * 4), invoiceLineAggregate.Narrative);
				apResult.SetField(fieldPos + (step * 5), invoiceLineAggregate.AnalysisCode);
				fieldPos++;
			}
		}

		protected override void AddSpecificInvoiceTaxAggregateCollection(Dictionary<ZString, SagInvoiceLineTaxAggregateDataRow> invoiceLineTaxAggregateCollection,
			ref SagFlatFileDataRow result)
		{
			SagAPExternalInvoiceDataRow apResult = (SagAPExternalInvoiceDataRow)result;
			int fieldPos = SagAPExternalInvoiceDataRow.Schema.TaxAnalysisTaxRate_1.Name;
			foreach (SagInvoiceLineTaxAggregateDataRow invoiceLineTaxAggregate in invoiceLineTaxAggregateCollection.Values)
			{
				apResult.SetField(fieldPos++, invoiceLineTaxAggregate.TaxRateIndicator);
				apResult.SetField(fieldPos++, invoiceLineTaxAggregate.GoodsValue);
				apResult.SetField(fieldPos++, string.Empty /*invoiceLineTaxAggregate.DiscountValue*/);			// Currently not in use.
				apResult.SetField(fieldPos++, string.Empty /*invoiceLineTaxAggregate.DiscountPercentage*/);	// Currently not in use.
				apResult.SetField(fieldPos++, invoiceLineTaxAggregate.TaxValue);
			}
		}
		#endregion

		protected override SagFlatFileDataRow NewExternalInvoice
		{
			get { return new SagAPExternalInvoiceDataRow(); }
		}

		protected override int LedgerSource
		{
			get { return 2; }
		}

		protected override SagInvoiceHeaderDataRow NewSagInvoiceHeaderDataRow
		{
			get { return new SagAPInvoiceHeaderDataRow(); }
		}
	}
}
