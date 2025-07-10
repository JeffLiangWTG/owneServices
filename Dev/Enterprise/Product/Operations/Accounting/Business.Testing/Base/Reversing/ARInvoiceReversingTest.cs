using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class ARInvoiceReversingTest : InvoicingBaseReversingTest
	{
		#region TestCantReverseTransactionForPaidRelatedInvoices

		public void TestCantReverseTransactionForPaidRelatedInvoices()
		{
			var invoice_NoRelatedInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "12341234", TestObjectCreator.AUD, 1.0m, 1000.00m, 100.00m, 1000.00m, 100.00m);
			invoice_NoRelatedInvoice.AH_TransactionCategory = "FIN";
			var invoice_NoRelatedSelfBilledInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "12341SBC", TestObjectCreator.AUD, 1.0m, 1000.00m, 100.00m, 1000.00m, 100.00m);
			invoice_NoRelatedSelfBilledInvoice.AH_TransactionCategory = "SBR";
			Factory.Save();
			Assert("Precondition: Standard Invoice", !invoice_NoRelatedInvoice.IsSelfBillingInvoice);
			Assert("Precondition: Self-Billed Invoice", invoice_NoRelatedSelfBilledInvoice.IsSelfBillingInvoice);

			Reversing = ReversingFactory.NewReversing(invoice_NoRelatedInvoice);
			var selfBilledARInvoiceReversing = (ARInvoiceReversing)ReversingFactory.NewReversing(invoice_NoRelatedSelfBilledInvoice);
			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = false;
			Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.IsAllowed = false;

			AssertEquals("CanReverseTransaction when rights for transaction with related paid invoice.", true, ARInvoiceReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when rights for transaction with related paid invoice.", "", ARInvoiceReversing.GenerateCantReverseErrorMessage_ForTestOnly());
			AssertEquals("CanReverseTransaction when rights for transaction with related paid invoice.", true, selfBilledARInvoiceReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when rights for transaction with related paid invoice.", "", selfBilledARInvoiceReversing.GenerateCantReverseErrorMessage_ForTestOnly());

			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = true;
			Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.IsAllowed = true;

			AssertEquals("CanReverseTransaction when rights for transaction with related paid invoice.", true, ARInvoiceReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when rights for transaction with related paid invoice.", "", ARInvoiceReversing.GenerateCantReverseErrorMessage_ForTestOnly());
			AssertEquals("CanReverseTransaction when rights for transaction with related paid invoice.", true, selfBilledARInvoiceReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when rights for transaction with related paid invoice.", "", selfBilledARInvoiceReversing.GenerateCantReverseErrorMessage_ForTestOnly());

			var inv1_HasRelatedInvoice = SetupInvoiceWithPaidRelatedInvoices();
			inv1_HasRelatedInvoice.AH_TransactionCategory = "FIN";
			var invSelfBilled_HasRelatedInvoice = SetupInvoiceWithPaidRelatedInvoices();
			invSelfBilled_HasRelatedInvoice.AH_TransactionCategory = "SBR";
			Factory.Save();

			Assert("Precondition: Standard Invoice", !inv1_HasRelatedInvoice.IsSelfBillingInvoice);
			Assert("Precondition: Self-Billed Invoice", invSelfBilled_HasRelatedInvoice.IsSelfBillingInvoice);

			Reversing = ReversingFactory.NewReversing(inv1_HasRelatedInvoice);
			selfBilledARInvoiceReversing = (ARInvoiceReversing)ReversingFactory.NewReversing(invSelfBilled_HasRelatedInvoice);
			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = false;
			Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.IsAllowed = true;

			AssertEquals("CanReverseTransaction when no rights for transaction with related paid invoice.", false, ARInvoiceReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when no rights for transaction with related paid invoice.",
				Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.ErrorMessageForNotAllowed, ARInvoiceReversing.GenerateCantReverseErrorMessage_ForTestOnly());
			AssertEquals("CanReverseTransaction when rights for self billed transaction with related paid invoice.", true, selfBilledARInvoiceReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when rights for self billed transaction with related paid invoice.", "", selfBilledARInvoiceReversing.GenerateCantReverseErrorMessage_ForTestOnly());

			inv1_HasRelatedInvoice.Factory.SetContext(BusinessContext.PeriodicInvoicePosting);
			AssertEquals("CanReverseTransaction when no rights for transaction with related paid invoice, but in Periodic Invoice context.", true, ARInvoiceReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when no rights for transaction with related paid invoice, but in Periodic Invoice context.",
				"", ARInvoiceReversing.GenerateCantReverseErrorMessage_ForTestOnly());
			inv1_HasRelatedInvoice.Factory.RemoveContext(BusinessContext.PeriodicInvoicePosting);

			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = true;
			Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.IsAllowed = false;
			inv1_HasRelatedInvoice.PaidRelatedInvoicesSecurityCertificate = null;
			invSelfBilled_HasRelatedInvoice.PaidRelatedSelfBilledInvoicesSecurityCertificate = null;

			AssertEquals("CanReverseTransaction when rights for transaction with related paid invoice.", true, ARInvoiceReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when rights for transaction with related paid invoice.", "", ARInvoiceReversing.GenerateCantReverseErrorMessage_ForTestOnly());
			AssertEquals("CanReverseTransaction when no rights for self billed transaction with related paid invoice.", false, selfBilledARInvoiceReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when no rights for self billed transaction with related paid invoice.",
				Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.ErrorMessageForNotAllowed, selfBilledARInvoiceReversing.GenerateCantReverseErrorMessage_ForTestOnly());

			inv1_HasRelatedInvoice = new BusinessObjectFactory().Load<ARInvoice>(inv1_HasRelatedInvoice.PK);
			Reversing = ReversingFactory.NewReversing(inv1_HasRelatedInvoice);
			AssertEquals("CanReverseTransaction with needed rights.", true, ARInvoiceReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage with needed rights.", "", ARInvoiceReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		[TestDate(2025, 5, 1)]
		public void TestCanReverseWhenLoginInCompanyIsVietnam()
		{
			var originalInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.VND, 1m, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateEInvoicingTransactionPivot(originalInvoice, status: EInvoicingPivotState.BatchedWithError);
			Factory.Save();

			var reversing = ReversingFactory.NewReversing(originalInvoice);

			AssertEquals("Should be able to reverse transaction", true, reversing.CanReverseTransaction);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.VietNam))
			{
				var originalInvoiceVN = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.VND, 1m, TestObjectCreator.ABIGAS);
				var pivotVN = TestObjectCreator.CreateEInvoicingTransactionPivot(originalInvoiceVN, status: EInvoicingPivotState.Succeed);
				Factory.Save();

				var reversing1 = ReversingFactory.NewReversing(originalInvoiceVN);

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Should be able to reverse transaction with Succeed Status and EInvoicing is enabled.", true, reversing1.CanReverseTransaction);
				}

				AssertEquals("Should not be able to reverse transaction with Succeed Status and EInvoicing is not enabled.", false, reversing1.CanReverseTransaction);

				pivotVN.AIP_Status = EInvoicingPivotState.Discarded;
				Factory.Save();

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Should be able to reverse transaction with Discarded Status and EInvoicing is enabled.", true, reversing1.CanReverseTransaction);
				}

				AssertEquals("Should not be able to reverse transaction with Discarded Status", false, reversing1.CanReverseTransaction);

				pivotVN.AIP_Status = EInvoicingPivotState.BatchedWithError;
				Factory.Save();

				var expectedError = "Invoices can only be reversed when e-Reporting status is 'SUC' or 'DCD'";
				AssertEquals("Should not be able to reverse transaction", false, reversing1.CanReverseTransaction);
				AssertEquals("Error message", expectedError, reversing1.GenerateCantReverseErrorMessage_ForTestOnly());

				var originalInvoiceForEmptyStatus = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.VND, 1m, TestObjectCreator.ABIGAS);
				var reversingForEmptyStatus = ReversingFactory.NewReversing(originalInvoiceForEmptyStatus);
				AssertEquals("Should be able to reverse transaction with empty status", true, reversingForEmptyStatus.CanReverseTransaction);

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Should be able to reverse transaction with empty status", true, reversingForEmptyStatus.CanReverseTransaction);
				}
			}
		}

		public void TestAlreadyReversedTNFTransactionWillNotThrowNullExceptionWhenReverseAgain()
		{
			AssertAlreadyReversedTransactionWillNotThrowNullExceptionWhenReverseAgain("TNF");
		}

		public void TestAlreadyReversedTAPTransactionWillNotThrowNullExceptionWhenReverseAgain()
		{
			AssertAlreadyReversedTransactionWillNotThrowNullExceptionWhenReverseAgain("TAP");
		}

		void AssertAlreadyReversedTransactionWillNotThrowNullExceptionWhenReverseAgain(string category)
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "12341234", TestObjectCreator.AUD, 1.0m, 1000.00m, 100.00m, 1000.00m, 100.00m);
			invoice.AH_TransactionCategory = category;
			invoice.AH_IsCancelled = true;

			Reversing = ReversingFactory.NewReversing(invoice);

			Assert("CanReverseTransaction when rights for transaction with related paid invoice.", !ARInvoiceReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage should not throw a null ref exception.", @"This TAP/TNF Journal cannot be reversed.
TAP/TNF journals are created in a pair. Reversing one part will result in an imbalance in the Clearing GL Account.", ARInvoiceReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestLoginFormMustPopupOnlyOnceWhenDoCheckForPaidRelatedInvoices()
		{
			InvoicingBase inv1_HasRelatedInvoice = SetupInvoiceWithPaidRelatedInvoices();

			Reversing = ReversingFactory.NewReversing(inv1_HasRelatedInvoice);

			TestSecurityOverrideProvider securityOverrideProvider = new TestSecurityOverrideProvider();
			securityOverrideProvider.OnRequestLoginCredentialsCall += new EventHandler(securityOverrideProvider_OnRequestLoginCredentialsCall);
			inv1_HasRelatedInvoice.SecurityOverrideProvider = securityOverrideProvider;

			Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.IsAllowed = false;
			WasRequestLoginCredentialsCalled = false;
			AssertEquals("CanReverseTransaction when no rights for transaction with related paid invoice.", false, ARInvoiceReversing.CanReverseTransaction);
			Assert("RequestLoginCredentialsCalled must be called.", WasRequestLoginCredentialsCalled);
			WasRequestLoginCredentialsCalled = false;
			AssertEquals("GenerateCantReverseErrorMessage when no rights for transaction with related paid invoice.",
				Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.ErrorMessageForNotAllowed, ARInvoiceReversing.GenerateCantReverseErrorMessage_ForTestOnly());
			Assert("RequestLoginCredentialsCalled must be called only ones.", !WasRequestLoginCredentialsCalled);
		}

		public void TestCantReverseARInvoiceWhenNotAllowedCreation()
		{
			AssertCreationOfReversalTransactionReceivable(false);
			AssertCreationOfReversalTransactionReceivable(true);
		}

		void AssertCreationOfReversalTransactionReceivable(bool reverseIsPrevented)
		{
			using (AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reverseIsPrevented))
			{
				ARInvoice originalInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
				var reversing = new ARInvoiceReversing(originalInvoice);
				AssertEquals("CanTransactionBeReversed", !reverseIsPrevented, reversing.CanTransactionBeReversed_ForTestOnly());
				if (reverseIsPrevented)
				{
					AssertEquals("GenerateCantReverseErrorMessage()", AccountingMasterFilesUtils.ARInvoiceReversalDisallowedMessage, reversing.GenerateCantReverseErrorMessage_ForTestOnly());
				}
				else
				{
					AssertNotEquals("GenerateCantReverseErrorMessage()", AccountingMasterFilesUtils.ARInvoiceReversalDisallowedMessage, reversing.GenerateCantReverseErrorMessage_ForTestOnly());
				}
			}
		}

		public void TestInvoiceReversalDeletesTaxExpenseRecoveryChargeOnJob()
		{
			AccountingMasterFilesRegistry.Instance.RevenueTaxExpenseRecoveryChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestObjectCreator.CC2.PK.ToGuid());

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1000", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 100m);
			var recoveryLine = TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 10m);
			line.AL_AC = TestObjectCreator.CC1.PK;
			recoveryLine.AL_AC = TestObjectCreator.CC2.PK;
			line.AL_JH = recoveryLine.AL_JH = TestObjectCreator.Job1.PK;
			line.AL_GB = recoveryLine.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = recoveryLine.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var charge1 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Charge Code 1", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			var charge2 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC2, "Tax expense recovery charge", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 10M, TestObjectCreator.ABIGAS);

			charge1.JR_AL_ARLine = line.PK;
			charge2.JR_AL_ARLine = recoveryLine.PK;

			Factory.Save();

			AssertEquals("Pre-condition: Number of charges on job before invoice reversal", 2, TestObjectCreator.Job1.Charges.Count);

			var reversing = new ARInvoiceReversing(invoice);
			reversing.Reverse();

			Factory.Save();

			var charges = TestObjectCreator.Job1.Charges;
			AssertEquals("Number of charges on job after invoice reversal", 1, charges.Count);
			AssertEquals(charge1.PK, charges[0].PK);
		}

		[TestDate(2021, 06, 01)]
		public void TestCheckIndiaGSTReversalAllowedPeriod()
		{
			var dateWillFail = new ZDateTime(2021, 03, 20);
			var dateWillPass = new ZDateTime(2021, 04, 01);

			AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
			AssertEquals("PreCondition", 2, AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value);

			AssertIndia();
			AssertNotIndia();

			void AssertIndia()
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry("IN"))
				{
					AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
					AssertEquals("PreCondition", 2, AccountingConfigurationRegistry.Instance.IndiaGSTReversalAllowedPeriod.Value);

					Guid currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
					Guid currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();
					TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(currentBranch, currentDepartment, 1000m, 2000m);

					var periodManagementTestHelper = new AccountingPeriodTestHelper();
					periodManagementTestHelper.PostPeriodsForEntireYear(2021);
					periodManagementTestHelper.PostPeriodsForEntireYear(2020);
					periodManagementTestHelper.PostPeriodsForEntireYear(2022);

					var tax = TestObjectCreator.CreateTaxRate("TAX1", "", 10);
					AssertEquals("PreCondition", "IN", Env.CurrentCompany.Country.Code);

					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
					var line = (InvoicingLineBase)invoice.Lines.AddNew();
					line.AL_AT = tax.PK;

					Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;
					Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
					CombineAssertions("only IntegratedGST(INT) and Rated(RAT) are GST target", () =>
					{
						foreach (var taxType in tax.Lookups.Types.GetAllCodes())
						{
							tax.AT_Type = taxType;
							if (IndiaGSTReversalHelper.CheckIsConstraintTaxID(line.TaxRate))
							{
								invoice.AH_PostDate = dateWillFail;
								var reversingFail = ReversingFactory.NewReversing(invoice);
								AssertEquals("Financial Year End:2021-03-31, Current Date:2021-06-01 ,Credit Note form should not be blocked, validation rules applied in transaction header validation",
									true,
									reversingFail.CanReverseTransaction);
								AssertEquals("Financial Year End:2021-03-31, Current Date:2021-06-01 ,Credit note form should be allowed to open, validation rules applied in transaction header validation",
									ZString.Empty,
									reversingFail.GenerateCantReverseErrorMessage_ForTestOnly());

								invoice.AH_PostDate = dateWillPass;
								var reversingPass = ReversingFactory.NewReversing(invoice);
								AssertEquals("Financial Year End:2022-03-31, Current Date:2021-06-01 ,it should be pass due to within 2 month offset",
									true,
									reversingPass.CanReverseTransaction);

								AssertEquals("Financial Year End:2021-03-31, Current Date:2021-04-01 ,Credit note form should be allowed to open",
									ZString.Empty,
									reversingFail.GenerateCantReverseErrorMessage_ForTestOnly());
							}
							else
							{
								invoice.AH_PostDate = dateWillFail;
								var reversingPass = ReversingFactory.NewReversing(invoice);
								AssertEquals($"Should not fail due to Tax Type:{taxType} not validation Constraint",
									true,
									reversingPass.CanReverseTransaction);
							}
						}
					});

					line.AL_AT = ZGuid.Empty;
					invoice.AH_PostDate = dateWillFail;
					var reversingPassWhenNoTaxInLine = ReversingFactory.NewReversing(invoice);
					AssertEquals("Should not fail due to Tax Rate is empty",
						true,
						reversingPassWhenNoTaxInLine.CanReverseTransaction);

					Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = true;
					line.AL_AT = tax.PK;
					invoice.AH_PostDate = dateWillFail;
					AssertNotErrorWithAnyTaxType("[Have Security]", line);
				}
			}

			void AssertNotIndia()
			{
				AssertNotEquals("PreCondition", "IN", Env.CurrentCompany.Country.Code);

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AT = TestObjectCreator.CreateTaxRate("TAX1", "", 10).PK;

				Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed = false;
				invoice.AH_PostDate = dateWillFail;
				AssertNotErrorWithAnyTaxType("[Country not India]", line);
			}

			void AssertNotErrorWithAnyTaxType(string comment, InvoicingLineBase line)
			{
				CombineAssertions($"{comment}Should not have error in any tax rate type", () =>
				{
					foreach (var taxType in line.TaxRate.Lookups.Types.GetAllCodes())
					{
						line.TaxRate.AT_Type = taxType;

						var reversingPass = ReversingFactory.NewReversing(line.InvoiceBase);
						AssertEquals(true, reversingPass.CanReverseTransaction);
					}
				});
			}
		}

		void securityOverrideProvider_OnRequestLoginCredentialsCall(object sender, EventArgs e)
		{
			WasRequestLoginCredentialsCalled = true;
		}

		bool WasRequestLoginCredentialsCalled;

		InvoicingBase SetupInvoiceWithPaidRelatedInvoices()
		{
			// Primary transactions
			InvoicingBase inv1_HasRelatedInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m);
			inv1_HasRelatedInvoice.FillWithValidTestData();
			InvoicingLineBase line1 = TestObjectCreator.CreateInvoiceLine(inv1_HasRelatedInvoice, inv1_HasRelatedInvoice.TransactionCurrency, inv1_HasRelatedInvoice.AH_ExchangeRate, 10m);
			line1.AL_AC = TestObjectCreator.CC1.PK;
			line1.AL_JH = TestObjectCreator.Job1.PK;
			line1.AL_GB = GlbBranch.CurrentBranch.PK;
			line1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.CreateJobCharge(line1, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			Factory.Save();

			// Related Transactions
			InvoicingBase inv1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m);
			inv1.FillWithValidTestData();
			InvoicingLineBase lineR1 = TestObjectCreator.CreateInvoiceLine(inv1, inv1.TransactionCurrency, inv1.AH_ExchangeRate, 10m);
			lineR1.AL_AC = TestObjectCreator.CC1.PK;
			lineR1.AL_JH = TestObjectCreator.Job1.PK;
			lineR1.AL_GB = GlbBranch.CurrentBranch.PK;
			lineR1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			lineR1.AL_OSExTaxAmount = 10M;
			inv1.AH_FullyPaidDate = ZDateTime.Today;
			inv1.AH_OutstandingAmount = 0M;
			TestObjectCreator.CreateJobCharge(lineR1, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			TransactionMatchLinkGroup matchLinkGroup = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink matchLink = matchLinkGroup.AddNew();
			matchLink.AP_AH = inv1.PK;
			matchLink.AP_Amount = inv1.AH_InvoiceAmount;
			matchLink = matchLinkGroup.AddNew();

			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_OSExTaxAmount = inv1.AH_OSExTaxAmount;
			payment.AH_OutstandingAmount = 0M;
			payment.AH_FullyPaidDate = ZDateTime.Today;

			matchLink.AP_AH = payment.PK;
			matchLink.AP_Amount = payment.AH_InvoiceAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinkGroup);
			Factory.Save();

			return inv1_HasRelatedInvoice;
		}

		#endregion

		ARInvoiceReversing ARInvoiceReversing
		{
			get { return (ARInvoiceReversing)Reversing; }
		}

		protected override Type GetTestingClassType()
		{
			return typeof(ARInvoiceReversing);
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = Factory.NewWithValidTestData<ARInvoiceForReversingTest>();
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = Factory.NewWithValidTestData<ARInvoiceForReversingTest>();
		}

		class ARInvoiceForReversingTest : ARInvoice, IPayablesAndReceivablesForTests
		{
			public ARInvoiceForReversingTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IReversingForTests Members

			public void SetIsClearedInCashbook(bool value)
			{
				AH_DateClearedInCashbook = value ? ZDateTime.Now : ZDateTime.Empty;
			}

			public void SetIsReversed(bool value)
			{
				AH_IsCancelled = value;
			}

			public void SetReverseTransactionToBeGenerated(IReversing reverseTransaction)
			{
				fReverseTransaction = (TransactionHeader)reverseTransaction;
			}

			#endregion

			#region ITransactionForTests Members

			public void SetFactory(BusinessObjectFactory factory)
			{
			}

			public ZDateTime FullyPaidDate
			{
				get
				{
					return AH_FullyPaidDate;
				}
				set
				{
					AH_FullyPaidDate = value;
				}
			}

			#endregion

			#region IPayablesAndReceivablesForTests Members

			public void SetIsMatched(bool value)
			{
				if (((IMatching)this).IsMatched != value)
				{
					if (value)
					{
						AH_LocalOutstandingAmount = AH_LocalExTaxAmount + AH_LocalTaxAmount + 1M;
					}
					else
					{
						AH_LocalOutstandingAmount = AH_LocalExTaxAmount + AH_LocalTaxAmount;
					}
				}
			}

			public void SetMatchLinksToBeGenerated(TransactionMatchLinkGroup matchLinksToSet)
			{
				fCurrentMatchGroup = matchLinksToSet;
			}

			#endregion
		}

		class TestSecurityOverrideProvider : SecurityOverrideProvider
		{
			protected override SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint)
			{
				if (OnRequestLoginCredentialsCall != null)
				{
					OnRequestLoginCredentialsCall(this, EventArgs.Empty);
				}
				return null;
			}

			public event EventHandler OnRequestLoginCredentialsCall;

			protected override SecurityCertificate RequestGrantedConfirmation(SecurityCheckpoint checkPoint)
			{
				throw new NotImplementedException();
			}
		}
	}
}
