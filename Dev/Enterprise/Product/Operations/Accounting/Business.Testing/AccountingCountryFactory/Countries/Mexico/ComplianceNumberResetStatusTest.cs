using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Mexico.Testing
{
	class ComplianceNumberResetStatusTest : TestCaseWithFactory
	{
		public void TestCheckIsComplianceNumberResetAllowedForSubmittedEInvoice()
		{
			var inputDataMock = new Mock<IComplianceNumberResetStatusInputData>(MockBehavior.Strict);

			var testObject = GetComplianceNumberResetStatusInstance();
			Assert(testObject.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(inputDataMock.Object));
		}

		IComplianceNumberResetStatus GetComplianceNumberResetStatusInstance() => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Mexico) as IInstanceProvider<IComplianceNumberResetStatus>).Get();
	}
}
