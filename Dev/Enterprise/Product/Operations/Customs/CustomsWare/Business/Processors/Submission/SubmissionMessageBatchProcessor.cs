using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class SubmissionMessageBatchProcessor : BaseOutgoingMessageProcessor
	{
		public SubmissionMessageBatchProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string FactoryNameForDebugging => "CustomWare Submission Message Processing";

		protected override bool IsBranchFilter => false;

		protected override ZQuery MessageFilter => new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationCodeList.Codes.CustomsWare);

		protected override ZQuery GetNewFilter()
		{
			var filter = new ZQuery(EDIMessageSchema.EM_Status, new[] { EDIMessage.Status.Queued, EDIMessage.Status.Pending });
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CustomsWare);
			if (!branchPKs.IsNullOrEmpty())
			{
				filter.AddToFilter(EDIMessageSchema.EM_GB, branchPKs.First());
			}
			return filter;
		}

		protected override ZString PreProcessMessages(NonDependentEDIMessageCollection readyMessages)
		{
			currentMessageBranchPK = readyMessages[0].EM_GB;
			if (branchPKs == null)
			{
				var nonCurrentBranchMessages = readyMessages.Cast<EDIMessage>().Where(x => x.EM_GB != currentMessageBranchPK).ToArray();
				if (nonCurrentBranchMessages.Length > 0)
				{
					readyMessages.RemoveRange(nonCurrentBranchMessages);
					branchPKs = nonCurrentBranchMessages.Select(x => x.EM_GB).Distinct().ToHashSet();
				}
			}
			else if (branchPKs.Remove(currentMessageBranchPK) && branchPKs.Count == 0)
			{
				branchPKs = null;
			}

			return ZString.Empty;
		}
		ZGuid currentMessageBranchPK;
		HashSet<ZGuid> branchPKs;
		IDisposable disposableEnvironment;

		protected override void HandleBeforeMessageProcessing(NonDependentEDIMessageCollection readyMessages, BusinessObjectFactory factory)
		{
			disposableEnvironment = DisposableEnvironment.ForBranch(currentMessageBranchPK.ToGuid());
			factory.Saved -= Factory_Saved;
			factory.Saved += Factory_Saved;
			foreach (EDIMessage message in readyMessages)
			{
				new SubmissionMessageProcessor(message, factory, Logger).Process();
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= Factory_Saved;
			disposableEnvironment?.Dispose();
			disposableEnvironment = null;
		}
	}
}
