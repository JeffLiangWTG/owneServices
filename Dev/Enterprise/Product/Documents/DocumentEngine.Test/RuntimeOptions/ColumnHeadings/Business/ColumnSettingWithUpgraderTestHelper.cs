using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class ColumnSettingWithUpgraderTestHelper
	{
		public ZGuid GetReportIDThruFullTemplatePivotMenuBuild(string testTemplateLocation)
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(testTemplateLocation, TestFilesSubFolder.ReportTestFiles);
			var stmMenuItems = new StmMenuItemBaseCollection(Factory);
			var stmTemplate = stmMenuItems.LoadNewTemplateFromFile(excelTemplate.FullTemplateSourceLocation);
			var stmMenu = stmMenuItems.AddNew();
			var reportID = stmMenu.PK;
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = stmTemplate.PK;
			pivot.SI_SU = stmMenu.PK;
			Factory.Save();
			return reportID;
		}
		readonly BusinessObjectFactory Factory = new BusinessObjectFactory();
	}
}
