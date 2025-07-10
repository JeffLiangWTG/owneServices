using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestsSubclassesOf(typeof(IAuditParent), RequireTestOnlyInFirstSubLevel = true)]
	public abstract class AuditParentTest<T> : TestCaseWithFactory where T : IAuditParent
	{
		public void TestIsEnterpriseBusinessObjectAndAudited()
		{
			var testAuditParent = NewTestAuditParent();
			var bizObj = testAuditParent as EnterpriseBusinessObject;
			AssertNotNull("testAuditParent as EnterpriseBusinessObject", bizObj);
			AssertColumnExistsInAuditDb(bizObj.PKSchemaColumn);
		}

		public void TestRelatedColumnsAreAudited()
		{
			var testAuditParent = NewTestAuditParent();
			var relatedColumns = testAuditParent.RelatedAuditChildren.ToArray();

			if (relatedColumns.Length == 0)
			{
				Assert("No related columns to test", true);
			}

			CombineAssertions(() =>
			{
				foreach (var relatedColumn in relatedColumns)
				{
					AssertColumnExistsInAuditDb(relatedColumn.KeyColumn);
				}
			});
		}

		public void TestRelatedColumnsAreUnique()
		{
			var testAuditParent = NewTestAuditParent();

			var duplicateRelatedColumns =
				from c in testAuditParent.RelatedAuditChildren
				group c by new { c.KeyColumn.TableName, c.KeyColumn.Name } into grp
				where grp.Count() > 1
				select "\t[" + grp.Key.TableName + "].[" + grp.Key.Name + "]";

			AssertEquals("The following related columns are duplicated:\r\n\r\n" + String.Join("\r\n", duplicateRelatedColumns) + "\r\n", 0, duplicateRelatedColumns.Count());
		}

		void AssertColumnExistsInAuditDb(SchemaColumn keyColumn)
		{
			string sqlSchema = keyColumn.TableSchema.SqlSchemaName;
			string tableName = keyColumn.TableName;
			string columnName = keyColumn.Name;

			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS (
					SELECT 1
					FROM
						[{0}].sys.columns c
						INNER JOIN [{0}].sys.tables t ON t.object_id = c.object_id
						INNER JOIN [{0}].sys.schemas s ON s.schema_id = t.schema_id
					WHERE
						s.name = '{1}'
						AND t.name = '{2}'
						AND c.name = '{3}'
				) SELECT 1 ELSE SELECT 0;",
				Db.AuditDatabaseName,
				sqlSchema,
				tableName,
				keyColumn.Name);

			AssertEquals(
				String.Format(CultureInfo.InvariantCulture, "Column [{0}].[{1}].[{2}] exists in the audit database.", sqlSchema, tableName, columnName),
				true,
				Convert.ToBoolean(((IDbConnected)Factory).Connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
		}

		protected abstract T NewTestAuditParent();
	}
}
