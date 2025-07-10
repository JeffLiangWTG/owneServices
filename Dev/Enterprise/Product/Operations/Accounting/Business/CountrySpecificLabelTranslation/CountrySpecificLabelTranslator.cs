using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public interface ILabelTranslator
	{
		ZString GetTranslation(LabelsEnum? label, params object[] parameters);
	}

	public class CountrySpecificLabelTranslator : ILabelTranslator
	{
		ZString ILabelTranslator.GetTranslation(LabelsEnum? label, params object[] parameters)
		{
			return (AccountingCountryFactory as ICountrySpecificLabelTranslator)?.GetTranslation(label, parameters) ?? GetDefaultLabelTranslation(label, parameters);
		}

		public ZString GetDefaultLabelTranslation(LabelsEnum? label, params object[] parameters)
		{
			switch (label)
			{
				case LabelsEnum.InvoiceAuthorisationRecordAuthorisationDataLabel:
					return Res.GetString("CF9E44CD-466D-4C69-8F33-16F431CB0157", "Authorization Data");
				case LabelsEnum.InvoiceAuthorisationRecordIssuerAuthorizationDataLabel:
					return Res.GetString("AD581339-5076-4F33-8F8B-16EF6E8C0546", "Issuer Authorization Data");
				case LabelsEnum.InvoiceAuthorisationRecordPlaceOfIssueLabel:
					return Res.GetString("E532C0E0-4FEF-432C-959E-2F5A080FCC6D", "Place of Issue");
				case LabelsEnum.InvoiceAuthorisationRecordIssuerCertificateIdentifierLabel:
					return Res.GetString("158D6A86-D603-4999-A44E-5E7693F31373", "Issuer Certificate Identifier");
				case LabelsEnum.GovernmentCreditTermsLabel:
					return Res.GetString("2823C2DE-22BB-450F-9F8F-30A5EFA7D5F7", "Government Credit Terms");
				case LabelsEnum.InvoiceAuthorisationRecordDebtorNumberLabel:
					return Res.GetString("D30D32B1-57DC-4EF8-8563-3C8FACC2FED5", "Debtor Number");
				case LabelsEnum.InvoiceAuthorisationRecordCounterLabel:
					return Res.GetString("46E21B3C-0B5E-4FDA-8E97-6CA2088D00EF", "Invoice Counter");
				case LabelsEnum.InvoiceAuthorisationRecordTimeLabel:
					return Res.GetString("9B093178-F80D-49EA-9199-0528213CB694", "SDC Time");
				case LabelsEnum.ComplianceSubtypeLabel:
					return Res.GetString("FDBA75B5-D4E9-4702-A977-7BF04D02F4B9", "Compliance Subtype");
				case LabelsEnum.EInvoicingGovernmentAllocatedNumberLabel:
					return Res.GetString("C356F128-4BCF-4021-A2B1-8856032B787A", "Government Allocated Number");
				case LabelsEnum.TaxRegimeInformationLabel:
					return Res.GetString("91D5C90A-012D-4E86-BFF3-2E3BCFBC4482", "Tax Regime");
				case LabelsEnum.GovernmentAgreedPaymentMethodLabel:
					return Res.GetString("77003C18-8A48-402A-AA6C-0172C45AAA6F", "Government Agreed Payment Method");
				case LabelsEnum.InvoiceAuthorizationRecordVerificationURLLabel:
					return Res.GetString("297F6315-E114-4DF7-BFF8-FD72A06D280C", "Authorization Record Verification URL");
				case LabelsEnum.SubjectToTaxLabel:
					return Res.GetString("7F45AEE2-A825-46F2-85E4-FF03710B884E", "Subject to Tax");
				case LabelsEnum.MeasurementUnitLabel:
					return Res.GetString("FCB4EB8F-FCB0-4192-A1D5-D2BC3E5CD4ED", "Measurement unit");
				case LabelsEnum.GovernmentReportingCodeLabel:
					return Res.GetString("8176A12F-53CD-4A4A-9B04-DD1DC2F62AEA", "Government charge code");
				case LabelsEnum.TaxBaseAmountLabel:
					return Res.GetString("7E3A3A8D-E9D5-4B9D-9DC3-1269ACA1C443", "Tax Base");
				case LabelsEnum.TaxLabel:
					return Res.GetString("32566324-6A1F-4BCF-BD22-315213506E12", "Tax");
				case LabelsEnum.TaxAmountLabel:
					return Res.GetString("2E092FA5-B04C-42AC-9050-06F81B357793", "Amount");
				case LabelsEnum.IVATaxLabel:
					return Res.GetString("0800B080-C872-4096-B0A0-90DB128727DF", "IVA");
				case LabelsEnum.RetentionTaxLabel:
					return Res.GetString("A17B8231-4CEF-4FE5-B78B-5D563CD2A5CD", "Retention");
				case LabelsEnum.RecipientConsumptionTaxRegimeHeading:
					return Res.GetString("FB881B7D-D564-43A1-A8D4-ABF0ADBC5045", "{0} REGIME", parameters);
				case LabelsEnum.DebtorTaxRegimeLabel:
					return Res.GetString("24B1E329-5305-4040-BB2A-548C8DD8ABF6", "Debtor's Tax Regime");
				default:
					throw new ArgumentException("Invalid parameter value", nameof(label));
			}
		}

		IAccountingCountryFactory AccountingCountryFactory => accountingCountryFactory ?? (accountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		IAccountingCountryFactory accountingCountryFactory;
	}
}
