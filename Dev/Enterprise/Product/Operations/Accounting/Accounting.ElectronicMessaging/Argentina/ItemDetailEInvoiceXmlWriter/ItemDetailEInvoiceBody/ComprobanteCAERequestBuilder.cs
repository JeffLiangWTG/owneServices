using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance.Argentina;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public interface IComprobanteCAERequestBuilder
	{
		XStreamingElement BuildXML(TransactionInfo transaction, BusinessObjectFactory factory);
	}

	class ComprobanteCAERequestBuilder : IComprobanteCAERequestBuilder
	{
		public ComprobanteCAERequestBuilder()
		{
			EInvoicingDependencies = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetArgentinaEInvoicingDependencyFactory();
			EInvoicingExtension = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetArgentinaEInvoicingExtension();
			Helper = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetTransactionInfoHelper();
			ArgentinaEInvoiceHelper = EInvoicingDependencies.GetArgentinaEInvoiceHelper();
		}

		readonly IArgentinaEInvoicingDependencyFactory EInvoicingDependencies;
		readonly IArgentinaEInvoicingExtension EInvoicingExtension;
		readonly ITransactionInfoHelper Helper;
		readonly IArgentinaEInvoiceHelper ArgentinaEInvoiceHelper;

		XStreamingElement IComprobanteCAERequestBuilder.BuildXML(TransactionInfo transactionInfo, BusinessObjectFactory factory)
		{
			(ZString currencyCode, ZString currencyExchangeRate) = EInvoicingExtension.GetCurrencyAndExchangeRateTransactionInfo(transactionInfo);
			(ZString codeDocumentType, ZString documentNumber) = EInvoicingExtension.GetRegistrationNumberTransactionInfo(transactionInfo);
			var condicionIvaReceptor = ArgentinaEInvoiceHelper.GetCondicionIvaReceptor(transactionInfo);

			#region SuppressResourceStringsCheckRegion

			var tributosTotalAmount = transactionInfo.OSTaxTransactionsAmount ?? ZDecimal.Zero;
			var isOriginalReferenceCreditOrDebitNoteTransaction = ArgentinaEInvoiceHelper.IsOriginalReferenceCreditOrDebitNoteTransaction(transactionInfo);

			(ZGuid branchPK, ZGuid companyPK, ZGuid departmentPK) = ArgentinaEInvoiceHelper.GetBranchCompanyAndDepartamentPKFromTransactionInfo(transactionInfo, factory);

			AccComplianceSequence transactionComplianceSequenceFromSubType = null;
			try
			{
				transactionComplianceSequenceFromSubType = EInvoicingDependencies.GetComplianceSequenceRetriever().GetComplianceSequenceFromSubType(transactionInfo.ComplianceSubType.GetValueOrDefault(), companyPK, branchPK, departmentPK, ZDateTime.Today);
			}
			catch (MultipleComplianceSequenceFoundException)
			{
				transactionComplianceSequenceFromSubType = null;
			}

			return new XStreamingElement("comprobanteCAERequest",
					new XElement("codigoTipoComprobante", EInvoicingExtension.GetDocumentType(transactionInfo.ComplianceSubType.GetValueOrDefault(ZString.Empty))),
					new XElement("numeroPuntoVenta", transactionComplianceSequenceFromSubType != null && !transactionComplianceSequenceFromSubType.XD_Prefix.IsEmpty ? transactionComplianceSequenceFromSubType.XD_Prefix : ZString.Empty),
					new XElement("numeroComprobante", ""),
					new XElement("fechaEmision", transactionInfo.TransactionDate.FormatNullableDate(ZDateTime.ISO8601ShortDateFormat)),
					new XElement("codigoTipoDocumento", codeDocumentType),
					new XElement("numeroDocumento", documentNumber),
					condicionIvaReceptor.HasValue ? new XElement("condicionIVAReceptor", condicionIvaReceptor) : null,
					BuildInvoiceTotalsAmount(transactionInfo),
					new XElement("codigoMoneda", currencyCode),
					new XElement("cotizacionMoneda", currencyExchangeRate),
					new XElement("codigoConcepto", 2),
					new XElement("fechaServicioDesde", transactionInfo.TransactionDate.FormatNullableDate(ZDateTime.ISO8601ShortDateFormat)),
					new XElement("fechaServicioHasta", transactionInfo.TransactionDate.FormatNullableDate(ZDateTime.ISO8601ShortDateFormat)),
					new XElement("fechaVencimientoPago", transactionInfo.DueDate.FormatNullableDate(ZDateTime.ISO8601ShortDateFormat)),
					isOriginalReferenceCreditOrDebitNoteTransaction ? ComprobantesAsociadosBuildXML(transactionInfo) : null,
					!tributosTotalAmount.IsEmpty ? OtrosTributosBuildXML(transactionInfo) : null,
					ItemsDetailBuildXML(transactionInfo),
					SubTotalesIvaBuildXML(transactionInfo),
					BuildDatosAdicionalesXML(transactionInfo, factory));

			#endregion
		}

		#region Implementations

		IEnumerable<XElement> BuildInvoiceTotalsAmount(TransactionInfo transaction)
		{
			var xElementList = new List<XElement>();
			var multiplier = transaction.TransactionType == TransactionType.CRD ? -1 : 1;
			var osTaxTransactionsAmount = transaction.OSTaxTransactionsAmount.GetValueOrDefault(ZDecimal.Zero);
			(ZDecimal importeGravado, ZDecimal importeNoGravado, ZDecimal importeExento) = ArgentinaEInvoiceHelper.GetTotalsTaxAmountFromTransactionInfo(transaction, multiplier);
			var importeSubtotal = (ZDecimal)(importeGravado + importeNoGravado + importeExento);

			xElementList.Add(new XElement("importeGravado", importeGravado.FormatDecimals(ArgentinaConstants.MoneyFormat)));
			xElementList.Add(new XElement("importeNoGravado", importeNoGravado.FormatDecimals(ArgentinaConstants.MoneyFormat)));
			xElementList.Add(new XElement("importeExento", importeExento.FormatDecimals(ArgentinaConstants.MoneyFormat)));
			xElementList.Add(new XElement("importeSubtotal", importeSubtotal.FormatDecimals(ArgentinaConstants.MoneyFormat)));
			xElementList.Add(!osTaxTransactionsAmount.IsEmpty ? new XElement("importeOtrosTributos", ((ZDecimal)(osTaxTransactionsAmount * multiplier)).FormatDecimals(ArgentinaConstants.MoneyFormat)) : null);
			xElementList.Add(new XElement("importeTotal", ((ZDecimal)(transaction.OSTotal.GetValueOrDefault(ZDecimal.Zero) * multiplier)).FormatDecimals(ArgentinaConstants.MoneyFormat)));

			return xElementList;
		}

		XStreamingElement OtrosTributosBuildXML(TransactionInfo transaction)
		{
			var tributosList = new List<XStreamingElement>();
			var multiplier = transaction.TransactionType.GetValueOrDefault() == TransactionType.CRD ? -1m : 1m;
			var taxTransactionCollection = transaction.TaxTransactionCollection ?? Enumerable.Empty<TaxTransaction>();

			foreach (var taxTransaction in taxTransactionCollection)
			{
				if (taxTransaction.TaxSuperType.Code.GetValueOrDefault() == TaxSuperTypeList.Perceptions.Code)
				{
					#region SuppressResourceStringsCheckRegion

					tributosList.Add(new XStreamingElement("otroTributo",
						new XElement("codigo", 7),
						new XElement("descripcion", taxTransaction.TaxConfiguration.Description),
						new XElement("baseImponible", ((ZDecimal)(taxTransaction.OSTaxBase.GetValueOrDefault(ZDecimal.Zero) * multiplier)).FormatDecimals(ArgentinaConstants.MoneyFormat)),
						new XElement("importe", ((ZDecimal)(taxTransaction.OSTaxAmount.GetValueOrDefault(ZDecimal.Zero) * multiplier)).FormatDecimals(ArgentinaConstants.MoneyFormat))));

					#endregion
				}
			}

			return tributosList.Any() ? new XStreamingElement("arrayOtrosTributos", tributosList) : null;
		}

		XStreamingElement ItemsDetailBuildXML(TransactionInfo transaction)
		{
			var itemList = new List<XStreamingElement>();

			if (transaction.TransactionType.HasValue)
			{
				int multiplier = transaction.TransactionType == TransactionType.CRD ? -1 : 1;

				if (transaction.PostingJournalCollection != null && transaction.PostingJournalCollection.Any())
				{
					var isClassAMComplianceSubTypeList = ArgentinaConstants.ClassAMComplianceSubTypeList.Contains(transaction.ComplianceSubType.GetValueOrDefault(ZString.Empty));

					foreach (var item in transaction.PostingJournalCollection)
					{
						if ((item.ChargeCode?.ChargeType?.Code).GetValueOrDefault() == "CMT")
						{
							continue;
						}

						var chargeCode = item.ChargeCode?.Code;
						if (!chargeCode.HasValue)
						{
							chargeCode = item.GLAccount?.AccountCode;
						}
						var amountprecioUnitario = (ZDecimal)((isClassAMComplianceSubTypeList ? item.OSAmount.GetValueOrDefault(ZDecimal.Zero) : item.OSTotalAmount.GetValueOrDefault(ZDecimal.Zero)) * multiplier);
						var amountimporteIVA = (ZDecimal)(item.OSGSTVATAmount.GetValueOrDefault(ZDecimal.Zero) * multiplier);
						var amountimporteItem = (ZDecimal)(item.OSTotalAmount.GetValueOrDefault(ZDecimal.Zero) * multiplier);
						var taxGroupCode = item.TaxMessageID?.TaxGroupCode?.Code;

						#region SuppressResourceStringsCheckRegion

						itemList.Add(new XStreamingElement("item",
							new XElement("unidadesMtx", ArgentinaConstants.UnitOfRefrence),
							new XElement("codigoMtx", item.GovernmentReportingChargeCode.GetValueOrDefault(ZString.Empty).SubstringSafe(0, 13)),
							new XElement("codigo", chargeCode.GetValueOrDefault(ZString.Empty)),
							new XElement("descripcion", item.Description.GetValueOrDefault(ZString.Empty)),
							new XElement("cantidad", ArgentinaConstants.Quantity),
							new XElement("codigoUnidadMedida", ArgentinaConstants.UnitOfMeasure),
							new XElement("precioUnitario", amountprecioUnitario.FormatDecimals("F6")),
							new XElement("codigoCondicionIVA", taxGroupCode), isClassAMComplianceSubTypeList ? new XElement("importeIVA", amountimporteIVA.FormatDecimals("F2")) : null,
							new XElement("importeItem", amountimporteItem.FormatDecimals("F2"))
						));

						#endregion
					}
				}
			}

			return (itemList.Any() ? new XStreamingElement("arrayItems", itemList) : null);
		}

		XStreamingElement BuildDatosAdicionalesXML(TransactionInfo transaction, BusinessObjectFactory factory)
		{
			var datosAdicionalesList = new List<XStreamingElement>();
			var complianceSubType = transaction.ComplianceSubType.GetValueOrDefault(ZString.Empty);
			if (ArgentinaEInvoiceHelper.IsMiPymeComplianceSubType(complianceSubType))
			{
				if (ArgentinaEInvoiceHelper.IsMiPymeDebitOrCreditNoteComplianceSubType(complianceSubType))
				{
					datosAdicionalesList.Add(AddDatoAdicional(ArgentinaConstants.IdIsAnnulment, ArgentinaEInvoiceHelper.GetMiPymeOriginalTransactionIsRejectedByBuyer(transaction)));
				}
				else
				{
					datosAdicionalesList.Add(AddDatoAdicional(ArgentinaConstants.IdCBUItemDetail, ArgentinaEInvoiceHelper.GetBankAccountCBUNumber(transaction, factory)));
					datosAdicionalesList.Add(AddDatoAdicional(ArgentinaConstants.IdTransferModality, ArgentinaConstants.MiPymeTransferModality));
				}
			}

			#region SuppressResourceStringsCheckRegion

			return datosAdicionalesList.Any() ? new XStreamingElement("arrayDatosAdicionales", datosAdicionalesList) : null; // Node description

			XStreamingElement AddDatoAdicional(string id, string value)
			{
				return new XStreamingElement("datoAdicional",
							new XElement("t", id),
							new XElement("c1", value));
			}

			#endregion
		}

		#region SubTotalesIva

		XStreamingElement SubTotalesIvaBuildXML(TransactionInfo transaction)
		{
			var xStreamingElement = new List<XStreamingElement>();
			var multiplier = transaction.TransactionType == TransactionType.CRD ? -1 : 1;
			var linesCollection = transaction.PostingJournalCollection ?? new List<PostingJournal>();
			var taxSummaryLines = from PostingJournal line in linesCollection
								  where line.VATTaxID != null
										&& line.TaxMessageID != null
										&& line.TaxMessageID.TaxGroupCode != null
										&& (line.TaxMessageID.TaxGroupCode.Code.GetValueOrDefault(ZString.Empty) == ArgentinaComplianceInfo.TaxMessageGroupCodes.N4 ||
											line.TaxMessageID.TaxGroupCode.Code.GetValueOrDefault(ZString.Empty) == ArgentinaComplianceInfo.TaxMessageGroupCodes.N5 ||
											line.TaxMessageID.TaxGroupCode.Code.GetValueOrDefault(ZString.Empty) == ArgentinaComplianceInfo.TaxMessageGroupCodes.N6)
								  orderby line.TaxMessageID.TaxGroupCode.Code
								  group line by new { line.TaxMessageID.TaxGroupCode.Code } into taxRates
								  select new TaxSummary
								  (
									   taxRates.Key.Code,
									   taxRates.Sum(p => p.OSGSTVATAmount.GetValueOrDefault(ZDecimal.Zero) * multiplier)
								   );

			foreach (var taxSummaryLine in taxSummaryLines)
			{
				var osGSTVatAmountsum = taxSummaryLine.OSGSTVATAmountSum.GetValueOrDefault(ZDecimal.Zero);
				if (!osGSTVatAmountsum.IsEmpty)
				{
					#region SuppressResourceStringsCheckRegion

					xStreamingElement.Add(new XStreamingElement("subtotalIVA",
								new XElement("codigo", taxSummaryLine.TaxGroupCode),
								new XElement("importe", osGSTVatAmountsum.FormatDecimals("F2"))));

					#endregion
				}
			}

			return (xStreamingElement.Any() ? new XStreamingElement("arraySubtotalesIVA", xStreamingElement) : null);
		}

		class TaxSummary
		{
			internal TaxSummary(string taxGroupCode, ZDecimal? osGSTVATAmountSum)
			{
				TaxGroupCode = taxGroupCode;
				OSGSTVATAmountSum = osGSTVATAmountSum;
			}

			public ZDecimal? OSGSTVATAmountSum;
			public string TaxGroupCode;
		}

		#endregion

		#region Comprobantes asociados

		XStreamingElement ComprobantesAsociadosBuildXML(TransactionInfo transactionInfo)
		{
			var originalReferenceComplianceSubtype = ArgentinaEInvoiceHelper.GetOriginalTransactionComplianceSubType(transactionInfo.OriginalReference);
			var complianceSubtype = EInvoicingExtension.GetDocumentType(originalReferenceComplianceSubtype);
			var originalTransactionCompliancePrefix = ArgentinaEInvoiceHelper.GetComplianceNumberPrefixFromTransaction(transactionInfo.OriginalReference);
			var originalTransactionComplianceNumber = ArgentinaEInvoiceHelper.GetComplianceNumberFromTransaction(transactionInfo.OriginalReference);
			var cuit = Helper.GetRegistrationCode(transactionInfo.BranchAddress, CountryCodes.Argentina, ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT);
			var isCuitPresentForMyPymeComplianceSubType = ArgentinaConstants.MiPymeDebitOrCreditNoteComplianceSubTypeMustIncludeCuitInComprobantesAsociadosList.Contains(transactionInfo.ComplianceSubType.GetValueOrDefault(ZString.Empty));

			var cmpsAsocList = new List<XStreamingElement>();

			#region SuppressResourceStringsCheckRegion

			var cmpsAsocItem = new XStreamingElement("comprobanteAsociado"); // Hard - coded xml node name

			cmpsAsocItem.Add(new XElement("codigoTipoComprobante", complianceSubtype),
				new XElement("numeroPuntoVenta", originalTransactionCompliancePrefix),
				new XElement("numeroComprobante", originalTransactionComplianceNumber),
				isCuitPresentForMyPymeComplianceSubType ? new XElement("cuit", ((ZString)cuit).RemoveNonNumericCharacters()) : null,
				new XElement("fechaEmision", transactionInfo.OriginalReference != null ? transactionInfo.OriginalReference.OriginalTransactionDate.FormatNullableDate(("yyyy-MM-dd")) : string.Empty));
			cmpsAsocList.Add(cmpsAsocItem);

			#endregion

			return new XStreamingElement("arrayComprobantesAsociados", cmpsAsocList);
		}

		#endregion

		#endregion
	}
}
