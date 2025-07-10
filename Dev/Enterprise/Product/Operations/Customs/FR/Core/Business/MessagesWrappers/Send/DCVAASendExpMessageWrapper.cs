using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Send
{
	public class DCVAASendExpMessageWrapper : DCSendExpMessageWrapper
	{
		public DCVAASendExpMessageWrapper(MessageSending.DeltaGJobDeclarationMessageSendingObject messageToSend, ErrorCollector errorCollector) : base(messageToSend, errorCollector)
		{
		}

		protected override IEnumerable<ILiquidationItem> GetLiquidationCore()
		{
			return SendWrapperHelper.GetLiquidation(entryHeader, true);
		}
	}
}
