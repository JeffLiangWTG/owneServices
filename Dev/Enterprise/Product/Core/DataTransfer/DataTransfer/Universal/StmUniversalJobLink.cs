using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.Business
{
	public class StmUniversalJobLink : AutoStmUniversalJobLink, IStmUniversalJobLink
	{
		public StmUniversalJobLink(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
