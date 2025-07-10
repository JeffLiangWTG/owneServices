using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlSetCollection : ActiveBusinessObjectCollection<FeatureControlSet>
	{
		public FeatureControlSetCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
