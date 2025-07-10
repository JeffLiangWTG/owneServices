using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlHeaderCollection : BusinessObjectCollection<FeatureControlHeader>
	{
		public FeatureControlHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
