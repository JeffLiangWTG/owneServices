using CargoWise.Data;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Telematics.Tca
{
	static class TelematicsRimRegistrationNumberStrategy
	{
		public static string GetRegistrationNumber(IDbConnected dbConnection)
		{
			return Env.NumberFountains.TelematicsRimRegistrationNumber.GetNextFormatted(dbConnection);
		}
	}
}
