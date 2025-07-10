using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRHeaderCollection : BusinessObjectCollection<JPAFRHeader>
	{
		public JPAFRHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
