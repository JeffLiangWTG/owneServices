using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Import
{
	public class ImportCWSINFMessageProcessor : ImportMessageProcessor<AtlasInboundEDIMessage<ICWSINF>, ICWSINF>
	{
		public ImportCWSINFMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("47854830-5734-46bd-a570-9740045ce402", "Import CWSINF Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICWSINF> message) => null;

		protected override bool MustHaveLinkedObject => false;

		protected override bool NeedAttachDocumentsToLinkedObject => false;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICWSINF> message)
		{
			var dataProvider = message.DataProvider;
			if (dataProvider == null)
			{
				message.EM_Status = EDIMessageStatusList.Codes.Error;
			}
			else
			{
				message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
				SendEmail(message);
				message.SetLogbookLocalReferenceNumber(dataProvider.CurrentProcedure);
			}
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var result = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			AttachDocumentsToEmail(result, attachedDocumentsCached);
			return result;
		}

		void SendEmail(AtlasInboundEDIMessage<ICWSINF> message)
		{
			attachedDocumentsCached = message.AttachedDocuments;

			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, null
				, Res.GetString("ec890d4a-4a33-4e88-b73e-dddedc357c62", "Import CWSINF – Stock Information bonded warehouse")
				, Res.GetString("ff981dca-3580-4054-b054-1c8ae9b8cf24", "For details please open the attached report.")
				, false
				, message.Branch
				, null
				, () => ZString.Empty);
		}

		IReadOnlyCollection<AttachedDocument> attachedDocumentsCached;
	}
}
