using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	public class NctsResponseMessageProcessorPhase5 : BaseMessageProcessor
	{
		public NctsResponseMessageProcessorPhase5(ILogger serviceLogger)
		{
			this.serviceLogger = serviceLogger;
		}

		protected override ZQuery ValidBranchesForMessageFilter
		{
			get
			{
				var result = new ZQuery();
				result.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Queued);
				result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.GbCustomsNCTS);
				return result;
			}
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.AddRange(MessageProcessorDefinitions.Select(x => x.processorFunc(serviceLogger, Logger)));
			return result;
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage inboundMessage)
		{
			var (subType, processorFunc) = MessageProcessorDefinitions.FirstOrDefault(x => x.subType == inboundMessage.EM_MessageSubType);
			if (processorFunc == null)
			{
				serviceLogger.Log(LogType.Error, ZString.Format("Processor for NCTS Phase 5 message type {0} not found", inboundMessage.EM_MessageSubType));
			}
			return processorFunc?.Invoke(serviceLogger, Logger);
		}

		IEnumerable<(string subType, Func<ILogger, LoggingInformation, ApplicationTypeMessageProcessor> processorFunc)> MessageProcessorDefinitions
		{
			get
			{
				yield return ("04C", (logger, logging) => new CC004CProcessor(logger, logging));
				yield return ("09C", (logger, logging) => new CC009CProcessor(logger, logging));
				yield return ("19C", (logger, logging) => new CC019CProcessor(logger, logging));
				yield return ("22C", (logger, logging) => new CC022CProcessor(logger, logging));
				yield return ("25C", (logger, logging) => new CC025CProcessor(logger, logging));
				yield return ("28C", (logger, logging) => new CC028CProcessor(logger, logging));
				yield return ("29C", (logger, logging) => new CC029CProcessor(logger, logging));
				yield return ("35C", (logger, logging) => new CC035CProcessor(logger, logging));
				yield return ("43C", (logger, logging) => new CC043CProcessor(logger, logging));
				yield return ("45C", (logger, logging) => new CC045CProcessor(logger, logging));
				yield return ("51C", (logger, logging) => new CC051CProcessor(logger, logging));
				yield return ("55C", (logger, logging) => new CC055CProcessor(logger, logging));
				yield return ("56C", (logger, logging) => new CC056CProcessor(logger, logging));
				yield return ("57C", (logger, logging) => new CC057CProcessor(logger, logging));
				yield return ("60C", (logger, logging) => new CC060CProcessor(logger, logging));
				yield return ("182", (logger, logging) => new CC182CProcessor(logger, logging));
				yield return ("928", (logger, logging) => new CC928CProcessor(logger, logging));
			}
		}

		readonly ILogger serviceLogger;
	}
}
