using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.AuditDataServices.TransportBooking.Subscribers
{
	public abstract class BaseDtbMasterBookingUpdateOnlyReplicationSubscriber : BaseDtbMasterBookingReplicationSubscriber
	{
		public BaseDtbMasterBookingUpdateOnlyReplicationSubscriber() : base()
		{
		}

		public BaseDtbMasterBookingUpdateOnlyReplicationSubscriber(BusinessObjectFactory factory) : base(factory)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging message")]
		protected override void PerformReplication(RowChangeType rowChangeType, ILogger logger, DataRow currentRow)
		{
			var newMasterBookingVersion = (ZShort)currentRow[MasterBookingVersionColumn.Name].GetDataRowValue<short>();
			var masterEntityPk = (ZGuid)currentRow[PKColumn.Name].GetDataRowValue<Guid>();
			var masterEntityDescription = GetChangeRowEntityDescription(currentRow, null);

			var subEntityQuery = new ZQuery(SubToMasterLinkColumn, masterEntityPk);
			var subEntities = Factory.Load(BizOType, subEntityQuery);

			var recordsUpdatedSuccessfully = 0;
			var recordsUpdatedUnsuccessfully = 0;
			foreach (var subEntity in subEntities.Where(e => (ZShort)e[MasterBookingVersionColumn] != newMasterBookingVersion))
			{
				var subEntityToProcess = subEntity;
				var processedSuccessfully = false;
				try
				{
					if (subEntityToProcess.Factory._Instance != Factory._Instance)
					{
						subEntityToProcess = Factory.Load(BizOType, subEntityToProcess.PK);
					}

					UpdateSubEntityFromRow(subEntityToProcess, currentRow, newMasterBookingVersion, checkSubEntityMasterBookingVersion: true);

					SaveToFactory();
					processedSuccessfully = true;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					processedSuccessfully = false;

					try
					{
						Factory = new BusinessObjectFactory();
						subEntityToProcess = Factory.Load(BizOType, subEntityToProcess.PK);
						var subDtbBooking = GetDtbBookingFromEntity(subEntityToProcess);

						var errorMessageToLog = GetErrorNoteLog(RowChangeType.Update, subEntity, subDtbBooking, currentRow, newMasterBookingVersion, ex);
						logger.Error(errorMessageToLog, ex);

						var errorNoteMessage = GetErrorNoteMessage(RowChangeType.Update, subEntity, subDtbBooking, currentRow, newMasterBookingVersion, ex);
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
					recordsUpdatedSuccessfully++;
					logger.Information(FormattableString.Invariant($"Updated sub {GetEntityDescription(subEntity)} for master {masterEntityDescription} on MasterBookingVersion {newMasterBookingVersion}"));
				}
				else
				{
					recordsUpdatedUnsuccessfully++;
				}
			}
			var updateMessage = recordsUpdatedSuccessfully + recordsUpdatedUnsuccessfully > 0 ?
				FormattableString.Invariant($"Updated {recordsUpdatedSuccessfully} subs successfully, {recordsUpdatedUnsuccessfully} failed") :
				"No updates were made to subs";

			logger.Log(LogType.Information, FormattableString.Invariant($"{updateMessage} for master {masterEntityDescription} on MasterBookingVersion {newMasterBookingVersion}"));
		}

		protected string GetErrorNoteMessage(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow, ZShort newMasterBookingVersion, Exception ex)
		{
			var subAndMasterMessagePart = GetErrorNoteSubAndMasterMessagePart(rowChangeType, subEntity, subDtbBooking, masterEntityChangeRow);
			return GetErrorNoteMessageCore(subAndMasterMessagePart, newMasterBookingVersion, ex);
		}

		protected abstract string GetErrorNoteSubAndMasterMessagePart(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow);

		protected string GetErrorNoteLog(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow, ZShort newMasterBookingVersion, Exception ex)
		{
			var subAndMasterMessagePart = GetErrorNoteSubAndMasterLogPart(rowChangeType, subEntity, subDtbBooking, masterEntityChangeRow);
			return GetErrorNoteLogCore(subAndMasterMessagePart, newMasterBookingVersion, ex);
		}

		protected abstract string GetErrorNoteSubAndMasterLogPart(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow);

		protected abstract BusinessObject GetDtbBookingFromEntity(BusinessObject entity);
	}
}
