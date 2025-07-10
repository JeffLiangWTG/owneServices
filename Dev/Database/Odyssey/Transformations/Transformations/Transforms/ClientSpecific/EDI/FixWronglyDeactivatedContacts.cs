using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	public class FixWronglyDeactivatedContacts : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Fix edge case deactivated contacts from CleanUpWTAOnlyContacts";

		protected override void OfflinePostUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, "LicenceDatabase") || !DbObjectCreator.TableExists(Db.Connection, "EdiCustomerUserAccount"))
			{
				return;
			}
			var statement = @"
UPDATE contactToUpdate
SET OC_IsActive = 1, OC_SystemLastEditTimeUtc = GETUTCDATE(), OC_SystemLastEditUser = '~BP'
FROM dbo.OrgContact contactToUpdate
WHERE contactToUpdate.OC_IsActive = 0
AND NOT EXISTS (
    SELECT 1
    FROM dbo.OrgContact duplicateContact
    WHERE duplicateContact.OC_OH = contactToUpdate.OC_OH
	AND duplicateContact.OC_PK != contactToUpdate.OC_PK
    AND duplicateContact.OC_Email != ''
    AND duplicateContact.OC_Email = contactToUpdate.OC_Email
    AND duplicateContact.OC_IsActive = 1
    AND duplicateContact.OC_WebAccessEnabled = 1
)
AND
contactToUpdate.OC_PK IN (
	SELECT EUA_OC_WebAccessContact 
	FROM EdiCustomerUserAccount activeAccount
	JOIN LicenceDatabase activeAccountLD ON EUA_LD = LD_PK WHERE LD_Product <> 'WTA' AND EUA_IsActive = 1
)
AND contactToUpdate.OC_SystemLastEditTimeUtc >= '2025-03-19'
AND
(
	contactToUpdate.OC_PK = ANY (
		SELECT EUA_OC_WebAccessContact FROM [dbo].[EdiCustomerUserAccount] JOIN [dbo].[LicenceDatabase] ON EUA_LD = LD_PK WHERE LD_Product = 'WTA' AND EUA_IsActive = 1
	)
	AND
	contactToUpdate.OC_PK <> ANY (
		SELECT EUA_OC_WebAccessContact FROM [dbo].[EdiCustomerUserAccount] JOIN [dbo].[LicenceDatabase] ON EUA_LD = LD_PK WHERE LD_Product <> 'WTA' AND EUA_IsActive = 1
	)
	AND
	(
		contactToUpdate.OC_PK = ANY (
			SELECT EUA_OC_WebAccessContact FROM [dbo].[EdiCustomerUserAccount] JOIN [dbo].[LicenceDatabase] ON EUA_LD = LD_PK WHERE LD_Product <> 'WTA' AND EUA_IsActive = 0
		)
		OR
		contactToUpdate.OC_PK = ANY (
			SELECT EUA_OC_WebAccessContact FROM [dbo].[EdiCustomerUserAccount] t1 JOIN [dbo].[LicenceDatabase] ld1 ON t1.EUA_LD = ld1.LD_PK WHERE ld1.LD_Product = 'WTA' AND t1.EUA_IsActive = 1 AND t1.EUA_Email = ANY (
				SELECT EUA_Email FROM [dbo].[EdiCustomerUserAccount] t2 JOIN [dbo].[LicenceDatabase] ld2 ON t2.EUA_LD = ld2.LD_PK WHERE t1.EUA_PK <> t2.EUA_PK AND t2.EUA_IsActive = 0 AND ld2.LD_Product <> 'WTA' AND ld2.LD_OH_WebAccessOrg = contactToUpdate.OC_OH
			)
		)
	)
)
";
			Db.Connection.ExecuteNonQuery(statement);
		}

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				if (DbObjectCreator.TableExists(Db.Connection, "EdiCustomerUserAccount"))
				{
					indexProvider.New("dbo", "EdiCustomerUserAccount")
						.Key("EUA_IsActive", "EUA_OC_WebAccessContact")
						.GetInfo();
				}
				return indexProvider;
			}
		}
	}
}
