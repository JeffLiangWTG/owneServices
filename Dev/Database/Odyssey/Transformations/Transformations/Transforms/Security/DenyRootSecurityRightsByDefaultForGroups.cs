using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.Security
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("An abstract data transformation that will continue to be useful so long as security rights exist and change over time.")]
	public abstract class DenyRootSecurityRightsByDefaultForGroups : DataTransformation
	{
		public override string UserDescription
		{
			get { return "Deny root security rights by default for groups"; }
		}

		public abstract IEnumerable<string> SecurityRights { get; }

		protected override void OfflinePostUpgradeTransform()
		{
			if (!SecurityRights.Any())
			{
				return;
			}

			var groupPKs = new List<Guid>();

			var sql = "SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Type = 'STF';";
			using (var command = Db.Connection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					groupPKs.Add(reader.GetGuid(0));
				}
			}

			foreach (var groupPK in groupPKs)
			{
				foreach (string securityRight in SecurityRights)
				{
					DenySecurityRightIfNotExists(groupPK, securityRight);
				}
			}
		}

		protected void DenySecurityRightIfNotExists(Guid groupPK, string securityRight)
		{
			var shouldInsert = false;

			var sql = string.Format(
				CultureInfo.InvariantCulture,
@"
SELECT COUNT(*)
FROM dbo.GlbSecurity
WHERE
	GU_SecurityRight = '{0}'
	AND GU_GG = '{1}'
	AND GU_GB IS NULL
	AND GU_GE IS NULL
	AND GU_GC IS NULL;",
				securityRight,
				groupPK);
			using (var command = Db.Connection.Command(sql))
			{
				var count = (int)command.ExecuteScalar();
				shouldInsert = count == 0;
			}

			if (shouldInsert)
			{
				sql = string.Format(
					CultureInfo.InvariantCulture,
@"
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityRight, GU_ItemGUID, GU_GB, GU_GE, GU_GS, GU_GG, GU_GC, GU_IsValid, GU_SecurityItemIsAllowed)
VALUES (NEWID(), '{0}', NULL, NULL, NULL, NULL, '{1}', NULL, 1, 0);",
					securityRight,
					groupPK);
				using (var command = Db.Connection.Command(sql))
				{
					command.ExecuteNonQuery();
				}
			}
		}
	}
}
