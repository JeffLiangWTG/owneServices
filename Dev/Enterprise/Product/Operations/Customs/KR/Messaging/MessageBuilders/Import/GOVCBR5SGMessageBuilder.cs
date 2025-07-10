using System.Collections.ObjectModel;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5SG;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5SG)]
	public class GOVCBR5SGMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5SGHeader dataProvider;
		public GOVCBR5SGMessageBuilder(IImport5SGHeader dataProvider)
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
				VersionId = PopulateVersionID(),
				GoodsShipment = PopulateGoodsShipment(),
				Submitter = PopulateSubmitter(),
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice };
		}

		DeclarationFunctionCodeType PopulateFunctionCode()
		{
			return new DeclarationFunctionCodeType { Value = FunctionCode.Original };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ApplicationNumber };
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5SG };
		}

		DeclarationVersionIdType PopulateVersionID()
		{
			return new DeclarationVersionIdType { Value = dataProvider.SequenceNo.ToString() };
		}

		Collection<DeclarationGoodsShipment> PopulateGoodsShipment()
		{
			var declarations = new Collection<DeclarationGoodsShipment>();

			foreach (var entry in dataProvider.Entries)
			{
				var item = new DeclarationGoodsShipment
				{
					AdditionalDocument = new DeclarationGoodsShipmentAdditionalDocument
					{
						Id = new AdditionalDocumentIdentificationIdType { Value = entry.ImportDeclarationNumber }
					},
					AdditionalInformation = new DeclarationGoodsShipmentAdditionalInformation
					{
						Content = new AdditionalInformationContentTextType { Value = entry.ApplicationReason },
						LimitDateTime = !entry.ExtensionDate.IsValid ? string.Empty : entry.ExtensionDate.ToString(DateFormatType.Date)
					}
				};
				declarations.Add(item);
			}
			return declarations;
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			return dataProvider.UnipassDeclarantID.IsEmpty ? null : new DeclarationSubmitter
			{
				Id = new SubmitterIdentificationIdType
				{
					Value = dataProvider.UnipassDeclarantID
				}
			};
		}
	}
}
