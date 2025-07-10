using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngineCore.Testing
{
	sealed class GetCustomFieldParameterParserTest : TestCaseWithFactory
	{
		public void TestParamParse()
		{
			var paramAndType = GetCustomFieldParameterParser.ExtractParameters("blah, \"STR\"");
			AssertEquals("blah", paramAndType.FieldName);
			AssertEquals("STR", paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters("blah");
			AssertEquals("blah", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters("blah,");
			AssertEquals("blah,", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters("blah, STR");
			AssertEquals("blah", paramAndType.FieldName);
			AssertEquals("STR", paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters("blah, TR");
			AssertEquals("blah, TR", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters("blah, \"TR\"");
			AssertEquals("blah, \"TR\"", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters("blah, \"\"");
			AssertEquals("blah, \"\"", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters("blah, \"STRA\"");
			AssertEquals("blah, \"STRA\"", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters("blah, T. ,STR");
			AssertEquals("blah, T.", paramAndType.FieldName);
			AssertEquals("STR", paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters("");
			AssertEquals("", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters("\t");
			AssertEquals("\t", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters(" ");
			AssertEquals(" ", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters("STR");
			AssertEquals("STR", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters(" , STR");
			AssertEquals(" , STR", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters("\"STR\"");
			AssertEquals("\"STR\"", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);

			paramAndType = GetCustomFieldParameterParser.ExtractParameters(" , \"STR\"");
			AssertEquals(" , \"STR\"", paramAndType.FieldName);
			AssertEquals(null, paramAndType.TypeName);
		}
	}
}
