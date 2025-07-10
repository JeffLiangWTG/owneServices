using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class VersionHistory
	{
		public VersionHistory()
		{
			SurchargePercentDefault = EDIDataRegistry.Instance.VersionSurchargePercent.Value;
			SurchargeAdditionalPercentDefault = EDIDataRegistry.Instance.VersionSurchargeAdditionalPercent.Value;
			ReleaseHistory = LoadReleaseHistory();
		}

		public decimal GetVersionSurchargePercent(LicenceDatabase db, VersionSurchargeLicenceSetting surchargeSetting, DateTime monthAsDate)
						=> GetVersionSurcharge(db, surchargeSetting, monthAsDate)?.SurchargePercent ?? 0;

		public VersionSurcharge GetVersionSurcharge(LicenceDatabase db, VersionSurchargeLicenceSetting surchargeSetting, DateTime monthAsDate)
		{
			var result = 0m;
			var surchargePercent = Math.Max(0, surchargeSetting?.LS9_Percent ?? SurchargePercentDefault);
			var surchargeAdditionalPercent = Math.Max(0, surchargeSetting?.AdditionalPercent ?? SurchargeAdditionalPercentDefault);
			var lastDayOfBillingPeriod = monthAsDate.AddMonths(1).AddMinutes(-1).Date;

			if (db == null || (surchargePercent == 0 && surchargeAdditionalPercent == 0))
			{
				return null;
			}

			// Use the 2nd day of the next month to allow for delays in information reaching us and imprecision in time zone of the dates.
			var dbVersion = GetDatabaseVersionForDate(db, monthAsDate.AddDays(1).AddMonths(1));
			if (dbVersion == null)
			{
				return null;
			}

			var version = GetVersionForDate(db, monthAsDate.AddMonths(1).AddMinutes(-1));
			if (version == null)
			{
				return null;
			}

			var versionGap = dbVersion.VersionIndex - version.VersionIndex;
			switch (dbVersion.ReleaseRing)
			{
				case ReleaseRings.Codes.GP1:
				case ReleaseRings.Codes.GP2:
					{
						if (versionGap == 2 && version.Date.AddDays(30).Date <= lastDayOfBillingPeriod)
						{
							result = surchargePercent;
						}
						else if (versionGap > 2)
						{
							result = surchargePercent + (versionGap - 2) * surchargeAdditionalPercent;
						}
						break;
					}
				case ReleaseRings.Codes.STD:
					{
						if (versionGap >= 3)
						{
							result = surchargePercent + (versionGap - 3) * surchargeAdditionalPercent;
						}
						break;
					}
				case ReleaseRings.Codes.DPR:
					{
						if (versionGap >= 4)
						{
							result = surchargePercent + (versionGap - 4) * surchargeAdditionalPercent;
						}
						break;
					}
			}

			return result > 0 ? new VersionSurcharge(result, dbVersion.Version) : null;
		}

		VersionInfo GetDatabaseVersionForDate(LicenceDatabase db, DateTime date)
		{
			var currentVersionDate = db.LD_CurrentVersionFirstReportUtc;
			if (!currentVersionDate.IsEmpty && currentVersionDate <= date)
			{
				var current = db.CurrentVersion;
				if (current != null)
				{
					return CreateVersionInfoFromVersionNumber(current.HL_Product, db.LD_ReleaseRing, current.VersionNumber);
				}
			}

			var logQuery = new ZQuery(StmALogSchema.SL_Parent, db.PK);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecordCode);
			logQuery.AddToFilter(StmALogSchema.SL_EventTime, SQLComparisonOperator.LessThanOrEqualTo, date);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, LogReferenceKey);
			logQuery.OrderBy = StmALogSchema.Constants.SL_EventTime + OrderByClause.Descending;
			var log = db.Factory.LoadTop1<StmALog>(logQuery);
			if (log == null)
			{
				return null;
			}

			var versionNumber = ParseVersionFromLogReference(log.SL_Reference);
			if (versionNumber.Major != 0)
			{
				var releaseProduct = GetReleaseProduct(db);
				if (releaseProduct == ProductTypes.Codes.CargoWiseNext && versionNumber < new VersionNumber("24.11.1.0"))  // the db was a CW1 product
				{
					/*
						the final CW1, 24.10.30.x
						the first CWN, 24.11.1.0
					*/
					return null;
				}
				else if (releaseProduct == ProductTypes.Codes.CargoWise && versionNumber < new VersionNumber("25.4.7.0"))  // the db was a CW1/CWN product
				{
					/*
						the final CW1, 24.10.30.x
						the final CWN, 24.4.6.x
						the first CGW, 25.4.7.0
					*/
					return null;
				}
				else
				{
					return CreateVersionInfoFromVersionNumber(releaseProduct, db.LD_ReleaseRing, versionNumber);
				}
			}
			else
			{
				return null;
			}
		}

		const string LogReferenceKey = " Version: ";

		VersionNumber ParseVersionFromLogReference(string reference)
		{
			var keyIndex = reference.IndexOf(LogReferenceKey);
			var startVersionIndex = keyIndex + LogReferenceKey.Length;
			var endVersionIndex = reference.IndexOf('(', startVersionIndex);
			if (endVersionIndex > 0)
			{
				var versionText = reference.Substring(startVersionIndex, endVersionIndex - startVersionIndex);
				try
				{
					return new VersionNumber(versionText);
				}
				catch (FormatException)
				{
				}
			}

			return new VersionNumber();
		}

		IReadOnlyDictionary<(string ReleaseProduct, string ReleaseStatus), IEnumerable<VersionInfo>> LoadReleaseHistory()
		{
			const string sql =
@"SELECT HL_Product, HL_ReleaseStatus, HL_MajorVersion, HL_MinorVersion, HL_Release, Patch = MIN(HL_Patch), ReleaseDate = MIN(HL_ExeVersionDate)
FROM dbo.ReleaseBuild
WHERE (HL_ReleaseStatus IN ('GP1', 'GP2', 'STD', 'DPR')) AND (HL_Product IN ('ENT', 'CWN', 'CGW')) AND (HL_ExeVersionDate IS NOT NULL) AND HL_MajorVersion > 2
GROUP BY HL_Product, HL_ReleaseStatus, HL_MajorVersion, HL_MinorVersion, HL_Release
ORDER BY HL_Product, HL_ReleaseStatus, MIN(HL_ExeVersionDate) DESC";

			using (var dataTable = Utilities.GetDataTableFromQuery(sql))
			{
				return dataTable.Rows.OfType<DataRow>()
					.Select(row => new
					{
						ReleaseProduct = Convert.ToString(row["HL_Product"]),
						ReleaseRing = Convert.ToString(row["HL_ReleaseStatus"]),
						VersionNumber = new VersionNumber(Convert.ToInt32(row["HL_MajorVersion"]), Convert.ToInt32(row["HL_MinorVersion"]), Convert.ToInt32(row["HL_Release"]), Convert.ToInt32(row["Patch"])),
						VersionDate = Convert.ToDateTime(row["ReleaseDate"])
					})
					.GroupBy(x => (x.ReleaseProduct, x.ReleaseRing))
					.ToDictionary(k => k.Key, v => v.Select((x, i) => new VersionInfo(x.ReleaseProduct, x.ReleaseRing, x.VersionNumber, x.VersionDate, i)).ToArray().AsEnumerable());
			}
		}

		VersionInfo GetVersionForDate(LicenceDatabase db, DateTime date)
		{
			return ReleaseHistory.TryGetValue((GetReleaseProduct(db), db.LD_ReleaseRing), out var versionInfos)
				? versionInfos.FirstOrDefault(x => x.Date <= date) : null;
		}

		VersionInfo CreateVersionInfoFromVersionNumber(string releaseProduct, string releaseRing, VersionNumber versionNumber)
		{
			bool IsEqual(VersionNumber verA, VersionNumber verB)
				=> verA.Major == verB.Major
				&& verA.Minor == verB.Minor
				&& verA.Release == verB.Release;

			VersionInfo result = null;
			if (ReleaseHistory.TryGetValue((releaseProduct, releaseRing), out var versionInfos))
			{
				var versionInfo = versionInfos.FirstOrDefault(x => IsEqual(versionNumber, x.Version));
				if (versionInfo != null)
				{
					result = new VersionInfo(versionInfo.ReleaseProduct, versionInfo.ReleaseRing, versionNumber, versionInfo.Date, versionInfo.VersionIndex);
				}
			}
			return result;
		}

		static string GetReleaseProduct(LicenceDatabase db)
		{
			var product = (db?.CurrentVersion?.HL_Product ?? db?.LD_Product ?? ZString.Empty).ToString().ToUpper();
			return product switch
			{
				ProductTypes.Codes.CargoWiseOne => ProductTypes.Codes.Enterprise, //the HL_Product contains ENT only
				_ => product
			};
		}

		class VersionInfo
		{
			public VersionInfo(string releaseProduct, string releaseRing, VersionNumber versionNumber, DateTime date, int versionIndex)
			{
				ReleaseProduct = releaseProduct;
				ReleaseRing = releaseRing;
				Version = versionNumber;
				Date = date;
				VersionIndex = versionIndex;
			}

			public string ReleaseProduct { get; }
			public string ReleaseRing { get; }
			public VersionNumber Version { get; }
			public DateTime Date { get; }
			public int VersionIndex { get; }
		}

		public class VersionSurcharge
		{
			public VersionSurcharge(decimal surchargePercent, VersionNumber nonCurrentVersion)
			{
				SurchargePercent = surchargePercent;
				NonCurrentVersion = nonCurrentVersion;
			}
			public readonly decimal SurchargePercent;
			public readonly VersionNumber NonCurrentVersion;
		}

		readonly decimal SurchargePercentDefault;
		readonly decimal SurchargeAdditionalPercentDefault;
		readonly IReadOnlyDictionary<(string ReleaseProduct, string ReleaseStatus), IEnumerable<VersionInfo>> ReleaseHistory;
	}
}
