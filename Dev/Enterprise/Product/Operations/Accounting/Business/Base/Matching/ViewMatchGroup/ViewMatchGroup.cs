using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class ViewMatchGroup : AutoViewMatchGroup
	{
		public ViewMatchGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ViewMatchGroupFetchStrategy(this);
		}

		// since this BizO is based on a DBview
		public override bool IsSavedByFactory
		{
			get { return false; }
		}
	}
}
