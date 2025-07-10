using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.GB.GVMS.Constants;

namespace Enterprise.Customs.GB.GVMS
{
	public class GVMSOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public GVMSOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new GVMSInterchangeProvider(Logger, readyMessages);
		}

		protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());
		ZQuery messageFilter;

		static ZQuery GetMessageFilterQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.GbCustomsGVMSManifest);
			result.AddToFilter(EDIMessageSchema.EM_MessageSubType, GetOutgoingMessageSubTypes());
			return result;
		}

		internal static string[] GetOutgoingMessageSubTypes() => new[]
		{
			GVMSMessageSubTypes.NEW,
			GVMSMessageSubTypes.AMEND,
			GVMSMessageSubTypes.CANCEL,
			GVMSMessageSubTypes.FINALISE,
		};
	}
}
