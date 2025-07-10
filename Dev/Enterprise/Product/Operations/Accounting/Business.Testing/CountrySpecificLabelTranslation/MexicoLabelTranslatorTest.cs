using System.Collections.Generic;

namespace Enterprise.Accounting.Business.Testing
{
	public class MexicoLabelTranslatorTest : CountryLabelTranslatorTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Mexico;

		protected override Dictionary<LabelsEnum, (string[] languages, object[] parametersForTranslation, string translation)[]> ExpectedLabelTranslations =>
			new Dictionary<LabelsEnum, (string[] languages, object[] parametersForTranslation, string translation)[]>
			{
			{
				LabelsEnum.InvoiceAuthorisationRecordAuthorisationDataLabel,
				new []
				{
					(EnglishLanguages, EmptyParametersForTranslation, "SAT's Digital Seal"),
					(SpanishLanguages, EmptyParametersForTranslation, "Sello Digital del SAT"),
				}
			},
			{
				LabelsEnum.InvoiceAuthorisationRecordDebtorNumberLabel,
				new []
				{
					(EnglishLanguages, EmptyParametersForTranslation, "Use of the Tax Document"),
					(SpanishLanguages, EmptyParametersForTranslation, "Uso del CFDI"),
				}
			},
			{
				LabelsEnum.InvoiceAuthorisationRecordIssuerAuthorizationDataLabel,
				new []
				{
					(EnglishLanguages, EmptyParametersForTranslation, "Digital Seal"),
					(SpanishLanguages, EmptyParametersForTranslation, "Sello Digital del CFDI"),
				}
			},
			{
				LabelsEnum.InvoiceAuthorisationRecordIssuerCertificateIdentifierLabel,
				new []
				{
					(EnglishLanguages, EmptyParametersForTranslation, "Digital Certificate Number"),
					(SpanishLanguages, EmptyParametersForTranslation, "Nro Certificado Digital"),
				}
			},
			{
				LabelsEnum.GovernmentCreditTermsLabel,
				new []
				{
					(EnglishLanguages, EmptyParametersForTranslation, "Payment Method"),
					(SpanishLanguages, EmptyParametersForTranslation, "Método de Pago"),
				}
			},
			{
				LabelsEnum.InvoiceAuthorisationRecordPlaceOfIssueLabel,
				new []
				{
					(SpanishLanguages, EmptyParametersForTranslation, "Lugar de expedición"),
				}
			},
			{
				LabelsEnum.InvoiceAuthorisationRecordCounterLabel,
				new []
				{
					(EnglishLanguages, EmptyParametersForTranslation, "SAT's Certificate Number"),
					(SpanishLanguages, EmptyParametersForTranslation,"Nro Certificado SAT"),
				}
			},
			{
				LabelsEnum.InvoiceAuthorisationRecordTimeLabel,
				new []
				{
					(EnglishLanguages, EmptyParametersForTranslation, "Certification Date"),
					(SpanishLanguages, EmptyParametersForTranslation, "Fecha de Certificación"),
				}
			},
			{
				LabelsEnum.ComplianceSubtypeLabel,
				new []
				{
					(SpanishLanguages, EmptyParametersForTranslation, "Tipo de Comprobante"),
				}
			},
			{
				LabelsEnum.EInvoicingGovernmentAllocatedNumberLabel,
				new []
				{
					(EnglishLanguages, EmptyParametersForTranslation, "Fiscal Number (UUID)"),
					(SpanishLanguages, EmptyParametersForTranslation, "Folio Fiscal (UUID)"),
				}
			},
			{
				LabelsEnum.TaxRegimeInformationLabel,
				new []
				{
					(EnglishLanguages, EmptyParametersForTranslation, "Tax Regime"),
					(SpanishLanguages, EmptyParametersForTranslation, "Régimen Fiscal del Emisor"),
				}
			},
			{
				LabelsEnum.GovernmentAgreedPaymentMethodLabel,
				new []
				{
					(EnglishLanguages, EmptyParametersForTranslation, "Agreed Payment Type"),
					(SpanishLanguages, EmptyParametersForTranslation, "Forma de Pago"),
				}
			},
			{
				LabelsEnum.InvoiceAuthorizationRecordVerificationURLLabel,
				new []
				{
					(EnglishLanguages, EmptyParametersForTranslation, "SAT's Digital Certification Original Chain"),
					(SpanishLanguages, EmptyParametersForTranslation, "Cadena Original del Complemento de Certificación Digital del SAT"),
				}
			},
			{
				LabelsEnum.SubjectToTaxLabel,
				new []
				{
					(SpanishLanguages, EmptyParametersForTranslation, "Objeto Impuesto"),
				}
			},
			{
				LabelsEnum.MeasurementUnitLabel,
				new []
				{
					(EnglishLanguages, EmptyParametersForTranslation, "Measurement unit: E48"),
					(SpanishLanguages, EmptyParametersForTranslation, "Clave Unidad: E48"),
				}
			},
			{
				LabelsEnum.GovernmentReportingCodeLabel,
				new []
				{
					(SpanishLanguages, EmptyParametersForTranslation, "Clave Producto o Servicio"),
				}
			},
			{
				LabelsEnum.TaxBaseAmountLabel,
				new []
				{
					(SpanishLanguages, EmptyParametersForTranslation, "Base"),
				}
			},
			{
				LabelsEnum.TaxLabel,
				new []
				{
					(SpanishLanguages, EmptyParametersForTranslation, "Impuesto"),
				}
			},
			{
				LabelsEnum.TaxAmountLabel,
				new []
				{
					(SpanishLanguages, EmptyParametersForTranslation, "Importe"),
				}
			},
			{
				LabelsEnum.IVATaxLabel,
				new []
				{
					(SpanishLanguages, EmptyParametersForTranslation, "Traslados"),
				}
			},
			{
				LabelsEnum.RetentionTaxLabel,
				new []
				{
					(SpanishLanguages,EmptyParametersForTranslation, "Retenciones"),
				}
			},
			{
				LabelsEnum.DebtorTaxRegimeLabel,
				new []
				{
					(SpanishLanguages,EmptyParametersForTranslation, "Régimen Fiscal del Receptor"),
				}
			},
		};
	}
}
