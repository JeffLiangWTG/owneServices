using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	public class CusStatementHeaderCollection : ActiveBusinessObjectCollection<CusStatementHeader>
	{
		public CusStatementHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

