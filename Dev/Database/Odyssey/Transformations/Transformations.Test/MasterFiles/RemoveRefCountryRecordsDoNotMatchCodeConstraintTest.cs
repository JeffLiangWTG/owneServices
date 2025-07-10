using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.MasterFiles
{
	[TestedType(typeof(RemoveRefCountryRecordsDoNotMatchCodeConstraint))]
	class RemoveRefCountryRecordsDoNotMatchCodeConstraintTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() =>
			new RemoveRefCountryRecordsDoNotMatchCodeConstraint();

		protected override void AssertPreConditions()
		{
			base.AssertPreConditions();

			AssertEquals(expected: true, Db.Connection.Exists("FROM RefCountry WHERE LEN(RN_Code) <> 2"));
		}

		protected override void PrepareTestData()
		{
			DropExistingConstraints(RefCountrySchema.RN_Code);

			var helper = new TransformationTestDataCreator();
			emptyCountryPk = helper.CreateRefCountry(string.Empty, "Test Empty Country", "USD");
			invalidCountryPk = helper.CreateRefCountry("A", "Test Invalid Country", "USD");
			validCountryPk = helper.CreateRefCountry("ZZ", "Test Valid Country", "USD");

			refLocoMapToDeletePk = helper.CreateRefLocoMap("A001", "P001", emptyCountryPk);
			refLocoMapToStayPk = helper.CreateRefLocoMap("A002", "P002", validCountryPk);
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				AssertRecord(exists: false, RefCountrySchema.PK, emptyCountryPk);
				AssertRecord(exists: false, RefCountrySchema.PK, invalidCountryPk);
				AssertRecord(exists: true, RefCountrySchema.PK, validCountryPk);
				AssertRecord(exists: false, RefLocoMapSchema.PK, refLocoMapToDeletePk);
				AssertRecord(exists: true, RefLocoMapSchema.PK, refLocoMapToStayPk);
				AssertEquals(expected: false, Db.Connection.Exists("FROM RefCountry WHERE LEN(RN_Code) <> 2"));
			});
		}

		void AssertRecord(bool exists, SchemaColumn column, Guid pk)
		{
			AssertEquals(exists, Db.Connection.Exists($"FROM {column.TableName} WHERE {column.Name}='{pk}'"));
		}

		void DropExistingConstraints(SchemaColumn column)
		{
			var sql = $@"SELECT cc.[name]
						FROM sys.check_constraints cc
							INNER JOIN sys.tables t
									ON t.object_id = cc.parent_object_id
							INNER JOIN sys.columns c
									ON c.object_id = cc.parent_object_id
										AND c.column_id = cc.parent_column_id
						WHERE t.[name] = '{column.TableName}'
								AND c.[name] = '{column.Name}'";

			var constraints = new List<string>();
			using (var cmd = Db.Connection.Command(sql))
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
				DBTransformationTestHelper.DropConstraintIfExists(column.TableName, constraint);
			}
		}

		Guid emptyCountryPk;
		Guid invalidCountryPk;
		Guid validCountryPk;
		Guid refLocoMapToDeletePk;
		Guid refLocoMapToStayPk;
	}
}
