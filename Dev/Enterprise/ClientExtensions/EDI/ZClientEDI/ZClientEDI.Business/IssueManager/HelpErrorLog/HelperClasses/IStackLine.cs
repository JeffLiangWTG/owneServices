namespace Enterprise.Client.EDI.IssueManager.Business
{
	public interface IStackLine
	{
		string Assembly { get; set; }
		string Type { get; set; }
		string Method { get; set; }
		string FullStackLine { get; }
	}
}
