using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RF409;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;
using RF409Provider = Enterprise.Customs.IE.Messaging.UCC5.RF409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(RF409Processor))]
	class RF409ProcessorTest : EntryHeaderMessageProcessorTest<RF409Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, RF409Provider>
	{
		public void TestNeedToSendEmailNotification()
		{
			var (_, _, _, incomingMessage) = CreateSetupData();
			var processor = new RF409ProcessorForTest(logger, typeof(Rf409));
			AssertEquals("NeedToSendEmailNotification", true, processor.NeedToSendEmailNotification_Exposed(incomingMessage));
		}

		public void TestMessageInterpreterType()
		{
			var processor = new RF409ProcessorForTest(logger, typeof(Rf409));
			AssertEquals("Interpreter Type", typeof(RF409MessageInterpreter), processor.MessageInterpreterType_Exposed);
		}

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.RefundApplicationAccepted, messageAttachee.CH_EntryStatus);
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "Deposit Refund Application Decision (RF409) has been received and linked to job B00001000." },
				new string[] { "staff1@where.com" });

			var refundDuty = (RefundDuty)messageAttachee.MergedLines[0].RefundDuties.First();
			AssertEquals("TaxAmountOfDutyToBeRepaid should have set", 152.05m, refundDuty.TaxAmountOfDutyToBeRepaid);
		}

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var entryLine = result.messageAttachee.MergedLines.FirstOrDefault() ?? result.messageAttachee.MergedLines.AddNew();
			var refundDuty = entryLine.RefundDuties.AddNew(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
			refundDuty.TaxAmountConfirmedRelease = 10.8m;
			refundDuty.TaxAmountConfirmedAmendment = 10.2m;
			refundDuty.TaxAmountDifferenceForRefunds = 0.6m;

			return result;
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.RF409;

		protected override ZString MessageText => Serialize(GenerateMessage());

		protected override ZString MessageFriendlyName => "RF409: Refund Application Decision";

		protected override RF409Processor Processor => new RF409Processor(logger, typeof(Rf409));

		Rf409 GenerateMessage()
		{
			return new Rf409
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "Abcd123aisdu3eh2u89764",
					ApplicationDecisionCodeType11 = "5",
					RefundApplicationAccepted = "1",
					Signature12 = "IE123456",
					DecisionTakingCustomsAuthority = "Au123456",
					TotalNumberOfDocuments = "456",
				},
				Parties = new Rf409Parties
				{
					Applicant32 = "APP_3_2_Content",
					RepresentativeIdentification34 = "REPR_ID_3_4_Value",
				},
				DatesPlaces = new Rf409DatesPlaces
				{
					Date42 = "20230812",
					OfficeOfDept = "Au456456",
					OfficeOfResponsibility = "Au789789",
				},
				Mrn = "IE2345678912345678",
				LegalBasisCodes = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.LegalBasisCodeType
				{
					LegalBasis = "A04",
				},
				CustomsProcedure = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.CustomsProcedureType
				{
					ProcedureCode = "A4",
				},
				DutiesToBeRemitted = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.AmountOfDutiesRf409Type
				{
					AmountOfDutiesToBeRepaid = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.AmountOfDutiesToBeRepaidRf409Type
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
				AttachedDocuments24 = new Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.AttachedDocumentType>
				{
					new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.AttachedDocumentType
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
					}
				}
			};
		}

		sealed class RF409ProcessorForTest : RF409Processor
		{
			public RF409ProcessorForTest(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
			{
			}

			public bool NeedToSendEmailNotification_Exposed(EDIMessage message) => NeedToSendEmailNotification(message);

			public Type MessageInterpreterType_Exposed => MessageInterpreterType;
		}
	}
}
