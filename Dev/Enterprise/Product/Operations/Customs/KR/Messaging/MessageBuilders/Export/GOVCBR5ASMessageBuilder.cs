using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AS;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[MessageType(ElectronicDocumentTypeList.Codes._5AS)]
	public class GOVCBR5ASMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IExportAmendmentHeader dataHeaderProvider;
		readonly IAmendmentDetails dataAmendProvider;
		readonly MessageFunctions.MessageFunctionCode messageFunctionCode;
		public GOVCBR5ASMessageBuilder(IExportAmendmentHeader dataHeaderProvider, IAmendmentDetails dataAmendProvider, MessageFunctions.MessageFunctionCode functionCode)
		{
			this.dataHeaderProvider = dataHeaderProvider;
			this.dataAmendProvider = dataAmendProvider;
			messageFunctionCode = functionCode;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				AcceptanceDateTime = PopulateAcceptanceDateTime(messageFunctionCode),
				TypeCode = PopulateDeclarationTypeCodeType(),
				VersionId = PopulateDeclarationVersionIDType(),
				TransactionNatureCode = PopulateDeclarationTransactionNatureCodeType(messageFunctionCode),
				Reason = PopulateDeclarationReasonTextType(),
				AdditionalInformation = PopulateDeclarationAdditionalInformation(),
				Consignment = PopulateDeclarationConsignment(),
				Exporter = PopulateDeclarationExporter(),
				Submitter = PopulateDeclarationSubmitter(),
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime()
			};
		}

		string PopulateAcceptanceDateTime(MessageFunctions.MessageFunctionCode messageFunctionCode)
		{
			string result = "";
			if (messageFunctionCode == MessageFunctions.MessageFunctionCode.Amendment)
			{
				result = (dataAmendProvider.DateOfFinalPrice == ZDate.Invalid || dataAmendProvider.DateOfFinalPrice.IsEmpty) ? null : dataAmendProvider.DateOfFinalPrice.ToString(DateFormatType.Date);
			}
			return result;
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataHeaderProvider.DeclarationCustomsOffice + dataHeaderProvider.DeclarationCustomsDivision };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataHeaderProvider.ExportDeclarationNumber };
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateDeclarationTypeCodeType()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5AS };
		}

		DeclarationVersionIdType PopulateDeclarationVersionIDType()
		{
			return new DeclarationVersionIdType { Value = dataAmendProvider.AmendmentVersionNo.ToString() };
		}

		DeclarationTransactionNatureCodeType PopulateDeclarationTransactionNatureCodeType(MessageFunctions.MessageFunctionCode messageFunctionCode)
		{
			ZString msgFunctionCode = string.Empty;
			switch (messageFunctionCode)
			{
				case MessageFunctions.MessageFunctionCode.Amendment:
					msgFunctionCode = _5ASAmendmentType.Codes.Amendment;
					break;
				case MessageFunctions.MessageFunctionCode.Extend:
					msgFunctionCode = _5ASAmendmentType.Codes.Extension;
					break;
			}
			return new DeclarationTransactionNatureCodeType
			{
				Value = msgFunctionCode
			};
		}

		DeclarationReasonTextType PopulateDeclarationReasonTextType()
		{
			return new DeclarationReasonTextType { Value = dataAmendProvider.FaultParty };
		}

		DeclarationAdditionalInformation PopulateDeclarationAdditionalInformation()
		{
			return new DeclarationAdditionalInformation
			{
				StatementCode = new AdditionalInformationStatementCodeType { Value = dataAmendProvider.ReasonCode },
				StatementDescription = new AdditionalInformationStatementDescriptionTextType { Value = dataAmendProvider.AmendReasonDescription }
			};
		}

		Collection<DeclarationConsignment> PopulateDeclarationConsignment()
		{
			if (!dataHeaderProvider.AmendmentItems.Any())
			{
				return null;
			}

			var amendmentItems = new Collection<DeclarationConsignment>();
			foreach (var amendmentItem in dataHeaderProvider.AmendmentItems)
			{
				var item = new DeclarationConsignment();

				if (!amendmentItem.RegulationCategorySequnceNo.IsEmpty)
				{
					item.AdditionalDocument = new DeclarationConsignmentAdditionalDocument
					{
						CriteriaConformanceId = new AdditionalDocumentCriteriaConformanceIdentificationTextType { Value = amendmentItem.RegulationCategorySequnceNo }
					};
				}

				item.Amendment = new DeclarationConsignmentAmendment
				{
					ChangeReasonCode = amendmentItem.LineAmendType.IsEmpty ? null : new AmendmentChangeReasonCodeType { Value = amendmentItem.LineAmendType },
					StatementDescription = amendmentItem.BeforeDescription.IsEmpty ? null : new AmendmentStatementDescriptionTextType { Value = amendmentItem.BeforeDescription },
					AdjustmentDescription = amendmentItem.AfterDescription.IsEmpty ? null : new AmendmentAdjustmentDescriptionTextType { Value = amendmentItem.AfterDescription },
					Pointer = new DeclarationConsignmentAmendmentPointer
					{
						SequenceNumeric = new PointerSequenceTextType { Value = amendmentItem.EntryLineNo },
						DocumentSectionCode = new PointerDocumentSectionTextType { Value = amendmentItem.LineDetailNo },
						TagId = new PointerTagIdType { Value = amendmentItem.AmendDataItemID }
					}
				};

				if (!amendmentItem.VINSequenceNo.IsEmpty)
				{
					item.ConsignmentItem = new DeclarationConsignmentConsignmentItem
					{
						Commodity = new DeclarationConsignmentConsignmentItemCommodity
						{
							Id = new CommodityIdentificationTextType { Value = amendmentItem.VINSequenceNo }
						}
					};
				}

				if (!amendmentItem.ContainerSequenceNo.IsEmpty)
				{
					item.TransportEquipment = new DeclarationConsignmentTransportEquipment
					{
						Id = new TransportEquipmentIdentificationTextType { Value = amendmentItem.ContainerSequenceNo }
					};
				}

				amendmentItems.Add(item);
			}
			return amendmentItems;
		}

		DeclarationExporter PopulateDeclarationExporter()
		{
			var matchedNumber = dataHeaderProvider.Exporter?.GetRegistrationTypeAndNumber(IdentificationType.UnipassIDForOrganization);
			return new DeclarationExporter
			{
				Id = string.IsNullOrEmpty(matchedNumber?.Number) ? null : new ExporterIdentificationIdType { Value = matchedNumber.Number },
				Name = new ExporterNameTextType { Value = dataHeaderProvider.Exporter?.CompanyName ?? "" }
			};
		}

		DeclarationSubmitter PopulateDeclarationSubmitter()
		{
			if (!dataHeaderProvider.UnipassDeclarantID.IsEmpty)
			{
				return new DeclarationSubmitter
				{
					Id = new SubmitterIdentificationIdType { Value = dataHeaderProvider.UnipassDeclarantID },
				};
			}
			return null;
		}
	}
}
