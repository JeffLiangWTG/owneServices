using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Security.Testing
{
	sealed class UserLoginControllerWithoutSetupTest : TransactionedTestCase
	{
		public void TestValidateAndRegisterUserForUpgrade()
		{
			UserLoginController controller = new UserLoginController();

			string errorMessage;
			bool loginOK = controller.ValidateAndRegisterUserForUpgrade(User.SupportUserName, CWSupportLoginToken.TokenForTest, out errorMessage);
			Assert("Developer login should exist", loginOK);

			loginOK = controller.ValidateAndRegisterUserForUpgrade(User.SupportUserName, "wrongpassword", out errorMessage);
			Assert("Developer login with a wrong password should have failed", !loginOK);
		}

		public void TestValidateAndRegisterUserForUpgradeWithActiveDirectoryTurnedOn()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, User.SupportUserName);
			staff.GS_ActiveDirectoryObjectGuid = new ZGuid(staff.PK);
			factory.Save();

			UserLoginController controller = new UserLoginController();

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			string errorMessage;
			bool loginOK = controller.ValidateAndRegisterUserForUpgrade(User.SupportUserName, CWSupportLoginToken.TokenForTest, out errorMessage);
			Assert("CWSupport login should exist", loginOK);
		}

		/// <summary>
		/// Drops a columns from dbo.GlbStaff and call TestValidateUserLoginAndPassword
		/// </summary>
		public void TestValidateAndRegisterUserForUpgradeWorksWithMissingGlbStaffFields()
		{
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(Db.Connection, Db.SqlDbOwnerSchema, "GlbStaff");
			string sqlText = string.Format(@"
				ALTER TABLE dbo.GlbStaff DROP CONSTRAINT {0};
				ALTER TABLE dbo.GlbStaff DROP COLUMN GS_EmploymentBasis;",
				GetColumnDefaultConstraintName("GlbStaff", "GS_EmploymentBasis"));
			TestConnection.ExecuteNonQuery(sqlText);

			TestValidateAndRegisterUserForUpgrade();
		}

		public void TestValidateAndRegisterUserForUpgradeWorksWithCharBooleanColumns()
		{
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS_IsActive");
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS_IsController");
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS_IsSystemAccount");
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, "GlbStaff", "GS_PER");

			string sqlText = string.Format(@"
				ALTER TABLE dbo.GlbStaff DROP CONSTRAINT {0};	
				ALTER TABLE dbo.GlbStaff DROP CONSTRAINT {1};
				ALTER TABLE dbo.GlbStaff DROP CONSTRAINT {2};
				ALTER TABLE dbo.GlbStaff DROP CONSTRAINT {3};
				ALTER TABLE dbo.GlbStaff DROP CONSTRAINT Constraint_GS_PER;
				ALTER TABLE dbo.GlbStaff ALTER COLUMN GS_IsActive CHAR(1);
				ALTER TABLE dbo.GlbStaff ALTER COLUMN GS_IsController CHAR(1);
				ALTER TABLE dbo.GlbStaff ALTER COLUMN GS_IsSystemAccount CHAR(1);
				UPDATE dbo.GlbStaff set GS_IsActive = 'Y', GS_IsController = 'Y', GS_IsSystemAccount = 'Y', GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E';",
				GetColumnDefaultConstraintName("GlbStaff", "GS_IsActive"),
				GetColumnDefaultConstraintName("GlbStaff", "GS_IsController"),
				GetColumnDefaultConstraintName("GlbStaff", "GS_IsSystemAccount"),
				"Constraint_GS_IsDevice");
			TestConnection.ExecuteNonQuery(sqlText);

			TestValidateAndRegisterUserForUpgrade();
		}

		public void TestValidateAndRegisterUserForUpgradeWorksOldGlbStaffSchema()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, User.SupportUserName);
			staff.GS_ActiveDirectoryObjectGuid = new ZGuid(staff.PK);
			factory.Save();

			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "GlbStaff", "GS_GC_PreferredPaymentCompany").DropRelateObjects(TestConnection);
			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "GlbStaff", "GS_IsOperational").DropRelateObjects(TestConnection);

			string sqlText = @"
				ALTER TABLE dbo.GlbStaff DROP COLUMN GS_GC_PreferredPaymentCompany;
				ALTER TABLE dbo.GlbStaff DROP COLUMN GS_IsOperational;";
			TestConnection.ExecuteNonQuery(sqlText);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			TestValidateAndRegisterUserForUpgrade();
		}

		public void TestValidateAndRegisterUserForUpgradeWorksOldGlbStaffSchemaWithoutGS_ActiveDirectoryObjectGuid()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, User.SupportUserName);
			staff.GS_ActiveDirectoryObjectGuid = new ZGuid(staff.PK);
			factory.Save();

			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "GlbStaff", "GS_ActiveDirectoryObjectGuid").DropRelateObjects(TestConnection);

			string sqlText = @"
				ALTER TABLE dbo.GlbStaff DROP COLUMN GS_ActiveDirectoryObjectGuid;";
			TestConnection.ExecuteNonQuery(sqlText);

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			TestValidateAndRegisterUserForUpgrade();
		}

		string GetColumnDefaultConstraintName(string tableName, string columnName)
		{
			string sqlString = string.Format(@"SELECT
	constobj.name ConstName
FROM
	sys.columns col
	INNER JOIN sys.tables tab ON tab.object_id = col.object_id
	INNER JOIN sys.default_constraints constobj
		ON constobj.parent_object_id = tab.object_id AND constobj.parent_column_id = col.column_id
WHERE
	tab.name = '{0}'
	AND col.name = '{1}'
", tableName, columnName);

			return (string)Db.Connection.ExecuteScalar(sqlString);
		}
	}
}
