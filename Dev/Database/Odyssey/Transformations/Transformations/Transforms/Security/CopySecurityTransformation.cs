using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Security
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("An abstract data transformation that will continue to be useful so long as security rights exist and change over time.")]
	abstract class CopySecurityTransformation : DataTransformation
	{
		/// <summary>
		/// key: New SecurityCheckPoint Name to which the value will be copied over | value: Old SecurityCheckPoint Name from which the value will be copied
		/// </summary>
		public abstract SecurityCheckPointMapping[] SecurityCheckPointMappings { get; }

		protected override void OfflinePostUpgradeTransform()
		{
			foreach (var item in SecurityCheckPointMappings)
			{
				string insertSql = string.Format(@"
IF NOT EXISTS(SELECT 1 FROM dbo.GlbSecurity WHERE GU_SecurityRight = '{0}')
	INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityRight, GU_ItemGUID, GU_GB, GU_GE, GU_GS, GU_GG, GU_GC, GU_IsValid, GU_SecurityItemIsAllowed)
	SELECT NEWID(), '{0}', GU_ItemGUID, GU_GB, GU_GE, GU_GS, GU_GG, GU_GC, GU_IsValid, GU_SecurityItemIsAllowed FROM dbo.GlbSecurity WHERE GU_SecurityRight = '{1}'
", item.Destination, item.Source);
				Db.Connection.Command(insertSql).ExecuteNonQuery();
			}
		}
	}

	public class SecurityCheckPointMapping
	{
		public SecurityCheckPointMapping(string source, string destination)
		{
			Source = source;
			Destination = destination;
		}

		public string Source { get; }
		public string Destination { get; }
	}
}
