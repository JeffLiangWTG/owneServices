using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ExportEntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestGetKeyForHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = new CargoWise.Types.ZDateTime(2010, 2, 10);
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			var strategy = new ExportEntryCreationStrategy(declaration);
			var keys = strategy.GetKeyForHeader(invoiceLine).Keys;

			CombineAssertions("MergeKeys for Headers", () =>
			{
				Assert("EffectiveValuationDate", !keys.Contains(invoice.EffectiveValuationDate));

				Assert("JI_CEI", keys.Contains(invoiceLine.JI_CEI));
				Assert("JZ_IncoTerm", keys.Contains(invoice.JZ_IncoTerm));
				Assert("JZ_RX_NKInvoice_Currency", keys.Contains(invoice.JZ_RX_NKInvoice_Currency));
				Assert("JZ_PaymentTerms", keys.Contains(invoice.JZ_PaymentTerms));
				Assert("JZ_LetterOfCreditNumber", keys.Contains(invoice.JZ_LetterOfCreditNumber));

				Assert("JZ_OA_ManufacturerAddress", keys.Contains(invoice.JZ_OA_ManufacturerAddress));
				Assert("JZ_OH_Buyer", keys.Contains(invoice.JZ_OH_Buyer));
				Assert("JZ_ImportCargoManagementNumber", keys.Contains(invoice.JZ_ImportCargoManagementNumber));
				Assert("JZ_DRWApplicantType", keys.Contains(invoice.JZ_DRWApplicantType));
			});
		}

		public void TestMergeKeyForLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Model = "X1";
			invoiceLine.JI_BrandName = "JACO";
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_PreviousEntryNumber = "ENT123";
			invoiceLine.JI_PreviousEntryLineNumber = 11;
			invoiceLine.JI_PackType = "CT";
			invoiceLine.JI_COOLabelLocation = "G";
			invoiceLine.JI_SkipManifestReport = "Y";
			var preApproval = invoiceLine.PreApprovalCollection.AddNew();
			preApproval.CSI_Procedure = "A";
			preApproval.CSI_ReferenceNumber = "PRE123";
			invoiceLine.CertificateOfOriginIssueStatus = "B";
			invoiceLine.CertificateOfOriginData.CSI_SubType = "G";
			var strategy = new ExportEntryCreationStrategy(declaration);
			var keys = strategy.GetKeyForLine(invoiceLine).Keys;

			CombineAssertions("MergeKeys for Lines", () =>
			{
				Assert("JI_Model", keys.Contains(invoiceLine.JI_Model));
				Assert("JI_BrandName", keys.Contains(invoiceLine.JI_BrandName));
				Assert("JI_CountryOfOrigin", keys.Contains(invoiceLine.JI_CountryOfOrigin));
				Assert("JI_PreviousEntryNumber", keys.Contains(invoiceLine.JI_PreviousEntryNumber));
				Assert("JI_PreviousEntryLineNo", keys.Contains(invoiceLine.JI_PreviousEntryLineNumber));
				Assert("JI_PackType", keys.Contains(invoiceLine.JI_PackType));
				Assert("JI_JZ", keys.Contains(invoiceLine.JI_JZ));

				Assert("JI_COOLabelLocation", keys.Contains(invoiceLine.JI_COOLabelLocation));
				Assert("JI_SkipManifestReport", keys.Contains(invoiceLine.JI_SkipManifestReport));

				Assert("CSI_ReferenceNumber", keys.Contains(preApproval.CSI_ReferenceNumber));
				Assert("CSI_Code", keys.Contains(invoiceLine.CertificateOfOriginData.CSI_Code));
				Assert("CSI_SubType", keys.Contains(invoiceLine.CertificateOfOriginData.CSI_SubType));
			});
		}
	}
}
