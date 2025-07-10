using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Security.Testing
{
	abstract class CopySecurityTransformationTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var mapping = ((CopySecurityTransformation)TransformationToTest).SecurityCheckPointMappings;
			AssertNotNull(mapping);

			StringBuilder insertSql = new StringBuilder();
			foreach (string oldSecurityRight in mapping.Select(m => m.Source).Distinct())
			{
				insertSql.AppendLine(string.Format(@"INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GS) VALUES (newid(), 1, '{0}', '70EFA270-3F0F-479C-9AED-0009455622E2')
													 INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GG) VALUES (newid(), 1, '{0}', '94755E71-A87A-4034-8DFA-785773A49607')", oldSecurityRight));
			}

			string[] checkPoints = new string[mapping.Length * 2];
			mapping.Select(m => m.Source).ToArray().CopyTo(checkPoints, 0);
			mapping.Select(m => m.Destination).ToArray().CopyTo(checkPoints, mapping.Length);

			string deleteSql = string.Format("DELETE FROM dbo.GlbSecurity WHERE GU_SecurityRight IN ('{0}')", string.Join("', '", checkPoints));

			using (DbCommand cmd = Db.Connection.Command(deleteSql))
			{
				cmd.ExecuteNonQuery();
			}

			using (DbCommand cmd = Db.Connection.Command(insertSql.ToString()))
			{
				cmd.ExecuteNonQuery();
			}

			AssertPreTransformationData(mapping);
		}

		void AssertPreTransformationData(SecurityCheckPointMapping[] mapping)
		{
			foreach (var item in mapping)
			{
				DbCommand oldCheckpointCommand = Db.Connection.Command(string.Format(@"SELECT count(*) FROM dbo.GlbSecurity WHERE GU_SecurityRight = '{0}'", item.Source));
				AssertEquals(string.Format("Pre-Transformation: {0} count", item.Destination), 2, oldCheckpointCommand.ExecuteScalar());

				DbCommand newCheckpointCommand = Db.Connection.Command(string.Format(@"SELECT count(*) FROM dbo.GlbSecurity WHERE GU_SecurityRight = '{0}'", item.Destination));
				AssertEquals(string.Format("Pre-Transformation: {0} count", item.Source), 0, newCheckpointCommand.ExecuteScalar());
			}
		}

		protected override void AssertTransformationResults()
		{
			var mapping = ((CopySecurityTransformation)TransformationToTest).SecurityCheckPointMappings;

			foreach (var item in mapping)
			{
				DbCommand oldCheckpointCommand = Db.Connection.Command(string.Format(@"SELECT count(*) FROM dbo.GlbSecurity WHERE GU_SecurityRight = '{0}'", item.Source));
				AssertEquals(string.Format("Post-Transformation: {0} count", item.Destination), 2, oldCheckpointCommand.ExecuteScalar());

				DbCommand newCheckpointCommand = Db.Connection.Command(string.Format(@"SELECT count(*) FROM dbo.GlbSecurity WHERE GU_SecurityRight = '{0}'", item.Destination));
				AssertEquals(string.Format("Post-Transformation: {0} count", item.Source), 2, newCheckpointCommand.ExecuteScalar());
			}
		}
	}
}
