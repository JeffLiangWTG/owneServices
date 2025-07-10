using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WhsPopulateComponentOrderStrategy : WhsPopulatePickableDocketStrategy
	{
		public WhsPopulateComponentOrderStrategy(BusinessObject docket)
			: base(docket)
		{
		}
	}
}
