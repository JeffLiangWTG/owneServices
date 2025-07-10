using System;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.HK.ServiceTasks
{
	public class TraxonResponseProcessor : ApplicationTypeMessageProcessor, IDisposable
	{
		public TraxonResponseProcessor(LoggingInformation logger)
			: base(logger)
		{
			traxonMessageProcessor = new TraxonMessageProcessor(logger);
		}

		public void Dispose()
		{
			DisposeCurrentSetting();
		}

		protected override string MessageFriendlyNameCore
		{
			get { return traxonMessageProcessor.MessageFriendlyName; }
		}

		protected override string ApplicationCodeCore
		{
			get { return EDIMessage.ApplicationCodes.Traxon; }
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message.Branch is GlbBranch branch)
			{
				if (branch.PK != GlbBranch.CurrentBranch.PK)
				{
					DisposeCurrentSetting();
					currentSetting = branch.SetAsTemporaryContext();
				}
			}
			traxonMessageProcessor.ProcessMessage(message);
		}

		readonly TraxonMessageProcessor traxonMessageProcessor;

		void DisposeCurrentSetting()
		{
			if (currentSetting != null)
			{
				currentSetting.Dispose();
				currentSetting = null;
			}
		}
		IDisposable currentSetting;
	}
}
