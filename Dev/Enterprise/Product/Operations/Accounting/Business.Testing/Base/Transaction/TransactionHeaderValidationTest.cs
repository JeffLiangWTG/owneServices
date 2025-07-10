using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public abstract class TransactionHeaderValidationTest : AccTransactionHeaderValidationTest
	{
		#region Data Refresh Bus Update Validation Tests

		#region AH_TransactionNum

		public void TestAH_TransactionNumBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			if (ShouldTestAH_TransactionNumForThisHeaderType)
			{
				var transactionNum1 = new ZString("1111");
				var transactionNum2 = new ZString("2222");
				var transactionNum3 = new ZString("3333");
				AssertPropertyBeingChangedByDataRefreshBus(AccTransactionHeaderSchema.Constants.AH_TransactionNum, transactionNum1, transactionNum2, transactionNum3);
			}
			else
			{
				Assert("Only incomplete transactions and unallocated transactions can update AH_TransactionNum after being saved.", true);
			}
		}

		ZBool ShouldTestAH_TransactionNumForThisHeaderType => GetTransactionTypesWhichCanChangeTransactionNumberAfterSave().Contains(HeaderType);
		static HashSet<Type> GetTransactionTypesWhichCanChangeTransactionNumberAfterSave() => new HashSet<Type>() { typeof(APInvoice), typeof(APCreditNote), typeof(APAdjustmentNote), typeof(TransactionPendingAllocation) };

		#endregion

		#region AH_InvoicePaymentReferenceCode

		public virtual void TestAH_InvoicePaymentReferenceCodeBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var referenceCode1 = new ZString("111");
			var referenceCode2 = new ZString("222");
			var referenceCode3 = new ZString("333");
			AssertPropertyBeingChangedByDataRefreshBus(AccTransactionHeaderSchema.Constants.AH_InvoicePaymentReferenceCode, referenceCode1, referenceCode2, referenceCode3);
		}

		#endregion

		protected override void GenerateReverseTransactionForTest(AccTransactionHeader header, ZString matchGroupNumber)
		{
			var objCreator = new TestObjectCreator(header.Factory);
			if (header is IMiscellaneousTransaction)
			{
				objCreator.UnmatchTransaction(matchGroupNumber);
			}
			else if (header is IReversing originalTransaction)
			{
				var transaction = (TransactionHeader)objCreator.ReverseTransaction(originalTransaction, out string message);
				if (transaction != null)
				{
					transaction.AH_TransactionNum = matchGroupNumber;
				}
			}
		}

		protected override void CreateMatchGroupForMiscellaneousTransaction(AccTransactionHeader header, ZString matchGroupNumber)
		{
			if (header is IMiscellaneousTransaction)
			{
				var isAP = header.AH_Ledger == LedgerTypes.AccountsPayable;
				var invoiceType = isAP ? typeof(APInvoice) : typeof(ARInvoice);

				var newFactory = new BusinessObjectFactory();
				var newObjectCreator = new TestObjectCreator(newFactory);
				var matchingInvoice = newObjectCreator.CreateInvoice(isAP ? typeof(APInvoice) : typeof(ARInvoice));
				matchingInvoice.AH_InvoiceAmount = matchingInvoice.AH_OSTotal = matchingInvoice.AH_OutstandingAmount = isAP ? -10m : 10m;
				newObjectCreator.CreateInvoiceLine(matchingInvoice, newObjectCreator.AUD, 1m, matchingInvoice.AH_InvoiceAmount, GlbBranch.CurrentBranch.PK, false);
				newFactory.Save();

				var matchingInvoiceInHeaderFactory = (TransactionHeader)Factory.Load(invoiceType, matchingInvoice.PK);
				header.AH_InvoiceAmount = header.AH_OSTotal = -matchingInvoiceInHeaderFactory.AH_InvoiceAmount;

				var miscTransactionMatchLink = TestObjectCreator.CreateMatchLink((TransactionHeader)header, header.AH_InvoiceAmount, ZDateTime.Today, matchGroupNumber);
				header.AH_OutstandingAmount = 0m;

				var invoiceMatchLink = TestObjectCreator.CreateMatchLink(matchingInvoiceInHeaderFactory, matchingInvoiceInHeaderFactory.AH_InvoiceAmount, ZDateTime.Today, matchGroupNumber);
				matchingInvoiceInHeaderFactory.AH_FullyPaidDate = ZDateTime.Today;
				matchingInvoiceInHeaderFactory.AH_OutstandingAmount = 0m;

				((IMatching)header).CurrentMatchGroup.RemoveAll();
				((IMatching)header).CurrentMatchGroup.Add(miscTransactionMatchLink);
				((IMatching)header).CurrentMatchGroup.Add(invoiceMatchLink);
			}
		}

		protected override void SetupTransactionForOutstandingAmountTest(AccTransactionHeader header)
		{
			if (header is TransactionHeaderWithLines headerWithLines)
			{
				var line = (TransactionLine)headerWithLines.Lines.AddNew();
				line.AL_OSExTaxAmount = 100m;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
			}
			else
			{
				header.AH_InvoiceAmount = header.AH_OSTotal = header.AH_OutstandingAmount = 100m;
			}
		}

		#endregion

		protected void AssertCheckAH_GovernmentAllocatedID_RunsValidation()
		{
			var mockIGovernmentAllocatedIDValidationProvider = new Mock<IGovernmentAllocatedIDValidationProvider>();
			mockIGovernmentAllocatedIDValidationProvider
				.Setup(x => x.ValidateGovernmentAllocatedID(It.IsAny<GovernmentAllocatedIDValidationData>()))
				.Returns("An error message");

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory
				.As<IInstanceProvider<IGovernmentAllocatedIDValidationProvider>>()
				.Setup(x => x.Get())
				.Returns(mockIGovernmentAllocatedIDValidationProvider.Object);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory
				.Setup(x => x.GetCountryFactory(It.IsAny<ZString>()))
				.Returns(mockIAccountingCountryFactory.Object);

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: true))
			{
				#region Assert when AH_OA_InvoiceAddressOverride is empty main address is used AND error message added to the property info

				Header.AH_OH = TestObjectCreator.Creditor1.PK;
				Header.AH_GovernmentAllocatedID = "12345678";

				mockIGovernmentAllocatedIDValidationProvider
					.Verify(x => x.ValidateGovernmentAllocatedID(It.Is<IGovernmentAllocatedIDValidationData>(
						x => x.EReportingStatus == ""
						&& x.GovernmentAllocatedID == "12345678"
						&& x.Ledger == Header.AH_Ledger
						&& x.OrgCountryCode == "AU")), Times.Once);

				AssertHasError(header.AH_GovernmentAllocatedIDInfo, "An error message");

				#endregion

				#region Assert AH_OA_InvoiceAddressOverride value is used when it is not empty AND error message added to the property info

				var debtorOverrideAddress = TestObjectCreator.CreateAddress(TestObjectCreator.Creditor1, "TR", SharedConstants.Languages.English, "Test Address", "", OrgAddressType.Receivables, "");
				Header.AH_OA_InvoiceAddressOverride = debtorOverrideAddress.PK;

				mockIGovernmentAllocatedIDValidationProvider
					.Verify(x => x.ValidateGovernmentAllocatedID(It.Is<IGovernmentAllocatedIDValidationData>(
						x => x.EReportingStatus == ""
						&& x.GovernmentAllocatedID == "12345678"
						&& x.Ledger == Header.AH_Ledger
						&& x.OrgCountryCode == "TR")), Times.Once);

				AssertHasError(header.AH_GovernmentAllocatedIDInfo, "An error message");

				#endregion

				AssertTransactionHeaderCompanyCountryCodeIsUsed(mockIGlobalAccountingCountryFactory);
			}

			mockIGovernmentAllocatedIDValidationProvider.Invocations.Clear();
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: false))
			{
				Header.AH_GovernmentAllocatedID = "1234567890";
				mockIGovernmentAllocatedIDValidationProvider
					.Verify(x => x.ValidateGovernmentAllocatedID(It.IsAny<IGovernmentAllocatedIDValidationData>()), Times.Never);

				AssertNoErrors(header.AH_GovernmentAllocatedIDInfo);
			}
		}

		void AssertTransactionHeaderCompanyCountryCodeIsUsed(Mock<IGlobalAccountingCountryFactory> mockIGlobalAccountingCountryFactory)
		{
			var testCompany = TestObjectCreator.CreateNewCompany("TST", "US");
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP100001", TestObjectCreator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, TestObjectCreator.Creditor1);
			apInvoice.AH_GC = testCompany.PK;
			mockIGlobalAccountingCountryFactory.Invocations.Clear();

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetTemporaryValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: true))
			{
				apInvoice.AH_GovernmentAllocatedID = "12345678";
			}

			AssertNotEquals(GlbCompany.CurrentCompany.Country.Code, "US");
			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory("US"), Times.Once);
		}

		public virtual void TestCheckAH_ReceiptType()
		{
			var expectedError = "To process E-Payments, please create a Payment or Payment Batch in the Payables Transactions module or a Payment Approval in the Payment Processing module.";

			Header.AH_ReceiptType = ReceiptTypes.Cheque;
			AssertEquals(false, Header.AH_ReceiptTypeInfo.Notifications.Any(n => n.Message.Contains(expectedError)));

			Header.AH_ReceiptType = ReceiptTypes.EPayment;
			if (Header is Payment)
			{
				AssertEquals(false, Header.AH_ReceiptTypeInfo.Notifications.Any(n => n.Message.Contains(expectedError)));
			}
			else
			{
				AssertHasError(Header.AH_ReceiptTypeInfo, expectedError);
			}
		}

		public virtual void TestCheckAH_AB()
		{
			var bankAccount = TestObjectCreator.CreateBankAccount("ANZ", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			var ofxAccount = TestObjectCreator.CreateBankAccount("OFX", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			ofxAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			ofxAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;

			var expectedError1 = "This bank account is an E-Payment Account. Please set Payment Type to EPA - E-Payment.";
			var expectedError2 = "Bank Account is not an E-Payment Account.";

			Header.AH_ReceiptType = ReceiptTypes.Cheque;
			Header.AH_AB = bankAccount.PK;
			AssertNull(Header.AH_ABInfo.Notifications.FirstOrDefault(x => x.Type == CargoWise.ComponentModel.NotificationType.Error && x.Message == expectedError1));
			AssertNull(Header.AH_ABInfo.Notifications.FirstOrDefault(x => x.Type == CargoWise.ComponentModel.NotificationType.Error && x.Message == expectedError2));

			Header.AH_AB = ofxAccount.PK;
			AssertNull(Header.AH_ABInfo.Notifications.FirstOrDefault(x => x.Type == CargoWise.ComponentModel.NotificationType.Error && x.Message == expectedError2));
			if (Header.AH_Ledger == LedgerTypes.CashBook)
			{
				AssertNull(Header.AH_ABInfo.Notifications.FirstOrDefault(x => x.Type == CargoWise.ComponentModel.NotificationType.Error && x.Message == expectedError1));
			}
			else
			{
				AssertHasError(Header.AH_ABInfo, expectedError1);
			}

			Header.AH_ReceiptType = ReceiptTypes.EPayment;
			Header.AH_AB = bankAccount.PK;
			AssertNull(Header.AH_ABInfo.Notifications.FirstOrDefault(x => x.Type == CargoWise.ComponentModel.NotificationType.Error && x.Message == expectedError1));
			if (Header.AH_Ledger == LedgerTypes.CashBook)
			{
				AssertNull(Header.AH_ABInfo.Notifications.FirstOrDefault(x => x.Type == CargoWise.ComponentModel.NotificationType.Error && x.Message == expectedError2));
			}
			else
			{
				AssertHasError(Header.AH_ABInfo, expectedError2);
			}

			Header.AH_AB = ofxAccount.PK;
			AssertNull(Header.AH_ABInfo.Notifications.FirstOrDefault(x => x.Type == CargoWise.ComponentModel.NotificationType.Error && x.Message == expectedError1));
			AssertNull(Header.AH_ABInfo.Notifications.FirstOrDefault(x => x.Type == CargoWise.ComponentModel.NotificationType.Error && x.Message == expectedError2));
		}

		public void CheckAH_ExchangeRate()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(HeaderType);

			if (invoice.IsSettingLineExchangeRateSupported)
			{
				InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

				line.AL_ExchangeRate = 0m;
				AssertHasErrors(line.AL_ExchangeRateInfo);

				line.AL_ExchangeRate = 1m;
				AssertNoErrors(line.AL_ExchangeRateInfo);

				line.AL_ExchangeRate = -0.5m;
				AssertHasErrors(line.AL_ExchangeRateInfo);

				line.AL_ExchangeRate = 1m;
				AssertNoErrors(line.AL_ExchangeRateInfo);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckSupportingDocumentNumber()
		{
			Assert(true);
		}

		public void TestCheckAH_GB()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_GB = ZGuid.NewZGuid();

			invoice.Validation.ValidateAH_GB();
			AssertHasError(invoice.AH_GBInfo, "Enter a valid Branch.");

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			invoice.AH_GB = branch.PK;

			invoice.Validation.ValidateAH_GB();
			AssertNoErrors(invoice.AH_GBInfo);

			branch.Delete();
			invoice.Validation.ValidateAH_GB();
			AssertHasError(invoice.AH_GBInfo, "Enter a valid Branch.");
		}

		public void TestCheckAH_GE_ensureDepartmentNotNull()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_GE = ZGuid.NewZGuid();

			invoice.Validation.ValidateAH_GE();
			AssertHasError("Null department should return an error", invoice.AH_GEInfo, "Enter a valid Department.");

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			invoice.AH_GE = department.PK;

			invoice.Validation.ValidateAH_GE();
			AssertNoErrors(invoice.AH_GEInfo);
		}

		public void TestCheckSourceReference()
		{
			if (Header.AH_Ledger == LedgerTypes.AccountsReceivable && (Header.AH_TransactionType == TransactionTypes.Invoice || Header.AH_TransactionType == TransactionTypes.CreditNote))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
				{
					var invoice = Factory.NewWithValidTestData<ARInvoice>();
					var testValidation = new TransactionHeaderValidation(invoice);

					foreach (var complianceSubType in new string[] {
					PortugalComplianceInfo.ComplianceSubTypeCodes.TXM,
					PortugalComplianceInfo.ComplianceSubTypeCodes.TCM,
					PortugalComplianceInfo.ComplianceSubTypeCodes.LTX,
					PortugalComplianceInfo.ComplianceSubTypeCodes.LCR,
					PortugalComplianceInfo.ComplianceSubTypeCodes.LCD,
					PortugalComplianceInfo.ComplianceSubTypeCodes.TDM })
					{
						var expectedPrefix = string.Empty;
						if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.LTX)
						{
							expectedPrefix = PortugalComplianceInfo.ReferencePrefixes.FTD;
						}
						else if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.LCR)
						{
							expectedPrefix = PortugalComplianceInfo.ReferencePrefixes.NCD;
						}
						else if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TCM)
						{
							expectedPrefix = PortugalComplianceInfo.ReferencePrefixes.NCM;
						}
						else if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TXM)
						{
							expectedPrefix = PortugalComplianceInfo.ReferencePrefixes.FTM;
						}
						else if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.LCD)
						{
							expectedPrefix = PortugalComplianceInfo.ReferencePrefixes.NDD;
						}
						else if (complianceSubType == PortugalComplianceInfo.ComplianceSubTypeCodes.TDM)
						{
							expectedPrefix = PortugalComplianceInfo.ReferencePrefixes.NDM;
						}

						var expectedError = $"When compliance sub type is {complianceSubType}, Source Reference must contain the prefix '{expectedPrefix}' plus a space and the Original Invoice Number.";

						invoice.AH_ComplianceSubType = complianceSubType;
						Assert(invoice.IsSourceReferenceEnabled);
						AssertEquals(expectedPrefix, invoice.SourceReference);
						testValidation.ValidateSourceReference();
						AssertHasError("Missing Prefix", invoice.SourceReferenceInfo, expectedError);

						invoice.SourceReference = $"{expectedPrefix} a";
						testValidation.ValidateSourceReference();
						AssertNoErrors(invoice.SourceReferenceInfo);
						invoice.SourceReference = $"{expectedPrefix} a123";
						testValidation.ValidateSourceReference();
						AssertNoErrors(invoice.SourceReferenceInfo);
						invoice.SourceReference = $"{expectedPrefix} a/123b";
						testValidation.ValidateSourceReference();
						AssertNoErrors(invoice.SourceReferenceInfo);
						invoice.SourceReference = $"{expectedPrefix} a/123/b";
						testValidation.ValidateSourceReference();
						AssertNoErrors(invoice.SourceReferenceInfo);

						invoice.SourceReference = string.Empty;
						testValidation.ValidateSourceReference();
						AssertHasError("cannot be empty", invoice.SourceReferenceInfo, expectedError);

						invoice.SourceReference = $"{expectedPrefix}a";
						testValidation.ValidateSourceReference();
						AssertHasError("Missing Prefix and space", invoice.SourceReferenceInfo, expectedError);

						invoice.SourceReference = $"{expectedPrefix} ";
						testValidation.ValidateSourceReference();
						AssertHasError("Missing Original Invoice Number", invoice.SourceReferenceInfo, expectedError);

						invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.SBD;
						testValidation.ValidateSourceReference();
						AssertNoErrors(invoice.SourceReferenceInfo);

						Assert("We should have a DisposableManager on the Factory to make sure SourceReference Mutex is disposed in case Factory is abandoned", Factory.TryGetDisposableManager(out var disposableManager));
						disposableManager.Dispose();
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDuplicateSourceReference()
		{
			if (Header.AH_Ledger == LedgerTypes.AccountsReceivable && (Header.AH_TransactionType == TransactionTypes.Invoice || Header.AH_TransactionType == TransactionTypes.CreditNote))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
				{
					var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
					invoice1.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
					invoice1.SourceReference = "FTM a123";
					Factory.Save();

					var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
					invoice2.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
					invoice2.SourceReference = "FTM a123";
					new TransactionHeaderValidation(invoice2).ValidateSourceReference();
					AssertHasError("Missing Prefix", invoice2.SourceReferenceInfo, $"The entered Source Reference value is already recorded against transaction {invoice1.AH_TransactionNum}.");
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckAH_InvoiceAmount()
		{
			AssertAmountExceedMaximumAllowedAmount(Header.AH_InvoiceAmountInfo);

			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);
			var apInvoice = (APInvoice)creator.CreateInvoiceWithLine(typeof(APInvoice), "APNV1", creator.AUD, 1m, 10m, 0m, 10m, 0m);
			AssertAmountExceedMaximumAllowedAmount_InDB(apInvoice, apInvoice.AH_InvoiceAmountInfo);
		}

		public void TestCheckAH_GSTAmount()
		{
			AssertAmountExceedMaximumAllowedAmount(Header.AH_GSTAmountInfo);

			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);
			var apInvoice = (APInvoice)creator.CreateInvoiceWithLine(typeof(APInvoice), "APNV1", creator.AUD, 1m, 10m, 0m, 10m, 0m);
			AssertAmountExceedMaximumAllowedAmount_InDB(apInvoice, apInvoice.AH_GSTAmountInfo);
		}

		void AssertAmountExceedMaximumAllowedAmount(ZPropertyInfo propertyInfo)
		{
			var maximumAllowedHeaderAmount = 50M;
			var registryValue = new MaximumAllowedTransactionAmount();
			registryValue.MaximumAllowedHeaderAmount = maximumAllowedHeaderAmount;
			registryValue.MaximumAllowedLineAmount = 10000M;
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				SetAmount(Header, propertyInfo, 49);
				Assert("Precondition: IsInDatabase is false", !Header.IsInDatabase);
				Assert("Precondition: IsReverseTransaction is false", !Header.IsReverseTransaction);
				AssertNoErrors(propertyInfo);

				SetAmount(Header, propertyInfo, 51);
				var expectedMessage = $"The transaction header amount exceed the maximum allowed amount {registryValue.MaximumAllowedHeaderAmount.ToString(GlbCompany.CurrentCompany.GetLocalDecimals())} which is defined in the '{registry.HumanReadableRegistryPath()}' registry.";
				AssertHasError(propertyInfo, expectedMessage);

				Header.IsReverseTransaction = true;
				SetAmount(Header, propertyInfo, 52);
				AssertNoErrors(propertyInfo);
			}
		}

		void AssertAmountExceedMaximumAllowedAmount_InDB(APInvoice apInvoice, ZPropertyInfo propertyInfo)
		{
			var registryValue = new MaximumAllowedTransactionAmount();
			registryValue.MaximumAllowedHeaderAmount = 1000M;
			registryValue.MaximumAllowedLineAmount = 10000M;
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;

			Validate();
			AssertNoErrors(propertyInfo);
			apInvoice.SaveAsIncomplete();

			var maximumAllowedHeaderAmount = 1M;
			registryValue.MaximumAllowedHeaderAmount = maximumAllowedHeaderAmount;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var expectedMessage = $"The transaction header amount exceed the maximum allowed amount {registryValue.MaximumAllowedHeaderAmount.ToString(GlbCompany.CurrentCompany.GetLocalDecimals())} which is defined in the '{registry.HumanReadableRegistryPath()}' registry.";
				SetAmount(apInvoice, propertyInfo, 52);

				AssertHasError(propertyInfo, expectedMessage);
			}

			void Validate()
			{
				if (propertyInfo.Name == apInvoice.AH_InvoiceAmountInfo.Name)
				{
					apInvoice.Validation.ValidateAH_InvoiceAmount();
				}
				else
				{
					apInvoice.Validation.ValidateAH_GSTAmount();
				}
			}
		}

		void SetAmount(TransactionHeader header, ZPropertyInfo propertyInfo, ZDecimal amount)
		{
			if (propertyInfo.Name == header.AH_InvoiceAmountInfo.Name)
			{
				header.AH_InvoiceAmount = amount;
			}
			else
			{
				header.AH_GSTAmount = amount;
			}
		}

		public virtual void TestCheckAH_PostDate()
		{
			BusinessObjectFactory periodFactory = new BusinessObjectFactory();
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(periodFactory);

			AccPeriodManagement accPeriod = periodCalculator.GetPeriodManagementFromDate(ZDateTime.Today);
			if (accPeriod == null)
			{
				accPeriod = periodFactory.New<AccPeriodManagement>();
				accPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
				accPeriod.AM_Period = periodCalculator.GetPeriodFromDate(ZDateTime.Today);
				accPeriod.AM_Year = (short)(accPeriod.AM_Period / 100);
				accPeriod.AM_StartDate = new ZDateTime(ZDateTime.Today.Year, ZDateTime.Today.Month, 1);
				accPeriod.AM_EndDate = accPeriod.AM_StartDate.AddMonths(1).AddDays(-1);
			}
			periodFactory.Save();

			Header.AH_PostDate = ZDateTime.Today;
			AssertEquals("Period is open", false, Header.AH_PostDateInfo.HasErrors());

			accPeriod.AM_IsSubLedgerClosed = true;
			accPeriod.AM_IsGeneralLedgerClosed = true;
			periodFactory.Save();
			Header.Validation.ValidateAH_PostDate();
			AssertEquals("Period closed", true, Header.AH_PostDateInfo.HasErrors());

			var newHeader = CreateDirectPaymentAsPartOfDirectDebitBatch();
			newHeader.Validation.ValidateAH_PostDate();
			AssertEquals("Period closed", false, newHeader.AH_PostDateInfo.HasErrors());

			newHeader.IsCancelled = true;
			newHeader.AH_PostDate = ZDateTime.Today;
			AssertEquals("Period closed", true, newHeader.AH_PostDateInfo.HasErrors());
		}

		protected virtual bool ShouldTestAH_AG
		{
			get { return true; }
		}

		public void TestCheckAH_AG()
		{
			if (ShouldTestAH_AG)
			{
				Header.AH_AG = Guid.Empty;
				Header.AH_Ledger = LedgerTypes.AccountsPayable;
				Header.AH_TransactionType = TransactionTypes.Journal;
				HeaderValidation.ValidateAH_AG();
				AssertHasError(Header.AH_AGInfo, "Please enter a GL Account.");

				AccGLHeader accHeader = new BusinessObjectFactory().NewWithValidTestData<AccGLHeader>();
				Header.AH_AG = accHeader.PK;
				accHeader.Factory.Save();
				accHeader.Delete();
				accHeader.Factory.Save();

				Header.AH_Ledger = LedgerTypes.AccountsPayable;
				Header.AH_TransactionType = TransactionTypes.Journal;
				HeaderValidation.ValidateAH_AG();
				AssertHasError(Header.AH_AGInfo, "Enter a valid GL Account.");

				Header.AH_AG = new TestObjectCreator(Factory).GLHeader1.PK;
				HeaderValidation.ValidateAH_AG();
				AssertNoErrors(Header.AH_AGInfo);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckAH_DocumentReceivedDate()
		{
			if (Header.IsDocumentReceivedDateApplicable)
			{
				using (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateMustBeEntered.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					Header.AH_DocumentReceivedDate = ZDateTime.Empty;
					AssertNoErrorContaining(Header.AH_DocumentReceivedDateInfo, MandatoryValidation.MustBeEntered);
				}

				using (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateMustBeEntered.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					Header.AH_DocumentReceivedDate = ZDateTime.Empty;
					AssertHasErrorContaining(Header.AH_DocumentReceivedDateInfo, MandatoryValidation.MustBeEntered);

					Header.AH_DocumentReceivedDate = new ZDateTime(2020, 01, 01);
					AssertNoErrorContaining(Header.AH_DocumentReceivedDateInfo, MandatoryValidation.MustBeEntered);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestCheckAH_PostDateForBackPosting()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			Header.AH_PostDate = ZDateTime.Today;

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition: Header.AllowBackPosting", false, Header.AllowBackPosting);
			HeaderValidation.ValidateAH_PostDate();
			AssertNoErrors("PostDate in Today, back posting is not allowed.", Header.AH_PostDateInfo);
			AssertNoWarnings("PostDate in Today, back posting is not allowed.", Header.AH_PostDateInfo);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition: Header.AllowBackPosting", true, Header.AllowBackPosting);
			HeaderValidation.ValidateAH_PostDate();
			AssertNoErrors("PostDate in Today, back posting is allowed.", Header.AH_PostDateInfo);
			AssertNoWarnings("PostDate in Today, back posting is allowed.", Header.AH_PostDateInfo);

			Header.AH_PostDate = ZDateTime.Today.AddDays(-10);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition: Header.AllowBackPosting", false, Header.AllowBackPosting);
			HeaderValidation.ValidateAH_PostDate();
			AssertHasError("PostDate in past date, back posting is not allowed.", Header.AH_PostDateInfo, "The post date cannot be in the past");
			AssertNoWarnings("PostDate in past date, back posting is not allowed.", Header.AH_PostDateInfo);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition: Header.AllowBackPosting", true, Header.AllowBackPosting);
			HeaderValidation.ValidateAH_PostDate();
			AssertNoErrors("PostDate in past date, back posting is allowed.", Header.AH_PostDateInfo);
			AssertHasWarning("PostDate in past date, back posting is allowed.", Header.AH_PostDateInfo, "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing");
		}

		public void TestCheckIsCancelledIsNotSetWithoutMatchLinks()
		{
			SetInvoices();

			ARInvoice aRInvoiceToTest = Factory.NewWithValidTestData<ARInvoice>();
			TransactionHeaderValidation testValidation = new TransactionHeaderValidation(aRInvoiceToTest);

			testValidation.CheckAH_IsCancelled_ForTestOnly();
			testValidation.ValidateAll();
			Assert(!aRInvoiceToTest.Notifications.ContainsNotificationContaining(IsCancelledError));

			aRInvoiceToTest.AH_IsCancelled = ZBool.True;
			ExceptionReporterTestListener.Instance.Clear();
			testValidation.ValidateAll();
			Assert("Critical Validation was moved OnSaving, so we are not expecting: " + IsCancelledError, !aRInvoiceToTest.Notifications.ContainsNotificationContaining(IsCancelledError));

			try
			{
				Factory.Save();
				Fail("Critical Validation should prevent saving");
			}
			catch (OnSavingCriticalCheckException ex)
			{
				AssertContains("Critical Validation Error", IsCancelledError, ex.Message);
				ExceptionReporterTestListener.Instance.Clear();
			}

			aRInvoiceToTest = Factory.NewWithValidTestData<ARInvoice>();
			testValidation = new TransactionHeaderValidation(aRInvoiceToTest);

			((IMatching)aRInvoiceToTest).CurrentMatchGroup.AddNew().AP_AH = aRInvoiceToTest.PK;
			aRInvoiceToTest.AH_IsCancelled = ZBool.True;
			testValidation.ValidateAll();
			Assert(!aRInvoiceToTest.Notifications.ContainsNotificationContaining(IsCancelledError));
		}

		string IsCancelledError
		{
			get { return "Missing Reversing Transaction for this canceled transaction"; }
		}

		public void TestMultipleReversingErrors()
		{
			TransactionHeaderValidation testValidation = new TransactionHeaderValidation(Header);

			Header.MultipleReversingErrors = null;
			testValidation.ValidateAll();
			AssertNoRowErrors(Header);

			Header.MultipleReversingErrors = new string[] { "error 1", "error 2" };
			testValidation.ValidateAll();
			AssertHasRowError(Header, "error 1");
			AssertHasRowError(Header, "error 2");
		}

		public void TestCheckAH_PostDate2()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

			var receipt = TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cheque, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, TestObjectCreator.AUDBankAccount.PK);
			receipt.AH_PostDate = ZDateTime.Empty;
			receipt.Validation.ValidateAH_PostDate();
			Assert("Should have error about missing post date", receipt.AH_PostDateInfo.HasError("Please enter an Invoice Post Date."));
			receipt.AH_PostDate = ZDateTime.Invalid;
			receipt.Validation.ValidateAH_PostDate();
			Assert("Should have error about invalid post date", receipt.AH_PostDateInfo.HasError("Enter a valid Invoice Post Date."));

			AssertReceiptPaymentAH_PostDateMessage(0, receipt, "", false);
			AssertReceiptPaymentAH_PostDateMessage(-2, receipt, "The post date cannot be in the past", true);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			AssertReceiptPaymentAH_PostDateMessage(0, receipt, "", false);
			AssertReceiptPaymentAH_PostDateMessage(-2, receipt, "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing", false);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;

			AssertReceiptPaymentAH_PostDateMessage(2, receipt, "", false);
			AssertReceiptPaymentAH_PostDateMessage(0, receipt, "", false);
			AssertReceiptPaymentAH_PostDateMessage(-2, receipt, "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing", false);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

			AssertReceiptPaymentAH_PostDateMessage(2, receipt, "", false);
			AssertReceiptPaymentAH_PostDateMessage(0, receipt, "", false);
			AssertReceiptPaymentAH_PostDateMessage(-2, receipt, "The post date cannot be in the past", true);
		}

		void AssertReceiptPaymentAH_PostDateMessage(int daysFromToday, ReceiptPaymentBase receipt, string message, bool isError)
		{
			receipt.AH_PostDate = ZDateTime.Now.AddDays(daysFromToday);
			receipt.Validation.ValidateAH_PostDate();
			string assertionMsg = string.Format("Should have {0} about post date is in {1}",
				string.IsNullOrEmpty(message) ? "no problem" : (isError ? "error" : "warning"),
				daysFromToday < 0 ? "past" : (daysFromToday == 0 ? "present" : "future"));
			bool result;
			if (!string.IsNullOrEmpty(message))
			{
				result = isError ? receipt.AH_PostDateInfo.HasError(message) : receipt.AH_PostDateInfo.HasWarning(message);
			}
			else
			{
				result = !receipt.AH_PostDateInfo.HasErrors() && !receipt.AH_PostDateInfo.HasWarnings();
			}
			Assert(assertionMsg, result);
		}

		public void TestCheckAH_AgreedPaymentMethodOverride()
		{
			TransactionHeaderValidation testValidation = new TransactionHeaderValidation(Header);

			Header.AH_AgreedPaymentMethodOverride = "";
			AssertNoErrors("Agreed payment method can be empty", Header.AH_AgreedPaymentMethodOverrideInfo);

			Header.AH_AgreedPaymentMethodOverride = "X";
			AssertHasError("Expect error", Header.AH_AgreedPaymentMethodOverrideInfo, "The agreed payment method 'X' is no longer valid, please review Organization > AR/AP > Agreed Payment Method value and corresponding system registry before posting.");

			AssertNotEquals(0, Header.DisplayAgreedPaymentMethodsList.Count);
			Header.AH_AgreedPaymentMethodOverride = Header.DisplayAgreedPaymentMethodsList.GetAllCodes()[0];
			AssertNoErrors("Valid value for agreed payment method", Header.AH_AgreedPaymentMethodOverrideInfo);
		}

		public void TestCheckAH_PlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var placeOfSupplyCode = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
				var placeOfSupplyTypeCode = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(GlbCompany.CurrentCompany, placeOfSupplyCode);

				Header.AH_PlaceOfSupplyType = placeOfSupplyTypeCode;
				AssertHasError("Place of Supply Type can not be empty when palce of supply is not empty", Header.AH_PlaceOfSupplyInfo, "Please enter a Place of Supply.");

				Header.AH_PlaceOfSupplyType = placeOfSupplyTypeCode;
				AssertNoErrors(Header.AH_PlaceOfSupplyTypeInfo);
				Header.AH_PlaceOfSupply = placeOfSupplyCode;
				AssertNoErrors(Header.AH_PlaceOfSupplyInfo);

				Header.AH_PlaceOfSupply = "ZZZ";
				AssertHasError(Header.AH_PlaceOfSupplyInfo, "Enter a valid Place of Supply.");
			}
		}

		public void TestCheckAH_PlaceOfSupplyType()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var placeOfSupplyCode = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
				var placeOfSupplyTypeCode = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(GlbCompany.CurrentCompany, placeOfSupplyCode);

				Header.AH_PlaceOfSupply = placeOfSupplyCode;
				Header.AH_PlaceOfSupplyType = string.Empty;
				AssertNoErrors(Header.AH_PlaceOfSupplyInfo);
				AssertHasError("Place of Supply Type can be empty", Header.AH_PlaceOfSupplyTypeInfo, "Please enter a Place of Supply Type.");

				Header.AH_PlaceOfSupplyType = placeOfSupplyTypeCode;
				AssertNoErrors(Header.AH_PlaceOfSupplyTypeInfo);
				Header.AH_PlaceOfSupply = placeOfSupplyCode;
				AssertNoErrors(Header.AH_PlaceOfSupplyInfo);

				Header.AH_PlaceOfSupplyType = "ZZZ";
				AssertHasError(Header.AH_PlaceOfSupplyTypeInfo, "Enter a valid Place of Supply Type.");
			}
		}

		public void TestCheckAH_OH_TransactionCreationRestriction()
		{
			List<string> allARAPTransactionTypes = new List<string>
				{
					TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote,
					TransactionTypes.Journal, TransactionTypes.Receipt, TransactionTypes.Payment,
					TransactionTypes.Transfer, TransactionTypes.Contra, TransactionTypes.Overpayment,
					TransactionTypes.Discount, TransactionTypes.ExchangeDifference, TransactionTypes.InvoiceBatch
				};

			var org = TestObjectCreator.CreateOrgHeader("TST", true, true);
			Factory.Save();

			Header.AH_OH = org.PK;

			AssertEquals("Pre-condition", Core.Constants.TransactionCreationRestriction.None, org.CompanyData.OB_APTransactionCreationRestriction);
			AssertNoErrors(Header.AH_OHInfo);

			org.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.All;
			org.CompanyData.OB_ARTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.All;

			HeaderValidation.ValidateAH_OH();

			if (Header.AH_Ledger == LedgerTypes.AccountsPayable || Header.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				if (allARAPTransactionTypes.Contains(Header.AH_TransactionType))
				{
					AssertHasError("Should have proper validation error", Header.AH_OHInfo, $@"You cannot create transaction '{Header.AH_TransactionNum}' because organization '{org.OH_Code}' has an {Header.AH_Ledger} transaction creation restriction policy set to ALL.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data.");
				}
				else
				{
					Fail("If you consider the new transaction type could follow existing Transaction Creation Restriction logic, please add it to allARAPTransactionTypes white list.");
				}
			}
			else
			{
				Assert(!Header.AH_OHInfo.Notifications.ContainsNotificationContaining("You cannot create transaction"));
			}
		}

		#region TestForARSequencingInPortugal

		public void TestARTransactionSequencingForPortugal()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var savedInvoice = Factory.NewWithValidTestData<ARInvoice>();
				savedInvoice.AH_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(3);
				Factory.Save();
				Assert(savedInvoice.IsInDatabase);
				Assert(savedInvoice.AH_SystemCreateTimeUtc > ZDateTime.UtcNow);

				var newInvoice = Factory.NewWithValidTestData<ARInvoice>();
				new TransactionHeaderValidation(newInvoice).ValidateAH_InvoiceDate();
				AssertHasError(newInvoice.AH_InvoiceDateInfo, "System date must be later than previous document");
			}
		}

		public void TestARTransactionSequencingForPortugalWhenCurrentTimeUTCGreaterThanPreviousTransaction()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var savedInvoice = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();
				Assert(savedInvoice.IsInDatabase);
				Assert(savedInvoice.AH_SystemCreateTimeUtc <= ZDateTime.UtcNow);

				var newInvoice = Factory.NewWithValidTestData<ARInvoice>();
				new TransactionHeaderValidation(newInvoice).ValidateAH_InvoiceDate();
				AssertNoErrors(newInvoice.AH_InvoiceDateInfo);
			}
		}

		public void TestTransactionSequencingShouldNotApplyForNonARTransactions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var savedInvoice = Factory.NewWithValidTestData<APInvoice>();
				savedInvoice.AH_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(3);
				Factory.Save();
				Assert(savedInvoice.IsInDatabase);
				Assert(savedInvoice.AH_SystemCreateTimeUtc > ZDateTime.UtcNow);

				var newInvoice = Factory.NewWithValidTestData<APInvoice>();
				new TransactionHeaderValidation(newInvoice).ValidateAH_InvoiceDate();
				AssertNoErrors(newInvoice.AH_InvoiceDateInfo);
			}
		}

		public void TestARTransactionSequencingShouldNotApplyForCountryOtherCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var savedInvoice = Factory.NewWithValidTestData<ARInvoice>();
				savedInvoice.AH_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(3);
				Factory.Save();
				Assert(savedInvoice.IsInDatabase);
				Assert(savedInvoice.AH_SystemCreateTimeUtc > ZDateTime.UtcNow);

				var newInvoice = Factory.NewWithValidTestData<ARInvoice>();
				new TransactionHeaderValidation(newInvoice).ValidateAH_InvoiceDate();
				AssertNoErrors(newInvoice.AH_InvoiceDateInfo);
			}
		}

		#endregion

		public virtual void TestCheckAH_GB_TaxBranch()
		{
			var anotherBranchInCurrentCompany = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "TST");
			GlbCompany.CurrentCompany.Factory.Save();

			Header.AH_GB_TaxBranch = TestObjectCreator.NonCurrentCompanyBranch.PK;
			AssertHasError(Header.AH_GB_TaxBranchInfo, "Enter a valid Tax Branch.");

			Header.AH_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			AssertNoErrors(Header.AH_GB_TaxBranchInfo);

			Header.AH_GB_TaxBranch = anotherBranchInCurrentCompany.PK;
			AssertNoErrors(Header.AH_GB_TaxBranchInfo);
		}

		void SetInvoices()
		{
			APInvoice aPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			APInvoice aPInvoice2 = Factory.NewWithValidTestData<APInvoice>();
			APInvoice aPInvoice3 = Factory.NewWithValidTestData<APInvoice>();
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData<ARInvoice>();

			aPInvoice1.AH_OH = Org1.PK;
			aPInvoice2.AH_OH = Org2.PK;
			aPInvoice3.AH_OH = Org1.PK;
			aRInvoice1.AH_OH = Org1.PK;

			aPInvoice1.AH_OutstandingAmount = -120m;
			aPInvoice1.AH_InvoiceAmount = -120m;
			aPInvoice2.AH_OutstandingAmount = -70m;
			aPInvoice2.AH_InvoiceAmount = -70m;
			aPInvoice3.AH_OutstandingAmount = -40m;
			aPInvoice3.AH_InvoiceAmount = -40m;
			aRInvoice1.AH_OutstandingAmount = 210m;
			aRInvoice1.AH_InvoiceAmount = 210m;

			Factory.Save();
		}

		OrgHeader fOrg1;
		protected OrgHeader Org1
		{
			get
			{
				if (fOrg1 == null)
				{
					fOrg1 = Factory.LoadTop1<OrgHeader>(new ZQuery());
				}
				return fOrg1;
			}
		}

		OrgHeader fOrg2;
		protected OrgHeader Org2
		{
			get
			{
				if (fOrg2 == null)
				{
					ZQuery filter = new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Org1.PK);
					fOrg2 = Factory.LoadTop1<OrgHeader>(filter);
				}
				return fOrg2;
			}
		}

		#region Implementation

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		protected TransactionHeader Header => header ?? (header = CreateHeader());
		TransactionHeader header;

		TransactionHeaderValidation HeaderValidation
		{
			get { return headerValidation ?? (headerValidation = new TransactionHeaderValidation(Header)); }
		}
		TransactionHeaderValidation headerValidation;

		protected virtual TransactionHeader CreateHeader() => (TransactionHeader)Factory.New(HeaderType);

		TransactionHeader CreateDirectPaymentAsPartOfDirectDebitBatch()
		{
			var header = (TransactionHeader)Factory.New(typeof(DirectPayment));
			var collection = new DirectDebitBatchLineCollection(Factory);
			collection.Add(header);
			return header;
		}

		#endregion
	}
}
