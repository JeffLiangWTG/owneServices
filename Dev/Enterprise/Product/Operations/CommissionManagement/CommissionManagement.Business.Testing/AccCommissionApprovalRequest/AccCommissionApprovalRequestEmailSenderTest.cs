using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class AccCommissionApprovalRequestEmailSenderTest : TestCaseWithFactory
	{
		public void TestLocalCommissionApprovalRequestTemplateExists()
		{
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var request = Factory.New<AccCommissionApprovalRequest>();
			AssertNotNull("If this fails, make sure to save the Local CommissionApprovalRequestTemplate in Documents.xml", request.EmailSender.CommissionApprovalRequestTemplate);
		}

		public void TestGlobalCommissionApprovalRequestTemplateExists()
		{
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var request = Factory.New<AccCommissionApprovalRequest>();
			AssertNotNull("If this fails, make sure to save the Global CommissionApprovalRequestTemplate in Documents.xml", request.EmailSender.CommissionApprovalRequestTemplate);
		}

		public void TestCreateCommissionApprovalDocument_WithIncludeSummaryAsEmailAttachment()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			staff1.GS_EmailAddress = "andrew.luong@wisetechglobal.com";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SCW";
			staff2.GS_EmailAddress = "samuel.wang@wisetechglobal.com";

			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			request.CRQ_BatchNumber = "00002003";
			request.CRQ_GS_NKApprovingStaff1 = "ADL";

			request.IncludeSummaryAsEmailAttachment = true;
			Factory.Save();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			CombineAssertions(() =>
			{
				AssertEquals("Subject", "Commission Approval Request 00002003", email.Subject);
				AssertStartsWith("Body", @"Please view <a href=""edient:Command=ShowEditForm&", email.Body);
				AssertEndsWith("Body", @">Commission Approval Request 00002003</a> for approval.", email.Body);
				AssertEquals("ContentType", EmailContentTypes.HTML, email.ContentType);

				AssertEquals("Attachments.Count", 1, email.Attachments.Count);
				if (email.Attachments.Count == 1)
				{
					var attachment = email.Attachments[0];
					AssertEquals("attachment.DisplayName", "Commission Approval Request 00002003.xls", attachment.DisplayName);
				}
			});
		}

		public void TestCreateCommissionApprovalDocument_WithoutIncludeSummaryAsEmailAttachment()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			staff1.GS_EmailAddress = "andrew.luong@wisetechglobal.com";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SCW";
			staff2.GS_EmailAddress = "samuel.wang@wisetechglobal.com";

			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			request.CRQ_BatchNumber = "00002003";
			request.CRQ_GS_NKApprovingStaff1 = "ADL";

			request.IncludeSummaryAsEmailAttachment = false;
			Factory.Save();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			CombineAssertions(() =>
			{
				AssertEquals("Subject", "Commission Approval Request 00002003", email.Subject);
				AssertStartsWith("Body", @"Please view <a href=""edient:Command=ShowEditForm&", email.Body);
				AssertEndsWith("Body", @">Commission Approval Request 00002003</a> for approval.", email.Body);
				AssertEquals("ContentType", EmailContentTypes.HTML, email.ContentType);

				AssertEquals("Attachments.Count", 0, email.Attachments.Count);
			});
		}
	}
}
