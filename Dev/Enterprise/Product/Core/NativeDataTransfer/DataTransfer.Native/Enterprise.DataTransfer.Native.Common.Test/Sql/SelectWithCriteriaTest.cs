using System.Collections.Generic;
using System.Data;
using Enterprise.DataTransfer.Native.DB.Sql;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Sql
{
	public class SelectWithCriteriaTest : TransactionedTestCase
	{
		public void TestGenerateSQL()
		{
			var startDef = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			var endDef = TestUtil.FindEntityDefinition("Dummy", "DummyBizo.DummyDependentBizo");
			var generator = new SelectWithCriteria(startDef, endDef);
			var result = generator.GenerateSQL(keyToParameters);
			const string expected = @"SELECT DummyBizo.* FROM (SELECT DummyBizo.* FROM dbo.DummyBizo) AS DummyBizo INNER JOIN (SELECT DummyDependentBizo.* FROM dbo.DummyDependentBizo) AS DummyDependentBizo ON DummyBizo.Z0_PK = DummyDependentBizo.ZD1_Z0 WHERE DummyDependentBizo.ZD1_Z0 IS NOT NULL";
			AssertEquals(expected, result);
			AssertEquals(0, keyToParameters.Count);
		}

		public void TestGenerateSQLWithCriteria()
		{
			var startDef = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			var endDef = TestUtil.FindEntityDefinition("Dummy", "DummyBizo.DummyDependentBizo");
			var criteria = new Criteria()
			{
				TableName = "DummyBizo",
				ColumnName = "Z0_PK",
				Value = "812a670f-1d1a-486a-bec0-31ded4f27010"
			};

			var generator = new SelectWithCriteria(startDef, endDef, criteria);
			var result = generator.GenerateSQL(keyToParameters);
			string expected = @"SELECT DummyBizo.* FROM (SELECT DummyBizo.* FROM dbo.DummyBizo WHERE (Z0_PK=@param1)) AS DummyBizo INNER JOIN (SELECT DummyDependentBizo.* FROM dbo.DummyDependentBizo) AS DummyDependentBizo ON DummyBizo.Z0_PK = DummyDependentBizo.ZD1_Z0 WHERE DummyDependentBizo.ZD1_Z0 IS NOT NULL";
			AssertEquals(expected, result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "812a670f-1d1a-486a-bec0-31ded4f27010", SqlDbType.VarChar);
		}

		public void TestGenerateSQL_SelfReferenceRelationship()
		{
			var startDef = TestUtil.FindEntityDefinition("Declaration", "JobDeclaration.JobComInvoiceHeader.GroupInvoiceFK");
			var endDef = TestUtil.FindEntityDefinition("Declaration", "JobDeclaration.JobComInvoiceHeader");
			var generator = new SelectWithCriteria(startDef, endDef);
			var result = generator.GenerateSQL(keyToParameters);
			string expected = @"SELECT JobComInvoiceHeader_1.* FROM (SELECT JobComInvoiceHeader.* FROM dbo.JobComInvoiceHeader) AS JobComInvoiceHeader_1 INNER JOIN (SELECT JobComInvoiceHeader.* FROM dbo.JobComInvoiceHeader) AS JobComInvoiceHeader_2 ON JobComInvoiceHeader_1.JZ_PK = JobComInvoiceHeader_2.JZ_JZ_GroupInvoiceFK WHERE JobComInvoiceHeader_2.JZ_JZ_GroupInvoiceFK IS NOT NULL";
			AssertEquals(expected, result);
			AssertEquals(0, keyToParameters.Count);

			expected = @"SELECT JobComInvoiceHeader_1.* FROM (SELECT JobComInvoiceHeader.* FROM dbo.JobComInvoiceHeader) AS JobComInvoiceHeader_1 INNER JOIN (SELECT JobComInvoiceHeader.* FROM dbo.JobComInvoiceHeader) AS JobComInvoiceHeader_2 ON JobComInvoiceHeader_1.JZ_JZ_GroupInvoiceFK = JobComInvoiceHeader_2.JZ_PK WHERE JobComInvoiceHeader_1.JZ_JZ_GroupInvoiceFK IS NOT NULL";
			generator = new SelectWithCriteria(endDef, startDef);
			keyToParameters = new Dictionary<string, SqlParameter>();
			result = generator.GenerateSQL(keyToParameters);
			AssertEquals(expected, result);
			AssertEquals(0, keyToParameters.Count);
		}

		public void TestGenerateSQL_SelfReferenceRelationshipWithCriteria()
		{
			var startDef = TestUtil.FindEntityDefinition("Declaration", "JobDeclaration.JobComInvoiceHeader.GroupInvoiceFK");
			var endDef = TestUtil.FindEntityDefinition("Declaration", "JobDeclaration.JobComInvoiceHeader");
			var criteria = new Criteria()
			{
				TableName = "JobComInvoiceHeader",
				ColumnName = "JZ_PK",
				Value = "812a670f-1d1a-486a-bec0-31ded4f27010"
			};

			var generator = new SelectWithCriteria(startDef, endDef, criteria);
			string expected = @"SELECT JobComInvoiceHeader_1.* FROM (SELECT JobComInvoiceHeader.* FROM dbo.JobComInvoiceHeader) AS JobComInvoiceHeader_1 INNER JOIN (SELECT JobComInvoiceHeader.* FROM dbo.JobComInvoiceHeader WHERE (JZ_PK=@param1)) AS JobComInvoiceHeader_2 ON JobComInvoiceHeader_1.JZ_PK = JobComInvoiceHeader_2.JZ_JZ_GroupInvoiceFK WHERE JobComInvoiceHeader_2.JZ_JZ_GroupInvoiceFK IS NOT NULL";
			var result = generator.GenerateSQL(keyToParameters);
			AssertEquals(expected, result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "812a670f-1d1a-486a-bec0-31ded4f27010", SqlDbType.VarChar);
			keyToParameters.Clear();

			generator = new SelectWithCriteria(endDef, startDef, criteria);
			expected = @"SELECT JobComInvoiceHeader_1.* FROM (SELECT JobComInvoiceHeader.* FROM dbo.JobComInvoiceHeader) AS JobComInvoiceHeader_1 INNER JOIN (SELECT JobComInvoiceHeader.* FROM dbo.JobComInvoiceHeader WHERE (JZ_PK=@param1)) AS JobComInvoiceHeader_2 ON JobComInvoiceHeader_1.JZ_JZ_GroupInvoiceFK = JobComInvoiceHeader_2.JZ_PK WHERE JobComInvoiceHeader_1.JZ_JZ_GroupInvoiceFK IS NOT NULL";
			result = generator.GenerateSQL(keyToParameters);
			AssertEquals(expected, result);
			AssertEquals(1, keyToParameters.Count);
			AssertContainsParameter("@param1", "812a670f-1d1a-486a-bec0-31ded4f27010", SqlDbType.VarChar);
			keyToParameters.Clear();
		}

		void AssertContainsParameter(string paramName, string paramValue, SqlDbType paramType)
		{
			foreach (var keyToParameter in keyToParameters)
			{
				var param = keyToParameter.Value;
				if (param.Name == paramName && param.Value == paramValue && param.Type == paramType)
				{
					return;
				}
			}
			Fail(string.Format("parameters doesn't contain parameter with name {0}, value {1}, type {2}", paramName, paramValue, paramType));
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
			keyToParameters = new Dictionary<string, SqlParameter>();
		}

		Dictionary<string, SqlParameter> keyToParameters;
	}
}
