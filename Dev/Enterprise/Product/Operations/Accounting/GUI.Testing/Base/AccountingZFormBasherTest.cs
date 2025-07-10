using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.ZForm;

namespace Enterprise.Accounting.GUI.Testing
{
	[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
	public abstract class AccountingZFormBasherTest : ZFormBasherTest
	{
		public virtual void TestApplyPostButtonTextForApprovalRequestInvoices()
		{
			using (var form = (AccountingZForm)GetFormToBashCore())
			{
				var transaction = form.BusinessEntity as InvoicingBase;
				if (transaction != null && (transaction is APInvoice || transaction is APCreditNote))
				{
					transaction.SetContext(APInvoiceChargesApprovalRequest.Context.Posting);
					AssertEquals("ApplyButtonText", "&Post", ZFormPostingButtonsStrategy.ApplyButtonText(form).Text);
					AssertEquals("ApplyButtonText", "P&ost && Close", ZFormPostingButtonsStrategy.PostButtonText(form).Text);
					transaction.RemoveContext(APInvoiceChargesApprovalRequest.Context.Posting);

					transaction.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);
					AssertEquals("ApplyButtonText", "&Save", ZFormPostingButtonsStrategy.ApplyButtonText(form).Text);
					AssertEquals("ApplyButtonText", "S&ave && Close", ZFormPostingButtonsStrategy.PostButtonText(form).Text);
					transaction.RemoveContext(APInvoiceChargesApprovalRequest.Context.Editing);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public virtual void TestIsReversingMode()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				foreach (var displayMode in Enum.GetValues(typeof(ODisplayMode)))
				{
					testForm.DisplayMode = (ODisplayMode)displayMode;
					bool expectedValue = ExpectedIsReveringModeValue(testForm);
					AssertEquals(expectedValue, testForm.IsReversingMode_ForTestOnly);
				}
			}
		}

		protected virtual bool ExpectedIsReveringModeValue(AccountingZForm testForm) => testForm.DisplayMode == ODisplayMode.Delete && testForm.ReverseTransaction_ForTestOnly != null;

		public void TestDataExportBatchPluginIsAddedWhereNeeded()
		{
			using (var form = (AccountingZForm)GetFormToBashCore())
			{
				if (ShouldHavePluginIfBusinessEntityIsHeader(form) || ShouldHavePluginIfBusinessEntityIsLine(form))
				{
					AssertNotNull("Should have plugin", form.PlugIns.GetPlugIn(ControllerIDs.DataExportBatchPlugin));
				}
				else
				{
					Assert(true);
				}
			}
		}

		bool ShouldHavePluginIfBusinessEntityIsHeader(AccountingZForm form)
		{
			var header = form.BusinessEntity as TransactionHeader;
			return header != null &&
				(header.AH_Ledger == LedgerTypes.AccountsPayable ||
				header.AH_Ledger == LedgerTypes.AccountsReceivable ||
				header.AH_Ledger == LedgerTypes.General ||
				header.AH_Ledger == LedgerTypes.JobCosting ||
				header.AH_Ledger == LedgerTypes.TransactionsPendingAllocation ||
				(header.AH_Ledger == LedgerTypes.CashBook && header.AH_TransactionType != TransactionTypes.DDRBatch));
		}

		bool ShouldHavePluginIfBusinessEntityIsLine(AccountingZForm form)
		{
			var line = form.BusinessEntity as TransactionLine;
			return line != null &&
				(line.AL_LineType == TransactionLineTypes.Accrual ||
				line.AL_LineType == TransactionLineTypes.WIP);
		}

		public void TestSave()
		{
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			using (AccountingZForm testForm = new AccountingZForm(header))
			{
				testForm.Save_ForTestOnly(new ITransactionParticipant[] { Factory });
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ContinueWithSave.Yes, testForm.ValidateAndSave_ForTestOnly());

				testForm.Transaction_ForTestOnly.AH_InvoiceAmount = 5;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				string msg = string.Empty;
				string devMessage = string.Empty;
				try
				{
					//TODO: Hello, fellow developer! If this test is failing, uncomment the "DISABLE TRIGGER" command below and remove this TODO line.
					//HACK: Temporarily allow UPDATE of AH_InvoiceAmount to test critical validation. Refer to WI00559931, WI00482153.
					//TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_AccTransactionHeader_ProtectCriticalFieldsFromUpdating ON AccTransactionHeader");
					testForm.Save_ForTestOnly(new ITransactionParticipant[] { Factory });
					Fail("Should stop execution. The exception is caught higher in the stack.");
				}
				catch (OnSavingCriticalCheckException ex)
				{
					devMessage = ex.DeveloperErrorMessage;
					msg = ex.Message;

					string expectedDeveloperMessage = string.Format(@"Header: PK = {4}, Ledger = AP, Transaction Type = {0}, Invoice Date = 4/03/2004 12:00:00 AM, Post Date = , Invoice Amount = 5, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = , Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = {1}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = Yes, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Fields with changes: AH_InvoiceAmount (0, 5), AH_SystemLastEditTimeUtc ({2}, {3})", testForm.Transaction_ForTestOnly.AH_TransactionType, testForm.Transaction_ForTestOnly.AH_TransactionNum, testForm.Transaction_ForTestOnly.AH_SystemLastEditTimeUtcInfo.OriginalValue, testForm.Transaction_ForTestOnly.AH_SystemLastEditTimeUtcInfo.Value, testForm.Transaction_ForTestOnly.PK);
					AssertContains(devMessage, ex.DeveloperErrorMessage);
				}
				catch (ZCannotSaveException ex)
				{
					msg = ex.Message;
				}

				string expectedUserMessage = string.Format(@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Incorrect outstanding amount.");

				AssertContains(expectedUserMessage, msg);
			}

			AssertNotNull("ExceptionReporter should have caught the exceptions", ExceptionReporterTestListener.Instance[0]);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSaveWithCriticalValidationException()
		{
			var creator = new TestObjectCreator(Factory);

			creator.CreateTestPeriods(ZDateTime.Today);

			var shipment = creator.CreateShipment("S001001");
			var job = creator.CreateJob(shipment, false, false);

			var invoice = creator.CreateInvoice(typeof(APInvoice), "AP001", creator.AUD, 1M, creator.AALSHI);
			var line = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 100M);
			line.GenericCharge = creator.FRT.PK;
			line.AL_JH = job.PK;
			line.AL_GE = creator.FIADepartment.PK;

			creator.CreateJobCharge(line, job, creator.FRT, creator.AUD);
			creator.CreateWIP(job, creator.FRT, 1M, "test wip", 10M);

			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			using (var testForm = new AccountingZForm(invoice))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Assert("Set true on initialization", testForm.LastSaveSuccessful_ForTestOnly);
				try
				{
					testForm.ValidateAndSave_ForTestOnly();
					Fail("Should stop execution. The exception is caught higher in the stack.");
				}
				catch { }
				Assert("Set false on critical validaiton error", !testForm.LastSaveSuccessful_ForTestOnly);
			}

			AssertEquals("ExceptionReporter should catch the critical validtion error and raise 1 issue", 1, ExceptionReporterTestListener.Instance.Count); //only one issue should be raised for the critical validation failure
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestSaveSuspendsCreditCheckerValidation()
		{
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_OH = org.PK;

			org.CompanyData.OB_IsDebtor = ZBool.True;
			org.CompanyData.OB_ARCreditLimit = 33m;
			header.AH_InvoiceAmount = header.AH_OutstandingAmount = 34M;
			Factory.Save();

			HeaderWithValidationOnSaving headerWithValidation = Factory.NewWithValidTestData<HeaderWithValidationOnSaving>();
			headerWithValidation.AH_Ledger = LedgerTypes.AccountsReceivable;
			headerWithValidation.AH_OH = org.PK;

			using (AccountingZForm testForm = new AccountingZForm(headerWithValidation))
			{
				testForm.Save_ForTestOnly(new ITransactionParticipant[] { Factory });
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNoWarnings("Should be no warnings because CheditChecker validation was suspended during Save", headerWithValidation.AH_OHInfo);
			}
		}

		public void TestHandleSaveExceptionDoesNotThrowNullReferenceException()
		{
			var header = Factory.NewWithValidTestData<HeaderWithActionOnSaving>();
			header.AH_InvoiceDate = ZDateTime.Today;

			using (AccountingZForm testForm = new AccountingZForm(header))
			{
				header.ActionOnSaving = () =>
				{
					((KForm)testForm).DataSource = null;
					throw new ZCannotSaveException("TEST", "Error");
				};

				AssertNoExceptionThrown(() => testForm.FireSaveButton());
				AssertEquals("No NullReferenceException reported", 0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		class HeaderWithValidationOnSaving : AccTransactionHeader
		{
			public HeaderWithValidationOnSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnFactorySaving()
			{
				base.OnFactorySaving();
				new CreditChecker(Header).ValidateIsCreditLimitExceeded(AH_OHInfo, LedgerTypes.AccountsReceivable);
			}
		}

		class HeaderWithActionOnSaving : AccTransactionHeader
		{
			public HeaderWithActionOnSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public Action ActionOnSaving { get; set; }

			protected override void OnFactorySaving()
			{
				base.OnFactorySaving();
				ActionOnSaving?.Invoke();
			}
		}

		public void TestComplianceSubTypeIsEditableForSupportedCountries()
		{
			foreach (string country in TestObjectCreator.CountriesSupportComplianceSubtype)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
					{
						testForm.DisplayMode = ODisplayMode.Delete;
						testForm.Show();
						Application.DoEvents();

						BusinessObject header = testForm.BusinessEntity as BusinessObject;
						if (header.ZPropertyInfoHash.ContainsKey(TransactionHeader.Schema.AH_ComplianceSubType))
						{
							if (header is InvoicingBase)
							{
								ZString ledgerType = ((InvoicingBase)header).AH_Ledger;
								if (ledgerType == LedgerTypes.AccountsPayable || ledgerType == LedgerTypes.UnapprovedPayableTransactions || ledgerType == LedgerTypes.IncompleteTransactions)
								{
									if (country != Core.Constants.CountryCodes.China)
									{
										AssertEquals(string.Format("AH_ComplianceSubType must be false for {0}.", country), false, header.ZPropertyInfoHash[TransactionHeader.Schema.AH_ComplianceSubType].ReadOnly);
									}
									else
									{
										AssertEquals(string.Format("AH_ComplianceSubType must be true for {0}.", country), true, header.ZPropertyInfoHash[TransactionHeader.Schema.AH_ComplianceSubType].ReadOnly);
									}
								}
							}
						}
					}
				}
			}
			Assert(true);
		}

		public void TestMakeRequiredFieldsEditable()
		{
			bool allowBackPosting = AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value;
			bool receivablesAllowed = Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
			bool payablesAllowed = Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
			bool cashBookAllowed = Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed;
			ZString currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam);
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

				using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
				{
					testForm.DisplayMode = ODisplayMode.Delete;
					testForm.Show();
					Application.DoEvents();

					BusinessObject header = testForm.BusinessEntity as BusinessObject;

					bool isReadonly;
					if (header.ZPropertyInfoHash.ContainsKey(TransactionHeader.Schema.AH_TransactionNum))
					{
						isReadonly = !(header is InvoicingBase) || ((InvoicingBase)header).AH_Ledger != ZArchitecture.Core.LedgerTypes.AccountsPayable;
						AssertEquals("AH_TransactionNum.IsReadOnly", isReadonly, header.ZPropertyInfoHash[TransactionHeader.Schema.AH_TransactionNum].ReadOnly);
					}

					if (header.ZPropertyInfoHash.ContainsKey(InvoicingBase.Schema.AH_Calc_AmendStatusCode))
					{
						if (header is InvoicingBase invoicingBase)
						{
							invoicingBase.OriginalTransactionReference = ZGuid.NewZGuid();
							AssertEquals("AH_Calc_AmendStatusCode.IsReadOnly", !testForm.IsReversingMode_ForTestOnly && !invoicingBase.IsAllowModifyAmendStatusCode, header.ZPropertyInfoHash[InvoicingBase.Schema.AH_Calc_AmendStatusCode].ReadOnly);

							invoicingBase.OriginalTransactionReference = ZGuid.Empty;
							AssertEquals("AH_Calc_AmendStatusCode.IsReadOnly", !testForm.IsReversingMode_ForTestOnly && !invoicingBase.IsAllowModifyAmendStatusCode, header.ZPropertyInfoHash[InvoicingBase.Schema.AH_Calc_AmendStatusCode].ReadOnly);
						}
					}

					if (header.ZPropertyInfoHash.ContainsKey(TransactionHeader.Schema.AH_PostDate))
					{
						isReadonly = !(header is ITransaction transactionHeader) || !transactionHeader.UserAllowedToBackPost;
						AssertEquals("AH_PostDateInfo.ReadOnly", isReadonly, header.ZPropertyInfoHash[TransactionHeader.Schema.AH_PostDate].ReadOnly);
					}

					if (header.ZPropertyInfoHash.ContainsKey(TransactionHeader.Schema.AH_InvoiceDate))
					{
						if (header is InvoicingBase)
						{
							ZString ledgerType = ((InvoicingBase)header).AH_Ledger;
							isReadonly = true;
							if (ledgerType == LedgerTypes.AccountsReceivable && (header is ARInvoice || header is ARCreditNote))
							{
								isReadonly = false;
							}
							else if (ledgerType == LedgerTypes.AccountsPayable && (header is APInvoice || header is APCreditNote))
							{
								isReadonly = false;
							}
							AssertEquals("AH_InvoiceDateInfo.ReadOnly", isReadonly, header.ZPropertyInfoHash[TransactionHeader.Schema.AH_InvoiceDate].ReadOnly);
						}
					}

					if (header.ZPropertyInfoHash.ContainsKey(TransactionHeader.Schema.SupportingDocumentNumber))
					{
						if (header is InvoicingBase invoice)
						{
							isReadonly = true;
							if (header is ARCreditNote)
							{
								isReadonly = false;
							}
							AssertEquals("SupportingDocumentNumber.ReadOnly", isReadonly, header.ZPropertyInfoHash[TransactionHeader.Schema.SupportingDocumentNumber].ReadOnly);
						}
					}

					if (header.ZPropertyInfoHash.ContainsKey(InvoicingBase.Schema.ReversalStatusCode))
					{
						if (header is InvoicingBase invoicingBase)
						{
							isReadonly = true;
							if (testForm.IsReversingMode_ForTestOnly)
							{
								isReadonly = false;
							}
							AssertEquals("ReversalStatusCode.IsReadOnly", isReadonly, header.ZPropertyInfoHash[InvoicingBase.Schema.ReversalStatusCode].ReadOnly);
						}
					}

					var writable = new List<string> {
						TransactionHeader.Schema.AH_TransactionNum,
						TransactionHeader.Schema.AH_PostDate,
						TransactionHeader.Schema.AH_ComplianceSubType,
						TransactionHeader.Schema.AH_InvoiceDate,
						TransactionHeader.Schema.SupportingDocumentNumber,
						InvoicingBase.Schema.AH_Calc_AmendStatusCode,
						InvoicingBase.Schema.ReversalStatusCode
					};

					var method = header.GetType().GetMethod("GetWritableProperties", BindingFlags.Instance | BindingFlags.NonPublic);
					if (method != null)
					{
						writable.AddRange((List<string>)(method.Invoke(header, Array.Empty<object>())));
					}

					var allProperties = header.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => !writable.Contains(x.Name) && header.ZPropertyInfoHash.ContainsKey(x.Name));
					foreach (ZPropertyInfo info in header.ZPropertyInfoHash)
					{
						if (!writable.Contains(info.Name) && allProperties.Any(x => x.Name == info.Name))
						{
							AssertEquals(info.Name + " should be readonly", true, info.ReadOnly);
						}
					}
				}

				foreach (string country in TestObjectCreator.CountriesSupportComplianceSubtype)
				{
					using (AccountingZForm form = (AccountingZForm)GetFormToBashCore())
					{
						GlbCompany.CurrentCompany.SetCountry(country);
						form.DisplayMode = ODisplayMode.Delete;
						form.Show();
						Application.DoEvents();

						BusinessObject header = form.BusinessEntity as BusinessObject;

						if (header.ZPropertyInfoHash.ContainsKey(TransactionHeader.Schema.AH_ComplianceSubType))
						{
							if (header is InvoicingBase)
							{
								ZString ledgerType = ((InvoicingBase)header).AH_Ledger;
								if (ledgerType == LedgerTypes.AccountsPayable || ledgerType == LedgerTypes.UnapprovedPayableTransactions || ledgerType == LedgerTypes.IncompleteTransactions)
								{
									if (country != Core.Constants.CountryCodes.China)
									{
										AssertEquals("AH_ComplianceSubType.IsReadOnly", false, header.ZPropertyInfoHash[TransactionHeader.Schema.AH_ComplianceSubType].ReadOnly);
									}
									else
									{
										AssertEquals("AH_ComplianceSubType.IsReadOnly", true, header.ZPropertyInfoHash[TransactionHeader.Schema.AH_ComplianceSubType].ReadOnly);
									}
								}
								else if (header is IReversing && ledgerType == LedgerTypes.AccountsReceivable && country == Core.Constants.CountryCodes.Portugal)
								{
									AssertEquals("AH_ComplianceSubType.IsReadOnly", false, header.ZPropertyInfoHash[TransactionHeader.Schema.AH_ComplianceSubType].ReadOnly);
								}
							}
						}
					}
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(currentCountryCode);
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowBackPosting);
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = receivablesAllowed;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = payablesAllowed;
				Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = cashBookAllowed;
			}
		}

		public void TestMakeRequiredFieldsEditable_Reversing()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				InvoicingBase invoice = testForm.BusinessEntity as InvoicingBase;

				if (invoice != null)
				{
					invoice.Lines.AddNew();
					invoice.GenerateReverseTransaction(true);
					invoice.SetCancellationFlag(true);
					var reverseInvoice = invoice.ReverseInvoice ?? invoice;
					reverseInvoice.SetCancellationFlag(true);

					testForm.DisplayMode = ODisplayMode.Delete;
					testForm.Show();
					Application.DoEvents();

					foreach (InvoicingLineBase line in invoice.Lines)
					{
						foreach (ZPropertyInfo info in line.ZPropertyInfoHash)
						{
							AssertEquals(info.Name + " should be readonly", true, info.ReadOnly);
						}
					}
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestUnmatchDateReadOnlyDuringReversing()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				var unmatchOnReversingObject = testForm.BusinessEntity as IUnmatchOnReversing;

				if (unmatchOnReversingObject != null)
				{
					unmatchOnReversingObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Today, AllowBackPosting = true });
					AssertEquals("Precondition: MinUnmatchDate value", ZDateTime.Today, unmatchOnReversingObject.UnmatchingData.MinUnmatchDate);
					AssertEquals("Precondition: AllowBackPosting value", true, unmatchOnReversingObject.UnmatchingData.AllowBackPosting);
					AssertEquals("Precondition: check if UnmatchDate is actually not readonly before showiong the form", false, unmatchOnReversingObject.UnmatchDateInfo.ReadOnly);

					testForm.DisplayMode = ODisplayMode.Delete;
					testForm.Show();
					Application.DoEvents();

					AssertEquals("Should not be readonly after showing the form.", false, unmatchOnReversingObject.UnmatchDateInfo.ReadOnly);

					unmatchOnReversingObject.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Empty, AllowBackPosting = true });
					AssertEquals("Precondition: MinUnmatchDate value", ZDateTime.Empty, unmatchOnReversingObject.UnmatchingData.MinUnmatchDate);
					AssertEquals("Precondition: AllowBackPosting value", true, unmatchOnReversingObject.UnmatchingData.AllowBackPosting);
					AssertEquals("Should be readonly if unmatching was not done.", true, unmatchOnReversingObject.UnmatchDateInfo.ReadOnly);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestPromptReversingReason()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				if (testForm.ReverseTransaction_ForTestOnly != null && ShouldTestForReversingReason)
				{
					ZString desc = "";
					testForm.FReversingCode_ForTestOnly = "IDE";
					testForm.FReversingReason_ForTestOnly = "test reason";
					desc = testForm.FReversingCode_ForTestOnly + " - " + testForm.FReversingReason_ForTestOnly;
					testForm.GetReversingReasonAndCode_ForTestOnly();
					AssertEquals("IDE - test reason", desc);
				}
				else
				{
					Assert(true);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestGetReversingReasonAndCode_ReverseTransactionIsNull()
		{
			using (var testForm = GetFormToBashCore() as AccountingZForm)
			{
				AssertNotNull(testForm);
				var type = typeof(KForm);
				var property = type.GetProperty("DataSource", BindingFlags.Public | BindingFlags.Instance);
				property.SetValue(testForm, null);

				AssertEquals(false, testForm.GetReversingReasonAndCode_ForTestOnly());
			}
		}

		public void TestCheckConsolidatedInvoiceReferenceNotInDatabaseWhenReversingWithInvoiceNumberRefOutOfLimitation()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var testObjectCreatorinNewFactory = new TestObjectCreator(Factory);
			testObjectCreatorinNewFactory.CreateTestPeriods(ZDate.Today);
			var shipment = testObjectCreator.CreateShipment("S00001024");
			var job = testObjectCreator.CreateJob(shipment);
			job.JH_JobNum = "S00001024";
			var existedInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			existedInvoice.AH_Ledger = "AR";
			existedInvoice.AH_JH = job.PK;
			existedInvoice.AH_TransactionType = "INV";
			existedInvoice.AH_ConsolidatedInvoiceRef = "S00001024/ZZ";
			Factory.Save();

			var reverseInvoice = testObjectCreatorinNewFactory.CreateARCreditNote("1000", testObjectCreatorinNewFactory.AALSHI, testObjectCreatorinNewFactory.AUD, 1m);
			reverseInvoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(reverseInvoice, job);
			reverseInvoice.AH_OH = testObjectCreatorinNewFactory.Debtor1.PK;
			reverseInvoice.AH_OSTotalAmount = 100;

			using (var form = new AccountingZForm(reverseInvoice))
			{
				form.FReversingCode_ForTestOnly = "IDE";
				form.FReversingReason_ForTestOnly = "Correct Data Entry";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var result = form.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("AH_ConsolidatedInvoiceRef has problems", ContinueWithDelete.No, result);
				AssertEquals("The maximum number of invoices for a job is 703. You cannot post any more invoices for this job.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolidatedInvoiceReferenceNumberIsNotDuplicatedByConcurrencyErrorWhenReversing()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var testObjectCreatorinNewFactory = new TestObjectCreator(newFactory);
			testObjectCreatorinNewFactory.CreateTestPeriods(ZDate.Today);

			var shipment = testObjectCreator.CreateShipment("S00001024");
			var job = testObjectCreator.CreateJob(shipment);
			job.JH_JobNum = "S00001024";
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(ARInvoice), testObjectCreator.AUD, 1m, testObjectCreator.ABIGAS);
			invoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(invoice, job);
			invoice.AH_OH = testObjectCreatorinNewFactory.Debtor1.PK;
			AssertEquals("S00001024", invoice.AH_ConsolidatedInvoiceRef);

			var reverseInvoice = testObjectCreatorinNewFactory.CreateARCreditNote("1000", testObjectCreatorinNewFactory.ABIGAS, testObjectCreatorinNewFactory.AUD, 1m);
			reverseInvoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(reverseInvoice, job);
			AssertEquals("S00001024", reverseInvoice.AH_ConsolidatedInvoiceRef);
			reverseInvoice.AH_OH = testObjectCreatorinNewFactory.Debtor1.PK;
			reverseInvoice.AH_OSTotalAmount = 100;

			Factory.Save();

			using (var form = new AccountingZForm(reverseInvoice))
			{
				form.FReversingCode_ForTestOnly = "IDE";
				form.FReversingReason_ForTestOnly = "Incorrect Data Entry";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var result = form.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("AH_ConsolidatedInvoiceRef should not be duplicated", ContinueWithDelete.No, result);
				AssertEquals("While you were working, the automatically assigned job invoice number was used by another user.\r\nPlease close the form and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolidatedInvoiceReferenceNumberIsNotDuplicatedByConcurrencyErrorWhenReversing_ShouldNotBeAffectedByAPInvoiceWithSameRef()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var testObjectCreatorinNewFactory = new TestObjectCreator(newFactory);
			testObjectCreatorinNewFactory.CreateTestPeriods(ZDate.Today);

			var shipment = testObjectCreator.CreateShipment("S00001024");
			var job = testObjectCreator.CreateJob(shipment);
			job.JH_JobNum = "S00001024";

			Factory.Save();

			var aPinvoice = testObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m);
			aPinvoice.AH_JH = job.PK;

			var reverseInvoice = testObjectCreatorinNewFactory.CreateARCreditNote("1000", testObjectCreatorinNewFactory.AALSHI, testObjectCreatorinNewFactory.AUD, 1m);
			reverseInvoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(reverseInvoice, job);
			AssertEquals("S00001024", reverseInvoice.AH_ConsolidatedInvoiceRef);
			reverseInvoice.AH_OH = testObjectCreatorinNewFactory.Debtor1.PK;
			reverseInvoice.AH_OSTotalAmount = 100;

			Factory.Save();

			aPinvoice.AH_ConsolidatedInvoiceRef = "S00001024";
			Factory.Save();
			AssertEquals("Precondition: APInvoice has same AH_ConsolidatedInvoiceRef as ARInvoice", aPinvoice.AH_ConsolidatedInvoiceRef, reverseInvoice.AH_ConsolidatedInvoiceRef);

			using (var form = new AccountingZForm(reverseInvoice))
			{
				form.FReversingCode_ForTestOnly = "IDE";
				form.FReversingReason_ForTestOnly = "Correct Data Entry";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var result = form.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("AH_ConsolidatedInvoiceRef has no problems", ContinueWithDelete.Yes, result);
				AssertNotEquals("While you were working, the automatically assigned job invoice number was used by another user.\r\nPlease close the form and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCurrentUserIsNull()
		{
			using (var testForm = GetFormToBashCore() as AccountingZForm)
			{
				AssertNotNull(testForm);
				if (testForm.ReverseTransaction_ForTestOnly != null && ShouldTestForReversingReason)
				{
					((BusinessObject)testForm.BusinessEntity).FillWithValidTestData();
					((IReversing)testForm.BusinessEntity).ReversingCode = "IDE";
					((IReversing)testForm.BusinessEntity).ReversingReason = "test reason";

					var context = Env.Instance.CurrentUserContext;
					var userContext = new UserContext(context.User, context.Branch.PK, context.Department.PK);

					var type1 = userContext.GetType();
					var property = type1.GetProperty("User", BindingFlags.Public | BindingFlags.Instance);
					AssertNotNull(property);
					property.SetValue(userContext, null);

					using (Env.Instance.SetTemporaryUserContext(userContext))
					{
						AssertNoExceptionThrown(() => testForm.GetReversingReasonAndCode_ForTestOnly());
					}
				}
				else
				{
					Assert(true);
				}
			}
		}

		public virtual void TestFormVerb()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				InvoicingBase transaction = testForm.BusinessEntity as InvoicingBase;
				IBadDebtWritingOff badDebt = testForm.BusinessEntity as IBadDebtWritingOff;
				if (badDebt != null)
				{
					badDebt.IsWritingOff = true;
					AssertEquals("Verb should be 'Write Off'", "Write Off", testForm.FormVerb);
					badDebt.IsWritingOff = false;
					AssertEquals("Verb should be 'Reverse'", "Reverse", testForm.FormVerb);
				}
				else
				{
					if (transaction != null)
					{
						if (transaction.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || (transaction.AH_Ledger == LedgerTypes.IncompleteTransactions && testForm.CancelInsteadOfDelete))
						{
							AssertEquals("Verb should be 'Cancel'", "Cancel", testForm.FormVerb);
						}
						else if (transaction.AH_Ledger == LedgerTypes.IncompleteTransactions && !testForm.CancelInsteadOfDelete)
						{
							AssertEquals("Verb", "Delete", testForm.FormVerb);
						}
						else
						{
							AssertEquals("Verb should be 'Reverse'", "Reverse", testForm.FormVerb);
						}
					}
					else if (testForm.BusinessEntity is IReversing)
					{
						AssertEquals("Verb should be 'Reverse'", "Reverse", testForm.FormVerb);
					}
					else
					{
						AssertEquals("Verb should be 'Delete'", "Delete", testForm.FormVerb);
					}
				}
				testForm.DisplayMode = ODisplayMode.New;
				if (badDebt != null)
				{
					badDebt.IsWritingOff = true;
					AssertEquals("Verb should be 'New'", "New", testForm.FormVerb);
					badDebt.IsWritingOff = false;
					AssertEquals("Verb should be 'New'", "New", testForm.FormVerb);
				}
				else if (transaction != null && transaction.AH_Ledger == LedgerTypes.IncompleteTransactions)
				{
					AssertEquals("Verb", "Edit", testForm.FormVerb);
				}
				else
				{
					AssertEquals("Verb should be 'New'", "New", testForm.FormVerb);
				}
				((BusinessObject)testForm.BusinessEntity).FillWithValidTestData();
				testForm.BusinessEntity.Factory.Save();
				if (((BusinessObject)testForm.BusinessEntity).IsInDatabase)
				{
					if (transaction != null && transaction.AH_Ledger == LedgerTypes.IncompleteTransactions)
					{
						AssertEquals("Verb", "Edit", testForm.FormVerb);
					}
					else
					{
						AssertEquals("Verb should be 'View'", "View", testForm.FormVerb);
					}
				}
				else
				{
					AssertEquals("Verb should be 'New'", "New", testForm.FormVerb);
				}
			}
		}

		public virtual void TestDeleteButtonText()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				IBadDebtWritingOff badDebt = testForm.BusinessEntity as IBadDebtWritingOff;
				if (badDebt != null)
				{
					badDebt.IsWritingOff = true;
					AssertEquals("Delete Button Text should be '&Write Off'", "&Write Off", ZFormPostingButtonsStrategy.DeleteButtonText(testForm).Text);
					badDebt.IsWritingOff = false;
					AssertEquals("Delete Button Text should be '&Reverse'", "&Reverse", ZFormPostingButtonsStrategy.DeleteButtonText(testForm).Text);
				}
				else
				{
					InvoicingBase transaction = testForm.BusinessEntity as InvoicingBase;
					if (transaction != null && transaction.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
					{
						AssertEquals("Delete Button Text should be 'Reject'", "Reject", ZFormPostingButtonsStrategy.DeleteButtonText(testForm).Text);
					}
					else if (transaction != null && transaction.AH_Ledger == LedgerTypes.IncompleteTransactions)
					{
						AssertEquals("Delete Button Text should be 'Delete'", "&Delete", ZFormPostingButtonsStrategy.DeleteButtonText(testForm).Text);
					}
					else
					{
						AssertEquals("Delete Button Text should be '&Reverse'", "&Reverse", ZFormPostingButtonsStrategy.DeleteButtonText(testForm).Text);
					}
				}
			}
		}

		public void TestDelete()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				AssertNotNull("TestForm.BusinessEntity", testForm.BusinessEntity);
				InvoicingBase transaction = testForm.BusinessEntity as InvoicingBase;

				if (transaction != null)
				{
					transaction.AH_TransactionNum = "TEST_TRANSACTION";

					bool canBeCancelled = transaction.AH_Ledger == LedgerTypes.IncompleteTransactions;
					if (canBeCancelled)
					{
						testForm.CancelInsteadOfDelete = true;

						testForm.Delete_ForTestOnly();
						AssertEquals("BizO should be canceled", true, transaction.AH_IsCancelled);

						testForm.CancelInsteadOfDelete = false;
					}

					bool shouldBePhysicallyDeleted = transaction.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || transaction.AH_Ledger == LedgerTypes.IncompleteTransactions;

					testForm.Delete_ForTestOnly();
					AssertEquals("Bizo should be deleted only for UA and IN transactions.", shouldBePhysicallyDeleted, ((BusinessObject)testForm.BusinessEntity).IsDeleted);
				}
			}
		}

		public void TestSaveToRecentItemWithTransaction()
		{
			using (var testForm = (AccountingZForm)GetFormToBashCore())
			{
				AssertNotNull("TestForm.BusinessEntity", testForm.BusinessEntity);
				var transaction = testForm.BusinessEntity as InvoicingBase;

				if (transaction != null)
				{
					transaction.AH_TransactionNum = "TEST_TRANSACTION";

					if (!transaction.IsInDatabase)
					{
						transaction.AcceptDataRowChangesToTestOriginalValueWhenWeCantSave_ForTestOnly();
					}

					AssertEquals("TestForm.IsINTransactionWithApprovalRequest_ForTestOnly", false, testForm.IsINTransactionWithApprovalRequest_ForTestOnly);
					AssertEquals("Transaction.IsAllocatingInvoice", false, transaction.IsAllocatingInvoice);
					AssertEquals("Transaction should be in database", true, transaction.IsInDatabase);

					if (transaction.AH_Ledger == LedgerTypes.IncompleteTransactions)
					{
						AssertEquals("TestForm.IsINTransaction_ForTestOnly", true, testForm.IsINTransaction_ForTestOnly);
						AssertEquals("SaveToRecentItems Counter", 0, testForm.SaveToRecentItemsCounterForTest);

						testForm.SaveToRecentItems_ForTestOnly();

						AssertEquals("Should not save to RecentItems", 0, testForm.SaveToRecentItemsCounterForTest);
					}
					else
					{
						AssertEquals("TestForm.IsINTransaction_ForTestOnly", false, testForm.IsINTransaction_ForTestOnly);
						AssertEquals("SaveToRecentItems Counter", 0, testForm.SaveToRecentItemsCounterForTest);

						testForm.SaveToRecentItems_ForTestOnly();

						AssertEquals("Should save to RecentItems only once", 1, testForm.SaveToRecentItemsCounterForTest);
					}
				}
			}

			using (var testForm = (AccountingZForm)GetFormToBashCore())
			{
				AssertNotNull("TestForm.BusinessEntity", testForm.BusinessEntity);
				var transaction = testForm.BusinessEntity as InvoicingBase;

				if (transaction != null)
				{
					transaction.AH_TransactionNum = "TEST_TRANSACTION";
					transaction.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
					transaction.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
					((INeedRow)transaction).Row.AcceptChanges();

					AssertEquals("TestForm.IsINTransactionWithApprovalRequest_ForTestOnly", false, testForm.IsINTransactionWithApprovalRequest_ForTestOnly);
					AssertEquals("TestForm.IsINTransaction_ForTestOnly", false, testForm.IsINTransaction_ForTestOnly);
					AssertEquals("Transaction.IsAllocatingInvoice", false, transaction.IsAllocatingInvoice);
					AssertEquals("Transaction should be in database", true, transaction.IsInDatabase);

					transaction.AH_Ledger = LedgerTypes.AccountsPayable;

					AssertEquals("TestForm.IsINTransactionWithApprovalRequest_ForTestOnly", false, testForm.IsINTransactionWithApprovalRequest_ForTestOnly);
					AssertEquals("TestForm.IsINTransaction_ForTestOnly", false, testForm.IsINTransaction_ForTestOnly);
					AssertEquals("Transaction.IsAllocatingInvoice should now be true", true, transaction.IsAllocatingInvoice);
					AssertEquals("SaveToRecentItems Counter", 0, testForm.SaveToRecentItemsCounterForTest);

					testForm.SaveToRecentItems_ForTestOnly();

					AssertEquals("Should not save to RecentItems", 0, testForm.SaveToRecentItemsCounterForTest);

					transaction.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;

					AssertEquals("TestForm.IsINTransactionWithApprovalRequest_ForTestOnly", false, testForm.IsINTransactionWithApprovalRequest_ForTestOnly);
					AssertEquals("TestForm.IsINTransaction_ForTestOnly", false, testForm.IsINTransaction_ForTestOnly);
					AssertEquals("Transaction.IsAllocatingInvoice should now be true", true, transaction.IsAllocatingInvoice);
					AssertEquals("SaveToRecentItems Counter", 0, testForm.SaveToRecentItemsCounterForTest);

					testForm.SaveToRecentItems_ForTestOnly();

					AssertEquals("Should not save to RecentItems", 0, testForm.SaveToRecentItemsCounterForTest);

					transaction.AH_TransactionType = TransactionTypes.CreditNote;
					transaction.AH_Ledger = LedgerTypes.AccountsReceivable;

					AssertEquals("TestForm.IsINTransactionWithApprovalRequest_ForTestOnly", false, testForm.IsINTransactionWithApprovalRequest_ForTestOnly);
					AssertEquals("TestForm.IsINTransaction_ForTestOnly", false, testForm.IsINTransaction_ForTestOnly);
					AssertEquals("Transaction.IsAllocatingInvoice should now be true", true, transaction.IsAllocatingInvoice);
					AssertEquals("SaveToRecentItems Counter", 0, testForm.SaveToRecentItemsCounterForTest);

					testForm.SaveToRecentItems_ForTestOnly();

					AssertEquals("Should not save to RecentItems", 0, testForm.SaveToRecentItemsCounterForTest);
				}
			}
		}

		public void TestCancelForIncompleteTransactionsUseCorrectSaveMethod()
		{
			AssertDeleteIncompleteTransactionsUseCorrectSaveMethod(true);
		}

		public void TestDeleteForIncompleteTransactionsUseCorrectSaveMethod()
		{
			AssertDeleteIncompleteTransactionsUseCorrectSaveMethod(false);
		}

		void AssertDeleteIncompleteTransactionsUseCorrectSaveMethod(bool cancelInsteadOfDelete)
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				AssertNotNull("TestForm.BusinessEntity", testForm.BusinessEntity);
				InvoicingBase transaction = testForm.BusinessEntity as InvoicingBase;

				string exceptionMessage = "Critical Error that show be correctly shown.";
				string expectedMessage = "Critical Error that show be correctly shown.\r\n\r\nYou can also try to save the transaction as 'incomplete'";
				if (transaction != null && transaction.AH_Ledger == LedgerTypes.IncompleteTransactions)
				{
					BusinessObjectFactory.SavingEventHandler factorySavingWithCriticalValidationError = factory =>
					{
						throw new OnSavingCriticalCheckException<InvoicingBase>(transaction, CriticalValidationErrorType.DummyErrorKeyForTest, exceptionMessage, "E=MC2");
					};

					testForm.DisplayMode = ODisplayMode.Delete;
					testForm.CancelInsteadOfDelete = cancelInsteadOfDelete;
					transaction.Factory.Saving += factorySavingWithCriticalValidationError;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					((IPostingButtonsProvider)testForm).CommandButtonPost.PerformClick();

					AssertEquals("Critical Validation exceptions should be catched and shown as a message.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCancelConfirmationForINTransactions()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				AssertNotNull("TestForm.BusinessEntity", testForm.BusinessEntity);
				InvoicingBase transaction = testForm.BusinessEntity as InvoicingBase;

				if (transaction != null && transaction.AH_Ledger == LedgerTypes.IncompleteTransactions)
				{
					testForm.DisplayMode = ODisplayMode.Delete;
					testForm.CancelInsteadOfDelete = true;

					testForm.Show();
					Application.DoEvents();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					((IPostingButtonsProvider)testForm).CommandButtonPost.PerformClick();
					AssertEquals("BizO should be canceled", true, transaction.AH_IsCancelled);
					Assert("Cancel Confirmation", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("You are about to cancel this transaction. Do you want to proceed?"));
				}
			}
		}

		public virtual void TestPrevAndNextButton()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				testForm.Show();
				Assert(!testForm.AutoAddPreviousNextButtons);
			}
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (AccountingZForm)GetFormToBashCore())
			{
				if (ShouldHaveAuditPlugIn)
				{
					Assert("Should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
					Assert(form.ShowAuditTabForTest);
				}
				else
				{
					Assert(!form.ShowAuditTabForTest);
				}
			}
		}

		protected virtual bool ShouldHaveAuditPlugIn => false;

		protected virtual bool ShouldTestForReversingReason
		{
			get { return true; }
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		protected override void TearDown()
		{
			base.TearDown();
			ZFormModaliser.LastFormShownForTest = null;
			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
			GC.Collect();
		}
	}
}
