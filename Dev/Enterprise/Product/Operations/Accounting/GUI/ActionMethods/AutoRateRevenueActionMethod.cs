using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public sealed class AutoRateRevenueActionMethod : OperationalActionMethod
	{
		public AutoRateRevenueActionMethod(JobInvoicingSecurityHelper securityHelper)
			: base(new ZGuid("F691931F-1505-4EB0-A51F-B443B1DA01D8"))
		{
			this.securityHelper = securityHelper;
		}

		public override string Name => Res.GetString("F691931F-1505-4EB0-A51F-B443B1DA01D8", "Autorate Revenue");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public override string Description => Res.GetString(
			"AutoRatingActionMethod|Description",
			"Auto rates all targets on which the operational action is run. " +
			"If auto-rating will be attempted on all targets, but if any fail " +
			"then the entire action will be aborted so that it can be combined " +
			"with document delivery and/or other defined processes.");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new AutoRatingActionMethodApplicator(factory, false);
		}

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints() => new SecurityCheckpoint[] { this.securityHelper.GetInvSecurity(SecurityCore.AutoRateRevenue) };

		readonly JobInvoicingSecurityHelper securityHelper;
	}
}
