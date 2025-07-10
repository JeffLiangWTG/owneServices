using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.GB;
public class EnableAllApplicationsToExistingTokens : DataTransformation
{
	public override string UserDescription => "Enable All Applications to Existing Tokens";

	protected override void OfflinePostUpgradeTransform()
	{
		var sql = @"
				DELETE GAC
					FROM [dbo].[GenAddOnColumn] GAC
					INNER JOIN [dbo].[GlbExternalPassword] GP ON GAC.XA_ParentID = GP.GP_PK
					INNER JOIN [dbo].[GlbCompany] GC ON GP.GP_GC = GC.GC_PK
					WHERE GAC.XA_Name IN ('IsTokenForCDS', 'IsTokenForEMCS', 'IsTokenForGVMS', 'IsTokenForNCTS', 'IsTokenForSnSGB')
					AND GC.GC_RN_NKCountryCode = 'GB';

				INSERT INTO [dbo].[GenAddOnColumn] (
					[XA_PK],
					[XA_Name],
					[XA_Type],
					[XA_ParentTableCode],
					[XA_ParentID],
					[XA_Data],
					[XA_SystemCreateTimeUtc],
					[XA_SystemCreateUser],
					[XA_SystemLastEditTimeUtc],
					[XA_SystemLastEditUser],
					[XA_AutoVersion]
				)
				SELECT 
					NEWID() AS XA_PK,
					tokens.XA_Name,
					'BOO' AS XA_Type,
					'GP' AS XA_ParentTableCode,
					GP.GP_PK AS XA_ParentID,
					'Y' AS XA_Data,
					GETUTCDATE() AS XA_SystemCreateTimeUtc,
					'~BP' AS XA_SystemCreateUser,
					GETUTCDATE() AS XA_SystemLastEditTimeUtc,
					'~BP' AS XA_SystemLastEditUser,
					0 AS XA_AutoVersion
				FROM 
					[dbo].[GlbExternalPassword] GP
				INNER JOIN 
					[dbo].[GlbCompany] GC ON GP.GP_GC = GC.GC_PK
				CROSS JOIN 
					(VALUES 
						('IsTokenForCDS'),
						('IsTokenForEMCS'),
						('IsTokenForGVMS'),
						('IsTokenForNCTS'),
						('IsTokenForSnSGB')
					) AS tokens(XA_Name)
				WHERE 
					GC.GC_RN_NKCountryCode = 'GB';";
		Db.Connection.ExecuteNonQuery(sql);
	}
}
