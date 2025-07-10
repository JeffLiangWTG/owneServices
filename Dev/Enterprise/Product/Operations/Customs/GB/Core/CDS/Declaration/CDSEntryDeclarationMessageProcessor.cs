using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSEntryDeclarationMessageProcessor : GBAutoSendCustomsMessageProcessor
	{
		public CDSEntryDeclarationMessageProcessor(JobDeclaration declaration) : base(declaration)
		{
			wrapper = new JobDeclarationMessageSendingObjectParent(Declaration);
		}

		readonly JobDeclarationMessageSendingObjectParent wrapper;

		protected override ZBool CanSendEntryHeader(CusEntryHeader entryHeader)
		{
			var cusEntryHeader = (EU.Business.Declaration.CusEntryHeader)entryHeader;
			if (cusEntryHeader != null)
			{
				return !GbDeclarationMessageSender.AreAnyHeadersAwaitingAResponse(new[] { cusEntryHeader });
			}
			return false;
		}

		protected override ZBool SendCustomsMessageCore(INotifications notifications, CusEntryHeader entryHeader)
		{
			try
			{
				foreach (var sendingObject in wrapper.SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>())
				{
					sendingObject.ShouldSend = sendingObject.Header == entryHeader;
					if (sendingObject.MessageType == CDSEDIMessageTypeList.Codes.AmendDeclaration)
					{
						if (sendingObject.VOCReason.IsEmpty)
						{
							sendingObject.VOCReason = "Automatic amendment";
						}
						if (sendingObject.ChangeAcknowledgementIndicator.IsEmpty)
						{
							sendingObject.ChangeAcknowledgementIndicator = AmendmentCancellationReasonCode.Codes.A_Other;
						}
					}
				}

				var sender = new CDSMessageSender(wrapper)
				{
					IsAutoSendCustomsMessageProcessor = true
				};
				sender.Send(new SendsMessagesToCustomsShutterUpperer(false));

				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogSystemError(notifications, ex.Message);
				return false;
			}
		}

		protected override ZString MessageDescription => Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
	}
}
