using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DE.Business.Import
{
	public class ImportSRAREVMessageProcessor : DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ISRAREV>>
	{
		public ImportSRAREVMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("025B1933-A1F8-46C9-8FDB-9157C05F5E04", "Import SRAREV Message Processor");

		protected override string ApplicationCodeCore => EDIInterchange.ApplicationCodes.DECustomsAtlasSystem;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject)
		{
			var result = ZGuid.Invalid;
			if (linkedObject is TaxChangeAssessment taxChangeAssessment)
			{
				result = taxChangeAssessment.EM_GB;
			}
			return result;
		}

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ISRAREV> message) => GetLinkedObjectFromMRN<TaxChangeAssessment>(message.Factory, message.DataProvider?.CancelledMRN, message.DataProvider?.CancelledReferenceNumber);

		protected override List<AttachedDocument> GetAttachedDocuments(AtlasInboundEDIMessage<ISRAREV> message) => message.AttachedDocuments;

		protected override ZString GetMessageIdentifier(AtlasInboundEDIMessage<ISRAREV> message) => message.DataProvider?.MessageIdentifier ?? ZString.Empty;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ISRAREV> message)
		{
			var linkedObject = (TaxChangeAssessment)message.EM_LinkedObject;
			message.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK;

			var dataProvider = message.DataProvider;
			var cancelledReferenceNumber = dataProvider.CancelledReferenceNumber;
			var cancelledMRN = dataProvider.CancelledMRN;
			UpdateEntryNumbersStatusAndAttachDocumentsToParents(factory, cancelledReferenceNumber, cancelledMRN, linkedObject);
			SendEmailIfNeeded(message, cancelledReferenceNumber, cancelledMRN, dataProvider.InterchangeRecipientEBS, linkedObject);
			message.SetLogbookRegistrationNumber(new ZString[] { cancelledReferenceNumber, cancelledMRN });
		}

		protected override IRegistryItem GetEmailGroupRegistryItem() => DECustomsDataRegistry.Instance.SendTaxChangeAcknowledgements;

		protected override ZString GetEmailSendMode(IGlbBranch branch) => Core.Constants.EmailTo.NominatedGroup;

		protected override ZGuid GetEmailGroupPK(IRegistryItem registryItem, IGlbBranch branch)
		{
			var result = ZGuid.Empty;
			if (registryItem is SendAcknowledgementsRegistryItem taxChangeRegistry)
			{
				result = taxChangeRegistry.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty)
					.Cast<SendAcknowledgementsRegistry>().FirstOrDefault(x => x.EBSCode == InterchangeRecipientEBS)?.SendGroupPK ?? ZGuid.Empty;
			}
			return result;
		}

		void UpdateEntryNumbersStatusAndAttachDocumentsToParents(BusinessObjectFactory factory, string cancelledReferenceNumber, string cancelledMRN, TaxChangeAssessment linkedObject)
		{
			var references = new List<ZString>();
			if (!string.IsNullOrWhiteSpace(cancelledReferenceNumber))
			{
				references.Add(cancelledReferenceNumber);
			}
			if (!string.IsNullOrWhiteSpace(cancelledMRN))
			{
				references.Add(cancelledMRN);
			}

			var cusEntryNumbers = references.Count > 0
				? CusEntryNumber.Load<CusEntryNumber>(factory, CusEntryNumberTypes.Standard.MovementReferenceNumber, references.ToArray(), Core.Constants.CountryCodes.Germany).Where(x => x.CE_ParentTable == AutoEDIMessage.Schema.TableName)
				: Array.Empty<CusEntryNumber>();
			foreach (var cusEntryNumber in cusEntryNumbers)
			{
				cusEntryNumber.CE_EntryStatus = TaxChangeAssessmentStatusCodeList.Codes.CAN;
				if (cusEntryNumber.CE_ParentID != linkedObject.PK)
				{
					SubscribeDocumentLinking(cusEntryNumber.Parent);
				}
			}
		}

		void SendEmailIfNeeded(AtlasInboundEDIMessage<ISRAREV> message, string cancelledReferenceNumber, string cancelledMRN, string interchangeRecipientEBS, TaxChangeAssessment linkedObject)
		{
			InterchangeRecipientEBS = interchangeRecipientEBS;
			var branch = message.Branch;
			var emailSendGroupPK = GetEmailGroupPK(GetEmailGroupRegistryItem(), branch);
			if (!emailSendGroupPK.IsEmpty)
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
					, linkedObject
					, Res.GetString("40AC40BB-2521-433D-A800-C236D2BE833D", "Import SRAREV – Reversal Tax Change Assessment")
					, GetEmailBody()
					, isFailure: false
					, branch
					, linkedObject
					, () => string.Empty);
			}

			string GetEmailBody()
			{
				var htmlBody = new StringBuilder();
				htmlBody.Append(Res.GetString("F5748957-1EAD-4B88-B93A-2BE47397F8B0", "For details please follow the Link to the Job."));
				htmlBody.Append("<br /><br />");

				var tableCreator = new HtmlTableCreator();
				if (!string.IsNullOrWhiteSpace(cancelledReferenceNumber))
				{
					tableCreator.WriteRow(Res.GetString("8B1A8929-6540-4A13-AEA1-38D31FED61C6", "Canceled Reference Number"), cancelledReferenceNumber);
				}
				if (!string.IsNullOrWhiteSpace(cancelledMRN))
				{
					tableCreator.WriteRow(Res.GetString("4246C696-11CD-4ABA-B00B-F3A8C68A4414", "Canceled MRN"), cancelledMRN);
				}
				htmlBody.Append(tableCreator.ToHtml());
				return htmlBody.ToString();
			}
		}

		string InterchangeRecipientEBS { get; set; }
	}
}
