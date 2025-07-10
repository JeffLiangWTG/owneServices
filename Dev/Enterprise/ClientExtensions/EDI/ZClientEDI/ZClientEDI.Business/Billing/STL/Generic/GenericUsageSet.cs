using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface IGenericUsageSet
	{
		string ProductCode { get; }
		string PriceListCode { get; }
		bool HasUsage { get; }
		void AppendTo(Dictionary<Guid, IBilledDatabase> usageDatabasePkMap, Dictionary<Guid, IBilledDatabase> mainDatabasePkMap, List<ClientChargeableUsage> chargeableUsages);
	}

	public class GenericUsageSet : IGenericUsageSet
	{
		public GenericUsageSet(string productCode, string usageCategory, string priceListCode, BillingRunContext context, IReadOnlyDictionary<Guid, IBilledDatabase> mainDatabaseMap = null)
		{
			AllUsages = GetUsages(context, productCode, usageCategory);
			ProductCode = productCode;
			UsageCategory = usageCategory;
			PriceListCode = priceListCode;
		}

		public bool HasUsage => AllUsages.Length > 0;
		readonly ClientChargeableUsage[] AllUsages;
		readonly Dictionary<Guid, IBilledDatabase> BilledDatabases = new Dictionary<Guid, IBilledDatabase>();

		public string ProductCode { get; private set; }

		public string UsageCategory { get; private set; }

		public string PriceListCode { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		ClientChargeableUsage[] GetUsages(
			BillingRunContext context,
			string productCode,
			string usageCategory)
		{
			using (var dataSet = new DataSet())
			{
				dataSet.Locale = CultureInfo.InvariantCulture;
				LoadAllChargeableUsage(dataSet, context.PeriodStart, context.OrganisationPK, context.EnterpriseCode, productCode, usageCategory);

				var usages = dataSet.Tables[0].Rows.OfType<DataRow>()
					.Select(r => new ClientChargeableUsage(context.Factory, r))
					.ToArray();

				var databases = dataSet.Tables[1].Rows.OfType<DataRow>()
					.Select(r => new BilledDatabase(
								DatabaseUsageSet.AsZGuid(r[LicenceDatabaseSchema.Constants.PK])
							, (string)r[LicenceDatabaseSchema.Constants.LD_ServerCode]
							, (int)r[LicenceDatabaseSchema.Constants.LD_DatabaseNumber]
							, DatabaseUsageSet.AsZGuid(r[LicenceDatabaseSchema.Constants.LD_LE])
							, (bool)r[LicenceDatabaseSchema.Constants.LD_IsBilledPerCompany]
							, (string)r[LicenceDatabaseSchema.Constants.LD_HostedLocation]
							, true
							, ZGuid.Empty
							, (string)r[LicenceEnterpriseSchema.Constants.LE_EnterpriseCode]
							, ZGuid.Empty
							, DatabaseUsageSet.AsZDateTime(r[LicenceHeaderSchema.Constants.LA_AgreedLiveDate])
							, true
							, (string)r[LicenceDatabaseSchema.Constants.LD_LicenceType]
							, productCode
							, (string)r[LicenceDatabaseSchema.Constants.LD_Billable]
							, DatabaseUsageSet.AsZGuid(r[LicenceDatabaseSchema.Constants.LD_OH_WebAccessOrg])));
				BilledDatabases.Clear();
				BilledDatabases.AddRange(databases.Select(x => new KeyValuePair<Guid, IBilledDatabase>(x.PK.ToGuid(), x)));

				return usages;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void LoadAllChargeableUsage(DataSet dataSet, ZDateTime periodStart, ZGuid orgPk, string enterpriseCode, string productCode, string usageCategory)
		{
			using (var cmd = Db.Connection.Command("EdiLoadAllGenericChargeableUsage"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@PeriodStart", SqlDbType.DateTime, periodStart.ToDateTime());
				cmd.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, !orgPk.IsEmpty ? orgPk.ToGuid() : DBNull.Value);
				cmd.AddParameter("@EnterpriseCode", SqlDbType.VarChar, (object)enterpriseCode ?? DBNull.Value);
				cmd.AddParameter("@ProductCode", SqlDbType.VarChar, productCode);
				cmd.AddParameter("@UsageCategory", SqlDbType.VarChar, usageCategory);

				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(dataSet);
				}
				dataSet.Tables[0].TableName = ClientChargeableUsageSchema.Constants.TableName;
			}
		}

		public void AppendTo(Dictionary<Guid, IBilledDatabase> usageDatabasePkMap, Dictionary<Guid, IBilledDatabase> mainDatabasePkMap, List<ClientChargeableUsage> chargeableUsages)
		{
			chargeableUsages.AddRange(AllUsages);
			usageDatabasePkMap.AddRange(BilledDatabases.Where(x => !usageDatabasePkMap.ContainsKey(x.Key)).ToArray());
			mainDatabasePkMap.AddRange(BilledDatabases.Where(x => !mainDatabasePkMap.ContainsKey(x.Key)).ToArray());
		}
	}
}
