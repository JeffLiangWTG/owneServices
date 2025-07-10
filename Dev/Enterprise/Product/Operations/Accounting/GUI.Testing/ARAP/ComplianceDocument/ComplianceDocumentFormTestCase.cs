using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.GUI.AccountingZForm;
using static Enterprise.Core.Constants;
using static Enterprise.ZArchitecture.GUI.ZForm;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	public abstract class ComplianceDocumentFormTestCase : ZFormBasherTest
	{
		#region EInvoicing test cases

		public void TestResetStatusToQueuedMenuItemIsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn()
		{
			Assert("Registry is off by default", !AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceDocument = GetComplianceDocumentWithValidTestData();
				Factory.Save();
				using (var testForm1 = GetFormByComplianceDocument(complianceDocument))
				{
					testForm1.Show();
					AssertNull("'Reset Status to Queued' Menu Item should not be available when 'Enable E-Reporting Functionality' regsitry is off", testForm1.ResetStatusToQueuedMenuItem);

					using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (var testForm2 = GetFormByComplianceDocument(complianceDocument))
					{
						testForm2.Show();
						if (complianceDocument.IsEligibleToCreateEInvoicingTransactionPivot)
						{
							AssertNotNull("'Reset Status to Queued' Menu Item should be available when 'Enable E-Reporting Functionality' regsitry is on", testForm2.ResetStatusToQueuedMenuItem);
						}
						else
						{
							AssertNull("Reset Status to Queued Menu Item should not exist for compliance documents not eligible for E-Reporting.", testForm1.ResetStatusToQueuedMenuItem);
						}
					}
				}
			}
		}

		public void TestResetStatusToQueuedMenuItemIsOnlyAvailableOnTransactionsEligibleForEReportingAndAlreadyInDB()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var complianceDocument = GetComplianceDocumentWithValidTestData();
				using (var testForm1 = new ComplianceDocumentForm(complianceDocument))
				{
					testForm1.Show();
					if (complianceDocument.IsEligibleToCreateEInvoicingTransactionPivot)
					{
						AssertNull("Reset Status to Queued Menu Item should not exist for compliance documents eligible for E-Reporting but not in DB.", testForm1.ResetStatusToQueuedMenuItem);
						Factory.Save();
						using (var testForm2 = new ComplianceDocumentForm(complianceDocument))
						{
							testForm2.Show();
							AssertNotNull("Reset Status to Queued Menu Item should exist for compliance documents eligible for E-Reporting and in DB.", testForm2.ResetStatusToQueuedMenuItem);
						}
					}
					else
					{
						AssertNull("Reset Status to Queued Menu Item should not exist for compliance documents not eligible for E-Reporting.", testForm1.ResetStatusToQueuedMenuItem);
					}
				}
			}
		}

		public void TestResetStatusToQueuedMenuItemNotThrowExceptionWhenPivotDoesNotExit()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceDocument = GetComplianceDocumentWithValidTestData();
				complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.NumberSet;
				complianceDocument.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI;
				complianceDocument.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
				complianceDocument.ADH_OH_Organisation = TestObjectCreator.AALSHI.PK;
				Factory.Save();

				if (complianceDocument.IsEligibleToCreateEInvoicingTransactionPivot)
				{
					using (var testForm = new ComplianceDocumentForm(complianceDocument))
					{
						testForm.Show();
						var resetStatusToQueuedMenuItem = testForm.ResetStatusToQueuedMenuItem;
						AssertNotNull("Reset Status to Queued Menu Item should exist.", resetStatusToQueuedMenuItem);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						AssertNoExceptionThrown(resetStatusToQueuedMenuItem.PerformClick);
						Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestResetStatusToQueuedMenuItem_WhenComplianceDocumentHasErros_BERStatus()
		{
			ResetStatusToQueuedMenuItem_WhenComplianceDocumenthHasErrosCore(EInvoicingPivotState.BatchedWithError, "This compliance document is batched with errors");
		}

		public void TestResetStatusToQueuedMenuItem_WhenComplianceDocumentHasErros_FALStatus()
		{
			ResetStatusToQueuedMenuItem_WhenComplianceDocumenthHasErrosCore(EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site");
		}

		public void TestResetStatusToQueuedMenuItem_WhenComplianceDocumentHasErros_BCHStatus()
		{
			ResetStatusToQueuedMenuItem_WhenComplianceDocumenthHasErrosCore(EInvoicingPivotState.Batched, "The batch of this compliance document was discarded");
		}

		public void TestResetStatusToQueuedMenuItem_WhenComplianceDocumentDoesNotHaveErrors()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceDocument = CreateComplianceDocument();

				if (complianceDocument.IsEligibleToCreateEInvoicingTransactionPivot)
				{
					AssertPivotDetails(complianceDocument.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

					using (var testForm = new ComplianceDocumentForm(complianceDocument))
					{
						testForm.Show();
						var resetStatusToQueuedMenuItem = testForm.ResetStatusToQueuedMenuItem;
						AssertNotNull("Reset Status to Queued Menu Item should exist.", resetStatusToQueuedMenuItem);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						resetStatusToQueuedMenuItem.PerformClick();
						AssertMultilineASCIIEquals(@"You can only reset compliance documents where E-Reporting pivot status is 'FAL' - Fail or 'BER' - Batched with errors or 'BCH' - Batched and batch status is 'DCD' - Discarded.", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

						AssertPivotDetails(complianceDocument.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					}
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestResetStatusToQueuedMenuItem_TransactionPivotStatusConcurrency_WithReloadingComplianceDocumentInNewFactory()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceDocument = CreateComplianceDocument();

				if (complianceDocument.IsEligibleToCreateEInvoicingTransactionPivot)
				{
					AssertPivotDetails(complianceDocument.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					var batchPK = CreateAccEInvoicingBatch();
					UpdatePivot(complianceDocument.PK, EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
					AssertPivotDetails(complianceDocument.PK, EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

					using (var user1Form = new ComplianceDocumentForm(complianceDocument))
					{
						user1Form.Show();
						var resetStatusToQueuedMenuItemInUser1Form = user1Form.ResetStatusToQueuedMenuItem;
						AssertNotNull("Reset Status to Queued Menu Item should exist.", resetStatusToQueuedMenuItemInUser1Form);

						using (var user2Form = new ComplianceDocumentForm(complianceDocument))
						{
							user2Form.Show();
							var resetStatusToQueuedMenuItemInUser2Form = user2Form.ResetStatusToQueuedMenuItem;
							AssertNotNull("Reset Status to Queued Menu Item should exist.", resetStatusToQueuedMenuItemInUser2Form);

							UpdateBatch(batchPK);

							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
							resetStatusToQueuedMenuItemInUser2Form.PerformClick();
							AssertEquals("Compliance Document was successfully reset.", UnitTestUserNotification.Instance.LastMessage.Text);
							Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
							AssertPivotDetails(complianceDocument.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
						}

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						resetStatusToQueuedMenuItemInUser1Form.PerformClick();
						AssertMultilineASCIIEquals(@"You can only reset compliance documents where E-Reporting pivot status is 'FAL' - Fail or 'BER' - Batched with errors or 'BCH' - Batched and batch status is 'DCD' - Discarded.", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertPivotDetails(complianceDocument.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					}
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestResetStatusToQueuedMenuItem_TransactionPivotStatusConcurrency_BatchingServiceTaskRunningInBetweenUserOperations()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceDocument = CreateComplianceDocument();

				if (complianceDocument.IsEligibleToCreateEInvoicingTransactionPivot)
				{
					AssertPivotDetails(complianceDocument.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					var batchPK = CreateAccEInvoicingBatch();
					UpdatePivot(complianceDocument.PK, EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
					AssertPivotDetails(complianceDocument.PK, EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

					using (var testForm = new ComplianceDocumentFormForConcurrencyTest(complianceDocument))
					{
						testForm.Show();
						var resetStatusToQueuedMenuItem = testForm.ResetStatusToQueuedMenuItem;
						AssertNotNull("Reset Status to Queued Menu Item should exist.", resetStatusToQueuedMenuItem);

						UpdateBatch(batchPK);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						resetStatusToQueuedMenuItem.PerformClick();
						AssertEquals("While you were working, another user has modified this compliance document. Please try again.", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

						var newBatchPK = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, SQLComparisonOperator.NotEqual, batchPK)).PK;
						AssertPivotDetails(complianceDocument.PK, EInvoicingPivotState.Batched, ZString.Empty, newBatchPK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					}
				}
				else
				{
					Assert(true);
				}
			}
		}

		void ResetStatusToQueuedMenuItem_WhenComplianceDocumenthHasErrosCore(ZString status, ZString errorMessage)
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceDocument = CreateComplianceDocument();

				if (complianceDocument.IsEligibleToCreateEInvoicingTransactionPivot)
				{
					AssertPivotDetails(complianceDocument.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					var batchPK = CreateAccEInvoicingBatch();
					UpdatePivot(complianceDocument.PK, status, errorMessage, batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
					AssertPivotDetails(complianceDocument.PK, status, errorMessage, batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

					using (var testForm = new ComplianceDocumentForm(complianceDocument))
					{
						testForm.Show();
						var resetStatusToQueuedMenuItem = testForm.ResetStatusToQueuedMenuItem;
						AssertNotNull("Reset Status to Queued Menu Item should exist.", resetStatusToQueuedMenuItem);

						if (status == EInvoicingPivotState.Batched)
						{
							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
							resetStatusToQueuedMenuItem.PerformClick();
							AssertEquals("You can only reset compliance documents where E-Reporting pivot status is 'FAL' - Fail or 'BER' - Batched with errors or 'BCH' - Batched and batch status is 'DCD' - Discarded.", UnitTestUserNotification.Instance.LastMessage.Text);
							Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

							UpdateBatch(batchPK);
						}

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						resetStatusToQueuedMenuItem.PerformClick();
						AssertEquals("Compliance Document was successfully reset.", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);

						AssertPivotDetails(complianceDocument.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					}
				}
				else
				{
					Assert(true);
				}
			}
		}

		void AssertPivotDetails(ZGuid parentPk, ZString expectedStatus, ZString expectedErrorDescription, ZGuid expectedBatchPK, ZDateTime expectedLastResponseReceived, ZDateTime expectedSentTime, ZBool expectedIsNotifiedByEmail)
		{
			var pivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, parentPk));
			AssertNotNull(pivot);
			AssertEquals(expectedStatus, pivot.AIP_Status);
			AssertEquals(expectedErrorDescription, pivot.AIP_ErrorDescription);
			AssertEquals(expectedBatchPK, pivot.AIP_AIB);
			AssertEquals(expectedLastResponseReceived, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals(expectedSentTime, pivot.AIP_LastSentTimeUtc);
			AssertEquals(expectedIsNotifiedByEmail, pivot.AIP_IsNotifiedByEmail);
		}

		ZGuid CreateAccEInvoicingBatch()
		{
			var factory = new BusinessObjectFactory();
			var batch = factory.New<AccEInvoicingBatch>();
			batch.AIB_Status = EInvoicingBatchState.Ready;
			batch.AIB_GC = GlbCompany.CurrentCompany.PK;
			batch.AIB_BatchNumber = new Random().Next();
			batch.AIB_SystemCreateTimeUtc = ZDateTime.Now.ToDateTime();
			batch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code.ToString();
			factory.Save();
			return batch.PK;
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;

		AccComplianceDocumentHeader CreateComplianceDocument()
		{
			var complianceDocument = GetComplianceDocumentWithValidTestData();
			complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.NumberSet;
			complianceDocument.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			complianceDocument.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceDocument.ADH_OH_Organisation = TestObjectCreator.AALSHI.PK;
			Factory.Save();
			return complianceDocument;
		}

		void UpdateBatch(ZGuid batchPk)
		{
			var factory = new BusinessObjectFactory();
			var batch = factory.Load<AccEInvoicingBatch>(batchPk);
			batch.AIB_Status = EInvoicingBatchState.Discarded;
			factory.Save();
		}

		void UpdatePivot(ZGuid parentPk, ZString errorStatus, ZString errorDescription, ZGuid batchPK, ZDateTime lastResponseReceived, ZDateTime sentTime, ZBool isNotifiedByEmail)
		{
			var factory = new BusinessObjectFactory();
			var pivot = factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, parentPk));
			AssertNotNull(pivot);
			pivot.AIP_AIB = batchPK;
			pivot.AIP_Status = errorStatus;
			pivot.AIP_ErrorDescription = errorDescription;
			pivot.AIP_LastResponseReceivedUtc = lastResponseReceived;
			pivot.AIP_LastSentTimeUtc = sentTime;
			pivot.AIP_IsNotifiedByEmail = isNotifiedByEmail;
			factory.Save();
		}

		class ComplianceDocumentFormForConcurrencyTest : ComplianceDocumentForm
		{
			public ComplianceDocumentFormForConcurrencyTest(AccComplianceDocumentHeader businessEntity) : base(businessEntity)
			{
			}

			protected override void ResetStatusToQueuedCore(AccEInvoicingTransactionPivot complianceDocumentPivot)
			{
				//another user requeues this transaction
				var anotherUserFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var pivot = anotherUserFactory.Load<AccEInvoicingTransactionPivot>(complianceDocumentPivot.PK);
				pivot.AIP_AIB = ZGuid.Empty;
				pivot.AIP_Status = EInvoicingPivotState.Queued;
				pivot.AIP_ErrorDescription = ZString.Empty;
				pivot.AIP_IsNotifiedByEmail = false;
				pivot.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
				pivot.AIP_LastSentTimeUtc = ZDateTime.Empty;
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
					"UPDATE dbo.AccEInvoicingTransactionPivot SET AIP_AIB = @batchPK, AIP_Status = @pivotStatus, AIP_SystemLastEditTimeUtc = GETUTCDATE(), AIP_SystemLastEditUser = @SystemLastEditUser WHERE AIP_PK = @pivotPK";
				using (var updateCommand = Db.Connection.Command(updateSQL)) // Stimulating service task SQL operation
				{
					updateCommand.AddParameter("@batchPK", SqlDbType.UniqueIdentifier, batchPk);
					updateCommand.AddParameter("@pivotStatus", SqlDbType.Char, EInvoicingPivotState.Batched);
					updateCommand.AddParameter("@pivotPK", SqlDbType.UniqueIdentifier, complianceDocumentPivot.PK.ToGuid());
					updateCommand.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
					updateCommand.ExecuteNonQuery();
				}

				//now current user try to requeue this compliance document
				base.ResetStatusToQueuedCore(complianceDocumentPivot);
			}
		}

		#endregion

		protected sealed override Form GetFormToBashCore()
		{
			return GetFormByComplianceDocument(GetComplianceDocumentWithValidTestData(false));
		}

		protected abstract AccComplianceDocumentHeader GetComplianceDocumentWithValidTestData(bool fillTestData = true);

		protected abstract ComplianceDocumentForm GetFormByComplianceDocument(AccComplianceDocumentHeader complianceDocumentHeader);

		protected abstract ComplianceDocumentForm GetFormByVoidComplianceDocument(AccComplianceDocumentHeader complianceDocumentHeader);

		[TestDate(2018, 7, 5)]
		public virtual void TestFormVerb()
		{
			var complianceDocument = GetComplianceDocumentWithValidTestData();
			complianceDocument.ADH_DocumentDate = new ZDate(2018, 7, 5);
			complianceDocument.ADH_ComplianceSubType = "TXI";
			complianceDocument.ADH_DocumentNumber = "001";
			complianceDocument.ADH_ReportingPeriod = 201807;
			complianceDocument.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (ZForm form = GetFormByComplianceDocument(complianceDocument))
			{
				Assert(!complianceDocument.IsFinalised);
				Assert(!complianceDocument.IsVoided);
				form.Show();
				Application.DoEvents();
				AssertEquals(FormVerbs_ForTestOnly.Edit, form.FormVerb);
			}

			complianceDocument.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.Finalised;
			Factory.Save();

			using (ZForm form = GetFormByComplianceDocument(complianceDocument))
			{
				Assert(complianceDocument.IsFinalised);
				form.Show();
				Application.DoEvents();
				AssertEquals(FormVerbs_ForTestOnly.View, form.FormVerb);
			}

			complianceDocument.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.Voided;
			Factory.Save();

			using (ComplianceDocumentForm form = GetFormByComplianceDocument(complianceDocument))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				Assert(complianceDocument.IsVoided);
				form.Show();
				Application.DoEvents();
				AssertEquals(FormVerbs_ForTestOnly.View, form.FormVerb);

				var specialVoidingPanel = form.ComplianceDocumentUserControl.GetField("specialVoidingPanel") as ZPanel;
				if (complianceDocument.ADH_Ledger == LedgerTypes.AccountsPayable)
				{
					AssertEquals(false, specialVoidingPanel.Visible);
				}
				else
				{
					AssertEquals(true, specialVoidingPanel.Visible);
				}
			}

			using (var form = GetFormByVoidComplianceDocument(complianceDocument))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Void", form.FormVerb);
			}
		}

		[TestDate(2018, 7, 5)]
		public virtual void TestShowPreDeleteDialogsForVoid()
		{
			var complianceDocument = GetComplianceDocumentWithValidTestData();
			complianceDocument.ADH_Description = "Description";
			complianceDocument.ADH_OH_Organisation = TestObjectCreator.AALSHI.PK;
			complianceDocument.ADH_DocumentDate = new ZDate(2018, 7, 5);
			complianceDocument.ADH_ComplianceSubType = "TXI";
			complianceDocument.ADH_DocumentNumber = "001";
			complianceDocument.ADH_ReportingPeriod = 201807;
			complianceDocument.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (var form = GetFormByVoidComplianceDocument(complianceDocument))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var result = form.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals(ContinueWithDelete.No, result);
				AssertEquals("The compliance document will be voided. Are you sure to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var form = GetFormByVoidComplianceDocument(complianceDocument))
			{
				Assert(!complianceDocument.IsFinalised);
				Assert(!complianceDocument.IsVoided);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();

				var result = form.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals(ContinueWithDelete.Yes, result);
				AssertEquals("The compliance document will be voided. Are you sure to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableFinalisedComplianceDocumentToBeSpecialVoided.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var form = GetFormByVoidComplianceDocument(complianceDocument))
			{
				complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Finalised;

				Assert(complianceDocument.IsFinalised);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				var result = form.ShowPreDeleteDialogs_ForTestOnly();

				AssertEquals(ContinueWithDelete.No, result);
				AssertEquals("This compliance document is already finalized.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableFinalisedComplianceDocumentToBeSpecialVoided.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = GetFormByVoidComplianceDocument(complianceDocument))
			{
				complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Finalised;
				complianceDocument.IsSpecialVoiding = false;

				Assert(complianceDocument.IsFinalised);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				var result = form.ShowPreDeleteDialogs_ForTestOnly();

				AssertEquals(ContinueWithDelete.No, result);
				AssertEquals("This compliance document record has been finalized, it can only be voided via 'Special Voiding'. Please take the 'Special Voiding' check box, state the voiding reason, and provide the approval number.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var form = GetFormByVoidComplianceDocument(complianceDocument))
			{
				complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;

				Assert(complianceDocument.IsVoided);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				var result = form.ShowPreDeleteDialogs_ForTestOnly();

				AssertEquals(ContinueWithDelete.No, result);
				AssertEquals("This compliance document is already voided.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestVoid()
		{
			var complianceDocument = GetComplianceDocumentWithValidTestData();
			complianceDocument.ADH_DocumentDate = new ZDate(2018, 7, 5);
			complianceDocument.ADH_ComplianceSubType = "TXI";
			complianceDocument.ADH_DocumentNumber = "001";
			complianceDocument.ADH_ReportingPeriod = 201807;
			complianceDocument.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (var form = GetFormByVoidComplianceDocument(complianceDocument))
			{
				Assert(!complianceDocument.IsVoided);
				form.Delete_ForTestOnly();
				Assert(complianceDocument.IsVoided);
				AssertEquals("Successfully void compliance document.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleSqlExceptionError()
		{
			var complianceDocument = GetComplianceDocumentWithValidTestData();
			Factory.Save();

			using (var form = new TestComplianceDocumentFormForSqlException(complianceDocument))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				Application.DoEvents();
				AssertEquals("Form diplay mode is Browse", ODisplayMode.Browse, form.DisplayMode);

				form.FireSaveButton();

				AssertEquals("Form becomes readonly", ODisplayMode.ReadOnly, form.DisplayMode);
			}
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestDeleteWithNoExceptionWhenComplianceDocumentDeleted()
		{
			var complianceDocument = GetComplianceDocumentWithValidTestData();
			Factory.Save();

			using (var form = GetFormByComplianceDocument(complianceDocument))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				Application.DoEvents();

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var newComplianceDocyment = newFactory.Load<AccComplianceDocumentHeader>(complianceDocument.PK);
				newComplianceDocyment.Delete();
				newFactory.Save();

				AssertNoExceptionThrown(() => form.OnPostButtonClick_ForTestOnly(null, null));
			}
		}
	}
}
