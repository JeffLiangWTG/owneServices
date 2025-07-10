using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public abstract class InvoicingBaseReversalValidationTestCase : TransactionReversalValidationTestCase
	{
		public void TestCheckAH_InvoiceDate()
		{
			using (AccountingMasterFilesRegistry.Instance.PreventInvoiceDateGreaterThanPostDate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.EUR, 1, 100, 10, 100, 10);
				invoice.AH_TransactionNum = "12345";
				TestObjectCreator.Factory.Save();

				((IReversing)invoice).GenerateReverseTransaction(true);
				var reversingHeader = ((IReversing)invoice).ReverseTransaction as TransactionHeader;
				reversingHeader.AH_PostDate = ZDateTime.Now;
				reversingHeader.AH_InvoiceDate = ZDateTime.Now.AddDays(1);

				AssertHasError(reversingHeader.AH_InvoiceDateInfo, @"Invoice or Credit Note Date must be earlier or same as the Post Date. This is controlled by the registry Accounting > Payable Defaults > Default Settings > Prevent Posting Invoice Date Greater Than Post Date.");
			}
		}

		(string, CodePairRegistryItem, CodePairRegistryItem) GetComplianceParams(TransactionHeader invoice)
		{
			string subType;
			CodePairRegistryItem complianceDocumentNumberAllocationRegistry;
			CodePairRegistryItem complianceNumberAllocationDateRegistry;

			switch (invoice.TypeOfReverseTransaction_ForTestOnly.Name)
			{
				case string x when x.Contains("AP"):
					subType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE;
					complianceDocumentNumberAllocationRegistry = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables;
					complianceNumberAllocationDateRegistry = AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP;
					break;
				case string x when x.Contains("AR"):
					subType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE;
					complianceDocumentNumberAllocationRegistry = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables;
					complianceNumberAllocationDateRegistry = AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR;
					break;
				default:
					return (null, null, null);
			}
			return (subType, complianceDocumentNumberAllocationRegistry, complianceNumberAllocationDateRegistry);
		}

		[TestDate(2021, 5, 15)]
		public void TestCheckAH_PostDate_ComplianceBookInvalid_PST()
		{
			AssertCheckAH_PostDate_ComplianceBookInvalid(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2021, 5, 15)]
		public void TestCheckAH_PostDate_ComplianceBookInvalid_INV()
		{
			AssertCheckAH_PostDate_ComplianceBookInvalid(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertCheckAH_PostDate_ComplianceBookInvalid(string dateOption)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				(var subType, var complianceDocumentNumberAllocationRegistry, var complianceNumberAllocationDateRegistry) = GetComplianceParams(Header);
				if (subType == null
					|| (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code && complianceNumberAllocationDateRegistry.Name == AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.Name))
				{
					Assert(true);
					return;
				}

				var originalAllocationDate = ZDateTime.Today;
				var dateOutsideSequence = new ZDateTime(2020, 12, 15);
				Header.AH_TransactionNum = "TRANS";
				SetInvoiceDate(Header, dateOption, originalAllocationDate, dateOutsideSequence);

				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

				var fullSeq = TestObjectCreator.SetupComplianceSequence(menuPK, subType, subType + ".21_", 1, 100, 101);
				fullSeq.XD_StartDate = originalAllocationDate.AddMonths(-1).Date;
				fullSeq.XD_ExpiryDate = originalAllocationDate.AddMonths(1).AddDays(-1);
				fullSeq.XD_IsActive = true;

				Factory.Save();

				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
				using (complianceNumberAllocationDateRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (complianceDocumentNumberAllocationRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					Header.GenerateReverseTransaction(false);
					var reverseTransaction = Header.ReverseTransaction as TransactionHeader;
					SetInvoiceDate(reverseTransaction, dateOption, originalAllocationDate.AddMonths(-2), dateOutsideSequence);
					AssertFieldWithError(reverseTransaction, dateOption, ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);

					Header.GenerateReverseTransaction(false);
					reverseTransaction = Header.ReverseTransaction as TransactionHeader;
					reverseTransaction.AH_ComplianceSubType = subType;
					AssertFieldWithError(reverseTransaction, dateOption, string.Empty);
					SetInvoiceDate(reverseTransaction, dateOption, originalAllocationDate.AddDays(-1), dateOutsideSequence);
					AssertFieldWithError(reverseTransaction, dateOption, ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage);
				}
			}
		}

		void AssertFieldWithError(TransactionHeader transaction, string dateOption, string error)
		{
			var fieldWithError = dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code ? transaction.AH_InvoiceDateInfo : transaction.AH_PostDateInfo;
			if (string.IsNullOrEmpty(error))
			{
				Assert(!fieldWithError.HasErrors());
			}
			else
			{
				Assert(fieldWithError.GetErrors().Contains(error));
			}
		}

		void SetInvoiceDate(TransactionHeader invoice, string complianceNumberAllocationDateOption, ZDateTime allocationDate, ZDateTime otherDate)
		{
			if (complianceNumberAllocationDateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				invoice.AH_PostDate = otherDate;
				invoice.AH_InvoiceDate = allocationDate;
			}
			else
			{
				invoice.AH_PostDate = allocationDate;
				invoice.AH_InvoiceDate = otherDate;
			}
		}

		[TestDate(2021, 1, 15)]
		public void TestCheckAH_PostDate_ComplianceNumberNotAllocatable_PST()
		{
			AssertCheckAllocationDate_ComplianceNumberNotAllocatable(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2021, 1, 15)]
		public void TestCheckAH_PostDate_ComplianceNumberNotAllocatable_INV()
		{
			AssertCheckAllocationDate_ComplianceNumberNotAllocatable(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertCheckAllocationDate_ComplianceNumberNotAllocatable(string dateOption)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				(var subType, var complianceDocumentNumberAllocationRegistry, var complianceNumberAllocationDateRegistry) = GetComplianceParams(Header);
				if (subType == null
					|| (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code && complianceNumberAllocationDateRegistry.Name == AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.Name))
				{
					Assert(true);
					return;
				}

				var originalAllocationDate = ZDate.Today;
				var dateOutsideSequence = new ZDateTime(2020, 12, 15);
				Header.AH_TransactionNum = "TRANS";
				SetInvoiceDate(Header, dateOption, originalAllocationDate, dateOutsideSequence);
				var revType = Header.TypeOfReverseTransaction_ForTestOnly;

				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

				var complSeq = TestObjectCreator.SetupComplianceSequence(menuPK, subType, subType + ".21_", 1, 100, 3);
				complSeq.XD_StartDate = new ZDate(originalAllocationDate.Year, originalAllocationDate.Month, 1);
				complSeq.XD_ExpiryDate = originalAllocationDate.AddMonths(1).AddDays(-1);
				complSeq.XD_IsActive = true;

				var lastDateUsed = originalAllocationDate;

				var inv1 = TestObjectCreator.CreateInvoice(revType, TestObjectCreator.EUR, 1);
				inv1.AH_TransactionNum = "INV1";
				SetInvoiceDate(inv1, dateOption, complSeq.XD_StartDate, dateOutsideSequence);
				inv1.AH_ComplianceSubType = subType;
				inv1.AH_TransactionReference = subType + ".21-0001";
				inv1.AH_XD_ComplianceBook = complSeq.PK;

				var inv2 = TestObjectCreator.CreateInvoice(revType, TestObjectCreator.EUR, 1);
				inv2.AH_TransactionNum = "INV2";
				SetInvoiceDate(inv2, dateOption, originalAllocationDate.AddDays(-5), dateOutsideSequence);
				inv2.AH_ComplianceSubType = subType;
				inv2.AH_XD_ComplianceBook = complSeq.PK;

				var inv3 = TestObjectCreator.CreateInvoice(revType, TestObjectCreator.EUR, 1);
				inv3.AH_TransactionNum = "INV3";
				SetInvoiceDate(inv3, dateOption, lastDateUsed, dateOutsideSequence);
				inv3.AH_ComplianceSubType = subType;
				inv3.AH_XD_ComplianceBook = complSeq.PK;
				inv3.AH_TransactionReference = subType + ".21-0002";

				TestObjectCreator.Factory.Save();

				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
				using (complianceNumberAllocationDateRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (complianceDocumentNumberAllocationRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					Header.GenerateReverseTransaction(false);
					var reverseTransaction = Header.ReverseTransaction as TransactionHeader;
					reverseTransaction.AH_ComplianceSubType = subType;
					AssertFieldWithError(reverseTransaction, dateOption, string.Empty);
					SetInvoiceDate(reverseTransaction, dateOption, originalAllocationDate.AddDays(-10), dateOutsideSequence);
					var expectedError = string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToAllocationDateEarlierThanLastDateUsedMessage(dateOption),
						subType, lastDateUsed.ToShortDateString());
					AssertFieldWithError(reverseTransaction, dateOption, expectedError);

					Header.GenerateReverseTransaction(false);
					reverseTransaction = Header.ReverseTransaction as TransactionHeader;
					reverseTransaction.AH_ComplianceSubType = subType;
					AssertFieldWithError(reverseTransaction, dateOption, string.Empty);
					SetInvoiceDate(reverseTransaction, dateOption, lastDateUsed, dateOutsideSequence);
					expectedError = string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToSparseComplianceBookMessage(dateOption),
						subType, lastDateUsed.ToShortDateString());
					AssertFieldWithError(reverseTransaction, dateOption, expectedError);
				}
			}
		}

		public void TestSupportingDocumentNumber()
		{
			if (HeaderType == typeof(APCreditNote))
			{
				var errorMessage = AccountingConstants.AccountingSupportingDocumentNumberErrorMessage.EmptySupportingDocumentNumber;

				var creditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
				creditNote1.IsReverseTransaction = true;
				creditNote1.SupportingDocumentNumber = string.Empty;
				AssertNoError(creditNote1.SupportingDocumentNumberInfo, errorMessage);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
				{
					var creditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
					creditNote2.IsReverseTransaction = true;
					creditNote2.SupportingDocumentNumber = string.Empty;
					AssertNoError(creditNote2.SupportingDocumentNumberInfo, errorMessage);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var creditNote3 = Factory.NewWithValidTestData<ARCreditNote>();
					creditNote3.IsReverseTransaction = true;
					creditNote3.SupportingDocumentNumber = string.Empty;
					AssertNoError(creditNote3.SupportingDocumentNumberInfo, errorMessage);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var creditNote4 = Factory.NewWithValidTestData<ARCreditNote>();
					var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.Debtor);
					var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);

					creditNote4.IsReverseTransaction = true;
					creditNote4.SupportingDocumentNumber = "123456789";
					AssertNoError(creditNote4.SupportingDocumentNumberInfo, errorMessage);

					creditNote4.OriginalTransaction = arInvoice;
					creditNote4.SupportingDocumentNumber = string.Empty;
					AssertHasError(creditNote4.SupportingDocumentNumberInfo, errorMessage);

					creditNote4.SupportingDocumentNumber = "123456789";
					AssertNoError(creditNote4.SupportingDocumentNumberInfo, errorMessage);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckAH_Calc_AmendStatusCode()
		{
			var originalAR = Factory.NewWithValidTestData<ARInvoice>();
			var invoicingBase = TestObjectCreator.ReverseTransaction(originalAR, out var cantReverseErrorMessage) as InvoicingBase;

			var originalAR2 = Factory.NewWithValidTestData<ARInvoice>();
			var invoicingBase2 = TestObjectCreator.ReverseTransaction(originalAR2, out var cantReverseErrorMessage2) as InvoicingBase;

			var mockIAmendStatusCodeProvider = new Mock<IAmendStatusCodeProvider>();
			var mockIAmendStatusCodeValidationProvider = new Mock<AmendStatusCodeValidationProvider>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			var amendStatusCodeList = new CodeDescriptionPairList();
			amendStatusCodeList.AddPair("01", "Test List Value");

			mockIAmendStatusCodeProvider.Setup(x => x.AmendStatusCodeList).Returns(amendStatusCodeList);
			mockIAmendStatusCodeProvider.Setup(x => x.ShouldShowAmendStatusCode()).Returns(true);
			mockIAmendStatusCodeProvider.Setup(x => x.AmendStatusCodeReferenceType).Returns("KRE");
			mockIAmendStatusCodeValidationProvider.Setup(x => x.ValidateAmendStatusCodeForInvoiceReversal(invoicingBase2)).Verifiable();
			mockIAmendStatusCodeValidationProvider.CallBase = true;
			mockIAccountingCountryFactory.As<IInstanceProvider<IAmendStatusCodeProvider>>().Setup(x => x.Get()).Returns(mockIAmendStatusCodeProvider.Object);
			mockIAccountingCountryFactory.As<IInstanceProvider<IAmendStatusCodeValidationProvider>>().Setup(x => x.Get()).Returns(mockIAmendStatusCodeValidationProvider.Object);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			AssertValidateAH_Calc_AmendStatusCode(invoicingBase, false, false);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			(invoicingBase2.Validation as InvoicingBaseReversalValidation).ValidateAH_Calc_AmendStatusCode();
			mockIAmendStatusCodeValidationProvider.Verify(x => x.ValidateAmendStatusCodeForInvoiceReversal(invoicingBase2), Times.Once);

			invoicingBase2.AH_Calc_AmendStatusCode = "01";
			mockIAmendStatusCodeValidationProvider.Verify(x => x.ValidateAmendStatusCodeForInvoiceReversal(invoicingBase2), Times.Exactly(2));

			invoicingBase2.AH_Calc_AmendStatusCode = "";
			mockIAmendStatusCodeValidationProvider.Verify(x => x.ValidateAmendStatusCodeForInvoiceReversal(invoicingBase2), Times.Exactly(3));

			AssertValidateAH_Calc_AmendStatusCode(invoicingBase, true, true);
		}

		protected void AssertValidateAH_Calc_AmendStatusCode(InvoicingBase invoice, bool isCurrentContextSupportAmendStatusCode, bool isAllowModifyAmendStatusCode)
		{
			AssertEquals("PreCondition", isAllowModifyAmendStatusCode, invoice.IsAllowModifyAmendStatusCode);
			invoice.AH_Calc_AmendStatusCode = ZString.Empty;

			if (invoice.Validation is InvoiceBaseValidation validation)
			{
				validation.ValidateAH_Calc_AmendStatusCode();
			}
			else if (invoice.Validation is InvoicingBaseReversalValidation reversalValidation)
			{
				reversalValidation.ValidateAH_Calc_AmendStatusCode();
			}
			else
			{
				Assert("Validation type is incorrect", true);
			}

			if (isAllowModifyAmendStatusCode)
			{
				AssertHasError(invoice.AH_Calc_AmendStatusCodeInfo, "An amendment status code is required for the amending transaction. Please select an amendment status code.");

				var settingValue = "AA";
				AssertEquals("PreCondition", false, invoice.Lookups.AmendStatusCodeList.ContainsCode(settingValue));
				invoice.AH_Calc_AmendStatusCode = settingValue;
				AssertHasError(invoice.AH_Calc_AmendStatusCodeInfo, "Enter a valid Amend Status Code.");
			}
			else
			{
				AssertNoErrors("Do not check empty value in default.", invoice.AH_Calc_AmendStatusCodeInfo);

				var settingValue = "AA";
				AssertEquals("PreCondition", false, invoice.Lookups.AmendStatusCodeList.ContainsCode(settingValue));
				invoice.AH_Calc_AmendStatusCode = settingValue;
				if (isCurrentContextSupportAmendStatusCode)
				{
					AssertHasError(invoice.AH_Calc_AmendStatusCodeInfo, "Enter a valid Amend Status Code.");
				}
				else
				{
					AssertNoErrors("There should be no validation when Amend Status Code is not supported", invoice.AH_Calc_AmendStatusCodeInfo);
				}
			}
		}

		public void TestReversalStatusCode_Validations_IsReversalStatusCodeAllowed()
		{
			var mockedList = new CodeDescriptionPairList();
			mockedList.AddPair("02", "Mock reversal code One");
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns("FFF");
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetIsReversalStatusCodeAllowed(It.IsAny<ZString>())).Returns(true);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeLookup()).Returns(mockedList);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			var reversedTransaction = GetReversedTransaction();
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			reversedTransaction.ReversalStatusCode = "02";
			AssertNoErrors(reversedTransaction.ReversalStatusCodeInfo);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetIsReversalStatusCodeAllowed(TransactionLedger), Times.Once);

			reversedTransaction.ReversalStatusCode = ZString.Empty;
			AssertHasError(reversedTransaction.ReversalStatusCodeInfo, "Please enter a Reversal Code.");
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetIsReversalStatusCodeAllowed(TransactionLedger), Times.Exactly(2));

			reversedTransaction.ReversalStatusCode = "xx";
			AssertHasError(reversedTransaction.ReversalStatusCodeInfo, "Enter a valid Reversal Code.");
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetIsReversalStatusCodeAllowed(TransactionLedger), Times.Exactly(3));
		}

		public void TestReversalStatusCode_Validations_NotIsReversalStatusCodeAllowed()
		{
			var mockedList = new CodeDescriptionPairList();
			mockedList.AddPair("02", "Mock reversal code One");
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeReferenceType()).Returns("FFF");
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetIsReversalStatusCodeAllowed(It.IsAny<ZString>())).Returns(false);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetReversalStatusCodeLookup()).Returns(mockedList);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			var reversedTransaction = GetReversedTransaction();
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			reversedTransaction.ReversalStatusCode = "02";
			AssertNoErrors(reversedTransaction.ReversalStatusCodeInfo);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetIsReversalStatusCodeAllowed(TransactionLedger), Times.Once);

			reversedTransaction.ReversalStatusCode = ZString.Empty;
			AssertNoErrors(reversedTransaction.ReversalStatusCodeInfo);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetIsReversalStatusCodeAllowed(TransactionLedger), Times.Exactly(2));

			reversedTransaction.ReversalStatusCode = "xx";
			AssertNoErrors(reversedTransaction.ReversalStatusCodeInfo);
			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Verify(x => x.Get().GetIsReversalStatusCodeAllowed(TransactionLedger), Times.Exactly(3));
		}

		public void TestReversalStatusCode_Validations_WithGenericStatusCodeConfiguration()
		{
			var mockedList = new CodeDescriptionPairList();
			mockedList.AddPair("02", "Mock reversal code One");

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			var reversedTransaction = GetReversedTransaction();
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mockedList))
			{
				if (TransactionLedger == LedgerTypes.AccountsReceivable)
				{
					AssertEquals("Precondition: Reversal Status Code is allowed", reversedTransaction.IsReversalStatusCodeAllowed, true);

					reversedTransaction.ReversalStatusCode = "02";
					AssertNoErrors(reversedTransaction.ReversalStatusCodeInfo);

					reversedTransaction.ReversalStatusCode = ZString.Empty;
					AssertHasError(reversedTransaction.ReversalStatusCodeInfo, "Please enter a Reversal Code.");

					reversedTransaction.ReversalStatusCode = "xx";
					AssertHasError(reversedTransaction.ReversalStatusCodeInfo, "Enter a valid Reversal Code.");
				}
				else
				{
					AssertEquals("Precondigion: Reversal Status Code is not allowed", reversedTransaction.IsReversalStatusCodeAllowed, false);

					reversedTransaction.ReversalStatusCode = "02";
					AssertNoErrors(reversedTransaction.ReversalStatusCodeInfo);

					reversedTransaction.ReversalStatusCode = ZString.Empty;
					AssertNoErrors(reversedTransaction.ReversalStatusCodeInfo);

					reversedTransaction.ReversalStatusCode = "xx";
					AssertNoErrors(reversedTransaction.ReversalStatusCodeInfo);
				}
			}
		}

		public void TestReversalStatusCode_CheckValidationAll_ValidateReversalStatusCode()
		{
			var reversedTransaction = GetReversedTransaction();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInstanceProvider<IReversalStatusCodeConfiguration>>().Setup(x => x.Get().GetIsReversalStatusCodeAllowed(It.IsAny<ZString>())).Returns(true);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			var mockDataRefreshBusUpdateActionDecider = new Mock<IDataRefreshBusUpdateActionDecider>();
			mockDataRefreshBusUpdateActionDecider.Setup(x => x.HasSkippedDataRefreshBusUpdate(It.IsAny<TransactionHeader>())).Returns(false);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			AssertNoErrors(reversedTransaction.ReversalStatusCodeInfo);

			var validationAll = new InvoicingBaseReversalValidation(reversedTransaction, mockDataRefreshBusUpdateActionDecider.Object);
			validationAll.ValidateAll();

			AssertHasError(reversedTransaction.ReversalStatusCodeInfo, "Please enter a Reversal Code.");
		}

		public void TestReversalStatusCode_GetCountryFactory_MustBeCalledWith_TransactionContryCode()
		{
			var expectedCountryCode = "MX";
			var reversedTransaction = GetReversedTransaction();
			reversedTransaction.Company.GC_RN_NKCountryCode = expectedCountryCode;

			AssertNotEquals("Precondition, current login country and invoice's country must be different", reversedTransaction.Company.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Code);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>()));
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			reversedTransaction.ReversalStatusCode = "02";
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(expectedCountryCode));
		}

		InvoicingBase GetReversedTransaction()
		{
			var originalTransaction = GetOriginalTransaction();
			var reversedTransaction = TestObjectCreator.ReverseTransaction(originalTransaction, out string cantReverseErrorMessage) as InvoicingBase;
			Assert("Precondition: transaction is reversed without errors", string.IsNullOrEmpty(cantReverseErrorMessage));
			AssertType("Precondition: test transaction has correct type", HeaderType, reversedTransaction);
			AssertEquals("Precondition: test transaction has correct ledger", TransactionLedger, reversedTransaction.AH_Ledger);
			AssertType("Precondition: test transaction Validation has correct type", typeof(InvoicingBaseReversalValidation), reversedTransaction.Validation);

			return reversedTransaction;
		}

		protected abstract InvoicingBase GetOriginalTransaction();

		protected abstract ZString TransactionLedger { get; }
	}
}
