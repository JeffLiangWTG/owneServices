using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class NonAccountingCognosLineGenerator
	{
		public NonAccountingCognosLineGenerator(BusinessObjectFactory factory, ZDateTime exportStartDate)
		{
			this.Factory = factory;
			CognosLineHelper = new CognosLineHelper();
			this.ExportStartDate = exportStartDate;
			CalendarYearStartDate = new ZDateTime(exportStartDate.Year, 1, 1);
		}

		public virtual string GetLinesAsString()
		{
			ZStringBuilder builder = new ZStringBuilder();

			AppendHeadCountLines(builder);
			AppendShipmentCountLines(builder);
			AppendAirFreightKilosLines(builder);
			AppendTEUCountLines(builder);
			AppendTotalInvoiceCountLines(builder);

			return builder.ToStringWithNewLineBetweenAppends();
		}

		#region Head Count

		static class HeadCountConstants
		{
			public const string HeadCountCode = "HCOUNT";
			public const string HeadCountAdminCode = "HCOUNTADM";
			public const string HeadCountSalesCode = "HCOUNTSALES";
			public const string HeadCountShipmentControlCode = "HCOUNTSC";

			public const string HeadCountName = "Headcount";
			public const string HeadCountAdminName = "Headcount Administration";
			public const string HeadCountSalesName = "Headcount Sales";
			public const string HeadCountShipmentControlName = "Headcount Shipment Control";
		}

		internal void AppendHeadCountLines(ZStringBuilder builder)
		{
			AppendTotalHeadCounts(builder);
			AppendHeadCountsByGroup(builder);
		}

		internal void AppendTotalHeadCounts(ZStringBuilder builder)
		{
			foreach (GlbBranch activeBranch in ActiveBranchCollection)
			{
				AppendCognosLine(builder, HeadCountConstants.HeadCountCode, HeadCountConstants.HeadCountName, activeBranch.GB_Code, CognosModes.Empty, GetActiveStaffCount(activeBranch));
			}
			AppendCognosLine(builder, HeadCountConstants.HeadCountCode, HeadCountConstants.HeadCountName, "", CognosModes.Empty, GetActiveStaffCount(null));
		}

		internal void AppendHeadCountsByGroup(ZStringBuilder builder)
		{
			AppendHeadCountsByGroup(builder, HeadCountConstants.HeadCountAdminCode, HeadCountConstants.HeadCountAdminName, JASDataRegistry.Instance.CognosAdminGroup);
			AppendHeadCountsByGroup(builder, HeadCountConstants.HeadCountSalesCode, HeadCountConstants.HeadCountSalesName, JASDataRegistry.Instance.CognosSalesGroup);
			AppendHeadCountsByGroup(builder, HeadCountConstants.HeadCountShipmentControlCode, HeadCountConstants.HeadCountShipmentControlName, JASDataRegistry.Instance.CognosShipmentControlGroup);
		}

		internal void AppendHeadCountsByGroup(ZStringBuilder builder, ZString code, ZString name, ZGuid groupPK)
		{
			if (!groupPK.IsEmpty)
			{
				foreach (GlbBranch activeBranch in ActiveBranchCollection)
				{
					foreach (CognosModes mode in Enum.GetValues(typeof(CognosModes)))
					{
						AppendCognosLine(builder, code, name, activeBranch.GB_Code, mode, GetStaffCountByGroupAndMode(activeBranch, groupPK, mode));
					}
				}
			}
		}

		ZInt GetStaffCountByGroupAndMode(GlbBranch branch, ZGuid groupPK, CognosModes mode)
		{
			GlbGroup group = Factory.Load<GlbGroup>(groupPK);
			ZQuery filter = new ZQuery();
			filter.AddToFilter(GlbStaffSchema.GS_IsActive, true);
			filter.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, false);

			if (branch != null)
			{
				filter.AddToFilter(GlbStaffSchema.GS_GB_HomeBranch, branch.PK);
			}
			else
			{
				filter.AddToFilter(GlbStaffSchema.GS_GB_HomeBranch, null);
			}

			if (mode != CognosModes.Empty)
			{
				ZQuery deptFilter = new ZQuery();
				deptFilter.DefaultJoinCondition = JoinCondition.Or;
				foreach (ZGuid departmentPK in JASDataRegistry.Instance.CognosModeMapping.GetDepartmentPKs(mode))
				{
					deptFilter.AddToFilter(GlbStaffSchema.GS_GE_HomeDepartment, departmentPK);
				}
				filter.AddToFilter(deptFilter);
			}
			else
			{
				filter.AddToFilter(GlbStaffSchema.GS_GE_HomeDepartment, null);
			}

			return group.Staff.Find(filter).Length;
		}

		ZInt GetActiveStaffCount(GlbBranch branch)
		{
			return GetActiveStaffCount(branch, null);
		}

		ZInt GetActiveStaffCount(GlbBranch branch, ZQuery additionalQuery)
		{
			ZQuery filter = new ZQuery(GlbStaffSchema.GS_IsActive, true);
			filter.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, false);
			if (branch != null)
			{
				filter.AddToFilter(GlbStaffSchema.GS_GB_HomeBranch, branch.PK);
			}
			else
			{
				filter.AddToFilter(GlbStaffSchema.GS_GB_HomeBranch, null);
			}

			return Factory.GetDatabaseCount(typeof(GlbStaff), filter);
		}

		#endregion

		#region Shipment Job Count

		static class ShipmentCountConstants
		{
			public const string DirectShipmentCode = "SDIRECT";
			public const string ShipmentCode = "SHOUSES";
			public const string ConsolidationCode = "SMASTERS";

			public const string AirExportName = "Air Export";
			public const string AirImportName = "Air Import";
			public const string CustomsHouseBrokerageName = "Customs House Brokerage";
			public const string WarehousePackingTrucking = "Warehousing/Packing/Trucking";
			public const string MaritimeExportName = "Maritime Export";
			public const string MaritimeImportName = "Maritime Import";
			public const string MaritimeExportNVOCCName = "Maritime Export NVOCC";
		}

		internal void AppendShipmentCountLines(ZStringBuilder builder)
		{
			foreach (GlbBranch branch in ActiveBranchCollection)
			{
				AppendHouseLevelShipmentCountLines(builder, branch);
				AppendMasterLevelShipmentCountLines(builder, branch);
			}
		}

		internal void AppendHouseLevelShipmentCountLines(ZStringBuilder builder, GlbBranch branch)
		{
			ZInt airExportDirectShipments = GetShipmentCount(true, true, Core.Constants.TransportModes.Air, branch, Core.Constants.AgentType.Direct);
			AppendCognosLine(builder, ShipmentCountConstants.DirectShipmentCode, ShipmentCountConstants.AirExportName, branch.GB_Code, CognosModes.AE, airExportDirectShipments);

			ZInt airExportShipments = GetShipmentCount(true, true, Core.Constants.TransportModes.Air, branch, NormalShipmentAgentTypes);
			AppendCognosLine(builder, ShipmentCountConstants.ShipmentCode, ShipmentCountConstants.AirExportName, branch.GB_Code, CognosModes.AE, airExportShipments);

			ZInt airImportShipments = GetShipmentCount(true, false, Core.Constants.TransportModes.Air, branch, NormalShipmentAgentTypes);
			AppendCognosLine(builder, ShipmentCountConstants.ShipmentCode, ShipmentCountConstants.AirImportName, branch.GB_Code, CognosModes.AI, airImportShipments);

			ZInt seaExportShipments = GetShipmentCount(true, true, Core.Constants.TransportModes.Sea, branch, NormalShipmentAgentTypes);
			AppendCognosLine(builder, ShipmentCountConstants.ShipmentCode, ShipmentCountConstants.MaritimeExportName, branch.GB_Code, CognosModes.ME, seaExportShipments);

			ZInt seaImportShipments = GetShipmentCount(true, false, Core.Constants.TransportModes.Sea, branch, NormalShipmentAgentTypes);
			AppendCognosLine(builder, ShipmentCountConstants.ShipmentCode, ShipmentCountConstants.MaritimeImportName, branch.GB_Code, CognosModes.MI, seaImportShipments);

			ZInt seaExportNVOCCShipments = GetShipmentCount(true, true, Core.Constants.TransportModes.Sea, branch, Core.Constants.AgentType.CoLoad);
			AppendCognosLine(builder, ShipmentCountConstants.ShipmentCode, ShipmentCountConstants.MaritimeExportNVOCCName, branch.GB_Code, CognosModes.NV, seaExportNVOCCShipments);

			AppendCustomsBrokerageShipmentJobCount(builder, branch);

			AppendWarehousePackingTruckingJobCount(builder, branch);
		}

		void AppendMasterLevelShipmentCountLines(ZStringBuilder builder, GlbBranch branch)
		{
			ZInt airExportShipments = GetShipmentCount(false, true, Core.Constants.TransportModes.Air, branch, NormalShipmentAgentTypes);
			AppendCognosLine(builder, ShipmentCountConstants.ConsolidationCode, ShipmentCountConstants.AirExportName, branch.GB_Code, CognosModes.AE, airExportShipments);

			ZInt airImportShipments = GetShipmentCount(false, false, Core.Constants.TransportModes.Air, branch, NormalShipmentAgentTypes);
			AppendCognosLine(builder, ShipmentCountConstants.ConsolidationCode, ShipmentCountConstants.AirImportName, branch.GB_Code, CognosModes.AI, airImportShipments);

			ZInt seaExportShipments = GetShipmentCount(false, true, Core.Constants.TransportModes.Sea, branch, NormalShipmentAgentTypes);
			AppendCognosLine(builder, ShipmentCountConstants.ConsolidationCode, ShipmentCountConstants.MaritimeExportName, branch.GB_Code, CognosModes.ME, seaExportShipments);

			ZInt seaImportShipments = GetShipmentCount(false, false, Core.Constants.TransportModes.Sea, branch, NormalShipmentAgentTypes);
			AppendCognosLine(builder, ShipmentCountConstants.ConsolidationCode, ShipmentCountConstants.MaritimeImportName, branch.GB_Code, CognosModes.MI, seaImportShipments);

			ZInt seaExportNVOCCShipments = GetShipmentCount(false, true, Core.Constants.TransportModes.Sea, branch, Core.Constants.AgentType.CoLoad);
			AppendCognosLine(builder, ShipmentCountConstants.ConsolidationCode, ShipmentCountConstants.MaritimeExportNVOCCName, branch.GB_Code, CognosModes.NV, seaExportNVOCCShipments);
		}

		void AppendCustomsBrokerageShipmentJobCount(ZStringBuilder builder, GlbBranch branch)
		{
			ZQuery filter = new ZQuery(JobDeclarationSchema.JE_JS, SQLComparisonOperator.NotEqual, null);
			filter.AddToFilter(JobDeclarationSchema.JE_GB, branch.PK);
			filter.AddToFilter(JobDeclarationSchema.JE_IsCancelled, false);
			filter.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, CalendarYearStartDate);
			filter.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ExportStartDate);

			ZInt customsBrokerageCount = Factory.GetDatabaseCount(typeof(BaseJobDeclaration), filter);
			AppendCognosLine(builder, ShipmentCountConstants.ShipmentCode, ShipmentCountConstants.CustomsHouseBrokerageName, branch.GB_Code, CognosModes.CHB, customsBrokerageCount);
		}

		void AppendWarehousePackingTruckingJobCount(ZStringBuilder builder, GlbBranch branch)
		{
			ZSqlParameterCollection paramCollection = new ZSqlParameterCollection();
			paramCollection.Add("@ReceiveDocketType", DocketType.Codes.Receive, WhsDocketSchema.WD_DocketType);
			paramCollection.Add("@OrderDocketType", DocketType.Codes.Order, WhsDocketSchema.WD_DocketType);
			paramCollection.Add("@BranchPK", branch.PK, WhsWarehouseSchema.WW_GB_RelatedCompanyBranch);

			ZQuery filter = new ZQuery();
			filter.AddFilterAndZSQLParameterCollection(@"
WD_PK IN
(
    SELECT  WD_PK
    FROM    dbo.WhsDocket
            INNER JOIN dbo.WhsWarehouse ON WD_WW_Whs = WW_PK
    WHERE	WD_DocketType IN (@ReceiveDocketType, @OrderDocketType)
            AND WW_GB_RelatedCompanyBranch = @BranchPK
)
", paramCollection);

			ZInt wPTCount = Factory.GetDatabaseCount(typeof(WhsDocket), filter);
			AppendCognosLine(builder, ShipmentCountConstants.ShipmentCode, ShipmentCountConstants.WarehousePackingTrucking, branch.GB_Code, CognosModes.WPT, wPTCount);
		}

		int GetShipmentCount(bool isHouseLevel, bool isExport, ZString transportMode, GlbBranch branch, params string[] agentTypes)
		{
			Type typeOfObject = (isHouseLevel) ? typeof(JASForwardingShipment) : typeof(JASForwardingConsol);
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeOfObject);
			AddFilterForShipmentCount(filter, isHouseLevel, isExport, transportMode, branch, agentTypes);
			return Factory.GetDatabaseCount(typeOfObject, filter);
		}

		void AddFilterForShipmentCount(ZDBOnlyQuery filter, bool isHouseLevel, bool isExport, ZString transportMode, GlbBranch branch, string[] agentTypes)
		{
			string exportImportFilterString;
			string agentTypeFilterString;

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@TransportMode", transportMode, JobConsolSchema.JK_TransportMode);
			@params.Add("@BranchPK", branch.PK, GlbBranchSchema.PK);
			@params.Add("@CalendarYearStartDate", CalendarYearStartDate, JobConsolSchema.JK_SystemCreateTimeUtc);
			@params.Add("@ExportStartDate", ExportStartDate, JobConsolSchema.JK_SystemCreateTimeUtc);

			ConstructExportImportFilterString(out exportImportFilterString, @params, isExport);
			ConstructAgentTypeFilterString(out agentTypeFilterString, @params, agentTypes);

			string selectClause;
			if (isHouseLevel)
			{
				selectClause =
						@"
                    JS_PK IN
                    (
                        SELECT  JS_PK";
			}
			else
			{
				selectClause =
						@"
                    JK_PK IN
                    (
                        SELECT  JK_PK";
			}

			filter.AddFilterAndZSQLParameterCollection(
							selectClause + @"                                              
                        FROM    dbo.JobShipment
                                INNER JOIN dbo.JobConShipLink ON JS_PK = JN_JS
                                INNER JOIN dbo.JobConsol ON JK_PK = JN_JK
                                INNER JOIN dbo.JobHeader ON JH_ParentID = JS_PK
                        WHERE   JH_GB = @BranchPK
                                AND JK_IsForwarding = 1
                                AND JK_IsCancelled = 0                                
                                AND JK_TransportMode = @TransportMode 
                                AND JK_SystemCreateTimeUtc >= @CalendarYearStartDate
                                AND JK_SystemCreateTimeUtc <= @ExportStartDate" + "\r\n" +
													exportImportFilterString + "\r\n" +
													agentTypeFilterString + @"
                    )
                    ", @params);
		}

		void ConstructExportImportFilterString(out string exportImportFilterString, ZSqlParameterCollection @params, bool isExport)
		{
			exportImportFilterString = GetExportImportFilterStringForConsol(isExport);
			@params.Add("@CountryCode", CurrentCompanyCountryCode, RefCountrySchema.RN_Code);
		}

		void ConstructAgentTypeFilterString(out string agentTypeFilterString, ZSqlParameterCollection @params, string[] agentTypes)
		{
			StringBuilder builder = new StringBuilder("AND JK_AgentType IN (");
			for (int i = 0; i < agentTypes.Length; i++)
			{
				char lastChar = builder[builder.Length - 1];
				string paramName = "@AgentType" + i.ToString();
				if (lastChar != '(')
				{
					builder.Append(", ");
				}
				builder.Append(paramName);
				@params.Add(paramName, agentTypes[i], JobConsolSchema.JK_AgentType);
			}
			builder.Append(')');

			agentTypeFilterString = builder.ToString();
		}

		ZString CurrentCompanyCountryCode
		{
			get { return GlbCompany.CurrentCompany.Country != null ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : ZString.Empty; }
		}

		#endregion

		#region Air Freight Kilos

		const string AirFreightKilosName = "Airfreight Kilos";
		const string AirFreightKilosCode = "SKGS";

		internal void AppendAirFreightKilosLines(ZStringBuilder builder)
		{
			foreach (GlbBranch branch in ActiveBranchCollection)
			{
				AppendAirFreightKilosLines(builder, branch, CognosModes.AE, true);
				AppendAirFreightKilosLines(builder, branch, CognosModes.AI, false);
			}
		}

		void AppendAirFreightKilosLines(ZStringBuilder builder, GlbBranch branch, CognosModes mode, bool isExport)
		{
			DbCommand command = GetDbCommand(@"
SELECT  SUM(ConvertedActualWeight_KG.Value)
FROM    dbo.JobShipment
        INNER JOIN dbo.JobHeader ON JH_ParentID = JS_PK
        CROSS APPLY dbo.ConvertWeight(JS_ActualWeight, JS_UnitOfWeight, 'KG') AS ConvertedActualWeight_KG
WHERE   JH_GB = @BranchPK
        AND JS_TransportMode IN ('AIR', 'FSA', 'FAS')
        AND JS_IsCancelled = 0
        AND JS_IsForwardRegistered = 1        
        AND JS_SystemCreateTimeUtc >= @CalendarYearStartDate
        AND JS_SystemCreateTimeUtc <= @ExportStartDate" + "\r\n" +
	GetExportImportFilterStringForShipment(isExport));

			command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branch.PK.ToGuid());
			command.AddParameter("@CalendarYearStartDate", SqlDbType.SmallDateTime, CalendarYearStartDate.ToDateTime());
			command.AddParameter("@ExportStartDate", SqlDbType.SmallDateTime, ExportStartDate.ToDateTime());
			command.AddParameter("@CountryCode", SqlDbType.Char, CurrentCompanyCountryCode.ToString());

			AppendCognosLine(builder, AirFreightKilosCode, AirFreightKilosName, branch.GB_Code, mode, ExecuteScalarReturningDecimal(command));
		}

		#endregion

		#region TEUs

		public const string TEUCountCode = "STEU";
		public const string TEUCountName = "Oceanfreight TEUs";

		internal void AppendTEUCountLines(ZStringBuilder builder)
		{
			foreach (GlbBranch branch in ActiveBranchCollection)
			{
				AppendTEUCountLines(builder, branch, CognosModes.ME, true, NormalShipmentAgentTypes);
				AppendTEUCountLines(builder, branch, CognosModes.MI, false, NormalShipmentAgentTypes);
				AppendTEUCountLines(builder, branch, CognosModes.NV, true, Core.Constants.AgentType.CoLoad);
			}
		}

		void AppendTEUCountLines(ZStringBuilder builder, GlbBranch branch, CognosModes mode, bool isExport, params string[] agentTypes)
		{
			string agentTypesFilter = "(";
			foreach (string agentType in agentTypes)
			{
				if (!agentTypesFilter.EndsWith("("))
				{
					agentTypesFilter += ", ";
				}
				agentTypesFilter += string.Format("'{0}'", agentType);
			}
			agentTypesFilter += ")";

			DbCommand command = GetDbCommand(@"
SELECT      SUM(RC_TEU * JC_ContainerCount)
FROM		dbo.JobContainer
			INNER JOIN dbo.RefContainer ON JC_RC = RC_PK
WHERE		JC_JK IN (  SELECT	JK_PK							
					    FROM	dbo.JobConsol
								INNER JOIN dbo.JobConShipLink ON JK_PK = JN_JK
								INNER JOIN dbo.JobHeader ON JH_ParentID = JN_JS
						WHERE	JH_GB = @BranchPK
								AND JK_IsForwarding = 1
								AND JK_IsCancelled = 0
								AND JK_TransportMode = 'SEA'
                                AND JK_SystemCreateTimeUtc >= @CalendarYearStartDate
                                AND JK_SystemCreateTimeUtc <= @ExportStartDate" + "\r\n" +
													GetExportImportFilterStringForConsol(isExport) + @"
                                AND JK_AgentType IN " + agentTypesFilter + ")");

			command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branch.PK.ToGuid());
			command.AddParameter("@CalendarYearStartDate", SqlDbType.SmallDateTime, CalendarYearStartDate.ToDateTime());
			command.AddParameter("@ExportStartDate", SqlDbType.SmallDateTime, ExportStartDate.ToDateTime());
			command.AddParameter("@CountryCode", SqlDbType.Char, CurrentCompanyCountryCode.ToString());

			AppendCognosLine(builder, TEUCountCode, TEUCountName, branch.GB_Code, mode, ExecuteScalarReturningDecimal(command));
		}

		#endregion

		#region Total Invoiced

		const string TotalInvoicedCode = "TI";
		const string TotalInvoicedName = "Total Invoiced";

		internal void AppendTotalInvoiceCountLines(ZStringBuilder builder)
		{
			DbCommand command = GetDbCommand(@"
SELECT  SUM(AH_InvoiceAmount + AH_GSTAmount) AS TotalInvoiced
FROM    dbo.AccTransactionHeader
WHERE   AH_Ledger = 'AR'
        AND AH_IsCancelled = 0
        AND (AH_TransactionType = 'INV' OR (AH_TransactionType = 'ADJ' AND AH_InvoiceAmount > 0)) 
        AND AH_PostDate >= @CalendarYearStartDate
        AND AH_PostDate <= @ExportStartDate
        AND AH_GB = @BranchPK");

			command.AddParameter("@CalendarYearStartDate", SqlDbType.SmallDateTime, CalendarYearStartDate.ToDateTime());
			command.AddParameter("@ExportStartDate", SqlDbType.SmallDateTime, ExportStartDate.ToDateTime());

			foreach (GlbBranch branch in ActiveBranchCollection)
			{
				command.RemoveParameterIfExists("@BranchPK");
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branch.PK.ToGuid());
				AppendCognosLine(builder, TotalInvoicedCode, TotalInvoicedName, branch.GB_Code, CognosModes.Empty, ExecuteScalarReturningDecimal(command));
			}
		}

		#endregion

		#region Implementation

		internal string[] NormalShipmentAgentTypes
		{
			get
			{
				return new string[]
					{
						Core.Constants.AgentType.Agent,
						Core.Constants.AgentType.Charter,
						Core.Constants.AgentType.Other
					};
			}
		}

		IList<string> CodesRequiringICTOTAAsCounterCompany
		{
			get
			{
				return new string[]
					{
						HeadCountConstants.HeadCountCode,
						ShipmentCountConstants.DirectShipmentCode,
						ShipmentCountConstants.ShipmentCode,
						ShipmentCountConstants.ConsolidationCode,
						AirFreightKilosCode,
						TEUCountCode,
						TotalInvoicedCode
					};
			}
		}

		CognosLine CreateCognosLine(ZString code, ZString name, ZString branch, CognosModes mode, ZDecimal amount)
		{
			string modeAsString = (mode != CognosModes.Empty) ? mode.ToString() : "";
			CognosLine result = new CognosLine();
			result.CounterCompany = (CodesRequiringICTOTAAsCounterCompany.Contains(code)) ? "ICTOTA" : "";
			result.AccountCode = code;
			result.AccountName = name;
			result.Branch = branch;
			result.Mode = modeAsString;
			result.Amount = amount;
			return result;
		}

		string GetExportImportFilterStringForConsol(bool isExport)
		{
			return (isExport)
						 ? "AND LEFT(JK_RL_NKLoadPort, 2) = @CountryCode AND LEFT(JK_RL_NKDischargePort, 2) != @CountryCode"
						 : "AND LEFT(JK_RL_NKDischargePort, 2) = @CountryCode AND LEFT(JK_RL_NKLoadPort, 2) != @CountryCode";
		}

		string GetExportImportFilterStringForShipment(bool isExport)
		{
			return (isExport)
						 ? "AND LEFT(JS_RL_NKOrigin, 2) = @CountryCode AND LEFT(JS_RL_NKDestination, 2) != @CountryCode"
						 : "AND LEFT(JS_RL_NKDestination, 2) = @CountryCode AND LEFT(JS_RL_NKOrigin, 2) != @CountryCode";
		}

		internal void AppendCognosLine(ZStringBuilder builder, ZString code, ZString name, ZString branch, CognosModes mode, ZInt amount)
		{
			AppendCognosLine(builder, code, name, branch, mode, (ZDecimal)amount);
		}

		internal void AppendCognosLine(ZStringBuilder builder, ZString code, ZString name, ZString branch, CognosModes mode, ZDecimal amount)
		{
			CognosLine line = CreateCognosLine(code, name, branch, mode, amount);
			builder.Append(CognosLineHelper.ConvertToString(line));
		}

		internal DbCommand GetDbCommand(string commandText)
		{
			return Db.Connection.Command(commandText, 600);
		}

		ZDecimal ExecuteScalarReturningDecimal(DbCommand command)
		{
			object result = command.ExecuteScalar();
			return (result == DBNull.Value) ? ZDecimal.Zero : new ZDecimal(result);
		}

		internal GlbBranchCollection ActiveBranchCollection
		{
			get
			{
				if (fActiveBranchCollection == null)
				{
					fActiveBranchCollection = new GlbBranchCollection(Factory);
					ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					filter.AddToFilter(GlbBranchSchema.GB_IsActive, true);
					fActiveBranchCollection.Load(filter);
				}
				return fActiveBranchCollection;
			}
		}

		GlbBranchCollection fActiveBranchCollection;

		internal readonly CognosLineHelper CognosLineHelper;
		internal readonly BusinessObjectFactory Factory;
		internal readonly ZDateTime CalendarYearStartDate;
		internal readonly ZDateTime ExportStartDate;

#endregion
	}
}
