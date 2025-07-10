using System;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class EuLandedCostingHelper : LandedCostingHelper
	{
		protected override DutyTaxEntryFee GetTotalDutyTaxEntryFeeItemsCore(BaseJobDeclaration declaration)
		{
			EnsureIsOverriddenIfActualLandedCostingUsed(declaration);
			return new DutyTaxEntryFee();
		}

		protected override DutyTaxEntryFee GetLineDutyTaxEntryFeeItemsCore(BaseJobComInvoiceLine invoiceLine)
		{
			EnsureIsOverriddenIfActualLandedCostingUsed(invoiceLine.Declaration);
			return new DutyTaxEntryFee();
		}

		void EnsureIsOverriddenIfActualLandedCostingUsed(ILandedCostHeader lcHeader)
		{
			if (lcHeader != null && lcHeader.LandedCostType == LandedCostType.Actual)
			{
				throw new NotImplementedException("This should be overridden by the country-specific context-provided xxLandedCostingHelper");
			}
		}
	}
}
