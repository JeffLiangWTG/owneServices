using System;
using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.GUI;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Module.Testing
{
	[TestedType(typeof(CommissionApprovalRequestController))]
	internal class CommissionApprovalRequestControllerTest : ZControllerBasherTest
	{
		#region Standard Overrides

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CommissionApprovalRequest;
		}

		#endregion

		#region Form

		public override void TestViewForm()
		{
			var approval = GetBusinessObjectThatIsInTheDatabase();
			var controller = ZControllerFactory.Create(ControllerIDs.CommissionApprovalRequest);

			Env.Security.CommissionApprovalRequest.IsAllowed = true;
			using (var approvalForm = controller.ShowViewForm(approval))
			{
				AssertType(typeof(CommissionApprovalRequestForm), approvalForm);
				AssertEquals(ODisplayMode.ReadOnly, approvalForm.DisplayMode);
			}

			Env.Security.CommissionApprovalRequest.IsAllowed = false;
			using (var approvalForm = controller.ShowViewForm(approval))
			{
				AssertNull(approvalForm);
				AssertEquals("LastMessage.Text", Env.Security.CommissionApprovalRequest.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public override void TestEditForm()
		{
			var approval = GetBusinessObjectThatIsInTheDatabase();
			var controller = ZControllerFactory.Create(ControllerIDs.CommissionApprovalRequest);

			Env.Security.CommissionApprovalRequest.IsAllowed = true;
			Env.Security.CommissionAuthorizationLevel1.IsAllowed = true;
			Env.Security.CommissionAuthorizationLevel2.IsAllowed = true;
			using (var approvalForm = controller.ShowEditForm(approval))
			{
				AssertType(typeof(CommissionApprovalRequestForm), approvalForm);
				AssertEquals(ODisplayMode.Browse, approvalForm.DisplayMode);
			}

			Env.Security.CommissionAuthorizationLevel1.IsAllowed = false;
			Env.Security.CommissionAuthorizationLevel2.IsAllowed = false;
			using (var approvalForm = controller.ShowEditForm(approval))
			{
				AssertType(typeof(CommissionApprovalRequestForm), approvalForm);
				AssertEquals(ODisplayMode.ReadOnly, approvalForm.DisplayMode);
			}

			Env.Security.CommissionApprovalRequest.IsAllowed = false;
			using (var approvalForm = controller.ShowEditForm(approval))
			{
				AssertNull(approvalForm);
				AssertEquals("LastMessage.Text", Env.Security.CommissionApprovalRequest.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public override void TestDeleteForm()
		{
			AssertExceptionThrown<ControllerShowDeleteFormNotSupportedException>(() => Controller.ShowDeleteForm(GetBusinessObjectThatIsInTheDatabase()));
		}

		#endregion

		#region Urls

		public void TestMakeUrlsOnlyOpenableForCurrentCompany()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.CommissionApprovalRequest);

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert("How publically available Urls are should be dependent on the Registry Item", !controller.MakeUrlsOnlyOpenableForCurrentCompany);

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("How publically available Urls are should be dependent on the Registry Item", controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			Factory.Save();
			return request;
		}

		#endregion
	}
}
