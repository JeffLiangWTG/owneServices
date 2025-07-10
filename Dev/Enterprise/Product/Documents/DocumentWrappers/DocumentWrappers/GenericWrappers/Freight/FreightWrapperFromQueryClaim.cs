using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromQueryClaim : FreightWrapper
	{
		public FreightWrapperFromQueryClaim(AccQueryClaim queryClaim, BusinessObjectFactory factory)
			: base(queryClaim, factory)
		{
			this.queryClaim = queryClaim;
		}
		readonly AccQueryClaim queryClaim;

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return queryClaim.PK;
		}

		protected override QueryClaimWrapper GetQueryClaim()
		{
			return new QueryClaimWrapper(queryClaim, Factory);
		}

		protected override ZString GetJobNumber()
		{
			return queryClaim.AY_QueryClaimReference;
		}
	}
}
