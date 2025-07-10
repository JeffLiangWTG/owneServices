using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	static class TestDataSetup
	{
		public static void SetupUSData(this DbConnection connection, string systemCreateDate = null)
		{
			connection.SetupUSData("US", systemCreateDate);
		}

		static void SetupUSData(this DbConnection connection, string countryCode, string systemCreateDate)
		{
			connection.SetupDataFor(countryCode, new[] { "ACS", "ACE" }, new[] { "REC", "EXP", "IMX", "PRO", "DRW", "IMP", "FTZ", "MSC" }, System.Array.Empty<string>(), systemCreateDate);
		}

		public static void SetupPRData(this DbConnection connection, string systemCreateDate = null)
		{
			connection.SetupUSData("PR", systemCreateDate);
		}

		public static void SetupAUData(this DbConnection connection, string systemCreateDate = null)
		{
			connection.SetupDataFor("AU", new[] { "AIR", "CMR", "LEG" }, new[] { "EXP", "AQS", "REF", "IMX", "DRW", "IMP", "WEA", "MSC", "EXW", "POC" }, System.Array.Empty<string>(), systemCreateDate);
		}

		public static void SetupSGData(this DbConnection connection, string systemCreateDate = null)
		{
			connection.SetupDataFor("SG", new[] { "SG4", "4.1", "NTP" }, new[] { "TNP", "OUT", "EXP", "COO", "IMP", "INP", "IPT" }, System.Array.Empty<string>(), systemCreateDate);
		}

		public static void SetupNZData(this DbConnection connection, string systemCreateDate = null)
		{
			connection.SetupDataFor("NZ", new[] { "TSW", "CUS" }, new[] { "EXP", "EXC", "IMP", "MSC" }, new[] { "IPI", "COM", "I10", "PER", "EXC", "E40", "DRB", "SIT", "ECI", "NOR", "I11", "I51", "SIM", "TMP" }, systemCreateDate);
		}

		public static void SetupGBData(this DbConnection connection, string systemCreateDate = null)
		{
			connection.SetupDataFor("GB", new[] { "CHF", "EMC", "EMS" }, new[] { "EXP", "EMC", "NCT", "IMP", "MSC" }, System.Array.Empty<string>(), systemCreateDate);
		}

		public static void SetupCAData(this DbConnection connection, string systemCreateDate = null)
		{
			connection.SetupDataFor("CA", System.Array.Empty<string>(), new[] { "EXP", "LVX", "B2", "IMP", "LVS", "MSC", "IM2" }, System.Array.Empty<string>(), systemCreateDate);
		}

		public static void SetupZAData(this DbConnection connection, string systemCreateDate = null)
		{
			connection.SetupDataFor("ZA", new[] { "ITF", "BLT" }, new[] { "EXP", "IMX", "IMP", "MSC", "EXW", "OTH" }, System.Array.Empty<string>(), systemCreateDate);
		}

		public static void SetupDEData(this DbConnection connection, string systemCreateDate = null)
		{
			connection.SetupDataFor("DE", new[] { "ITF", "EMC", "BLT" }, new[] { "EXP", "IMX", "IMP", "MSC", "EXW" }, System.Array.Empty<string>(), systemCreateDate);
		}

		public static void SetupDataFor(this DbConnection connection, string countryCode, IEnumerable<string> applicationCodes, IEnumerable<string> messageTypes, IEnumerable<string> messageSubTypes, string systemCreateDate)
		{
			var script = @"
DECLARE @companyPK UNIQUEIDENTIFIER = NEWID(), @branchPK UNIQUEIDENTIFIER = NEWID()
INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (@companyPK, @countryCode, @countryCode + '$', 'AU company')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (@branchPK, @companyPK, @countryCode + '#')
DECLARE @minClusterKey INT = (SELECT ISNULL(MAX(JE_ClusterKey), 0) FROM dbo.JobDeclaration) + 1

DECLARE @EntryInstructionPK UNIQUEIDENTIFIER,
	@DecPK UNIQUEIDENTIFIER,
	@ClusterKey INT;

SELECT ApplicationCode, MessageType, MessageSubType, TransportMode, ClusterKey
INTO #Scenarios
FROM
(
	SELECT ApplicationCode, MessageType, MessageSubType, TransportMode, RN + (RN - 1) + (RN - 1) + @minClusterKey ClusterKey
	FROM
	(
		SELECT ApplicationCode, MessageType, MessageSubType, TransportMode, ROW_NUMBER() OVER (ORDER BY ApplicationCode, MessageType, MessageSubType, TransportMode) as RN
		FROM
		(
			SELECT Split.a.value('.', 'NVARCHAR(MAX)') AS ApplicationCode
			FROM
			(
				SELECT CAST('<X>'+REPLACE(@applicationCodes, '|', '</X><X>')+'</X>' AS XML) AS String
			) AS A
			CROSS APPLY String.nodes('/X') AS Split(a)
		) AS ApplicationCodes
		INNER JOIN
		(
			SELECT Split.a.value('.', 'NVARCHAR(MAX)') AS MessageType
			FROM
			(
				SELECT CAST('<X>'+REPLACE(@messageTypes, '|', '</X><X>')+'</X>' AS XML) AS String
			) AS A
			CROSS APPLY String.nodes('/X') AS Split(a)
		) AS MessageTypes ON 1 = 1
		INNER JOIN
		(
			SELECT Split.a.value('.', 'NVARCHAR(MAX)') AS MessageSubType
			FROM
			(
				SELECT CAST('<X>'+REPLACE(@messageSubTypes, '|', '</X><X>')+'</X>' AS XML) AS String
			) AS A
			CROSS APPLY String.nodes('/X') AS Split(a)
		) AS MessageSubTypes ON 1 = 1
		INNER JOIN
		(
			SELECT Split.a.value('.', 'NVARCHAR(MAX)') AS TransportMode
			FROM
			(
				SELECT CAST('<X>'+REPLACE(@transportModes, '|', '</X><X>')+'</X>' AS XML) AS String
			) AS A
			CROSS APPLY String.nodes('/X') AS Split(a)
		) AS TransportModes ON 1 = 1
	) A
) B

select * from #Scenarios ORDER BY ApplicationCode, MessageType, MessageSubType, TransportMode

INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_MessageType, JE_MessageSubType, JE_TransportMode, JE_SystemCreateTimeUtc, JE_ClusterKey, JE_AgentsReference)
SELECT
	NEWID() as JE_PK,
	@countryCode as JE_DataModel,
	@branchPK as JE_GB,
	@companyPK as JE_GC,
	@countryCode + ApplicationCode + MessageType + MessageSubType + TransportMode + convert(varchar(5), ClusterKey) + '1' as JE_DeclarationReference,
	ApplicationCode as JE_ApplicationCode,
	MessageType as JE_MessageType,
	MessageSubType as JE_MessageSubType,
	TransportMode as JE_TransportMode,
	@systemCreateDate as JE_SystemCreateTimeUtc,
	ClusterKey as JE_ClusterKey,
	'1' as JE_AgentsReference
FROM #Scenarios

SET @EntryInstructionPK = NEWID();
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_SystemCreateUser, CEI_SystemCreateTimeUTC, CEI_SystemLastEditUser, CEI_SystemLastEditTimeUTC)
SELECT
	NEWID() AS CEI_PK,
	@countryCode as CEI_DataModel,
	JE_PK as CEI_PK,
	JE_ClusterKey as CEI_ClusterKey,
	'~BP' as CEI_SystemCreateUser,
	 @systemCreateDate as CEI_SystemCreateTimeUTC, 
	 '~BP' as CEI_SystemLastEditUser, 
	 @systemCreateDate as CEI_SystemLastEditTimeUTC
FROM dbo.JobDeclaration
WHERE JE_ClusterKey >= @minClusterKey

INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_CEI_Instruction, CH_EntrySubmittedDate, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
SELECT
	NEWID() AS CH_PK,
	@countryCode as CH_DataModel,
	JE_PK as CH_PK,
	CEI_PK as CH_CEI_Instruction,
	@systemCreateDate as CH_EntrySubmittedDate,
	JE_ClusterKey as CH_ClusterKey,
	 getutcdate() as CH_SystemCreateTimeUTC, 
	 '~BP' as CH_SystemCreateUser,
	 getutcdate() as CH_SystemLastEditTimeUTC,
	 '~BP' as CH_SystemLastEditUser
FROM dbo.JobDeclaration
JOIN dbo.CusEntryInstruction on CEI_ClusterKey = JE_ClusterKey
WHERE JE_ClusterKey >= @minClusterKey

INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_MessageType, JE_MessageSubType, JE_TransportMode, JE_SystemCreateTimeUtc, JE_ClusterKey, JE_AgentsReference)
SELECT
	NEWID() as JE_PK,
	@countryCode as JE_DataModel,
	@branchPK as JE_GB,
	@companyPK as JE_GC,
	@countryCode + ApplicationCode + MessageType + MessageSubType + TransportMode + convert(varchar(5), ClusterKey + 1) + '2' as JE_DeclarationReference,
	ApplicationCode as JE_ApplicationCode,
	MessageType as JE_MessageType,
	MessageSubType as JE_MessageSubType,
	TransportMode as JE_TransportMode,
	@systemCreateDate as JE_SystemCreateTimeUtc,
	ClusterKey + 1 as JE_ClusterKey,
	'2' as JE_AgentsReference
FROM #Scenarios

SET @DecPK = NEWID();
SET @ClusterKey += 1;
INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_MessageType, JE_MessageSubType, JE_TransportMode, JE_SystemCreateTimeUtc, JE_ClusterKey, JE_AgentsReference)
SELECT
	NEWID() as JE_PK,
	@countryCode as JE_DataModel,
	@branchPK as JE_GB,
	@companyPK as JE_GC,
	@countryCode + ApplicationCode + MessageType + MessageSubType + TransportMode + convert(varchar(5), ClusterKey + 2) + '3' as JE_DeclarationReference,
	ApplicationCode as JE_ApplicationCode,
	MessageType as JE_MessageType,
	MessageSubType as JE_MessageSubType,
	TransportMode as JE_TransportMode,
	@systemCreateDate as JE_SystemCreateTimeUtc,
	ClusterKey + 2 as JE_ClusterKey,
	'3' as JE_AgentsReference
FROM #Scenarios

INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_SystemCreateUser, CEI_SystemCreateTimeUTC, CEI_SystemLastEditUser, CEI_SystemLastEditTimeUTC)
SELECT
	NEWID() as CEI_PK,
	@countryCode as CEI_DataModel,
	JE_PK as CEI_JE,
	JE_ClusterKey as CEI_ClusterKey,
	'~BP' as CEI_SystemCreateUser,
	 @systemCreateDate as CEI_SystemCreateTimeUTC, 
	 '~BP' as CEI_SystemLastEditUser, 
	 @systemCreateDate as CEI_SystemLastEditTimeUTC
FROM dbo.JobDeclaration
cross apply (select 1 b union select 2) c
WHERE JE_AgentsReference = '3' and JE_ClusterKey >= @minClusterKey

INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_CEI_Instruction, CH_EntrySubmittedDate, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
SELECT
	NEWID() as CH_PK,
	@countryCode as CH_DataModel,
	JE_PK as CH_JE,
	CEI_PK as CH_CEI_Instruction,
	@systemCreateDate as CH_EntrySubmittedDate,
	JE_ClusterKey as CH_ClusterKey,
	 getutcdate() as CH_SystemCreateTimeUTC, 
	 '~BP' as CH_SystemCreateUser,
	 getutcdate() as CH_SystemLastEditTimeUTC,
	 '~BP' as CH_SystemLastEditUser
FROM dbo.JobDeclaration
JOIN dbo.CusEntryInstruction on CEI_ClusterKey = JE_ClusterKey
WHERE JE_AgentsReference = '3' and JE_ClusterKey >= @minClusterKey

INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_JE, JZ_ClusterKey, JZ_GB, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
SELECT
	NEWID() as JZ_PK,
	@countryCode as JZ_DataModel,
	JE_PK as JZ_JE,
	JE_ClusterKey as JZ_ClusterKey,
	@branchPK as JZ_GB,
	JE_SystemCreateTimeUtc as JZ_SystemCreateTimeUtc, 
	'~BP' as JZ_SystemCreateUser,
	JE_SystemCreateTimeUtc as JZ_SystemLastEditTimeUtc,
	'~BP' as JZ_SystemLastEditUser
FROM dbo.JobDeclaration
";
			using (var cmd = connection.Command(script))
			{
				cmd.AddParameter("@countryCode", SqlDbType.VarChar, countryCode);
				cmd.AddParameter("@applicationCodes", SqlDbType.VarChar, GetDelimitedData(applicationCodes));
				cmd.AddParameter("@messageTypes", SqlDbType.VarChar, GetDelimitedData(messageTypes));
				cmd.AddParameter("@messageSubTypes", SqlDbType.VarChar, GetDelimitedData(messageSubTypes));
				cmd.AddParameter("@transportModes", SqlDbType.VarChar, "RAI|ROA|TRK|SEA|AIR|");
				cmd.AddParameter("@systemCreateDate", SqlDbType.VarChar, systemCreateDate ?? "2019-01-01");
				cmd.ExecuteNonQuery();
			}
		}

		static string GetDelimitedData(IEnumerable<string> codes)
		{
			var list = codes.Select(x => x.PadRight(3)).ToList();
			if (!list.Contains("   "))
			{
				list.Add("   ");
			}
			return string.Join("|", list);
		}
	}
}
