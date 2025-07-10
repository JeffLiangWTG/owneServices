using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.UniversalDataBuss.ServiceTasks
{
	class GrEngineKeyGenMessageProcessor : GrEngineMessageProcessor
	{
		public GrEngineKeyGenMessageProcessor(XmlEDIGrEngine grEngine, LoggingInformation logger, IEnumerable<string> messageSubTypes, IFactoryService factoryService)
			: base(logger, messageSubTypes, factoryService)
		{
			GrEngine = grEngine;
		}

		XmlEDIGrEngine GrEngine { get; }

		protected override void ProcessMessageCore(EDIMessage message)
		{
			var processingManager = new UniversalMessageProcessingManager(Logger);
			using (ObjectFactory.Get<ISuppressHookHelper>().SuppressFieldOnChangeHook())
			{
				var result = processingManager.GetKeysForBlockingParallelImport(message);

				if (result.ShouldShortCircuit)
				{
					// In general the UMK (KeyGen) service task should not cause any side affects other than updating StmQueueState
					// The one exception to this is the fast fail messages to be Discarded or Rejected. To remove any chance of unintended
					// changes being saved we isolate and save the EdiMessage here.
					var isolatedFactory = message.Factory.CreateNewFactory();
					using (isolatedFactory.AddDisposableService())
					{
						var isolatedMessage = (EDIMessage)isolatedFactory.ImportFromAnotherFactory(message);

						UniversalMessageProcessingManager.LogFailedMessage(isolatedMessage, result.Status, new XmlSessionTracker(Logger));
						isolatedFactory.Save();
					}
				}
				else
				{
					GrEngine.PreEnqueuer.Enqueue(message, result.Keys.ToArray());
					GrEngine.NudgeMaster();
				}
			}
		}
	}
}
