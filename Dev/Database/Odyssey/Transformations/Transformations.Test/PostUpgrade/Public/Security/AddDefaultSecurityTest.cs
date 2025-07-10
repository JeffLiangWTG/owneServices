using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.Testing
{
	abstract class AddDefaultSecurityTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			using (var cmd = Db.Connection.Command(
				@"delete from dbo.GlbSecurity where GU_SecurityRight = @securityRight and GU_GG = @groupPk"))
			{
				cmd.AddParameter("securityRight", SqlDbType.VarChar, ((AddDefaultSecurity)TransformationToTest).SecurityRight);
				cmd.AddParameter("groupPk", SqlDbType.UniqueIdentifier, Core.Constants.Groups.AllPK);
				cmd.ExecuteNonQuery();
			}
		}

		protected override void AssertTransformationResults()
		{
			using (var cmd = Db.Connection.Command(
				@"select GU_SecurityItemIsAllowed from dbo.GlbSecurity where GU_SecurityRight = @securityRight and GU_GG = @groupPk"))
			{
				cmd.AddParameter("securityRight", SqlDbType.VarChar, ((AddDefaultSecurity)TransformationToTest).SecurityRight);
				cmd.AddParameter("groupPk", SqlDbType.UniqueIdentifier, Core.Constants.Groups.AllPK);
				AssertEquals(ExpectedDefaultValue, cmd.ExecuteScalar());
			}
		}

		protected virtual bool ExpectedDefaultValue => false;
	}
}
