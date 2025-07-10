using System.Collections.ObjectModel;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5SI;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5SI)]
	public class GOVCBR5SIMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5SIHeader dataProvider;
		public GOVCBR5SIMessageBuilder(IImport5SIHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}
		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				FunctionCode = PopulateFunctionCode(),
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				TypeCode = PopulateTypeCode(),
				GoodsShipment = PopulateGoodsShipment(),
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
		}

		DeclarationFunctionCodeType PopulateFunctionCode()
		{
			return new DeclarationFunctionCodeType { Value = FunctionCode.Original };
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
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5SI };
		}

		Collection<DeclarationGoodsShipment> PopulateGoodsShipment()
		{
			if (dataProvider.MailItemIDs == null)
			{
				return null;
			}

			var mailItemIDs = new Collection<DeclarationGoodsShipment>();
			foreach (var mailItemID in dataProvider.MailItemIDs)
			{
				var item = new DeclarationGoodsShipment
				{
					AdditionalDocument = new Collection<AdditionalDocumentIdentificationIdType>
					{
						new AdditionalDocumentIdentificationIdType {
							SchemeAgencyId = AgencyIdentificationCodeContentType.Item380,
							Value = mailItemID.ParcelNumber
						},
						new AdditionalDocumentIdentificationIdType {
							SchemeAgencyId = AgencyIdentificationCodeContentType.Kps,
							Value = mailItemID.ParcelCustomsNumber
						}
					},
					AdditionalInformation = new DeclarationGoodsShipmentAdditionalInformation
					{
						StatementCode = new AdditionalInformationStatementCodeType { Value = mailItemID.DeliveryType }
					}
				};
				mailItemIDs.Add(item);
			}
			return mailItemIDs;
		}
	}
}
