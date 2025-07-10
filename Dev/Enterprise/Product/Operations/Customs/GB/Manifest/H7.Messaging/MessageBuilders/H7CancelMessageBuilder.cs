using System.Collections.Generic;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.H7.Business;
using static Enterprise.Customs.GB.CDS.Constants;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public class H7CancelMessageBuilder : H7ArrivalAndCancelBaseMessageBuilder, IGbCDSMessageBuilder
	{
		public H7CancelMessageBuilder(MessageSendingObject objectToSend, string functionCode)
			: base(objectToSend, functionCode)
		{
		}

		public override ZString Build()
		{
			ZString messageText = string.Empty;

			var metaData = CreateMetaData();
			var metaDeclaration = CreateMetaDeclaration();

			var result = metaData
				.SetDeclaration(metaDeclaration)
				.Serialize();

			return XmlMessageHelper.RemoveEmptyXmlElements(result);
		}

		protected override ZString TypeCode => Constants.ThreeCharFunctionCodes.DeclarationCancelled;

		protected override IEnumerable<DeclarationAmendment> Amendments
		{
			get
			{
				yield return new DeclarationAmendment
				{
					ChangeReasonCode = new AmendmentChangeReasonCodeType
					{
						Value = ObjectToSend.AmendmentReasonCode,
					}
				};
			}
		}

		protected override DeclarationAdditionalInformation[] AdditionalInformation =>
			new[]
			{
				new DeclarationAdditionalInformation
				{
					StatementDescription = new AdditionalInformationStatementDescriptionTextType
					{
						Value = ObjectToSend.AmendmentInvalidationReason,
					},
					StatementTypeCode = new AdditionalInformationStatementTypeCodeType
					{
						Value = StatementTypeCodes.AES
					},
					Pointer =
					[
						new DeclarationAdditionalInformationPointer
						{
							SequenceNumeric = 1,
							SequenceNumericSpecified = true,
							DocumentSectionCode = new PointerDocumentSectionCodeType { Value = "42A" }
						},
						new DeclarationAdditionalInformationPointer
						{
							SequenceNumeric = 1,
							SequenceNumericSpecified = true,
							DocumentSectionCode = new PointerDocumentSectionCodeType { Value = "06A" }
						}
					]
				}
			};
	}
}
