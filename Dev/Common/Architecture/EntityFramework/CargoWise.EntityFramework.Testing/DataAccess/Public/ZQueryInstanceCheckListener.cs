using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	// We create a new ConcurrentBag instead of using Clear() because net48 ConcurrentBag does not have a Clear() method
	public class ZQueryInstanceCheckListener : BaseTestListener
	{
		string currentTestName;

		public override void StartAllTests(DateTime startTime)
		{
			ZQuery.InstancesCacheForUnitTest = new();
			ZQuery.EnableRecordAllInstances = true;
		}

		public override void EndAllTests(DateTime endTime)
		{
			ZQuery.EnableRecordAllInstances = false;
			ZQuery.InstancesCacheForUnitTest = new();
		}

		public override void StartTest(TestCase test, DateTime startTime)
		{
			currentTestName = test?.Name;

			base.StartTest(test, startTime);
		}

		public override void AfterEachTest(DateTime endTime)
		{
			var issuedZQuery = new List<string>();
			if (currentTestName == null || !currentTestName.Equals("TestSparseColumnQuery"))
			{
				foreach (var zQuery in ZQuery.InstancesCacheForUnitTest)
				{
					var columns = RecursiveGetSparseColumns(zQuery);
					foreach (var column in columns)
					{
						if (zQuery.LiteralTextADO.IndexOf($"OR {column.Name} is NULL", StringComparison.OrdinalIgnoreCase) == -1 && zQuery.LiteralTextADO.IndexOf($"AND {column.Name} is not NULL", StringComparison.OrdinalIgnoreCase) == -1)
						{
							issuedZQuery.Add(zQuery.LiteralTextADO);
						}
					}
				}
			}

			ZQuery.InstancesCacheForUnitTest = new();

			if (issuedZQuery.Count > 0)
			{
				Assertion.Fail(@$"
sparse column should use sparse filter.
Queries:
{string.Join("\r\n", issuedZQuery)}
");
			}

			base.AfterEachTest(endTime);
		}

		List<SchemaColumn> RecursiveGetSparseColumns(IFilterPart filterPart)
		{
			var columns = new List<SchemaColumn>();

			foreach (var part in filterPart.FilterParts)
			{
				if (part is IZSqlParameter)
				{
					var param = part as ZSqlParameter;
					if (param.SchemaColumn.IsSparse && param.ComparisonOperator.IsNegativeSQLOperator() && param.Value.GetType() != typeof(System.DBNull))
					{
						columns.Add(param.SchemaColumn);
					}
				}
				else
				{
					columns.AddRange(RecursiveGetSparseColumns(part));
				}
			}

			return columns;
		}
	}
}
