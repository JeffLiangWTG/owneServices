using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.BorderWise
{
	public interface IBorderWiseUsageSet : IGenericUsageSet
	{
		IEnumerable<EDIOrgHeader> GetUsingOrgsWithMissingDatabase(BillingRunContext context);
	}

	public interface IBorderWiseUsageSetProvider
	{
		/// <summary>
		/// Creates STL usage if context.IncludeOdpl is false.
		/// Creates ODPL usage if context.IncludeOdpl is true.
		/// </summary>
		IBorderWiseUsageSet Create(BillingRunContext context, IReadOnlyDictionary<Guid, IBilledDatabase> mainDatabaseMap);
	}

	public class BorderWiseUsageSetProvider : IBorderWiseUsageSetProvider
	{
		public IBorderWiseUsageSet Create(BillingRunContext context, IReadOnlyDictionary<Guid, IBilledDatabase> mainDatabaseMap)
		{
			return new BorderWiseUsageSet(context, mainDatabaseMap);
		}
	}

	/// <summary>
	/// All BorderWise usage for a single billing run
	/// </summary>
	public class BorderWiseUsageSet : IBorderWiseUsageSet
	{
		/// <summary>
		/// Creates STL usage if context.IncludeOdpl is false.
		/// Creates ODPL usage if context.IncludeOdpl is true.
		/// </summary>
		public BorderWiseUsageSet(BillingRunContext context, IReadOnlyDictionary<Guid, IBilledDatabase> mainDatabaseMap = null)
		{
			companyPkToDatabase = new Dictionary<Guid, BilledDatabase>();
			AllUsages = GetUsagesAndLinkedCW1(context, companyPkToDatabase, mainDatabaseMap);
			HasUsageWithDatabase = AllUsages.Any(x => !x.U1_LD.IsEmpty);
		}

		public bool HasUsage => AllUsages.Length > 0;
		public readonly bool HasUsageWithDatabase;

		readonly ClientChargeableUsage[] AllUsages;
		readonly Dictionary<Guid, BilledDatabase> companyPkToDatabase;
		public IEnumerable<ClientChargeableUsage> AllUsagesWithDatabase => AllUsages.Where(x => !x.U1_LD.IsEmpty);

		public string ProductCode => ProductTypes.Codes.BorderWise;

		public string PriceListCode => ProductTypes.Codes.BorderWise;

		public static ClientChargeableUsage[] GetUsagesAndLinkedCW1(
			BillingRunContext context,
			Dictionary<Guid, BilledDatabase> companyPkToDatabase,
			IReadOnlyDictionary<Guid, IBilledDatabase> mainDatabaseMap = null)
		{
			using (var dataSet = new DataSet())
			{
				dataSet.Locale = CultureInfo.InvariantCulture;
				LoadAllChargeableUsage(dataSet, context.PeriodStart, !context.IncludeOdpl, context.OrganisationPK, context.EnterpriseCode);

				var usageRows = dataSet.Tables[0].Rows;
				int numRows = usageRows.Count;
				var usages = new ClientChargeableUsage[numRows];
				for (int i = 0; i < numRows; ++i)
				{
					usages[i] = new ClientChargeableUsage(context.Factory, usageRows[i]);
				}

				var dbRows = dataSet.Tables[1].Rows;
				numRows = dbRows.Count;
				var dbs = new Dictionary<Guid, BilledDatabase>(numRows);
				companyPkToDatabase.Clear();
				for (int i = 0; i < numRows; ++i)
				{
					var row = dbRows[i];
					var licenceCompanyPk = (Guid)row[LicenceCompanySchema.Constants.PK];
					var databasePk = (Guid)(row[LicenceDatabaseSchema.Constants.PK]);
					BilledDatabase db;
					if (!dbs.TryGetValue(databasePk, out db))
					{
						var parentPk = DatabaseUsageSet.AsZGuid(row["ParentPk"]);
						var billable = (string)(row[LicenceDatabaseSchema.Constants.LD_Billable]);
						bool isBillable = billable == DatabaseBillableFlagList.Codes.YesCustomer || billable == DatabaseBillableFlagList.Codes.YesPartner;
						bool isMainDb = true;
						if (!parentPk.IsEmpty)
						{
							if (isBillable && (mainDatabaseMap?.ContainsKey(parentPk.ToGuid()) ?? false))
							{
								isMainDb = false;
							}
							else
							{
								parentPk = ZGuid.Empty;
							}
						}
						db = new BilledDatabase(databasePk
							, (string)(row[LicenceDatabaseSchema.Constants.LD_ServerCode])
							, (int)(row[LicenceDatabaseSchema.Constants.LD_DatabaseNumber])
							, DatabaseUsageSet.AsZGuid(row[LicenceDatabaseSchema.Constants.LD_LE])
							, (bool)row[LicenceDatabaseSchema.Constants.LD_IsBilledPerCompany]
							, "" // hosted location
							, isMainDb
							, parentPk
							, (string)(row["EnterpriseCode"])
							, ZGuid.Empty // priceHeaderPk
							, DatabaseUsageSet.AsZDateTime(row["AgreedLiveDate"])
							, true // isLive
							, (string)(row[LicenceDatabaseSchema.Constants.LD_LicenceType])
							, ProductTypes.Codes.BorderWise
							, billable
							, DatabaseUsageSet.AsZGuid(row[LicenceDatabaseSchema.Constants.LD_OH_WebAccessOrg])
							);

						dbs.Add(databasePk, db);
					}

					companyPkToDatabase.Add(licenceCompanyPk, db);
				}

				foreach (var usage in usages)
				{
					if (companyPkToDatabase.TryGetValue(usage.U1_LC.ToGuid(), out var db))
					{
						usage.U1_LD = db.PK;
					}
				}

				return usages;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public static void LoadAllChargeableUsage(DataSet dataSet, ZDateTime periodStart, bool isStl, ZGuid orgPk, string enterpriseCode)
		{
			using (var cmd = Db.Connection.Command("EdiLoadAllBorderWiseChargeableUsage"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@PeriodStart", SqlDbType.DateTime, periodStart.ToDateTime());
				cmd.AddParameter("@IsStl", SqlDbType.Bit, isStl);
				cmd.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, !orgPk.IsEmpty ? orgPk.ToGuid() : DBNull.Value);
				cmd.AddParameter("@EnterpriseCode", SqlDbType.VarChar, (object)enterpriseCode ?? DBNull.Value);

				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(dataSet);
				}
				dataSet.Tables[0].TableName = ClientChargeableUsageSchema.Constants.TableName;
			}
		}

		public void AppendTo(Dictionary<Guid, IBilledDatabase> usageDatabasePkMap, Dictionary<Guid, IBilledDatabase> mainDatabasePkMap, List<ClientChargeableUsage> chargeableUsages)
		{
			foreach (var borderWiseUsage in AllUsages)
			{
				if (companyPkToDatabase.TryGetValue(borderWiseUsage.U1_LC.ToGuid(), out var db))
				{
					chargeableUsages.Add(borderWiseUsage);
					if (!usageDatabasePkMap.ContainsKey(db.PK.ToGuid()))
					{
						usageDatabasePkMap.Add(db.PK.ToGuid(), db);
					}

					if (db.IsMainDatabase && !mainDatabasePkMap.ContainsKey(db.PK.ToGuid()))
					{
						mainDatabasePkMap.Add(db.PK.ToGuid(), db);
					}
				}
			}
		}

		public IEnumerable<EDIOrgHeader> GetUsingOrgsWithMissingDatabase(BillingRunContext context)
		{
			var companyPks = AllUsages.Where(x => x.U1_LD.IsEmpty)
				.Select(x => x.U1_LC.ToGuid())
				.Distinct()
				.ToList();

			context.CompanyDeliveries.AddOwnerCompanyPks(companyPks);

			return context.Factory.Load<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.PK,
				companyPks.Select(x => context.CompanyDeliveries.GetCompany(x).LC_OH)));
		}
	}
}

