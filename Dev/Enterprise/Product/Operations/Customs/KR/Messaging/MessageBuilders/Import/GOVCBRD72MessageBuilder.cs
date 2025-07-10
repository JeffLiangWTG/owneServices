using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRD72;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._D72)]
	public class GOVCBRD72MessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImportD72Header dataProvider;
		public GOVCBRD72MessageBuilder(IImportD72Header dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				SubsequentDeclarationOfficeId = PopulateSubsequentDeclarationOfficeID(),
				TypeCode = PopulateTypeCode(),
				VersionId = PopulateVersionID(),
				Reason = PopulateReason(),
				AdditionalInformation = PopulateAdditionalInformation(),
				GoodsShipment = PopulateGoodsShipment(),
				PreviousDocument = PopulatePreviousDocument(),
				Submitter = PopulateSubmitter()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ImportDeclarationNumber };
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationSubsequentDeclarationOfficeIdType PopulateSubsequentDeclarationOfficeID()
		{
			return dataProvider.DeclarationCustomsDivision.IsEmpty ? null : new DeclarationSubsequentDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsDivision };
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._D72 };
		}

		DeclarationVersionIdType PopulateVersionID()
		{
			return new DeclarationVersionIdType { Value = dataProvider.SequenceNo.ToString() };
		}

		DeclarationReasonTextType PopulateReason()
		{
			return new DeclarationReasonTextType { Value = dataProvider.ReasonDescription };
		}

		DeclarationAdditionalInformation PopulateAdditionalInformation()
		{
			return new DeclarationAdditionalInformation
			{
				LimitDateTime = dataProvider.AfterReExportScheduledDate.IsValid ? dataProvider.AfterReExportScheduledDate.ToString(DateFormatType.Date) : string.Empty,
			};
		}

		Collection<DeclarationGoodsShipment> PopulateGoodsShipment()
		{
			var entryLines = new Collection<DeclarationGoodsShipment>();
			foreach (var entryLine in dataProvider.Lines)
			{
				var item = new DeclarationGoodsShipment
				{
					GovernmentAgencyGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
					{
						SequenceNumeric = entryLine.EntryLineNo,
						AdditionalInformation = entryLine.Remark.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation
						{
							Content = new AdditionalInformationContentTextType { Value = entryLine.Remark }
						},
						Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
						{
							CargoDescription = new CommodityCargoDescriptionTextType { Value = entryLine.HSDescription },
							CountQuantity = new CommodityCountQuantityType { Value = entryLine.Quantity, KcsUnitCode = entryLine.QuantityUnit },

							Description = new CommodityDescriptionTextType { Value = entryLine.ItemDescription },

							IdentityQualifierCode = new CommodityIdentityQualifierCodeType { Value = entryLine.DetailLineNo.ToString() },

							ValueAmount = new CommodityValueAmountType
							{
								Value = entryLine.Amount,
								CurrencyId = Enum.TryParse(entryLine.AmountCurrency, true, out Iso3AlphaCurrencyCodeContentType result) ? result : new Iso3AlphaCurrencyCodeContentType?(),
							}
						}
					}
				};
				entryLines.Add(item);
			}
			return entryLines;
		}

		DeclarationPreviousDocument PopulatePreviousDocument()
		{
			return new DeclarationPreviousDocument { IssueDateTime = dataProvider.BeforeReExportScheduledDate.IsValid ? dataProvider.BeforeReExportScheduledDate.ToString(DateFormatType.Date) : string.Empty };
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			var numberAndType = dataProvider.Declarant?.GetBusinessOrIndividualRegistrationNumber();
			return new DeclarationSubmitter
			{
				Id = new SubmitterIdentificationIdType { Value = numberAndType?.Number ?? ZString.Empty },
				Name = (dataProvider.Declarant?.CompanyName ?? ZString.Empty).IsEmpty ? null : new SubmitterNameTextType { Value = dataProvider.Declarant.CompanyName },
				RoleCode = new SubmitterRoleCodeType { Value = numberAndType?.Type ?? ZString.Empty },
				TypeCode = new SubmitterTypeCodeType { Value = dataProvider.DeclarantType },
				Address = new DeclarationSubmitterAddress
				{
					CountrySubDivisionId = (dataProvider.Declarant?.RoadNameCode ?? ZString.Empty).IsEmpty ? null : new AddressCountrySubDivisionIdType { Value = dataProvider.Declarant.RoadNameCode },
					Line = (dataProvider.Declarant?.AddressLine2 ?? ZString.Empty).IsEmpty ? null : new AddressLineTextType { Value = dataProvider.Declarant.AddressLine2 },
					PostcodeId = (dataProvider.Declarant?.Postcode ?? ZString.Empty).IsEmpty ? null : new AddressPostcodeIdType { Value = dataProvider.Declarant.Postcode },
					BuildingNumber = (dataProvider.Declarant?.BuildingNumber ?? ZString.Empty).IsEmpty ? null : new AddressBuildingNumberTextType { Value = dataProvider.Declarant.BuildingNumber },
					Description = new AddressDescriptionTextType { Value = dataProvider.Declarant?.AddressLine1 ?? ZString.Empty }
				},
				Contact = new DeclarationSubmitterContact
				{
					Name = new ContactNameTextType { Value = dataProvider.Declarant?.RepresentativeName ?? ZString.Empty }
				}
			};
		}
	}
}
