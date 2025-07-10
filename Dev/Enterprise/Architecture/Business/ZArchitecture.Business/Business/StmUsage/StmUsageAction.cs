using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public sealed class StmUsageAction : AutoStmUsageAction
	{
		public StmUsageAction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
