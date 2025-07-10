
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance.Argentina;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework;
using static System.FormattableString;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public interface IFEDetRequestBuilder
	{
		XStreamingElement BuildFEDetRequestInfo(TransactionInfo transaction, XNamespace fev1, BusinessObjectFactory factory, ZString? originalTransactionComplianceSubtype = null, ZString? originalReferenceRegNumber = null);
	}

	class FEDetRequestBuilder : IFEDetRequestBuilder
	{
		public FEDetRequestBuilder()
		{
			EInvoicingExtension = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetArgentinaEInvoicingExtension();
			EInvoicingDependecies = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetArgentinaEInvoicingDependencyFactory();
			ArgentinaEInvoiceHelper = EInvoicingDependecies.GetArgentinaEInvoiceHelper();
		}

		readonly IArgentinaEInvoicingExtension EInvoicingExtension;
		readonly IArgentinaEInvoicingDependencyFactory EInvoicingDependecies;
		readonly IArgentinaEInvoiceHelper ArgentinaEInvoiceHelper;

		public XStreamingElement BuildFEDetRequestInfo(TransactionInfo transaction, XNamespace fev1, BusinessObjectFactory factory, ZString? originalTransactionComplianceSubtype = null, ZString? originalReferenceRegNumber = null)
		{
			var isOriginalReferenceCreditOrDebitNoteTransaction = ArgentinaEInvoiceHelper.IsOriginalReferenceCreditOrDebitNoteTransaction(transaction);
			var monedaIdCtzInfo = EInvoicingExtension.GetCurrencyAndExchangeRateTransactionInfo(transaction);
			var tributosTotalAmount = transaction.OSTaxTransactionsAmount ?? ZDecimal.Zero;
			var condicionIvaReceptor = ArgentinaEInvoiceHelper.GetCondicionIvaReceptor(transaction);

			#region SuppressResourceStringsCheckRegion

			var feDetRequestElement = new XStreamingElement(fev1 + "FECAEDetRequest",
									new XElement(fev1 + "Concepto", 2),
									BuildRecipientRegistrationNumberInfo(transaction, fev1),
									new XElement(fev1 + "CbteDesde", ZString.Empty),
									new XElement(fev1 + "CbteHasta", ZString.Empty),
									new XElement(fev1 + "CbteFch", transaction.TransactionDate.FormatNullableDate("yyyyMMdd")),
									BuildInvoiceTotalsAmount(transaction, fev1),
									new XElement(fev1 + "FchServDesde", transaction.TransactionDate.FormatNullableDate("yyyyMMdd")),
									new XElement(fev1 + "FchServHasta", transaction.TransactionDate.FormatNullableDate("yyyyMMdd")),
									!ArgentinaEInvoiceHelper.IsMiPymeDebitOrCreditNoteComplianceSubType(transaction.ComplianceSubType.GetValueOrDefault()) ? new XElement(fev1 + "FchVtoPago", transaction.DueDate.FormatNullableDate("yyyyMMdd")) : null,
									new XElement(fev1 + "MonId", monedaIdCtzInfo.MonendaId),
									new XElement(fev1 + "MonCotiz", monedaIdCtzInfo.MonedaCtz),
									condicionIvaReceptor.HasValue ? new XElement(fev1 + "CondicionIVAReceptorId", condicionIvaReceptor) : null,
									isOriginalReferenceCreditOrDebitNoteTransaction ? CbtesAsocBuildXML(transaction, fev1, originalTransactionComplianceSubtype, originalReferenceRegNumber) : null,
									tributosTotalAmount != 0 ? TributosBuildXML(transaction, fev1) : null,
									AlicsIvaBuildXML(transaction, fev1),
									OpcionalesBuildXML(transaction, fev1, factory),
									CompradoresBuildXML(fev1)
									);

			return new XStreamingElement(fev1 + "FeDetReq", feDetRequestElement);

			#endregion
		}

		#region Implementations

		IEnumerable<XElement> BuildInvoiceTotalsAmount(TransactionInfo transaction, XNamespace fev1)
		{
			var xElementList = new List<XElement>();

			int multiplier = transaction.TransactionType == TransactionType.CRD ? -1 : 1;

			(ZDecimal amountImpNeto, ZDecimal amountTotConc, ZDecimal amountImpOpEx) = ArgentinaEInvoiceHelper.GetTotalsTaxAmountFromTransactionInfo(transaction, multiplier);

			xElementList.Add(new XElement(fev1 + "ImpTotal", ((ZDecimal)(transaction.OSTotal.GetValueOrDefault(ZDecimal.Zero) * multiplier)).FormatDecimals("F2")));
			xElementList.Add(new XElement(fev1 + "ImpTotConc", amountTotConc.FormatDecimals("F2")));
			xElementList.Add(new XElement(fev1 + "ImpNeto", amountImpNeto.FormatDecimals("F2")));
			xElementList.Add(new XElement(fev1 + "ImpOpEx", amountImpOpEx.FormatDecimals("F2")));
			xElementList.Add(new XElement(fev1 + "ImpTrib", ((ZDecimal)(transaction.OSTaxTransactionsAmount.GetValueOrDefault(ZDecimal.Zero) * multiplier)).FormatDecimals("F2")));
			xElementList.Add(new XElement(fev1 + "ImpIVA", ((ZDecimal)(transaction.OSGSTVATAmount.GetValueOrDefault(ZDecimal.Zero) * multiplier)).FormatDecimals("F2")));

			return xElementList;
		}

		IEnumerable<XElement> BuildRecipientRegistrationNumberInfo(TransactionInfo transaction, XNamespace fev1)
		{
			(ZString regType, ZString regValue) regNo = EInvoicingExtension.GetRegistrationNumberTransactionInfo(transaction);

			var buyerXElementList = new List<XElement>();
			buyerXElementList.Add(new XElement(fev1 + "DocTipo", regNo.regType));
			buyerXElementList.Add(new XElement(fev1 + "DocNro", regNo.regValue));

			return buyerXElementList;
		}

		XStreamingElement TributosBuildXML(TransactionInfo transaction, XNamespace fev1)
		{
			#region SuppressResourceStringsCheckRegion

			var tributosList = new List<XStreamingElement>();
			var multiplier = transaction.TransactionType.GetValueOrDefault() == TransactionType.CRD ? -1 : 1;
			var taxTransactionCollection = transaction.TaxTransactionCollection ?? new List<TaxTransaction>();

			foreach (var taxTransaction in taxTransactionCollection)
			{
				if (taxTransaction.TaxSuperType.Code.GetValueOrDefault() == TaxSuperTypeList.Perceptions.Code)
				{
					tributosList.Add(new XStreamingElement(fev1 + "Tributo",
											new XElement(fev1 + "Id", 7),
											new XElement(fev1 + "Desc", taxTransaction.TaxConfiguration.Description),
											new XElement(fev1 + "BaseImp", ((ZDecimal)(taxTransaction.OSTaxBase.GetValueOrDefault(ZDecimal.Zero) * multiplier)).FormatDecimals("F2")),
											new XElement(fev1 + "Alic", taxTransaction.TaxID.TaxRate.GetValueOrDefault()),
											new XElement(fev1 + "Importe", ((ZDecimal)(taxTransaction.OSTaxAmount.GetValueOrDefault(ZDecimal.Zero) * multiplier)).FormatDecimals("F2"))));
				}
				else
				{
					ErrorReporter.ReportOnce(Invariant($"Unsupported TaxSuperTypeList code {taxTransaction.TaxSuperType.Code.GetValueOrDefault()}"));
				}
			}

			return tributosList.Any() ? new XStreamingElement(fev1 + "Tributos", tributosList) : null;

			#endregion
		}

		XStreamingElement CompradoresBuildXML(XNamespace fev1)
		{
			List<XStreamingElement> compradoresList = new List<XStreamingElement>();

			#region SuppressResourceStringsCheckRegion

			// These nodes are optional for the time being we will be returning them empty, 
			//they will not be created until we know if they should be informed and their conditions to do it.
			var compradoresItem = new XStreamingElement(fev1 + "Comprador");
			compradoresItem.Add(new XElement(fev1 + "DocTipo", 0),
							new XElement(fev1 + "DocNro", 0),
							new XElement(fev1 + "Porcentaje", 0.0m));

			return (compradoresList.Any() ? new XStreamingElement(fev1 + "Compradores", compradoresList) : null);

			#endregion
		}

		XStreamingElement CbtesAsocBuildXML(TransactionInfo transaction, XNamespace fev1, ZString? originalReferenceComplianceSubtype, ZString? originalReferenceRegNumber)
		{
			List<XStreamingElement> cbtesAsocList = new List<XStreamingElement>();

			var complianceSubtype = EInvoicingExtension.GetDocumentType(originalReferenceComplianceSubtype.GetValueOrDefault(ZString.Empty));
			var originalTransactionCompliancePrefix = ArgentinaEInvoiceHelper.GetComplianceNumberPrefixFromTransaction(transaction.OriginalReference);
			var originalTransactionComplianceNumber = ArgentinaEInvoiceHelper.GetComplianceNumberFromTransaction(transaction.OriginalReference);

			#region SuppressResourceStringsCheckRegion

			var cbtesAsocItem = new XStreamingElement(fev1 + "CbteAsoc");

			cbtesAsocItem.Add(new XElement(fev1 + "Tipo", complianceSubtype),
							new XElement(fev1 + "PtoVta", originalTransactionCompliancePrefix),
							new XElement(fev1 + "Nro", originalTransactionComplianceNumber),
							new XElement(fev1 + "Cuit", originalReferenceRegNumber.GetValueOrDefault(ZString.Empty).RemoveNonNumericCharacters()),
							new XElement(fev1 + "CbteFch", transaction.OriginalReference.OriginalTransactionDate.FormatNullableDate("yyyyMMdd")));
			cbtesAsocList.Add(cbtesAsocItem);

			return (cbtesAsocList.Any() ? new XStreamingElement(fev1 + "CbtesAsoc", cbtesAsocList) : null);

			#endregion
		}

		XStreamingElement OpcionalesBuildXML(TransactionInfo transaction, XNamespace fev1, BusinessObjectFactory factory)
		{
			List<XStreamingElement> opcionalesList = new List<XStreamingElement>();
			var complianceSubtype = transaction.ComplianceSubType.GetValueOrDefault(ZString.Empty);

			if (ArgentinaEInvoiceHelper.IsMiPymeComplianceSubType(complianceSubtype))
			{
				if (ArgentinaEInvoiceHelper.IsMiPymeDebitOrCreditNoteComplianceSubType(complianceSubtype))
				{
					opcionalesList.Add(AddOpcionalNode(ArgentinaConstants.IdIsAnnulment, ArgentinaEInvoiceHelper.GetMiPymeOriginalTransactionIsRejectedByBuyer(transaction)));
				}
				else
				{
					opcionalesList.Add(AddOpcionalNode(ArgentinaConstants.IdCBU, ArgentinaEInvoiceHelper.GetBankAccountCBUNumber(transaction, factory)));
					opcionalesList.Add(AddOpcionalNode(ArgentinaConstants.IdTransferModality, ArgentinaConstants.MiPymeTransferModality));
				}
			}

			#region SuppressResourceStringsCheckRegion

			return opcionalesList.Any() ? new XStreamingElement(fev1 + "Opcionales", opcionalesList) : null;

			XStreamingElement AddOpcionalNode(string id, string valor)
			{
				return new XStreamingElement(fev1 + "Opcional", // SubNode description
							new XElement(fev1 + "Id", id),  // Node Name
							new XElement(fev1 + "Valor", valor)); // Node Name
			}

			#endregion
		}

		#region AlicsIva

		XStreamingElement AlicsIvaBuildXML(TransactionInfo transaction, XNamespace fev1)
		{
			var linesCollection = transaction.PostingJournalCollection ?? new List<PostingJournal>();
			int multiplier = transaction.TransactionType == TransactionType.CRD ? -1 : 1;
			var result = new List<XStreamingElement>();

			var taxSummaryLines = from PostingJournal line in linesCollection
								  where line.VATTaxID != null
										&& line.TaxMessageID != null
										&& line.TaxMessageID.TaxGroupCode != null
										&& ArgentinaConstants.GetTaxGroupsCodeList.Contains(line.TaxMessageID.
										TaxGroupCode.Code.GetValueOrDefault())
								  orderby line.TaxMessageID.TaxGroupCode.Code
								  group line by new { line.TaxMessageID.TaxGroupCode.Code } into taxRates
								  select new TaxSummary
								  (
									   taxRates.Key.Code,
									   taxRates.Sum(p => p.OSAmount.GetValueOrDefault(ZDecimal.Zero) * multiplier),
									   taxRates.Sum(p => p.OSGSTVATAmount.GetValueOrDefault(ZDecimal.Zero) * multiplier)
								   );

			#region SuppressResourceStringsCheckRegion

			foreach (var summaryLine in taxSummaryLines)
			{
				if (summaryLine.OSAmountSum.HasValue)
				{
					var alicIvaItem = new XStreamingElement(fev1 + "AlicIva",
										new XElement(fev1 + "Id", summaryLine.TaxGroupCode),
										new XElement(fev1 + "BaseImp", summaryLine.OSAmountSum.GetValueOrDefault().FormatDecimals("F2")),
										new XElement(fev1 + "Importe", summaryLine.OSGSTVATAmountSum.GetValueOrDefault().FormatDecimals("F2"))
									);

					result.Add(alicIvaItem);
				}
			}

			return result.Any() ? new XStreamingElement(fev1 + "Iva", result) : null;

			#endregion
		}

		class TaxSummary
		{
			internal TaxSummary(string taxGroupCode, ZDecimal? osAmountSum, ZDecimal? osGSTVATAmountSum)
			{
				TaxGroupCode = taxGroupCode;
				OSAmountSum = osAmountSum;
				OSGSTVATAmountSum = osGSTVATAmountSum;
			}

			public ZDecimal? OSAmountSum;
			public ZDecimal? OSGSTVATAmountSum;
			public string TaxGroupCode;
		}

		#endregion

		#endregion
	}
}
