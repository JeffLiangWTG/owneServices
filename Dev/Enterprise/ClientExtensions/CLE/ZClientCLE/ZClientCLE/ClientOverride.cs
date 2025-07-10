using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.CLE;
using Enterprise.Client.CLE.Modules;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Environment;
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
			get { return fInstance ?? (fInstance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride fInstance;
		#endregion

		#region IClientHook Members

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return CLEDataRegistry.Instance; }
		}

		public override Clients Client
		{
			get { return Clients.CLE; }
		}

		public override string ClientDisplayName
		{
			get { return "CLIENT"; }
		}

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();

			ClientOverrideModuleInfo ordersModuleInfo = new ClientOverrideModuleInfo(new ClientOverrideModuleIdentifier(ModuleIDs.Orders), typeof(CLEOrdersModule).Assembly.FullName, typeof(CLEOrdersModule).FullName);
			moduleOverrides.AddModuleOverride(ordersModuleInfo);

			ClientOverrideModuleInfo containerModuleInfo = new ClientOverrideModuleInfo(new ClientOverrideModuleIdentifier(ModuleIDs.Containers), typeof(CLEContainerModule).Assembly.FullName, typeof(CLEContainerModule).FullName);
			moduleOverrides.AddModuleOverride(containerModuleInfo);
			return moduleOverrides;
		}

		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (clientTypeDeciders == null)
				{
					if (!Globals.IsWeb)
					{
						var dic = new Dictionary<Type, ITypeDecider>();
						dic.Add(typeof(Order), new TypeDeciderImpl(typeof(CLEOrder)));
						clientTypeDeciders = new TypeDeciderDictionary(dic);
					}
				}
				return clientTypeDeciders;
			}
		}
		TypeDeciderDictionary clientTypeDeciders;

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new ExtensionObjects(
			ImmutableArray<DatabaseObjectCreateScript>.Empty,
			ViewAndRoutinesCreationScripts);

		protected static ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts { get; } = ImmutableArray.Create(
			new DatabaseViewAndRoutineCreateScript(
				"Client_CLE_GMCInvoiceExceptionReport",
				Client_CLE_GMCInvoiceExceptionReportScript,
				"Drop Function Client_CLE_GMCInvoiceExceptionReport",
				DbRoutineType.SqlFunctionTableTypeDesc)
		);

		#region Client_CLE_GMCInvoiceExceptionReportScript
		const string Client_CLE_GMCInvoiceExceptionReportScript = @"

		CREATE FUNCTION Client_CLE_GMCInvoiceExceptionReport
(
	@CompanyPK uniqueidentifier,
	@NoOfDays as varchar(3)
)
RETURNS @Invoice TABLE
(
	InvoiceNumber varchar(38),
	JobInvoiceNumber varchar(38),
	InvoiceDate smalldatetime,
	PostDate smalldatetime,
	InvoiceAmount money,
	Currency char(3),
	JobNumber varchar(35)
)

BEGIN
	 DECLARE @NoOfDaysInInt smallint
	 SET @NoOfDaysInInt = (CASE WHEN ISNUMERIC(@NoOfDays) = 1
								  THEN -CAST(@NoOfDays AS smallint) ELSE 0 END)

    INSERT @Invoice
			SELECT
				AH_TransactionNum,
				AH_ConsolidatedInvoiceRef,
				AH_InvoiceDate,
				AH_PostDate,
				AH_OSTotal,
				RX_Code,
				JH_JobNum
			FROM dbo.AccTransactionHeader
			INNER JOIN dbo.JobHeader ON JH_PK = AH_JH AND JH_GC = @CompanyPK
			LEFT JOIN
			(
				SELECT SL_Parent as Link,
							 Count(*) as Count
				FROM dbo.Stmalog
				WHERE SL_IsCancelled != 'Y'
				AND SL_SE_NKEvent ='DEX'
				AND SL_Reference LIKE 'GMC AR Invoice Export%'
				Group by SL_Parent
			) DEXEvent ON DEXEvent.Link = AH_PK
			INNER JOIN dbo.RefCurrency on RX_Code =AH_RX_NKTransactionCurrency
			INNER JOIN
			(
				SELECT
						SD_Owner,
						 CAST (
							CAST
									(CAST( SD_BinaryValue AS varbinary(8000)) AS nvarchar(4000))
							AS uniqueidentifier
							) as OrgPK
			   FROM dbo.StmData
			   WHERE SD_Name = 'GMCOrganisation'
			) GMCOrgLink ON GMCOrgLink.SD_Owner = @CompanyPK
			WHERE DEXEvent.Count IS NULL
			AND AH_Ledger = 'AR'
			AND AH_TransactionType = 'INV'
			AND AH_IsCancelled = 0
			AND AH_JH IS NOT NULL
			AND AH_OH = GMCOrgLink.OrgPK
			AND AH_PostDate <=  (CASE WHEN @NoOfDaysInInt = 0
											 THEN GetDate()
										 	 ELSE dbo.GetValidEndDate(DateAdd(day,@NoOfDaysInInt, GetDate()))
									  END
									  )
RETURN
END

";
		#endregion

		#endregion
	}
}

#region ClientOverrideTest
#endregion
