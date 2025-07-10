using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Billing.ODPL
{
	public class OdplRawUsage : SystemRawUsage
	{
		public OdplRawUsage(BillingLoadRawUsageContext context, IDatabaseUsers databaseUsersService = null)
			: base(context)
		{
			this.databaseUsersService = databaseUsersService;
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.ODM; }
		}

		#region IDatabaseUsers

		public IDatabaseUsers DatabaseUsersService
		{
			get { return databaseUsersService ?? (databaseUsersService = new DatabaseUsers(true)); }
			internal set
			{
				if (Globals.IsTest)
				{
					databaseUsersService = value;
				}
			}
		}

		IDatabaseUsers databaseUsersService;

		#endregion

		#region Module Usages

		internal int ModuleCount
		{
			get { return serverModuleUsages.Sum(x => x.ModuleInfoList.Count(y => y.StaffNames != null && y.StaffNames.Count != 0)); }
		}

		internal int StaffCount(ZString moduleCode)
		{
			return serverModuleUsages.Sum(x => x.ModuleInfoList.Where(s => s.Code == moduleCode && s.StaffNames != null).Select(z => z.StaffNames.Count).FirstOrDefault());
		}

		internal bool HasStaff(ZString moduleCode, ZString staffName)
		{
			return serverModuleUsages
				.SelectMany(x => x.ModuleInfoList)
				.Any(x => x.Code == moduleCode && x.StaffNames != null && x.StaffNames.Any(z => z == staffName));
		}

		class ModuleInfo
		{
			internal string Code;
			internal IList<string> StaffNames;
			internal string FeeType;
			internal string Description;
			internal string ParentCode;
			internal string WebParentCode;
			internal bool HasChildModuleUsage;
		}

		class ServerInfo
		{
			internal string ServerCode;
			internal Guid DatabasePk;
			internal List<ModuleInfo> ModuleInfoList = new List<ModuleInfo>();
		}

		readonly List<ServerInfo> serverModuleUsages = new List<ServerInfo>();

		public void AddModuleUsage(Guid databasePk, string serverCode, string moduleCode, string moduleDescription, string feeType, string parentCode, string webParentCode, IList<string> names)
		{
			// NOTE: Can't use the factory in this method since the connection has an active reader
			if (serverModuleUsages.Count == 0 ||
				serverModuleUsages.Last().ServerCode != serverCode)
			{
				var info = new ServerInfo();
				info.ServerCode = serverCode;
				info.DatabasePk = databasePk;
				serverModuleUsages.Add(info);
			}

			var moduleInfoList = serverModuleUsages.Last().ModuleInfoList;

			var moduleInfo = new ModuleInfo();
			moduleInfo.Code = moduleCode;
			moduleInfo.FeeType = feeType;
			moduleInfo.Description = moduleDescription;
			moduleInfo.ParentCode = parentCode;
			moduleInfo.WebParentCode = webParentCode;
			moduleInfo.StaffNames = names;
			moduleInfoList.Add(moduleInfo);
		}

		public void AddModuleUsage(string serverCode, string moduleCode, string moduleDescription, string feeType, IList<string> names)
		{
			AddModuleUsage(Guid.Empty, serverCode, moduleCode, moduleDescription, feeType, "", "", names);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void ApplyFeeTypes()
		{
			foreach (var serverInfo in serverModuleUsages)
			{
				List<string> dbUserNames = null;
				var coreInfo = serverInfo.ModuleInfoList.First();
				if (coreInfo != null && coreInfo.Code != BillingConstants.CoreModuleCode)
				{
					coreInfo = null;
				}

				foreach (var moduleInfo in serverInfo.ModuleInfoList)
				{
					if (string.IsNullOrEmpty(moduleInfo.Description))
					{
						moduleInfo.Description = Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceModuleList.Instance.GetDescription(moduleInfo.Code);
					}

					if (BillingConstants.FeeType.IsPerDatabaseUser(moduleInfo.FeeType) && serverInfo.DatabasePk != Guid.Empty)
					{
						var db = Factory.Load<LicenceDatabase>(serverInfo.DatabasePk);
						if (OdplBillingSystem.IsPerDatabaseUsageOwner(db, context.OrganisationPK))
						{
							if (dbUserNames == null)
							{
								dbUserNames = DatabaseUsersService.DatabaseMonthlyUserList(serverInfo.DatabasePk, PeriodStart).ToList();
							}

							moduleInfo.StaffNames = dbUserNames;
						}
						else
						{
							moduleInfo.StaffNames = null;
						}
					}
					else if (BillingConstants.FeeType.CoreUsers == moduleInfo.FeeType)
					{
						if (coreInfo != null)
						{
							moduleInfo.StaffNames = coreInfo.StaffNames;
						}
						else
						{
							moduleInfo.StaffNames = null;
						}
					}
					else if (BillingConstants.FeeType.Included == moduleInfo.FeeType
						&& !string.IsNullOrEmpty(moduleInfo.WebParentCode)
						&& !string.IsNullOrEmpty(moduleInfo.ParentCode))
					{
						var webParentInfo = serverInfo.ModuleInfoList.FirstOrDefault(x => x.Code == moduleInfo.WebParentCode);
						if (!(webParentInfo == null || webParentInfo.StaffNames == null || webParentInfo.StaffNames.Count == 0))
						{
							// There are base module users for this web tracker usage
							var parentInfo = serverInfo.ModuleInfoList.FirstOrDefault(x => x.Code == moduleInfo.ParentCode);
							if (parentInfo != null)
							{
								parentInfo.HasChildModuleUsage = true;
							}
						}
					}
				}

				// Second loop for web tracker usage
				foreach (var moduleInfo in serverInfo.ModuleInfoList)
				{
					if (BillingConstants.FeeType.Module == moduleInfo.FeeType
						&& !moduleInfo.HasChildModuleUsage)
					{
						if (!string.IsNullOrEmpty(moduleInfo.WebParentCode))
						{
							var webParentInfo = serverInfo.ModuleInfoList.FirstOrDefault(x => x.Code == moduleInfo.WebParentCode);
							if (webParentInfo == null || webParentInfo.StaffNames == null || webParentInfo.StaffNames.Count == 0)
							{
								// There are no module users for this web tracker usage
								moduleInfo.StaffNames = null;
							}
						}
						else
						{
							moduleInfo.StaffNames = null;
						}
					}
					else if (BillingConstants.FeeType.Included == moduleInfo.FeeType)
					{
						moduleInfo.StaffNames = null;
					}
				}

				// Core Usage triggers Self Hosted User charges if this Org is the per-database usage owner
				if (coreInfo != null && serverInfo.DatabasePk != Guid.Empty)
				{
					var db = Factory.Load<LicenceDatabase>(serverInfo.DatabasePk);
					bool isHostedWithCargoWise = EDIDataRegistry.Instance.DatabaseHostedLocations.Value.GetBoolFromCode(db.LD_HostedLocation);
					if (!isHostedWithCargoWise && OdplBillingSystem.IsPerDatabaseUsageOwner(db, context.OrganisationPK))
					{
						var org = Factory.Load<EDIOrgHeader>(context.OrganisationPK);
						var priceHeader = OdplUsage.PriceHeaderForDate(PeriodStart.AddMonths(1).AddDays(-1), org, org.LicCompany.InvoiceDeliveries.FindByServerAndSystem(db.LD_ServerCode, SystemCode));
						if (priceHeader != null)
						{
							foreach (var priceItem in priceHeader.LocalOrStandardItems
								.Where(x => x.L7_FeeType == BillingConstants.FeeType.SelfHostedDatabaseUsers
									&& !x.L7_Code.IsEmpty
									&& x.L7_Price > 0))
							{
								if (dbUserNames == null)
								{
									dbUserNames = DatabaseUsersService.DatabaseMonthlyUserList(serverInfo.DatabasePk, PeriodStart).ToList();
								}

								var info = new ModuleInfo();
								info.Code = priceItem.L7_Code;
								info.Description = priceItem.L7_DescriptionLocalized;
								info.FeeType = priceItem.L7_FeeType;
								info.StaffNames = dbUserNames;
								serverInfo.ModuleInfoList.Add(info);
							}
						}
					}
				}
			}
		}

		#endregion

		#region GetRawUsageSummarySections

		public override SummarySection[] GetRawUsageSummarySections()
		{
			List<SummarySection> result = new List<SummarySection>();

			foreach (var serverUsage in serverModuleUsages)
			{
				var moduleUsages = serverUsage.ModuleInfoList;

				string description = "Production License Usage";
				if (!string.IsNullOrEmpty(serverUsage.ServerCode))
				{
					description += " (" + serverUsage.ServerCode + ")";
				}
				result.Add(CreateUsageSummarySection(moduleUsages, description));
			}

			return result.ToArray();
		}

		SummarySection CreateUsageSummarySection(IEnumerable<ModuleInfo> moduleInfoList, string description)
		{
			SummarySection summarySection = new SummarySection(Factory);
			foreach (var moduleInfo in moduleInfoList.Where(x => !BillingConstants.FeeType.IsPerDatabaseUser(x.FeeType) && x.StaffNames != null))
			{
					bool includeNames = moduleInfo.FeeType != BillingConstants.FeeType.CoreUsers;
					AddModuleLines(summarySection, moduleInfo, includeNames);
			}

			// put database user modules last
			List<ModuleInfo> perDatabaseModules = moduleInfoList.Where(x => BillingConstants.FeeType.IsPerDatabaseUser(x.FeeType) && x.StaffNames != null).ToList();
			for (int i = 0; i < perDatabaseModules.Count; ++i)
			{
				// only include names on the last module
				bool includeNames = i == perDatabaseModules.Count - 1;
				AddModuleLines(summarySection, perDatabaseModules[i], includeNames);
			}

			summarySection.Header.TopLevelDescription = description;
			summarySection.Header.MainDescription = "Module / Staff Name / Country";
			summarySection.Header.AdditionalDescription = "Count";

			return summarySection;
		}

		const string NameIndention = "                ";

		void AddModuleLines(SummarySection summarySection, ModuleInfo moduleInfo, bool includeNames)
		{
			var staffNames = moduleInfo.StaffNames;
			SummaryLine moduleLine = summarySection.Lines.AddNew();
			moduleLine.MainDescription = GetModuleName(moduleInfo);

			int staffCount = staffNames != null ? staffNames.Count : 0;
			if (staffCount != 0)
			{
				moduleLine.AdditionalDescription = staffCount.ToString(CultureInfo.InvariantCulture);
				if (includeNames)
				{
					foreach (string staffName in staffNames)
					{
						SummaryLine staffLine = summarySection.Lines.AddNew();
						staffLine.MainDescription = NameIndention + staffName;
					}
				}
			}
		}

		ZString GetModuleName(ModuleInfo info)
		{
			string name = info.Description;

			if (string.IsNullOrEmpty(name))
			{
				return info.Code;
			}
			else
			{
				return name.Trim() + " (" + info.Code + ")";
			}
		}

		#endregion
	}
}

