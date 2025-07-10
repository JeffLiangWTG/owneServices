using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RF409;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(RF409Processor))]
	sealed class RF409ProcessorTest : EntryHeaderMessageProcessorTest<RF409Processor, AISInboundEDIMessage, AISOutboundEDIMessage, RF409Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.RF409;

		protected override ZString MessageText => Serialize(GenerateMessage());

		protected override ZString MessageFriendlyName => "RF409: Refund Application Decision";

		protected override RF409Processor Processor => new RF409Processor(logger, typeof(Rf409Type));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.RefundApplicationRejected, messageAttachee.CH_EntryStatus);
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);

			AssertMessageInterpretation(incomingMessage, RF409MessageInterpreterTest.ExpectedInterpretation);

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "Deposit Refund Application Decision (RF409) has been received and linked to job B00001000." },
				new string[] { "staff1@where.com" });
		}

		internal static Rf409Type GenerateMessage(bool accepted = false)
		{
			return new Rf409Type
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "Abcd123aisdu3eh2u89764",
					ApplicationDecisionCodeType11 = "5",
					RefundApplicationAccepted = accepted ? "1" : "0",
					Signature12 = "IE123456",
					DecisionTakingCustomsAuthority = "Au123456",
					TotalNumberOfDocuments = "456",
				},
				Parties = new Rf409TypeParties
				{
					Applicant32 = "APP_3_2_Content",
					RepresentativeIdentification34 = "REPR_ID_3_4_Value",
				},
				DatesPlaces = new Rf409TypeDatesPlaces
				{
					Date42 = "20230812",
					OfficeOfDept = "Au456456",
					OfficeOfResponsibility = "Au789789",
				},
				Mrn = "IE2345678912345678",
				LegalBasisCodes = new LegalBasisCodeType
				{
					LegalBasis = "A04",
				},
				CustomsProcedure = new CustomsProcedureType
				{
					ProcedureCode = "A4",
				},
				DutiesToBeRemitted = new AmountOfDutiesRf409Type
				{
					AmountOfDutiesToBeRepaid = new AmountOfDutiesToBeRepaidRf409Type
					{
						Amount = 152.05m,
						Currency = Core.Constants.CurrencyCodes.Ireland,
					}
				},
				DestinationOfGoods = "BR",
				TimeLimit = "102",
				StatementOfTheDecision = "STA_OF_DEC",
				DescriptionOfGrounds = "DESC_OF_GROUNDS",
				GoodsInformation = new Collection<GoodsInformationType>
				{
					new GoodsInformationType
					{
						CustomsValue = new GoodsInformationTypeCustomsValue
						{
							Amount = 150m,
							Currency = Core.Constants.CurrencyCodes.Ireland,
						}
					}
				},
				TypeOfDuty = new Collection<TypeOfDutyType>
				{
					new TypeOfDutyType
					{
						TypeOfDuty = new TypeOfDutyRf409Type
						{
							NationalCode = "ABC",
							UnionCode = "CBA",
						}
					}
				},
				AttachedDocuments24 = new Collection<AttachedDocumentType>
				{
					new AttachedDocumentType
					{
						DocumentDate = "20230512",
						DocumentIdentifier = "456789",
						DocumentType = "DOC8"
					}
				},
				GeneralRemarks = new Collection<RemarksType>
				{
					new RemarksType
					{
						GeneralRemarks = "ETU",
					},
					new RemarksType
					{
						GeneralRemarks = "RM2",
					}
				}
			};
		}
	}
}
