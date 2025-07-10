using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public partial class InvoicingBaseTest
	{
		public void TestRefreshBinding_ForReversalStatusCodeInfo()
		{
			var count = 0;
			var invoicingBase = GetTestingInvoice();
			invoicingBase.ReversalStatusCodeInfo.ValueChanged += delegate { count++; };

			AssertEquals("Precondition, RefreshBinding was not called", 0, count);

			invoicingBase.ReversalStatusCode = "01";
			AssertEquals("RefreshBinding must have been called", 1, count);
		}

		public void TestReversalStatusCode_UseTransactionHeaderReference_WhenIReversalStatusCodeConfigurationProvider_UsesGlobalImplementation()
		{
			var invoicingBase = GetTestingInvoice();
			Factory.Save();

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			AssertEquals("", invoicingBase.ReversalStatusCode);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(ExpectedGenericCountryCode), Times.Once);

			invoicingBase.ReversalStatusCode = "02";
			AssertEquals("02", invoicingBase.ReversalStatusCode);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(ExpectedGenericCountryCode));
		}

		public void TestReversalStatusCode_UseTransactionHeaderReference_WhenIReversalStatusCodeConfigurationProvider_IsImplemented()
		{
			var invoicingBase = GetTestingInvoice();
			var result = Factory.New<AccTransactionHeaderReference>();
			result.AH1_AH = invoicingBase.PK;
			result.AH1_Type = "AAA";
			result.AH1_Reference = "ReasonCode";

			Factory.Save();

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns("KKK");
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			AssertEquals("", invoicingBase.ReversalStatusCode);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetReversalStatusCodeReferenceType(), Times.Once);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(ExpectedGenericCountryCode), Times.Once);

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns("AAA");

			AssertEquals("ReasonCode", invoicingBase.ReversalStatusCode);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetReversalStatusCodeReferenceType(), Times.Exactly(2));
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(ExpectedGenericCountryCode), Times.Exactly(2));
		}

		public void TestReversalStatusCode_UseTransactionHeaderReference_WhenIReversalStatusCodeConfigurationProvider_IsImplemented_AndReferenceTypeIsNull()
		{
			var invoicingBase = GetTestingInvoice();
			Factory.Save();

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns((string)null);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			AssertEquals("", invoicingBase.ReversalStatusCode);

			invoicingBase.ReversalStatusCode = "02";
			AssertEquals("", invoicingBase.ReversalStatusCode);
		}

		public void TestReversalStatusCode_UseTransactionHeaderReference_WhenInvoice_IsNotInDB()
		{
			var invoicingBase = GetTestingInvoice();
			var result = Factory.New<AccTransactionHeaderReference>();
			result.AH1_AH = invoicingBase.PK;
			result.AH1_Type = "AAA";
			result.AH1_Reference = "ReasonCode";

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns("AAA");
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			AssertEquals("Invoice is not in database", "", invoicingBase.ReversalStatusCode);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetReversalStatusCodeReferenceType(), Times.Never);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(ExpectedGenericCountryCode), Times.Never);

			Factory.Save();
			AssertEquals("Invoice is in database", "ReasonCode", invoicingBase.ReversalStatusCode);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetReversalStatusCodeReferenceType(), Times.Once);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(ExpectedGenericCountryCode), Times.AtLeastOnce);
		}

		public void TestReversalStatusCode_CreateTransactionHeaderReference_WhenIReversalStatusCodeConfigurationProvider_UsesGlobalImplementation()
		{
			var invoicingBase = GetTestingInvoice();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			invoicingBase.ReversalStatusCode = "02";
			var references = GetTransactionHeaderReference(Factory, invoicingBase);

			AssertEquals(1, references.Length);
			var reference = references[0];
			AssertEquals("02", reference.AH1_Reference);
			AssertEquals("ERC", reference.AH1_Type);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(ExpectedGenericCountryCode));
		}

		public void TestReversalStatusCode_CreateTransactionHeaderReference_WhenIReversalStatusCodeConfigurationProvider_IsImplemented()
		{
			var invoicingBase = GetTestingInvoice();
			var references = GetTransactionHeaderReference(Factory, invoicingBase);
			AssertEquals("Precondition, TransactionHeaderReference must not exist ", 0, references.Length);

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns("EEE");
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			invoicingBase.ReversalStatusCode = ZString.Empty;
			references = GetTransactionHeaderReference(Factory, invoicingBase);

			AssertEquals(0, references.Length);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(ExpectedGenericCountryCode), Times.Never);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetReversalStatusCodeReferenceType(), Times.Never);

			invoicingBase.ReversalStatusCode = "01";
			references = GetTransactionHeaderReference(Factory, invoicingBase);
			AssertEquals(1, references.Length);

			var reference = references[0];
			AssertEquals("01", reference.AH1_Reference);
			AssertEquals("EEE", reference.AH1_Type);
			AssertEquals(invoicingBase.PK, reference.AH1_AH);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(ExpectedGenericCountryCode), Times.Exactly(2));
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetReversalStatusCodeReferenceType(), Times.Exactly(2));
		}

		public void TestReversalStatusCode_CreateTransactionHeaderReference_WhenIReversalStatusCodeConfigurationProvider_IsImplemented_AndReferenceTypeIsNull()
		{
			var invoicingBase = GetTestingInvoice();
			var references = GetTransactionHeaderReference(Factory, invoicingBase);
			AssertEquals("Precondition, TransactionHeaderReference must not exist ", 0, references.Length);

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns((ZString)null);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			invoicingBase.ReversalStatusCode = ZString.Empty;
			references = GetTransactionHeaderReference(Factory, invoicingBase);
			AssertEquals(0, references.Length);

			invoicingBase.ReversalStatusCode = "01";
			references = GetTransactionHeaderReference(Factory, invoicingBase);
			AssertEquals(0, references.Length);
		}

		#region ReversalStatusCode

		#region TransactionHeaderTest

		public override void TestReversalStatusCode_ShouldBeEmpty()
		{
			Assert("Not Applicable, specific tests are implemented in this class", true);
		}

		public override void TestReversalStatusCode_ReadOnly_ShouldBeTrue()
		{
			Assert("Not Applicable, specific tests are implemented in this class", true);
		}

		public override void TestReversalStatusCodeList_ShouldBeNull()
		{
			Assert("Not Applicable, specific tests are implemented in this class", true);
		}

		#endregion TransactionHeaderTest

		public void TestReversalStatusCode_ReadOnly_IReversalStatusCodeConfigurationProvider_IsNotImplemented()
		{
			CreateDependencyMocks_WhenIReversalStatusCodeConfigurationProvider_IsNotImplemented();

			var invoicingBase = GetTestingInvoice() as ITransaction;

			AssertEquals("ReversalStatusCode_ReadOnly should be true", true, invoicingBase.ReversalStatusCode_ReadOnly);
		}

		public void TestReversalStatusCode_ReadOnly_IReversalStatusCodeConfigurationProvider_IsImplemented()
		{
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			AssertReversalStatusCode_ReadOnly(true, false);
			AssertReversalStatusCode_ReadOnly(false, true);

			void AssertReversalStatusCode_ReadOnly(bool isReversalStatusCodeAllowed, bool expectedReadOnly)
			{
				mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetIsReversalStatusCodeAllowed(It.IsAny<ZString>())).Returns(isReversalStatusCodeAllowed);
				mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

				var invoicingBase = GetTestingInvoice() as ITransaction;
				AssertEquals($"ReversalStatusCode_ReadOnly should be {expectedReadOnly}.", expectedReadOnly, invoicingBase.ReversalStatusCode_ReadOnly);
			}
		}

		public void TestReversalStatusCodeList_IReversalStatusCodeConfigurationProvider_IsNotImplemented()
		{
			CreateDependencyMocks_WhenIReversalStatusCodeConfigurationProvider_IsNotImplemented();

			var invoicingBase = GetTestingInvoice() as ITransaction;
			var reversalStatusCodeList = invoicingBase.ReversalStatusCodeList;

			AssertEquals("ReversalStatusCodeList should be an empty list", 0, reversalStatusCodeList.Count);
		}

		public void TestReversalStatusCodeList_IInstanceProvider_IReversalStatusCodeConfiguration_IsImplemented()
		{
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("02", "Mock reversal code One");
			expectedList.AddPair("03", "Mock reversal code Two");

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeLookup()).Returns(expectedList);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			var invoicingBase = GetTestingInvoice() as ITransaction;
			var reversalStatusCodeList = invoicingBase.ReversalStatusCodeList;

			AssertContainsExactElementsInAnyOrder("ReversalStatusCodeList should has elements", expectedList, reversalStatusCodeList);
		}

		#endregion ReversalStatusCode

		public void TestReversalStatusCode_DeleteTransactionHeaderReference()
		{
			var invoicingBase = GetTestingInvoice();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns("EEE");
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			invoicingBase.ReversalStatusCode = "01";
			var references = GetTransactionHeaderReference(Factory, invoicingBase);
			AssertEquals("Precondition, must be Reference in DB", 1, references.Length);

			invoicingBase.Delete();
			references = GetTransactionHeaderReference(Factory, invoicingBase);
			AssertEquals("References must be delated when invoice is deleted", 0, references.Length);
		}

		#region Properties that must be called with the transaction's country code

		public void TestReversalStatusCode_GetCountryFactory_MustBeCalledWith_TransactionCountryCode()
		{
			AssertGetCountryFactory_MustBeCalledWith_TransactionCountryCode(x => x.ReversalStatusCode = "01");
		}

		public void TestIsReversalStatusCodeAllowed_CountryFactory_MustBeCalledWith_TransactionCountryCode()
		{
			AssertGetCountryFactory_MustBeCalledWith_TransactionCountryCode(x => _ = x.IsReversalStatusCodeAllowed);
		}

		public void TestReversalStatusCode_ReadOnly_CountryFactory_MustBeCalledWith_TransactionCountryCode()
		{
			AssertGetCountryFactory_MustBeCalledWith_TransactionCountryCode(x => _ = (x as ITransaction).ReversalStatusCode_ReadOnly);
		}

		void AssertGetCountryFactory_MustBeCalledWith_TransactionCountryCode(Action<InvoicingBase> propertyToTest)
		{
			var expectedCountryCode = "MX";
			var invoicingBase = GetTestingInvoice();
			invoicingBase.Company.GC_RN_NKCountryCode = expectedCountryCode;

			AssertNotEquals("Precondition, current login country and invoice's country must be different", invoicingBase.Company.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Code);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>()));
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			propertyToTest(invoicingBase);
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(expectedCountryCode));
		}

		#endregion Properties that must be called with the transaction's country code

		#region IsReversalStatusCodeAllowed

		public void TestIsReversalStatusCodeAllowed_When_IReversalStatusCodeConfigurationProvider_IsNotImplemented_And_GenericConditionsAreSatisfied()
		{
			CreateDependencyMocks_WhenIReversalStatusCodeConfigurationProvider_IsNotImplemented();
			var codes = new CodeDescriptionPairList();
			codes.AddPair("01", "First test value");

			var invoicingBase = GetTestingInvoice();

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, codes))
			{
				var expectedResult = invoicingBase.AH_Ledger == "AR";
				AssertEquals(expectedResult, invoicingBase.IsReversalStatusCodeAllowed);
			}
		}

		public void TestIsReversalStatusCodeAllowed_When_IReversalStatusCodeConfigurationProvider_IsNotImplemented_And_GenericConditionsAreNotSatisfied()
		{
			CreateDependencyMocks_WhenIReversalStatusCodeConfigurationProvider_IsNotImplemented();

			var invoicingBase = GetTestingInvoice();

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("IsReversalStatusCodeAllowed should be false", false, invoicingBase.IsReversalStatusCodeAllowed);
			}
		}

		public void TestIsReversalStatusCodeAllowed_DependsOnGetReversalStatusCodeAllowed()
		{
			var (_, mockIReversalStatusCodeConfiguration) = CreateDependencyMocks_WhenIReversalStatusCodeConfiguration_IsImplemented();

			AssertReversalStatusCodeAllowed(true);
			AssertReversalStatusCodeAllowed(false);

			void AssertReversalStatusCodeAllowed(bool getReversalStatusCodeAllowed)
			{
				mockIReversalStatusCodeConfiguration.Setup(x => x.GetIsReversalStatusCodeAllowed(It.IsAny<ZString>()))
					.Returns(getReversalStatusCodeAllowed);

				var invoicingBase = GetTestingInvoice();
				var result = invoicingBase.IsReversalStatusCodeAllowed;

				AssertEquals($"When GetReversalStatusCodeAllowed returns ${getReversalStatusCodeAllowed}, IsReversalStatusCodeAllowed should also be ${getReversalStatusCodeAllowed}.",
					getReversalStatusCodeAllowed, result);
			}
		}

		#endregion IsReversalStatusCodeAllowed

		#region Properties which must call GetIsReversalStatusCodeAllowed with the invoice's ledger

		[ExpectNoExceptions]
		public void TestIsReversalStatusCodeAllowed_GetIsReversalStatusCodeAllowed_UseInvoiceLedger()
		{
			AssertGetReversalStatusCodeAllowed_UseInvoiceLedger(x => _ = x.IsReversalStatusCodeAllowed);
		}

		[ExpectNoExceptions]
		public void TestReversalStatusCode_ReadOnly_GetIsReversalStatusCodeAllowed_UseInvoiceLedger()
		{
			AssertGetReversalStatusCodeAllowed_UseInvoiceLedger(x => _ = (x as ITransaction).ReversalStatusCode_ReadOnly);
		}

		void AssertGetReversalStatusCodeAllowed_UseInvoiceLedger(Action<InvoicingBase> propertyToTest)
		{
			var (mockIAccountingCountryFactory, _) = CreateDependencyMocks_WhenIReversalStatusCodeConfiguration_IsImplemented();

			var invoicingBase = GetTestingInvoice();
			propertyToTest(invoicingBase);

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>()
				.Verify(x => x.Get().GetIsReversalStatusCodeAllowed((invoicingBase as ITransaction).Ledger), "GetIsReversalStatusCodeAllowed must use invoice Ledger");
		}

		#endregion Properties which must call GetIsReversalStatusCodeAllowed with the invoice's ledger

		[ExpectNoExceptions]
		public override void TestBizObjectFields()
		{
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns("EEE");
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			base.TestBizObjectFields();
		}

		AccTransactionHeaderReference[] GetTransactionHeaderReference(BusinessObjectFactory factory, InvoicingBase invoicingBase)
		{
			var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_AH, invoicingBase.PK);
			return factory.Load<AccTransactionHeaderReference>(query);
		}

		InvoicingBase GetTestingInvoice()
		{
			var invoiceBase = TestObjectCreator.CreateInvoice(GetExpectedBusinessObjectType());
			invoiceBase.AH_TransactionNum = "1111";

			return invoiceBase;
		}

		const string ExpectedGenericCountryCode = "AU";

		#region Implementation

		void CreateDependencyMocks_WhenIReversalStatusCodeConfigurationProvider_IsNotImplemented()
		{
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);
		}

		(Mock<IAccountingCountryFactory>, Mock<IReversalStatusCodeConfiguration>) CreateDependencyMocks_WhenIReversalStatusCodeConfiguration_IsImplemented()
		{
			var mockIReversalStatusCodeConfiguration = new Mock<IReversalStatusCodeConfiguration>();

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>()
				.Setup(x => x.Get())
				.Returns(mockIReversalStatusCodeConfiguration.Object);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>()))
				.Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			return (mockIAccountingCountryFactory, mockIReversalStatusCodeConfiguration);
		}

		#endregion Implementation
	}
}
