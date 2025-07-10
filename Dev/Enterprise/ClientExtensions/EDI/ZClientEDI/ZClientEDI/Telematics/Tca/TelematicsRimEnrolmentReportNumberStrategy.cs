using CargoWise.Data;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Telematics.Tca
{
	static class TelematicsRimEnrolmentReportNumberStrategy
	{
		public static string GetReportNumber(IDbConnected dbConnection)
		{
			return Env.NumberFountains.TelematicsRimEnrolmentReportNumber.GetNextFormatted(dbConnection);
		}
	}
}
