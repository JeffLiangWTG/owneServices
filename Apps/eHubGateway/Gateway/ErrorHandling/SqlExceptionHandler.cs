using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;

namespace CargoWise.eHub.Gateway
{
	public class SqlExceptionHandler
	{
		static internal void ThrowSystemUnderMaintananceExceptionIfApplicable(SqlException sqlException)
		{
			if (!IsApplicationException(sqlException.Number))
			{
				throw new SystemUnderMaintananceException(sqlException);
			}
		}

		static internal void ThrowFaultExceptionWithSystemUnderMaintananceExceptionMessageIfApplicable(SqlException sqlException)
		{
			if (!IsApplicationException(sqlException.Number))
			{
				throw new FaultException(new SystemUnderMaintananceException(sqlException).Message + sqlException.Message);
			}
		}

		public static int[] sqlApplicationErrorCode = new int[] { 2601, 2627, 8152, 50000 };

		public static bool IsApplicationException(int sqlErrorCode)
		{
			return sqlApplicationErrorCode.Contains(sqlErrorCode);
		}
	}
}
