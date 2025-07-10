using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public class ExportDataSender : ExportDeclarationSender
	{
		public ExportDataSender(ExportEntryMessageSendingAction action)
			: base(action.MessagingObject, ExportDeclarationMessageBuilderLoader.ExportData)
		{
			this.action = action;
		}
		readonly ExportEntryMessageSendingAction action;

		protected override ZString MessageSubType => ExportMessageSubTypeList.Codes.EXP;

		protected override IAESMessageHeader DataProvider => (IAESMessageHeader)Activator.CreateInstance(outboundMessageDetails.ProviderType, action);
	}
}
