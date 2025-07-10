using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHeaderMessageSendingObjectValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T>
		where T : class, INctsHeaderMessageSendingObjectValidationDecider
	{
		public NctsHeaderMessageSendingObjectValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
			: base(factory, ruleDeciderInterfaceTypes)
		{
		}

		protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
		{
			var messageSendingConfiguration = new Mock<MessageSendingConfiguration> { CallBase = true };
			messageSendingConfiguration
				.Protected()
				.Setup<INctsHeaderMessageSendingObjectValidationDecider>("GetValidationDeciderCore")
				.Returns(validationDecider);

			configuration
				.Protected()
				.Setup<MessageSendingConfiguration>("GetNewMessageSendingConfiguration")
				.Returns(messageSendingConfiguration.Object);
		}
	}
}
