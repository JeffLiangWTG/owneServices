using System;
using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Business
{
	public class AesStatusRequestSender : StatusRequestSender
	{
		public AesStatusRequestSender(StatusRequest statusRequest)
			: base(statusRequest)
		{
			outboundMessageDetails = ExportDeclarationMessageBuilderLoader.Instance.GetOutboundMessageDetailsForCurrentVersion(ExportDeclarationMessageBuilderLoader.StatusRequest);
		}
		readonly OutboundMessageDetails outboundMessageDetails;

		protected override IStatusRequestHeader Provider => (IStatusRequestHeader)Activator.CreateInstance(outboundMessageDetails.ProviderType, statusRequest);

		protected override Func<IStatusRequestHeader, IProduceMessageXml> MessageBuilder => (IStatusRequestHeader provider) => (IProduceMessageXml)Activator.CreateInstance(outboundMessageDetails.MessageBuilderType, provider);
	}
}
