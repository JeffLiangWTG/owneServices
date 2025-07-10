using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformations.Customs.Shared.Testing
{
	abstract class ModifyDataModelTest : DataTransformationTestCase
	{
		protected abstract string TableName { get; }
		protected abstract string TablePrefix { get; }
		protected abstract string ColumnName { get; }
		protected abstract string TestColumnField { get; }
		protected abstract string TestColumnValue { get; }

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				var data = new Dictionary<string, int> { { "AU", 1 }, { "US", 1 }, { "CH", 1 }, { "FR", 7 } };
				foreach (var d in data)
				{
					AssertEquals(d.Value, Convert.ToInt32(Db.Connection.ExecuteScalar($"SELECT COUNT(1) FROM {TableName} WHERE {ColumnName} = '{d.Key}'")));
				}
			});
		}

		protected override void PrepareTestData()
		{
			if (DbObjectCreator.TableExists(Db.Connection, TableName) && DbObjectCreator.ColumnExists(Db.Connection, TableName, ColumnName))
			{
				var transformation = GetNewTestTransformationInstance();
				((ITransformationIndexProvider)transformation).IndexProvider.CreateIndexes(TestConnection);
				DropConstraints();
				var countryList = new List<string> { "AU", "PR", "LI", "GF", "GP", "MQ", "YT", "RE", "MF", "BL" };
				var clusterKey = 1;
				var stringBuilder = new StringBuilder();
				foreach (var country in countryList)
				{
					stringBuilder.Append($@"INSERT {TableName}({TestColumnField},{TablePrefix}_SystemCreateTimeUtc,{TablePrefix}_SystemCreateUser,{TablePrefix}_SystemLastEditTimeUtc,{TablePrefix}_SystemLastEditUser)
										values({TestColumnValue.Replace("%CountryCode%", country).Replace("%ClusterKey%", clusterKey++.ToString())},GetUTCDate(),'~BP',GetUtcDate(),'~BP');");
				}
				Db.Connection.ExecuteNonQuery(stringBuilder.ToString());
			}
		}

		void DropConstraints()
		{
			var constraints = new List<string>();
			using (var cmd = Db.Connection.Command($"SELECT constraint_name FROM information_schema.table_constraints WHERE table_name = '{TableName}' and constraint_type in ('CHECK','FOREIGN KEY') "))
			{
				var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					constraints.Add(reader[0].ToString());
				}
				reader.Close();
			}

			foreach (var constraint in constraints)
			{
				DBTransformationTestHelper.DropConstraintIfExists(TableName, constraint);
			}
		}
	}
}
