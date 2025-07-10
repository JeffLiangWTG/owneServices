using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Italy.Testing
{
	class ComplianceNumberResetStatusTest : TestCaseWithFactory
	{
		public void TestCheckIsComplianceNumberResetAllowedForSubmittedEInvoice()
		{
			AssertContainsExactElementsInAnyOrder("Precondition: EReportingTransactionNumber contains only values tested here. If not this test should be changed to test all of them.",
				new[] { EReportingTransactionNumberOptions.InvoiceNr.Code, EReportingTransactionNumberOptions.ComplianceOrInvoiceNr.Code, EReportingTransactionNumberOptions.ComplianceNr.Code },
				new EReportingTransactionNumberOptions().GetAllCodes());

			var inputDataMock = new Mock<IComplianceNumberResetStatusInputData>(MockBehavior.Strict);

			var testObject = GetComplianceNumberResetStatusInstance();

			AccountingMasterFilesRegistry.Instance.EReportingTransactionNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EReportingTransactionNumberOptions.InvoiceNr.Code);
			Assert("EReportingTransactionNumber registry is set to INV", testObject.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputDataMock.Object));

			AccountingMasterFilesRegistry.Instance.EReportingTransactionNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EReportingTransactionNumberOptions.ComplianceNr.Code);
			Assert("EReportingTransactionNumber registry is set to COM", !testObject.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputDataMock.Object));

			AccountingMasterFilesRegistry.Instance.EReportingTransactionNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EReportingTransactionNumberOptions.ComplianceOrInvoiceNr.Code);
			Assert("EReportingTransactionNumber registry is set to CIN", !testObject.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputDataMock.Object));
		}

		IComplianceNumberResetStatus GetComplianceNumberResetStatusInstance() => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Italy) as IInstanceProvider<IComplianceNumberResetStatus>).Get();
	}
}
