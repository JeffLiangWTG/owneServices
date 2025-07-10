namespace Enterprise.Customs.EU.ExitControl.Business;

public interface ICusExitConsignmentItemValidationDecider
{
}

public interface ICusExitConsignmentItemUcc6ValidationDecider : ICusExitConsignmentItemValidationDecider
{
	bool ValidateCCI_DiscrepancyStatusLookup { get; }
	bool ValidateCCI_UniqueConsignmentReferenceStatusLookup { get; }
}
