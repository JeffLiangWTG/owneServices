using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARCreditNoteApprovalBulkForm))]
	class ARCreditNoteApprovalBulkFormBasherTest :
		TransactionApprovalBulkFormBasherTest<ARCreditNoteApprovalBulkForm, InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		public void TestApprovingUsersVisibility_ONE()
		{
			var request = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);
			request.PostingDetails.ApprovingOption = "ONE";

			for (var level = 6; level >= 1; level--)
			{
				request.PostingDetails.MaxAuthorisationLevelRequired = level;
				using (var testForm = GetRequestForm(new[] { request }))
				{
					testForm.Show();
					AssertApprovingUserVisibility(testForm, true, false, false, false, false, false);
				}
			}
		}

		public void TestApprovingUsersVisibility_TWO()
		{
			var request = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);
			request.PostingDetails.ApprovingOption = "TWO";

			for (var level = 6; level >= 1; level--)
			{
				request.PostingDetails.MaxAuthorisationLevelRequired = level;
				using (var testForm = GetRequestForm(new[] { request }))
				{
					testForm.Show();
					AssertApprovingUserVisibility(testForm, true, true, false, false, false, false);
				}
			}
		}

		public void TestApprovingUsersVisibility_SEQ()
		{
			var request = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);
			request.PostingDetails.ApprovingOption = "SEQ";

			request.PostingDetails.MaxAuthorisationLevelRequired = 6;
			using (var testForm = GetRequestForm(new[] { request }))
			{
				testForm.Show();
				AssertApprovingUserVisibility(testForm, true, true, true, true, true, true);
			}

			request.PostingDetails.MaxAuthorisationLevelRequired = 5;
			using (var testForm = GetRequestForm(new[] { request }))
			{
				testForm.Show();
				AssertApprovingUserVisibility(testForm, true, true, true, true, true, false);
			}

			request.PostingDetails.MaxAuthorisationLevelRequired = 4;
			using (var testForm = GetRequestForm(new[] { request }))
			{
				testForm.Show();
				AssertApprovingUserVisibility(testForm, true, true, true, true, false, false);
			}

			request.PostingDetails.MaxAuthorisationLevelRequired = 3;
			using (var testForm = GetRequestForm(new[] { request }))
			{
				testForm.Show();
				AssertApprovingUserVisibility(testForm, true, true, true, false, false, false);
			}

			request.PostingDetails.MaxAuthorisationLevelRequired = 2;
			using (var testForm = GetRequestForm(new[] { request }))
			{
				testForm.Show();
				AssertApprovingUserVisibility(testForm, true, true, false, false, false, false);
			}

			request.PostingDetails.MaxAuthorisationLevelRequired = 1;
			using (var testForm = GetRequestForm(new[] { request }))
			{
				testForm.Show();
				AssertApprovingUserVisibility(testForm, true, true, false, false, false, false);
			}
		}

		void AssertApprovingUserVisibility(ARCreditNoteApprovalBulkForm testForm, bool expectUser1Visible, bool expectUser2Visible, bool expectUser3Visible, bool expectUser4Visible, bool expectUser5Visible, bool expectUser6Visible)
		{
			AssertEquals(expectUser1Visible, testForm.GetControl<ZCodeFindBox>("ApprovingUserCodeFindBox").Visible);
			AssertEquals(expectUser2Visible, testForm.GetControl<ZCodeFindBox>("ApprovingUser2CodeFindBox").Visible);
			AssertEquals(expectUser3Visible, testForm.GetControl<ZCodeFindBox>("ApprovingUser3CodeFindBox").Visible);
			AssertEquals(expectUser4Visible, testForm.GetControl<ZCodeFindBox>("ApprovingUser4CodeFindBox").Visible);
			AssertEquals(expectUser5Visible, testForm.GetControl<ZCodeFindBox>("ApprovingUser5CodeFindBox").Visible);
			AssertEquals(expectUser6Visible, testForm.GetControl<ZCodeFindBox>("ApprovingUser6CodeFindBox").Visible);
		}

		public void TestSupplyTypeColumnVisibility()
		{
			AssertSupplyTypeVisibilityWithRegistry(true);
			AssertSupplyTypeVisibilityWithRegistry(false);

			void AssertSupplyTypeVisibilityWithRegistry(bool isRegistryEnable)
			{
				var request = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);

				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isRegistryEnable))
				using (var testForm = GetRequestForm(new[] { request }))
				{
					AssertDetailsGridColumnsVisibility(testForm, "SupplyType", isRegistryEnable);
				}
			}
		}

		public void TestJobNumberBranchAndDepartmentTextBoxCaptions_ParentTableJH()
		{
			var request = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);
			Factory.Save();
			using (var testForm = GetRequestForm(new[] { request }))
			{
				testForm.Show();
				AssertEquals("Job Number", testForm.GetControl<ZTextBox>("JobNumberTextBox").CaptionResourceString.Caption);
				AssertEquals("Job Department", testForm.GetControl<ZGuidFindBox>("deptGuidFindBox").CaptionResourceString.Caption);
				AssertEquals("Job Branch", testForm.GetControl<ZGuidFindBox>("branchGuidFindBox").CaptionResourceString.Caption);
			}
		}

		public void TestJobNumberBranchAndDepartmentTextBoxCaptions_ParentTableAH()
		{
			var request = TestObjectCreator.CreateInvoiceReversalApprovalRequest(10m);
			Factory.Save();
			using (var testForm = GetRequestForm(new[] { request }))
			{
				testForm.Show();
				AssertEquals("Invoice Number", testForm.GetControl<ZTextBox>("JobNumberTextBox").CaptionResourceString.Caption);
				AssertEquals("Invoice Department", testForm.GetControl<ZGuidFindBox>("deptGuidFindBox").CaptionResourceString.Caption);
				AssertEquals("Invoice Branch", testForm.GetControl<ZGuidFindBox>("branchGuidFindBox").CaptionResourceString.Caption);
			}
		}

		public void TestJobNumberBranchAndDepartmentTextBoxCaptions_ParentTableXP_ParentRequestLinkedToJob()
		{
			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var request = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);
				Factory.Save();

				var childRequest = request.ChildRequests[0];
				using (var testForm = GetRequestForm(new[] { childRequest }))
				{
					testForm.Show();
					AssertEquals("Job Number", testForm.GetControl<ZTextBox>("JobNumberTextBox").CaptionResourceString.Caption);
					AssertEquals("Job Department", testForm.GetControl<ZGuidFindBox>("deptGuidFindBox").CaptionResourceString.Caption);
					AssertEquals("Job Branch", testForm.GetControl<ZGuidFindBox>("branchGuidFindBox").CaptionResourceString.Caption);
				}
			}
		}

		public void TestJobNumberBranchAndDepartmentTextBoxCaptions_ParentTableXP_ParentRequestLinkedToInvoice()
		{
			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var request = TestObjectCreator.CreateInvoiceReversalApprovalRequest(10m);
				Factory.Save();

				var childRequest = request.ChildRequests[0];
				using (var testForm = GetRequestForm(new[] { childRequest }))
				{
					testForm.Show();
					AssertEquals("Invoice Number", testForm.GetControl<ZTextBox>("JobNumberTextBox").CaptionResourceString.Caption);
					AssertEquals("Invoice Department", testForm.GetControl<ZGuidFindBox>("deptGuidFindBox").CaptionResourceString.Caption);
					AssertEquals("Invoice Branch", testForm.GetControl<ZGuidFindBox>("branchGuidFindBox").CaptionResourceString.Caption);
				}
			}
		}

		public void TestJobNumberBranchAndDepartmentTextBoxCaptions__DefaultValue()
		{
			var request = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);
			request.XP_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Factory.Save();

			using (var testForm = GetRequestForm(new[] { request }))
			{
				testForm.Show();
				AssertEquals("Job/Invoice Number", testForm.GetControl<ZTextBox>("JobNumberTextBox").CaptionResourceString.Caption);
				AssertEquals("Job/Invoice Department", testForm.GetControl<ZGuidFindBox>("deptGuidFindBox").CaptionResourceString.Caption);
				AssertEquals("Job/Invoice Branch", testForm.GetControl<ZGuidFindBox>("branchGuidFindBox").CaptionResourceString.Caption);
			}
		}

		public void TestRelatedRequestGrid()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var creditNote = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
			TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, 25m);
			var request = Factory.New<ARCreditNoteApprovalRequest>();

			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				request.Initialize(new ARCreditNote[] { creditNote }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				request.XP_ReasonCode = Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				request.XP_ReasonDescription = "reason desc 01";
				var childRequest1 = request.RelatedRequests[0];

				using (var testForm = GetRequestForm(new[] { request }))
				{
					testForm.Show();
					var relatedRequestsGrid = testForm.GetControl<ZGrid>("RelatedRequestsGrid");
					Assert(relatedRequestsGrid.Visible);
					relatedRequestsGrid.SelectAllElements();
					AssertEquals(1, relatedRequestsGrid.SelectedRowCount);
					AssertEquals(childRequest1.PK, relatedRequestsGrid.SelectedElements[0].PK);
					var realtedRequestsGroupBox = testForm.GetControl<ZGroupBox>("RelatedRequestsGroupBox");
					AssertEquals("Related line-level requests", realtedRequestsGroupBox.CaptionResourceString.Caption);
					var realtedRequestsLabel = testForm.GetControl<ZLabel>("RelatedRequestsLabel");
					AssertEquals("This request can be approved once the associated line-level requests are in the Approved status", realtedRequestsLabel.Text);
				}

				using (var testForm = GetRequestForm(new[] { childRequest1 }))
				{
					testForm.Show();
					var relatedRequestsGrid = testForm.GetControl<ZGrid>("RelatedRequestsGrid");
					Assert(relatedRequestsGrid.Visible);
					relatedRequestsGrid.SelectAllElements();
					AssertEquals(1, relatedRequestsGrid.SelectedRowCount);
					AssertEquals(request.PK, relatedRequestsGrid.SelectedElements[0].PK);
					var realtedRequestsGroupBox = testForm.GetControl<ZGroupBox>("RelatedRequestsGroupBox");
					AssertEquals("Related header request", realtedRequestsGroupBox.CaptionResourceString.Caption);
					var realtedRequestsLabel = testForm.GetControl<ZLabel>("RelatedRequestsLabel");
					AssertEquals("This request is associated with the following header request. Credit note can be posted once the header request is approved.", realtedRequestsLabel.Text);
				}
			}

			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var testForm = GetRequestForm(new[] { request }))
			{
				testForm.Show();
				var relatedRequestsGrid = testForm.GetControl<ZGrid>("RelatedRequestsGrid");
				Assert("When line level requests registry is off, related requests grid should not be visible.", !relatedRequestsGrid.Visible);
			}
		}

		public void TestColumnsInRelatedRequestsGrid()
		{
			var request1 = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);
			var request2 = TestObjectCreator.CreateARCreditNoteApprovalRequest(20m);
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var testForm = GetRequestForm(new[] { request1, request2 }))
			{
				testForm.Show();

				var relatedRequestsGrid = testForm.GetControl<ZGrid>("RelatedRequestsGrid");
				Assert(!relatedRequestsGrid.GetColumnStyle("JobNumber").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_GB_JobBranch").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_GE_JobDepartment").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_GB_RequestingBranch").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_ApprovalStatus").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_GS_NKApprovingUser1").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_GS_NKApprovingUser2").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_GS_NKApprovingUser3").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_GS_NKApprovingUser4").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_GS_NKApprovingUser5").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_GS_NKApprovingUser6").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_ApprovalDate").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_ReasonCodeDescription").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_ReasonDescription").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_SystemCreateUser").IsUnavailable);
				Assert(!relatedRequestsGrid.GetColumnStyle("XP_SystemCreateTimeUtc").IsUnavailable);
			}
		}

		public void TestPlaceOfSupplyVisibility()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				AssertPlaceOfSupplyVisibilityWithCountry(Core.Constants.CountryCodes.India, true);
			}

			AssertPlaceOfSupplyVisibilityWithCountry(Core.Constants.CountryCodes.Australia, false);

			void AssertPlaceOfSupplyVisibilityWithCountry(string country, bool visibility)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				using (var testForm = (ARCreditNoteApprovalBulkForm)GetFormToBash())
				{
					AssertDetailsGridColumnsVisibility(testForm, "PlaceOfSupply", visibility);
				}
			}
		}

		void AssertDetailsGridColumnsVisibility(ARCreditNoteApprovalBulkForm from, string columnName, bool visibility)
		{
			from.Show();
			var detailsGrid = from.GetControl<ZGrid>("DetailsGrid");
			AssertEquals($"Visibility of {columnName}", visibility, detailsGrid.Columns.Contains(columnName));
		}

		protected override ARCreditNoteApprovalRequest GetNewApprovalRequest()
		{
			return Factory.New<ARCreditNoteApprovalRequest>();
		}

		protected override ARCreditNoteApprovalBulkForm GetRequestForm(params ARCreditNoteApprovalRequest[] bizos)
		{
			return new ARCreditNoteApprovalBulkForm(new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProvider(), bizos), ApprovalFormMode);
		}
	}

	[TestedType(typeof(ARCreditNoteApprovalBulkForm))]
	class ARCreditNoteApprovalBulkFormWithMultipleRequestsBasherTest :
		TransactionApprovalBulkFormWithMultipleRequestsBasherTest<ARCreditNoteApprovalBulkForm, InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		public void TestColumnsInTopGrid()
		{
			var request1 = TestObjectCreator.CreateARCreditNoteApprovalRequest(10m);
			var request2 = TestObjectCreator.CreateARCreditNoteApprovalRequest(20m);
			Factory.Save();

			using (var testForm = GetRequestForm(new[] { request1, request2 }))
			{
				testForm.Show();

				var multiRequestGrid = testForm.GetControl<ZGrid>("TopGrid");
				Assert(!multiRequestGrid.GetColumnStyle("JobNumber").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_GB_JobBranch").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_GE_JobDepartment").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_GB_RequestingBranch").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_ApprovalStatus").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_GS_NKApprovingUser1").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_GS_NKApprovingUser2").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_GS_NKApprovingUser3").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_GS_NKApprovingUser4").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_GS_NKApprovingUser5").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_GS_NKApprovingUser6").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_ApprovalDate").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_ReasonCodeDescription").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_ReasonDescription").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_SystemCreateUser").IsUnavailable);
				Assert(!multiRequestGrid.GetColumnStyle("XP_SystemCreateTimeUtc").IsUnavailable);
			}
		}

		public void TestRelatedRequestGrid()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var creditNote = TestObjectCreator.CreateARCreditNote(100m, TestObjectCreator.LocalClient.PK, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
			TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, 25m);

			var request = Factory.New<ARCreditNoteApprovalRequest>();
			ARCreditNoteApprovalRequest childRequest = null;

			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				request.Initialize(new ARCreditNote[] { creditNote }, job.PK, job.TablePrefix, JobInvoicingPostingOption.Revenue);
				request.XP_ReasonCode = Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails;
				request.XP_ReasonDescription = "reason desc 01";
				childRequest = request.RelatedRequests[0];

				using (var testForm = GetRequestForm(new[] { request, childRequest }))
				{
					testForm.Show();
					var multiRequestGrid = testForm.GetControl<ZGrid>("TopGrid");
					var relatedRequestsGrid = testForm.GetControl<ZGrid>("RelatedRequestsGrid");
					Assert(relatedRequestsGrid.Visible);

					relatedRequestsGrid.SelectAllElements();
					AssertEquals(1, relatedRequestsGrid.SelectedRowCount);
					var realtedRequestsGroupBox = testForm.GetControl<ZGroupBox>("RelatedRequestsGroupBox");
					AssertEquals("Related Requests", realtedRequestsGroupBox.CaptionResourceString.Caption);
					var realtedRequestsLabel = testForm.GetControl<ZLabel>("RelatedRequestsLabel");
					Assert(!realtedRequestsLabel.Visible);
				}
			}

			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var testForm = GetRequestForm(new[] { request, childRequest }))
			{
				testForm.Show();
				var relatedRequestsGrid = testForm.GetControl<ZGrid>("RelatedRequestsGrid");
				Assert("When line level requests registry is off, related requests grid should not be visible.", !relatedRequestsGrid.Visible);
			}
		}

		protected override ARCreditNoteApprovalRequest GetNewApprovalRequest(bool isValidParent)
		{
			return Factory.New<ARCreditNoteApprovalRequest>();
		}

		protected override ARCreditNoteApprovalBulkForm GetRequestForm(params ARCreditNoteApprovalRequest[] bizos)
		{
			return new ARCreditNoteApprovalBulkForm(new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProvider(), bizos), ApprovalFormMode);
		}
	}
}
