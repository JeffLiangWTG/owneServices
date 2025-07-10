using System;

namespace Enterprise.Customs.DE.Business
{
	public class OutboundMessageDetails
	{
		public OutboundMessageDetails(Type messageBuilderType, Type providerType)
		{
			MessageBuilderType = messageBuilderType;
			ProviderType = providerType;
		}

		public Type MessageBuilderType { get; }

		public Type ProviderType { get; }
	}
}
