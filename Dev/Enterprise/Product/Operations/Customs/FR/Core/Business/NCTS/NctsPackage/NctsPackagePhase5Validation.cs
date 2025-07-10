using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.NCTS;

public class NctsPackagePhase5Validation(NctsPackage parent) : EU.NCTS.Business.NctsPackagePhase5Validation(parent)
{
	public new NctsPackage Parent => (NctsPackage)base.Parent;

	protected override void CheckRuleC0670()
	{
		var rowMessageError = ValidationRuleConfiguration.Messages.C0670Message;

		var package = Parent;
		package.RemoveRowMessageError(rowMessageError);
		if (package.ValidationDecider is INctsPackagePhase5ValidationDecider { IsRuleC0670Active: true }
		&& !package.IsPackDifference
		&& package.HasContainer
		&& !package.HasSelectedContainer)
		{
			package.AddRowMessageError(rowMessageError);
		}
	}
}
