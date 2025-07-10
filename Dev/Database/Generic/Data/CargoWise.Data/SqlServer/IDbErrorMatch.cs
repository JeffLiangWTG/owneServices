namespace CargoWise.Data
{
	public interface IDbErrorMatch
	{
		bool IsInfrastructureDbError { get; }
		DbErrorType ExceptionType { get; }
		string GetUserFriendlyMessage(DbConnection connection);
		string GetIndexNameIfUniqueIndexViolation();
	}
}