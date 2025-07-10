using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Module.Testing
{
	public class CommissionLinesPreviewPaneTest : TestCaseWithFactory
	{
		#region OpenSourceButton

		public void TestOpenSourceButton_WithNoGrouping()
		{
			using (var form = new ZForm())
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.OpenSourceButton_Exposed.PerformClick();

				AssertEquals("Cannot open job / transaction", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Please select a job / transaction to open", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenSourceButtonAndTopPanelSizeAndPosition()
		{
			using (var form = new ZForm())
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Show();
				var topPanel = control.Controls.Find("TopPanel", true).First();

				AssertEquals(control.OpenSourceButton_Exposed.Size, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 28, true));
				AssertEquals(control.OpenSourceButton_Exposed.Location, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 3, true));
				AssertEquals(topPanel.Padding, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true));
				AssertEquals(topPanel.Size, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 36, true));
			}
		}

		#endregion

		#region Lines Grid

		public void TestLinesGrid_DoubleClick()
		{
			var grouping = GetNewGrouping(Factory, new[] { line1A, line1B, line2A, line2B });
			using (var form = new ZForm(grouping))
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(4, control.LinesGrid.ListManager.Count);

				control.LinesGrid.SelectSingleElement(line1A);
				control.ViewLinesGridCurrentSelection_Exposed();
				AssertEquals(line1A, control.LastCommissionLineShownViewForm);

				control.LinesGrid.SelectSingleElement(line2B);
				control.ViewLinesGridCurrentSelection_Exposed();
				AssertEquals(line2B, control.LastCommissionLineShownViewForm);
			}

			grouping = GetNewGrouping(Factory, new[] { Factory.New<ViewCommissionLine>() });
			using (var form = new ZForm(grouping))
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.LinesGrid.ListManager.List.Clear();
				control.ViewLinesGridCurrentSelection_Exposed();

				AssertNull(control.LastCommissionLineShownViewForm);
				AssertEquals("Please select a commission line to view.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestLinesGrid_ContextMenuOrdering()
		{
			var grouping = GetNewGrouping(Factory, new[] { line1A, line1B, line2A, line2B });
			using (var form = new ZForm(grouping))
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Controls.Add(control);
				form.Show();

				AssertMultilineASCIIEquals("MenuItems",
@"&View
&View Commission Agreement
&View Approval Request
&Cancel",
					string.Join(System.Environment.NewLine, control.LinesGrid.ContextMenu.MenuItems.Cast<MenuItem>().Take(4).Select(x => x.Text)));
			}
		}

		public void TestLinesGrid_ViewMenuItem()
		{
			var grouping = GetNewGrouping(Factory, new[] { line1A, line1B, line2A });
			using (var form = new ZForm(grouping))
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var viewMenuItem = control.LinesGrid.ContextMenu.MenuItems[0];
				AssertEquals("&View", viewMenuItem.Text);

				AssertEquals(3, control.LinesGrid.ListManager.Count);
				control.LinesGrid.SelectSingleElement(line1A);
				{
					viewMenuItem.PerformClick();
					AssertEquals(line1A, control.LastCommissionLineShownViewForm);
				}

				control.LastCommissionLineShownViewForm = null;
				control.LinesGrid.SelectSingleElement(line2A);
				{
					viewMenuItem.PerformClick();
					AssertEquals(line2A, control.LastCommissionLineShownViewForm);
				}
			}

			grouping = GetNewGrouping(Factory, new[] { Factory.New<ViewCommissionLine>() });
			using (var form = new ZForm(grouping))
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var viewMenuItem = control.LinesGrid.ContextMenu.MenuItems[0];

				control.LinesGrid.ListManager.List.Clear();
				viewMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Unable to view Commission Line", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Please select a commission line to view.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestLinesGrid_ViewCommissionAgreementMenuItem()
		{
			var grouping = GetNewGrouping(Factory, new[] { line1A, line1B, line1WithoutRate, line2A });
			using (var form = new ZForm(grouping))
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var viewAgreementMenuItem = control.LinesGrid.ContextMenu.MenuItems[1];
				AssertEquals("&View Commission Agreement", viewAgreementMenuItem.Text);

				AssertEquals(4, control.LinesGrid.ListManager.Count);
				control.LinesGrid.SelectSingleElement(line1A);
				{
					viewAgreementMenuItem.PerformClick();
					AssertEquals(agreement1RecipientRateA, control.LastRecipientRateShownViewForm);
				}

				control.LastRecipientRateShownViewForm = null;
				control.LinesGrid.SelectSingleElement(line1WithoutRate);
				{
					viewAgreementMenuItem.PerformClick();
					CombineAssertions(() =>
					{
						AssertEquals("LastMessage.Caption", "Unable to view Commission Agreement", UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertEquals("LastMessage.Text", "Selected commission line does not have a commission agreement.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertNull("LastRecipientRateShownViewForm", control.LastRecipientRateShownViewForm);
					});
				}
			}

			grouping = GetNewGrouping(Factory, new[] { Factory.New<ViewCommissionLine>() });
			using (var form = new ZForm(grouping))
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var viewAgreementMenuItem = control.LinesGrid.ContextMenu.MenuItems[1];

				UnitTestUserNotification.Instance.ClearMessages();
				control.LinesGrid.ListManager.List.Clear();

				viewAgreementMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Unable to view Commission Agreement", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Please select a commission line that you wish to view commission agreement for.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("LastRecipientRateShownViewForm", control.LastRecipientRateShownViewForm);
				});
			}
		}

		public void TestLinesGrid_ViewApprovalRequestMenuItem()
		{
			var grouping = GetNewGrouping(Factory, new[] { line1A, line2A });
			using (var form = new ZForm(grouping))
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var viewApprovalRequestMenuItem = control.LinesGrid.ContextMenu.MenuItems[2];
				AssertEquals("&View Approval Request", viewApprovalRequestMenuItem.Text);

				AssertEquals(2, control.LinesGrid.ListManager.Count);
				control.LinesGrid.SelectSingleElement(line1A);
				{
					viewApprovalRequestMenuItem.PerformClick();
					AssertEquals(approvalRequest, control.LastApprovalRequestShownViewForm);
				}

				control.LastApprovalRequestShownViewForm = null;
				control.LinesGrid.SelectSingleElement(line2A);
				{
					viewApprovalRequestMenuItem.PerformClick();
					CombineAssertions(() =>
					{
						AssertEquals("LastMessage.Caption", "Unable to view Approval Request", UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertEquals("LastMessage.Text", "Selected commission line does not have an approval request.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertNull("LastApprovalRequestShownViewForm", control.LastApprovalRequestShownViewForm);
					});
				}
			}

			grouping = GetNewGrouping(Factory, new[] { Factory.New<ViewCommissionLine>() });
			using (var form = new ZForm(grouping))
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var viewApprovalRequestMenuItem = control.LinesGrid.ContextMenu.MenuItems[2];

				control.LinesGrid.ListManager.List.Clear();
				UnitTestUserNotification.Instance.ClearMessages();

				viewApprovalRequestMenuItem.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("LastMessage.Caption", "Unable to view Approval Request", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Please select a commission line that you wish to view approval request for.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("LastRecipientRateShownViewForm", control.LastRecipientRateShownViewForm);
				});
			}
		}

		public void TestLinesGrid_CancelMenuItem()
		{
			var grouping = GetNewGrouping(Factory, new[] { line1A, line1B, line2A });
			using (var form = new ZForm(grouping))
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var cancelMenuItem = control.LinesGrid.ContextMenu.MenuItems[3];
				AssertEquals("&Cancel", cancelMenuItem.Text);

				AssertEquals(3, control.LinesGrid.ListManager.Count);

				control.LinesGrid.SelectSingleElement(line1A);
				{
					ZFormModaliser.LastFormShownForTest = null;
					cancelMenuItem.PerformClick();
					AssertType(typeof(BulkCancelCommissionLinesForm), ZFormModaliser.LastFormShownForTest);
				}

				control.LastCommissionLineShownViewForm = null;
				control.LastRecipientRateShownViewForm = null;
				control.LinesGrid.SelectSingleElement(line2A);
				{
					ZFormModaliser.LastFormShownForTest = null;
					cancelMenuItem.PerformClick();
					AssertType(typeof(BulkCancelCommissionLinesForm), ZFormModaliser.LastFormShownForTest);
				}
			}

			grouping = GetNewGrouping(Factory, new[] { Factory.New<ViewCommissionLine>() });
			using (var form = new ZForm(grouping))
			using (var control = new CommissionLinesPreviewPaneForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var cancelMenuItem = control.LinesGrid.ContextMenu.MenuItems[3];

				control.LinesGrid.ListManager.List.Clear();
				ZFormModaliser.LastFormShownForTest = null;
				cancelMenuItem.PerformClick();
				AssertEquals("Cancel Entity Commission", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Please select commission line(s) to cancel.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
			}
		}

		#endregion

		#region Implementation

		static ViewCommissionLineGrouping GetNewGrouping(BusinessObjectFactory factory, IEnumerable<ViewCommissionLine> commissionLines)
		{
			var grouping = new ViewCommissionLineGrouping(factory);
			grouping.Init(commissionLines);
			return grouping;
		}

		protected override void SetUp()
		{
			base.SetUp();

			agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement1Recipient = agreement1.Recipients.AddNew();
			agreement1Recipient.FillWithValidTestData();
			agreement1RecipientRateA = agreement1Recipient.Rates.AddNew();
			agreement1RecipientRateB = agreement1Recipient.Rates.AddNew();
			agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var agreement2Recipient = agreement2.Recipients.AddNew();
			agreement2Recipient.FillWithValidTestData();
			agreement2RecipientRateA = agreement2Recipient.Rates.AddNew();
			agreement2RecipientRateB = agreement2Recipient.Rates.AddNew();

			var commissionHeader1 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader1.CH0_CA0 = agreement1.PK;
			commissionHeader1.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeader1.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionHeader1LineA = commissionHeader1.Lines.AddNew();
			commissionHeader1LineA.CL0_CAT = agreement1RecipientRateA.PK;
			var commissionHeader1LineB = commissionHeader1.Lines.AddNew();
			commissionHeader1LineB.CL0_CAT = agreement1RecipientRateB.PK;
			var commissionHeader1LineWithoutRate = commissionHeader1.Lines.AddNew();

			var commissionHeader2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeader2.CH0_CA0 = agreement2.PK;
			commissionHeader2.CH0_GroupingSourceTableCode = JobHeaderSchema.Constants.Prefix;
			commissionHeader2.CH0_GroupingSourceID = ZGuid.NewZGuid();

			var commissionHeader2LineA = commissionHeader2.Lines.AddNew();
			commissionHeader2LineA.CL0_CAT = agreement2RecipientRateA.PK;
			var commissionHeader2LineB = commissionHeader2.Lines.AddNew();
			commissionHeader2LineB.CL0_CAT = agreement2RecipientRateB.PK;

			approvalRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			approvalRequest.Items.AddNew().CRI_CL0 = commissionHeader1LineA.PK;
			approvalRequest.Items.AddNew().CRI_CL0 = commissionHeader1LineB.PK;
			approvalRequest.Items.AddNew().CRI_CL0 = commissionHeader1LineWithoutRate.PK;

			Factory.Save();

			line1A = Factory.Load<ViewCommissionLine>(commissionHeader1LineA.PK);
			line1B = Factory.Load<ViewCommissionLine>(commissionHeader1LineB.PK);
			line1WithoutRate = Factory.Load<ViewCommissionLine>(commissionHeader1LineWithoutRate.PK);
			line2A = Factory.Load<ViewCommissionLine>(commissionHeader2LineA.PK);
			line2B = Factory.Load<ViewCommissionLine>(commissionHeader2LineB.PK);
		}

		OrgCommissionAgreement agreement1;
		OrgCommissionAgreement agreement2;
		OrgCommissionAgreementRecipientRate agreement1RecipientRateA;
		OrgCommissionAgreementRecipientRate agreement1RecipientRateB;
		OrgCommissionAgreementRecipientRate agreement2RecipientRateA;
		OrgCommissionAgreementRecipientRate agreement2RecipientRateB;
		ViewCommissionLine line1A;
		ViewCommissionLine line1B;
		ViewCommissionLine line1WithoutRate;
		ViewCommissionLine line2A;
		ViewCommissionLine line2B;
		AccCommissionApprovalRequest approvalRequest;

		#endregion

		#region Classes

		class CommissionLinesPreviewPaneForTest : CommissionLinesPreviewPane
		{
			public ZButton OpenSourceButton_Exposed
			{
				get { return base.OpenSourceButton; }
			}

			public void ViewLinesGridCurrentSelection_Exposed()
			{
				ViewLinesGridCurrentSelection();
			}

			public ViewCommissionLine LastCommissionLineShownViewForm;
			public OrgCommissionAgreementRecipientRate LastRecipientRateShownViewForm;
			public AccCommissionApprovalRequest LastApprovalRequestShownViewForm;

			protected override void ShowViewForm(ViewCommissionLine commissionLine)
			{
				LastCommissionLineShownViewForm = commissionLine;
			}

			protected override void ShowViewForm(OrgCommissionAgreementRecipientRate recipientRate)
			{
				LastRecipientRateShownViewForm = recipientRate;
			}

			protected override void ShowViewForm(AccCommissionApprovalRequest approvalRequest)
			{
				LastApprovalRequestShownViewForm = approvalRequest;
			}
		}

		#endregion
	}
}
