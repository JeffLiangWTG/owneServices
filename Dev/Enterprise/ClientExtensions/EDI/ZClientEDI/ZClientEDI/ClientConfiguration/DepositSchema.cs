using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI
{
	public static class DepositSchema
	{
		#region EdiDepositBalance

		internal static DatabaseObjectCreateScript EdiDepositBalance
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiDepositBalance", @"
CREATE TABLE dbo.EdiDepositBalance
(
	[DEB_OH] [uniqueidentifier] NOT NULL constraint [EdiDepositBalance_DEB_OH_FK2_OrgHeader] REFERENCES [OrgHeader] ([OH_PK]) on delete cascade,
	[DEB_ChargeCode] [varchar](10) NOT NULL,
	[DEB_RX_NKCurrency] [varchar](3) NOT NULL,
	[DEB_Amount] [money] NOT NULL,
	[DEB_Tax] [money] NOT NULL default(0),
	[DEB_LastTransactionUtc] smalldatetime NOT NULL,
	[DEB_LastUpdatedUtc] smalldatetime NOT NULL,
	[DEB_RX_NKBaseCurrency] [varchar](3) NOT NULL default (''),
	[DEB_RX_NKBaseAmount] [money] NOT NULL default(0),
	[DEB_RX_NKBaseDate] date NULL,
	[DEB_IsValid] BIT NOT NULL DEFAULT 1,
);

ALTER TABLE [EdiDepositBalance]
    SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE INDEX NR_UX__EdiDepositBalance_DEB_OH_DEB_ChargeCode on EdiDepositBalance(DEB_OH, DEB_ChargeCode) WITH (ALLOW_PAGE_LOCKS = OFF)
;
",
"DROP TABLE EdiDepositBalance");
			}
		}

		#endregion

		#region EdiDepositAdjust

		internal static DatabaseObjectCreateScript EdiDepositAdjust
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiDepositAdjust", @"
CREATE TABLE dbo.EdiDepositAdjust
(
	[DEA_PK] UNIQUEIDENTIFIER NOT NULL,
	[DEA_OH] [uniqueidentifier] NOT NULL constraint [EdiDepositAdjust_DEA_OH_FK2_OrgHeader] REFERENCES [OrgHeader] ([OH_PK]) on delete cascade,
	[DEA_GC] UNIQUEIDENTIFIER NOT NULL constraint [EdiDepositAdjust_DEA_GC_FK2_GlbCompany] REFERENCES [GlbCompany] ([GC_PK]),
	[DEA_ChargeCode] [varchar](10) NOT NULL,
	[DEA_RX_NKCurrency] [varchar](3) NOT NULL,
	[DEA_Amount] [money] NOT NULL,
	[DEA_Tax] [money] NOT NULL default(0),
	[DEA_Description] NVARCHAR(100) NOT NULL DEFAULT '',
	[DEA_SystemCreateTimeUtc] smalldatetime NULL,
	[DEA_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
	[DEA_SystemLastEditTimeUtc] SMALLDATETIME NULL,
	[DEA_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
	CONSTRAINT [Constraint_DEA_RX_NKCurrency] check (DEA_RX_NKCurrency <> ''),
	CONSTRAINT [Constraint_DEA_Amounts] check (DEA_Amount <> 0 or DEA_Tax <> 0),
	CONSTRAINT [Constraint_DEA_ChargeCode] check (DEA_ChargeCode <> ''),
);

ALTER TABLE [EdiDepositAdjust]
    SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX NR_RC__DEA_OH_DEA_ChargeCode ON EdiDepositAdjust(DEA_OH, DEA_ChargeCode) WITH (ALLOW_PAGE_LOCKS = OFF)
;
",
"DROP TABLE EdiDepositAdjust");
			}
		}

		#endregion

		#region EdiDepositBalanceChanges

		internal static DatabaseObjectCreateScript EdiDepositBalanceChanges
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiDepositBalanceChanges", @"
CREATE TABLE dbo.EdiDepositBalanceChanges
(
	[DEC_AH] UNIQUEIDENTIFIER NULL,
	[DEC_DEA] UNIQUEIDENTIFIER NULL,
	[DEC_OH] UNIQUEIDENTIFIER NOT NULL,
	[DEC_AH_IsCancelled] bit NOT NULL default(0),
	[DEC_SystemLastEditTimeUtc] smalldatetime NOT NULL
);

ALTER TABLE [EdiDepositBalanceChanges]
    SET (LOCK_ESCALATION = DISABLE);

create clustered index NR_RC__DEC_OH on EdiDepositBalanceChanges(DEC_OH)
WITH (ALLOW_PAGE_LOCKS = OFF)
;
",
"DROP TABLE EdiDepositBalanceChanges");
			}
		}

		#endregion

		#region View Deposit Balance

		public static DatabaseViewAndRoutineCreateScript EdiViewDepositBalanceScript()
		{
			return new DatabaseViewAndRoutineCreateScript("EdiViewDepositBalance", @"
CREATE VIEW EdiViewDepositBalance AS
with cte(DEC_OH) as (select distinct DEC_OH from dbo.EdiDepositBalanceChanges)
select DEB_OH, DEB_ChargeCode, DEB_RX_NKCurrency, DEB_Amount, DEB_Tax, DEB_IsCurrent = cast(case when DEC_OH is null then 1 else 0 end as bit), DEB_IsValid, DEB_LastTransactionUtc
from dbo.EdiDepositBalance
left join cte a on a.DEC_OH = DEB_OH
union all 
select DEC_OH, '', '', cast(0 as money), cast(0 as money), cast(0 as bit), cast(1 as bit), cast(null as smalldatetime)
from cte
where DEC_OH not in (SELECT DEB_OH from dbo.EdiDepositBalance)
", "DROP VIEW EdiViewDepositBalance", DbRoutineType.SqlViewTypeDesc);
		}

		#endregion

		#region TriggerAccTransactionHeaderScript

		public static DatabaseViewAndRoutineCreateScript TriggerAccTransactionHeaderToDepositScript()
		{
			return new DatabaseViewAndRoutineCreateScript("TG_AccTransactionHeader_InsertToEdiDepositBalanceChanges", @"
CREATE TRIGGER TG_AccTransactionHeader_InsertToEdiDepositBalanceChanges
ON AccTransactionHeader
FOR INSERT, UPDATE
AS
SET NOCOUNT ON

INSERT INTO dbo.EdiDepositBalanceChanges (DEC_AH, DEC_OH, DEC_AH_IsCancelled, DEC_SystemLastEditTimeUtc)
SELECT
	DEC_AH = AH_PK,
	DEC_OH = AH_OH,
	DEC_AH_IsCancelled = AH_IsCancelled,
	DEC_SystemLastEditTimeUtc = AH_SystemLastEditTimeUtc
FROM inserted
WHERE AH_Ledger IN ('AR')
	AND AH_TransactionType IN ('INV', 'CRD')
	AND AH_OH IS NOT NULL
	AND AH_SystemLastEditTimeUtc IS NOT NULL
",
				"DROP TRIGGER TG_AccTransactionHeader_InsertToEdiDepositBalanceChanges",
				DbRoutineType.SqlTriggerTypeDesc);
		}

		#endregion

		#region TriggerEdiDepositAdjust

		public static DatabaseViewAndRoutineCreateScript TriggerEdiDepositAdjustScript()
		{
			return new DatabaseViewAndRoutineCreateScript("TG_EdiDepositAdjust_InsertToEdiDepositBalanceChanges", @"
CREATE TRIGGER TG_EdiDepositAdjust_InsertToEdiDepositBalanceChanges
ON EdiDepositAdjust
FOR INSERT, UPDATE, DELETE
AS
SET NOCOUNT ON

INSERT INTO dbo.EdiDepositBalanceChanges (DEC_DEA, DEC_OH, DEC_SystemLastEditTimeUtc)
SELECT
	DEC_DEA = DEA_PK,
	DEC_OH = DEA_OH,
	DEC_SystemLastEditTimeUtc = DEA_SystemLastEditTimeUtc
FROM inserted
union all
SELECT
	DEC_DEA = DEA_PK,
	DEC_OH = DEA_OH,
	DEC_SystemLastEditTimeUtc = DEA_SystemLastEditTimeUtc
FROM deleted
where not exists(select 1 from inserted)
",
				"DROP TRIGGER TG_EdiDepositAdjust_InsertToEdiDepositBalanceChanges",
				DbRoutineType.SqlTriggerTypeDesc);
		}

		#endregion

		#region Deposit Charge Codes

		public const string GetDepositChargeCodes = "EdiGetDepositChargeCodes";

		public static DatabaseViewAndRoutineCreateScript GetDepositChargeCodesScript()
		{
			const string name = GetDepositChargeCodes;
			return new DatabaseViewAndRoutineCreateScript(name, @"
CREATE function " + name + @"()
RETURNS @Result TABLE
(
	ChargeCode varchar(" + AccChargeCodeSchema.AC_Code.MaxLength.ToString(CultureInfo.InvariantCulture) + @") NOT NULL,
	MainChargeCode varchar(" + AccChargeCodeSchema.AC_Code.MaxLength.ToString(CultureInfo.InvariantCulture) + @") NULL
)
with schemabinding
BEGIN
	DECLARE @Xml XML = (SELECT CONVERT(VARCHAR(MAX), SD_BinaryValue) AS XML FROM dbo.StmData WHERE SD_Name = '" + EDIDataRegistry.DepositChargeCodesKeyName + @"');

	if @Xml is not null
	begin
		INSERT INTO @Result(ChargeCode, MainChargeCode)
		SELECT 
			NodeTable.Item.value('Code[1]', 'varchar(" + AccChargeCodeSchema.AC_Code.MaxLength.ToString(CultureInfo.InvariantCulture) + @")'),
			NULLIF(NodeTable.Item.value('Description[1]', 'varchar(" + AccChargeCodeSchema.AC_Code.MaxLength.ToString(CultureInfo.InvariantCulture) + @")'), '')
		FROM
			@Xml.nodes('/NewDataSet/Table1') AS NodeTable(Item)
	end
	else
	begin
		INSERT INTO @Result(ChargeCode, MainChargeCode)
		values " + AsSqlValues(EDIDataRegistry.Instance.DepositChargeCodes.DefaultValue.Cast<ICodeDescription>()) + @"
	end;

	return;
end

", "DROP function " + name, DbRoutineType.SqlFunctionTableTypeDesc);
		}

		static string AsSqlValues(IEnumerable<ICodeDescription> pairList)
		{
			StringBuilder s = new StringBuilder();
			foreach (var pair in pairList)
			{
				if (s.Length != 0)
				{
					s.Append(", ");
				}
				s.Append("('");
				s.Append(pair.Code);
				s.Append("', ");
				s.Append(string.IsNullOrEmpty(pair.Description) ? "NULL" : "'" + pair.Description + "'");
				s.Append(")");
			}

			return s.ToString();
		}

		#endregion

		#region EdiDepositBalanceUpdate stored procedure

		public static DatabaseViewAndRoutineCreateScript EdiDepositBalanceUpdateScript()
		{
			return new DatabaseViewAndRoutineCreateScript("EdiDepositBalanceUpdate", @"
CREATE PROCEDURE EdiDepositBalanceUpdate
	@Rebuild bit = 0
AS 
BEGIN
SET NOCOUNT ON;

BEGIN TRANSACTION;
BEGIN TRY

	DECLARE @CanGetLock INT;

	EXEC @CanGetLock = sp_getapplock @Resource = 'EdiDepositBalanceUpdate',
				@LockMode = 'Exclusive',
				@LockOwner =  'Transaction', 
				@LockTimeout = '0',
				@DbPrincipal = 'public';

	IF (@CanGetLock < 0)
	BEGIN
		DECLARE @Error VARCHAR(200);
		SET @Error = 'Could not obtain lock on ''EdiDepositBalanceUpdate'' - process is already running ' + CAST(@CanGetLock as VARCHAR(10));
		THROW 50000, @Error, 1
	END

	DECLARE @Changes TABLE
	(
		[DEC_AH] UNIQUEIDENTIFIER NULL,
		[DEC_DEA] UNIQUEIDENTIFIER NULL,
		[DEC_OH] UNIQUEIDENTIFIER NOT NULL,
		[DEC_AH_IsCancelled] bit NOT NULL,
		[DEC_SystemLastEditTimeUtc] smalldatetime NOT NULL
	);
	
	declare @hasChanges bit = 0;
	DELETE FROM dbo.EdiDepositBalanceChanges WITH (ROWLOCK, READPAST, READCOMMITTEDLOCK)
		OUTPUT DELETED.DEC_AH, DELETED.DEC_DEA, DELETED.DEC_OH, DELETED.DEC_AH_IsCancelled, DELETED.DEC_SystemLastEditTimeUtc
		INTO @Changes;

	if @@ROWCOUNT > 0
		set @hasChanges = 1

	if @hasChanges = 1 or @Rebuild = 1
	begin
		DECLARE @DepositChargeCode table (ChargeCode varchar(10) NOT NULL, MainChargeCode varchar(10) NULL)
		insert @DepositChargeCode(ChargeCode, MainChargeCode)
		select ChargeCode, MainChargeCode
		from EdiGetDepositChargeCodes()

		declare @line table
		(
			OH_PK uniqueidentifier not null,
			MainCode varchar(10) not null,
			GC_PK uniqueidentifier null,
			TransactionCurrency varchar(3) not null,
			OSAmount money not null,
			OSTax money not null,
			LocalAmount money not null,
			LocalTax money not null,
			AH_TransactionNum varchar(38) not null,
			SystemCreateTimeUtc smalldatetime not null,
			LocalCurrency varchar(3) not null,
			IsReciprocal bit not null
		)

		declare @ChargeCodePk table
		(
			MainCode varchar(10) not null,
			ChargeCode varchar(10) not null,
			AC_PK uniqueidentifier not null
		)

		insert @ChargeCodePk(MainCode, ChargeCode, AC_PK)
		select MainCode = ISNULL(MainChargeCode, ChargeCode), ChargeCode, AC_PK
		from @DepositChargeCode join dbo.AccChargeCode on AC_Code = ChargeCode collate database_default

		if @Rebuild = 1
		begin
			insert @line(OH_PK, MainCode, GC_PK, TransactionCurrency, OSAmount, OSTax, LocalAmount, LocalTax, SystemCreateTimeUtc, AH_TransactionNum, LocalCurrency, IsReciprocal)
			select OH_PK = AH_OH, MainCode, GC_PK = AH_GC, TransactionCurrency = AL_RX_NKTransactionCurrency
				, OSAmount
				, OSTax = AL_OSAmount - OSAmount
				, LocalAmount = AL_LineAmount
				, LocalTax = AL_GSTVAT
				, SystemCreateTimeUtc = AH_SystemCreateTimeUtc
				, AH_TransactionNum
				, LocalCurrency = GC_RX_NKLocalCurrency, IsReciprocal = GC_IsReciprocal
			from dbo.AccTransactionLines
			join dbo.AccTransactionHeader on AL_AH = AH_PK
			join dbo.GlbCompany on AH_GC = GlbCompany.GC_PK
			join dbo.RefCurrency on AL_RX_NKTransactionCurrency = RX_Code collate database_default
			join @ChargeCodePk on AC_PK = AL_AC
			cross apply
			(
				select OSAmount = case
							when AL_ExchangeRate = 1 then AL_LineAmount 
							when AL_GSTVAT = 0 then AL_OSAmount
							else ROUND(case when GC_IsReciprocal = 0 then AL_LineAmount * AL_ExchangeRate else AL_LineAmount / AL_ExchangeRate end
								, case when RX_SubUnitRatio <= 1 then 0 else cast(LOG10(RX_SubUnitRatio) as int) end)
							end
			) a
			where AH_IsCancelled = 0
			union all
			select OH_PK = DEA_OH, MainCode = ISNULL(MainChargeCode, ChargeCode), GC_PK = DEA_GC, TransactionCurrency = DEA_RX_NKCurrency
				, OSAmount = DEA_Amount, OSTax = DEA_Tax, LocalAmount = 0, LocalTax = 0, DEA_SystemCreateTimeUtc, AH_TransactionNum = ''
				, LocalCurrency = GC_RX_NKLocalCurrency, IsReciprocal = GC_IsReciprocal
			from dbo.EdiDepositAdjust
			join dbo.GlbCompany on DEA_GC = GC_PK
			join @DepositChargeCode on DEA_ChargeCode = ChargeCode collate database_default
		end
		else
		begin
			-- recalculate the balance from scratch for changed orgs

			insert @line(OH_PK, MainCode, GC_PK, TransactionCurrency, OSAmount, OSTax, LocalAmount, LocalTax, SystemCreateTimeUtc, AH_TransactionNum, LocalCurrency, IsReciprocal)
			select OH_PK = AH_OH, MainCode, GC_PK = AH_GC, TransactionCurrency = AL_RX_NKTransactionCurrency
				, OSAmount
				, OSTax = AL_OSAmount - OSAmount
				, LocalAmount = AL_LineAmount
				, LocalTax = AL_GSTVAT
				, SystemCreateTimeUtc = AH_SystemCreateTimeUtc
				, AH_TransactionNum
				, LocalCurrency = GC_RX_NKLocalCurrency, IsReciprocal = GC_IsReciprocal
			from dbo.AccTransactionLines
			join dbo.AccTransactionHeader on AL_AH = AH_PK
			join dbo.GlbCompany on AH_GC = GlbCompany.GC_PK
			join dbo.RefCurrency on AL_RX_NKTransactionCurrency = RX_Code collate database_default
			join @ChargeCodePk on AC_PK = AL_AC
			cross apply
			(
				select OSAmount = case
							when AL_ExchangeRate = 1 then AL_LineAmount 
							when AL_GSTVAT = 0 then AL_OSAmount
							else ROUND(case when GC_IsReciprocal = 0 then AL_LineAmount * AL_ExchangeRate else AL_LineAmount / AL_ExchangeRate end
								, case when RX_SubUnitRatio <= 1 then 0 else cast(LOG10(RX_SubUnitRatio) as int) end)
							end
			) a

			where AH_IsCancelled = 0
				and AH_OH in (select DEC_OH from @Changes)
			union all
			select OH_PK = DEA_OH, MainCode = ISNULL(MainChargeCode, ChargeCode), GC_PK = DEA_GC, TransactionCurrency = DEA_RX_NKCurrency
				, OSAmount = DEA_Amount, OSTax = DEA_TAx, LocalAmount = 0, LocalTax = 0, DEA_SystemCreateTimeUtc, AH_TransactionNum = ''
				, LocalCurrency = GC_RX_NKLocalCurrency, IsReciprocal = GC_IsReciprocal
			from dbo.EdiDepositAdjust
			join dbo.GlbCompany on DEA_GC = GC_PK
			join @DepositChargeCode on DEA_ChargeCode = ChargeCode collate database_default
			where DEA_OH in (select DEC_OH from @Changes)
		end

		declare @bal table
		(
			OH_PK uniqueidentifier not null,
			MainCode varchar(10) not null,
			Currency varchar(3) not null,
			Amount money not null,
			Tax money not null,
			LastDate smalldatetime not null,
			IsValid bit not null
		)

		-- most deposits are all in the same currency
		insert @bal(OH_PK, MainCode, Currency, Amount, Tax, LastDate, IsValid)
		select OH_PK, MainCode, Currency = Max(TransactionCurrency), Amount = sum(OSAmount), Tax = sum(OSTax)
			, LastDate = MAX(SystemCreateTimeUtc), IsValid = 1
		from @line
		group by OH_PK, MainCode
		having Max(TransactionCurrency) = MIN(TransactionCurrency)

		-- extract the lines for orgs with mixed currencies
		select line.OH_PK, line.MainCode, line.GC_PK, TransactionCurrency
			, OSAmount = sum(OSAmount)
			, OSTax = sum(OSTax)
			, LocalAmount = sum(LocalAmount)
			, LocalTax = sum(LocalTax)
			, SystemCreateTimeUtc, AH_TransactionNum
			, LocalCurrency, IsReciprocal
			, Seq = ROW_NUMBER() over (PARTITION BY line.OH_PK, line.MainCode order by SystemCreateTimeUtc, AH_TransactionNum)
		into #mixline
		from @line line
		left join @bal bal on line.OH_PK = bal.OH_PK and line.MainCode = bal.MainCode
		where bal.OH_PK is null
		group by line.OH_PK, line.MainCode, line.GC_PK, AH_TransactionNum, LocalCurrency, IsReciprocal, TransactionCurrency, SystemCreateTimeUtc

		if exists(select 1 from #mixline)
		begin
			create unique clustered index LineIndex on #mixline(Seq, MainCode, OH_PK)

			select OH_PK, MainCode, Currency = TransactionCurrency, Amount = OSAmount, Tax = OSTax
				, LastDate = SystemCreateTimeUtc, IsValid = CAST(1 AS BIT) 
			into #mixbal
			from #mixline
			where Seq = 1

			create unique clustered index BalIndex on #mixbal(OH_PK, MainCode)

			declare @seq int = 1
			while @seq > 0
			begin
				update #mixbal set
					Amount = CASE WHEN XRate IS NULL THEN Amount ELSE 
							Amount + (case
								when (Amount = 0) or (Currency = TransactionCurrency collate database_default) then OSAmount
								when Currency = LocalCurrency collate database_default then LocalAmount

								-- convert LocalCurrency to Currency
								else ROUND(case when IsReciprocal = 0 then LocalAmount * XRate else LocalAmount / XRate end,
									case when RX_SubUnitRatio <= 1 then 0 else cast(LOG10(RX_SubUnitRatio) as int) end)
								end) END,
					Tax =  CASE WHEN XRate IS NULL THEN Tax ELSE 
							Tax + (case
								when (Tax = 0) or (Currency = TransactionCurrency collate database_default) then OSTax
								when Currency = LocalCurrency collate database_default then LocalTax

								-- convert LocalCurrency to Currency
								else ROUND(case when IsReciprocal = 0 then LocalTax * XRate else LocalTax / XRate end,
									case when RX_SubUnitRatio <= 1 then 0 else cast(LOG10(RX_SubUnitRatio) as int) end)
								end) END,
					Currency = CASE WHEN XRate IS NULL THEN Currency ELSE (case when Amount = 0 then TransactionCurrency else Currency end) END,
					LastDate = CASE WHEN XRate IS NULL THEN LastDate ELSE SystemCreateTimeUtc END,
					IsValid  = CASE WHEN XRate IS NULL THEN 0 ELSE IsValid END
				from #mixbal
				join #mixline on @seq = Seq - 1 and #mixbal.OH_PK = #mixline.OH_PK and #mixbal.MainCode = #mixline.MainCode
				join dbo.RefCurrency on Currency = RX_Code collate database_default
				outer apply
				(
					select XRate = convert(DECIMAL(18,9), 1.0) where (Amount = 0 and Tax = 0) or Currency = TransactionCurrency collate database_default or Currency = LocalCurrency collate database_default
					union all
					select TOP 1 RE_SellRate
					FROM dbo.RefExchangeRate
					WHERE
						Amount != 0 and Currency != TransactionCurrency collate database_default and Currency != LocalCurrency collate database_default
						AND RE_GC = GC_PK
						AND RE_RX_NKExCurrency = Currency collate database_default
						AND RE_ExRateType = 'SEL'
						AND RE_StartDate <= SystemCreateTimeUtc
						AND RE_ExpiryDate >= SystemCreateTimeUtc
					ORDER BY RE_StartDate DESC
				) a

				set @seq = case when @@ROWCOUNT = 0 then 0 else @seq + 1 end
			end

			insert @bal (OH_PK, MainCode, Currency, Amount, Tax, LastDate, IsValid)
			select OH_PK, MainCode, Currency, Amount, Tax, LastDate, IsValid from #mixbal

			drop table #mixbal;
		end

		drop table #mixline;

		delete dbo.EdiDepositBalance
		from dbo.EdiDepositBalance
		left join @bal bal on bal.OH_PK = EdiDepositBalance.DEB_OH and bal.MainCode = EdiDepositBalance.DEB_ChargeCode collate database_default
		where bal.OH_PK is null
			and (@Rebuild = 1 or (EdiDepositBalance.DEB_OH in (select DEC_OH from @Changes)))

		declare @nowutc smalldatetime = getutcdate();

		merge into EdiDepositBalance as dest
		using (select * from @bal) as src
		on src.OH_PK = dest.DEB_OH and src.MainCode = dest.DEB_ChargeCode collate database_default
		when matched then update set
			DEB_RX_NKCurrency = src.Currency,
			DEB_Amount = src.Amount,
			DEB_Tax = src.Tax,
			DEB_LastTransactionUtc = src.LastDate,
			DEB_LastUpdatedUtc = @nowutc,
			DEB_RX_NKBaseCurrency = '',
			DEB_RX_NKBaseAmount = 0,
			DEB_RX_NKBaseDate = null,
			DEB_IsValid = src.IsValid
		when not matched by target then
			insert (DEB_OH, DEB_ChargeCode, DEB_RX_NKCurrency, DEB_Amount, DEB_Tax, DEB_LastTransactionUtc, DEB_LastUpdatedUtc, DEB_RX_NKBaseCurrency, DEB_RX_NKBaseAmount, DEB_RX_NKBaseDate, DEB_IsValid)
			values (src.OH_PK, src.MainCode, src.Currency, src.Amount, src.Tax, src.LastDate, @nowutc, '', 0, null, src.IsValid)
		OPTION (MAXDOP 1); -- MAXDOP 1 stops extra locks being taken on pages
	end

	commit transaction

END TRY
BEGIN CATCH
	
	IF (@@TRANCOUNT > 0)
	BEGIN
		ROLLBACK TRANSACTION;
	END

END CATCH
END
",
				"DROP procedure EdiDepositBalanceUpdate",
				DbRoutineType.SqlProcedureTypeDesc);
		}

		#endregion
	}
}
