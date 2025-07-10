using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.DB.Helpers;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(AutoStmEntityScreeningLog.Schema))]

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.ScreeningStatus
{
	abstract class ScreeningStatusPostUpdate<T> : PostUpdateProcessHandler where T : BusinessObject
	{
		const string ScreeningStatusPropertyName = "ScreeningStatus";

		protected ScreeningStatusPostUpdate(IEntityContext context, IEntity rootEntity) : base(context, rootEntity)
		{
		}

		protected override void UpdateCore()
		{
			using (var readOnlyDbConnection = Db.NewExtraConnectionToMainDbWithReaderCredentials())
			{
				var readOnlyFactory = new ReadOnlyBusinessObjectFactory(readOnlyDbConnection);
				UpdateScreeningStatus(GetOriginalBusinessObject(readOnlyFactory));
			}
		}

		T GetOriginalBusinessObject(ReadOnlyBusinessObjectFactory readOnlyFactory)
		{
			return RootEntity.InternalPK != Guid.Empty ? readOnlyFactory.Load<T>(RootEntity.InternalPK) : null;
		}

		protected abstract bool HasChanges(T screeningBusinessObject);

		protected abstract void SetScreeningStatus(ZGuid pk, string screeningStatus);

		void UpdateScreeningStatus(T screeningBusinessObject)
		{
			string newScreeningStatus;
			if (screeningBusinessObject == null)
			{
				newScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
				SetScreeningStatusValue(RootEntity, newScreeningStatus);
			}
			else if (screeningBusinessObject is IScreeningPartyProvider screeningPartyProvider)
			{
				if (RequireIntercept(screeningPartyProvider) && HasChanges(screeningBusinessObject))
				{
					newScreeningStatus = ScreeningStatusesList.Codes.Unknown;
					SetScreeningStatusValue(RootEntity, newScreeningStatus);

					CreateScreeningStatusLog(string.Empty, AutoEvents.StatusChangeCode);
				}
				else if (!string.IsNullOrWhiteSpace(RootEntity.GetPropertyOrBlankString(ScreeningStatusPropertyName)))
				{
					newScreeningStatus = screeningPartyProvider.ScreeningStatus.ToString();
					SetScreeningStatusValue(RootEntity, newScreeningStatus);
				}
			}
		}

		void SetScreeningStatusValue(IEntity rootEntity, string newScreeningStatus)
		{
			rootEntity[ScreeningStatusPropertyName] = newScreeningStatus;
			SetScreeningStatus(rootEntity.InternalPK, newScreeningStatus);
		}

		bool RequireIntercept(IScreeningPartyProvider screeningPartyProvider)
		{
			return screeningPartyProvider.ScreeningStatus != ScreeningStatusesList.Codes.PermanentClear &&
				screeningPartyProvider.ScreeningStatus != ScreeningStatusesList.Codes.NotScreened &&
				screeningPartyProvider.ScreeningStatus != ScreeningStatusesList.Codes.Unknown;
		}

		void CreateScreeningStatusLog(string reference, string eventCode)
		{
			var stmLogRow = Context.RowFactory.New(StmALogSchema.Constants.TableName);
			stmLogRow[StmALogSchema.Constants.PK] = Guid.NewGuid();
			stmLogRow[StmALogSchema.Constants.SL_IsEstimate] = false;
			stmLogRow[StmALogSchema.Constants.SL_IsCancelled] = false;
			stmLogRow[StmALogSchema.Constants.SL_FireWorkflow] = false;
			stmLogRow[StmALogSchema.Constants.SL_Reference] = reference;
			stmLogRow[StmALogSchema.Constants.SL_PostedTimeUtc] = ZDateTime.UtcNow.ToDateTime();
			stmLogRow[StmALogSchema.Constants.SL_EventTime] = ZDateTime.Now.ToDateTime();
			stmLogRow[StmALogSchema.Constants.SL_GS_NKUser] = GlbStaff.CurrentUser.GS_Code;
			stmLogRow[StmALogSchema.Constants.SL_Table] = RootEntity.TableName;
			stmLogRow[StmALogSchema.Constants.SL_Parent] = RootEntity.InternalPK;
			stmLogRow[StmALogSchema.Constants.SL_SE_NKEvent] = eventCode;
			stmLogRow.Table.Rows.Add(stmLogRow);

			var row = Context.RowFactory.New(AutoStmEntityScreeningLog.Schema.TableName);
			row[AutoStmEntityScreeningLog.Schema.PK] = Guid.NewGuid();
			row[AutoStmEntityScreeningLog.Schema.PJ_ParentID] = RootEntity.InternalPK;
			row[AutoStmEntityScreeningLog.Schema.PJ_ParentTableCode] = TableNameHelper.GetPrefixFromTableName(RootEntity.TableName);
			row[AutoStmEntityScreeningLog.Schema.PJ_Status] = DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges;
			row[AutoStmEntityScreeningLog.Schema.PJ_MatchingData] = reference;
			row[AutoStmEntityScreeningLog.Schema.PJ_LowConfidenceResultsCount] = 0;
			row[AutoStmEntityScreeningLog.Schema.PJ_SourceID] = RootEntity.InternalPK;
			row[AutoStmEntityScreeningLog.Schema.PJ_SourceTableCode] = TableNameHelper.GetPrefixFromTableName(RootEntity.TableName);
			row[AutoStmEntityScreeningLog.Schema.PJ_Sequence] = (int)Env.NumberFountains.StmEntityScreeningLogNumber.GetNext(Context.ObjectFactory);
			row[AutoStmEntityScreeningLog.Schema.PJ_SystemCreateTimeUtc] = ZDateTime.UtcNow.ToDateTime();
			row[AutoStmEntityScreeningLog.Schema.PJ_SystemCreateUser] = GlbStaff.CurrentUser.GS_Code;
			row[AutoStmEntityScreeningLog.Schema.PJ_SystemLastEditTimeUtc] = ZDateTime.UtcNow.ToDateTime();
			row[AutoStmEntityScreeningLog.Schema.PJ_SystemLastEditUser] = GlbStaff.CurrentUser.GS_Code;

			row.Table.Rows.Add(row);
		}
	}
}
