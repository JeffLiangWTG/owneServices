using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	public abstract class CusStatementLine : BaseCusStatementLine
	{
		public CusStatementLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ThreadSafe]
		public static new readonly CusStatementLineTypeDecider TypeDecider = new CusStatementLineTypeDecider();
	}
}
