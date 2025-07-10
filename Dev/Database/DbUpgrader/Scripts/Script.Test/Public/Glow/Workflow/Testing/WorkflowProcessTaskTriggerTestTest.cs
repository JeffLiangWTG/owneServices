using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Public.Glow.Workflow.Testing
{
	abstract class WorkflowProcessTaskTriggerTest<TView> : DbCreateScriptTest
	{
		public void TestInsert()
		{
			var viewName = typeof(TView).Name;

			ClearDataForTest(viewName);

			var rowPK = GetProcessTaskItem(viewName);
			var guids = new List<Guid>();

			using (var command = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, "SELECT P9_PK FROM {0}", viewName)))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					guids.Add(reader.GetGuid(0));
				}
			}

			AssertEquals("Only one item returned", guids.Count, 1);
			AssertEquals("Returned PK is equal to inserted", rowPK, guids[0]);
		}

		protected abstract string ProcessTaskType { get; }

		#region Helpers

		void ClearDataForTest(string viewName)
		{
			using (var command = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0}", viewName)))
			{
				command.ExecuteNonQuery();
			}
		}

		Guid GetProcessTaskItem(string viewName)
		{
			var sql = string.Format("insert into {0} (P9_PK, P9_SystemCreateUser, P9_SystemLastEditUser) values (@PK, '', '')", viewName);
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);

				command.ExecuteNonQuery();
			}

			return pk;
		}
		#endregion
	}
}

