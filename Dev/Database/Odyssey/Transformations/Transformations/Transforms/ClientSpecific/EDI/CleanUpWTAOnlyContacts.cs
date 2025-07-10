using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	public class CleanUpWTAOnlyContacts : DataTransformation
	{
		public override string UserDescription => "Clean up contacts where only linked account is from WTA";

		protected override void OnlinePreUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, "LicenceDatabase") || !DbObjectCreator.TableExists(Db.Connection, "EdiCustomerUserAccount"))
			{
				return;
			}
			DbObjectCreator.CreateIndexIfNotExists(Db.Connection, "EdiCustomerUserAccount", "NR_UX__EUA_IsActive_EUA_OC_WebAccessContact", @"
CREATE INDEX NR_UX__EUA_IsActive_EUA_OC_WebAccessContact ON dbo.EdiCustomerUserAccount (EUA_IsActive, EUA_OC_WebAccessContact) INCLUDE (EUA_Email, EUA_PK)");
		}

		protected override void OfflinePostUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, "LicenceDatabase") || !DbObjectCreator.TableExists(Db.Connection, "EdiCustomerUserAccount"))
			{
				return;
			}
			var statement = @"
UPDATE [dbo].[OrgContact]
SET OC_IsActive = 0, OC_SystemLastEditTimeUtc = GETUTCDATE(), OC_SystemLastEditUser = '~BP'
WHERE OC_IsActive = 1
AND
OC_PK = ANY (
	SELECT EUA_OC_WebAccessContact FROM [dbo].[EdiCustomerUserAccount] JOIN [dbo].[LicenceDatabase] ON EUA_LD = LD_PK WHERE LD_Product = 'WTA' AND EUA_IsActive = 1
)
AND
OC_PK <> ANY (
	SELECT EUA_OC_WebAccessContact FROM [dbo].[EdiCustomerUserAccount] JOIN [dbo].[LicenceDatabase] ON EUA_LD = LD_PK WHERE LD_Product <> 'WTA' AND EUA_IsActive = 1
)
AND
(
	OC_PK = ANY (
		SELECT EUA_OC_WebAccessContact FROM [dbo].[EdiCustomerUserAccount] JOIN [dbo].[LicenceDatabase] ON EUA_LD = LD_PK WHERE LD_Product <> 'WTA' AND EUA_IsActive = 0
	)
	OR
	OC_PK = ANY (
		SELECT EUA_OC_WebAccessContact FROM [dbo].[EdiCustomerUserAccount] t1 JOIN [dbo].[LicenceDatabase] ld1 ON t1.EUA_LD = ld1.LD_PK WHERE ld1.LD_Product = 'WTA' AND t1.EUA_IsActive = 1 AND t1.EUA_Email = ANY (
			SELECT EUA_Email FROM [dbo].[EdiCustomerUserAccount] t2 JOIN [dbo].[LicenceDatabase] ld2 ON t2.EUA_LD = ld2.LD_PK WHERE t1.EUA_PK <> t2.EUA_PK AND t2.EUA_IsActive = 0 AND ld2.LD_Product <> 'WTA' AND ld2.LD_OH_WebAccessOrg = OC_OH
		)
	)
)
";
			Db.Connection.ExecuteNonQuery(statement);
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (!DbObjectCreator.TableExists(Db.Connection, "LicenceDatabase") || !DbObjectCreator.TableExists(Db.Connection, "EdiCustomerUserAccount"))
			{
				return;
			}
			Db.Connection.ExecuteNonQuery("DROP INDEX IF EXISTS NR_UX__EUA_IsActive_EUA_OC_WebAccessContact ON [dbo].[EdiCustomerUserAccount]");
		}
	}
}
