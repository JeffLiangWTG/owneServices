using System.Collections.ObjectModel;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRDF3;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._DF3)]
	public class GOVCBRDF3MessageBuilder : MessageBuilder<Declaration>
	{
		readonly ILocalExportAmendEntryHeader dataProvider;
		public GOVCBRDF3MessageBuilder(ILocalExportAmendEntryHeader dataProvider)
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
				BorderTransportMeans = PopulateBorderTransportMeans(),
				LoadingLocation = PopulateLoadingLocation(),
				Submitter = PopulateSubmitter()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOfficeAndDivision };
		}

		DeclarationFunctionCodeType PopulateFunctionCode()
		{
			return new DeclarationFunctionCodeType { Value = FunctionCode.Original };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.CustomsReceiptNumber };
		}

		string PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._DF3 };
		}

		Collection<DeclarationBorderTransportMeans> PopulateBorderTransportMeans()
		{
			if (dataProvider.Stevedores == null)
			{
				return null;
			}

			var stevedores = new Collection<DeclarationBorderTransportMeans>();
			foreach (var stevedore in dataProvider.Stevedores)
			{
				var item = new DeclarationBorderTransportMeans
				{
					PersonOnBoard = new DeclarationBorderTransportMeansPersonOnBoard
					{
						SequenceNumeric = stevedore.SequenceNo,
						GivenName = new PersonOnBoardGivenNameTextType { Value = stevedore.FullName },
						Communication = new Collection<DeclarationBorderTransportMeansPersonOnBoardCommunication>
						{
							new DeclarationBorderTransportMeansPersonOnBoardCommunication
							{
								TypeId = new CommunicationTypeIdType { Value = Communication.TelNo },
								Id = new CommunicationIdentificationIdType { Value = stevedore.PhoneNumber }
							},
							new DeclarationBorderTransportMeansPersonOnBoardCommunication
							{
								TypeId = new CommunicationTypeIdType { Value = Communication.Phone },
								Id = new CommunicationIdentificationIdType { Value = stevedore.MobileNumber }
							},
						},
						Contact = new DeclarationBorderTransportMeansPersonOnBoardContact
						{
							DepartmentName = new ContactDepartmentNameTextType { Value = stevedore.CompanyName }
						}
					}
				};
				stevedores.Add(item);
			}
			return stevedores;
		}

		DeclarationLoadingLocation PopulateLoadingLocation()
		{
			return new DeclarationLoadingLocation
			{
				LoadingDateTime = dataProvider.LoadingDate.ToString(DateFormatType.DateTimeNoSecond)
			};
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			return new DeclarationSubmitter { RoleCode = new SubmitterRoleCodeType { Value = RoleDeclarant } };
		}

		const string RoleDeclarant = "1";
	}
}





