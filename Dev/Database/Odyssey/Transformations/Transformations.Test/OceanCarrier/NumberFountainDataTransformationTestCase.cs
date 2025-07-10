using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

abstract class NumberFountainDataTransformationTestCase<T> : DataTransformationTestCase where T : DataTransformation
{
	protected int CurrentSequenceValue { get; private set; }

	protected override DataTransformation GetNewTestTransformationInstance()
	{
		var transformation = Activator.CreateInstance<T>();
		var manager = new DummyUpgradeManager();
		transformation.Initialise(manager: manager);

		return transformation;
	}

	protected void LoadCurrentSequenceValue(string sequenceName)
	{
		var sql = $@"SELECT		ISNULL(last_used_value, 0)
							FROM	sys.sequences
							WHERE	name = '" + sequenceName + "'";

		var result = TestConnection.ExecuteScalar(sql);

		CurrentSequenceValue = result == DBNull.Value ? 0 : Convert.ToInt32(result);
	}

	protected void DeleteFountainProcedures()
	{
		var fountainProcedures = new List<string>();

		TestConnection.ExecuteReader(@"
				SELECT TOP 100 o.name as procedure_name, s.name as schema_name
				FROM sys.objects o
				JOIN sys.schemas s ON o.schema_id = s.schema_id
				WHERE o.type = 'P' and o.name like 'Fountain%'
				",
			record => fountainProcedures.Add($"{record["schema_name"]}.{record["procedure_name"]}"));

		if (fountainProcedures.Count > 0)
		{
			TestConnection.ExecuteNonQuery(string.Join("\r\n", fountainProcedures.Select(name => $"DROP PROCEDURE {name};")));
		}
	}
}
