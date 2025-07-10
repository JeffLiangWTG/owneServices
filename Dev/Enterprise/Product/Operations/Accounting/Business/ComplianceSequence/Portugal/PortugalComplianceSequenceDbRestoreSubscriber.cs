using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ComplianceSequence
{
	public class PortugalComplianceSequenceDbRestoreSubscriber : IDbRestoreSubscriber
	{
		public string ReadableName => (NoResString)"Disable Compliance Sequence books for Portugal Companies";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public string Run(DbConnection connection)
		{
			var result = string.Empty;
			try
			{
				using (var command = connection.Command(DisableComplianceSequenceAfterDbRestoreSQL))
				{
					command.ExecuteNonQuery();
				}
			}
			catch (System.Data.Common.DbException ex)
			{
				var type = new DbErrorMatch(ex).ExceptionType;
				if (type == DbErrorType.InvalidColumnName || type == DbErrorType.InvalidObjectName)
				{
					result = ex.Message;
				}
				else
				{
					throw;
				}
			}
			return result;
		}

		#region Properties

		const string SupportedCountry = CountryCodes.Portugal;

		const string RegistryName = "LastUTCDateToDisableComplianceBookAfterDbRestored";

		string DisableComplianceSequenceAfterDbRestoreSQL => $@"
DECLARE @UtcDateTime SMALLDATETIME = GetUtcDate()

UPDATE
  dbo.AccComplianceSequence 
SET
  XD_IsActive = 0, 
  XD_PermanentDisableTimeUtc = @UtcDateTime,
  XD_SystemLastEditTimeUtc = GETUTCDATE(),
  XD_SystemLastEditUser = '~BP'
FROM
  dbo.AccComplianceSequence 
  JOIN dbo.GlbCompany ON GC_PK = XD_GC_Company 
WHERE
  GC_RN_NKCountryCode = '{SupportedCountry}' 
  AND XD_PermanentDisableTimeUtc IS NULL


IF @@ROWCOUNT > 0
BEGIN

	IF EXISTS (SELECT 1 FROM dbo.StmData WHERE SD_Name = '{RegistryName}')
		UPDATE
			dbo.StmData
		SET
			SD_BinaryValue = CAST(CONVERT(NVARCHAR(MAX), @UtcDateTime, 121) as varbinary(max)),
			SD_SystemLastEditUser = '{User.ServiceUserCode}',
			SD_SystemLastEditTimeUtc = @UtcDateTime
		WHERE
			SD_Name = '{RegistryName}'
	ELSE
		INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Type, SD_IsLogged, SD_BinaryValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser)
		VALUES (NEWID(), '{RegistryName}', 'DT', 1, CAST(CONVERT(NVARCHAR(MAX), @UtcDateTime, 121) as varbinary(max)), @UtcDateTime, '{User.ServiceUserCode}', @UtcDateTime, '{User.ServiceUserCode}')
END
";

		#endregion
	}
}
