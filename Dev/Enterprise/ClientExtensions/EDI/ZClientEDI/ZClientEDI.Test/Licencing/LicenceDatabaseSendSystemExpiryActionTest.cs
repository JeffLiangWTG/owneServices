using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Licencing.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	class LicenceDatabaseSendSystemExpiryActionTest : TestCaseWithFactory
	{
		LicenceDatabase[] Databases;

		[GuiTest]
		public void TestSendNewSystemShutdownDateCW1()
		{
			var currentYear = ZDateTime.Now.Year;
			var expiryTime = new ZDateTime(currentYear, 12, 12);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "ABC", "SY1");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN2", "BAC", "SY2");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "EN3", "CAB", "SY3");

			var db1 = lic1.Database;
			db1.LD_LastHeartbeat = new ZDateTime(currentYear, 10, 10);
			db1.LD_LicenceExpiry = expiryTime;
			db1.LD_Status = DatabaseStatusList.Codes.REG;
			db1.LD_HostDBInstance = "INSTANC1";
			db1.LD_HostDBName = "HOSTDB1";
			db1.LD_LicenceType = DatabaseTypes.Codes.Test;
			db1.LD_DBServerSecurityMode = db1.Lookups.DatabaseSecurityModesList[0].Code;

			var db2 = lic2.Database;
			db2.LD_LastHeartbeat = new ZDateTime(currentYear, 10, 11);
			db2.LD_LicenceExpiry = expiryTime;
			db2.LD_Status = DatabaseStatusList.Codes.REG;
			db2.LD_HostDBInstance = "INSTANC2";
			db2.LD_HostDBName = "HOSTDB1";
			db2.LD_LicenceType = DatabaseTypes.Codes.Test;
			db2.LD_DBServerSecurityMode = db2.Lookups.DatabaseSecurityModesList[0].Code;

			var db3 = lic3.Database;
			db3.LD_LastHeartbeat = new ZDateTime(currentYear, 10, 12);
			db3.LD_LicenceExpiry = expiryTime;
			db3.LD_HostDBInstance = "INSTANC3";
			db3.LD_Status = DatabaseStatusList.Codes.Preregistered;

			lic1.LA_LastLicenceSyncCheck = new ZDateTime(currentYear, 10, 10);
			lic2.LA_LastLicenceSyncCheck = new ZDateTime(currentYear, 10, 10);

			Databases = new[] { db1, db2, db3 };

			Factory.Save();
			var sendSystemExpiryAction = new LicenceDatabaseSendSystemExpiryActionForTesting(Databases);
			sendSystemExpiryAction.SendNewSystemShutdownDate();

			AssertEquals(true, db1.Logs.HasLogWith(StmALogSchema.SL_Reference, $"Send new system shutdown date '12/12/{currentYear}' for 'SY1' Server."));
			AssertEquals(true, db2.Logs.HasLogWith(StmALogSchema.SL_Reference, $"Send new system shutdown date '12/12/{currentYear}' for 'SY2' Server."));
			AssertEquals(false, db3.Logs.HasLogWith(StmALogSchema.SL_Reference, $"Send new system shutdown date '12/12/{currentYear}' for 'SY3' Server."));

			AssertContains(String.Format("2 Database(s) successfully sent\r\n1 Database(s) had errors.\r\nFailures shown below <Enterprise Code>-<Server Code> <Error Message>\r\n{0}-{1} Can't send update since server SID is empty. A heartbeat is needed from the client.", db3.LicEnterprise.LE_EnterpriseCode, db3.LD_ServerCode), UnitTestUserNotification.Instance.LastMessage.Text);

			List<LicenceDatabase> anotherDummyList1 = new List<LicenceDatabase>();
			anotherDummyList1.Add(db1);
			Factory.Save();
			var anotherDummySendSystemExpiryAction1 = new LicenceDatabaseSendSystemExpiryActionForTesting(anotherDummyList1.ToArray());
			anotherDummySendSystemExpiryAction1.SendNewSystemShutdownDate();

			AssertContains("Shutdown date updated successfully", UnitTestUserNotification.Instance.LastMessage.Text);

			List<LicenceDatabase> anotherDummyList2 = new List<LicenceDatabase>();
			anotherDummyList2.Add(db3);
			Factory.Save();
			var anotherDummySendSystemExpiryAction2 = new LicenceDatabaseSendSystemExpiryActionForTesting(anotherDummyList2.ToArray());
			anotherDummySendSystemExpiryAction2.SendNewSystemShutdownDate();

			AssertContains("Can't send update since server SID is empty. A heartbeat is needed from the client.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[GuiTest]
		public void TestSendNewSystemShutdownDate()
		{
			var currentYear = ZDateTime.Now.Year;
			var expiryTime = new ZDateTime(currentYear, 12, 12);

			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "EN1", "ABC", "SY1", false);
			var licHeader2 = BillingTestHelper.CreateLicence(Factory, "EN2", "BAC", "SY2", false);
			var licHeader3 = BillingTestHelper.CreateLicence(Factory, "EN3", "CAB", "SY3", false);

			var db1 = licHeader1.Database;
			db1.LD_LastHeartbeat = new ZDateTime(currentYear, 10, 10);
			db1.LD_LicenceExpiry = expiryTime;
			db1.LD_HostServerSID = new ZGuid("31BA1E9A-A8AA-4D1D-8473-8E77765FF84C");
			db1.LD_HostDBInstance = "INSTANC1";
			db1.LD_HostDBName = "HOSTDB1";
			db1.LD_LicenceType = DatabaseTypes.Codes.Test;
			db1.LD_DBServerSecurityMode = db1.Lookups.DatabaseSecurityModesList[0].Code;
			db1.LD_PublicEmailAddressForUpdate = "test1@email.com";

			var db2 = licHeader2.Database;
			db2.LD_LastHeartbeat = new ZDateTime(currentYear, 10, 11);
			db2.LD_LicenceExpiry = expiryTime;
			db2.LD_HostServerSID = new ZGuid("3D9305C6-61C3-46B4-B9BE-D6E99CF5F5EA");
			db2.LD_HostDBInstance = "INSTANC2";
			db2.LD_HostDBName = "HOSTDB1";
			db2.LD_LicenceType = DatabaseTypes.Codes.Test;
			db2.LD_DBServerSecurityMode = db2.Lookups.DatabaseSecurityModesList[0].Code;
			db2.LD_PublicEmailAddressForUpdate = "test2@email.com";

			var db3 = licHeader3.Database;
			db3.LD_LastHeartbeat = new ZDateTime(currentYear, 10, 12);
			db3.LD_LicenceExpiry = expiryTime;
			db3.LD_HostDBInstance = "INSTANC3";
			db3.LD_PublicEmailAddressForUpdate = "test3@email.com";

			licHeader1.LA_LastLicenceSyncCheck = new ZDateTime(currentYear, 10, 10);
			licHeader2.LA_LastLicenceSyncCheck = new ZDateTime(currentYear, 10, 10);

			List<LicenceDatabase> list = new List<LicenceDatabase>();
			list.Add(db1);
			list.Add(db2);
			list.Add(db3);
			Databases = list.ToArray();

			Factory.Save();
			var sendSystemExpiryAction = new LicenceDatabaseSendSystemExpiryActionForTesting(Databases);
			sendSystemExpiryAction.SendNewSystemShutdownDate();

			AssertEquals(true, db1.Logs.HasLogWith(StmALogSchema.SL_Reference, $"Send new system shutdown date '12/12/{currentYear}' for 'SY1' Server."));
			AssertEquals(true, db2.Logs.HasLogWith(StmALogSchema.SL_Reference, $"Send new system shutdown date '12/12/{currentYear}' for 'SY2' Server."));
			AssertEquals(false, db3.Logs.HasLogWith(StmALogSchema.SL_Reference, $"Send new system shutdown date '12/12/{currentYear}' for 'SY3' Server."));

			AssertContains(String.Format("2 Database(s) successfully sent\r\n1 Database(s) had errors.\r\nFailures shown below <Enterprise Code>-<Server Code> <Error Message>\r\n{0}-{1} Can't send update since server SID is empty. A heartbeat is needed from the client.", db3.LicEnterprise.LE_EnterpriseCode, db3.LD_ServerCode), UnitTestUserNotification.Instance.LastMessage.Text);

			List<LicenceDatabase> dummyList = new List<LicenceDatabase>();
			Factory.Save();
			var dummySendSystemExpiryAction = new LicenceDatabaseSendSystemExpiryActionForTesting(dummyList.ToArray());
			dummySendSystemExpiryAction.SendNewSystemShutdownDate();

			AssertEquals("Please select a Licence Database(s)", UnitTestUserNotification.Instance.LastMessage.Text);

			List<LicenceDatabase> anotherDummyList1 = new List<LicenceDatabase>();
			anotherDummyList1.Add(db1);
			Factory.Save();
			var anotherDummySendSystemExpiryAction1 = new LicenceDatabaseSendSystemExpiryActionForTesting(anotherDummyList1.ToArray());
			anotherDummySendSystemExpiryAction1.SendNewSystemShutdownDate();

			AssertContains("Shutdown date updated successfully", UnitTestUserNotification.Instance.LastMessage.Text);

			List<LicenceDatabase> anotherDummyList2 = new List<LicenceDatabase>();
			anotherDummyList2.Add(db3);
			Factory.Save();
			var anotherDummySendSystemExpiryAction2 = new LicenceDatabaseSendSystemExpiryActionForTesting(anotherDummyList2.ToArray());
			anotherDummySendSystemExpiryAction2.SendNewSystemShutdownDate();

			AssertContains("Can't send update since server SID is empty. A heartbeat is needed from the client.", UnitTestUserNotification.Instance.LastMessage.Text);
			ErrorReporter.Clear();
		}

		class LicenceDatabaseSendSystemExpiryActionForTesting : LicenceDatabaseSendSystemExpiryAction
		{
			public LicenceDatabaseSendSystemExpiryActionForTesting(LicenceDatabase[] databaseList)
				: base(databaseList)
			{
			}

			protected override SendNewSystemShutdownDateForm GetNewShutdownDateForm(SystemShutdownDate systemShutDownDate)
			{
				systemShutDownDate.NewSystemShutdownDate = new ZDateTime(ZDateTime.Now.Year, 12, 12);
				var result = base.GetNewShutdownDateForm(systemShutDownDate);
				result.Shown += (object sender, EventArgs e) =>
				{
					result.SendButton.PerformClick();
				};
				return result;
			}
		}
	}
}
