using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ServiceTasks
{
	class NativeMessageProcessor : ApplicationTypeMessageProcessor
	{
		public NativeMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string ApplicationCodeCore
		{
			get { return EDIMessage.ApplicationCodes.NativeDataMessaging; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is the name of a Service Task.")]
		protected override string MessageFriendlyNameCore
		{
			get { return "XML Native Data Message"; }
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			new NativeMessageProcessingManager(Logger).Process(message);
		}

		protected override ZQuery MessageFilterCore
		{
			get
			{
				var result = base.MessageFilterCore;
				result.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
				return result;
			}
		}
	}
}
