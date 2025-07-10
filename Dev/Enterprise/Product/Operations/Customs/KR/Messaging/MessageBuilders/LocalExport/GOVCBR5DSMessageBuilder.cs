using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DS;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[MessageType(ElectronicDocumentTypeList.Codes._5DS)]
	public class GOVCBR5DSMessageBuilder : MessageBuilder<Declaration>
	{
		readonly ILocalExportAmendEntryHeader dataProvider;
		readonly IAmendmentDetails amendmentDetails;
		readonly MessageFunctions.MessageFunctionCode messageType;
		public GOVCBR5DSMessageBuilder(ILocalExportAmendEntryHeader dataProvider, IAmendmentDetails amendmentDetails, MessageFunctions.MessageFunctionCode messageType)
		{
			this.dataProvider = dataProvider;
			this.amendmentDetails = amendmentDetails;
			this.messageType = messageType;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				TypeCode = PopulateTypeCode(),
				TransactionNatureCode = PopulateTransactionNatureCode(),
				Reason = PopulateReason(),
				ReasonCode = PopulateReasonCode(),
				AdditionalDocument = PopulateAdditionalDocument(),
				Amendment = PopulateAmendment(),
				Submitter = PopulateSubmitter(),
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOfficeAndDivision };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.DeclarationNumber };
		}

		string PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5DS };
		}

		DeclarationTransactionNatureCodeType PopulateTransactionNatureCode()
		{
			return new DeclarationTransactionNatureCodeType
			{
				Value = LocalExportAmendmentTypeList.GetAmendmentType(messageType)
			};
		}

		DeclarationReasonTextType PopulateReason()
		{
			return amendmentDetails.AmendReasonDescription.IsEmpty ? null : new DeclarationReasonTextType { Value = amendmentDetails.AmendReasonDescription };
		}

		DeclarationReasonCodeType PopulateReasonCode()
		{
			return new DeclarationReasonCodeType { Value = amendmentDetails.ReasonCode };
		}

		DeclarationAdditionalDocument PopulateAdditionalDocument()
		{
			return new DeclarationAdditionalDocument
			{
				Id = new AdditionalDocumentIdentificationIdType { Value = dataProvider.CustomsReceiptNumber }
			};
		}

		Collection<DeclarationAmendment> PopulateAmendment()
		{
			if (dataProvider.AmendedItems == null || !dataProvider.AmendedItems.Any())
			{
				return null;
			}

			var amendedItems = new Collection<DeclarationAmendment>();
			foreach (var amendedItem in dataProvider.AmendedItems)
			{
				var item = new DeclarationAmendment
				{
					ChangeReasonCode = new AmendmentChangeReasonCodeType { Value = amendedItem.AmendType },
					StatementDescription = amendedItem.BeforeValue.IsEmpty ? null : new AmendmentStatementDescriptionTextType { Value = amendedItem.BeforeValue },
					AdjustmentDescription = amendedItem.AfterValue.IsEmpty ? null : new AmendmentAdjustmentDescriptionTextType { Value = amendedItem.AfterValue },
					Pointer = new DeclarationAmendmentPointer
					{
						SequenceNumeric = amendedItem.ItemSequenceNumber,
						TagId = amendedItem.DataItemNo.IsEmpty ? null : new PointerTagIdType { Value = amendedItem.DataItemNo }
					}
				};
				amendedItems.Add(item);
			}
			return amendedItems;
		}

		Collection<SubmitterIdentificationIdType> PopulateSubmitter()
		{
			var result = new Collection<SubmitterIdentificationIdType>();
			var unipassID = dataProvider.Supplier?.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization);
			var businessRegNo = dataProvider.Supplier?.GetRegistrationNumber(IdentificationType.BusinessRegNo);

			if (!unipassID.IsEmpty())
			{
				var idNumber = new SubmitterIdentificationIdType
				{
					SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
					Value = dataProvider.Supplier.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization)
				};
				result.Add(idNumber);
			}
			if (!businessRegNo.IsEmpty())
			{
				var idNumber = new SubmitterIdentificationIdType
				{
					SchemeAgencyId = AgencyIdentificationCodeContentType.Ktx,
					Value = dataProvider.Supplier.GetRegistrationNumber(IdentificationType.BusinessRegNo)
				};
				result.Add(idNumber);
			}
			return result;
		}
	}
}





