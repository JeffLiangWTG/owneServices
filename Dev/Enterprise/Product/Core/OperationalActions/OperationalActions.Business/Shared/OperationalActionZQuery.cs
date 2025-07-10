using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionZQuery : ZQuery
	{
		public OperationalActionZQuery()
		{
			AddToFilter(StmMenuItemSchema.SU_MenuType, Constants.StmMenuItemTypes.OperationalActions);
		}

		public OperationalActionZQuery(ZString businessContext)
			: this()
		{
			AddToFilter(StmMenuItemSchema.SU_BusinessContext, businessContext);
		}

		public OperationalActionZQuery(ZString businessContext, ZString staffCode)
			: this(businessContext)
		{
			ZQuery staffCodeFilter = new ZQuery(StmMenuItemSchema.SU_GS_NKStaffCode, "");
			staffCodeFilter.AddToFilter(JoinCondition.Or, StmMenuItemSchema.SU_GS_NKStaffCode, staffCode);
			AddToFilter(staffCodeFilter, JoinCondition.And);
		}

		public OperationalActionZQuery(BusinessContext businessContext)
			: this(OperationalAction.GetFullBusinessContext(businessContext))
		{
		}

		public OperationalActionZQuery(BusinessContext businessContext, ZString staffCode)
			: this(OperationalAction.GetFullBusinessContext(businessContext), staffCode)
		{
		}
	}
}
