using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(MoveDatabasesToNewEnterpriseBizObj))]
	public class MoveDatabasesToNewEnterpriseBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMoveDatabasesToNewEnterprise()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "ENT");
			var defaultEnterpriseID = licHeader.Database.EnterpriseID;
			var db1 = licHeader.Database.LicEnterprise.Databases.AddNew();
			db1.LD_ServerCode = "DB1";
			db1.LD_Product = "ABU";
			db1.LD_TenantID = "";
			var db2 = licHeader.Database.LicEnterprise.Databases.AddNew();
			db2.LD_ServerCode = "DB2";
			db2.LD_Product = "ABU";
			db2.LD_TenantID = "ABU_Ref222";
			db2.LD_Status = "NON";
			db2.LD_AllowAutoLogin = true;

			var licHeader2 = BillingTestHelper.CreateLicence(Factory, "EN2");
			licHeader2.Database.LD_ServerCode = "DB2";

			Factory.Save();

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultEnterpriseID);

			var logger = new DetailedLoggerForTest();
			var bizObj = new MoveDatabasesToNewEnterpriseBizObj(Factory, new[] { db1.PK, db2.PK }, logger);
			AssertEquals("REG", bizObj.RegistrationStatus);
			AssertEquals(true, bizObj.AllowWebAutoLogin);
			bizObj.LicenceEnterpriseID = licHeader2.Database.EnterpriseID;
			bizObj.MoveDatabasesToNewEnterprise();

			var newFactory = new BusinessObjectFactory();
			var db1InNewFactory = newFactory.Load<LicenceDatabase>(db1.PK);
			var db2InNewFactory = newFactory.Load<LicenceDatabase>(db2.PK);

			AssertEquals(licHeader.Database.LD_LE, db1InNewFactory.LD_LE);
			AssertEquals(licHeader.Database.LD_LE, db2InNewFactory.LD_LE);
			AssertEquals("DB1", db1InNewFactory.LD_ServerCode);
			AssertEquals("DB2", db2InNewFactory.LD_ServerCode);
			AssertEquals(@"The following database(s) don't meet the requirements, you might have to move them manually.

Please use the filter 'Databases Not registered with an Tenant ID' to list applicable databases.

Operation terminated, no databases have been moved.

ServerCode	Product	TenantID
DB1		ABU	
", string.Join("\r\n", logger.Logs.Where(x => x.Item1 == LogType.Error).Select(x => x.Item2)));

			logger = new DetailedLoggerForTest();
			bizObj = new MoveDatabasesToNewEnterpriseBizObj(Factory, new[] { db2.PK }, logger);
			bizObj.LicenceEnterpriseID = licHeader2.Database.EnterpriseID;
			bizObj.RegistrationStatus = "PRE";
			bizObj.AllowWebAutoLogin = false;
			bizObj.MoveDatabasesToNewEnterprise();

			newFactory = new BusinessObjectFactory();
			var leInNewFactory = newFactory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, bizObj.LicenceEnterpriseID));
			db1InNewFactory = newFactory.Load<LicenceDatabase>(db1.PK);
			db2InNewFactory = newFactory.Load<LicenceDatabase>(db2.PK);

			AssertEquals(licHeader.Database.LD_LE, db1InNewFactory.LD_LE);
			AssertEquals(leInNewFactory.PK, db2InNewFactory.LD_LE);
			AssertEquals("DB1", db1InNewFactory.LD_ServerCode);
			AssertEquals("CBB", db2InNewFactory.LD_ServerCode);
			AssertEquals("PRE", db2InNewFactory.LD_Status);
			AssertEquals(false, db2InNewFactory.LD_AllowAutoLogin);
			AssertEquals(leInNewFactory.LE_OH, db2InNewFactory.LD_OH_WebAccessOrg);
			Assert(leInNewFactory.Organisation.LicCompany.LicDatabases.Contains(db2InNewFactory));
			Assert(leInNewFactory.Databases.Contains(db2InNewFactory));
			AssertEquals(@"Operation was successful, please reopen all related forms to view the changes.", string.Join("\r\n", logger.Logs.Where(x => x.Item1 == LogType.Information).Select(x => x.Item2)));
		}

		public void TestValidations()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "ENT");
			Factory.Save();

			var obj = (MoveDatabasesToNewEnterpriseBizObj)GetNewBusinessObject();

			AssertEquals("REG", obj.RegistrationStatus);
			obj.RegistrationStatus = "";
			AssertHasError(obj.RegistrationStatusInfo, "Please enter a Registration.");
			obj.RegistrationStatus = "@@@";
			AssertHasError(obj.RegistrationStatusInfo, "Enter a valid Registration.");
			obj.RegistrationStatus = "PRE";
			AssertNoErrors(obj.RegistrationStatusInfo);

			obj.LicenceEnterpriseID = "123";
			AssertEquals("", obj.LicenceEnterpriseID);
			AssertHasError(obj.LicenceEnterpriseIDInfo, "Please enter an Enterprise.");
			obj.LicenceEnterpriseID = licHeader.Database.EnterpriseID;
			AssertNoErrors(obj.LicenceEnterpriseIDInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var newBizO = new MoveDatabasesToNewEnterpriseBizObj(Factory, Enumerable.Empty<ZGuid>(), new LoggerForTest());
			return newBizO;
		}
	}
}
