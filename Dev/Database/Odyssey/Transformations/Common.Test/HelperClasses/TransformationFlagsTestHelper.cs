using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	[CodeAlive("TransformationFlagsTestHelper")]
	public static class TransformationFlagsTestHelper
	{
		public static void DeleteTransformPreviouslyRunSql(string reference)
		{
			using (var command = Db.Connection.Command($"DELETE FROM dbo.StmData WHERE SD_Name = @SD_Name"))
			{
				command.AddParameterBasedOnDbColumn("@SD_Name", reference, StmDataSchema.SD_Name);
				command.ExecuteNonQuery();
			}
		}

		public static int CountTransformPreviouslyRun(string reference)
		{
			using (var command = Db.Connection.Command($"SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = @SD_Name"))
			{
				command.AddParameterBasedOnDbColumn("@SD_Name", reference, StmDataSchema.SD_Name);
				return (int)command.ExecuteScalar();
			}
		}
	}
}
