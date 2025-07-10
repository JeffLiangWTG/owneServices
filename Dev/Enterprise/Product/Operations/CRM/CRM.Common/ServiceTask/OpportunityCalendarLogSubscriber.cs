using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CRM.Common
{
	[Serializable]
	public class OpportunityCalendarLogSubscriber : LogSubscriber
	{
		public override string Name => nameof(OpportunityCalendarLogSubscriber);

		public override string[] EventTypes => new[] { AutoEvents.RecallDateUpdated.Code };

		public override string[] TableNames => new[] { OrgOpportunitySchema.Constants.TableName, CrmOpportunitySchema.Constants.TableName };
		public override string FriendlyName => (NoResString)"Opportunity Calendar Reminder Subscriber";

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (var log in queuedLogs)
			{
				if (log.SJ_ParentTableCode == OrgOpportunitySchema.Constants.Prefix)
				{
					var opportunity = log.Factory.Load<OrgOpportunity>(log.SJ_ParentID);
					if (opportunity != null)
					{
						CreateAppointment(log, opportunity, opportunity.GetNewRecallReminder);
					}
				}
				else if (log.SJ_ParentTableCode == CrmOpportunitySchema.Constants.Prefix)
				{
					var opportunity = log.Factory.Load<CrmOpportunity>(log.SJ_ParentID);
					if (opportunity != null)
					{
						CreateAppointment(log, opportunity, opportunity.GetNewReminder);
					}
				}
			}
		}

		internal void CreateAppointment(IQueuedLog log, BusinessObject opportunity, Func<ZDateTime, ZDateTime, Reminder> getReminder)
		{
			var logQuery = new ZQuery(StmALogSchema.PK, log.SJ_ALogReference);
			logQuery.AddToFilter(StmALogSchema.SL_Parent, opportunity.PK);
			logQuery.AddToFilter(StmALogSchema.SL_Table, opportunity.TableName);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.RecallDateUpdated.Code);
			var stmALog = log.Factory.LoadTop1<StmALog>(logQuery);

			var staff = log.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, stmALog.SL_GS_NKUser));
			var branch = log.Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, stmALog.SL_GB_NKBranch));
			var department = log.Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, stmALog.SL_GE_NKDepartment));
			var originalDate = GetDateFromLog(stmALog, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old);
			var newDate = GetDateFromLog(stmALog, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				try
				{
					getReminder(originalDate, newDate).CreateAppointment();
				}
				catch (EmailSendFailedException ex)
				{
					DefaultLogger.Log(LogType.Error, $"Could not create appointment due to the following error:{System.Environment.NewLine}{ex.Message}" );
				}
			}
		}

		ZDateTime GetDateFromLog(StmALog log, string eventReferenceParameterCode)
		{
			var recallDate = ZDateTime.Empty;
			var parameters = StmALog.GetParametersFromReference(log.SL_Reference);
			if (parameters.TryGetValue(eventReferenceParameterCode, out var parameter))
			{
				if (ZDateTime.TryParseExact(parameter, out var resultTime, "yyyyMMdd\\THHmmss\\Z"))
				{
					recallDate = resultTime;
				}
			}
			return recallDate;
		}
	}
}
