using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Sales.Testing
{
	class GetCommunicationStatusesIntegrationTest : TransactionedTestCase
	{
		const string registryName = "CommunicationStatus";

		public void TestDefaultStatusRegistry()
		{
			using (var command = TestConnection.Command("DELETE FROM " + StmDataSchema.Constants.SqlSchemaName + "." + StmDataSchema.Constants.TableName + " WHERE " + StmDataSchema.Constants.SD_Name + "='" + registryName + "'"))
			{
				command.ExecuteNonQuery();
			}
			var defaultStatuses = GetStatusesRegistryValue();
			AssertCorrectStatuses("Should return default statuses when no registry value", defaultStatuses);

			string insertSql = @"INSERT INTO " + StmDataSchema.Constants.SqlSchemaName + "." + StmDataSchema.Constants.TableName + "(" + StmDataSchema.PK.Name + "," + StmDataSchema.SD_Name.Name + "," + StmDataSchema.SD_BinaryValue.Name + ") values (newid(), '" + registryName + "', null)";
			using (var command = TestConnection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}
			AssertCorrectStatuses("Should return default statuses when registry value is null", defaultStatuses);
		}

		public void TestOverridenStatusRegistry()
		{
			var overridenStatuses = GetStatusesRegistryValue();
			foreach (var status in overridenStatuses)
			{
				status.GetType().GetProperty("Closed", BindingFlags.Public | BindingFlags.Instance).SetValue(status, ZBool.False, null);
			}
			SetStatusesRegistryValue(overridenStatuses);

			using (var command = TestConnection.Command("SELECT COUNT(*) FROM " + StmDataSchema.Constants.SqlSchemaName + "." + StmDataSchema.Constants.TableName + " WHERE " + StmDataSchema.Constants.SD_Name + "='" + registryName + "'"))
			{
				AssertEquals("Precondition: Should inserted status override", 1, command.ExecuteScalar());
			}

			AssertCorrectStatuses("Should return overriden statuses", overridenStatuses);
		}

		void AssertCorrectStatuses(string message, IEnumerable expectedStatuses)
		{
			IList<KeyValuePair<ZString, ZBool>> expectedCodeClosedPairList = new List<KeyValuePair<ZString, ZBool>>();
			foreach (var status in expectedStatuses)
			{
				var statusCode = (ZString)status.GetType().GetProperty("Code", BindingFlags.Public | BindingFlags.Instance).GetValue(status, null);
				var closed = (ZBool)status.GetType().GetProperty("Closed", BindingFlags.Public | BindingFlags.Instance).GetValue(status, null);
				expectedCodeClosedPairList.Add(new KeyValuePair<ZString, ZBool>(statusCode, closed));
			}

			IList<KeyValuePair<ZString, ZBool>> actualCodeClosedPairList = new List<KeyValuePair<ZString, ZBool>>();
			using (var command = TestConnection.Command("SELECT * FROM GetCommunicationStatuses()"))
			{
				using (var result = command.ExecuteReader())
				{
					while (result.Read())
					{
						actualCodeClosedPairList.Add(new KeyValuePair<ZString, ZBool>(new ZString(result["Code"]), new ZBool(result["Closed"])));
					}
				}
			}

			AssertContainsExactElementsInAnyOrder(message, expectedCodeClosedPairList, actualCodeClosedPairList);
		}

		#region Implementation

		object GetStatusesRegistry()
		{
			var orgRegistry = Type.GetType("Enterprise.Registry.Business.OrganisationsDataRegistry,Enterprise.Registry.Business", true).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
			return orgRegistry.GetType().GetProperty("CommunicationStatusList", BindingFlags.Public | BindingFlags.Instance).GetValue(orgRegistry, null);
		}

		IEnumerable GetStatusesRegistryValue()
		{
			var statusesRegistry = GetStatusesRegistry();
			return (IEnumerable)statusesRegistry.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance).GetValue(statusesRegistry, null);
		}

		void SetStatusesRegistryValue(IEnumerable statuses)
		{
			var statusesRegistry = GetStatusesRegistry();
			statusesRegistry.GetType().GetMethod("SetValue", BindingFlags.Public | BindingFlags.Instance).Invoke(statusesRegistry, new object[] { Guid.Empty, Guid.Empty, Guid.Empty, statuses });
		}
		#endregion
	}
}

