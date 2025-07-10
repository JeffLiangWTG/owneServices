using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.Integration;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI
{
	public static class BillingUsageSchema
	{
		// Standard prefix for ZClientEDI objects to avoid clashes with generic schema objects.
		// Not using "Client" prefix so objects get automatically removed from the database when they are removed from the schema.
		// "Client" objects don't get removed.
		const string ObjectPrefix = "Edi";

		#region ViewClientCompanyUniqueCodes

		public const string ViewClientCompanyUniqueCodes = ObjectPrefix + "ViewClientCompanyUniqueCodes";

		/// <summary>
		/// All the codes used for a ClientCompany. One row per code and ClientCompany.
		/// Note, the same code may appear more than once per database if a code
		/// was moved from one company to another.
		/// </summary>
		public static DatabaseViewAndRoutineCreateScript ViewClientCompanyUniqueCodesScript()
		{
			const string name = ViewClientCompanyUniqueCodes;
			return new DatabaseViewAndRoutineCreateScript(name, @"
CREATE VIEW " + name + @"
with schemabinding
as
	select IsCurrent = 1, LCC_LD, LCC_PK, LCC_Code, LCC_OH from dbo.ClientCompany
	union all
	select IsCurrent = 0, LCC_LD, CCH_LCC, CCH_Code, LCC_OH from dbo.ClientCompanyCodeHistory
	join dbo.ClientCompany on LCC_PK = CCH_LCC
	where CCH_Code != LCC_Code
	group by LCC_OH, LCC_LD, CCH_Code, CCH_LCC
	
", "DROP VIEW " + name, DbRoutineType.SqlViewTypeDesc);
		}

		#endregion

		#region ViewClientCompanyCodeHistory

		public const string ViewClientCompanyCodeHistory = ObjectPrefix + "ViewClientCompanyCodeHistory";

		/// <summary>
		/// All the codes used for a ClientCompany. One row per code, company and date it came into use.
		/// Note, the same code may appear more than once per company if a code was changed and then changed back.
		/// </summary>
		public static DatabaseViewAndRoutineCreateScript ViewClientCompanyCodeHistoryScript()
		{
			const string name = ViewClientCompanyCodeHistory;
			return new DatabaseViewAndRoutineCreateScript(name, @"
CREATE VIEW " + name + @"
with schemabinding
as
	select IsCurrent = 1, LCC_LD, LCC_PK, LCC_Code, LCC_OH, LCC_CodeValidFromUtc from dbo.ClientCompany
	union all
	select IsCurrent = 0, LCC_LD, CCH_LCC, CCH_Code, LCC_OH, CCH_CodeValidFromUtc from dbo.ClientCompanyCodeHistory
	join dbo.ClientCompany on LCC_PK = CCH_LCC
	
", "DROP VIEW " + name, DbRoutineType.SqlViewTypeDesc);
		}

		#endregion

		#region ViewDatabaseCompanyUniqueCodes

		public const string ViewDatabaseCompanyUniqueCodes = ObjectPrefix + "ViewDatabaseCompanyUniqueCodes";

		/// <summary>
		/// All the company codes used for a database. One row per code and database.
		/// Note, if a code was was moved from one company to another this will pick the company
		/// where the code was most recently valid. If there is more than one such company
		/// it will pick the first created.
		/// </summary>
		public static DatabaseViewAndRoutineCreateScript ViewDatabaseCompanyUniqueCodesScript()
		{
			const string name = ViewDatabaseCompanyUniqueCodes;
			return new DatabaseViewAndRoutineCreateScript(name, @"
CREATE VIEW " + name + @"
with schemabinding
as
		select IsCurrent = 1, LCC_LD, LCC_PK, LCC_Code, LCC_CodeValidFromUtc from dbo.ClientCompany
		union all
		select IsCurrent = 0, a.LCC_LD, b.LCC_PK, a.CCH_Code, a.CCH_CodeValidFromUtc from 
		(
			select c1.LCC_LD, CCH_Code, CCH_CodeValidFromUtc = MAX(CCH_CodeValidFromUtc)
			from dbo.ClientCompanyCodeHistory
			join dbo.ClientCompany c1 on LCC_PK = CCH_LCC
			left join dbo.ClientCompany dupe on c1.LCC_LD = dupe.LCC_LD and c1.LCC_PK != dupe.LCC_PK and dupe.LCC_Code = CCH_Code
			where c1.LCC_Code != CCH_Code and dupe.LCC_PK is null
			group by c1.LCC_LD, CCH_Code
		) a
		cross apply
		(
			select top 1 LCC_PK
			from dbo.ClientCompanyCodeHistory hb
			join dbo.ClientCompany c3 on LCC_PK = CCH_LCC
			where c3.LCC_LD = a.LCC_LD and hb.CCH_Code = a.CCH_Code and a.CCH_CodeValidFromUtc = hb.CCH_CodeValidFromUtc
			order by c3.LCC_CreateTimeUtc
		) b
", "DROP VIEW " + name, DbRoutineType.SqlViewTypeDesc);
		}

		#endregion

		#region EdiClientCompanyMerge

		public static DatabaseViewAndRoutineCreateScript EdiClientCompanyMergeScript()
		{
			return new DatabaseViewAndRoutineCreateScript("EdiClientCompanyMerge",
@"CREATE PROCEDURE EdiClientCompanyMerge
	@OldPk uniqueidentifier,
	@NewPk uniqueidentifier
AS
BEGIN
SET NOCOUNT ON;

-- Fix duplicate EdiLicenceUsage
--
select OldPk = uold.LX2_PK, NewPk = unew.LX2_PK, 
	FirstUsageUtc = case when uold.LX2_FirstUsageUtc < unew.LX2_FirstUsageUtc then uold.LX2_FirstUsageUtc else unew.LX2_FirstUsageUtc end,
	LastUsageUtc = case when uold.LX2_LastUsageUtc > unew.LX2_LastUsageUtc then uold.LX2_LastUsageUtc else unew.LX2_LastUsageUtc end,
	UsageCount = uold.LX2_UsageCount + unew.LX2_UsageCount
into #dupeLX2
from dbo.EdiLicenceUsage uold
join dbo.EdiLicenceUsage unew on uold.LX2_Period = unew.LX2_Period 
	and uold.LX2_LicenceMode = unew.LX2_LicenceMode
	and uold.LX2_ModuleCode = unew.LX2_ModuleCode
	and uold.LX2_LS = unew.LX2_LS
where uold.LX2_LCC = @OldPk
	and unew.LX2_LCC = @NewPk

update dbo.EdiLicenceUsage
set LX2_FirstUsageUtc = FirstUsageUtc,
	LX2_LastUsageUtc = LastUsageUtc,
	LX2_UsageCount = UsageCount
from dbo.EdiLicenceUsage
join #dupeLX2 on LX2_PK = NewPk

delete from dbo.EdiLicenceUsage where LX2_PK in (select OldPk from #dupeLX2)
drop table #dupeLX2

-- Fix duplicate ClientChargeableUsage
--
select OldPk = uold.U1_PK, NewPk = unew.U1_PK
into #dupeU1
from dbo.ClientChargeableUsage uold
join dbo.ClientChargeableUsage unew on 
	uold.U1_Code = unew.U1_Code
	and uold.U1_PeriodStart = unew.U1_PeriodStart
	and ((uold.U1_LC is null and unew.U1_LC is null) or (uold.U1_LC = unew.U1_LC))
	and ((uold.U1_Parent is null and unew.U1_Parent is null) or (uold.U1_Parent = unew.U1_Parent))
	and uold.U1_Reference1 = unew.U1_Reference1
	and uold.U1_Reference2 = unew.U1_Reference2
	and uold.U1_Reference3 = unew.U1_Reference3
	and uold.U1_Reference4 = unew.U1_Reference4
	and uold.U1_SubCode = unew.U1_SubCode
	and ((uold.U1_LD is null and unew.U1_LD is null) or (uold.U1_LD = unew.U1_LD))
where uold.U1_LCC = @OldPk
	and unew.U1_LCC = @NewPk

delete from dbo.ClientChargeableUsage where U1_PK in (select OldPk from #dupeU1)
drop table #dupeU1

update dbo.ClientLicenceUsage
set LX_LCC = @NewPk
where LX_LCC = @OldPk

update dbo.EdiLicenceUsage
set LX2_LCC = @NewPk
where LX2_LCC = @OldPk

update dbo.ClientChargeableUsage
set U1_LCC = @NewPk
where U1_LCC = @OldPk

update dbo.EdiBilledUsage
set BU9_LCC = @NewPk
where BU9_LCC = @OldPk

update dbo.IncidentMain
set IM_LCC = @NewPk
where IM_LCC = @OldPk

update dbo.HelpErrorLogOccurrence
set HO_LCC = @NewPk
where HO_LCC = @OldPk

update dbo.ClientPremiumService 
set CPS_LCC = @NewPk
where CPS_LCC = @OldPk

update dbo.EdiCommissionAgreementCompanyPivot
set EPY_LCC = @NewPk
where EPY_LCC = @OldPk and EPY_EZN not in (select EPY_EZN from dbo.EdiCommissionAgreementCompanyPivot where EPY_LCC = @newPk)


update dbo.EdiCommissionHeaderAdditionalInfo
set ECH_LCC = @NewPk
where ECH_LCC = @OldPk

insert dbo.EdiClientCompanyMergeHistory(CMH_FromPK, CMH_ToPK, CMH_FromCode, CMH_MergeTimeUtc)
select @OldPk, @NewPk, LCC_Code, GETUTCDATE()
from dbo.ClientCompany where LCC_PK = @OldPk

END
", "DROP PROCEDURE EdiClientCompanyMerge", DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion

		#region Company Code To Client Company

		public const string CompanyCodeToClientCompany = ObjectPrefix + "CompanyCodeToClientCompany";

		/// <summary>
		/// Best guess for which ClientCompany matches an EnterpriseCode plus CompanyCode.
		/// Returns a record for every combination of EnterpriseCode + ClientCompanyCode on production databases 
		/// along with ClientCompany PK and any linked LicenceCompany PK.
		/// Note, a LicenceCompany PK may occur more than once since multiple CompanyCodes may match the same LicenceCompany
		/// if the code has changed over time.
		/// Chooses the most "active" production database.
		/// Mostly for eRouter usage, which reports EnterpriseCode plus CompanyCode, but not server code.
		/// </summary>
		public static DatabaseViewAndRoutineCreateScript CompanyCodeToClientCompanyScript()
		{
			const string name = CompanyCodeToClientCompany;
			return new DatabaseViewAndRoutineCreateScript(name,
@"create function " + name + @"(@now smalldatetime)
returns table
with schemabinding
as return
	select LC_PK, LCC_PK, LC_CompanyCode, LE_EnterpriseCode
	from
	(
		SELECT LC_PK, LCC_PK, LC_CompanyCode = LCC_Code, LE_EnterpriseCode, LD_ServerCode,
			n = ROW_NUMBER() over
			(
				partition by LE_PK, LCC_Code
				order by
					case when LD_LastHeartbeat is null or LD_LastHeartbeat < DATEADD(DAY, 30, @now) then 0 else 1 end desc, -- prefer alive
					LD_IsActive desc,
					ISNULL(LA_IsActive, 0) desc,
					case when LA_AgreedLiveDate is not null and LA_AgreedLiveDate < @now then LA_AgreedLiveDate else DATEFROMPARTS(1990, 1, 1) end desc -- prefer youngest
			)
		FROM dbo." + ViewClientCompanyUniqueCodes + @" vw
		JOIN dbo.LicenceDatabase on LCC_LD = LD_PK
		join dbo.LicenceEnterprise on LD_LE = LE_PK
		left join dbo.LicenceCompany on LCC_OH = LC_OH
		left join dbo.LicenceHeader on LA_LD = LD_PK and LA_LC = LC_PK
		where LD_LicenceType = 'PRD' and LD_Product in ('ENT', 'CW1', 'CWN', 'CGW', 'PRW')
	) a where n = 1;
", "DROP function " + name, DbRoutineType.SqlFunctionInlineTypeDesc);
		}

		#endregion

		#region Non billed Enterprise codes

		public const string NonBilledEnterpriseCodes = ObjectPrefix + "NonBilledEnterpriseCodes";

		public static DatabaseViewAndRoutineCreateScript NonBilledEnterpriseCodesScript()
		{
			const string name = NonBilledEnterpriseCodes;
			return new DatabaseViewAndRoutineCreateScript(name, @"
CREATE function " + name + @"()
RETURNS @Result TABLE
(
	EnterpriseCode varchar(3) NOT NULL
)
with schemabinding
BEGIN
	DECLARE @CodeXml XML = (SELECT CONVERT(VARCHAR(MAX), SD_BinaryValue) AS XML FROM dbo.StmData WHERE SD_Name = 'NotBilledEnterpriseCodes');

	if @CodeXml is not null
	begin
		INSERT INTO @Result
		SELECT 
			NodesTable.Item.value('Code[1]', 'char(3)')
		FROM
			@CodeXml.nodes('/NewDataSet/Table1') AS NodesTable(Item);
	end
	else
	begin
		insert @Result
		values ('" + string.Join("'), ('", EDIDataRegistry.Instance.NonBilledEnterpriseCodes.DefaultValue.Cast<ICodeDescription>().Select(x => x.Code)) + @"');
	end;

	return;
end

", "DROP function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region ChargeableUsageUpdate

		public const string ChargeableUsageUpdate = ObjectPrefix + "ChargeableUsageUpdate";

		public static DatabaseViewAndRoutineCreateScript GetChargeableUsageUpdateScript()
		{
			const string name = ChargeableUsageUpdate;
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE PROCEDURE " + name + @"
	@FirstDayOfMonth smalldatetime,
	@PeriodStartTimeUtc smalldatetime, 
	@PeriodEndTimeUtc smalldatetime,
	@Code varchar(3),
	@BillingDbPriceItemCode varchar(3) = '',
	@BillingDbKeyRefIndex1 tinyint = 0
with recompile
AS
BEGIN
	SET NOCOUNT ON;
	
	CREATE TABLE #Usage
	(
		LC_PK uniqueidentifier NULL,
		LD_PK uniqueidentifier NULL,
		LCC_PK uniqueidentifier NULL,
		SubCode varchar(50) NOT NULL,
		UnitCount decimal(14, 4) NOT NULL,
		Reference1 varchar(50) NOT NULL,
		Reference2 varchar(50) NOT NULL,
		Reference3 varchar(50) NOT NULL,
		Reference4 varchar(50) NOT NULL,
		PK uniqueidentifier NULL,
		IsModified bit NULL,
		TotalPrice money NOT NULL DEFAULT(0),
		RX_NKCurrency varchar(3) NOT NULL DEFAULT (''),
		Direction varchar(3) NOT NULL DEFAULT (''),
	)

	-- For usage for Traxxon we have LC_PK, but not LD_PK and LCC_PK.
	-- For system usage (e.g. hosting) we know LD_PK, but not LC_PK, LCC_PK.
	-- For other usage we know LD_PK and LCC_PK, but not LC_PK if it's a customer created company.

	declare @Period int = YEAR(@FirstDayOfMonth) * 100 + MONTH(@FirstDayOfMonth);
	
	begin try
		if @BillingDbPriceItemCode != ''
		begin
			INSERT INTO #Usage (LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4)
			Select LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, ISNULL(Reference1, ''), ISNULL(Reference2, ''), ISNULL(Reference3, ''), ISNULL(Reference4, '')
			from EdiGetBillingDbUsage(@Period, @Code, @BillingDbPriceItemCode, @BillingDbKeyRefIndex1) OPTION (RECOMPILE);
		end
		else if @Code != 'ODM'
			begin
				DECLARE @sql nvarchar(500);
				if @Code != 'STL'
				begin
					if object_id('" + GetUsageFunctionOrProcedureName("") + @"' + @Code) is not null
					begin
						SET @sql = N'Select LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, Reference1 = ISNULL(Reference1, ''''), Reference2 = ISNULL(Reference2, ''''), Reference3 = ISNULL(Reference3, ''''), Reference4 = ISNULL(Reference4, '''') from " + GetUsageFunctionOrProcedureName("") + @"' + @Code + '(@Period, @PeriodStartTimeUtc, @PeriodEndTimeUtc) OPTION (RECOMPILE)';
					end	
					else
					begin
						SET @sql = N'Select LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, Reference1 = ISNULL(Reference1, ''''), Reference2 = ISNULL(Reference2, ''''), Reference3 = ISNULL(Reference3, ''''), Reference4 = ISNULL(Reference4, '''') from " + GetUsageFunctionOrProcedureName("GenericUsage") + @"(@Period, @PeriodStartTimeUtc, @PeriodEndTimeUtc, @Code) OPTION (RECOMPILE)';
					end
					INSERT INTO #Usage (LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4)
					EXECUTE sp_executesql @sql, N'@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime, @Code varchar(3)', @Period = @Period, @PeriodStartTimeUtc = @PeriodStartTimeUtc, @PeriodEndTimeUtc = @PeriodEndTimeUtc, @Code = @Code;
				end
				else
					begin
						SET @sql = N'Select LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, Reference1 = ISNULL(Reference1, ''''), Reference2 = ISNULL(Reference2, ''''), Reference3 = ISNULL(Reference3, ''''), Reference4 = ISNULL(Reference4, ''''), TotalPrice, RX_NKCurrency, Direction from " + GetUsageFunctionOrProcedureName("") + @"' + @Code + '(@Period, @PeriodStartTimeUtc, @PeriodEndTimeUtc) OPTION (RECOMPILE)';
						INSERT INTO #Usage (LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4, TotalPrice, RX_NKCurrency, Direction)
						EXECUTE sp_executesql @sql, N'@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime, @Code varchar(3)', @Period = @Period, @PeriodStartTimeUtc = @PeriodStartTimeUtc, @PeriodEndTimeUtc = @PeriodEndTimeUtc, @Code = @Code;
					end
			end
		else
			begin
				INSERT INTO #Usage (LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4)
				EXECUTE " + GetUsageFunctionOrProcedureName("ODM") + @" @Period, @PeriodStartTimeUtc, @PeriodEndTimeUtc;
			end
	end try
	begin catch
		throw;
	end catch

	create clustered index IX_LCC_PK on #usage(LCC_PK)

	-- remove non-billed usage
	declare @NonBilledEnterprises table (EnterprisePk uniqueidentifier primary key clustered);
	insert @NonBilledEnterprises select distinct LE_PK from dbo.LicenceEnterprise where LE_EnterpriseCode in (select EnterpriseCode from EdiNonBilledEnterpriseCodes());

	declare @NonBilledDatabasePk table (PK uniqueidentifier primary key clustered)
	insert @NonBilledDatabasePk(PK) select LD_PK from dbo.LicenceDatabase where LD_LE in (select EnterprisePk from @NonBilledEnterprises)

	declare @NonBilledClientCompanyPk table (PK uniqueidentifier primary key clustered)
	insert @NonBilledClientCompanyPk(PK) select LCC_PK from dbo.ClientCompany where LCC_LD in (select PK from @NonBilledDatabasePk)

	declare @NonBilledLicenceCompanyPk table (PK uniqueidentifier primary key clustered)
	insert @NonBilledLicenceCompanyPk(PK) select LC_PK from dbo.LicenceCompany where LC_LE in (select EnterprisePk from @NonBilledEnterprises)

	delete from #Usage 
	where LD_PK in (select PK from @NonBilledDatabasePk)
		or LC_PK in (select PK from @NonBilledLicenceCompanyPk)
		or LCC_PK in (select PK from @NonBilledClientCompanyPk)
	option(recompile);

	declare @AllClientCompanyPk table (PK uniqueidentifier primary key clustered)
	insert @AllClientCompanyPk(PK) select LCC_PK from dbo.ClientCompany;

	-- remove invalid LCC_PK
	delete from #Usage 
	where (LCC_PK is not null and LCC_PK not in (select PK from @AllClientCompanyPk))
	option(recompile);

	-- ensure U1_LD is set when U1_LCC is set
	UPDATE #Usage
		SET LD_PK = LCC_LD
	FROM #Usage u
	JOIN dbo.ClientCompany c on u.LCC_PK = c.LCC_PK
	WHERE LD_PK is null
	option(recompile);

	UPDATE #Usage
	SET
		SubCode = UPPER(SubCode)
	WHERE LEN(SubCode) <= 3 -- long subcode should case-sensitive, because it can be used as item description.
	option(recompile);

	UPDATE
		#Usage
	SET
		PK = U1_PK,
		IsModified = CASE WHEN (UnitCount != U1_UnitCount OR TotalPrice != U1_TotalPrice) THEN 1 ELSE 0 END
	FROM
		#Usage
		JOIN dbo.ClientChargeableUsage on
			(
				((LCC_PK is null and U1_LCC is null) or LCC_PK = U1_LCC)
				and
				(LCC_PK is not null or LC_PK is null or LC_PK = U1_LC) -- only join on LC_PK if LCC_PK is null
				and
				((LD_PK is null and U1_LD is null) or LD_PK = U1_LD)
			)
			AND U1_Code = @Code
			AND U1_SubCode = SubCode COLLATE DATABASE_DEFAULT
			AND U1_PeriodStart = @FirstDayOfMonth
			AND U1_Reference1 = Reference1 COLLATE DATABASE_DEFAULT
			AND U1_RX_NKCurrency = RX_NKCurrency COLLATE DATABASE_DEFAULT
			AND U1_Direction = Direction COLLATE DATABASE_DEFAULT
	option(recompile);

	-- set LC_PK from LCC_PK for ediProd created companies
	UPDATE #Usage
		SET LC_PK = LicenceCompany.LC_PK
	FROM #Usage u
	JOIN dbo.ClientCompany c on u.LCC_PK = c.LCC_PK
	JOIN dbo.LicenceCompany on c.LCC_OH = LC_OH
	WHERE u.LC_PK is null
	option(recompile);

	-- set LC_PK from the LD_PK owner for database level charges
	UPDATE #Usage
		SET LC_PK = vw.LC_PK
	FROM #Usage u
	JOIN " + ViewLicenceDatabaseOwner + @" vw on vw.LD_PK = u.LD_PK
	WHERE u.LC_PK is null and u.LCC_PK is null and u.LD_PK is not null
	option(recompile);

	BEGIN TRAN

	IF 0 = 0
	BEGIN
		-- delete old, non-invoiced usage if the usage no longer exists
		DELETE dbo.ClientChargeableUsage
		FROM dbo.ClientChargeableUsage
		LEFT JOIN dbo.AccTransactionHeader on U1_AH_Invoice = AH_PK
		LEFT JOIN #Usage on U1_PK = PK
		WHERE
			U1_PeriodStart = @FirstDayOfMonth
			and (U1_AH_Invoice is NULL or AH_IsCancelled = 1)
			and U1_Code = @Code
			and (@BillingDbPriceItemCode = '' or U1_SubCode = @BillingDbPriceItemCode)
			and PK IS NULL
			option(recompile);
			
		-- clear old, invoiced usage if it no longer exists
		UPDATE dbo.ClientChargeableUsage
		SET
			U1_UnitCount = 0,
			U1_TotalPrice = 0,
			U1_UpdateTime = GETUTCDATE()
		FROM dbo.ClientChargeableUsage
		JOIN dbo.AccTransactionHeader on U1_AH_Invoice = AH_PK
		LEFT JOIN #Usage on U1_PK = PK
		WHERE
			U1_PeriodStart = @FirstDayOfMonth
			and AH_IsCancelled = 0
			and U1_Code = @Code
			and (@BillingDbPriceItemCode = '' or U1_SubCode = @BillingDbPriceItemCode)
			and PK IS NULL
			and (U1_UnitCount != 0 OR U1_TotalPrice != 0)
			option(recompile);
	END;

	-- delete new #Usage if there is no change
	DELETE FROM #Usage
	WHERE IsModified = 0 AND PK IS NOT NULL
	option(recompile);

	-- insert new usage
	INSERT INTO dbo.ClientChargeableUsage(
		U1_PK,
		U1_LC,
		U1_LD,
		U1_LCC,
		U1_Code,
		U1_SubCode,
		U1_PeriodStart,
		U1_UnitCount,
		U1_Reference1, U1_Reference2, U1_Reference3, U1_Reference4,
		U1_TotalPrice, U1_RX_NKCurrency, U1_Direction,
		U1_UpdateTime
		)
	SELECT 
		NEWID(),
		LC_PK,
		LD_PK,
		LCC_PK,
		@Code,
		SubCode,
		@FirstDayOfMonth,
		UnitCount,
		Reference1, Reference2, Reference3, Reference4,
		TotalPrice, RX_NKCurrency, Direction,
		GETUTCDATE()
	FROM #Usage
	WHERE PK IS NULL
	option(recompile);

	-- update modified usage
	UPDATE
		dbo.ClientChargeableUsage
	SET 
		U1_UnitCount = UnitCount,
		U1_TotalPrice = TotalPrice,
		U1_UpdateTime = GETUTCDATE()
	FROM 
		dbo.ClientChargeableUsage
		JOIN #Usage on U1_PK = PK
	WHERE IsModified = 1
	option(recompile);

	COMMIT

END
", "DROP PROCEDURE " + name, DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion

		#region ViewLicenceDatabaseOwner

		public const string ViewLicenceDatabaseOwner = ObjectPrefix + "ViewLicenceDatabaseOwner";

		/// <summary>
		/// View of LicenceDatabase and the LicenceHeader that is the designated owner.
		/// </summary>
		public static DatabaseViewAndRoutineCreateScript ViewLicenceDatabaseOwnerScript()
		{
			const string name = ViewLicenceDatabaseOwner;
			return new DatabaseViewAndRoutineCreateScript(name, @"
CREATE VIEW " + name + @"
with schemabinding
as
select LD_PK, LA_PK, LC_PK, LC_OH from dbo.LicenceDatabase
join dbo.LicenceHeader on LA_LD = LD_PK
join dbo.LicenceCompany on LA_LC = LC_PK
where LD_OH_BillingParty = LC_OH

union all

select LD_PK, LA_PK, LC_PK, LC_OH
from dbo.LicenceDatabase
cross apply
(
	select top 1 LA_PK, LC_PK, LC_OH from dbo.LicenceHeader h
	join dbo.LicenceCompany c on h.LA_LC = c.LC_PK
	join dbo.OrgHeader on LC_OH = OH_PK
	where h.LA_LD = LD_PK
	order by
		(OH_IsActive & h.LA_IsActive) desc,
		case when ISNULL(h.LA_AgreedLiveDate, h.LA_EstimatedLiveDate) is not null then 0 else 1 end,
		ISNULL(h.LA_AgreedLiveDate, h.LA_EstimatedLiveDate),
		c.LC_CompanyCode
) a
where LD_OH_BillingParty is null;
", "DROP VIEW " + name, DbRoutineType.SqlViewTypeDesc);
		}

		#endregion

		#region ViewClientCompanyLicence

		public const string ViewClientCompanyLicence = ObjectPrefix + "ViewClientCompanyLicence";

		/// <summary>
		/// View of ClientCompany and the LicenceHeader that owns the usage.
		/// For a customer created company it will be the LicenceHeader that owns the database.
		/// </summary>
		public static DatabaseViewAndRoutineCreateScript ViewClientCompanyLicenceScript()
		{
			const string name = ViewClientCompanyLicence;
			return new DatabaseViewAndRoutineCreateScript(name, @"
CREATE VIEW " + name + @"
with schemabinding
as
	select LCC_PK, LA_PK, LC_PK, LC_OH from dbo.ClientCompany
	join dbo.LicenceDatabase on LCC_LD = LD_PK
	join dbo.LicenceCompany on LCC_OH = LC_OH
	join dbo.LicenceHeader on LA_LD = LD_PK and LA_LC = LC_PK
	where LCC_OH is not null

	union all

	select LCC_PK, LA_PK, LC_PK, LC_OH from dbo.ClientCompany
	join dbo." + ViewLicenceDatabaseOwner + @" owner on LCC_LD = owner.LD_PK
	where LCC_OH is null;
", "DROP VIEW " + name, DbRoutineType.SqlViewTypeDesc);
		}

		#endregion

		#region View vwBillingClientCompany

		public static DatabaseViewAndRoutineCreateScript GetViewBillingClientCompanyScript()
		{
			return new DatabaseViewAndRoutineCreateScript("vwBillingClientCompany", @"
CREATE VIEW vwBillingClientCompany WITH SCHEMABINDING
AS
SELECT
	[Client Company PK]     = lcc.LCC_PK,
	[Enterprise Code]       = le.LE_EnterpriseCode,
	[Database Number]       = ld.LD_DatabaseNumber,
	[Licence Key]           = le.LE_EnterpriseCode + '-' + lcc.LCC_Code + '-' + ld.LD_ServerCode,
	[Client Enterprise]     = '(' + le.LE_EnterpriseCode + ') ' + ohEnterprise.OH_FullName,
	[Client System]         = le.LE_EnterpriseCode + '-' + ld.LD_ServerCode,
	[Company]               = '(' + le.LE_EnterpriseCode + '-' + lcc.LCC_Code + '-' + ld.LD_ServerCode + ') ' + ISNULL((ohCompany.OH_FullName + ' [' + ohCompany.OH_Code + ']'), lcc.LCC_Name),
	[Home Country]          = ISNULL('(' + lcc.LCC_RN_NKCountryCode + ') ' + rn.RN_Desc, ''),
	[Hosted Location]       = ld.LD_HostedLocation,
	[System Type]           = ld.LD_LicenceType,
	[STL Start Date]        = CAST(stl.StlStartDate AS DATE)
FROM
	dbo.LicenceEnterprise le
	INNER JOIN dbo.LicenceDatabase ld ON ld.LD_LE = le.LE_PK
	INNER JOIN dbo.OrgHeader ohEnterprise ON ohEnterprise.OH_PK = le.LE_OH
	INNER JOIN dbo.ClientCompany lcc ON lcc.LCC_LD = ld.LD_PK
	LEFT JOIN dbo.EdiViewClientCompanyLicence vwCcl ON vwCcl.LCC_PK = lcc.LCC_PK
	LEFT JOIN dbo.OrgHeader ohCompany ON ohCompany.OH_PK = vwCcl.LC_OH
	LEFT JOIN dbo.RefCountry rn ON rn.RN_Code = lcc.LCC_RN_NKCountryCode
	LEFT JOIN (
		SELECT PHL_LD, StlStartDate = min(PHL_ValidFrom)
		FROM dbo.EdiPriceHeaderLink
		GROUP BY PHL_LD
	) stl ON stl.PHL_LD = ld.LD_PK
", "DROP VIEW vwBillingClientCompany", DbRoutineType.SqlViewTypeDesc);
		}

		#endregion

		#region MyAccount Usage Report

		#region GetSystemUsage (ODPL)

		public const string GetSystemUsage = ObjectPrefix + "GetSystemUsage";

		public static DatabaseViewAndRoutineCreateScript GetSystemUsageScript()
		{
			const string name = GetSystemUsage;

			// Return usage that is related to an org
			// An org can:
			// - be linked to DBs (i.e., the DB is attached to the org)
			// - pay for other orgs

			// Related orgs are defined as:
			// - also on a linked DB
			// - payed for by the org
			// - pay for the org
			// - payed for by same org.
			// So if A pays for B and A also pays for C
			// then A, B and C are all related.

			// All usage is owned by an org.
			// So related usage is usage where the owner is the org or a related org
			return new DatabaseViewAndRoutineCreateScript(name,
@"create function " + name + @"(@OrgPk uniqueidentifier, @PeriodStart DateTime)
RETURNS @Result TABLE
(
	OH_PK uniqueidentifier NOT NULL,
	OH_Code varchar(100) NOT NULL, 
	OH_FullName varchar(1000) NOT NULL, 
	U1_Code varchar(3) NOT NULL,
	ServerCode varchar(3) NOT NULL,
	CompanyCode varchar(3) NOT NULL,
	LCC_PK uniqueidentifier,
	LC_PK uniqueidentifier,
	LD_PK uniqueidentifier,
	U1_UnitCount int NOT NULL,
	L7_PK UNIQUEIDENTIFIER NULL,
	L7_Description NVARCHAR(500) NULL
)
BEGIN
declare @CompanyPk uniqueidentifier = (select LC_PK from dbo.LicenceCompany where LC_OH = @OrgPk);
declare @RelatedCompany table (LC uniqueidentifier, OH uniqueidentifier);

insert @RelatedCompany(LC, OH)
select @CompanyPk, @OrgPk

union

-- companies that share an active database
select h2.LA_LC, c2.LC_OH
from dbo.LicenceCompany c1
join dbo.LicenceHeader h1 on h1.LA_LC = LC_PK and h1.LA_IsActive = 1
join dbo.LicenceDatabase on h1.LA_LD = LD_PK and LD_IsActive = 1
join dbo.LicenceHeader h2 on h2.LA_LD = h1.LA_LD and h2.LA_IsActive = 1
join dbo.LicenceCompany c2 on h2.LA_LC = c2.LC_PK
join dbo.OrgHeader o2 on c2.LC_OH = o2.OH_PK and o2.OH_IsActive = 1
where c1.LC_OH = @OrgPk

union

-- companies invoiced to...
select L9_LC, LC_OH
from dbo.ClientInvoiceDelivery
join dbo.LicenceCompany on LC_PK = L9_LC
where 
	L9_OH_InvoiceTo in 
(
	-- same invoicing org as this org (if not a partner org)
	select L9_OH_InvoiceTo 
	from dbo.ClientInvoiceDelivery 
	JOIN dbo.LicenceCompany parent on parent.LC_OH = L9_OH_InvoiceTo
	LEFT JOIN dbo.ClientLicenceBilling parentBilling on parentBilling.L4_LC = parent.LC_PK
	where L9_LC = @CompanyPk and L9_OH_InvoiceTo is not null
		AND ISNULL(parentBilling.L4_IsPartner, 'N') = 'N'

	union

	-- this org (if not a partner org)
	select LC_OH
	from dbo.LicenceCompany 
	LEFT JOIN dbo.ClientLicenceBilling on L4_LC = LC_PK
	WHERE LC_OH = @OrgPk
		AND ISNULL(L4_IsPartner, 'N') = 'N'
);

-- production, non STL databases
declare @RelatedDatabase table (LD uniqueidentifier not null, LC uniqueidentifier, OH uniqueidentifier, LD_ServerCode varchar(3) not null, L6 uniqueidentifier);
insert @RelatedDatabase (LD, LC, OH, LD_ServerCode, L6)
select vw.LD_PK, LC_PK, LC_OH, LD_ServerCode, L6_PK
from (
	select distinct LD = LA_LD, LD_ServerCode
	from @RelatedCompany rc
	join dbo.LicenceHeader on LA_LC = rc.LC
	join dbo.LicenceDatabase on LA_LD = LD_PK and LD_LicenceType = 'PRD' and LD_Product in ('ENT', 'CW1', 'CWN', 'CGW', 'BOR', 'PRW')
) db
join dbo.EdiViewLicenceDatabaseOwner vw on vw.LD_PK = db.LD
join @RelatedCompany rc on rc.LC = vw.LC_PK
left join EdiGetOdmPriceHeadersForDate(@PeriodStart) ph on ph.LA_PK = vw.LA_PK
outer apply
(
	SELECT top 1 PHL_L6
	FROM dbo.EdiPriceHeaderLink 
	WHERE PHL_ValidFrom <= @PeriodStart AND (PHL_ValidTo IS NULL OR PHL_ValidTo > @PeriodStart)
		and PHL_LD = vw.LD_PK
	-- order not needed since we're looking for not exists
) p
where PHL_L6 is null

declare @TestDatabase table (LD uniqueidentifier not null, LC uniqueidentifier, OH uniqueidentifier, LD_ServerCode varchar(3) not null);
insert @TestDatabase (LD, LC, OH, LD_ServerCode)
select testDb.LD_PK, vw.LC_PK, vw.LC_OH, testDb.LD_ServerCode
from dbo.LicenceDatabase testDb
join @RelatedDatabase parentDb on testDb.LD_LD_ParentDatabase = parentDb.LD
join dbo.EdiViewLicenceDatabaseOwner vw on vw.LD_PK = testDb.LD_PK
join @RelatedCompany rc on rc.LC = vw.LC_PK
where testDb.LD_Product in ('ENT', 'CW1', 'CWN', 'CGW', 'PRW')

-- client companies - related to org, and production, non STL databases
declare @RelatedClientCompany table (LCC uniqueidentifier, LC uniqueidentifier, OH uniqueidentifier);
insert @RelatedClientCompany(LCC, LC, OH)
select ClientCompany.LCC_PK, LC_PK, LC_OH
from dbo.ClientCompany
join @RelatedDatabase db on LCC_LD = LD
join dbo.EdiViewClientCompanyLicence vw on ClientCompany.LCC_PK = vw.LCC_PK
where LC_PK in (select LC from @RelatedCompany);

-- Stl Usage Codes from Registry.ValidStlUsageCodesOnOdplPricelists
declare @StlUsageCodes table ( STL_SubCode varchar(50) NOT NULL primary key );
declare @xmldata xml = (select convert(varchar(max), SD_BinaryValue ) from dbo.StmData where SD_Name = 'ValidStlUsageCodesOnOdplPricelists');

insert @StlUsageCodes(STL_SubCode)
select distinct T.X.value('(Code/text())[1]', 'varchar(50)') STL_SubCode from @xmlData.nodes('/NewDataSet/Table1') as T(X);

with UsageRaw(U1_LD, U1_LCC, U1_LC, U1_Code, U1_SubCode, U1_UnitCount) as 
(
	select U1_LD, U1_LCC, U1_LC, U1_Code, U1_SubCode, U1_UnitCount = SUM(U1_UnitCount)
	from dbo.ClientChargeableUsage 
	where U1_UnitCount > 0 AND U1_PeriodStart = @PeriodStart
	group by U1_LD, U1_LCC, U1_LC, U1_Code, U1_SubCode
)
,Usage(U1_LD, U1_LCC, U1_LC, U1_Code, U1_SubCode, U1_UnitCount) as 
(
	select U1_LD, U1_LCC, U1_LC, U1_Code, U1_SubCode, U1_UnitCount
	from UsageRaw
	where U1_Code != 'STL'
)
insert @Result(OH_PK, OH_Code, OH_FullName, U1_Code, ServerCode, CompanyCode, LCC_PK, LC_PK, LD_PK, U1_UnitCount, L7_PK, L7_Description)
select OH_PK, OH_Code, OH_FullName, U1_Code, ServerCode, CompanyCode, LCC, LC, LD, U1_UnitCount, L7_PK, L7_Description
from
(
	-- usage with a database
	select OH, U1_Code, ServerCode, CompanyCode, LCC, LC, LD, U1_UnitCount = SUM(U1_UnitCount), L7_PK = null, L7_Description = null, L7_Order = null
	from
	(
		select 
			OH = coalesce(cc.OH, rc.OH, db.OH), 
			U1_Code = case U1_Code when 'CPT' then U1_SubCode else U1_Code end,
			ServerCode = ISNULL(LD_ServerCode, ''), 
			CompanyCode = coalesce(LCC_Code, LC_CompanyCode, ''), 
			LCC = case when cc.OH is not null then cc.LCC else null end, 
			LC = case when cc.OH is null and rc.OH is not null then rc.LC else null end, 
			LD = db.LD,
			U1_UnitCount
		from 
			Usage
			join @RelatedDatabase db on db.LD = U1_LD
			left join @RelatedClientCompany cc on cc.LCC = U1_LCC
			left join @RelatedCompany rc on rc.LC = U1_LC
			left join dbo.ClientCompany on cc.LCC = LCC_PK
			left join dbo.LicenceCompany on rc.LC = LC_PK
	) a
	group by OH, U1_Code, ServerCode, CompanyCode, LCC, LC, LD

	union all

	-- usage with a database (STL)
	select OH, U1_Code, ServerCode, CompanyCode, LCC, LC, LD, U1_UnitCount = SUM(U1_UnitCount), L7_PK, L7_Description, L7_Order
	from
	(
		select 
			OH = coalesce(cc.OH, rc.OH, db.OH), 
			U1_Code = case U1_Code when 'CPT' then U1_SubCode else U1_Code end,
			ServerCode = ISNULL(LD_ServerCode, ''), 
			CompanyCode = coalesce(LCC_Code, LC_CompanyCode, ''), 
			LCC = case when cc.OH is not null then cc.LCC else null end, 
			LC = case when cc.OH is null and rc.OH is not null then rc.LC else null end, 
			LD = db.LD,
			U1_UnitCount,
			L7_PK,
			L7_Description,
			L7_Order
		from 
			UsageRaw
			join @StlUsageCodes stl on stl.STL_SubCode = U1_SubCode
			join @RelatedDatabase db on db.LD = U1_LD
			join dbo.ClientLicencePriceItem on L7_L6 = L6 and U1_SubCode = L7_Code
			left join @RelatedClientCompany cc on cc.LCC = U1_LCC
			left join @RelatedCompany rc on rc.LC = U1_LC
			left join dbo.ClientCompany on cc.LCC = LCC_PK
			left join dbo.LicenceCompany on rc.LC = LC_PK
		where U1_Code = 'STL' and L6 is not null
	) d
	group by OH, U1_Code, ServerCode, CompanyCode, LCC, LC, LD, L7_PK, L7_Description, L7_Order

	union all

	-- no database
	select OH, U1_Code, ServerCode, CompanyCode, LCC, LC, LD, U1_UnitCount = SUM(U1_UnitCount), L7_PK = null, L7_Description = null, L7_Order = null
	from
	(
		select 
			rc.OH, 
			U1_Code = case U1_Code when 'CPT' then U1_SubCode else U1_Code end,
			ServerCode = '', 
			CompanyCode = LC_CompanyCode, 
			LCC = null, 
			LC = rc.LC, 
			LD = null,
			U1_UnitCount
		from 
			Usage
			left join @RelatedCompany rc on rc.LC = U1_LC
			left join dbo.LicenceCompany on rc.LC = LC_PK
		where u1_LD is null
	) b
	group by OH, U1_Code, ServerCode, CompanyCode, LCC, LC, LD

	union all

	-- test hosted storage usage
	select OH, U1_Code, ServerCode, CompanyCode, LCC, LC, LD, U1_UnitCount = SUM(U1_UnitCount), L7_PK = null, L7_Description = null, L7_Order = null
	from
	(
		select 
			db.OH, 
			U1_Code,
			ServerCode = LD_ServerCode, 
			CompanyCode = LC_CompanyCode, 
			LCC = null, 
			db.LC, 
			db.LD,
			U1_UnitCount
		from 
			Usage
			join @TestDatabase db on db.LD = U1_LD
			join dbo.LicenceCompany on db.LC = LC_PK
		where U1_Code = 'HOS'
	) c
	group by OH, U1_Code, ServerCode, CompanyCode, LCC, LC, LD

) u
join dbo.OrgHeader on OH_PK = OH
ORDER BY
	OH_FullName, U1_Code, ServerCode, CompanyCode, L7_Order;

return;
end
", "drop function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region LoadStlChargeableUsage

		public const string LoadAllStlChargeableUsage = ObjectPrefix + "LoadAllStlChargeableUsage";

		/// <summary>
		/// Used by Billing STL > Generate Report to return all ClientChargeableUsage and related LicenceDatabase records.
		/// Ideally should take less than a minute.
		/// Unit tests are in DatabaseUsageSetTest.
		/// </summary>
		public static DatabaseViewAndRoutineCreateScript LoadAllStlChargeableUsageScript()
		{
			const string name = LoadAllStlChargeableUsage;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE PROCEDURE " + name + @"
	@PeriodStart datetime,
	@OrgPk uniqueidentifier,
	@EnterpriseCode varchar(3),
	@IncludeOnlySiteLive bit = 1,
	@AccumulateMonths int = 1,
	@CodeFilter varchar(1000) = null
with recompile
as
begin
	set nocount on;

	if @EnterpriseCode = '' set @EnterpriseCode = null;
	if @CodeFilter = '' set @CodeFilter = null;

	create table #DatabasePks(DatabasePk uniqueidentifier);

	if @OrgPk is not null
	begin
		insert #DatabasePks(DatabasePk)
		select GroupPk from EdiGetBillingGroups(@OrgPk, @PeriodStart, null)
		where IsOrg = 0
	end;

	create table #StlDatabases(
		LD_PK uniqueidentifier not null primary key clustered,
		LD_ServerCode varchar(3) not null,
		LD_DatabaseNumber int not null,
		LD_LE uniqueidentifier not null,
		LD_IsBilledPerCompany bit not null,
		LD_HostedLocation char(3) not null,
		IsMainDatabase bit not null,
		ParentPk uniqueidentifier null,
		EnterpriseCode varchar(3) not null,
		PHL_L6 uniqueidentifier null,
		AgreedLiveDate smalldatetime null,
		IsLive bit not null default(0),
		LD_LicenceType  varchar(3) not null,
		LD_Billable varchar(3) not null,
		LD_OH_WebAccessOrg uniqueidentifier null
	);

	-- Main databases
	insert #StlDatabases(LD_PK, LD_ServerCode, LD_DatabaseNumber, LD_LE, LD_IsBilledPerCompany, LD_HostedLocation, IsMainDatabase, EnterpriseCode, PHL_L6, LD_LicenceType, LD_Billable, LD_OH_WebAccessOrg)
	select LD_PK, LD_ServerCode, LD_DatabaseNumber, LD_LE, LD_IsBilledPerCompany, LD_HostedLocation, IsMainDatabase = 1, LE_EnterpriseCode, PHL_L6, LD_LicenceType, LD_Billable, LD_OH_WebAccessOrg
	from dbo.LicenceDatabase
	join dbo.LicenceEnterprise on LD_LE = LE_PK
	outer apply
	(
		select top 1 PHL_L6 from dbo.EdiPriceHeaderLink
		where PHL_LD = LD_PK and PHL_ValidFrom <= DATEADD(DAY, 15, @PeriodStart)
		order by PHL_ValidFrom desc
	) prices
	outer apply
	(
		select top 1 IsStl = 1 from dbo.LicenceHeader where LA_LD = LD_PK and LA_IsActive = 1 and LA_LicenceAdvStdOth = 'STL'
	) isStl
	where LD_IsActive = 1
		and LD_Product in ('ENT', 'CW1', 'CWN', 'CGW', 'PRW')
		and (LD_LicenceType = 'PRD' OR LD_LD_ParentDatabase IS NULL)
		and (@OrgPk is null or LD_PK in (select DatabasePk from #DatabasePks))
		and (ISNULL(IsStl, 0) = 1 or PHL_L6 is not null)
		and (@EnterpriseCode is null or LE_EnterpriseCode = @EnterpriseCode)
		and LE_IsInternal = 0
		option (recompile);

	-- Test databases
	insert #StlDatabases(LD_PK, LD_ServerCode, LD_DatabaseNumber, LD_LE, LD_IsBilledPerCompany, LD_HostedLocation, IsMainDatabase, EnterpriseCode, ParentPk, LD_LicenceType, LD_Billable, LD_OH_WebAccessOrg)
	select test.LD_PK, test.LD_ServerCode, test.LD_DatabaseNumber, test.LD_LE, test.LD_IsBilledPerCompany, test.LD_HostedLocation, IsMainDatabase = 0, prod.EnterpriseCode, ParentPk = test.LD_LD_ParentDatabase, test.LD_LicenceType, test.LD_Billable, test.LD_OH_WebAccessOrg
	from dbo.LicenceDatabase test
	join #StlDatabases prod on test.LD_LD_ParentDatabase = prod.LD_PK and prod.LD_LicenceType = 'PRD'
	where test.LD_IsActive = 1
		and test.LD_Billable != 'N';

	-- Agreed live
	update #StlDatabases
	set AgreedLiveDate = LA_AgreedLiveDate
		, IsLive = case when LA_AgreedLiveDate < DATEADD(DAY, 15, @PeriodStart) then 1 else 0 end
	from #StlDatabases
	join
	(
		select LA_LD, LA_AgreedLiveDate = min(LA_AgreedLiveDate)
		from dbo.LicenceHeader
		where LA_IsActive = 1 and LA_AgreedLiveDate is not null
		group by LA_LD
	) SiteLive on LD_PK = LA_LD;

	-- Agreed live continued.
	-- It is only mandatory for production database.
	-- Many test databases have blank (but some have other installation date columns with values)
	-- Use production value as fallback.
	update test
	set AgreedLiveDate = prod.AgreedLiveDate
		, IsLive = prod.IsLive
	from #StlDatabases test
	join #StlDatabases prod on test.ParentPk = prod.LD_PK
	where test.AgreedLiveDate is null and prod.IsLive = 1

	if (@IncludeOnlySiteLive = 1)
		-- remove non-live DB's and their children
		delete from #StlDatabases where IsLive = 0 or ParentPK in (select LD_PK from #StlDatabases where IsLive = 0)

	-- We only want per-database price items for ODM usage (language packs etc)
	select distinct PriceAndUsageCodes.L7_L6, PriceAndUsageCodes.L7_Category, PriceAndUsageCodes.L7_Code
	into #PerDatabaseCodes
	from dbo.ClientLicencePriceItem
	cross apply
	(
		select L7_L6, L7_Category, L7_Code
		union all
		select L7_L6, PUM_UsageCategory, PUM_UsageCode from dbo.EdiPriceUsageMapping
		where PUM_L6 = L7_L6 and PUM_PriceCategory = L7_Category and PUM_PriceCode = L7_Code
	) PriceAndUsageCodes
	where ClientLicencePriceItem.L7_L6 in (select PHL_L6 from #StlDatabases where PHL_L6 is not null)
		and ClientLicencePriceItem.L7_Code != ''
		--please change PriceList.GetUsageCodesForPreview() accordingly
		and L7_FeeType in ('DBU', 'SHU', 'DAT', 'VDF', 'DAL', 'DAZ', 'DAC', 'COU', 'UCB') 

	-- Databases with a bill for a previous accumulated month
	create table #WasBilled (BU9_LD uniqueidentifier not null, BU9_PeriodStart date not null, MaxBillCreateTimeUtc smalldatetime not null)
	create unique clustered index Idx on #WasBilled(BU9_LD, BU9_PeriodStart)

	if (@AccumulateMonths > 0)
	begin
		insert #WasBilled(BU9_LD, BU9_PeriodStart, MaxBillCreateTimeUtc)
		select BU9_LD, BU9_PeriodStart, MaxBillCreateTimeUtc = max(AH_SystemCreateTimeUtc)
		from dbo.EdiBilledUsage 
		JOIN dbo.AccTransactionHeader on BU9_AH_Invoice = AH_PK 
		join #StlDatabases on BU9_LD = LD_PK
		where AH_IsCancelled = 0
			and BU9_PeriodStart < @PeriodStart and BU9_PeriodStart >= DATEADD(MONTH, -@AccumulateMonths, @PeriodStart)
		group by BU9_LD, BU9_PeriodStart
		option (recompile);
	end;

	-- Chargeable usages
	select ClientChargeableUsage.*
	from dbo.ClientChargeableUsage
	join #StlDatabases on U1_LD = LD_PK and IsLive >= @IncludeOnlySiteLive
	left join #PerDatabaseCodes on PHL_L6 = L7_L6 and U1_Code = 'ODM' and U1_SubCode = L7_Code
	where 1=1
		and U1_ManuallyProcessed = 0
		and U1_Code != 'PUR'
		and U1_PeriodStart = @PeriodStart
		and (U1_Code != 'ODM' or #PerDatabaseCodes.L7_L6 is not null)
		and (@CodeFilter is null or U1_Code in (select value from SplitStringToTable(@CodeFilter, DEFAULT)))
	union all
	-- Accumulate previous usage if the database had at least something billed in the month,
	-- and the usage was created after the invoice,
	-- and the fee type is not per database.
	-- If nothing was billed then we don't accumulate. They should be billed for that month separately.
	select ClientChargeableUsage.*
	from dbo.ClientChargeableUsage
	join #StlDatabases on U1_LD = LD_PK and IsLive >= @IncludeOnlySiteLive
	left join #PerDatabaseCodes ODMPerDbCode on PHL_L6 = ODMPerDbCode.L7_L6 and U1_Code = 'ODM' and U1_SubCode = ODMPerDbCode.L7_Code
	left join #PerDatabaseCodes PerDbCodes on PHL_L6 = PerDbCodes.L7_L6 and U1_Code = PerDbCodes.L7_Category and U1_SubCode = PerDbCodes.L7_Code
	join #WasBilled on #WasBilled.BU9_PeriodStart = U1_PeriodStart and U1_LD = #WasBilled.BU9_LD and U1_SystemCreateTimeUtc > MaxBillCreateTimeUtc
	LEFT JOIN dbo.AccTransactionHeader ON U1_AH_Invoice = AH_PK
	where @AccumulateMonths > 0
		and U1_ManuallyProcessed = 0
		and U1_Code not in ('PUR', 'STL')
		and U1_PeriodStart < @PeriodStart and U1_PeriodStart >= DATEADD(MONTH, -@AccumulateMonths, @PeriodStart)
		and (U1_Code != 'ODM' or ODMPerDbCode.L7_L6 is not null)
		and PerDbCodes.L7_L6 is null
		and (@CodeFilter is null or U1_Code in (select value from SplitStringToTable(@CodeFilter, DEFAULT)))
		and ISNULL(AH_IsCancelled, 1) = 1
		option (recompile);

	select * from #StlDatabases;

	drop table #DatabasePks;
	drop table #StlDatabases;
	drop table #WasBilled;
	drop table #PerDatabaseCodes;
end
", "DROP PROCEDURE " + name, DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion

		#region LoadBilledUsageScript

		public const string LoadBilledUsage = ObjectPrefix + "LoadBilledUsage";

		public static DatabaseViewAndRoutineCreateScript LoadBilledUsageScript()
		{
			const string name = LoadBilledUsage;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE PROCEDURE " + name + @"
(
	@InvoicePostDate int
)
WITH RECOMPILE
AS
BEGIN
	SET NOCOUNT ON;

	declare @PostDate date = CONVERT(date, CAST(@InvoicePostDate as char(6)) + '01',112) 

IF OBJECT_ID('tempdb..#Usages') IS NOT NULL DROP TABLE #Usages;
IF OBJECT_ID('tempdb..#Discounts') IS NOT NULL DROP TABLE #Discounts;

select	PayingOrg = payingParty.OH_Code, AG_AccountNum, AG_Description, GB_Code, AH_PostDate, BU9_PeriodStart, BU9_UnitCount, PriceCategory, BU9_PriceCode, BU9_UnitPrice, BU9_PriceCurrency, BU9_LocalAmountPreDiscount, 
		BU9_LocalAmountPostDiscount, BU9_LocalProcessingAmount, BU9_TransactionAmountPreDiscount, BU9_TransactionAmountPostDiscount, BU9_TransactionProcessingAmount, BU9_BillingModel, L7_Order, L7_FeeType, 
		L7_Price, L7_ParentCode, L7_WebParentCode, L7_LicenceUnits, L7_Description, L7_DiscountChargeCode, 
		LE_EnterpriseCode, UsingOrg = usingParty.OH_Code, AH_IsCancelled, AH_TransactionNum, AH_RX_NKTransactionCurrency, 
		BU9_UsageCode, BU9_UsageSubCode, AC_Code, BU9_PK, PayEnt
into #Usages
from
(
	-- things with a price item
	select EdiBilledUsage.*,
		L7_Order, L7_FeeType, L7_Price, PriceCategory = L7_Category, L7_ParentCode, L7_WebParentCode, L7_LicenceUnits, L7_Description, L7_DiscountChargeCode, 
		OH_PK = isnull(LicenceCompany.LC_OH, v.LC_OH),
		LE_PK = isnull(LicenceCompany.LC_LE, LD_LE)
	From dbo.EdiBilledUsage
		Join dbo.ClientLicencePriceItem on BU9_L7 = L7_PK
		left join dbo.LicenceCompany on LicenceCompany.LC_PK = BU9_LC
		left join dbo.LicenceDatabase on bu9_ld = ld_pk
		left join dbo.EdiViewLicenceDatabaseOwner v on v.LD_PK = LicenceDatabase.LD_PK
	Where 1=1
	union

	-- things with a database and no price item such as database fees
	select EdiBilledUsage.*,
		L7_Order = 0, L7_FeeType = '', L7_Price = BU9_UnitPrice, PriceCategory = '', L7_ParentCode = '', L7_WebParentCode = '' ,L7_LicenceUnits = 0, L7_Description = '', L7_DiscountChargeCode = '',
		OH_PK = LC_OH,
		LE_PK = LD_LE
	From dbo.EdiBilledUsage
		join dbo.LicenceDatabase on bu9_ld = ld_pk
		join dbo.EdiViewLicenceDatabaseOwner v on v.LD_PK = LicenceDatabase.LD_PK
	Where 1=1
		and BU9_L7 is null

	union

	-- things with no database and no price item such as old-style org fees
	select EdiBilledUsage.*,
		L7_Order = 0, L7_FeeType = '', L7_Price = BU9_UnitPrice, PriceCategory = '', L7_ParentCode = '', L7_WebParentCode = '' ,L7_LicenceUnits = 0, L7_Description = '', L7_DiscountChargeCode = '',
		OH_PK = AH_OH,
		LE_PK = LC_LE
	From dbo.EdiBilledUsage
		Join dbo.AccTransactionHeader on AH_PK = BU9_AH_Invoice
		left join dbo.LicenceCompany on LC_OH = AH_OH
	Where 1=1
		and BU9_L7 is null
		and BU9_LD is null
) a
join dbo.OrgHeader usingParty on usingParty.OH_PK = a.OH_PK
left join dbo.LicenceEnterprise on LicenceEnterprise.LE_PK = a.LE_PK
Join dbo.AccTransactionHeader on AH_PK = BU9_AH_Invoice
join dbo.OrgHeader payingParty on payingParty.OH_PK = AH_OH
left join 
(
       select PayOrgPk = payCo.LC_OH, PayEnt = payLicEnt.LE_EnterpriseCode
       from dbo.LicenceCompany payCo
       join dbo.LicenceEnterprise payLicEnt on payCo.LC_LE = payLicEnt.LE_PK
) PayingEnt on AH_OH = PayingEnt.PayOrgPk
join dbo.GlbBranch on AH_GB = GB_PK
left join dbo.AccChargeCode on BU9_AC_AmountChargeCode = AC_PK
left join dbo.AccGLHeader on AC_AG_RevenueAccount = AG_PK
where AH_PostDate >= @PostDate;

select distinct BU9_PK,
	   COALESCE('STL.' + PHD_Name, 'ODPL.' + BD9_Type, '') BD9_Calc_DiscountName,
	   ISNULL(BD9_Percent, 0) BD9_Percent,
	   ISNULL(BD9_TransactionAmount, 0) BD9_TransactionAmount
into #Discounts
from #Usages
join dbo.EdiBilledDiscount on BD9_BU9_Usage = BU9_PK
left join dbo.EdiPriceHeaderDiscount on BD9_PHD_Discount = PHD_PK;

declare @DiscountNames table ( DiscountName NVARCHAR(100) );
insert into @DiscountNames (DiscountName) select distinct BD9_Calc_DiscountName from #Discounts where BD9_Calc_DiscountName <> '';

if NOT EXISTS (select * from @DiscountNames)
begin
	select PayingOrg, AG_AccountNum, AG_Description, GB_Code, AH_PostDate, BU9_PeriodStart, BU9_UnitCount, PriceCategory, BU9_PriceCode, BU9_UnitPrice, BU9_PriceCurrency, BU9_LocalAmountPreDiscount, 
		BU9_LocalAmountPostDiscount, BU9_LocalProcessingAmount, BU9_TransactionAmountPreDiscount, BU9_TransactionAmountPostDiscount, BU9_TransactionProcessingAmount, BU9_BillingModel, L7_Order, L7_FeeType, 
		L7_Price, L7_ParentCode, L7_WebParentCode, L7_LicenceUnits, L7_Description, L7_DiscountChargeCode, 
		LE_EnterpriseCode, UsingOrg, AH_IsCancelled, AH_TransactionNum, AH_RX_NKTransactionCurrency, 
		BU9_UsageCode, BU9_UsageSubCode, AC_Code, PayEnt
		from #Usages
		order by LE_EnterpriseCode, BU9_PeriodStart, UsingOrg, L7_Order;
	return;
end

declare @colNames nvarchar(max);
select @colNames = STRING_AGG(CONVERT(NVARCHAR(max), quotename(DiscountName)), ',') from @DiscountNames;

declare @colNamesWithNullCheck nvarchar(max);
select @colNamesWithNullCheck = 
STRING_AGG(CONVERT(NVARCHAR(max), CONCAT('isnull(p.', quotename(DiscountName), ', 0) ', quotename(DiscountName + '%') +
', isnull(a.', quotename(DiscountName), ', 0) ', quotename(DiscountName))), ', ') WITHIN GROUP (order by DiscountName)
from @DiscountNames;
 
declare @query nvarchar(max) = 
'
select PayingOrg, AG_AccountNum, AG_Description, GB_Code, AH_PostDate, BU9_PeriodStart, BU9_UnitCount, PriceCategory, BU9_PriceCode, BU9_UnitPrice, BU9_PriceCurrency, BU9_LocalAmountPreDiscount, 
	BU9_LocalAmountPostDiscount, BU9_LocalProcessingAmount, BU9_TransactionAmountPreDiscount, BU9_TransactionAmountPostDiscount, BU9_TransactionProcessingAmount, BU9_BillingModel, L7_Order, L7_FeeType, 
	L7_Price, L7_ParentCode, L7_WebParentCode, L7_LicenceUnits, L7_Description, L7_DiscountChargeCode, 
	LE_EnterpriseCode, UsingOrg, AH_IsCancelled, AH_TransactionNum, AH_RX_NKTransactionCurrency, 
	BU9_UsageCode, BU9_UsageSubCode, AC_Code, PayEnt, ' + @colNamesWithNullCheck + ' 
from #Usages Usages
left join
(
	select BU9_PK, ' +  @colNames + ' 
	from (select BU9_PK, BD9_Calc_DiscountName, BD9_Percent from #Discounts) tb1
	pivot
	(
		sum(BD9_Percent)
		for BD9_Calc_DiscountName in
		( ' + @colNames + ' )
	)
	as pvt1
) p on Usages.BU9_PK = p.BU9_PK
left join 
(
	select BU9_PK, ' +  @colNames + ' 
	from (select BU9_PK, BD9_Calc_DiscountName, BD9_TransactionAmount from #Discounts) tb1
	pivot
	(
		sum(BD9_TransactionAmount)
		for BD9_Calc_DiscountName in
		( ' + @colNames + ' )
	)
	as pvt1
) a on Usages.BU9_PK = a.BU9_PK
order by LE_EnterpriseCode, BU9_PeriodStart, UsingOrg, L7_Order;

';

execute(@query);

END
", "DROP PROCEDURE " + name, DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion

		#region GetStlUsage (STL)

		public const string GetStlUsageSummary = ObjectPrefix + "GetStlUsageSummary";

		/// <summary>
		/// STL usage summary for MyAccount. Returns the list of usage.
		/// </summary>
		/// <returns></returns>
		public static DatabaseViewAndRoutineCreateScript GetStlUsageSummaryScript()
		{
			const string name = GetStlUsageSummary;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@OrgPk UNIQUEIDENTIFIER, @PeriodStart DATETIME)
RETURNS @Result TABLE
(
	LD_PK UNIQUEIDENTIFIER NOT NULL,
	LD_ServerCode VARCHAR(3) NOT NULL,
	LE_EnterpriseCode VARCHAR(3) NOT NULL,
	L7_PK UNIQUEIDENTIFIER NOT NULL,
	L7_Description NVARCHAR(500) NOT NULL,
	L7_Order SMALLINT NOT NULL,
	LCC_PK UNIQUEIDENTIFIER,
	LCC_Code VARCHAR(3) NOT NULL,
	U1_Code VARCHAR(3) NOT NULL,
	U1_UnitCount INT NOT NULL
)
BEGIN

--Target Databases
DECLARE @DB TABLE
(
	LD_PK UNIQUEIDENTIFIER NOT NULL,
	L6_PK UNIQUEIDENTIFIER NOT NULL,
	L6_LC UNIQUEIDENTIFIER NOT NULL,
	LD_ServerCode VARCHAR(3) NOT NULL,
	LE_EnterpriseCode VARCHAR(3) NOT NULL,
	MainPk UNIQUEIDENTIFIER NOT NULL,
	IsMain BIT NOT NULL,
	IsProduction BIT NOT NULL,
	L6_TestDbPriceCode varchar(3) not null default('')
);

-- 1. Production Databases
INSERT @DB (LD_PK, L6_PK, L6_LC, LD_ServerCode, LE_EnterpriseCode, MainPk, IsMain, IsProduction, L6_TestDbPriceCode)
SELECT LD_PK, L6_PK, L6_LC, LD_ServerCode, LE_EnterpriseCode, LD_PK, CAST(1 AS BIT), IsProduction, L6_TestDbPriceCode
FROM
(
	SELECT DISTINCT LD_PK, LD_ServerCode, LE_EnterpriseCode, IsProduction = CASE WHEN LD_LicenceType = 'PRD' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
	FROM
		EdiGetBillingGroups(@OrgPk, @PeriodStart, null)
		JOIN dbo.LicenceDatabase ON LD_PK = GroupPk
		JOIN dbo.LicenceEnterprise ON LD_LE = LE_PK
	WHERE
		IsOrg = 0
		AND IsFromGroup = 0
		AND LD_Product IN ('ENT', 'CW1', 'CWN', 'CGW', 'PRW')
) a
CROSS APPLY
(
	SELECT TOP 1 PHL_L6
	FROM dbo.EdiPriceHeaderLink
	WHERE PHL_LD = LD_PK and PHL_ValidFrom <= @PeriodStart AND (PHL_ValidTo IS NULL OR PHL_ValidTo > @PeriodStart)
	ORDER BY PHL_ValidFrom DESC
) p
JOIN dbo.ClientLicencePriceHeader ON PHL_L6 = L6_PK
WHERE L6_SystemCode = 'STL';

-- 2. Test database
INSERT @DB (LD_PK, L6_PK, L6_LC, LD_ServerCode, LE_EnterpriseCode, MainPk, IsMain, IsProduction)
SELECT testDb.LD_PK, L6_PK, L6_LC, testDb.LD_ServerCode, LE_EnterpriseCode, productionDb.LD_PK, 0, 0
FROM @db productionDb
JOIN dbo.LicenceDatabase testDb ON productionDb.LD_PK = testDb.LD_LD_ParentDatabase
WHERE productionDb.IsProduction = 1 AND testDb.LD_Product in ('ENT', 'CW1', 'CWN', 'CGW', 'PRW');

--Query Usages
DECLARE @Usage TABLE
(
    U1_PK UNIQUEIDENTIFIER,
	LD_PK UNIQUEIDENTIFIER,
	LCC_PK UNIQUEIDENTIFIER,
	U1_Code VARCHAR(3),
	U1_SubCode VARCHAR(50),
	PriceCategory VARCHAR(3),
	PriceCode VARCHAR(50),
	U1_UnitCount INT,
	L6_PK UNIQUEIDENTIFIER,
	L7_FeeType VARCHAR(3),
	L7_PK UNIQUEIDENTIFIER,
	GroupingFlag BIT NOT NULL DEFAULT(0)
);

-- 1. Raw usages
INSERT INTO @Usage(U1_PK, LD_PK, LCC_PK, U1_Code, U1_SubCode, U1_UnitCount, L6_PK)
SELECT U1_PK, LD_PK, U1_LCC, U1_Code, U1_SubCode, U1_UnitCount, L6_PK
FROM dbo.ClientChargeableUsage
JOIN @DB on LD_PK = U1_LD
WHERE U1_PeriodStart = @PeriodStart and U1_UnitCount > 0 
	AND ( IsProduction = 1
		OR (IsProduction = 0 and U1_Code in ('HOS', 'HDA'))) -- Test usage - only includes hosted storage and data access since that is the only test usage we charge
	AND (U1_Code <> 'SVC' OR U1_SubCode <> L6_TestDbPriceCode OR U1_SubCode = ''); -- exclude test system fee

-- 2. Special conditions before mapping
UPDATE @Usage SET U1_SubCode = U1_Code WHERE U1_SubCode = '';

-- 3. Map usages/prices
UPDATE U
SET PriceCategory = COALESCE(PUM_PriceCategory, U1_Code), PriceCode = COALESCE(PUM_PriceCode, U1_SubCode)
FROM @Usage U
LEFT JOIN dbo.EdiPriceUsageMapping ON PUM_L6 = L6_PK AND PUM_UsageCategory = U1_Code and PUM_UsageCode = U1_SubCode;

-- 4. Special conditions after mapping
UPDATE @Usage SET U1_Code = U1_SubCode WHERE U1_Code = 'CPT';

-- 5.
-- If the usages contain mapped PUM_PriceCategory/PUM_PriceCode already, 
-- should only use PUM_PriceCategory/PUM_PriceCode instead of real U1_Code/U1_SubCode, 
-- so the total unit count can be combined correctly.
UPDATE U1
SET U1_Code = PriceCategory, U1_SubCode = PriceCode
FROM @Usage U1
WHERE (U1_Code <> PriceCategory OR U1_SubCode <> PriceCode) --mapped usages
AND EXISTS
(
	SELECT TOP 1 1 FROM @Usage U2
	WHERE U1.LD_PK = U2.LD_PK AND U1.L6_PK = U2.L6_PK 
	AND U1.PriceCategory = U2.U1_Code AND U1.PriceCode = U2.U1_SubCode
	AND U1.U1_PK <> U2.U1_PK
);

-- 6. Fee Type
UPDATE U
SET U.L7_FeeType = L7.L7_FeeType
FROM @Usage U
JOIN
(
	SELECT DISTINCT L7_L6, L7_Category, L7_Code, L7_FeeType FROM dbo.ClientLicencePriceItem WHERE L7_Code <> ''
) L7 ON L6_PK = L7_L6 AND L7_Category = PriceCategory AND L7_Code = PriceCode

-- 7. filter out named user usage unless this is charged per-database (StlBilling.CalculatePricing())
DELETE @Usage
WHERE (U1_Code = 'ODM' AND L7_FeeType NOT IN ('COU', 'DAT', 'VDF', 'DAL', 'DAZ', 'DAC', 'UCB'));

-- 8. The 'TRB BREAK' type should only show '[All]' on usage report, because it's determined by the sum of usages from all LCC.
UPDATE @Usage
SET LCC_PK = NULL 
FROM @Usage
WHERE L7_FeeType = 'TRB';

-- 9. Grouping
UPDATE @Usage SET GroupingFlag = 0;
INSERT @Usage(LD_PK, LCC_PK, U1_Code, U1_SubCode, PriceCategory, PriceCode, U1_UnitCount, L6_PK, L7_FeeType, GroupingFlag)
SELECT LD_PK, LCC_PK, U1_Code, U1_SubCode, PriceCategory, PriceCode, U1_UnitCount = SUM(U1_UnitCount), L6_PK, L7_FeeType, GroupingFlag = 1
FROM @Usage GROUP BY LD_PK, LCC_PK, U1_Code, U1_SubCode, PriceCategory, PriceCode, L6_PK, L7_FeeType;
DELETE @Usage WHERE GroupingFlag = 0;

-- 10. UCB
UPDATE Usage
SET Usage.L7_PK = L7.L7_PK
FROM @Usage Usage
JOIN 
(
	SELECT GPC_LD = LD_PK, GPC_AvgUsersPerCountry = ISNULL(SUM(CASE WHEN U1_SubCode = 'USR' THEN U1_UnitCount ELSE 0 END) / NULLIF(SUM(CASE WHEN U1_SubCode = 'GPC' THEN U1_UnitCount ELSE 0 END), 0), 0)
	FROM @Usage
	WHERE (U1_Code = 'STL' AND U1_SubCode = 'USR') OR (U1_Code = 'ODM' AND U1_SubCode = 'GPC')
	GROUP by LD_PK
) AverageUsersPerCountry ON Usage.LD_PK = GPC_LD
OUTER APPLY
(
	SELECT TOP 1 L7_PK FROM dbo.ClientLicencePriceItem 
	WHERE L7_FeeType = 'UCB' AND L7_L6 = Usage.L6_PK AND L7_Category = Usage.PriceCategory AND L7_Code = Usage.PriceCode AND GPC_AvgUsersPerCountry > L7_UnitBreak
	ORDER BY L7_UnitBreak DESC
) L7
WHERE Usage.L7_FeeType = 'UCB';

-- 11. TRB
UPDATE Usage
SET Usage.L7_PK = L7.L7_PK
FROM @Usage Usage
OUTER APPLY
(
	SELECT TOP 1 L7_PK FROM dbo.ClientLicencePriceItem 
	WHERE L7_FeeType = 'TRB' AND L7_L6 = Usage.L6_PK AND L7_Category = Usage.PriceCategory AND L7_Code = Usage.PriceCode AND U1_UnitCount > L7_UnitBreak
	ORDER BY L7_UnitBreak DESC
) L7
WHERE Usage.L7_FeeType = 'TRB';

-- 12. COT
UPDATE Usage
SET Usage.L7_PK = L7.L7_PK
FROM @Usage Usage
OUTER APPLY
(
	SELECT TOP 1 L7_PK FROM dbo.ClientLicencePriceItem
	JOIN dbo.ClientLicencePriceHeader H ON L7_L6 = H.L6_PK
	JOIN dbo.ClientCompany CC ON CC.LCC_PK = Usage.LCC_PK
	JOIN dbo.EdiGetCountryTierPriceCodeMappings() ON COT_PriceCode = Usage.PriceCode AND COT_SystemCode = L6_SystemCode AND CC.LCC_RN_NKCountryCode = COT_CountryCode
	WHERE
		L7_FeeType = 'COT'
		AND L7_L6 = Usage.L6_PK
		AND L7_Category = Usage.PriceCategory
		AND L7_Code = Usage.PriceCode
		AND L7_CountryTierCode = COT_CountryTierCode
) L7
WHERE Usage.L7_FeeType = 'COT';

-- 13. Other L7
UPDATE Usage
SET Usage.L7_PK = L7.L7_PK
FROM @Usage Usage
JOIN dbo.ClientLicencePriceItem L7 ON L7.L7_L6 = Usage.L6_PK AND L7_Category = Usage.PriceCategory AND Usage.PriceCode = L7.L7_Code
WHERE Usage.L7_PK IS NULL AND Usage.L7_FeeType NOT IN ('UCB', 'TRB', 'COT')

DELETE @Usage WHERE L7_PK IS NULL;

-- 14. Output
INSERT @Result (LD_PK, LD_ServerCode, LE_EnterpriseCode, L7_PK, L7_Description, L7_Order, LCC_PK, LCC_Code, U1_Code, U1_UnitCount)
SELECT LD.LD_PK, LD.LD_ServerCode, LE.LE_EnterpriseCode, L7.L7_PK, L7_Description, L7_Order, Usage.LCC_PK, LCC_Code = LTRIM(ISNULL(LCC_Code, '')), U1_Code, U1_UnitCount = SUM(U1_UnitCount)
FROM @Usage Usage
JOIN dbo.LicenceDatabase LD ON Usage.LD_PK = LD.LD_PK
JOIN dbo.LicenceEnterprise LE ON LD.LD_LE = LE.LE_PK
LEFT JOIN dbo.ClientCompany LCC ON Usage.LCC_PK = LCC.LCC_PK
JOIN dbo.ClientLicencePriceItem L7 ON Usage.L7_PK = L7.L7_PK
GROUP BY LD.LD_PK, LD.LD_ServerCode, LE.LE_EnterpriseCode, L7.L7_PK, L7_Description, L7_Order, Usage.LCC_PK, LCC_Code, U1_Code;
  
-- Global pricelist usage
DECLARE @BillingDbCodes TABLE 
(
	Category VARCHAR(3) NOT NULL, 
	PriceItemCode VARCHAR(3) NOT NULL, 
	PriceHeaderCode VARCHAR(3)
);

DECLARE @BillingDbCodesXml XML = (SELECT cast(SD_BinaryValue AS NVARCHAR(MAX)) FROM dbo.StmData WHERE SD_Name = 'BillingDbUsageCodes');
INSERT INTO @BillingDbCodes(Category, PriceItemCode, PriceHeaderCode)
SELECT Category, PriceItemCode, PriceHeaderCode
FROM
(
	SELECT 
		Category = NodeTable.Item.value('Category[1]', 'varchar(3)'),
		PriceItemCode = NodeTable.Item.value('PriceItemCode[1]', 'varchar(3)'),
		PriceHeaderCode = NodeTable.Item.value('PriceHeaderCode[1]', 'varchar(3)')
	FROM  
		@BillingDbCodesXml.nodes('/ArrayOfBillingDbUsageCodes/BillingDbUsageCodes') AS NodeTable(Item)
) a 
	-- remove billing db codes that were already matched on STL price list
LEFT JOIN @Usage u on u.U1_Code = Category AND u.U1_SubCode = PriceItemCode
WHERE PriceHeaderCode != 'STL' AND u.U1_Code IS NULL;

DECLARE @GlobalUsage TABLE
(
    U1_PK UNIQUEIDENTIFIER,
	LD_PK UNIQUEIDENTIFIER,
	LCC_PK UNIQUEIDENTIFIER,
	U1_Code VARCHAR(3),
	U1_SubCode VARCHAR(50),
	PriceCategory VARCHAR(3),
	PriceCode VARCHAR(50),
	U1_UnitCount INT,
	L6_PK UNIQUEIDENTIFIER,
	L7_FeeType VARCHAR(3),
	L7_PK UNIQUEIDENTIFIER,
	GroupingFlag BIT NOT NULL DEFAULT(0)
);

-- 1. Raw usages
INSERT @GlobalUsage(U1_PK, LD_PK, LCC_PK, U1_Code, U1_SubCode, U1_UnitCount, L6_PK)
SELECT U1_PK, LD_PK, U1_LCC, U1_Code, U1_SubCode, U1_UnitCount, L6_PK
FROM
(
	SELECT U1_PK, LD_PK, U1_LCC, U1_Code, U1_SubCode, U1_UnitCount, PriceHeaderCode, L6_LC
	FROM dbo.ClientChargeableUsage
	JOIN @DB db ON LD_PK = U1_LD
	JOIN @BillingDbCodes ON U1_Code = Category AND (U1_SubCode = PriceItemCode OR PriceItemCode = '')
	WHERE U1_PeriodStart = @PeriodStart AND U1_UnitCount > 0
) U1
CROSS APPLY
(
	SELECT TOP 1 L6_PK FROM dbo.ClientLicencePriceHeader
	WHERE ClientLicencePriceHeader.L6_LC = U1.L6_LC
		AND L6_SystemCode = U1.PriceHeaderCode
		AND L6_ValidFrom <= @PeriodStart
		AND (L6_ValidTo IS NULL OR L6_ValidTo > @PeriodStart)
	ORDER BY L6_ValidFrom DESC
) GlobalL6;

-- 2. Map usages/prices
UPDATE U
SET PriceCategory = COALESCE(PUM_PriceCategory, U1_Code), PriceCode = COALESCE(PUM_PriceCode, U1_SubCode)
FROM @GlobalUsage U
LEFT JOIN dbo.EdiPriceUsageMapping ON PUM_L6 = L6_PK AND PUM_UsageCategory = U1_Code and PUM_UsageCode = U1_SubCode;

-- 3.
-- If the usages contain mapped PUM_PriceCategory/PUM_PriceCode already, 
-- should only use PUM_PriceCategory/PUM_PriceCode instead of real U1_Code/U1_SubCode, 
-- so the total unit count can be combined correctly.
UPDATE U1
SET U1_Code = PriceCategory, U1_SubCode = PriceCode
FROM @GlobalUsage U1
WHERE (U1_Code <> PriceCategory OR U1_SubCode <> PriceCode) --mapped usages
AND EXISTS
(
	SELECT TOP 1 1 FROM @GlobalUsage U2
	WHERE U1.LD_PK = U2.LD_PK AND U1.L6_PK = U2.L6_PK 
	AND U1.PriceCategory = U2.U1_Code AND U1.PriceCode = U2.U1_SubCode
	AND U1.U1_PK <> U2.U1_PK
);

-- 4. Fee Type
UPDATE U
SET U.L7_FeeType = L7.L7_FeeType
FROM @GlobalUsage U
JOIN
(
	SELECT DISTINCT L7_L6, L7_Category, L7_Code, L7_FeeType FROM dbo.ClientLicencePriceItem WHERE L7_Code <> ''
) L7 ON L6_PK = L7_L6 AND L7_Category = PriceCategory AND L7_Code = PriceCode

-- 5. The 'TRB BREAK' type should only show '[All]' on usage report, because it's determined by the sum of usages from all LCC.
UPDATE @GlobalUsage
SET LCC_PK = NULL 
FROM @GlobalUsage
WHERE L7_FeeType = 'TRB';

-- 6. Grouping
UPDATE @GlobalUsage SET GroupingFlag = 0;
INSERT @GlobalUsage(LD_PK, LCC_PK, U1_Code, U1_SubCode, PriceCategory, PriceCode, U1_UnitCount, L6_PK, L7_FeeType, GroupingFlag)
SELECT LD_PK, LCC_PK, U1_Code, U1_SubCode, PriceCategory, PriceCode, U1_UnitCount = SUM(U1_UnitCount), L6_PK, L7_FeeType, GroupingFlag = 1
FROM @GlobalUsage GROUP BY LD_PK, LCC_PK, U1_Code, U1_SubCode, PriceCategory, PriceCode, L6_PK, L7_FeeType;
DELETE @GlobalUsage WHERE GroupingFlag = 0;

-- 7. TRB
UPDATE Usage
SET Usage.L7_PK = L7.L7_PK
FROM @GlobalUsage Usage
OUTER APPLY
(
	SELECT TOP 1 L7_PK FROM dbo.ClientLicencePriceItem 
	WHERE L7_FeeType = 'TRB' AND L7_L6 = Usage.L6_PK AND L7_Category = Usage.PriceCategory AND L7_Code = Usage.PriceCode AND U1_UnitCount > L7_UnitBreak
	ORDER BY L7_UnitBreak DESC
) L7
WHERE Usage.L7_FeeType = 'TRB';

-- 8. Other L7
UPDATE Usage
SET Usage.L7_PK = L7.L7_PK
FROM @GlobalUsage Usage
JOIN dbo.ClientLicencePriceItem L7 ON L7.L7_L6 = Usage.L6_PK AND L7_Category = Usage.PriceCategory AND Usage.PriceCode = L7.L7_Code
WHERE Usage.L7_PK IS NULL AND Usage.L7_FeeType <> 'TRB'

DELETE @GlobalUsage WHERE L7_PK IS NULL;

-- 9. Output
INSERT @Result (LD_PK, LD_ServerCode, LE_EnterpriseCode, L7_PK, L7_Description, L7_Order, LCC_PK, LCC_Code, U1_Code, U1_UnitCount)
SELECT LD.LD_PK, LD.LD_ServerCode, LE.LE_EnterpriseCode, L7.L7_PK, L7_Description, L7_Order, Usage.LCC_PK, LCC_Code = LTRIM(ISNULL(LCC_Code, '')), U1_Code, U1_UnitCount = SUM(U1_UnitCount)
FROM @GlobalUsage Usage
JOIN dbo.LicenceDatabase LD ON Usage.LD_PK = LD.LD_PK
JOIN dbo.LicenceEnterprise LE ON LD.LD_LE = LE.LE_PK
LEFT JOIN dbo.ClientCompany LCC ON Usage.LCC_PK = LCC.LCC_PK
JOIN dbo.ClientLicencePriceItem L7 ON Usage.L7_PK = L7.L7_PK
GROUP BY LD.LD_PK, LD.LD_ServerCode, LE.LE_EnterpriseCode, L7.L7_PK, L7_Description, L7_Order, Usage.LCC_PK, LCC_Code, U1_Code;

-- Other price lists
DECLARE @abmPriceItemPk UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001'
INSERT @Result (LD_PK, LD_ServerCode, LE_EnterpriseCode, L7_PK, L7_Description, L7_Order, LCC_PK, LCC_Code, U1_Code, U1_UnitCount)
SELECT LD_PK, LD_ServerCode, LE_EnterpriseCode, @abmPriceItemPk, 'ABM Customs', 32763, LCC_PK, LCC_Code = ISNULL(LCC_Code, ''), U1_Code, U1_UnitCount = SUM(U1_UnitCount)
FROM
	dbo.ClientChargeableUsage
	JOIN @DB ON LD_PK = U1_LD
	LEFT JOIN dbo.ClientCompany ON U1_LCC = LCC_PK	
WHERE
	U1_Code = 'ABM'
	AND U1_PeriodStart = @PeriodStart 
	AND U1_UnitCount > 0
GROUP BY LD_PK, LD_ServerCode, LE_EnterpriseCode, LCC_PK, LCC_Code, U1_Code

DECLARE @hubPriceItemPk UNIQUEIDENTIFIER =  '00000000-0000-0000-0000-000000000003'
INSERT @Result (LD_PK, LD_ServerCode, LE_EnterpriseCode, L7_PK, L7_Description, L7_Order, LCC_PK, LCC_Code, U1_Code, U1_UnitCount)
SELECT LD_PK, LD_ServerCode, LE_EnterpriseCode, @hubPriceItemPk, 'eHub Interfaces - Transactional Fee', 32767, LCC_PK, LCC_Code = ISNULL(LCC_Code, ''), U1_Code, U1_UnitCount = SUM(U1_UnitCount)
FROM
	dbo.ClientChargeableUsage
	JOIN @DB ON LD_PK = U1_LD
	LEFT JOIN dbo.ClientCompany ON U1_LCC = LCC_PK	
WHERE
	U1_Code = 'CMP'
	AND U1_PeriodStart = @PeriodStart 
	AND U1_UnitCount > 0
GROUP BY LD_PK, LD_ServerCode, LE_EnterpriseCode, LCC_PK, LCC_Code, U1_Code

DECLARE @flightStatsPriceItemPk UNIQUEIDENTIFIER =  '00000000-0000-0000-0000-000000000005'
INSERT @Result (LD_PK, LD_ServerCode, LE_EnterpriseCode, L7_PK, L7_Description, L7_Order, LCC_PK, LCC_Code, U1_Code, U1_UnitCount)
SELECT LD_PK, LD_ServerCode, LE_EnterpriseCode, @flightStatsPriceItemPk, 'Air Waybill Automation', 32759, LCC_PK, LCC_Code = ISNULL(LCC_Code, ''), U1_Code, U1_UnitCount = SUM(U1_UnitCount)
FROM
	dbo.ClientChargeableUsage
	JOIN @DB ON LD_PK = U1_LD
	LEFT JOIN dbo.ClientCompany ON U1_LCC = LCC_PK	
WHERE
	U1_Code = 'FMS'
	AND U1_SubCode = 'FMS'
	AND U1_PeriodStart = @PeriodStart 
	AND U1_UnitCount > 0
GROUP BY LD_PK, LD_ServerCode, LE_EnterpriseCode, LCC_PK, LCC_Code, U1_Code

--CWN
INSERT @Result (LD_PK, LD_ServerCode, LE_EnterpriseCode, L7_PK, L7_Description, L7_Order, LCC_PK, LCC_Code, U1_Code, U1_UnitCount)
SELECT LD_PK, LD_ServerCode, LE_EnterpriseCode, L7_PK, L7_Description, L7_Order, LCC_PK, LCC_Code = ISNULL(LCC_Code, ''), U1_Code, U1_UnitCount = SUM(U1_UnitCount)
FROM
(
	SELECT  RN = ROW_NUMBER() OVER(PARTITION BY L7_Code ORDER BY L7_Order)
			, L7_PK, L7_Code, L7_Description, L7_Order
	FROM dbo.ClientLicencePriceItem
	WHERE L7_L6 = 
	(
		SELECT TOP 1 L6_PK FROM 
		dbo.ClientLicencePriceHeader
		WHERE L6_SystemCode = 'CWN'
			AND L6_ValidFrom <= @PeriodStart
			AND (L6_ValidTo IS NULL OR L6_ValidTo > @PeriodStart)
		ORDER BY L6_ValidFrom DESC
	)
	AND L7_Code != '' AND L7_Description != ''
) L7
JOIN dbo.ClientChargeableUsage ON U1_Code = 'STL' AND U1_SubCode = L7_Code
JOIN @DB ON LD_PK = U1_LD
LEFT JOIN dbo.ClientCompany ON U1_LCC = LCC_PK	
WHERE L7.RN = 1 -- the CWN L7 may have multiple rows with same code but different currency/direction
AND U1_PeriodStart = @PeriodStart 
AND U1_UnitCount > 0
GROUP BY LD_PK, LD_ServerCode, LE_EnterpriseCode, LCC_PK, LCC_Code, U1_Code, L7_PK, L7_Description, L7_Order;

RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		public const string GetStlGenericUsageSummary = ObjectPrefix + "GetStlGenericUsageSummary";

		public static DatabaseViewAndRoutineCreateScript GetStlGenericUsageSummaryScript()
		{
			const string name = GetStlGenericUsageSummary;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@OrgPk UNIQUEIDENTIFIER, @PeriodStart DATETIME, @Product VARCHAR(3))
RETURNS @Result TABLE
(
	LD_PK UNIQUEIDENTIFIER NOT NULL,
	LD_ServerCode VARCHAR(3) NOT NULL,
	LE_EnterpriseCode VARCHAR(3) NOT NULL,
	L7_PK UNIQUEIDENTIFIER NOT NULL,
	L7_Description NVARCHAR(500) NOT NULL,
	L7_Order SMALLINT NOT NULL,
	LCC_PK UNIQUEIDENTIFIER,
	LCC_Code VARCHAR(3) NOT NULL,
	U1_Code VARCHAR(3) NOT NULL,
	U1_UnitCount INT NOT NULL
)
BEGIN

--Target Databases
DECLARE @DB TABLE
(
	LD_PK UNIQUEIDENTIFIER NOT NULL
);

-- 1. Production Databases
INSERT @DB (LD_PK)
SELECT LD_PK
FROM
(
	SELECT DISTINCT LD_PK
	FROM
		EdiGetBillingGroups(@OrgPk, @PeriodStart, @Product)
		JOIN dbo.LicenceDatabase LD ON LD_PK = GroupPk
		JOIN dbo.LicenceEnterprise ON LD_LE = LE_PK
	WHERE
		IsOrg = 0
		AND IsFromGroup = 0
		AND LD_Product = @Product
		AND LD_LicenceType = 'PRD'
		AND NOT EXISTS -- The LD cannot be exclusively billed to another org.
		(
			SELECT L9_OH_InvoiceTo
			FROM
			(
				SELECT TOP 1 L9_OH_InvoiceTo
				FROM dbo.EdiViewLicenceDatabaseOwner v
				JOIN dbo.ClientInvoiceDelivery ON L9_LC = v.LC_PK
				WHERE v.LD_PK = LD.LD_PK
				AND (L9_SystemCode = 'ALL' OR L9_SystemCode = @Product)
				AND (L9_ServerCode = '' OR L9_ServerCode = LD.LD_ServerCode)
				AND L9_OH_InvoiceTo IS NOT NULL
				AND L9_IsBilled = 'Y'
				ORDER BY IIF(L9_SystemCode = @Product, 0, 1), IIF(L9_ServerCode = LD.LD_ServerCode, 0, 1)
			) t WHERE L9_OH_InvoiceTo != @OrgPk
		)
)a;

-- pricelist usage
DECLARE @PriceLists TABLE 
(
	Product VARCHAR(3) NOT NULL, 
	RawUsageCategory VARCHAR(3) NOT NULL, 
	PriceListCode VARCHAR(3) NOT NULL
);

DECLARE @SettingsXml XML = (SELECT cast(SD_BinaryValue AS NVARCHAR(MAX)) FROM dbo.StmData WHERE SD_Name = 'UsageBillingSettings');
INSERT INTO @PriceLists(Product, RawUsageCategory, PriceListCode)
SELECT ProductCode, RawUsageCategory, PriceListCode
FROM
(
	SELECT 
		ProductCode = NodeTable.Item.value('ProductCode[1]', 'varchar(3)'),
		RawUsageCategory = NodeTable.Item.value('RawUsageCategory[1]', 'varchar(3)'),
		PriceListCode = NodeTable.Item.value('PriceListCode[1]', 'varchar(3)')
	FROM  
		@SettingsXml.nodes('/UsageBillingSettings/Products/PriceLists') AS NodeTable(Item)
) R WHERE ProductCode = @Product;

DECLARE @GlobalUsage TABLE
(
    U1_PK UNIQUEIDENTIFIER,
	LD_PK UNIQUEIDENTIFIER,
	LCC_PK UNIQUEIDENTIFIER,
	U1_Code VARCHAR(3),
	U1_SubCode VARCHAR(50),
	PriceCategory VARCHAR(3),
	PriceCode VARCHAR(50),
	U1_UnitCount INT,
	L6_PK UNIQUEIDENTIFIER,
	L7_FeeType VARCHAR(3),
	L7_PK UNIQUEIDENTIFIER,
	GroupingFlag BIT NOT NULL DEFAULT(0)
);

-- 1. Raw usages
INSERT @GlobalUsage(U1_PK, LD_PK, LCC_PK, U1_Code, U1_SubCode, U1_UnitCount, L6_PK)
SELECT U1_PK, LD_PK, U1_LCC, U1_Code, U1_SubCode, U1_UnitCount, L6_PK
FROM
(
	SELECT U1_PK, LD_PK, U1_LCC, U1_Code, U1_SubCode, U1_UnitCount, PriceListCode
	FROM dbo.ClientChargeableUsage
	JOIN @DB db ON LD_PK = U1_LD
	JOIN @PriceLists priceLists ON U1_Code = RawUsageCategory
	WHERE U1_PeriodStart = @PeriodStart AND U1_UnitCount > 0
) U1
CROSS APPLY
(
	SELECT TOP 1 L6_PK FROM dbo.ClientLicencePriceHeader
	WHERE L6_SystemCode = U1.PriceListCode
		AND L6_ValidFrom <= @PeriodStart
		AND (L6_ValidTo IS NULL OR L6_ValidTo > @PeriodStart)
	ORDER BY L6_ValidFrom DESC
) GlobalL6;

-- 2. Map usages/prices
UPDATE U
SET PriceCategory = COALESCE(PUM_PriceCategory, U1_Code), PriceCode = COALESCE(PUM_PriceCode, U1_SubCode)
FROM @GlobalUsage U
LEFT JOIN dbo.EdiPriceUsageMapping ON PUM_L6 = L6_PK AND PUM_UsageCategory = U1_Code and PUM_UsageCode = U1_SubCode;

-- 3.
-- If the usages contain mapped PUM_PriceCategory/PUM_PriceCode already, 
-- should only use PUM_PriceCategory/PUM_PriceCode instead of real U1_Code/U1_SubCode, 
-- so the total unit count can be combined correctly.
UPDATE U1
SET U1_Code = PriceCategory, U1_SubCode = PriceCode
FROM @GlobalUsage U1
WHERE (U1_Code <> PriceCategory OR U1_SubCode <> PriceCode) --mapped usages
AND EXISTS
(
	SELECT TOP 1 1 FROM @GlobalUsage U2
	WHERE U1.LD_PK = U2.LD_PK AND U1.L6_PK = U2.L6_PK 
	AND U1.PriceCategory = U2.U1_Code AND U1.PriceCode = U2.U1_SubCode
	AND U1.U1_PK <> U2.U1_PK
);

-- 4. Fee Type
UPDATE U
SET U.L7_FeeType = L7.L7_FeeType
FROM @GlobalUsage U
JOIN
(
	SELECT DISTINCT L7_L6, L7_Category, L7_Code, L7_FeeType FROM dbo.ClientLicencePriceItem WHERE L7_Code <> ''
) L7 ON L6_PK = L7_L6 AND L7_Category = PriceCategory AND L7_Code = PriceCode

-- 5. The 'TRB BREAK' type should only show '[All]' on usage report, because it's determined by the sum of usages from all LCC.
UPDATE @GlobalUsage
SET LCC_PK = NULL 
FROM @GlobalUsage
WHERE L7_FeeType = 'TRB';

-- 6. Grouping
UPDATE @GlobalUsage SET GroupingFlag = 0;
INSERT @GlobalUsage(LD_PK, LCC_PK, U1_Code, U1_SubCode, PriceCategory, PriceCode, U1_UnitCount, L6_PK, L7_FeeType, GroupingFlag)
SELECT LD_PK, LCC_PK, U1_Code, U1_SubCode, PriceCategory, PriceCode, U1_UnitCount = SUM(U1_UnitCount), L6_PK, L7_FeeType, GroupingFlag = 1
FROM @GlobalUsage GROUP BY LD_PK, LCC_PK, U1_Code, U1_SubCode, PriceCategory, PriceCode, L6_PK, L7_FeeType;
DELETE @GlobalUsage WHERE GroupingFlag = 0;

-- 7. TRB
UPDATE Usage
SET Usage.L7_PK = L7.L7_PK
FROM @GlobalUsage Usage
OUTER APPLY
(
	SELECT TOP 1 L7_PK FROM dbo.ClientLicencePriceItem 
	WHERE L7_FeeType = 'TRB' AND L7_L6 = Usage.L6_PK AND L7_Category = Usage.PriceCategory AND L7_Code = Usage.PriceCode AND U1_UnitCount > L7_UnitBreak
	ORDER BY L7_UnitBreak DESC
) L7
WHERE Usage.L7_FeeType = 'TRB';

-- 8. Other L7
UPDATE Usage
SET Usage.L7_PK = L7.L7_PK
FROM @GlobalUsage Usage
JOIN dbo.ClientLicencePriceItem L7 ON L7.L7_L6 = Usage.L6_PK AND L7_Category = Usage.PriceCategory AND Usage.PriceCode = L7.L7_Code
WHERE Usage.L7_PK IS NULL AND Usage.L7_FeeType <> 'TRB'

DELETE @GlobalUsage WHERE L7_PK IS NULL;

-- 9. Output
INSERT @Result (LD_PK, LD_ServerCode, LE_EnterpriseCode, L7_PK, L7_Description, L7_Order, LCC_PK, LCC_Code, U1_Code, U1_UnitCount)
SELECT LD.LD_PK, LD.LD_ServerCode, LE.LE_EnterpriseCode, L7.L7_PK, L7_Description, L7_Order, Usage.LCC_PK, LCC_Code = LTRIM(ISNULL(LCC_Code, '')), U1_Code, U1_UnitCount = SUM(U1_UnitCount)
FROM @GlobalUsage Usage
JOIN dbo.LicenceDatabase LD ON Usage.LD_PK = LD.LD_PK
JOIN dbo.LicenceEnterprise LE ON LD.LD_LE = LE.LE_PK
LEFT JOIN dbo.ClientCompany LCC ON Usage.LCC_PK = LCC.LCC_PK
JOIN dbo.ClientLicencePriceItem L7 ON Usage.L7_PK = L7.L7_PK
GROUP BY LD.LD_PK, LD.LD_ServerCode, LE.LE_EnterpriseCode, L7.L7_PK, L7_Description, L7_Order, Usage.LCC_PK, LCC_Code, U1_Code;


RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		public const string GetBillingDbDetailedUsageQuery = "EdiLoadBillingDbDetailedUsage";

		/// <summary>
		/// Script to return detailed usage from the billing DB for a single price item and customer database
		/// </summary>
		/// <returns></returns>
		public static DatabaseViewAndRoutineCreateScript GetBillingDbDetailedUsageScript()
		{
			const string name = GetBillingDbDetailedUsageQuery;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period INT, @DatabaseId VARCHAR(50), @DatabaseNumber INT, @ClientCompanyPk UNIQUEIDENTIFIER, @PriceItemPk UNIQUEIDENTIFIER, @ExcludedSystemCodes VARCHAR(200))
RETURNS @Result TABLE
(
	TX_PriceItemCode varchar(3) not null,
	CompanyCode varchar(3) not null,
	BranchCode varchar(3) not null,
	TX_BillableCount int not null,
	TX_Reference1 varchar(50) not null,
	TX_Reference2 varchar(50) null,
	TX_Reference3 varchar(50) null,
	TX_Reference4 varchar(50) null,
	TX_ServiceOccuredUTC datetime2(7) not null,
	StaffCode varchar(3) not null,
	AdjustedUnitCount decimal(14, 4) null,
	TenantID varchar(50) null
)
BEGIN

DECLARE @Codes TABLE (Seq int identity(1,1), Category VARCHAR(3), Code VARCHAR(3), SystemCode VARCHAR(3), FeeType VARCHAR(3), CountryTierCode VARCHAR(3))

INSERT INTO @Codes(Category, Code, SystemCode, FeeType, CountryTierCode)
SELECT Category = PUM_UsageCategory, Code = PUM_UsageCode, SystemCode = L6_SystemCode, FeeType = L7_FeeType, CountryTierCode = L7_CountryTierCode
FROM 
	dbo.EdiPriceUsageMapping
	JOIN dbo.ClientLicencePriceHeader ON PUM_L6 = L6_PK
	JOIN dbo.ClientLicencePriceItem ON L7_L6 = L6_PK AND PUM_PriceCategory = L7_Category AND PUM_PriceCode = L7_Code
	LEFT JOIN
	(
		SELECT VALUE FROM dbo.SplitStringToTable(@ExcludedSystemCodes, DEFAULT)
	) T ON T.VALUE = PUM_UsageCategory
	WHERE L7_PK = @PriceItemPk AND T.VALUE IS NULL
UNION ALL
SELECT Category = L7_Category, Code = L7_Code, SystemCode = L6_SystemCode, FeeType = L7_FeeType, CountryTierCode = L7_CountryTierCode FROM dbo.ClientLicencePriceItem JOIN dbo.ClientLicencePriceHeader ON L7_L6 = L6_PK WHERE L7_PK = @PriceItemPk

declare @CodeCount int = @@ROWCOUNT

declare @remoteData table (
	TX_LCC uniqueidentifier NULL,
	TX_PriceItemCode varchar(3) NOT NULL,
	TX_Reference1 varchar(50) NOT NULL,
	TX_Reference2 varchar(50) NULL,
	TX_Reference3 varchar(50) NULL,
	TX_Reference4 varchar(50) NULL,
	TX_BillableCount int not null,
	TX_ClientStaffCode varchar(3) null,
	TX_Branch varchar(3) null,
	TX_ServiceOccuredUTC datetime2(7) not null,
	AdjustedUnitCount decimal(14, 4) null,
	TenantID varchar(50) null
)

DECLARE @BillingDbCodes TABLE (Seq int, DbCode VARCHAR(3));

-- Fetch remote data with a simple query for each category-price code pair.
-- Query is covered by the remote index and has no cross server joins so will be fast.
while @CodeCount > 0
begin
	declare @Category varchar(3)
	declare @Code varchar(3)
	declare @SystemCode varchar(3)
	declare @FeeType varchar(3)
	declare @CountryTierCode varchar(3)
	select @Category = Category, @Code = Code, @SystemCode = SystemCode, @FeeType = FeeType, @CountryTierCode = CountryTierCode from @Codes where Seq = @CodeCount
	set @CodeCount = @CodeCount - 1

	-- special cases where code in billing DB needs mapping
	delete @BillingDbCodes;
	declare @BillingDbCode varchar(3) = @Code;

	if @Category = 'STL' AND @Code = 'UCN'
	BEGIN
		INSERT @BillingDbCodes(Seq, DbCode) VALUES(1, 'USR');
	END
	ELSE IF @Category = 'STL' and @Code IN ('USW', 'UCS')
	BEGIN
		INSERT @BillingDbCodes(Seq, DbCode) VALUES(1, 'USR');

		IF @Period >= 202207
		BEGIN 
			INSERT @BillingDbCodes(Seq, DbCode) VALUES(2, 'RBU');
		END
	END
	ELSE IF @Category = 'CTR' and @Code = 'CTR'
	BEGIN
		INSERT @BillingDbCodes(Seq, DbCode) VALUES(1, 'CTO');
	END
	ELSE
	BEGIN
		INSERT @BillingDbCodes(Seq, DbCode) VALUES(1, @Code);
	END

	DECLARE @DbCodeCount INT = (SELECT COUNT(*) FROM @BillingDbCodes);

	WHILE @DbCodeCount > 0
	BEGIN
		SELECT @BillingDbCode = DbCode FROM @BillingDbCodes WHERE Seq = @DbCodeCount;
		SET @DbCodeCount = @DbCodeCount - 1;

		IF @BillingDbCode = 'WTU'
			BEGIN
				insert @remoteData(
						TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, 
						TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC, AdjustedUnitCount)
				select TX_LCC,             @Code, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, 
						TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC, AdjustedUnitCount
				from dbo.EdiLoadBillingDbDetailedUsageWTU(@Period, @DatabaseId, @ClientCompanyPk);
			END
		ELSE IF @BillingDbCode = 'USR'
			BEGIN
				INSERT @remoteData(
						TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC)
				SELECT TX_LCC,            @Code, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC
				FROM dbo.EdiLoadBillingDbDetailedUsageUSR(@Period, @DatabaseId, @ClientCompanyPk);
			END
		ELSE IF @FeeType = 'COT'
			BEGIN
				INSERT @remoteData(
						TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC)
				SELECT TX_LCC,            @Code, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC
			FROM dbo.EdiLoadBillingDbDetailedUsageCOT(@Code, @Category, @SystemCode, @Period, @DatabaseId, @CountryTierCode, @ClientCompanyPk);
			END
		ELSE IF @Category = 'STL' AND @BillingDbCode IN --Disbursement
									(
										SELECT TRIM(VALUE) AS SplitValue
										FROM dbo.EdiGetBillingDisbursementUsageMappings()
										CROSS APPLY STRING_SPLIT(UDM_UsageCodes, ',')
									)
			BEGIN
				INSERT @remoteData(
						TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC)
				SELECT TX_LCC,             @Code, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC
				FROM dbo.EdiLoadBillingDbDetailedUsageDisbursement(@Period, @Category, @BillingDbCode, @DatabaseNumber, @ClientCompanyPk);
			END
		ELSE IF EXISTS (SELECT TOP 1 1
							FROM dbo.EdiLicenceDatabaseConsolidationHistory 
							JOIN dbo.LicenceDatabase ON LD_PK = EDH_LD_ConsolidatedDatabase
							WHERE EDH_Period = @Period 
							  AND LD_DatabaseNumber = @DatabaseNumber)
			BEGIN
				insert @remoteData(
						TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, 
						TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC, TenantID)
				select  @Code, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, 
						TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC, TenantID
				from dbo.EdiLoadBillingDbDetailedConsolidationUsage(@Period, @Category, @BillingDbCode, @DatabaseNumber);
			END
		ELSE
			BEGIN

				--JPC.AFR -> STL.AFR cut-off
				--if the mapping is STL.AFR but the AFR is billed by JPC.AFR, change the category to JPC for old reports
				IF @Category = 'STL' AND @BillingDbCode = 'AFR' AND EXISTS (
							SELECT TOP 1 1
							FROM dbo.EdiBilledUsage
							WHERE BU9_L7 = @PriceItemPk
							AND BU9_UsageCode = 'JPC' AND BU9_UsageSubCode = 'AFR'
							AND BU9_PeriodStart = (SELECT CAST(CAST(@Period AS VARCHAR(6)) + '01' AS DATETIME))
							AND BU9_LD = (SELECT TOP 1 LD_PK FROM dbo.LicenceDatabase WHERE LD_DatabaseNumber = @DatabaseNumber))
				BEGIN
					SET @Category = 'JPC'
				END
				
				--WI00884526
				--Some clients were manually billed under STL.AFR, so we need to display STL.AFR for them.
				IF @Category = 'JPC' AND @BillingDbCode = 'AFR' AND (@Period BETWEEN 202312 AND 202501)  
					AND @DatabaseNumber IN ( 1995, 24024, 8626, 3125, 26946 )
				BEGIN
					SET @Category = 'STL';
				END

				insert @remoteData(
						TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC)
				select TX_LCC,            @Code, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC
				from dbo.BillingViewChargeable
				where TX_Period = @Period
					and TX_Category = @Category
					AND TX_PriceItemCode = @BillingDbCode
					AND
					(
							(TX_SystemId IS NOT NULL AND TX_SystemId = @DatabaseId)
						OR (TX_SystemId IS NULL AND TX_DatabaseNumber = @DatabaseNumber)
					)
					AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
					AND TX_BillableCount > 0
			END
	END
end

insert @result(TX_PriceItemCode, CompanyCode, BranchCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ServiceOccuredUTC, StaffCode, AdjustedUnitCount, TenantID)
select
	TX_PriceItemCode,
	CompanyCode = ISNULL(LCC_Code, ''),
	BranchCode = ISNULL(TX_Branch, ''),
	TX_BillableCount,
	TX_Reference1 = TX_Reference1, 
	TX_Reference2 = ISNULL(TX_Reference2, ''),
	TX_Reference3 = ISNULL(TX_Reference3, ''),
	TX_Reference4 = ISNULL(TX_Reference4, ''),
	TX_ServiceOccuredUTC,
	StaffCode = ISNULL(TX_ClientStaffCode, ''),
	AdjustedUnitCount,
	TenantID
FROM
	@remoteData
	LEFT JOIN dbo.ClientCompany ON TX_LCC = LCC_PK;

RETURN
END
", "drop function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#region WTU

		public const string GetBillingDbDetailedUsageWTUQuery = "EdiLoadBillingDbDetailedUsageWTU";

		public static DatabaseViewAndRoutineCreateScript GetBillingDbDetailedUsageWTUScript()
		{
			const string name = GetBillingDbDetailedUsageWTUQuery;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period INT, @DatabaseId VARCHAR(50), @ClientCompanyPk UNIQUEIDENTIFIER)
RETURNS @Result TABLE (
	TX_LCC uniqueidentifier NOT NULL,
	TX_PriceItemCode varchar(3) NOT NULL,
	TX_Reference1 varchar(50) NOT NULL,
	TX_Reference2 varchar(50) NULL,
	TX_Reference3 varchar(50) NULL,
	TX_Reference4 varchar(50) NULL,
	TX_BillableCount int not null,
	TX_ClientStaffCode varchar(3) null,
	TX_Branch varchar(3) null,
	TX_ServiceOccuredUTC datetime2(7) not null,
	AdjustedUnitCount decimal(14, 4) null,
	TX_Reference4AsINT int not null
)
BEGIN

	DECLARE @wtuAdjustment TABLE
	(
		ADJ_OriginalUnitCount INT NOT NULL PRIMARY KEY,
		ADJ_AdjustedUnitCount DECIMAL(14,4) NOT NULL,
		ADJ_DefaultAdjustedIncrement DECIMAL(14,4) NOT NULL
	);

	INSERT @wtuAdjustment(ADJ_OriginalUnitCount, ADJ_AdjustedUnitCount, ADJ_DefaultAdjustedIncrement)
	SELECT ADJ_OriginalUnitCount, ADJ_AdjustedUnitCount, ADJ_DefaultAdjustedIncrement FROM dbo.EdiGetBillingUnitCountAdjustments() WHERE ADJ_PriceCode = 'WTU';

	IF NOT EXISTS (SELECT * FROM @wtuAdjustment)
	BEGIN
		RETURN
	END

	INSERT @Result(
			TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4,
			TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC, AdjustedUnitCount, TX_Reference4AsINT)
	SELECT TX_LCC,  TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4,
			TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC, NULL, CAST(TX_Reference4 AS INT)
	FROM dbo.BillingViewChargeable
	WHERE TX_Period = @Period
		and TX_Category = 'STL'
		AND TX_PriceItemCode = 'WTU'
		AND TX_SystemId = @DatabaseId
		AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
		AND TX_BillableCount > 0
		AND TX_Reference3 is not null
		AND TX_Reference3 <> ''
		AND ISNUMERIC(TX_Reference4) = 1

	DECLARE @wtuIncrement TABLE
	(
		INC_UnitCount INT NOT NULL PRIMARY KEY,
		INC_AdjustedUnitCount DECIMAL(14,4) NOT NULL
	);
	DECLARE @wtuDefaultIncrement DECIMAL(14,4) = (SELECT TOP 1 ADJ_DefaultAdjustedIncrement FROM @wtuAdjustment);

	INSERT INTO @wtuIncrement(INC_UnitCount, INC_AdjustedUnitCount)
	SELECT CUR.ADJ_OriginalUnitCount, CUR.ADJ_AdjustedUnitCount - PRE.ADJ_AdjustedUnitCount
	FROM @wtuAdjustment CUR
	JOIN 
	(
		SELECT ADJ_OriginalUnitCount, ADJ_AdjustedUnitCount FROM @wtuAdjustment
		UNION
		SELECT 0, 0
	) PRE ON CUR.ADJ_OriginalUnitCount - 1 = PRE.ADJ_OriginalUnitCount;

	UPDATE R
	SET AdjustedUnitCount = ISNULL(INC_AdjustedUnitCount, @wtuDefaultIncrement)
	FROM @Result R
	LEFT JOIN @wtuIncrement ON TX_Reference4AsINT = INC_UnitCount

	RETURN
END
", "drop function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region USR

		public const string GetBillingDbDetailedUsageUSRQuery = "EdiLoadBillingDbDetailedUsageUSR";

		public static DatabaseViewAndRoutineCreateScript GetBillingDbDetailedUsageUSRScript()
		{
			const string name = GetBillingDbDetailedUsageUSRQuery;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period INT, @DatabaseId VARCHAR(50), @ClientCompanyPk UNIQUEIDENTIFIER)
RETURNS @Result TABLE (
	TX_LCC uniqueidentifier NOT NULL,
	TX_PriceItemCode varchar(3) NOT NULL,
	TX_Reference1 varchar(50) NOT NULL,
	TX_Reference2 varchar(50) NULL,
	TX_Reference3 varchar(50) NULL,
	TX_Reference4 varchar(50) NULL,
	TX_BillableCount int not null,
	TX_ClientStaffCode varchar(3) null,
	TX_Branch varchar(3) null,
	TX_ServiceOccuredUTC datetime2(7) not null,
	INDEX INDEX1 NONCLUSTERED(TX_PriceItemCode, TX_ClientStaffCode) WITH (ALLOW_PAGE_LOCKS = OFF)
)
BEGIN
	declare @remoteDataUSR table (
		TX_LCC uniqueidentifier NOT NULL,
		TX_PriceItemCode varchar(3) NOT NULL,
		TX_Reference1 varchar(50) NOT NULL,
		TX_Reference2 varchar(50) NULL,
		TX_Reference3 varchar(50) NULL,
		TX_Reference4 varchar(50) NULL,
		TX_BillableCount int not null,
		TX_ClientStaffCode varchar(3) null,
		TX_Branch varchar(3) null,
		TX_ServiceOccuredUTC datetime2(7) not null
	);

	INSERT @remoteDataUSR(
		   TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC)
	SELECT TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC
	FROM dbo.BillingViewChargeable
	WHERE TX_Period = @Period
		AND TX_Category = 'STL'
		AND (TX_PriceItemCode = 'USR' OR TX_PriceItemCode = 'RBU')
		AND TX_SystemId = @DatabaseId
		AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
		AND TX_BillableCount > 0;

	INSERT @Result(
		   TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC)
	SELECT TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC
	FROM @remoteDataUSR USR
	WHERE TX_PriceItemCode = 'USR' AND NOT EXISTS 
	(
		SELECT 1 FROM @remoteDataUSR RBU WHERE 
		RBU.TX_PriceItemCode = 'RBU' AND
		RBU.TX_ClientStaffCode = USR.TX_ClientStaffCode
	);

	RETURN;
END
", "drop function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region COT

		public const string GetBillingDbDetailedUsageCOTQuery = "EdiLoadBillingDbDetailedUsageCOT";

		public static DatabaseViewAndRoutineCreateScript GetBillingDbDetailedUsageCOTScript()
		{
			const string name = GetBillingDbDetailedUsageCOTQuery;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Code VARCHAR(3), @Category VARCHAR(3), @SystemCode VARCHAR(3), @Period INT, @DatabaseId VARCHAR(50), @CountryTierCode VARCHAR(3), @ClientCompanyPk UNIQUEIDENTIFIER)
RETURNS @Result TABLE (
	TX_LCC uniqueidentifier NOT NULL,
	TX_PriceItemCode varchar(3) NOT NULL,
	TX_Reference1 varchar(50) NOT NULL,
	TX_Reference2 varchar(50) NULL,
	TX_Reference3 varchar(50) NULL,
	TX_Reference4 varchar(50) NULL,
	TX_BillableCount int not null,
	TX_ClientStaffCode varchar(3) null,
	TX_Branch varchar(3) null,
	TX_ServiceOccuredUTC datetime2(7) not null
)
BEGIN
	INSERT @Result(
		   TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC)
	SELECT TX_LCC,            @Code, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC
	FROM dbo.BillingViewChargeable
		JOIN dbo.ClientCompany ON TX_LCC = LCC_PK
		JOIN dbo.EdiGetCountryTierPriceCodeMappings() ON COT_PriceCode = @Code AND COT_SystemCode = @SystemCode AND LCC_RN_NKCountryCode = COT_CountryCode
	WHERE
		TX_PriceItemCode = @Code
		AND TX_Period = @Period
		AND TX_Category = @Category
		AND TX_SystemId = @DatabaseId
		AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
		AND TX_BillableCount > 0
		AND COT_CountryTierCode = @CountryTierCode;

	RETURN;
END
", "drop function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region Consolidation Usage

		public const string GetBillingDbDetailedConsolidationUsageQuery = "EdiLoadBillingDbDetailedConsolidationUsage";

		public static DatabaseViewAndRoutineCreateScript GetBillingDbDetailedConsolidationUsageScript()
		{
			const string name = GetBillingDbDetailedConsolidationUsageQuery;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"
(
	@Period INT,
	@Category VARCHAR(3), 
	@PriceItemCode VARCHAR(3), 
	@DatabaseNumber INT
)
RETURNS @Result TABLE
(
	TX_PriceItemCode VARCHAR(3) NOT NULL,
	TX_Branch VARCHAR(3) NULL,
	TX_BillableCount INT NOT NULL,
	TX_Reference1 VARCHAR(50) NOT NULL,
	TX_Reference2 VARCHAR(50) NULL,
	TX_Reference3 VARCHAR(50) NULL,
	TX_Reference4 VARCHAR(50) NULL,
	TX_ServiceOccuredUTC DATETIME2(7) NOT NULL,
	TX_ClientStaffCode VARCHAR(3) NULL,
	TenantID VARCHAR(50) NULL
)
BEGIN

	DECLARE @SubDatabaseNumber INT;
	DECLARE @SubDatabaseTenantID VARCHAR(50);
	DECLARE DatabaseNumberCursor CURSOR FOR
		SELECT Sub.LD_DatabaseNumber, Sub.LD_TenantID
		FROM dbo.EdiLicenceDatabaseConsolidationHistory
		JOIN dbo.LicenceDatabase Main ON Main.LD_PK = EDH_LD_ConsolidatedDatabase
		JOIN dbo.LicenceDatabase Sub ON Sub.LD_PK = EDH_LD
		WHERE EDH_Period = @Period 
		  AND Main.LD_DatabaseNumber = @DatabaseNumber
		ORDER BY Sub.LD_DatabaseNumber;

	OPEN DatabaseNumberCursor
	FETCH NEXT FROM DatabaseNumberCursor INTO @SubDatabaseNumber, @SubDatabaseTenantID

	WHILE @@FETCH_STATUS = 0
	BEGIN

		INSERT @Result
		(
			TX_PriceItemCode,
			TX_Branch,
			TX_BillableCount,
			TX_Reference1,
			TX_Reference2,
			TX_Reference3,
			TX_Reference4,
			TX_ServiceOccuredUTC,
			TX_ClientStaffCode,
			TenantID
		)
		SELECT
			TX_PriceItemCode,
			TX_Branch,
			TX_BillableCount,
			TX_Reference1,
			TX_Reference2,
			TX_Reference3,
			TX_Reference4,
			TX_ServiceOccuredUTC,
			TX_ClientStaffCode,
			@SubDatabaseTenantID
		FROM dbo.BillingViewChargeable
		WHERE TX_Period = @Period
			and TX_Category = @Category
			AND TX_PriceItemCode = @PriceItemCode
			AND TX_DatabaseNumber = @SubDatabaseNumber
			AND TX_BillableCount > 0;

		FETCH NEXT FROM DatabaseNumberCursor INTO @SubDatabaseNumber, @SubDatabaseTenantID
	END

	CLOSE DatabaseNumberCursor
	DEALLOCATE DatabaseNumberCursor

	RETURN;
END
", "drop function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region Disbursement

		public const string GetBillingDbDetailedUsageDisbursementQuery = "EdiLoadBillingDbDetailedUsageDisbursement";

		public static DatabaseViewAndRoutineCreateScript GetBillingDbDetailedUsageDisbursementScript()
		{
			const string name = GetBillingDbDetailedUsageDisbursementQuery;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"
(
	@Period INT, 
	@Category VARCHAR(3),
	@PriceItemCode VARCHAR(3),
	@DatabaseNumber INT,
	@ClientCompanyPk UNIQUEIDENTIFIER
)
RETURNS @Result TABLE (
	TX_LCC UNIQUEIDENTIFIER NOT NULL,
	TX_PriceItemCode VARCHAR(3) NOT NULL,
	TX_Reference1 VARCHAR(50) NOT NULL,
	TX_Reference2 VARCHAR(50) NULL,
	TX_Reference3 VARCHAR(50) NULL,
	TX_Reference4 VARCHAR(50) NULL,
	TX_BillableCount INT NOT NULL,
	TX_ClientStaffCode VARCHAR(3) NULL,
	TX_Branch VARCHAR(3) NULL,
	TX_ServiceOccuredUTC DATETIME2(7) NOT NULL
)
BEGIN
	DECLARE @RemoteData TABLE (
		TX_LCC UNIQUEIDENTIFIER NOT NULL,
		TX_PriceItemCode VARCHAR(3) NOT NULL,
		TX_Reference1 VARCHAR(50) NOT NULL,
		TX_Reference2 VARCHAR(50) NULL,
		TX_Reference3 VARCHAR(50) NULL,
		TX_Reference4 VARCHAR(50) NULL,
		TX_Reference5 VARCHAR(50) NULL,
		TX_BillableCount INT NOT NULL,
		TX_ClientStaffCode VARCHAR(3) NULL,
		TX_Branch VARCHAR(3) NULL,
		TX_ServiceOccuredUTC DATETIME2(7) NOT NULL
	);
 
	DECLARE @DisbursementCode VARCHAR(50);
	SELECT TOP 1 @DisbursementCode = UDM_DisbursementCode
	FROM
	(
		SELECT UDM_DisbursementCode, TRIM(VALUE) AS UsageCode
		FROM dbo.EdiGetBillingDisbursementUsageMappings()
		CROSS APPLY STRING_SPLIT(UDM_UsageCodes, ',')
	) T WHERE UsageCode = @PriceItemCode;

	INSERT @RemoteData(
			TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_Reference5, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC)
	SELECT TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_Reference5, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC
	FROM dbo.BillingViewChargeable
	WHERE TX_Period = @Period
		AND TX_Category = 'STL'
		AND (TX_PriceItemCode = @PriceItemCode OR TX_PriceItemCode = @DisbursementCode)
		AND TX_DatabaseNumber = @DatabaseNumber
		AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
		AND TX_BillableCount > 0;

	INSERT @Result(
			TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC)
	SELECT TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_ClientStaffCode, TX_Branch, TX_ServiceOccuredUTC
	FROM @RemoteData
	WHERE TX_PriceItemCode = @PriceItemCode
	  AND IIF(@PriceItemCode = 'SHP', TX_Reference5, TX_Reference4) NOT IN
		  (
				SELECT TX_Reference5 FROM @RemoteData WHERE TX_PriceItemCode = @DisbursementCode -- matched by DisbursementCode
		  );

	RETURN;
END
", "drop function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#endregion

		#region GetBillingGroups

		public const string GetBillingGroups = ObjectPrefix + "GetBillingGroups";

		public static DatabaseViewAndRoutineCreateScript GetBillingGroupsScript()
		{
			const string name = GetBillingGroups;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@OrgPk UNIQUEIDENTIFIER, @PeriodStart DateTime, @Product VARCHAR(3))
RETURNS @Result TABLE
(
	IsOrg BIT NOT NULL,
	GroupPk UNIQUEIDENTIFIER NOT NULL,
	IsFromGroup BIT NOT NULL
)
BEGIN

declare @OrgPks as TABLE(OrgPk uniqueidentifier NOT NULL);

insert @OrgPks(OrgPk)
	-- Organisation itself
	SELECT @OrgPk as OrgPk
	UNION
	(
		-- Non-partner Organisation the given organisation is invoiced to
		SELECT ClientInvoiceDelivery.L9_OH_InvoiceTo 
		FROM dbo.ClientInvoiceDelivery
		JOIN dbo.LicenceCompany on L9_LC = LC_PK
		JOIN dbo.LicenceCompany parent on parent.LC_OH = L9_OH_InvoiceTo
		LEFT JOIN dbo.ClientLicenceBilling parentBilling on parentBilling.L4_LC = parent.LC_PK
		WHERE LicenceCompany.LC_OH = @OrgPk
			AND ISNULL(parentBilling.L4_IsPartner, 'N') = 'N'
	)
	UNION
	(
		-- Organisations that are invoiced to the given non-partner organisation
		SELECT LicenceCompany.LC_OH 
		FROM dbo.LicenceCompany
		JOIN dbo.ClientInvoiceDelivery on L9_LC = LC_PK
		JOIN dbo.LicenceCompany parent on parent.LC_OH = L9_OH_InvoiceTo
		LEFT JOIN dbo.ClientLicenceBilling parentBilling on parentBilling.L4_LC = parent.LC_PK
		WHERE ClientInvoiceDelivery.L9_OH_InvoiceTo = @OrgPk
			AND ISNULL(parentBilling.L4_IsPartner, 'N') = 'N'
	)
	UNION
	(
		-- Organisations that are invoiced to the Organisation the given organisation is invoiced to
		SELECT LC_OH
		FROM dbo.LicenceCompany
		JOIN dbo.ClientInvoiceDelivery on L9_LC = LC_PK
		WHERE L9_OH_InvoiceTo in 
		(
			-- Non-partner Organisation the given organisation is invoiced to
			SELECT ClientInvoiceDelivery.L9_OH_InvoiceTo 
			FROM dbo.ClientInvoiceDelivery
			JOIN dbo.LicenceCompany on L9_LC = LC_PK
			JOIN dbo.LicenceCompany parent on parent.LC_OH = L9_OH_InvoiceTo
			LEFT JOIN dbo.ClientLicenceBilling parentBilling on parentBilling.L4_LC = parent.LC_PK
			WHERE LicenceCompany.LC_OH = @OrgPk
				AND ISNULL(parentBilling.L4_IsPartner, 'N') = 'N'
		)
	);

declare @DbPks as TABLE(DbPk uniqueidentifier NOT NULL, IsFromGroup bit NOT NULL);

-- all databases belonging to the orgs
insert @DbPks(DbPk, IsFromGroup)
select PK = LA_LD, IsFromGroup = cast(0 as bit)
from 
	@OrgPks
	join dbo.LicenceCompany on lc_oh = OrgPk
	join dbo.LicenceHeader on la_lc = lc_pk and LA_IsActive = 1
	join dbo.LicenceDatabase on LA_LD = LD_PK
where (@Product is null AND LD_Product in ('ENT', 'CW1', 'CWN', 'CGW', 'BOR', 'PRW'))
   or (@Product is not null AND LD_Product = @Product);


-- all databases in a buying group or shared commitment group
with buygroup as 
(
	select LS9_LD, LS9_Name
	from dbo.EdiLicenceSetting
	where LS9_Type in ('" + Billing.Business.BillingConstants.LicenceSetting.BuyingGroup + "', '" + Billing.Business.BillingConstants.LicenceSetting.Commitment + @"')
		and LS9_Name != ''
		and (LS9_ValidFrom is null or LS9_ValidFrom <= @PeriodStart)
		and (LS9_ValidTo is null or LS9_ValidTo > @PeriodStart)
)
insert @DbPks(DbPk, IsFromGroup)
select distinct LS9_LD, IsFromGroup = cast(1 as bit)
from buygroup 
where 
	LS9_Name in (select LS9_Name from buygroup where LS9_LD in (select DbPk from @DbPks))
	and LS9_LD not in (select DbPk from @DbPks);


insert @Result
select IsOrg = cast(1 as bit), PK = OrgPk, IsFromGroup = cast(0 as bit) from @OrgPks
union all
select IsOrg = cast(0 as bit), PK = DbPk, IsFromGroup from @DbPks
union

(
	-- all organizations on the same database
	select IsOrg = cast(1 as bit), PK = c2.LC_OH, IsFromGroup
	from @DbPks
	join dbo.LicenceHeader h2 on h2.LA_LD = DbPk and h2.LA_IsActive = 1
	join dbo.LicenceCompany c2 on h2.LA_LC = c2.LC_PK
);


RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#endregion

		#region EdiGetOdmPriceHeadersForDate

		public static DatabaseViewAndRoutineCreateScript EdiGetOdmPriceHeadersForDateScript()
		{
			const string name = "EdiGetOdmPriceHeadersForDate";
			return new DatabaseViewAndRoutineCreateScript(name,
@"create function " + name + @"(@DateFrom smalldatetime)
returns table
with schemabinding
as return
(
	select LA_PK, LA_LC, L6_PK = itemOwner.L6_PK, validPrices.L6_ValidFrom
	from dbo.LicenceHeader
	outer apply
	(
		select top 1 L9_OH_InvoiceTo, L9_UseParentPrices
		from dbo.ClientInvoiceDelivery
		join dbo.LicenceDatabase on LA_LD = LD_PK
		where L9_SystemCode in ('ODM', 'ALL') and L9_ServerCode in ('', LD_ServerCode)
			and L9_LC = LA_LC
		order by L9_SystemCode desc, L9_ServerCode desc
	) delivery
	outer apply
	(
		select L6_PK, L6_ValidFrom, L6_IsStandard, L6_RN_NKCountry, L6_RX_NKCurrency, L6_LicenceEdition, L6_PricelistVersion
		from
		(
			select top 1 L6_PK, L6_ValidFrom, L6_ValidTo, L6_IsStandard, L6_RN_NKCountry, L6_RX_NKCurrency, L6_LicenceEdition, L6_PricelistVersion
			from dbo.ClientLicencePriceHeader
			where L6_LC = LA_LC
				and L6_SystemCode = 'ODM'
				and L6_ValidFrom <= @DateFrom
				and ISNULL(L9_UseParentPrices, 0) = 0
			order by L6_ValidFrom desc
		) localPrices
		where (localPrices.L6_ValidTo is NULL OR localPrices.L6_ValidTo >= @DateFrom)
			and
			(
				L6_IsStandard = 'Y'
				or
				L6_PK in (select L7_L6 from dbo.ClientLicencePriceItem where L7_Code = 'COR') -- ignore quick transactional
			)
	) localValidPrices
	outer apply
	(
		select payerPrices.L6_PK, payerPrices.L6_ValidFrom, payerPrices.L6_IsStandard, payerPrices.L6_RN_NKCountry, payerPrices.L6_RX_NKCurrency, payerPrices.L6_LicenceEdition, payerPrices.L6_PricelistVersion
		from
		(
			select top 1 p.L6_PK, p.L6_ValidFrom, p.L6_ValidTo, p.L6_IsStandard, p.L6_RN_NKCountry, p.L6_RX_NKCurrency, p.L6_LicenceEdition, p.L6_PricelistVersion
			from dbo.LicenceCompany 
			join dbo.ClientLicencePriceHeader p on L6_LC = LC_PK
			where localValidPrices.L6_PK is null
				and LC_OH = L9_OH_InvoiceTo
				and p.L6_SystemCode = 'ODM'
				and p.L6_ValidFrom <= @DateFrom
			order by L6_ValidFrom desc
		) payerPrices
		where payerPrices.L6_ValidTo is NULL OR payerPrices.L6_ValidTo >= @DateFrom
	) payerValidPrices
	cross apply
	(
		-- take local prices, falling back to payer/parent prices
		select localValidPrices.L6_PK, localValidPrices.L6_ValidFrom, localValidPrices.L6_IsStandard, localValidPrices.L6_RN_NKCountry, localValidPrices.L6_RX_NKCurrency, localValidPrices.L6_LicenceEdition, localValidPrices.L6_PricelistVersion
		union all
		select payerValidPrices.L6_PK, payerValidPrices.L6_ValidFrom, payerValidPrices.L6_IsStandard, payerValidPrices.L6_RN_NKCountry, payerValidPrices.L6_RX_NKCurrency, payerValidPrices.L6_LicenceEdition, payerValidPrices.L6_PricelistVersion
		where localValidPrices.L6_PK is null
	) validPrices
	cross apply
	(
		select validPrices.L6_PK where validPrices.L6_IsStandard = 'N'
		union all
		select top 1 std.L6_PK
		from dbo.ClientLicencePriceHeader std
		where validPrices.L6_IsStandard = 'Y'
			AND std.L6_LC = '31754C3F-4782-4504-AC75-C92B0EEB1B73'
			and std.L6_SystemCode = 'ODM'
			and validPrices.L6_RN_NKCountry = std.L6_RN_NKCountry
			and validPrices.L6_RX_NKCurrency = std.L6_RX_NKCurrency
			and validPrices.L6_LicenceEdition = std.L6_LicenceEdition
			and validPrices.L6_PricelistVersion = std.L6_PricelistVersion
	) itemOwner
)", "drop function " + name, DbRoutineType.SqlFunctionInlineTypeDesc);
		}

		#endregion

		#region FAX

		public static DatabaseViewAndRoutineCreateScript GetFaxScript()
		{
			return GetUsageConverterFromLegacyId(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.Fax, true);
		}

		public static DatabaseViewAndRoutineCreateScript GetFaxLegacyScript()
		{
			string name = GetUsageByLegacyIdFunctionName(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.Fax);
			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	EnterpriseCode varchar(3) NOT NULL, 
	CompanyCode varchar(3) NOT NULL, 
	ServerCode varchar(3) NOT NULL,
	SubCode varchar(50) NOT NULL,
	UnitCount int NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	INSERT @Result(EnterpriseCode, CompanyCode, ServerCode, SubCode, UnitCount)
	SELECT
		EXU_EnterpriseCode, 
		EXU_CompanyCode,
		EXU_ServerCode,
		SubCode = '',
		EXU_UnitCount
	FROM dbo.EdiExternalChargeableUsage
	WHERE EXU_Period = @Period AND EXU_Code = 'FAX';

	RETURN
END
",
				"DROP FUNCTION " + name,
				DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region eRouter

		#region IQM

		//public static DatabaseViewAndRoutineCreateScript GetIQMScript()
		//{
		//	return GetErouterCreateScript(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.eBACCA,
		//		ApplicationCodeList.Codes.NZMAFeBACCa, false);
		//}

		#endregion

		#region EXD

		//public static DatabaseViewAndRoutineCreateScript GetEXDScript()
		//{
		//	return GetErouterCreateScript(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ExDocs,
		//		ApplicationCodeList.Codes.AUExDoc, true);
		//}

		#endregion

//		public static DatabaseViewAndRoutineCreateScript GetErouterCreateScript(string systemCode, string applicationCode, bool includeReference)
//		{
//			string name = GetUsageFunctionOrProcedureName(systemCode);

//			// Using non-inline function so external database need not exist for function to be created.
//			return new DatabaseViewAndRoutineCreateScript(name,
//@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
//RETURNS @Result TABLE
//(
//	LC_PK uniqueidentifier NULL,
//	LD_PK uniqueidentifier NULL,
//	LCC_PK uniqueidentifier NOT NULL,
//	SubCode varchar(50) NOT NULL,
//	UnitCount int NOT NULL,
//	Reference1 varchar(50) NULL,
//	Reference2 varchar(50) NULL,
//	Reference3 varchar(50) NULL,
//	Reference4 varchar(50) NULL
//)
//BEGIN
//	INSERT @Result(LCC_PK, SubCode, UnitCount)
//" + GetErouterSelectSql(applicationCode, includeReference) + @"

//	RETURN
//END", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
//		}

//		internal static string GetErouterSelectSql(string applicationCode, bool includeReference)
//		{
//			return
//@"	SELECT
//		LCC_PK = U2_LCC,
//		SubCode = " + (includeReference ? "U2_Reference" : "''") + ",\r\n" +
//@"		UnitCount = COUNT(*)
//	FROM
//		ClientERouterChargeableMessage
//	WHERE
//		U2_ApplicationCode = '" + applicationCode + @"'
//		AND U2_Date >= @PeriodStartTimeUtc 
//		AND U2_Date < @PeriodEndTimeUtc
//		AND U2_LCC IS NOT NULL
//	GROUP BY
//		U2_LCC" + (includeReference ? ", U2_Reference" : "");
//		}

		#endregion

		#region DPS

		public static DatabaseViewAndRoutineCreateScript GetDPSScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.DeniedPartyScreening, true);
		}

		public static DatabaseViewAndRoutineCreateScript GetDPS_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.DeniedPartyScreening);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	SELECT
		TX_DatabaseNumber, TX_LCC, SubCode = '', UnitCount = count(*)
	FROM
		dbo.BillingViewChargeable
	WHERE				
		TX_Category = 'DPS'
		AND TX_PriceItemCode = 'DPS'
		AND TX_Reference1 = 'SCR'
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	RETURN
END
", "drop function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region DCG

		public static DatabaseViewAndRoutineCreateScript GetDCGScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.DistanceCalculatorGeneric);
		}

		public static DatabaseViewAndRoutineCreateScript GetDCGBillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.DistanceCalculatorGeneric);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount int NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	SELECT
		TX_DatabaseNumber, TX_LCC, SubCode = '', UnitCount = count(*)
	FROM
		dbo.BillingViewChargeable
	WHERE				
		TX_Category = 'DCG'
		AND TX_PriceItemCode = 'DCG'
		AND TX_Reference1 = 'GOO'
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region DCP

		public static DatabaseViewAndRoutineCreateScript GetDCPScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.DistanceCalculatorPcMiler);
		}

		public static DatabaseViewAndRoutineCreateScript GetDCPBillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.DistanceCalculatorPcMiler);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount int NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	SELECT
		TX_DatabaseNumber, TX_LCC, SubCode = '', UnitCount = count(*)
	FROM
		dbo.BillingViewChargeable
	WHERE				
		TX_Category = 'DCG'
		AND TX_PriceItemCode = 'DCG'
		AND TX_Reference1 = 'PCM'
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region HOS

		public static DatabaseViewAndRoutineCreateScript GetHOSScript()
		{
			var name = GetUsageFunctionOrProcedureName(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.HostingStorage);

			return new DatabaseViewAndRoutineCreateScript(name,
$@"CREATE FUNCTION {name}(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	LC_PK uniqueidentifier NULL,
	LD_PK uniqueidentifier NULL,
	LCC_PK uniqueidentifier NULL,
	SubCode varchar(50) NOT NULL,
	UnitCount int NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	
	DECLARE @RawData TABLE
	(
		TX_DatabaseNumber INT NOT NULL,
		SizeMB INT NOT NULL,
		IsDocs BIT NOT NULL
	);

	--raw usages
	INSERT INTO @RawData (TX_DatabaseNumber, SizeMB, IsDocs)
	SELECT TX_DatabaseNumber
		  ,SizeMB = TX_BillableCount
		  ,IsDocs = IIF(TX_Reference1 like '%{Db.StorageDocDbSuffixSqlPattern}' 
						OR TX_Reference1 NOT LIKE 'Odyssey%', 1, 0) -- irregular database names, they were treated as eDocs previously, ex. wced-apac-001701280923-ac3syd-001-pri
	FROM [dbo].[BillingViewChargeable]
	WHERE TX_Period = @Period
	AND TX_Category = 'STL'
	AND TX_PriceItemCode = 'STS';

	-- populate #HE/#HD/#HN
	INSERT @Result (LD_PK, SubCode, UnitCount)
	SELECT  LD_PK, SubCode, UnitCount = SUM(SizeMB)
	FROM
	(
		SELECT
			LD_PK,
			SubCode = CASE WHEN LD_LicenceType = 'PRD' AND IsDocs = 1 THEN '{BillingConstants.Hosting.eDocsStorageCode}'
						   WHEN LD_LicenceType = 'PRD' AND IsDocs = 0 THEN '{BillingConstants.Hosting.DataStorageCode}'
						   ELSE '{BillingConstants.Hosting.NonProductionStorageCode}' 
					  END,
			SizeMB
		FROM @RawData RawData
		JOIN dbo.LicenceDatabase ON TX_DatabaseNumber = LD_DatabaseNumber AND LD_HostedLocation <> '{Core.Constants.LicenceConstants.NotHostedWithCargoWise}'
		JOIN
		(
			SELECT LA_LD, LA_AgreedLiveDate = MIN(ISNULL(LA_AgreedLiveDate, @PeriodStartTimeUtc)) 
			FROM dbo.LicenceHeader 
			WHERE LA_IsActive = 1 GROUP BY LA_LD
		) live ON LD_PK = LA_LD
		WHERE LD_IsActive = 1 AND LA_AgreedLiveDate < DATEADD(DAY, 15, @PeriodStartTimeUtc)
	) Usage
	GROUP BY LD_PK, SubCode;

	-- populate #HA
	INSERT @Result (LD_PK, SubCode, UnitCount)
	SELECT LD_PK, SubCode = '{BillingConstants.Hosting.UltraFastStorageCode}', UnitCount = SUM(UnitCount)
	FROM @Result where SubCode in ('{BillingConstants.Hosting.DataStorageCode}', '{BillingConstants.Hosting.eDocsStorageCode}')
	GROUP BY LD_PK;

	RETURN
END
", $"DROP FUNCTION {name}", DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region HDA Hosting Data Access

		public static DatabaseViewAndRoutineCreateScript GetHDAScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.HostingDataAccess, false);
		}

		public static DatabaseViewAndRoutineCreateScript GetHDA_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.HostingDataAccess);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, SubCode, UnitCount)
	SELECT
		TX_DatabaseNumber, TX_PriceItemCode, UnitCount = sum(TX_BillableCount)
	FROM
		dbo.BillingViewChargeable
	WHERE				
		TX_Category = 'HOS'
		AND TX_PriceItemCode IN ('#HG')
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_PriceItemCode

	INSERT @Result(DatabaseNumber, SubCode, UnitCount)
	SELECT DatabaseNumber, SubCode = '#RG', UnitCount = CEILING(CAST(UnitCount AS FLOAT) / 1024.0) --rounds up to the next whole GB, so even 1 MB becomes 1 GB.
	  FROM @Result
	 WHERE SubCode = '#HG';

	RETURN
END
",
						"drop function " + name,
						DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region AirlineMessaging

		public static DatabaseViewAndRoutineCreateScript GetAirlineMessagingScript()
		{
			string name = GetUsageFunctionOrProcedureName(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.AirlineMessaging);

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	LC_PK uniqueidentifier NULL,
	LD_PK uniqueidentifier NULL,
	LCC_PK uniqueidentifier NULL,
	SubCode varchar(50) NOT NULL,
	UnitCount int NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	
	DECLARE @Data TABLE (
		TX_DatabaseNumber int NOT NULL,
		TX_LCC uniqueidentifier,
		LegacyId VARCHAR(9),
		TX_PriceItemCode VARCHAR(3),
		TX_Reference3 varchar(50),
		TX_Reference4 varchar(50),
		UnitCount INT
	);

	-- Grab data off remote server
	INSERT @Data (TX_DatabaseNumber, TX_LCC, LegacyId, TX_PriceItemCode, TX_Reference3, TX_Reference4, UnitCount)
	SELECT TX_DatabaseNumber, TX_LCC, LegacyId = TX_ClientId, TX_PriceItemCode, TX_Reference3, TX_Reference4, UnitCount = SUM(TX_BillableCount)
	FROM 
		dbo.BillingViewChargeable
	WHERE
		TX_Category = 'AMG'
		AND TX_PriceItemCode IN ('FWB', 'FHL', 'FSU')
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC, TX_ClientId, TX_PriceItemCode, TX_Reference3, TX_Reference4;

	DECLARE @TraxonLicenceCode VARCHAR(9);
	select @TraxonLicenceCode = isnull(max(convert(nvarchar(max), SD_BinaryValue)), 'TXNFFO---')
	from dbo.StmData where SD_Name = 'AirlineMessagingTraxonLicenceIdentifier';

	declare @TraxonCompanyPk uniqueidentifier;
	select @TraxonCompanyPk = LC_PK from dbo.LicenceCompany 
	join dbo.LicenceEnterprise on LE_PK = LC_LE
	where LE_EnterpriseCode = substring(@TraxonLicenceCode, 1, 3)
		and LC_CompanyCode = substring(@TraxonLicenceCode, 4, 3);

	DECLARE @Usage TABLE (TX_DatabaseNumber int, TX_LCC uniqueidentifier, LegacyId VARCHAR(9), SubCode VARCHAR(3), UnitCount INT);

	-- Airline message usage
	insert @Usage(TX_DatabaseNumber, TX_LCC, LegacyId, SubCode, UnitCount)
	select TX_DatabaseNumber, TX_LCC, LegacyId, SubCode, UnitCount = SUM(UnitCount)
	from
	(
	SELECT 
			TX_DatabaseNumber, TX_LCC, LegacyId,
			SubCode = ( " + Billing.AirlineMessagingBillingSystem.GetUsageCodeQuery + @" ),
			UnitCount = CASE WHEN TX_PriceItemCode IN ('FWB', 'FHL', 'FSU') THEN UnitCount ELSE -UnitCount END
	FROM 
			@Data
	) a
	group by TX_DatabaseNumber, TX_LCC, LegacyId, SubCode;


	-- Traxon service (EDP, RCF) usage
	insert @Usage(TX_DatabaseNumber, TX_LCC, LegacyId, SubCode, UnitCount)
	SELECT 
		TX_DatabaseNumber, TX_LCC, LegacyId,
		SubCode = CASE
			WHEN TX_PriceItemCode = 'FWB' AND TX_Reference4 = 'TraxonEDP' THEN 'WEC'
			WHEN TX_PriceItemCode = 'FHL' AND TX_Reference4 = 'TraxonEDP' THEN 'HEC'
			WHEN TX_PriceItemCode = 'FWB' AND TX_Reference4 = 'TraxonRCF' THEN 'WRC'
			WHEN TX_PriceItemCode = 'FHL' AND TX_Reference4 = 'TraxonRCF' THEN 'HRC'
		END,
		UnitCount
	FROM 
		@Data
	WHERE
		TX_PriceItemCode IN ('FWB', 'FHL')
		AND TX_Reference4 IN ('TraxonEDP', 'TraxonRCF');

	-- Populate to result table
	INSERT @Result (SubCode, LD_PK, LCC_PK, UnitCount)
	SELECT
		SubCode,
		LD_PK,
		TX_LCC,
		UnitCount
	FROM
	(
		SELECT		TX_DatabaseNumber, TX_LCC, SubCode, SUM(UnitCount) AS UnitCount
		FROM		@Usage
		GROUP BY	TX_DatabaseNumber, TX_LCC, SubCode
		HAVING SUM(UnitCount) > 0
	) UsageSummary
	join dbo.LicenceDatabase on LD_DatabaseNumber = TX_DatabaseNumber
	where LD_LicenceType = 'PRD';


	-- Traxon
	INSERT @Result (SubCode, LC_PK, UnitCount)
	SELECT SubCode, LC_PK = @TraxonCompanyPk, UnitCount = sum(UnitCount)
	from
	(
		-- Traxon charge and remit
		SELECT
			SubCode = SubString(SubCode, 1, 2) + 
			CASE
				WHEN 
					LegacyId IN (SELECT CEX_LicenceCode collate database_default FROM dbo.ClientLicenceBillingExcludeOrg WHERE CEX_BillingSystem = '" + Billing.Business.BillingConstants.BillingSystem.AirlineMessaging + @"')
					AND
					SubCode IN ('WAC','HAC','SAC')
					THEN 'N'
				ELSE 'P'
			END,  
			UnitCount
		FROM 
			@Usage usage
		WHERE
			SubCode IN ('WAC','HAC','SAC','WXC','HXC','SXC','WEC','HEC','WRC','HRC')
	) a
	group by SubCode;

	RETURN
END
", "drop function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region NZC

		public static DatabaseViewAndRoutineCreateScript GetNZCScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.NZCustoms);
		}

		public static DatabaseViewAndRoutineCreateScript GetNZC_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.NZCustoms);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	-- get message counts
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	SELECT
		TX_DatabaseNumber, TX_LCC, TX_PriceItemCode, UnitCount = count(*)
	FROM
		dbo.BillingViewChargeable
	WHERE				
		TX_Category = 'NZC'
		AND TX_PriceItemCode IN ('NZC', 'DEC', 'CAR', 'IM1', 'EX1', 'CRE', 'OCR', 'ICR')
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region Japan Customs AFR

		public static DatabaseViewAndRoutineCreateScript GetJapanCustomsAFRScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.JapanAFR);
		}

		public static DatabaseViewAndRoutineCreateScript GetJapanCustomsAFR_Billing()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.JapanAFR);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	select TX_DatabaseNumber, TX_LCC, SubCode = TX_PriceItemCode, UnitCount = count(*)
		FROM
		dbo.BillingViewChargeable
		WHERE				
			TX_Category = 'JPC'
			AND TX_PriceItemCode IN ('AFR')
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region CMP (ClientMapping)

		public static DatabaseViewAndRoutineCreateScript GetCMPScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ClientMapping);
		}

		public static DatabaseViewAndRoutineCreateScript GetCMP_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ClientMapping);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount, Reference3)
	select TX_DatabaseNumber, TX_LCC, SubCode = TX_Reference1, UnitCount = SUM(TX_BillableCount), Reference3 = TX_Reference2
	FROM dbo.BillingViewChargeable
	WHERE
		TX_Category = 'CMP'
		AND TX_PriceItemCode = 'CMP'
		AND TX_Period = @Period
		AND TX_BillableCount > 0
	group by TX_DatabaseNumber, TX_LCC, TX_Reference2, TX_Reference1

	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount, Reference3)
	select 
		DatabaseNumber = TX_DatabaseNumber, TX_LCC, 
		SubCode = TX_Reference1, 
		UnitCount = count(*),
		Reference3 = 
			CASE TX_PriceItemCode
				WHEN 'SCO' THEN 'Order'
				WHEN 'SCS' THEN 'Shipment'
			END
	from
		dbo.BillingViewChargeable
		WHERE				
			TX_Category = 'CMP'
			AND TX_PriceItemCode IN ('SCO', 'SCS')
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC, TX_PriceItemCode, TX_Reference1

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#region ClientMappingNameSync

		public static DatabaseViewAndRoutineCreateScript GetClientMappingNameSyncScript()
		{
			return new DatabaseViewAndRoutineCreateScript("ClientMappingNameSync",
@"CREATE PROCEDURE ClientMappingNameSync
AS
BEGIN
SET NOCOUNT ON;
declare @utcStart datetime = (select ISNULL(max(cast(SD_BinaryValue as datetime)), datetimefromparts(2015, 1, 1, 0, 0, 0, 0)) from dbo.StmData where SD_Name = 'ClientMappingNameSyncLastRun')

declare @latestNames TABLE (Name varchar(50), FirstCapturedUtc DateTime2(0));
insert into @latestNames(Name, FirstCapturedUtc)
select Name, FirstCapturedUtc from dbo.BillingClientMappingInterface where FirstCapturedUtc >= @utcStart

declare @lastUtc datetime = (select max(FirstCapturedUtc) from @latestNames)

DECLARE @NameXml XML = (SELECT CONVERT(VARCHAR(MAX), SD_BinaryValue) AS XML FROM dbo.StmData WHERE SD_Name = 'ClientMappingBillingNames')

declare @allNameDesc TABLE (Name varchar(1000), Descr varchar(1000))
INSERT INTO @allNameDesc
SELECT 
	NodeTable.Item.value('Code[1]', 'varchar(1000)'),
	NodeTable.Item.value('Description[1]', 'varchar(1000)')
FROM  
	@NameXml.nodes('/NewDataSet/Table1') AS NodeTable(Item)

INSERT INTO @allNameDesc(Name, Descr)
select Name, Descr = '' from @latestNames where Name not in (select Name from @allNameDesc)

if @@ROWCOUNT > 0
begin
	declare @NewXml varchar(max) = (
		select '<?xml version=""1.0""?><NewDataSet>' +
		(
			select Table1 = convert(xml,
			(
				select
					Code = Name,
					[Description] = Descr
				FOR XML PATH('')
			))
			from @allNameDesc
			order by Name
			FOR XML PATH('')
		) + '</NewDataSet>')

	MERGE dbo.StmData
	USING (SELECT convert(varbinary(max), @NewXml), 'ClientMappingBillingNames') AS s (BinaryValue, Name)
	ON SD_name = Name
	WHEN MATCHED THEN
		UPDATE SET SD_BinaryValue = BinaryValue
		WHEN NOT MATCHED THEN
			INSERT (SD_PK, SD_Name, SD_Type, SD_BinaryValue)
				VALUES(NEWID(), Name, 'BIN', BinaryValue);

end

if @lastUtc is not null
begin
	merge dbo.StmData
	using (select convert(varbinary(max), @lastUtc), 'ClientMappingNameSyncLastRun') s(BinaryValue, Name)
	on SD_Name = Name
	when matched then
		UPDATE SET SD_BinaryValue = BinaryValue
		WHEN NOT MATCHED THEN
			INSERT (SD_PK, SD_Name, SD_BinaryValue)
				VALUES(NEWID(), Name, BinaryValue);
end
END
", "DROP PROCEDURE ClientMappingNameSync", DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion

		#endregion

		#region EAD (eAdaptor)

		public static DatabaseViewAndRoutineCreateScript GetEADScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.eAdaptor);
		}

		public static DatabaseViewAndRoutineCreateScript GetEAD_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.eAdaptor);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	select TX_DatabaseNumber, TX_LCC, SubCode = TX_PriceItemCode, UnitCount = SUM(TX_BillableCount)
	from dbo.BillingViewChargeable
	where TX_Category = 'EAD'
		and (
			(TX_PriceItemCode >= 'IC1' and TX_PriceItemCode <= 'ICL' and TX_PriceItemCode not in ('ICA','ICJ', 'ICK', 'ICL')) -- create
			or
			(TX_PriceItemCode >= 'IU1' and TX_PriceItemCode <= 'IUL' and TX_PriceItemCode not in ('IUA','IUJ', 'IUK', 'IUL')) -- update
			or
			TX_PriceItemCode = 'EAO' -- outbound
		)
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	-- Master/Ref - count each message (TX_Reference3 contains EI_SessionGUID)
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	select TX_DatabaseNumber, TX_LCC,
		SubCode = TX_PriceItemCode,
		UnitCount = count(distinct TX_Reference3)
	from dbo.BillingViewChargeable
	where TX_Category = 'EAD'
		and TX_PriceItemCode in ('ICL', 'IUL')
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	-- Order lines in excess of 10 (ref2 is the order number, ref3 is the org)
	-- Warehouse lines in excess of 10 (ref2 is the docket ID, ref3 is the warehouse)
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	select TX_DatabaseNumber, TX_LCC, SubCode = TX_PriceItemCode,
		UnitCount = count(*)
	FROM  dbo.BillingViewChargeable
		where TX_Category = 'EAD'
			and TX_PriceItemCode in ('ICA', 'IUA', 'ICJ', 'ICK', 'IUJ', 'IUK')
		and TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		public const string EAdaptorDetailedUsage = ObjectPrefix + "GetDetailedUsageEAD";

		public static DatabaseViewAndRoutineCreateScript GetEAD_DetailedUsageScript()
		{
			return new DatabaseViewAndRoutineCreateScript(EAdaptorDetailedUsage,
@"CREATE FUNCTION " + EAdaptorDetailedUsage + @"(@Period int, @DatabaseId varchar(50), @ClientCompanyPk uniqueidentifier)
RETURNS @Result TABLE
(
	TX_ClientID varchar(9) NOT NULL,
	TX_LCC uniqueidentifier NOT NULL,
	CompanyCode varchar(3) NOT NULL,
	TX_PriceItemCode varchar(3) NOT NULL,
	TX_Reference1 varchar(50) NULL,
	TX_Reference2 varchar(50) NULL,
	TX_Reference3 varchar(50) NULL,
	TX_Reference4 varchar(50) NULL,
	TX_BillableCount int not null,
	TX_ServiceOccuredUTC datetime2(7)
)
BEGIN

insert @Result(TX_ClientID, TX_LCC, CompanyCode, TX_PriceItemCode, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_BillableCount, TX_ServiceOccuredUTC)
select TX_ClientID,
	TX_LCC,
	CompanyCode = '',
	TX_PriceItemCode,
	TX_Reference1,
	TX_Reference2,
	TX_Reference3,
	TX_Reference4,
	TX_BillableCount,
	TX_ServiceOccuredUTC
from dbo.BillingViewChargeable
where
	TX_Category = 'EAD'
	and TX_SystemId = @DatabaseId 
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	and 
	(
		(TX_PriceItemCode >= 'IC1' and TX_PriceItemCode <= 'ICL' and TX_PriceItemCode not in ('ICA','ICJ', 'ICK', 'ICL')) -- create
		or
		(TX_PriceItemCode >= 'IU1' and TX_PriceItemCode <= 'IUL' and TX_PriceItemCode not in ('IUA','IUJ', 'IUK', 'IUL')) -- update
		or
		TX_PriceItemCode = 'EAO' -- outbound
	)
	AND TX_Period = @Period

-- Master/Ref - count each message (TX_Reference3 contains message ID)
insert @Result(TX_ClientID, TX_LCC, CompanyCode, TX_PriceItemCode, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_BillableCount, TX_ServiceOccuredUTC)
select TX_ClientID = MAX(TX_ClientID),
	TX_LCC,
	CompanyCode = '',
	TX_PriceItemCode,
	TX_Reference1 = '',
	TX_Reference2 = max(TX_Reference2),
	TX_Reference3,
	TX_Reference4 = '',
	TX_BillableCount = 1,
	TX_ServiceOccuredUTC = min(TX_ServiceOccuredUTC)
from 
	dbo.BillingViewChargeable
where 
	TX_Category = 'EAD'
	and TX_SystemId = @DatabaseId 
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	and TX_PriceItemCode in ('ICL', 'IUL')
	AND TX_Period = @Period
group by 
	TX_LCC, TX_PriceItemCode, TX_Reference3

-- Order lines in excess of 10 (ref1 = JO_Partno + '/' + JO_LineNo, ref2 is the order number, ref3 is the org, ref4 = EI_SessionGUID)
-- Warehouse lines in excess of 10 (ref1 = WE_LineNo, ref2 is the docket ID, ref3 is the warehouse, ref4 = EI_SessionGUID
insert @Result(TX_ClientID, TX_LCC, CompanyCode, TX_PriceItemCode, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_BillableCount, TX_ServiceOccuredUTC)
select TX_ClientID, TX_LCC, CompanyCode = '', TX_PriceItemCode, TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_BillableCount = 1, TX_ServiceOccuredUTC
FROM  dbo.BillingViewChargeable
where
	TX_Category = 'EAD'
	and TX_SystemId = @DatabaseId 
	AND (@ClientCompanyPk IS NULL OR TX_LCC = @ClientCompanyPk)
	and TX_PriceItemCode in ('ICA', 'IUA', 'ICJ', 'ICK', 'IUJ', 'IUK')
	and TX_Period = @Period


-- set company code
update @Result
set CompanyCode = LCC_Code
from
	@Result ResultTable
	join dbo.ClientCompany on TX_LCC = LCC_PK


return 
END
", "drop function " + EAdaptorDetailedUsage, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region E2E

		public static DatabaseViewAndRoutineCreateScript GetE2EScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.E2E);
		}

		public static DatabaseViewAndRoutineCreateScript GetE2E_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.E2E);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN

	DECLARE @BillingData TABLE
	(
		[TX_DatabaseNumber] [INT] NOT NULL,
		[TX_LCC] [UNIQUEIDENTIFIER] NULL,
		[TX_BillableCount] [INT] NOT NULL,
		[TX_ServiceOccuredUTC] [DATETIME2](7) NOT NULL,
		[TX_Reference1] [VARCHAR](50) NOT NULL,
		[TX_Period] [INT] NOT NULL
	);

	DECLARE @WIPSenders TABLE
	(
		[WIP_Client] [VARCHAR](50) NOT NULL,
		[WIP_CreateTimeUtc] [SMALLDATETIME]
	);

	INSERT @BillingData (TX_DatabaseNumber, TX_LCC, TX_BillableCount, TX_ServiceOccuredUTC, TX_Reference1, TX_Period)
	SELECT TX_DatabaseNumber, TX_LCC, TX_BillableCount, TX_ServiceOccuredUTC, TX_Reference1, TX_Period
	FROM dbo.BillingViewChargeable
	WHERE TX_Category = 'E2E' AND TX_PriceItemCode = 'E2E' AND TX_Period = @Period;
 
	INSERT @WIPSenders(WIP_Client, WIP_CreateTimeUtc)
	SELECT WIP_Client = TX_Reference1, WIP_CreateTimeUtc = MIN(PR_SystemCreateTimeUtc)
	FROM 
	(
		SELECT DISTINCT TX_Reference1
		FROM @BillingData
		WHERE LEN(TX_Reference1) = 9
	)AS Senders
	JOIN dbo.LicenceEnterprise ON LE_EnterpriseCode = LEFT(TX_Reference1, 3) COLLATE DATABASE_DEFAULT
	JOIN dbo.LicenceDatabase ON LD_LE = LE_PK and LD_ServerCode = RIGHT(TX_Reference1, 3) COLLATE DATABASE_DEFAULT
	JOIN dbo.EdiViewLicenceDatabaseOwner dbOwner on LicenceDatabase.LD_PK = dbOwner.LD_PK
	LEFT JOIN dbo.ClientCompany LCC ON LCC.LCC_LD = LicenceDatabase.LD_PK AND LCC.LCC_Code = SUBSTRING(TX_Reference1, 4, 3) COLLATE DATABASE_DEFAULT
	LEFT JOIN dbo.EdiViewClientCompanyLicence VCCL ON VCCL.LCC_PK = LCC.LCC_PK
	JOIN dbo.OrgRelatedParty ON PR_OH_Parent = ISNULL(VCCL.LC_OH, dbOwner.LC_OH) and PR_PartyType = 'WRP'
	JOIN dbo.OrgHeader RelatedOrg ON PR_OH_RelatedParty = RelatedOrg.OH_PK AND RelatedOrg.OH_IsUserFlag24 = 1
	GROUP BY TX_Reference1;

	INSERT @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	SELECT TX_DatabaseNumber, TX_LCC, SubCode, UnitCount = SUM(TX_BillableCount)
	FROM 
	(
		SELECT TX_DatabaseNumber, TX_LCC, TX_BillableCount,
			CASE 
			WHEN @Period >= 201705 AND WIP_CreateTimeUtc IS NOT NULL AND WIP_CreateTimeUtc <= TX_ServiceOccuredUTC THEN 'E2W'
			ELSE 'E2E'
			END AS SubCode
		FROM @BillingData BillingData
		LEFT JOIN @WIPSenders WIPSenders ON WIP_Client = TX_Reference1
	) AS BillingTab
	GROUP BY TX_DatabaseNumber, TX_LCC, SubCode;

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}
		#endregion

		#region ODM (On Demand)

		public const string ViewBillableUsage = "EdiViewBillableUsage";

		public static DatabaseViewAndRoutineCreateScript ViewBillableUsageScript()
		{
			return new DatabaseViewAndRoutineCreateScript(ViewBillableUsage, @"
CREATE VIEW EdiViewBillableUsage
with schemabinding
AS
	SELECT
		LX2_Period,
		LX2_FirstUsageUtc,
		LX2_LastUsageUtc,
		LX2_LicenceMode,
		LX2_ModuleCode,
		LX2_UsageCount,

		LCC_PK,
		LCC_Code,
		LD_PK = LCC_LD,
		LC_PK,
		LA_PK,
		LA_IsActive,

		LD_ServerCode,
		LD_HostedLocation,
		LD_IsActive,
		LD_DatabaseNumber,

		LS_PK,
		LS_Code,
		LS_FullName
	FROM
		dbo.EdiLicenceUsage
		JOIN dbo.ClientCompany ON LX2_LCC = LCC_PK
		JOIN dbo.ClientStaff ON LX2_LS = LS_PK AND LS_LD = LCC_LD
		JOIN dbo.LicenceDatabase ON LCC_LD = LD_PK
		left JOIN dbo.LicenceCompany ON LCC_OH = LC_OH
		left JOIN dbo.LicenceHeader ON LA_LD = LD_PK AND LA_LC = LC_PK
		cross apply (
			select LivePeriod = case when LA_AgreedLiveDate is null then 200001
				when DAY(LA_AgreedLiveDate) <= 15 then YEAR(LA_AgreedLiveDate) * 100 + MONTH(LA_AgreedLiveDate)
				else YEAR(DATEADD(MONTH, 1, LA_AgreedLiveDate)) * 100 + MONTH(DATEADD(MONTH, 1, LA_AgreedLiveDate))
				end
		) live
	WHERE LD_LicenceType = 'PRD'
		AND
		(
			LA_AgreedLiveDate IS NULL
			OR
			(
				LX2_LastUsageUtc >= LA_AgreedLiveDate
				AND
				LX2_Period >= LivePeriod
			)
		)
", "DROP VIEW EdiViewBillableUsage", DbRoutineType.SqlViewTypeDesc);
		}

		public const string OdmMonthlyModuleBundleUsers = "EdiOdmMonthlyModuleBundleUsers";

		public static DatabaseViewAndRoutineCreateScript GetOdmMonthlyModuleBundleUsersScript()
		{
			return new DatabaseViewAndRoutineCreateScript(OdmMonthlyModuleBundleUsers,
@"CREATE FUNCTION " + OdmMonthlyModuleBundleUsers + @"(@FirstDayOfMonth smalldatetime)
RETURNS TABLE
with schemabinding
AS RETURN
(
	select LC_PK = h2.LA_LC, usage.LD_PK, usage.LCC_PK, LX2_ModuleCode = ISNULL(ParentOrChildCode, LX2_ModuleCode), LS_PK, LS_Code, LS_FullName, L6_PK, L6_ValidFrom, items.L7_PK
	from
	(
		select LD_PK, LCC_PK, LX2_ModuleCode, LS_PK, LS_Code, LS_FullName
		from dbo.EdiViewBillableUsage
		where LX2_Period = YEAR(@FirstDayOfMonth) * 100 + MONTH(@FirstDayOfMonth)
			and LX2_LicenceMode = 'ODM'
		group by LD_PK, LCC_PK, LX2_ModuleCode, LS_PK, LS_Code, LS_FullName
	) usage
	left join dbo.EdiViewClientCompanyLicence vw on vw.LCC_PK = usage.LCC_PK
	left join dbo.LicenceHeader h2 on h2.LA_PK = vw.LA_PK
	left join dbo.EdiGetOdmPriceHeadersForDate(@FirstDayOfMonth) priceHeader on priceHeader.LA_PK = h2.LA_PK
	outer apply
	(
		select top 1 L7_PK, L7_ParentCode = case when L7_ParentCode != 'COR' then L7_ParentCode else '' end
		from dbo.ClientLicencePriceItem where L7_L6 = L6_PK and L7_Category = 'ODM' and L7_Code = LX2_ModuleCode and L7_UnitBreak = 0 and L7_Code <> ''
	) item
	outer apply
	(
		-- For groups, return usage under both the child (included) code and the group (parent) code
		-- since the particular child with usage can be significant
		-- while the price is under the parent code
		-- Can ignore COR children since doesn't apply to COR.
		select item.L7_PK, ParentOrChildCode = LX2_ModuleCode
		union all
		select top 1 parent.L7_PK, ParentOrChildCode = item.L7_ParentCode from dbo.ClientLicencePriceItem parent 
		where item.L7_ParentCode != ''
			and parent.L7_L6 = L6_PK and item.L7_ParentCode = parent.L7_Code and parent.L7_Category = 'ODM' and parent.L7_UnitBreak = 0 and parent.L7_Code <> ''
	) items
)", "DROP FUNCTION " + OdmMonthlyModuleBundleUsers, DbRoutineType.SqlFunctionInlineTypeDesc);
		}

		public static DatabaseViewAndRoutineCreateScript GetODMScript()
		{
			string name = GetUsageFunctionOrProcedureName(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ODM);
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE PROCEDURE " + name + @"
	@Period int, 
	@PeriodStartTimeUtc smalldatetime, 
	@PeriodEndTimeUtc smalldatetime
AS
BEGIN

	declare @FirstDayOfMonth smalldatetime = (select convert(smalldatetime, convert(varchar(6), @Period) + '01'))

	-- On Demand usages
	select
		LC_PK = cast(NULL as uniqueidentifier),
		LD_PK = u.LD_PK,
		LCC_PK,
		SubCode = LX2_ModuleCode,
		UnitCount = count(distinct case when LS_Code != '' then LS_Code else LS_FullName end)
	into
		#OnDemandUsages
	from dbo.EdiOdmMonthlyModuleBundleUsers(@FirstDayOfMonth) u
	group by LD_PK, LCC_PK, u.LC_PK, LX2_ModuleCode

	-- Calculate registered users (login users + active users don't login)
	if @Period >= 201611
	begin
		select distinct 
			TX_LCC, TX_Reference1, TX_DatabaseNumber
		into #ActiveUsers
		from 
			dbo.BillingViewChargeable
		where
			TX_Period = @Period
			and TX_Category = 'STL'
			and TX_PriceItemCode = 'USR'

		select LCC_PK, LS_PK, LX2_FirstUsageUtc, LS_Code = case when LS_Code != '' then LS_Code else LS_FullName end, LD_DatabaseNumber
		into #LoginUsers
		from dbo.EdiViewBillableUsage
		where
			LX2_ModuleCode = 'COR'
			and LX2_LicenceMode = 'ODM'
			and LX2_Period = @Period
		
		create index LoginUsersIdx on #LoginUsers (LD_DatabaseNumber, LS_Code);

		-- copy COR to USO and add any USR not in COR
		insert into #OnDemandUsages
		select LC_PK = cast(NULL as uniqueidentifier), LD_PK = LCC_LD, LCC_PK = LCC_PK, SubCode = 'USO', UnitCount = COUNT(*)
		from 
			(
				select TX_LCC, TX_Reference1
				from #ActiveUsers
				where not exists (select 1 from #LoginUsers where LS_Code = TX_Reference1 and LD_DatabaseNumber = TX_DatabaseNumber)

				union

				select LCC_PK, LS_Code from #LoginUsers
			) a 
			join dbo.ClientCompany on TX_LCC = LCC_PK
			join dbo.LicenceDatabase on LCC_LD = LD_PK
		where
			LD_LicenceType = 'PRD'
		group by
			LCC_PK, LCC_LD

		-- For hosted systems,
		-- copy first COR for each staff => COW 
		-- add any USR not in COR
		if @Period >= 201806 
		begin
			insert into #OnDemandUsages(LC_PK, LD_PK, LCC_PK, SubCode, UnitCount)
			select LC_PK = cast(NULL as uniqueidentifier), LD_PK = LCC_LD, LCC_PK, SubCode = 'COW', UnitCount = COUNT(*)
			from 
			(
				select TX_LCC, TX_Reference1
				from #ActiveUsers
				where not exists (select 1 from #LoginUsers where LS_Code = TX_Reference1 and LD_DatabaseNumber = TX_DatabaseNumber)

				union

				select LCC_PK, LS_Code
				from
				(
					select LD_DatabaseNumber, LCC_PK, LS_Code, LoginSeq = row_number() over (partition by LD_DatabaseNumber, LS_Code order by LX2_FirstUsageUtc, LCC_PK)
					from #LoginUsers
				) orderedLogins
				where LoginSeq = 1
			) a 
			join dbo.ClientCompany on TX_LCC = LCC_PK
			join dbo.LicenceDatabase on LCC_LD = LD_PK
			where
				LD_LicenceType = 'PRD'
				and LD_HostedLocation in (select Code from " + BillingUsageSchema.GetDatabaseBillingHostedLocations + @"() where isCW = 'Y')
			group by
				LCC_PK, LCC_LD
		end

		drop table #ActiveUsers
		drop table #LoginUsers
	end

	--Country Usages (DCC/MCC)
	if @Period >= 201710 
	begin
		declare @CountryUsages table (LD_PK uniqueidentifier, LCC_PK uniqueidentifier);

		insert into @CountryUsages(LD_PK, LCC_PK)
		select LD_PK, LCC_PK
		from
		(
			--the client company in each country (if there are more than one in a country, pick the one with most COR usage, then first created)
			select 
				LD_PK = LCC_LD,
				ODM.LCC_PK,
				row_number() over(partition by LCC_LD, isnull(BCG_MainCountryCode, LCC_RN_NKCountryCode) order by UnitCount desc, LCC_CreateTimeUtc, LCC_Code) as Row#
			from
			(		
				select LCC_PK, UnitCount = sum(UnitCount)
				from
				(
					-- if the company has COR usage in the month.
					select LCC_PK, UnitCount
					from #OnDemandUsages 
					where SubCode = 'COR'
					union all
					-- if the company is reported as active the month
					select LCC_PK = CSH_LCC, UnitCount = 0
					from dbo.ClientCompanyActiveStatusHistory where CSH_Period = @Period
				) u
				group by LCC_PK
			) ODM
			join dbo.ClientCompany on ClientCompany.LCC_PK = ODM.LCC_PK and LCC_RN_NKCountryCode <> ''
			left join
			(
				select BCG_CountryCode, BCG_MainCountryCode
				from " + BillingUsageSchema.GetBillingCountryGroups + @"()
				where BDG_IsDomesticUserGroup = 'Y'
			) CountryGroups on LCC_RN_NKCountryCode = BCG_CountryCode
		)tb1 where Row# = 1;
 
		insert into #OnDemandUsages(LC_PK, LD_PK, LCC_PK, SubCode, UnitCount)
		select LC_PK = null, LD_PK, LCC_PK, SubCode, UnitCount = 1
		from @CountryUsages Usages
		join
		(
			--DCC/MCC (DCC if only one country has usage in the database, MCC otherwise)
			select LD_PK LD, case when count(*) = 1 then 'DCC' else 'MCC' end SubCode from @CountryUsages group by LD_PK
		) CountryUsageSubCodes on LD_PK = LD;

		--GPC
		insert into #OnDemandUsages(LC_PK, LD_PK, LCC_PK, SubCode, UnitCount)
		select LC_PK = null, LD_PK, LCC_PK, SubCode = 'GPC', UnitCount = 1
		from @CountryUsages Usages;
	end

	-- Return result
	select *,
		Reference1 = cast('' as varchar(50)),
		Reference2 = cast('' as varchar(50)),
		Reference3 = cast('' as varchar(50)),
		Reference4 = cast('' as varchar(50))
	from #OnDemandUsages

	drop table #OnDemandUsages

END", "DROP PROCEDURE " + name, DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion

		#region CPT

		public static DatabaseViewAndRoutineCreateScript GetCPTScript()
		{
			return GetUsageCreateScript(Enterprise.Licensing.LicenceTypes.Codes.CPT,
@"	SELECT
		LC_PK,
		LD_PK,
		LCC_PK,
		SubCode = LX2_ModuleCode,
		UnitCount = sum(LX2_UsageCount),
		Reference1 = cast(NULL as varchar(50)),
		Reference2 = cast(NULL as varchar(50)),
		Reference3 = cast(NULL as varchar(50)),
		Reference4 = cast(NULL as varchar(50))
	FROM
		dbo.EdiLicenceUsage
		JOIN dbo.ClientCompany ON LX2_LCC = LCC_PK
		JOIN dbo.ClientStaff ON LX2_LS = LS_PK AND LS_LD = LCC_LD
		JOIN dbo.LicenceDatabase ON LCC_LD = LD_PK
		left join dbo.LicenceCompany on LCC_OH = LC_OH
	WHERE
		LX2_Period = @Period
		AND LX2_LicenceMode = 'CPT'
		-- ignore usage from modules that are billed from eHub/eRouter, and not LicenceUsage
		AND LX2_ModuleCode not in ('DCS', 'DPS', 'FAX', 'IQM', 'ISF', 'RSH', 'VIM')
		AND LD_LicenceType = 'PRD'
	GROUP BY LD_PK, LCC_PK, LC_PK, LX2_ModuleCode
");
		}

		#endregion

		#region USC (US Customs)

		public static DatabaseViewAndRoutineCreateScript GetUSCScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.USCustoms);
		}

		public static DatabaseViewAndRoutineCreateScript GetUSC_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.USCustoms);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	select TX_DatabaseNumber, TX_LCC, SubCode = 'USC', UnitCount = count(*)
	FROM
		dbo.BillingViewChargeable
	WHERE
		TX_Category = 'USC'
		AND TX_PriceItemCode in ('UJL','UPL','URB','UQT','URR','USO','UFT','UXT')
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC

	RETURN
END", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region RIC (Railinc by message)

		public static DatabaseViewAndRoutineCreateScript GetRICScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.RailincByMessage);
		}

		public static DatabaseViewAndRoutineCreateScript GetRIC_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.RailincByMessage);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN

	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	SELECT 
		DatabaseNumber = TX_DatabaseNumber, TX_LCC, SubCode = 'RIC', UnitCount = COUNT(*)
	FROM
		dbo.BillingViewChargeable
	WHERE
		TX_Category = 'RIM'
		AND TX_PriceItemCode = 'RIC' 
		AND TX_Period = @Period
	GROUP BY 
		TX_DatabaseNumber, TX_LCC

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region OCT (Ocean Tracing)

		public static DatabaseViewAndRoutineCreateScript GetOCTScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.OceanTracing);
		}

		public static DatabaseViewAndRoutineCreateScript GetOCT_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.OceanTracing);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	SELECT 
		DatabaseNumber = TX_DatabaseNumber, TX_LCC, SubCode = TX_PriceItemCode, UnitCount = count(*)
	FROM 
		dbo.BillingViewChargeable
	WHERE
		TX_Category = 'OCT'
		AND TX_PriceItemCode = 'OCT'
		AND TX_Period = @Period
	GROUP BY 
		TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region Forward Air

		public static DatabaseViewAndRoutineCreateScript GetFWAScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ForwardAir);
		}

		public static DatabaseViewAndRoutineCreateScript GetFWA_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ForwardAir);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	SELECT 
		DatabaseNumber = TX_DatabaseNumber, TX_LCC, SubCode = TX_PriceItemCode, UnitCount = count(*)
	FROM 
		dbo.BillingViewChargeable
	WHERE
		TX_Category = 'FWA'
		AND TX_PriceItemCode = 'FWA'
		AND TX_Period = @Period
	GROUP BY 
		TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region SPM (Shipping Port Messaging)

		public static DatabaseViewAndRoutineCreateScript GetSPMScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ShippingPortMessaging);
		}

		public static DatabaseViewAndRoutineCreateScript GetSPM_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ShippingPortMessaging);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
			SELECT 
		DatabaseNumber = TX_DatabaseNumber, TX_LCC, SubCode = TX_PriceItemCode, UnitCount = SUM(TX_BillableCount)
			FROM
		dbo.BillingViewChargeable
			WHERE
				TX_Category = 'SPM'
		AND TX_PriceItemCode in ('SPA', 'SPE', 'SPR')
		AND TX_Period = @Period
	GROUP BY
		TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region GBC (GB Customs)

		public static DatabaseViewAndRoutineCreateScript GetGBCScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.GBCustoms);
		}

		public static DatabaseViewAndRoutineCreateScript GetGBC_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.GBCustoms);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	SELECT 
		DatabaseNumber = TX_DatabaseNumber, TX_LCC, SubCode = TX_PriceItemCode, UnitCount = COUNT(*)
	FROM 
		dbo.BillingViewChargeable
	WHERE
		TX_Category = 'GBC'
		AND TX_PriceItemCode IN ('AWB', 'GTM', 'CUE')
		AND TX_Period = @Period
	GROUP BY 
		TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region ZAC (ZA Customs)

		public static DatabaseViewAndRoutineCreateScript GetZACScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ZACustoms);
		}

		public static DatabaseViewAndRoutineCreateScript GetZAC_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ZACustoms);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	SELECT 
		DatabaseNumber = TX_DatabaseNumber, TX_LCC, SubCode = TX_PriceItemCode, UnitCount = COUNT(*)
	FROM 
		dbo.BillingViewChargeable
	WHERE
		TX_Category = 'ZAC'
		AND TX_PriceItemCode IN ('ZX1', 'ZX2', 'ZX3')
		AND TX_Period = @Period
	GROUP BY 
		TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region ISF (Importer Security Filing)

		public static DatabaseViewAndRoutineCreateScript GetISFScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ImporterSecurityFiling);
		}

		public static DatabaseViewAndRoutineCreateScript GetISF_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ImporterSecurityFiling);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
		SELECT
		DatabaseNumber = TX_DatabaseNumber, TX_LCC, SubCode = 'ISF', UnitCount = COUNT(*)
		FROM
		dbo.BillingViewChargeable
		WHERE
			TX_Category = 'USC'
			AND TX_PriceItemCode = 'ISF'
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region ASC (ASYCUDA)

		public static DatabaseViewAndRoutineCreateScript GetASCScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ASYCUDA);
		}

		public static DatabaseViewAndRoutineCreateScript GetASC_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.ASYCUDA);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount INT NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
		SELECT
		DatabaseNumber = TX_DatabaseNumber, TX_LCC, SubCode = 'ASC', UnitCount = COUNT(*)
		FROM
		dbo.BillingViewChargeable
		WHERE
			TX_Category = 'ASC'
			AND TX_PriceItemCode = 'ASC'
		AND TX_Period = @Period
	group by TX_DatabaseNumber, TX_LCC

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region STL

		public static DatabaseViewAndRoutineCreateScript GetStlScript()
		{
			return GetUsageConverterFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.STL);
		}

		public static DatabaseViewAndRoutineCreateScript GetStl_BillingScript()
		{
			string name = GetUsageFromBilling(Enterprise.Client.EDI.Billing.Business.BillingConstants.BillingSystem.STL);

			// Using non-inline function so external database need not exist for function to be created.
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	DatabaseNumber int NOT NULL,
	LCC_PK uniqueidentifier,
	SubCode varchar(50) NOT NULL,
	UnitCount decimal(14, 4) NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL,
	TotalPrice money NOT NULL DEFAULT(0),
	RX_NKCurrency varchar(3) NOT NULL DEFAULT (''),
	Direction varchar(3) NOT NULL DEFAULT ('')
)
BEGIN
	insert @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
	select TX_DatabaseNumber, TX_LCC, SubCode = TX_PriceItemCode, UnitCount
	from
	(
		select TX_DatabaseNumber, TX_LCC, TX_PriceItemCode, UnitCount = sum(TX_BillableCount)
		FROM
				dbo.BillingViewChargeable
		WHERE
			TX_Category = 'STL'
			AND TX_Period = @Period
			AND TX_PriceItemCode != 'STL' AND TX_PriceItemCode != 'USR'
		group by TX_DatabaseNumber, TX_LCC, TX_PriceItemCode

		union all

		select TX_DatabaseNumber, TX_LCC, SubCode,
			UnitCount = case when HasLastMilestone = 1 then 1 else n + 1 end
		from
		(
			select TX_DatabaseNumber, TX_LCC, SubCode = 'STL', 
				HasLastMilestone = MAX(case when TX_ServiceOccuredUTC > DATEADD(MINUTE, -30, @PeriodEndTimeUtc) then 1 else 0 end),
				n = count(*)
			FROM
					dbo.BillingViewChargeable
			WHERE
				TX_Category = 'STL'
				AND TX_Period = @Period
				AND TX_PriceItemCode = 'STL'
				AND TX_Period >= 201611
				AND DATEPART(MINUTE, TX_ServiceOccuredUTC) = 59
			group by TX_DatabaseNumber, TX_LCC
		) a
	) remotePart

	-- Disbursement
	DECLARE @DisbursementMapping TABLE
	(
		UDM_DisbursementCode VARCHAR(50) NOT NULL,
		UDM_UsageCodes VARCHAR(500) NOT NULL
	);
	INSERT INTO @DisbursementMapping(UDM_DisbursementCode, UDM_UsageCodes)
	SELECT UDM_DisbursementCode, UDM_UsageCodes FROM dbo.EdiGetBillingDisbursementUsageMappings();
	
	IF EXISTS (SELECT TOP 1 * FROM @DisbursementMapping)
	BEGIN

		-- remove disbursement codes 
		DELETE R
		FROM @Result R
		WHERE R.SubCode IN
		(
			SELECT UDM_DisbursementCode FROM @DisbursementMapping
			UNION
			SELECT TRIM(VALUE) AS SplitValue
			FROM @DisbursementMapping
			CROSS APPLY STRING_SPLIT(UDM_UsageCodes, ',')
		);

		DECLARE @UDM_DisbursementCode VARCHAR(50), @UDM_UsageCodes VARCHAR(500);
		DECLARE DisbursementMappingCursor CURSOR FOR
		SELECT UDM_DisbursementCode, UDM_UsageCodes FROM @DisbursementMapping;

		OPEN DisbursementMappingCursor;
		FETCH NEXT FROM DisbursementMappingCursor INTO @UDM_DisbursementCode, @UDM_UsageCodes;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount, TotalPrice, RX_NKCurrency, Direction)
			SELECT DatabaseNumber, LCC_PK, SubCode, UnitCount, TotalPrice, RX_NKCurrency, Direction
			FROM dbo.EdiGetChargeableUsageFromBillingSTLDisbursement(@Period, @UDM_DisbursementCode, @UDM_UsageCodes)
			FETCH NEXT FROM DisbursementMappingCursor INTO @UDM_DisbursementCode, @UDM_UsageCodes;
		END

		CLOSE DisbursementMappingCursor;
		DEALLOCATE DisbursementMappingCursor;
	END

	--USR
	BEGIN
		DECLARE @ResultUSR TABLE
		(
			TX_DatabaseNumber INT NOT NULL,
			TX_LCC UNIQUEIDENTIFIER,
			TX_PriceItemCode VARCHAR(3) NOT NULL,
			TX_BillableCount DECIMAL(14, 4) NOT NULL,
			TX_ClientStaffCode VARCHAR (3) NULL,
			INDEX INDEX1 NONCLUSTERED(TX_DatabaseNumber, TX_PriceItemCode, TX_ClientStaffCode) WITH (ALLOW_PAGE_LOCKS = OFF)
		);

		INSERT INTO @ResultUSR(TX_DatabaseNumber, TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_ClientStaffCode)
		SELECT TX_DatabaseNumber, TX_LCC, TX_PriceItemCode, TX_BillableCount, TX_ClientStaffCode
		FROM dbo.BillingViewChargeable
		WHERE TX_Category = 'STL'
			AND TX_Period = @Period
			AND (TX_PriceItemCode = 'USR' OR TX_PriceItemCode = 'RBU');

		INSERT @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
		SELECT TX_DatabaseNumber, TX_LCC, TX_PriceItemCode, UnitCount = SUM(TX_BillableCount)
		FROM @ResultUSR USR
		WHERE TX_PriceItemCode = 'USR' AND NOT EXISTS 
		(
			SELECT 1 FROM @ResultUSR RBU WHERE 
			RBU.TX_PriceItemCode = 'RBU' AND
			RBU.TX_DatabaseNumber = USR.TX_DatabaseNumber AND 
			RBU.TX_ClientStaffCode = USR.TX_ClientStaffCode
		)
		GROUP BY TX_DatabaseNumber, TX_LCC, TX_PriceItemCode;
	END  --USR END

	--WTU
	BEGIN
		DECLARE @wtuAdjustment TABLE
		(
			ADJ_OriginalUnitCount INT NOT NULL PRIMARY KEY,
			ADJ_AdjustedUnitCount DECIMAL(14,4) NOT NULL,
			ADJ_DefaultAdjustedIncrement DECIMAL(14,4) NOT NULL
		);
		INSERT @wtuAdjustment(ADJ_OriginalUnitCount, ADJ_AdjustedUnitCount, ADJ_DefaultAdjustedIncrement)
		SELECT ADJ_OriginalUnitCount, ADJ_AdjustedUnitCount, ADJ_DefaultAdjustedIncrement FROM dbo.EdiGetBillingUnitCountAdjustments() WHERE ADJ_PriceCode = 'WTU';

		IF EXISTS (SELECT * FROM @wtuAdjustment)
		BEGIN
			DELETE @Result WHERE SubCode = 'WTU';

			DECLARE @wtuUsage TABLE
			(
				WTU_DatabaseNumber INT NOT NULL,
				WTU_TotalTxCount INT NOT NULL,
				WTU_BilledTxCount INT NOT NULL,
				WTU_AdjustedUnitCount DECIMAL(14, 4) NOT NULL
			);

			INSERT @wtuUsage(WTU_DatabaseNumber, WTU_TotalTxCount, WTU_BilledTxCount, WTU_AdjustedUnitCount)
			SELECT TX_DatabaseNumber, WTU_Total, (WTU_Total - WTU_TxCount) AS WTU_BilledTxCount, 0
			FROM
			(
				SELECT TX_DatabaseNumber, MAX(CAST(TX_Reference4 AS INT)) AS WTU_Total, COUNT(*) AS WTU_TxCount
				FROM dbo.BillingViewChargeable
				WHERE TX_Category = 'STL'
					AND TX_Period = @Period
					AND TX_PriceItemCode = 'WTU'
					AND TX_BillableCount > 0
					AND TX_Reference3 is not null
					AND TX_Reference3 <> ''
					AND ISNUMERIC(TX_Reference4) = 1
				GROUP BY TX_DatabaseNumber, TX_Reference3
			) wtu
			WHERE WTU_Total >= WTU_TxCount;

			DECLARE @adjMaxUnitCount INT;
			DECLARE @adjMaxAdjustedUnitCount DECIMAL(14,4);
			DECLARE @adjDefaultIncrement DECIMAL(14,4);
			SELECT TOP 1 @adjMaxUnitCount = ADJ_OriginalUnitCount, @adjMaxAdjustedUnitCount = ADJ_AdjustedUnitCount, @adjDefaultIncrement = ADJ_DefaultAdjustedIncrement
			FROM @wtuAdjustment ORDER BY ADJ_OriginalUnitCount DESC;

			UPDATE wtuUsage
			SET WTU_AdjustedUnitCount = ISNULL(wtuAdjmtTotal.ADJ_AdjustedUnitCount, 
												@adjMaxAdjustedUnitCount + (WTU_TotalTxCount - @adjMaxUnitCount) * @adjDefaultIncrement)
										-
										IIF(WTU_BilledTxCount = 0, 0, 
												ISNULL(wtuAdjmtBilledTxCount.ADJ_AdjustedUnitCount, 
													@adjMaxAdjustedUnitCount + (WTU_BilledTxCount - @adjMaxUnitCount) * @adjDefaultIncrement))
			FROM @wtuUsage wtuUsage
			LEFT JOIN @wtuAdjustment wtuAdjmtTotal ON wtuUsage.WTU_TotalTxCount = wtuAdjmtTotal.ADJ_OriginalUnitCount
			LEFT JOIN @wtuAdjustment wtuAdjmtBilledTxCount ON wtuUsage.WTU_BilledTxCount = wtuAdjmtBilledTxCount.ADJ_OriginalUnitCount;

			INSERT @Result(DatabaseNumber, SubCode, UnitCount)
			SELECT WTU_DatabaseNumber, 'WTU', UnitCount = SUM(WTU_AdjustedUnitCount)
			FROM @wtuUsage wtuUsage
			WHERE WTU_AdjustedUnitCount > 0
			GROUP BY WTU_DatabaseNumber;
		END
	END --WTU END

	--RF2 -> BYO
	BEGIN

		DECLARE @BillingStartDate DATE = DATEFROMPARTS(@Period / 100, @Period % 100, 1);
		DECLARE @RF2 TABLE
		(
			RF2_DBNum INT NOT NULL PRIMARY KEY,
			RF2_DeviceCount INT NOT NULL
		);

		INSERT INTO @RF2(RF2_DBNum, RF2_DeviceCount)
		SELECT TX_DatabaseNumber, SUM(TX_BillableCount) AS DeviceCount
		FROM dbo.BillingViewChargeable
		WHERE TX_Category = 'STL'
			AND TX_Period = @Period
			AND TX_PriceItemCode = 'RF2'
			AND TX_BillableCount > 0
			AND TX_Reference1 IS NOT NULL
			AND TX_Reference1 <> ''
		GROUP BY TX_DatabaseNumber;

		INSERT @Result(DatabaseNumber, SubCode, UnitCount)
		SELECT BYO_DBNum, 'BYO', BYO_UnitCount
		FROM
		(
			SELECT RF2_DBNum AS BYO_DBNum, RF2_DeviceCount - ISNULL(PRM_UnitCount, 0) - ISNULL(BYR_UnitCount, 0) AS BYO_UnitCount
			FROM @RF2 RF2
			LEFT JOIN 
			(
				SELECT LD_DatabaseNumber AS PRM_DBNum, SUM(CPS_Units) AS PRM_UnitCount
				FROM dbo.ClientPremiumService
				JOIN dbo.LicenceDatabase ON LD_PK = CPS_LD
				AND CPS_Type IN (SELECT Code FROM EdiGetHandheldDevicePremiumTypes())
				AND (
						(CPS_StartDate IS NULL OR @BillingStartDate >= CPS_StartDate)
						AND (CPS_EndDate IS NULL OR @BillingStartDate < CPS_EndDate )
					)
				GROUP BY LD_DatabaseNumber
			) PRM ON PRM_DBNum = RF2_DBNum
			LEFT JOIN 
			(
				SELECT LD_DatabaseNumber AS BYR_DBNum, SUM(CPS_Units) AS BYR_UnitCount
				FROM dbo.ClientPremiumService
				JOIN dbo.LicenceDatabase ON LD_PK = CPS_LD
				AND CPS_Type = 'BYR'
				AND (
						(CPS_StartDate IS NULL OR @BillingStartDate >= CPS_StartDate)
						AND (CPS_EndDate IS NULL OR @BillingStartDate < CPS_EndDate )
					)
				GROUP BY LD_DatabaseNumber
			) BYR ON BYR_DBNum = RF2_DBNum
		) BYO
		WHERE BYO_UnitCount > 0;

	END --RF2 -> BYO

	declare @stdLicenceCode varchar(9) = ISNULL((select cast(sd_binaryvalue as nvarchar(9)) from dbo.stmdata where sd_name = 'StandardPriceCompanyLicenceIdentifier'), 'EDIEDISYD')

	declare @stdCompanyPk uniqueidentifier =
		(
			select LC_PK
			from dbo.LicenceCompany
			join dbo.LicenceEnterprise on LC_LE = LE_PK
			where LC_CompanyCode = substring(@stdLicenceCode, 4, 3)
				and LE_EnterpriseCode = LEFT(@stdLicenceCode, 3)
		)

	declare @STLCodesInUse table
	(
		Code varchar(3) not null
		PRIMARY KEY CLUSTERED (Code) WITH (ALLOW_PAGE_LOCKS = OFF)
	)
	insert @STLCodesInUse(Code)
	select distinct L7_Code
	from dbo.ClientLicencePriceItem 
	join dbo.ClientLicencePriceHeader on L7_L6 = L6_PK
	where L6_LC = @stdCompanyPk
		and L7_L6 != '66803A9D-C5CD-4F97-A932-9B343D744F0C' -- Dalmo special list is not a real pricelist, just a reference of codes in use
		and L6_SystemCode IN ('STL', 'CWN')
		and L7_Code != ''
		and L7_Category IN ('STL', 'CWN')
			
	union

	select distinct PUM_UsageCode
	from dbo.ClientLicencePriceHeader
	join dbo.EdiPriceUsageMapping on PUM_L6 = L6_PK
	where L6_LC = @stdCompanyPk
		and L6_PK != '66803A9D-C5CD-4F97-A932-9B343D744F0C'
		and L6_SystemCode IN ('STL', 'CWN')
		and PUM_UsageCategory = 'STL'

	-- remove any codes not on a standard STL pricelist
	delete from @Result
	where SubCode not in (select Code from @STLCodesInUse)
		and SubCode != 'STL' -- always keep milestones

	-- only keep active LD/LA for PRS/PRT
	delete from @Result
	where SubCode in ('PRS', 'PRT')
	and DatabaseNumber not in
	(
		select LD_DatabaseNumber from dbo.LicenceDatabase
		join
		(
			select LA_LD, LA_AgreedLiveDate = MIN(ISNULL(LA_AgreedLiveDate, @PeriodStartTimeUtc)) from dbo.LicenceHeader where LA_IsActive = 1 group by LA_LD
		) live on LD_PK = LA_LD
		where LD_IsActive = 1 AND LD_LicenceType = 'PRD' and LA_AgreedLiveDate < DATEADD(DAY, 15, @PeriodStartTimeUtc)
	);

	-- USR + RBU => USW
	if @Period >= 201806 
	begin
		insert into @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
		select DatabaseNumber, LCC_PK, SubCode = 'USW', UnitCount = SUM(UnitCount)
		from @Result
		join dbo.LicenceDatabase on LD_DatabaseNumber = DatabaseNumber
		where SubCode IN ('USR', 'RBU') and LD_HostedLocation in (select Code from " + BillingUsageSchema.GetDatabaseBillingHostedLocations + @"() where isCW = 'Y')
		group by DatabaseNumber, LCC_PK;
	end

	-- USR => UCN
	if @Period >= 202205 
	begin
		insert into @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
		select DatabaseNumber, LCC_PK, SubCode = 'UCN', UnitCount
		from @Result
		join dbo.LicenceDatabase on LD_DatabaseNumber = DatabaseNumber
		where SubCode = 'USR' and LD_HostedLocation = 'CN1';
	end

	-- USR + RBU => UCS
	if @Period >= 202205 
	begin
		insert into @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
		select DatabaseNumber, LCC_PK, SubCode = 'UCS', UnitCount = SUM(UnitCount)
		from @Result
		join dbo.LicenceDatabase on LD_DatabaseNumber = DatabaseNumber
		where SubCode IN ('USR', 'RBU') and LD_HostedLocation in (select Code from " + BillingUsageSchema.GetDatabaseBillingHostedLocations + @"() where isCW = 'Y')
		group by DatabaseNumber, LCC_PK;
	end

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		public static DatabaseViewAndRoutineCreateScript GetBillingDisbursementUsageMappingsScript()
		{
			const string name = "EdiGetBillingDisbursementUsageMappings";
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"()
RETURNS @Result TABLE
(
	UDM_DisbursementCode VARCHAR(50) NOT NULL,
	UDM_UsageCodes VARCHAR(500) NOT NULL,
	PRIMARY KEY (UDM_DisbursementCode)
)
BEGIN
	DECLARE @regXmldata XML = (SELECT CAST(CONVERT(NVARCHAR(MAX), SD_BinaryValue) AS XML) FROM dbo.StmData WHERE SD_Name = 'BILLINGDISBURSEMENTUSAGEMAPPINGS');

	IF @regXmldata IS NOT NULL
	BEGIN
		INSERT INTO @Result ( UDM_DisbursementCode, UDM_UsageCodes )
		SELECT Code = NodeTable.Item.value('Code[1]', 'varchar(50)')
			,Description = NodeTable.Item.value('Description[1]', 'varchar(500)')
		FROM @regXmldata.nodes('/ArrayOfCodeDescriptionBool/CodeDescriptionBool') AS NodeTable(Item);
	END
RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		public static DatabaseViewAndRoutineCreateScript GetChargeableUsageFromBillingSTLDisbursementScript()
		{
			const string name = "EdiGetChargeableUsageFromBillingSTLDisbursement";
			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period INT, @DisbursementCode VARCHAR(50), @UsageCodes VARCHAR(500))
RETURNS @Result TABLE
(
	DatabaseNumber INT NOT NULL,
	LCC_PK UNIQUEIDENTIFIER,
	SubCode VARCHAR(50) NOT NULL,
	UnitCount DECIMAL(14, 4) NOT NULL,
	TotalPrice MONEY NOT NULL DEFAULT(0),
	RX_NKCurrency VARCHAR(3) NOT NULL DEFAULT (''),
	Direction VARCHAR(3) NOT NULL DEFAULT ('')
)
BEGIN

	DECLARE @Disbursement TABLE
	(
		DatabaseNumber INT NOT NULL,
		LCC_PK UNIQUEIDENTIFIER,
		UnitCount DECIMAL(14, 4) NOT NULL,
		JR_RX_NKCostCurrency VARCHAR(3) NOT NULL DEFAULT (''),
		JR_OSCostAmt MONEY NOT NULL DEFAULT(0),
		JH_Direction VARCHAR(3) NOT NULL DEFAULT (''),
		JH_PK UNIQUEIDENTIFIER
	);

	INSERT @Disbursement(DatabaseNumber, LCC_PK, UnitCount, 
			JR_RX_NKCostCurrency, JR_OSCostAmt, JH_Direction, JH_PK)
	SELECT TX_DatabaseNumber, TX_LCC, TX_BillableCount, 
			JR_RX_NKCostCurrency = ISNULL(LEFT(TX_Reference2, 3), ''), 
			JR_OSCostAmt = ISNULL(TRY_CAST(TX_Reference3 AS MONEY), 0),
			JH_Direction = ISNULL(LEFT(TX_Reference4, 3), ''), 
			JH_PK = TRY_CAST(TX_Reference5 AS UNIQUEIDENTIFIER)
	FROM   dbo.BillingViewChargeable
	WHERE
		TX_Category = 'STL'
		AND TX_Period = @Period
		AND TX_PriceItemCode = @DisbursementCode;

	DELETE @Disbursement WHERE JR_RX_NKCostCurrency = '' OR JR_OSCostAmt = 0 OR JH_PK IS NULL OR UnitCount = 0;

	DECLARE @SingleUsageCode VARCHAR(50);
	DECLARE UsageCodeTableCursor CURSOR FOR
	SELECT TRIM(VALUE) FROM STRING_SPLIT(@UsageCodes, ',');
	OPEN UsageCodeTableCursor;
	FETCH NEXT FROM UsageCodeTableCursor INTO @SingleUsageCode;

	IF EXISTS (SELECT * FROM @Disbursement)
	BEGIN

		DECLARE @Usage TABLE
		(
			SubCode VARCHAR(50) NOT NULL,
			DatabaseNumber INT NOT NULL,
			LCC_PK UNIQUEIDENTIFIER,
			UnitCount DECIMAL(14, 4) NOT NULL,
			JH_PK UNIQUEIDENTIFIER
		);

		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT @Usage(SubCode, DatabaseNumber, LCC_PK, UnitCount, JH_PK)
			SELECT TX_PriceItemCode, TX_DatabaseNumber, TX_LCC, TX_BillableCount,
				   JH_PK = TRY_CAST(IIF(@SingleUsageCode = 'SHP', TX_Reference5, TX_Reference4) AS UNIQUEIDENTIFIER)
			FROM   dbo.BillingViewChargeable
			WHERE
				TX_Category = 'STL'
				AND TX_Period = @Period
				AND TX_PriceItemCode = @SingleUsageCode;

			FETCH NEXT FROM UsageCodeTableCursor INTO @SingleUsageCode;
		END

		-- not matched yet, keep usage and remove disbursement
		DELETE D
		FROM @Disbursement D
		LEFT JOIN @Usage U ON U.DatabaseNumber = D.DatabaseNumber AND U.JH_PK = D.JH_PK
		WHERE U.DatabaseNumber IS NULL;

		-- matched, keep disbursement and remove usage
		DELETE U
		FROM   @Usage U
		JOIN @Disbursement D ON U.DatabaseNumber = D.DatabaseNumber AND U.JH_PK = D.JH_PK;

		INSERT @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
		SELECT DatabaseNumber, LCC_PK, SubCode, UnitCount = SUM(UnitCount)
		FROM   @Usage
		GROUP BY DatabaseNumber, SubCode, LCC_PK;

		INSERT @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount, TotalPrice, RX_NKCurrency, Direction)
		SELECT DatabaseNumber, LCC_PK, @DisbursementCode, UnitCount = SUM(UnitCount), TotalPrice = SUM(JR_OSCostAmt), JR_RX_NKCostCurrency, JH_Direction
		FROM   @Disbursement
		GROUP BY DatabaseNumber, LCC_PK, JR_RX_NKCostCurrency, JH_Direction;

	END
	ELSE  -- no disbursement yet, just usage
		BEGIN
			WHILE @@FETCH_STATUS = 0
			BEGIN
				INSERT @Result(DatabaseNumber, LCC_PK, SubCode, UnitCount)
				SELECT TX_DatabaseNumber, TX_LCC, TX_PriceItemCode, UnitCount = SUM(TX_BillableCount)
				FROM
					   dbo.BillingViewChargeable
				WHERE
					TX_Category = 'STL'
					AND TX_Period = @Period
					AND TX_PriceItemCode = @SingleUsageCode
				GROUP BY TX_DatabaseNumber, TX_LCC, TX_PriceItemCode;

				FETCH NEXT FROM UsageCodeTableCursor INTO @SingleUsageCode;
			END
		END

	CLOSE UsageCodeTableCursor;
	DEALLOCATE UsageCodeTableCursor;
	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#region WTU

		public const string GetBillingUnitCountAdjustments = "EdiGetBillingUnitCountAdjustments";

		public static DatabaseViewAndRoutineCreateScript GetBillingUnitCountAdjustmentsScript()
		{
			const string name = GetBillingUnitCountAdjustments;
			var defaultValueAsSql = new StringBuilder();
			foreach (BillingUnitCountAdjustment adjustment in EDIDataRegistry.Instance.BillingUnitCountAdjustments.DefaultValue)
			{
				foreach (BillingUnitCountAdjustmentSetting setting in adjustment.AdjustmentSettings)
				{
					defaultValueAsSql.AppendLine($"INSERT @Result(ADJ_PriceCode, ADJ_DefaultAdjustedIncrement, ADJ_OriginalUnitCount, ADJ_AdjustedUnitCount) VALUES ('{adjustment.PriceCode}', {adjustment.DefaultAdjustedIncrement}, {setting.OriginalUnitCount}, {setting.AdjustedUnitCount});");
				}
			}

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"()
RETURNS @Result TABLE
(
	ADJ_PriceCode VARCHAR(3) NOT NULL,
	ADJ_DefaultAdjustedIncrement DECIMAL(14,4) NOT NULL,
	ADJ_OriginalUnitCount INT NOT NULL,
	ADJ_AdjustedUnitCount DECIMAL(14,4) NOT NULL,
	PRIMARY KEY (ADJ_PriceCode, ADJ_OriginalUnitCount)
)
BEGIN
    
	DECLARE @adjmtRegXmldata XML = (SELECT CAST(CONVERT(NVARCHAR(MAX), SD_BinaryValue) AS XML) FROM dbo.StmData WHERE SD_Name = 'BILLINGUNITCOUNTADJUSTMENTS');

	IF @adjmtRegXmldata IS NOT NULL
	BEGIN
		INSERT @Result(ADJ_PriceCode, ADJ_DefaultAdjustedIncrement, ADJ_OriginalUnitCount, ADJ_AdjustedUnitCount)
		SELECT PriceCode = XAdjustments.value('(PriceCode)[1]', 'varchar(3)')
			,DefaultAdjustedIncrement = XAdjustments.value('(DefaultAdjustedIncrement)[1]', 'decimal(14,4)') 
			,OriginalUnitCount = XSetting.value('(OriginalUnitCount)[1]', 'int')
			,AdjustedUnitCount = XSetting.value('(AdjustedUnitCount)[1]', 'decimal(14,4)')
		FROM @adjmtRegXmldata.nodes('/ArrayOfBillingUnitCountAdjustment/BillingUnitCountAdjustment') AS XT1(XAdjustments)
		CROSS APPLY XAdjustments.nodes('AdjustmentSettings/BillingUnitCountAdjustmentSetting') AS XT2(XSetting);
	END
	ELSE
	BEGIN
		" +
		defaultValueAsSql.ToString()
		+
@"
	END
RETURN
END
", "drop function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region COT

		public const string GetCountryTierPriceCodeMappings = "EdiGetCountryTierPriceCodeMappings";

		public static DatabaseViewAndRoutineCreateScript GetCountryTierPriceCodeMappingsScript()
		{
			const string name = GetCountryTierPriceCodeMappings;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"()
RETURNS @Result TABLE
(
	COT_SystemCode VARCHAR(3) NOT NULL,
	COT_PriceCode VARCHAR(3) NOT NULL,
	COT_CountryTierCode VARCHAR(3) NOT NULL,
	COT_CountryCode VARCHAR(2) NOT NULL,
	PRIMARY KEY (COT_SystemCode, COT_PriceCode, COT_CountryCode)
)
BEGIN
    
	DECLARE @countryTierXmlData XML = (SELECT CAST(CONVERT(NVARCHAR(MAX), SD_BinaryValue) AS XML) FROM dbo.StmData WHERE SD_Name = 'COUNTRYTIERPRICECODEMAPPINGS');

	IF @countryTierXmlData IS NOT NULL
	BEGIN
		INSERT @Result(COT_SystemCode, COT_PriceCode, COT_CountryCode, COT_CountryTierCode)
		SELECT SystemCode = PriceCodeMappings.value('(SystemCode)[1]', 'varchar(3)')
			,PriceCode = PriceCodeMappings.value('(PriceCode)[1]', 'varchar(3)')
			,CountryCode = CountryTierMapping.value('(CountryCode)[1]', 'varchar(3)')
			,CountryTierCode = CountryTierMapping.value('(CountryTierCode)[1]', 'varchar(3)')
		FROM @countryTierXmlData.nodes('/ArrayOfCountryTierPriceCodeMapping/CountryTierPriceCodeMapping') AS CT1(PriceCodeMappings)
		CROSS APPLY PriceCodeMappings.nodes('MappingLines/CountryTierPriceCodeMappingLine') AS CT2(CountryTierMapping);
	END
RETURN
END
", "drop function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region Licence Database Consolidation

		public const string UpdateLicenceDatabaseConsolidation = "EdiUpdateLicenceDatabaseConsolidation";

		public static DatabaseViewAndRoutineCreateScript UpdateLicenceDatabaseConsolidationScript()
		{
			return new DatabaseViewAndRoutineCreateScript(UpdateLicenceDatabaseConsolidation,
@"CREATE PROCEDURE " + UpdateLicenceDatabaseConsolidation + @"
(
	@Period INT,
	@ProductCodes VARCHAR(200)
)
AS 
BEGIN
SET NOCOUNT ON;

BEGIN TRANSACTION;
BEGIN TRY

	DECLARE @Changes TABLE
	(
		CHG_LD UNIQUEIDENTIFIER NOT NULL PRIMARY KEY
	);

	WITH Source (SRC_LD, SRC_LD_Consol) AS
	(
		SELECT LD.LD_PK, ConsolidatedDatabase.LD_PK
		FROM
		(
			SELECT LD_PK, 
					ConsolidatedDatabaseNumber = MIN(LD_DatabaseNumber) OVER (PARTITION BY LD_OH_WebAccessOrg)
			FROM dbo.LicenceDatabase
			WHERE LD_IsActive = 1
				AND LD_LicenceType = 'PRD'
				AND LD_OH_WebAccessOrg IS NOT NULL
				AND LD_Product IN (SELECT VALUE FROM SplitStringToTable(@ProductCodes, DEFAULT))
		) LD
		JOIN dbo.LicenceDatabase ConsolidatedDatabase ON ConsolidatedDatabase.LD_DatabaseNumber = LD.ConsolidatedDatabaseNumber
	)
	MERGE INTO EdiLicenceDatabaseConsolidationHistory AS Target
	USING Source AS Source
	ON Target.EDH_Period = @Period AND Target.EDH_LD = Source.SRC_LD
	WHEN MATCHED AND Target.EDH_LD_ConsolidatedDatabase <> Source.SRC_LD_Consol THEN
		UPDATE SET Target.EDH_LD_ConsolidatedDatabase = Source.SRC_LD_Consol
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase) VALUES (@Period, Source.SRC_LD, Source.SRC_LD_Consol)
	WHEN NOT MATCHED BY SOURCE AND Target.EDH_Period = @Period THEN
		DELETE
	OUTPUT ISNULL(DELETED.EDH_LD, INSERTED.EDH_LD)
	INTO @Changes;

	WITH CGH (CHG_LD, CHG_ConsolidatedDatabaseNumber) AS
	(
		SELECT CHG_LD, CHG_ConsolidatedDatabaseNumber = ISNULL(LD_DatabaseNumber, 0)
		FROM @Changes
		LEFT JOIN dbo.EdiLicenceDatabaseConsolidationHistory ON EDH_LD = CHG_LD AND EDH_Period = @Period
		LEFT JOIN dbo.LicenceDatabase ON LD_PK = EDH_LD_ConsolidatedDatabase
	)
	INSERT INTO dbo.StmALog
	(
		SL_PK,
		SL_Parent,
		SL_Table,
		SL_SE_NKEvent,
		SL_Reference,
		SL_EventTime,
		SL_GS_NKUser
	)
	SELECT
		NEWID(),
		CHG_LD,
		'LicenceDatabase',
		'EDT',
		Reference = CONCAT('Database Consolidation Target set to ', CHG_ConsolidatedDatabaseNumber, ' for Period ', @Period),
		GETDATE(),
		'E'
	FROM CGH

	COMMIT TRANSACTION;

END TRY
BEGIN CATCH
	IF (@@TRANCOUNT > 0)
	BEGIN
		ROLLBACK TRANSACTION;
		THROW;
	END
END CATCH
END
", "DROP PROCEDURE " + UpdateLicenceDatabaseConsolidation, DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion

		#endregion

		#region BorderWise

		/// <summary>
		/// Get BW users
		/// </summary>
		public static DatabaseViewAndRoutineCreateScript GetBorderWiseUsersScript()
		{
			const string name = "EdiGetBorderWiseUsers";

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int)
RETURNS @Result TABLE
(
	ContactPk uniqueidentifier,
	TX_PriceItemCode char(3) not null,
	TX_DatabaseNumber int not null,
	TX_ServiceOccuredUtc [datetime2](7) NOT NULL,
	MachineId varchar(50),
	EditionName varchar(50),
	EditionCountry varchar(50),
	SpecialUser int not null,
	IsTrade bit not null,
	IsCW bit not null,
	LC_PK uniqueidentifier,
	XZ_Comment varchar(50),
	XZ_IssueDate smalldatetime,
	XZ_ExpiryOrDueDate smalldatetime,
	EOR_MembershipType varchar(16),
	LocationCountryCode char(2)
)
BEGIN

declare @PeriodAsDate smalldatetime = DATEFROMPARTS(@Period / 100, @Period % 100, 1);

declare @remoteData TABLE
(
	TX_PriceItemCode char(3) not null,
	TX_DatabaseNumber int not null,
	TX_ServiceOccuredUtc [datetime2](7) NOT NULL,
	ContactPk uniqueidentifier not null primary key clustered,
	MachineId varchar(50) not null,
	EditionName varchar(50) not null,
	EditionCountry varchar(50)
);

insert @remoteData (TX_PriceItemCode, TX_DatabaseNumber, TX_ServiceOccuredUtc, ContactPk, MachineId, EditionName, EditionCountry)
select TX_PriceItemCode,
	TX_DatabaseNumber,
	TX_ServiceOccuredUTC,
	ContactPk = cast(TX_Reference2 as uniqueidentifier),
	MachineId = TX_Reference3,
	EditionName = TX_Reference4,
	EditionCountry = TX_Reference5
from BillingViewChargeable where TX_period = @Period and TX_Category = 'BOR'

;with BwUser as
(
	select * from @remoteData
),
BwLicence as
(
	select * from BwUser
	join dbo.OrgContact on ContactPk = OC_PK
	join dbo.OrgHeader on OC_OH = OH_PK
	cross apply (select LocationCountryCode = case when OC_RN_NKNationality != '' then OC_RN_NKNationality else LEFT(OH_RL_NKClosestPort, 2) end) loc
	outer apply
	(
		select top 1 *
		from dbo.EdiOrgMembership
		where EOR_OH = OC_OH
			and @Period >= 201804 -- don't care about membership before this
			and EOR_MembershipType in ('FTA', 'CBAFF', 'IFCBAA')
			and EOR_ValidFrom <= @PeriodAsDate
			and (EOR_ValidTo is null or EOR_ValidTo > @PeriodAsDate)
			and
			(
				(LocationCountryCode = 'AU' and EOR_MembershipType in ('FTA', 'IFCBAA'))
				or
				(LocationCountryCode = 'NZ' and EOR_MembershipType = 'CBAFF')
			)
	) mem
	where OC_Email not like '%@tradefoxinc.com'
),
ContactCerts as
(
	select XZ_ParentID, XZ_Comment, XZ_IssueDate, XZ_ExpiryOrDueDate, SpecialUser = case 
		when XZ_Comment like 'Student%' then 1
		when XZ_Comment like 'BorderWise% Free Trial' then 3
		else 2 end -- legacy product user
	from dbo.GenRegCertAccredMaintList
	where XZ_Type = 'MSC'
		and 
		(
			XZ_Comment like 'Student%' 
			or 
			XZ_Comment like 'BorderWise% Free Trial' 
		)
		and (XZ_ExpiryOrDueDate is null or XZ_ExpiryOrDueDate > @PeriodAsDate)
		and (XZ_IssueDate is null or XZ_IssueDate < DATEADD(MONTH, 1, @PeriodAsDate))
),
CW1Payers as
(
	select CWpayerLC_PK = isnull(billToCo.LC_PK, LA_LC)
	from dbo.LicenceHeader
	join dbo.LicenceDatabase on LA_LD = LD_PK and LD_Product in ('CW1', 'ENT', 'CWN', 'CGW', 'PRW') and LD_IsActive = 1
	join dbo.LicenceCompany on LA_LC = LC_PK
	join dbo.OrgHeader on LC_OH = OH_PK and OH_IsActive = 1
	join dbo.ClientInvoiceDelivery on L9_LC = LA_LC and L9_IsBilled = 'Y' and L9_ServerCode in ('', LD_ServerCode)
	left join dbo.LicenceCompany billToCo on billToCo.LC_OH = L9_OH_InvoiceTo
	where LA_IsActive = 1
),
BWUserPayer as
(
	select BWuserLC_PK = LA_LC, BWpayerLC_PK = isnull(billToCo.LC_PK, LA_LC)
	from dbo.LicenceHeader
	join dbo.LicenceDatabase on LA_LD = LD_PK and LD_Product = 'BOR' and LD_IsActive = 1
	join dbo.LicenceCompany on LA_LC = LC_PK
	join dbo.OrgHeader on LC_OH = OH_PK and OH_IsActive = 1
	join dbo.ClientInvoiceDelivery on L9_LC = LA_LC and L9_IsBilled = 'Y' and L9_ServerCode in ('', LD_ServerCode)
	left join dbo.LicenceCompany billToCo on billToCo.LC_OH = L9_OH_InvoiceTo
	where LA_IsActive = 1
),
CW1Companies as
(
	-- company has CW1 if it has a CW1 db
	select LA_LC
	from dbo.LicenceHeader
	join dbo.LicenceDatabase on LA_LD = LD_PK and LD_Product in ('CW1', 'ENT', 'CWN', 'CGW', 'PRW') and LD_IsActive = 1
	where LA_IsActive = 1
		and LA_AgreedLiveDate <= @PeriodAsDate
	group by LA_LC

	union

	-- or is billed to the same company as a CW1 company is billed to
	select BWuserLC_PK
	from BWUserPayer
	join CW1Payers on BWpayerLC_PK = CWpayerLC_PK
	group by BWuserLC_PK
),
LicenceWithUserType as
(
	select
		SpecialUser = ISNULL(SpecialUser, 0),
		IsTrade = case when EOR_MembershipType is not null then cast(1 as bit) else cast(0 as bit) end,
		IsCW = case when CW1Companies.LA_LC is not null then cast(1 as bit) else cast(0 as bit) end,
		LC_PK,
		BwLicence.*,
		XZ_Comment, XZ_IssueDate, XZ_ExpiryOrDueDate
	from BwLicence
	outer apply (
		select top 1 * from ContactCerts
		where XZ_ParentID = ContactPk
			-- certificate must start on or before the usage to be in effect, or cover the entire month
			and (XZ_IssueDate is null or XZ_IssueDate <= DATEADD(HOUR, 11, TX_ServiceOccuredUTC) or XZ_IssueDate <= @PeriodAsDate)
		order by SpecialUser
	) SpecialUsers
	left join dbo.LicenceCompany on LC_OH = OC_OH
	left join CW1Companies on CW1Companies.LA_LC = LC_PK
)
INSERT @Result(
	ContactPk,
	TX_PriceItemCode,
	TX_DatabaseNumber,
	TX_ServiceOccuredUtc,
	MachineId,
	EditionName,
	EditionCountry,
	SpecialUser,
	IsTrade,
	IsCW,
	LC_PK,
	XZ_Comment,
	XZ_IssueDate,
	XZ_ExpiryOrDueDate,
	EOR_MembershipType,
	LocationCountryCode
)
select
	ContactPk,
	TX_PriceItemCode,
	TX_DatabaseNumber,
	TX_ServiceOccuredUtc,
	MachineId,
	EditionName,
	EditionCountry,
	SpecialUser,
	IsTrade,
	IsCW,
	LC_PK,
	XZ_Comment,
	XZ_IssueDate,
	XZ_ExpiryOrDueDate,
	EOR_MembershipType,
	LocationCountryCode
from LicenceWithUserType
option(recompile)

RETURN
END", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		/// <summary>
		/// Get BW users
		/// </summary>
		public static DatabaseViewAndRoutineCreateScript GetBorderWiseUsersForPeriodRange()
		{
			const string name = "EdiGetBorderWiseUsersForPeriodRange";

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@PeriodStart int, @PeriodEnd int)
RETURNS @Result TABLE
(
	TX_Period int not null,
	ContactPk uniqueidentifier not null,
	TX_PriceItemCode char(3) not null,
	TX_DatabaseNumber int not null,
	TX_ServiceOccuredUtc [datetime2](7) NOT NULL,
	MachineId varchar(50),
	EditionName varchar(50),
	SpecialUser int,
	IsTrade bit,
	IsCW bit,
	LC_PK uniqueidentifier,
	XZ_Comment varchar(50),
	XZ_IssueDate smalldatetime,
	XZ_ExpiryOrDueDate smalldatetime,
	EOR_MembershipType varchar(16),
	LocationCountryCode char(2)
)
BEGIN

declare @period int = @PeriodStart

while @period <= @PeriodEnd
begin
	insert @Result(
		 TX_Period, ContactPk, TX_PriceItemCode, TX_DatabaseNumber, TX_ServiceOccuredUtc, MachineId, EditionName, SpecialUser, IsTrade, IsCW, LC_PK, XZ_Comment, XZ_IssueDate, XZ_ExpiryOrDueDate, EOR_MembershipType, LocationCountryCode)
	select @period, ContactPk, TX_PriceItemCode, TX_DatabaseNumber, TX_ServiceOccuredUtc, MachineId, EditionName, SpecialUser, IsTrade, IsCW, LC_PK, XZ_Comment, XZ_IssueDate, XZ_ExpiryOrDueDate, EOR_MembershipType, LocationCountryCode
	from EdiGetBorderWiseUsers(@period)

	declare @nextPeriodIndex int = (@period / 100) * 12 + (@period % 100);
	set @period = (@nextPeriodIndex / 12) * 100 + (@nextPeriodIndex % 12) + 1
end

RETURN
END", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		/// <summary>
		/// Get BW licences
		/// </summary>
		public static DatabaseViewAndRoutineCreateScript GetBorderWiseLicencesScript()
		{
			const string name = "EdiGetBorderWiseLicences";

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS TABLE
AS 
return
select
	OH_Code,
	OC_ContactName,
	OC_Email,
	OC_OH,
	LC_PK,
	BWL_OC_Contact = ContactPk,
	BWL_ValidFromUtc = TX_ServiceOccuredUtc,
	BWL_ValidToUtc = cast(null as datetime),
	BWL_LastUsedUtc = cast(null as datetime),
	BWL_MachineIdentification = MachineId,
	XZ_Comment,
	EOR_MembershipType,
	LocationCountryCode,
	BWE_Name = EditionName,
	SpecialUser,
	IsTrade,
	IsCW,
	XZ_IssueDate,
	XZ_ExpiryOrDueDate
from EdiGetBorderWiseUsersForPeriodRange(
		YEAR(DATEADD(HOUR, 11, @PeriodStartTimeUtc)) * 100 + MONTH(DATEADD(HOUR, 11, @PeriodStartTimeUtc)) + 1,
		YEAR(DATEADD(HOUR, 11, @PeriodEndTimeUtc)) * 100 + MONTH(DATEADD(HOUR, 11, @PeriodEndTimeUtc) + 1)) bw
join dbo.OrgContact on ContactPk = OC_PK
join dbo.OrgHeader on OC_OH = OH_PK
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionInlineTypeDesc);
		}

		public static DatabaseViewAndRoutineCreateScript GetBorderWiseScript()
		{
			string name = GetUsageFunctionOrProcedureName(Billing.Business.BillingConstants.BillingSystem.BorderWise);

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	LC_PK uniqueidentifier NULL,
	LD_PK uniqueidentifier NULL,
	LCC_PK uniqueidentifier NULL,
	SubCode varchar(50) NOT NULL,
	UnitCount int NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN

with LicenceWithPriceCode as
(
	select PriceCode, users.*
	from EdiGetBorderWiseUsers(@Period) users
	cross apply
	(
		select PriceCode = 'BS1' where SpecialUser = 1 -- student

		union all

		select PriceCode = 'BF1' where SpecialUser = 3 -- free trial

		union all

		-- Global edition
		select PriceCode
		from
		(
			select PriceCode = (case when IsTrade = 0 and IsCW = 0 then 'BWS' -- standalone Global
						else 'BWX' -- all other Global
						end)
			union all
			select PriceCode = 'BW3' -- global data / Pro Pack
		) GlobalEdition
		where SpecialUser = 0 and EditionCountry = ''

		union all

		-- Single Window
		select PriceCode
		from
		(
			select PriceCode = EditionCountry
				+ (case
					when IsTrade = 1 and IsCW = 0 then 'P' -- Partner 
					when IsTrade = 0 and IsCW = 1 then 'W' -- CW1
					when IsTrade = 1 and IsCW = 1 then 'X' -- CW1 + Partner 
					when IsTrade = 0 and IsCW = 0 then 'S' -- Standalone
					end)
			union all
			select PriceCode = EditionCountry + '3' where TX_PriceItemCode like '__3' -- Pro Pack
		) SingleWindow
		where SpecialUser = 0 and EditionCountry != ''
	) PriceCategory
)
INSERT @Result(LC_PK, SubCode, UnitCount)
select LC_PK, SubCode = PriceCode, UnitCount
from
(
	select LC_PK, PriceCode, UnitCount = count(*)
	from LicenceWithPriceCode
	where LC_PK is not null
	group by LC_PK, PriceCode
) d
where @Period >= 201807

RETURN
END", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		public const string LoadAllBorderWiseChargeableUsage = "EdiLoadAllBorderWiseChargeableUsage";

		public static DatabaseViewAndRoutineCreateScript LoadAllBorderWiseChargeableUsageScript()
		{
			const string name = LoadAllBorderWiseChargeableUsage;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE PROCEDURE " + name + @"
	@PeriodStart datetime,
	@IsStl bit,
	@OrgPk uniqueidentifier,
	@EnterpriseCode varchar(3)
with recompile
as
begin
	set nocount on;
	if @EnterpriseCode = '' set @EnterpriseCode = null;

	create table #OrgPks(OrgPk uniqueidentifier);

	if @OrgPk is not null
	begin
		insert #OrgPks(OrgPk)
		select distinct GroupPk from EdiGetBillingGroups(@OrgPk, @PeriodStart, null)
		where IsOrg = 1
	end;

	if @EnterpriseCode is not null
	begin
		insert #OrgPks(OrgPk)
		select LC_OH
		from dbo.LicenceCompany
		join dbo.LicenceEnterprise on LC_LE = LE_PK
		where LE_EnterpriseCode = @EnterpriseCode
			and LC_OH not in (select OrgPk from #OrgPks)
	end;

	select ClientChargeableUsage.*
	into #usage
	from dbo.ClientChargeableUsage
	join dbo.LicenceCompany on LC_PK = U1_LC
	left join #OrgPks on LC_OH = OrgPk and (@OrgPk is not null or @EnterpriseCode is not null)
	where 1=1
		and U1_ManuallyProcessed = 0
		and U1_Code = 'BOR'
		and U1_PeriodStart = @PeriodStart
		and (
			(@OrgPk is null and @EnterpriseCode is null)
			or
			LC_OH in (select OrgPk from #OrgPks)
		);

	select LC_PK = U1_LC, LA_LicenceAdvStdOth, AgreedLiveDate = LA_AgreedLiveDate,
		LD_PK, LD_ServerCode, LD_DatabaseNumber, LD_LE, LD_IsBilledPerCompany, EnterpriseCode = LE_EnterpriseCode,
		LD_LicenceType, ParentPk = cast (null as uniqueidentifier), LD_Billable, LD_OH_BillingParty, LD_OH_WebAccessOrg
	into #db
	from (select distinct U1_LC from #usage where U1_LC is not null) u
	cross apply
	(
		select top 1 *
		from dbo.LicenceHeader 
		join dbo.LicenceDatabase on LA_LD = LD_PK and LD_Product in ('BOR') and LD_IsActive = 1
		join dbo.LicenceEnterprise on LD_LE = LE_PK
		where LA_IsActive = 1
			and LA_LC = U1_LC
			and (LA_AgreedLiveDate is null or LA_AgreedLiveDate < DATEADD(DAY, 25, @PeriodStart))
		order by (case LD_LicenceType when 'PRD' then 0 else 1 end),
			(case when LA_AgreedLiveDate is not null then 0 else 1 end),
			LA_AgreedLiveDate
	) BorderWiseDb

	update #db
		set ParentPk = cwDatabasePk
	from #db
	join
	(
		-- find a single parent CW1 database for each BW database if possible
		select bwDatabasePk, cwDatabasePk = cwBest.LD_PK
		from (select distinct bwDatabasePk = LD_PK, bwOwnerOrgPk = LD_OH_BillingParty from #db) bwDbPks
		cross apply
		(
			select top 1 cwDb.LD_PK
			from #db bwDbCompany
			join dbo.LicenceHeader bwLic on bwLic.LA_LD = bwDatabasePk
			join dbo.LicenceHeader cwLic on bwLic.LA_LC = cwLic.LA_LC 
				and cwLic.LA_IsActive = 1 
				and cwLic.LA_AgreedLiveDate < DATEADD(DAY, 25, @periodstart)
			join dbo.LicenceDatabase cwDb on cwLic.LA_LD = cwDb.LD_PK
				and cwDb.LD_Product in ('CW1', 'ENT', 'CWN', 'CGW', 'PRW')
				and cwDb.LD_IsActive = 1
			join dbo.LicenceCompany cwCompany on cwCompany.LC_PK = cwLic.LA_LC
			where bwDbCompany.LD_PK = bwDatabasePk
			order by
				(case when cwCompany.LC_OH = bwOwnerOrgPk then 0 else 1 end), -- db usage owner
				(case when cwCompany.LC_PK = bwDbCompany.LC_PK then 0 else 1 end), -- prefer a CW database on the same org that used BW
				(case cwDb.LD_LicenceType when 'PRD' then 0 else 1 end),
				(case when cwDb.LD_LastHeartbeat is null or cwDb.LD_LastHeartbeat < @PeriodStart then 1 else 0 end), -- prefer recent heartbeat
				(case when bwLic.LA_LicenceAdvStdOth = cwLic.LA_LicenceAdvStdOth then 0 else 1 end),
				cwLic.LA_AgreedLiveDate,
				cwCompany.LC_CompanyCode -- if everything else is equal, pick the first company code
		) cwBest
	) cwDb on LD_PK = bwDatabasePk

	if @IsStl = 1
	begin
		select * from #usage where U1_LC not in (select LC_PK from #db where LA_LicenceAdvStdOth != 'STL');
		select * from #db where LA_LicenceAdvStdOth = 'STL';
	end
	else
	begin
		select * from #usage where U1_LC in (select LC_PK from #db where LA_LicenceAdvStdOth != 'STL');
		select * from #db where LA_LicenceAdvStdOth != 'STL';
	end;

	drop table #OrgPks;
	drop table #usage;
	drop table #db;
end
", "DROP PROCEDURE " + name, DbRoutineType.SqlProcedureTypeDesc);
		}

		public const string LoadAllGenericChargeableUsage = "EdiLoadAllGenericChargeableUsage";

		public static DatabaseViewAndRoutineCreateScript LoadAllGenericChargeableUsageScript()
		{
			const string name = LoadAllGenericChargeableUsage;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE PROCEDURE " + name + @"
	@PeriodStart datetime,
	@OrgPk uniqueidentifier,
	@EnterpriseCode varchar(3),
	@ProductCode varchar(3),
	@UsageCategory varchar(3)
with recompile
as
begin
	set nocount on;
	if @EnterpriseCode = '' set @EnterpriseCode = null;

	create table #DatabasePks(DatabasePk uniqueidentifier);

	if @OrgPk is not null
	begin
		insert #DatabasePks(DatabasePk)
		select GroupPk from EdiGetBillingGroups(@OrgPk, @PeriodStart, @ProductCode)
		where IsOrg = 0
	end;

	create table #StlDatabases(
		LD_PK uniqueidentifier not null primary key clustered,
		LD_ServerCode varchar(3) not null,
		LD_DatabaseNumber int not null,
		LD_LE uniqueidentifier not null,
		LD_IsBilledPerCompany bit not null,
		LD_HostedLocation char(3) not null,
		LE_EnterpriseCode varchar(3) not null,
		LA_AgreedLiveDate smalldatetime not null,
		LD_LicenceType  varchar(3) not null,
		LD_Billable varchar(3) not null,
		LD_OH_WebAccessOrg uniqueidentifier null
	);

	insert #StlDatabases(LD_PK, LD_ServerCode, LD_DatabaseNumber, LD_LE, LD_IsBilledPerCompany, LD_HostedLocation, LE_EnterpriseCode, LA_AgreedLiveDate, LD_LicenceType, LD_Billable, LD_OH_WebAccessOrg)
	select LD_PK, LD_ServerCode, LD_DatabaseNumber, LD_LE, LD_IsBilledPerCompany, LD_HostedLocation, LE_EnterpriseCode, LA_AgreedLiveDate, LD_LicenceType, LD_Billable, LD_OH_WebAccessOrg
	from dbo.LicenceDatabase
	join dbo.LicenceEnterprise on LD_LE = LE_PK
	join
	(
		select LA_LD, LA_AgreedLiveDate = min(isnull(LA_AgreedLiveDate, @PeriodStart))
		from dbo.LicenceHeader
		where LA_IsActive = 1
		group by LA_LD
	) SiteLive on LD_PK = LA_LD
	where LD_IsActive = 1
	    and LD_LicenceType = 'PRD'
		and LD_Product = @ProductCode
		and (@OrgPk is null or LD_PK in (select DatabasePk from #DatabasePks))
		and (@EnterpriseCode is null or LE_EnterpriseCode = @EnterpriseCode)
		and LE_IsInternal = 0
		and LA_AgreedLiveDate < DATEADD(DAY, 15, @PeriodStart)
		option (recompile);
  
	select ClientChargeableUsage.*
	from dbo.ClientChargeableUsage
	join #StlDatabases on U1_LD = LD_PK
		and U1_ManuallyProcessed = 0
		and U1_Code = @UsageCategory
		and U1_PeriodStart = @PeriodStart;
 
	select * from #StlDatabases;

	drop table #DatabasePks;
	drop table #StlDatabases;
end
", "DROP PROCEDURE " + name, DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion

		#region EdiOrgMembership

		internal static DatabaseObjectCreateScript EdiOrgMembership
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiOrgMembership", @"
CREATE TABLE dbo.EdiOrgMembership
(
	[EOR_PK] uniqueidentifier not null,
	[EOR_OH] [uniqueidentifier] NOT NULL constraint [EdiOrgMembership_EOR_OH_FK2_OrgHeader] REFERENCES [OrgHeader] ([OH_PK]) on delete cascade,
	[EOR_ValidFrom] [date] NOT NULL,
	[EOR_ValidTo] [date] NULL,
	[EOR_AgreementVersion] [date] NULL,
	[EOR_MembershipType] [varchar](16) NOT NULL,
	[EOR_OH_Organisation] [UNIQUEIDENTIFIER] NULL,
	CONSTRAINT [PK_UX__EOR_PK] PRIMARY KEY NONCLUSTERED ([EOR_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)
);

ALTER TABLE [EdiOrgMembership]
    SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX [FK_RC__EOR_OH] ON [EdiOrgMembership] ([EOR_OH] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__EOR_OH_EOR_MembershipType_EOR_ValidFrom] ON [EdiOrgMembership] (EOR_OH, EOR_MembershipType, EOR_ValidFrom) WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__EOR_OH_EOR_MembershipType] ON [EdiOrgMembership] ([EOR_OH] ASC, [EOR_MembershipType] ASC)
WHERE [EOR_ValidTo] IS NULL
WITH (ALLOW_PAGE_LOCKS = OFF);

CREATE NONCLUSTERED INDEX [FK_RX__EOR_OH_Organisation] ON [EdiOrgMembership] ([EOR_OH_Organisation] ASC) WITH (ALLOW_PAGE_LOCKS = OFF);

ALTER TABLE [EdiOrgMembership] WITH NOCHECK
	ADD CONSTRAINT [EdiOrgMembership_EOR_OH_Organisation_FK2_OrgHeader_RRR_120N] FOREIGN KEY
		([EOR_OH_Organisation])
		REFERENCES [OrgHeader]
		([OH_PK]);
",
"DROP TABLE EdiOrgMembership");
			}
		}

		#endregion

		#region FlightStats

		public static DatabaseViewAndRoutineCreateScript GetFlightStatsScript()
		{
			string name = GetUsageFunctionOrProcedureName(Billing.Business.BillingConstants.BillingSystem.FlightStats);

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	LC_PK uniqueidentifier NULL,
	LD_PK uniqueidentifier NOT NULL,
	LCC_PK uniqueidentifier NULL,
	SubCode varchar(50) NOT NULL,
	UnitCount int NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN

	DECLARE @Raw TABLE
	(
		DatabaseNumber int NOT NULL,
		LCC_PK uniqueidentifier,
		UnitCount INT NOT NULL
	);

	INSERT @Raw(DatabaseNumber, LCC_PK, UnitCount)
	SELECT TX_DatabaseNumber, TX_LCC, UnitCount = Sum(TX_BillableCount)
	FROM dbo.BillingViewChargeable
	WHERE
		TX_Category = 'FMS'
		AND TX_PriceItemCode = 'FMS'
		AND TX_Period = @Period
	GROUP BY TX_DatabaseNumber, TX_LCC;

	INSERT @Result(LD_PK, LCC_PK, SubCode, UnitCount)
	SELECT LD_PK, LCC_PK, SubCode = 'FMS', UnitCount
	FROM @Raw R
	JOIN dbo.LicenceDatabase on LD_DatabaseNumber = DatabaseNumber
	where LD_LicenceType = 'PRD';

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region Contact Country

		public const string UpdateContactCountry = "EdiUpdateContactCountry";

		public static DatabaseViewAndRoutineCreateScript UpdateContactCountryScript()
		{
			const string name = UpdateContactCountry;

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE PROCEDURE " + name + @"
	@Period int
with recompile
as
begin

set NOCOUNT ON;

select TX_DatabaseNumber, StaffCode = TX_Reference1, TX_LCC
into #usr
from BillingViewChargeable
where TX_Period = @Period and TX_Category = 'STL' and TX_PriceItemCode = 'USR'

select Email = LS_Email, Country = LCC_RN_NKCountryCode
into #ec
from 
(
	select LS_Email, LCC_RN_NKCountryCode, MostUsedCountryRank = ROW_NUMBER() over (partition by LS_Email order by DatabaseCount desc)
	from
	(
		select LS_Email, LCC_RN_NKCountryCode, DatabaseCount = count(*)
		from dbo.clientstaff 
		join dbo.LicenceDatabase on LD_PK = LS_LD
		join dbo.ClientCompany on LCC_LD = LS_LD
		join #usr on LS_Code = StaffCode collate database_default and LCC_PK = TX_LCC
		join dbo.LicenceEnterprise on LD_LE = LE_PK and LE_IsInternal = 0
		where LS_EMail != ''
		group by LS_Email, LCC_RN_NKCountryCode
	) EmailCountry
) a
where MostUsedCountryRank = 1

update dbo.OrgContact
set OC_RN_NKNationality = Country
from dbo.OrgContact
join dbo.OrgHeader on OC_OH = OH_PK and OH_IsActive = 1
join #ec on OC_Email = Email
where OC_RN_NKNationality = '' and OC_RN_NKNationality != Country
	and OC_IsActive = 1
	option (recompile)

end
", "DROP PROCEDURE " + name, DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion

		#region Common

		public static DatabaseViewAndRoutineCreateScript EdiGetBillingDbUsageScript()
		{
			const string name = "EdiGetBillingDbUsage";

			return new DatabaseViewAndRoutineCreateScript(name, @"
CREATE function " + name + @"(@Period int, @Category varchar(3), @PriceItemCode varchar(3), @KeyRefIndex1 tinyint)
RETURNS @Result TABLE
(
	LC_PK uniqueidentifier NULL,
	LD_PK uniqueidentifier NULL,
	LCC_PK uniqueidentifier NULL,
	SubCode varchar(50) NOT NULL,
	UnitCount int NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN

	declare @raw TABLE
	(
		DatabaseNumber int NOT NULL,
		LCC_PK uniqueidentifier,
		UnitCount INT NOT NULL,
		Ref1 varchar(50) NULL
	);

	-- special case where CTR-CTR in ediProd is actually CTR-CTO in Billing DB
	declare @EdiPriceCode varchar(3) = @PriceItemCode
	if @Category = 'CTR' and @EdiPriceCode = 'CTR'
		set @PriceItemCode = 'CTO'

	if ISNULL(@KeyRefIndex1, 0) = 0
	begin
		insert @raw(DatabaseNumber, LCC_PK, UnitCount)
		select TX_DatabaseNumber, TX_LCC, UnitCount = sum(TX_BillableCount)
		FROM
			dbo.BillingViewChargeable
		WHERE				
			TX_Category = @Category
			AND TX_PriceItemCode = @PriceItemCode
			AND TX_Period = @Period
		group by TX_DatabaseNumber, TX_LCC
	end
	else if @KeyRefIndex1 = 1
	begin
		insert @raw(DatabaseNumber, LCC_PK, UnitCount, Ref1)
		select TX_DatabaseNumber, TX_LCC, UnitCount = sum(TX_BillableCount), TX_Reference1
		FROM
			dbo.BillingViewChargeable
		WHERE				
			TX_Category = @Category
			AND TX_PriceItemCode = @PriceItemCode
			AND TX_Period = @Period
		group by TX_DatabaseNumber, TX_LCC, TX_Reference1
	end
	else if @KeyRefIndex1 = 2
	begin
		insert @raw(DatabaseNumber, LCC_PK, UnitCount, Ref1)
		select TX_DatabaseNumber, TX_LCC, UnitCount = sum(TX_BillableCount), TX_Reference2
		FROM
			dbo.BillingViewChargeable
		WHERE				
			TX_Category = @Category
			AND TX_PriceItemCode = @PriceItemCode
			AND TX_Period = @Period
		group by TX_DatabaseNumber, TX_LCC, TX_Reference2
	end
	else if @KeyRefIndex1 = 3
	begin
		insert @raw(DatabaseNumber, LCC_PK, UnitCount, Ref1)
		select TX_DatabaseNumber, TX_LCC, UnitCount = sum(TX_BillableCount), TX_Reference3
		FROM
			dbo.BillingViewChargeable
		WHERE				
			TX_Category = @Category
			AND TX_PriceItemCode = @PriceItemCode
			AND TX_Period = @Period
		group by TX_DatabaseNumber, TX_LCC, TX_Reference3
	end
	else if @KeyRefIndex1 = 4
	begin
		insert @raw(DatabaseNumber, LCC_PK, UnitCount, Ref1)
		select TX_DatabaseNumber, TX_LCC, UnitCount = sum(TX_BillableCount), TX_Reference4
		FROM
			dbo.BillingViewChargeable
		WHERE				
			TX_Category = @Category
			AND TX_PriceItemCode = @PriceItemCode
			AND TX_Period = @Period
		group by TX_DatabaseNumber, TX_LCC, TX_Reference4
	end
	else if @KeyRefIndex1 = 5
	begin
		insert @raw(DatabaseNumber, LCC_PK, UnitCount, Ref1)
		select TX_DatabaseNumber, TX_LCC, UnitCount = sum(TX_BillableCount), TX_Reference5
		FROM
			dbo.BillingViewChargeable
		WHERE				
			TX_Category = @Category
			AND TX_PriceItemCode = @PriceItemCode
			AND TX_Period = @Period
		group by TX_DatabaseNumber, TX_LCC, TX_Reference5
	end

	INSERT @Result(LD_PK, LCC_PK, SubCode, UnitCount, Reference1)
	SELECT LD_PK, LCC_PK, @EdiPriceCode, UnitCount, Ref1
	FROM @raw R
	JOIN dbo.LicenceDatabase on LD_DatabaseNumber = DatabaseNumber
	where LD_LicenceType = 'PRD'
		option(recompile)

	RETURN
END
", "DROP function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		public static DatabaseViewAndRoutineCreateScript GetUsageConverterFromBilling(string systemCode, bool productionSystemOnly = true)
		{
			string name = GetUsageFunctionOrProcedureName(systemCode);
			string sql = @"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	LC_PK uniqueidentifier NULL,
	LD_PK uniqueidentifier NULL,
	LCC_PK uniqueidentifier NULL,
	SubCode varchar(50) NOT NULL,
	UnitCount decimal(14, 4) NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	declare @raw TABLE
	(
		DatabaseNumber int NOT NULL,
		LCC_PK uniqueidentifier,
		SubCode varchar(50) NOT NULL,
		UnitCount decimal(14, 4) NOT NULL,
		Reference1 varchar(50) NULL,
		Reference2 varchar(50) NULL,
		Reference3 varchar(50) NULL,
		Reference4 varchar(50) NULL
	)

	insert @raw(DatabaseNumber, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4)
	select DatabaseNumber, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4
	from " + GetUsageFromBilling(systemCode) + @"(@Period, @PeriodStartTimeUtc, @PeriodEndTimeUtc)

	INSERT @Result (LD_PK, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4)
	SELECT
		LD_PK,
		LCC_PK,
		SubCode,
		UnitCount = sum(UnitCount),
		Reference1, Reference2, Reference3, Reference4
	FROM @raw
	join dbo.LicenceDatabase d on LD_DatabaseNumber = DatabaseNumber
	where 1=1" + (productionSystemOnly ? " and LD_LicenceType = 'PRD'" : "") + @"
	group by LD_PK, LCC_PK, SubCode, Reference1, Reference2, Reference3, Reference4
		option(recompile)

	RETURN
END
";
			var sqlForSTL = @"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	LC_PK uniqueidentifier NULL,
	LD_PK uniqueidentifier NULL,
	LCC_PK uniqueidentifier NULL,
	SubCode varchar(50) NOT NULL,
	UnitCount decimal(14, 4) NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL,
	TotalPrice money NOT NULL DEFAULT(0),
	RX_NKCurrency varchar(3) NOT NULL DEFAULT (''),
	Direction varchar(3) NOT NULL DEFAULT ('')
)
BEGIN
	declare @raw TABLE
	(
		DatabaseNumber int NOT NULL,
		LCC_PK uniqueidentifier,
		SubCode varchar(50) NOT NULL,
		UnitCount decimal(14, 4) NOT NULL,
		Reference1 varchar(50) NULL,
		Reference2 varchar(50) NULL,
		Reference3 varchar(50) NULL,
		Reference4 varchar(50) NULL,
		TotalPrice money NOT NULL DEFAULT(0),
		RX_NKCurrency varchar(3) NOT NULL DEFAULT (''),
		Direction varchar(3) NOT NULL DEFAULT ('')
	)

	insert @raw(DatabaseNumber, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4, TotalPrice, RX_NKCurrency, Direction)
	select DatabaseNumber, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4, TotalPrice, RX_NKCurrency, Direction
	from " + GetUsageFromBilling(systemCode) + @"(@Period, @PeriodStartTimeUtc, @PeriodEndTimeUtc)

	INSERT @Result (LD_PK, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4, TotalPrice, RX_NKCurrency, Direction)
	SELECT
		LD_PK,
		LCC_PK,
		SubCode,
		UnitCount = sum(UnitCount),
		Reference1, Reference2, Reference3, Reference4,
		TotalPrice = sum(TotalPrice), RX_NKCurrency, Direction
	FROM @raw
	join dbo.LicenceDatabase d on LD_DatabaseNumber = DatabaseNumber
	where 1=1" + (productionSystemOnly ? " and LD_LicenceType = 'PRD'" : "") + @"
	group by LD_PK, LCC_PK, SubCode, Reference1, Reference2, Reference3, Reference4, RX_NKCurrency, Direction
		option(recompile)

	RETURN
END
";

			string routineType = DbRoutineType.SqlFunctionTableTypeDesc;
			return new DatabaseViewAndRoutineCreateScript(name,
				systemCode == BillingConstants.BillingSystem.STL ? sqlForSTL : sql,
				"DROP FUNCTION " + name,
				routineType);
		}

		public static DatabaseViewAndRoutineCreateScript GetUsageConverterFromLegacyId(string systemCode, bool includeNonProduction = false)
		{
			return GetUsageCreateScript(systemCode,
@"	SELECT
		LC_PK, 
		LD_PK,
		LCC_PK,
		SubCode,
		UnitCount = sum(UnitCount),
		Reference1,
		Reference2,
		Reference3,
		Reference4
	from " + GetUsageByLegacyIdFunctionName(systemCode) + @"(@Period, @PeriodStartTimeUtc, @PeriodEndTimeUtc)
	join dbo.LicenceEnterprise on LE_EnterpriseCode = EnterpriseCode COLLATE DATABASE_DEFAULT
	join dbo.LicenceDatabase on LD_LE = LE_PK and LD_ServerCode = ServerCode COLLATE DATABASE_DEFAULT
	outer apply
	(
		select top 1 LCC_PK, LCC_OH from " + ViewClientCompanyCodeHistory + @"
		where LCC_LD = LD_PK
			and LCC_Code = CompanyCode COLLATE DATABASE_DEFAULT
		order by (case when LCC_CodeValidFromUtc <= DATEADD(DAY, 15, @PeriodStartTimeUtc) then 0 else 1 end), LCC_CodeValidFromUtc desc
	) a
	left join dbo.LicenceCompany on LC_OH = LCC_OH"
			+ (includeNonProduction ? "" : @"
	where LD_LicenceType = 'PRD'")
			+ @"
	group by
		LC_PK,
		LD_PK,
		LCC_PK,
		SubCode,
		Reference1,
		Reference2,
		Reference3,
		Reference4");
		}

		internal static DatabaseViewAndRoutineCreateScript GetUsageCreateScript(string systemCode, string selectSql)
		{
			string name = GetUsageFunctionOrProcedureName(systemCode);
			string sql = @"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime)
RETURNS @Result TABLE
(
	LC_PK uniqueidentifier NULL,
	LD_PK uniqueidentifier NULL,
	LCC_PK uniqueidentifier NULL,
	SubCode varchar(50) NOT NULL,
	UnitCount int NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN
	INSERT @Result (LC_PK, LD_PK, LCC_PK, SubCode, UnitCount, Reference1, Reference2, Reference3, Reference4)
" + selectSql + @"
	RETURN
END
";
			string routineType = DbRoutineType.SqlFunctionTableTypeDesc;
			return new DatabaseViewAndRoutineCreateScript(name,
				sql,
				"DROP FUNCTION " + name,
				routineType);
		}

		public const string GetDatabaseBillingHostedLocations = ObjectPrefix + "GetDatabaseBillingHostedLocations";

		public static DatabaseViewAndRoutineCreateScript GetDatabaseBillingHostedLocationsScript()
		{
			return new DatabaseViewAndRoutineCreateScript(GetDatabaseBillingHostedLocations,
@"CREATE FUNCTION " + GetDatabaseBillingHostedLocations + @"()
RETURNS @Result TABLE
(
	Code char(3) NOT NULL, 
	IsCW char(1) NULL
)
BEGIN

DECLARE @xmldata XML;
SET @xmldata = (SELECT CAST(CONVERT(NVARCHAR(MAX), SD_BinaryValue) AS XML) FROM dbo.StmData WHERE SD_Name = 'DatabaseBillingHostedLocations');

INSERT @Result(Code, IsCW)
SELECT T.X.value('(Code/text())[1]', 'char(3)') Code,
		T.X.value('(Bool/text())[1]', 'char(1)') IsCW
FROM @xmlData.nodes('/ArrayOfCodeDescriptionBool/CodeDescriptionBool') AS T(X);

--The default values of dbo.EdiGetDatabaseBillingHostedLocations()/Registry.DatabaseBillingHostedLocations.Default must be synchronized logically.
IF NOT EXISTS (SELECT * FROM @Result)
BEGIN
	INSERT INTO @Result(Code, IsCW) VALUES ('NCW', 'N');
	INSERT INTO @Result(Code, IsCW) VALUES ('SYD', 'Y');
	INSERT INTO @Result(Code, IsCW) VALUES ('CHI', 'Y');
	INSERT INTO @Result(Code, IsCW) VALUES ('LON', 'Y');
	INSERT INTO @Result(Code, IsCW) VALUES ('TRA', 'N');
END

RETURN;
END
", "DROP FUNCTION " + GetDatabaseBillingHostedLocations, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		public const string GetHandheldDevicePremiumTypes = ObjectPrefix + "GetHandheldDevicePremiumTypes";

		public static DatabaseViewAndRoutineCreateScript GetHandheldDevicePremiumTypesScript()
		{
			var defaultValueAsSql = new StringBuilder();
			foreach (ICodeDescription codeDescription in EDIDataRegistry.Instance.HandheldDevicePremiumTypes.DefaultValue)
			{
				defaultValueAsSql.AppendLine($"INSERT @Result(Code) VALUES ('{codeDescription.Code}');");
			}

			return new DatabaseViewAndRoutineCreateScript(GetHandheldDevicePremiumTypes,
@"CREATE FUNCTION " + GetHandheldDevicePremiumTypes + @"()
RETURNS @Result TABLE
(
	Code char(3) NOT NULL
)
BEGIN

	DECLARE @xmldata XML;
	SET @xmldata = (SELECT CAST(CONVERT(VARCHAR(MAX), SD_BinaryValue) AS XML) FROM dbo.StmData WHERE SD_Name = 'HandheldDevicePremiumTypes');

	IF @xmldata IS NOT NULL
	BEGIN
		INSERT @Result(Code)
		SELECT DISTINCT T.X.value('(Code/text())[1]', 'char(3)') Code
		FROM @xmlData.nodes('/NewDataSet/Table1') AS T(X);
	END
	ELSE
	BEGIN
		" +
		defaultValueAsSql.ToString()
		+
@"
	END

	RETURN;
END
", "DROP FUNCTION " + GetHandheldDevicePremiumTypes, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#region GetBillingCountryGroups

		public const string GetBillingCountryGroups = ObjectPrefix + "GetBillingCountryGroups";

		public static DatabaseViewAndRoutineCreateScript GetBillingCountryGroupsScript()
		{
			return new DatabaseViewAndRoutineCreateScript(GetBillingCountryGroups,
@"CREATE FUNCTION " + GetBillingCountryGroups + @"()
RETURNS @Result TABLE
(
	BCG_CountryCode CHAR(2) NOT NULL PRIMARY KEY,
	BCG_MainCountryCode CHAR(2) NOT NULL,
	BDG_IsDomesticUserGroup CHAR(1) NOT NULL
)
BEGIN

DECLARE @xmldata XML;
SET @xmldata = (SELECT CAST(CONVERT(NVARCHAR(MAX), SD_BinaryValue) AS XML) FROM dbo.StmData WHERE SD_Name = 'BillingCountryGroup');

INSERT @Result(BCG_CountryCode, BCG_MainCountryCode, BDG_IsDomesticUserGroup)
SELECT
	T.X.value('(Code/text())[1]', 'char(2)') BCG_CountryCode,
	T.X.value('(Description/text())[1]', 'char(2)') BCG_MainCountryCode,
	T.X.value('(Bool/text())[1]', 'char(1)') BDG_IsDomesticUserGroup
FROM @xmlData.nodes('/ArrayOfCodeDescriptionBool/CodeDescriptionBool') AS T(X);


--The default values of dbo.EdiGetBillingCountryGroups() & Registry.BillingCountryGroups.Default must be synchronized logically.
IF NOT EXISTS (SELECT * FROM @Result)
BEGIN
	INSERT INTO @Result(BCG_CountryCode, BCG_MainCountryCode, BDG_IsDomesticUserGroup)
	VALUES
		('CN', 'CN', 'Y'),
		('MO', 'CN', 'Y'),
		('HK', 'CN', 'Y');
END

RETURN;
END
", "DROP FUNCTION " + GetBillingCountryGroups, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		#region GenericUsage

		public static DatabaseViewAndRoutineCreateScript GetGenericUsageScript()
		{
			string name = GetUsageFunctionOrProcedureName("GenericUsage");

			return new DatabaseViewAndRoutineCreateScript(name,
@"CREATE FUNCTION " + name + @"(@Period int, @PeriodStartTimeUtc smalldatetime, @PeriodEndTimeUtc smalldatetime, @Category varchar(3))
RETURNS @Result TABLE
(
	LC_PK uniqueidentifier NULL,
	LD_PK uniqueidentifier NOT NULL,
	LCC_PK uniqueidentifier NULL,
	SubCode varchar(50) NOT NULL,
	UnitCount int NOT NULL,
	Reference1 varchar(50) NULL,
	Reference2 varchar(50) NULL,
	Reference3 varchar(50) NULL,
	Reference4 varchar(50) NULL
)
BEGIN

	DECLARE @Raw TABLE
	(
		DatabaseNumber int NOT NULL,
		SubCode varchar(50) NOT NULL,
		LCC_PK uniqueidentifier,
		UnitCount INT NOT NULL
	);

	INSERT @Raw(DatabaseNumber, SubCode, LCC_PK, UnitCount)
	SELECT TX_DatabaseNumber, TX_PriceItemCode, TX_LCC, UnitCount = Sum(TX_BillableCount)
	FROM dbo.BillingViewChargeable
	WHERE TX_Category = @Category AND TX_Period = @Period
	GROUP BY TX_DatabaseNumber, TX_PriceItemCode, TX_LCC;

	INSERT @Result(LD_PK, LCC_PK, SubCode, UnitCount)
	SELECT LD_PK, LCC_PK, SubCode, UnitCount = SUM(UnitCount)
	FROM
	(
		SELECT LD_PK = ISNULL(EDH_LD_ConsolidatedDatabase, LD_PK), 
			   LCC_PK = IIF(EDH_LD_ConsolidatedDatabase IS NULL, LCC_PK, NULL),
			   SubCode, UnitCount
		FROM @Raw R
		JOIN dbo.LicenceDatabase ON LD_DatabaseNumber = DatabaseNumber
		LEFT JOIN dbo.EdiLicenceDatabaseConsolidationHistory ON EDH_LD = LD_PK AND EDH_Period = @Period
		WHERE LD_LicenceType = 'PRD'
	) T
	GROUP BY LD_PK, LCC_PK, SubCode;

	-- insert min fee. Note: @Category and LD_Product may differ.
	DECLARE @UsageBillingSettingsXml XML = (SELECT cast(SD_BinaryValue AS NVARCHAR(MAX)) FROM dbo.StmData WHERE SD_Name = 'UsageBillingSettings');
	DECLARE @UsageMinimumFeeSettingsXml XML = (SELECT CAST(SD_BinaryValue AS NVARCHAR(MAX)) FROM dbo.StmData WHERE SD_Name = 'UsageMinimumFeeSettings');

	IF @UsageBillingSettingsXml IS NOT NULL AND @UsageMinimumFeeSettingsXml IS NOT NULL
	BEGIN
		DECLARE @MinFeeCode varchar(3);
		DECLARE @ProductCode varchar(3)
		
		SELECT @MinFeeCode = MFE.MFE_MinFeeCode, @ProductCode = MFE_ProductCode
		FROM
		(
			SELECT 
				BIL_ProductCode = NodeTable.Item.value('ProductCode[1]', 'varchar(3)'),
				BIL_RawUsageCategory = NodeTable.Item.value('RawUsageCategory[1]', 'varchar(3)'),
				BIL_PriceListCode = NodeTable.Item.value('PriceListCode[1]', 'varchar(3)')
			FROM @UsageBillingSettingsXml.nodes('/UsageBillingSettings/Products/PriceLists') AS NodeTable(Item)
		) BIL
		JOIN 
		(
			SELECT 
				MFE_ProductCode = NodeTable.Item.value('ProductCode[1]', 'varchar(3)'),
				MFE_PriceListCode = NodeTable.Item.value('PriceListCode[1]', 'varchar(3)'),
				MFE_MinFeeCode = NodeTable.Item.value('MinimumFeeCode[1]', 'varchar(3)')
			FROM @UsageMinimumFeeSettingsXml.nodes('/UsageMinimumFeeSettings/MinimumFeeUsageList/MinimumFeeUsage') AS NodeTable(Item)
		) MFE ON BIL_ProductCode = MFE_ProductCode AND BIL_PriceListCode = MFE_PriceListCode 
		WHERE BIL_RawUsageCategory = @Category;
 
		IF @MinFeeCode IS NOT NULL AND @ProductCode IS NOT NULL
		BEGIN
			INSERT INTO @Result (LD_PK, SubCode, UnitCount)
			SELECT LD.LD_PK, @MinFeeCode, 1
			FROM dbo.LicenceDatabase LD
			LEFT JOIN dbo.EdiLicenceDatabaseConsolidationHistory 
					  ON EDH_Period = @Period AND EDH_LD = LD.LD_PK AND EDH_LD_ConsolidatedDatabase != LD.LD_PK 
			LEFT JOIN @Result R ON R.LD_PK = LD.LD_PK
			WHERE LD_Product = @ProductCode
			  AND LD_LicenceType = 'PRD'
			  AND LD_IsActive = 1
			  AND R.LD_PK IS NULL  -- no usages yet.
			  AND EDH_LD IS NULL;  -- master LD only.
		END
	END

	RETURN
END
", "DROP FUNCTION " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		#endregion

		static string GetUsageFunctionOrProcedureName(string systemCode)
		{
			return ObjectPrefix + "GetChargeableUsage" + systemCode;
		}

		static string GetUsageByLegacyIdFunctionName(string systemCode)
		{
			return ObjectPrefix + "GetChargeableUsageByLegacyId" + systemCode;
		}

		static string GetUsageFromBilling(string systemCode)
		{
			return ObjectPrefix + "GetChargeableUsageFromBilling" + systemCode;
		}

		#endregion
	}
}
