using System.Data;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class MultipleChoiceSqlDbTypeHelperTest : TestCase
	{
		public void TestUpdateParamSqlDbTypeBasedOnStringValue_Char()
		{
			AssertUpdateParamSqlDbTypeBasedOnStringValue(new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.Char), SqlDbType.Char, SqlDbType.NChar);
			AssertUpdateParamSqlDbTypeBasedOnStringValue(new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.NChar), SqlDbType.Char, SqlDbType.NChar);
			AssertUpdateParamSqlDbTypeBasedOnStringValue(new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.VarChar), SqlDbType.VarChar, SqlDbType.NVarChar);
			AssertUpdateParamSqlDbTypeBasedOnStringValue(new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.NVarChar), SqlDbType.VarChar, SqlDbType.NVarChar);
		}

		void AssertUpdateParamSqlDbTypeBasedOnStringValue(SqlParameter param, SqlDbType expectedCharType, SqlDbType expectedNCharType)
		{
			param.Value = "France";
			SqlDbTypeDecider.UpdateParamSqlDbTypeBasedOnStringValue(param);
			AssertEquals(expectedCharType, param.SqlDbType);

			param.Value = "日本";
			SqlDbTypeDecider.UpdateParamSqlDbTypeBasedOnStringValue(param);
			AssertEquals(expectedNCharType, param.SqlDbType);

			param.Value = "Australia";
			SqlDbTypeDecider.UpdateParamSqlDbTypeBasedOnStringValue(param);
			AssertEquals(expectedCharType, param.SqlDbType);

			param.Value = "New 日本";
			SqlDbTypeDecider.UpdateParamSqlDbTypeBasedOnStringValue(param);
			AssertEquals(expectedNCharType, param.SqlDbType);

			param.Value = "";
			SqlDbTypeDecider.UpdateParamSqlDbTypeBasedOnStringValue(param);
			AssertEquals(expectedCharType, param.SqlDbType);
		}
	}
}
