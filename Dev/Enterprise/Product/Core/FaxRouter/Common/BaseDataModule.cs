using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.FaxRouter
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClassesAnalyzer", Justification = "This assembly has no need to change SqlConnection")]
	public class BaseDataModule : Constants
	{
		public static SqlConnection GetMailDBConnection()
		{
			SqlConnection c = new SqlConnection(MAILDB_CONNECTION);
			c.Open();
			return c;
		}

		public static SqlConnection GetEDIFaxDBConnection()
		{
			SqlConnection c = new SqlConnection(EDIFAXDB_CONNECTION);
			c.Open();
			return c;
		}

		public static DateTime GetCurrentDateTime()
		{
			DateTime result;
			using (SqlConnection conn = GetMailDBConnection())
			{
				SqlCommand sqlCmd = new SqlCommand(@"SELECT GetDate()", conn);
				result = (DateTime)sqlCmd.ExecuteScalar();
				conn.Close();
			}
			return result;
		}
	}
}
