using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	public class FixUcTransformationEncoding : DataTransformation
	{
		public override string UserDescription => "Cleanup data for new constraint on column B5_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			if (!HasAffectedCountry())
			{
				return;
			}

			var sql =
				$"""
				UPDATE dbo.StmModuleFilter
				SET
					S9_FilterData = dbo.CLRCompressAsBytes(
						CAST(SUBSTRING(dbo.CLRUncompressAsBytes(S9_FilterData), 3, LEN(dbo.CLRUncompressAsBytes(S9_FilterData)) - 3) AS VARBINARY(MAX))
					),
					S9_SystemLastEditTimeUtc = GETUTCDATE(),
					S9_SystemLastEditUser = '~BP'
				WHERE 
					({WhereClause})
					AND SUBSTRING(dbo.CLRUncompressAsBytes(S9_FilterData), 0, 3) = CAST(16191 AS BINARY(2)) -- 0x3F3F
				""";
			Db.Connection.ExecuteNonQuery(sql);
		}

		static bool HasAffectedCountry()
		{
			var sql = $"FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode IN ('{string.Join("', '", CountryModuleIds.Select(pair => pair.CountryCode))}')";
			return Db.Connection.Exists(sql);
		}

		internal static IReadOnlyCollection<(string CountryCode, IReadOnlyCollection<string> ModuleIds)> CountryModuleIds =>
		[
			("KR", ["CusDec_UC"]),
			("TW", ["Enterprise.Customs.TW.Business.JobComInvoiceLine_UC"]),
			("BR", ["CusDec_UC"]),
			("CN", ["CusDec_UC", "Enterprise.Customs.CN.Business.JobComInvoiceLine_UC"])
		];

		internal static string WhereClause => string.Join(
			" OR ",
			CountryModuleIds.Select(pair => $"(S9_GC IN (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = '{pair.CountryCode}') AND S9_ModuleId IN ('{string.Join("', '", pair.ModuleIds)}'))")
		);
	}
}
