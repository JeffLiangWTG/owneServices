using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSDISQueryMessageCollection : ActiveBusinessObjectCollection<CDSDISQueryMessage>
	{
		public CDSDISQueryMessageCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
