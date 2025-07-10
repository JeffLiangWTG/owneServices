using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(APInvoiceCharges))]
	class APInvoiceChargesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesAPInvoiceCharges()
		{
			var invoiceChargesForRequest = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", null);

			var localList = new List<string>
			{
				nameof(invoiceChargesForRequest.AH_LocalTotalAmount)
			};

			var tester = new DecimalPlacesAttributeTester(invoiceChargesForRequest);
			tester.CheckLocalCurrency(localList, nameof(invoiceChargesForRequest.LocalCurrencyDecimals));
		}

		public void TestValidateRequisitionStatus()
		{
			var aaa = new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = "AAA" };
			var paymentRequisitionStatuses = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { aaa };
			paymentRequisitionStatuses.SetDefaultCode("AAA", true);
			AccountingConfigurationRegistry.Instance.PaymentRequisitionStatuses.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, paymentRequisitionStatuses);

			var invoiceChargesForRequest = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", null);
			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			invoiceChargesForRequest.PostingGUIProvider.ShowApprovalFormToSetDescription(request);

			invoiceChargesForRequest.IsExcludedFromPosting = true;
			AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			invoiceChargesForRequest.RequisitionStatus = ZString.Empty;
			AssertNoErrors(invoiceChargesForRequest.RequisitionStatusInfo);

			invoiceChargesForRequest.IsExcludedFromPosting = false;
			AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			invoiceChargesForRequest.RequisitionStatus = ZString.Empty;
			AssertNoErrors(invoiceChargesForRequest.RequisitionStatusInfo);

			invoiceChargesForRequest.IsExcludedFromPosting = true;
			AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			invoiceChargesForRequest.RequisitionStatus = ZString.Empty;
			AssertNoErrors(invoiceChargesForRequest.RequisitionStatusInfo);

			invoiceChargesForRequest.IsExcludedFromPosting = false;
			AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			invoiceChargesForRequest.RequisitionStatus = ZString.Empty;
			AssertHasErrors(invoiceChargesForRequest.RequisitionStatusInfo);

			invoiceChargesForRequest.RequisitionStatus = "ABC";
			AssertHasErrors(invoiceChargesForRequest.RequisitionStatusInfo);

			invoiceChargesForRequest.RequisitionStatus = "AAA";
			AssertNoErrors(invoiceChargesForRequest.RequisitionStatusInfo);
		}

		public void TestValidateRequisitionDate()
		{
			var invoiceChargesForRequest = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", null);
			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			invoiceChargesForRequest.PostingGUIProvider.ShowApprovalFormToSetDescription(request);

			invoiceChargesForRequest.IsExcludedFromPosting = true;
			AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			invoiceChargesForRequest.RequisitionDate = ZDateTime.Empty;
			AssertNoErrors(invoiceChargesForRequest.RequisitionDateInfo);

			invoiceChargesForRequest.IsExcludedFromPosting = false;
			AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			invoiceChargesForRequest.RequisitionDate = ZDateTime.Empty;
			AssertNoErrors(invoiceChargesForRequest.RequisitionDateInfo);

			invoiceChargesForRequest.IsExcludedFromPosting = true;
			AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			invoiceChargesForRequest.RequisitionDate = ZDateTime.Empty;
			AssertNoErrors(invoiceChargesForRequest.RequisitionDateInfo);

			invoiceChargesForRequest.IsExcludedFromPosting = false;
			AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			invoiceChargesForRequest.RequisitionDate = ZDateTime.Empty;
			AssertHasErrors(invoiceChargesForRequest.RequisitionDateInfo);

			invoiceChargesForRequest.RequisitionDate = new ZDateTime(2016, 3, 14);
			AssertNoErrors(invoiceChargesForRequest.RequisitionDateInfo);
		}

		public void TestRequisitionStatusAndDate()
		{
			var aaa = new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = "AAA" };
			var bbb = new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = "BBB" };
			var paymentRequisitionStatuses = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { aaa, bbb };
			paymentRequisitionStatuses.SetDefaultCode("BBB", true);
			AccountingConfigurationRegistry.Instance.PaymentRequisitionStatuses.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, paymentRequisitionStatuses);

			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			var invoiceChargesForRequest = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", null);
			var charge = Factory.NewWithValidTestData<Charge>();
			var paymentDate = new ZDateTime(2016, 3, 21);
			charge.JR_PaymentDate = paymentDate;
			invoiceChargesForRequest.Charges.Add(charge);
			invoiceChargesForRequest.PostingGUIProvider.ShowApprovalFormToSetDescription(request);
			AssertEquals("Default Status", "BBB", invoiceChargesForRequest.RequisitionStatus);
			AssertEquals("Default Date", paymentDate, invoiceChargesForRequest.RequisitionDate);

			invoiceChargesForRequest.RequisitionStatus = "TST";
			var requisitionDate = new ZDateTime(2016, 3, 14);
			invoiceChargesForRequest.RequisitionDate = requisitionDate;
			AssertEquals("TST", request.RequisitionStatus);
			AssertEquals("TST", invoiceChargesForRequest.RequisitionStatus);
			AssertEquals(requisitionDate, request.RequisitionDate);
			AssertEquals(requisitionDate, invoiceChargesForRequest.RequisitionDate);

			var invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", null);
			invoiceCharges.RequisitionStatus = "TST";
			invoiceCharges.RequisitionDate = requisitionDate;
			AssertEquals("TST", request.RequisitionStatus);
			AssertEquals(requisitionDate, request.RequisitionDate);
		}

		public void TestRequisitionStatusAndDate_Readonly()
		{
			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			var invoiceChargesForRequest = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", null);
			invoiceChargesForRequest.PostingGUIProvider.ShowApprovalFormToSetDescription(request);
			AssertRequisitionStatusAndDate_Readonly(invoiceChargesForRequest, true, true);
			AssertRequisitionStatusAndDate_Readonly(invoiceChargesForRequest, true, false);
			AssertRequisitionStatusAndDate_Readonly(invoiceChargesForRequest, false, true);
			AssertRequisitionStatusAndDate_Readonly(invoiceChargesForRequest, false, false);
		}

		void AssertRequisitionStatusAndDate_Readonly(APInvoiceCharges invoiceCharges, bool isExcludedFromPosting, bool allowPaymentRequisitionStatusOverride)
		{
			invoiceCharges.IsExcludedFromPosting = isExcludedFromPosting;
			AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowPaymentRequisitionStatusOverride);
			AssertEquals(isExcludedFromPosting || !allowPaymentRequisitionStatusOverride, invoiceCharges.RequisitionStatus_ReadOnly);
			AssertEquals(isExcludedFromPosting || !allowPaymentRequisitionStatusOverride, invoiceCharges.RequisitionDate_ReadOnly);
		}

		public void TestIsExcludedFromPosting()
		{
			var invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", null);
			Assert(!invoiceCharges.IsExcludedFromPosting);
			Assert(!invoiceCharges.IsExcludedFromPostingInfo.ReadOnly);

			invoiceCharges.ContinueProcessing = false;
			Assert(invoiceCharges.IsExcludedFromPosting);
			Assert(invoiceCharges.IsExcludedFromPostingInfo.ReadOnly);

			invoiceCharges.ContinueProcessing = true;
			Assert(!invoiceCharges.IsExcludedFromPosting);
			Assert(!invoiceCharges.IsExcludedFromPostingInfo.ReadOnly);

			invoiceCharges.IsExcludedFromPosting = true;
			Assert(invoiceCharges.IsExcludedFromPosting);
			Assert(!invoiceCharges.IsExcludedFromPostingInfo.ReadOnly);

			invoiceCharges.IsExcludedFromPosting = false;
			Assert(!invoiceCharges.IsExcludedFromPosting);
			Assert(!invoiceCharges.IsExcludedFromPostingInfo.ReadOnly);

			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			var invoiceChargesForRequest = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", null);
			request.InitializeJobRelated(invoiceChargesForRequest, ZGuid.Empty, "");
			invoiceCharges = new APInvoiceCharges(request);
			Assert(!invoiceCharges.IsExcludedFromPosting);
			Assert(invoiceCharges.IsExcludedFromPostingInfo.ReadOnly);
		}

		public void TestReferenceNumber()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C123");
			var shipment = TestObjectCreator.CreateShipment("S123", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var postGUIProviderMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			postGUIProviderMock.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(consol.PK, (ZString)consol.TablePrefix));

			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			var invoiceChargesForRequest = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", null);
			request.InitializeJobRelated(invoiceChargesForRequest, job.PK, job.TablePrefix);
			var invoiceCharges = new APInvoiceCharges(request);
			AssertEquals("S123", invoiceCharges.ReferenceNumber);

			invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", postGUIProviderMock.Object);
			AssertEquals("C123", invoiceCharges.ReferenceNumber);

			invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", job.PK, job.TablePrefix, postGUIProviderMock.Object);
			AssertEquals("S123", invoiceCharges.ReferenceNumber);

			var transportBookingConsol = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBookingConsolidation>());
			transportBookingConsol[DtbBookingConsolidationSchema.KB_JobType] = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			Factory.Save();
			transportBookingConsol[DtbBookingConsolidationSchema.KB_JobID] = "T123";
			Factory.Save();
			postGUIProviderMock.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(transportBookingConsol.PK, (ZString)transportBookingConsol.TablePrefix));
			invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", postGUIProviderMock.Object);
			AssertEquals("T123", invoiceCharges.ReferenceNumber);
		}

		public void TestApprovingRequestDescription()
		{
			var invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", null);
			AssertEquals("", invoiceCharges.ApprovingRequestDescription);
			Assert(invoiceCharges.ApprovingRequestDescriptionInfo.ReadOnly);
			invoiceCharges.RunPreSaveValidation();
			AssertNoErrors(invoiceCharges.ApprovingRequestDescriptionInfo);

			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			invoiceCharges.PostingGUIProvider.ShowApprovalFormToSetDescription(request);
			AssertEquals("", invoiceCharges.ApprovingRequestDescription);
			Assert(!invoiceCharges.ApprovingRequestDescriptionInfo.ReadOnly);
			invoiceCharges.RunPreSaveValidation();
			var expectedError = "Please enter a Description.";
			AssertHasError(invoiceCharges.ApprovingRequestDescriptionInfo, expectedError);

			invoiceCharges.IsExcludedFromPosting = true;
			AssertEquals("", invoiceCharges.ApprovingRequestDescription);
			Assert(invoiceCharges.ApprovingRequestDescriptionInfo.ReadOnly);
			invoiceCharges.RunPreSaveValidation();
			AssertNoErrors(invoiceCharges.ApprovingRequestDescriptionInfo);

			invoiceCharges.IsExcludedFromPosting = false;
			AssertEquals("", invoiceCharges.ApprovingRequestDescription);
			Assert(!invoiceCharges.ApprovingRequestDescriptionInfo.ReadOnly);
			invoiceCharges.RunPreSaveValidation();
			AssertHasError(invoiceCharges.ApprovingRequestDescriptionInfo, expectedError);

			var expectedDescription = "Some desc";
			invoiceCharges.ApprovingRequestDescription = expectedDescription;
			AssertEquals(expectedDescription, invoiceCharges.ApprovingRequestDescription);
			AssertEquals(expectedDescription, request.XP_ReasonDescription);
			Assert(!invoiceCharges.ApprovingRequestDescriptionInfo.ReadOnly);
			invoiceCharges.RunPreSaveValidation();
			AssertNoErrors(invoiceCharges.ApprovingRequestDescriptionInfo);

			invoiceCharges.IsExcludedFromPosting = true;
			AssertEquals("", invoiceCharges.ApprovingRequestDescription);
			Assert(invoiceCharges.ApprovingRequestDescriptionInfo.ReadOnly);
			invoiceCharges.RunPreSaveValidation();
			AssertNoErrors(invoiceCharges.ApprovingRequestDescriptionInfo);

			invoiceCharges.IsExcludedFromPosting = false;
			AssertEquals(expectedDescription, invoiceCharges.ApprovingRequestDescription);
			AssertEquals(expectedDescription, request.XP_ReasonDescription);
			Assert(!invoiceCharges.ApprovingRequestDescriptionInfo.ReadOnly);
			invoiceCharges.RunPreSaveValidation();
			AssertNoErrors(invoiceCharges.ApprovingRequestDescriptionInfo);
		}

		public void TestSummary()
		{
			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			var invoiceCharges = new APInvoiceCharges(request);
			AssertEquals("Previous request is canceled as invoice is not in this posting.", invoiceCharges.Summary);

			var job = TestObjectCreator.CreateJob("job1", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var guiWrapper = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", job.PK, job.TablePrefix, guiWrapper.Object);
			AssertEquals("Invoice is created.", invoiceCharges.Summary);

			var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
			guiWrapper.Setup(m => m.IsForPreviewOnly).Returns(false);
			guiWrapper.Setup(m => m.ShowLoginFormForTest).Returns(false);
			guiWrapper.Setup(m => m.IsBulkPosting).Returns(false);
			guiWrapper.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
			var helper = new Mock<ITransactionApprovalHelper<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>>();
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(false);
			helper.Setup(m => m.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails).Returns(true);
			helper.Setup(m => m.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails()).Returns(true);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(false);
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.GetNewHelper_ForTestOnly = x => helper.Object;
			invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", job.PK, job.TablePrefix, guiWrapper.Object);
			var result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { invoiceCharges });
			AssertEquals("Invoice is created. Previous approved request with different data is canceled.", invoiceCharges.Summary);

			helper.Setup(m => m.CancelApprovedRequestForThisPostingActionButAnotherPostingDetails()).Returns(false);
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.GetNewHelper_ForTestOnly = x => helper.Object;
			invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", job.PK, job.TablePrefix, guiWrapper.Object);
			result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { invoiceCharges });
			AssertEquals("Invoice is created.", invoiceCharges.Summary);

			helper.Setup(m => m.IsThereApprovedRequestForThisPostingActionButAnotherPostingDetails).Returns(false);
			helper.Setup(m => m.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()).Returns(true);
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.GetNewHelper_ForTestOnly = x => helper.Object;
			invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", job.PK, job.TablePrefix, guiWrapper.Object);
			result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { invoiceCharges });
			AssertEquals("Invoice is created. Previous request is canceled.", invoiceCharges.Summary);

			helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(true);
			APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.GetNewHelper_ForTestOnly = x => helper.Object;
			invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", job.PK, job.TablePrefix, guiWrapper.Object);
			result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { invoiceCharges });
			AssertEquals("Invoice is created. Approved request is posted.", invoiceCharges.Summary);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 0;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			const string testUserCode = "tst";
			const string testUserLogin = "newuser";
			var user = new BusinessObjectFactory().LoadFromUniqueKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)testUserCode);
			if (user == null)
			{
				SecurityTestObject.CreateTestUser(false, Env.Security.APInvoiceApproval.Code, testUserCode, testUserLogin, "password");
			}
			using (Env.SetTemporaryUserContext(testUserLogin, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
				invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", job.PK, job.TablePrefix, guiWrapper.Object);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 1, TestObjectCreator.Creditor1, null, 0, null);
				invoiceCharges.Charges.Add(charge);
				result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { invoiceCharges });
				AssertEquals("New request is created.", invoiceCharges.Summary);

				helper.Setup(m => m.IsThereApprovedRequestForThisPostingDetails).Returns(false);
				helper.Setup(m => m.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus).Returns(true);
				APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.GetNewHelper_ForTestOnly = x => helper.Object;
				invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", job.PK, job.TablePrefix, guiWrapper.Object);
				invoiceCharges.Charges.Add(charge);
				result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { invoiceCharges });
				AssertEquals("Previous request is canceled. New request is created.", invoiceCharges.Summary);

				securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(false);
				invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", job.PK, job.TablePrefix, guiWrapper.Object);
				charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 1, TestObjectCreator.Creditor1, null, 0, null);
				invoiceCharges.Charges.Add(charge);
				result = APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.PerformLevelAuthorizationAndAddNotProcessedHere(new[] { invoiceCharges });
				AssertEquals("", invoiceCharges.Summary);
			}
		}

		public void TestITransactionForApproval()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 100, TestObjectCreator.Creditor1, "INV1", null, 0, null);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 200, TestObjectCreator.Creditor1, "INV1", null, 0, null);
			var invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", null);
			invoiceCharges.Charges.Add(charge1);
			invoiceCharges.Charges.Add(charge2);

			var transactionForApproval = invoiceCharges as ITransactionForApproval;
			AssertEquals("AH_LocalTotalAmount", 330M, transactionForApproval.AH_LocalTotalAmount);
			AssertEquals("HumanReadableName", "Accounts Payable Invoice", transactionForApproval.HumanReadableName);
		}

		public void TestConstructorsAndInitializedProperties()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 100, TestObjectCreator.Creditor1, "INV1", null, 0, null);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 200, TestObjectCreator.Creditor1, "INV1", null, 0, null);

			var expectedCreditor = "Credtor1";
			var expectedInvoiceNumber = "INV1";
			var invoiceCharges = new APInvoiceCharges(expectedCreditor, expectedInvoiceNumber, ZGuid.Empty, "", null);
			invoiceCharges.Charges.Add(charge1);
			invoiceCharges.Charges.Add(charge2);
			AssertEquals("Creditor", expectedCreditor, invoiceCharges.Creditor);
			AssertEquals("InvoiceNumber", expectedInvoiceNumber, invoiceCharges.InvoiceNumber);
			AssertEquals("AH_LocalTotalAmount", 330M, invoiceCharges.AH_LocalTotalAmount);

			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			var invoiceChargesForRequest = new APInvoiceCharges(expectedCreditor, expectedInvoiceNumber, ZGuid.Empty, "", null);
			invoiceChargesForRequest.Charges.Add(charge1);
			invoiceChargesForRequest.Charges.Add(charge2);
			request.InitializeJobRelated(invoiceChargesForRequest, ZGuid.Empty, "");
			invoiceCharges = new APInvoiceCharges(request);
			AssertEquals("Creditor", expectedCreditor, invoiceCharges.Creditor);
			AssertEquals("InvoiceNumber", expectedInvoiceNumber, invoiceCharges.InvoiceNumber);
			AssertEquals("AH_LocalTotalAmount", 330M, invoiceCharges.AH_LocalTotalAmount);
		}

		public void TestITransactionBranchCalculationDataProviderFromJobCharge()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 100, TestObjectCreator.Creditor1, "INV1", null, 0, null);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 200, TestObjectCreator.Creditor1, "INV1", null, 0, null);
			var invoiceCharges = new APInvoiceCharges("Credtor1", "INV1", ZGuid.Empty, "", null);
			invoiceCharges.Charges.Add(charge1);
			invoiceCharges.Charges.Add(charge2);
			var charges = invoiceCharges as ITransactionBranchCalculationDataProviderFromJobCharge;

			var expectedJobBranchPK = job.JH_GB;
			var expectedLineBranchPKs = invoiceCharges.Charges.Select(x => x.JR_GB).ToHashSet();

			CombineAssertions(() =>
			{
				AssertEquals("JobBranchPK", expectedJobBranchPK, charges.JobBranchPK);
				AssertContainsExactElementsInAnyOrder("LineBranchPKs", expectedLineBranchPKs, charges.LineBranchPKs);
				AssertEquals("charges.AnyCharges", true, charges.AnyCharges);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var invoiceCharges = new APInvoiceCharges("", "", ZGuid.Empty, "", null);
			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			invoiceCharges.PostingGUIProvider.ShowApprovalFormToSetDescription(request);
			return invoiceCharges;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;
	}
}
