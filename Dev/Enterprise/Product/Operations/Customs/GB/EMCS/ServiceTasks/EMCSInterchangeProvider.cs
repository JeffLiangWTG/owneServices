using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.CDS;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.EMCS.ServiceTasks
{
	public class EMCSInterchangeProvider : CDSInterchangeProvider
	{
		public EMCSInterchangeProvider(LoggingInformation logger, NonDependentEDIMessageCollection messages) : base(logger, messages)
		{
		}

		protected override string GetCollationKey(EDIMessage message) => message.PK.ToString();

		protected override ZString GetInterchangeFooter(int messageCount) => ZString.Empty;

		protected override Type InterchangeType => typeof(CDSInterchange);

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var emcsRequestMessage = messages.Cast<EDIMessage>().Single();
			SetInterchangeValuesForTransmit(interchange, messages, emcsRequestMessage.EM_MessageType, CDS.Constants.EDIInterchange.GBCustoms, emcsRequestMessage.EM_MessageOwner);
			interchange.EI_HeaderText = GetHeaderText(emcsRequestMessage);
		}

		static ZString GetHeaderText(EDIMessage message)
		{
			var gbCustomsRequest = EMCSExtensions.NewGBCustomsRequest(message);
			if (gbCustomsRequest != null)
			{
				return gbCustomsRequest.Serialize();
			}
			else
			{
				message.EM_Status = EDIMessage.Status.Failed;
				return ZString.Empty;
			}
		}
	}
}
