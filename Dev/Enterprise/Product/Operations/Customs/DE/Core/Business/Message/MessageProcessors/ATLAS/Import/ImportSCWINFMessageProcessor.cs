using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Import
{
	public class ImportSCWINFMessageProcessor : ImportMessageProcessor<AtlasInboundEDIMessage<ISCWINF>, ISCWINF>
	{
		public ImportSCWINFMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("5AEA053C-83DE-4F52-855D-175AF51DE5BA", "Import SCWINF Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ISCWINF> message) => null;

		protected override bool MustHaveLinkedObject => false;

		protected override bool NeedAttachDocumentsToLinkedObject => false;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ISCWINF> message)
		{
			var dataProvider = message.DataProvider;
			if (dataProvider == null)
			{
				message.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error;
			}
			else
			{
				message.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK;
				SendEmail(message, dataProvider);

				message.SetLogbookRegistrationNumber(new ZString[] { dataProvider.ReferenceNumber, dataProvider.MRN });
				message.SetLogbookLocalReferenceNumber(dataProvider.CurrentProcedure);
			}
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var result = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			AttachDocumentsToEmail(result, attachedDocumentsCached);
			return result;
		}

		void SendEmail(AtlasInboundEDIMessage<ISCWINF> message, ISCWINF dataProvider)
		{
			attachedDocumentsCached = message.AttachedDocuments;
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, null
				, Res.GetString("59E173FD-9502-4B73-BB7E-6E14ADFE215D", "Import SCWINF – Transfer Information bonded warehouse")
				, GetEmailBody(dataProvider)
				, false
				, message.Branch
				, null
				, () => string.Empty);
		}

		static ZString GetEmailBody(ISCWINF provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("D2B7DC6B-3122-4B85-A70D-3D2B3C8A5F8C", "For details please open the attached report."));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			if (!string.IsNullOrEmpty(provider.MRN))
			{
				tableCreator.WriteRow(Res.GetString("d63299b9-e209-4618-96c2-3bd298546ff6", "MRN"), provider.MRN);
			}

			if (!string.IsNullOrEmpty(provider.ReferenceNumber))
			{
				tableCreator.WriteRow(Res.GetString("2A24BA8D-3957-441F-9733-2475D36E9028", "Reference Number"), provider.ReferenceNumber);
			}
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}

		IReadOnlyCollection<AttachedDocument> attachedDocumentsCached;
	}
}
