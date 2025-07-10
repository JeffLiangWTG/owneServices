using System.Collections.ObjectModel;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RF409;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using RF409Provider = Enterprise.Customs.IE.Messaging.UCC5.RF409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(RF409MessageInterpreter))]
	sealed class RF409MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, RF409MessageInterpreter, RF409Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.RF409;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest()
		{
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.LegalBasisCode, "A04", "A post clearance request for the benefit of a preferential regime");
			var messageText = IEXmlObjectSerializer.Serialize(GenerateMessage());
			return AISInterchangeProcessorTestHelper.GetAISUCC5MailboxMessage(Factory, "B00000012", messageText);
		}

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message)
		{
			return @"Deposit Refund Application Decision (RF409) has been received and linked to job B00000012. Decision: Refund Application Rejected.<br />
				<br />
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>Application Reference ID</td><td>Abcd123aisdu3eh2u89764</td></tr>
					<tr><td>Application Decision Code Type</td><td>5</td></tr>
					<tr><td>Refund Application Accepted</td><td>N</td></tr>
					<tr><td>Decision Taking Customs Authority</td><td>Au123456</td></tr>
					<tr><td>MRN</td><td>IE2345678912345678</td></tr>
					<tr><td>Time limit for completion of formalities</td><td>102</td></tr>
					<tr><td>Statement of the Decision Taking Customs Authority</td><td>STA_OF_DEC</td></tr>
					<tr><td>Description of Grounds</td><td>DESC_OF_GROUNDS</td></tr>
				</table><br />
				<br />
				General Remarks<br />
				<br />
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>Sequence</td><td>General Remarks</td></tr>
					<tr><td>1</td><td>ETU</td></tr>
					<tr><td>2</td><td>RM2</td></tr>
				</table>";
		}

		protected override RF409Provider GetProvider(TextReader reader) => new RF409Provider(new MailBoxItemProvider<Rf409>(reader).Message);

		Rf409 GenerateMessage()
		{
			return new Rf409
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "Abcd123aisdu3eh2u89764",
					ApplicationDecisionCodeType11 = "5",
					RefundApplicationAccepted = "0",
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
