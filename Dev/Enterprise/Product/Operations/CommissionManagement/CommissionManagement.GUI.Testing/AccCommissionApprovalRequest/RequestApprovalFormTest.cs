using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(RequestApprovalForm))]
	class RequestApprovalFormTest : ZFormBasherTest
	{
		#region Form Caption

		public void TestFormVerb()
		{
			using (var form = (RequestApprovalForm)GetFormToBash())
			{
				AssertEquals("", form.FormVerb);
			}
		}

		#endregion

		#region Menu Items

		public void TestDocumentsMenuItem()
		{
			var request = Factory.New<AccCommissionApprovalRequest>();

			var nonSupportUser = Factory.New<GlbStaff>();
			AssertEquals("Precondition", false, nonSupportUser.IsSupportUser);
			using (Env.SetTemporaryUserContext(new UserContext(nonSupportUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				using (var form = new RequestApprovalForm(request))
				{
					form.Show();

					AssertNull("Should not have added Menu", form.Menu);
				}
			}

			var supportUser = Factory.New<GlbStaff>();
			supportUser.GS_LoginName = User.SupportUserName;
			AssertEquals("Precondition", true, supportUser.IsSupportUser);
			using (Env.SetTemporaryUserContext(new UserContext(supportUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				using (var form = new RequestApprovalForm(request))
				{
					form.Show();

					AssertNotNull("Should have added Menu", form.Menu);
					AssertCollectionContains("Should have added Documents Menu Item", "&Documents", form.Menu.MenuItems.Cast<MenuItem>().Select(x => x.Text));
				}
			}
		}

		#endregion

		#region Buttons

		public void TestPostButton()
		{
			OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			var approvalRequest = Factory.New<AccCommissionApprovalRequest>();
			using (var form = new RequestApprovalFormForTesting(approvalRequest))
			{
				form.Show();

				var formClosed = false;
				form.FormClosed += (sender, e) =>
				{
					formClosed = true;
				};
				{
					approvalRequest.CRQ_GS_NKApprovingStaff1 = ZString.Empty;
					approvalRequest.RunPreSaveValidation();
					AssertEquals("Precondition", true, approvalRequest.HasErrors);

					form.PostButton_Exposed.PerformClick();

					AssertEquals(false, formClosed);

					AssertEquals("Caption", "Errors!", UnitTestUserNotification.Instance.LastMessage.Caption);
				}

				{
					var staff = Factory.New<GlbStaff>();
					staff.GS_Code = "ADL";
					staff.GS_EmailAddress = "andrew.luong@wisetechglobal.com";

					Factory.Save();

					approvalRequest.CRQ_GS_NKApprovingStaff1 = "ADL";
					approvalRequest.RunPreSaveValidation();
					AssertEquals("Precondition", false, approvalRequest.HasErrors);

					form.PostButton_Exposed.PerformClick();

					AssertEquals(true, formClosed);
				}
			}
		}

		public void TestPostButton_WithoutCommissionApprovalTemplate()
		{
			OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			var query = new ZQuery(StmTemplateSchema.SO_Name, AccCommissionApprovalRequestEmailSender.CommissionApprovalRequestTemplateName);
			query.AddToFilter(StmTemplateSchema.SO_DataContext, Constants.DataContext.CommissionApprovalReq);
			var commissionApprovalTemplate = Factory.LoadTop1<StmTemplate>(query);
			commissionApprovalTemplate.Delete();
			var menuItems = Factory.Load<StmMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_SO, commissionApprovalTemplate.PK));
			foreach (var menuItem in menuItems)
			{
				menuItem.Delete();
			}

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_EmailAddress = "andrew.luong@wisetechglobal.com";

			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.CommissionAuthorizationLevel1.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			var approvalRequest = Factory.New<AccCommissionApprovalRequest>();
			using (var form = new RequestApprovalFormForTesting(approvalRequest))
			{
				form.Show();

				var formClosed = false;
				form.FormClosed += (sender, e) =>
				{
					formClosed = true;
				};

				approvalRequest.CRQ_GS_NKApprovingStaff1 = "ADL";
				approvalRequest.IncludeSummaryAsEmailAttachment = true;
				approvalRequest.RunPreSaveValidation();
				AssertEquals("Precondition", false, approvalRequest.HasErrors);

				form.PostButton_Exposed.PerformClick();

				CombineAssertions("Should not save because no commision approval request template to use for email", () =>
				{
					AssertEquals("LastMessage.Caption", "Unable to post this Commission Approval Request...", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("LastMessage.Text", "Could not find Commission Approval Request Template to use for email.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("IsInDatabase", false, approvalRequest.IsInDatabase);
					AssertEquals("formClosed", false, formClosed);
				});
			}
		}

		public void TestCloseButton()
		{
			var approvalRequest = Factory.New<AccCommissionApprovalRequest>();
			using (var form = new RequestApprovalFormForTesting(approvalRequest))
			{
				form.Show();

				var formClosed = false;
				form.FormClosed += (sender, e) =>
					{
						formClosed = true;
					};

				form.CloseButton_Exposed.PerformClick();

				AssertEquals(true, formClosed);
			}
		}

		#endregion

		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var approvalRequest = Factory.New<AccCommissionApprovalRequest>();
			return new RequestApprovalForm(approvalRequest);
		}

		#endregion

		#region Classes

		class RequestApprovalFormForTesting : RequestApprovalForm
		{
			public RequestApprovalFormForTesting(AccCommissionApprovalRequest approvalRequest)
				: base(approvalRequest)
			{
			}

			public ZButton PostButton_Exposed
			{
				get { return base.SendButton; }
			}

			public ZButton CloseButton_Exposed
			{
				get { return base.CloseButton; }
			}
		}

		#endregion
	}
}
