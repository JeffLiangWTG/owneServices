using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
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
	public class ImportSRATAXMessageProcessor : DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ISRATAX>>
	{
		public ImportSRATAXMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("6EBC522D-ED6D-4F59-B3BA-5707B787BAF1", "Import SRATAX Message Processor");

		protected override string ApplicationCodeCore => EDIInterchange.ApplicationCodes.DECustomsAtlasSystem;

		protected override bool NeedAttachDocumentsToMessage => true;

		protected override bool NeedAttachDocumentsToLinkedObject => false;

		protected override bool MustHaveLinkedObject => false;

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ISRATAX> message) => null;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject) => ZGuid.Invalid;

		protected override List<AttachedDocument> GetAttachedDocuments(AtlasInboundEDIMessage<ISRATAX> message) => message.AttachedDocuments;

		protected override ZString GetMessageIdentifier(AtlasInboundEDIMessage<ISRATAX> message) => message.DataProvider?.MessageIdentifier ?? ZString.Empty;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ISRATAX> message)
		{
			message.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK;
			var dataProvider = message.DataProvider;
			if (dataProvider != null)
			{
				var referenceNumber = dataProvider.ReferenceNumber ?? ZString.Empty;
				var mrn = dataProvider.MRN ?? ZString.Empty;
				var taxChangeAssessmentType = dataProvider.TaxChangeAssessmentType ?? ZString.Empty;
				var taxAssessmentCreationDate = dataProvider.TaxAssessmentCreationDate ?? default;
				var maturityDate = dataProvider.MaturityDate ?? default;
				CreateStmNote();
				CreateCusEntryNum();
				SendEmailIfNeeded();

				void CreateStmNote()
				{
					message.CreateStmNote(taxChangeAssessmentType, TaxChangeAssessment.Schema.TaxChangeAssessmentType);
					message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, mrn });
				}

				void CreateCusEntryNum()
				{
					var cusEntryNumber = CusEntryNumber.LoadOrCreate(message, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
					cusEntryNumber.CE_EntryNum = !referenceNumber.IsEmpty() ? referenceNumber : mrn;
					cusEntryNumber.CE_EntryStatus = TaxChangeAssessmentStatusCodeList.Codes.OPN;
					cusEntryNumber.CE_IssueDate = taxAssessmentCreationDate;
					cusEntryNumber.CE_ExpiryDate = maturityDate;
				}

				void SendEmailIfNeeded()
				{
					InterchangeRecipientEBS = dataProvider.InterchangeRecipientEBS;
					var branch = message.Branch;
					var taxChangeAssessment = factory.Load<TaxChangeAssessment>(message.PK);
					var emailSendGroupPK = GetEmailGroupPK(GetEmailGroupRegistryItem(), branch);
					if (!emailSendGroupPK.IsEmpty)
					{
						GenerateHtmlEmailAndSendToOriginalOrGroup(factory
							, taxChangeAssessment
							, Res.GetString("3A73D26B-079C-4B27-AF30-4A26D1871FE9", "Import SRATAX – Tax Change Assessment")
							, GetEmailBody()
							, isFailure: false
							, branch
							, taxChangeAssessment
							, () => string.Empty);
					}
				}

				string GetEmailBody()
				{
					var htmlBody = new StringBuilder();
					htmlBody.Append(Res.GetString("86CB540C-AF74-4A48-8709-CBE8FD29D940", "For details please follow the Link to the Job."));
					htmlBody.Append("<br /><br />");

					var tableCreator = new HtmlTableCreator();
					var taxChangeAssessmentTypeDescription = new ImportTaxChangeAssessmentTypeList().GetDescriptionFromCode(taxChangeAssessmentType);
					tableCreator.WriteRow(Res.GetString("2DB22198-67ED-4BFE-900F-78B6929E491F", "Tax Change Assessment Type"), taxChangeAssessmentTypeDescription);
					if (!mrn.IsEmpty())
					{
						tableCreator.WriteRow(Res.GetString("E80745AF-8A4D-4FAE-8524-8EC1E65ED117", "MRN"), mrn);
					}
					if (!referenceNumber.IsEmpty())
					{
						tableCreator.WriteRow(Res.GetString("65C9BE18-E514-4103-8F4E-58EED62AF8F4", "Reference Number"), referenceNumber);
					}
					var localReferenceNumber = dataProvider.LocalReferenceNumber;
					if (!localReferenceNumber.IsEmpty())
					{
						tableCreator.WriteRow(Res.GetString("800B9149-4074-4A1A-99A6-4864DA7E182E", "Local Reference Number"), localReferenceNumber);
					}
					if (taxAssessmentCreationDate != default)
					{
						tableCreator.WriteRow(Res.GetString("3315392C-F59B-4DE1-BA50-03931F7C943B", "Creation Date"), taxAssessmentCreationDate.ToString("dd.MM.yyyy"));
					}
					if (maturityDate != default)
					{
						tableCreator.WriteRow(Res.GetString("6EF6C4DE-A51B-49CB-8748-498AC75E8571", "Maturity Date"), maturityDate.ToString("dd.MM.yyyy"));
					}
					htmlBody.Append(tableCreator.ToHtml());
					return htmlBody.ToString();
				}
			}
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

		string InterchangeRecipientEBS { get; set; }
	}
}
