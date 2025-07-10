using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class ActiveCusStatementLineCollection : ActiveBusinessObjectCollection<CusStatementLine>
	{
		public ActiveCusStatementLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ActiveCusStatementLineCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
