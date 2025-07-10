using System;
using CargoWise.Application;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.OperationalAction
{
	public class OperationalActionBulkMucrMessageSender
	{
		readonly JobDeclaration dec;

		public OperationalActionBulkMucrMessageSender(JobDeclaration dec)
		{
			this.dec = dec;
		}

		public void OperationalActionSendMucrMessage(GbDes242MessageFunction howToSend, ISendsMessagesToCustoms notifier)
		{
			if (howToSend != null)
			{
				if (!dec.IsExport)
				{
					notifier.NotifyUserOfAnInvalidOperation("Cannot send this MUCR message for this declaration, it is not an export job.");
				}
				else
				{
					IDeclarationMessageSender messageSender = (IDeclarationMessageSender)Activator.CreateInstance(ObjectFactory.GetType<Integration.Customs.GB.IDeclarationMessageSenderChooser>());
					messageSender.Send(dec, notifier, howToSend);
				}
			}
		}
	}
}
