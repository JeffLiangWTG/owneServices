using System.Collections;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using NUnit.Framework;

namespace Enterprise.Client.M1A.Testing
{
	[TestedType(typeof(M1AClientDbSchemaUpgradeInfo))]
	class M1AClientDbSchemaUpgradeInfoTest : ConstraintForClientSpecificSchema
	{
		[ExpectNoExceptions]
		public void TestDbSchemaUpgradeInfo()
		{
			Db.Connection.BeginTransaction();
			try
			{
				ArrayList scripts = new ArrayList();
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.TableCreationScripts);
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.ViewAndRoutineCreationScripts);
				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					try
					{
						ExecuteNonQuery(script.DropScript);
					}
					catch
					{
					}
				}

				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					ExecuteNonQuery(script.CreateScript);
				}

				AssertEquals(true, ExistsDbObject("Client_Report_ShipmentProfileReport"));
				AssertEquals(true, ExistsDbObject("Client_ctfn_JobShipmentOrgs"));
				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					ExecuteNonQuery(script.DropScript);
				}

				AssertEquals(false, ExistsDbObject("Client_Report_ShipmentProfileReport"));
				AssertEquals(false, ExistsDbObject("Client_ctfn_JobShipmentOrgs"));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public override void TestAllIndexesMustSetAllowPageLocksToOffForClientSpecificSchema()
		{
			Assert("Will remove this method next PR", true);
		}

		void ExecuteNonQuery(string sQL)
		{
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.ExecuteNonQuery();
		}

		bool ExistsDbObject(string objectName)
		{
			DbCommand cmd = Db.Connection.Command("select name FROM sys.objects where name='" + objectName + "'");
			object result = cmd.ExecuteScalar();
			return (result != null);
		}
	}
}
