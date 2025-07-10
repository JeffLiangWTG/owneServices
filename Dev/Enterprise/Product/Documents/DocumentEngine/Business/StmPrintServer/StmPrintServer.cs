using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class StmPrintServer : AutoStmPrintServer
	{
		public StmPrintServer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
