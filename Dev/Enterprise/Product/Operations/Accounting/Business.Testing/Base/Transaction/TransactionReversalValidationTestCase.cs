using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public abstract class TransactionReversalValidationTestCase : TestCaseWithFactory
	{
		public void TestConstructorException_WhenDataRefreshBusUpdateActionDeciderIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: dataRefreshBusUpdateActionDecider", () => new TransactionReversalValidation(Header , null));
			AssertNoExceptionThrown(() => new TransactionReversalValidation(Header , new Mock<IDataRefreshBusUpdateActionDecider>().Object));
		}

		public void TestValidateAlreadyGeneratedComplianceDocument()
		{
			if (ShouldCheckComplianceDocument)
			{
				string errorMessage = "You cannot reverse an INV or CRD that is linked to a compliance document record. You need to void all compliance document records related to the transaction before proceeding to reverse.";

				var vat3 = ObjectCreator.CreateTaxRate("VAT3", "VAT3", 3);
				vat3.AT_PostingGroupId = 0;

				var ac1 = ObjectCreator.CreateChargeCode("AC1");
				ac1.AC_AT_GSTRate = vat3.PK;

				var invoiceLine = (Header as InvoicingBase).Lines.AddNew() as InvoicingLineBase;
				invoiceLine.AL_JH = ObjectCreator.Job1.PK;
				invoiceLine.AL_AC = ac1.PK;
				invoiceLine.AL_AT = vat3.PK;

				ObjectCreator.CreateJobCharge(invoiceLine, ObjectCreator.Job1, ac1);
				ObjectCreator.CreateComplianceDocumentHeaderWithLine(Header.AH_Ledger, "desc", "0001", "NTC", "lineDesc", invoiceLine);
				Factory.Save();

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				(Header as IReversing).GenerateReverseTransaction(true);

				var reversingHeader = (Header as IReversing).ReverseTransaction as TransactionHeader;
				reversingHeader.Validation.ValidateAll();
				AssertHasRowError(reversingHeader, errorMessage);

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				reversingHeader.RemoveRowError(errorMessage, true);
				reversingHeader.Validation.ValidateAll();
				AssertHasRowError(reversingHeader, errorMessage);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestValidateAlreadyGeneratedComplianceDocument_ForWriteOffAsBadDebt()
		{
			if (ShouldCheckComplianceDocument && Header.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				var vat3 = ObjectCreator.CreateTaxRate("VAT3", "VAT3", 3);
				vat3.AT_PostingGroupId = 0;

				var ac1 = ObjectCreator.CreateChargeCode("AC1");
				ac1.AC_AT_GSTRate = vat3.PK;

				var invoiceLine1 = (Header as InvoicingBase).Lines.AddNew() as InvoicingLineBase;
				invoiceLine1.AL_JH = ObjectCreator.Job1.PK;
				invoiceLine1.AL_AC = ac1.PK;
				invoiceLine1.AL_AT = vat3.PK;

				ObjectCreator.CreateJobCharge(invoiceLine1, ObjectCreator.Job1, ac1);
				ObjectCreator.CreateComplianceDocumentHeaderWithLine(Header.AH_Ledger, "desc", "0001", "NTC", "lineDesc", invoiceLine1);
				Factory.Save();

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				(Header as IBadDebtWritingOff).IsWritingOff = true;
				(Header as IBadDebtWritingOff).GenerateReverseTransaction(true);

				var reversingHeader = (Header as IBadDebtWritingOff).ReverseTransaction as TransactionHeader;
				reversingHeader.Validation.ValidateAll();
				AssertNoRowErrors(reversingHeader);
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				reversingHeader.Validation.ValidateAll();
				AssertNoRowErrors(reversingHeader);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckTransactionNum_OnlyChecksAPInvCrdADJ()
		{
			if (ShouldCheckTransactionNum)
			{
				if (ShouldFillTransactionNumber)
				{
					Header.AH_TransactionNum = ZString.Empty;
				}
				Header.Validation.ValidateAH_TransactionNum();
				AssertEquals("HasErrors", true, Header.AH_TransactionNumInfo.HasErrors());

				Header.AH_TransactionNum = "123";
				AssertEquals("HasErrors", false, Header.AH_TransactionNumInfo.HasErrors());

				Header.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
				Header.IsSelfBillingInvoice = true;
				Header.AH_TransactionNum = ZString.Empty;
				Header.Validation.ValidateAH_TransactionNum();
				AssertEquals("Shouldn't validate for reversals of self billing invoices", false, Header.AH_TransactionNumInfo.HasErrors());
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckTransactionNum_ForNumbersAlreadyExist_Standard()
		{
			AssertCheckTransactionNum_ForNumbersAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today, ZDateTime.Now.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		public void TestCheckTransactionNum_ForNumbersAlreadyExist_Calendar()
		{
			AssertCheckTransactionNum_ForNumbersAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today, new ZDateTime(ZDateTime.Today.Year + 1, 1, 1));
		}

		void AssertCheckTransactionNum_ForNumbersAlreadyExist(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate1, ZDateTime invoiceDate2)
		{
			if (ShouldCheckTransactionNum)
			{
				using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
				{
					string existingTransactionNum = "TESTAP5787";

					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					TransactionHeader existingHeader = newFactory.NewWithValidTestData(HeaderType) as TransactionHeader;
					existingHeader.AH_OH = ObjectCreator.AALSHI.PK;
					existingHeader.AH_TransactionNum = existingTransactionNum;
					existingHeader.AH_InvoiceDate = invoiceDate1;
					newFactory.Save();

					Header.IsReverseTransaction = true;
					Header.AH_OH = ObjectCreator.AALSHI.PK;
					Header.AH_TransactionNum = existingTransactionNum;

					Header.AH_InvoiceDate = invoiceDate2;
					AssertHasError(Header.AH_TransactionNumInfo, "The transaction number is already in use. Please select another one.");
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckTransactionNum_DoesntCheckNumbersForOtherCompanies()
		{
			if (ShouldCheckTransactionNum)
			{
				string transactionNum = "TRANSACTIONNUM";
				var nonCurrentCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

				using (TestObjectCreator.SwitchEnvToBranch(nonCurrentCompanyBranch))
				{
					var factoryForOtherCompany = new BusinessObjectFactory();
					var postedHeaderInOtherCompany = factoryForOtherCompany.NewWithValidTestData(HeaderType) as TransactionHeader;
					postedHeaderInOtherCompany.AH_TransactionNum = transactionNum;
					postedHeaderInOtherCompany.AH_OH = ObjectCreator.AALSHI.PK;
					factoryForOtherCompany.Save();
				}

				Header.IsReverseTransaction = true;
				Header.AH_OH = ObjectCreator.AALSHI.PK;
				Header.AH_TransactionNum = transactionNum;
				AssertNoErrors("Should be no errors as saved invoice belongs to different company", Header.AH_TransactionNumInfo);

				var factoryForCurrentCompany = new BusinessObjectFactory();
				var postedHeaderInCurrentCompany = factoryForCurrentCompany.NewWithValidTestData(HeaderType) as TransactionHeader;
				postedHeaderInCurrentCompany.AH_TransactionNum = transactionNum;
				postedHeaderInCurrentCompany.AH_OH = ObjectCreator.AALSHI.PK;
				factoryForCurrentCompany.Save();

				Header.Validation.ValidateAH_TransactionNum();
				AssertHasErrors("Should be an error as saved invoice belongs to the same company", Header.AH_TransactionNumInfo);
				AssertHasError(Header.AH_TransactionNumInfo, "The transaction number is already in use. Please select another one.");
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckTransactionNum_ForUANumbersAlreadyExist_Standard()
		{
			AssertCheckTransactionNum_ForUANumbersAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today, ZDateTime.Now.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		public void TestCheckTransactionNum_ForUANumbersAlreadyExist_Calendar()
		{
			AssertCheckTransactionNum_ForUANumbersAlreadyExist(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today, new ZDateTime(ZDateTime.Today.Year + 1, 1, 1));
		}

		void AssertCheckTransactionNum_ForUANumbersAlreadyExist(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate1, ZDateTime invoiceDate2)
		{
			if (ShouldCheckTransactionNum && (HeaderType == typeof(APInvoice) || HeaderType == typeof(APCreditNote)))
			{
				using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
				{
					string existingTransactionNum = "TESTUA5787";
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					TransactionHeader existingHeader = null;
					if (HeaderType == typeof(APInvoice))
					{
						existingHeader = newFactory.NewWithValidTestData(typeof(UAInvoice)) as TransactionHeader;
					}
					else if (HeaderType == typeof(APCreditNote))
					{
						existingHeader = newFactory.NewWithValidTestData(typeof(UACreditNote)) as TransactionHeader;
					}

					existingHeader.AH_OH = ObjectCreator.AALSHI.PK;
					existingHeader.AH_TransactionNum = existingTransactionNum;
					existingHeader.AH_InvoiceDate = invoiceDate1;
					newFactory.Save();

					Header.IsReverseTransaction = true;
					Header.AH_OH = ObjectCreator.AALSHI.PK;
					Header.AH_TransactionNum = existingTransactionNum;

					Header.AH_InvoiceDate = invoiceDate2;
					AssertHasError(Header.AH_TransactionNumInfo, "The transaction number is already in use by Unapproved Invoice. Please select another one.");
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckTransactionNum_DoesntCheckUANumbersForOtherCompanies()
		{
			if (ShouldCheckTransactionNum && (HeaderType == typeof(APInvoice) || HeaderType == typeof(APCreditNote)))
			{
				var uaHeaderType = HeaderType == typeof(APInvoice)    ? typeof(UAInvoice)
								 : HeaderType == typeof(APCreditNote) ? typeof(UACreditNote)
								 : throw new InvalidOperationException("Unexpected type: " + HeaderType.FullName);
				string transactionNum = "TRANSACTIONNUM";

				var nonCurrentCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

				using (TestObjectCreator.SwitchEnvToBranch(nonCurrentCompanyBranch))
				{
					var newFactoryForOtherCompany = new BusinessObjectFactory();
					var postedHeaderInOtherCompany = newFactoryForOtherCompany.NewWithValidTestData(uaHeaderType) as TransactionHeader;
					postedHeaderInOtherCompany.AH_TransactionNum = transactionNum;
					postedHeaderInOtherCompany.AH_OH = ObjectCreator.AALSHI.PK;
					newFactoryForOtherCompany.Save();
				}

				Header.IsReverseTransaction = true;
				Header.AH_OH = ObjectCreator.AALSHI.PK;
				Header.AH_TransactionNum = transactionNum;
				AssertNoErrors("Should be no errors as saved invoice belongs to different company", Header.AH_TransactionNumInfo);

				var newFactoryForCurrentCompany = new BusinessObjectFactory();
				var postedHeaderInCurrentCompany = newFactoryForCurrentCompany.NewWithValidTestData(uaHeaderType) as TransactionHeader;
				postedHeaderInCurrentCompany.AH_TransactionNum = transactionNum;
				postedHeaderInCurrentCompany.AH_OH = ObjectCreator.AALSHI.PK;
				newFactoryForCurrentCompany.Save();

				Header.Validation.ValidateAH_TransactionNum();
				AssertHasErrors("Should be an error as saved invoice belongs to the same company", Header.AH_TransactionNumInfo);
				AssertHasError(Header.AH_TransactionNumInfo, "The transaction number is already in use by Unapproved Invoice. Please select another one.");
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCheckTransactionNum_ForNumbersUsedInJobInvoicing()
		{
			if (ShouldCheckTransactionNum)
			{
				string existingTransactionNum = "TESTAP5787";

				TransactionHeader existingHeader = Factory.NewWithValidTestData(HeaderType) as TransactionHeader;
				existingHeader.AH_TransactionNum = existingTransactionNum;
				existingHeader.AH_OH = ObjectCreator.AALSHI.PK;
				existingHeader.AH_InvoiceDate = ZDateTime.Today;
				Factory.Save();

				Header.IsReverseTransaction = true;
				Header.AH_TransactionNum = "000000000 0";
				Header.AH_OH = ObjectCreator.AALSHI.PK;
				Header.Validation.ValidateAH_TransactionNum();

				AssertNoErrors(Header.AH_OHInfo);
				Assert(!Header.AH_TransactionNumInfo.HasError("The transaction number is already in use on Job Invoicing. Please select another one."));
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2006, 11, 15)]
		public void TestCheckAH_PostDate()
		{
			ZDateTime originalPostDate = ZDateTime.Today.AddDays(-5);
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			AccPeriodManagement accPeriod = periodCalculator.GetPeriodManagementFromDate(originalPostDate);

			Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;
			if (accPeriod == null)
			{
				accPeriod = Factory.New<AccPeriodManagement>();
				accPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
				accPeriod.AM_Period = periodCalculator.GetPeriodFromDate(originalPostDate);
				accPeriod.AM_Year = (short)(accPeriod.AM_Period / 100);
				accPeriod.AM_StartDate = new ZDateTime(originalPostDate.Year, originalPostDate.Month, 1);
				accPeriod.AM_EndDate = accPeriod.AM_StartDate.AddMonths(1).AddDays(-1);
			}
			Header.AH_PostDate = originalPostDate;

			Header.GenerateReverseTransaction(true);
			TransactionHeader reverseTransaction = Header.ReverseTransaction as TransactionHeader;
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseTransaction.AH_PostDateInfo.HasErrors());

			reverseTransaction.AH_PostDate = originalPostDate.AddDays(-1);
			AssertEquals("AH_PostDateInfo.HasErrors()", true, reverseTransaction.AH_PostDateInfo.HasErrors());
			string expectedError = "Reversing post date cannot be before the original post date of '" + originalPostDate.ToShortDateString() + "'.";
			AssertEquals("Contains(ExpectedError)", true, reverseTransaction.AH_PostDateInfo.GetErrors().Contains(expectedError));

			reverseTransaction.AH_PostDate = originalPostDate;
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseTransaction.AH_PostDateInfo.HasErrors());

			reverseTransaction.AH_PostDate = originalPostDate.AddDays(1);
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseTransaction.AH_PostDateInfo.HasErrors());

			//reverseTransaction.AddWritableProperties(reverseTransaction.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name != reverseTransaction.AH_PostDateInfo.Name).Select(x => x.Name).ToArray());
			reverseTransaction.AddWritableProperties(Array.Empty<string>());
			reverseTransaction.AH_PostDate = originalPostDate.AddDays(-1);
			AssertEquals("expect AH_PostDateInfo no error because AH_PostDate is read only", false, reverseTransaction.AH_PostDateInfo.HasErrors());

			reverseTransaction.Factory.SetContext(BusinessContext.ReverseDateForm);
			reverseTransaction.AH_PostDate = originalPostDate.AddDays(-2);
			AssertEquals("expect AH_PostDateInfo has error because factory has ReverseDateForm context", true, reverseTransaction.AH_PostDateInfo.HasErrors());
		}

		[TestDate(2021, 06, 01)]
		public void TestCheckAH_PostDateReversalAllowed_WhenAnyOriginalLineIsTaxable()
		{
			OrgHeader orgHeader = TestObjectCreator.ABIGAS;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var periodHelper = new AccountingPeriodTestHelper();
				periodHelper.PostPeriodsForEntireYear(2021);
				periodHelper.PostPeriodsForEntireYear(2020);
				periodHelper.PostPeriodsForEntireYear(2022);

				AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
				Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;

				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), 1000m, 2000m);
				AssertEquals("PreCondition", 2, AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value);

				var originalInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
				originalInvoice.AH_PostDate = new ZDate(2021, 03, 20);
				var line1 = TestObjectCreator.CreateARInvoiceLine(originalInvoice, null, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1m, "GST line", 10m);
				line1.AL_AT = TestObjectCreator.GST1.PK;
				Assert("PreCondition: one line is applicable", IndiaGSTReversalHelper.CheckIsConstraintTaxID(line1.TaxRate));
				var line2 = TestObjectCreator.CreateARInvoiceLine(originalInvoice, null, TestObjectCreator.RevenueNoTaxChargeCode, TestObjectCreator.AUD, 1m, "No GST line", 20m);
				line2.AL_AT = TestObjectCreator.ExcludedTax.PK;
				Assert("PreCondition: one line is not applicable", !IndiaGSTReversalHelper.CheckIsConstraintTaxID(line2.TaxRate));

				originalInvoice.GenerateReverseTransaction(true);
				TransactionHeader reverseTransaction = originalInvoice.ReverseTransaction as TransactionHeader;
				reverseTransaction.AH_PostDate = ZDate.Today;

				AssertHasError("When any original invoice line is taxable (according to India constraint), there should be a validation error.", reverseTransaction.AH_PostDateInfo, IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_ReverseINV);
			}
		}

		[TestDate(2021, 06, 01)]
		public void TestCheckAH_PostDateReversalAllowed_WhenNotIndia()
		{
			OrgHeader orgHeader = TestObjectCreator.ABIGAS;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Iceland))
			{
				var periodHelper = new AccountingPeriodTestHelper();
				periodHelper.PostPeriodsForEntireYear(2021);
				periodHelper.PostPeriodsForEntireYear(2020);
				periodHelper.PostPeriodsForEntireYear(2022);

				AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
				Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;

				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), 1000m, 2000m);
				AssertEquals("PreCondition", 2, AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value);

				var originalInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
				originalInvoice.AH_PostDate = new ZDate(2021, 03, 20);
				var line1 = TestObjectCreator.CreateARInvoiceLine(originalInvoice, null, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1m, "GST line", 10m);
				line1.AL_AT = TestObjectCreator.GST1.PK;

				originalInvoice.GenerateReverseTransaction(true);
				TransactionHeader reverseTransaction = originalInvoice.ReverseTransaction as TransactionHeader;
				reverseTransaction.AH_PostDate = ZDate.Today;

				AssertNoErrors("The Post Date reversal allowed rule should only apply to India login companies.", reverseTransaction.AH_PostDateInfo);
			}
		}

		[TestDate(2021, 06, 01)]
		public void TestCheckAH_PostDateReversalAllowed_WhenPostDateIsReadOnly()
		{
			OrgHeader orgHeader = TestObjectCreator.ABIGAS;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var periodHelper = new AccountingPeriodTestHelper();
				periodHelper.PostPeriodsForEntireYear(2021);
				periodHelper.PostPeriodsForEntireYear(2020);
				periodHelper.PostPeriodsForEntireYear(2022);

				AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
				Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;

				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), 1000m, 2000m);
				AssertEquals("PreCondition", 2, AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value);

				var originalInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
				originalInvoice.AH_PostDate = new ZDate(2021, 03, 20);
				var line1 = TestObjectCreator.CreateARInvoiceLine(originalInvoice, null, TestObjectCreator.RevenueChargeCode, TestObjectCreator.AUD, 1m, "GST line", 10m);
				line1.AL_AT = TestObjectCreator.GST1.PK;

				originalInvoice.GenerateReverseTransaction(true);
				TransactionHeader reverseTransaction = originalInvoice.ReverseTransaction as TransactionHeader;

				reverseTransaction.AddWritableProperties(Array.Empty<string>());
				Assert("Precondition: Post Date is read only", reverseTransaction.AH_PostDateInfo.ReadOnly);
				reverseTransaction.AH_PostDate = ZDate.Today;
				AssertHasError("Validation should be run for Post Date reversal allowed rule even when field is read only.", reverseTransaction.AH_PostDateInfo, IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_ReverseINV);

				reverseTransaction.AH_PostDate = originalInvoice.AH_PostDate;
				AssertNoError("Validation should pass when the original invoice date is used.", reverseTransaction.AH_PostDateInfo, IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_ReverseINV);

				using (reverseTransaction.GetValidationSuspender())
				{
					reverseTransaction.AH_PostDate = ZDate.Today;
				}
				reverseTransaction.RunPreSaveValidation();
				AssertHasError("Validation should be run for Post Date reversal allowed rule during save even when field is read only and has not been validated.", reverseTransaction.AH_PostDateInfo, IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_ReverseINV);
			}
		}

		public void TestMultipleReversingErrors()
		{
			var mockDataRefreshBusUpdateActionDecider = new Mock<IDataRefreshBusUpdateActionDecider>();

			var testValidation = new TransactionReversalValidation(Header, mockDataRefreshBusUpdateActionDecider.Object);

			Header.MultipleReversingErrors = null;
			testValidation.ValidateAll();
			AssertNoRowErrors(Header);

			Header.MultipleReversingErrors = new string[] { "error 1", "error 2" };
			testValidation.ValidateAll();
			AssertHasRowError(Header, "error 1");
			AssertHasRowError(Header, "error 2");
		}

		public void TestAlreadyReversedError()
		{
			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
			TestObjectCreator.Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var invoiceCopy = newFactory.Load<APInvoice>(invoice.PK);

			string cantReverseReason;
			var reversedTransaction = (InvoicingBase)TestObjectCreator.ReverseTransaction(invoice, out cantReverseReason);
			Assert("Precondition: invoice must be valid for reversing.", string.IsNullOrEmpty(cantReverseReason));

			reversedTransaction.AH_TransactionNum = invoice.AH_TransactionNum;
			TestObjectCreator.Factory.Save();

			((IReversing)invoiceCopy).GenerateReverseTransaction(true);
			var reversingHeader = ((IReversing)invoiceCopy).ReverseTransaction as TransactionHeader;

			reversingHeader.Validation.ValidateAll();
			AssertHasRowError(reversingHeader, "This transaction was already reversed by another user.");
		}

		[ExpectNoExceptions]
		public void TestValidateDataRefreshBusUpdate_HasSkippedDataRefreshBusUpdate_OrignialTransactionIsPassed()
		{
			Header.GenerateReverseTransaction(true);
			var reverseTransaction = Header.ReverseTransaction as TransactionHeader;

			var mockDataRefreshBusUpdateActionDecider = new Mock<IDataRefreshBusUpdateActionDecider>();

			var testValidation = new TransactionReversalValidation(reverseTransaction, mockDataRefreshBusUpdateActionDecider.Object);
			AssertNotNull("Precondition: Original Transaction", reverseTransaction.OriginalTransaction);

			testValidation.ValidateAll();
			mockDataRefreshBusUpdateActionDecider.Verify(x => x.HasSkippedDataRefreshBusUpdate(Header), Times.Once());
		}

		[ExpectNoExceptions]
		public void TestValidateDataRefreshBusUpdate_HasSkippedDataRefreshBusUpdate_OrignialTransactionIsNull()
		{
			Header.GenerateReverseTransaction(true);
			var reverseTransaction = Header.ReverseTransaction as TransactionHeader;
			reverseTransaction.OriginalTransaction = null;

			var mockDataRefreshBusUpdateActionDecider = new Mock<IDataRefreshBusUpdateActionDecider>();

			var testValidation = new TransactionReversalValidation(reverseTransaction, mockDataRefreshBusUpdateActionDecider.Object);
			AssertNull("Precondition: Original Transaction", reverseTransaction.OriginalTransaction);

			testValidation.ValidateAll();
			mockDataRefreshBusUpdateActionDecider.Verify(x => x.HasSkippedDataRefreshBusUpdate(It.IsAny<TransactionHeader>()), Times.Never());

			reverseTransaction.OriginalTransaction = Header;
			testValidation.ValidateAll();
			mockDataRefreshBusUpdateActionDecider.Verify(x => x.HasSkippedDataRefreshBusUpdate(It.IsAny<TransactionHeader>()), Times.Once());
		}

		public void TestValidateDataRefreshBusUpdate_CheckRowErrorMessage()
		{
			Header.GenerateReverseTransaction(true);
			var errorMessage = "Original Transaction was modified by this user during another operation. Please cancel your changes and reload the form.";
			var reverseTransaction = Header.ReverseTransaction as TransactionHeader;

			var mockDataRefreshBusUpdateActionDecider = new Mock<IDataRefreshBusUpdateActionDecider>();
			mockDataRefreshBusUpdateActionDecider.Setup(x => x.HasSkippedDataRefreshBusUpdate(It.IsAny<TransactionHeader>())).Returns(false);

			var testValidation = new TransactionReversalValidation(reverseTransaction, mockDataRefreshBusUpdateActionDecider.Object);
			AssertNotNull("Precondition: Original Transaction", reverseTransaction.OriginalTransaction);

			testValidation.ValidateAll();
			AssertNoRowErrorContaining(reverseTransaction, errorMessage);

			mockDataRefreshBusUpdateActionDecider.Setup(x => x.HasSkippedDataRefreshBusUpdate(It.IsAny<TransactionHeader>())).Returns(true);
			testValidation.ValidateAll();
			AssertHasRowError(reverseTransaction, errorMessage);
		}

		public void TestCheckAH_InvoiceDate_DefaultBlank()
		{
			using (AccountingMasterFilesRegistry.Instance.InvoiceDateDefaultValue.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.InvoiceDateDefaultValueCodes.Blank))
			{
				var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
				invoice.AH_InvoiceDate = ZDateTime.Today;
				TestObjectCreator.Factory.Save();

				((IReversing)invoice).GenerateReverseTransaction(true);
				var reversingHeader = ((IReversing)invoice).ReverseTransaction as TransactionHeader;

				reversingHeader.Validation.ValidateAll();
				AssertEquals("AH_InvoiceDate.HasErrors()", true, reversingHeader.AH_InvoiceDateInfo.HasErrors());
				AssertEquals("Has AH_InvoiceDate empty error message.", true, reversingHeader.AH_InvoiceDateInfo.GetErrors().Select(x => x.Message).Contains("Please enter an Invoice Date."));
			}
		}

		#region Implementation

		protected TransactionHeader Header;
		protected AccountingPeriodCalculator PeriodCalculator;
		bool AllowBackPosting;
		bool ReceivablesAllowed;
		bool PayablesAllowed;
		bool CashBookAllowed;
		TestObjectCreator testObjectCreator;

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
				}
				return testObjectCreator;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			AllowBackPosting = AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value;
			ReceivablesAllowed = Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
			PayablesAllowed = Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
			CashBookAllowed = Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed;

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			Header = Factory.New(HeaderType) as TransactionHeader;
			if (ShouldFillTransactionNumber)
			{
				Header.AH_TransactionNum = "Test001";
			}
			PeriodCalculator = new AccountingPeriodCalculator(Factory);
		}

		protected virtual bool ShouldFillTransactionNumber => false;

		protected override void TearDown()
		{
			base.TearDown();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AllowBackPosting);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = ReceivablesAllowed;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = PayablesAllowed;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = CashBookAllowed;
		}

		protected virtual Type HeaderType
		{
			get { return typeof(ARInvoice); }
		}

		#region ObjectCreator

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}
				return fObjectCreator;
			}
		}
		TestObjectCreator fObjectCreator;

		#endregion

		bool ShouldCheckTransactionNum
		{
			get
			{
				return Header.AH_Ledger == LedgerTypes.AccountsPayable &&
													!Header.IsSelfBillingInvoice &&
					(Header.AH_TransactionType == TransactionTypes.Invoice ||
					Header.AH_TransactionType == TransactionTypes.CreditNote ||
					Header.AH_TransactionType == TransactionTypes.AdjustmentNote);
			}
		}

		bool ShouldCheckComplianceDocument
		{
			get
			{
				return (Header.AH_Ledger == LedgerTypes.AccountsReceivable || Header.AH_Ledger == LedgerTypes.AccountsPayable)
					&& (Header.AH_TransactionType == TransactionTypes.Invoice || Header.AH_TransactionType == TransactionTypes.CreditNote);
			}
		}

		#endregion
	}
}
