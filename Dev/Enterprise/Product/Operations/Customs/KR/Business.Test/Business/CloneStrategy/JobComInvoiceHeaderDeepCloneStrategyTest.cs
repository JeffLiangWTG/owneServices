using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	class JobComInvoiceHeaderDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCloneForCOOCusSupportingInfo()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice.CriteriaForDeterminingCountryOfOrigin = CountryOfOriginDeterminationRuleCodeList.Codes.A;
			invoice.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			invoice.JZ_COOLabelLocation = CountryOfOriginLabelLocationCodeList.Codes.B;

			Factory.Save();

			var clonedInvoiceHeader = (JobComInvoiceHeader)new JobComInvoiceHeaderDeepCloneStrategy(invoice, CloneType.TemplateCopy).Clone();
			CombineAssertions(() =>
			{
				AssertEquals("JZ_RN_NKDefaultOrigin cloned", invoice.JZ_RN_NKDefaultOrigin, clonedInvoiceHeader.JZ_RN_NKDefaultOrigin);
				AssertEquals("CriteriaForDeterminingCountryOfOrigin cloned", invoice.CriteriaForDeterminingCountryOfOrigin, clonedInvoiceHeader.CriteriaForDeterminingCountryOfOrigin);
				AssertEquals("CertificateOfOriginIssueStatus cloned", invoice.CertificateOfOriginIssueStatus, clonedInvoiceHeader.CertificateOfOriginIssueStatus);
				AssertEquals("JZ_COOLabelLocation cloned", invoice.JZ_COOLabelLocation, clonedInvoiceHeader.JZ_COOLabelLocation);
			});
		}
	}
}
