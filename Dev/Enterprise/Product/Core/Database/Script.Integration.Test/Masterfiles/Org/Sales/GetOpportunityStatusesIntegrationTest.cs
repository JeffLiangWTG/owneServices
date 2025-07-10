using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Sales.Testing
{
	class GetOpportunityStatusesIntegrationTest : TransactionedTestCase
	{
		const string registryName = "OPPORTUNITYMANAGEMENTOPPORTUNITYSTATUS";

		public void TestDefaultStatusRegistry()
		{
			using (var command = TestConnection.Command("DELETE FROM " + StmDataSchema.Constants.SqlSchemaName + "." + StmDataSchema.Constants.TableName + " WHERE " + StmDataSchema.Constants.SD_Name + "='" + registryName + "'"))
			{
				command.ExecuteNonQuery();
			}
			var defaultStatuses = GetStatusesRegistryValue();
			AssertCorrectStatuses("Should return default statuses when no registry value", Guid.Empty, defaultStatuses);

			string insertSql = @"INSERT INTO " + StmDataSchema.Constants.SqlSchemaName + "." + StmDataSchema.Constants.TableName + "(" + StmDataSchema.PK.Name + "," + StmDataSchema.SD_Name.Name + "," + StmDataSchema.SD_BinaryValue.Name + ") values (newid(), '" + registryName + "', null)";
			using (var command = TestConnection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}
			AssertCorrectStatuses("Should return default statuses when registry value is null", Guid.Empty, defaultStatuses);
		}

		public void TestOverridenStatusRegistry()
		{
			var companyPk = Guid.NewGuid();
			using (var command = TestConnection.Command("INSERT INTO dbo.GlbCompany (GC_PK, GC_CODE, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@GC_PK, 'XXX', 'AU company', 'AU', 'AUD')"))
			{
				command.AddParameter("@GC_PK", SqlDbType.UniqueIdentifier, companyPk);
				command.ExecuteNonQuery();
			}

			var defaultStatuses = GetStatusesRegistryValue();

			var overridenStatuses = GetStatusesRegistryValue();
			foreach (var status in overridenStatuses)
			{
				status.GetType().GetProperty("Bool", BindingFlags.Public | BindingFlags.Instance).SetValue(status, ZBool.True, null);
				status.GetType().GetProperty("EffectiveAgreement", BindingFlags.Public | BindingFlags.Instance).SetValue(status, ZBool.True, null);
			}

			SetStatusesRegistryValue(overridenStatuses, companyPk);

			using (var command = TestConnection.Command("SELECT COUNT(*) FROM " + StmDataSchema.Constants.SqlSchemaName + "." + StmDataSchema.Constants.TableName + " WHERE " + StmDataSchema.Constants.SD_Name + "='" + registryName + "' AND " + StmDataSchema.Constants.SD_Owner + "=@CompanyPk"))
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, companyPk);
				AssertEquals("Precondition: Should inserted status override for company", 1, command.ExecuteScalar());
			}

			AssertCorrectStatuses("Should return overriden statuses for company", companyPk, overridenStatuses);
			AssertCorrectStatuses("Should return default statuses for no company", Guid.Empty, defaultStatuses);

			SetStatusesRegistryValue(overridenStatuses, Guid.Empty);
			AssertCorrectStatuses("Should return overriden statuses for no company", Guid.Empty, overridenStatuses);
		}

		void AssertCorrectStatuses(string message, Guid companyPk, IEnumerable expectedStatuses)
		{
			IList<OpportunityStatusValues> expectedOpporunityStatusValuesList = new List<OpportunityStatusValues>();
			foreach (var status in expectedStatuses)
			{
				var statusCode = (ZString)status.GetType().GetProperty("Code", BindingFlags.Public | BindingFlags.Instance).GetValue(status, null);
				var closed = (ZBool)status.GetType().GetProperty("Bool", BindingFlags.Public | BindingFlags.Instance).GetValue(status, null);
				var effectiveAgreement = (ZBool)status.GetType().GetProperty("EffectiveAgreement", BindingFlags.Public | BindingFlags.Instance).GetValue(status, null);
				expectedOpporunityStatusValuesList.Add(new OpportunityStatusValues(statusCode, closed, effectiveAgreement));
			}

			IList<OpportunityStatusValues> actualOpporunityStatusValuesList = new List<OpportunityStatusValues>();
			using (var command = TestConnection.Command("SELECT * FROM GetOpportunityStatuses(@CompanyPk)"))
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, companyPk);

				using (var result = command.ExecuteReader())
				{
					while (result.Read())
					{
						actualOpporunityStatusValuesList.Add(new OpportunityStatusValues(new ZString(result["Code"]), new ZBool(result["Closed"]), new ZBool(result["EffectiveAgreement"])));
					}
				}
			}

			AssertContainsExactElementsInAnyOrder(message, expectedOpporunityStatusValuesList, actualOpporunityStatusValuesList);
		}

		#region Implementation

		object GetStatusesRegistry()
		{
			var orgRegistry = Type.GetType("Enterprise.Registry.Business.OrganisationsDataRegistry,Enterprise.Registry.Business", true).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
			return orgRegistry.GetType().GetProperty("OpportunityStatus", BindingFlags.Public | BindingFlags.Instance).GetValue(orgRegistry, null);
		}

		IEnumerable GetStatusesRegistryValue()
		{
			var statusesRegistry = GetStatusesRegistry();
			return (IEnumerable)statusesRegistry.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance).GetValue(statusesRegistry, null);
		}

		void SetStatusesRegistryValue(IEnumerable statuses, Guid companyPk)
		{
			var statusesRegistry = GetStatusesRegistry();
			statusesRegistry.GetType().GetMethod("SetValue", BindingFlags.Public | BindingFlags.Instance).Invoke(statusesRegistry, new object[] { companyPk, Guid.Empty, Guid.Empty, statuses });
		}

		struct OpportunityStatusValues
		{
			public OpportunityStatusValues(ZString code, ZBool closed, ZBool effectiveAgreement)
			{
				Code = code;
				Closed = closed;
				EffectiveAgreement = effectiveAgreement;
			}

			public readonly ZString Code;
			public readonly ZBool Closed;
			public readonly ZBool EffectiveAgreement;
		}
		#endregion
	}
}
