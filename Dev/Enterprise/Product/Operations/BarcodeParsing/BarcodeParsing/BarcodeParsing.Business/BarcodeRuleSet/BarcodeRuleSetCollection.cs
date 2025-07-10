using CargoWise.EntityFramework;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeRuleSetCollection : ActiveBusinessObjectCollection<BarcodeRuleSet>
	{
		public BarcodeRuleSetCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
