using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.ODPL;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface IModuleUsers
	{
		/// <summary>
		/// User count for a module for a given database company and month.
		/// Used by transactional systems, for example eAdaptor needs to
		/// know the number of users of the Warehouse module.
		/// </summary>
		int CompanyMonthlyUserCount(ClientCompany clientCompany, string moduleCode, ZDateTime periodStart);

		/// <summary>
		/// Total licence units consumed by named user modules for a database and month.
		/// Used by some transactional volume discounts where the volume includes both
		/// the transaction licence units and the named-user licence units.
		/// </summary>
		decimal TotalUserLicenceUnits(LicenceDatabase licDatabase, ZDateTime periodStart);
	}

	public class ModuleUsers : IModuleUsers
	{
		readonly OdplSystemBill[] odplBills;
		readonly ZDateTime odplBillPeriodStart;
		readonly ZDateTime generateDate;

		Dictionary<ZDateTime, MonthUsageSet> dateToUsageMap;

		public ModuleUsers(ZDateTime generateDate)
		{
			this.generateDate = generateDate;
		}

		public ModuleUsers(OdplSystemBill[] odplBills, ZDateTime odplMonth, ZDateTime generateDate)
			: this(generateDate)
		{
			this.odplBills = odplBills;
			this.odplBillPeriodStart = new ZDateTime(odplMonth.Year, odplMonth.Month, 1);
		}

		/// <summary>
		/// Usage for a database (for a month)
		/// Note, OdplBillingSystem will load all usage for a database, not just usage of a single licence or client company.
		/// Thus we can assume if any usage is loaded then all usage is loaded.
		/// </summary>
		class DbUsageSet
		{
			readonly List<OdplUsage> odplUsageList = new List<OdplUsage>();

			public decimal LicenceUnits
			{
				get { return licenceUnits ?? (licenceUnits = odplUsageList.Sum(x => x.LicenceUnitsAmount)).Value; }
			}
			decimal? licenceUnits;

			internal void Add(OdplUsage usage)
			{
				odplUsageList.Add(usage);
			}

			public int CompanyMonthlyUserCount(ClientCompany clientCompany, string moduleCode)
			{
				int result = 0;
				OdplUsage usage = odplUsageList.FirstOrDefault(x => x.ClientCo != null && x.ClientCo.PK == clientCompany.PK);
				if (usage != null)
				{
					var module = usage.ModuleUsages.Cast<OdplModuleUsage>().FirstOrDefault(x => x.ModuleCode == moduleCode);
					if (module != null)
					{
						result = Math.Max(module.StaffCount, module.PurchasedStaffCount);
					}
				}

				return result;
			}
		}

		// Usage for a month
		class MonthUsageSet
		{
			readonly Dictionary<ZGuid, DbUsageSet> databasePkToUsage = new Dictionary<ZGuid, DbUsageSet>();

			public void Add(SystemBill[] bills)
			{
				var newDbs = new Dictionary<ZGuid, DbUsageSet>();

				foreach (OdplSystemBill billToAdd in bills)
				{
					foreach (OdplUsage usage in billToAdd.SystemUsages)
					{
						if (usage.LicHeader != null)
						{
							ZGuid databasePk = usage.LicHeader.LA_LD;
							if (!databasePkToUsage.ContainsKey(databasePk))
							{
								DbUsageSet dbUsage;
								if (!newDbs.TryGetValue(databasePk, out dbUsage))
								{
									dbUsage = new DbUsageSet();
									newDbs.Add(databasePk, dbUsage);
								}
								dbUsage.Add(usage);
							}
						}
					}
				}

				foreach (var newDb in newDbs)
				{
					databasePkToUsage.Add(newDb.Key, newDb.Value);
				}
			}

			internal DbUsageSet LoadBills(LicenceHeader ownerLicence, ZDateTime periodStart, ZDateTime generateDate)
			{
				BillingRunContext context = new BillingRunContext(ownerLicence.Factory, generateDate, periodStart.AddMonths(1).AddDays(-1), ownerLicence.Company.LC_OH);
				var odplBilling = new OdplBillingSystem();
				var bills = odplBilling.LoadSystemBills(context);
				Add(bills);
				return GetUsage(ownerLicence.LA_LD)
					?? Create(ownerLicence.LA_LD);
			}

			internal DbUsageSet GetUsage(ZGuid databasePk)
			{
				DbUsageSet dbUsage;
				databasePkToUsage.TryGetValue(databasePk, out dbUsage);
				return dbUsage;
			}

			internal DbUsageSet Create(ZGuid databasePk)
			{
				var dbUsage = new DbUsageSet();
				databasePkToUsage.Add(databasePk, dbUsage);
				return dbUsage;
			}
		}

		MonthUsageSet GetUsage(ZDateTime periodStart)
		{
			MonthUsageSet monthUsage;

			if (dateToUsageMap == null)
			{
				dateToUsageMap = new Dictionary<ZDateTime, MonthUsageSet>();
			}

			if (!dateToUsageMap.TryGetValue(periodStart, out monthUsage))
			{
				monthUsage = new MonthUsageSet();
				dateToUsageMap.Add(periodStart, monthUsage);
				if (odplBillPeriodStart == periodStart && odplBills != null)
				{
					monthUsage.Add(odplBills);
				}
			}

			return monthUsage;
		}

		public int CompanyMonthlyUserCount(ClientCompany clientCompany, string moduleCode, ZDateTime periodStart)
		{
			MonthUsageSet monthUsage = GetUsage(periodStart);
			var databasePk = clientCompany.LCC_LD;
			var dbUsage = monthUsage.GetUsage(databasePk);

			if (dbUsage == null)
			{
				var ownerLicence = clientCompany.UsageOwnerLicence;
				if (ownerLicence != null)
				{
					dbUsage = monthUsage.LoadBills(ownerLicence, periodStart, generateDate);
				}
			}

			if (dbUsage == null)
			{
				dbUsage = monthUsage.Create(clientCompany.LCC_LD);
			}

			return dbUsage.CompanyMonthlyUserCount(clientCompany, moduleCode);
		}

		public decimal TotalUserLicenceUnits(LicenceDatabase licDatabase, ZDateTime periodStart)
		{
			MonthUsageSet monthUsage = GetUsage(periodStart);
			var dbUsage = monthUsage.GetUsage(licDatabase.PK);
			if (dbUsage == null)
			{
				var ownerLicence = licDatabase.UsageOwnerLicence;
				if (ownerLicence != null)
				{
					dbUsage = monthUsage.LoadBills(ownerLicence, periodStart, generateDate);
				}
			}

			if (dbUsage == null)
			{
				dbUsage = monthUsage.Create(licDatabase.PK);
			}

			return dbUsage.LicenceUnits;
		}
	}
}

