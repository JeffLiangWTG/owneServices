namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitConsignmentItemUcc6ValidationDecider : ICusExitConsignmentItemUcc6ValidationDecider
{
	public bool ValidateCCI_DiscrepancyStatusLookup => true;

	public bool ValidateCCI_UniqueConsignmentReferenceStatusLookup => true;
}
