using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(CommissionFinalizerForm))]
	class CommissionFinalizerFormTest : ZFormBasherTest
	{
		#region Form Caption

		public void TestFormVerb()
		{
			using (var form = (CommissionFinalizerForm)GetFormToBash())
			{
				AssertEquals("", form.FormVerb);
			}
		}

		#endregion

		#region Perform Search

		public void TestPerformSearch_CommissionFinalizerResultCountMessage()
		{
			OrganisationsDataRegistry.Instance.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var commissionHeader = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader.CH0_GC = company.PK;

			for (var i = 0; i < 7; i++)
			{
				commissionHeader.Lines.AddNew();
			}

			Factory.Save();

			using (Environment.Env.Instance.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var commissionFinalizer = new CommissionFinalizer();
				using (var form = new CommissionFinalizerFormForTest(commissionFinalizer))
				{
					form.Show();
					form.FilterControl_Exposed.FirePerformSearch();
					AssertContains("Found\r\n7 records", form.FilterControl_Exposed.GetToolStripRecordsFoundLabelText());
				}

				OrganisationsDataRegistry.Instance.CommissionFinalizerMaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
				using (var form = new CommissionFinalizerFormForTest(commissionFinalizer))
				{
					form.Show();
					form.FilterControl_Exposed.FirePerformSearch();
					AssertContains("Found\r\n  too many\r\n   records", form.FilterControl_Exposed.GetToolStripRecordsFoundLabelText());
				}
			}
		}

		#endregion

		#region Double Click on Grid

		public void TestDoubleClick_ArchivedJobs()
		{
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "S0001005";

			var commissionHeader1 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader1.CH0_GC = company.PK;

			commissionHeader1.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeader1.CH0_GroupingSourceID = jobHeader.PK;
			commissionHeader1.CH0_JobNumber = jobHeader.JH_JobNum;
			commissionHeader1.Lines.AddNew();

			var commissionHeader2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader2.CH0_GC = company.PK;
			commissionHeader2.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeader2.CH0_GroupingSourceID = ZGuid.Empty;
			commissionHeader2.CH0_JobNumber = "S0001006";
			commissionHeader2.Lines.AddNew();

			var commissionHeader3 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader3.CH0_GC = company.PK;
			commissionHeader3.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeader3.CH0_GroupingSourceID = ZGuid.NewZGuid();
			commissionHeader3.CH0_JobNumber = "S0001007";
			commissionHeader3.Lines.AddNew();

			Factory.Save();

			using (Environment.Env.Instance.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var commissionFinalizer = new CommissionFinalizer();
				using (var form = new CommissionFinalizerFormForTest(commissionFinalizer))
				{
					form.Show();
					form.FilterControl_Exposed.FirePerformSearch();

					var gridsControl = form.FilterControl_Exposed.Controls.Find("CommissionFinalizerItemGridsControl", true).FirstOrDefault();

					var totalsGrid = (ZArchitecture.ZGrid)((ICommissionFinalizerItemGridsControl)gridsControl).TopLevelGrid;
					var detailsGrid = (ZArchitecture.ZGrid)gridsControl.Controls.Find("DetailCommissionGrid", true).FirstOrDefault();

					totalsGrid.PerformMouseDownForTest(0, 1);
					detailsGrid.PerformMouseDownForTest(0, 1);
					detailsGrid.PerformMouseDownForTest(0, 2);

					using (var jobForm = OpenedFormCache.LastActiveForm.Target as IZForm)
					{
						AssertEquals(ControllerIDs.JobManagement, jobForm.ControllerID);
					}

					OpenedFormCache.LastActiveForm.Target = null;

					detailsGrid.PerformMouseDownForTest(1, 1);
					detailsGrid.PerformMouseDownForTest(1, 2);

					var lastOpenForm = OpenedFormCache.LastActiveForm.Target;

					AssertNull(lastOpenForm);
					AssertEquals("Cannot view form as the Job has been archived", UnitTestUserNotification.Instance.LastMessage.Text);

					OpenedFormCache.LastActiveForm.Target = null;

					detailsGrid.PerformMouseDownForTest(2, 1);
					detailsGrid.PerformMouseDownForTest(2, 2);

					lastOpenForm = OpenedFormCache.LastActiveForm.Target as IZForm;
					AssertNull(lastOpenForm);
					AssertEquals("Cannot view form as the Job has been archived", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion

		#region Buttons

		public void TestRequestApprovalButton_Visibility()
		{
			var commissionFinalizer = new CommissionFinalizer();

			OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			using (var form = new CommissionFinalizerFormForTest(commissionFinalizer))
			{
				form.Show();

				AssertEquals(false, form.RequestApprovalButton_Exposed.Visible);
			}

			OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			using (var form = new CommissionFinalizerFormForTest(commissionFinalizer))
			{
				form.Show();

				AssertEquals(true, form.RequestApprovalButton_Exposed.Visible);
			}
		}

		public void TestRequestApprovalButton()
		{
			var commissionFinalizer = new CommissionFinalizer();
			using (var form = new CommissionFinalizerFormForTest(commissionFinalizer))
			{
				form.Show();

				form.RequestApprovalButton_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Cannot Request Approval", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "No entity commissions were selected for approval.", UnitTestUserNotification.Instance.LastMessage.Text);
				});

				var item = commissionFinalizer.CommissionFinalizerLineItemCollection.AddNew(Factory.New<ViewCommissionLine>());
				item.IsSelected = true;
				item.AddRowWarning("Problem!");

				form.RequestApprovalButton_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Cannot Request Approval", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Selected entity commission(s) have issues that must be fixed before they can be approved. Please reference the warning(s) for additional information.", UnitTestUserNotification.Instance.LastMessage.Text);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				item.ClearAllNotifications();
				form.RequestApprovalButton_Exposed.PerformClick();
				using (var lastDialogShown = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType(typeof(RequestApprovalForm), lastDialogShown);
				}
			}
		}

		[TestDate(2002, 2, 2)]
		public void TestProcessPaymentButton()
		{
			var commissionFinalizer = new CommissionFinalizer();
			using (var form = new CommissionFinalizerFormForTest(commissionFinalizer))
			{
				form.Show();

				form.ProcessPaymentButton_Exposed.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Unable to Process Payment", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "No entity commissions were selected for payment.", UnitTestUserNotification.Instance.LastMessage.Text);
				});

				var grouping = Factory.NewWithValidTestData<AccCommissionLineGroup>();
				grouping.CommissionHeader.CH0_GroupingSourceID = ZGuid.NewZGuid();
				grouping.CommissionHeader.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;

				var approvedLine = Factory.NewWithValidTestData<AccCommissionLine>();
				approvedLine.CL0_ApprovedDateTimeUtc = new ZDateTime(2002, 2, 2);
				approvedLine.CL0_GS_NKStaff = "AR";
				approvedLine.CL0_RX_NKCommissionCurrency = "AUD";
				approvedLine.CL0_RX_NKTransactionCurrency = "AUD";
				approvedLine.CL0_TransactionAmount = 1000m;
				approvedLine.CL0_CommissionType = "PCT";
				approvedLine.CL0_RX_NKCommissionCurrency = "AUD";
				approvedLine.CL0_TotalCommissionableAmount = 1000m;
				approvedLine.CL0_ShareTotal = 1;
				approvedLine.CL0_SharePortion = 1;
				approvedLine.CL0_ShareCommissionAmount = 1000m;
				approvedLine.CL0_EntityCommissionAmount = 100m;
				approvedLine.CL0_EntityPercentage = 10;

				grouping.CommissionHeader.Lines.Add(approvedLine);

				Factory.Save();

				var approvedLineView = Factory.Load<ViewCommissionLine>(approvedLine.PK);
				var item = commissionFinalizer.CommissionFinalizerLineItemCollection.AddNew(approvedLineView);
				item.IsSelected = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);  // for 'Are you sure' Dialog
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;  // for PrintTask Dialog
				form.ProcessPaymentButton_Exposed.PerformClick();

				CombineAssertions("Should have processed payment", () =>
				{
					AssertEquals("LastMessage.Caption", "Process Payment", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Payment successfully processed.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		#endregion

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var finalizer = new CommissionFinalizer();
			return new CommissionFinalizerForm(finalizer);
		}

		#endregion

		#region Classes

		class CommissionFinalizerFormForTest : CommissionFinalizerForm
		{
			public CommissionFinalizerFormForTest(CommissionFinalizer finalizer)
				: base(finalizer)
			{
			}

			public ZButton RequestApprovalButton_Exposed
			{
				get { return base.RequestApprovalButton; }
			}

			public ZButton ProcessPaymentButton_Exposed
			{
				get { return base.ProcessPaymentButton; }
			}

			public ZButton FormCancelButton_Exposed
			{
				get { return base.FormCancelButton; }
			}

			public ZPanel ItemGridsPanel_Exposed
			{
				get { return null; }
			}

			public CommissionFinalizerFilterControl FilterControl_Exposed
			{
				get { return FilterControl; }
			}
		}

		#endregion
		}
	}
