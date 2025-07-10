using System;
using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.Definitions;
using Enterprise.Client.DHL.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		protected ClientOverride()
		{
		}

		#region Instance
		public static ClientOverride Instance
		{
			get { return instance ?? (instance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride instance;
		#endregion

		#region IClientHook Members

		public override Clients Client
		{
			get { return Clients.DHL; }
		}

		public const string DHL = "DHL";

		public override string ClientDisplayName
		{
			get { return DHL; }
		}

		public override IExtensionObjects DbSchemaExtensionObjects => new ExtensionObjects(TableCreationScripts, ViewAndRoutinesCreationScripts);

		static readonly ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts = ImmutableArray<DatabaseObjectCreateScript>.Empty;

		#region Reports SQL scripts

		static readonly ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts = ImmutableArray.Create(
			new DatabaseViewAndRoutineCreateScript("Client_GetCustomsEntryCharge", @"
CREATE FUNCTION Client_GetCustomsEntryCharge(@EntryHeader uniqueidentifier, @ChargeType varchar(3)) RETURNS decimal(10, 4)
BEGIN

	DECLARE @ChargeAmount as decimal(10, 4)

	SELECT @ChargeAmount = min(C1_ChargeAmount)
		FROM dbo.CusEntryHeaderCharges
		WHERE C1_CH = @EntryHeader and C1_ChargeType = @ChargeType

	RETURN @ChargeAmount
END
","drop function Client_GetCustomsEntryCharge", DbRoutineType.SqlFunctionScalarTypeDesc),

			new DatabaseViewAndRoutineCreateScript("ClientCustomsEntryPayments", @"
CREATE FUNCTION ClientCustomsEntryPayments(@StartDate datetime, @EndDate datetime)


RETURNS TABLE
AS
RETURN
(

SELECT	min(EM_SystemCreateTimeUtc) AS EntryDate,
	min(OH_FullName) AS Consignee,
	min(OH_Code) AS Importer,
	min(JE_DeclarationReference) AS JobNumber,
	min(JE_HouseBill) AS Airwaybill,
	min(CE_EntryNum) AS EntryNumber,
	min(CH_TotalPaid) AS CustomsTotal,
	dbo.Client_GetCustomsEntryCharge( CH_PK, 'DTY' ) AS CustomsDuty,
	dbo.Client_GetCustomsEntryCharge( CH_PK, 'REG' ) AS CustomsFee,
	min(JE_PaymentMethod) AS PaymentType

FROM	dbo.JobDeclaration
	LEFT OUTER JOIN dbo.OrgHeader ON OH_PK = JE_OH_Importer
	LEFT OUTER JOIN dbo.CusEntryHeader ON CH_JE = JE_PK
	LEFT OUTER JOIN dbo.CusEntryNum ON CE_ParentID = CH_PK
	LEFT OUTER JOIN dbo.CusEntryHeaderCharges ON C1_CH = CH_PK
	LEFT OUTER JOIN dbo.EDIMessage ON EM_LinkUniqueID = CH_PK

WHERE	CH_PK IN
		(SELECT EM_LinkUniqueID
		FROM	dbo.EDIMessage
		WHERE	EM_SystemCreateTimeUtc BETWEEN @StartDate AND @EndDate AND
			EM_ApplicationCode = 'UAE' AND EM_ReceiveTransmit = 'RCV' AND
			EM_MessageSubType = 'C' )
			AND C1_ChargeAmount != 0
GROUP BY CH_PK
)
","drop function ClientCustomsEntryPayments", DbRoutineType.SqlFunctionInlineTypeDesc)
		);

		#endregion

		#region ModuleOverrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();
			ClientOverrideModuleIdentifier jobDeclarationID = new ClientOverrideModuleIdentifier(ModuleIDs.Customs.JobDeclaration);
			moduleOverrides.AddModuleOverride(new ClientOverrideModuleInfo(jobDeclarationID, typeof(NZJobDeclarationModule), Enterprise.Core.Constants.CountryCodes.NewZealand));
			return moduleOverrides;
		}

		#endregion

		#endregion

	}
}
