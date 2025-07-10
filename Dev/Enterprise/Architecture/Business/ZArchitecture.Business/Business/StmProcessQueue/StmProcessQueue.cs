using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class StmProcessQueue : AutoStmProcessQueue
	{
		public StmProcessQueue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
