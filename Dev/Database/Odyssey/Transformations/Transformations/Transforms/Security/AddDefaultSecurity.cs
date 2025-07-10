using System.Data;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("An abstract data transformation that will continue to be useful so long as security rights exist and change over time.")]
	abstract class AddDefaultSecurity : DataTransformation
	{
		protected override void OfflinePostUpgradeTransform()
		{
			var sqlText =
@"DECLARE @groupPK uniqueidentifier
SET @groupPK = (SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Code = @groupCode)
IF NOT EXISTS (SELECT * FROM dbo.GlbSecurity WHERE GU_SecurityRight = @securityRight AND GU_GG = @groupPk)
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GG, GU_IsValid) VALUES (newid(), @defaultValue, @securityRight, @groupPk, '1')";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("securityRight", SqlDbType.VarChar, SecurityRight);
				cmd.AddParameter("groupCode", SqlDbType.VarChar, "ALL");
				cmd.AddParameter("defaultValue", SqlDbType.Bit, DefaultValue);
				cmd.ExecuteNonQuery();
			}
		}

		internal abstract string SecurityRight { get; }

		internal virtual bool DefaultValue => false;
	}
}
