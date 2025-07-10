using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.Asycuda;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class ShipmentDataSelector
	{
		public ShipmentDataSelector(BusinessObjectFactory factory, ZDateTime startDate, ZDateTime endDate)
		{
			this.Factory = factory;
			this.StartDate = startDate;
			this.EndDate = endDate;
		}

		public IShipmentData[] GetShipmentDataToBeExported()
		{
			var countryCode = Env.CurrentCompany.Country.Code;
			var result = new List<IShipmentData>();
			if (countryCode == CountryCodes.Australia)
			{
				result.AddRange((IShipmentData[])Factory.Load(typeof(UPECusHAWB), Query));
			}
			else if (countryCode == CountryCodes.Singapore)
			{
				var bills = Factory.Load(typeof(AsycudaBill), SGQuery);
				foreach (var bill in bills)
				{
					var asycudaProxy = new AsycudaIShipmentDataProxy((AsycudaBill)bill);
					result.Add(asycudaProxy);
				}

				var logs = new DynamicBusinessObjectCollection(Factory);
				logs.Load(FilterStringTradeNet, ParameterCollection);
				foreach (DynamicBusinessObject log in logs)
				{
					var declaration = Factory.Load<JobDeclaration>(new ZGuid(log[StmALogSchema.Constants.SL_Parent]));
					if (declaration != null)
					{
						var tradeNet = new TradeNetIShipmentDataProxy(new ZString(log[StmALogSchema.Constants.SL_Reference]), new ZDateTime(log[StmALogSchema.Constants.SL_PostedTimeUtc]), declaration);
						result.Add(tradeNet);
					}
				}
			}
			return result.ToArray();
		}

		#region Implementation

		ZDBOnlyQuery Query
		{
			get
			{
				if (fQuery == null)
				{
					fQuery = new ZDBOnlyQuery(typeof(UPECusHAWB));

					var branches = UPETools.Instance.UPECustomisationBranches(true).ToArray();
					ZDBOnlySubQuery mawbSubQuery = new ZDBOnlySubQuery(typeof(UPECusMAWB), CusMAWBSchema.PK);
					mawbSubQuery.FilterByForeignKey(CusMAWBSchema.CM_GB, branches);

					ZDBOnlySubQuery cusDecSubQuery = new ZDBOnlySubQuery(typeof(UPEJobDeclaration), JobDeclarationSchema.PK);
					cusDecSubQuery.FilterByForeignKey(JobDeclarationSchema.JE_GB, branches);

					fQuery.AddSubQuery(CusHAWBSchema.CS_CM, mawbSubQuery, JoinCondition.And);
					fQuery.AddSubQuery(CusHAWBSchema.CS_JE_CustomsFormalEntry, cusDecSubQuery, JoinCondition.Or);

					fQuery.AddFilterAndZSQLParameterCollection(FilterString, ParameterCollection);
				}
				return fQuery;
			}
		}
		ZDBOnlyQuery fQuery;

		ZDBOnlyQuery SGQuery
		{
			get
			{
				if (fSGQuery == null)
				{
					fSGQuery = new ZDBOnlyQuery(typeof(AsycudaBill));
					fSGQuery.AddFilterAndZSQLParameterCollection(FilterStringSG, ParameterCollection);
				}
				return fSGQuery;
			}
		}
		ZDBOnlyQuery fSGQuery;

		ZSqlParameterCollection ParameterCollection
		{
			get
			{
				if (fParameterCollection == null)
				{
					fParameterCollection = new ZSqlParameterCollection
						(
						ZSqlParameter.New("@LowerBound", Env.Time.GetUtcFromLocalTime(StartDate.ToDateTime()), StmALogSchema.SL_PostedTimeUtc),
						ZSqlParameter.New("@UpperBound", Env.Time.GetUtcFromLocalTime(EndDate.ToDateTime()), StmALogSchema.SL_PostedTimeUtc)
						);
				}
				return fParameterCollection;
			}
		}
		ZSqlParameterCollection fParameterCollection;

		protected ZString FilterString =>
			 FormattableString.Invariant($@"{CusHAWBSchema.Constants.PK} IN (
				SELECT {CusHAWBSchema.Constants.PK}
				FROM {CusHAWBSchema.Constants.SqlSchemaName}.{CusHAWBSchema.Constants.TableName}
				INNER JOIN {ProcessQueueSchema.Constants.SqlSchemaName}.{ProcessQueueSchema.Constants.TableName} ON {ProcessQueueSchema.Constants.P4_ParentID} = {CusHAWBSchema.Constants.PK}
				INNER JOIN {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName} ON {ProcessQueueSchema.Constants.PK} = {StmALogSchema.Constants.SL_Parent}
				WHERE {StmALogSchema.Constants.SL_Reference} LIKE '{Queue_CompletedReference}%'
				AND {StmALogSchema.Constants.SL_PostedTimeUtc} >= @LowerBound
				AND {StmALogSchema.Constants.SL_PostedTimeUtc} < @UpperBound
				AND {CusHAWBSchema.Constants.CS_JE_CustomsFormalEntry} IS NULL
				UNION ALL
				SELECT {CusHAWBSchema.Constants.PK}
				FROM {CusHAWBSchema.Constants.SqlSchemaName}.{CusHAWBSchema.Constants.TableName}
				INNER JOIN {ProcessQueueSchema.Constants.SqlSchemaName}.{ProcessQueueSchema.Constants.TableName} ON {ProcessQueueSchema.Constants.P4_ParentID} = {CusHAWBSchema.Constants.CS_JE_CustomsFormalEntry}
				INNER JOIN {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName} ON {ProcessQueueSchema.Constants.PK} = {StmALogSchema.Constants.SL_Parent}
				WHERE ({StmALogSchema.Constants.SL_Reference} LIKE '{Queue_CompletedReference}%' OR {StmALogSchema.Constants.SL_Reference} LIKE '{DeclarationQueue_CustomsBondingReference}%')
				AND {StmALogSchema.Constants.SL_PostedTimeUtc} >= @LowerBound
				AND {StmALogSchema.Constants.SL_PostedTimeUtc} < @UpperBound)");

		ZString FilterStringSG =>
			FormattableString.Invariant($@"{AsycudaBillSchema.Constants.PK} IN (
				SELECT {AsycudaBillSchema.Constants.PK}
				FROM  {AsycudaBillSchema.Constants.SqlSchemaName}.{AsycudaBillSchema.Constants.TableName}
				INNER JOIN {AsycudaPackSchema.Constants.SqlSchemaName}.{AsycudaPackSchema.Constants.TableName} ON {AsycudaPackSchema.Constants.APA_ABL_Bill} = {AsycudaBillSchema.Constants.PK}
				INNER JOIN {AsycudaPackPackedItemPivotSchema.Constants.SqlSchemaName}.{AsycudaPackPackedItemPivotSchema.Constants.TableName} ON {AsycudaPackPackedItemPivotSchema.Constants.APP_APA_Pack} = {AsycudaPackSchema.Constants.PK}
				INNER JOIN {AsycudaPackedItemSchema.Constants.SqlSchemaName}.{AsycudaPackedItemSchema.Constants.TableName} ON {AsycudaPackPackedItemPivotSchema.Constants.APP_API_Item} = {AsycudaPackedItemSchema.Constants.PK}
				INNER JOIN {AsycudaManifestHeaderSchema.Constants.SqlSchemaName}.{AsycudaManifestHeaderSchema.Constants.TableName} ON {AsycudaManifestHeaderSchema.Constants.PK}  = {AsycudaBillSchema.Constants.ABL_AMA} AND {AsycudaManifestHeaderSchema.Constants.AMA_ManifestType}  <> '{Level1DataFileImporterForSGAccess.Constants.ManifestType.Export}'
				INNER JOIN {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName} SL1 ON {AsycudaBillSchema.Constants.PK} = SL1.{StmALogSchema.Constants.SL_Parent}
				LEFT JOIN {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName} SL2 ON {AsycudaBillSchema.Constants.PK} = SL2.{StmALogSchema.Constants.SL_Parent} AND SL2.{StmALogSchema.Constants.SL_Table} = '{AsycudaBillSchema.Constants.TableName}' AND SL2.{StmALogSchema.Constants.SL_SE_NKEvent} = '{Events.DataExport}' AND SL2.{StmALogSchema.Constants.SL_Reference} LIKE '%{IShipmentDataExtension.BISIReference}%' AND SL2.{StmALogSchema.Constants.SL_IsCancelled} = 'N'
				WHERE SL1.{StmALogSchema.Constants.SL_Table} = '{AsycudaBillSchema.Constants.TableName}'
				AND SL1.{StmALogSchema.Constants.SL_SE_NKEvent} = '{Events.StatusChange}'
				AND SL1.{StmALogSchema.Constants.SL_Reference} IN ('CR', 'IP')
				AND SL1.{StmALogSchema.Constants.SL_PostedTimeUtc} >= @LowerBound
				AND SL1.{StmALogSchema.Constants.SL_PostedTimeUtc} < @UpperBound
				AND SL1.{StmALogSchema.Constants.SL_IsCancelled} = 'N'
				AND SL2.{StmALogSchema.Constants.PK} IS NULL)");

		ZString FilterStringTradeNet =>
			FormattableString.Invariant($@"
				SELECT DISTINCT *
				FROM (
					SELECT SL1.*
					FROM {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName} SL1
					INNER JOIN {JobDeclarationSchema.Constants.SqlSchemaName}.{JobDeclarationSchema.Constants.TableName} ON SL1.{StmALogSchema.Constants.SL_Parent} = {JobDeclarationSchema.Constants.PK}
					LEFT JOIN {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName} SL2 ON {JobDeclarationSchema.Constants.PK} = SL2.{StmALogSchema.Constants.SL_Parent} AND SL2.{StmALogSchema.Constants.SL_SE_NKEvent} = '{Events.DataExport}' AND SL2.{StmALogSchema.Constants.SL_Reference} = '{IShipmentDataExtension.BISIReference}' AND SL2.{StmALogSchema.Constants.SL_IsCancelled} = 'N'
					WHERE SL1.{StmALogSchema.Constants.SL_Reference} LIKE '%{IShipmentDataExtension.BISIReference}%'
					AND SL1.{StmALogSchema.Constants.SL_PostedTimeUtc}  >= @LowerBound
					AND SL1.{StmALogSchema.Constants.SL_PostedTimeUtc} < @UpperBound
					AND SL1.{StmALogSchema.Constants.SL_SE_NKEvent} = '{Events.DeclarationQueued}'
					AND SL1.{StmALogSchema.Constants.SL_IsCancelled} = 'N'
					AND SL2.{StmALogSchema.Constants.PK} IS NULL
					AND {JobDeclarationSchema.Constants.JE_MessageType} IN ('{MessageTypeCodeList.Codes.INP}','{MessageTypeCodeList.Codes.IPT}','{MessageTypeCodeList.Codes.TNP}')
				) IQ"
			);

		string Queue_CompletedReference
		{
			get { return ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, DefaultQueueCodeDescriptionPairList.Codes.Completed); }
		}

		string DeclarationQueue_CustomsBondingReference
		{
			get { return ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding); }
		}

		readonly BusinessObjectFactory Factory;
		readonly ZDateTime StartDate;
		readonly ZDateTime EndDate;

		#endregion
	}
}
