using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Netting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting
{
	[Serializable]
	public class NettingTransactionSubscriber : LogSubscriber
	{
		public override string[] EventTypes
		{
			get { return new string[] { Events.AddedARecordToTheSystem.Code, Events.EditedARecord.Code }; }
		}

		public override string Name
		{
			get { return "NettingTransactionSubscriber"; }
		}

		public override string[] TableNames
		{
			get { return new string[] { NettingReceivableTransactionSchema.Constants.TableName }; }
		}

		public override bool IsRequired => AccountingMasterFilesRegistry.Instance.EnableNetting.Value;

		public override bool HasDynamicProperties => true;

		protected override ILogBatcher GetLogBatcher() => new LogBatcher(DefaultLogger);

		class LogBatcher : LogBatcher<ZGuid?>
		{
			public LogBatcher(ILogger logger)
			{
				this.logger = logger;
			}

			readonly ILogger logger;

			protected override void AddGroupingFetchHints(IEnumerable<IQueuedLog> enumberable) { }

			protected override ZGuid? GetGroupLogKey(IQueuedLog log) => GetNettingCompany(log);

			protected override LogsGroupContext SetContextForLogsGroup(ZGuid? groupKey, IEnumerable<IQueuedLog> queuedLogs)
			{
				var firstLog = queuedLogs.First();
				var orgCode = firstLog.Factory.Load<NettingReceivableTransaction>(firstLog.SJ_ParentID)?.Recipient?.Organisation?.OH_Code;
				var companyPK = GetNettingCompany(firstLog);
				GlbCompany company = null;
				if (companyPK.HasValue)
				{
					company = firstLog.Factory.Load<GlbCompany>(companyPK.Value);
				}
				if (company != null && company.GC_IsActive)
				{
					if (company.FirstActiveBranch != null)
					{
						return new LogsGroupContext(false, DisposableEnvironment.ForBranch(company.FirstActiveBranch.PK.ToGuid()));
					}
					else
					{
						logger.Log(LogType.Warning, $@"Netting Participant {orgCode}’s Company {company.GC_Code} does not have any active branch. Please check and correct the Netting Participant in the Netting System Portal(NSP), and ensure there is atleast one active branch in Company {company.GC_Code}.");
						return new LogsGroupContext(false);
					}
				}
				logger.Log(LogType.Warning, $@"No active Netting Company Found for the participant {orgCode}. Please check and correct the Netting Participant in the Netting System Portal(NSP).");
				return new LogsGroupContext(false);
			}
		}

		static ZGuid? GetNettingCompany(IQueuedLog arg)
		{
			if (arg.Factory.Load<NettingReceivableTransaction>(arg.SJ_ParentID) is NettingReceivableTransaction tran)
			{
				return tran?.NettingSystem.NS_GC;
			}
			return null;
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var factory = queuedLogs[0].Factory;
			var company = factory.Load<GlbCompany>(GetNettingCompany(queuedLogs[0]).Value);
			Argument.NotNull(company, nameof(company));
			if (AccountingConfigurationRegistry.Instance.IsNettingSystem.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				foreach (var queuedLog in queuedLogs)
				{
					if (queuedLog.SJ_ParentTableCode != NettingReceivableTransactionSchema.Constants.Prefix)
					{
						continue;
					}
					var nettingReceivableTransaction = factory.Load<NettingReceivableTransaction>(queuedLog.SJ_ParentID);

					if (nettingReceivableTransaction.NRT_ApprovalStatus != Enterprise.ZArchitecture.Core.NettingTransactionApprovalStatus.Approved)
					{
						continue;
					}

					if (nettingReceivableTransaction.NRT_SystemCreateUser == User.ServiceUserCode)
					{
						continue;
					}

					var recipient = factory.Load<NettingOrganisation>(nettingReceivableTransaction.NRT_NSO_Recipient);
					var recipientOrgHeader = recipient.Organisation;

					var cusCodes = recipientOrgHeader.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.EHubOrganisationID);
					SendMessage(nettingReceivableTransaction, cusCodes);
				}
			}
		}

		void SendMessage(NettingReceivableTransaction nettingReceivableTransaction, OrgCusCode[] cusCodes)
		{
			var notificationBuffer = new NotificationBuffer();

			Func<OrgHeader> orgProvider = null;

#if DEBUG
			var factory = cusCodes[0].Factory;
			var nettingSystemOrgHeader = factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			orgProvider = () => nettingSystemOrgHeader;
#endif

			var actionInfo = new ActionWrapper(nettingReceivableTransaction, MessageRecipientPartyTypeList.Codes.WiseNettingSystem);
			Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = (outboundSessionTracker) => GetUniversalTransactionDataObjectWriter(outboundSessionTracker, nettingReceivableTransaction);
			var processor = UniversalXmlWorkflowProcessorBuilder.New(actionInfo
					, new OrgProxyCommunicationModeProvider(EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction, orgProvider: orgProvider, filter: comMode => cusCodes.Any(cusCode => cusCode.OK_CustomsRegNo == comMode.EK_Destination))
					, dataWriterGetter
					, nettingReceivableTransaction
					, null
					, null
					, null);

			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;

#if DEBUG
			var result = processor.Process(notificationBuffer, replaceThisTokenEventuallyQuestionMarkExclamationMark);
			NumberOfSuccessfulProcessedLogForTest += result.Where(r => r.EventType != Events.DataExportFailureCode).Count();
#else
			processor.Process(notificationBuffer, replaceThisTokenEventuallyQuestionMarkExclamationMark);
#endif
		}

		public ITopLevelDataObjectWriter GetUniversalTransactionDataObjectWriter(IDataWritingManager outboundSessionTracker, NettingReceivableTransaction nettingReceivableTransaction)
		{
			var transactionManager = (ITransactionDataContextManager)nettingReceivableTransaction.GetUniversalDataContextManager();
			return transactionManager.GetTransactionDataObjectWriter(outboundSessionTracker);
		}

#if DEBUG
		public int NumberOfSuccessfulProcessedLogForTest;
#endif
	}
}
