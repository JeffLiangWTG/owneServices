using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.India.Testing
{
	class ComplianceNumberResetStatusTest : TestCaseWithFactory
	{
		public void TestCheckIsComplianceNumberResetAllowedForSubmittedEInvoice_InvoiceDateAndCompanyPK_DefaultRegistryValue()
		{
			var companyPK = ZGuid.NewZGuid();
			var inputDataMock = new Mock<IComplianceNumberResetStatusInputData>();
			inputDataMock.Setup(x => x.CompanyPK).Returns(companyPK);

			var testObject = GetComplianceNumberResetStatusInstance();

			var pastDate = ZDate.Today.AddDays(-7);
			var futureDate = ZDate.Today.AddDays(3);

			inputDataMock.Setup(x => x.InvoiceDate).Returns(pastDate);
			Assert("Past date", testObject.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputDataMock.Object));

			inputDataMock.Setup(x => x.InvoiceDate).Returns(ZDate.Today);
			Assert("Today", testObject.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputDataMock.Object));

			inputDataMock.Setup(x => x.InvoiceDate).Returns(futureDate);
			Assert("Future date", testObject.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputDataMock.Object));
		}

		public void TestCheckIsComplianceNumberResetAllowedForSubmittedEInvoice_InvoiceDateAndCompanyPK_NonEmptyRegistryValue()
		{
			var companyPK = Guid.NewGuid();
			var inputDataMock = new Mock<IComplianceNumberResetStatusInputData>();
			inputDataMock.Setup(x => x.CompanyPK).Returns(companyPK);

			var testObject = GetComplianceNumberResetStatusInstance();

			var invoiceDate = ZDate.Today.AddDays(-14);
			var lessThenInvoiceDate = invoiceDate.AddDays(-1);
			var moreThenInvoiceDate = invoiceDate.AddDays(1);
			inputDataMock.Setup(x => x.InvoiceDate).Returns(invoiceDate);

			AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetValue(companyPK, Guid.Empty, Guid.Empty, lessThenInvoiceDate.ToDateTime());
			Assert("Less then invoice date", !testObject.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputDataMock.Object));

			AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetValue(companyPK, Guid.Empty, Guid.Empty, invoiceDate.ToDateTime());
			Assert("Equal invoice date", !testObject.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputDataMock.Object));

			AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.SetValue(companyPK, Guid.Empty, Guid.Empty, moreThenInvoiceDate.ToDateTime());
			Assert("More then invoice date", testObject.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputDataMock.Object));
		}

		IComplianceNumberResetStatus GetComplianceNumberResetStatusInstance() => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.India) as IInstanceProvider<IComplianceNumberResetStatus>).Get();
	}
}
