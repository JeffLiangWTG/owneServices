using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ICYDUniversalTestData
	{
		public void SetupDataForPickup(string acceptanceNumber, string releaseNumber, ZDate fromDate, ZDate toDate, string containerTypeCode, string[] containerNumbers, int orgId = 1);
	}
}
