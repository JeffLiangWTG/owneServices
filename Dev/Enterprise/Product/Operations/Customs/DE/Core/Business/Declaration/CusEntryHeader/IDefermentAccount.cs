namespace Enterprise.Customs.DE.Business.Declaration
{
	public interface IDefermentAccount
	{
		string AccountHolder { get; }
		string AccountNumber { get; }
		string AccountPrefix { get; }
		string Applicant { get; }
		string ApplicationType { get; }
		string AuthorisationNumber { get; }
		string Type { get; }
	}
}
