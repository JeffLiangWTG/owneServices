namespace CargoWise.Integration
{
	public interface IActiveUserQuery
	{
		string[] GetActiveUsers(bool includeCurrentUser);
	}
}