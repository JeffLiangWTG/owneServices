using System;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsStatusRequestSender : StatusRequestSender
	{
		public NctsStatusRequestSender(StatusRequest statusRequest) : base(statusRequest)
		{
		}

		protected override IStatusRequestHeader Provider => new TRQQUEProvider(statusRequest);

		protected override Func<IStatusRequestHeader, IProduceMessageXml> MessageBuilder => NctsMessageBuilderLoader.Instance.GetStatusRequestMessageBuilder;
	}
}
