using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsECWINFMessageProcessor : NctsMessageProcessor<AtlasInboundEDIMessage<IECWINF>, IECWINF>
	{
		public NctsECWINFMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("7D4AEEDF-7643-4F86-96C1-46BE99322277", "NCTS ECWINF Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IECWINF> message) => GetLinkedObjectFromMRN<NctsHeader>(message.Factory, message.DataProvider?.ReferenceNumber ?? ZString.Empty)?.MovementHeader;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IECWINF> message)
		{
			var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = movementHeader.Header;
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			var registrationNumbers = new List<ZString> { message.DataProvider.ReferenceNumber };

			var dataProvider = message.DataProvider;
			var goodsItems = dataProvider.GoodsItems;

			message.SetLogbookLocalReferenceNumber(dataProvider.LocalReferenceNumber);
			message.SetLogbookRegistrationNumber(registrationNumbers.Concat(goodsItems.Select(g => g.ReferencedRegistrationNumber)));

			SendEmail();

			void SendEmail()
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(factory
					, nctsHeader
					, Res.GetString("F6C9DBFD-1D55-4E52-B913-A93F65C538DA", "NCTS ECWINF - Bonded Warehouse Completion Information")
					, GetEmailBody(nctsHeader, message.DataProvider)
					, false
					, message.Branch
					, nctsHeader
					, () => movementHeader.Messages.LastSentOutgoingMessage);
			}
		}

		static ZString GetEmailBody(NctsHeader header, IECWINF provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("C450AFEF-2120-4ECE-A499-B129FD722CF9", "Your NCTS Departure Declaration for Job {0} received a Bonded Warehouse Completion Information. For details please follow the link to the job.", header.BH_JobReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("C2964B6C-EDD8-4605-88B8-EEB1C7EA0022", "Additional Registration Number"), provider.ReferenceNumber);

			var localReferenceNumber = provider.LocalReferenceNumber;
			if (!localReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("CF3D24DA-89FC-4E1D-8A44-2FABFF45E434", "Local Reference Number"), localReferenceNumber);
			}
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}
	}
}
