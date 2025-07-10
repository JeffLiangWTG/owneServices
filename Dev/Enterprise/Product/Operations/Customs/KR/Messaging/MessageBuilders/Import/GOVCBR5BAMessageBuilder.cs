using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BA;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5BA)]
	public class GOVCBR5BAMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5BAHeader dataProvider;
		public GOVCBR5BAMessageBuilder(IImport5BAHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				TypeCode = PopulateTypeCode(),
				Consignment = PopulateConsignment(),
				PreviousDocument = PopulatePreviousDocument(),
				Submitter = PopulateSubmitter(),
			};
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ImportDeclarationNumber };
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5BA };
		}

		Collection<DeclarationConsignment> PopulateConsignment()
		{
			var entryLines = new Collection<DeclarationConsignment>();
			foreach (var entryLine in dataProvider.EntryLines)
			{
				var item = new DeclarationConsignment
				{
					SequenceNumeric = Convert.ToDecimal(entryLine.EntryLineNo),
					ConsignmentItem = new DeclarationConsignmentConsignmentItem
					{
						Commodity = new DeclarationConsignmentConsignmentItemCommodity
						{
							CargoDescription = new CommodityCargoDescriptionTextType { Value = entryLine.InvoiceDescription },
							Description = new CommodityDescriptionTextType { Value = entryLine.HSDescription },
							Classification = new DeclarationConsignmentConsignmentItemCommodityClassification
							{
								Id = new ClassificationIdentificationIdType { Value = entryLine.HSCode },
							},
							DutyTaxFee = new DeclarationConsignmentConsignmentItemCommodityDutyTaxFee
							{
								TaxRateNumeric = dataProvider.TariffRate,
								TypeCode = new DutyTaxFeeTypeCodeType { Value = dataProvider.TariffRateClassification },
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
			return new DeclarationPreviousDocument
			{
				IssueDateTime = dataProvider.ImportDeclarationDate.IsValid ? dataProvider.ImportDeclarationDate.ToString(DateFormatType.Date) : string.Empty,
			};
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			var countrySubDivisionID = dataProvider.Declarant?.RoadNameCode ?? ZString.Empty;
			var addressLine2 = dataProvider.Declarant?.AddressLine2 ?? ZString.Empty;
			var buildingNumber = dataProvider.Declarant?.BuildingNumber ?? ZString.Empty;

			return new DeclarationSubmitter
			{
				Id = new SubmitterIdentificationIdType
				{
					Value = dataProvider.Declarant?.GetRegistrationNumber(IdentificationType.BusinessRegNo) ?? ZString.Empty
				},
				Name = new SubmitterNameTextType { Value = dataProvider.Declarant?.CompanyName ?? ZString.Empty },
				Address = new DeclarationSubmitterAddress
				{
					CountrySubDivisionId = countrySubDivisionID.IsEmpty ? null : new AddressCountrySubDivisionIdType { Value = countrySubDivisionID },
					Line = addressLine2.IsEmpty ? null : new AddressLineTextType { Value = addressLine2 },
					PostcodeId = new AddressPostcodeIdType { Value = dataProvider.Declarant?.Postcode ?? ZString.Empty },
					BuildingNumber = buildingNumber.IsEmpty ? null : new AddressBuildingNumberTextType { Value = buildingNumber },
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
