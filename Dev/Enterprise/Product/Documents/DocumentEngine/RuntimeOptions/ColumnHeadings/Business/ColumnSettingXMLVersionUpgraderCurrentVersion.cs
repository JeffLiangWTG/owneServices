using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ExcelTemplates;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ColumnSettingXMLVersionUpgraderCurrentVersion : ColumnSettingXMLVersionUpgraderGeneric<ColumnHeadingCollection, ReportColumnSettings, ColumnSettingXMLVersionUpgraderCurrentVersionParameters>
	{
		public ColumnSettingXMLVersionUpgraderCurrentVersion(ColumnSettingXMLVersionUpgraderCurrentVersionParameters v1Params)
			: base(v1Params)
		{
			this.ReportID = ((IColumnSettingXMLVersionUpgraderV1Parameters)v1Params).ReportID;
		}
		readonly ZGuid ReportID;

		protected override ColumnSettingXMLVersionUpgrader GetPreviousVersion()
		{
			return null;
		}

		protected override ReportColumnSettings GetNewClass()
		{
			return new ReportColumnSettings();
		}

		protected override ReportColumnSettings MapClasses(ColumnHeadingCollection oldClass, ReportColumnSettings newClass)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			StmMenuTemplatePivotBase pivot = factory.LoadTop1<StmMenuTemplatePivotBase>(new ZQuery(ZArchitecture.Schema.StmMenuTemplatePivotSchema.SI_SU, ReportID)) ?? throw new ColumnSettingXMLInvalidException();

			StmTemplateBase stmTemplate = factory.LoadTop1<StmTemplateBase>(new ZQuery(ZArchitecture.Schema.StmTemplateSchema.PK, pivot.SI_SO));
			ExcelTemplate excelTemplate = new ExcelTemplateReadFromStmTemplateTable(stmTemplate);
			using (Report report = new Report(new DocumentPack(), excelTemplate))
			{
				newClass.Worksheets.AddNew(report.FirstRenderableSheetName);
				foreach (ColumnHeading heading in oldClass)
				{
					Enterprise.DocumentEngine.RuntimeOptions.ColumnHeading newHeading = new Enterprise.DocumentEngine.RuntimeOptions.ColumnHeading(heading.DisplayLabel, "", heading.HeadingText, heading.OriginalColumnNumber, heading.CurrentPosition, heading.WidthInPixels, heading.Hidden);
					newClass.Worksheets[report.FirstRenderableSheetName].ColumnHeadings.Add(newHeading);
				}
				newClass.Version = 1;
			}
			return newClass;
		}
	}
}
