using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using BorderWise.Sync;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.BorderWise.Subscribers
{
	public class OrgMergeMessageHelper
	{
		public bool IsMergeTransactionForAddressChange(DataRow addressChangeRow)
		{
			return IsMergeTransactionForChangeRow(addressChangeRow, OrgAddressSchema.Constants.OA_OH, OrgAddressSchema.Constants.OA_IsActive);
		}

		public bool IsMergeTransactionForContactChange(DataRow contactChangeRow)
		{
			return IsMergeTransactionForChangeRow(contactChangeRow, OrgContactSchema.Constants.OC_OH, OrgContactSchema.Constants.OC_IsActive);
		}

		bool IsMergeTransactionForChangeRow(DataRow changeRow, string orgReferenceColumnName, string isActiveColumnName)
		{
			var changeType = changeRow.RowState.ToChangeType();
			var isDelete = changeType == ChangeType.Delete;
			var isUpdateWithOrgChangeOrDeactivate = changeType == ChangeType.Update &&
				(
					changeRow[orgReferenceColumnName, DataRowVersion.Original] != changeRow[orgReferenceColumnName, DataRowVersion.Current] ||
					RowWasDeactivated(changeRow, isActiveColumnName)
				);

			if (isDelete || isUpdateWithOrgChangeOrDeactivate)
			{
				var orgPK = new ZGuid(changeRow[orgReferenceColumnName, DataRowVersion.Original]);
				if (orgPK.IsValid)
				{
					return IsMergeTransaction((byte[])changeRow[AuditFieldNames.StartLsnFieldName, DataRowVersion.Original], orgPK.ToGuid(), false, out _, out _);
				}
			}

			return false;
		}

		static bool RowWasDeactivated(DataRow changeRow, string isActiveColumnName)
		{
			var changeType = changeRow.RowState.ToChangeType();
			var isUpdateAndDeactivate = changeType == ChangeType.Update &&
				changeRow[isActiveColumnName, DataRowVersion.Current] is bool isActive &&
				!isActive &&
				changeRow[isActiveColumnName, DataRowVersion.Original] is bool wasActive &&
				wasActive;

			return isUpdateAndDeactivate;
		}

		[SuppressMessage("Microsoft.Design", "CA1021")]
		public bool IsMergeTransactionForOrgChange(DataRow orgChangeRow, out Guid targetOrgPk, out OrgHeader targetOrg)
		{
			var changeType = orgChangeRow.RowState.ToChangeType();
			var isDelete = changeType == ChangeType.Delete;
			var isUpdateAndDeactivate = changeType == ChangeType.Update &&
				RowWasDeactivated(orgChangeRow, OrgHeaderSchema.Constants.OH_IsActive);

			if (isDelete || isUpdateAndDeactivate)
			{
				var rowVersion = isDelete ? DataRowVersion.Original : DataRowVersion.Current;
				var pk = (Guid)orgChangeRow[OrgHeaderSchema.Constants.PK, rowVersion];
				return IsMergeTransaction((byte[])orgChangeRow[AuditFieldNames.StartLsnFieldName, DataRowVersion.Original], pk, true, out targetOrgPk, out targetOrg);
			}

			targetOrgPk = Guid.Empty;
			targetOrg = null;
			return false;
		}

		[SuppressMessage("Microsoft.Design", "CA1021")]
		public bool IsMergeTransaction(byte[] transactionLsn, Guid orgPk, bool needTargetOrg, out Guid targetOrgPk, out OrgHeader targetOrg)
		{
			targetOrgPk = Guid.Empty;
			targetOrg = null;

			if (!HasPotentialOrgHeaderChange(transactionLsn, orgPk))
			{
				return false;
			}

			var dataFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var mergeLogQuery = new ZQuery(StmALogSchema.SL_Parent, orgPk);
			mergeLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, OrganisationMerger.MergedLogEvent.Code);

			var transactionEndTime = GetTransactionEndTime(((IDbConnected)dataFactory).Connection, transactionLsn);
			if (transactionEndTime.IsValid)
			{
				mergeLogQuery.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, transactionEndTime);
			}

			mergeLogQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + " DESC";

			var mergeLog = dataFactory.LoadTop1<StmALog>(mergeLogQuery);
			if (mergeLog != null)
			{
				var delimiterIndex = mergeLog.SL_Reference.LastIndexOf('|');
				if (delimiterIndex >= 0)
				{
					var targetOrgId = mergeLog.SL_Reference.SubstringSafe(delimiterIndex + 1).Trim();
					if (targetOrgId.Length > 0 && Guid.TryParse(targetOrgId, out targetOrgPk))
					{
						if (needTargetOrg)
						{
							targetOrg = dataFactory.Load<OrgHeader>(targetOrgPk);
						}
						return true;
					}
				}
			}

			return false;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System user code")]
		protected virtual bool HasPotentialOrgHeaderChange(byte[] transactionLsn, Guid orgPk)
		{
			const string OrgMergerUserCode = "~OM";

			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
			using (var auditConnection = Db.NewExtraConnectionWithMainDbCredentials(auditServer, Db.AuditDatabaseName))
			{
				string checkOrgChangeQuery = $@"
SELECT COUNT(1)
  FROM dbo.OrgHeader
 WHERE {AuditFieldNames.StartLsnFieldName} = @TransactionLsn
   AND {OrgHeaderSchema.Constants.PK} = @OrgPk
   AND {AuditFieldNames.OperationFieldName} = 4
   AND {OrgHeaderSchema.Constants.OH_SystemLastEditUser} = '{OrgMergerUserCode}'";

				using (var command = auditConnection.Command(checkOrgChangeQuery))
				{
					command.AddParameter("@TransactionLsn", SqlDbType.Binary, 10, transactionLsn);
					command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, orgPk);

					var orgChangeCountObject = command.ExecuteScalar();
					if (orgChangeCountObject is int orgChangeCount && orgChangeCount > 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected virtual ZDateTime GetTransactionEndTime(DbConnection connection, byte[] transactionLsn)
		{
			const string TransactionTimeQuery = @"
SELECT TOP 1 tran_end_time
  FROM cdc.lsn_time_mapping
 WHERE start_lsn = @TransactionLsn";
			using (var command = connection.Command(TransactionTimeQuery))
			{
				command.AddParameter("@TransactionLsn", SqlDbType.Binary, 10, transactionLsn);
				return new ZDateTime(command.ExecuteScalar());
			}
		}
	}
}
