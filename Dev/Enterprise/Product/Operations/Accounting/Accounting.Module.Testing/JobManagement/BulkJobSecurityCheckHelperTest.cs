using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	class BulkJobSecurityCheckHelperTest : TestCaseWithFactory
	{
		public void TestCheckSecurityCheckPoint()
		{
			Func<BusinessObject[], SecurityCheckpoint[]> getSecurityCheckpoints = bizos => new SecurityCheckpoint[] { Env.Security.MaintainShipmentJobInvoicing, Env.Security.CFSShipmentJobInvoicing };
			var result = BulkJobSecurityCheckHelper.CheckSecurityCheckPoint(Array.Empty<BusinessObject>(), bizos => new SecurityCheckpoint[] { null });
			AssertEquals("Action is allowed if no securities was found.", true, result);
			AssertNull("No mesages should be shown", UnitTestUserNotification.Instance.LastMessage.Text);

			Env.Security.MaintainShipmentJobInvoicing.IsAllowed = true;
			Env.Security.CFSShipmentJobInvoicing.IsAllowed = true;
			result = BulkJobSecurityCheckHelper.CheckSecurityCheckPoint(Array.Empty<BusinessObject>(), getSecurityCheckpoints);
			AssertEquals("Action is allowed when all securities are allowed.", true, result);
			AssertNull("No mesages should be shown", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.Security.MaintainShipmentJobInvoicing.IsAllowed = true;
			Env.Security.CFSShipmentJobInvoicing.IsAllowed = false;
			result = BulkJobSecurityCheckHelper.CheckSecurityCheckPoint(Array.Empty<BusinessObject>(), getSecurityCheckpoints);
			AssertEquals("Action is disallowed when any of securities are disallowed.", false, result);
			string expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> CFS/CTO -> Shipments -> Billing";
			AssertEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.Security.MaintainShipmentJobInvoicing.IsAllowed = false;
			Env.Security.CFSShipmentJobInvoicing.IsAllowed = true;
			result = BulkJobSecurityCheckHelper.CheckSecurityCheckPoint(Array.Empty<BusinessObject>(), getSecurityCheckpoints);
			AssertEquals("Action is disallowed when any of securities are disallowed.", false, result);
			expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing";
			AssertEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.Security.MaintainShipmentJobInvoicing.IsAllowed = false;
			Env.Security.CFSShipmentJobInvoicing.IsAllowed = false;
			result = BulkJobSecurityCheckHelper.CheckSecurityCheckPoint(Array.Empty<BusinessObject>(), getSecurityCheckpoints);
			AssertEquals("Action is disallowed when any of securities are disallowed.", false, result);
			expectedMessage =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing
Operate -> CFS/CTO -> Shipments -> Billing";
			AssertEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestCheckSecurityCheckPointCantBeCalledWithEmptySecurityCheckpointArray()
		{
			BulkJobSecurityCheckHelper.CheckSecurityCheckPoint(Array.Empty<BusinessObject>(), bizos => Array.Empty<SecurityCheckpoint>());
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestGetSecurityCheckPointsCantBeCalledIfBizoDoesntSupportSecurities()
		{
			BulkJobSecurityCheckHelper.GetSecurityCheckPoints(SecurityCore.BulkPostAll, new BusinessObject[] { Factory.New<APInvoice>() });
		}

		public void TestGetSecurityCheckPoints()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var jobPlugIn1 = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var jobPlugIn2 = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.CFSLoadList);
			var jobPlugIn3 = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var jobPlugIn4 = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var jobPlugIn5 = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.WarehouseInwards);
			var jobPlugIn6 = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.CFSLoadList);
			var job1 = testObjectCreator.CreateJob(jobPlugIn1, false);
			var job2 = testObjectCreator.CreateJob(jobPlugIn2, false);
			var job3 = testObjectCreator.CreateJob(jobPlugIn3, false);
			var job4 = testObjectCreator.CreateJob(jobPlugIn4, false);
			var job5 = testObjectCreator.CreateJob(jobPlugIn5, false);
			var job6 = testObjectCreator.CreateJob(jobPlugIn6, false);

			var result = BulkJobSecurityCheckHelper.GetSecurityCheckPoints(SecurityCore.BulkPostAll, new BusinessObject[] { job1, job2, job3, job4, job5 });
			AssertEquals("Security checkpoints should not be duplicated.", 3, result.Length);
			string[] expectedNames =
				{
					"Operate -> Forwarding -> Shipments -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost",
					"Operate -> Product Warehouse -> Receive -> Billing",
					"Operate -> CFS/CTO -> Load Lists -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost"
				};
			AssertContainsExactElementsInAnyOrder("Security checkpoints. Root should be returned if securitycheckpoint not found.", expectedNames,
				result.Select(checkpoint => checkpoint == null ? "null" : checkpoint.DisplayTextPathToSecurityRight).ToArray());

			result = BulkJobSecurityCheckHelper.GetSecurityCheckPoints(SecurityCore.BulkPostAll,
				new BusinessObject[] { (BusinessObject)jobPlugIn1, (BusinessObject)jobPlugIn2, (BusinessObject)jobPlugIn3, (BusinessObject)jobPlugIn4, (BusinessObject)jobPlugIn5 });
			AssertEquals("Security checkpoints should not be duplicated.", 3, result.Length);
			AssertContainsExactElementsInAnyOrder("Security checkpoints. Root should be returned if securitycheckpoint not found.", expectedNames,
				result.Select(checkpoint => checkpoint == null ? "null" : checkpoint.DisplayTextPathToSecurityRight).ToArray());
		}

		public void TestCallGetSecurityCheckPointsFromCheckSecurityCheckPoint()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var jobPlugIn1 = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var jobPlugIn2 = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.CFSLoadList);
			var jobPlugIn3 = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.WarehouseInwards);
			var job1 = testObjectCreator.CreateJob(jobPlugIn1, false);
			var job2 = testObjectCreator.CreateJob(jobPlugIn2, false);
			var job3 = testObjectCreator.CreateJob(jobPlugIn3, false);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "TestStaff";
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var result = BulkJobSecurityCheckHelper.CheckSecurityCheckPoint(new BusinessObject[] { job1, job2, job3 },
								bizos => BulkJobSecurityCheckHelper.GetSecurityCheckPoints(SecurityCore.BulkPostAll, bizos));
				AssertEquals("Action is disallowed when any of securities are disallowed.", false, result);
				string expectedMessage =
	@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost
Operate -> CFS/CTO -> Load Lists -> Billing -> Bulk Job Billing Actions -> Post All Charges and Cost
Operate -> Product Warehouse -> Receive -> Billing";
				AssertEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				result = BulkJobSecurityCheckHelper.CheckSecurityCheckPoint(new BusinessObject[] { (BusinessObject)jobPlugIn1, (BusinessObject)jobPlugIn2, (BusinessObject)jobPlugIn3 },
								bizos => BulkJobSecurityCheckHelper.GetSecurityCheckPoints(SecurityCore.BulkPostAll, bizos));
				AssertEquals("Action is disallowed when any of securities are disallowed.", false, result);
				AssertEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
