namespace Enterprise.Customs.EU.ExitControl.Business;

public interface ICusExitConsignmentPackageValidationDecider
{
}

public interface ICusExitConsignmentPackageUcc6ValidationDecider : ICusExitConsignmentPackageValidationDecider
{
	bool ValidateCXP_MarksAndNumbersStatusLookup { get; }
}
