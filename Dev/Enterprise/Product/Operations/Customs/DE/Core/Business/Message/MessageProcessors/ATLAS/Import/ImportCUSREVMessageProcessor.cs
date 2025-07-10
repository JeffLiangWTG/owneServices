using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportCUSREVMessageProcessor : ImportMessageProcessor<AtlasInboundEDIMessage<ICUSREV>, ICUSREV>
	{
		public ImportCUSREVMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("0107C810-9C6F-411C-9BDA-F732D3EE4FAE", "Import CUSREV Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSREV> message) => GetEntryHeaderFromMRN(message.Factory, message.DataProvider?.CancelledMRN, message.DataProvider?.CancelledReferenceNumber);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSREV> message)
		{
			var entryHeader = message.EM_LinkedObject as CusEntryHeader;
			entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.REV;
			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, entryHeader.CH_EntryStatus, ZDateTime.Now.ToOffset());
			message.EM_Status = AtlasEDIMessage.Status.ProcessedOK;
			var dataProvider = message.DataProvider;
			var referenceNumber = dataProvider.ReferenceNumber;
			var cancelledReferenceNumber = dataProvider.CancelledReferenceNumber;
			var mrn = dataProvider.MRN;
			var cancelledMRN = dataProvider.CancelledMRN;
			var entryHeaderMRN = entryHeader.MovementReferenceNumber;

			message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, cancelledReferenceNumber, mrn, cancelledMRN });
			if (!referenceNumber.IsEmpty && entryHeaderMRN == cancelledReferenceNumber)
			{
				entryHeader.MovementReferenceNumberSetter(referenceNumber);
			}
			else if (!mrn.IsNullOrEmpty() && entryHeaderMRN == cancelledMRN)
			{
				entryHeader.MovementReferenceNumberSetter(mrn);
			}

			var declaration = entryHeader.Declaration;
			SkipSnapshotUpdate(declaration);
			SendEmail();

			void SendEmail()
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(factory
					, declaration
					, Res.GetString("90889152-6275-4B61-B4F7-AF52FDBAAF2B", "Import CUSREV – Customs Reverse Message")
					, GetEmailBody(declaration, dataProvider)
					, false
					, message.Branch
					, declaration
					, () => entryHeader.Messages.LastSentOutgoingMessage);
			}
		}

		static ZString GetEmailBody(JobDeclaration declaration, ICUSREV provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("B07EB298-6F93-4831-A873-B0174D647D12", "Your Import Declaration for Job {0} received a Customs Reverse Message. For details please follow the link to the job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var mrn = provider.MRN;
			var cancelledMRN = provider.CancelledMRN;
			var referenceNumber = provider.ReferenceNumber;
			var cancelledReferenceNumber = provider.CancelledReferenceNumber;
			var reason = provider.Reason;

			if (!mrn.IsNullOrEmpty() || !cancelledMRN.IsNullOrEmpty() || !referenceNumber.IsEmpty || !cancelledReferenceNumber.IsEmpty || !reason.IsEmpty)
			{
				var tableCreator = new HtmlTableCreator();
				if (!mrn.IsNullOrEmpty())
				{
					tableCreator.WriteRow(Res.GetString("2F0635E0-790F-47E0-A49A-6FC77C9BF479", "New MRN"), mrn);
				}
				if (!cancelledMRN.IsNullOrEmpty())
				{
					tableCreator.WriteRow(Res.GetString("604742AE-E56D-4D14-9D06-38C9F085E0F1", "Canceled MRN"), cancelledMRN);
				}
				if (!referenceNumber.IsEmpty)
				{
					tableCreator.WriteRow(Res.GetString("77A6237C-CA38-48DB-987B-C06CB7DDDE2B", "New Registration Number"), referenceNumber);
				}
				if (!cancelledReferenceNumber.IsEmpty)
				{
					tableCreator.WriteRow(Res.GetString("5982B211-B98B-4134-8C76-C21A7D6C4FD1", "Canceled Registration Number"), cancelledReferenceNumber);
				}
				if (!reason.IsEmpty)
				{
					tableCreator.WriteRow(Res.GetString("A0FE16F0-578D-40BA-BDC8-14D9E0B58F54", "Cancellation Reason"), reason);
				}
				htmlBody.Append(tableCreator.ToHtml());
			}

			return htmlBody.ToString();
		}
	}
}
