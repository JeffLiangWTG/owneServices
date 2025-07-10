using System;
using System.Data;
using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	sealed class AutoBusinessObjectTest : AutoCodeTestCase
	{
		public void TestConstructor()
		{
			var table = CreateTestDataTable();
			table.Columns.Add(new DataColumn("TT_SystemLastEditTimeUtc", typeof(DateTime)));
			var info = CreateInfo(table);
			info.DbTypes.Add("TT_SystemLastEditTimeUtc", "datetime");
			var testObject = new AutoBusinessObject(info);
			var source = testObject.SourceCode;
			Assert(source, source.Contains(@"		protected AutoTestTable(BusinessObjectFactory Factory, DataRow Row) : base(Factory, Row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(TT_SystemLastEditTimeUtc), ConcurrencyPolicy.Ignore);
		}"));
		}

		public void TestAutoBusinessObjectSchema()
		{
			AutoBusinessObject testObject = new AutoBusinessObject(CreateInfo(CreateTestDataTable()));
			AssertNotNull("Failed to create AutoBusinessObject", testObject);
			AssertEquals("Result", ExpectedSchemaResult, testObject.SchemaOLD);
		}

		public void TestAuditDetailsWithContextInterface()
		{
			var table = CreateTestDataTable();
			table.Columns.Add(new DataColumn("TT_SystemCreateTimeUtc", typeof(DateTime)));
			table.Columns.Add(new DataColumn("TT_SystemLastEditTimeUtc", typeof(DateTime)));
			table.Columns.Add(new DataColumn("TT_SystemCreateUser", typeof(string)));
			table.Columns.Add(new DataColumn("TT_SystemLastEditUser", typeof(string)));
			table.Columns.Add(new DataColumn("TT_SystemCreateBranch", typeof(string)));
			table.Columns.Add(new DataColumn("TT_SystemCreateDepartment", typeof(string)));
			var info = CreateInfo(table);
			info.DbTypes.Add("TT_SystemLastEditTimeUtc", "datetime");
			info.DbTypes.Add("TT_SystemCreateTimeUtc", "datetime");

			var testObject = new AutoBusinessObject(info);
			var source = testObject.SourceCode;
			string expectedCode = testObject.LinesOfCode(
				"		#region IAuditDetailsWithContext",
				"",
				"		ZString IAuditDetailsWithContext.SystemCreateBranch",
				"		{",
				"			get { return TT_SystemCreateBranch; }",
				"		}",
				"",
				"		ZString IAuditDetailsWithContext.SystemCreateDepartment",
				"		{",
				"			get { return TT_SystemCreateDepartment; }",
				"		}",
				"",
				"		#endregion");

			string message = testObject.LinesOfCode(
				"<b>Generated results should contain:</b>",
				"",
				"<code>" + expectedCode + "</code>",
				"",
				"<b>Actual generated code:</b>",
				"",
				"<code>" + testObject.SourceCode + "</code>"
				);

			HtmlAssertEquals(message, true, testObject.SourceCode.IndexOf(expectedCode) != -1);
		}

		public void TestLookups()
		{
			AutoBusinessObject testObject = new AutoBusinessObject(CreateInfo(CreateTestDataTable()));

			string expectedCode = testObject.LinesOfCode
				(
					"		#region Lookups",
					"",
					"		public TestTableLookups Lookups",
					"		{",
					"			get",
					"			{",
					"				if (fLookups == null || !IsLookupsCachedInBase)",
					"				{",
					"					fLookups = GetNewLookups();",
					"				}",
					"",
					"				return fLookups;",
					"			}",
					"		}",
					"",
					"		protected virtual TestTableLookups GetNewLookups()",
					"		{",
					"			return new TestTableLookups(this);",
					"		}",
					"",
					"		TestTableLookups fLookups;",
					"",
					"		#endregion"
				);

			string message = testObject.LinesOfCode(
				"<b>Generated results should contain:</b>",
				"",
				"<code>" + expectedCode + "</code>",
				"",
				"<b>Actual generated code:</b>",
				"",
				"<code>" + testObject.SourceCode + "</code>"
				);

			HtmlAssertEquals(message, true, testObject.SourceCode.IndexOf(expectedCode) != -1);
		}

		void AssertEquals(string message, string expected, string actual)
		{
			if (expected.IndexOf(System.Environment.NewLine) == -1 || actual.IndexOf(System.Environment.NewLine) == -1)
			{
				TestCase.AssertEquals(message, expected, actual);
			}
			else
			{
				string[] expectedStrings = expected.Split('\r', '\n');
				string[] actualStrings = actual.Split('\r', '\n');

				for (int i = 0; i < expectedStrings.Length; i++)
				{
					TestCase.AssertEquals(expectedStrings[i], actualStrings[i]);
				}
			}
		}
	}
}
