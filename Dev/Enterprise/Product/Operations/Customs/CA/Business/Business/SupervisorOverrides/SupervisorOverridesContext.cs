namespace Enterprise.Customs.CA.Business;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
public class SupervisorOverridesContext : Customs.Business.SupervisorOverridesContext
{
	public const string ManualCancelRelease = "ManualCancelRelease";
	public const string ForceSendB3CADMessage = "Force Send CAD Message";
}
