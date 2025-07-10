using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.GUI.Base.MultipleReversingForHeaderForm;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.Base.Testing
{
	[TestedType(typeof(MultipleReversingForHeaderForm))]
	public class MultipleReversingForHeaderFormTest : MultipleReversingBaseFormTest
	{
		public void TestMakeRequiredFieldsEditableForAPInvoice()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesModifyInvoiceDateWhenReversing.IsAllowed = true;

			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			multipleReversingProvider.BizObjectsForReversing.Add(invoice);
			invoice.GenerateReverseTransaction(false);
			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice.ReverseTransaction));

			using (var testForm = new MultipleReversingForHeaderForm(multipleReversingProvider))
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				Application.DoEvents();

				var header = ((MultipleReversingProviderForHeader)testForm.BusinessEntity).TransactionsAlreadyReversed[0] as BusinessObject;

				AssertEquals("TransactionNum.IsReadOnly", false, header.ZPropertyInfoHash["TransactionNumber"].ReadOnly);
				AssertEquals("PostDateInfo.ReadOnly", false, header.ZPropertyInfoHash["PostDate"].ReadOnly);
				AssertEquals("TransactionDateInfo.ReadOnly", false, header.ZPropertyInfoHash["TransactionDate"].ReadOnly);

				foreach (ZPropertyInfo info in header.ZPropertyInfoHash)
				{
					if (info.Name != "TransactionNumber" && info.Name != "PostDate" && info.Name != "TransactionDate" && info.Name != "AH_InvoiceDate" && info.Name != "SupportingDocumentNumber")
					{
						AssertEquals(info.Name + " should be readonly", true, info.ReadOnly);
					}
				}
			}
		}

		public void TestMakeRequiredFieldsEditableForARInvoice()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.ReceivablesModifyInvoiceDateWhenReversing.IsAllowed = true;

			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			multipleReversingProvider.BizObjectsForReversing.Add(invoice);
			invoice.GenerateReverseTransaction(false);
			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice.ReverseTransaction));

			using (var testForm = new MultipleReversingForHeaderForm(multipleReversingProvider))
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				Application.DoEvents();

				var header = ((MultipleReversingProviderForHeader)testForm.BusinessEntity).TransactionsAlreadyReversed[0] as BusinessObject;

				AssertEquals("PostDateInfo.ReadOnly", false, header.ZPropertyInfoHash["PostDate"].ReadOnly);
				AssertEquals("TransactionDateInfo.Readonly", false, header.ZPropertyInfoHash["TransactionDate"].ReadOnly);

				foreach (ZPropertyInfo info in header.ZPropertyInfoHash)
				{
					if (info.Name != "PostDate" && info.Name != "TransactionDate" && info.Name != "AH_InvoiceDate" && info.Name != "SupportingDocumentNumber")
					{
						AssertEquals(info.Name + " should be readonly", true, info.ReadOnly);
					}
				}
			}
		}

		public void TestMarkSupportingDocumentNumberFieldEditable()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.Debtor);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			Factory.Save();

			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			multipleReversingProvider.BizObjectsForReversing.Add(arInvoice);
			arInvoice.GenerateReverseTransaction(false);
			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(arInvoice.ReverseTransaction));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var testForm = new MultipleReversingForHeaderForm(multipleReversingProvider))
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				Application.DoEvents();

				var header = ((MultipleReversingProviderForHeader)testForm.BusinessEntity).TransactionsAlreadyReversed[0] as BusinessObject;

				AssertEquals("SupportingDocumentNumber should NOT be readonly", false, header.ZPropertyInfoHash["SupportingDocumentNumber"].ReadOnly);
			}
		}

		public void TestColumnVisibility()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.GenerateReverseTransaction(false);

			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			multipleReversingProvider.BizObjectsForReversing.Add(invoice);
			multipleReversingProvider.BizObjectsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice.ReverseTransaction));

			using (var testForm = new MultipleReversingForHeaderForm(multipleReversingProvider))
			{
				testForm.Show();
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "TransactionType", typeof(ZTextBoxColumnStyle), "Transaction Type", 94, 0);
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "Organization", typeof(ZGuidFindBoxColumnStyle), "Organization", 80, 1);
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "TransactionDate", typeof(ZDateEditColumnStyle), "Transaction Date", 93, 2);
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "PostDate", typeof(ZDateEditColumnStyle), "Post Date", 80, 3);
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "SupportingDocumentNumber", typeof(ZTextBoxColumnStyle), "Supporting Document Number", 140, 4);
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "UnmatchDate", typeof(ZDateEditColumnStyle), "Unmatch Date", 80, 5);
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "TransactionNumber", typeof(ZTextBoxColumnStyle), "Transaction Num.", 95, 6);
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "CurrencyCode", typeof(ZTextBoxColumnStyle), "Currency", 80, 7);
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "OverseasTotalAmount", typeof(ZCalcEditColumnStyle), "OS Total Amount", 92, 8);
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "Ledger", typeof(ZTextBoxColumnStyle), "Ledger", 80, 12, false);
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "OriginalTransactionNumber", typeof(ZTextBoxColumnStyle), "Original Transaction Num.", 140, 9);
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "OriginalTransactionType", typeof(ZTextBoxColumnStyle), "Original Transaction Type", 140, 10);
				AssertGridColumnInfo(testForm.Grid_ForTestOnly, "ReversalStatusCode", typeof(ZDropEditColumnStyle), "Reversal Code", 80, 11);
			}

			void AssertGridColumnInfo(ZGrid containersGrid, string columnName, Type expectedColumnStyleType, string expectedCaption, int expectedWidth, int expectedIndexOf, bool expectedIsVisible = true)
			{
				var gridColumnInfo = containersGrid.GetColumnStyle(columnName);
				var indexOfColumn = containersGrid.Columns.IndexOf(x => x.ColumnName == columnName);

				AssertEquals(columnName + " Column Style Type", expectedColumnStyleType, gridColumnInfo.ColumnStyleType);
				AssertEquals(columnName + " IsVisible", expectedIsVisible, gridColumnInfo.IsVisible);
				AssertEquals(columnName + " CaptionResourceString.Caption", expectedCaption, gridColumnInfo.CaptionResourceString.Caption);
				AssertEquals(columnName + " Width", expectedWidth, gridColumnInfo.Width);
				AssertEquals(columnName + " IndexOfColumn", expectedIndexOf, indexOfColumn);

				var columnInstance = containersGrid.Columns.OfType<ZGridColumn>().SingleOrDefault(x => x.ColumnName == columnName);

				AssertNotNull(columnName + " Column Instance", columnInstance);
				AssertEquals(columnName + " Column.Caption", expectedCaption, columnInstance.ColumnStyle.HeaderText);
			}
		}

		public new void TestMakeRequiredFieldsEditable()
		{
			Assert("Other 2 tests in this class replaces this one.", true);
		}

		public void TestFormCaption()
		{
			using (var form = (MultipleReversingForHeaderForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Caption: ", "Multiple Transactions", form.FormCaption);
			}
		}

		public void TestRemoveErrorTransactionsButtonCaption()
		{
			using (var form = (MultipleReversingForHeaderForm)GetFormToBashCore())
			{
				AssertEquals("Remove Error Transactions Button Caption: ", "Remove Original Transactions That Can\'t Be Reversed", form.RemoveErrorTransactionsButtonCaption_ForTestOnly.Caption);
			}
		}

		public new void TestDelete()
		{
			MultipleReversingProviderForHeader multipleReversingProvider = new MultipleReversingProviderForHeader();
			var invoice1 = Factory.NewWithValidTestData<UAInvoice>();
			var invoice2 = Factory.NewWithValidTestData<APInvoice>();
			invoice1.GenerateReverseTransaction(false);
			invoice2.GenerateReverseTransaction(false);

			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice1));
			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice2.ReverseTransaction));
			using (var form = new MultipleReversingForHeaderForm(multipleReversingProvider))
			{
				form.Show();
				Application.DoEvents();

				form.Delete_ForTestOnly();
				AssertEquals("UAInvoice must be deleted.", true, invoice1.IsDeleted);
				AssertEquals("Non UAInvoice must not be deleted.", false, ((BusinessObject)invoice2.ReverseTransaction).IsDeleted);
			}
		}

		public void TestDeleteWithConcurrentReverseClearsGrid()
		{
			var dataFromSetup = SetUpForReverseFrom();
			var controller = dataFromSetup.controllerForConcurrent;
			var invoicePK = dataFromSetup.arInvoicePK;

			using (var form = (MultipleReversingForHeaderForm)controller.LastShownForm)
			{
				AssertEquals(2, form.Grid_ForTestOnly.List.Count);

				form.OnShowPreDeleteDialogs_ForTestOnly += new EventHandler<DeleteDialogEventArgs>((x, e) =>
				{
					SimulateConcurrentReverse(invoicePK);
					e.Result = ZForm.ContinueWithDelete.Yes;
					e.ReturnResult = true;
				});

				string expectedMessage = @"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.";

				try
				{
					form.HandleApplyPostingButtonClickUnsafe_ForTestOnly(true);
				}
				catch (ZSaveConcurrencyException ex)
				{
					form.HandleSaveException_ForTestOnly(ex);
				}

				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertStartsWith("Concurrency Error", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Form is not closed", false, form.IsDisposed);
			}
		}

		public void TestDeleteWithConcurrentReverseFromAnotherUser()
		{
			var dataFromSetup = SetUpForReverseFrom();
			var controller = dataFromSetup.controllerForConcurrent;
			var invoicePK = dataFromSetup.arInvoicePK;

			using (var form = (MultipleReversingForHeaderForm)controller.LastShownForm)
			{
				AssertEquals(2, form.Grid_ForTestOnly.List.Count);

				form.OnShowPreDeleteDialogs_ForTestOnly += new EventHandler<DeleteDialogEventArgs>((x, e) =>
				{
					SimulateConcurrentReverse(invoicePK);
					e.Result = ZForm.ContinueWithDelete.Yes;
					e.ReturnResult = false;
				});

				form.HandleApplyPostingButtonClickUnsafe_ForTestOnly(true);

				AssertEquals("Form is not closed", false, form.IsDisposed);
				Assert(form.MultipleReversingProvider_ForTestOnly.HasErrors);
				var notification = form.MultipleReversingProvider_ForTestOnly.NotificationsIncludingChildren.FirstOrDefault();
				AssertNotNull(notification);
				AssertEquals("Error - Accounts Receivable Credit Note: This transaction was already reversed by another user.", notification.Message);
			}
		}

		public void TestDontRemoveTransactionWhenDeleteWithConcurrentReverseFromAnotherUser()
		{
			var dataFromSetup = SetUpForReverseFrom();
			var controller = dataFromSetup.controllerForConcurrent;
			var invoicePK = dataFromSetup.arInvoicePK;

			using (var form = (MultipleReversingForHeaderForm)controller.LastShownForm)
			{
				AssertEquals(2, form.Grid_ForTestOnly.List.Count);

				form.OnShowPreDeleteDialogs_ForTestOnly += new EventHandler<DeleteDialogEventArgs>((x, e) =>
				{
					SimulateConcurrentReverse(invoicePK);
					e.Result = ZForm.ContinueWithDelete.Yes;
					e.ReturnResult = false;
				});

				form.HandleApplyPostingButtonClickUnsafe_ForTestOnly(true);

				Assert(form.MultipleReversingProvider_ForTestOnly.HasErrors);
				var notification = form.MultipleReversingProvider_ForTestOnly.NotificationsIncludingChildren.FirstOrDefault();
				AssertNotNull(notification);
				AssertEquals("Error - Accounts Receivable Credit Note: This transaction was already reversed by another user.", notification.Message);

				form.RemoveErrorTransactionsButton_Click_ForTestOnly(null, null);
				AssertEquals("Can not remove transaction when it has been reversed by another user concurrently.", 2, form.Grid_ForTestOnly.List.Count);
			}
		}

		(ZController controllerForConcurrent, ZGuid arInvoicePK) SetUpForReverseFrom()
		{
			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			invoice.GenerateReverseTransaction(false);
			multipleReversingProvider.BizObjectsForReversing.Add(invoice);
			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice.ReverseTransaction));

			var controllerForConcurrent = ZControllerFactory.Create(ControllerIDs.ARInvoice);
			controllerForConcurrent.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });

			return (controllerForConcurrent, invoice.PK);
		}

		void SimulateConcurrentReverse(ZGuid invoicePK)
		{
			// Reverse transaction to simulate other user has done it
			var dbCommand = Db.Connection.Command(string.Format("UPDATE {0} SET {1} = 1, {2} = GETUTCDATE(), {3} = 'TST' WHERE {4} = @PK",  // SQL query, not translatable string
					 AccTransactionHeaderSchema.Constants.TableName,
					 AccTransactionHeaderSchema.Constants.AH_IsCancelled,
					 AccTransactionHeaderSchema.Constants.AH_SystemLastEditTimeUtc,
					 AccTransactionHeaderSchema.Constants.AH_SystemLastEditUser,
					 AccTransactionHeaderSchema.Constants.PK));
			dbCommand.AddParameter("@PK", SqlDbType.UniqueIdentifier, invoicePK.ToGuid());
			dbCommand.ExecuteNonQuery();
		}

		public void TestErrorReportedWhenRemovingTransactionWithChanged()
		{
			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			TestObjectCreator.ReverseTransaction(invoice2, out _);
			Factory.Save();

			invoice1.GenerateReverseTransaction(false);
			multipleReversingProvider.BizObjectsForReversing.Add(invoice2);
			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice1));

			var controllerForConcurrent = ZControllerFactory.Create(ControllerIDs.ARInvoice);
			controllerForConcurrent.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });

			using (var form = (MultipleReversingForHeaderForm)controllerForConcurrent.LastShownForm)
			{
				AssertEquals(2, form.Grid_ForTestOnly.List.Count);

				form.OnShowPreDeleteDialogs_ForTestOnly += new EventHandler<DeleteDialogEventArgs>((x, e) =>
				{
					e.Result = ZForm.ContinueWithDelete.Yes;
					e.ReturnResult = false;
				});

				form.HandleApplyPostingButtonClickUnsafe_ForTestOnly(true);

				invoice2.HasChanges = true;
				form.RemoveErrorTransactionsButton_Click_ForTestOnly(null, null);

				var expectedErrorMessage = "We will save this bizo changes when we do not expect it has any changes as it is original not reversed transaction.";
				AssertContains("LastExceptionReported.Message", expectedErrorMessage, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestDontPostIfThereareNoTransactions()
		{
			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.GenerateReverseTransaction(false);

			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice.ReverseTransaction));
			using (var form = new MultipleReversingForHeaderForm(multipleReversingProvider))
			{
				form.Show();
				Application.DoEvents();

				form.PostingButtons_ForTestOnly.SaveButton.Enabled = form.PostingButtons_ForTestOnly.SaveAndCloseButton.Enabled = true;

				Assert("Precondition: SaveButton is enabled.", form.PostingButtons_ForTestOnly.SaveButton.Enabled);
				Assert("Precondition: SaveAndCloseButton is enabled.", form.PostingButtons_ForTestOnly.SaveAndCloseButton.Enabled);

				multipleReversingProvider.TransactionsAlreadyReversed.RemoveAll();
				Assert("SaveButton is disabled.", !form.PostingButtons_ForTestOnly.SaveButton.Enabled);
				Assert("SaveAndCloseButton is disabled.", !form.PostingButtons_ForTestOnly.SaveAndCloseButton.Enabled);
			}
		}

		public void TestSetReadOnlyIncludingChildren()
		{
			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			multipleReversingProvider.BizObjectsForReversing.Add(invoice);
			invoice.GenerateReverseTransaction(false);
			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice.ReverseTransaction));

			using (var testForm = new MultipleReversingForHeaderForm(multipleReversingProvider))
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				Application.DoEvents();

				foreach (Control control in testForm.Controls)
				{
					Assert(control.Name + " must be editable", !control.GetReadOnly());
				}
			}
		}

		public void TestReOpenClosedJobGrantedMessageOnShowPreDeleteDialogs()
		{
			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.SecurityOverrideProvider = new ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider(multipleReversingProvider);

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_Status = "CLS";
			job1.JH_JobNum = "TESTJOB1";
			invoice.GenerateReverseTransaction(false);
			((InvoicingBase)invoice.ReverseTransaction).AddRelatedJobsForReversing_ForTestOnly(job1);

			Env.Security.ReopenJob.IsAllowed = true;

			multipleReversingProvider.BizObjectsForReversing.Add(invoice);
			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice.ReverseTransaction));

			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.ARInvoice);
			controller.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });

			using (var form = (MultipleReversingForHeaderForm)controller.LastShownForm)
			{
				form.ShowPreDeleteDialogs_ForTestOnly();

				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				string expectedMessage = @"Closed Job(s) :TESTJOB1
You are about to reopen these closed jobs. Do you want to proceed?";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				form.ReOpenClosedJob_ForTestOnly();
				AssertEquals("WRK", job1.JH_Status);
			}
		}

		[TestDate(2021, 1, 10)]
		public void TestComplianceErrorsOnShowPreDeleteDialogs_PST()
		{
			AssertComplianceErrorsOnShowPreDeleteDialogs(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2021, 1, 10)]
		public void TestComplianceErrorsOnShowPreDeleteDialogs_INV()
		{
			AssertComplianceErrorsOnShowPreDeleteDialogs(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertComplianceErrorsOnShowPreDeleteDialogs(string dateOption)
		{
			var today = ZDate.Today;
			var dateOutsideSequencePeriod = new ZDateTime(2020, 10, 15);

			const string subType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.XCL;
			Type invType = typeof(ARInvoice);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ARROS";
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

				var alterSeq = TestObjectCreator.SetupComplianceSequence(menuPK, subType, subType + ".21-", 1, 100, 2);
				alterSeq.XD_StartDate = new ZDate(today.Year, today.Month, 1);
				alterSeq.XD_ExpiryDate = alterSeq.XD_StartDate.AddMonths(1).AddDays(-2);
				alterSeq.XD_IsActive = true;

				var fullSeq = TestObjectCreator.SetupComplianceSequence(menuPK, subType, subType + ".21_", 1, 100, 101);
				fullSeq.XD_StartDate = alterSeq.XD_StartDate.AddMonths(1);
				fullSeq.XD_ExpiryDate = fullSeq.XD_StartDate.AddMonths(1).AddDays(-1);
				fullSeq.XD_IsActive = true;

				var invAlter = TestObjectCreator.CreateInvoice(invType);
				invAlter.AH_TransactionNum = "1000";
				SetInvoiceDate(invAlter, dateOption, alterSeq.XD_ExpiryDate.Date, dateOutsideSequencePeriod);
				invAlter.AH_ComplianceSubType = subType;

				var invFull = TestObjectCreator.CreateInvoice(invType);
				invFull.AH_TransactionNum = "1001";
				SetInvoiceDate(invFull, dateOption, fullSeq.XD_StartDate.AddDays(1), dateOutsideSequencePeriod);
				invFull.AH_ComplianceSubType = subType;

				invAlter.GenerateReverseTransaction(false);
				var revInvAlter = invAlter.ReverseTransaction as InvoicingBase;
				var wrapperAlter = new IReversingImplicitlyImplementedWrapperForBinding(revInvAlter);

				invFull.GenerateReverseTransaction(false);
				var revInvFull = invFull.ReverseTransaction as InvoicingBase;
				var wrapperFull = new IReversingImplicitlyImplementedWrapperForBinding(revInvFull);

				var multipleReversingProvider = new MultipleReversingProviderForHeader();
				multipleReversingProvider.BizObjectsForReversing.AddRange(invAlter, invFull);
				multipleReversingProvider.TransactionsAlreadyReversed.AddRange(wrapperAlter, wrapperFull);

				Factory.Save();

				var controller = ZControllerFactory.Create(ControllerIDs.ARInvoice);
				controller.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });

				var registry = AccountingMasterFilesRegistry.Instance;
				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
				using (registry.ComplianceNumberAllocationDate_AR.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (registry.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (var form = (MultipleReversingForHeaderForm)controller.LastShownForm)
				{
					revInvAlter.AH_ComplianceSubType = subType;
					SetInvoiceDate(revInvAlter, dateOption, alterSeq.XD_ExpiryDate.Date.AddDays(1), dateOutsideSequencePeriod);

					revInvFull.AH_ComplianceSubType = subType;
					SetInvoiceDate(revInvFull, dateOption, fullSeq.XD_StartDate, dateOutsideSequencePeriod);

					form.ShowPreDeleteDialogs_ForTestOnly();

					Assert(multipleReversingProvider.HasErrors);
					var notifications = multipleReversingProvider.NotificationsIncludingChildren.ToArray();
					AssertContains(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage, notifications[0].Message);
					AssertContains(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage, notifications[1].Message);
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
		}

		[ExpectNoExceptions()]
		public void TestReverseMultipleARCreditNoteIsSuccessfulWithClosedJob()
		{
			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			var invoice1 = Factory.NewWithValidTestData<ARCreditNote>();
			invoice1.SecurityOverrideProvider = new InvoicingSecurityOverrideProvider(invoice1);
			invoice1.GenerateReverseTransaction(false);

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_Status = "CLS";
			job1.JH_JobNum = "TESTJOB1";

			((InvoicingBase)invoice1.ReverseTransaction).AddRelatedJobsForReversing_ForTestOnly(job1);

			var invoice2 = Factory.NewWithValidTestData<ARCreditNote>();
			invoice2.SecurityOverrideProvider = new InvoicingSecurityOverrideProvider(invoice2);
			invoice2.GenerateReverseTransaction(false);

			Env.Security.ReopenJob.IsAllowed = true;

			multipleReversingProvider.BizObjectsForReversing.Add(invoice1);
			multipleReversingProvider.BizObjectsForReversing.Add(invoice2);

			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice1.ReverseTransaction));
			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice2.ReverseTransaction));

			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.ARCreditNote);

			controller.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });

			using (var form = (MultipleReversingForHeaderForm)controller.LastShownForm)
			{
				form.ShowPreDeleteDialogs_ForTestOnly();

				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				string expectedMessage = @"Closed Job(s) :TESTJOB1
You are about to reopen these closed jobs. Do you want to proceed?";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				AssertEquals("Job status is closed", "CLS", job1.JH_Status);
				form.ReOpenClosedJob_ForTestOnly();
				AssertEquals("Job status is working", "WRK", job1.JH_Status);
			}
		}

		public void TestReverseMultipleARInvoice_OneInvoiceIsMatch_ShowReadonlyFormAfterReverseFailed()
		{
			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			var invoice1 = newFactory.NewWithValidTestData<ARInvoice>();
			invoice1.SecurityOverrideProvider = new InvoicingSecurityOverrideProvider(invoice1);
			invoice1.GenerateReverseTransaction(false);

			var invoice2 = newFactory.NewWithValidTestData<ARInvoice>();
			invoice2.SecurityOverrideProvider = new InvoicingSecurityOverrideProvider(invoice2);
			invoice2.GenerateReverseTransaction(false);

			multipleReversingProvider.BizObjectsForReversing.Add(invoice1);
			multipleReversingProvider.BizObjectsForReversing.Add(invoice2);

			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice1.ReverseTransaction));
			multipleReversingProvider.TransactionsAlreadyReversed.Add(new IReversingImplicitlyImplementedWrapperForBinding(invoice2.ReverseTransaction));

			newFactory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.ARInvoice);

			controller.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });

			using (var form = (MultipleReversingForHeaderForm)controller.LastShownForm)
			{
				Assert("Reverse Form 1 should be editable.", form.DisplayMode == ODisplayMode.Delete);

				var newFactory2 = Factory.CreateNewFactory();
				newFactory2.RefreshEnabled = false;
				var loadInvoice1 = newFactory2.Load<ARInvoice>(invoice1.PK);
				loadInvoice1.AH_FullyPaidDate = ZDateTime.Now.AddHours(1);
				newFactory2.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoExceptionThrown(() =>
				{
					form.PostingButtons_ForTestOnly.SaveAndCloseButton.PerformClick();
					using (var form2 = (AccountingZForm)controller.LastShownForm)
					{
						form2.FReversingCode_ForTestOnly = "IDE";
						form2.FReversingReason_ForTestOnly = "test reason";
						form2.AcceptButton.PerformClick();
					}
				});

				var expectedWarning = @"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.
";

				AssertContains(expectedWarning, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Reverse Form should be read only.", form.DisplayMode == ODisplayMode.ReadOnly);
			}
		}

		public void TestSetupGridForAmendStatusCode()
		{
			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new MultipleReversingForHeaderForm(multipleReversingProvider))
			{
				form.Show();
				var grid = form.GetControl<ZGrid>("Grid");
				var amendStatusCodeColumn = grid.GetColumnStyle("AmendStatusCode");
				AssertEquals(false, amendStatusCodeColumn.IsUnavailable);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new MultipleReversingForHeaderForm(multipleReversingProvider))
			{
				form.Show();
				var grid = form.GetControl<ZGrid>("Grid");
				var amendStatusCodeColumn = grid.GetColumnStyle("AmendStatusCode");
				AssertEquals(true, amendStatusCodeColumn.IsUnavailable);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new MultipleReversingForHeaderForm(multipleReversingProvider))
			{
				form.Show();
				var grid = form.GetControl<ZGrid>("Grid");
				var amendStatusCodeColumn = grid.GetColumnStyle("AmendStatusCode");
				AssertEquals(true, amendStatusCodeColumn.IsUnavailable);
			}

			Application.DoEvents();
		}

		#region TestMakeRequiredFieldsEditableForReversingOnBizObject

		protected override void AssertNotReadonlyWhenNoRowErrors(string message, NonPersistentBusinessObject wrapper)
		{
			var wrapperForBinding = (IReversingImplicitlyImplementedWrapperForBinding)wrapper;
			Assert(message, !wrapperForBinding.TransactionDateInfo.ReadOnly);
			Assert(message, wrapperForBinding.PostDateInfo.ReadOnly);
		}

		#endregion

		#region Implementation

		protected override NonPersistentBusinessObject GetNewAlreadyReversedBusinessObjectWrappedForBinding()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.GenerateReverseTransaction(false);

			return new IReversingImplicitlyImplementedWrapperForBinding(invoice.ReverseTransaction);
		}

		protected override BusinessObject GetOriginalTransaction(BusinessObject reversedBizo) => ((TransactionHeader)reversedBizo).OriginalTransaction;

		protected override MultipleReversingProviderBase GetNewMultipleReversingProvider() => new MultipleReversingProviderForHeader();

		protected override MultipleReversingBaseForm GetNewForm(MultipleReversingProviderBase multipleReversingProvider) => new MultipleReversingForHeaderForm((MultipleReversingProviderForHeader)multipleReversingProvider);

		#endregion
	}
}
