using CargoWise.Types;
using Enterprise.ExcelTemplates.Integration;

namespace Enterprise.ExcelTemplates
{
	public class ExcelTemplateReadFromStmTemplateTable : ExcelTemplateReadFromByteArray
	{
		public ExcelTemplateReadFromStmTemplateTable(IStmTemplate template)
			: base(template.SO_Name, template.SO_ExcelTemplatePath, template.SO_Template)
		{
			this.template = template;
		}

		readonly IStmTemplate template;

		public ZBool IsSystemDefined => template?.SO_IsSystemDefined ?? false;
	}
}
