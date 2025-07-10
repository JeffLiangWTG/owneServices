using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ServiceManager.Business
{
	[CodeAlive("To be used in an upcoming project.")]
	public class CategorizedApplicationLogger : AutoCategorizedApplicationLogger
	{
		public CategorizedApplicationLogger(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString CTL_Category { get => base.CTL_Category; set => base.CTL_Category = value; }

		public override ZString CTL_Name { get => base.CTL_Name; set => base.CTL_Name = value; }

		public override ZString CTL_Product { get => base.CTL_Product; set => base.CTL_Product = value; }
	}
}
