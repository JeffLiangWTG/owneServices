using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.Schema;
#if DEBUG
using Enterprise.DbUpgrader.Data.Testing;
#endif
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class DocumentsDataFile : EmbeddedDataFile, IFixReferencesAndDuplicates
	{
		public DocumentsDataFile() : this(DataFileRelativePath)
		{
		}

		public DataSet LoadEmptyDataSet()
		{
			DataSet data = LoadDataFromDatabase();
			foreach (DataTable table in data.Tables)
			{
				table.Clear();
			}

			return data;
		}

		public DocumentsDataFile(string fileRelativePath)
			: base(fileRelativePath, DataFileTables)
		{
		}

#if DEBUG
		public override string DefaultDataFileBasePath
		{
			get { return TestFileConstants.DefaultDocumentsDataFileBasePath; }
		}
#endif

		public override void WriteXml(DataSet dataSet, string fileName, XmlWriteMode mode)
		{
			DataTable menuItems = dataSet.Tables[StmMenuItemSchema.Constants.TableName];
			foreach (DataRow row in menuItems.Rows)
			{
				//All system reports are meant to be published on check in
				if (!AllowedUnpublishedSystemReportPks.Contains((Guid)row[StmMenuItemSchema.Constants.PK]))
				{
					row[StmMenuItemSchema.Constants.SU_IsPublished] = true;
				}
				row[StmMenuItemSchema.Constants.SU_GS_NKStaffCode] = string.Empty;
			}
			DataTable configs = dataSet.Tables[StmMenuDocumentConfigSchema.Constants.TableName];
			foreach (DataRow row in configs.Rows)
			{
				row[StmMenuDocumentConfigSchema.Constants.S3_IsSystem] = true;
				row[StmMenuDocumentConfigSchema.Constants.S3_IsClientSpecific] = (IsClientSpecificFlag == "Y");
			}
			DataTable configItems = dataSet.Tables[StmMenuDocumentConfigItemSchema.Constants.TableName];
			foreach (DataRow row in configItems.Rows)
			{
				row[StmMenuDocumentConfigItemSchema.Constants.S4_IsSystemDefined] = true;
				row[StmMenuDocumentConfigItemSchema.Constants.S4_IsClientSpecific] = (IsClientSpecificFlag == "Y");
			}
			base.WriteXml(dataSet, fileName, mode);
		}

		HashSet<Guid> AllowedUnpublishedSystemReportPks
		{
			get
			{
				if (allowedUnpublishedSystemReportPks == null)
				{
					allowedUnpublishedSystemReportPks = new HashSet<Guid>
					{
						Guid.Parse("7d08c248-6143-42b8-b6fc-232e76101da3"),
						Guid.Parse("d0aae50c-c0b9-40af-99b2-35850ee88c43"),
						Guid.Parse("a79a55e7-82ab-4263-97a1-3d8397d87bf8"),
						Guid.Parse("1dd32ff3-453f-4d2e-bd9d-583f73455847"),
						Guid.Parse("442e2ef3-f05d-49d4-890c-c81177714681")
					};
				}

				return allowedUnpublishedSystemReportPks;
			}
		}
		HashSet<Guid> allowedUnpublishedSystemReportPks;

		protected override string SelectQuery
		{
			get
			{
				if (string.IsNullOrEmpty(query))
				{
					string isSystemWhereIsNeverClientSpecific = (IsClientSpecificFlag == "N") ? "1" : "1 and 1=2"; //The 1=2 means that no rows will ever match.
					string isSystemWhereIsNeverClientSpecificLegacy = (IsClientSpecificFlag == "N") ? "1" : "1 and 1=2"; //The 1=2 means that no rows will ever match.

					string stmMenuEDocsQuery = string.Format(@"
						SELECT {0}
						FROM dbo.StmMenuEDocs
						WHERE SX_IsSystemDefined = {1}
						AND SX_SU NOT IN
						(
							SELECT SU_PK
							FROM dbo.StmMenuItem
							WHERE SU_FilterList LIKE '%{2}%'
						)
						ORDER BY SX_PK;",
						GetColumnList(StmMenuEDocsSchema.All), isSystemWhereIsNeverClientSpecific, DocumentsUpgradeTask.EDocsProviderPlaceholderTag);

					string refDocTypeQuery = string.Format(@"
						SELECT {0} 
						FROM dbo.RefDocType 
						WHERE RT_IsSystem = {1}
						ORDER BY RT_PK;",
														GetColumnList(RefDocTypeSchema.All), isSystemWhereIsNeverClientSpecificLegacy);

					string stmMenuDocumentConfigQuery = string.Format(@"
						SELECT {0} 
						FROM dbo.StmMenuDocumentConfig 
						WHERE S3_IsSystem = 1 AND S3_IsClientSpecific = {1}
						ORDER BY S3_PK;",
						GetColumnList(StmMenuDocumentConfigSchema.All), IsClientSpecificFlag == "Y" ? 1 : 0);

					string stmMenuDocumentConfigItemQuery = string.Format(@"
						SELECT {0}
						FROM dbo.StmMenuDocumentConfigItem 
						WHERE S4_IsSystemDefined = 1 AND S4_IsClientSpecific = {1}
						ORDER BY S4_PK;",
						GetColumnList(StmMenuDocumentConfigItemSchema.All), IsClientSpecificFlag == "Y" ? 1 : 0);

					string stmMenuMenuPivotQuery = string.Format(@"
						SELECT {0} 
						FROM dbo.StmMenuMenuPivot 
						WHERE SF_IsSystemDefined = 1 AND SF_IsClientSpecific = {1}
						ORDER BY SF_PK;",
						GetColumnList(StmMenuMenuPivotSchema.All), IsClientSpecificFlag == "Y" ? 1 : 0);

					string stmMenuTemplatePivotQuery = string.Format(@"
						SELECT {0} 
						FROM dbo.StmMenuTemplatePivot 
						WHERE SI_IsSystemDefined = 1 AND SI_IsClientSpecific = {1} 
						ORDER BY SI_PK;",
						GetColumnList(StmMenuTemplatePivotSchema.All), IsClientSpecificFlag == "Y" ? 1 : 0);

					string stmTemplateQuery = string.Format(@"
						SELECT {0}
						FROM dbo.StmTemplate 
						WHERE SO_IsSystemDefined = 1 AND SO_IsClientSpecific = {1}
						ORDER BY SO_PK;",
						StmTemplateSelectList(), IsClientSpecificFlag == "Y" ? 1 : 0);

					string stmMenuItemQuery = string.Format(@"
						SELECT {0} 
						FROM dbo.StmMenuItem 
						WHERE SU_IsSystemDefined = 1 AND SU_IsClientSpecific = {1} AND SU_FilterList NOT LIKE '%{2}%' 
						ORDER BY SU_PK;",
						GetColumnList(StmMenuItemSchema.All), IsClientSpecificFlag == "Y" ? 1 : 0, DocumentsUpgradeTask.EDocsProviderPlaceholderTag);

					string rateAttachmentSetQuery = string.Format(@"
						SELECT {0}
						FROM dbo.RateAttachmentSet
						WHERE TS_IsSystemDefined = 1 AND TS_IsClientSpecific = {1}
						ORDER BY TS_PK;",
						GetColumnList(RateAttachmentSetSchema.All), IsClientSpecificFlag == "Y" ? 1 : 0);

					StringBuilder queryBuilder = new StringBuilder();
					queryBuilder.Append(stmMenuMenuPivotQuery);
					queryBuilder.Append(stmMenuTemplatePivotQuery);
					queryBuilder.Append(stmTemplateQuery);
					queryBuilder.Append(stmMenuItemQuery);
					queryBuilder.Append(stmMenuEDocsQuery);
					queryBuilder.Append(refDocTypeQuery);
					queryBuilder.Append(stmMenuDocumentConfigQuery);
					queryBuilder.Append(stmMenuDocumentConfigItemQuery);
					queryBuilder.Append(rateAttachmentSetQuery);
					query = queryBuilder.ToString();
				}
				return query;
			}
		}
		string query;

		string GetColumnList(SchemaColumnCollection columns)
		{
			StringBuilder strBuilder = new StringBuilder();

			foreach (SchemaColumn column in columns)
			{
				if (column.IsPKColumn)
				{
					strBuilder.Insert(0, string.Format("{0}", column.Name));
				}
				else
				{
					strBuilder.Append(string.Format(", {0}", column.Name));
				}
			}

			return strBuilder.ToString();
		}

		protected virtual string StmTemplateSelectList()
		{
			return GetstmTemplateSelectListExcludingSO_Template();
		}

		string GetstmTemplateSelectListExcludingSO_Template()
		{
			StringBuilder columnWithoutSO_Template = new StringBuilder();
			foreach (SchemaColumn column in StmTemplateSchema.All)
			{
				columnWithoutSO_Template.Append((column.Name == "SO_Template") ? "CAST(NULL AS VARBINARY(max)) AS SO_Template, " : column.Name + ", ");
			}
			if (columnWithoutSO_Template.Length > 2)
			{
				columnWithoutSO_Template.Remove(columnWithoutSO_Template.Length - 2, 2);
			}

			return columnWithoutSO_Template.ToString();
		}

		protected virtual string IsClientSpecificFlag
		{
			get { return "N"; }
		}

		protected override List<UniqueIndexInfo> GetUniqueIndexesToDropBeforeSaveAndRecreateAfterwards()
		{
			var result = new List<UniqueIndexInfo>();
			var indexInfo = new ViewUniqueIndexInfo(RefDocTypeSchema.Constants.TableName, RefDocTypeSchema.Constants.Indexes.NR_UC__RT_ReferenceType_RT_DocType, "RT_ReferenceType, RT_DocType");
			result.Add(indexInfo);

			return result;
		}

		void IFixReferencesAndDuplicates.PerformExtraDataManipulationBeforeEnablingConstraints()
		{
			RemoveDocTypeDuplicates();
			DeleteOrphanPivotLinks();
			NullifyOrphanDocTypeLinks();
			DeleteOrphanDocConfigs();
			DeleteOrphanRateAttachmentSets();
			CheckForMissingParentsInPivots();
		}

		void RemoveDocTypeDuplicates()
		{
			new DocTypeDuplicatesRemover().RemoveDocTypeDuplicates(Db.Connection);
		}

		void NullifyOrphanDocTypeLinks()
		{
			string sqlText = @"
				UPDATE dbo.StmMenuTemplatePivot SET SI_RT_DocType = null WHERE SI_RT_DocType not in (SELECT RT_PK FROM dbo.RefDocType)
				UPDATE dbo.StmMenuEDocs SET SX_RT_DocType = null WHERE SX_RT_DocType not in (SELECT RT_PK FROM dbo.RefDocType)";
			Db.Connection.ExecuteNonQuery(sqlText);
		}

		void DeleteOrphanDocConfigs()
		{
			string sqlText = @"
				DELETE dbo.StmMenuDocumentConfig WHERE S3_SI NOT IN (SELECT SI_PK FROM dbo.StmMenuTemplatePivot);
				DELETE dbo.StmMenuDocumentConfigItem WHERE S4_S3 NOT IN (SELECT S3_PK FROM dbo.StmMenuDocumentConfig);";
			using (DbCommand command = Db.Connection.Command(sqlText))
			{
				command.ExecuteNonQuery();
			}
		}

		void DeleteOrphanPivotLinks()
		{
			string sqlText = @"
				DELETE dbo.StmMenuTemplatePivot WHERE SI_SU not in (SELECT SU_PK FROM dbo.StmMenuItem) AND (SI_IsSystemDefined = 0 OR SI_IsClientSpecific = 1)
				DELETE dbo.StmMenuTemplatePivot WHERE SI_SO not in (SELECT SO_PK FROM dbo.StmTemplate) AND (SI_IsSystemDefined = 0 OR SI_IsClientSpecific = 1)
				DELETE dbo.StmMenuMenuPivot WHERE SF_SU_Outward not in (SELECT SU_PK FROM dbo.StmMenuItem) AND (SF_IsSystemDefined = 0 OR SF_IsClientSpecific = 1)
				DELETE dbo.StmMenuMenuPivot WHERE SF_SU_Inward  not in (SELECT SU_PK FROM dbo.StmMenuItem) AND (SF_IsSystemDefined = 0 OR SF_IsClientSpecific = 1)
				DELETE dbo.StmMenuEDocs WHERE SX_SU not in (SELECT SU_PK FROM dbo.StmMenuItem) AND SX_IsSystemDefined = 0";
			Db.Connection.ExecuteNonQuery(sqlText);
		}

		void DeleteOrphanRateAttachmentSets()
		{
			string sqlText = @"
				DELETE dbo.RateAttachment WHERE TA_TS in
				(
					SELECT TS_PK FROM dbo.RateAttachmentSet
					WHERE TS_SU not in (SELECT SU_PK FROM dbo.StmMenuItem) AND TS_IsSystemDefined = 0
				)
				DELETE dbo.RateAttachmentSet WHERE TS_SU not in (SELECT SU_PK FROM dbo.StmMenuItem) AND TS_IsSystemDefined = 0";
			Db.Connection.ExecuteNonQuery(sqlText);
		}

		const string DataFileRelativePath = @"Documents\Documents.xml";
#if DEBUG
		public
#endif
 static readonly string[] DataFileTables = new string[]
		{
			StmMenuMenuPivotSchema.Constants.TableName,
			StmMenuTemplatePivotSchema.Constants.TableName,
			StmTemplateSchema.Constants.TableName,
			StmMenuItemSchema.Constants.TableName,
			StmMenuEDocsSchema.Constants.TableName,
			RefDocTypeSchema.Constants.TableName,
			StmMenuDocumentConfigSchema.Constants.TableName,
			StmMenuDocumentConfigItemSchema.Constants.TableName,
			RateAttachmentSetSchema.Constants.TableName,
		};

		void CheckForMissingParentsInPivots()
		{
			CheckForMissingParentsWithSql(
				"select SI_PK from dbo.StmMenuTemplatePivot where SI_SU not in (select SU_PK from dbo.StmMenuItem)",
				"StmMenuItem",
				"StmMenuTeplatePivot");

			CheckForMissingParentsWithSql(
				"select SI_PK from dbo.StmMenuTemplatePivot where SI_SO not in (select SO_PK from dbo.StmTemplate)",
				"StmTeplate",
				"StmMenuTeplatePivot");

			CheckForMissingParentsWithSql(
				"select SF_PK from dbo.StmMenuMenuPivot where SF_SU_Inward not in (select SU_PK from dbo.StmMenuItem) or SF_SU_Outward not in (select SU_PK from dbo.StmMenuItem)",
				"StmMenuItem",
				"StmMenuMenuPivot");
		}

		void CheckForMissingParentsWithSql(string sql, string parentName, string pivotName)
		{
			StringBuilder pks = new StringBuilder();

			using (IDataReader reader = Db.Connection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					if (pks.Length > 0)
					{
						pks.Append(", ");
					}
					pks.Append(reader[0].ToString());
				}
			}

			if (pks.Length > 0)
			{
				throw new ApplicationException(pks.Insert(0, string.Format("Following {0}(s) reference missing {1}(s): ", pivotName, parentName)).ToString());
			}
		}
	}
}
