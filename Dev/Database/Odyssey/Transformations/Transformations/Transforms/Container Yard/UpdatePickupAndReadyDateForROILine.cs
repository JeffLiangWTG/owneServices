using System;
using System.Globalization;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	class UpdatePickupAndReadyDateForROILine : DataTransformation
	{
		public override string UserDescription => "Update Pick up and Ready dates for Release Order Line";

		protected override void OfflinePreUpgradeTransform()
		{
			var defaultDateTime = $"'{FallbackEditDateTime}'";
			if (DbObjectCreator.TableExists(Db.Connection, CYDReleaseAdviceLineSchema.Constants.TableName)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDReleaseAdviceLineSchema.Constants.TableName, CYDReleaseAdviceLineSchema.Constants.YEL_PickupDate, "DATE", defaultDateTime)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDReleaseAdviceLineSchema.Constants.TableName, CYDReleaseAdviceLineSchema.Constants.YEL_ReadyDate, "DATE", defaultDateTime))
			{
				var sql = @"
DECLARE @CurrentUtcDate DATETIME;
SET @CurrentUtcDate = GetUtcDate();

UPDATE dbo.CYDReleaseAdviceLine
SET
    YEL_PickupDate = COALESCE(YRE_ToDate, @CurrentUtcDate),
    YEL_ReadyDate = COALESCE(YRE_ToDate, @CurrentUtcDate),
    YEL_SystemLastEditTimeUtc = @CurrentUtcDate,
    YEL_SystemLastEditUser = '~BP'
FROM
    dbo.CYDReleaseAdvice
WHERE
    YRE_PK = YEL_YRE_ReleaseAdvice";

				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		internal string FallbackEditDateTime { get; set; } = GetDateTimeInSqlFormat(DateTime.Now);

		static string GetDateTimeInSqlFormat(DateTime dateTime)
		{
			return dateTime.ToString("F", sqlServerDateTimeFormatInfo);
		}

		static readonly IFormatProvider sqlServerDateTimeFormatInfo = new DateTimeFormatInfo
		{
			FullDateTimePattern = "yyyy-MM-dd HH:mm:ss.fff",
			TimeSeparator = ":",
			DateSeparator = "-"
		};
	}
}
