using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSDocumentUploadConfirmationMessageProcessor : CDSMessageProcessor<CDSDocumentUploadConfirmationResponse>
	{
		public CDSDocumentUploadConfirmationMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "CDS Document Upload Confirmation";

		protected override ZString MessageType => CDSEDIMessageTypeList.Codes.DocumentUploadConfirmation;

		protected override ZString ProcessMessageCore(CDSDocumentUploadConfirmationResponse cdsEDIMessage, BusinessObjectFactory factory)
		{
			if (ZGuid.TryParse(cdsEDIMessage.EM_ApplicationReference, out ZGuid eHubTrackingID))
			{
				var query = new ZQuery();
				query.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, eHubTrackingID);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				var originalOutgoingEdiInterchange = factory.LoadTop1<EDIInterchange>(query);
				var outgoingEdiMessage = originalOutgoingEdiInterchange?.ContainedMessages[0];
				var linkedObject = outgoingEdiMessage?.EM_LinkedObject;
				if (linkedObject is IMessageAttachee messageAttachee)
				{
					var jobNumber = messageAttachee.JobNumber;
					var fileName = cdsEDIMessage.MessageDataObject.FileName;
					var isSuccess = cdsEDIMessage.MessageDataObject.IsSuccess;
					var details = cdsEDIMessage.MessageDataObject.Details;
					_ = messageAttachee.Logs.AddNew(isSuccess ? AutoEvents.DocumentDelivered : AutoEvents.DocumentUnallocated, GetNewEventReference(isSuccess, jobNumber, fileName, details));
					cdsEDIMessage.EM_LinkedObject = linkedObject;
					if (linkedObject is JobDeclaration)
					{
						var emailOfUserWhoUploadedDoc = outgoingEdiMessage?.UserWhoQueuedThisRecord?.GS_EmailAddress;
						if (!string.IsNullOrEmpty(emailOfUserWhoUploadedDoc))
						{
							Env.OutgoingCustomsMailManager.CreateAndSaveSimple(GetEmailSubject(jobNumber, fileName), GetEmailBody(isSuccess, jobNumber, fileName, details), emailOfUserWhoUploadedDoc);
						}
					}
				}
			}
			return EDIMessage.Status.ProcessedOK;
		}

		string GetNewEventReference(bool success, string jobNumber, string fileName, string details)
		{
			return $"{(success ? delivered : failure)} {fileName}|Job Number: {jobNumber}|Details: {details}".LeftEmptyIfNull(StmALog.Schema.SL_ReferenceMaxLength);
		}

		string GetEmailSubject(string jobNumber, string fileName)
		{
			return $"Document Upload Status ({fileName})|Job Number: {jobNumber}";
		}

		string GetEmailBody(bool success, string jobNumber, string fileName, string details)
		{
			return $"{(success ? delivered : failure)} {fileName}{System.Environment.NewLine}Job Number: {jobNumber}{System.Environment.NewLine}Details: {details}";
		}

		readonly string delivered = "Delivered successfully to CDS:";
		readonly string failure = "Failure to deliver to CDS:";
	}
}
