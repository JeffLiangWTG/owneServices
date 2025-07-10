using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.ELG
{
	internal class SagARConverter : SagAccountsConverter
	{
		public SagARConverter(BusinessObjectFactory factory, NotificationBuffer notifications)
			: base(factory, notifications)
		{
		}

		protected override bool IsOkToProcess(Xsd.TxnHeader xmlHeader)
		{
			return (xmlHeader.Ledger == Xsd.TxnLedgerType.AR);
		}

		protected override ZString MapToNominalCode(ZString transportMode, ZString chargeCode)
		{
			return ELGDataRegistry.Instance.TransportAndChargeCodeCollectionItem.Value.FindNominalRevenueCode(transportMode, chargeCode);
		}

		protected override string NominalCodeType
		{
			get { return "Revenue"; }
		}

		protected override void BuildSpecificInvoiceHeaderRow(Xsd.TxnHeader xmlHeader, SagInvoiceHeaderDataRow headerRow)
		{
			SagARInvoiceHeaderDataRow result = (SagARInvoiceHeaderDataRow)headerRow;
			result.JobNumberReference = xmlHeader.TxnLines[0].ConsolOrJobNo;
			result.TransactionNumber = xmlHeader.TxnNumber;
		}

		#region Build Denormalised External Invoice
		protected override void AddSpecificInvoiceHeader(SagFlatFileDataRow invoiceHeader, ref SagFlatFileDataRow result)
		{
			SagARInvoiceHeaderDataRow arInvoiceHeader = (SagARInvoiceHeaderDataRow)invoiceHeader;
			SagARExternalInvoiceDataRow arResult = (SagARExternalInvoiceDataRow)result;
			arResult.AccountNumber = arInvoiceHeader.AccountNumber;
			arResult.DueDate = arInvoiceHeader.SettlementDueDate;
			arResult.GoodsValueInAccountCurrency = arInvoiceHeader.OsGoodsValue;
			arResult.SaleControlnValueInBaseCurrency = arInvoiceHeader.LocalControlValue;
			arResult.DocumentToBaseCurrencyRate = arInvoiceHeader.ExchangeCurrencyRate;
			arResult.DocumentToAccountCurrencyRate = arInvoiceHeader.ReciprocalExchangeCurrencyRate;
			arResult.TransactionReference = arInvoiceHeader.TransactionNumber;
			arResult.SecondReference = arInvoiceHeader.JobNumberReference;
			arResult.Source = arInvoiceHeader.LedgerSource;
			arResult.SYSTraderTranType = arInvoiceHeader.TransactionType;
			arResult.TransactionDate = arInvoiceHeader.InvoiceDate;
			arResult.TaxValue = arInvoiceHeader.LocalTaxValue;
		}

		protected override void AddSpecificInvoiceLineAggregateCollection(Dictionary<ZString, SagInvoiceLineAggregateDataRow> invoiceLineAggregateCollection,
			ref SagFlatFileDataRow result)
		{
			SagARExternalInvoiceDataRow arResult = (SagARExternalInvoiceDataRow)result;
			int fieldPos = SagARExternalInvoiceDataRow.Schema.NominalAnalysisTransactionValue_1.Name;
			int step = 15;
			foreach (SagInvoiceLineAggregateDataRow invoiceLineAggregate in invoiceLineAggregateCollection.Values)
			{
				arResult.SetField(fieldPos, invoiceLineAggregate.TransactionValue, 2);
				arResult.SetField(fieldPos + step, invoiceLineAggregate.AccountNumber);
				arResult.SetField(fieldPos + (step * 2), invoiceLineAggregate.CostCentre);
				arResult.SetField(fieldPos + (step * 3), invoiceLineAggregate.Department);
				arResult.SetField(fieldPos + (step * 4), invoiceLineAggregate.Narrative);
				arResult.SetField(fieldPos + (step * 5), invoiceLineAggregate.AnalysisCode);
				fieldPos++;
			}
		}

		protected override void AddSpecificInvoiceTaxAggregateCollection(Dictionary<ZString, SagInvoiceLineTaxAggregateDataRow> invoiceLineTaxAggregateCollection,
			ref SagFlatFileDataRow result)
		{
			SagARExternalInvoiceDataRow arResult = (SagARExternalInvoiceDataRow)result;
			int fieldPos = SagARExternalInvoiceDataRow.Schema.TaxAnalysisTaxRate_1.Name;
			foreach (SagInvoiceLineTaxAggregateDataRow invoiceLineTaxAggregate in invoiceLineTaxAggregateCollection.Values)
			{
				arResult.SetField(fieldPos++, invoiceLineTaxAggregate.TaxRateIndicator);
				arResult.SetField(fieldPos++, invoiceLineTaxAggregate.GoodsValue);
				arResult.SetField(fieldPos++, string.Empty /*invoiceLineTaxAggregate.DiscountValue*/);			// Currently not in use.
				arResult.SetField(fieldPos++, string.Empty /*invoiceLineTaxAggregate.DiscountPercentage*/);	// Currently not in use.
				arResult.SetField(fieldPos++, invoiceLineTaxAggregate.TaxValue);
			}
		}
		#endregion

		protected override SagFlatFileDataRow NewExternalInvoice
		{
			get { return new SagARExternalInvoiceDataRow(); }
		}

		protected override int LedgerSource
		{
			get { return 1; }
		}

		protected override SagInvoiceHeaderDataRow NewSagInvoiceHeaderDataRow
		{
			get { return new SagARInvoiceHeaderDataRow(); }
		}
	}
}
