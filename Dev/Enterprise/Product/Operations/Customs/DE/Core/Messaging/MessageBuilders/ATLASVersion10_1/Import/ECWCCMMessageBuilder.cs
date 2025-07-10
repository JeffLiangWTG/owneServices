using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.DE.Messaging.MessageSchema.ATLASMessageSchema;
using MessageBuilderExtensions = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public sealed class ECWCCMMessageBuilder : MessageBuilder<LECWCG>
	{
		public ECWCCMMessageBuilder(IImportMessageHeader provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
			header = (IECWCCMHeader)provider.Header;
		}
		readonly IImportMessageHeader provider;
		readonly IECWCCMHeader header;

		protected override LECWCG GetMessageCore() => new LECWCG
		{
			MetaData = PopulateMetaData(),
			Header = PopulateHeader(),
			CustomsAuthorisationOwner = PopulateCustomsAuthorisationOwner(),
			Representative = PopulateRepresentative(),
			Body = PopulateBody()
		};

		LECWCGMetaData PopulateMetaData() => new LECWCGMetaData
		{
			Preparation = new LECWCGMetaDataPreparation
			{
				Date = provider.PreparationDateAndTimeCET.Date,
				Time = provider.PreparationDateAndTimeCET.Time
			},
			InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder,
			MessageReferenceNumber = "1",
			MessageIdentifier = EDIMessage.SendersReferencePlaceHolder,
			MessageGroup = provider.MessageGroup.MapCodeToEnumWithDefault<LECWCGMetaDataMessageGroup>(),
			MessageType = LECWCGMetaDataMessageType.LECWCG,
			InterchangeSender = new LECWCGMetaDataInterchangeSender
			{
				Identification = new LECWCGMetaDataInterchangeSenderIdentification
				{
					ReferenceNumber = provider.InterchangeSender.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = provider.InterchangeSender.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				}
			},
			InterchangeRecipient = new LECWCGMetaDataInterchangeRecipient
			{
				Identification = new LECWCGMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = provider.InterchangeRecipientID.LeftOrNull(InterchangeRecipientReferenceNumberMaxLength)
				}
			}
		};

		LECWCGHeader PopulateHeader() => new LECWCGHeader
		{
			MessageVersion = "G.1.0",
			MessageCreationDate = provider.PreparationDateAndTimeCET.Date,
			LRN = header.LocalReferenceNumber.LeftOrNull(LRNMaxLength),
			CustomsAuthorisation = new LECWCGHeaderCustomsAuthorisation
			{
				CurrentProcedure = header.CustomsAuthorisationCurrentProcedure.LeftOrNull(35)
			},
			RepresentativeRelationshipFlag = header.RepresentativeRelationshipFlag.MapCodeToEnumWithDefaultAndItemPrefix<LECWCGHeaderRepresentativeRelationshipFlag>(),
			AuthorisationNumber = provider.AuthorisationNumber.LeftOrNull(25)
		};

		LECWCGCustomsAuthorisationOwner PopulateCustomsAuthorisationOwner()
		{
			var customsAuthorisationOwner = header.CustomsAuthorisationOwner;
			return new LECWCGCustomsAuthorisationOwner
			{
				Identification = new LECWCGCustomsAuthorisationOwnerIdentification
				{
					ReferenceNumber = customsAuthorisationOwner?.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = customsAuthorisationOwner?.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				}
			};
		}

		LECWCGRepresentative PopulateRepresentative()
		{
			LECWCGRepresentative result = null;
			var representative = header.Representative;
			if (representative != null)
			{
				result = new LECWCGRepresentative
				{
					Identification = new LECWCGRepresentativeIdentification
					{
						ReferenceNumber = representative.EoriNumber.LeftOrNull(EoriCodeMaxLength),
						SubsidiaryNumber = representative.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
					}
				};
			}
			return result;
		}

		LECWCGGoodsItem[] PopulateBody() => header.Lines.Select(line => PopulateLine(line)).ToArray();

		LECWCGGoodsItem PopulateLine(IECWCCMLine line) =>
			new ()
			{
				SequenceNumber = line.SequenceNumber.ToString(),
				InwardMovement = MessageBuilderExtensions.CreateCommonIdentificationByRegistration<LECWCGGoodsItemInwardMovement>(line.InwardMovementRegistrationNumber, im => im.SequenceNumber = line.InwardMovementSequenceNumber.ToString()),
				OutwardMovement = new LECWCGGoodsItemOutwardMovement
				{
					CompletionType = line.OutwardMovementCompletionType,
					CompletionRegistrationNumber = line.OutwardMovementCompletionRegistrationNumber.LeftOrNull(35),
					DecisiveDate = line.OutwardMovementDecisiveDate.GetValueOrDefault(),
					Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LECWCGGoodsItemOutwardMovementAmount>(line.OutwardMovementAmount)
				}
			};
	}
}
