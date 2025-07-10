using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.AuditDataServices.TransportBooking.Subscribers
{
	public abstract class BaseDtbMasterBookingInsertingReplicationSubscriber : BaseDtbMasterBookingReplicationSubscriber
	{
		public BaseDtbMasterBookingInsertingReplicationSubscriber() : base()
		{
		}

		public BaseDtbMasterBookingInsertingReplicationSubscriber(BusinessObjectFactory factory) : base(factory)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging message")]
		protected override void PerformReplication(RowChangeType initialRowChangeType, ILogger logger, DataRow changeRow)
		{
			var updateSubIfExists = initialRowChangeType == RowChangeType.Update;
			var rowChangeType = initialRowChangeType;
			var newMasterBookingVersion = (ZShort)changeRow[MasterBookingVersionColumn.Name].GetDataRowValue<short>();
			var masterEntityPk = (ZGuid)changeRow[PKColumn.Name].GetDataRowValue<Guid>();
			var masterEntity = GetMasterEntity(masterEntityPk);
			if (masterEntity == null)
			{
				return;
			}

			var parentEntityPk = (ZGuid)changeRow[ParentDtbEntityLinkColumn.Name].GetDataRowValue<Guid>();
			var masterParentEntity = Factory.Load(ParentDtbEntityBizOType, parentEntityPk);
			if (masterParentEntity == null)
			{
				return;
			}

			var masterEntityDescription = GetChangeRowEntityDescription(changeRow, masterParentEntity);

			var queryForSubParents = new ZQuery(ParentSubToMasterLinkColumn, masterParentEntity.PK);
			var subParents = Factory.Load(ParentDtbEntityBizOType, queryForSubParents);

			var updates = 0;
			var updatesFailed = 0;
			var inserts = 0;
			var insertsFailed = 0;

			foreach (var subParent in subParents)
			{
				var subParentToProcess = subParent;
				var processedSuccessfully = false;
				BusinessObject subEntity = null;
				var infoMessageToLog = string.Empty;
				try
				{
					if (subParentToProcess.Factory._Instance != Factory._Instance)
					{
						subParentToProcess = Factory.Load(ParentDtbEntityBizOType, subParentToProcess.PK);
					}

					var queryForSubEntity = new ZQuery(SubToMasterLinkColumn, masterEntityPk);
					subEntity = ((IActiveBusinessObjectCollection)subParentToProcess[ParentEntityCollectionProperty]).Find(queryForSubEntity).FirstOrDefault();

					if (subEntity == null)
					{
						rowChangeType = RowChangeType.Insert;
						subEntity = Factory.New(BizOType);
						subEntity[ParentDtbEntityLinkColumn] = subParentToProcess.PK;
						subEntity[SubToMasterLinkColumn] = masterEntityPk;
						UpdateSubEntityFromRow(subEntity, changeRow, newMasterBookingVersion, checkSubEntityMasterBookingVersion: false);
						infoMessageToLog = FormattableString.Invariant($"Inserted sub {GetEntityDescription(subEntity)} for master {masterEntityDescription} on MasterBookingVersion {newMasterBookingVersion}");
					}
					else if (updateSubIfExists)
					{
						var oldSubDescription = GetEntityDescription(subEntity);
						UpdateSubEntityFromRow(subEntity, changeRow, newMasterBookingVersion, checkSubEntityMasterBookingVersion: true);
						infoMessageToLog = FormattableString.Invariant($"Updated sub {oldSubDescription} for master {masterEntityDescription} on MasterBookingVersion {newMasterBookingVersion}");
					}
					SaveToFactory();
					processedSuccessfully = true;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					processedSuccessfully = false;

					try
					{
						Factory = new BusinessObjectFactory();

						if (subEntity != null)
						{
							subEntity = Factory.Load(BizOType, subEntity.PK);
						}
						subParentToProcess = (subParentToProcess == null) ? null : Factory.Load(ParentDtbEntityBizOType, subParentToProcess.PK);
						var subDtbBooking = (subParentToProcess == null) ? null : GetDtbBookingFromParentEntity(subParentToProcess);

						var errorMessageToLog = GetErrorNoteLog(rowChangeType, subEntity, subParentToProcess, subDtbBooking, changeRow, masterParentEntity, newMasterBookingVersion, ex);
						logger.Error(errorMessageToLog, ex);

						var errorNoteMessage = GetErrorNoteMessage(rowChangeType, subEntity, subParentToProcess, subDtbBooking, changeRow, masterParentEntity, newMasterBookingVersion, ex);
						subDtbBooking.GetNotes().AddNew(isCustomDescription: true, description: ErrorNoteDescription, noteText: errorNoteMessage);
						Factory.Save();
					}
					catch (Exception noteHandlingEx) when (!noteHandlingEx.IsCriticalException())
					{
						Factory = new BusinessObjectFactory();

						ErrorReporter.ReportOnce(FormattableString.Invariant($"Error while handling error replicating master {masterEntityDescription} on MasterBookingVersion {newMasterBookingVersion}"), new AggregateException(ex, noteHandlingEx));
					}
				}

				if (processedSuccessfully)
				{
					if (rowChangeType == RowChangeType.Update)
					{
						updates++;
					}
					else
					{
						inserts++;
					}
					logger.Information(infoMessageToLog);
				}
				else
				{
					if (rowChangeType == RowChangeType.Update)
					{
						updatesFailed++;
					}
					else
					{
						insertsFailed++;
					}
				}
			}

			var updateMessage = updates + updatesFailed == 0 ? string.Empty : FormattableString.Invariant($"Updated {updates} subs successfully, {updatesFailed} failed");
			var insertMessage = inserts + insertsFailed == 0 ? string.Empty : FormattableString.Invariant($"Inserted {inserts} subs successfully, {insertsFailed} failed");
			var updateAndInsertMessage = ((string.IsNullOrEmpty(updateMessage)) ? ((string.IsNullOrEmpty(insertMessage)) ? "No updates or inserts were made to subs" : insertMessage) : (updateMessage + ((string.IsNullOrEmpty(insertMessage)) ? string.Empty : " and " + insertMessage.ToLowerInvariant())));
			logger.Information(FormattableString.Invariant($"{updateAndInsertMessage} for master {masterEntityDescription} on MasterBookingVersion {newMasterBookingVersion}"));
		}

		protected string GetErrorNoteMessage(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subParentEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow, BusinessObject masterParentEntity, ZShort newMasterBookingVersion, Exception ex)
		{
			var subAndMasterMessagePart = GetErrorNoteSubAndMasterMessagePart(rowChangeType, subEntity, subParentEntity, subDtbBooking, masterEntityChangeRow, masterParentEntity);
			return GetErrorNoteMessageCore(subAndMasterMessagePart, newMasterBookingVersion, ex);
		}

		protected abstract string GetErrorNoteSubAndMasterMessagePart(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subParentEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow, BusinessObject masterParentEntity);

		protected string GetErrorNoteLog(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subParentEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow, BusinessObject masterParentEntity, ZShort newMasterBookingVersion, Exception ex)
		{
			var subAndMasterMessagePart = GetErrorNoteSubAndMasterLogPart(rowChangeType, subEntity, subParentEntity, subDtbBooking, masterEntityChangeRow, masterParentEntity);
			return GetErrorNoteLogCore(subAndMasterMessagePart, newMasterBookingVersion, ex);
		}

		protected abstract string GetErrorNoteSubAndMasterLogPart(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subParentEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow, BusinessObject masterParentEntity);

		protected abstract BusinessObject GetDtbBookingFromParentEntity(BusinessObject parentEntity);

		protected abstract Type ParentDtbEntityBizOType { get; }
		protected abstract SchemaColumn ParentDtbEntityLinkColumn { get; }
		protected abstract string ParentEntityCollectionProperty { get; }
		protected abstract SchemaColumn ParentSubToMasterLinkColumn { get; }

#if DEBUG
		internal BusinessObject ReloadParentEntityFromCurrentFactory(BusinessObject entity) => Factory.Load(ParentDtbEntityBizOType, entity.PK);
#endif

	}
}
