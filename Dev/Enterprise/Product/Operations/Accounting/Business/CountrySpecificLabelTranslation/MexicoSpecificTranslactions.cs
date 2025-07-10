using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	internal static class MexicoSpecificTranslactions
	{
		internal static ZString? TranslateLabel(LabelsEnum? label)
		{
			var language = Res.CurrentLanguage;

			if (Res.IsEnglish(language))
			{
				return EnglishLabelSpecificTranslations(label);
			}
			else if (language == Core.SharedConstants.Languages.Spanish || language == Core.SharedConstants.Languages.SpanishLatin)
			{
				return SpanishLabelSpecificTranslations(label);
			}

			return null;
		}

		#region SuppressResourceStringsCheckRegion

		static ZString? EnglishLabelSpecificTranslations(LabelsEnum? label)
		{
			switch (label)
			{
				case LabelsEnum.InvoiceAuthorisationRecordAuthorisationDataLabel:
					return "SAT's Digital Seal";
				case LabelsEnum.InvoiceAuthorisationRecordDebtorNumberLabel:
					return "Use of the Tax Document";
				case LabelsEnum.InvoiceAuthorisationRecordIssuerAuthorizationDataLabel:
					return "Digital Seal";
				case LabelsEnum.GovernmentCreditTermsLabel:
					return "Payment Method";
				case LabelsEnum.InvoiceAuthorisationRecordIssuerCertificateIdentifierLabel:
					return "Digital Certificate Number";
				case LabelsEnum.InvoiceAuthorisationRecordCounterLabel:
					return "SAT's Certificate Number";
				case LabelsEnum.InvoiceAuthorisationRecordTimeLabel:
					return "Certification Date";
				case LabelsEnum.EInvoicingGovernmentAllocatedNumberLabel:
					return "Fiscal Number (UUID)";
				case LabelsEnum.TaxRegimeInformationLabel:
					return "Tax Regime";
				case LabelsEnum.GovernmentAgreedPaymentMethodLabel:
					return "Agreed Payment Type";
				case LabelsEnum.InvoiceAuthorizationRecordVerificationURLLabel:
					return "SAT's Digital Certification Original Chain";
				case LabelsEnum.MeasurementUnitLabel:
					return "Measurement unit: E48";
				default:
					return null;
			}
		}

		static ZString? SpanishLabelSpecificTranslations(LabelsEnum? label)
		{
			switch (label)
			{
				case LabelsEnum.InvoiceAuthorisationRecordAuthorisationDataLabel:
					return "Sello Digital del SAT";
				case LabelsEnum.InvoiceAuthorisationRecordDebtorNumberLabel:
					return "Uso del CFDI";
				case LabelsEnum.InvoiceAuthorisationRecordIssuerAuthorizationDataLabel:
					return "Sello Digital del CFDI";
				case LabelsEnum.GovernmentCreditTermsLabel:
					return "Método de Pago";
				case LabelsEnum.InvoiceAuthorisationRecordIssuerCertificateIdentifierLabel:
					return "Nro Certificado Digital";
				case LabelsEnum.InvoiceAuthorisationRecordPlaceOfIssueLabel:
					return "Lugar de expedición";
				case LabelsEnum.InvoiceAuthorisationRecordCounterLabel:
					return "Nro Certificado SAT";
				case LabelsEnum.InvoiceAuthorisationRecordTimeLabel:
					return "Fecha de Certificación";
				case LabelsEnum.ComplianceSubtypeLabel:
					return "Tipo de Comprobante";
				case LabelsEnum.EInvoicingGovernmentAllocatedNumberLabel:
					return "Folio Fiscal (UUID)";
				case LabelsEnum.TaxRegimeInformationLabel:
					return "Régimen Fiscal del Emisor";
				case LabelsEnum.GovernmentAgreedPaymentMethodLabel:
					return "Forma de Pago";
				case LabelsEnum.InvoiceAuthorizationRecordVerificationURLLabel:
					return "Cadena Original del Complemento de Certificación Digital del SAT";
				case LabelsEnum.SubjectToTaxLabel:
					return "Objeto Impuesto";
				case LabelsEnum.MeasurementUnitLabel:
					return "Clave Unidad: E48";
				case LabelsEnum.GovernmentReportingCodeLabel:
					return "Clave Producto o Servicio";
				case LabelsEnum.TaxBaseAmountLabel:
					return "Base";
				case LabelsEnum.TaxLabel:
					return "Impuesto";
				case LabelsEnum.TaxAmountLabel:
					return "Importe";
				case LabelsEnum.IVATaxLabel:
					return "Traslados";
				case LabelsEnum.RetentionTaxLabel:
					return "Retenciones";
				case LabelsEnum.DebtorTaxRegimeLabel:
					return "Régimen Fiscal del Receptor";
				default:
					return null;
			}
		}

		#endregion
	}
}
