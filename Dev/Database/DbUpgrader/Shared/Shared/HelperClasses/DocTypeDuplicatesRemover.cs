using CargoWise.Data;

namespace Enterprise.DbUpgrader.Shared
{
	public class DocTypeDuplicatesRemover
	{
		/// <summary>
		/// Delete the least restrictive groupings to prevent duplicates (which would violate the unique index)
		/// </summary>
		public void RemoveDocTypeDuplicates(DbConnection conn)
		{
			conn.ExecuteNonQuery(removeDuplicatesSql);
		}

		const string removeDuplicatesSql = @"
			IF (OBJECT_ID('tempdb..#DocTypesToKeep') IS NOT NULL)
			BEGIN
				DROP TABLE #DocTypesToKeep
			END

			CREATE TABLE #DocTypesToKeep
			(
				ReferenceType varchar(3) COLLATE database_default,
				DocType varchar(4) COLLATE database_default,
				PkToKeep uniqueidentifier
			)

			INSERT INTO #DocTypesToKeep (ReferenceType, DocType)
				SELECT RT_ReferenceType, RT_DocType
				FROM dbo.RefDocType
				GROUP BY RT_ReferenceType, RT_DocType
				HAVING count(*) > 1

			UPDATE #DocTypesToKeep
				SET PkToKeep =
				(
					SELECT TOP 1 RT_PK
					FROM dbo.RefDocType
					WHERE
						RefDocType.RT_ReferenceType = ReferenceType
						AND RefDocType.RT_DocType = DocType
					ORDER BY
						RT_IsSystem DESC, -- 'Y' overrides 'N'
						RT_IsActive DESC, -- 'Y' overrides 'N'
						RT_IsPublished ASC, -- 'N' overrides 'Y'
						RT_ForceUserToRead DESC, -- 'Y' overrides 'N'
						RT_IsPublishUpdatable DESC, -- 'Y' overrides 'N'
						RT_PK ASC -- The same PK should prevail in any DB when all fields above match
				)

			UPDATE dbo.StmMenuTemplatePivot
				SET SI_RT_DocType = DocTypesToKeep.PkToKeep
				FROM
					dbo.StmMenuTemplatePivot
					INNER JOIN dbo.RefDocType ON RT_PK = SI_RT_DocType
					INNER JOIN #DocTypesToKeep DocTypesToKeep
						ON ReferenceType = RefDocType.RT_ReferenceType
						AND DocType = RefDocType.RT_DocType
				WHERE
					SI_RT_DocType != DocTypesToKeep.PkToKeep

			UPDATE dbo.StmMenuEDocs
				SET SX_RT_DocType = DocTypesToKeep.PkToKeep
				FROM
					dbo.StmMenuEDocs
					INNER JOIN dbo.RefDocType ON RT_PK = SX_RT_DocType
					INNER JOIN #DocTypesToKeep DocTypesToKeep
						ON ReferenceType = RefDocType.RT_ReferenceType
						AND DocType = RefDocType.RT_DocType
				WHERE
					SX_RT_DocType != DocTypesToKeep.PkToKeep

			DELETE dbo.RefDocType
				FROM
					dbo.RefDocType
					INNER JOIN #DocTypesToKeep DocTypesToKeep
						ON ReferenceType = RefDocType.RT_ReferenceType
						AND DocType = RefDocType.RT_DocType
				WHERE
					RT_PK != DocTypesToKeep.PkToKeep

			IF (OBJECT_ID('tempdb..#DocTypesToKeep') IS NOT NULL)
			BEGIN
				DROP TABLE #DocTypesToKeep
			END";
	}
}
