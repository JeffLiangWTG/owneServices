using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DbUpgrader.Transformations.Transforms.DocumentScanning
{
	public class PopulateRT_ParseTypeInRefDocTypeTableOffline : DataTransformation
	{
		public override string UserDescription => "Populate pre-defined RT_ParseType for doc types in RefDocType table.";

		protected override void OfflinePostUpgradeTransform()
		{
			base.OfflinePostUpgradeTransform();

			var sql = string.Empty;
			foreach (var docType in ParseTypes.Keys)
			{
				sql += $@"
Update dbo.RefDocType
Set
	RT_ParseType = '{ParseTypes[docType]}',
	RT_SystemLastEditUser = '~BP',
	RT_SystemLastEditTimeUtc = GetUtcDate()
Where RT_IsSystem = 1 And RT_DocType = '{docType}';";
			}

			_ = Db.Connection.ExecuteNonQuery(sql);
		}

		[ThreadSafe]
		public static readonly Dictionary<string, string> ParseTypes = new ()
		{
			{ "CIV", "CIV" },
			{ "PIN", "PIN" },
			{ "PKL", "PKL" },
			{ "HBL", "BOL" },
			{ "MBL", "BOL" },
		};
	}
}
