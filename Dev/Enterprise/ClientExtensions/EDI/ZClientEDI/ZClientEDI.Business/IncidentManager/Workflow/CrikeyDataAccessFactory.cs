using CargoWise.Data;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public static class CrikeyDataAccessFactory
	{
		public static ICrikeyDataAccess GetInstance(DbConnection connection) => new CrikeyDataAccess(connection);
	}
}
