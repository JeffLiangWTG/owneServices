using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Registry;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.FR.ServiceTasks
{
	public class PdsRegulariser
	{
		const string Code = "FRF";
		const int DefaultBatch = 1;
		const int DefaultBatchSize = 50;
		const int DefaultRegularisationTimer = 5;
		const int DefaultRegularisationPeriod = 60;

		readonly BusinessObjectFactory factory;
		readonly LoggingInformation logger;
		readonly GlbBranch branch;

		public PdsRegulariser(BusinessObjectFactory factory, GlbBranch branch, LoggingInformation logger)
		{
			this.factory = factory;
			this.branch = branch;
			this.logger = logger;
		}

		public void DoEverything(ZString country)
		{
			RegulariseAfterEndOfFallbackIfRelevant(country);
		}

		void RegulariseAfterEndOfFallbackIfRelevant(ZString country)
		{
			var deltaGRegistrySetting = (FRCustomsDataRegistry.Instance.DeltaGMode.Value as FallbackSettings);
			var regularisationTimer = ServiceTaskHelper.IfZero(FRCustomsDataRegistry.Instance.RegularisationTimerInMinutes.Value, DefaultRegularisationTimer);
			if (deltaGRegistrySetting.End.IsInThePast() && deltaGRegistrySetting.Regularisation.IsInThePast())
			{
				CalculateRegularisationCountAndBatchSize(deltaGRegistrySetting, regularisationTimer, country);

				var batchSize = ServiceTaskHelper.IfZero(deltaGRegistrySetting.RegularisationBatchSize.ToZInt(), DefaultBatchSize);
				var pdsEdiMessage = GetBatchOfQueuedMessagesInPdsStatusAwaitingRegularisation(deltaGRegistrySetting.Start, deltaGRegistrySetting.End, batchSize, country);

				if (pdsEdiMessage.Any())
				{
					foreach (var ediMessage in pdsEdiMessage)
					{
						RegulariseOneEntryAndReleaseItsHeldMessage(ediMessage, country);
					}
					factory.Save();
					SetupNextRuntimeNMinutesFromNow(regularisationTimer);
				}
				else
				{
					ClearDeltaGRegistrySetting(deltaGRegistrySetting);
					ScheduleReport();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Logger strings")]
		void ScheduleReport()
		{
			var recipientGroup = (ZGuid)FRCustomsDataRegistry.Instance.FallbackRegularisationReportNotificationGroup.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
			if (!recipientGroup.IsValid)
			{
				logger.LogWarning("No group set in registry Customs > France > FR Customs Regularisation Report Notification group, the report won't be delivered.");
			}
			else
			{
				var schedule = factory.New<ReportScheduleTask>();
				var scheduledTime = ZDateTime.UtcNow.AddMinutes(1);
				schedule.S5_NextScheduledPrintRunTimeUtc = scheduledTime;
				schedule.S5_EndAfterCount = 1;
				schedule.S5_ParentID = Guid.Parse("a7c54ba4-08a0-427f-b409-f997c1d4b820");
				schedule.S5_ParentTableCode = StmMenuItemSchema.Constants.Prefix;
				schedule.S5_ScheduleDescription = FormattableString.Invariant($"FR Delta G Regularization Report for {ZDateTime.UtcToday:yyyy-MM-dd}");
				var recipient = schedule.Recipients.AddNew();
				recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				recipient.S6_AttachmentType = AttachmentTypeList.Codes.Pdf;
				recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Group;
				recipient.S6_GG = recipientGroup;
				logger.Log(FormattableString.Invariant($"Report is scheduled to be delivered at {scheduledTime:yyyyMMddHHmm}"));
			}
			factory.Save();
		}

		void CalculateRegularisationCountAndBatchSize(FallbackSettings fallbackSetting, int regularisationTimer, ZString country)
		{
			var existingRegularisationCount = fallbackSetting.RegularisationCount;
			if (existingRegularisationCount.IsEmpty)
			{
				var calculatedRegularisationCount = GetBatchOfQueuedMessagesInPdsStatusAwaitingRegularisation(fallbackSetting.Start, fallbackSetting.End, 0, country).Length;

				var deltaGRegularisationPeriod = ServiceTaskHelper.IfZero(fallbackSetting.RegularisationPeriod, DefaultRegularisationPeriod);
				var howMatchBatxchesNeededToSendMessagesEvenlySpaced = ServiceTaskHelper.IfZero(deltaGRegularisationPeriod / regularisationTimer, DefaultBatch);
				var batchSize = Convert.ToInt32(Math.Ceiling((decimal)calculatedRegularisationCount / howMatchBatxchesNeededToSendMessagesEvenlySpaced));

				fallbackSetting.RegularisationCount = calculatedRegularisationCount;
				fallbackSetting.RegularisationBatchSize = batchSize;
			}
		}

		EDIMessage[] GetBatchOfQueuedMessagesInPdsStatusAwaitingRegularisation(ZDateTime fallbackSatatDate, ZDateTime fallbackEndDate, int batchSize, ZString country)
		{
			var entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			ServiceTaskHelper.AddFallbackEntryNumFilter(entryNumQuery, DeltaGFallbackStatusList.Codes.PDS, country);
			if (!fallbackSatatDate.IsEmpty)
			{
				entryNumQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, fallbackSatatDate);
			}
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, fallbackEndDate);

			var ediMessageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			ediMessageQuery.AddSubQuery(EDIMessageSchema.EM_LinkUniqueID, entryNumQuery, JoinCondition.And);

			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), EDIMessageSchema.EM_GB);
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
			companyQuery.AddToFilter(GlbCompanySchema.PK, Environment.Env.CurrentCompanyPK);
			branchQuery.AddSubQuery(companyQuery, JoinCondition.And);
			ediMessageQuery.AddSubQuery(branchQuery, JoinCondition.And);

			if (batchSize > 0)
			{
				ediMessageQuery.MaximumRows = batchSize;
			}
			ediMessageQuery.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name;
			return factory.Load<EDIMessage>(ediMessageQuery);
		}

		void RegulariseOneEntryAndReleaseItsHeldMessage(EDIMessage message, ZString country)
		{
			message.EM_HeldUntilDate = ZDateTime.UtcNow;

			var entryNumQuery = new ZQuery();
			ServiceTaskHelper.AddFallbackEntryNumFilter(entryNumQuery, DeltaGFallbackStatusList.Codes.PDS, country);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, message.EM_LinkUniqueID);
			var entryNumber = factory.LoadTop1<CusEntryNumber>(entryNumQuery);

			if (entryNumber == null)
			{
				logger.Log(FormattableString.Invariant($"Unable to find Customs EntryNum for MessageType : {message.EM_MessageType} and MessageNum : {message.EM_MessageNum}"), Integration.LogType.Error);
			}
			else
			{
				entryNumber.CE_EntryStatus = DeltaGFallbackStatusList.Codes.RGA;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		void SetupNextRuntimeNMinutesFromNow(int regularisationTimer)
		{
			var nextRuntime = ZDateTime.UtcNow.AddMinutes(regularisationTimer);
			ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskNextRuntime(Code, nextRuntime.UtcToDateTimeOffset().ToDateTimeOffsetSafe());
			logger.Log(string.Format(CultureInfo.InvariantCulture, "FR Customs Fallback Processor nudged to run at: {0} ", nextRuntime), Integration.LogType.Information);
		}

		void ClearDeltaGRegistrySetting(FallbackSettings deltaGRegistrySetting)
		{
			deltaGRegistrySetting.Start = ZDateTime.Empty;
			deltaGRegistrySetting.End = ZDateTime.Empty;
			deltaGRegistrySetting.Regularisation = ZDateTime.Empty;
			deltaGRegistrySetting.RegularisationPeriod = ZInt.Zero;
			deltaGRegistrySetting.RegularisationCount = ZDecimal.Zero;
			deltaGRegistrySetting.RegularisationBatchSize = ZDecimal.Zero;
			deltaGRegistrySetting.InvocationReason = ZString.Empty;
			deltaGRegistrySetting.RevocationReason = ZString.Empty;
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, deltaGRegistrySetting);
		}
	}
}
