using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Sales.Testing
{
	class GetCommissionPeriodListIntegrationTest : TransactionedTestCase
	{
		const string registryName = "COMMISSIONPERIODLIST";

		public void TestDefaultPeriodRegistry()
		{
			using (var command = TestConnection.Command("DELETE FROM " + StmDataSchema.Constants.SqlSchemaName + "." + StmDataSchema.Constants.TableName + " WHERE " + StmDataSchema.Constants.SD_Name + "='" + registryName + "'"))
			{
				command.ExecuteNonQuery();
			}
			var defaultPeriods = GetPeriodsRegistryValue();
			AssertCorrectPeriods("Should return default periods when no registry value", defaultPeriods);

			string insertSql = @"INSERT INTO " + StmDataSchema.Constants.SqlSchemaName + "." + StmDataSchema.Constants.TableName + "(" + StmDataSchema.PK.Name + "," + StmDataSchema.SD_Name.Name + "," + StmDataSchema.SD_BinaryValue.Name + ") values (newid(), '" + registryName + "', null)";
			using (var command = TestConnection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}
			AssertCorrectPeriods("Should return default periods when registry value is null", defaultPeriods);
		}

		public void TestOverridenPeriodRegistry()
		{
			var defaultPeriods = GetPeriodsRegistryValue();

			var overridenPeriods = GetPeriodsRegistryValue();
			foreach (var period in overridenPeriods)
			{
				period.GetType().GetProperty("Start", BindingFlags.Public | BindingFlags.Instance).SetValue(period, new ZInt(6), null);
				period.GetType().GetProperty("End", BindingFlags.Public | BindingFlags.Instance).SetValue(period, new ZInt(12), null);
				period.GetType().GetProperty("IsEnabled", BindingFlags.Public | BindingFlags.Instance).SetValue(period, ZBool.True, null);
			}

			SetPeriodRegistryValue(overridenPeriods);

			using (var command = TestConnection.Command("SELECT COUNT(*) FROM " + StmDataSchema.Constants.SqlSchemaName + "." + StmDataSchema.Constants.TableName + " WHERE " + StmDataSchema.Constants.SD_Name + "='" + registryName + "'"))
			{
				AssertEquals("Precondition: Should inserted period override", 1, command.ExecuteScalar());
			}

			AssertCorrectPeriods("Should return overriden periods", overridenPeriods);
		}

		void AssertCorrectPeriods(string message, IEnumerable expectedPeriods)
		{
			IList<PeriodValues> expectedOpporunityPeriodValuesList = new List<PeriodValues>();
			foreach (var period in expectedPeriods)
			{
				var code = (ZString)period.GetType().GetProperty("Code", BindingFlags.Public | BindingFlags.Instance).GetValue(period, null);
				var start = (ZInt)period.GetType().GetProperty("Start", BindingFlags.Public | BindingFlags.Instance).GetValue(period, null);
				var end = (ZInt)period.GetType().GetProperty("End", BindingFlags.Public | BindingFlags.Instance).GetValue(period, null);
				var isEnabled = (ZBool)period.GetType().GetProperty("IsEnabled", BindingFlags.Public | BindingFlags.Instance).GetValue(period, null);
				expectedOpporunityPeriodValuesList.Add(new PeriodValues(code, start, end, isEnabled));
			}

			IList<PeriodValues> actualOpporunityPeriodValuesList = new List<PeriodValues>();
			using (var command = TestConnection.Command("SELECT * FROM GetCommissionPeriods()"))
			{
				using (var result = command.ExecuteReader())
				{
					while (result.Read())
					{
						actualOpporunityPeriodValuesList.Add(new PeriodValues(new ZString(result["Code"]), new ZInt(result["StartMonth"]), new ZInt(result["EndMonth"]), new ZBool(result["IsEnabled"])));
					}
				}
			}

			AssertContainsExactElementsInAnyOrder(message, expectedOpporunityPeriodValuesList, actualOpporunityPeriodValuesList);
		}

		#region Implementation

		object GetPeriodsRegistry()
		{
			var orgRegistry = Type.GetType("Enterprise.Registry.Business.OrganisationsDataRegistry,Enterprise.Registry.Business", true).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
			return orgRegistry.GetType().GetProperty("CommissionPeriodList", BindingFlags.Public | BindingFlags.Instance).GetValue(orgRegistry, null);
		}

		IEnumerable GetPeriodsRegistryValue()
		{
			var periodsRegistry = GetPeriodsRegistry();
			return (IEnumerable)periodsRegistry.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance).GetValue(periodsRegistry, null);
		}

		void SetPeriodRegistryValue(IEnumerable periods)
		{
			var periodsRegistry = GetPeriodsRegistry();
			periodsRegistry.GetType().GetMethod("SetValue", BindingFlags.Public | BindingFlags.Instance).Invoke(periodsRegistry, new object[] { Guid.Empty, Guid.Empty, Guid.Empty, periods });
		}

		struct PeriodValues
		{
			public PeriodValues(ZString code, ZInt start, ZInt end, ZBool isEnabled)
			{
				Code = code;
				Start = start;
				End = end;
				IsEnabled = isEnabled;
			}

			public readonly ZString Code;
			public readonly ZInt Start;
			public readonly ZInt End;
			public readonly ZBool IsEnabled;
		}
		#endregion
	}
}

