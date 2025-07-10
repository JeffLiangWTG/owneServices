using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(IReversingImplicitlyImplementedWrapperForBinding))]
	public class IReversingImplicitlyImplementedWrapperForBindingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWrappedBusinessEntity()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			IReversingImplicitlyImplementedWrapperForBinding wrapper = new IReversingImplicitlyImplementedWrapperForBinding(invoice);
			AssertEquals(invoice, wrapper.WrappedBusinessEntity);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesIReversingImplicitlyImplementedWrapperForBinding()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			IReversingImplicitlyImplementedWrapperForBinding wrapper = new IReversingImplicitlyImplementedWrapperForBinding(invoice);

			var osList = new List<string>
			{
				nameof(wrapper.OverseasTotalAmount)
			};

			var tester = new DecimalPlacesAttributeTester(wrapper);
			tester.CheckNonLocalCurrency(osList, nameof(wrapper.OSDecimals), nameof(invoice.AH_RX_NKTransactionCurrency), invoice);
		}

		public void TestUnmatchDate()
		{
			var transaction = Factory.New<APPayment>();
			IReversingImplicitlyImplementedWrapperForBinding wrapper = new IReversingImplicitlyImplementedWrapperForBinding(transaction);
			ZDateTime expectedDate = ZDateTime.BrettsBirthday.AddDays(3);
			transaction.UnmatchDate = expectedDate;
			AssertEquals(expectedDate, wrapper.UnmatchDate);

			expectedDate = ZDateTime.BrettsBirthday.AddDays(-11);
			wrapper.UnmatchDate = expectedDate;
			AssertEquals(expectedDate, transaction.UnmatchDate);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			transaction.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Today, AllowBackPosting = true });
			AssertEquals("Precondition: MinUnmatchDate value", ZDateTime.Today, transaction.UnmatchingData.MinUnmatchDate);
			AssertEquals("Precondition: AllowBackPosting value", true, transaction.UnmatchingData.AllowBackPosting);
			wrapper.UnmatchDate = ZDateTime.Today;
			AssertNoErrors(wrapper.UnmatchDateInfo);
			AssertNoErrors(transaction.UnmatchDateInfo);

			wrapper.UnmatchDate = ZDateTime.Today.AddDays(-1);
			AssertHasErrors(wrapper.UnmatchDateInfo);
			AssertHasErrors(transaction.UnmatchDateInfo);

			transaction.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Empty, AllowBackPosting = true });
			AssertEquals("Precondition: MinUnmatchDate value", ZDateTime.Empty, transaction.UnmatchingData.MinUnmatchDate);
			AssertEquals("Precondition: AllowBackPosting value", true, transaction.UnmatchingData.AllowBackPosting);
			AssertEquals(true, transaction.UnmatchDate_ReadOnly);
			AssertEquals(transaction.UnmatchDate_ReadOnly, wrapper.UnmatchDate_ReadOnly);

			transaction.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Today, AllowBackPosting = true });
			AssertEquals("Precondition: MinUnmatchDate value", ZDateTime.Today, transaction.UnmatchingData.MinUnmatchDate);
			AssertEquals("Precondition: AllowBackPosting value", true, transaction.UnmatchingData.AllowBackPosting);
			AssertEquals(false, transaction.UnmatchDate_ReadOnly);
			AssertEquals(transaction.UnmatchDate_ReadOnly, wrapper.UnmatchDate_ReadOnly);
		}

		public void TestRunPreSaveValidation()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			IReversingImplicitlyImplementedWrapperForBinding wrapper = new IReversingImplicitlyImplementedWrapperForBinding(invoice);
			Assert("Precondition: Invoice has not errors.", !invoice.HasErrors);

			wrapper.RunPreSaveValidation();
			Assert("Invoice must have errors after wrapper validation.", invoice.HasErrors);
			Assert("The wrapper must have errors if invoice has ones.", wrapper.HasErrors);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var errorMessage = AccountingConstants.AccountingSupportingDocumentNumberErrorMessage.EmptySupportingDocumentNumber;

				var creditNote1 = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.Debtor);
				var wrapper1 = new IReversingImplicitlyImplementedWrapperForBinding(creditNote1);

				var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.Debtor);
				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice1);
				creditNote1.OriginalTransaction = invoice1;

				Factory.Save();

				wrapper1.RunPreSaveValidation();
				AssertHasRowError(creditNote1, errorMessage);

				creditNote1.SupportingDocumentNumber = "456";

				wrapper1.RunPreSaveValidation();
				AssertNoRowError(creditNote1, errorMessage);
			}
		}

		public void TestTransactionDate()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			IReversingImplicitlyImplementedWrapperForBinding wrapper = new IReversingImplicitlyImplementedWrapperForBinding(invoice);
			ZDateTime transactionDate = ZDateTime.Now.AddDays(10);
			wrapper.TransactionDate = transactionDate;
			AssertEquals("Transaction Date", transactionDate, wrapper.TransactionDate);
			AssertEquals(false, wrapper.TransactionDateInfo.ReadOnly);

			//MonthEndSuspension configed
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			AssertEquals(true, wrapper.TransactionDateInfo.ReadOnly);

			//MonthEndSuspension not configed
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
			AssertEquals(false, wrapper.TransactionDateInfo.ReadOnly);
		}

		#region ReversalStatusCode

		public void TestReversalStatusCode_UseWrapperDependency()
		{
			var mockIReversing = new Mock<IReversing>();
			mockIReversing.Setup(x => x.ReversalStatusCode).Returns("03");

			var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(mockIReversing.Object);
			AssertEquals(nameof(wrapper.ReversalStatusCode), "03", wrapper.ReversalStatusCode);

			wrapper.ReversalStatusCode = "02";
			mockIReversing.VerifySet(x => x.ReversalStatusCode = "02");
		}

		public void TestReversalStatusCodeList_Null()
		{
			var mockIReversing = new Mock<IReversing>();
			mockIReversing.Setup(x => x.ReversalStatusCodeList).Returns(() => null);

			var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(mockIReversing.Object);

			AssertNull(nameof(wrapper.ReversalStatusCodeList), wrapper.ReversalStatusCodeList);
		}
	
		public void TestReversalStatusCodeList_Empty()
		{
			var expectedList = new ReadOnlyCodeDescriptionPairList();
			var mockIReversing = new Mock<IReversing>();
			mockIReversing.Setup(x => x.ReversalStatusCodeList).Returns(expectedList);

			var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(mockIReversing.Object);

			AssertEquals(nameof(wrapper.ReversalStatusCodeList), expectedList, wrapper.ReversalStatusCodeList);
		}

		public void TestReversalStatusCodeList_HasElements()
		{
			var expectedResult = new CodeDescriptionPairList();
			expectedResult.AddPair("02");
			expectedResult.AddPair("03");

			var mockIReversing = new Mock<IReversing>();
			mockIReversing.Setup(x => x.ReversalStatusCodeList).Returns(expectedResult);

			var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(mockIReversing.Object);

			AssertEquals(nameof(wrapper.ReversalStatusCode), expectedResult, wrapper.ReversalStatusCodeList);
		}

		public void TestReversalStatusCode_ReadOnly()
		{
			AssertReversalStatusCode_ReadOnly(true);
			AssertReversalStatusCode_ReadOnly(false);

			void AssertReversalStatusCode_ReadOnly(bool expectedReadOnly)
			{
				var mockIReversing = new Mock<IReversing>();
				mockIReversing.Setup(x => x.ReversalStatusCode_ReadOnly).Returns(expectedReadOnly);
			
				var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(mockIReversing.Object);

				AssertEquals(nameof(wrapper.ReversalStatusCode_ReadOnly), expectedReadOnly, wrapper.ReversalStatusCode_ReadOnly);
			}
		}

		#endregion

		#region Amend Status Code

		public void TestAmendStatusCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.KoreaSouth))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00001", TestObjectCreator.AUD, 0.5m, TestObjectCreator.Debtor);
				var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(arInvoice);
				AssertEquals(ZString.Empty, wrapper.AmendStatusCode);
				wrapper.AmendStatusCode = "03";
				Factory.Save();

				var anotherFactory = new BusinessObjectFactory();
				var arInvoiceInAnotherFactory = anotherFactory.Load<ARInvoice>(arInvoice.PK);
				AssertEquals("03", arInvoiceInAnotherFactory.AH_Calc_AmendStatusCode);
			}
		}

		public void TestAmendStatusCode_Editable()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.KoreaSouth))
			{
				var arInvoice = TestObjectCreator.CreateARCreditNote("00001", TestObjectCreator.ActiveOrg, TestObjectCreator.AUD);
				var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(arInvoice);

				AssertEquals(false, wrapper.AmendStatusCode_ReadOnly);
			}
		}

		public void TestAmendStatusCode_ReadOnlyDueToRowError()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.KoreaSouth))
			{
				var arInvoice = TestObjectCreator.CreateARCreditNote("00001", TestObjectCreator.ActiveOrg, TestObjectCreator.AUD);
				arInvoice.AddRowError("Demo Error.");
				var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(arInvoice);

				AssertEquals(true, wrapper.AmendStatusCode_ReadOnly);
			}
		}

		public void TestAmendStatusCode_ReadOnlyDueToNotInKorea()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var arInvoice = TestObjectCreator.CreateARCreditNote("00001", TestObjectCreator.ActiveOrg, TestObjectCreator.AUD);
				var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(arInvoice);

				AssertEquals(true, wrapper.AmendStatusCode_ReadOnly);
			}
		}

		public void TestAmendStatusCode_ReadOnlyDueToTransactionNotEligible()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.KoreaSouth))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00001", TestObjectCreator.AUD, 0.5m, TestObjectCreator.Debtor);
				var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(arInvoice);

				AssertEquals(true, wrapper.AmendStatusCode_ReadOnly);
			}
		}

		public void TestAmendStatusCodeList()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var arInvoice = TestObjectCreator.CreateARCreditNote("00001", TestObjectCreator.ActiveOrg, TestObjectCreator.AUD);
			var wrapper = new IReversingImplicitlyImplementedWrapperForBinding(arInvoice);

			var amendStatusCodeInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeProvider>;
			var amendStatusCodeProvider = amendStatusCodeInstanceProvider?.Get();
			var statusCodeFromCodeProvider = amendStatusCodeProvider.AmendStatusCodeList;
			var statusCodeFromWrapper = wrapper.AmendStatusCodeList;
			for (var i = 0; i < statusCodeFromCodeProvider.Count; i++)
			{
				AssertEquals(statusCodeFromCodeProvider[i].Code, statusCodeFromWrapper[i].Code);
			}
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return new IReversingImplicitlyImplementedWrapperForBinding(Factory.NewWithValidTestData<ARInvoice>());
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
