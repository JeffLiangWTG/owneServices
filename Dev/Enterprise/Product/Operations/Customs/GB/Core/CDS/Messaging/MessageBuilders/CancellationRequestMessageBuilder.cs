using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using static Enterprise.Customs.GB.CDS.Constants;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class CancellationRequestMessageBuilder : IGbCDSMessageBuilder
	{
		protected readonly JobDeclarationMessageSendingObject ObjectToSend;
		protected readonly string functionCode;

		public CancellationRequestMessageBuilder(JobDeclarationMessageSendingObject objectToSend, string functionCode)
		{
			this.ObjectToSend = objectToSend;
			this.functionCode = functionCode;
		}

		public virtual ZString Build()
		{
			var metaData = CreateMetaData();
			var metaDeclaration = CreateMetaDeclaration();

			var result = metaData
				.SetDeclaration(metaDeclaration)
				.Serialize();

			return XmlMessageHelper.RemoveEmptyXmlElements(result);
		}

		protected virtual ZString TypeCode => Constants.ThreeCharFunctionCodes.DeclarationCancelled;

		protected virtual IEnumerable<DeclarationAmendment> Amendments
		{
			get
			{
				yield return new DeclarationAmendment
				{
					ChangeReasonCode = new AmendmentChangeReasonCodeType
					{
						Value = ObjectToSend.ChangeAcknowledgementIndicator
					}
				};
			}
		}

		protected MetaData CreateMetaData()
		{
			return new MetaData
			{
				WCODataModelVersionCode = new MetaDataWCODataModelVersionCodeType
				{
					Value = "3.6"
				},
				WCOTypeName = new MetaDataWCOTypeNameTextType
				{
					Value = "DEC"
				},
				ResponsibleCountryCode = new MetaDataResponsibleCountryCodeType
				{
					Value = "GB"
				},
				ResponsibleAgencyName = new MetaDataResponsibleAgencyNameTextType
				{
					Value = "HMRC"
				},
				AgencyAssignedCustomizationVersionCode = new MetaDataAgencyAssignedCustomizationVersionCodeType
				{
					Value = "v2.1"
				}
			};
		}

		protected virtual CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration CreateMetaDeclaration()
		{
			return new CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration
			{
				FunctionCode = new DeclarationFunctionCodeType
				{
					Value = functionCode
				},
				FunctionalReferenceID = new DeclarationFunctionalReferenceIDType
				{
					Value = CusEntryHeader.LRNReferencePlaceHolderXmlFriendly
				},
				ID = new DeclarationIdentificationIDType
				{
					Value = ObjectToSend.Header.MovementReferenceNumber
				},
				TypeCode = new DeclarationTypeCodeType
				{
					Value = TypeCode
				},
				AdditionalInformation = CreateAdditionalInformation(),
				Amendment = Amendments.ToArray()
			};
		}

		protected virtual DeclarationAdditionalInformation[] CreateAdditionalInformation()
		{
			return new[]
			{
				new DeclarationAdditionalInformation
				{
					StatementDescription = new AdditionalInformationStatementDescriptionTextType
					{
						Value = ObjectToSend.VOCReason
					},
					StatementTypeCode = new AdditionalInformationStatementTypeCodeType
					{
						Value = StatementTypeCodes.AES
					},
					Pointer = new DeclarationAdditionalInformationPointer[]
					{
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
					}
				}
			};
		}
	}
}
