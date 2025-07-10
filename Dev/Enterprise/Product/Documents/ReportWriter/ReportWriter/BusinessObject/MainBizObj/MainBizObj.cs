using System.IO;
using CargoWise.EntityFramework;
using Enterprise.ExcelTemplates;

namespace Enterprise.ReportWriter
{
	public class MainBizObj : AutoMainBizObj
	{
		public MainBizObj(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static MainBizObj New(ExcelTemplate template)
		{
			var filename = Path.GetFileName(Path.Combine(template.TemplateSourceLocation, template.TemplateName));
			return new MainBizObj(new BusinessObjectFactory())
			{
				Filename = filename
			};
		}

		public ReportBizObjCollection ReportBizObjs
		{
			get
			{
				if (reportBizObjs == null)
				{
					reportBizObjs = new ReportBizObjCollection(this);
					RegisterEditableChildObject(reportBizObjs);
				}
				return reportBizObjs;
			}
		}
		ReportBizObjCollection reportBizObjs;
	}
}
