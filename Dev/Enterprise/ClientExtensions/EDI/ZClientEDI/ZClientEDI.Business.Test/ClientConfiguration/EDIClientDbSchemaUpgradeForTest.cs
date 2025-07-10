using System;
using System.Linq;
using CargoWise.Data;

namespace ZClientEDI.Business.Test
{
	public static class EDIClientDbSchemaUpgradeForTest
	{
		public static void UpgradeViewClientProcessHeader(DbConnection connection)
		{
			var clientProcessHeader = ViewClientProcessHeaderCreateScript.Create();

			var scriptIndex = CargoWise.DbUpgrader.Scripts.Definitions.CoreScriptIndex.GetScripts();

			var dependencies = new string[]
			{
				"ViewProcessHeader",
				"TG_UPD_ViewProcessHeader",
				"Report_ContainmentBarrierOutcomes"
			};

			var dropScripts = new string[]
			{
				"drop trigger TG_UPD_ViewProcessHeader",
				"drop function Report_ContainmentBarrierOutcomes",
				"drop view ViewProcessHeader"
			};

			foreach (var dropScript in dropScripts)
			{
				connection.ExecuteNonQuery(dropScript);
			}

			connection.ExecuteNonQuery(clientProcessHeader.DropScript);

			connection.ExecuteNonQuery(clientProcessHeader.CreateScript);

			foreach (var dependency in dependencies)
			{
				var script = scriptIndex.FirstOrDefault(s => s.Name.Equals(dependency, StringComparison.InvariantCultureIgnoreCase));

				connection.ExecuteNonQuery(script.Text);
			}
		}
	}
}
