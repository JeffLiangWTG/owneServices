namespace Enterprise.Client.EDI.IssueManager.Business
{
	public interface IIssueAssignmentCalculator
	{
		IssueAssignment GetAssignment(EdiHelpErrorLog log, DataFormatter formatter);
	}
}
