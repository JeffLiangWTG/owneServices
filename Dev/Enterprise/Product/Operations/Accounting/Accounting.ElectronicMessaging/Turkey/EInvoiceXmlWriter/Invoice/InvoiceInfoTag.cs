using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;
using static Enterprise.MasterFiles.Business.TurkeyOrgCusCodeInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class InvoiceInfoTag
	{
		public InvoiceInfoTag(TransactionInfo uInvoice, GlbCompany company)
		{
			Argument.NotNull(uInvoice, nameof(uInvoice));
			Argument.NotNull(company, nameof(company));
			Argument.NotNullOrEmpty(uInvoice.ComplianceSubType, nameof(uInvoice.ComplianceSubType));
			Argument.NotNullOrEmpty(uInvoice.TransactionReference, nameof(uInvoice.TransactionReference));

			Helper = new EInvoiceHelper(uInvoice, company);
		}
		readonly EInvoiceHelper Helper;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Turkish Language")]
		public InvoiceInfo BuildInvoiceInfo()
		{
			var eInvoiceInfo = new InvoiceInfo();
			eInvoiceInfo.Invoice = BuildInvoice();
			eInvoiceInfo.LocalDocumentId = Helper.UInvoice.TransactionReference.Value;
			eInvoiceInfo.CreateDateUtc = new DateTime(Helper.UInvoice.CreateTime.Value.Ticks, DateTimeKind.Utc);

			//Creating Scenario tag - we need a parameter about scenario - Default is "Automated", otherwise (Debtor is e-archive, value is "eArchive"), (Debtor is e-invoice, value is "eInvoice")
			eInvoiceInfo.Scenario = InvoiceScenarioChoosen.Automated;

			var debtorPEC = Helper.GetOrgCusCodeValue(CountryCodes.Turkey, new ZString[] { OrgCusCodes.PEC })?.Value.Value ?? ZString.Empty;

			if (eInvoiceInfo.Invoice.ProfileID.Value == EInvoiceProfileTypes.EARSIVFATURA)
			{
				eInvoiceInfo.EArchiveInvoiceInfo = new EArchiveInvoiceInformation() { DeliveryType = InvoiceDeliveryType.Paper };
				eInvoiceInfo.Notification = new NotificationInformation();
				var mailing = new List<MailingInformation>();
				var info = new MailingInformation();
				info.To = debtorPEC;
				info.EnableNotification = true;
				info.BodyXsltIdentifier = GetBodyXsltIdentifier();
				info.Subject = ((NoResString)"e-Fatura Bilgileri");
				info.Attachment = new MailAttachmentInformation();
				info.Attachment.Html = false;
				info.Attachment.Pdf = true;
				info.Attachment.Xml = true;
				mailing.Add(info);
				eInvoiceInfo.Notification.Mailing = mailing.ToArray();
			}
			else
			{
				eInvoiceInfo.TargetCustomer = new CustomerInfo();
				eInvoiceInfo.TargetCustomer.VknTckn = Helper.IsOrganizationCategoryNAT
					? Helper.CustomerTCKN
					: Helper.CustomerVATCode;
				eInvoiceInfo.TargetCustomer.Alias = !debtorPEC.IsEmpty ? string.Format(CultureInfo.CurrentCulture, "urn:mail:{0}", debtorPEC) : string.Empty;
				eInvoiceInfo.TargetCustomer.Title = Helper.UInvoice.OrganizationAddress.CompanyName;
			}

			return eInvoiceInfo;
		}

		InvoiceType BuildInvoice()
		{
			var eInvoice = new InvoiceType();
			eInvoice.UBLVersionID = new UBLVersionIDType() { Value = "2.1" };
			eInvoice.CustomizationID = new CustomizationIDType() { Value = "TR1.2" };
			eInvoice.ID = new IDType() { Value = Helper.UInvoice.TransactionReference.Value };
			eInvoice.CopyIndicator = new CopyIndicatorType() { Value = false };
			eInvoice.UUID = new UUIDType() { Value = string.Empty }; // Authomaticaly is given by Uyumsoft
			eInvoice.IssueDate = new IssueDateType() { Value = Helper.UInvoice.TransactionDate.Value.ToDateTime().Date };
			eInvoice.Note = new InvoiceNote().BuildNote(Helper);
			eInvoice.DocumentCurrencyCode = new DocumentCurrencyCodeType() { Value = Helper.UInvoice.OSCurrency.Code.Value };
			eInvoice.AccountingCost = new AccountingCostType() { Value = EInvoiceHelper.Sell }; // Turkish Language
			eInvoice.LineCountNumeric = new LineCountNumericType() { Value = Helper.NonCommentInvoiceLines.Count };
			var template = GetXsltTemplateInfo();
			var additionalDocumentReferences = new List<DocumentReferenceType>();

			if (!string.IsNullOrEmpty(template.Name) || !template.ExternalReference.IsEmpty)
			{
				additionalDocumentReferences.Add(new AdditionalDocumentReference().BuildAdditionalDocumentReference(template.Name, template.Data, template.ExternalReference));
			}
			additionalDocumentReferences.Add(new AdditionalDocumentReference().BuildAdditionalDocumentReferenceForTransactionNumber(Helper.UInvoice.Number.Value));

			eInvoice.AdditionalDocumentReference = additionalDocumentReferences.ToArray() ;

			eInvoice.OrderReference = BuildOrderReference();

			eInvoice.AccountingSupplierParty = new Party().BuildAccountingSupplierParty(Helper);
			eInvoice.AccountingCustomerParty = new Party().BuildAccountingCustomerParty(Helper);
			eInvoice.BuyerCustomerParty = new Party().BuildBuyerCustomerParty(Helper);

			eInvoice.PaymentMeans = new PaymentMeans().BuildPaymentMeans(Helper);

			eInvoice.PricingExchangeRate = BuildPricingExchangeRate();

			var taxTotal = new TaxTotal(Helper);
			eInvoice.InvoiceTypeCode = new InvoiceTypeCodeType() { Value = GetInvoiceInfoType(taxTotal) };
			eInvoice.ProfileID = new ProfileIDType() { Value = GetProfileID(eInvoice.InvoiceTypeCode.Value) };

			eInvoice.BillingReference = BuildBillingReferences(eInvoice.InvoiceTypeCode.Value);

			eInvoice.TaxTotal = taxTotal.BuildTaxTotals();
			if (taxTotal.HasWithholdingTax)
			{
				eInvoice.WithholdingTaxTotal = taxTotal.BuildWithholdingTaxTotals();
			}
			eInvoice.LegalMonetaryTotal = new LegalMonetaryTotal().BuildLegalMonetaryTotal(Helper);
			eInvoice.InvoiceLine = new InvoiceLine().BuildInvoiceLine(Helper);
			return eInvoice;
		}

		efatura.uyumsoft.com.tr.ExchangeRateType BuildPricingExchangeRate()
		{
			// As legacy system
			if (Helper.UInvoice.OSCurrency.Code.Value != CurrencyCodes.Turkey)
			{
				var pricingExchangeRate = new efatura.uyumsoft.com.tr.ExchangeRateType();
				pricingExchangeRate.SourceCurrencyCode = new SourceCurrencyCodeType() { Value = Helper.UInvoice.OSCurrency.Code.Value };
				pricingExchangeRate.TargetCurrencyCode = new TargetCurrencyCodeType() { Value = CurrencyCodes.Turkey };
				pricingExchangeRate.CalculationRate = new CalculationRateType() { Value = Helper.UInvoice.ExchangeRate.Value };
				pricingExchangeRate.Date = new DateType1() { Value = Helper.UInvoice.TransactionDate.Value.Date.ToDateTime() };

				return pricingExchangeRate;
			}

			return null;
		}

		string GetProfileID(string invoiceType)
		{
			switch (Helper.UInvoice.ComplianceSubType.Value)
			{
				case ComplianceSubTypeCodes.EIN:
				case ComplianceSubTypeCodes.ICN:
				case ComplianceSubTypeCodes.DIN:
					return EInvoiceProfileTypes.TEMELFATURA;

				case ComplianceSubTypeCodes.EIC:
					return EInvoiceProfileTypes.TICARIFATURA;

				case ComplianceSubTypeCodes.EAR:
				case ComplianceSubTypeCodes.DAR:
					return EInvoiceProfileTypes.EARSIVFATURA;

				case ComplianceSubTypeCodes.CCN:
					return GetProfileIDForOrganization(invoiceType);

				default:
					throw new ArgumentException("Invalid Compliance Sub Type.");
			}
		}

		string GetProfileIDForOrganization(string invoiceType)
		{
			if (Helper.HasTurkeyCusCode(OrgCusCodes.VTE))
			{
				return EInvoiceProfileTypes.TEMELFATURA;
			}
			else if (Helper.HasTurkeyCusCode(OrgCusCodes.VTC))
			{
				return invoiceType == EInvoiceInfoTypes.Return ? EInvoiceProfileTypes.TEMELFATURA : EInvoiceProfileTypes.TICARIFATURA;
			}
			else
			{
				return EInvoiceProfileTypes.EARSIVFATURA;
			}
		}

		//specialRate: OZELMATRAH
		//TO-DO: OzelMatrah InvoiceInfoType will be added if it's deemed necessary.
		string GetInvoiceInfoType(TaxTotal taxTotal) =>
			ReturnInvoiceComplianceSubTypes.Contains(Helper.UInvoice.ComplianceSubType.Value)
			? EInvoiceInfoTypes.Return
			: taxTotal.HasTaxExemption
			? EInvoiceInfoTypes.TaxExemption
			: taxTotal.HasWithholdingTax
			? EInvoiceInfoTypes.WithHoldingTax
			: EInvoiceInfoTypes.ARInvoice;

		List<string> ReturnInvoiceComplianceSubTypes => new List<string>() { ComplianceSubTypeCodes.DAR, ComplianceSubTypeCodes.DIN };

		OrderReferenceType BuildOrderReference()
		{
			var collectedShipments = new List<Shipment>();
			Helper.CollectShipments(new DataObjectList<Shipment>(Helper.UInvoice.ShipmentCollection), collectedShipments);

			var orderNumbersStringBuilder = new ZStringBuilder();
			var orderDate = ZDate.Empty;

			foreach (var shipment in collectedShipments)
			{
				shipment.LocalProcessing?.OrderNumberCollection?.ForEach(order => orderNumbersStringBuilder.AppendIfNotEmptyAndNotExists(order.OrderReference));

				if (orderDate.IsEmpty && shipment.RelatedShipmentCollection != null)
				{
					foreach (var relatedShipment in shipment.RelatedShipmentCollection)
					{
						var date = relatedShipment.DateCollection?.FirstOrDefault(x => x.Type == UniversalDataBuss.DataObjects.Universal.DateType.OrderDate && x.Value.HasValue);
						if (date != null)
						{
							orderDate = new ZDate(date.Value.ToString());
							break;
						}
					}
				}
			}

			var orderNumbers = orderNumbersStringBuilder.ToStringWithDelimiterBetweenAppends(Extensions.Comma);
			orderDate = new ZDate(
				orderNumbers.Contains(Extensions.Comma) || !orderDate.IsValid
				? Helper.UInvoice.TransactionDate.Value
				: orderDate);

			return BuildOrderReference(orderNumbers, orderDate);
		}

		OrderReferenceType BuildOrderReference(string orderNumbers, ZDate orderDate) =>
			!string.IsNullOrEmpty(orderNumbers)
			? new OrderReferenceType()
			{
				ID = new IDType() { Value = orderNumbers },
				IssueDate = new IssueDateType() { Value = orderDate.ToDateTime() }
			}
			: null;

		const string FATURA = nameof(FATURA);

		BillingReferenceType[] BuildBillingReferences(string invoiceType) =>
			(invoiceType == EInvoiceInfoTypes.Return
			&& Helper.UInvoice.OriginalReference != null
			&& Helper.UInvoice.OriginalReference.OriginalTransactionNumber.HasValue
			&& Helper.UInvoice.OriginalReference.OriginalTransactionDate.HasValue
			&& !Helper.UInvoice.OriginalReference.OriginalTransactionDate.Value.IsEmpty)
			? new List<BillingReferenceType>()
			{
				new BillingReferenceType()
				{
					InvoiceDocumentReference = new DocumentReferenceType()
					{
						ID = new IDType() { Value = Helper.UInvoice.OriginalReference.OriginalTransactionNumber },
						IssueDate = new IssueDateType() { Value = Helper.UInvoice.OriginalReference.OriginalTransactionDate.Value.ToDateTime().Date },
						DocumentType = new DocumentTypeType() { Value = FATURA },
						DocumentTypeCode = new DocumentTypeCodeType() { Value = EInvoiceInfoTypes.Return },
					}
				}
			}.ToArray()
			: null;

		(ZString Name, ZBlob Data, ZGuid ExternalReference) GetXsltTemplateInfo()
		{
			var invoiceBase = Helper.TransactionHeader as InvoicingBase;
			var jobType = invoiceBase?.JobType?.Code;
			var transportMode = invoiceBase?.TransportMode ?? ZString.Empty;

			var templateFile = Helper.Debtor?.GetValidTemplateFileForEInvoice(jobType, transportMode);

			return templateFile == null || !templateFile.TFS_IsActive
				? (ZString.Empty, ZBlob.Empty, ZGuid.Empty)
				: (templateFile.TFS_FileName, templateFile.TFS_FileData, templateFile.TFS_ExternalReference);
		}

		string GetBodyXsltIdentifier()
		{
			var isProductionServer = Env.Instance.IsProductionSystem;
			var bodyXsltIdentifier = isProductionServer ? string.Empty : ((NoResString)"C5A2BD86-4054-4387-9499-831AC6B108CA");
			return bodyXsltIdentifier;
		}
	}
}
