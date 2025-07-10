using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class DatiGenerali
	{
		public DatiGenerali(BusinessObjectFactory businessObjectFactory)
		{
			Argument.NotNull(businessObjectFactory, "factory"); // Argument factory;
			factory = businessObjectFactory;
		}

		readonly BusinessObjectFactory factory;

		public XStreamingElement BuildXML(TransactionInfo transaction, INotifications errorNotifications, INotifications warningNotifications = null)
		{
			var result = new XStreamingElement("DatiGenerali", BuildXmlForDatiGeneraliDocumento(transaction, errorNotifications));
			var datiOrdineAcquisto = BuildXmlForDatiOrdineAcquisto(transaction, errorNotifications);
			if (datiOrdineAcquisto != null)
			{
				result.Add(datiOrdineAcquisto);
			}
			var datiFattureCollegate = BuildXmlForDatiFattureCollegate(transaction, errorNotifications, warningNotifications);
			if (datiFattureCollegate != null)
			{
				result.Add(datiFattureCollegate);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it is the xml node name, we use the string constant to generate the node value")]
		XStreamingElement BuildXmlForDatiGeneraliDocumento(TransactionInfo transaction, INotifications errorNotifications)
		{
			var tipoDocumento = MapTransactionTypeToTipoDocumento(transaction);
			var numero = MapNumeroBasedOnRegistryValue(transaction, errorNotifications);
			var dataDocumento = FatturaElettronicaDataHelper.IsPayable(transaction) ? transaction.PostDate : transaction.TransactionDate;

			var datiGeneraliDocumentoElement = new XStreamingElement("DatiGeneraliDocumento",
			new XElement("TipoDocumento", tipoDocumento),
			new XElement("Divisa", Core.Constants.CurrencyCodes.EuropeanUnion),
			new XElement("Data", dataDocumento.ToDateType()),
			new XElement("Numero", numero.EnsureComplianceWithBasicLatin()));

			var datiBolloElement = BuildXmlForDatiBollo(transaction, errorNotifications);
			if (datiBolloElement != null)
			{
				datiGeneraliDocumentoElement.Add(datiBolloElement);
			}

			var importoTotaleDocumentoValue = ZDecimal.Zero;
			if (transaction.LocalTotal != null)
			{
				importoTotaleDocumentoValue = transaction.LocalTotal.Value * GetMultiplier(transaction);
			}

			ZDecimal? importoTotaleDocumento = importoTotaleDocumentoValue + GetTotalRVSLinesAmount(transaction);
			datiGeneraliDocumentoElement.Add(new XElement("ImportoTotaleDocumento", importoTotaleDocumento.ToAmountDecimalType(transaction)));

			var transOSCurrencyCode = transaction.OSCurrency?.Code;
			if (transOSCurrencyCode.HasValue)
			{
				var causaleElementValue = ZString.Empty;
				if (FatturaElettronicaDataHelper.IsPayable(transaction))
				{
					var originalTransactionNumber = transaction.Number.HasValue ? transaction.Number : ZString.Empty;
					var originalTransactionDate = transaction.TransactionDate.HasValue ? transaction.TransactionDate : ZDateTime.Empty;

					causaleElementValue = string.Format((NoResString)"Documento Autofattura/Integrazione relativo a documento originario numero {0} del {1} in {2} al cambio {3} per totali {2} {4}",
						originalTransactionNumber.Value.EnsureComplianceWithBasicLatin(),
						originalTransactionDate.ToDateType(),
						transOSCurrencyCode.Value,
						transaction.ExchangeRate.ToRateDecimalType(),
						transaction.OSTotal.ToAmountDecimalType(transaction)
						);
				}
				else
				{
					causaleElementValue = string.Format((NoResString)"Documento emesso in valuta {0} al cambio {1} per totali {0} {2}",
						transOSCurrencyCode.Value, transaction.ExchangeRate.ToRateDecimalType(), transaction.OSTotal.ToAmountDecimalType(transaction));
				}

				var causaleElement = new XElement("Causale", string.Format(causaleElementValue));
				datiGeneraliDocumentoElement.Add(causaleElement);
			}

			var transactionDescription = transaction.Description;

			if (!string.IsNullOrWhiteSpace(transactionDescription))
			{
				var causaleElement = new XElement("Causale", transactionDescription.EnsureComplianceWithBasicLatin());
				datiGeneraliDocumentoElement.Add(causaleElement);
			}

			return datiGeneraliDocumentoElement;
		}

		string MapNumeroBasedOnRegistryValue(TransactionInfo transaction, INotifications errorNotifications)
		{
			var numero = ZString.Empty;
			if (FatturaElettronicaDataHelper.IsPayable(transaction))
			{
				if (transaction.TransactionReference.HasValue)
				{
					numero = transaction.TransactionReference.Value; //Currently there is no error report on empty Compliance Number for AP but it will fail on XSD validation
				}
			}
			else
			{
				switch (AccountingMasterFilesRegistry.Instance.EReportingTransactionNumber.Value)
				{
					case TransactionNumberCodes.ComplianceNr:
						if (transaction.TransactionReference.HasValue)
						{
							numero = transaction.TransactionReference.Value;
						}
						else
						{
							errorNotifications.AddError(Res.GetString("DD9AE0BC-8239-47F0-94D6-6B444A98C82F",
								"Reference Number cannot be empty. Please assign the Compliance Number to this transaction, then re-queue the transaction for sending."));
						}
						break;
					case TransactionNumberCodes.InvoiceNr:
						if (transaction.Number.HasValue)
						{
							numero = transaction.Number.Value;
						}
						break;
					case TransactionNumberCodes.ComplianceOrInvoiceNr:
						numero = transaction.TransactionReference ?? transaction.Number ?? ZString.Empty;
						break;
				}
			}

			return numero;
		}

		ZDecimal? GetTotalRVSLinesAmount(TransactionInfo transaction)
		{
			ZDecimal? value = ZDecimal.Zero;
			if (FatturaElettronicaDataHelper.IsPayable(transaction))
			{
				var lineCollection = transaction?.PostingJournalCollection ?? new List<PostingJournal>();
				foreach (var line in lineCollection.Where(l => l.VATTaxID?.TaxType.Code.ToString() == AccTaxRate.Types.ReverseRated))
				{
					var localAmount = line.LocalAmount.Value * GetMultiplier(transaction);
					value += (ZDecimal)(localAmount * (line.VATTaxID.TaxRate / 100));
				}
				value = Utilities.Round(value.Value, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			}
			return value;
		}

		string MapTransactionTypeToTipoDocumento(TransactionInfo transaction)
		{
			var result = string.Empty;
			if (FatturaElettronicaDataHelper.IsPayable(transaction) && transaction.ComplianceSubType.HasValue)
			{
				switch (transaction.ComplianceSubType.Value)
				{
					case ItalyComplianceInfo.ComplianceSubTypeCodes.INI:
						result = "TD16";
						break;
					case ItalyComplianceInfo.ComplianceSubTypeCodes.INT:
						result = FatturaElettronicaDataHelper.IsPayableForGoods(transaction) ? "TD18" : "TD17";
						break;
					case ItalyComplianceInfo.ComplianceSubTypeCodes.APS:
						result = FatturaElettronicaDataHelper.IsPayableForGoods(transaction) ? "TD19" : "TD17";
						break;
				}
			}
			else if (FatturaElettronicaDataHelper.IsReceivable(transaction) && transaction.TransactionType.HasValue)
			{
				switch (transaction.TransactionType.Value)
				{
					case TransactionType.INV:
						result = FatturaElettronicaDataHelper.IsReceivableForInternal(transaction, factory) ? "TD27" :
							transaction.ComplianceSubType.GetValueOrDefault() == ItalyComplianceInfo.ComplianceSubTypeCodes.G26 ? "TD26" :
							transaction.OriginalReference == null ? "TD01" : "TD05";
						break;
					case TransactionType.CRD:
						result = "TD04";
						break;
					case TransactionType.ADJ:
						result = transaction.LocalExVATAmount <= 0 ? "TD04" :
							FatturaElettronicaDataHelper.IsReceivableForInternal(transaction, factory) ? "TD27" :
							transaction.ComplianceSubType.GetValueOrDefault() == ItalyComplianceInfo.ComplianceSubTypeCodes.G26 ? "TD26" :
							"TD01";
						break;
				}
			}
			return result;
		}

		XStreamingElement BuildXmlForDatiBollo(TransactionInfo transaction, INotifications errorNotifications)
		{
			GetBolloVirtuale(transaction, out ZString bolloVirtuale, out ZDecimal? importoBollo, errorNotifications);
			return bolloVirtuale == OnlyValidBolloVirtualeValue ? new XStreamingElement("DatiBollo",
				new XElement("BolloVirtuale", bolloVirtuale),
				new XElement("ImportoBollo", importoBollo.ToAmountDecimalType())) : null;
		}

		void GetBolloVirtuale(TransactionInfo transaction, out ZString bolloVirtuale, out ZDecimal? importoBollo, INotifications errorNotifications)
		{
			bolloVirtuale = ZString.Empty;
			importoBollo = 0m;
			var invoiceInDB = GetInvoiceInDB(transaction, errorNotifications);
			if (invoiceInDB != null
				&& invoiceInDB.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.StampDutyLiability.Code).Any()
				&& !AccountingConfigurationRegistry.Instance.StampDutyARDocumentMessage.GetValueWithoutFallback(invoiceInDB.AH_GC.ToGuid(), Guid.Empty, Guid.Empty).IsEmpty)
			{
				bolloVirtuale = OnlyValidBolloVirtualeValue;
				importoBollo = AccountingConfigurationRegistry.Instance.StampDutyFixedAmount.GetValueWithoutFallback(invoiceInDB.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			}
		}

		const string OnlyValidBolloVirtualeValue = "SI";

		InvoicingBase GetInvoiceInDB(TransactionInfo transaction, INotifications errorNotifications)
		{
			InvoicingBase result = null;
			if (transaction.Ledger.HasValue && transaction.TransactionType.HasValue && transaction.Number.HasValue)
			{
				var invoiceQuery = new ZDBOnlyQuery(typeof(InvoicingBase));
				invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, transaction.Ledger.Value);
				invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transaction.TransactionType.Value);
				invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.Number.Value);

				var companySubQuery = new ZDBOnlySubQuery(typeof(GlbCompany), AccTransactionHeaderSchema.AH_GC);
				companySubQuery.AddToFilter(GlbCompanySchema.GC_IsActive, ZBool.True);
				if (transaction.DataContext != null)
				{
					companySubQuery.AddToFilter(GlbCompanySchema.GC_Code, transaction.DataContext.GetEnterpriseServerAndCompanyIDs().CompanyCode);
					invoiceQuery.AddSubQuery(companySubQuery, JoinCondition.And);
					result = factory.LoadTop1<InvoicingBase>(invoiceQuery);
				}
				else
				{
					errorNotifications.AddError(Res.GetString("7dc9e0b4-373d-4bb7-b60e-287be522abce", "Failed to process General Data due to Data context is null. Transaction type: '{0}', Ledger type: '{1}', Transaction number: '{2}'.",
							Enum.GetName(typeof(TransactionType), transaction.TransactionType.Value), transaction.Ledger.Value, transaction.Number.Value));
				}
			}
			else
			{
				errorNotifications.AddError(Res.GetString("6e27d2d4-1be1-4dfa-a4ee-99aba10dd9a3", "Failed to process General Data due to unsupported transaction. Transaction type: '{0}', Ledger type: '{1}', Transaction number: '{2}'.",
							transaction.TransactionType.HasValue ? Enum.GetName(typeof(TransactionType), transaction.TransactionType.Value) : string.Empty,
							transaction.Ledger ?? ZString.Empty,
							transaction.Number ?? ZString.Empty));
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML path.")]
		XStreamingElement BuildXmlForDatiFattureCollegate(TransactionInfo transaction, INotifications errorNotifications, INotifications warningNotifications)
		{
			XStreamingElement result = null;

			if (FatturaElettronicaDataHelper.IsPayable(transaction))
			{
				var transactionNumber = transaction.Number;
				var invoiceDate = transaction.TransactionDate;
				if (transactionNumber.HasValue && invoiceDate.HasValue)
				{
					result = new XStreamingElement("DatiFattureCollegate",
						new XElement("IdDocumento", transactionNumber.Value.EnsureComplianceWithBasicLatin()),
						new XElement("Data", invoiceDate.ToDateType()));
				}
			}
			else
			{
				var transOriginalReference = transaction.OriginalReference;
				if (transOriginalReference != null)
				{
					var idDocumento = ZString.Empty;
					var originalTransactionDate = transOriginalReference.OriginalTransactionDate;
					var originalTransactionReference = transOriginalReference.OriginalTransactionReference;
					var originalTransactionNumber = transOriginalReference.OriginalTransactionNumber;
					var originalTransactionReferenceIsPresent = originalTransactionReference.HasValue && !originalTransactionReference.Value.IsEmpty;
					var originalTransactionNumberIsPresent = originalTransactionNumber.HasValue && !originalTransactionNumber.Value.IsEmpty;

					if (originalTransactionDate.HasValue && originalTransactionDate.Value.IsValid &&
						(originalTransactionNumberIsPresent || originalTransactionReferenceIsPresent))
					{
						switch (AccountingMasterFilesRegistry.Instance.EReportingTransactionNumber.Value)
						{
							case TransactionNumberCodes.ComplianceOrInvoiceNr:
								idDocumento = originalTransactionReferenceIsPresent ? originalTransactionReference.Value : originalTransactionNumber.Value;
								break;
							case TransactionNumberCodes.InvoiceNr:
								idDocumento = originalTransactionNumberIsPresent ? originalTransactionNumber.Value : ZString.Empty;
								break;
							case TransactionNumberCodes.ComplianceNr:
								idDocumento = originalTransactionReferenceIsPresent ? originalTransactionReference.Value : ZString.Empty;
								break;
						}
					}

					if (!idDocumento.IsEmpty)
					{
						result = new XStreamingElement("DatiFattureCollegate",
							new XElement("IdDocumento", idDocumento.EnsureComplianceWithBasicLatin()),
							new XElement("Data", transOriginalReference.OriginalTransactionDate.ToDateType()));
					}
					else
					{
						const string xmlPath = "FatturaElettronicaBody/DatiGenerali/DatiFattureCollegate ";
						var warningMsg = xmlPath + Res.GetString("01A95712-D5C5-40B1-A198-5BB3242D0CD1", "<IdDocumento>, <Data> should contain Original Invoice Number and Original Invoice Date of Original Transaction. Without these references, the invoice may be not linked to the original transaction.");
						warningNotifications.AddWarning(warningMsg);
					}
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML path.")]
		XStreamingElement BuildXmlForDatiOrdineAcquisto(TransactionInfo transaction, INotifications errorNotifications)
		{
			XStreamingElement result = null;
			var recipientOrgCode = transaction.OrganizationAddress?.OrganizationCode?.SourceValue;
			var recipientOrgCategory = ZString.Empty;
			if (recipientOrgCode.HasValue)
			{
				var recipientOrg = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, recipientOrgCode));
				if (recipientOrg != null)
				{
					recipientOrgCategory = recipientOrg.OH_Category;
				}
			}
			if (transaction.ShipmentCollection != null)
			{
				var jobNumber = ZString.Empty;
				var jobNumberKey = transaction.Job?.Key;
				if (jobNumberKey.HasValue)
				{
					jobNumber = jobNumberKey.Value;
				}

				var cupReferenceNum = ZString.Empty;
				var cigReferenceNum = ZString.Empty;
				var idDocumento = ZString.Empty;
				var shipment = transaction.ShipmentCollection.FirstOrDefault();
				ExtractDatiOrdineAcquistoData(shipment, jobNumber, ref cupReferenceNum, ref cigReferenceNum, ref idDocumento);
				if (!idDocumento.IsEmpty)
				{
					result = new XStreamingElement("DatiOrdineAcquisto",
							new XElement("IdDocumento", idDocumento.EnsureComplianceWithBasicLatin()),
							!cupReferenceNum.IsEmpty ? new XElement("CodiceCUP", cupReferenceNum.EnsureComplianceWithBasicLatin()) : null,
							!cigReferenceNum.IsEmpty ? new XElement("CodiceCIG", cigReferenceNum.EnsureComplianceWithBasicLatin()) : null);
				}
				else if (!cigReferenceNum.IsEmpty || !cupReferenceNum.IsEmpty)
				{
					const string xmlPath = "FatturaElettronicaBody/DatiGenerali/DatiOrdineAcquisto ";
					var errorMsgForGov = xmlPath + Res.GetString("686F13EB-596A-4FDE-AA9C-084759EA6364", "<IdDocumento>, <CodiceCUP> and <CodiceCIG> Job must contain Order Reference, CIG and CUP when Account is a GOV Organization.");
					var errorMsgForNotGov = xmlPath + Res.GetString("3F84E41A-94FF-4127-82B1-DD1552EBF7FF", "<IdDocumento>, <CodiceCUP> and <CodiceCIG> Job must contain Order Reference when CIG and CUP are specified.");
					errorNotifications.AddError(recipientOrgCategory == OrgConstants.Category.Government
						? errorMsgForGov
						: errorMsgForNotGov);
				}
			}
			return result;
		}

		void ExtractDatiOrdineAcquistoData(Shipment shipment, ZString jobNumber, ref ZString cupReferenceNum, ref ZString cigReferenceNum, ref ZString idDocumento)
		{
			if (shipment != null && jobNumber != ZString.Empty)
			{
				List<Shipment> allShipmentsIncludingSubShipments = new List<Shipment> { shipment };
				allShipmentsIncludingSubShipments.AddRange(GetAllSubShipmentsForShipment(shipment));
				ExtractDatiOrdineAcquistoDataFromShipment(allShipmentsIncludingSubShipments, jobNumber, ref cupReferenceNum, ref cigReferenceNum, ref idDocumento);
			}
		}

		List<Shipment> GetAllSubShipmentsForShipment(Shipment shipment)
		{
			var result = new List<Shipment>();
			if (shipment.SubShipmentCollection != null && shipment.SubShipmentCollection.Any())
			{
				foreach (var subShipment in shipment.SubShipmentCollection)
				{
					result.Add(subShipment);
					result.AddRange(GetAllSubShipmentsForShipment(subShipment));
				}
			}
			return result;
		}

		void ExtractDatiOrdineAcquistoDataFromShipment(List<Shipment> allShipmentsIncludingSubShipments, ZString jobNumber, ref ZString cupReferenceNum, ref ZString cigReferenceNum, ref ZString idDocumento)
		{
			foreach (var shipment in allShipmentsIncludingSubShipments)
			{
				var ds = shipment.DataContext?.DataSourceCollection?.FirstOrDefault();
				if (ds != null && ds.Key.HasValue && ds.Key.Value == jobNumber)
				{
					idDocumento = ExtractLocalProcessingData(shipment);
					ExtractAdditionalReferenceData(shipment, ref cupReferenceNum, ref cigReferenceNum);
					return;
				}
			}
		}

		void ExtractAdditionalReferenceData(Shipment shipment, ref ZString cupReferenceNum, ref ZString cigReferenceNum)
		{
			var addRefs = shipment.AdditionalReferenceCollection?.Where(x => x.Type != null && x.Type.Code.HasValue && x.ReferenceNumber.HasValue && !x.ReferenceNumber.Value.IsEmpty);
			if (addRefs != null && addRefs.Any())
			{
				var cupReference = addRefs.FirstOrDefault(x => x.Type.Code.Value == ItalyAdditionalReferenceNumberTypes.Codes.CUP);
				if (cupReference != null)
				{
					cupReferenceNum = cupReference.ReferenceNumber.Value.Left(15);
				}
				var cigReference = addRefs.FirstOrDefault(x => x.Type.Code.Value == ItalyAdditionalReferenceNumberTypes.Codes.CIG);
				if (cigReference != null)
				{
					cigReferenceNum = cigReference.ReferenceNumber.Value.Left(15);
				}
			}
		}

		ZString ExtractLocalProcessingData(Shipment shipment)
		{
			var result = ZString.Empty;
			var orderReferences = shipment.LocalProcessing?.OrderNumberCollection?.Where(x => x.OrderReference.HasValue && !x.OrderReference.Value.IsEmpty);
			if (orderReferences != null && orderReferences.Any())
			{
				result = string.Join(",", orderReferences.Select(x => x.OrderReference.Value));
				result = result.Left(20);
			}
			return result;
		}

		int GetMultiplier(TransactionInfo transaction) => FatturaElettronicaDataHelper.IsPayableInvOrPositiveAdj(transaction) ? -1 : 1;
	}
}
