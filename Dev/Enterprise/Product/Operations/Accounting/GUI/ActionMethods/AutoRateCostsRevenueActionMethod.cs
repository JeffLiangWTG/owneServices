using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public sealed class AutoRateCostsRevenueActionMethod : OperationalActionMethod
	{
		public AutoRateCostsRevenueActionMethod(JobInvoicingSecurityHelper securityHelper)
			: base(new ZGuid("84364BDC-FB9A-41d2-851A-E7B32CCF7339"))
		{
			this.securityHelper = securityHelper;
		}

		public override string Name => Res.GetString("84364BDC-FB9A-41d2-851A-E7B32CCF7339", "Autorate Costs and Revenue");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public override string Description => Res.GetString(
			"AutoRatingActionMethod|Description",
			"Auto rates all targets on which the operational action is run. " +
			"If auto-rating will be attempted on all targets, but if any fail " +
			"then the entire action will be aborted so that it can be combined " +
			"with document delivery and/or other defined processes.");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new AutoRatingActionMethodApplicator(factory);
		}

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints()
		{
			return new SecurityCheckpoint[]
			{
				this.securityHelper.GetInvSecurity(SecurityCore.AutoRateCost),
				this.securityHelper.GetInvSecurity(SecurityCore.AutoRateRevenue),
			};
		}

		readonly JobInvoicingSecurityHelper securityHelper;
	}
}
