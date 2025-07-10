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
	public sealed class CUSWATMessageBuilder : MessageBuilder<LCUSWK>
	{
		public CUSWATMessageBuilder(IImportMessageHeader provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
			header = (ICUSWATHeader)provider.Header;
		}
		readonly IImportMessageHeader provider;
		readonly ICUSWATHeader header;

		protected override LCUSWK GetMessageCore() => new LCUSWK
		{
			MetaData = PopulateMetaData(),
			Header = PopulateHeader(),
			WarehouseOwner = PopulateWarehouseOwner(),
			ContactPerson = PopulateContactPerson(),
			DepartureCustomsWarehouseSupervisingCustomsOffice = PopulateDepartureCustomsWarehouseSupervisingCustomsOffice(),
			Body = PopulateBody(),
		};

		LCUSWKMetaData PopulateMetaData() => new LCUSWKMetaData
		{
			Preparation = new LCUSWKMetaDataPreparation
			{
				Date = provider.PreparationDateAndTimeCET.Date,
				Time = provider.PreparationDateAndTimeCET.Time,
			},
			InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder,
			MessageReferenceNumber = "1",
			MessageIdentifier = EDIMessage.SendersReferencePlaceHolder,
			MessageGroup = LCUSWKMetaDataMessageGroup.LUE,
			MessageType = LCUSWKMetaDataMessageType.LCUSWK,
			InterchangeSender = new LCUSWKMetaDataInterchangeSender
			{
				Identification = new LCUSWKMetaDataInterchangeSenderIdentification
				{
					ReferenceNumber = provider.InterchangeSender.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = provider.InterchangeSender.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength),
				},
			},
			InterchangeRecipient = new LCUSWKMetaDataInterchangeRecipient
			{
				Identification = new LCUSWKMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = provider.InterchangeRecipientID.LeftOrNull(InterchangeRecipientReferenceNumberMaxLength),
				},
			},
		};

		LCUSWKHeader PopulateHeader() => new LCUSWKHeader
		{
			MessageVersion = "K.1.0",
			MessageCreationDate = provider.PreparationDateAndTimeCET.Date,
			LRN = header.LocalReferenceNumber.LeftOrNull(LRNMaxLength),
			CustomsWarehouseDepartureLocalReferenceNumber = header.CustomsWarehouseDepartureLocalReferenceNumber,
			CustomsAuthorisation = new LCUSWKHeaderCustomsAuthorisation
			{
				CustomsWarehouseDeparture = header.CustomsWarehouseDeparture,
				CurrentProcedure = header.CurrentProcedure,
			},
			CurrencyCode = LCUSWKHeaderCurrencyCode.EUR,
			DeclarationPlace = header.DeclarationPlace,
			AuthorisationNumber = provider.AuthorisationNumber,
		};

		LCUSWKWarehouseOwner PopulateWarehouseOwner()
		{
			var warehouseOwner = header.WarehouseOwner;
			return warehouseOwner.EoriNumberIsEmpty() ? null : new LCUSWKWarehouseOwner
			{
				Identification = new LCUSWKWarehouseOwnerIdentification
				{
					ReferenceNumber = warehouseOwner.EoriNumber,
					SubsidiaryNumber = warehouseOwner.EoriBranchSuffix,
				},
			};
		}

		LCUSWKContactPerson PopulateContactPerson() => new LCUSWKContactPerson
		{
			Name = ContactPerson.PersonName,
			Position = ContactPerson.Position,
			PhoneNumber = ContactPerson.PhoneNumber,
			MailAddress = ContactPerson.MailAddress,
		};

		LCUSWKDepartureCustomsWarehouseSupervisingCustomsOffice PopulateDepartureCustomsWarehouseSupervisingCustomsOffice()
		{
			var referenceNumber = header.DepartureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber;
			return referenceNumber.IsEmpty() ? null : new LCUSWKDepartureCustomsWarehouseSupervisingCustomsOffice
			{
				ReferenceNumber = referenceNumber,
			};
		}

		LCUSWKBody PopulateBody()
		{
			return new LCUSWKBody
			{
				Document = header.Documents.Select(PopulateDocument).ToArray(),
				GoodsItem = header.Lines.Select(PopulateLine).ToArray(),
			};
		}

		LCUSWKBodyDocument PopulateDocument(IImportDocument d)
		{
			return new LCUSWKBodyDocument
			{
				Division = LCUSWKBodyDocumentDivision.Item4,
				Type = d.Type,
				ReferenceNumber = d.ReferenceNumber,
				IssuingDate = d.IssuingDate.GetValueOrDefault(),
			};
		}

		LCUSWKBodyGoodsItem PopulateLine(ICUSWATLine line)
		{
			return new LCUSWKBodyGoodsItem
			{
				SequenceNumber = line.SequenceNumber.ToString(),
				Procedure = new LCUSWKBodyGoodsItemProcedure
				{
					RequestedPreviousProcedure = LCUSWKBodyGoodsItemProcedureRequestedPreviousProcedure.Item7171,
				},
				GoodsDescription = line.GoodsDescription,
				ArticleNumber = line.ArticleNumber,
				NetMassMeasure = line.NetMassMeasure.RoundAndNormalize(1),
				OriginCountry = line.OriginCountry,
				DepartureCountry = line.DepartureCountry,
				SupplementaryInformation = line.SupplementaryInformation,
				DecisiveDate = line.DecisiveDate.GetValueOrDefault(),
				CommodityCode = new LCUSWKBodyGoodsItemCommodityCode
				{
					CommodityCode = line.CommodityCode,
				},
				AdditionalProcedure = line.AdditionalProcedure.Select(p => PopulateBodyGoodsItemAdditionalProcedure(p)).ToArray(),
				SupplementaryCodes = line.SupplementaryCodes.Select(p => PopulateBodyGoodsItemSupplementaryCodes(p)).ToArray(),
				Containers = new LCUSWKBodyGoodsItemContainers
				{
					ContainerFlag = line.ContainerFlag.MapBoolToJN(),
					Container = line.ContainerIdentificationNumbers.Select(PopulateBodyGoodsItemContainersContainer).ToArray(),
				},
				Package = line.Package != null ? PopulateBodyGoodsItemPackage(line.Package) : null,
				InwardMovement = new LCUSWKBodyGoodsItemInwardMovement()
				{
					Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LCUSWKBodyGoodsItemInwardMovementAmount>(line.InwardMovementAmount),
				},
				Assessment = new LCUSWKBodyGoodsItemAssessment
				{
					CustomsValue = line.AssessmentCustomsValue.RoundAndNormalize(2),
					CustomsValueSpecified = !line.AssessmentCustomsValue.IsZero(),
					Amount = line.AssessmentAmount.Select(CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LCUSWKBodyGoodsItemAssessmentAmount>).ToArray(),
					SpecificRate = line.AssessmentSpecificRate.Select(PopulateBodyGoodsItemAssessmentSpecificRate).ToArray(),
					ContentInformation = line.AssessmentContentInformation.Select(PopulateBodyGoodsItemAssessmentContentInformation).ToArray(),
				},
				ExciseDuty = line.ExciseDuty.Select(PopulateBodyGoodsItemExciseDuty).ToArray(),
				PreferentialTreatment = new LCUSWKBodyGoodsItemPreferentialTreatment
				{
					RequestedPreferentialTreatment = line.RequestedPreferentialTreatment,
				},
				Document = line.Documents.Select(PopulateBodyGoodsItemDocument).ToArray(),
				InwardMovementDepartureCustomsWarehouse = MessageBuilderExtensions.CreateCommonIdentificationByRegistration<LCUSWKBodyGoodsItemInwardMovementDepartureCustomsWarehouse>(line.InwardMovementDepartureCustomsWarehouseReferenceNumber,
					a =>
					{
						a.SequenceNumber = line.InwardMovementDepartureCustomsWarehouseSequenceNumber.ToString();
						a.AccessViaAtlasFlag = line.InwardMovementDepartureCustomsWarehouseAccessViaAtlasFlag.MapBoolToJN();
						a.UsualProcessingFlag = line.InwardMovementDepartureCustomsWarehouseUsualProcessingFlag.MapBoolToJN();
						a.AdditionalInformation = line.InwardMovementDepartureCustomsWarehouseAdditionalInformation;
						a.DebitAmount = line.InwardMovementDepartureCustomsWarehouseDebitAmount.CreateCommonAmount<LCUSWKBodyGoodsItemInwardMovementDepartureCustomsWarehouseDebitAmount>();
					}),
			};
		}

		LCUSWKBodyGoodsItemDocument PopulateBodyGoodsItemDocument(IImportLineDocument document)
		{
			return new LCUSWKBodyGoodsItemDocument
			{
				Division = document.Division,
				Type = document.DocumentType,
				ReferenceNumber = document.ReferenceNumber,
				AtHandFlag = document.AtHandFlag,
				IssuingDate = document.IssuingDate.GetValueOrDefault(),
				IssuingDateSpecified = document.IssuingDate.HasValue,
				WriteOff = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LCUSWKBodyGoodsItemDocumentWriteOff>(document.WriteOff),
			};
		}

		LCUSWKBodyGoodsItemExciseDuty PopulateBodyGoodsItemExciseDuty(IExciseDuty duty)
		{
			return new LCUSWKBodyGoodsItemExciseDuty
			{
				Code = duty.Code,
				Amount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<LCUSWKBodyGoodsItemExciseDutyAmount>(duty.Amount),
				DegreePercentage = duty.DegreePercentage.RoundAndNormalize(2),
				DegreePercentageSpecified = !duty.DegreePercentage.IsZero(),
				Value = duty.Value.RoundAndNormalize(2),
				ValueSpecified = !duty.Value.IsZero(),
			};
		}

		LCUSWKBodyGoodsItemAssessmentContentInformation PopulateBodyGoodsItemAssessmentContentInformation(IContentInformation info)
		{
			return new LCUSWKBodyGoodsItemAssessmentContentInformation
			{
				Type = info.ContentType,
				DegreePercentage = info.DegreePercentage.Round(2).Normalize(),
			};
		}

		LCUSWKBodyGoodsItemAssessmentSpecificRate PopulateBodyGoodsItemAssessmentSpecificRate(IImportSpecificRate specificRate)
		{
			return new LCUSWKBodyGoodsItemAssessmentSpecificRate
			{
				Type = specificRate.Type,
				Value = specificRate.Value.RoundAndNormalize(2),
			};
		}

		LCUSWKBodyGoodsItemPackage PopulateBodyGoodsItemPackage(IImportPackage package)
		{
			var result = new LCUSWKBodyGoodsItemPackage { Kind = package.Kind };
			if (package.Quantity.HasValue)
			{
				result.Quantity = package.Quantity.ToString();
				result.MarksNumbers = package.MarksNumbers;
			}
			return result;
		}

		LCUSWKBodyGoodsItemContainersContainer PopulateBodyGoodsItemContainersContainer(string id) => new LCUSWKBodyGoodsItemContainersContainer { IdentificationNumber = id };

		LCUSWKBodyGoodsItemAdditionalProcedure PopulateBodyGoodsItemAdditionalProcedure(string code) => new LCUSWKBodyGoodsItemAdditionalProcedure { Code = code };

		LCUSWKBodyGoodsItemSupplementaryCodes PopulateBodyGoodsItemSupplementaryCodes(string code) => new LCUSWKBodyGoodsItemSupplementaryCodes { Code = code };

		IImportPartyContactPerson ContactPerson => header.ContactPerson;
	}
}
