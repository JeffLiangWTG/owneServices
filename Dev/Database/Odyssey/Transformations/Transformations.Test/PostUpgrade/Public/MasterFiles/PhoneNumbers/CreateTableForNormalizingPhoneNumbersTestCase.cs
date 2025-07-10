using System.Globalization;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.MasterFiles.Testing
{
	public abstract class CreateTableForNormalizingPhoneNumbersTestCase : DataTransformationTestCase
	{
		#region Helpers

		protected CreateTableForNormalizingPhoneNumbers TransformationForTest
		{
			get { return (CreateTableForNormalizingPhoneNumbers)TransformationToTest; }
		}

		protected static string GetUnlocoCodeByCountryCode(string countryCode)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, "SELECT TOP(1) RL_Code FROM dbo.RefUNLOCO WHERE RL_RN_NKCountryCode='{0}'", countryCode);
			return (string)Db.Connection.ExecuteScalar(sql);
		}

		#endregion
	}
}
