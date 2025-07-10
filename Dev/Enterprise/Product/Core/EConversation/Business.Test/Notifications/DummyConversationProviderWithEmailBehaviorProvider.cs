using System.Data;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;

namespace Enterprise.EConversation.Testing
{
	sealed class DummyConversationProviderWithEmailBehaviorProvider : DummyConversationProvider, IConversationEmailBehaviorProvider
	{
		public DummyConversationProviderWithEmailBehaviorProvider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool ShouldSendEmailFromSender { get; set; }

		public bool ShouldExcludeSender { get; set; }
	}
}
