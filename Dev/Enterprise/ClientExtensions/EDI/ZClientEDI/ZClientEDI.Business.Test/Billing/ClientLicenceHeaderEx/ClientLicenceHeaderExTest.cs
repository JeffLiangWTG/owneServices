using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceHeaderEx))]
	internal class ClientLicenceHeaderExTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLicHeader()
		{
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			ClientLicenceHeaderEx billing = lic.Billing;
			AssertEquals(lic.PK, billing.LicHeader.PK);
		}

		public void TestSetDefaultValues()
		{
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			ClientLicenceHeaderEx billing = lic.Billing;
			AssertEquals(30m, billing.L0_NextNewSeatMaintenancePercent);
			AssertEquals(30m, billing.L0_LastNewSeatMaintenancePercent);
		}

		public void TestPropertiesReadOnly()
		{
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			ClientLicenceHeaderEx billing = lic.Billing;

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			foreach (ZPropertyInfo propertyInfo in billing.ZPropertyInfoHash)
			{
				bool expectReadonly = false;
				AssertEquals(propertyInfo.Name, expectReadonly, propertyInfo.ReadOnly);
			}

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			foreach (ZPropertyInfo propertyInfo in billing.ZPropertyInfoHash)
			{
				AssertEquals(propertyInfo.Name, true, propertyInfo.ReadOnly);
			}

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			lic.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			foreach (ZPropertyInfo propertyInfo in billing.ZPropertyInfoHash)
			{
				AssertEquals(propertyInfo.Name, false, propertyInfo.ReadOnly);
			}

			billing.L0_FixedMaintenanceAmount = 10m;
			foreach (ZPropertyInfo propertyInfo in billing.ZPropertyInfoHash)
			{
				bool expectReadonly = propertyInfo.Name == ClientLicenceHeaderExSchema.Constants.L0_NextMaintenancePercent
					|| propertyInfo.Name == ClientLicenceHeaderExSchema.Constants.L0_NextNewSeatMaintenancePercent;
				AssertEquals(propertyInfo.Name, expectReadonly, propertyInfo.ReadOnly);
			}
		}

		public void TestSynchronize()
		{
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			Factory.Save();

			AssertNull("child not created yet", lic.ReadonlyBilling);

			lic.Billing.L0_RenewalMonths = 6;

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			LicenceHeader licReloaded = factory2.Load<LicenceHeader>(lic.PK);
			AssertNull("factory2 child not created yet", licReloaded.ReadonlyBilling);

			licReloaded.Billing.L0_RenewalMonths = 1;

			factory2.Save();
			Factory.Save();

			AssertEquals("child synchronized", lic.Billing.PK, licReloaded.Billing.PK);
			AssertEquals("last save wins", 6, licReloaded.Billing.L0_RenewalMonths);
			AssertEquals("last save wins", 6, lic.Billing.L0_RenewalMonths);

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			ClientLicenceHeaderEx[] allChildren = factory3.Load<ClientLicenceHeaderEx>(new ZQuery(ClientLicenceHeaderExSchema.L0_LA, lic.PK));
			AssertEquals("only 1 child in DB", 1, allChildren.Length);
		}

		[TestDate(2010, 1, 1)]
		public void TestLogChanges()
		{
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			lic.Billing.L0_LastMaintenancePercent = 5m;
			lic.Billing.L0_RenewalMonths = 6;
			Factory.Save();

			ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Maintenance");
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			StmALog[] logs = lic.Logs.Find(query);
			AssertEquals("No log for new record", 0, logs.Length);

			lic.Billing.L0_LastMaintenancePercent = 15m;
			lic.Billing.L0_LastNewSeatMaintenancePercent = 20m;
			lic.Billing.L0_NextMaintenancePercent = 25m;
			lic.Billing.L0_NextNewSeatMaintenancePercent = 29m;

			Factory.Save();
			logs = lic.Logs.Find(query);
			AssertEquals("Should be one log", 1, logs.Length);

			string expected = "Maintenance | Last%:5=>15 | LastNew%:30=>20 | Next%:0=>25 | NextNew%:30=>29";
			AssertEquals(expected, logs[0].SL_Reference);
			AssertEquals(Events.EditedARecordCode, logs[0].SL_SE_NKEvent);

			TestDateAttribute.Date = new DateTime(2010, 1, 2);
			lic.Billing.L0_FixedMaintenanceAmount = 5000m;
			lic.Billing.L0_RX_NKFixedMaintenanceCurrency = "AUD";
			Factory.Save();
			logs = lic.Logs.Find(query);
			AssertEquals("Should be another log", 2, logs.Length);
			expected = "Maintenance Fixed | Amount:0=>5000 | Currency:=>AUD";
			AssertEquals(expected, logs[0].SL_Reference);
			AssertEquals(Events.EditedARecordCode, logs[0].SL_SE_NKEvent);
		}

		public void TestSynchronize_ParentDeleted()
		{
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			Factory.Save();

			lic.Billing.L0_RenewalMonths = 6;
			lic.Delete();

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ClientLicenceHeaderEx[] all = factory2.Load<ClientLicenceHeaderEx>(new ZQuery());
			AssertEquals(0, all.Length);
		}
	}
}
