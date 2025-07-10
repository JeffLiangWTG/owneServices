using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Diagnostics
{
	public sealed class DocumentCollection : NonPersistentBusinessObjectCollection<Document>
	{
		public DocumentCollection(string sectionName)
		{
			SectionName = sectionName;
		}

		readonly ZString SectionName;

		#region Implement

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public new void Load()
		{
			var resultTable = DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(SQL, SectionName));

			foreach (DataRow row in resultTable.Rows)
			{
				var document = new Document();

				document.Context = row[StmMenuItem.Schema.SU_BusinessContext].ToString();
				document.MenuItemName = row[StmMenuItem.Schema.SU_MenuName].ToString();
				document.DocumentTitle = row[StmMenuTemplatePivot.Schema.SI_DocumentTitle].ToString();
				document.PrintOrder = row[StmMenuDocumentConfigItem.Schema.S4_PrintOrder].ToString();
				document.SectionType = row[StmMenuDocumentConfigItem.Schema.S4_SectionType].ToString();
				document.FilterList = row[StmMenuDocumentConfigItem.Schema.S4_FilterList].ToString();
				document.IsSystemDefined = (bool)row[StmMenuDocumentConfig.Schema.S3_IsSystem];

				Add(document);
			}
		}

		const string SQL = @"
SELECT
StmMenuItem.SU_BusinessContext,
StmMenuItem.SU_MenuName,
StmMenuTemplatePivot.SI_DocumentTitle,
StmMenuDocumentConfigItem.S4_PrintOrder,
StmMenuDocumentConfigItem.S4_SectionType,
StmMenuDocumentConfigItem.S4_FilterList,
StmMenuDocumentConfig.S3_IsSystem
FROM
dbo.StmMenuDocumentConfig
INNER JOIN dbo.StmMenuDocumentConfigItem ON StmMenuDocumentConfig.S3_PK = StmMenuDocumentConfigItem.S4_S3
INNER JOIN dbo.StmMenuTemplatePivot ON StmMenuDocumentConfig.S3_SI = StmMenuTemplatePivot.SI_PK
INNER JOIN dbo.StmMenuItem ON StmMenuTemplatePivot.SI_SU = StmMenuItem.SU_PK
WHERE StmMenuDocumentConfigItem.S4_SectionItemName = '{0}'
ORDER BY StmMenuDocumentConfigItem.S4_SectionItemName, StmMenuItem.SU_BusinessContext, StmMenuItem.SU_MenuName";
	}
}
