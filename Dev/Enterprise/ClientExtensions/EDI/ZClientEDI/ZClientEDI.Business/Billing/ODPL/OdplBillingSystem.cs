using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.ODPL
{
	public class OdplBillingSystem : BillingSystemWithDatabase
	{
		public OdplBillingSystem()
			: base()
		{
		}

		#region System Code

		public override string SystemCode
		{
			get { return BillingConstants.BillingSystem.ODM; }
		}

		#endregion

		#region Load Raw Usage

		const string query_Raw_Usage =
@"
DECLARE @LD_PK UNIQUEIDENTIFIER;
DECLARE @L6_PK UNIQUEIDENTIFIER;
DECLARE @AvgUserPerCountry INT = 0;
DECLARE @L6_ValidFrom SMALLDATETIME;
DECLARE @DatabaseNumber int;
DECLARE @LD_ServerCode varchar(3);

SELECT @LD_PK = LCC_LD, @DatabaseNumber = LD_DatabaseNumber, @LD_ServerCode = LD_ServerCode FROM dbo.ClientCompany join dbo.LicenceDatabase on LCC_LD = LD_PK WHERE LCC_PK = @ClientCompanyPk;

DECLARE @LA_PK UNIQUEIDENTIFIER
SELECT @LA_PK = vw.LA_PK
FROM dbo.EdiViewClientCompanyLicence vw
WHERE vw.LCC_PK = @ClientCompanyPk;

SELECT TOP 1 @L6_PK = L6_PK, @L6_ValidFrom = L6_ValidFrom
FROM EdiGetOdmPriceHeadersForDate(@FirstDayOfMonth) where LA_PK = @LA_PK

SELECT @AvgUserPerCountry = ISNULL(SUM(CASE WHEN U1_SubCode = 'USR' THEN U1_UnitCount ELSE 0 END) / NULLIF(SUM(CASE WHEN U1_SubCode = 'GPC' THEN U1_UnitCount ELSE 0 END), 0), 0)
FROM dbo.ClientChargeableUsage
WHERE U1_PeriodStart = @FirstDayOfMonth
AND U1_LD = @LD_PK AND ((U1_Code = 'STL' AND U1_SubCode = 'USR') OR (U1_Code = 'ODM' AND U1_SubCode = 'GPC'))
GROUP BY U1_LD;

select L7_Code, L7_ParentCode = case when L7_ParentCode != 'COR' then L7_ParentCode else '' end, L7_UnitBreak, L7_Price, L7_WebParentCode, L7_FeeType, L7_Description, L7_Order
into #PriceItem
from dbo.ClientLicencePriceItem WHERE L7_L6 = @L6_PK AND L7_Code <> ''

create unique clustered index PriceItemIdx on #PriceItem (L7_Code, L7_UnitBreak)

declare @HasCommitment bit
select top 1 @HasCommitment = 1
from
(
	select 
		L9_PK = COALESCE(ExactMatchedDelivery.L9_PK, SystemMatchedDelivery.L9_PK, ServerMatchedDelivery.L9_PK, AllMatchedDelivery.L9_PK),
		L9_OH_InvoiceTo = 
			case 
				when ExactMatchedDelivery.L9_PK is not null then ExactMatchedDelivery.L9_OH_InvoiceTo
				when SystemMatchedDelivery.L9_PK is not null then SystemMatchedDelivery.L9_OH_InvoiceTo
				when ServerMatchedDelivery.L9_PK is not null then ServerMatchedDelivery.L9_OH_InvoiceTo
				when AllMatchedDelivery.L9_PK is not null then AllMatchedDelivery.L9_OH_InvoiceTo
			end,
		OH_PK
	from 
		(
			select LCC_PK, LC_PK, OH_PK = LCC_OH
			from dbo.ClientCompany
				join dbo.LicenceCompany on LCC_OH = LC_OH
			where
				LCC_PK = @ClientCompanyPk
				and LCC_OH is not null

			union

			select LCC_PK, LC_PK, OH_PK = LC_OH
			from dbo.ClientCompany
				join dbo.LicenceDatabase on LCC_LD = LD_PK
				join dbo.EdiViewLicenceDatabaseOwner DbOwner on DbOwner.LD_PK = LicenceDatabase.LD_PK
			where 
				LCC_PK = @ClientCompanyPk
				and LCC_OH is null
		) a
		left join dbo.ClientInvoiceDelivery ExactMatchedDelivery on ExactMatchedDelivery.L9_LC = LC_PK and ExactMatchedDelivery.L9_SystemCode = 'ODM' and ExactMatchedDelivery.L9_ServerCode = @LD_ServerCode
		left join dbo.ClientInvoiceDelivery SystemMatchedDelivery on SystemMatchedDelivery.L9_LC = LC_PK and SystemMatchedDelivery.L9_SystemCode = 'ODM' and SystemMatchedDelivery.L9_ServerCode = ''
		left join dbo.ClientInvoiceDelivery ServerMatchedDelivery on ServerMatchedDelivery.L9_LC = LC_PK and ServerMatchedDelivery.L9_SystemCode = 'ALL' and ServerMatchedDelivery.L9_ServerCode = @LD_ServerCode
		left join dbo.ClientInvoiceDelivery AllMatchedDelivery on AllMatchedDelivery.L9_LC = LC_PK and AllMatchedDelivery.L9_SystemCode = 'ALL' and AllMatchedDelivery.L9_ServerCode = ''
) b
where
	L9_PK is not null
	and 
	COALESCE(L9_OH_InvoiceTo, OH_PK) in
	(
		select LC_OH 
		from
			dbo.LicenceCompany
			join dbo.ClientLicenceBilling on L4_LC = LC_PK
			join dbo.ClientLicenceBillingDiscount on L5_L4 = L4_PK
		where
			L5_SystemCode = 'ODM'
			and L5_Type = 'COM'
			and L5_StartDate < '2016-07-01'
			and (L5_EndDate is null or L5_EndDate >= @DateFrom)
	)
set @HasCommitment = ISNULL(@HasCommitment, 0)

declare @IsCWPriceVersion bit = (select case when L6_PricelistVersion like 'CW%' then 1 else 0 end from dbo.ClientLicencePriceHeader where L6_PK = @L6_PK)

-- logged in user vs registered user pricing
declare @IsRegisteredUserPricing bit = case when @Period >= 201611
	and @HasCommitment = 0
	and ISNULL(@IsCWPriceVersion, 1) = 1
	and (@L6_ValidFrom >= '2016-07-01' or @L6_ValidFrom is null) then 1 else 0 end

declare @HasCOWUsage bit
select top 1 @HasCOWUsage = 1 from dbo.ClientChargeableUsage where U1_PeriodStart = @FirstDayOfMonth and U1_Code = 'ODM' AND U1_SubCode = 'COW' AND U1_LD = @LD_PK
set @HasCOWUsage = ISNULL(@HasCOWUsage, 0)

-- do cross server queries seperately to avoid cross server joins
select TX_Reference1, TX_Reference2
into #UsrUsage
from 
	dbo.BillingViewChargeable
where 
	@Period >= 201611
	and TX_Period = @Period
	and TX_Category = 'STL'
	and TX_PriceItemCode = 'USR'
	and TX_DatabaseNumber = @DatabaseNumber
	and TX_LCC = @ClientCompanyPk

-- put usage into temp table since it is queried more than once
select ModuleCode = ISNULL(PriceCode, LX2_ModuleCode), LS_PK, LS_Code, LS_FullName, PriceCode
into #usage
from dbo.EdiViewBillableUsage
left join
(
	select L7_code, PriceCode = L7_code from #PriceItem child where L7_UnitBreak = 0
	union all
	select child.L7_Code, parent.L7_Code
	from #PriceItem child
	join #PriceItem parent on child.L7_UnitBreak = 0 and child.L7_ParentCode != '' and child.L7_ParentCode = parent.L7_Code and parent.L7_UnitBreak = 0
) childToParent on LX2_ModuleCode = L7_Code
where LX2_Period = @Period
	and LX2_LicenceMode = 'ODM'
	and LCC_PK = @ClientCompanyPk
-- need to group since a parent with usage from multiple children will otherwise cause duplicate records
group by ISNULL(PriceCode, LX2_ModuleCode), LS_PK, LS_Code, LS_FullName, PriceCode

select LCC_PK = LX2_LCC, LX2_FirstUsageUtc, LS_Code
into #LoginUsers
from dbo.EdiLicenceUsage
join dbo.ClientStaff on LX2_LS = LS_PK and LS_LD = @LD_PK
join dbo.ClientCompany on LX2_LCC = LCC_PK and LCC_LD = @LD_PK
where
	LX2_ModuleCode = 'COR'
	and LX2_LicenceMode = 'ODM'
	and LX2_Period = @Period
	and LS_Code != '';

create unique clustered index LoginUsersIdx on #LoginUsers (LS_Code, LX2_FirstUsageUtc, LCC_PK);

;with DbCoreUsage(LS_Code) AS
(
	select distinct LS_Code from #LoginUsers
)
SELECT
	StaffNum,
	LS_FullName,
	LS_Code,
	@LD_ServerCode,
	ModuleCode,
	L7_FeeType = case StaffNum when 1 then ISNULL(L7_FeeType, '') else NULL end,
	L7_Description = case StaffNum when 1 then ISNULL(L7_Description, '') else NULL end,
	L7_ParentCode = case StaffNum when 1 then ISNULL(L7_ParentCode, '') else NULL end,
	L7_WebParentCode = case StaffNum when 1 then ISNULL(L7_WebParentCode, '') else NULL end,
	LD_PK = case StaffNum when 1 then @LD_PK else NULL end
from 
	(
		select ModuleCode, LS_FullName, LS_Code, L7_FeeType, L7_Order, L7_ParentCode, L7_WebParentCode, 
			L7_Description =
				case 
					when @IsRegisteredUserPricing = 1 and ModuleCode = 'COR' then 'Registered User'
					else L7_Description
				end,
			StaffNum = ROW_NUMBER() OVER (PARTITION BY ModuleCode ORDER BY LS_FullName, LS_Code)
		from
			(
				select ModuleCode, LS_FullName, LS_Code, #PriceItem.* from #usage left join #PriceItem on L7_Code = PriceCode and L7_UnitBreak = 0

				union all

				-- Country Usage
				select U1_SubCode, RN_Desc, LCC_RN_NKCountryCode, #PriceItem.*
				from dbo.ClientChargeableUsage
				join dbo.ClientCompany on LCC_PK = U1_LCC
				join dbo.RefCountry on RN_Code = LCC_RN_NKCountryCode
				join #PriceItem on L7_Code = U1_SubCode
				where U1_LCC = @ClientCompanyPk
					and U1_Code = 'ODM'
					and U1_LD = @LD_PK
					and U1_PeriodStart = @FirstDayOfMonth and U1_SubCode in ( @CountryUsagePriceCodes )

				union all
				
				-- copy USR to GPC
				select 'GPC', LS_FullName = TX_Reference2, LS_Code = TX_Reference1, p.*
				from #UsrUsage
				cross apply (SELECT TOP 1 #PriceItem.* FROM #PriceItem where L7_Code = 'GPC' AND @AvgUserPerCountry > L7_UnitBreak ORDER BY L7_UnitBreak DESC) p

				union all

				-- copy USR to COR if they don't already have COR usage (i.e., they didn't login)
				-- and they are on registered user pricing
				select 'COR', LS_FullName = TX_Reference2, LS_Code = TX_Reference1, p.*
				from #UsrUsage
				cross apply (select top 1 * from #PriceItem where L7_Code = 'COR') p
				where @IsRegisteredUserPricing = 1
					and TX_Reference1 not in (select LS_Code from DbCoreUsage)

				union all

				-- copy COR usage to COW usage if they are eligible
				-- only from first company
				select 'COW', LS_FullName, LS_Code, p.*
				from #usage
				cross apply (SELECT TOP 1 * FROM #PriceItem where L7_Code = 'COW') p
				where ModuleCode = 'COR' and @HasCOWUsage = 1
					and LS_Code in
					(
						select LS_Code
						from
						(
							select LCC_PK, LS_Code, LoginSeq = row_number() over (partition by LS_Code order by LX2_FirstUsageUtc, LCC_PK)
							from #LoginUsers
							where @HasCOWUsage = 1
						) orderedLogins
						where LoginSeq = 1 and LCC_PK = @ClientCompanyPk
					)

				union all

				-- copy USR usage to COW usage for user if they don't already have COR usage (i.e., they didn't login)
				select 'COW', LS_FullName = TX_Reference2, LS_Code = TX_Reference1, p.*
				from #UsrUsage
				cross apply (SELECT TOP 1 * FROM #PriceItem where L7_Code = 'COW') p
				where @HasCOWUsage = 1
					and TX_Reference1 not in (select LS_Code from DbCoreUsage)
			) u
		where
			@L6_PK is null or L7_Price > 0 or (L7_WebParentCode != '' and L7_Price = 0)
	) u
where 1=1
ORDER BY case ModuleCode when 'COR' then 0 else 1 end, L7_Order, ModuleCode, StaffNum

drop table #usage;
drop table #UsrUsage;
drop table #PriceItem;
drop table #LoginUsers
";

		internal class RawUsageInfo
		{
			public string ServerCode;
			public string ModuleCode;
			public string FeeType;
			public string Description;
			public string ParentCode;
			public string WebParentCode;
			public List<string> Names = new List<string>();
			public Guid DatabasePk = Guid.Empty;
		}

		public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
		{
			OdplRawUsage odplRaw;
			using (var command = GetRawUsageQuery(context))
			{
				//same thing - Richard Smith/PER, let me know if you want to undo any of this
				command.AddParameter("@FirstDayOfMonth", SqlDbType.DateTime, context.Period.ToDateTime());
				UpdateProgressStatus("Loading data from database...", 20);
				using (var reader = command.ExecuteReader())
				{
					UpdateProgressStatus("Processing data...", 40);
					odplRaw = (OdplRawUsage)LoadOdplRawUsageFromDataReader(reader, context);
				}
			}

			if (odplRaw != null)
			{
				odplRaw.ApplyFeeTypes();
			}
			return odplRaw;
		}

		protected override SystemRawUsage LoadOdplRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			OdplRawUsage odplRaw = new OdplRawUsage(context, context.DatabaseUsersService);
			RawUsageInfo rawInfo = null;

			while (reader.Read())
			{
				// NOTE: Can't access the Factory while the reader is active
				int i = 0;
				int staffNum = Convert.ToInt32(reader[i++], CultureInfo.InvariantCulture);
				string staffName = reader.GetString(i++);
				string staffCode = reader.GetString(i++).Trim();
				if (!string.IsNullOrEmpty(staffCode))
				{
					staffName = staffName + " (" + staffCode + ")";
				}

				if (staffNum == 1) // first row for a module
				{
					AddRawUsage(odplRaw, rawInfo);

					rawInfo = new RawUsageInfo();
					rawInfo.ServerCode = reader.GetString(i++);
					rawInfo.ModuleCode = reader.GetString(i++);
					rawInfo.FeeType = reader.GetString(i++);
					rawInfo.Description = reader.GetString(i++);
					rawInfo.ParentCode = reader.GetString(i++);
					rawInfo.WebParentCode = reader.GetString(i++);
					rawInfo.DatabasePk = reader.GetGuid(i++);
				}

				rawInfo.Names.Add(staffName);
			}

			AddRawUsage(odplRaw, rawInfo);
			return odplRaw;
		}

		void AddRawUsage(OdplRawUsage odplRaw, RawUsageInfo rawInfo)
		{
			if (rawInfo != null)
			{
				odplRaw.AddModuleUsage(rawInfo.DatabasePk, rawInfo.ServerCode, rawInfo.ModuleCode, rawInfo.Description, rawInfo.FeeType, rawInfo.ParentCode, rawInfo.WebParentCode, rawInfo.Names);
			}
		}

		internal static bool IsPerDatabaseUsageOwner(LicenceDatabase db, ZGuid organisationPK)
		{
			if (db.LD_OH_BillingParty == organisationPK)
			{
				return true;
			}
			else if (!db.LD_OH_BillingParty.IsEmpty)
			{
				return false;
			}
			else
			{
				var owner = LicenceHeader.FirstActiveLive(db.ActiveLicHeadersForAllCompanies.Cast<LicenceHeader>().Where(x => IsBilled(x)));
				return owner != null && owner.Company.LC_OH == organisationPK;
			}
		}

		static bool IsBilled(LicenceHeader licHeader)
		{
			ClientInvoiceDelivery invoiceDelivery = licHeader.Company.InvoiceDeliveries.FindByServerAndSystem(licHeader.Database.LD_ServerCode, BillingConstants.BillingSystem.ODM);
			return invoiceDelivery != null && invoiceDelivery.L9_IsBilled;
		}

		//Should not get STL usage for ODPL system
		public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
		{
			var rawUsage = new StlRawUsage(context);
			rawUsage.Summary.Header.Column1 = "Error";
			var line = rawUsage.Summary.Lines.AddNew();
			line.Column1 = "Usage report is not available for STL billing";
			return rawUsage;
		}

		protected override StlRawUsage LoadStlRawUsageFromDataReader(IDataReader reader, BillingLoadRawUsageContext context)
		{
			throw new NotImplementedException();
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
			if (isStlBilling)
			{
				var dataCsvLine = new OCsvLine(new string[] { "Error", "Usage report is not available for STL billing" });
				action(dataCsvLine.ToString());
			}
			else
			{
				var headerColumns = new string[] { "Module / Staff Name / Country", "Count" };
				var headerCsvLine = new OCsvLine(headerColumns);
				action(headerCsvLine.ToString());

				var rawUsage = LoadOdplRawUsage(context);
				foreach (var summarySection in rawUsage.GetRawUsageSummarySections())
				{
					foreach (SummaryLine line in summarySection.Lines)
					{
						string[] dataValues = new string[] { line.MainDescription, line.AdditionalDescription };
						var dataCsvLine = new OCsvLine(dataValues);
						action(dataCsvLine.ToString());
					}
				}
			}
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
			if (!isStlBilling)
			{
				var rawUsage = LoadOdplRawUsage(context);
				foreach (var summarySection in rawUsage.GetRawUsageSummarySections())
				{
					foreach (SummaryLine line in summarySection.Lines)
					{
						writer.WriteCsvUsageReport(context.PeriodStartTimeUtc, context.CompanyCode, "", "", string.Concat(line.MainDescription, ' ', line.AdditionalDescription).Trim(), context.PriceItemCode, context.PriceItemDescription, 1);
					}
				}
			}
		}

		#endregion

		#region Load Usages

		protected override void AddAdditionalFilter(ZDBOnlyQuery chargeableUsageQuery)
		{
			// WiseCloud user fee is calculated under U1_Code=ODM, but billed separately since it needs it own charge code.
			chargeableUsageQuery.AddToFilter(ClientChargeableUsageSchema.U1_SubCode, SQLComparisonOperator.NotEqual, BillingConstants.Hosting.WiseCloudUserFeeCode);

			base.AddAdditionalFilter(chargeableUsageQuery);
		}

		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			List<OdplUsage> result = new List<OdplUsage>();
			var dbUsage = new DatabaseUsage(Context);
			var stlUsageCodes = new HashSet<string>(EDIDataRegistry.Instance.ValidStlUsageCodesOnOdplPricelists.Value.OfType<ICodeDescription>()
				.Select(x => x.Code).Where(x => !string.IsNullOrWhiteSpace(x)));

			foreach (var databaseGroup in chargeableUsages.Where(s => s.Database != null).GroupBy(s => s.Database))
			{
				LicenceDatabase db = databaseGroup.Key;
				dbUsage.Build(db, databaseGroup.Where(x => x.U1_Code == SystemCode || (x.U1_Code == BillingConstants.BillingSystem.STL && stlUsageCodes.Contains(x.U1_SubCode))),
					databaseGroup.Where(x => x.U1_Code == BillingConstants.BillingSystem.STL && x.U1_SubCode == Business.DatabaseUsage.ActiveUsersUsageCode));
				var odplUsages = dbUsage.OdplUsageList.Where(x => x.HasOnDemandUsage);
				if (!odplUsages.Any())
				{
					odplUsages = dbUsage.OdplUsageList;
				}
				result.AddRange(odplUsages);
			}

			if (!Context.IsPreviewOnly)
			{
				AddCommitmentUsage(result, chargeableUsages.Where(x => x.U1_Code == SystemCode).ToArray());
			}

			return result.ToArray();
		}

		public class DatabaseUsage
		{
			public DatabaseUsage(BillingRunContext context)
			{
				this.context = context;
				this.periodStart = context.PeriodStart;
			}

			public IEnumerable<OdplUsage> OdplUsageList
			{
				get { return odplUsagePerDb; }
			}

			IEnumerable<LicenceUsage> LicenceUsageList
			{
				get { return licUsageList; }
			}

			readonly BillingRunContext context;
			readonly ZDateTime periodStart;
			LicenceDatabase db;
			readonly Dictionary<ClientCompany, LicenceUsage> clientCompanyToUsageMap = new Dictionary<ClientCompany, LicenceUsage>();
			readonly List<OdplUsage> odplUsagePerDb = new List<OdplUsage>();
			readonly List<ModuleUsageAndSeats> usageBundle = new List<ModuleUsageAndSeats>();
			readonly List<LicenceUsage> licUsageList = new List<LicenceUsage>();
			readonly List<ModuleUsageAndSeats> orgBundle = new List<ModuleUsageAndSeats>();

			// Fast lookup of the parent price item given a price header PK and a module code.
			readonly Dictionary<ZGuid, Dictionary<string, ClientLicencePriceItem>> priceMaps = new Dictionary<ZGuid, Dictionary<string, ClientLicencePriceItem>>();

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
			public void Build(LicenceDatabase licenceDatabase, IEnumerable<ClientChargeableUsage> usages, IEnumerable<ClientChargeableUsage> usrUsages)
			{
				this.db = licenceDatabase;

				clientCompanyToUsageMap.Clear();
				licUsageList.Clear();

				if (usages.Any())
				{
					BuildLicenceUsageAndSeats(usages);
				}
				else
				{
					// Create an empty usage for a minimum fee
					var usageOwnerLicence = db.UsageOwnerOrFirstLicence;
					CreateCompanyUsage(usageOwnerLicence, null);
				}

				BuildPriceMaps(LicenceUsageList.Select(x => x.OdplUsage), priceMaps, mapIncludedToParent: true);
				SetMainPriceItem();
				AddMissingSelfHostedDatabaseUsersModules();

				odplUsagePerDb.Clear();
				odplUsagePerDb.AddRange(LicenceUsageList.Select(x => x.OdplUsage));

				var countryUsers = new DatabaseCountryUsers(clientCompanyToUsageMap.Keys.ToArray(), usrUsages);
				var databaseCountryLanguageBillingHelper = new DatabaseCountryLanguageBillingHelper();

				// Process the usage by main price item to simplify per-database calculations.
				// Do modules with dependencies after the modules they depend on so the information they require is already available.
				// A parent and its child modules define a bundle with a single price. A module with no children is the simplest bundle.
				foreach (var priceBundle in LicenceUsageList.SelectMany(x => x.ModuleUsages.Values)
					.GroupBy(x => new MainCodeFeeTypePair(x))
					.OrderBy(x => x.Key.ProcessingOrder)
					.ThenBy(x => x.Key.MainModuleCode))
				{
					usageBundle.Clear();
					usageBundle.AddRange(priceBundle);
					var mainModuleCode = priceBundle.Key.MainModuleCode;
					var mainFeeType = priceBundle.Key.MainFeeType;

					// modules with a per database fee basis are billed to a single owning licence.
					LicenceUsage usageOwner = null;
					bool isPerDatabase = BillingConstants.FeeType.IsPerDatabase(mainFeeType);
					int dbPurchasedSeats = 0;
					if (isPerDatabase)
					{
						// for per-database fees, look in all licences for purchased seats
						foreach (string moduleCode in usageBundle.Select(x => x.ModuleCode).Distinct())
						{
							foreach (var lic in LicenceUsageList)
							{
								ModuleUsageAndSeats seats;
								if (lic.ModuleUsages.TryGetValue(moduleCode, out seats))
								{
									dbPurchasedSeats = Math.Max(dbPurchasedSeats, seats.SeatCount);
								}
							}
						}

						usageOwner = FindPerDatabaseFeeOwner();
					}

					var commitmentOrgs = GetOrgsWithCommitmentDiscount();
					foreach (var orgGroup in usageBundle.GroupBy(x => usageOwner ?? x.LicenceUsage))
					{
						LicenceUsage licUsage = orgGroup.Key;
						OdplUsage odplUsage = licUsage.OdplUsage;
						orgBundle.Clear();
						orgBundle.AddRange(orgGroup);
						var priceHeader = odplUsage.PriceHeader;
						ClientLicencePriceItem priceItem = null;
						if (priceHeader != null)
						{
							var priceItemCode = mainModuleCode == BillingConstants.RegisteredUserModuleCode ? BillingConstants.CoreModuleCode : mainModuleCode;
							priceMaps[priceHeader.LocalOrStandardItems.Master.PK].TryGetValue(priceItemCode, out priceItem);
						}

						bool isPerCoreUser = mainFeeType == BillingConstants.FeeType.CoreUsers;
						int purchasedStaffCount = 0;
						if (isPerDatabase)
						{
							purchasedStaffCount = dbPurchasedSeats;
						}
						else if (isPerCoreUser)
						{
							var coreUsage = odplUsage.CoreUsage;
							if (coreUsage != null)
							{
								purchasedStaffCount = coreUsage.PurchasedStaffCount;
							}
						}
						else
						{
							purchasedStaffCount = orgBundle.Max(x => x.SeatCount);
						}

						if (context.IsPreviewOnly)
						{
							purchasedStaffCount = 0;
						}

						int staffCount = CalculateStaffCount();
						bool billCoreWithRegisteredUsersCount =
							(periodStart >= context.PeriodStartBillingRegisteredUser
							&& (priceHeader?.L6_PricelistVersion ?? ZString.Empty).StartsWith("CW", StringComparison.OrdinalIgnoreCase)
							&& (context.IsPreviewOnly || (priceHeader?.L6_ValidFrom ?? ZDateTime.MinSmallDateTimeValue) >= context.EarliestPriceCommenceDateCanBillRegisteredUser)
							&& !commitmentOrgs.Contains(odplUsage.InvoicedOrganisationPK));

						if (databaseCountryLanguageBillingHelper.ShouldBillPriceItem(priceItem, countryUsers))
						{
							AddModuleUsage(odplUsage, mainModuleCode, staffCount, purchasedStaffCount, priceItem, billCoreWithRegisteredUsersCount, countryUsers);
						}

						odplUsage.ChargeableUsagePKs.AddRange(orgGroup.Where(x => x.ChargeableUsage != null).Select(x => x.ChargeableUsage.PK));
					}
				}

				AddDomesticDiscount();
				AddPurchasedLicenceUnits();
			}

			ILookup<ZGuid, ZGuid> GetOrgsWithCommitmentDiscount()
			{
				var companyQuery = new ZDBOnlyQuery(typeof(LicenceCompany));
				var billingSubQuery = new ZDBOnlySubQuery(typeof(ClientLicenceBilling), ClientLicenceBillingSchema.L4_LC);
				var discountSubQuery = new ZDBOnlySubQuery(typeof(ClientLicenceBillingDiscount), ClientLicenceBillingDiscountSchema.L5_L4);
				discountSubQuery.AddToFilter(ClientLicenceBillingDiscountSchema.L5_SystemCode, BillingConstants.BillingSystem.ODM);
				discountSubQuery.AddToFilter(ClientLicenceBillingDiscountSchema.L5_Type, BillingConstants.DiscountType.Commitment);
				discountSubQuery.AddToFilter(ClientLicenceBillingDiscountSchema.L5_StartDate, SQLComparisonOperator.LessThan, context.EarliestPriceCommenceDateCanBillRegisteredUser);
				var discountEndDateFilter = new ZQuery(ClientLicenceBillingDiscountSchema.L5_EndDate, ZDateTime.Empty);
				discountEndDateFilter.AddToFilter(JoinCondition.Or, ClientLicenceBillingDiscountSchema.L5_EndDate, SQLComparisonOperator.GreaterThan, context.PeriodStart);
				discountSubQuery.AddToFilter(discountEndDateFilter);
				billingSubQuery.AddSubQuery(discountSubQuery, JoinCondition.And);
				companyQuery.AddSubQuery(billingSubQuery, JoinCondition.And);
				return context.Factory.Load<LicenceCompany>(companyQuery).Select(x => x.LC_OH).ToLookup(x => x);
			}

			LicenceUsage FindPerDatabaseFeeOwner()
			{
				LicenceUsage result;
				LicenceHeader usageOwnerLicence = db.UsageOwnerLicence;
				if (usageOwnerLicence != null)
				{
					var clientCo = usageOwnerLicence.ClientCompany;
					if (clientCo != null)
					{
						result = clientCompanyToUsageMap[clientCo];
					}
					else
					{
						result = LicenceUsageList
							.Where(x => x.OdplUsage.LicHeader == usageOwnerLicence)
							.OrderBy(x => x.OdplUsage.ClientCo == null ? 0 : 1)
							.First();
					}
				}
				else
				{
					// pick the using company that first went live
					result = usageBundle
						.Select(u => u.LicenceUsage)
						.Distinct()
						.OrderBy(u => u.OdplUsage.IsBilled ? 0 : 1)
						.ThenBy(u => u.SiteLive)
						.ThenBy(u => u.OdplUsage.LicCompany != null ? (string)u.OdplUsage.LicCompany.LC_CompanyCode : "")
						.FirstOrDefault();
				}

				return result;
			}

			LicenceUsage CreateCompanyUsage(LicenceHeader owner, ClientCompany clientCo)
			{
				OdplUsage odplUsage = new OdplUsage(context.Factory, owner, periodStart, clientCo);
				if (context.IsPreviewOnly)
				{
					odplUsage.SetPreviewOnly(context.PreviewPriceHeader);
				}
				LicenceUsage licUsage = new LicenceUsage();
				licUsage.OdplUsage = odplUsage;
				if (clientCo != null)
				{
					clientCompanyToUsageMap.Add(clientCo, licUsage);
				}
				licUsageList.Add(licUsage);
				return licUsage;
			}

			LicenceUsage FindCompanyUsage(ClientCompany clientCompany)
			{
				LicenceUsage result;
				clientCompanyToUsageMap.TryGetValue(clientCompany, out result);
				return result;
			}

			void SetMainPriceItem()
			{
				foreach (var licUsage in LicenceUsageList)
				{
					var priceHeader = licUsage.OdplUsage.PriceHeader;
					foreach (var moduleUsage in licUsage.ModuleUsages.Values)
					{
						if (priceHeader != null)
						{
							ClientLicencePriceItem mainPriceItem;
							priceMaps[priceHeader.LocalOrStandardItems.Master.PK].TryGetValue(moduleUsage.ModuleCode, out mainPriceItem);
							if (mainPriceItem != null)
							{
								moduleUsage.MainFeeType = mainPriceItem.L7_FeeType;
								moduleUsage.MainModuleCode = mainPriceItem.L7_Code;
							}
						}

						if (moduleUsage.MainModuleCode == null)
						{
							moduleUsage.MainModuleCode = moduleUsage.ModuleCode;
						}
					}
				}
			}

			void AddMissingSelfHostedDatabaseUsersModules()
			{
				if (!EDIDataRegistry.Instance.DatabaseHostedLocations.Value.GetBoolFromCode(db.LD_HostedLocation))
				{
					foreach (var licUsage in LicenceUsageList)
					{
						var priceHeader = licUsage.OdplUsage.PriceHeader;
						if (priceHeader != null)
						{
							foreach (var priceItem in priceHeader.LocalOrStandardItems.Cast<ClientLicencePriceItem>().Where(
								x => x.L7_FeeType == BillingConstants.FeeType.SelfHostedDatabaseUsers
								&& !x.L7_Code.IsEmpty
								&& !licUsage.ModuleUsages.ContainsKey(x.L7_Code)))
							{
								var moduleSeats = new ModuleUsageAndSeats(licUsage);
								licUsage.ModuleUsages.Add(priceItem.L7_Code, moduleSeats);
								moduleSeats.MainFeeType = BillingConstants.FeeType.SelfHostedDatabaseUsers;
								moduleSeats.MainModuleCode = priceItem.L7_Code;
							}
						}
					}
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
			void BuildLicenceUsageAndSeats(IEnumerable<ClientChargeableUsage> databaseGroup)
			{
				foreach (var orgGroup in databaseGroup.GroupBy(s => s.ClientCompany))
				{
					var clientCompany = orgGroup.Key;
					var ownerLicence = clientCompany.UsageOwnerLicence;
					if (ownerLicence != null)
					{
						var licUsage = CreateCompanyUsage(ownerLicence, clientCompany);
						foreach (var chargeableUsage in orgGroup)
						{
							licUsage.AddModuleUsage(chargeableUsage);
						}
					}
				}

				// Ensure the database usage owner licence is included for recording per-database charges
				var usageOwnerLicence = db.UsageOwnerLicence;
				if (usageOwnerLicence != null)
				{
					var ownerClientCompany = usageOwnerLicence.ClientCompany;

					if (ownerClientCompany == null || FindCompanyUsage(ownerClientCompany) == null)
					{
						CreateCompanyUsage(usageOwnerLicence, ownerClientCompany);
					}
				}

				// Add purchased seats since they count towards volume discounts
				// Purchased seats only apply to ediEnterprise where we create the companies for the customer.
				// CargoWise One does not support purchased seats.
				foreach (LicenceHeader licHeader in db.LicHeadersForAllCompanies.Cast<LicenceHeader>())
				{
					var clientCompany = licHeader.ClientCompany;

					if (clientCompany != null)
					{
						LicenceUsage licUsage = FindCompanyUsage(clientCompany);
						bool shouldAddPurchasedSeats = licUsage != null;
						if (!shouldAddPurchasedSeats)
						{
							if (licHeader.LA_IsActive &&
								!licHeader.LA_ContractExpiryDate.IsEmpty &&
								licHeader.LA_ContractExpiryDate.AddMonths(4) >= periodStart &&
								licHeader.Company.Header.OH_IsActive)
							{
								var invoiceDelivery = licHeader.Company.InvoiceDeliveries.FindByServerAndSystem(licHeader.Database.LD_ServerCode, BillingConstants.BillingSystem.Maintenance);
								if ((invoiceDelivery == null || invoiceDelivery.L9_IsBilled))
								{
									shouldAddPurchasedSeats = true;
								}
							}
						}

						if (shouldAddPurchasedSeats)
						{
							foreach (var licModule in licHeader.PurchasedModules.Cast<LicenceModules>().Where(x => OdplUsage.HasPurchasedSeats(x, periodStart)))
							{
								if (licUsage == null)
								{
									licUsage = CreateCompanyUsage(licHeader, clientCompany);
								}
								ModuleUsageAndSeats moduleSeats;
								if (!licUsage.ModuleUsages.TryGetValue(licModule.LM_GroupModuleCode, out moduleSeats))
								{
									moduleSeats = new ModuleUsageAndSeats(licUsage);
									licUsage.ModuleUsages.Add(licModule.LM_GroupModuleCode, moduleSeats);
								}
								moduleSeats.Seats = licModule;
							}
						}
					}
				}
			}

			int CalculateStaffCount()
			{
				string mainFeeType = orgBundle[0].MainFeeType;
				OdplUsage odplUsage = orgBundle[0].LicenceUsage.OdplUsage;
				int staffCount = 0;
				int maxUnitCount = orgBundle.Max(x => x.UnitCount);
				if (mainFeeType == BillingConstants.FeeType.CoreUsers)
				{
					if (maxUnitCount > 0)
					{
						OdplModuleUsage coreUsage = odplUsage.CoreUsage;
						staffCount = coreUsage != null ? (int)coreUsage.StaffCount : 0;
					}
				}
				else if (BillingConstants.FeeType.IsPerDatabaseUser(mainFeeType))
				{
					// must be a self-hosted system for FeeType.SelfHostedDatabaseUsers
					if (mainFeeType != BillingConstants.FeeType.SelfHostedDatabaseUsers
							|| !EDIDataRegistry.Instance.DatabaseHostedLocations.Value.GetBoolFromCode(odplUsage.ClientCo.Database.LD_HostedLocation))
					{
						var dbUsers = context.DatabaseUsersService;
						staffCount = dbUsers.DatabaseMonthlyUserCount(odplUsage.LicHeader.LA_LD, odplUsage.PeriodStart);
					}
				}
				else if (mainFeeType == BillingConstants.FeeType.Module
					|| mainFeeType == BillingConstants.FeeType.OldWebModule)
				{
					staffCount = 0;
					foreach (var usage in orgBundle)
					{
						int usageStaffCount = 0;
						var priceItem = odplUsage.PriceHeader.LocalOrStandardItems.FindByCode(usage.ModuleCode);
						if (priceItem != null && !priceItem.L7_WebParentCode.IsEmpty)
						{
							OdplModuleUsage webParentUsage = odplUsage.ModuleUsages.Cast<OdplModuleUsage>().FirstOrDefault(x => x.ModuleCode == priceItem.L7_WebParentCode);
							if (webParentUsage != null &&
								(
									(priceItem.L7_FeeType == BillingConstants.FeeType.OldWebModule && odplUsage.HasWebUsage)
									||
									(priceItem.L7_FeeType != BillingConstants.FeeType.OldWebModule && usage.UnitCount != 0)
								))
							{
								usageStaffCount = webParentUsage.StaffCount;
							}
							else
							{
								// web parent had no usage so child has no usage
							}
						}

						staffCount = Math.Max(staffCount, usageStaffCount);
					}
				}
				else
				{
					staffCount = maxUnitCount;
				}

				return staffCount;
			}

			#region core discounts

			void AddDomesticDiscount()
			{
				var odplUsagesForDiscount = odplUsagePerDb.Where(x => x.CoreUsage != null).ToList();

				if (odplUsagesForDiscount.Count == 0)
				{
					return;
				}

				ZString firstCountryCode = odplUsagesForDiscount[0].CountryCode;
				var countryGroups = EDIDataRegistry.Instance.BillingCountryGroups.Value.GetActiveCodeDescriptionPairList();
				var countryGroup = countryGroups.GetDescriptionFromCode(firstCountryCode);
				int entityCount = odplUsagesForDiscount.Count;
				bool multipleCountries = odplUsagesForDiscount.Any(s => string.IsNullOrEmpty(countryGroup)
					? s.CountryCode != firstCountryCode
					: countryGroups.GetDescriptionFromCode(s.CountryCode) != countryGroup);

				if (!multipleCountries)
				{
					foreach (OdplUsage odplUsage in odplUsagesForDiscount)
					{
						OdplModuleUsage coreUsage = odplUsage.CoreUsage;
						int totalCoreUsers = coreUsage != null ? (int)coreUsage.MixedUnitCount : 0;
						ZString[] discountModules = DomesticDiscountCodes(firstCountryCode, totalCoreUsers, entityCount);
						if (discountModules.Length > 0)
						{
							var priceHeader = odplUsage.PriceHeader;
							var priceItemMap = priceHeader != null
								? priceMaps[priceHeader.LocalOrStandardItems.Master.PK]
								: null;

							int coreOnDemandUsers = coreUsage != null ? (int)coreUsage.UnitCount : 0;

							if (coreOnDemandUsers > 0)
							{
								foreach (ZString discount in discountModules)
								{
									ClientLicencePriceItem priceItem = null;
									if (priceItemMap != null)
									{
										priceItemMap.TryGetValue(discount, out priceItem);
									}
									var moduleUsage = odplUsage.AddModuleUsage(discount, coreOnDemandUsers, priceItem);
									moduleUsage.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage = coreUsage?.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage ?? false;
								}
							}
						}
					}
				}
			}

			public static ZString[] DomesticDiscountCodes(ZString countryCode, int users, int domesticEntities)
			{
				List<ZString> discounts = new List<ZString>();

				if (domesticEntities > 1)
				{
					discounts.Add(BillingConstants.CoreDiscount.MultiEntityDomestic);
				}
				else if (users < 20)
				{
					discounts.Add(BillingConstants.CoreDiscount.SingleEntityDomesticUnder20Users);

					// check for developing world countries for additional discounts
					switch (countryCode)
					{
						case Enterprise.Core.Constants.CountryCodes.China:
							discounts.Add(BillingConstants.CoreDiscount.ChinaDomesticUnder20Users);
							break;
						case Enterprise.Core.Constants.CountryCodes.India:
							discounts.Add(BillingConstants.CoreDiscount.IndiaDomesticUnder20Users);
							break;
						// TODO: add check for tier 3 countries once we have a definition
						default:
							//discounts.Add(CoreDiscounts.Tier3DomesticUnder20Users);
							break;
					}
				}
				else
				{
					discounts.Add(BillingConstants.CoreDiscount.SingleEntityDomestic20PlusUsers);
				}

				return discounts.ToArray();
			}

			#endregion

			void AddPurchasedLicenceUnits()
			{
				if (db.LD_PurchasedLicenceUnits != 0)
				{
					var odplUsage = odplUsagePerDb.Where(x => x.PriceHeader != null).OrderBy(x => x.OrganisationPK == x.InvoicedOrganisationPK ? 1 : 0).FirstOrDefault() ?? odplUsagePerDb.FirstOrDefault();

					if (odplUsage != null)
					{
						odplUsage.PurchasedLicenceUnits = db.LD_PurchasedLicenceUnits;
						if (odplUsage.PriceHeader != null)
						{
							odplUsage.LicenceUnitRate = odplUsage.PriceHeader.L6_LicenceUnitRate;
						}
					}
				}
			}
		}

		class MainCodeFeeTypePair : IEquatable<MainCodeFeeTypePair>, IComparable<MainCodeFeeTypePair>
		{
			public MainCodeFeeTypePair(ModuleUsageAndSeats moduleUsage)
			{
				MainModuleCode = moduleUsage.MainModuleCode;
				MainFeeType = moduleUsage.MainFeeType ?? string.Empty;
			}

			public readonly string MainFeeType;
			public readonly string MainModuleCode;

			/// <summary>
			/// Dependency order (low to high):
			/// 1. Core
			/// 2. Registered User
			/// 3. Fee other than Module/OldWebModule
			/// 4. Fee of Module/OldWebModule
			/// </summary>
			public int ProcessingOrder
			{
				get
				{
					if (MainModuleCode == BillingConstants.CoreModuleCode)
					{
						return 1;
					}
					else if (MainModuleCode == BillingConstants.RegisteredUserModuleCode)
					{
						return 2;
					}
					else if (MainFeeType != BillingConstants.FeeType.Module
						&& MainFeeType != BillingConstants.FeeType.OldWebModule)
					{
						return 3;
					}
					else
					{
						return 4;
					}
				}
			}

			public bool Equals(MainCodeFeeTypePair other)
			{
				return CompareTo(other) == 0;
			}

			public int CompareTo(MainCodeFeeTypePair other)
			{
				int result = string.Compare(MainModuleCode, other.MainModuleCode, StringComparison.OrdinalIgnoreCase);
				if (result == 0)
				{
					result = string.Compare(MainFeeType, other.MainFeeType, StringComparison.OrdinalIgnoreCase);
				}
				return result;
			}

			public override int GetHashCode()
			{
				return MainModuleCode.GetHashCode() ^ MainFeeType.GetHashCode();
			}
		}

		class ModuleUsageAndSeats
		{
			public ModuleUsageAndSeats(LicenceUsage licenceUsage)
			{
				this.LicenceUsage = licenceUsage;
			}

			public readonly LicenceUsage LicenceUsage;
			public ClientChargeableUsage ChargeableUsage;
			public LicenceModules Seats;
			public string MainModuleCode;
			public string MainFeeType;

			public string ModuleCode
			{
				get
				{
					if (ChargeableUsage != null) { return ChargeableUsage.U1_SubCode; }
					else if (Seats != null) { return Seats.LM_GroupModuleCode; }
					else { return MainModuleCode; }
				}
			}

			public int UnitCount => ChargeableUsage?.U1_UnitCountAsInt ?? ZInt.Zero;

			public int SeatCount
			{
				get { return Seats != null ? Seats.LM_UserCount : 0; }
			}
		}

		class LicenceUsage
		{
			public OdplUsage OdplUsage;
			public Dictionary<string, ModuleUsageAndSeats> ModuleUsages = new Dictionary<string, ModuleUsageAndSeats>();

			public ModuleUsageAndSeats AddModuleUsage(ClientChargeableUsage chargeableUsage)
			{
				var moduleUsage = new ModuleUsageAndSeats(this) { ChargeableUsage = chargeableUsage };
				ModuleUsages.Add(chargeableUsage.U1_SubCode, moduleUsage);
				return moduleUsage;
			}

			public ZDateTime SiteLive
			{
				get
				{
					if (siteLive.IsEmpty)
					{
						if (OdplUsage.ClientCo != null)
						{
							siteLive = OdplUsage.ClientCo.LCC_CreateTimeUtc;
						}

						if (OdplUsage.LicHeader != null)
						{
							var live = OdplUsage.LicHeader.LA_AgreedLiveDate;
							if (!live.IsEmpty && (siteLive.IsEmpty || siteLive > live))
							{
								siteLive = live;
							}
						}

						if (siteLive.IsEmpty)
						{
							siteLive = ZDateTime.MaxSmallDateTime;
						}
					}

					return siteLive;
				}
			}

			ZDateTime siteLive;
		}

		static void AddModuleUsage(OdplUsage odplUsage, ZString moduleCode, int staffCount, int purchasedStaffCount, ClientLicencePriceItem priceItem, bool billCoreWithRegisteredUsersCount, DatabaseCountryUsers countryUsers)
		{
			if (priceItem != null && !priceItem.IsOnDemandFeeType)
			{
				return;
			}

			var moduleUsages = odplUsage.ModuleUsages;

			if (priceItem != null && priceItem.L7_UnitBreak > 0)
			{
				if (priceItem.L7_FeeType == BillingConstants.FeeType.UsersPerCountryVolumeBreak && countryUsers.DomesticCountryUserGroup.MainCountryCount != 0)
				{
					var averageUsersPerCountry = countryUsers.DomesticCountryUserGroup.AverageUsersPerCountry;

					var firstMatchedItem = odplUsage.PriceHeader.LocalOrStandardItems.FindAllByCode(moduleCode)
						.Where(x => averageUsersPerCountry > x.L7_UnitBreak)
						.OrderByDescending(x => x.L7_UnitBreak).FirstOrDefault();

					if (firstMatchedItem != null)
					{
						var moduleUsage = moduleUsages.AddNew();
						moduleUsage.ModuleCode = moduleCode;
						moduleUsage.StaffCount = staffCount;
						moduleUsage.PurchasedStaffCount = purchasedStaffCount;
						moduleUsage.PopulateFromPriceItem(firstMatchedItem);
						moduleUsage.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage = billCoreWithRegisteredUsersCount;
					}
				}
				else
				{
					int mixedCount = Math.Max(staffCount, purchasedStaffCount);
					// Highest break first...
					foreach (var item in odplUsage.PriceHeader.LocalOrStandardItems.FindAllByCode(moduleCode).OrderByDescending(item => item.L7_UnitBreak))
					{
						if (mixedCount > item.L7_UnitBreak)
						{
							var moduleUsage = moduleUsages.AddNew();
							moduleUsage.ModuleCode = moduleCode;
							moduleUsage.StaffCount = Math.Max(0, staffCount - item.L7_UnitBreak);
							staffCount -= moduleUsage.StaffCount;
							moduleUsage.PurchasedStaffCount = Math.Max(0, purchasedStaffCount - item.L7_UnitBreak);
							purchasedStaffCount -= moduleUsage.PurchasedStaffCount;
							mixedCount = item.L7_UnitBreak;
							moduleUsage.PopulateFromPriceItem(item);

							moduleUsage.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage = billCoreWithRegisteredUsersCount;
						}
					}
				}
			}
			else if (moduleCode == BillingConstants.RegisteredUserModuleCode)
			{
				if (billCoreWithRegisteredUsersCount)
				{
					var coreUsage = odplUsage.CoreUsage;
					if (coreUsage == null)
					{
						coreUsage = moduleUsages.AddNew();
						coreUsage.ModuleCode = BillingConstants.CoreModuleCode;
						if (priceItem != null)
						{
							coreUsage.PopulateFromPriceItem(priceItem);
						}
					}
					coreUsage.StaffCount = staffCount;
					coreUsage.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage = true;
				}
			}
			else
			{
				var moduleUsage = moduleUsages.AddNew();
				moduleUsage.ModuleCode = moduleCode;
				moduleUsage.StaffCount = staffCount;
				moduleUsage.PurchasedStaffCount = purchasedStaffCount;

				if (priceItem != null)
				{
					moduleUsage.PopulateFromPriceItem(priceItem);
				}

				moduleUsage.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage = billCoreWithRegisteredUsersCount;
			}
		}

		protected override bool AccumulateUnbilledMonths
		{
			get { return false; }
		}

		#endregion

		#region commitment discounts

		void AddCommitmentUsage(List<OdplUsage> result, ClientChargeableUsage[] chargeableUsages)
		{
			var periodStart = GetStartDate(Context.DateToInclusive);
			var commitmentDiscounts = Context.Factory.Load<ClientLicenceBillingDiscount>(GetCommitmentDiscountQuery());
			HashSet<Guid> invoicedOrgPks = new HashSet<Guid>(result.Where(x => x.InvoicedOrganisationPK.IsValid).Select(x => x.InvoicedOrganisationPK.ToGuid()));
			var discountsWithoutUsage = commitmentDiscounts.Where(x => !invoicedOrgPks.Contains(x.Parent.Company.LC_OH.ToGuid())).ToArray();
			if (discountsWithoutUsage.Length > 0)
			{
				var parentUsageMap = chargeableUsages.Where(s => !s.U1_Parent.IsEmpty && s.U1_SubCode == OdplSystemBill.CommitDiscountUsageSubCode).ToDictionary(x => x.U1_Parent);
				foreach (var orgPk in discountsWithoutUsage.Select(x => x.Parent.Company.LC_OH).Distinct())
				{
					var org = Context.Factory.Load<EDIOrgHeader>(orgPk);
					var odplUsage = new OdplUsage(Context.Factory, new UsingParty(org), periodStart);
					ClientChargeableUsage chargeableUsage;
					if (parentUsageMap.TryGetValue(orgPk, out chargeableUsage))
					{
						odplUsage.ChargeableUsagePKs.Add(chargeableUsage.PK);
					}
					result.Add(odplUsage);
				}
			}
		}

		ZQuery GetCommitmentDiscountQuery()
		{
			var query = new ZDBOnlyQuery(typeof(ClientLicenceBillingDiscount));
			query.AddToFilter(ClientLicenceBillingDiscountSchema.L5_SystemCode, BillingConstants.BillingSystem.ODM);
			query.AddToFilter(ClientLicenceBillingDiscountSchema.L5_Type, BillingConstants.DiscountType.Commitment);

			ZQuery startDateQuery = new ZQuery(ClientLicenceBillingDiscountSchema.L5_StartDate, ZDateTime.Empty);
			startDateQuery.AddToFilter(JoinCondition.Or, ClientLicenceBillingDiscountSchema.L5_StartDate, SQLComparisonOperator.LessThan, Context.DateToInclusive);

			ZQuery endDateQuery = new ZQuery(ClientLicenceBillingDiscountSchema.L5_EndDate, ZDateTime.Empty);
			endDateQuery.AddToFilter(JoinCondition.Or, ClientLicenceBillingDiscountSchema.L5_EndDate, SQLComparisonOperator.GreaterThan, GetStartDate(Context.DateToInclusive));

			query.AddToFilter(startDateQuery);
			query.AddToFilter(endDateQuery);

			if (!Context.OrganisationPK.IsEmpty)
			{
				ZDBOnlySubQuery billingSubQuery = new ZDBOnlySubQuery(typeof(ClientLicenceBilling), ClientLicenceBillingDiscountSchema.L5_L4);
				ZDBOnlySubQuery licenceCompanySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), ClientLicenceBillingSchema.L4_LC);
				licenceCompanySubQuery.AddToFilter(LicenceCompanySchema.LC_OH, Context.BillingGroupOrgPKs);
				billingSubQuery.AddSubQuery(licenceCompanySubQuery, JoinCondition.And);
				query.AddSubQuery(billingSubQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#region Create System Bill

		protected override SystemBill CreateSystemBill()
		{
			return new OdplSystemBill(Context);
		}

		#endregion

		#region SQL

		protected override string Query_Raw_Usage
		{
			get
			{
				var result = query_Raw_Usage;
				result = result.Replace("@CountryUsagePriceCodes", "'" + string.Join("', '", BillingConstants.GeographicCompliance.CountryUsagePriceCodes) + @"'");
				return result;
			}
		}

		#endregion

		protected override void AddCodeFilter(ZDBOnlyQuery chargeableUsageQuery)
		{
			var query = new ZQuery(ClientChargeableUsageSchema.U1_Code, SystemCode);
			var usrQuery = new ZQuery(ClientChargeableUsageSchema.U1_Code, BillingConstants.BillingSystem.STL);
			var subCodes = EDIDataRegistry.Instance.ValidStlUsageCodesOnOdplPricelists.Value.OfType<ICodeDescription>()
				.Select(x => x.Code).Where(x => !string.IsNullOrWhiteSpace(x)).Concat(new[] { Business.DatabaseUsage.ActiveUsersUsageCode }).Distinct();

			usrQuery.AddToFilter(ClientChargeableUsageSchema.U1_SubCode, subCodes);
			query.AddToFilter(usrQuery, JoinCondition.Or);

			chargeableUsageQuery.AddToFilter(query);
		}
	}
}

