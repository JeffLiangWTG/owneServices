namespace Enterprise.Customs.EU.ExitControl.Business;

public interface ICusExitContainerValidationDecider
{
}

public interface ICusExitContainerUcc6ValidationDecider : ICusExitContainerValidationDecider
{
	bool ValidateCXN_StatusLookups { get; }
}
