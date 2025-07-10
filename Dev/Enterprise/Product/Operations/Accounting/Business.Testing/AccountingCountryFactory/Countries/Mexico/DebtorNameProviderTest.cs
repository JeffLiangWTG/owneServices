using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Mexico.Testing
{
	public class DebtorNameProviderTest : TestCaseWithFactory
	{
		public void TestGetDebtorNumber_With_AccTransactionHeaderAuthorisationRecord_Null()
		{
			var result = ((ObjectFactory.Get<IGlobalAccountingCountryFactory>()).GetCountryFactory(Core.Constants.CountryCodes.Mexico) as IDebtorNumberProvider);

			AssertEquals(ZString.Empty, result.GetDebtorName(null));
		}

		public void TestGetUsosCFDI_With_Valid_DebtorNumber()
		{
			var usosCFDI = new CodeDescriptionPairList();
			usosCFDI.AddPair("G03", "Gastos en general");

			var authorizationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_DebtorNumber = "G03";

			var mockIAccountingMasterFilesDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();

			mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetMexicoEInvoicingExtension().GetUsosCFDI()).Returns(usosCFDI);

			ObjectFactory.Substitute(mockIAccountingMasterFilesDependencyFactory.Object);

			var result = ((ObjectFactory.Get<IGlobalAccountingCountryFactory>()).GetCountryFactory(Core.Constants.CountryCodes.Mexico) as IDebtorNumberProvider);

			AssertEquals("G03 - Gastos en general", result.GetDebtorName(authorizationRecord));
		}

		public void TestGetUsosCFDI_With_InValidCode_DebtorNumber()
		{
			var usosCFDI = new CodeDescriptionPairList();
			usosCFDI.AddPair("G03", "Gastos en general");

			var authorizationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_DebtorNumber = "KKK";

			var mockIAccountingMasterFilesDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();

			mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetMexicoEInvoicingExtension().GetUsosCFDI()).Returns(usosCFDI);

			ObjectFactory.Substitute(mockIAccountingMasterFilesDependencyFactory.Object);

			var result = ((ObjectFactory.Get<IGlobalAccountingCountryFactory>()).GetCountryFactory(Core.Constants.CountryCodes.Mexico) as IDebtorNumberProvider);

			AssertEquals("KKK", result.GetDebtorName(authorizationRecord));
		}

		public void TestGetUsosCFDI_With_GetUsosCFDI_IsNull()
		{
			var authorizationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_DebtorNumber = "G03";

			var mockIAccountingMasterFilesDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();

			mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetMexicoEInvoicingExtension().GetUsosCFDI()).Returns((CodeDescriptionPairList)null);

			ObjectFactory.Substitute(mockIAccountingMasterFilesDependencyFactory.Object);

			var result = ((ObjectFactory.Get<IGlobalAccountingCountryFactory>()).GetCountryFactory(Core.Constants.CountryCodes.Mexico) as IDebtorNumberProvider);

			AssertEquals("G03", result.GetDebtorName(authorizationRecord));
		}

		[ExpectNoExceptions]
		public void TestGetDebtorNumberCaching()
		{
			var usosCFDI = new CodeDescriptionPairList();
			usosCFDI.AddPair("G03", "Gastos en general");

			var authorizationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_DebtorNumber = "G03";

			var mockIAccountingMasterFilesDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();

			mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetMexicoEInvoicingExtension().GetUsosCFDI()).Returns(usosCFDI);

			ObjectFactory.Substitute(mockIAccountingMasterFilesDependencyFactory.Object);

			var result = ((ObjectFactory.Get<IGlobalAccountingCountryFactory>()).GetCountryFactory(Core.Constants.CountryCodes.Mexico) as IDebtorNumberProvider);
			result.GetDebtorName(authorizationRecord);

			mockIAccountingMasterFilesDependencyFactory.Verify(x => x.GetMexicoEInvoicingExtension().GetUsosCFDI(), Times.Once);
			mockIAccountingMasterFilesDependencyFactory.Reset();

			result.GetDebtorName(authorizationRecord);

			mockIAccountingMasterFilesDependencyFactory.Verify(x => x.GetMexicoEInvoicingExtension().GetUsosCFDI(), Times.Never);
		}
	}
}
