using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.DocumentEngine.ReportTableProviders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This is used by reflection in TableProviderFactory.cs.")]
	public class DocStripSectionNamesTableProvider : TableProvider
	{
		readonly BusinessObjectFactory factory = new BusinessObjectFactory();

		public override DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool needsToAddWhereClause)
		{
			return GetDataTable(tableName, dataSourceString, report, needsToAddWhereClause, -1);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
		public override DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool needsToAddWhereClause, int maximumNumberOfRows)
		{
			var table = new DataTable("DocStripSection");
			foreach (string column in ColumnNames)
			{
				table.Columns.Add(column);
			}

			PopulateTable(table, maximumNumberOfRows);

			return table;
		}

		public override bool HandlesSortInternally
		{
			get { return false; }
		}

		void PopulateTable(DataTable table, int maximumNumberOfRows)
		{
			var docStripSections = TemplateSectionCollection.CacheManager.Get(factory).GetConfigurableOnlyCollection();
			int maxIndex = maximumNumberOfRows == -1 ? docStripSections.Count : maximumNumberOfRows;
			for (int i = 0; i < maxIndex; i++)
			{
				TemplateSection section = docStripSections[i];

				DataRow row = table.NewRow();
				row[0] = section.TemplateName;
				row[1] = section.TypeCode;
				row[2] = section.Category;
				row[3] = section.SectionName;
				row[4] = section.FullLabel;

				table.Rows.Add(row);
			}
		}

		string[] ColumnNames
		{
			get
			{
				return new string[] { "TemplateName", "SectionType", "SectionCategory", "SectionName", "FullLabel" };
			}
		}
	}
}
