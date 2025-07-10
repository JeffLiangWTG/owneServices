using System;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.EdiMessages;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DeltaIEMessageSender : EntryMessageSender<DeltaIEJobDeclarationMessageSendingObject>
	{
		public DeltaIEMessageSender(DeltaIEJobDeclarationMessageSendingObject decWrapper, ErrorCollector errorCollector) : base(decWrapper, errorCollector)
		{
		}

		protected override bool ShouldIncreaseSequenceNumber => true;

		protected override MessageBuilderManager<DeltaIEJobDeclarationMessageSendingObject> GetBuilderManager()
		{
			return new DeltaIEMessageBuilderManager();
		}

		protected override Type GetTypeOfMessageToSend() => typeof(DeltaIEFREDIMessage);
	}
}
