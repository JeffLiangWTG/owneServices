using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class DocStripsTestHelper : Assertion
	{
		public static void AssertMenuItemContainsRequiredDocStripsInAllContexts(string documentTitle, string menuItemName, string templateName, List<(string expectedSectionType, string expectedSectionName)> expectedStrips)
		{
			var stripsGroupedBycontext = RunScriptAndReturnDataGroupedByContext(documentTitle, menuItemName, templateName);

			foreach (var strip in stripsGroupedBycontext)
			{
				AssertContainsExactElementsInExactOrder($"DocStrips in {menuItemName} for {strip.Key} context", expectedStrips, strip);
			}
		}

		public static DataTable RunScript(string documentTitle, string menuItemName, string templateName)
		{
			string sql = $@"SELECT SU_BusinessContext, S4_SectionType, S4_SectionItemName FROM dbo.StmMenuDocumentConfigItem
	JOIN dbo.StmMenuDocumentConfig ON S4_S3 = S3_PK
	JOIN dbo.StmMenuTemplatePivot ON S3_SI = SI_PK
	JOIN dbo.StmMenuItem ON SI_SU = SU_PK
	JOIN dbo.StmTemplate ON SI_SO = SO_PK
	WHERE SI_DocumentTitle = '{documentTitle}' 
	AND SU_MenuName = '{menuItemName}'
	AND SO_Name = '{templateName}'
	ORDER BY SU_BusinessContext, S4_PrintOrder";

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		public static IEnumerable<IGrouping<string , (string SectionType, string SectionName)>> RunScriptAndReturnDataGroupedByContext(string documentTitle, string menuItemName, string templateName)
		{
			var dataTable = RunScript(documentTitle, menuItemName, templateName);
			var rowsGroupedBycontext = dataTable.AsEnumerable().GroupBy(x => x.Field<string>(0), x => (x.Field<string>(1), x.Field<string>(2)));

			return rowsGroupedBycontext;
		}
	}
}
