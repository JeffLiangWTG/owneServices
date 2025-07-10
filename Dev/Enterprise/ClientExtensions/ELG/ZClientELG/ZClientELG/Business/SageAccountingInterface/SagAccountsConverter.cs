using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.ELG
{
	internal abstract class SagAccountsConverter : AccountsConverterARAP
	{
		public SagAccountsConverter(BusinessObjectFactory factory, NotificationBuffer notifications)
			: base(factory, notifications)
		{
		}

		protected override void CreateHeader(FlatFileDataRowCollection document)
		{
			if (!HasHeadingBeenApplied)
			{
				document.Add(new SagAPExternalInvoiceHeadingDataRow());
				HasHeadingBeenApplied = true;
			}
		}

		protected override ZBool fCheckThatAllTransactionsAreExported
		{
			get { return ZBool.True; }
		}

		protected override FlatFileDataRowCollection ExportAccounts(Xsd.TxnHeader xmlHeader)
		{
			FlatFileDataRowCollection result;
			SagFlatFileDataRow invoiceHeader = BuildInvoiceHeaderRow(xmlHeader);
			ZString invoiceLineAggregateKey;
			ZString invoiceLineTaxAggregateKey;
			Dictionary<ZString, SagInvoiceLineAggregateDataRow> invoiceLineAggregateCollection = new Dictionary<ZString, SagInvoiceLineAggregateDataRow>(2);
			Dictionary<ZString, SagInvoiceLineTaxAggregateDataRow> invoiceTaxAggregateCollection = new Dictionary<ZString, SagInvoiceLineTaxAggregateDataRow>(2);
			SagInvoiceLineAggregateDataRow invoiceLineAggregate;
			SagInvoiceLineTaxAggregateDataRow invoiceLineTaxAggregate;

			foreach (Xsd.TxnLine xmlLine in xmlHeader.TxnLines)
			{
				if (IsOkToProcess(xmlHeader, xmlLine))
				{
					invoiceLineAggregate = BuildInvoiceLineAggregate(xmlLine);
					invoiceLineAggregateKey = string.Format("{0}|{1}", invoiceLineAggregate.AccountNumber, invoiceLineAggregate.CostCentre);
					if (invoiceLineAggregateCollection.ContainsKey(invoiceLineAggregateKey))
					{
						invoiceLineAggregateCollection[invoiceLineAggregateKey] += invoiceLineAggregate;
					}
					else
					{
						invoiceLineAggregateCollection.Add(invoiceLineAggregateKey, invoiceLineAggregate);
					}

					invoiceLineTaxAggregate = BuildInvoiceLineTaxAggregate(xmlHeader, xmlLine);
					invoiceLineTaxAggregateKey = invoiceLineTaxAggregate.TaxRateIndicator;
					if (invoiceTaxAggregateCollection.ContainsKey(invoiceLineTaxAggregateKey))
					{
						invoiceTaxAggregateCollection[invoiceLineTaxAggregateKey] += invoiceLineTaxAggregate;
					}
					else
					{
						invoiceTaxAggregateCollection.Add(invoiceLineTaxAggregateKey, invoiceLineTaxAggregate);
					}
				}
			}

			result = BuildInvoice(invoiceHeader, invoiceLineAggregateCollection, invoiceTaxAggregateCollection);
			if (result.Count > 0)
			{
				TxnHeaderHasBeenProcessed(xmlHeader);
			}
			return result;
		}

		SagInvoiceHeaderDataRow BuildInvoiceHeaderRow(Xsd.TxnHeader xmlHeader)
		{
			txnReverseSign = ((xmlHeader.Ledger == Xsd.TxnLedgerType.AR) && (xmlHeader.TxnType == Xsd.TxnType.CRD)) ||
				((xmlHeader.Ledger == Xsd.TxnLedgerType.AP) && (xmlHeader.TxnType == Xsd.TxnType.INV));
			SagInvoiceHeaderDataRow result = NewSagInvoiceHeaderDataRow;
			result.AccountNumber = MapToSageAccountCode(xmlHeader.DebtorOrCreditor.EDICode, xmlHeader.OsInvoiceAmtExclTax.CurrencyCode, xmlHeader.Ledger);
			result.SettlementDueDate = xmlHeader.DueDate;
			result.OsGoodsValue = GetAmount(xmlHeader.OsInvoiceAmtInclTax.Value, txnReverseSign);
			result.LocalControlValue = GetAmount(xmlHeader.LocalInvoiceAmtInclTax.Value, txnReverseSign);
			result.ExchangeCurrencyRate = ExchangeRate.GetRate(xmlHeader.LocalInvoiceAmtExclTax.Value, xmlHeader.OsInvoiceAmtExclTax.Value);
			result.ReciprocalExchangeCurrencyRate = ReciprocalExchangeRate.GetRate(xmlHeader.LocalInvoiceAmtExclTax.Value, xmlHeader.OsInvoiceAmtExclTax.Value);
			result.LedgerSource = LedgerSource;
			result.TransactionType = GetTransactionType(xmlHeader.TxnType);
			result.InvoiceDate = xmlHeader.InvoiceDate;
			result.LocalTaxValue = GetAmount(xmlHeader.LocalTaxAmount.Value, txnReverseSign);
			BuildSpecificInvoiceHeaderRow(xmlHeader, result);
			return result;
		}

		SagInvoiceLineAggregateDataRow BuildInvoiceLineAggregate(Xsd.TxnLine xmlLine)
		{
			SagInvoiceLineAggregateDataRow result = new SagInvoiceLineAggregateDataRow();
			result.TransactionValue = GetAmount(xmlLine.OsInvoiceAmtExclTax.Value, txnReverseSign);
			result.AccountNumber = MapToNominalCode(xmlLine.ModeOfTransport.ToString(), xmlLine.ChargeCode);
			result.CostCentre = MapToProfitCentre(xmlLine.Branch, xmlLine.Department);
			result.Department = MapToNominalDepartment(xmlLine.Branch, xmlLine.Department);
			result.Narrative = Narrative;
			result.AnalysisCode = ZString.Empty;
			return result;
		}

		SagInvoiceLineTaxAggregateDataRow BuildInvoiceLineTaxAggregate(Xsd.TxnHeader xmlHeader, Xsd.TxnLine xmlLine)
		{
			SagInvoiceLineTaxAggregateDataRow result = new SagInvoiceLineTaxAggregateDataRow();
			switch (xmlLine.TaxCode.ToUpper())
			{
				case "EXEMPT":
					result.TaxRateIndicator = TaxIndicator.EXEMPT;
					break;

				case "FREEVAT":
					result.TaxRateIndicator = TaxIndicator.FREEVAT;
					break;

				case "CAPVAT":
					result.TaxRateIndicator = TaxIndicator.CAPVAT;
					break;

				case "VAT":
					result.TaxRateIndicator = TaxIndicator.VAT;
					break;

				case "LOWVAT":
					result.TaxRateIndicator = TaxIndicator.LOWVAT;
					break;

				case "MIDVAT":
					result.TaxRateIndicator = TaxIndicator.MIDVAT;
					break;

				case "LOWVATREV":
					result.TaxRateIndicator = TaxIndicator.LOWVATREV;
					break;

				case "MIDVATREV":
					result.TaxRateIndicator = TaxIndicator.MIDVATREV;
					break;

				case "FREEVATREV":
					result.TaxRateIndicator = GetFREEVATREV(xmlHeader);
					break;

				case "VATREV":
					result.TaxRateIndicator = GetVATREV(xmlHeader);
					break;

				default:
					result.TaxRateIndicator = TaxIndicator.EXEMPT;
					break;
			}

			result.TaxValue = GetAmount(xmlLine.OsTaxAmount.Value, txnReverseSign);

			result.GoodsValue = GetAmount(xmlLine.OsInvoiceAmtExclTax.Value, txnReverseSign);
			result.DiscountValue = ZDecimal.Zero;
			result.DiscountPercentage = ZDecimal.Zero;
			return result;
		}

		ZString GetVATREV(Xsd.TxnHeader xmlHeader)
		{
			ZString result = ZString.Empty;

			if (xmlHeader.Ledger == Xsd.TxnLedgerType.AR)
			{
				result = TaxIndicator.AR_VATREV;
			}
			else if (xmlHeader.Ledger == Xsd.TxnLedgerType.AP)
			{
				result = TaxIndicator.AP_VATREV;
			}

			return result;
		}

		ZString GetFREEVATREV(Xsd.TxnHeader xmlHeader)
		{
			ZString result = ZString.Empty;

			if (xmlHeader.Ledger == Xsd.TxnLedgerType.AR)
			{
				result = TaxIndicator.AR_FREEVATREV;
			}
			else if (xmlHeader.Ledger == Xsd.TxnLedgerType.AP)
			{
				result = TaxIndicator.AP_FREEVATREV;
			}

			return result;
		}

		protected ZDecimal GetAmount(ZDecimal amount, bool reverseSign)
		{
			ZDecimal result = amount;
			if (reverseSign)
			{
				result *= -1;
			}
			return result;
		}

		#region Build Denormalised External Invoice
		FlatFileDataRowCollection BuildInvoice(SagFlatFileDataRow invoiceHeader,
			Dictionary<ZString, SagInvoiceLineAggregateDataRow> invoiceLineAggregateCollection,
			Dictionary<ZString, SagInvoiceLineTaxAggregateDataRow> invoiceTaxAggregateCollection)
		{
			FlatFileDataRowCollection result = new FlatFileDataRowCollection();
			SagFlatFileDataRow externalInvoice = NewExternalInvoice;
			AddSpecificInvoiceHeader(invoiceHeader, ref externalInvoice);
			AddSpecificInvoiceLineAggregateCollection(invoiceLineAggregateCollection, ref externalInvoice);
			AddSpecificInvoiceTaxAggregateCollection(invoiceTaxAggregateCollection, ref externalInvoice);
			result.Add(externalInvoice);
			return result;
		}
		#endregion

		bool IsOkToProcess(Xsd.TxnHeader xmlHeader, Xsd.TxnLine xmlLine)
		{
			ZString message = ZString.Empty;
			if (!NominalCodeExists(xmlLine.ModeOfTransport.ToString(), xmlLine.ChargeCode))
			{
				message = ELGDataRegistry.GetMessage(ELGDataRegistry.transportAndChargeCodeMappingCaption,
					ZString.Format(ErrorNoNominalCodeFound, xmlLine.ModeOfTransport.ToString(), xmlLine.ChargeCode, NominalCodeType),
					ELGDataRegistry.sageInterfaceRegoLocation);
				notifications.Notify(new ErrorNotification(ErrorType.Error, message));
				throw new ELGException(ELGExceptionType.NoTransportModeAndChargeCodeMappingsSetOrFound);
			}

			if (!ProfitCentreExists(xmlLine.Branch, xmlLine.Department))
			{
				message = ELGDataRegistry.GetMessage(ELGDataRegistry.branchDepartmentCodeMappingErrorCategory,
					ZString.Format(ErrorNoProfitCentreFound, xmlLine.Branch, xmlLine.Department),
					ELGDataRegistry.sageInterfaceRegoLocation);
				notifications.Notify(new ErrorNotification(ErrorType.Error, message));
				throw new ELGException(ELGExceptionType.NoBranchDepartmentMappingsSetOrFound);
			}

			if (!NominalDepartmentExists(xmlLine.Branch, xmlLine.Department))
			{
				message = ELGDataRegistry.GetMessage(ELGDataRegistry.branchDepartmentCodeMappingErrorCategory,
					ZString.Format(ErrorNoNominalDepartmentFound, xmlLine.Branch, xmlLine.Department),
					ELGDataRegistry.sageInterfaceRegoLocation);
				notifications.Notify(new ErrorNotification(ErrorType.Error, message));
				throw new ELGException(ELGExceptionType.NoBranchDepartmentMappingsSetOrFound);
			}

			if (!SageAccountCodeExists(xmlHeader.DebtorOrCreditor.EDICode, xmlHeader.OsInvoiceAmtExclTax.CurrencyCode, xmlHeader.Ledger))
			{
				message = ELGDataRegistry.GetMessage(ELGDataRegistry.sageAccountCodeMappingCaption,
					ZString.Format(ErrorNoSageAccountCodeFound,
					xmlHeader.DebtorOrCreditor.EDICode, xmlHeader.OsInvoiceAmtExclTax.CurrencyCode, xmlHeader.Ledger.ToString()), ELGDataRegistry.sageInterfaceRegoLocation);
				notifications.Notify(new ErrorNotification(ErrorType.Error, message));
				throw new ELGException(ELGExceptionType.NoSageAccountCodeMappingsSetOrFound);
			}
			return message.IsEmpty;
		}

		static bool ProfitCentreExists(ZString branch, ZString department)
		{
			return !MapToProfitCentre(branch, department).IsEmpty;
		}

		static ZString MapToProfitCentre(ZString branch, ZString department)
		{
			return ELGDataRegistry.Instance.BranchDepartmentCodeCollectionItem.Value.FindProfitCentre(branch, department);
		}

		static bool NominalDepartmentExists(ZString branch, ZString department)
		{
			return !MapToNominalDepartment(branch, department).IsEmpty;
		}

		static ZString MapToNominalDepartment(ZString branch, ZString department)
		{
			return ELGDataRegistry.Instance.BranchDepartmentCodeCollectionItem.Value.FindNominalDepartmentCentre(branch, department);
		}

		protected bool NominalCodeExists(ZString transportMode, ZString chargeCode)
		{
			return !MapToNominalCode(transportMode, chargeCode).IsEmpty;
		}

		static bool SageAccountCodeExists(ZString ediCode, ZString currencyCode, Xsd.TxnLedgerType ledgerType)
		{
			return !MapToSageAccountCode(ediCode, currencyCode, ledgerType).IsEmpty;
		}

		static ZString MapToSageAccountCode(ZString ediCode, ZString currencyCode, Xsd.TxnLedgerType ledgerType)
		{
			return ELGDataRegistry.Instance.SageAccountCodeCollectionItem.Value.FindSageAccountCode(ediCode, currencyCode, ledgerType.ToString());
		}

		protected static ZInt GetTransactionType(Xsd.TxnType txnType)
		{
			if (txnType == Xsd.TxnType.INV)
			{
				return 4;
			}
			else
			{
				return 5;
			}
		}

		ExchangeRate ExchangeRate
		{
			get { return exchangeRate ?? (exchangeRate = new ExchangeRate(false, 6, Env.CurrentCompany.PK)); }
		}
		ExchangeRate exchangeRate;

		ExchangeRate ReciprocalExchangeRate
		{
			get { return reciprocalExchangeRate ?? (reciprocalExchangeRate = new ExchangeRate(true, 6, Env.CurrentCompany.PK)); }
		}
		ExchangeRate reciprocalExchangeRate;

		protected abstract ZString MapToNominalCode(ZString transportMode, ZString chargeCode);
		protected abstract string NominalCodeType { get; }
		protected abstract SagFlatFileDataRow NewExternalInvoice { get; }
		protected abstract void BuildSpecificInvoiceHeaderRow(Xsd.TxnHeader xmlHeader, SagInvoiceHeaderDataRow headerRow);
		protected abstract void AddSpecificInvoiceHeader(SagFlatFileDataRow invoiceHeader, ref SagFlatFileDataRow externalInvoice);
		protected abstract void AddSpecificInvoiceLineAggregateCollection(Dictionary<ZString, SagInvoiceLineAggregateDataRow> invoiceLineAggregateCollection, ref SagFlatFileDataRow result);
		protected abstract void AddSpecificInvoiceTaxAggregateCollection(Dictionary<ZString, SagInvoiceLineTaxAggregateDataRow> invoiceLineTaxAggregateCollection, ref SagFlatFileDataRow result);
		protected abstract int LedgerSource { get; }
		protected abstract SagInvoiceHeaderDataRow NewSagInvoiceHeaderDataRow { get; }

		bool txnReverseSign;
		internal bool HasHeadingBeenApplied;

		internal static class TaxIndicator
		{
			public const string EXEMPT = "0";
			public const string FREEVAT = "1";

			public const string CAPVAT = "2";
			public const string VAT = "2";
			public const string LOWVAT = "2";
			public const string MIDVAT = "2";

			public const string MIDVATREV = "4";
			public const string LOWVATREV = "4";

			public const string AR_FREEVATREV = "4";
			public const string AP_FREEVATREV = "7";

			public const string AP_VATREV = "8";
			public const string AR_VATREV = "9";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		const string Narrative = "Import from CargoWise One";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		const string ErrorNoNominalCodeFound = "No Nominal {2} Code found for CargoWise One Mode of Transport ({0}) and Charge Code ({1}).";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		const string ErrorNoProfitCentreFound = "No Profit Centre found for CargoWise One Branch ({0}) and Department ({1}).";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		const string ErrorNoNominalDepartmentFound = "No Nominal Department found for CargoWise One Branch ({0}) and Department ({1}).";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		const string ErrorNoSageAccountCodeFound = "No Sage Account Code found for CargoWise One Code ({0}), Currency ({1}) and Ledger Type ({2}).";
	}
}
