using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	sealed class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestTypedIndexer()
		{
			var collection = new InvoiceLineCompleteCollection((JobDeclaration)base.Declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		public void TestSetDefaultForCommonInvoiceLine()
		{
			var invoiceHeader = (JobComInvoiceHeader)Declaration.Invoices.AddNew();
			var emptyInvoiceLine = (JobComInvoiceLine)Declaration.InvoiceLines.AddNew();
			AssertEquals(ZString.Empty, emptyInvoiceLine.CertificateOfOriginIssueStatus);
			AssertEquals(ZString.Empty, emptyInvoiceLine.CriteriaForDeterminingCountryOfOrigin);
			AssertEquals(ZString.Empty, emptyInvoiceLine.SupportingDocumentCode);
			AssertEquals(ZString.Empty, emptyInvoiceLine.SupportingDocumentReferenceNumber);
			AssertEquals(ZDateTime.Empty, emptyInvoiceLine.JI_InboundDate);
			AssertEquals(ZString.Empty, emptyInvoiceLine.JI_CountryOfOrigin);
			AssertEquals(ZString.Empty, emptyInvoiceLine.JI_COOLabelLocation);
			AssertEquals(ZString.Empty, emptyInvoiceLine.JI_COOLabelType);
			AssertEquals(ZString.Empty, emptyInvoiceLine.JI_COOExemptionReason);
			AssertEquals(ZString.Empty, emptyInvoiceLine.CertificateOfOriginIssuingCountry);
			AssertEquals(ZDateTime.Empty, emptyInvoiceLine.CertificateOfOriginIssueDate);
			AssertEquals(ZString.Empty, emptyInvoiceLine.CertificateOfOriginNo);
			AssertEquals(ZString.Empty, emptyInvoiceLine.CertificateOfOriginCriteriaCode);
			AssertEquals(ZString.Empty, emptyInvoiceLine.CertificateOfOriginAgencyName);
			AssertEquals(ZString.Empty, emptyInvoiceLine.CertificateOfOriginAreaName);
			AssertEquals(ZString.Empty, emptyInvoiceLine.CertificateOfOriginPersonName);
			AssertEquals(ZString.Empty, emptyInvoiceLine.CertificateOfOriginStatus);

			invoiceHeader.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.N;
			invoiceHeader.CriteriaForDeterminingCountryOfOrigin = CountryOfOriginDeterminationRuleCodeList.Codes._8;
			invoiceHeader.SupportingDocumentCode = RequirementDocumentTypeCodeList.Codes.C;
			invoiceHeader.SupportingDocumentReferenceNumber = "ZZZZ000011111";
			invoiceHeader.JZ_InboundDate = ZDateTime.Today;
			invoiceHeader.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.KoreaSouth;
			invoiceHeader.JZ_COOLabelLocation = CountryOfOriginLabelLocationCodeList.Codes.B;
			invoiceHeader.JZ_COOLabelType = CountryOfOriginLabelTypeCodeList.Codes.A;
			invoiceHeader.JZ_COOExemptionReason = CountryOfOriginExemptionReasonCodeList.Codes._12;
			invoiceHeader.CertificateOfOriginIssuingCountry = Core.Constants.CountryCodes.UnitedStates;
			invoiceHeader.CertificateOfOriginIssueDate = new ZDateTime(2024, 05, 24);
			invoiceHeader.CertificateOfOriginNo = "800324356053";
			invoiceHeader.CertificateOfOriginCriteriaCode = CountryOfOriginDeterminationRuleCodeList.Codes.A;
			invoiceHeader.CertificateOfOriginAgencyName = "발행기관명";
			invoiceHeader.CertificateOfOriginAreaName = "발급지역명";
			invoiceHeader.CertificateOfOriginPersonName = "발급담당자명";
			invoiceHeader.CertificateOfOriginStatus = Constants.YesNo.Yes;

			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertExport();

			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertLocalExport();

			Declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertImport();
		}
		void AssertExport()
		{
			var invoiceLine = (JobComInvoiceLine)Declaration.InvoiceLines.AddNew();
			AssertEquals(CertificateOfOriginIssuedCodeList.Codes.N, invoiceLine.CertificateOfOriginIssueStatus);
			AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes._8, invoiceLine.CriteriaForDeterminingCountryOfOrigin);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.SupportingDocumentCode);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.SupportingDocumentReferenceNumber);
			AssertEquals("When message type is 'EXP', not used this column.", ZDateTime.Empty, invoiceLine.JI_InboundDate);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.JI_COOLabelLocation);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.JI_COOLabelType);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.JI_COOExemptionReason);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginIssuingCountry);
			AssertEquals("When message type is 'EXP', not used this column.", ZDateTime.Empty, invoiceLine.CertificateOfOriginIssueDate);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginNo);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginCriteriaCode);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginAgencyName);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginAreaName);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginPersonName);
			AssertEquals("When message type is 'EXP', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginStatus);
		}

		void AssertLocalExport()
		{
			var invoiceLine = (JobComInvoiceLine)Declaration.InvoiceLines.AddNew();
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginIssueStatus);
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.CriteriaForDeterminingCountryOfOrigin);
			AssertEquals(RequirementDocumentTypeCodeList.Codes.C, invoiceLine.SupportingDocumentCode);
			AssertEquals("ZZZZ000011111", invoiceLine.SupportingDocumentReferenceNumber);
			AssertEquals(ZDateTime.Today, invoiceLine.JI_InboundDate);
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.JI_COOLabelLocation);
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.JI_COOLabelType);
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.JI_COOExemptionReason);
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginIssuingCountry);
			AssertEquals("When message type is 'LEX', not used this column.", ZDateTime.Empty, invoiceLine.CertificateOfOriginIssueDate);
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginNo);
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginCriteriaCode);
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginAgencyName);
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginAreaName);
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginPersonName);
			AssertEquals("When message type is 'LEX', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginStatus);
		}

		void AssertImport()
		{
			var invoiceLine = (JobComInvoiceLine)Declaration.InvoiceLines.AddNew();
			AssertEquals("When message type is 'IMP', not used this column.", ZString.Empty, invoiceLine.CertificateOfOriginIssueStatus);
			AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes._8, invoiceLine.CriteriaForDeterminingCountryOfOrigin);
			AssertEquals("When message type is 'IMP', not used this column.", ZString.Empty, invoiceLine.SupportingDocumentCode);
			AssertEquals("When message type is 'IMP', not used this column.", ZString.Empty, invoiceLine.SupportingDocumentReferenceNumber);
			AssertEquals("When message type is 'IMP', not used this column.", ZDateTime.Empty, invoiceLine.JI_InboundDate);
			AssertEquals(Core.Constants.CountryCodes.KoreaSouth, invoiceLine.JI_CountryOfOrigin);
			AssertEquals(CountryOfOriginLabelLocationCodeList.Codes.B, invoiceLine.JI_COOLabelLocation);
			AssertEquals(CountryOfOriginLabelTypeCodeList.Codes.A, invoiceLine.JI_COOLabelType);
			AssertEquals(CountryOfOriginExemptionReasonCodeList.Codes._12, invoiceLine.JI_COOExemptionReason);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, invoiceLine.CertificateOfOriginIssuingCountry);
			AssertEquals(new ZDateTime(2024, 05, 24), invoiceLine.CertificateOfOriginIssueDate);
			AssertEquals("800324356053", invoiceLine.CertificateOfOriginNo);
			AssertEquals(CountryOfOriginDeterminationRuleCodeList.Codes.A, invoiceLine.CertificateOfOriginCriteriaCode);
			AssertEquals("발행기관명", invoiceLine.CertificateOfOriginAgencyName);
			AssertEquals("발급지역명", invoiceLine.CertificateOfOriginAreaName);
			AssertEquals("발급담당자명", invoiceLine.CertificateOfOriginPersonName);
			AssertEquals(Constants.YesNo.Yes, invoiceLine.CertificateOfOriginStatus);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceLineCompleteCollection((JobDeclaration)base.Declaration);
		}

		protected override BaseJobDeclaration GetMeANewJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
