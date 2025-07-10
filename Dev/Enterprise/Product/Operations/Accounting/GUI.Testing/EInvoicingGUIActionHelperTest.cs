using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.GUI.EInvoicing.PenaltyTaxMessage;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.Testing
{
	public class EInvoicingGUIActionHelperTest : TestCaseWithFactory
	{
		#region Romania Authrecord Reset Test

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_AuthRecordIsResetForRomania()
			=> AssertAuthRecordIsReset(CountryCodes.Romania, true);

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_AuthRecordIsResetForOtherCountry()
			=> AssertAuthRecordIsReset(CountryCodes.SaudiArabia, false);

		[TestDate(2006, 5, 10)]
		void AssertAuthRecordIsReset(string countryCode, bool authRecordRefreshed)
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var testDate = ZDate.Today;
				invoice.AH_GovernmentAllocatedID = "TEST01";
				invoice.AH_ComplianceDocumentDate = testDate;
				var authRecord = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(invoice);
				var authRecordPK = authRecord.PK;
				var batchPK = TestObjectCreator.CreateEInvoicingBatch();
				Factory.Save();

				var batch = Factory.Load<AccEInvoicingBatch>(batchPK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batchPK, ZDateTime.Empty, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batchPK, ZDateTime.Empty, ZDateTime.Today, ZBool.True);

				BusinessObject[] selectedBusinessObjects = { invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.ReceivablesResetTransactionStatus;
					securityCheckPoint.IsAllowed = true;
					guiHelper.ResetStatusToQueued(securityCheckPoint);

					AssertEquals("Transaction was successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);

					if (authRecordRefreshed)
					{
						AssertNull("Old Auth record should be deleted.", Factory.Load<AccTransactionHeaderAuthorisationRecord>(authRecordPK));
						AssertNotNull("New Auth record should be created.", Factory.Load<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, invoice.PK)));
						AssertEquals(ZString.Empty, invoice.AH_GovernmentAllocatedID);
						AssertEquals(ZDate.Empty, invoice.AH_ComplianceDocumentDate);
					}
					else
					{
						AssertNotNull("Old Auth record should not be deleted.", Factory.Load<AccTransactionHeaderAuthorisationRecord>(authRecordPK));
						AssertEquals("TEST01", invoice.AH_GovernmentAllocatedID);
						AssertEquals(testDate, invoice.AH_ComplianceDocumentDate);
					}
				}
			}
		}

		#endregion

		#region Romania Authrecord Reset Test

		[TestDate(2006, 5, 10)]
		public void TestRomaniaResetStatusToQueued_StatusProvider_DLV()
			=> AssertRomaniaResetStatusToQueued(EInvoicingPivotState.Delivered, true, true);

		[TestDate(2006, 5, 10)]
		public void TestRomaniaResetStatusToQueued_StatusProvider_SNT()
			=> AssertRomaniaResetStatusToQueued(EInvoicingPivotState.Sent, true, true);

		[TestDate(2006, 5, 10)]
		public void TestRomaniaResetStatusToQueued_StatusProvider_BER()
			=> AssertRomaniaResetStatusToQueued(EInvoicingPivotState.BatchedWithError, false, true);

		[TestDate(2006, 5, 10)]
		public void TestRomaniaResetStatusToQueued_StatusProvider_FAL()
			=> AssertRomaniaResetStatusToQueued(EInvoicingPivotState.Failed, false, true);

		[TestDate(2006, 5, 10)]
		public void TestRomaniaResetStatusToQueued_StatusProvider_DCD()
			=> AssertRomaniaResetStatusToQueued(EInvoicingPivotState.Discarded, true, false);

		[TestDate(2006, 5, 10)]
		public void TestRomaniaResetStatusToQueued_StatusProvider_SecurityConstraint_DLV()
			=> AssertRomaniaResetStatusToQueued(EInvoicingPivotState.Delivered, false, false);

		[TestDate(2006, 5, 10)]
		public void TestRomaniaResetStatusToQueued_StatusProvider_SecurityConstraint_SNT()
			=> AssertRomaniaResetStatusToQueued(EInvoicingPivotState.Sent, false, false);

		void AssertRomaniaResetStatusToQueued(ZString piviotStatus, bool hasSecurityRight, bool canRequeue)
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var authRecord = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(invoice);
				var authRecordPK = authRecord.PK;
				var batchPK = TestObjectCreator.CreateEInvoicingBatch();
				Factory.Save();

				var batch = Factory.Load<AccEInvoicingBatch>(batchPK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice.PK, piviotStatus, "This transaction was sent to the IIS site and waiting response.", batchPK, ZDateTime.Empty, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(invoice.PK, piviotStatus, "This transaction was sent to the IIS site and waiting response.", batchPK, ZDateTime.Empty, ZDateTime.Today, ZBool.True);

				BusinessObject[] selectedBusinessObjects = { invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.ReceivablesResetTransactionStatus;
					securityCheckPoint.IsAllowed = hasSecurityRight;
					guiHelper.ResetStatusToQueued(securityCheckPoint);

					if (canRequeue)
					{
						AssertEquals("Transaction was successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, "", ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					}
					else
					{
						AssertEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:
- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights), or
- 'DLV' - Delivered (when you have appropriate security rights).


No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		#endregion

		#region Vietnam Request e-Invoice Copy

		public void TestRequestEInvoiceCopyMenuItemForAmendingCRDInVietnam()
		{
			AssertRequestEInvoiceCopyMenuItemForVietnam(EInvoicingPivotActionType.Approve);
		}

		public void TestRequestEInvoiceCopyMenuItemForCancelledCRDInVietnam()
		{
			AssertRequestEInvoiceCopyMenuItemForVietnam(EInvoicingPivotActionType.Cancel);
		}

		void AssertRequestEInvoiceCopyMenuItemForVietnam(string previousPivotActionType)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "4", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				invoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.EXI;
				Factory.Save();

				TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, status: EInvoicingPivotState.Succeed);
				Factory.Save();

				ARCreditNote aRCreditNote;

				if (previousPivotActionType == EInvoicingPivotActionType.Cancel)
				{
					aRCreditNote = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice);
				}
				else
				{
					aRCreditNote = (ARCreditNote)TestObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, invoice).amendTransaction;
				}
				aRCreditNote.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.EXI;
				aRCreditNote.AH_TransactionReference = "Test123";

				Factory.Save();

				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var eInvoiceCopyMenuName = "Request e-Invoice Copy";
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText(eInvoiceCopyMenuName);
				AssertNotNull(eInvoiceCopyMenuName + " button should be in Action menu when logged in with a Vietnam company and Vietnam Compliance Feature is active", requestMenuItem);

				requestMenuItem.PerformClick();
				AssertEquals(@"Your request for a copy of the tax invoice is being processed.
Please Note:
- A request for a copy of the tax invoice can only be made for new INV, INV amendment with CRD, canceled INV.
- A new request for a copy of the tax invoice is allowed where response from an existing request has not been received after 60 minutes from request submission.", UnitTestUserNotification.Instance.LastMessage.Text);

				var invoicingBatch1 = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				Factory.Save();
				var invoicePivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch1, aRCreditNote, EInvoicingPivotState.Queued, previousPivotActionType);
				Factory.Save();

				selectedBusinessObjects.Add(aRCreditNote);
				requestMenuItem.PerformClick();
				AssertEquals($@"Transaction Number {aRCreditNote.AH_TransactionNum}: The transaction is not eligible for requests due to one of the following conditions:
1. The E-Reporting Status of the transaction is not equal to SUC.
2. The transaction has not been approved.", UnitTestUserNotification.Instance.LastMessage.Text);

				invoicePivot1.AIP_Status = EInvoicingPivotState.Succeed;
				Factory.Save();

				Assert("Is valid submit pivot", previousPivotActionType == EInvoicingPivotActionType.Cancel ? invoicePivot1.IsCancelPivotSucceed : invoicePivot1.IsApprovePivotSucceed);

				requestMenuItem.PerformClick();
				AssertEquals($"Transaction Number {aRCreditNote.AH_TransactionNum}: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				requestMenuItem.PerformClick();
				AssertEquals($"Transaction Number {aRCreditNote.AH_TransactionNum}: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var queuedPivot = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, aRCreditNote.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.FirstOrDefault();
				AssertNotNull(queuedPivot);
			}
		}

		public void TestRequestEInvoicePDFCopyInVietnam_IfExistingPDFCopyRequestIsTooOld()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.VND, 1m, TestObjectCreator.AALSHI);
				arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice.AH_TransactionReference = "123";
				Factory.Save();
				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertNotNull(pivot);
				AssertEquals(EInvoicingPivotActionType.Submit, pivot.AIP_ActionType);
				Factory.Save();

				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var eInvoiceCopyMenuName = "Request e-Invoice Copy";
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText(eInvoiceCopyMenuName);

				requestMenuItem.PerformClick();

				var invoicingBatch1 = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var invoicePivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch1, arInvoice, EInvoicingPivotState.Queued, EInvoicingPivotActionType.Submit);
				Factory.Save();

				selectedBusinessObjects.Add(arInvoice);
				requestMenuItem.PerformClick();

				invoicePivot1.AIP_Status = EInvoicingPivotState.Succeed;
				Factory.Save();

				requestMenuItem.PerformClick();

				var queuedPivot = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.FirstOrDefault();
				AssertNotNull(queuedPivot);

				queuedPivot.AIP_Status = EInvoicingPivotState.Sent;
				queuedPivot.AIP_LastSentTimeUtc = ZDateTime.UtcNow;
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals($"Transaction Number {arInvoice.AH_TransactionNum}: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				queuedPivot.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-70);
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals($"Transaction Number {arInvoice.AH_TransactionNum}: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Request e-Invoice Status

		public void TestMalaysiaRequestEInvoiceStatus_CheckCanExistSucceedPivot()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.Malaysia, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var (arInvoice, pivotStatusCheck) = PrepareDataForRequestEInvoiceStatus();
				pivotStatusCheck.AIP_Status = EInvoicingPivotState.Succeed;
				pivotStatusCheck.AIP_LastResponseReceivedUtc = ZDateTime.UtcNow;
				Factory.Save();

				var selectedBusinessObjects = new List<BusinessObject>();
				selectedBusinessObjects.Add(arInvoice);

				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var eInvoiceCopyMenuName = "Request e-Invoice Status";
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText(eInvoiceCopyMenuName);

				requestMenuItem.PerformClick();

				var statusCheckPivots = Factory.Load<AccEInvoicingTransactionPivot>(
					new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK)
						.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck));
				AssertEquals(1, statusCheckPivots.Count(x => x.AIP_Status == EInvoicingPivotState.Queued));
				AssertEquals(1, statusCheckPivots.Count(x => x.AIP_Status == EInvoicingPivotState.Discarded));
			}
		}

		public void TestMalaysiaRequestEInvoiceStatus_CheckExistActivePivot()
		{
			AssertRequestEInvoiceStatus(CountryCodes.Malaysia, 1, 1);
		}

		public void TestKoreaRequestEInvoiceStatus_CheckExistActivePivot()
		{
			AssertRequestEInvoiceStatus(CountryCodes.KoreaSouth, 1, 0);
		}

		public void AssertRequestEInvoiceStatus(string countryCode, int queuePivotCount, int discardPivotCount)
		{
			using (TestObjectCreator.SetUpForTestingEInvoicing(countryCode, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var (arInvoice, pivotStatusCheck) = PrepareDataForRequestEInvoiceStatus();
				pivotStatusCheck.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-40);
				Factory.Save();

				var selectedBusinessObjects = new List<BusinessObject>();
				selectedBusinessObjects.Add(arInvoice);

				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var eInvoiceCopyMenuName = "Request e-Invoice Status";
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText(eInvoiceCopyMenuName);

				requestMenuItem.PerformClick();

				var statusCheckPivots = Factory.Load<AccEInvoicingTransactionPivot>(
					new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK)
						.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck));
				AssertEquals(queuePivotCount, statusCheckPivots.Count(x => x.AIP_Status == EInvoicingPivotState.Queued));
				AssertEquals(discardPivotCount, statusCheckPivots.Count(x => x.AIP_Status == EInvoicingPivotState.Discarded));
			}
		}

		public void TestMalaysiaRequestEInvoiceStatus_CheckExistActivePivot_WhenStatusIsBER()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.Malaysia, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var (arInvoice, pivotStatusCheck) = PrepareDataForRequestEInvoiceStatus();
				pivotStatusCheck.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-40);
				pivotStatusCheck.AIP_Status = EInvoicingPivotState.BatchedWithError;
				Factory.Save();

				var selectedBusinessObjects = new List<BusinessObject>();
				selectedBusinessObjects.Add(arInvoice);

				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var eInvoiceCopyMenuName = "Request e-Invoice Status";
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText(eInvoiceCopyMenuName);

				requestMenuItem.PerformClick();

				var statusCheckPivots = Factory.Load<AccEInvoicingTransactionPivot>(
						new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK)
								.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck));
				AssertEquals(1, statusCheckPivots.Count(x => x.AIP_Status == EInvoicingPivotState.Queued));
				AssertEquals(1, statusCheckPivots.Count(x => x.AIP_Status == EInvoicingPivotState.Discarded));
			}
		}

		(ARInvoice, AccEInvoicingTransactionPivot) PrepareDataForRequestEInvoiceStatus()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "01");
			TestObjectCreator.CreateInvoiceLine(arInvoice, 110m, arInvoice.TransactionCurrency, 1m);
			Factory.Save();

			var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
			AssertEquals(EInvoicingPivotActionType.Submit, pivot.AIP_ActionType);

			pivot.AIP_Status = EInvoicingPivotState.Delivered;

			var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Queued);
			var batchQuery = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotQuery, 1, EInvoicingBatchState.Sent);

			Factory.Save();

			return (arInvoice, pivotQuery);
		}

		public void TestRequestEInvoiceStatus_DiscardExistDocumentDetailPivot()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.Malaysia, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var (arInvoice, pivotStatusCheck) = PrepareDataForRequestEInvoiceStatus();
				pivotStatusCheck.AIP_Status = EInvoicingPivotState.Succeed;
				pivotStatusCheck.AIP_LastResponseReceivedUtc = ZDateTime.UtcNow;

				arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.DocumentDetail, EInvoicingPivotState.Queued);

				Factory.Save();

				var selectedBusinessObjects = new List<BusinessObject>();
				selectedBusinessObjects.Add(arInvoice);

				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var eInvoiceCopyMenuName = "Request e-Invoice Status";
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText(eInvoiceCopyMenuName);

				requestMenuItem.PerformClick();

				var documentDetailPivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(
						new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK)
								.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentDetail));
				AssertEquals(EInvoicingPivotState.Discarded, documentDetailPivot.AIP_Status);
			}
		}

		#endregion

		#region Turkey E-Invoicing Requests Menu Items Tests

		public void TestEInvoiceRequestsMenuItemsIsNotVisible()
		{
			AssertMenuItemIsNotVisible(TurkeyBranch.PK.ToGuid(), null, pdfRequestMenuItemDesc, " menu item should not be present when Turkey Compliance Features TestTransactionModule is disabled.");

			AssertMenuItemIsNotVisible(AfghanistanBranch.PK.ToGuid(), null, pdfRequestMenuItemDesc, " menu item should not be present in Afghanistan (because there are NO sub-types configured for it)");
			AssertMenuItemIsNotVisible(AfghanistanBranch.PK.ToGuid(), DateTime.Today.AddDays(-1), pdfRequestMenuItemDesc, " menu item should not be present in Afghanistan (because there are NO sub-types configured for it)");

			AssertMenuItemIsNotVisible(TurkeyBranch.PK.ToGuid(), null, statusRequestMenuItemDesc, " menu item should not be present when Turkey Compliance Features TestTransactionModule is disabled.");
			AssertMenuItemIsNotVisible(AfghanistanBranch.PK.ToGuid(), null, statusRequestMenuItemDesc, " menu item should not be present in Afghanistan (because there are NO sub-types configured for it)");
			AssertMenuItemIsNotVisible(AfghanistanBranch.PK.ToGuid(), DateTime.Today.AddDays(-1), statusRequestMenuItemDesc, " menu item should not be present in Afghanistan (because there are NO sub-types configured for it)");
		}

		void AssertMenuItemIsNotVisible(Guid branchPK, DateTime? date, string menuItemDesc, string additionalMessage)
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(branchPK, date))
			{
				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText(menuItemDesc);
				AssertNull(menuItemDesc + additionalMessage, requestMenuItem);
			}
		}

		#endregion

		#region PDF Request

		public void TestRequestEInvoicePDFCopyMenuItemForTurkeyCompany_ForDeliveredStatusAR()
		{
			AssertRequestEInvoicePDFCopyMenuItemForTurkeyCompanyAR(EInvoicingPivotState.Delivered);
		}

		public void TestRequestEInvoicePDFCopyMenuItemForTurkeyCompany_ForSucceedStatusAR()
		{
			AssertRequestEInvoicePDFCopyMenuItemForTurkeyCompanyAR(EInvoicingPivotState.Succeed);
		}

		public void TestRequestEInvoicePDFCopyMenuItemForTurkeyCompany_ForDeliveredStatusAP()
		{
			AssertRequestEInvoicePDFCopyMenuItemForTurkeyCompanyAP(EInvoicingPivotState.Delivered);
		}

		public void TestRequestEInvoicePDFCopyMenuItemForTurkeyCompany_ForSucceedStatusAP()
		{
			AssertRequestEInvoicePDFCopyMenuItemForTurkeyCompanyAP(EInvoicingPivotState.Succeed);
		}

		public void TestRequestEInvoicePDFCopyMenuItemForCancelledAR()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(TurkeyBranch.PK.ToGuid(), DateTime.Today.AddDays(-1)))
			{
				var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "4", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				invoice.AH_OH = TestObjectCreator.DebtorTR.PK;
				var orgProxy = invoice.Company.OrgProxy;
				orgProxy.CustomsCodes.AddNew("VAT", "34567890123");
				invoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
				invoice.AH_Desc = "set Transaction to cancelled";

				Factory.Save();

				var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				invoicingBatch.AIB_GovernmentAllocatedNumber = "55985165-DAC0-423A-883B-5F1039C0D585";

				Factory.Save();

				var invoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, invoice, EInvoicingPivotState.Succeed, EInvoicingPivotActionType.Submit);

				Factory.Save();

				InvoicingBaseReversing reverser = new ARInvoiceReversing((ARInvoice)invoice);
				reverser.Reverse();

				Factory.Save();

				var reversedInvoice = invoice.ReverseTransaction;
				var reversedInvoiceBizO = Factory.Load<ARCreditNote>(reversedInvoice.PK);

				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText(pdfRequestMenuItemDesc);
				AssertNotNull(pdfRequestMenuItemDesc + " button should be in Action menu when logged in with a Turkey company and Turkey Compliance Feature is active", requestMenuItem);

				selectedBusinessObjects.Add(invoice);
				requestMenuItem.PerformClick();

				var expectedMessage = @"Your request for a PDF copy of the tax invoice is being processed.
Please Note:
- No request for a copy of the tax invoice will be made for electronic invoices under the following conditions:
    When transaction does not have a Compliance Sub Type of EIN, EIC, or EAR attached.
    When transaction has been reversed.
    When transaction does not have an ETTN attached.
    When transaction has an existing request for a copy where it is still being processed.";

				var queuedPivot = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.FirstOrDefault();

				AssertNull(queuedPivot);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				selectedBusinessObjects.Remove(invoice);
				selectedBusinessObjects.Add(reversedInvoiceBizO);
				requestMenuItem.PerformClick();

				queuedPivot = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, reversedInvoiceBizO.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.FirstOrDefault();

				AssertNull(queuedPivot);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRequestEInvoicePDFCopy_IfExistingPDFCopyRequestIsTooOld()
		{
			using (AccountingMasterFilesRegistry.Instance.TaxInvoiceStatusUpdateAutomatedRequestSchedule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 60))
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(TurkeyBranch.PK.ToGuid(), DateTime.Today.AddDays(-1)))
			{
				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "4", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				arInvoice.AH_OH = TestObjectCreator.DebtorTR.PK;
				arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
				arInvoice.AH_TransactionReference = "ABC1";
				var orgProxy = arInvoice.Company.OrgProxy;
				orgProxy.CustomsCodes.AddNew("VAT", "34567890123");
				Factory.Save();

				var submitBatch = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany, "55985165-DAC0-423A-883B-5F1039C0D585");
				var submitPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(submitBatch, arInvoice, EInvoicingPivotState.Succeed, EInvoicingPivotActionType.Submit);
				Factory.Save();

				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText(pdfRequestMenuItemDesc);
				AssertNotNull(pdfRequestMenuItemDesc + " button should be in Action menu when logged in with a Turkey company and Turkey Compliance Feature is active", requestMenuItem);

				selectedBusinessObjects.Add(arInvoice);

				// First PDF Request
				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var docPivot = getActiveDocPivot(Factory, arInvoice.PK);
				AssertNotNull(docPivot);
				AssertEquals(EInvoicingPivotState.Queued, docPivot.AIP_Status);

				// PDF Request before service task run
				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var docBatch = TestObjectCreator.CreateEInvoicingBatch(101, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				docPivot.AIP_AIB = docBatch.PK;
				docPivot.AIP_Status = EInvoicingPivotState.Sent;
				docPivot.AIP_LastSentTimeUtc = ZDateTime.UtcNow;
				Factory.Save();

				// PDF Request when it is on SNT status.
				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				// Converting to an old request
				docPivot.AIP_LastSentTimeUtc = docPivot.AIP_LastSentTimeUtc.AddMinutes(-70);
				Factory.Save();

				// PDF request when it is too old
				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var newFactory = new BusinessObjectFactory();

				var docDiscardedPivot = newFactory.Load<AccEInvoicingTransactionPivot>(docPivot.PK);
				AssertNotNull(docDiscardedPivot);
				AssertEquals(EInvoicingPivotState.Discarded, docDiscardedPivot.AIP_Status);

				var docDiscardedBatch = newFactory.Load<AccEInvoicingBatch>(docBatch.PK);
				AssertNotNull(docDiscardedBatch);
				AssertEquals(EInvoicingBatchState.Discarded, docDiscardedBatch.AIB_Status);

				var docNewPivot = getActiveDocPivot(newFactory, arInvoice.PK);
				AssertNotNull(docNewPivot);
				AssertEquals(EInvoicingPivotState.Queued, docNewPivot.AIP_Status);

				AccEInvoicingTransactionPivot getActiveDocPivot(BusinessObjectFactory factory, ZGuid pk)
					=> factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, pk)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded))
					.FirstOrDefault();
			}
		}

		public void TestRequestEInvoicePDFCopy_ShouldIgnoreLocalCache()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(TurkeyBranch.PK.ToGuid(), DateTime.Today.AddDays(-1)))
			{
				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var pdfRequestMenuItem = guiHelper.GetActionMenuItems().FindByText(pdfRequestMenuItemDesc);
				var statusRequestMenuItem = guiHelper.GetActionMenuItems().FindByText(statusRequestMenuItemDesc);
				AssertNotNull(pdfRequestMenuItem);
				AssertNotNull(statusRequestMenuItem);

				TestObjectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN);

				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				arInvoice.AH_OH = TestObjectCreator.DebtorTR.PK;
				var orgProxy = arInvoice.Company.OrgProxy;
				orgProxy.CustomsCodes.AddNew("VAT", "34567890123");
				arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
				Factory.Save();

				var invoicingSubmitBatch = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				invoicingSubmitBatch.AIB_GovernmentAllocatedNumber = "5268D74F-1905-4CFC-BBE8-C15BE278CA4D";
				Factory.Save();
				var invoiceSubmitPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingSubmitBatch, arInvoice, EInvoicingPivotState.Delivered, EInvoicingPivotActionType.Submit);
				Factory.Save();
				var invoicingDocBatch = TestObjectCreator.CreateEInvoicingBatch(101, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				invoicingDocBatch.AIB_GovernmentAllocatedNumber = "0C23677B-B0ED-486F-B90B-D607C0EBB609";
				Factory.Save();
				var invoiceDocPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingDocBatch, arInvoice, EInvoicingPivotState.Sent, EInvoicingPivotActionType.DocumentAction);
				Factory.Save();
				AssertEquals("Pre-condition: Submit pivot should be delivered to make AR Invoice eligible for PDF batching", EInvoicingPivotState.Delivered, invoiceSubmitPivot.AIP_Status);
				AssertEquals("Pre-condition: Simulate a PDF request", EInvoicingPivotState.Sent, invoiceDocPivot.AIP_Status);

				selectedBusinessObjects.Add(arInvoice);
				statusRequestMenuItem.PerformClick();

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoiceDocPivotInNewFactory = newFactory.Load<AccEInvoicingTransactionPivot>(invoiceDocPivot.PK);
				invoiceDocPivotInNewFactory.AIP_Status = EInvoicingPivotState.Succeed;
				newFactory.Save();
				AssertEquals("Pre-condition: This is to simulate UMI service task processing sucessfully-get-PDF XUE.", EInvoicingPivotState.Succeed, invoiceDocPivotInNewFactory.AIP_Status);
				AssertEquals("Pre-condition: Local cache of PDF request", EInvoicingPivotState.Sent, invoiceDocPivot.AIP_Status);

				AssertNoExceptionThrown("Should load latest AIP_Status from DB", () => pdfRequestMenuItem.PerformClick());
				AssertEquals("Transaction Number 00001000: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertRequestEInvoicePDFCopyMenuItemForTurkeyCompanyAR(string submitPivotStatus)
		{
			var arInvoice0 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "4", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			arInvoice0.AH_OH = TestObjectCreator.DebtorTR.PK;
			arInvoice0.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(TurkeyBranch.PK.ToGuid(), DateTime.Today.AddDays(-1)))
			{
				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText(pdfRequestMenuItemDesc);
				AssertNotNull(pdfRequestMenuItemDesc + " button should be in Action menu when logged in with a Turkey company and Turkey Compliance Feature is active", requestMenuItem);

				// Assert for when is no selected transaction
				requestMenuItem.PerformClick();
				AssertEquals(@"Your request for a PDF copy of the tax invoice is being processed.
Please Note:
- No request for a copy of the tax invoice will be made for electronic invoices under the following conditions:
    When transaction does not have a Compliance Sub Type of EIN, EIC, or EAR attached.
    When transaction has been reversed.
    When transaction does not have an ETTN attached.
    When transaction has an existing request for a copy where it is still being processed.", UnitTestUserNotification.Instance.LastMessage.Text);

				selectedBusinessObjects.Add(arInvoice0);
				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: Transaction is not eligible for Electronic Invoicing.", UnitTestUserNotification.Instance.LastMessage.Text);

				selectedBusinessObjects.RemoveAt(0);

				TestObjectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN);

				var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				arInvoice1.AH_OH = TestObjectCreator.DebtorTR.PK;
				var orgProxy = arInvoice1.Company.OrgProxy;
				orgProxy.CustomsCodes.AddNew("VAT", "34567890123");
				arInvoice1.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
				Factory.Save();

				var invoicingBatch1 = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				Factory.Save();
				var invoicePivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch1, arInvoice1, submitPivotStatus, EInvoicingPivotActionType.Submit);
				Factory.Save();

				// Assert for one selected transaction and when is not eligible
				selectedBusinessObjects.Add(arInvoice1);
				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The transaction is not eligible for requests, or missing an ETTN.", UnitTestUserNotification.Instance.LastMessage.Text);

				invoicingBatch1.AIB_GovernmentAllocatedNumber = "55985165-DAC0-423A-883B-5F1039C0D585";
				Factory.Save();

				AssertEquals("Is valid submit pivot", true, invoicePivot1.IsSubmitPivotSucceedOrDelivered);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "2", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				arInvoice2.AH_OH = TestObjectCreator.DebtorTR.PK;
				arInvoice2.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;

				var arInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "3", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				arInvoice3.AH_OH = TestObjectCreator.DebtorTR.PK;
				arInvoice3.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
				Factory.Save();

				selectedBusinessObjects.Add(arInvoice2);
				selectedBusinessObjects.Add(arInvoice3);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request has already been sent.\r\nTransaction Number 00001001: The transaction is not eligible for requests, or missing an ETTN.", UnitTestUserNotification.Instance.LastMessage.Text);

				var arInvoice2Batch1 = TestObjectCreator.CreateEInvoicingBatch(101, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				Factory.Save();
				TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice2Batch1, arInvoice2, EInvoicingPivotState.Succeed, EInvoicingPivotActionType.Submit);
				arInvoice2Batch1.AIB_GovernmentAllocatedNumber = "F3A5099B-A694-4D87-9F61-1D2FE805FE6A";
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request has already been sent.\r\nTransaction Number 00001001: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var queuedPivot = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice1.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.FirstOrDefault();
				AssertNotNull(queuedPivot);

				var arInvoice1Batch2 = TestObjectCreator.CreateEInvoicingBatch(102, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				queuedPivot.AIP_Status = EInvoicingPivotState.BatchedWithError;
				queuedPivot.AIP_AIB = arInvoice1Batch2.PK;
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals(1, Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery()
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Discarded))
					.ToList().Count);
				AssertEquals("Transaction Number 00001000: The request is sent.\r\nTransaction Number 00001001: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request has already been sent.\r\nTransaction Number 00001001: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery()
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.ToList().Count);
			}
		}

		void AssertRequestEInvoicePDFCopyMenuItemForTurkeyCompanyAP(string submitPivotStatus)
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Payables(TurkeyBranch.PK.ToGuid(), DateTime.Today.AddDays(-1)))
			{
				var apCreditNote0 = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "00001000", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				apCreditNote0.AH_OH = TestObjectCreator.DebtorTR.PK;
				apCreditNote0.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN;
				apCreditNote0.AH_TransactionReference = "apCreditNote0";
				Factory.Save();

				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var requestMenuItem = guiHelper.GetActionMenuItems(isAPTransaction: true).FindByText(pdfRequestMenuItemDesc);
				AssertNotNull(pdfRequestMenuItemDesc + " button should be in Action menu when logged in with a Turkey company and Turkey Compliance Feature is active", requestMenuItem);

				// Assert for when is no selected transaction
				requestMenuItem.PerformClick();
				AssertEquals(@"Your request for a PDF copy of the tax invoice is being processed.
Please Note:
- No request for a copy of the tax invoice will be made for electronic invoices under the following conditions:
    When transaction is not Payable Credit Note
    When transaction does not have a Compliance Sub Type of DIN or DAR attached.
    When transaction has been reversed.
    When transaction does not have an ETTN attached.
    When transaction has an existing request for a copy where it is still being processed.", UnitTestUserNotification.Instance.LastMessage.Text);

				selectedBusinessObjects.Add(apCreditNote0);
				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: Transaction is not eligible for Electronic Invoicing.", UnitTestUserNotification.Instance.LastMessage.Text);

				selectedBusinessObjects.RemoveAt(0);

				var apCreditNote1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "00001001", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				apCreditNote1.AH_OH = TestObjectCreator.DebtorTR.PK;
				var orgProxy = apCreditNote1.Company.OrgProxy;
				orgProxy.CustomsCodes.AddNew("VAT", "34567890123");
				apCreditNote1.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN;
				apCreditNote1.AH_TransactionReference = "apCreditNote1";
				Factory.Save();

				var apCreditNote1Batch1 = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				Factory.Save();
				var apCreditNote1Pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(apCreditNote1Batch1, apCreditNote1, submitPivotStatus, EInvoicingPivotActionType.Submit);
				Factory.Save();

				// Assert for one selected transaction and when is not eligible
				selectedBusinessObjects.Add(apCreditNote1);
				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001001: The transaction is not eligible for requests, or missing an ETTN.", UnitTestUserNotification.Instance.LastMessage.Text);

				apCreditNote1Batch1.AIB_GovernmentAllocatedNumber = "55985165-DAC0-423A-883B-5F1039C0D585";
				Factory.Save();

				AssertEquals("Is valid submit pivot", true, apCreditNote1Pivot1.IsSubmitPivotSucceedOrDelivered);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001001: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001001: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var apCreditNote2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "00001002", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				apCreditNote2.AH_OH = TestObjectCreator.DebtorTR.PK;
				apCreditNote2.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN;
				apCreditNote2.AH_TransactionReference = "apCreditNote2";

				var apInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001003", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				apInvoice3.AH_OH = TestObjectCreator.DebtorTR.PK;
				apInvoice3.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN;
				Factory.Save();

				selectedBusinessObjects.Add(apCreditNote2);
				selectedBusinessObjects.Add(apInvoice3);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001001: The request has already been sent.\r\nTransaction Number 00001002: The transaction is not eligible for requests, or missing an ETTN.", UnitTestUserNotification.Instance.LastMessage.Text);

				var apCreditNote2Batch1 = TestObjectCreator.CreateEInvoicingBatch(101, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				Factory.Save();
				TestObjectCreator.CreateEInvoicingTransactionPivot(apCreditNote2Batch1, apCreditNote2, EInvoicingPivotState.Succeed, EInvoicingPivotActionType.Submit);
				apCreditNote2Batch1.AIB_GovernmentAllocatedNumber = "F3A5099B-A694-4D87-9F61-1D2FE805FE6A";
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001001: The request has already been sent.\r\nTransaction Number 00001002: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var queuedPivot = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, apCreditNote1.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.FirstOrDefault();
				AssertNotNull(queuedPivot);

				var arInvoice1Batch2 = TestObjectCreator.CreateEInvoicingBatch(102, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				queuedPivot.AIP_Status = EInvoicingPivotState.BatchedWithError;
				queuedPivot.AIP_AIB = arInvoice1Batch2.PK;
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals(1, Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery()
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Discarded))
					.ToList().Count);
				AssertEquals("Transaction Number 00001001: The request is sent.\r\nTransaction Number 00001002: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001001: The request has already been sent.\r\nTransaction Number 00001002: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery()
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentAction)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.ToList().Count);
			}
		}

		#endregion

		#region Status Update

		public void TestRequestStatusUpdateMenuItemForTurkeyCompany_ForDeliveredStatusAR()
		{
			TestObjectCreator.CreateNewComplianceSequence(TurkeyBranch.GB_GC, TurkeyBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN);
			AssertRequestEInvoiceStatusUpdateMenuItemForTurkeyCompanyAR(EInvoicingPivotState.Delivered);
		}

		public void TestRequestStatusUpdateMenuItemForTurkeyCompany_ForSucceedStatusAR()
		{
			TestObjectCreator.CreateNewComplianceSequence(TurkeyBranch.GB_GC, TurkeyBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN);
			AssertRequestEInvoiceStatusUpdateMenuItemForTurkeyCompanyAR(EInvoicingPivotState.Succeed);
		}

		void AssertRequestEInvoiceStatusUpdateMenuItemForTurkeyCompanyAR(string submitPivotStatus)
		{
			var arInvoice0 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "4", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			arInvoice0.AH_OH = TestObjectCreator.DebtorTR.PK;
			arInvoice0.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(TurkeyBranch.PK.ToGuid(), DateTime.Today.AddDays(-1)))
			{
				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText(statusRequestMenuItemDesc);
				AssertNotNull(statusRequestMenuItemDesc + " button should be in Action menu when logged in with a Turkey company and Turkey Compliance Feature is active", requestMenuItem);

				// Assert for when is no selected transaction
				requestMenuItem.PerformClick();
				AssertEquals(@"Your request for an update to the status of the electronic invoice is now being processed.
Please Note:
- No status request will be made for electronic invoices under the following conditions:
    When transaction already has a status of Success.
    When transaction has been Canceled.
    When transaction does not have a compliance sub type of EIN, EIC or EAR attached.
    When transaction does not have an ETTN attached.
    When transaction's status has already been requested.", UnitTestUserNotification.Instance.LastMessage.Text);

				selectedBusinessObjects.Add(arInvoice0);
				requestMenuItem.PerformClick();
				AssertEquals(@"Transaction Number 00001000: Transaction is not eligible for Electronic Invoicing.", UnitTestUserNotification.Instance.LastMessage.Text);

				selectedBusinessObjects.RemoveAt(0);

				var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				arInvoice1.AH_OH = TestObjectCreator.DebtorTR.PK;
				var orgProxy = arInvoice1.Company.OrgProxy;
				orgProxy.CustomsCodes.AddNew("VAT", "34567890123");
				arInvoice1.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
				Factory.Save();

				var invoicingBatch1 = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				Factory.Save();

				var invoicePivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch1, arInvoice1, submitPivotStatus, EInvoicingPivotActionType.Submit);
				Factory.Save();

				// Assert for one selected transaction and when is not eligible
				selectedBusinessObjects.Add(arInvoice1);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The transaction is not eligible for requests, or missing an ETTN.", UnitTestUserNotification.Instance.LastMessage.Text);

				invoicingBatch1.AIB_GovernmentAllocatedNumber = "55985165-DAC0-423A-883B-5F1039C0D585";
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "2", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				arInvoice2.AH_OH = TestObjectCreator.DebtorTR.PK;
				arInvoice2.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;

				var arInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "3", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				arInvoice3.AH_OH = TestObjectCreator.DebtorTR.PK;
				arInvoice3.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
				Factory.Save();

				selectedBusinessObjects.Add(arInvoice2);
				selectedBusinessObjects.Add(arInvoice3);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request has already been sent.\r\nTransaction Number 00001001: The transaction is not eligible for requests, or missing an ETTN.", UnitTestUserNotification.Instance.LastMessage.Text);

				var arInvoice2Batch1 = TestObjectCreator.CreateEInvoicingBatch(101, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				Factory.Save();
				TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice2Batch1, arInvoice2, EInvoicingPivotState.Succeed, EInvoicingPivotActionType.Submit);
				arInvoice2Batch1.AIB_GovernmentAllocatedNumber = "F3A5099B-A694-4D87-9F61-1D2FE805FE6A";
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: The request has already been sent.\r\nTransaction Number 00001001: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var queuedPivot = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice1.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.FirstOrDefault();
				AssertNotNull(queuedPivot);

				var arInvoice1Batch2 = TestObjectCreator.CreateEInvoicingBatch(102, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				queuedPivot.AIP_Status = EInvoicingPivotState.BatchedWithError;
				queuedPivot.AIP_AIB = arInvoice1Batch2.PK;
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals(1, Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery()
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Discarded))
					.ToList().Count);
				AssertEquals("Transaction Number 00001000: The request is sent.\r\nTransaction Number 00001001: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				queuedPivot = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice1.PK)
	.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck)
	.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
	.FirstOrDefault();
				AssertNotNull(queuedPivot);

				var arInvoice1Batch3 = TestObjectCreator.CreateEInvoicingBatch(103, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				queuedPivot.AIP_Status = EInvoicingPivotState.Succeed;
				queuedPivot.AIP_AIB = arInvoice1Batch3.PK;
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001000: Transaction already succeeded, no further request will be made.\r\nTransaction Number 00001001: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery()
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.ToList().Count);
			}
		}

		public void TestRequestStatusUpdateMenuItemForTurkeyCompany_ForDeliveredStatusAP()
		{
			TestObjectCreator.CreateNewComplianceSequence(TurkeyBranch.GB_GC, TurkeyBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN);
			AssertRequestEInvoiceStatusUpdateMenuItemForTurkeyCompanyAP(EInvoicingPivotState.Delivered);
		}

		public void TestRequestStatusUpdateMenuItemForTurkeyCompany_ForSucceedStatusAP()
		{
			TestObjectCreator.CreateNewComplianceSequence(TurkeyBranch.GB_GC, TurkeyBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN);
			AssertRequestEInvoiceStatusUpdateMenuItemForTurkeyCompanyAP(EInvoicingPivotState.Succeed);
		}

		void AssertRequestEInvoiceStatusUpdateMenuItemForTurkeyCompanyAP(string submitPivotStatus)
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Payables(TurkeyBranch.PK.ToGuid(), DateTime.Today.AddDays(-1)))
			{
				var apCreditNote0 = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "00001000", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				apCreditNote0.AH_OH = TestObjectCreator.DebtorTR.PK;
				apCreditNote0.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN;
				apCreditNote0.AH_TransactionReference = "apCreditNote0";
				Factory.Save();

				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var requestMenuItem = guiHelper.GetActionMenuItems(isAPTransaction: true).FindByText(statusRequestMenuItemDesc);
				AssertNotNull(statusRequestMenuItemDesc + " button should be in Action menu when logged in with a Turkey company and Turkey Compliance Feature is active", requestMenuItem);

				// Assert for when is no selected transaction
				requestMenuItem.PerformClick();
				AssertEquals(@"Your request for an update to the status of the electronic invoice is now being processed.
Please Note:
- No status request will be made for electronic invoices under the following conditions:
    When transaction is not Payable Credit Note.
    When transaction already has a status of Success.
    When transaction has been Canceled.
    When transaction does not have a compliance sub type of DIN or DAR attached.
    When transaction does not have an ETTN attached.
    When transaction's status has already been requested.", UnitTestUserNotification.Instance.LastMessage.Text);

				selectedBusinessObjects.Add(apCreditNote0);
				requestMenuItem.PerformClick();
				AssertEquals(@"Transaction Number 00001000: Transaction is not eligible for Electronic Invoicing.", UnitTestUserNotification.Instance.LastMessage.Text);

				selectedBusinessObjects.RemoveAt(0);

				var apCreditNote1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "00001001", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				apCreditNote1.AH_OH = TestObjectCreator.DebtorTR.PK;
				var orgProxy = apCreditNote1.Company.OrgProxy;
				orgProxy.CustomsCodes.AddNew("VAT", "34567890123");
				apCreditNote1.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN;
				apCreditNote1.AH_TransactionReference = "apCreditNote1";
				Factory.Save();

				var apCreditNote1Batch1 = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				Factory.Save();

				var apCreditNote1Pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(apCreditNote1Batch1, apCreditNote1, submitPivotStatus, EInvoicingPivotActionType.Submit);
				Factory.Save();

				// Assert for one selected transaction and when is not eligible
				selectedBusinessObjects.Add(apCreditNote1);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001001: The transaction is not eligible for requests, or missing an ETTN.", UnitTestUserNotification.Instance.LastMessage.Text);

				apCreditNote1Batch1.AIB_GovernmentAllocatedNumber = "55985165-DAC0-423A-883B-5F1039C0D585";
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001001: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001001: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var apCreditNote2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "00001002", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				apCreditNote2.AH_OH = TestObjectCreator.DebtorTR.PK;
				apCreditNote2.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN;
				apCreditNote2.AH_TransactionReference = "apCreditNote2";

				var apInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001003", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
				apInvoice3.AH_OH = TestObjectCreator.DebtorTR.PK;
				apInvoice3.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN;
				Factory.Save();

				selectedBusinessObjects.Add(apCreditNote2);
				selectedBusinessObjects.Add(apInvoice3);

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001001: The request has already been sent.\r\nTransaction Number 00001002: The transaction is not eligible for requests, or missing an ETTN.", UnitTestUserNotification.Instance.LastMessage.Text);

				var apCreditNote2Batch1 = TestObjectCreator.CreateEInvoicingBatch(101, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				Factory.Save();
				TestObjectCreator.CreateEInvoicingTransactionPivot(apCreditNote2Batch1, apCreditNote2, EInvoicingPivotState.Succeed, EInvoicingPivotActionType.Submit);
				apCreditNote2Batch1.AIB_GovernmentAllocatedNumber = "F3A5099B-A694-4D87-9F61-1D2FE805FE6A";
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001001: The request has already been sent.\r\nTransaction Number 00001002: The request is sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				var queuedPivot = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, apCreditNote1.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.FirstOrDefault();
				AssertNotNull(queuedPivot);

				var apCreditNote1Batch2 = TestObjectCreator.CreateEInvoicingBatch(102, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				queuedPivot.AIP_Status = EInvoicingPivotState.BatchedWithError;
				queuedPivot.AIP_AIB = apCreditNote1Batch2.PK;
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals(1, Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery()
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Discarded))
					.ToList().Count);
				AssertEquals("Transaction Number 00001001: The request is sent.\r\nTransaction Number 00001002: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				queuedPivot = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, apCreditNote1.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.FirstOrDefault();
				AssertNotNull(queuedPivot);

				var arInvoice1Batch3 = TestObjectCreator.CreateEInvoicingBatch(103, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				queuedPivot.AIP_Status = EInvoicingPivotState.Succeed;
				queuedPivot.AIP_AIB = arInvoice1Batch3.PK;
				Factory.Save();

				requestMenuItem.PerformClick();
				AssertEquals("Transaction Number 00001001: Transaction already succeeded, no further request will be made.\r\nTransaction Number 00001002: The request has already been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery()
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued))
					.ToList().Count);
			}
		}

		[TestDate(2021, 3, 5, 23, 8, 32)]
		public void TestConcurrentStatusRequestsByMenuItemAR()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(TurkeyBranch.PK.ToGuid(), TestDateAttribute.Date.AddDays(-10)))
			{
				TestObjectCreator.CreateNewComplianceSequence(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN);

				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "4", TestObjectCreator.TRY, 1m, 100m, 0m, 100m, 0m);
				arInvoice.AH_OH = TestObjectCreator.DebtorTR.PK;
				var orgProxy = arInvoice.Company.OrgProxy;
				orgProxy.CustomsCodes.AddNew("VAT", "34567890123");
				arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
				Factory.Save();

				var submitBatch = TestObjectCreator.CreateEInvoicingBatch(104, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				submitBatch.AIB_GovernmentAllocatedNumber = "F3A5099B-A694-4D87-9F61-1D2FE805FE6A";
				TestObjectCreator.CreateEInvoicingTransactionPivot(submitBatch, arInvoice, EInvoicingPivotState.Delivered, EInvoicingPivotActionType.Submit);
				var statusBatch = TestObjectCreator.CreateEInvoicingBatch(105, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var statusPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(statusBatch, arInvoice, EInvoicingPivotState.Sent, EInvoicingPivotActionType.StatusCheck);
				statusPivot.AIP_LastSentTimeUtc = TestDateAttribute.Date.AddHours(-1);
				Factory.Save();

				Assert(statusPivot.AIP_LastResponseReceivedUtc.IsEmpty);

				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText(statusRequestMenuItemDesc);

				selectedBusinessObjects.Add(arInvoice);

				//Asserting request is already sent
				PerformClickAndAssertPivotAndExpectedMessage(requestMenuItem, "Transaction Number 00001000: The request has already been sent.", statusPivot, EInvoicingPivotState.Sent);

				//New attempt for pivot with response received
				statusPivot.AIP_LastResponseReceivedUtc = TestDateAttribute.Date.AddMinutes(-30);
				Factory.Save();
				PerformClickAndAssertPivotAndExpectedMessage(requestMenuItem, "Transaction Number 00001000: The request is sent.", statusPivot, EInvoicingPivotState.Discarded);

				////Sent Time > Received Time
				statusPivot.AIP_LastSentTimeUtc = TestDateAttribute.Date.AddMinutes(-15);
				statusPivot.AIP_LastResponseReceivedUtc = TestDateAttribute.Date.AddMinutes(-30);
				Factory.Save();
				PerformClickAndAssertPivotAndExpectedMessage(requestMenuItem, "Transaction Number 00001000: The request has already been sent.", statusPivot, EInvoicingPivotState.Discarded);

				//Asserting a new status request pivot is created
				var newStatusPivot = GetActiveStatusPivot(arInvoice.PK);
				AssertNotNull(newStatusPivot);
				AssertNotEquals(statusPivot.PK, newStatusPivot.PK);
				AssertEquals("Expected status for newly created pivot should be Queued", EInvoicingPivotState.Queued, newStatusPivot.AIP_Status);
				Assert(newStatusPivot.AIP_LastSentTimeUtc.IsEmpty);
				Assert(newStatusPivot.AIP_LastResponseReceivedUtc.IsEmpty);
			}
		}

		[TestDate(2021, 3, 5, 23, 8, 32)]
		public void TestConcurrentStatusRequestsByMenuItemAP()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Payables(TurkeyBranch.PK.ToGuid(), TestDateAttribute.Date.AddDays(-10)))
			{
				var apCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "00001000", TestObjectCreator.TRY, 1m, 100m, 0m, 100m, 0m);
				apCreditNote.AH_OH = TestObjectCreator.DebtorTR.PK;
				var orgProxy = apCreditNote.Company.OrgProxy;
				orgProxy.CustomsCodes.AddNew("VAT", "34567890123");
				apCreditNote.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN;
				apCreditNote.AH_TransactionReference = "apCreditNote";
				Factory.Save();

				var submitBatch = TestObjectCreator.CreateEInvoicingBatch(104, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				submitBatch.AIB_GovernmentAllocatedNumber = "F3A5099B-A694-4D87-9F61-1D2FE805FE6A";
				TestObjectCreator.CreateEInvoicingTransactionPivot(submitBatch, apCreditNote, EInvoicingPivotState.Delivered, EInvoicingPivotActionType.Submit);
				var statusBatch = TestObjectCreator.CreateEInvoicingBatch(105, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var statusPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(statusBatch, apCreditNote, EInvoicingPivotState.Sent, EInvoicingPivotActionType.StatusCheck);
				statusPivot.AIP_LastSentTimeUtc = TestDateAttribute.Date.AddHours(-1);
				Factory.Save();

				Assert(statusPivot.AIP_LastResponseReceivedUtc.IsEmpty);

				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				var requestMenuItem = guiHelper.GetActionMenuItems(isAPTransaction: true).FindByText(statusRequestMenuItemDesc);

				selectedBusinessObjects.Add(apCreditNote);

				//Asserting request is already sent
				PerformClickAndAssertPivotAndExpectedMessage(requestMenuItem, "Transaction Number 00001000: The request has already been sent.", statusPivot, EInvoicingPivotState.Sent);

				//New attempt for pivot with response received
				statusPivot.AIP_LastResponseReceivedUtc = TestDateAttribute.Date.AddMinutes(-30);
				Factory.Save();
				PerformClickAndAssertPivotAndExpectedMessage(requestMenuItem, "Transaction Number 00001000: The request is sent.", statusPivot, EInvoicingPivotState.Discarded);

				////Sent Time > Received Time
				statusPivot.AIP_LastSentTimeUtc = TestDateAttribute.Date.AddMinutes(-15);
				statusPivot.AIP_LastResponseReceivedUtc = TestDateAttribute.Date.AddMinutes(-30);
				Factory.Save();
				PerformClickAndAssertPivotAndExpectedMessage(requestMenuItem, "Transaction Number 00001000: The request has already been sent.", statusPivot, EInvoicingPivotState.Discarded);

				//Asserting a new status request pivot is created
				var newStatusPivot = GetActiveStatusPivot(apCreditNote.PK);
				AssertNotNull(newStatusPivot);
				AssertNotEquals(statusPivot.PK, newStatusPivot.PK);
				AssertEquals("Expected status for newly created pivot should be Queued", EInvoicingPivotState.Queued, newStatusPivot.AIP_Status);
				Assert(newStatusPivot.AIP_LastSentTimeUtc.IsEmpty);
				Assert(newStatusPivot.AIP_LastResponseReceivedUtc.IsEmpty);
			}
		}

		void AssertPivotExistsAndStatus(AccEInvoicingTransactionPivot pivotToBeAsserted, string pivotStatus)
		{
			AssertNotNull(pivotToBeAsserted);
			AssertEquals(pivotStatus, pivotToBeAsserted.AIP_Status);
		}

		AccEInvoicingTransactionPivot GetActiveStatusPivot(ZGuid pK)
		{
			return Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, pK)
			.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck)
			.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded)).First();
		}

		void PerformClickAndAssertPivotAndExpectedMessage(MenuItem menuItem, string expectedMessage, AccEInvoicingTransactionPivot pivotToBeAsserted, string pivotStatus)
		{
			menuItem.PerformClick();
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertPivotExistsAndStatus(pivotToBeAsserted, pivotStatus);

			UnitTestUserNotification.Instance.ClearMessages();
		}

		#endregion

		#region Queue Pending Invoice

		public void TestQueuePendingInvoiceMenuItemForChinaCompany_OnlyDisplayWhenRegistryIsPending()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingChina(DateTime.Today.AddDays(-30)))
			{
				var selectedBusinessObjects = new List<BusinessObject>();
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());

				using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
				{
					var requestMenuItem = guiHelper.GetActionMenuItems().FindByText("Authorize And Send");

					AssertNull("We will not have menu item 'Authorize And Send' when EReportingSubmitPivotDefaultStatus is not pending.", requestMenuItem);
				}

				using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
				{
					var requestMenuItem = guiHelper.GetActionMenuItems().FindByText("Authorize And Send");

					AssertNotNull("We will  have menu item 'Authorize And Send' when EReportingSubmitPivotDefaultStatus is pending.", requestMenuItem);
				}
			}
		}

		public void TestQueuePendingInvoiceMenuItemForChinaCompany()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingChina(DateTime.Today.AddDays(-30)))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1, TestObjectCreator.AALSHI);
				TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1, 100);
				arInvoice.AH_OH = TestObjectCreator.DebtorTR.PK;
				arInvoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;

				Factory.Save();

				var pivot = arInvoice.GetMostRecentEInvoicingTransactionPivot();
				AssertEquals("Pivot is Pending for invoice.", EInvoicingPivotState.Pending, pivot.AIP_Status);

				var selectedInvoice = new List<TransactionHeader> { arInvoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedInvoice);
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText("Authorize And Send");

				AssertNotNull("We will have menu item 'Authorize And Send' when EReportingSubmitPivotDefaultStatus is pending.", requestMenuItem);

				requestMenuItem.PerformClick();

				AssertEquals("Pivot is Queue for invoice 1.", EInvoicingPivotState.Queued, pivot.AIP_Status);
			}
		}

		public void TestQueuePendingInvoiceMenuItemForChinaCompany_SelectNotPendingInvoice()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingChina(DateTime.Today.AddDays(-30)))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1, TestObjectCreator.AALSHI);
				TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1, 100);
				arInvoice.AH_OH = TestObjectCreator.DebtorTR.PK;
				arInvoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				Factory.Save();

				var pivot = arInvoice.GetMostRecentEInvoicingTransactionPivot();
				AssertEquals("Pivot is Pending for invoice.", EInvoicingPivotState.Pending, pivot.AIP_Status);

				var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1, TestObjectCreator.AALSHI);
				TestObjectCreator.CreateInvoiceLine(arInvoice2, TestObjectCreator.AUD, 1, 100);
				arInvoice2.AH_OH = TestObjectCreator.DebtorTR.PK;
				arInvoice2.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				Factory.Save();

				var pivot2 = arInvoice2.GetMostRecentEInvoicingTransactionPivot();
				pivot2.AIP_Status = EInvoicingPivotState.Failed;
				Factory.Save();
				AssertEquals("Pivot is not Pending for invoice.", EInvoicingPivotState.Failed, pivot2.AIP_Status);

				var selectedInvoice = new List<TransactionHeader> { arInvoice, arInvoice2 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedInvoice);
				var requestMenuItem = guiHelper.GetActionMenuItems().FindByText("Authorize And Send");

				AssertNotNull("We will have menu item 'Authorize And Send' when EReportingSubmitPivotDefaultStatus is pending.", requestMenuItem);

				UnitTestUserNotification.Instance.ClearMessages();
				requestMenuItem.PerformClick();

				AssertEquals("Pivot is Queue for invoice 1.", EInvoicingPivotState.Queued, pivot.AIP_Status);
				AssertNotEquals("Pivot is not Queue for invoice 2.", EInvoicingPivotState.Queued, pivot2.AIP_Status);
				AssertContains("You can only Authorize and Send transactions where the E-Reporting status is 'PEN - Pending'.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			}
		}

		public void TestGetPenaltyTaxInfoActionMenuItem_ForAllCountries()
		{
			var countriesWithSpecialTestCase = new ZString[] { CountryCodes.KoreaSouth };

			var countriesQuery = new ZQuery() { OrderBy = RefCountrySchema.Constants.RN_Code };
			var countries = Factory.Load<RefCountry>(countriesQuery).Where(x => !countriesWithSpecialTestCase.Contains(x.Code));

			foreach (var country in countries)
			{
				using (TestObjectCreator.SetUpForTestingEInvoicing(country.Code, true))
				{
					var guiHelper = new EInvoicingGUIActionHelper(() => null);
					var menuItem = guiHelper.GetPenaltyTaxInfoActionMenuItem();

					AssertNull("By default should not show PenaltyTaxInfo menu item.", menuItem);
				}
			}
		}

		public void TestGetPenaltyTaxInfoActionMenuItem_KoreaSouth()
		{
			EInvoicingGUIActionHelper guiHelper;
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, false))
			{
				guiHelper = new EInvoicingGUIActionHelper(() => null);
				AssertEquals("PreCondition", CountryCodes.KoreaSouth, Env.CurrentCompany.Country.Code);
				var menuItem = guiHelper.GetPenaltyTaxInfoActionMenuItem();
				AssertNull(menuItem);
			}
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			{
				var eInvoicingPenaltyTaxInfoProvider = EInvoicingPenaltyTaxInfoFormFactory.CreateEInvoicingPenaltyInfoFormProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				AssertEquals("Additional Tax Information", eInvoicingPenaltyTaxInfoProvider.PenaltyTaxInfoMenuName);

				guiHelper = new EInvoicingGUIActionHelper(() => null);
				var menuItem = guiHelper.GetPenaltyTaxInfoActionMenuItem();
				AssertNotNull(menuItem);
				AssertEquals("Additional Tax Information", menuItem.Text);

				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ShowDialogsInTest = true;

				menuItem.PerformClick();

				using (var form = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull(form);
					AssertType<KoreaSouthEInvoicingPenaltyTaxInfoForm>(form);
					AssertEquals("Additional Tax Information", form.Text);
				}
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TurkeyBranch = TestObjectCreator.CreateBranchWithCompany("TRIST");
			AfghanistanBranch = TestObjectCreator.CreateBranchWithCompany("AFBIN");
			Factory.Save();
		}

		GlbBranch TurkeyBranch;
		GlbBranch AfghanistanBranch;

		const string pdfRequestMenuItemDesc = "Request e-Invoice PDF Copy";
		const string statusRequestMenuItemDesc = "Request e-Invoice Transaction Status Update";

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		#endregion

		#region E-Invoicing Transaction Status Resetting

		public void TestResetStatusToQueued_SingleTransaction_HasWarningConfirmWhenRequeSentForKorea()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			{
				var guiHelper = setUp();
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.ReceivablesResetTransactionStatus;
					securityCheckPoint.IsAllowed = true;
					Assert("User has security right to requeue SNT transactions", securityCheckPoint.IsAllowed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Last Confirmation", "Yes", UnitTestUserNotification.Instance.LastConfirmationStringShown);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var guiHelper = setUp();
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.ReceivablesResetTransactionStatus;
					securityCheckPoint.IsAllowed = true;
					Assert("User has security right to requeue SNT transactions", securityCheckPoint.IsAllowed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Transaction was successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			EInvoicingGUIActionHelper setUp()
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", invoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				var batchPK = TestObjectCreator.CreateEInvoicingBatch();
				Factory.Save();

				var batch = Factory.Load<AccEInvoicingBatch>(batchPK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batchPK, ZDateTime.Empty, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batchPK, ZDateTime.Empty, ZDateTime.Today, ZBool.True);

				BusinessObject[] selectedBusinessObjects = { invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				return guiHelper;
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_SingleTransaction_HasErrorStatus_BER()
		{
			ResetStatusToQueued_SingleTransaction_HasErrorStatus_Core(EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors");
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_SingleTransaction_HasErrorStatus_FAL()
		{
			ResetStatusToQueued_SingleTransaction_HasErrorStatus_Core(EInvoicingPivotState.Failed, "This transaction was rejected by IIS site");
		}

		[TestDate(2006, 5, 10)]
		public void ResetStatusToQueued_SingleTransaction_HasErrorStatus_Core(ZString status, ZString errorMessage)
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice = Factory.New<ARInvoice>();
				invoice.FillWithValidTestData();
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				var batchPK = TestObjectCreator.CreateEInvoicingBatch();
				Factory.Save();

				var batch = Factory.Load<AccEInvoicingBatch>(batchPK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice.PK, status, errorMessage, batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(invoice.PK, status, errorMessage, batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

				BusinessObject[] selectedBusinessObjects = { invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Transaction was successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					Assert("No warning message is displayed when selected transaction has an error status and can be requeued", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You can only reset transactions"));

					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch.AIB_Status);
					AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_SingleTransaction_HasNoErrorOrSentStatus()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice.IsEligibleToCreateEInvoicingTransactionPivot);
				var preRequeuePivot = AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				BusinessObject[] selectedBusinessObjects = { invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					var postRequeuePivot = AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertEquals("Pivot should not be changed", preRequeuePivot.PK, postRequeuePivot.PK);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_SingleTransaction_HasSentStatusWithSecurityRights()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				var batchPK = TestObjectCreator.CreateEInvoicingBatch();
				Factory.Save();

				var batch = Factory.Load<AccEInvoicingBatch>(batchPK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batchPK, ZDateTime.Empty, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batchPK, ZDateTime.Empty, ZDateTime.Today, ZBool.True);

				BusinessObject[] selectedBusinessObjects = { invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.ReceivablesResetTransactionStatus;
					securityCheckPoint.IsAllowed = false;
					Assert("User does not have security right to requeue SNT transactions", !securityCheckPoint.IsAllowed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch.AIB_Status);
					AssertPivotDetails(invoice.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batchPK, ZDateTime.Empty, ZDateTime.Today, ZBool.True);

					securityCheckPoint.IsAllowed = true;
					Assert("User has security right to requeue SNT transactions", securityCheckPoint.IsAllowed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Transaction was successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					Assert("No warning message is displayed when selected transaction has SNT status and user has security rights", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You do not have appropriate security rights to reset transactions where the E-Reporting status is 'SNT' - Sent."));

					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch.AIB_Status);
					AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_ForPreviousTransactionsWhichDoNotHavePivots()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var pivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				AssertNull(pivot);
				Assert("Precondition: eInvoicing enabled", invoice.IsEligibleToCreateEInvoicingTransactionPivot);

				BusinessObject[] selectedBusinessObjects = { invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNoExceptionThrown("Transaction pivot not available in DB, should not throw null reference exception", () => guiHelper.ResetStatusToQueued(securityCheckPoint));
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_ManyTransactions_AllHaveErrorStatus()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var creditNote1 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice1);
				var creditNote2 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice2);
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", invoice2.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote2.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				var batch1PK = TestObjectCreator.CreateEInvoicingBatch(101);
				var batch2PK = TestObjectCreator.CreateEInvoicingBatch(102);
				var batch3PK = TestObjectCreator.CreateEInvoicingBatch(103);
				var batch4PK = TestObjectCreator.CreateEInvoicingBatch(104);
				Factory.Save();

				var batch1 = Factory.Load<AccEInvoicingBatch>(batch1PK);
				var batch2 = Factory.Load<AccEInvoicingBatch>(batch2PK);
				var batch3 = Factory.Load<AccEInvoicingBatch>(batch3PK);
				var batch4 = Factory.Load<AccEInvoicingBatch>(batch4PK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice1.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch1PK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice2.PK, EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors", batch2PK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote1.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch3PK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote2.PK, EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors", batch4PK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);

				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch1PK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors", batch2PK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch3PK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors", batch4PK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);
				AssertEquals("Precondition", EInvoicingBatchState.Sent, batch1.AIB_Status);
				AssertEquals("Precondition", EInvoicingBatchState.Sent, batch2.AIB_Status);
				AssertEquals("Precondition", EInvoicingBatchState.Sent, batch3.AIB_Status);
				AssertEquals("Precondition", EInvoicingBatchState.Sent, batch4.AIB_Status);

				BusinessObject[] selectedBusinessObjects = { invoice1, invoice2, creditNote1, creditNote2 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Eligible transactions were successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					Assert("No warning message is displayed when all selected transactions have errors and can be requeued", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You can only reset transactions"));

					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch1.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch2.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch3.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch4.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_ManyTransactions_AllHaveNoErrorOrSentStatus()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var creditNote1 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice1);
				var creditNote2 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice2);
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", invoice2.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote2.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice1.PK, EInvoicingPivotState.Succeed, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote1.PK, EInvoicingPivotState.Succeed, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Succeed, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Succeed, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				BusinessObject[] selectedBusinessObjects = { invoice1, invoice2, creditNote1, creditNote2 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Succeed, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Succeed, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_ManyTransactions_WithMixedErrorAndNonErrorStatus()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var creditNote1 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice1);
				var creditNote2 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice2);
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", invoice2.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote2.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				var batch1PK = TestObjectCreator.CreateEInvoicingBatch(101);
				var batch2PK = TestObjectCreator.CreateEInvoicingBatch(102);
				var batch3PK = TestObjectCreator.CreateEInvoicingBatch(103);
				var batch4PK = TestObjectCreator.CreateEInvoicingBatch(104);
				Factory.Save();

				var batch1 = Factory.Load<AccEInvoicingBatch>(batch1PK);
				var batch2 = Factory.Load<AccEInvoicingBatch>(batch2PK);
				var batch3 = Factory.Load<AccEInvoicingBatch>(batch3PK);
				var batch4 = Factory.Load<AccEInvoicingBatch>(batch4PK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice1.PK, EInvoicingPivotState.Succeed, "", batch1PK, ZDateTime.Today, ZDateTime.Today, ZBool.False);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote1.PK, EInvoicingPivotState.Succeed, "", batch3PK, ZDateTime.Today, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch4PK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);

				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Succeed, ZString.Empty, batch1PK, ZDateTime.Today, ZDateTime.Today, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Succeed, ZString.Empty, batch3PK, ZDateTime.Today, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch4PK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);

				BusinessObject[] selectedBusinessObjects = { invoice1, invoice2, creditNote1, creditNote2 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).
", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasWarning);
					AssertEquals("Eligible transactions were successfully re-queued.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					Assert(UnitTestUserNotification.Instance.PreviousMessages[0].WasInformation);

					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch1.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch2.AIB_Status);
					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch3.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch4.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Succeed, ZString.Empty, batch1PK, ZDateTime.Today, ZDateTime.Today, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Succeed, ZString.Empty, batch3PK, ZDateTime.Today, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_ManyTransactions_AllHaveSentStatusWithSecurityRights()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var creditNote1 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice1);
				var creditNote2 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice2);
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", invoice2.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote2.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				var batch1PK = TestObjectCreator.CreateEInvoicingBatch(101);
				var batch2PK = TestObjectCreator.CreateEInvoicingBatch(102);
				var batch3PK = TestObjectCreator.CreateEInvoicingBatch(103);
				var batch4PK = TestObjectCreator.CreateEInvoicingBatch(104);
				Factory.Save();

				var batch1 = Factory.Load<AccEInvoicingBatch>(batch1PK);
				var batch2 = Factory.Load<AccEInvoicingBatch>(batch2PK);
				var batch3 = Factory.Load<AccEInvoicingBatch>(batch3PK);
				var batch4 = Factory.Load<AccEInvoicingBatch>(batch4PK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch1PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice2.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch2PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch3PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote2.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch4PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);

				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch1PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch2PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch3PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch4PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);

				BusinessObject[] selectedBusinessObjects = { invoice1, invoice2, creditNote1, creditNote2 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.ReceivablesResetTransactionStatus;
					securityCheckPoint.IsAllowed = false;
					Assert("User does not have security right to requeue SNT transactions", !securityCheckPoint.IsAllowed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch1.AIB_Status);
					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch2.AIB_Status);
					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch3.AIB_Status);
					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch4.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch1PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch2PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch3PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch4PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);

					securityCheckPoint.IsAllowed = true;
					Assert("User has security right to requeue SNT transactions", securityCheckPoint.IsAllowed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Eligible transactions were successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					Assert("No warning message is displayed when all selected transactions have SNT status and user has security rights", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You do not have appropriate security rights to reset transactions where the E-Reporting status is 'SNT' - Sent."));

					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch1.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch2.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch3.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch4.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_ManyTransactions_WithMixedErrorAndSentStatusWithSecurityRights()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var creditNote1 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice1);
				var creditNote2 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice2);
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", invoice2.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote2.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				var batch1PK = TestObjectCreator.CreateEInvoicingBatch(101);
				var batch2PK = TestObjectCreator.CreateEInvoicingBatch(102);
				var batch3PK = TestObjectCreator.CreateEInvoicingBatch(103);
				var batch4PK = TestObjectCreator.CreateEInvoicingBatch(104);
				Factory.Save();

				var batch1 = Factory.Load<AccEInvoicingBatch>(batch1PK);
				var batch2 = Factory.Load<AccEInvoicingBatch>(batch2PK);
				var batch3 = Factory.Load<AccEInvoicingBatch>(batch3PK);
				var batch4 = Factory.Load<AccEInvoicingBatch>(batch4PK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch1PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch3PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch4PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True, EInvoicingPivotActionType.Cancel);

				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch1PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch3PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch4PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True, EInvoicingPivotActionType.Cancel);

				BusinessObject[] selectedBusinessObjects = { invoice1, invoice2, creditNote1, creditNote2 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.ReceivablesResetTransactionStatus;
					securityCheckPoint.IsAllowed = false;
					Assert("User does not have security right to requeue SNT transactions", !securityCheckPoint.IsAllowed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).
", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasWarning);
					AssertEquals("Eligible transactions were successfully re-queued.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					Assert(UnitTestUserNotification.Instance.PreviousMessages[0].WasInformation);

					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch1.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch2.AIB_Status);
					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch3.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch4.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch1PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch3PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

					TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice2.PK, EInvoicingPivotState.BatchedWithError, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.BatchedWithError, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

					TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote2.PK, EInvoicingPivotState.BatchedWithError, "This transaction was rejected by IIS site", batch4PK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.BatchedWithError, "This transaction was rejected by IIS site", batch4PK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);

					securityCheckPoint.IsAllowed = true;
					Assert("User has security right to requeue SNT transactions", securityCheckPoint.IsAllowed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Eligible transactions were successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					Assert("No warning message is displayed when all selected transactions are requeued and user has security rights", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You do not have appropriate security rights to reset transactions where the E-Reporting status is 'SNT' - Sent."));

					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch1.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch2.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch3.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch4.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_ManyTransactions_WithMixedNonErrorAndSentStatusWithSecurityRights()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var creditNote1 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice1);
				var creditNote2 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice2);
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", invoice2.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote2.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				var batch1PK = TestObjectCreator.CreateEInvoicingBatch(101);
				var batch2PK = TestObjectCreator.CreateEInvoicingBatch(102);
				Factory.Save();

				var batch1 = Factory.Load<AccEInvoicingBatch>(batch1PK);
				var batch2 = Factory.Load<AccEInvoicingBatch>(batch2PK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch1PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch2PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);

				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch1PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch2PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				BusinessObject[] selectedBusinessObjects = { invoice1, invoice2, creditNote1, creditNote2 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.ReceivablesResetTransactionStatus;
					securityCheckPoint.IsAllowed = false;
					Assert("User does not have security right to requeue SNT transactions", !securityCheckPoint.IsAllowed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch1.AIB_Status);
					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch2.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch1PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch2PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

					securityCheckPoint.IsAllowed = true;
					Assert("User has security right to requeue SNT transactions", securityCheckPoint.IsAllowed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).
", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasWarning);
					AssertEquals("Eligible transactions were successfully re-queued.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					Assert(UnitTestUserNotification.Instance.PreviousMessages[0].WasInformation);

					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch1.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch2.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_ManyTransactions_WithMixedErrorAndNonErrorAndSentStatusWithSecurityRights()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice3 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var creditNote1 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice1);
				var creditNote2 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice2);
				var creditNote3 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice3);
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", invoice2.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", invoice3.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote2.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote3.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(invoice3.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote3.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				var batch1PK = TestObjectCreator.CreateEInvoicingBatch(101);
				var batch2PK = TestObjectCreator.CreateEInvoicingBatch(102);
				var batch3PK = TestObjectCreator.CreateEInvoicingBatch(103);
				var batch4PK = TestObjectCreator.CreateEInvoicingBatch(104);
				Factory.Save();

				var batch1 = Factory.Load<AccEInvoicingBatch>(batch1PK);
				var batch2 = Factory.Load<AccEInvoicingBatch>(batch2PK);
				var batch3 = Factory.Load<AccEInvoicingBatch>(batch3PK);
				var batch4 = Factory.Load<AccEInvoicingBatch>(batch4PK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch1PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch3PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch4PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True, EInvoicingPivotActionType.Cancel);

				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch1PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True);
				AssertPivotDetails(invoice3.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch3PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch4PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote3.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				BusinessObject[] selectedBusinessObjects = { invoice1, invoice2, invoice3, creditNote1, creditNote2, creditNote3 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.ReceivablesResetTransactionStatus;
					securityCheckPoint.IsAllowed = false;
					Assert("User does not have security right to requeue SNT transactions", !securityCheckPoint.IsAllowed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).
", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasWarning);
					AssertEquals("Eligible transactions were successfully re-queued.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					Assert(UnitTestUserNotification.Instance.PreviousMessages[0].WasInformation);

					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch1.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch2.AIB_Status);
					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch3.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch4.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch1PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice3.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Sent, "This transaction was sent to the IIS site and waiting response.", batch3PK, ZDateTime.Empty, ZDateTime.Today, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote3.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

					TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice2.PK, EInvoicingPivotState.BatchedWithError, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.BatchedWithError, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

					TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote2.PK, EInvoicingPivotState.BatchedWithError, "This transaction was rejected by IIS site", batch4PK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.BatchedWithError, "This transaction was rejected by IIS site", batch4PK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);

					securityCheckPoint.IsAllowed = true;
					Assert("User has security right to requeue SNT transactions", securityCheckPoint.IsAllowed);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Eligible transactions were successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					Assert("No warning message is displayed when all selected transactions are requeued and user has security rights", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You do not have appropriate security rights to reset transactions where the E-Reporting status is 'SNT' - Sent."));

					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch1.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch2.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch3.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch4.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice3.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote3.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_ManyTransactions_WhenTransactionIsReversed_ShouldNotReject()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var creditNote = (CreditNote)TestObjectCreator.ReverseTransaction(invoice, out string cantReverseMessage);
				creditNote.AH_TransactionNum = invoice.AH_TransactionNum;
				Factory.Save();

				Assert(invoice.AH_IsCancelled);
				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				var invoiceBatchPK = TestObjectCreator.CreateEInvoicingBatch(101);
				var creditNoteBatchPK = TestObjectCreator.CreateEInvoicingBatch(102);
				Factory.Save();

				var invoiceBatch = Factory.Load<AccEInvoicingBatch>(invoiceBatchPK);
				var creditNoteBatch = Factory.Load<AccEInvoicingBatch>(creditNoteBatchPK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", invoiceBatchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", creditNoteBatchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);

				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", invoiceBatchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(creditNote.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", creditNoteBatchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);

				BusinessObject[] selectedBusinessObjects = { invoice, creditNote };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Eligible transactions were successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, Guid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote.PK, EInvoicingPivotState.Queued, ZString.Empty, Guid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals("Postcondition: second re-queue perform where is no eligible transaction to re-queue", @"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					AssertEquals("Postcondition: original batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, invoiceBatch.AIB_Status);
					AssertEquals("Postcondition: original batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, creditNoteBatch.AIB_Status);
					AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, Guid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote.PK, EInvoicingPivotState.Queued, ZString.Empty, Guid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				}
			}
		}

		public void TestResetStatusToQueued_ManyTransactions_WhenTransactionIsReversed_ShouldReject()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var creditNote = (CreditNote)TestObjectCreator.ReverseTransaction(invoice, out string cantReverseMessage);
				creditNote.AH_TransactionNum = invoice.AH_TransactionNum;
				Factory.Save();

				TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Queued);
				TestObjectCreator.CreateEInvoicingTransactionPivot(creditNote, EInvoicingPivotActionType.Cancel, EInvoicingPivotState.Queued);
				Factory.Save();

				Assert(invoice.AH_IsCancelled);
				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				var invoiceBatchPK = TestObjectCreator.CreateEInvoicingBatch(101);
				var creditNoteBatchPK = TestObjectCreator.CreateEInvoicingBatch(102);
				Factory.Save();

				var invoiceBatch = Factory.Load<AccEInvoicingBatch>(invoiceBatchPK);
				var creditNoteBatch = Factory.Load<AccEInvoicingBatch>(creditNoteBatchPK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", invoiceBatchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", creditNoteBatchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);

				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", invoiceBatchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(creditNote.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", creditNoteBatchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);

				BusinessObject[] selectedBusinessObjects = { invoice, creditNote };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Transaction was successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertPivotDetails(invoice.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", invoiceBatchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
					AssertPivotDetails(creditNote.PK, EInvoicingPivotState.Queued, ZString.Empty, Guid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

Reversed transactions cannot be re-queued.

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					AssertEquals("Postcondition: original batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, invoiceBatch.AIB_Status);
					AssertEquals("Postcondition: original batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, creditNoteBatch.AIB_Status);
					AssertPivotDetails(invoice.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", invoiceBatchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
					AssertPivotDetails(creditNote.PK, EInvoicingPivotState.Queued, ZString.Empty, Guid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				}
			}
		}

		public void TestResetStatusToQueued_ManyTransactions_WhenTransactionIsReversed_ShouldNotReject_APInvoice()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice = Factory.NewWithValidTestData<APInvoice>();
				Factory.Save();

				var creditNote = (CreditNote)TestObjectCreator.ReverseTransaction(invoice, out string cantReverseMessage);
				creditNote.AH_TransactionNum = invoice.AH_TransactionNum;
				creditNote.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN;
				Factory.Save();

				TestObjectCreator.CreateEInvoicingTransactionPivot(creditNote, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Queued);
				Factory.Save();

				Assert(invoice.AH_IsCancelled);
				AssertPivotDetails(creditNote.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Submit);

				var creditNoteBatchPK = TestObjectCreator.CreateEInvoicingBatch(102);
				Factory.Save();

				var creditNoteBatch = Factory.Load<AccEInvoicingBatch>(creditNoteBatchPK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote.PK, EInvoicingPivotState.BatchedWithError, "CargoWise encountered an unexpected error", creditNoteBatchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Submit);

				AssertPivotDetails(creditNote.PK, EInvoicingPivotState.BatchedWithError, "CargoWise encountered an unexpected error", creditNoteBatchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Submit);

				BusinessObject[] selectedBusinessObjects = { invoice, creditNote };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Transaction was successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertPivotDetails(creditNote.PK, EInvoicingPivotState.Queued, ZString.Empty, Guid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Submit);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					AssertEquals("Postcondition: original batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, creditNoteBatch.AIB_Status);
					AssertPivotDetails(creditNote.PK, EInvoicingPivotState.Queued, ZString.Empty, Guid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Submit);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_ManyTransactions_WhenOnlySomeTransactionsInABatchAreRequeued()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Egypt))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice3 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var creditNote1 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice1);
				var creditNote2 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice2);
				var creditNote3 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice3);
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", invoice2.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", invoice3.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote2.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote3.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(invoice3.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote3.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				var batch1PK = TestObjectCreator.CreateEInvoicingBatch(101);
				var batch2PK = TestObjectCreator.CreateEInvoicingBatch(102);
				Factory.Save();

				var batch1 = Factory.Load<AccEInvoicingBatch>(batch1PK);
				var batch2 = Factory.Load<AccEInvoicingBatch>(batch2PK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice1.PK, EInvoicingPivotState.BatchedWithError, "This transaction was rejected by IIS site", batch1PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch1PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice3.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch1PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote1.PK, EInvoicingPivotState.BatchedWithError, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True, EInvoicingPivotActionType.Cancel);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True, EInvoicingPivotActionType.Cancel);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote3.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True, EInvoicingPivotActionType.Cancel);

				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.BatchedWithError, "This transaction was rejected by IIS site", batch1PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch1PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True);
				AssertPivotDetails(invoice3.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch1PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.BatchedWithError, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote3.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True, EInvoicingPivotActionType.Cancel);

				BusinessObject[] selectedBusinessObjects = { invoice1, invoice2, creditNote1, creditNote2 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Eligible transactions were successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					Assert("No warning message is displayed when all selected transactions have errors and can be requeued", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You can only reset transactions"));

					AssertEquals("Postcondition: not discarded since there are other transactions in the batch not yet requeued", EInvoicingBatchState.Sent, batch1.AIB_Status);
					AssertEquals("Postcondition: not discarded since there are other transactions in the batch not yet requeued", EInvoicingBatchState.Sent, batch2.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice3.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch1PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote3.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batch2PK, ZDateTime.Today, ZDateTime.Empty, ZBool.True, EInvoicingPivotActionType.Cancel);

					selectedBusinessObjects = new BusinessObject[] { invoice3, creditNote3 };
					guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("Eligible transactions were successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					Assert("No warning message is displayed when all selected transactions have errors and can be requeued", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You can only reset transactions"));

					AssertEquals("Postcondition: discarded since all transactions that were in the batch have been requeued", EInvoicingBatchState.Discarded, batch1.AIB_Status);
					AssertEquals("Postcondition: discarded since all transactions that were in the batch have been requeued", EInvoicingBatchState.Discarded, batch2.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice3.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote3.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_WhenTransactionHasSentStatusWithoutSubmitOrCancelActionType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				// Create a pivot since TR isn't eligible to create pivots with current setup
				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice1, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Failed);
				var batch1 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot1, 101, EInvoicingBatchState.Sent);
				Factory.Save();

				// When DocumentAction pivot status is Sent
				var batch1StatusCheck = TestObjectCreator.CreateEInvoicingBatch(102, EInvoicingBatchState.Sent, invoice1.Company);
				var pivot1StatusCheck = TestObjectCreator.CreateEInvoicingTransactionPivot(batch1StatusCheck, invoice1, EInvoicingPivotState.Failed, EInvoicingPivotActionType.StatusCheck);

				var batch1PdfRequest = TestObjectCreator.CreateEInvoicingBatch(103, EInvoicingBatchState.Sent, invoice1.Company);
				var pivot1PdfRequest = TestObjectCreator.CreateEInvoicingTransactionPivot(batch1PdfRequest, invoice1, EInvoicingPivotState.Sent, EInvoicingPivotActionType.DocumentAction);
				Factory.Save();

				BusinessObject[] selectedBusinessObjects = { invoice1 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Failed, ZString.Empty, batch1.PK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Failed, ZString.Empty, batch1StatusCheck.PK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.StatusCheck);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Sent, ZString.Empty, batch1PdfRequest.PK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.DocumentAction);

					// When StatusCheck pivot status is Sent
					pivot1StatusCheck.AIP_Status = EInvoicingPivotState.Sent;
					pivot1PdfRequest.AIP_Status = EInvoicingPivotState.Succeed;
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Failed, ZString.Empty, batch1.PK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Sent, ZString.Empty, batch1StatusCheck.PK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.StatusCheck);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Succeed, ZString.Empty, batch1PdfRequest.PK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.DocumentAction);

					// When not exist the request pivots on Sent status
					pivot1StatusCheck.AIP_Status = EInvoicingPivotState.Failed;
					pivot1PdfRequest.AIP_Status = EInvoicingPivotState.Succeed;
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertMultilineASCIIEquals(@"Transaction was successfully re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);

					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch1.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch1StatusCheck.AIB_Status);
					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch1PdfRequest.AIB_Status);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Discarded, ZString.Empty, batch1StatusCheck.PK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.StatusCheck);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Discarded, ZString.Empty, batch1PdfRequest.PK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.DocumentAction);
				}
			}
		}

		public void TestResetStatusToQueued_WhenCountry_DoesNotImplementBatchStrategy()
		{
			var mockGlobalFactory = new Mock<IGlobalEInvoicingObjectFactory>();
			mockGlobalFactory.Setup(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>())).Returns(() => null);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, EInvoicingPivotActionType.Amend, EInvoicingPivotState.Sent);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 101, EInvoicingBatchState.Sent);
			Factory.Save();

			ObjectFactory.Substitute(mockGlobalFactory.Object);
			var selectedTransactions = new List<TransactionHeader>() { invoice };
			var guiActionHelper = new EInvoicingGUIActionHelper(() => selectedTransactions);
			guiActionHelper.ResetStatusToQueued(Env.Security.None);

			mockGlobalFactory.Verify(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>()), Times.Once);
			AssertMultilineASCIIEquals(RequeueFullErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

			var securityCheckPoint = Env.Security.ReceivablesResetTransactionStatus;
			securityCheckPoint.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessages();

			guiActionHelper.ResetStatusToQueued(securityCheckPoint);
			mockGlobalFactory.Verify(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>()), Times.Once);
			AssertMultilineASCIIEquals(RequeueFullErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestResetStatusToQueued_WhenCountry_DoesNotImplementBatchStrategy_DiscardBatch()
		{
			var mockGlobalFactory = new Mock<IGlobalEInvoicingObjectFactory>();
			mockGlobalFactory.Setup(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>())).Returns(() => null);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, EInvoicingPivotActionType.Amend, EInvoicingPivotState.Failed);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 101, EInvoicingBatchState.Sent);

			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice2, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Failed);
			var batch2 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot2, 102, EInvoicingBatchState.Sent);
			Factory.Save();

			ObjectFactory.Substitute(mockGlobalFactory.Object);
			var selectedTransactions = new List<TransactionHeader>() { invoice, invoice2 };
			var guiActionHelper = new EInvoicingGUIActionHelper(() => selectedTransactions);
			guiActionHelper.ResetStatusToQueued(Env.Security.None);

			var previousMessage = UnitTestUserNotification.Instance.PreviousMessages[1];
			mockGlobalFactory.Verify(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>()), Times.Once);
			AssertMultilineASCIIEquals(RequeueErrorMessage, previousMessage.Text);
			Assert(previousMessage.WasWarning);
			AssertEquals("Batch must be discarded", EInvoicingBatchState.Discarded, batch.AIB_Status);
		}

		public void TestResetStatusToQueued_WhenCountry_ImplementBatchStrategy()
		{
			var mockGlobalFactory = new Mock<IGlobalEInvoicingObjectFactory>();
			var mockBatchStrategy = new Mock<IBatchCreatorStrategy>();
			mockBatchStrategy.Setup(x => x.SupportsWaitForOriginalTransactionForAmending).Returns(false);
			mockGlobalFactory.Setup(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>())).Returns(mockBatchStrategy.Object);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, EInvoicingPivotActionType.Amend, EInvoicingPivotState.Failed);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 101, EInvoicingBatchState.Sent);
			Factory.Save();

			ObjectFactory.Substitute(mockGlobalFactory.Object);
			var selectedTransactions = new List<TransactionHeader>() { invoice };
			var guiActionHelper = new EInvoicingGUIActionHelper(() => selectedTransactions);
			guiActionHelper.ResetStatusToQueued(Env.Security.None);

			AssertMultilineASCIIEquals(RequeueFullErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertPivotDetails(invoice.PK, EInvoicingPivotState.Failed, ZString.Empty, batch.PK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Amend);

			mockBatchStrategy.Setup(x => x.SupportsWaitForOriginalTransactionForAmending).Returns(true);
			guiActionHelper = new EInvoicingGUIActionHelper(() => selectedTransactions);
			guiActionHelper.ResetStatusToQueued(Env.Security.None);

			AssertEquals(RequeueSuccessMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Amend);
		}

		public void TestResetStatusToQueued_WhenCountry_ImplementBatchStrategy_SentPivotActionType()
		{
			var mockGlobalFactory = new Mock<IGlobalEInvoicingObjectFactory>();
			var mockBatchStrategy = new Mock<IBatchCreatorStrategy>();
			mockBatchStrategy.Setup(x => x.SupportsWaitForOriginalTransactionForAmending).Returns(true);
			mockGlobalFactory.Setup(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>())).Returns(mockBatchStrategy.Object);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, EInvoicingPivotActionType.Amend, EInvoicingPivotState.Sent);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 101, EInvoicingBatchState.Sent);
			Factory.Save();

			ObjectFactory.Substitute(mockGlobalFactory.Object);
			var selectedTransactions = new List<TransactionHeader>() { invoice };
			var guiActionHelper = new EInvoicingGUIActionHelper(() => selectedTransactions);

			var securityCheckPoint = Env.Security.ReceivablesResetTransactionStatus;
			securityCheckPoint.IsAllowed = false;
			guiActionHelper.ResetStatusToQueued(securityCheckPoint);

			AssertMultilineASCIIEquals(RequeueFullErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertPivotDetails(invoice.PK, EInvoicingPivotState.Sent, ZString.Empty, batch.PK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Amend);

			UnitTestUserNotification.Instance.ClearMessages();
			securityCheckPoint.IsAllowed = true;

			guiActionHelper.ResetStatusToQueued(securityCheckPoint);
			AssertEquals(RequeueSuccessMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Amend);
		}

		public void TestResetStatusToQueued_ForDiscardVietNam()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, status: EInvoicingPivotState.Discarded);
				pivot.AIP_ErrorDescription = AccEInvoicingTransactionPivot.ErrorDescriptionWhenEInvoicingDisabled();
				Factory.Save();

				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Discarded, AccEInvoicingTransactionPivot.ErrorDescriptionWhenEInvoicingDisabled(), ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				var selectedTransactions = new List<TransactionHeader>() { invoice };
				var guiActionHelper = new EInvoicingGUIActionHelper(() => selectedTransactions);

				GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
				guiActionHelper.ResetStatusToQueued(Env.Security.None);

				AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Discarded, AccEInvoicingTransactionPivot.ErrorDescriptionWhenEInvoicingDisabled(), ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				GlbStaff.CurrentUser.GS_LoginName = "CWSupport";
				guiActionHelper.ResetStatusToQueued(Env.Security.None);

				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
			}
		}

		public void TestResetStatusToQueuedForMalaysia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				TestObjectCreator.CreateEInvoicingTransactionPivot(invoice1, status: EInvoicingPivotState.Succeed);
				Factory.Save();

				var selectedTransactions = new List<TransactionHeader>() { invoice1 };
				var guiActionHelper = new EInvoicingGUIActionHelper(() => selectedTransactions);
				guiActionHelper.ResetStatusToQueued(Env.Security.None);
				AssertMultilineASCIIEquals(@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:
- 'FAL' - Fail and E-Reporting Government # is blank, or
- 'BER' - Batched with errors.


No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice3 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice4 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice2, status: EInvoicingPivotState.Failed);
				TestObjectCreator.CreateEInvoicingTransactionPivot(invoice3, status: EInvoicingPivotState.Failed);
				TestObjectCreator.CreateEInvoicingTransactionPivot(invoice4, status: EInvoicingPivotState.BatchedWithError);
				var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				pivot2.AIP_AIB = batch.PK;

				Factory.Save();

				var guiActionHelper1 = new EInvoicingGUIActionHelper(() => new List<TransactionHeader>() { invoice1, invoice2, invoice3, invoice4 });
				guiActionHelper1.ResetStatusToQueued(Env.Security.None);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text == @"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:
- 'FAL' - Fail and E-Reporting Government # is blank, or
- 'BER' - Batched with errors.

"));
			}
		}

		[ExpectNoExceptions]
		public void TestResetStatusToQueued_WhenCountry_ImplementBatchStrategy_Parameters()
		{
			var mockGlobalFactory = new Mock<IGlobalEInvoicingObjectFactory>();
			var mockBatchStrategy = new Mock<IBatchCreatorStrategy>();
			mockBatchStrategy.Setup(x => x.SupportsWaitForOriginalTransactionForAmending).Returns(false);
			mockGlobalFactory.Setup(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>())).Returns(mockBatchStrategy.Object);
			ObjectFactory.Substitute(mockGlobalFactory.Object);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var selectedTransactions = new List<TransactionHeader>() { invoice };
			var guiActionHelper = new EInvoicingGUIActionHelper(() => selectedTransactions);

			guiActionHelper.ResetStatusToQueued(Env.Security.None);
			mockGlobalFactory.Verify(x => x.GetCountryEInvoicingBatchCreatorStrategy("AU"), Times.Once);

			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Mexico);
			guiActionHelper = new EInvoicingGUIActionHelper(() => selectedTransactions);
			guiActionHelper.ResetStatusToQueued(Env.Security.None);
			mockGlobalFactory.Verify(x => x.GetCountryEInvoicingBatchCreatorStrategy("MX"), Times.Once);
		}

		[TestDate(2023, 01, 15, 12, 30, 00)]
		public void TestResetStatusToQueued_DelayTime_AllFailed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, status: EInvoicingPivotState.Sent);
				pivot.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-5);
				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice1, status: EInvoicingPivotState.Sent);
				pivot1.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-5);
				Factory.Save();

				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Sent, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, new ZDateTime(2023, 01, 15, 12, 25, 00), ZBool.False);
				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Sent, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, new ZDateTime(2023, 01, 15, 12, 25, 00), ZBool.False);

				var selectedTransactions = new List<TransactionHeader>() { invoice, invoice1 };
				var guiActionHelper = new EInvoicingGUIActionHelper(() => selectedTransactions);

				GlbStaff.CurrentUser.GS_LoginName = "CWSupport";
				guiActionHelper.ResetStatusToQueued(Env.Security.None);

				AssertMultilineASCIIEquals(@"The selected transactions cannot be re-queued. Re-queuing is allowed 30 minutes after the last transmission.

No eligible transactions found to be re-queued.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		[TestDate(2023, 01, 15, 12, 30, 00)]
		public void TestResetStatusToQueued_DelayTime_PartFailed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, status: EInvoicingPivotState.Sent);
				pivot.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-5);
				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice1, status: EInvoicingPivotState.Sent);
				pivot1.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-5);
				var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice2, status: EInvoicingPivotState.Sent);
				pivot2.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-31);
				Factory.Save();

				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Sent, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, new ZDateTime(2023, 01, 15, 12, 25, 00), ZBool.False);
				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Sent, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, new ZDateTime(2023, 01, 15, 12, 25, 00), ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Sent, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, new ZDateTime(2023, 01, 15, 11, 59, 00), ZBool.False);

				var selectedTransactions = new List<TransactionHeader>() { invoice, invoice1, invoice2 };
				var guiActionHelper = new EInvoicingGUIActionHelper(() => selectedTransactions);

				GlbStaff.CurrentUser.GS_LoginName = "CWSupport";
				guiActionHelper.ResetStatusToQueued(Env.Security.None);

				AssertMultilineASCIIEquals($@"Some of the selected transactions cannot be re-queued. Re-queuing is allowed 30 minutes after the last transmission.
The transactions are: {string.Join(", ", new List<string>() { invoice.AH_TransactionNum, invoice1.AH_TransactionNum }.OrderBy(x => x))}

Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasWarning);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		ZString RequeueSuccessMessage => "Transaction was successfully re-queued.";

		ZString RequeueErrorMessage => $@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).";

		ZString RequeueFullErrorMessage => $@"{RequeueErrorMessage}

No eligible transactions found to be re-queued.";

		ZString RequeueFullErrorMessageWithPossibleReasons(ZString message)
		{
			return $@"{RequeueErrorMessage}
{message}
No eligible transactions found to be re-queued.";
		}

		[ExpectNoExceptions]
		public void TestResetStatusToQueued_IsTransactionEnableToRequeue_Parameters()
		{
			var (electronicInvoicingAccountingObjectFactoryMock, electronicInvoicingRequeueStrategyMock, pivot) = SetupAndRunResetStatusToQueued(RequeueDecision.Requeue);

			electronicInvoicingAccountingObjectFactoryMock.Verify(x => x.GetElectronicInvoicingRequeueStrategy(), Times.Once);
			electronicInvoicingRequeueStrategyMock.Verify(x => x.IsTransactionEnableToRequeue(pivot.ParentTransactionHeader), Times.Once);
		}

		public void TestResetStatusToQueued_WhenIsTransactionEnableToRequeue_ReturnsRequeue()
		{
			SetupAndRunResetStatusToQueued(RequeueDecision.Requeue);

			AssertMultilineASCIIEquals(RequeueSuccessMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
		}

		public void TestResetStatusToQueued_WhenIsTransactionEnableToRequeue_ReturnsRejectCancelled()
		{
			var message = "Message: RejectCancelled";
			SetupAndRunResetStatusToQueued(RequeueDecision.RejectCancelled, new HashSet<ZString>() { message });

			AssertMultilineASCIIEquals(RequeueFullErrorMessageWithPossibleReasons(message), UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestResetStatusToQueued_ManyReasonsToExcludeTransaction()
		{
			var message1 = "Message 1: XXXX";
			var message2 = "Message 2: XXXX";

			SetupAndRunResetStatusToQueued(RequeueDecision.RejectCancelled, new HashSet<ZString>() { message1, message2 });

			AssertMultilineASCIIEquals(GetExpectedMessage(), UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

			ZString GetExpectedMessage()
			{
				return $@"{RequeueErrorMessage}
{message1}
{message2}
No eligible transactions found to be re-queued.";
			}
		}

		[ExpectNoExceptions]
		public void TestResetStatusToQueued_GetReasonsToExcludeTransaction_Parameters()
		{
			var (_, electronicInvoicingRequeueStrategyMock, _) = SetupAndRunResetStatusToQueued(RequeueDecision.RejectCancelled);

			electronicInvoicingRequeueStrategyMock.Verify(x => x.GetReasonsToExcludeTransaction(new HashSet<RequeueDecision>() { RequeueDecision.RejectCancelled }), Times.Once);
		}

		(Mock<IElectronicInvoicingAccountingObjectFactory> electronicInvoicingAccountingObjectFactoryMock, Mock<IElectronicInvoicingRequeueStrategy> electronicInvoicingRequeueStrategyMock, AccEInvoicingTransactionPivot pivot) SetupAndRunResetStatusToQueued(RequeueDecision requeueDecision, HashSet<ZString> messages = null)
		{
			var electronicInvoicingRequeueStrategyMock = new Mock<IElectronicInvoicingRequeueStrategy>();
			electronicInvoicingRequeueStrategyMock.Setup(x => x.IsTransactionEnableToRequeue(It.IsAny<TransactionHeader>())).Returns(requeueDecision);

			electronicInvoicingRequeueStrategyMock.Setup(x => x.GetReasonsToExcludeTransaction(It.IsAny<HashSet<RequeueDecision>>())).Returns(messages ?? new HashSet<ZString>());

			var electronicInvoicingAccountingObjectFactoryMock = new Mock<IElectronicInvoicingAccountingObjectFactory>();
			electronicInvoicingAccountingObjectFactoryMock.Setup(x => x.GetElectronicInvoicingRequeueStrategy()).Returns(electronicInvoicingRequeueStrategyMock.Object);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Sent);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 101, EInvoicingBatchState.Sent);
			Factory.Save();

			ObjectFactory.Substitute(electronicInvoicingAccountingObjectFactoryMock.Object);
			var selectedTransactions = new List<TransactionHeader>() { invoice };
			var guiActionHelper = new EInvoicingGUIActionHelper(() => selectedTransactions);
			guiActionHelper.ResetStatusToQueued(Env.Security.None);

			return (electronicInvoicingAccountingObjectFactoryMock, electronicInvoicingRequeueStrategyMock, pivot);
		}

		public void TestResetStatusToDelivered_AllTheTransactionsAreEligible()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.AUD, 1, TestObjectCreator.Debtor);
				var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("0002", TestObjectCreator.AUD, 1, TestObjectCreator.Debtor);

				var authData1 = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(invoice1);
				authData1.AHF_Number = "001";
				var authData2 = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(invoice2);
				authData2.AHF_Number = "002";

				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice1, status: EInvoicingPivotState.Failed);
				var batch1 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot1, 1, EInvoicingBatchState.Ready);
				var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice2, status: EInvoicingPivotState.BatchedWithError);
				var batch2 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot2, 2, EInvoicingBatchState.Discarded);
				Factory.Save();

				var selectedBusinessObjects = new BusinessObject[] { invoice1, invoice2 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				guiHelper.ResetStatusToDelivered();

				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(EInvoicingBatchState.Discarded, batch1.AIB_Status);
				AssertEquals(ZGuid.Empty, pivot1.AIP_AIB);
				AssertEquals(EInvoicingPivotState.Delivered, pivot1.AIP_Status);

				AssertEquals(EInvoicingBatchState.Discarded, batch2.AIB_Status);
				AssertEquals(ZGuid.Empty, pivot2.AIP_AIB);
				AssertEquals(EInvoicingPivotState.Delivered, pivot2.AIP_Status);
			}
		}

		public void TestResetStatusToDelivered_MixEligibleAndNotEligibleTransactions()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.AUD, 1, TestObjectCreator.Debtor);
				var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("0002", TestObjectCreator.AUD, 1, TestObjectCreator.Debtor);

				var authData1 = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(invoice1);
				authData1.AHF_Number = "001";

				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice1, status: EInvoicingPivotState.Failed);
				var batch1 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot1, 1, EInvoicingBatchState.Ready);
				var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice2, status: EInvoicingPivotState.Failed);
				var batch2 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot2, 2, EInvoicingBatchState.Ready);
				Factory.Save();

				var selectedBusinessObjects = new BusinessObject[] { invoice1, invoice2 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				guiHelper.ResetStatusToDelivered();

				var msg = @"Previously delivered (DLV) e-Reporting transactions will be re-queued to delivered if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched With Error";
				AssertEquals(msg, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(EInvoicingBatchState.Discarded, batch1.AIB_Status);
				AssertEquals(ZGuid.Empty, pivot1.AIP_AIB);
				AssertEquals(EInvoicingPivotState.Delivered, pivot1.AIP_Status);

				AssertEquals("Original batch status should be kept due to no Authorization Number", EInvoicingBatchState.Ready, batch2.AIB_Status);
				AssertEquals("Original batch number should be kept due to no Authorization Number", batch2.PK, pivot2.AIP_AIB);
				AssertEquals("Original pivot status should be kept due to no Authorization Number", EInvoicingPivotState.Failed, pivot2.AIP_Status);
			}
		}

		public void TestResetStatusToDelivered_NoEligibleTransaction()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("0001", TestObjectCreator.AUD, 1, TestObjectCreator.Debtor);
				var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("0002", TestObjectCreator.AUD, 1, TestObjectCreator.Debtor);

				var authData1 = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(invoice1);
				authData1.AHF_Number = "001";

				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice1, status: EInvoicingPivotState.Delivered);
				var batch1 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot1, 1, EInvoicingBatchState.Ready);
				var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice2, status: EInvoicingPivotState.Failed);
				var batch2 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot2, 2, EInvoicingBatchState.Sent);
				Factory.Save();

				var selectedBusinessObjects = new BusinessObject[] { invoice1, invoice2 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
				guiHelper.ResetStatusToDelivered();

				var msg = @"Previously delivered (DLV) e-Reporting transactions will be re-queued to delivered if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched With Error

No eligible transactions found to be re-queued";
				AssertEquals(msg, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Original batch status should be kept due to pivot status is DLV", EInvoicingBatchState.Ready, batch1.AIB_Status);
				AssertEquals("Original batch number should be kept due to pivot status is DLV", batch1.PK, pivot1.AIP_AIB);
				AssertEquals("Original pivot status should be kept due to pivot status is DLV", EInvoicingPivotState.Delivered, pivot1.AIP_Status);

				AssertEquals("Original batch status should be kept due to no Authorization Number", EInvoicingBatchState.Sent, batch2.AIB_Status);
				AssertEquals("Original batch number should be kept due to no Authorization Number", batch2.PK, pivot2.AIP_AIB);
				AssertEquals("Original pivot status should be kept due to no Authorization Number", EInvoicingPivotState.Failed, pivot2.AIP_Status);
			}
		}

		#endregion

		#region E-Invoicing Transaction Status Resetting - Concurrency Testing

		public class EInvoicingGUIActionHelperForConcurrency_ForTestOnly : EInvoicingGUIActionHelper
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
			public EInvoicingGUIActionHelperForConcurrency_ForTestOnly(Func<IEnumerable<TransactionHeader>> getSelectedTransactions) : base(getSelectedTransactions)
			{
			}

			protected override void ResetStatusToQueuedCore(IReadOnlyCollection<AccEInvoicingTransactionPivot> transactionsWithFailedStatus, IReadOnlyCollection<AccEInvoicingTransactionPivot> transactionPivotsToDiscard)
			{
				SimulateAnotherUserRequeuingAndServiceTaskRun(transactionsWithFailedStatus);
				//Current user try to requeue the transactions again
				base.ResetStatusToQueuedCore(transactionsWithFailedStatus, transactionPivotsToDiscard);
			}

			protected static void SimulateAnotherUserRequeuingAndServiceTaskRun(IReadOnlyCollection<AccEInvoicingTransactionPivot> transactionPivotsToRequeue)
			{
				//another user requeues transaction
				var anotherUserFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var pivotsInAnotherUserFactory = anotherUserFactory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.PK, transactionPivotsToRequeue.Select(x => x.PK)));
				foreach (var pivot in pivotsInAnotherUserFactory)
				{
					var batch = anotherUserFactory.Load<AccEInvoicingBatch>(pivot.AIP_AIB);
					batch.AIB_Status = EInvoicingBatchState.Discarded;
					pivot.AIP_AIB = ZGuid.Empty;
					pivot.AIP_Status = EInvoicingPivotState.Queued;
					pivot.AIP_ErrorDescription = ZString.Empty;
					pivot.AIP_IsNotifiedByEmail = false;
					pivot.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
					pivot.AIP_LastSentTimeUtc = ZDateTime.Empty;
				}
				anotherUserFactory.Save();

				//service task runs right after that
				var batchPk = Guid.Empty;
				var insertSQL = "INSERT INTO dbo.AccEInvoicingBatch (AIB_PK, AIB_GC, AIB_BatchNumber, AIB_SystemCreateTimeUtc, AIB_SystemCreateUser) " +
										"OUTPUT INSERTED.AIB_PK " +
										"VALUES (newid(), @companyPK, @batchNo, @createdTime, @createdUser)";

				using (var insertCommand = Db.Connection.Command(insertSQL)) // Stimulating service task SQL operation
				{
					insertCommand.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
					insertCommand.AddParameter("@batchNo", SqlDbType.Int, new Random().Next());
					insertCommand.AddParameter("@createdTime", SqlDbType.SmallDateTime, ZDateTime.UtcNow.ToDateTime());
					insertCommand.AddParameter("@createdUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
					using (var reader = insertCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							batchPk = reader.GetGuid(0);
						}
					}
				}

				var updateSQL =
					"UPDATE dbo.AccEInvoicingTransactionPivot SET AIP_AIB = @batchPK, AIP_Status = @pivotStatus, AIP_SystemLastEditTimeUtc = GETUTCDATE(), AIP_SystemLastEditUser = @SystemLastEditUser WHERE AIP_PK IN (SELECT value FROM @parentPKs)";
				using (var updateCommand = Db.Connection.Command(updateSQL)) // Stimulating service task SQL operation
				{
					updateCommand.AddParameter("@batchPK", SqlDbType.UniqueIdentifier, batchPk);
					updateCommand.AddParameter("@pivotStatus", SqlDbType.Char, EInvoicingPivotState.Batched);
					updateCommand.AddTableValuedParameter("@parentPKs", AccEInvoicingTransactionPivotSchema.PK, transactionPivotsToRequeue.Select(x => x.PK));
					updateCommand.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
					updateCommand.ExecuteNonQuery();
				}
			}
		}

		[TestDate(2006, 05, 10)]
		public void TestResetStatusToQueued_TransactionPivotStatusConcurrencySingle_BatchingServiceTaskRunningInBetweenUserOperations()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
				var invoice = Factory.New<ARInvoice>();
				invoice.FillWithValidTestData();
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				var batchPK = TestObjectCreator.CreateEInvoicingBatch();
				Factory.Save();

				var batch = Factory.Load<AccEInvoicingBatch>(batchPK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

				BusinessObject[] selectedBusinessObjects = { invoice };
				var guiHelper = new EInvoicingGUIActionHelperForConcurrency_ForTestOnly(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (var form = new Form())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("While you were working, another user has modified this transaction. Please try again.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					var newBatchPK = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, SQLComparisonOperator.NotEqual, batchPK)).PK;
					AssertPivotDetails(invoice.PK, EInvoicingPivotState.Batched, ZString.Empty, newBatchPK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch.AIB_Status);
				}
			}
		}

		[TestDate(2006, 5, 10)]
		public void TestResetStatusToQueued_TransactionPivotStatusConcurrencyMultiple_BatchingServiceTaskRunningInBetweenUserOperations()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

				var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
				var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				var creditNote1 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice1);
				var creditNote2 = TestObjectCreator.CreateARCreditNoteReverseTransaction(invoice2);
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", invoice2.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote1.IsEligibleToCreateEInvoicingTransactionPivot);
				Assert("Precondition: eInvoicing enabled", creditNote2.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);

				var batchPK = TestObjectCreator.CreateEInvoicingBatch();
				Factory.Save();

				var batch = Factory.Load<AccEInvoicingBatch>(batchPK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice1.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice2.PK, EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote1.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote2.PK, EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);

				AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(invoice2.PK, EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);
				AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.BatchedWithError, "This transaction is batched with errors", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True, EInvoicingPivotActionType.Cancel);

				BusinessObject[] selectedBusinessObjects = { invoice1, invoice2, creditNote1, creditNote2 };
				var guiHelper = new EInvoicingGUIActionHelperForConcurrency_ForTestOnly(() => selectedBusinessObjects.OfType<TransactionHeader>());
				using (ZForm form = new ZForm())
				{
					form.Show();

					var securityCheckPoint = Env.Security.None;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					guiHelper.ResetStatusToQueued(securityCheckPoint);
					AssertEquals("While you were working, another user has modified these transactions. Please refresh the grid and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					var newBatchPK = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, SQLComparisonOperator.NotEqual, batchPK)).PK;
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Batched, ZString.Empty, newBatchPK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Batched, ZString.Empty, newBatchPK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Batched, ZString.Empty, newBatchPK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Batched, ZString.Empty, newBatchPK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False, EInvoicingPivotActionType.Cancel);
					AssertEquals("Postcondition: batch remains sent as transaction was not requeued", EInvoicingBatchState.Sent, batch.AIB_Status);
				}
			}
		}

		#endregion

		public static AccEInvoicingTransactionPivot AssertPivotDetails(ZGuid parentPk, ZString expectedStatus, ZString expectedErrorDescription, ZGuid expectedBatchPK, ZDateTime expectedLastResponseReceived, ZDateTime expectedSentTime, ZBool expectedIsNotifiedByEmail, string actionType = EInvoicingPivotActionType.Submit)
		{
			var pivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, parentPk).AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType));
			AssertNotNull(pivot);
			AssertEquals(expectedStatus, pivot.AIP_Status);
			AssertEquals(expectedErrorDescription, pivot.AIP_ErrorDescription);
			AssertEquals(expectedBatchPK, pivot.AIP_AIB);
			AssertEquals(expectedLastResponseReceived, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals(expectedSentTime, pivot.AIP_LastSentTimeUtc);
			AssertEquals(expectedIsNotifiedByEmail, pivot.AIP_IsNotifiedByEmail);
			return pivot;
		}

		public static AccEInvoicingTransactionPivot AssertPivotDetails(ZGuid parentPk, ZString expectedStatus)
		{
			return AssertPivotDetails(parentPk, expectedStatus, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
		}

		(BooleanRegistryItem, DateTimeRegistryItem, SecurityCheckpoint) GetRegistryItemsForEInvoicing(string ledger)
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			BooleanRegistryItem regFunction = null;
			DateTimeRegistryItem regDate = null;
			var security = Env.Security.None;
			if (ledger == LedgerTypes.AccountsReceivable)
			{
				regFunction = registry.EnableEInvoicingFunctionality;
				regDate = registry.EReportingComplianceDate;
				security = Env.Security.ReceivablesSetTransactionStatusToAwait;
			}
			else if (ledger == LedgerTypes.AccountsPayable)
			{
				regFunction = registry.EnableEInvoicingFunctionalityForPayables;
				regDate = registry.EReportingComplianceDateForPayables;
				security = Env.Security.PayablesSetTransactionStatusToAwait;
			}
			return (regFunction, regDate, security);
		}

		#region E-Invoicing Transaction Status Await Review

		[TestDate(2021, 11, 11)]
		public void TestSetStatusToAwait_SingleTransaction()
		{
			string securityMessageText = "You do not have appropriate security rights to Review transactions.";
			string okMessageText = "Transaction status was successfully set to Awaiting Review.";
			string errorMessageText = @"You can only Review transactions where the E-Reporting status is 'PEN' - Pending.
No transactions will be reviewed.";
			var infoMsg = $"Awaiting Review status set by user CWSupport on day {ZDateTime.Today}. To send the invoice, click 'Authorize and Send''";

			AssertSingleTransaction(okMessageText, errorMessageText, infoMsg, securityMessageText, EInvoicingPivotState.AwaitingReview);
		}

		[TestDate(2021, 11, 11)]
		public void TestSetStatusToAwait_ManyTransactions()
		{
			string okMessageText = "Transactions status were successfully set to Awaiting Review.";
			string warningMessageText = @"You can only Review transactions where the E-Reporting status is 'PEN' - Pending.
Transactions status were successfully set to Awaiting Review.";
			var infoMsg = $"Awaiting Review status set by user CWSupport on day {ZDateTime.Today}. To send the invoice, click 'Authorize and Send''";

			AssertManyTransactions(okMessageText, warningMessageText, infoMsg, EInvoicingPivotState.AwaitingReview);
		}

		[TestDate(2021, 11, 11)]
		public void TestAuthorizeAndSend_SingleTransaction()
		{
			string securityMessageText = "You do not have appropriate security rights to Authorize and Send transactions.";
			string okMessageText = "Transaction is now queued for sending.";
			string errorMessageText = @"You can only Authorize and Send transactions where the E-Reporting status is 'PEN - Pending' or 'AWA - Awaiting Review'.
No transactions will be set.";

			AssertSingleTransaction(okMessageText, errorMessageText, "", securityMessageText, EInvoicingPivotState.Queued);
		}

		[TestDate(2021, 11, 11)]
		public void TestAuthorizeAndSend_ManyTransactions()
		{
			string okMessageText = "Transactions are now queued for sending.";
			string warningMessageText = "Transactions are now queued for sending.";

			AssertManyTransactions(okMessageText, warningMessageText, "", EInvoicingPivotState.Queued);
		}

		void AssertManyTransactions(string okMessageText, string warningMessageText, string infoMsg, string expectedPivotState)
		{
			bool isStatusToAwaitTest = expectedPivotState == EInvoicingPivotState.AwaitingReview;

			var currDate = ZDateTime.Now;
			var registry = AccountingMasterFilesRegistry.Instance;
			var currCompany = GlbCompany.CurrentCompany;
			var currCompGuid = currCompany.PK.ToGuid();

			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "Test";
			taxRate.AT_Type = AccTaxRate.Types.ReverseRated;
			Factory.Save();

			(var regFunctionality, var regComplianceDate, var securityCheckPoint) = GetRegistryItemsForEInvoicing(LedgerTypes.AccountsPayable);

			using (currCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (regFunctionality?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
			using (regComplianceDate?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, currDate.ToDateTime().AddDays(-10)))
			{
				foreach (bool notAllPending in new[] { false, true })
				{
					TestObjectCreator.CreateTestPeriods(currDate);

					var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001" + notAllPending, TestObjectCreator.EUR, 1, 10, 0, 10, 0);
					invoice1.Lines[0].AL_AT = taxRate.PK;
					var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "002" + notAllPending, TestObjectCreator.EUR, 1, 10, 0, 10, 0);
					invoice2.Lines[0].AL_AT = taxRate.PK;
					var creditNote1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "003" + notAllPending, TestObjectCreator.EUR, 1, 4, 0, 4, 0);
					creditNote1.Lines[0].AL_AT = taxRate.PK;
					var creditNote2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "004" + notAllPending, TestObjectCreator.EUR, 1, 4, 0, 4, 0);
					creditNote2.Lines[0].AL_AT = taxRate.PK;
					Factory.Save();

					Assert("Precondition: eInvoicing enabled", invoice1.IsEligibleToCreateEInvoicingTransactionPivot);
					Assert("Precondition: eInvoicing enabled", invoice2.IsEligibleToCreateEInvoicingTransactionPivot);
					Assert("Precondition: eInvoicing enabled", creditNote1.IsEligibleToCreateEInvoicingTransactionPivot);
					Assert("Precondition: eInvoicing enabled", creditNote2.IsEligibleToCreateEInvoicingTransactionPivot);
					AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Pending);
					AssertPivotDetails(invoice2.PK, EInvoicingPivotState.Pending);
					AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Pending);
					AssertPivotDetails(creditNote2.PK, EInvoicingPivotState.Pending);

					if (notAllPending)
					{
						TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
						TestObjectCreator.UpdateEInvoicingTransactionPivot(creditNote1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

						AssertPivotDetails(invoice1.PK, EInvoicingPivotState.Queued);
						AssertPivotDetails(creditNote1.PK, EInvoicingPivotState.Queued);
					}

					if (!isStatusToAwaitTest)
					{
						TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice2.PK, EInvoicingPivotState.AwaitingReview, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
						AssertPivotDetails(invoice2.PK, EInvoicingPivotState.AwaitingReview);
					}

					BusinessObject[] selectedBusinessObjects = { invoice1, invoice2, creditNote1, creditNote2 };
					var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
					using (var form = new ZForm())
					{
						form.Show();
						var testNotif = UnitTestUserNotification.Instance;

						testNotif.ClearMessagesAndAnswers();
						if (isStatusToAwaitTest)
						{
							guiHelper.SetStatusToAwait(securityCheckPoint);
						}
						else
						{
							guiHelper.SetStatusToQueued(securityCheckPoint);
						}

						if (notAllPending)
						{
							AssertMultilineASCIIEquals(warningMessageText, testNotif.LastMessage.Text);
							if (isStatusToAwaitTest)
							{
								Assert(testNotif.LastMessage.WasWarning);
							}
							else
							{
								Assert("Warning message is displayed when it is not possible to review all selected transactions", testNotif.PreviousMessages.ContainsMessageContainingThisText("You can only Authorize and Send"));
							}
						}
						else
						{
							AssertEquals(okMessageText, testNotif.LastMessage.Text);
							Assert(testNotif.LastMessage.WasInformation);
							Assert("No warning message is displayed when all selected transactions can be reviewed", !testNotif.PreviousMessages.ContainsMessageContainingThisText("You can only Review transactions"));
						}

						AssertPivotDetails(invoice1.PK, notAllPending ? EInvoicingPivotState.Queued : expectedPivotState, notAllPending ? string.Empty : infoMsg, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
						AssertPivotDetails(invoice2.PK, expectedPivotState, infoMsg, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
						AssertPivotDetails(creditNote1.PK, notAllPending ? EInvoicingPivotState.Queued : expectedPivotState, notAllPending ? string.Empty : infoMsg, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
						AssertPivotDetails(creditNote2.PK, expectedPivotState, infoMsg, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					}
				}
			}
		}

		void AssertSingleTransaction(string okMessageText, string errorMessageText, string infoMsg, string securityMessageText, string expectedPivotState)
		{
			var currDate = ZDateTime.Now;
			var currCompany = GlbCompany.CurrentCompany;
			var currCompGuid = currCompany.PK.ToGuid();
			int transNum = 0;
			AccTaxRate taxRate = null;

			using (currCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(currDate);

				taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "Test";
				taxRate.AT_Type = AccTaxRate.Types.ReverseRated;
				Factory.Save();

				var type = typeof(APInvoice);
				ChangeStatus_Core(type, false);
				ChangeStatus_Core(type, true);

				type = typeof(APCreditNote);
				ChangeStatus_Core(type, false);
				ChangeStatus_Core(type, true);

				type = typeof(APAdjustmentNote);
				ChangeStatus_Core(type, false);
				ChangeStatus_Core(type, true);
			}

			void ChangeStatus_Core(Type invType, bool wrongStatus)
			{
				var invoice = TestObjectCreator.CreateInvoiceWithLine(invType, (++transNum).ToString(), TestObjectCreator.EUR, 1, 10, 0, 10, 0);

				(var regFunctionality, var regComplianceDate, var securityCheckPoint) = GetRegistryItemsForEInvoicing(invoice.AH_Ledger);
				using (regFunctionality?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
				using (regComplianceDate?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, currDate.ToDateTime().AddDays(-10)))
				{
					invoice.Lines[0].AL_AT = taxRate.PK;
					Factory.Save();

					Assert("Precondition: eInvoicing enabled", invoice.IsEligibleToCreateEInvoicingTransactionPivot);
					AssertPivotDetails(invoice.PK, EInvoicingPivotState.Pending);

					BusinessObject[] selectedBusinessObjects = { invoice };
					var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects.OfType<TransactionHeader>());
					using (var form = new ZForm())
					{
						form.Show();
						var testNotif = UnitTestUserNotification.Instance;

						securityCheckPoint.IsAllowed = false;
						testNotif.ClearMessagesAndAnswers();
						SetStatusHelper();
						AssertEquals(securityMessageText, testNotif.LastMessage.Text);
						Assert(testNotif.LastMessage.WasError);

						AssertPivotDetails(invoice.PK, EInvoicingPivotState.Pending);

						securityCheckPoint.IsAllowed = true;
						testNotif.ClearMessagesAndAnswers();

						if (wrongStatus)
						{
							var batchPK = TestObjectCreator.CreateEInvoicingBatch();
							Factory.Save();

							var batch = Factory.Load<AccEInvoicingBatch>(batchPK);
							TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, batchPK, ZDateTime.Empty, ZDateTime.Empty, ZBool.True);
							AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, batchPK, ZDateTime.Empty, ZDateTime.Empty, ZBool.True);

							SetStatusHelper();
							AssertMultilineASCIIEquals(errorMessageText, testNotif.LastMessage.Text);
							Assert(testNotif.LastMessage.WasError);
							AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, batchPK, ZDateTime.Empty, ZDateTime.Empty, ZBool.True);
						}
						else
						{
							SetStatusHelper();
							AssertEquals(okMessageText, testNotif.LastMessage.Text);
							Assert(testNotif.LastMessage.WasInformation);
							AssertPivotDetails(invoice.PK, expectedPivotState, infoMsg, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
						}

						void SetStatusHelper()
						{
							if (expectedPivotState == EInvoicingPivotState.AwaitingReview)
							{
								guiHelper.SetStatusToAwait(securityCheckPoint);
							}
							else
							{
								guiHelper.SetStatusToQueued(securityCheckPoint);
							}
						}
					}
				}
			}
		}

		#endregion

		#region Queue invoice for transmission

		InvoicingBase Invoice;
		InvoicingBase Invoice1;
		InvoicingBase Invoice2;
		InvoicingBase Invoice3;

		void SetupForQueueInvoiceForTransmission()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
			Invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", TestObjectCreator.CNY, 1m, 100m, 6m, 100m, 6m);
			Invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001001", TestObjectCreator.CNY, 1m, 100m, 6m, 100m, 6m);
			Invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001002", TestObjectCreator.CNY, 1m, 100m, 6m, 100m, 6m);
			Invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001003", TestObjectCreator.CNY, 1m, 100m, 6m, 100m, 6m);
			Factory.Save();
		}

		public void TestQueueInvoiceForTransmission_Security()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			using (var form = new ZForm())
			{
				form.Show();

				SetupForQueueInvoiceForTransmission();

				var selectedBusinessObjects = new List<InvoicingBase>() { Invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects);

				var securityCheckPoint = Env.Security.ReceivablesQueueInvoiceForTransmission;
				securityCheckPoint.IsAllowed = false;
				guiHelper.QueueInvoiceForTransmission(securityCheckPoint);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> Queue Invoice for Transmission", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				securityCheckPoint.IsAllowed = true;
				Assert("User has security right to queue invoice", securityCheckPoint.IsAllowed);
			}
		}

		public void TestQueueInvoiceForTransmission_SingleInvoiceIneligible()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			using (var form = new ZForm())
			{
				form.Show();

				SetupForQueueInvoiceForTransmission();

				var securityCheckPoint = Env.Security.ReceivablesQueueInvoiceForTransmission;
				securityCheckPoint.IsAllowed = true;

				var selectedBusinessObjects = new List<InvoicingBase>() { Invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects);

				guiHelper.QueueInvoiceForTransmission(securityCheckPoint);
				AssertEquals(@"The transaction was not queued, due to one of the following reasons:
* Transaction Header Branch Does not have a value recorded against 'E-Invoicing Credentials' under Accounting > E-Reporting and E-Invoicing Configurations > China registry.
* Transaction Type is not INV.
* Compliance Sub Type is blank.
* E-Reporting Status is not blank.
* Compliance Number/Date is not blank.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQueueInvoiceForTransmission_OneEligibleAndOneIneligible()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			using (var form = new ZForm())
			{
				form.Show();

				SetupForQueueInvoiceForTransmission();

				var securityCheckPoint = Env.Security.ReceivablesQueueInvoiceForTransmission;
				securityCheckPoint.IsAllowed = true;

				Invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;
				Factory.Save();

				var selectedBusinessObjects = new List<InvoicingBase>() { Invoice, Invoice1 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects);

				guiHelper.QueueInvoiceForTransmission(securityCheckPoint);
				AssertEquals(@"The following transactions were not queued:
00001001

due to one of the following reasons:
* Transaction Header Branch Does not have a value recorded against 'E-Invoicing Credentials' under Accounting > E-Reporting and E-Invoicing Configurations > China registry.
* Transaction Type is not INV.
* Compliance Sub Type is blank.
* E-Reporting Status is not blank.
* Compliance Number/Date is not blank.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQueueInvoiceForTransmission_MultipleInvoicesIneligible()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			using (var form = new ZForm())
			{
				form.Show();

				SetupForQueueInvoiceForTransmission();

				var securityCheckPoint = Env.Security.ReceivablesQueueInvoiceForTransmission;
				securityCheckPoint.IsAllowed = true;

				var selectedBusinessObjects = new List<InvoicingBase>() { Invoice, Invoice1, Invoice2, Invoice3 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects);

				guiHelper.QueueInvoiceForTransmission(securityCheckPoint);
				AssertEquals(@"The following transactions were not queued:
00001000, 00001001, 00001002, 00001003

due to one of the following reasons:
* Transaction Header Branch Does not have a value recorded against 'E-Invoicing Credentials' under Accounting > E-Reporting and E-Invoicing Configurations > China registry.
* Transaction Type is not INV.
* Compliance Sub Type is blank.
* E-Reporting Status is not blank.
* Compliance Number/Date is not blank.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQueueInvoiceForTransmission_SingleInvoiceEligible()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			using (var form = new ZForm())
			{
				form.Show();

				SetupForQueueInvoiceForTransmission();

				var securityCheckPoint = Env.Security.ReceivablesQueueInvoiceForTransmission;
				securityCheckPoint.IsAllowed = true;

				var selectedBusinessObjects = new List<InvoicingBase>() { Invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects);

				Invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;
				Factory.Save();

				guiHelper.QueueInvoiceForTransmission(securityCheckPoint);
				AssertEquals("Transaction(s) is queued for transmission.", UnitTestUserNotification.Instance.LastMessage.Text);

				var query = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, Invoice.PK);
				query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued);
				Assert(Factory.Exists(typeof(AccEInvoicingTransactionPivot), query));
			}
		}

		public void TestQueueInvoiceForTransmission_PartInvoicesEligible()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			using (var form = new ZForm())
			{
				form.Show();

				SetupForQueueInvoiceForTransmission();

				var securityCheckPoint = Env.Security.ReceivablesQueueInvoiceForTransmission;
				securityCheckPoint.IsAllowed = true;

				var selectedBusinessObjects = new List<InvoicingBase>() { Invoice, Invoice1, Invoice2, Invoice3 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects);

				Invoice1.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				guiHelper.QueueInvoiceForTransmission(securityCheckPoint);
				AssertEquals(@"The following transactions were not queued:
00001000, 00001002, 00001003

due to one of the following reasons:
* Transaction Header Branch Does not have a value recorded against 'E-Invoicing Credentials' under Accounting > E-Reporting and E-Invoicing Configurations > China registry.
* Transaction Type is not INV.
* Compliance Sub Type is blank.
* E-Reporting Status is not blank.
* Compliance Number/Date is not blank.", UnitTestUserNotification.Instance.LastMessage.Text);

				var query = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, Invoice1.PK);
				query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Queued);
				Assert(Factory.Exists(typeof(AccEInvoicingTransactionPivot), query));
			}
		}

		public void TestQueueInvoiceForTransmission_Ineligible_Queued()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			using (var form = new ZForm())
			{
				form.Show();

				SetupForQueueInvoiceForTransmission();
				TestObjectCreator.CreateEInvoicingTransactionPivot(Invoice);
				Factory.Save();

				var securityCheckPoint = Env.Security.ReceivablesQueueInvoiceForTransmission;
				securityCheckPoint.IsAllowed = true;

				var selectedBusinessObjects = new List<InvoicingBase>() { Invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects);

				guiHelper.QueueInvoiceForTransmission(securityCheckPoint);
				AssertEquals(@"The transaction was not queued, due to one of the following reasons:
* Transaction Header Branch Does not have a value recorded against 'E-Invoicing Credentials' under Accounting > E-Reporting and E-Invoicing Configurations > China registry.
* Transaction Type is not INV.
* Compliance Sub Type is blank.
* E-Reporting Status is not blank.
* Compliance Number/Date is not blank.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQueueInvoiceForTransmission_Ineligible_HasComplianceFields()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			using (var form = new ZForm())
			{
				form.Show();

				SetupForQueueInvoiceForTransmission();
				Invoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;
				Invoice.AH_TransactionReference = "123";
				Factory.Save();

				var securityCheckPoint = Env.Security.ReceivablesQueueInvoiceForTransmission;
				securityCheckPoint.IsAllowed = true;

				var selectedBusinessObjects = new List<InvoicingBase>() { Invoice };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects);

				guiHelper.QueueInvoiceForTransmission(securityCheckPoint);
				AssertEquals(@"The transaction was not queued, due to one of the following reasons:
* Transaction Header Branch Does not have a value recorded against 'E-Invoicing Credentials' under Accounting > E-Reporting and E-Invoicing Configurations > China registry.
* Transaction Type is not INV.
* Compliance Sub Type is blank.
* E-Reporting Status is not blank.
* Compliance Number/Date is not blank.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQueueInvoiceForTransmission_Ineligible_CreditNote()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			using (var form = new ZForm())
			{
				form.Show();

				var creditNote = TestObjectCreator.CreateARCreditNoteWithLine("00001004", TestObjectCreator.Debtor, TestObjectCreator.CNY, 1m, "test", null, TestObjectCreator.FRT, 100m, ZDateTime.Today, false);
				Factory.Save();

				var securityCheckPoint = Env.Security.ReceivablesQueueInvoiceForTransmission;
				securityCheckPoint.IsAllowed = true;

				var selectedBusinessObjects = new List<InvoicingBase>() { creditNote };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects);

				guiHelper.QueueInvoiceForTransmission(securityCheckPoint);
				AssertEquals(@"The transaction was not queued, due to one of the following reasons:
* Transaction Header Branch Does not have a value recorded against 'E-Invoicing Credentials' under Accounting > E-Reporting and E-Invoicing Configurations > China registry.
* Transaction Type is not INV.
* Compliance Sub Type is blank.
* E-Reporting Status is not blank.
* Compliance Number/Date is not blank.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQueueInvoiceForTransmission_HasNotQueueChargeCode()
		{
			var settings = new GenericChargeConfigurationCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty), Factory);
			var setting = settings.AddNew();
			setting.ChargePK = TestObjectCreator.FRT.PK;

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"))
			using (AccountingConfigurationRegistry.Instance.DoNotQueueInvoicesContainingSpecificChargesForTransmission.SetTemporaryValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, settings))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			using (var form = new ZForm())
			{
				form.Show();

				var invoice4 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", TestObjectCreator.CNY, 1m, 100m, 6m, 100m, 6m);
				invoice4.Lines[0].AL_AC = TestObjectCreator.FRT.PK;
				Factory.Save();

				var securityCheckPoint = Env.Security.ReceivablesQueueInvoiceForTransmission;
				securityCheckPoint.IsAllowed = true;

				var selectedBusinessObjects = new List<InvoicingBase>() { invoice4 };
				var guiHelper = new EInvoicingGUIActionHelper(() => selectedBusinessObjects);

				guiHelper.QueueInvoiceForTransmission(securityCheckPoint);
				AssertEquals(@"The transaction was not queued, due to one of the following reasons:
* Transaction Header Branch Does not have a value recorded against 'E-Invoicing Credentials' under Accounting > E-Reporting and E-Invoicing Configurations > China registry.
* Transaction Type is not INV.
* Compliance Sub Type is blank.
* E-Reporting Status is not blank.
* Compliance Number/Date is not blank.
* Contains charges listed in ‘Do Not Queue Invoices Containing Specific Charges For Transmission’ registry.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion
	}
}
