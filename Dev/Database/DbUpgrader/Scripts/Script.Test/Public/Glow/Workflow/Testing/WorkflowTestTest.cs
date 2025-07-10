using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Data;
using NUnit.Framework.TestHelper;

namespace Enterprise.Build.Database.Script.Public.Glow.Workflow.Testing
{
	abstract class WorkflowTest<TItem> : DbCreateScriptTest
	{
		public void TestViewContainsCorrectItems()
		{
			var allGuids = new List<Guid>();
			AllItems.ForEach(t => allGuids.Add(GenerateNewItem(t)));

			var guids = new List<Guid>();
			using (var command = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, "SELECT {0} FROM {1}", PKName, TestedTypeHelper.GetTestedType(GetType()).Name)))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					guids.Add(reader.GetGuid(0));
				}
			}

			AssertEquals("Precondition: Number of expected items is the same as all items", ExpectedItems.Length, AllItems.Count);

			for (var i = 0; i < ExpectedItems.Length; i++)
			{
				if (ExpectedItems[i])
				{
					AssertCollectionContains(allGuids[i], guids);
				}
				else
				{
					AssertCollectionNotContains(allGuids[i], guids);
				}
			}
		}

		public void TestViewContainsCorrectColumns()
		{
			var columns = new List<string>();
			using (var command = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, "SELECT COLUMN_NAME FROM information_schema.columns WHERE TABLE_NAME = '{0}'", TestedTypeHelper.GetTestedType(GetType()).Name)))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					columns.Add(reader.GetString(0));
				}
			}

			AssertEquals("Precondition: Number of expected columns is the same as columns", columns.Count, ExpectedColumns.Length);

			AssertContainsExactElementsInAnyOrder(ExpectedColumns, columns);
		}

		protected abstract string PKName { get; }

		protected abstract bool[] ExpectedItems { get; }

		protected abstract string[] ExpectedColumns { get; }

		protected abstract List<TItem> AllItems { get; }

		protected abstract Guid GenerateNewItem(TItem item);
	}
}

