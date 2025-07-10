using System;
using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using AccountingBusiness = Enterprise.Accounting.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class CreditControlledDocumentsApprovalEmailDefTest : AccountingEmailDefTest
	{
		public void TestSendApproveRejectEmail_NoJobNumberDescriptionOrUser()
		{
			var shipment = Factory.New<ICommonShipment>();
			Factory.Save();

			var approvalRequest = CreateTestApprovalRequest((BusinessObject)shipment, Constants.GenApprovalRequestApprovalStatus.Rejected);

			var emailDef = new CreditControlledDocumentsApprovalEmailDef(approvalRequest);
			AssertEquals("No receipients", 0, emailDef.Recipients.Count);

			approvalRequest.XP_SystemCreateUser = requestingUser.GS_Code;
			Factory.Save();

			emailDef = new CreditControlledDocumentsApprovalEmailDef(approvalRequest);
			emailDef.Send();
			AssertEquals("One receipient", 1, emailDef.Recipients.Count);
			AssertEquals("Email Subject", "AR Credit Controlled Documents approval request for Job Number 'S00001000' was Rejected", emailDef.Subject);
			AssertEquals("Email Recipient count", 1, emailDef.Recipients.Count);
			AssertEquals("Email Recipient", "thomas@shippingco.com", emailDef.Recipients[0].Email);
			AssertContains("Email Body Contains Main Paragraph", @"<p>AR Credit Controlled Documents Approval request with description '' was Rejected by user ''.</p>", emailDef.Body);
			AssertContains("Email Body Contains Link, due to parent being a jobshipment", @"<a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobShipment&BusinessEntityPK=" + shipment.PK.ToString(), emailDef.Body);
		}

		public void TestSendApproveRejectEmail_WithJobNumberDescriptionAndUser()
		{
			var shipment = Factory.New<ICommonShipment>();
			((BusinessObject)shipment)[JobShipmentSchema.JS_UniqueConsignRef] = "J003";
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var approvalRequest = CreateTestApprovalRequest((BusinessObject)shipment, Constants.GenApprovalRequestApprovalStatus.Approved);
			approvalRequest.XP_GS_NKApprovingUser1 = approvingUser.GS_Code;
			approvalRequest.XP_ReasonDescription = "FooBar";
			approvalRequest.XP_SystemCreateUser = requestingUser.GS_Code;

			var emailDef = new CreditControlledDocumentsApprovalEmailDef(approvalRequest);
			emailDef.Send();
			AssertEquals("One receipient", 1, emailDef.Recipients.Count);
			AssertEquals("Email Subject", "AR Credit Controlled Documents approval request for Job Number 'J003' was Approved", emailDef.Subject);
			AssertEquals("Email Recipient count", 1, emailDef.Recipients.Count);
			AssertEquals("Email Recipient", "thomas@shippingco.com", emailDef.Recipients[0].Email);
			AssertContains("Email Body Contains Main Paragraph", @"<p>AR Credit Controlled Documents Approval request with description 'FooBar' was Approved by user 'Jonathan'.</p>", emailDef.Body);
			AssertContains("Email Body Contains Link, due to parent being a jobshipment", @"<a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobShipment&BusinessEntityPK=" + shipment.PK.ToString(), emailDef.Body);
		}

		public void TestSendApproveRejectEmail_NoLink()
		{
			var shipment = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Factory.Save();

			var approvalRequest = CreateTestApprovalRequest(shipment, Constants.GenApprovalRequestApprovalStatus.Rejected);

			var emailDef = new CreditControlledDocumentsApprovalEmailDef(approvalRequest);
			emailDef.Send();
			AssertNotContains("Email Body Contains No Link, because the parent object (a job header) isn't linked to a module in the system.", @"<a", emailDef.Body);
		}

		public void TestSendRequestEmail_WithJobNumberDescriptionAndUser()
		{
			SetupTestARCreditControlledDocumentsApprovalNotifyGroup(Factory);

			var shipment = Factory.New<ICommonShipment>();
			((BusinessObject)shipment)[JobShipmentSchema.JS_UniqueConsignRef] = "J003";
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var approvalRequest = CreateTestApprovalRequest((BusinessObject)shipment, Constants.GenApprovalRequestApprovalStatus.Requested);
			approvalRequest.XP_ReasonDescription = "FooBar";
			approvalRequest.XP_SystemCreateUser = requestingUser.GS_Code;

			var emailDef = new CreditControlledDocumentsApprovalEmailDef(approvalRequest);
			emailDef.Send();
			AssertEquals("One receipient", 2, emailDef.Recipients.Count);
			AssertEquals("Email Subject", "AR Credit Controlled Documents approval request for Job Number 'J003' was Requested", emailDef.Subject);
			AssertEquals("Email Recipient count", 2, emailDef.Recipients.Count);
			Assert("Email Recipient", emailDef.Recipients.Contains("pointyhairedguy1@shippingco.com"));
			Assert("Email Recipient", emailDef.Recipients.Contains("pointyhairedguy2@shippingco.com"));
			AssertContains("Email Body Contains Main Paragraph", @"<p>AR Credit Controlled Documents Approval request with description 'FooBar' was Requested by user 'Thomas'.</p>", emailDef.Body);
			AssertContains("Email Body Contains Link, due to parent being a jobshipment", @"<a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobShipment&BusinessEntityPK=" + shipment.PK.ToString(), emailDef.Body);
		}

		public void TestSendRequestEmail_ContentType()
		{
			SetupTestARCreditControlledDocumentsApprovalNotifyGroup(Factory);

			var shipment = Factory.New<ICommonShipment>();
			((BusinessObject)shipment)[JobShipmentSchema.JS_UniqueConsignRef] = "J003";
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var approvalRequest = CreateTestApprovalRequest((BusinessObject)shipment, Constants.GenApprovalRequestApprovalStatus.Requested);
			approvalRequest.XP_ReasonDescription = "FooBar";
			approvalRequest.XP_SystemCreateUser = requestingUser.GS_Code;

			var emailDef = new CreditControlledDocumentsApprovalEmailDef(approvalRequest);
			emailDef.Send();
			AssertEquals("Content type should be HTML", EmailContentTypes.HTML, emailDef.ContentType);
		}

		public void TestEmailLinkOpensForwardingConsolWhenRestrictedDocumentIsGeneratedFromForwardingConsol()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			var forwardingConsol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			forwardingConsol.AddShipment(shipment);
			var approvalRequest1 = CreateTestApprovalRequest((BusinessObject)forwardingConsol, Constants.GenApprovalRequestApprovalStatus.Requested);
			Factory.Save();

			var emailDefForForwardingConsolWithoutCFSLoadLIst = new CreditControlledDocumentsApprovalEmailDef(approvalRequest1);
			emailDefForForwardingConsolWithoutCFSLoadLIst.Send();
			var expectedEmailBodyContainingLink = string.Format(@"<p>AR Credit Controlled Documents Approval request with description '' was Requested by user 'CargoWise Support'.</p><p>See <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobConsol&BusinessEntityPK={0}", forwardingConsol.PK.ToString());
			AssertContains("Email Body Contains Link to Forwarding Consol", expectedEmailBodyContainingLink, emailDefForForwardingConsolWithoutCFSLoadLIst.Body);

			var query = @"
UPDATE dbo.jobconsol
SET
	JK_IsCFS = @cfsStatus,
	JK_SystemLastEditTimeUtc = GETUTCDATE(),
	JK_SystemLastEditUser = '~BP'
WHERE
	JK_PK = @consolPK ";
			using (var command = ((IDbConnected)Factory).Connection.Command(query))
			{
				command.AddParameter("@cfsStatus", SqlDbType.Bit, 1);
				command.AddParameter("@consolPK", SqlDbType.UniqueIdentifier, forwardingConsol.PK.ToGuid());
				command.ExecuteNonQuery();
			}

			var differentFactory = new BusinessObjectFactory();
			var approvalRequest2 = differentFactory.Load<AccountingBusiness.CreditControlledDocumentsApproval>(approvalRequest1.PK);
			var emailDefForForwardingConsolWithCFSLoadList = new CreditControlledDocumentsApprovalEmailDef(approvalRequest2);
			emailDefForForwardingConsolWithCFSLoadList.Send();
			AssertContains("Email Body Contains Link to Forwarding Consol", expectedEmailBodyContainingLink, emailDefForForwardingConsolWithCFSLoadList.Body);
		}

		#region Implementation

		protected override Type EmailDefType
		{
			get { return typeof(CreditControlledDocumentsApprovalEmailDef); }
		}

		GlbStaff approvingUser;
		GlbStaff requestingUser;

		protected override void SetUp()
		{
			SetupData();
		}

		void SetupData()
		{
			approvingUser = Factory.NewWithValidTestData<GlbStaff>();
			approvingUser.GS_FullName = "Jonathan";
			requestingUser = Factory.NewWithValidTestData<GlbStaff>();
			requestingUser.GS_FullName = "Thomas";
			requestingUser.GS_EmailAddress = "thomas@shippingco.com";
		}

		AccountingBusiness.CreditControlledDocumentsApproval CreateTestApprovalRequest(BusinessObject parent, ZString status)
		{
			var menutItem = Factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path";
			menutItem.SU_MenuName = "name";
			var approvalRequest = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvalRequest.Initialize(parent, menutItem.PK);
			approvalRequest.XP_ApprovalStatus = status;
			return approvalRequest;
		}

		public static void SetupTestARCreditControlledDocumentsApprovalNotifyGroup(BusinessObjectFactory factory)
		{
			var pointy1 = factory.NewWithValidTestData<GlbStaff>();
			pointy1.GS_FullName = "pointyhairedguy1";
			pointy1.GS_EmailAddress = "pointyhairedguy1@shippingco.com";
			var pointy2 = factory.NewWithValidTestData<GlbStaff>();
			pointy2.GS_FullName = "pointyhairedguy2";
			pointy2.GS_EmailAddress = "pointyhairedguy2@shippingco.com";
			var notifyGroup = factory.New<GlbGroup>();
			notifyGroup.Staff.Add(pointy1);
			notifyGroup.Staff.Add(pointy2);
			ObjectFactory.Get<IAccounting>().Registry.ARCreditControlledDocumentsApprovalNotifyGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, notifyGroup.PK.ToGuid());
		}

		#endregion
	}
}
