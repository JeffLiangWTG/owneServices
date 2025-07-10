using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public class EntireDataSender : ExportDeclarationSender
	{
		public EntireDataSender(ExportEntryMessageSendingAction action)
			: base(action.MessagingObject, ExportDeclarationMessageBuilderLoader.EntireData)
		{
			this.action = action;
		}
		readonly ExportEntryMessageSendingAction action;

		protected override ZString MessageSubType => ExportMessageSubTypeList.Codes.EXP;

		protected override IAESMessageHeader DataProvider => (IAESMessageHeader)Activator.CreateInstance(outboundMessageDetails.ProviderType, action);
	}
}
