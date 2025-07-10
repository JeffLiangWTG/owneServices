using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(Report_CustomsEntriesbyBroker))]
	class Report_CustomsEntriesbyBrokerTest : DbCreateScriptTest
	{
		#region SQL
		const string Sql = @"
			declare @Comp uniqueIdentifier
			declare @Branch uniqueIdentifier, @CountryCode VARCHAR(2)
			SELECT TOP 1 @CountryCode = GC_RN_NKCountryCode, @Branch = GB_PK, @Comp = GB_GC FROM dbo.GlbBranch INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK WHERE GC_RN_NKCountryCode <> 'CA';

			declare @RN_Code varchar(2)
			set @RN_Code = (select top 1 RN_Code from dbo.refcountry)

			declare @JE_PK uniqueIdentifier
			set @JE_PK = NEWID()

			declare @ch1_PK uniqueIdentifier
			set @ch1_PK = NEWID()

			declare @ch2_PK uniqueIdentifier
			set @ch2_PK = NEWID()

			declare @JZ_PK uniqueIdentifier
			set @JZ_PK = NEWID()

			declare @CEI1_PK uniqueIdentifier, @CEI2_PK uniqueIdentifier
			set @CEI1_PK = NEWID()
			set @CEI2_PK = NEWID()

			insert into dbo.JobDeclaration
			(
				JE_PK, JE_DataModel, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_DeclarationReference, JE_ClusterKey, JE_CustomsCommencedDate
			)
			values
			(
				@JE_PK, @CountryCode, @Branch, @Comp, getutcdate(), 'B0001000', 1, @JE_CustomsCommencedDate
			)

			insert into dbo.JobComInvoiceHeader
			(
				JZ_PK, JZ_DataModel, JZ_JE, JZ_GB, JZ_ClusterKey
			)
			values
			(
				@JZ_PK, @CountryCode, @JE_PK, @Branch, 1
			)

			insert into dbo.CusEntryInstruction
			(CEI_PK, CEI_DataModel, CEI_Style, CEI_JE, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
			values
			(@CEI1_PK, @CountryCode, '1', @JE_PK, 1, getutcdate(), '~BP', getutcdate(), '~BP'),
			(@CEI2_PK, @CountryCode, '2', @JE_PK, 1, getutcdate(), '~BP', getutcdate(), '~BP');

			insert into dbo.CusEntryHeader
			(CH_PK, CH_DataModel, CH_JE, CH_BGMReference, CH_ClusterKey, CH_CEI_Instruction, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
			values
			(@ch1_PK, @CountryCode, @JE_PK, 'CEH BGM 1', 1, @CEI1_PK, getutcdate(), '~BP', getutcdate(), '~BP'),
			(@ch2_PK, @CountryCode, @JE_PK, 'CEH BGM 2', 1, @CEI2_PK, getutcdate(), '~BP', getutcdate(), '~BP');

			insert into dbo.CusEntryNum
			(CE_PK, CE_EntryType, CE_ParentID, CE_RN_NKCountryCode, CE_EntryNum, CE_EntryIsSystemGenerated, CE_ParentTable, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
			values
			(NEWID(), 'EXP', @ch1_PK, @RN_Code, 'One:11111', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
			(NEWID(), 'EXP', @ch1_PK, @RN_Code, 'One:22222', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
			(NEWID(), 'EXP', @ch2_PK, @RN_Code, 'Two:1111', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
			(NEWID(), 'UCR', @JE_PK, @RN_Code, 'Dec:aaaaa', 1, 'JobDeclaration', getutcdate(), '~BP', getutcdate(), '~BP');

			insert into dbo.JobComInvoiceLine
			(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey, JI_CEI)
			values
			(NEWID(), @CountryCode, @JZ_PK, 1, @CEI1_PK),
			(NEWID(), @CountryCode, @JZ_PK, 1, @CEI2_PK),
			(NEWID(), @CountryCode, @JZ_PK, 1, @CEI2_PK);

			insert into dbo.StmALog
			(
				SL_PK, SL_EventTime, SL_PostedTimeUtc, SL_SE_NKEvent, SL_Parent, SL_Table, SL_IsEstimate, SL_IsCancelled
			)
			values
			(
				NEWID(), GETDATE(), GETUTCDATE(), 'ccc', @JE_PK, 'JobDeclaration', @SL_IsEstimate, @SL_IsCancelled
			)

			select * from Report_CustomsEntriesbyBroker('ccc', @EventFromDate, @EventToDate, @EventFromDateUtc, @EventToDateUtc, @Comp)
			order by CE_EntryNum
		";
		#endregion

		public void TestFunctionalityOfTheFunction()
		{
			using (var command = CargoWise.Data.Db.Connection.Command(Sql))
			{
				command.AddParameter("@SL_IsEstimate", SqlDbType.Char, 'N');
				command.AddParameter("@SL_IsCancelled", SqlDbType.Char, 'N');
				command.AddParameter("@JE_CustomsCommencedDate", SqlDbType.DateTime, DateTime.Today);
				command.AddParameter("@EventFromDate", SqlDbType.DateTime, DateTime.Now.AddMonths(-6));
				command.AddParameter("@EventToDate", SqlDbType.DateTime, DateTime.Now.AddMonths(6));
				command.AddParameter("@EventFromDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(-6));
				command.AddParameter("@EventToDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(6));

				using (var reader = command.ExecuteReader())
				{
					AssertEquals("One row", true, reader.Read());
					AssertEquals("The CusEntryNum field shows entry's numbers.", "One:11111, One:22222", reader["CE_EntryNum"]);
					AssertEquals("A single declaration.", "B0001000", reader["JE_DeclarationReference"]);
					AssertEquals("Does not show dec's own number", false, reader["CE_EntryNum"].ToString().Contains("Dec:aaaaa"));
					AssertEquals("Invoice lines count", 1, reader["Invoice"]);

					AssertEquals("Two rows, one for each CEH, not each CEN", true, reader.Read());
					AssertEquals("The CusEntryNum field shows entry's numbers.", "Two:1111", reader["CE_EntryNum"]);
					AssertEquals("A single declaration.", "B0001000", reader["JE_DeclarationReference"]);
					AssertEquals("Does not show dec's own number", false, reader["CE_EntryNum"].ToString().Contains("Dec:aaaaa"));
					AssertEquals("Invoice lines count", 2, reader["Invoice"]);

					AssertEquals("Two rows for two entries", false, reader.Read());
				}
			}
		}

		public void TestEventFromDateUtc_LessThanOneYearAhead()
		{
			using (var command = CargoWise.Data.Db.Connection.Command(Sql))
			{
				command.AddParameter("@SL_IsEstimate", SqlDbType.Char, 'N');
				command.AddParameter("@SL_IsCancelled", SqlDbType.Char, 'N');
				command.AddParameter("@JE_CustomsCommencedDate", SqlDbType.DateTime, DateTime.Today);
				command.AddParameter("@EventFromDate", SqlDbType.DateTime, DateTime.Now.AddMonths(-6));
				command.AddParameter("@EventToDate", SqlDbType.DateTime, DateTime.Now.AddMonths(6));
				command.AddParameter("@EventFromDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddYears(1).AddDays(-1));
				command.AddParameter("@EventToDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddYears(2));

				using (var reader = command.ExecuteReader())
				{
					AssertEquals("Should be able to get declaration data when @EventFromDateUtc is less than 1 year ahead of declaration create time", true, reader.Read());
				}
			}
		}

		public void TestEventFromDateUtc_MoreThanOneYearAhead()
		{
			using (var command = CargoWise.Data.Db.Connection.Command(Sql))
			{
				command.AddParameter("@SL_IsEstimate", SqlDbType.Char, 'N');
				command.AddParameter("@SL_IsCancelled", SqlDbType.Char, 'N');
				command.AddParameter("@JE_CustomsCommencedDate", SqlDbType.DateTime, DateTime.Today);
				command.AddParameter("@EventFromDate", SqlDbType.DateTime, DateTime.Now.AddMonths(-6));
				command.AddParameter("@EventToDate", SqlDbType.DateTime, DateTime.Now.AddMonths(6));
				command.AddParameter("@EventFromDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddYears(1).AddDays(1));
				command.AddParameter("@EventToDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddYears(2));

				using (var reader = command.ExecuteReader())
				{
					AssertEquals("Should not be able to get declaration data when @EventFromDateUtc is more than 1 year ahead of declaration create time", false, reader.Read());
				}
			}
		}

		public void TestDateParametersNotSpecified()
		{
			using (var command = CargoWise.Data.Db.Connection.Command(Sql))
			{
				command.AddParameter("@SL_IsEstimate", SqlDbType.Char, 'N');
				command.AddParameter("@SL_IsCancelled", SqlDbType.Char, 'N');
				command.AddParameter("@JE_CustomsCommencedDate", SqlDbType.DateTime, DateTime.Today);
				command.AddParameter("@EventFromDate", SqlDbType.DateTime, DateTime.Now.AddMonths(-6));
				command.AddParameter("@EventToDate", SqlDbType.VarChar, "");
				command.AddParameter("@EventFromDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(-6));
				command.AddParameter("@EventToDateUtc", SqlDbType.VarChar, "");

				using (var reader = command.ExecuteReader())
				{
					AssertEquals("One row", true, reader.Read());
				}
			}
		}

		public void TestEstimatedLogEntry()
		{
			using (var command = CargoWise.Data.Db.Connection.Command(Sql))
			{
				command.AddParameter("@SL_IsEstimate", SqlDbType.Char, 'Y');
				command.AddParameter("@SL_IsCancelled", SqlDbType.Char, 'N');
				command.AddParameter("@JE_CustomsCommencedDate", SqlDbType.DateTime, DBNull.Value);
				command.AddParameter("@EventFromDate", SqlDbType.DateTime, DateTime.Now.AddMonths(-6));
				command.AddParameter("@EventToDate", SqlDbType.DateTime, DateTime.Now.AddMonths(6));
				command.AddParameter("@EventFromDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(-6));
				command.AddParameter("@EventToDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(6));

				using (var reader = command.ExecuteReader())
				{
					AssertEquals("No rows", false, reader.Read());
				}
			}
		}

		public void TestCancelledLogEntry()
		{
			using (var command = CargoWise.Data.Db.Connection.Command(Sql))
			{
				command.AddParameter("@SL_IsEstimate", SqlDbType.Char, 'N');
				command.AddParameter("@SL_IsCancelled", SqlDbType.Char, 'Y');
				command.AddParameter("@JE_CustomsCommencedDate", SqlDbType.DateTime, DBNull.Value);
				command.AddParameter("@EventFromDate", SqlDbType.DateTime, DateTime.Now.AddMonths(-6));
				command.AddParameter("@EventToDate", SqlDbType.DateTime, DateTime.Now.AddMonths(6));
				command.AddParameter("@EventFromDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(-6));
				command.AddParameter("@EventToDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(6));

				using (var reader = command.ExecuteReader())
				{
					AssertEquals("No rows", false, reader.Read());
				}
			}
		}

		public void TestCustomsCommencedFields()
		{
			using (var command = CargoWise.Data.Db.Connection.Command(Sql))
			{
				command.AddParameter("@SL_IsEstimate", SqlDbType.Char, 'N');
				command.AddParameter("@SL_IsCancelled", SqlDbType.Char, 'N');
				command.AddParameter("@JE_CustomsCommencedDate", SqlDbType.DateTime, DateTime.Today);
				command.AddParameter("@EventFromDate", SqlDbType.DateTime, DateTime.Now.AddMonths(-6));
				command.AddParameter("@EventToDate", SqlDbType.DateTime, DateTime.Now.AddMonths(6));
				command.AddParameter("@EventFromDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(-6));
				command.AddParameter("@EventToDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(6));

				using (var reader = command.ExecuteReader())
				{
					AssertEquals("One row", true, reader.Read());
					AssertEquals("SL_EventTimeCCC", DateTime.Today, (DateTime)reader["SL_EventTimeCCC"]);
				}
			}
		}

		public void TestIsActiveFlag()
		{
			#region SQL
			string @sql = @"
				declare @Comp uniqueIdentifier
				set @Comp = (select top 1 GC_PK from dbo.GlbCompany)

				declare @Branch uniqueIdentifier
				set @Branch = (select top 1 GB_PK from dbo.GlbBranch where gb_gc = @Comp)

				declare @RN_Code varchar(2)
				set @RN_Code = (select top 1 rn_code from dbo.refcountry)

				declare @je_PK uniqueIdentifier
				set @je_PK = NEWID()

				declare @ch1_PK uniqueIdentifier
				set @ch1_PK = NEWID()

				declare @ch2_PK uniqueIdentifier
				set @ch2_PK = NEWID()

				declare @jz_pk uniqueIdentifier
				set @jz_pk = NEWID()

				insert into dbo.JobDeclaration
				(
					JE_PK, JE_DataModel, jE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_DeclarationReference, JE_ClusterKey, JE_CustomsCommencedDate
				)
				values
				(
					@je_PK, 'AU', @Branch, @Comp, getdate(), 'B0001000', 1, @JE_CustomsCommencedDate
				)

				insert into dbo.JobComInvoiceHeader 
				(
					JZ_PK, JZ_DataModel, JZ_JE, JZ_GB, JZ_ClusterKey 
				)
				values
				(
					@jz_pk, 'AU', @je_PK, @Branch, 1
				)

				insert into dbo.JobComInvoiceLine
				(
					JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey
				)
				values
				(
					NEWID(), 'AU', @jz_pk, 1
				)

				insert into dbo.JobComInvoiceLine
				(
					JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey
				)
				values
				(
					NEWID(), 'AU', @jz_pk, 1
				)

				insert into dbo.CusEntryHeader 
				(CH_PK, CH_DataModel, CH_JE, CH_BGMReference, CH_AddInfo, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
				values
				(
				@ch1_PK, 'AU', @JE_PK, 'CEH BGM 1', 'EDITransmitDate=2015-02-02 00:00:00.000*IsActive=Y*LastEntryStyle=SIM*LastNumberOfLinesSentToCustoms=1*RecordAdded=2014-11-21 15:50:56.957', 1, getutcdate(), '~BP', getutcdate(), '~BP'
				)

				insert into dbo.CusEntryHeader 
				(CH_PK, CH_DataModel, CH_JE, CH_BGMReference, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
				values
				(
				@ch2_PK, 'AU', @JE_PK, 'CEH BGM 2', 1, getutcdate(), '~BP', getutcdate(), '~BP'
				)

				insert into dbo.CusEntryNum
				(CE_PK, CE_EntryType, CE_ParentID, CE_RN_NKCountryCode, CE_EntryNum, CE_EntryIsSystemGenerated, CE_ParentTable, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
				values
				(NEWID(), 'EXP', @ch1_PK, @RN_Code, 'One:11111', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), 'EXP', @ch1_PK, @RN_Code, 'One:22222', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), 'EXP', @ch2_PK, @RN_Code, 'Two:1111', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), 'UCR', @je_PK, @RN_Code, 'Dec:aaaaa', 1, 'JobDeclaration', getutcdate(), '~BP', getutcdate(), '~BP');

				insert into dbo.StmALog
				(
				SL_PK, SL_EventTime, SL_PostedTimeUtc, SL_SE_NKEvent, SL_Parent, SL_Table
				)
				values
				(
				NEWID(), GETDATE(), GETUTCDATE(), 'ccc', @je_PK, 'JobDeclaration'
				)

				select * from Report_CustomsEntriesbyBroker('ccc', dateadd(m, -6, GetDate()), dateadd(m, 6, GetDate()), dateadd(m, -6, GETUTCDATE()), dateadd(m, 6, GETUTCDATE()), @Comp)
				order by CE_EntryNum
			";
			#endregion

			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				command.AddParameter("@JE_CustomsCommencedDate", SqlDbType.DateTime, DateTime.Today);
				using (var reader = command.ExecuteReader())
				{
					AssertEquals("One row", true, reader.Read());
					AssertEquals("The CusEntryNum field shows entry's numbers.", "One:11111, One:22222", reader["CE_EntryNum"]);
					AssertEquals("A single declaration.", "B0001000", reader["JE_DeclarationReference"]);
					AssertEquals("Does not show dec's own number", false, reader["CE_EntryNum"].ToString().Contains("Dec:aaaaa"));

					AssertEquals("Two rows, one for each CEH, not each CEN", true, reader.Read());
					AssertEquals("The CusEntryNum field shows entry's numbers.", "Two:1111", reader["CE_EntryNum"]);
					AssertEquals("A single declaration.", "B0001000", reader["JE_DeclarationReference"]);
					AssertEquals("Does not show dec's own number", false, reader["CE_EntryNum"].ToString().Contains("Dec:aaaaa"));

					AssertEquals("Two rows for two entries", false, reader.Read());
				}
			}
		}

		public void TestGetEntryNumberForAUEXPDeclaration()
		{
			#region SQL

			string sql = @"
				declare @Comp uniqueIdentifier
				declare @Branch uniqueIdentifier, @CountryCode VARCHAR(2)
				SELECT TOP 1 @CountryCode = GC_RN_NKCountryCode, @Branch = GB_PK, @Comp = GB_GC FROM dbo.GlbBranch INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK;

				declare @RN_Code varchar(2)
				set @RN_Code = (select top 1 RN_Code from dbo.refcountry)

				declare @JE_PK uniqueIdentifier
				set @JE_PK = NEWID()

				declare @CH_PK uniqueIdentifier
				set @CH_PK = NEWID()

				declare @JZ_PK uniqueIdentifier
				set @JZ_PK = NEWID()

				insert into dbo.JobDeclaration
				(
					JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_DeclarationReference, JE_ClusterKey, JE_CustomsCommencedDate
				)
				values
				(
					@JE_PK, @CountryCode, 'EXP', @Branch, @Comp, getdate(), 'B0001000', 1, @JE_CustomsCommencedDate
				)

				insert into dbo.JobComInvoiceHeader
				(
					JZ_PK, JZ_DataModel, JZ_JE, JZ_GB, JZ_ClusterKey
				)
				values
				(
					@JZ_PK, @CountryCode, @JE_PK, @Branch, 1
				)

				insert into dbo.JobComInvoiceLine
				(
					JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey
				)
				values
				(
					NEWID(), @CountryCode, @JZ_PK, 1
				)

				insert into dbo.JobComInvoiceLine
				(
					JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey
				)
				values
				(
					NEWID(), @CountryCode, @JZ_PK, 1
				)

				insert into dbo.CusEntryHeader
				(CH_PK, CH_DataModel, CH_JE, CH_BGMReference, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
				values
				(@CH_PK, @CountryCode, @JE_PK, 'CEH BGM 1', 1, getutcdate(), '~BP', getutcdate(), '~BP')

				insert into dbo.CusEntryNum
				(CE_PK, CE_EntryType, CE_ParentID, CE_RN_NKCountryCode, CE_EntryNum, CE_EntryIsSystemGenerated, CE_ParentTable, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
				values
				(NEWID(), 'CAN', @CH_PK, @RN_Code, 'One:11111', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), 'UCR', @JE_PK, @RN_Code, 'Dec:aaaaa', 1, 'JobDeclaration', getutcdate(), '~BP', getutcdate(), '~BP');

				insert into dbo.StmALog
				(
					SL_PK, SL_EventTime, SL_PostedTimeUtc, SL_SE_NKEvent, SL_Parent, SL_Table, SL_IsEstimate, SL_IsCancelled
				)
				values
				(
					NEWID(), GETDATE(), GETUTCDATE(), 'ccc', @JE_PK, 'JobDeclaration', @SL_IsEstimate, @SL_IsCancelled
				)

				select * from Report_CustomsEntriesbyBroker('ccc', dateadd(m, -6, GetDate()), dateadd(m, 6, GetDate()), dateadd(m, -6, GETUTCDATE()), dateadd(m, 6, GETUTCDATE()), @Comp)
				order by CE_EntryNum
			";

			#endregion

			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				command.AddParameter("@SL_IsEstimate", SqlDbType.Char, 'N');
				command.AddParameter("@SL_IsCancelled", SqlDbType.Char, 'N');
				command.AddParameter("@JE_CustomsCommencedDate", SqlDbType.DateTime, DateTime.Today);

				using (var reader = command.ExecuteReader())
				{
					AssertEquals("One row", true, reader.Read());
					AssertEquals("The CusEntryNum field shows entry's numbers.", "One:11111", reader["CE_EntryNum"]);
					AssertEquals("A single declaration.", "B0001000", reader["JE_DeclarationReference"]);
					AssertEquals("Only one row for EntryNum against EntryHeader", false, reader.Read());
				}
			}
		}

		public void TestNullCusEntryInstructionInInvoiceLines()
		{
			const string sql = @"
				declare @Comp uniqueIdentifier
				declare @Branch uniqueIdentifier, @CountryCode VARCHAR(2)
				SELECT TOP 1 @CountryCode = GC_RN_NKCountryCode, @Branch = GB_PK, @Comp = GB_GC FROM dbo.GlbBranch INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK;

				declare @RN_Code varchar(2)
				set @RN_Code = (select top 1 RN_Code from dbo.refcountry)

				declare @JE_PK uniqueIdentifier
				set @JE_PK = NEWID()

				declare @ch1_PK uniqueIdentifier
				set @ch1_PK = NEWID()

				declare @ch2_PK uniqueIdentifier
				set @ch2_PK = NEWID()

				declare @JZ_PK uniqueIdentifier
				set @JZ_PK = NEWID()

				insert into dbo.JobDeclaration
				(
					JE_PK, JE_DataModel, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_DeclarationReference, JE_ClusterKey, JE_CustomsCommencedDate
				)
				values
				(
					@JE_PK, @CountryCode, @Branch, @Comp, getutcdate(), 'B0001000', 1, @JE_CustomsCommencedDate
				)

				insert into dbo.JobComInvoiceHeader
				(
					JZ_PK, JZ_DataModel, JZ_JE, JZ_GB, JZ_ClusterKey
				)
				values
				(
					@JZ_PK, @CountryCode, @JE_PK, @Branch, 1
				)

				insert into dbo.CusEntryHeader
				(CH_PK, CH_DataModel, CH_JE, CH_BGMReference, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
				values
				(@ch1_PK, @CountryCode, @JE_PK, 'CEH BGM 1', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
				(@ch2_PK, @CountryCode, @JE_PK, 'CEH BGM 2', 1, getutcdate(), '~BP', getutcdate(), '~BP');

				insert into dbo.CusEntryNum
				(CE_PK, CE_EntryType, CE_ParentID, CE_RN_NKCountryCode, CE_EntryNum, CE_EntryIsSystemGenerated, CE_ParentTable, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
				values
				(NEWID(), 'EXP', @ch1_PK, @RN_Code, 'One:11111', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), 'EXP', @ch1_PK, @RN_Code, 'One:22222', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), 'EXP', @ch2_PK, @RN_Code, 'Two:1111', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
				(NEWID(), 'UCR', @JE_PK, @RN_Code, 'Dec:aaaaa', 1, 'JobDeclaration', getutcdate(), '~BP', getutcdate(), '~BP');

				insert into dbo.JobComInvoiceLine
				(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey)
				values
				(NEWID(), @CountryCode, @JZ_PK, 1),
				(NEWID(), @CountryCode, @JZ_PK, 1),
				(NEWID(), @CountryCode, @JZ_PK, 1);

				insert into dbo.StmALog
				(
					SL_PK, SL_EventTime, SL_PostedTimeUtc, SL_SE_NKEvent, SL_Parent, SL_Table, SL_IsEstimate, SL_IsCancelled
				)
				values
				(
					NEWID(), GETDATE(), GETUTCDATE(), 'ccc', @JE_PK, 'JobDeclaration', @SL_IsEstimate, @SL_IsCancelled
				)

				select * from Report_CustomsEntriesbyBroker('ccc', @EventFromDate, @EventToDate, @EventFromDateUtc, @EventToDateUtc, @Comp)
				order by CE_EntryNum
			";
			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				command.AddParameter("@SL_IsEstimate", SqlDbType.Char, 'N');
				command.AddParameter("@SL_IsCancelled", SqlDbType.Char, 'N');
				command.AddParameter("@JE_CustomsCommencedDate", SqlDbType.DateTime, DateTime.Today);
				command.AddParameter("@EventFromDate", SqlDbType.DateTime, DateTime.Now.AddMonths(-6));
				command.AddParameter("@EventToDate", SqlDbType.DateTime, DateTime.Now.AddMonths(6));
				command.AddParameter("@EventFromDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(-6));
				command.AddParameter("@EventToDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(6));

				using (var reader = command.ExecuteReader())
				{
					AssertEquals("One row", true, reader.Read());
					AssertEquals("The CusEntryNum field shows entry's numbers.", "One:11111, One:22222", reader["CE_EntryNum"]);
					AssertEquals("A single declaration.", "B0001000", reader["JE_DeclarationReference"]);
					AssertEquals("Does not show dec's own number", false, reader["CE_EntryNum"].ToString().Contains("Dec:aaaaa"));
					AssertEquals("Invoice lines count", 3, reader["Invoice"]);

					AssertEquals("Two rows, one for each CEH, not each CEN", true, reader.Read());
					AssertEquals("The CusEntryNum field shows entry's numbers.", "Two:1111", reader["CE_EntryNum"]);
					AssertEquals("A single declaration.", "B0001000", reader["JE_DeclarationReference"]);
					AssertEquals("Does not show dec's own number", false, reader["CE_EntryNum"].ToString().Contains("Dec:aaaaa"));
					AssertEquals("Invoice lines count", 3, reader["Invoice"]);

					AssertEquals("Two rows for two entries", false, reader.Read());
				}
			}
		}

		public void TestOnlyOneRowIsFoundWhenDeclarationHasTwoEntries_CAB3C()
		{
			AssertOnlyOneRowIsFoundWhenDeclarationHasTwoEntries_CA("B3C");
		}

		public void TestOnlyOneRowIsFoundWhenDeclarationHasTwoEntries_CACAD()
		{
			AssertOnlyOneRowIsFoundWhenDeclarationHasTwoEntries_CA("CAD");
		}

		void AssertOnlyOneRowIsFoundWhenDeclarationHasTwoEntries_CA(string messageType)
		{
			#region SQL
			const string sql = @"
DECLARE @Comp UNIQUEIDENTIFIER
SET @Comp = NEWID()
DECLARE @Branch UNIQUEIDENTIFIER, @CountryCode VARCHAR(2)
SET @Branch = NEWID()
SET @CountryCode = 'CA'

INSERT INTO dbo.GlbCompany
	(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser, GC_SystemCreateUser)
VALUES
	(@Comp, @CountryCode, 'CAD', 'HXU', 'AU company', GetUtcDate(), GetUtcDate(), 'E', 'E')

INSERT INTO dbo.GlbBranch 
	(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser, GB_SystemCreateUser)
VALUES
	(@Branch, @Comp, 'HXU', GetUtcDate(), GetUtcDate(), 'E', 'E')

DECLARE @RN_Code VARCHAR(2)
SET @RN_Code = (SELECT TOP 1 RN_Code FROM dbo.refcountry)

DECLARE @JE_PK UNIQUEIDENTIFIER
SET @JE_PK = NEWID()

DECLARE @ch1_PK UNIQUEIDENTIFIER
SET @ch1_PK = NEWID()

DECLARE @ch2_PK UNIQUEIDENTIFIER
SET @ch2_PK = NEWID()

DECLARE @JZ_PK UNIQUEIDENTIFIER
SET @JZ_PK = NEWID()

DECLARE @CEI1_PK uniqueIdentifier, @CEI2_PK uniqueIdentifier
SET @CEI1_PK = NEWID()
SET @CEI2_PK = NEWID()

INSERT INTO dbo.JobDeclaration
	(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_MessageType, JE_SystemCreateTimeUtc, JE_DeclarationReference, JE_ClusterKey, JE_CustomsCommencedDate)
VALUES
	(@JE_PK, @CountryCode, @Branch, @Comp, 'IMP', getutcdate(), 'B0001000', 1, @JE_CustomsCommencedDate)

INSERT INTO dbo.JobComInvoiceHeader
	(JZ_PK, JZ_DataModel, JZ_JE, JZ_GB, JZ_ClusterKey)
VALUES
	(@JZ_PK, @CountryCode, @JE_PK, @Branch, 1)

INSERT INTO dbo.CusEntryInstruction
	(CEI_PK, CEI_DataModel, CEI_Style, CEI_JE, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
VALUES
	(@CEI1_PK, @CountryCode, '1', @JE_PK, 1, getutcdate(), '~BP', getutcdate(), '~BP'),
	(@CEI2_PK, @CountryCode, '2', @JE_PK, 1, getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO dbo.CusEntryHeader
	(CH_PK, CH_DataModel, CH_JE, CH_BGMReference, CH_ClusterKey, CH_CEI_Instruction, CH_MessageType, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES
	(@ch1_PK, @CountryCode, @JE_PK, 'CEH BGM 1', 1, @CEI1_PK, 'REL', getutcdate(), '~BP', getutcdate(), '~BP'),
	(@ch2_PK, @CountryCode, @JE_PK, 'CEH BGM 2', 1, @CEI2_PK, @CH_MessageType, getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO dbo.CusEntryNum
	(CE_PK, CE_EntryType, CE_ParentID, CE_RN_NKCountryCode, CE_EntryNum, CE_EntryIsSystemGenerated, CE_ParentTable, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
VALUES
	(NEWID(), 'EXP', @ch1_PK, @RN_Code, 'One:11111', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
	(NEWID(), 'EXP', @ch1_PK, @RN_Code, 'One:22222', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
	(NEWID(), 'EXP', @ch2_PK, @RN_Code, 'Two:1111', 1, 'CusEntryHeader', getutcdate(), '~BP', getutcdate(), '~BP'),
	(NEWID(), 'UCR', @JE_PK, @RN_Code, 'Dec:aaaaa', 1, 'JobDeclaration', getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO dbo.JobComInvoiceLine
	(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey, JI_CEI)
VALUES
	(NEWID(), @CountryCode, @JZ_PK, 1, @CEI1_PK),
	(NEWID(), @CountryCode, @JZ_PK, 1, @CEI2_PK),
	(NEWID(), @CountryCode, @JZ_PK, 1, @CEI2_PK);

INSERT INTO dbo.StmALog
	(SL_PK, SL_EventTime, SL_PostedTimeUtc, SL_SE_NKEvent, SL_Parent, SL_Table, SL_IsEstimate, SL_IsCancelled)
VALUES
	(NEWID(), GETDATE(), GETUTCDATE(), 'ccc', @JE_PK, 'JobDeclaration', @SL_IsEstimate, @SL_IsCancelled)

SELECT COUNT(*) FROM Report_CustomsEntriesbyBroker('ccc', @EventFromDate, @EventToDate, @EventFromDateUtc, @EventToDateUtc, @Comp)
		";
			#endregion

			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				command.AddParameter("@SL_IsEstimate", SqlDbType.Char, 'N');
				command.AddParameter("@SL_IsCancelled", SqlDbType.Char, 'N');
				command.AddParameter("@JE_CustomsCommencedDate", SqlDbType.DateTime, DateTime.Today);
				command.AddParameter("@EventFromDate", SqlDbType.DateTime, DateTime.Now.AddMonths(-6));
				command.AddParameter("@EventToDate", SqlDbType.DateTime, DateTime.Now.AddMonths(6));
				command.AddParameter("@EventFromDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(-6));
				command.AddParameter("@EventToDateUtc", SqlDbType.DateTime, DateTime.UtcNow.AddMonths(6));
				command.AddParameter("@CH_MessageType", SqlDbType.VarChar, messageType);

				using (var reader = command.ExecuteReader())
				{
					AssertEquals("One row", true, reader.Read());
					AssertEquals("Only one rows", 1, (int)reader[0]);
				}
			}
		}
	}
}
