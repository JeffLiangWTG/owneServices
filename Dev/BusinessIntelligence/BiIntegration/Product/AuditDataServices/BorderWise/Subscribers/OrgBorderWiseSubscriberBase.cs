using System;
using System.Data;
using BorderWise.Sync;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.BorderWise.Subscribers
{
	public abstract class OrgBorderWiseSubscriberBase<T> : BorderWiseSubscriberBase
		where T : class, IDataObjectWithBorderWisePK
	{
		protected abstract string PkMapRecordName { get; }

		protected abstract string PKColumnName { get; }

		protected override void PublishChange(ILogger logger, DataRow changeRow, IBorderWiseChangesPublisher publisher)
		{
			var changeType = GetChangeType(logger, changeRow);
			if (changeType == ChangeType.None)
			{
				return;
			}

			var rowVersion = changeType == ChangeType.Delete ? DataRowVersion.Original : DataRowVersion.Current;

			var orgChangeData = new OrgChangeDataObject<T>();
			orgChangeData.Source = MessageSource;
			orgChangeData.ChangeType = changeType;
			orgChangeData.ChangeSequence = GetChangeSequence(changeRow, rowVersion);

			var key = MessageKey;
			if (changeType == ChangeType.Delete || changeType == ChangeType.Update)
			{
				orgChangeData.OriginalVersion = GetDataObject(changeRow, DataRowVersion.Original, changeType);
			}
			if (changeType == ChangeType.Add || changeType == ChangeType.Update || changeType == ChangeType.Acknowledgement)
			{
				orgChangeData.CurrentVersion = GetDataObject(changeRow, DataRowVersion.Current, changeType);
			}

			var ediProdPK = new ZGuid();

			if (changeType == ChangeType.Acknowledgement)
			{
				ediProdPK = new ZGuid(changeRow[PKColumnName, DataRowVersion.Current]);
				var borderWisePk = GetMappedBorderWisePK(PkMapRecordName, ediProdPK);
				if (borderWisePk.IsValid)
				{
					orgChangeData.CurrentVersion.BorderWisePK = borderWisePk.ToGuid();
				}
			}

			var message = SerializeObject(orgChangeData);
			publisher.Publish(key.ToStringSafe().Trim(), message);
			logger.Information($"BorderWise Subscriber Published Message: {message}"); // Log message

			if (changeType == ChangeType.Acknowledgement)
			{
				DeleteStmData(PkMapRecordName, ediProdPK);
			}
		}

		void DeleteStmData(string pkMapName, ZGuid ediProdPK)
		{
			var dataRecord = GetMappedBorderWiseOrg(pkMapName, ediProdPK);
			dataRecord?.Delete();
			ZExceptionReporting.ProcessWithSaveExceptionHandling(DataFactory.Save, null, true);
		}

		protected override ChangeType GetChangeType(ILogger logger, DataRow changeRow)
		{
			var changeType = base.GetChangeType(logger, changeRow);

			if (changeType == ChangeType.Add)
			{
				changeType = OverrideAddChangeType(changeRow);
			}

			return changeType;
		}

		ChangeType OverrideAddChangeType(DataRow changeRow)
		{
			var ediProdPK = new ZGuid(changeRow[PKColumnName, DataRowVersion.Current]);
			var borderWisePk = GetMappedBorderWisePK(PkMapRecordName, ediProdPK);
			return borderWisePk.IsValid ? ChangeType.Acknowledgement : ChangeType.Add;
		}

		protected ZGuid GetMappedBorderWisePK(string pkMapName, ZGuid ediProdPK)
		{
			var dataRecord = GetMappedBorderWiseOrg(pkMapName, ediProdPK);
			return dataRecord != null ? dataRecord.SD_DepartmentGuid : ZGuid.Missing;
		}

		StmData GetMappedBorderWiseOrg(string pkMapName, ZGuid ediProdPK)
		{
			var dataRecordFilter = new ZQuery(StmDataSchema.SD_Name, pkMapName);
			dataRecordFilter.AddToFilter(StmDataSchema.SD_Type, "BOR");
			dataRecordFilter.AddToFilter(StmDataSchema.SD_Owner, ediProdPK);

			return DataFactory.LoadTop1<StmData>(dataRecordFilter);
		}

		protected OrgSecurity GetOrgSecurityRight(ZGuid orgPk)
		{
			var orgRightQuery = new ZQuery(OrgSecuritySchema.OX_OH, orgPk);
			orgRightQuery.AddToFilter(OrgSecuritySchema.OX_SecurityItemName, "BorderWise");

			return DataFactory.LoadTop1<OrgSecurity>(orgRightQuery);
		}

		protected abstract T GetDataObject(DataRow changeRow, DataRowVersion rowVersion, ChangeType changeType);

		protected DateTime GetDateTimeValue(ZDateTime dateTime)
		{
			return dateTime.IsValid ? dateTime.ToDateTime() : DateTime.MinValue;
		}
	}
}
