using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared
{
	public sealed class DocTypeDuplicatesRemoverTest : TransactionedTestCase
	{
		public void TestRemoveDocTypeDuplicates()
		{
			Guid docTypePk_SCL_PKD_Original = GetRefDocTypePk("SCL", "PKD");
			Guid docTypePk_ALL_PRV_Original = GetRefDocTypePk("ALL", "PRV");

			Guid docTypePk_SCL_PKD_Test = Guid.NewGuid();
			Guid docTypePk_ALL_PRV_Test = Guid.NewGuid();

			Guid stmMenuItemPk = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 SU_PK FROM dbo.StmMenuItem WHERE SU_MenuType = 'DOC'");
			Guid stmTemplatePk = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 SO_PK FROM dbo.StmTemplate");

			int menuEDocsCount_SCL_PKD_Original = GetMenuEDocsRowCount(docTypePk_SCL_PKD_Original);
			int menuEDocsCount_ALL_PRV_Original = GetMenuEDocsRowCount(docTypePk_ALL_PRV_Original);
			int stmMenuTemplatePivotCount_SCL_PKD_Original = GetStmMenuTemplatePivotRowCount(docTypePk_SCL_PKD_Original);
			int stmMenuTemplatePivotCount_ALL_PRV_Original = GetStmMenuTemplatePivotRowCount(docTypePk_ALL_PRV_Original);

			Db.Connection.ExecuteNonQuery("DROP INDEX RefDocType.NR_UC__RT_ReferenceType_RT_DocType");

			string sqlText = String.Format(@"
				INSERT dbo.RefDocType (RT_PK, RT_ReferenceType, RT_DocType, RT_Desc, RT_IsActive, RT_IsSystem) VALUES ('{0}', 'SCL', 'PKD', 'Test_SCL_PKD', 1, 0)
				INSERT dbo.RefDocType (RT_PK, RT_ReferenceType, RT_DocType, RT_Desc, RT_IsActive, RT_IsSystem) VALUES ('{1}', 'ALL', 'PRV', 'Test_ALL_PRV', 1, 0)
				INSERT dbo.StmMenuEDocs (SX_PK, SX_SU, SX_RT_DocType, SX_IsSystemDefined) VALUES ('4841DE6D-3F30-4713-9368-58213BBAED30', '{2}', '{0}', 0)
				INSERT dbo.StmMenuEDocs (SX_PK, SX_SU, SX_RT_DocType, SX_IsSystemDefined) VALUES ('97A2CACB-8689-41BE-9B6B-D8FA2CAB7A80', '{2}', '{1}', 0)
				INSERT dbo.StmMenuTemplatePivot (SI_PK, SI_SU, SI_SO, SI_RT_DocType, SI_IsSystemDefined) VALUES('C6967639-85C8-4F70-A7CE-CD780B75B5A4', '{2}', '{3}', '{0}', 0);
				",
				docTypePk_SCL_PKD_Test.ToString(), docTypePk_ALL_PRV_Test.ToString(), stmMenuItemPk.ToString(), stmTemplatePk.ToString());
			Db.Connection.ExecuteNonQuery(sqlText);

			AssertEquals("[PRE-CONDITION] SCL-PKD count", 2, GetRefDocTypeRowCount("SCL", "PKD"));
			AssertEquals("[PRE-CONDITION] ALL-PRV count", 2, GetRefDocTypeRowCount("ALL", "PRV"));

			AssertEquals("[PRE-CONDITION] StmMenuEDocs FK to SCL-PKD", 1, GetMenuEDocsRowCount(docTypePk_SCL_PKD_Test));
			AssertEquals("[PRE-CONDITION] StmMenuEDocs FK to ALL-PRV", 1, GetMenuEDocsRowCount(docTypePk_ALL_PRV_Test));
			AssertEquals("[PRE-CONDITION] StmMenuTemplatePivot FK to SCL-PKD", 1, GetStmMenuTemplatePivotRowCount(docTypePk_SCL_PKD_Test));
			AssertEquals("[PRE-CONDITION] StmMenuTemplatePivot FK to ALL-PRV", 0, GetStmMenuTemplatePivotRowCount(docTypePk_ALL_PRV_Test));

			// Remove duplicates
			new DocTypeDuplicatesRemover().RemoveDocTypeDuplicates(Db.Connection);

			// Unique index should be able to be recreated now
			Db.Connection.ExecuteNonQuery("CREATE UNIQUE NONCLUSTERED INDEX NR_UC__RT_ReferenceType_RT_DocType ON RefDocType (RT_ReferenceType, RT_DocType)");

			AssertEquals("SCL-PKD PK (After)", docTypePk_SCL_PKD_Original, GetRefDocTypePk("SCL", "PKD"));
			AssertEquals("ALL-PRV PK (After)", docTypePk_ALL_PRV_Original, GetRefDocTypePk("ALL", "PRV"));

			AssertEquals("StmMenuEDocs FK to Test SCL-PKD (After)", 0, GetMenuEDocsRowCount(docTypePk_SCL_PKD_Test));
			AssertEquals("StmMenuEDocs FK to Test ALL-PRV (After)", 0, GetMenuEDocsRowCount(docTypePk_ALL_PRV_Test));
			AssertEquals("StmMenuTemplatePivot FK to SCL-PKD (After)", 0, GetStmMenuTemplatePivotRowCount(docTypePk_SCL_PKD_Test));
			AssertEquals("StmMenuTemplatePivot FK to ALL-PRV (After)", 0, GetStmMenuTemplatePivotRowCount(docTypePk_ALL_PRV_Test));

			AssertEquals("StmMenuEDocs FK to Original SCL-PKD (After)", menuEDocsCount_SCL_PKD_Original + 1, GetMenuEDocsRowCount(docTypePk_SCL_PKD_Original));
			AssertEquals("StmMenuEDocs FK to Original ALL-PRV (After)", menuEDocsCount_ALL_PRV_Original + 1, GetMenuEDocsRowCount(docTypePk_ALL_PRV_Original));
			AssertEquals("StmMenuTemplatePivot FK to SCL-PKD (After)", stmMenuTemplatePivotCount_SCL_PKD_Original + 1, GetStmMenuTemplatePivotRowCount(docTypePk_SCL_PKD_Original));
			AssertEquals("StmMenuTemplatePivot FK to ALL-PRV (After)", stmMenuTemplatePivotCount_ALL_PRV_Original, GetStmMenuTemplatePivotRowCount(docTypePk_ALL_PRV_Original));
		}

		public void TestRefDocTypeFksDidNotChange()
		{
			AssertEquals("Number of FKs to RefDocType", 2, Db.Connection.ExecuteScalar<int>("select count(*) from sys.foreign_keys where referenced_object_id = object_id('RefDocType')"));
			AssertEquals("StmMenuEDocs_SX_RT_DocType_FK2_RefDocType_RRR_120N Exists", true, DoesGivenFkToRefDocTypeExist("StmMenuEDocs_SX_RT_DocType_FK2_RefDocType_RRR_120N"));
			AssertEquals("StmMenuTemplatePivot_SI_RT_DocType_FK2_RefDocType_RRR_120N Exists", true, DoesGivenFkToRefDocTypeExist("StmMenuTemplatePivot_SI_RT_DocType_FK2_RefDocType_RRR_120N"));
		}

		Guid GetRefDocTypePk(string referenceType, string docType)
		{
			string sqlText = String.Format(
				"SELECT RT_PK FROM dbo.RefDocType WHERE RT_ReferenceType = '{0}' AND RT_DocType = '{1}'",
				referenceType, docType);
			Guid result = (Guid)Db.Connection.ExecuteScalar(sqlText);
			return result;
		}

		int GetRefDocTypeRowCount(string referenceType, string docType)
		{
			string sqlText = String.Format(
				"SELECT count(*) FROM dbo.RefDocType WHERE RT_ReferenceType = '{0}' AND RT_DocType = '{1}'",
				referenceType, docType);
			int result = (int)Db.Connection.ExecuteScalar(sqlText);
			return result;
		}

		int GetMenuEDocsRowCount(Guid docTypePk)
		{
			string sqlText = String.Format("SELECT count(*) FROM dbo.StmMenuEDocs WHERE SX_RT_DocType = '{0}'", docTypePk.ToString());
			int result = (int)Db.Connection.ExecuteScalar(sqlText);
			return result;
		}

		int GetStmMenuTemplatePivotRowCount(Guid docTypePk)
		{
			string sqlText = String.Format("SELECT count(*) FROM dbo.StmMenuTemplatePivot WHERE SI_RT_DocType = '{0}'", docTypePk.ToString());
			int result = (int)Db.Connection.ExecuteScalar(sqlText);
			return result;
		}

		bool DoesGivenFkToRefDocTypeExist(string fkName)
		{
			string sqlText = String.Format(@"
				SELECT
					COUNT(*)
				FROM
					SYS.FOREIGN_KEYS
				WHERE
					REFERENCED_OBJECT_ID = OBJECT_ID('RefDocType')
					AND NAME = '{0}'
				", fkName);
			return Db.Connection.ExecuteScalar<int>(sqlText) == 1;
		}
	}
}
