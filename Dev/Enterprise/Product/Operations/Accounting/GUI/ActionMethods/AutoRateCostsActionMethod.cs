using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public sealed class AutoRateCostsActionMethod : OperationalActionMethod
	{
		public AutoRateCostsActionMethod(JobInvoicingSecurityHelper securityHelper)
			: base(new ZGuid("9A5E3907-3F2C-4C94-A037-25207E33A835"))
		{
			this.securityHelper = securityHelper;
		}

		public override string Name => Res.GetString("9A5E3907-3F2C-4C94-A037-25207E33A835", "Autorate Costs");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public override string Description => Res.GetString(
			"AutoRatingActionMethod|Description",
			"Auto rates all targets on which the operational action is run. " +
			"If auto-rating will be attempted on all targets, but if any fail " +
			"then the entire action will be aborted so that it can be combined " +
			"with document delivery and/or other defined processes.");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new AutoRatingActionMethodApplicator(factory, true, false);
		}

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints() => new SecurityCheckpoint[] { this.securityHelper.GetInvSecurity(SecurityCore.AutoRateCost) };

		readonly JobInvoicingSecurityHelper securityHelper;
	}
}
