using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.Presentation.GUI;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Accounting.Business.Presentation.Testing.GUI
{
	public abstract class InvoiceFormPresentationProviderTest : TestCaseWithFactory
	{
		#region GetIsReversalStatusCodeVisible

		public void TestGetIsReversalStatusCodeVisible_WhenInvoiceIsNull()
		{
			var invoiceFormPresentationProvider = GetInvoiceFormPresentationProvider();

			var result = invoiceFormPresentationProvider.GetIsReversalStatusCodeVisible(null);

			AssertEquals(false, result);
		}

		public void TestGetIsReversalStatusCodeVisible_WhenInvoiceIsNotReversal()
		{
			var invoiceFormPresentationProvider = GetInvoiceFormPresentationProvider();

			var result = invoiceFormPresentationProvider.GetIsReversalStatusCodeVisible(GetOriginalTransaction());

			AssertEquals(false, result);
		}

		public void TestGetIsReversalStatusCodeVisible_WhenInvoiceIsReversal_AndReversalStatusCodeConfigurationIsNotImplemented()
		{
			var invoiceFormPresentationProvider = GetInvoiceFormPresentationProvider();

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			var result = invoiceFormPresentationProvider.GetIsReversalStatusCodeVisible(GetReversedTransaction());

			AssertEquals(false, result);
		}

		public void TestGetIsReversalStatusCodeVisible_WhenInvoiceIsReversal_AndReversalStatusCodeConfigurationReturnsFalse()
		{
			var invoiceFormPresentationProvider = GetInvoiceFormPresentationProvider();
			var mockReversalStatusCodeConfig = CreateMockReversalStatusCodeConfiguration(getIsReversalStatusCodeAllowedReturnValue: false);
			CreateMocksForGetIsReverseStatusCodeVisibleTests(mockReversalStatusCodeConfig.Object);

			var result = invoiceFormPresentationProvider.GetIsReversalStatusCodeVisible(GetReversedTransaction());

			AssertEquals(false, result);
		}

		public void TestGetIsReversalStatusCodeVisible_WhenInvoiceIsReversal_AndReversalStatusCodeConfigurationReturnsTrue()
		{
			var invoiceFormPresentationProvider = GetInvoiceFormPresentationProvider();
			var mockReversalStatusCodeConfig = CreateMockReversalStatusCodeConfiguration(getIsReversalStatusCodeAllowedReturnValue: true);
			CreateMocksForGetIsReverseStatusCodeVisibleTests(mockReversalStatusCodeConfig.Object);

			var result = invoiceFormPresentationProvider.GetIsReversalStatusCodeVisible(GetReversedTransaction());

			AssertEquals(true, result);
		}

		Mock<IReversalStatusCodeConfiguration> CreateMockReversalStatusCodeConfiguration(bool getIsReversalStatusCodeAllowedReturnValue)
		{
			var mock = new Mock<IReversalStatusCodeConfiguration>();
			mock.Setup(x => x.GetIsReversalStatusCodeAllowed(It.IsAny<ZString>())).Returns(getIsReversalStatusCodeAllowedReturnValue);
			return mock;
		}

		void CreateMocksForGetIsReverseStatusCodeVisibleTests(IReversalStatusCodeConfiguration config)
		{
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get()).Returns(config);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);
		}

		public void TestGetIsReversalStatusCodeVisible_GetCountryFactory_MustBeCalledWith_TransactionCountryCode()
		{
			var invoiceFormPresentationProvider = GetInvoiceFormPresentationProvider();
			var expectedCountryCode = "MX";
			var invoicingBase = GetReversedTransaction();
			invoicingBase.Company.GC_RN_NKCountryCode = expectedCountryCode;

			AssertNotEquals("Precondition, current login country and invoice's country must be different", invoicingBase.Company.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Code);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>()));
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			_ = invoiceFormPresentationProvider.GetIsReversalStatusCodeVisible(invoicingBase);

			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(expectedCountryCode));
		}

		public void TestGetIsReversalStatusCodeVisible_GetIsReversalStatusCodeAllowed_UseInvoiceLedger()
		{
			var invoiceFormPresentationProvider = GetInvoiceFormPresentationProvider();
			var invoicingBase = GetReversedTransaction();

			var mockReversalStatusCodeConfiguration = new Mock<IReversalStatusCodeConfiguration>();
			CreateMocksForGetIsReverseStatusCodeVisibleTests(mockReversalStatusCodeConfiguration.Object);

			_ = invoiceFormPresentationProvider.GetIsReversalStatusCodeVisible(invoicingBase);

			mockReversalStatusCodeConfiguration.Verify(x => x.GetIsReversalStatusCodeAllowed((invoicingBase as ITransaction).Ledger), "GetIsReversalStatusCodeAllowed must use invoice Ledger");
		}

		#endregion GetIsReversalStatusCodeVisible

		public void TestIsSupplyTypeColumnVisible()
		{
			var invoiceFormPresentationProvider = GetInvoiceFormPresentationProvider();

			AssertEquals("EnableSupplyTypeClassificationCodes Default value", false, AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value);
			AssertEquals("EnableSupplyTypeClassificationCodes registry is false", false, invoiceFormPresentationProvider.IsSupplyTypeColumnVisible());

			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("EnableSupplyTypeClassificationCodes registry is true", true, invoiceFormPresentationProvider.IsSupplyTypeColumnVisible());
		}

		public void TestIsTaxBranchColumnVisible()
		{
			var invoiceFormPresentationProvider = GetInvoiceFormPresentationProvider();

			AssertEquals("EnableTaxBranchReporting Default value", false, AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value);
			AssertEquals("EnableTaxBranchReporting registry is false", false, invoiceFormPresentationProvider.IsTaxBranchColumnVisible());

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("EnableTaxBranchReporting registry is true", true, invoiceFormPresentationProvider.IsTaxBranchColumnVisible());
		}

		public void TestIsEInvoicingColumnsAvailable()
		{
			var electronicInvoicingAccountingObjectFactoryMock = new Mock<IElectronicInvoicingAccountingObjectFactory>();
			var eInvoicingConfigurationChecksMock = new Mock<IEInvoicingConfigurationChecks>();

			electronicInvoicingAccountingObjectFactoryMock.Setup(x => x.GetEInvoicingConfigurationChecks()).Returns(eInvoicingConfigurationChecksMock.Object);
			ObjectFactory.Substitute(electronicInvoicingAccountingObjectFactoryMock.Object);

			IInvoiceFormPresentationProvider provider = GetInvoiceFormPresentationProvider();

			AssertEInvoicingColumnsAvailable(LedgerTypes.AccountsPayable, true);
			AssertEInvoicingColumnsAvailable(LedgerTypes.AccountsPayable, false);
			AssertEInvoicingColumnsAvailable(LedgerTypes.AccountsReceivable, true);
			AssertEInvoicingColumnsAvailable(LedgerTypes.AccountsReceivable, false);

			void AssertEInvoicingColumnsAvailable(ZString ledgerType, bool expectedResult)
			{
				eInvoicingConfigurationChecksMock.Setup(x => x.IsEInvoicingSupportedForCurrentCountry(ledgerType)).Returns(expectedResult);

				var actualResult = provider.IsEInvoicingColumnsAvailable(ledgerType);

				AssertEquals(expectedResult, actualResult);
			}
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		InvoicingBase GetReversedTransaction()
		{
			var originalTransaction = GetOriginalTransaction();
			var reversedTransaction = TestObjectCreator.ReverseTransaction(originalTransaction, out string cantReverseErrorMessage) as InvoicingBase;
			Assert("Precondition: transaction is reversed without errors", string.IsNullOrEmpty(cantReverseErrorMessage));
			AssertType("Precondition: test transaction has correct type", HeaderType, reversedTransaction);
			AssertEquals("Precondition: test transaction has correct ledger", TransactionLedger, reversedTransaction.AH_Ledger);

			return reversedTransaction;
		}

		protected abstract InvoicingBase GetOriginalTransaction();

		protected abstract ZString TransactionLedger { get; }

		protected abstract Type HeaderType { get; }

		static IInvoiceFormPresentationProvider GetInvoiceFormPresentationProvider()
		{
			return ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetInvoiceFormPresentationProvider();
		}
	}
}
