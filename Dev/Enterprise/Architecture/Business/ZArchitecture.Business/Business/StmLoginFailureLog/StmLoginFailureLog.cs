using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class StmLoginFailureLog : AutoStmLoginFailureLog
	{
		public StmLoginFailureLog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
