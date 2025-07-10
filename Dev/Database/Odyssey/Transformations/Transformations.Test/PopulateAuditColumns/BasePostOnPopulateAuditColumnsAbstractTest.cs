using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.PopulateAuditColumns;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation.Testing
{
	[TestsSubclassesOf(typeof(BasePostOnPopulateAuditColumns<>))]
	abstract class BasePostOnPopulateAuditColumnsAbstractTest<TTransformation, TTableSchema> : DataTransformationTestCase
		where TTransformation : BasePostOnPopulateAuditColumns<TTableSchema>
		where TTableSchema : ITableSchema
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return Activator.CreateInstance<TTransformation>();
		}

		protected abstract string PrepareTestDataSql { get; }
		protected abstract string AssertTransformationResultSql { get; }
		protected abstract string ExpectedResult { get; }

		void DisableAuditDetailsAreNotMissingInsertTrigger()
		{
			Db.Connection.ExecuteNonQuery($@"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = '{InsertTriggerName}')
BEGIN
	DISABLE TRIGGER {InsertTriggerName} ON {TableName}
END");
		}

		void EnableAuditDetailsAreNotMissingInsertTrigger()
		{
			Db.Connection.ExecuteNonQuery($@"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = '{InsertTriggerName}')
BEGIN
	ENABLE TRIGGER {InsertTriggerName} ON {TableName}
END");
		}

		protected override void PrepareTestData()
		{
			DisableAuditDetailsAreNotMissingInsertTrigger();
			Db.Connection.ExecuteNonQuery(PrepareTestDataSql);
			EnableAuditDetailsAreNotMissingInsertTrigger();
		}

		protected override void AssertTransformationResults()
		{
			var resultList = new List<string>();
			Db.Connection.ExecuteReader(AssertTransformationResultSql, reader => resultList.Add($"{reader[0]}, {BasePostOnPopulateAuditColumnsBaseOnlyTest.ConvertDateToTest(reader[1])}, '{reader[2]}', {BasePostOnPopulateAuditColumnsBaseOnlyTest.ConvertDateToTest(reader[3])}, '{reader[4]}'".ToUpper()));
			AssertMultilineASCIIEquals("Result", ExpectedResult, string.Join("\r\n", resultList));

			AssertNull("ExtProperty.Table should have been cleared.", BasePostOnPopulateAuditColumnsBaseOnlyTest.GetTableExtProperty(((BasePostOnPopulateAuditColumns<TTableSchema>)GetNewTestTransformationInstance()).TableSchema));
			CheckAuditDetailsAreNotMissingTriggersEnabled();
		}

		ITableSchema TableSchema => tableSchema ?? (tableSchema = (ITableSchema)typeof(TTableSchema).GetField("Instance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null));
		ITableSchema tableSchema;
		string TableName => TableSchema.TableName;
		string InsertTriggerName => $@"TG_{TableName}_AuditDetailsAreNotMissing_Insert";
		string UpdateTriggerName => $@"TG_{TableName}_AuditDetailsAreNotMissing_Update";

		void CheckAuditDetailsAreNotMissingTriggersEnabled()
		{
			var sql = $@"SELECT name, is_disabled FROM sys.triggers where name in ('{InsertTriggerName}', '{UpdateTriggerName}')";
			var resultList = new List<string>();
			Db.Connection.ExecuteReader(sql, reader => resultList.Add(reader[0].ToString() + "," + reader[1].ToString()));

			var insertTriggerResult = resultList.FirstOrDefault(r => r.StartsWith(InsertTriggerName));
			if (insertTriggerResult != null)
			{
				AssertEquals(InsertTriggerName + " should be enbaled after run.", InsertTriggerName + ",False", insertTriggerResult);
			}

			var updateTriggerResult = resultList.FirstOrDefault(r => r.StartsWith(UpdateTriggerName));
			if (updateTriggerResult != null)
			{
				AssertEquals(UpdateTriggerName + " should be enbaled after run.", UpdateTriggerName + ",False", updateTriggerResult);
			}
		}
	}
}
