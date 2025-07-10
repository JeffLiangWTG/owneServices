using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business.Import
{
	public class ImportECWINFMessageProcessor : ImportMessageProcessor<AtlasInboundEDIMessage<IECWINF>, IECWINF>
	{
		public ImportECWINFMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("B68361C1-FC98-47FA-8085-8B95DB041DF7", "Import ECWINF Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IECWINF> message) => GetEntryHeaderFromMRN(message.Factory, message.DataProvider?.MRN, message.DataProvider?.ReferenceNumber);

		public bool ShouldProcessMessage(AtlasInboundEDIMessage<IECWINF> message)
		{
			return GetLinkedObject(message) is CusEntryHeader;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IECWINF> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			var dataProvider = message.DataProvider;
			var registrationNumbers = new List<ZString> { dataProvider.ReferenceNumber, dataProvider.MRN };

			var goodsItems = dataProvider.GoodsItems;

			message.SetLogbookLocalReferenceNumber(dataProvider.LocalReferenceNumber);
			var goodsItemsRegistrationNumbers = goodsItems.Select(g => g.ReferencedRegistrationNumber);
			var goodsItemsMRNs = goodsItems.Select(g => (ZString)g.MRN);
			message.SetLogbookRegistrationNumber(registrationNumbers.Concat(goodsItemsRegistrationNumbers).Concat(goodsItemsMRNs));

			var declaration = entryHeader.Declaration;
			SkipSnapshotUpdate(declaration);
			SendEmail();

			void SendEmail()
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(factory
					, declaration
					, Res.GetString("36F5E4AF-F6AD-47FA-A0AC-6A670D1DD551", "Import ECWINF – Bonded Warehouse Completion Information")
					, GetEmailBody(declaration, message.DataProvider)
					, false
					, message.Branch
					, declaration
					, () => entryHeader.Messages.LastSentOutgoingMessage);
			}
		}

		static ZString GetEmailBody(JobDeclaration declaration, IECWINF provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("0AF0DB58-D632-4A83-9363-8BDE9744D465", "Your Import Declaration for Job {0} received a Bonded Warehouse Completion Information. For details please follow the link to the job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			var referenceNumber = provider.ReferenceNumber;
			if (!referenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("FF8CB0BE-EF39-4EBB-B5AC-41E84585F0B2", "Additional Registration Number"), referenceNumber);
			}

			var mrn = provider.MRN;
			if (mrn != null)
			{
				tableCreator.WriteRow(Res.GetString("A95FD5E0-8332-4BDD-A204-CEF3C4919CBE", "MRN"), mrn);
			}

			var localReferenceNumber = provider.LocalReferenceNumber;
			if (!localReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("2D96D8B6-A7B2-4E83-8B31-4070F6D7CF77", "Local Reference Number"), localReferenceNumber);
			}
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}
	}
}
