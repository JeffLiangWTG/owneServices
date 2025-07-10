using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	public abstract class TransferTest : NonPersistentBusinessObjectTestCase, IReversingTest
	{
		protected Transfer TestTransfer;
		protected Transfer TestReversingTransfer;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Transfer.New(GetExpectedBusinessObjectType(), Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MakeActiveAndInActiveOrgsDebtorAndCreditor();
			TestTransfer = Transfer.New(GetExpectedBusinessObjectType(), Factory);
			CurrentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		}
		GlbCompany CurrentCompany;

		void MakeActiveAndInActiveOrgsDebtorAndCreditor()
		{
			TestObjectCreator.InActiveOrg.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.InActiveOrg.CompanyData.OB_IsDebtor = true;
			TestObjectCreator.ActiveOrg.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.ActiveOrg.CompanyData.OB_IsDebtor = true;
			Factory.Save();
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#region Test Business Object Overrides

		public void TestSetDefaultValues()
		{
			AssertZDateTimeDefaultBehaviour(TestTransfer, TestTransfer.AH_PostDateInfo);
			AssertZDateTimeDefaultBehaviour(TestTransfer, TestTransfer.AH_InvoiceDateInfo);
			Assert("should be read only by default", TestTransfer.AH_TransactionNumInfo.ReadOnly);
			Assert("from-before amount should always be read only", TestTransfer.AH_Calc_FromBeforeTransferInfo.ReadOnly);
			Assert("from-after amount should always be read only", TestTransfer.AH_Calc_FromAfterTransferInfo.ReadOnly);
			Assert("to-before amount should always be read only", TestTransfer.AH_Calc_ToBeforeTransferInfo.ReadOnly);
			Assert("to-after amount should always be read only", TestTransfer.AH_Calc_ToAfterTransferInfo.ReadOnly);
		}

		public void TestOnSaving()
		{
			TestTransfer.OnSaving();
			AssertEquals("TransferTo and TransferFrom should have the same transaction number", TestTransfer.TransferFrom.AH_TransactionNum, TestTransfer.TransferTo.AH_TransactionNum);
		}

		#endregion

		OrgHeader fFromAccount;
		protected OrgHeader FromAccount
		{
			get
			{
				if (fFromAccount == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);

					ZQuery fromAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(fromAccountFilter);

					query.AddSubQuery(subQuery, JoinCondition.And);
					query.AddToFilter(OrgHeaderSchema.PK, ToAccount.PK);

					fFromAccount = Factory.LoadTop1<OrgHeader>(query);
				}

				return fFromAccount;
			}
		}

		OrgHeader fToAccount;
		protected OrgHeader ToAccount
		{
			get
			{
				if (fToAccount == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);

					ZQuery toAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					toAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					toAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(toAccountFilter);

					query.AddSubQuery(subQuery, JoinCondition.And);

					fToAccount = Factory.LoadTop1<OrgHeader>(query);
				}
				return fToAccount;
			}
		}

		RefCurrency fForeignCurrency;
		protected RefCurrency ForeignCurrency
		{
			get
			{
				if (fForeignCurrency == null)
				{
					fForeignCurrency = Factory.New<RefCurrency>();
					fForeignCurrency.RX_Code = "FOR";
					fForeignCurrency.RX_SubUnitRatio = 100;
				}
				return fForeignCurrency;
			}
		}

		public void TestTransactionNumberGenerator()
		{
			TransactionNumberSequenceCustomisationCollection customisation = new TransactionNumberSequenceCustomisationCollection();
			TransactionNumberSequenceCustomisation element = customisation.AddNew();
			element.Order = 1;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode;
			element.Include = true;
			element = customisation.AddNew();
			element.Length = 8;
			element.Order = 2;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber;
			element.Include = true;
			element = customisation.AddNew();
			element.Order = 50;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode;
			element.Include = true;
			AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisation);

			Factory.Save();
			AssertEquals("Transaction number", "BNE00000001BRN", TestTransfer.TransferFrom.AH_TransactionNum);
			AssertEquals("Transaction number", "BNE00000001BRN", TestTransfer.TransferTo.AH_TransactionNum);
		}

		public void TestFromToAccountActiveValidtion()
		{
			using (new TestObjectCreator.MakeOrgTemporarilyInactive(TestObjectCreator.InActiveOrg))
			{
				ZGuid activeOrgPK = TestObjectCreator.ActiveOrg.PK;
				ZGuid inactiveOrgPK = TestObjectCreator.InActiveOrg.PK;

				AssertAccountActiveValidationWorks(TestTransfer.AH_FromAccountInfo, activeOrgPK, inactiveOrgPK);
				AssertAccountActiveValidationWorks(TestTransfer.AH_ToAccountInfo, activeOrgPK, inactiveOrgPK);
			}
		}

		void AssertAccountActiveValidationWorks(ZPropertyInfo fromOrToAccountInfo, ZGuid activeOrg, ZGuid inactiveOrg)
		{
			fromOrToAccountInfo.Value = activeOrg;
			AssertNoErrors(fromOrToAccountInfo);

			fromOrToAccountInfo.Value = inactiveOrg;
			AssertHasError(fromOrToAccountInfo, "This Account is inactive - it may not be used.");
		}

		public void TestDefaultAH_PostDateReadOnly()
		{
			bool postingAllowed = CheckPointForPostToPreviousOrFutureOpenPeriod.IsAllowed;

			try
			{
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				CheckPointForPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				TestTransfer = Transfer.New(GetExpectedBusinessObjectType(), Factory);
				Assert("AH_PostDate should not be readonly", !TestTransfer.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				TestTransfer = Transfer.New(GetExpectedBusinessObjectType(), Factory);
				Assert("AH_PostDate should be readonly", TestTransfer.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				CheckPointForPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				TestTransfer = Transfer.New(GetExpectedBusinessObjectType(), Factory);
				Assert("AH_PostDate should be readonly", TestTransfer.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				TestTransfer = Transfer.New(GetExpectedBusinessObjectType(), Factory);
				Assert("AH_PostDate should be readonly", TestTransfer.AH_PostDateInfo.ReadOnly);
			}
			finally
			{
				CheckPointForPostToPreviousOrFutureOpenPeriod.IsAllowed = postingAllowed;
			}
		}

		protected abstract Security.SecurityCheckpoint CheckPointForPostToPreviousOrFutureOpenPeriod { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestAmountsTogether()
		{
			bool originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			CurrentCompany.GC_IsReciprocal = false;
			Factory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			try
			{
				var transfer = Transfer.New(GetExpectedBusinessObjectType(), Factory);
				transfer.AH_OSTotal = 0;
				transfer.AH_ExchangeRateAmount = 1;

				AssertEquals(0m, transfer.AH_InvoiceAmount);
				AssertEquals(0m, transfer.AH_OSTotal);
				AssertEquals(1m, transfer.AH_ExchangeRateAmount);

				transfer.AH_OSTotal = 1.5;
				AssertEquals(1.5m, transfer.AH_InvoiceAmount);
				AssertEquals(1.5m, transfer.AH_OSTotal);
				AssertEquals(1m, transfer.AH_ExchangeRateAmount);

				transfer.AH_OSTotal = 2;
				AssertEquals(2m, transfer.AH_InvoiceAmount);
				AssertEquals(2m, transfer.AH_OSTotal);
				AssertEquals(1m, transfer.AH_ExchangeRateAmount);

				transfer.AH_InvoiceAmount = 4;
				AssertEquals(4m, transfer.AH_InvoiceAmount);
				AssertEquals(2m, transfer.AH_OSTotal);
				AssertEquals(0.5m, transfer.AH_ExchangeRateAmount);

				transfer.AH_ExchangeRateAmount = 3;
				transfer.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
				AssertEquals(0.67m, transfer.AH_InvoiceAmount);
				AssertEquals(2m, transfer.AH_OSTotal);
				AssertEquals(3m, transfer.AH_ExchangeRateAmount);

				transfer.AH_InvoiceAmount = 0;
				transfer.AH_ExchangeRateAmount = 0;
				transfer.AH_OSTotal = 0;

				transfer.AH_ExchangeRateAmount = 2;
				transfer.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
				transfer.AH_OSTotal = 10;
				AssertEquals(5m, transfer.AH_InvoiceAmount);
				AssertEquals(10m, transfer.AH_OSTotal);
				AssertEquals(2m, transfer.AH_ExchangeRateAmount);

				transfer.AH_OSTotal = 5;
				AssertEquals(2.5m, transfer.AH_InvoiceAmount);
				AssertEquals(5m, transfer.AH_OSTotal);
				AssertEquals(2m, transfer.AH_ExchangeRateAmount);
			}
			finally
			{
				CurrentCompany.GC_IsReciprocal = originalIsReciprocal;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		public void TestValidateAH_Desc()
		{
			TestTransfer.AH_Desc = "";
			TestTransfer.ValidateAH_Desc();
			Assert("description cannot be empty", TestTransfer.AH_DescInfo.HasErrors());
		}

		public void TestOSPartialPaymentAmount_ReadOnly()
		{
			Assert(((IMatching)TestTransfer).OSPartialPaymentAmount_ReadOnly);
		}

		#region AH_PostDate

		public void TestValidateAH_PostDate()
		{
			TestTransfer.AH_PostDate = ZDateTime.Empty;
			TestTransfer.ValidateAH_PostDate();
			Assert("Post date cannot be empty", TestTransfer.AH_PostDateInfo.HasErrors());
		}

		[TestDate(2006, 11, 15)]
		public void TestValidateAH_PostDate_Reversing()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZDateTime originalPostDate = ZDateTime.Today.AddDays(-5);
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			AccPeriodManagement accPeriod = periodCalculator.GetPeriodManagementFromDate(originalPostDate);
			if (accPeriod == null)
			{
				accPeriod = Factory.New<AccPeriodManagement>();
				accPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
				accPeriod.AM_Period = periodCalculator.GetPeriodFromDate(originalPostDate);
				accPeriod.AM_Year = (short)(accPeriod.AM_Period / 100);
				accPeriod.AM_StartDate = new ZDateTime(originalPostDate.Year, originalPostDate.Month, 1);
				accPeriod.AM_EndDate = accPeriod.AM_StartDate.AddMonths(1).AddDays(-1);
			}
			Factory.Save();

			TestTransfer.AH_PostDate = originalPostDate;

			ReversingFactory reversingFactory = new ReversingFactory();
			ReversingBase reversing = reversingFactory.NewReversing(TestTransfer);
			reversing.Reverse();

			Transfer reverseTransfer = reversing.ReverseTransaction as Transfer;
			reverseTransfer.ValidateAH_PostDate();
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseTransfer.AH_PostDateInfo.HasErrors());

			reverseTransfer.AH_PostDate = originalPostDate.AddDays(-1);
			AssertEquals("AH_PostDateInfo.HasErrors()", true, reverseTransfer.AH_PostDateInfo.HasErrors());
			string expectedError = "Reversing post date cannot be before the original post date of '" + TestTransfer.AH_PostDate.ToShortDateString() + "'.";
			AssertHasErrorContaining(reverseTransfer.AH_PostDateInfo, expectedError);

			reverseTransfer.AH_PostDate = originalPostDate;
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseTransfer.AH_PostDateInfo.HasErrors());

			reverseTransfer.AH_PostDate = originalPostDate.AddDays(1);
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseTransfer.AH_PostDateInfo.HasErrors());

			reverseTransfer.TransferTo.ReadOnly = true;
			reverseTransfer.TransferFrom.ReadOnly = true;
			Assert("TransferTo validation should be a TransactionReversalValidation", reverseTransfer.TransferTo.Validation is TransactionReversalValidation);
			Assert("TransferFrom validation should be a TransactionReversalValidation", reverseTransfer.TransferFrom.Validation is TransactionReversalValidation);

			reverseTransfer.AH_PostDate = originalPostDate.AddDays(6);
			AssertEquals("AH_PostDateInfo should not have any error because the context is not set", false, reverseTransfer.AH_PostDateInfo.HasErrors());
			reverseTransfer.AH_PostDate = originalPostDate.AddYears(2);
			AssertEquals("AH_PostDateInfo should not have any error because the context is not set", false, reverseTransfer.AH_PostDateInfo.HasErrors());

			Factory.SetContext(BusinessContext.ReverseDateForm);

			expectedError = "The post date cannot be in the future";
			reverseTransfer.AH_PostDate = originalPostDate.AddDays(6);
			AssertEquals("We should get an error because the context is set : " + expectedError, true, reverseTransfer.AH_PostDateInfo.GetErrors().Contains(expectedError));

			expectedError = "This date does not fall into a valid accounting period’s date range.\r\nPlease go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.";
			reverseTransfer.AH_PostDate = originalPostDate.AddYears(2);
			AssertEquals("We should get an error because the context is set : " + expectedError, true, reverseTransfer.AH_PostDateInfo.GetErrors().Contains(expectedError));
		}

		#endregion

		#region AH_InvoiceDate

		public void TestValidateAH_InvoiceDate()
		{
			TestTransfer.AH_InvoiceDate = ZDateTime.Empty;
			TestTransfer.ValidateAH_InvoiceDate();
			Assert("Invoice date cannot be empty", TestTransfer.AH_InvoiceDateInfo.HasErrors());
		}

		#endregion

		#region Exchange Rate Tests

		public virtual void TestDefaultExchangeRate()
		{
			AssertEquals("default should be local currency", TestTransfer.AH_ExchangeRateCurrencyCode, TestTransfer.AH_RX_NKTransactionCurrency);
			AssertEquals("should default to 1", TestTransfer.AH_ExchangeRateAmount, new ZDecimal(1));
		}

		public void TestValidateAH_ExchangeRateAmount()
		{
			TestTransfer.AH_ExchangeRateAmount = 0;
			TestTransfer.ValidateAH_ExchangeRateAmount();
			Assert("ExchangeRateAmount cannot be empty", TestTransfer.AH_ExchangeRateAmountInfo.HasErrors());
		}

		public void TestValidateAH_ExchangeRateCurrency()
		{
			TestTransfer.AH_ExchangeRateCurrencyCode = ZString.Empty;
			TestTransfer.ValidateAH_ExchangeRateCurrency();
			Assert("ExchangeRateCurrency cannot be empty", TestTransfer.AH_ExchangeRateCurrencyCodeInfo.HasErrors());
		}

		#endregion

		#region TestAH_OSTotal

		public void TestAH_OSTotal()
		{
			TestTransfer.AH_OSTotal = 0;
			Assert("OS Total cannot equal 0", TestTransfer.AH_OSTotalInfo.HasErrors());
		}

		#endregion

		#region TestAH_InvoiceAmount

		/*		public void TestAH_InvoiceAmount()
				{
					TestTransfer.AH_OSTotal = 100;
					TestTransfer.ExchangeRate.Rate = 20;
					AssertEquals("Invoice amount should be 5", TestTransfer.AH_InvoiceAmount, new ZDecimal(5));

					TestTransfer.AH_OSTotal = -100;
					TestTransfer.ExchangeRate.Rate = 20;
					AssertEquals("Invoice amount should be -5", TestTransfer.AH_InvoiceAmount, new ZDecimal(-5));

					TestTransfer.AH_OSTotal = -120;
					TestTransfer.ExchangeRate.Rate = 20;
					AssertEquals("Invoice amount should be -6", TestTransfer.AH_InvoiceAmount, new ZDecimal(-6));

					TestTransfer.ExchangeRate.Rate = -1;
					Assert("Rate cannot be negative", TestTransfer.AH_ExchangeRateAmountInfo.HasErrors());

					TestTransfer.ExchangeRate.Rate = 0;
					Assert("Rate cannot equal 0", TestTransfer.AH_ExchangeRateAmountInfo.HasErrors());
				}*/

		#endregion

		#region TestValidateAH_FromAccount

		public virtual void TestValidateAH_FromAccount()
		{
			TestTransfer.AH_FromAccount = ZGuid.Empty;
			TestTransfer.ValidateAH_FromAccount();
			Assert("FromAccount cannot be empty", TestTransfer.AH_FromAccountInfo.HasErrors());
			TestTransfer.AH_FromAccount = (Factory.LoadTop1(typeof(OrgHeader), new ZQuery()) as OrgHeader).PK;
			TestTransfer.AH_ToAccount = TestTransfer.AH_FromAccount;
			Assert("From account cannot equal to account", TestTransfer.AH_ToAccountInfo.HasErrors());
		}

		#endregion

		#region TestValidateAH_ToAccount

		public virtual void TestValidateAH_ToAccount()
		{
			TestTransfer.AH_ToAccount = ZGuid.Empty;
			TestTransfer.ValidateAH_ToAccount();
			Assert("ToAccount cannot be empty", TestTransfer.AH_ToAccountInfo.HasErrors());
			TestTransfer.AH_FromAccount = (Factory.LoadTop1(typeof(OrgHeader), new ZQuery()) as OrgHeader).PK;
			TestTransfer.AH_ToAccount = TestTransfer.AH_FromAccount;
			Assert("From account cannot equal to account", TestTransfer.AH_ToAccountInfo.HasErrors());
		}

		#endregion

		#region TestAH_Calc_FromDueDate

		public void TestAH_Calc_FromDueDate()
		{
			TestTransfer.AH_Calc_FromDueDate = Env.Time.CurrentLocalDate;
			AssertEquals("TransferFrom should have it's due date set", TestTransfer.TransferFrom.AH_DueDate, (ZDateTime)Env.Time.CurrentLocalDate);
			TestTransfer.AH_Calc_FromDueDate = ZDateTime.Empty;
			TestTransfer.ValidateAH_Calc_FromDueDate();
			AssertHasError(TestTransfer.AH_Calc_FromDueDateInfo, "Please enter a value.");
		}

		#endregion

		#region TestAH_CalcToDueDate

		public void TestAH_CalcToDueDate()
		{
			TestTransfer.AH_Calc_ToDueDate = Env.Time.CurrentLocalDate;
			Assert("TransferTo should have its due date set", TestTransfer.TransferTo.AH_DueDate == (ZDateTime)Env.Time.CurrentLocalDate);
			TestTransfer.AH_Calc_ToDueDate = ZDateTime.Empty;
			TestTransfer.ValidateAH_Calc_ToDueDate();
			AssertHasError(TestTransfer.AH_Calc_ToDueDateInfo, "Please enter a value.");
		}

		#endregion

		#region TestAH_ExchangeRateCurrency

		public void TestAH_ExchangeRateCurrency()
		{
			TestTransfer.ExchangeRate.Currency = "USD";
			TestTransfer.ExchangeRate.Rate = 0.65m;
			AssertNoErrors("Should be no error on currency field", TestTransfer.AH_ExchangeRateCurrencyCodeInfo);
			AssertNoErrors("Should be no error on exrate field", TestTransfer.AH_ExchangeRateAmountInfo);

			TestTransfer.ExchangeRate.Currency = "XXX";
			AssertHasErrors("Should be error on currency field", TestTransfer.AH_ExchangeRateCurrencyCodeInfo);
			AssertNoErrors("Should be no error on exrate field", TestTransfer.AH_ExchangeRateAmountInfo);

			TestTransfer.ExchangeRate.Currency = "USD";
			TestTransfer.ExchangeRate.Rate = 0m;
			AssertNoErrors("Should be no error on currency field", TestTransfer.AH_ExchangeRateCurrencyCodeInfo);
			AssertHasErrors("Should be error on exrate field", TestTransfer.AH_ExchangeRateAmountInfo);

			TestTransfer.ExchangeRate.Rate = 0.78m;
			AssertNoErrors("Should be no error on currency field", TestTransfer.AH_ExchangeRateCurrencyCodeInfo);
			AssertNoErrors("Should be no error on exrate field", TestTransfer.AH_ExchangeRateAmountInfo);
		}

		#endregion

		#region TestInvoiceBatchNumber

		public void TestInvoiceBatchNumber()
		{
			Assert("InvoiceBatch Number should be empty", ((IMatching)TestTransfer).InvoiceBatchNumber.IsEmpty);
		}

		#endregion

		public void TestInvoiceTransactionReference()
		{
			Assert("Invoice Transaction Reference should be empty", ((IMatching)TestTransfer).InvoiceTransactionReference.IsEmpty);
		}

		public void TestFactoryInstantiationOnNew()
		{
			Transfer createdTransfer = Transfer.New(TestTransfer.GetType(), Factory);
			AssertEquals("Type should be AP Transfer Type", TestTransfer.GetType(), createdTransfer.GetType());
		}

		public void TestFactoryInstantiationOnLoad()
		{
			Transfer loadedTransfer = Transfer.Load(TestTransfer.GetType(), Factory, TestTransfer.TransferFrom, TestTransfer.TransferTo, Transfer.TransferDirectionTypes.TransferFrom);
			AssertEquals("Type should be AP Transfer Type", TestTransfer.GetType(), loadedTransfer.GetType());
		}

		#region TransferDirectionTypes

		public void TestLoadCorrectTransferDirectionTypes()
		{
			var loadedTransferFrom = Transfer.Load(TestTransfer.GetType(), Factory, TestTransfer.TransferFrom, TestTransfer.TransferTo, Transfer.TransferDirectionTypes.TransferFrom);
			CombineAssertions("Transfer From Part", () =>
			{
				AssertEquals("Should load Transfer From Row", Transfer.TransferDirectionTypes.TransferFrom, loadedTransferFrom.TransferLedger);
				AssertEquals("LogsAndNotesTarget should be Transfer From Row", loadedTransferFrom.TransferFrom, loadedTransferFrom.LogsAndNotesTarget_ForTestOnly);
				AssertEquals("PK should be Transfer From Row", loadedTransferFrom.PK, loadedTransferFrom.GetPK_ForTestOnly);
				AssertEquals("Identifier should be Transfer From Row", ((IIdentified)loadedTransferFrom).Identifier, loadedTransferFrom.Identifier_ForTestOnly);
			});

			var loadedTransferTo = Transfer.Load(TestTransfer.GetType(), Factory, TestTransfer.TransferFrom, TestTransfer.TransferTo, Transfer.TransferDirectionTypes.TransferTo);
			CombineAssertions("Transfer To Part", () =>
			{
				AssertEquals("Should load Transfer To Row", Transfer.TransferDirectionTypes.TransferTo, loadedTransferTo.TransferLedger);
				AssertEquals("LogsAndNotesTarget Should be Transfer To Row", loadedTransferTo.TransferTo, loadedTransferTo.LogsAndNotesTarget_ForTestOnly);
				AssertEquals("PK should be Transfer To Row", loadedTransferTo.PK, loadedTransferTo.GetPK_ForTestOnly);
				AssertEquals("Identifier should be Transfer To Row", ((IIdentified)loadedTransferTo).Identifier, loadedTransferTo.Identifier_ForTestOnly);
			});
		}

		#endregion

		#region IReversingTest Members

		public void TestIsReversedImplementation()
		{
			Assert("Transfer should not be reversed", !((IReversing)TestTransfer).IsReversed);

			TestTransfer.TransferFrom.AH_IsCancelled = ZBool.True;
			Assert("Transfer should be reversed", ((IReversing)TestTransfer).IsReversed);

			TestTransfer.TransferFrom.AH_IsCancelled = ZBool.False;
			Assert("Transfer should not be reversed", !((IReversing)TestTransfer).IsReversed);

			TestTransfer.TransferTo.AH_IsCancelled = ZBool.True;
			Assert("Transfer should be reversed", ((IReversing)TestTransfer).IsReversed);

			TestTransfer.TransferFrom.AH_IsCancelled = ZBool.True;
			Assert("Transfer should be reversed", ((IReversing)TestTransfer).IsReversed);
		}

		public void TestIsMatchedImplementation()
		{
			Assert("Transfer should not be matched", !((IMatching)TestTransfer).IsMatched);

			TestTransfer.TransferFrom.AH_OSExTaxAmount = 200.00m;
			TestTransfer.TransferFrom.AH_LocalOutstandingAmount = 200.00m;
			Assert("Transfer should still not be matched", !((IMatching)TestTransfer).IsMatched);

			TestTransfer.TransferFrom.AH_LocalOutstandingAmount = 150.00m;
			Assert("Transfer should be matched", ((IMatching)TestTransfer).IsMatched);

			TestTransfer.TransferFrom.AH_LocalOutstandingAmount = 200.00m;
			Assert("Transfer should not be matched", !((IMatching)TestTransfer).IsMatched);

			TestTransfer.TransferTo.AH_LocalOutstandingAmount = 15.00m;
			Assert("Transfer should be matched", ((IMatching)TestTransfer).IsMatched);

			TestTransfer.TransferFrom.AH_LocalOutstandingAmount = 30.00m;
			Assert("Transfer should be matched", ((IMatching)TestTransfer).IsMatched);
		}

		public void TestSetCancellationFlag()
		{
			((IReversing)TestTransfer).SetCancellationFlag(true);
			AssertEquals("Cancelled flag on from row", ZBool.True, TestTransfer.TransferFrom.AH_IsCancelled);
			AssertEquals("Cancelled flag on to row", ZBool.True, TestTransfer.TransferTo.AH_IsCancelled);

			((IReversing)TestTransfer).SetCancellationFlag(false);
			AssertEquals("Cancelled flag on from row", ZBool.False, TestTransfer.TransferFrom.AH_IsCancelled);
			AssertEquals("Cancelled flag on to row", ZBool.False, TestTransfer.TransferTo.AH_IsCancelled);
		}

		public virtual void TestReverseTransaction()
		{
			TestTransfer.AH_FromAccount = FromAccount.PK;
			TestTransfer.AH_ToAccount = ToAccount.PK;
			TestTransfer.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			TestTransfer.AH_ExchangeRateAmount = 0.95m;
			TestTransfer.AH_PostDate = ZDateTime.Now.AddDays(-2);
			TestTransfer.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
			TestTransfer.AH_Desc = "DESCRIPTION";
			TestTransfer.AH_OSTotal = 500.00m;

			((IReversing)TestTransfer).GenerateReverseTransaction(true);
			TestReversingTransfer = (Transfer)((IReversing)TestTransfer).ReverseTransaction;

			AssertEquals("AP Account on reversing Transfer", FromAccount.PK, TestReversingTransfer.AH_FromAccount);
			AssertEquals("AR Account on reversing Transfer", ToAccount.PK, TestReversingTransfer.AH_ToAccount);
			AssertEquals("Currency on reversing Transfer", ForeignCurrency.RX_Code, TestReversingTransfer.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate on reversing Transfer", 0.95m, TestReversingTransfer.AH_ExchangeRateAmount);
			AssertEquals("Post date should be today", ZDateTime.Now.Date, TestReversingTransfer.AH_PostDate.Date);
			AssertEquals("Invoice date should be today", ZDateTime.Now.Date, TestReversingTransfer.AH_InvoiceDate.Date);

			AssertEquals("OS Total should be negative of original Transfer", -500.00m, TestReversingTransfer.AH_OSTotal);
			AssertEquals("Invoice Amount should be negative of original Transfer", -526.32m, TestReversingTransfer.AH_InvoiceAmount);

			AssertEquals("Original transaction number should be set", TestTransfer.AH_TransactionNum, TestReversingTransfer.OriginalTransactionNumber);
			AssertEquals("Original transaction type should be set", TransactionTypes.Transfer, TestReversingTransfer.OriginalTransactionType);
		}

		public void TestReverseAndSaveDoesntResetOriginalTransactionNumber()
		{
			TestTransfer.AH_OSTotal = 500.00m;
			TestTransfer.AH_FromAccount = FromAccount.PK;
			TestTransfer.AH_ToAccount = ToAccount.PK;
			TestTransfer.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			TestTransfer.AH_ExchangeRateAmount = 0.95m;
			TestTransfer.AH_PostDate = ZDateTime.Now.AddDays(-2);
			TestTransfer.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
			TestTransfer.AH_Desc = "DESCRIPTION";
			TestTransfer.AH_OSTotal = 500.00m;

			ZString expectedNumber = TestTransfer.TransferNumberFountain.PeekPreliminary(Factory);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			TransferRow transferFromRow = (TransferRow)newFactory.Load(typeof(TransactionHeader), TestTransfer.TransferFrom.PK);
			TransferRow transferToRow = (TransferRow)newFactory.Load(typeof(TransactionHeader), TestTransfer.TransferTo.PK);

			Transfer loadedTransfer = Transfer.Load(TestTransfer.GetType(), newFactory, transferFromRow, transferToRow, Transfer.TransferDirectionTypes.TransferFrom);

			AssertEquals("Should have set transaction number on AP row to next", expectedNumber, loadedTransfer.TransferTo.AH_TransactionNum);
			AssertEquals("Should have set transaction number on AR row to next", expectedNumber, loadedTransfer.TransferFrom.AH_TransactionNum);

			((IReversing)loadedTransfer).GenerateReverseTransaction(true);
			Transfer fReverseTransaction = (Transfer)((IReversing)loadedTransfer).ReverseTransaction;

			((IReversing)fReverseTransaction).SetCancellationFlag(true);
			((IMatching)fReverseTransaction.TransferFrom).CurrentMatchGroup.AddNew().AP_AH = fReverseTransaction.TransferFrom.PK;
			((IMatching)fReverseTransaction.TransferTo).CurrentMatchGroup.AddNew().AP_AH = fReverseTransaction.TransferTo.PK;

			TestObjectCreator.SetupMatchLinkMatchDate(fReverseTransaction.TransferFrom);
			TestObjectCreator.SetupMatchLinkMatchDate(fReverseTransaction.TransferTo);
			((IReversing)loadedTransfer).SetCancellationFlag(true);
			((IMatching)loadedTransfer.TransferFrom).CurrentMatchGroup.AddNew().AP_AH = loadedTransfer.TransferFrom.PK;
			((IMatching)loadedTransfer.TransferTo).CurrentMatchGroup.AddNew().AP_AH = loadedTransfer.TransferTo.PK;

			TestObjectCreator.SetupMatchLinkMatchDate(loadedTransfer.TransferFrom);
			TestObjectCreator.SetupMatchLinkMatchDate(loadedTransfer.TransferTo);

			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(newFactory);
			TransactionMatchLink matchLink = newFactory.NewWithValidTestData<TransactionMatchLink>();
			matchLink.AP_AH = transferToRow.PK;
			group.Add(matchLink);
			matchLink = newFactory.NewWithValidTestData<TransactionMatchLink>();
			matchLink.AP_AH = transferFromRow.PK;
			group.Add(matchLink);
			matchLink = newFactory.NewWithValidTestData<TransactionMatchLink>();
			matchLink.AP_AH = fReverseTransaction.TransferTo.PK;
			group.Add(matchLink);
			matchLink = newFactory.NewWithValidTestData<TransactionMatchLink>();
			matchLink.AP_AH = fReverseTransaction.TransferFrom.PK;
			group.Add(matchLink);
			TestObjectCreator.SetupMatchLinkMatchDate(group);

			ZString reverseTransactionNumber = TestTransfer.TransferNumberFountain.PeekPreliminary(Factory);

			newFactory.Save();

			newFactory = new BusinessObjectFactory();

			TransferRow originalTransactionFrom = (TransferRow)newFactory.Load(typeof(TransactionHeader), loadedTransfer.TransferFrom.PK);
			TransferRow originalTransactionTo = (TransferRow)newFactory.Load(typeof(TransactionHeader), loadedTransfer.TransferTo.PK);

			TransferRow reversingTransferFrom = (TransferRow)newFactory.Load(typeof(TransactionHeader), fReverseTransaction.TransferFrom.PK);
			TransferRow reversingTransferTo = (TransferRow)newFactory.Load(typeof(TransactionHeader), fReverseTransaction.TransferTo.PK);

			loadedTransfer = Transfer.Load(TestTransfer.GetType(), Factory, originalTransactionFrom, originalTransactionTo, Transfer.TransferDirectionTypes.TransferFrom);
			fReverseTransaction = Transfer.Load(TestTransfer.GetType(), Factory, reversingTransferFrom, reversingTransferTo, Transfer.TransferDirectionTypes.TransferFrom);

			AssertEquals("Cancellation Flag on Loaded Transfer From", ZBool.True, loadedTransfer.TransferFrom.AH_IsCancelled);
			AssertEquals("Cancellation Flag on Loaded Transfer To", ZBool.True, loadedTransfer.TransferTo.AH_IsCancelled);

			AssertEquals("Should still have same transaction number on original AP Row", expectedNumber, loadedTransfer.TransferTo.AH_TransactionNum);
			AssertEquals("Should still have same transaction number on original AR Row", expectedNumber, loadedTransfer.TransferFrom.AH_TransactionNum);

			AssertEquals("Reversing Transfer To should have next number", reverseTransactionNumber, fReverseTransaction.TransferTo.AH_TransactionNum);
			AssertEquals("Reversing Transfer From should have next number", reverseTransactionNumber, fReverseTransaction.TransferFrom.AH_TransactionNum);
		}

		public void TestAmountRestorationOnLoad()
		{
			ZDateTime postDate = ZDateTime.Now.AddDays(-2);
			ZDateTime invoiceDate = ZDateTime.Now.AddDays(-10);
			TestTransfer.AH_FromAccount = FromAccount.PK;
			TestTransfer.AH_ToAccount = ToAccount.PK;
			TestTransfer.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			TestTransfer.AH_ExchangeRateAmount = 0.95m;
			TestTransfer.AH_PostDate = postDate;
			TestTransfer.AH_InvoiceDate = invoiceDate;
			TestTransfer.AH_Desc = "DESCRIPTION";
			TestTransfer.AH_OSTotal = 500.00m;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			TransferRow transferFromRow = (TransferRow)newFactory.Load(typeof(TransactionHeader), TestTransfer.TransferFrom.PK);
			TransferRow transferToRow = (TransferRow)newFactory.Load(typeof(TransactionHeader), TestTransfer.TransferTo.PK);

			Transfer loadedTransfer = Transfer.Load(TestTransfer.GetType(), newFactory, transferFromRow, transferToRow, Transfer.TransferDirectionTypes.TransferFrom);

			AssertEquals("OS Amount on loaded transfer shoould be same as when saved", 500.00m, loadedTransfer.AH_OSTotal);
			AssertEquals("Local Amount on loaded transfer shoould be same as when saved", 526.32m, loadedTransfer.AH_InvoiceAmount);

			AssertEquals("From account should be same as when saved", FromAccount.PK, loadedTransfer.AH_FromAccount);
			AssertEquals("To account should be same as when saved", ToAccount.PK, loadedTransfer.AH_ToAccount);

			AssertEquals("Post Date should be same as when saved", postDate.Date, loadedTransfer.AH_PostDate.Date);
			AssertEquals("Invoice Date should be same as when saved", invoiceDate.Date, loadedTransfer.AH_InvoiceDate.Date);

			AssertEquals("Exchange Rate currency should be same as when saved", ForeignCurrency.RX_Code, loadedTransfer.AH_ExchangeRateCurrencyCode);
			AssertEquals("Exchange Rate amount should be same as when saved", 0.95m, loadedTransfer.AH_ExchangeRateAmount);
		}

		public void TestSetTransactionBelongsToGroupField()
		{
			((IReversing)TestTransfer).GenerateReverseTransaction(true);
			Transfer fReverseTransaction = (Transfer)((IReversing)TestTransfer).ReverseTransaction;

			ZGuid groupingGuid = TestTransfer.TransferFrom.AH_TransactionBelongsToGroup; // This is set initially and set to both Transfer row objects

			((IReversing)TestTransfer).SetTransactionBelongsToGroupField(groupingGuid);

			AssertEquals("Both rows should have the transaction belongs to group field set to the grouping guid passed",
				groupingGuid, TestTransfer.TransferFrom.AH_TransactionBelongsToGroup);
			AssertEquals("Both rows should have the transaction belongs to group field set to the grouping guid passed",
				groupingGuid, TestTransfer.TransferTo.AH_TransactionBelongsToGroup);

			AssertEquals("Both rows in reversing Transfer should have the transaction belongs to group field set to the grouping guid passed",
				groupingGuid, fReverseTransaction.TransferFrom.AH_TransactionBelongsToGroup);
			AssertEquals("Both rows in reversing Transfer should have the transaction belongs to group field set to the grouping guid passed",
				groupingGuid, fReverseTransaction.TransferTo.AH_TransactionBelongsToGroup);
		}

		public void TestSetDescription()
		{
			TestTransfer.AH_OSTotal = 500.00m;
			TestTransfer.AH_FromAccount = FromAccount.PK;
			TestTransfer.AH_ToAccount = ToAccount.PK;
			TestTransfer.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			TestTransfer.AH_ExchangeRateAmount = 0.95m;
			TestTransfer.AH_PostDate = ZDateTime.Now.AddDays(-2);
			TestTransfer.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
			TestTransfer.AH_Desc = "DESCRIPTION";
			TestTransfer.AH_OSTotal = 500.00m;

			Factory.Save();

			ZString transactionNumber = TestTransfer.AH_TransactionNum;

			((IReversing)TestTransfer).GenerateReverseTransaction(true);
			IReversing fReverseTransaction = ((IReversing)TestTransfer).ReverseTransaction;

			ZString descriptionToSet = string.Format("Reversal Related to {0}", transactionNumber);
			fReverseTransaction.SetDescription(descriptionToSet);
			AssertEquals("Description for reversing Transaction before Reversing Reason is known", descriptionToSet.ToUpper(), ((Transfer)fReverseTransaction).AH_Desc.ToUpper());
		}

		public void TestReversingReason()
		{
			TestTransfer.AH_OSTotal = 500.00m;
			TestTransfer.AH_FromAccount = FromAccount.PK;
			TestTransfer.AH_ToAccount = ToAccount.PK;
			TestTransfer.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			TestTransfer.AH_ExchangeRateAmount = 0.95m;
			TestTransfer.AH_PostDate = ZDateTime.Now.AddDays(-2);
			TestTransfer.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
			TestTransfer.AH_Desc = "DESCRIPTION";
			TestTransfer.AH_OSTotal = 500.00m;

			((IReversing)TestTransfer).GenerateReverseTransaction(true);
			IReversing fReverseTransaction = ((IReversing)TestTransfer).ReverseTransaction;

			ZString reversingReason = "Reversing Reason";

			((Transfer)fReverseTransaction).AH_Desc = "Description";

			fReverseTransaction.ReversingReason = reversingReason;

			AssertEquals("Description on reversing transfer", "Description Reversing Reason", ((Transfer)fReverseTransaction).AH_Desc);

			AssertEquals("Reversing Reason should return same as value set", reversingReason, fReverseTransaction.ReversingReason);
		}

		[ExpectNoExceptions]
		public void TestReversingReasonExceedsMaxLength()
		{
			string reverseReason = new string('d', AccTransactionHeaderSchema.AH_Desc.MaxLength + 1);
			TestTransfer.ReversingReason = reverseReason;
		}

		#endregion

		#region ITransaction Implementation Testing

		public void TestCurrencyImplementation()
		{
			TestTransfer.ExchangeRate.Currency = ForeignCurrency.RX_Code;
			AssertEquals("should return foreign", ForeignCurrency.RX_Code, ((ITransaction)TestTransfer).CurrencyCode);

			TestTransfer.ExchangeRate.Currency = ZString.Empty;
			AssertEquals("Should return empty code", ZString.Empty, ((ITransaction)TestTransfer).CurrencyCode);

			TestTransfer.ExchangeRate.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("should return local currency code", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ((ITransaction)TestTransfer).CurrencyCode);
		}

		public void TestLedgerImplementation()
		{
			AssertEquals("Should be same ledger as from row", TestTransfer.TransferFrom.AH_Ledger, ((ITransaction)TestTransfer).Ledger);
		}

		public void TestOverseasTotalAmountImplementation()
		{
			TestTransfer.AH_OSTotal = 350.00m;
			AssertEquals("Overseas total", 350.00m, ((ITransaction)TestTransfer).OverseasTotalAmount);
		}

		public void TestPostDateImplementation()
		{
			ZDateTime postDate = ZDateTime.Now.AddDays(-2);
			TestTransfer.AH_PostDate = postDate;
			AssertEquals("Post Date", postDate, ((ITransaction)TestTransfer).PostDate);
		}

		public void TestTransactionDateImplementation()
		{
			ZDateTime transactionDate = ZDateTime.Now.AddDays(20);
			TestTransfer.AH_InvoiceDate = transactionDate;
			AssertEquals("Transaction Date", transactionDate, ((ITransaction)TestTransfer).TransactionDate);

			transactionDate = ZDateTime.Now.AddDays(10);
			((ITransaction)TestTransfer).TransactionDate = transactionDate;
			AssertEquals("Transaction Date", transactionDate, ((ITransaction)TestTransfer).TransactionDate);
		}

		public void TestTransactionTypeImplementation()
		{
			AssertEquals("Transaction Type", ZArchitecture.Core.TransactionTypes.Transfer, ((ITransaction)TestTransfer).TransactionType);
		}

		public void TestTransactionCategoryImplementation()
		{
			AssertEquals("TransactionCategory", ZString.Empty, ((IMatching)TestTransfer).TransactionCategory);
		}

		#region ReversalStatusCode

		public void TestReversalStatusCode_ShouldBeEmpty()
		{
			AssertEquals(nameof(TestTransfer.ReversalStatusCode), ZString.Empty, TestTransfer.ReversalStatusCode);
		}

		public void TestReversalStatusCode_ReadOnly_ShouldBeTrue()
		{
			AssertEquals(nameof(ITransaction.ReversalStatusCode_ReadOnly), true, (TestTransfer as ITransaction).ReversalStatusCode_ReadOnly);
		}

		public void TestReversalStatusCodeList_ShouldBeNull()
		{
			AssertNull(nameof(ITransaction.ReversalStatusCodeList), (TestTransfer as ITransaction).ReversalStatusCodeList);
		}

		#endregion ReversalStatusCode

		#endregion

		#region IDataExportBatchSource Members

		public void TestIsDataExportBatchSupported()
		{
			AssertEquals("IsDataExportBatchSupported", ((IDataExportBatchSource)TestTransfer.TransferFrom).IsDataExportBatchSupported, ((IDataExportBatchSource)TestTransfer).IsDataExportBatchSupported);
		}

		public void TestRelatedBatchCollection()
		{
			var batch = TestObjectCreator.CreateDataExportBatchForHeader(TestTransfer.TransferFrom);
			Factory.Save();
			AssertCollectionContains("Public collection contains batch", batch, TestTransfer.DataExportBatchCollection);
		}

		#endregion

		#region Decimal Places

		public void TestZDecimalsHaveCorrectDecimalPlaces()
		{
			var localList = new List<string>
				{
					nameof(TestTransfer.AH_Calc_FromBeforeTransfer),
					nameof(TestTransfer.AH_Calc_FromAfterTransfer),
					nameof(TestTransfer.AH_Calc_ToBeforeTransfer),
					nameof(TestTransfer.AH_Calc_ToAfterTransfer),
				};

			var tester = new DecimalPlacesAttributeTester(TestTransfer);
			tester.CheckLocalCurrency(localList, nameof(TestTransfer.LocalDecimals));
		}

		public void TestDecimalPlacesAttributeApplyToAllZDecimalProperties()
		{
			var properties = typeof(Transfer).GetProperties().Where(x => x.PropertyType == typeof(ZDecimal)).ToList().SkipWhile(x => x.Name == "AH_ExchangeRateAmount" || x.Name == "AH_OSTotal" || x.Name == "AH_InvoiceAmount");
			Assert(properties.All(x => Attribute.IsDefined(x, typeof(DecimalPlacesAttribute))));
		}

		#endregion

		public void TestGeneratePaymentApprovalItems()
		{
			// This will only be called for manually created contras.
			PaymentApprovalBase newPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			IMatching transferAsIMatching = TestTransfer;
			transferAsIMatching.GeneratePaymentApprovalItems(newPaymentApproval);

			AssertEquals("Payment Approval Items", 2, transferAsIMatching.PaymentApprovalItems.Count);
		}

		public void TestDoNotValidateBranchDepartmentCombinationWhenReversing()
		{
			var transfer = TestTransfer;
			transfer.AH_FromAccount = FromAccount.PK;
			transfer.AH_ToAccount = ToAccount.PK;
			transfer.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			transfer.AH_ExchangeRateAmount = 0.95m;
			transfer.AH_PostDate = ZDateTime.Now.AddDays(-2);
			transfer.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
			transfer.AH_Desc = "DESCRIPTION";
			transfer.AH_OSTotal = 500.00m;

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { department });

			transfer.GenerateReverseTransaction(true);
			var reversingtransfer = (Transfer)((IReversing)transfer).ReverseTransaction;

			AssertEquals(transfer.TransferTo.AH_GB, Env.CurrentBranch.PK);
			AssertEquals(transfer.TransferTo.AH_GE, Env.CurrentDepartment.PK);

			AssertEquals(transfer.TransferFrom.AH_GB, Env.CurrentBranch.PK);
			AssertEquals(transfer.TransferFrom.AH_GE, Env.CurrentDepartment.PK);

			reversingtransfer.RunPreSaveValidation();
			reversingtransfer.TransferFrom.RunPreSaveValidation();
			reversingtransfer.TransferTo.RunPreSaveValidation();

			AssertNoErrors(reversingtransfer.TransferFrom.AH_GEInfo);
			AssertNoErrors(reversingtransfer.TransferTo.AH_GEInfo);
		}

		public void TestARAPTranferReversingValidatedForConcurrentReversal()
		{
			var handleError = TestTransfer as IHandleDeleteError;

			AssertNotNull("Implements IHandleError", handleError);
			Assert("RollbackAfterDeleteError false on new ARAPTransfer", !handleError.RollbackAfterDeleteError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);

			Factory.Save();

			Assert("RollbackAfterDeleteError is true by default", handleError.RollbackAfterDeleteError);
			Assert("DisableFormOnDeleteConcurrencyError is false by default", !handleError.DisableFormOnDeleteConcurrencyError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);

			TestTransfer.TransferFrom.IsCancelled = true;

			Assert("RollbackAfterDeleteError on saved and cancelled ARAPTransfer is false to prevent rollback to non-cancelled state", !handleError.RollbackAfterDeleteError);
			Assert("DisableFormOnDeleteConcurrencyError on saved and cancelled ARAPTransfer is true", handleError.DisableFormOnDeleteConcurrencyError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);
		}

		public void TestTablePrefix()
		{
			AssertEquals("Table Prefix should be AH.", "AH", TestTransfer.TablePrefix);
		}

		public void TestTableName()
		{
			AssertEquals("Table Name should be AccTransactionHeader.", "AccTransactionHeader", TestTransfer.TableName);
		}

		public void TestPKSchemaColumn()
		{
			AssertEquals("PKSchemaColumn should be AccTransactionHeaderSchema.PK.", AccTransactionHeaderSchema.PK, TestTransfer.PKSchemaColumn);
		}

		public abstract class TransferMatchingTest : Base.Transaction.Testing.IMatchingTestCase
		{
			protected override bool IsShownOnMatchingForm
			{
				get { return false; }
			}

			protected override IMatching GetNewIMatching(ZGuid branchPK, ZGuid organisationPK, ZString currencyCode)
			{
				Transfer bizObj = GetNewTransfer();
				bizObj.InitialiseNew_ForTestOnly();
				bizObj.TransferFrom.AH_GB = branchPK;
				bizObj.TransferFrom.AH_OH = organisationPK;
				bizObj.TransferTo.AH_RX_NKTransactionCurrency = currencyCode;
				return bizObj;
			}

			protected abstract Transfer GetNewTransfer();
		}
	}
}
