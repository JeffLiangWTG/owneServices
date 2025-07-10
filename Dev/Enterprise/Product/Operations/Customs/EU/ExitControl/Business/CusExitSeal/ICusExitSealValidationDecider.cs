namespace Enterprise.Customs.EU.ExitControl.Business;

public interface ICusExitSealValidationDecider
{
}

public interface ICusExitSealUcc6ValidationDecider : ICusExitSealValidationDecider
{
	bool ValidateBK_UnloadingStateLookups { get; }
}
