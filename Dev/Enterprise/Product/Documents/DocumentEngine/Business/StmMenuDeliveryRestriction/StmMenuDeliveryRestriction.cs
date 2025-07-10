using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuDeliveryRestriction : AutoStmMenuDeliveryRestriction
	{
		public StmMenuDeliveryRestriction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
