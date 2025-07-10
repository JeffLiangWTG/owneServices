using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class EMCSInboundBranchMessageProcessor : BranchMessageProcessor
	{
		public EMCSInboundBranchMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZQuery ValidBranchesForMessageFilter
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(EDIMessage));
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.GbCustomsEMCS);
				return query.AddToFilter(base.ValidBranchesForMessageFilter);
			}
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			BranchCustomsApplicationTypeMessageProcessor result = null;

			var processorDetail = EMCSResponseMessageDetails.Instance.GetResponseMessage(message.EM_MessageType);
			if (processorDetail != null)
			{
				var obj = Activator.CreateInstance(processorDetail.Value.ProcessorType, Logger, processorDetail.Value.XmlObjectType);

				result = (BranchCustomsApplicationTypeMessageProcessor)obj;
			}
			return result;
		}

		protected override bool HasAnyMessageAnticipated() => true;
	}
}
