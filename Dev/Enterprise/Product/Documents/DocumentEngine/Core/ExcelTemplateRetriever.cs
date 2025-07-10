using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine
{
	public static class ExcelTemplateRetriever
	{
		public static ExcelTemplateReadFromStmTemplateTable GetTemplate(string templateName, ZString dataContext, BusinessObjectFactory factory)
		{
			ExcelTemplateReadFromStmTemplateTable template1 = null;
			if (factory == null)
			{
				factory = new BusinessObjectFactory();
			}

			ZQuery filter = new ZQuery();
			filter.AddToFilter(StmTemplateSchema.SO_Name, templateName);
			filter.AddToFilter(StmTemplateSchema.SO_DataContext, dataContext);

			StmTemplate[] result = (StmTemplate[])factory.Load(typeof(StmTemplate), filter);
			if (result != null && result.Length > 0)
			{
				StmTemplate template = result[0];
				template1 = new ExcelTemplateReadFromStmTemplateTable(template);
			}
			return template1;
		}

		public static ExcelTemplateReadFromStmTemplateTable GetTemplate(string templateName, Core.Constants.DataContext dataContext, BusinessObjectFactory factory)
		{
			return GetTemplate(templateName, dataContext.ToString(), factory);
		}
	}
}
