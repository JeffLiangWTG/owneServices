using System.Collections.ObjectModel;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF409;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using GoodsInformationType = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF409.GoodsInformationType;
using HeaderType = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF409.HeaderType;
using PartiesType = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF409.PartiesType;
using RF409Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.RF409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(RF409MessageInterpreter))]
	sealed class RF409MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, RF409MessageInterpreter, RF409Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.RF409;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.LegalBasisCode, "A04", "A post clearance request for the benefit of a preferential regime");
			var messageText = IEXmlObjectSerializer.Serialize(GenerateMessage());
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", messageText);
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message)
		{
			return @"Deposit Refund Application Decision (RF409) has been received and linked to job B00000012. Decision: Refund Application Rejected.<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Application Reference ID</td><td>Abcd123aisdu3eh2u89764</td></tr><tr><td>Application Decision Code Type</td><td>5</td></tr><tr><td>Refund Application Accepted</td><td>N</td></tr><tr><td>Signature</td><td>IE123456</td></tr><tr><td>Decision Taking Customs Authority</td><td>Au123456</td></tr><tr><td>Total Number of Documents</td><td>456</td></tr><tr><td>Applicant</td><td>APP_3_2_Content</td></tr><tr><td>Representative</td><td>REPR_ID_3_4_Value</td></tr><tr><td>Date</td><td>20230812</td></tr><tr><td>Office of Department</td><td>Au456456</td></tr><tr><td>Office of Responsibility</td><td>Au789789</td></tr><tr><td>MRN</td><td>IE2345678912345678</td></tr><tr><td>Legal Basis Code</td><td>A04</td></tr><tr><td>Legal Basis Description</td><td>A post clearance request for the benefit of a preferential regime</td></tr><tr><td>Customs procedure (request for completion of formalities)</td><td>001</td></tr><tr><td>Amount of Duties to be Repaid or be Remitted</td><td>EUR 152.05</td></tr><tr><td>Use or destination of goods</td><td>BR</td></tr><tr><td>Time limit for completion of formalities</td><td>102</td></tr><tr><td>Statement of the Decision Taking Customs Authority</td><td>STA_OF_DEC</td></tr><tr><td>Description of Grounds</td><td>DESC_OF_GROUNDS</td></tr></table><br/>
<br/>Goods Information<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Sequence</td><td>Customs Value Currency</td><td>Customs Value Amount</td></tr><tr><td>1</td><td>EUR</td><td>150</td></tr></table><br/>
<br/>Type of import or export duty<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Sequence</td><td>Union Code</td><td>National Code</td></tr><tr><td>1</td><td>CBA</td><td>ABC</td></tr></table><br/>
<br/>Attached Documents<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Document Type</td><td>Document Identifier</td><td>Document Date</td></tr><tr><td>DOC8</td><td>456789</td><td>20230512</td></tr></table><br/>
<br/>General Remarks<br/>
<br/>
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
					ApplicationDecisionCodeType = "5",
					RefundApplicationAccepted = "0",
					Signature = "IE123456",
					DecisionTakingCustomsAuthority = "Au123456",
					TotalNumberOfDocuments = "456",
				},
				Parties = new PartiesType
				{
					Applicant = "APP_3_2_Content",
					RepresentativeIdentification = "REPR_ID_3_4_Value",
				},
				DatesPlaces = new DatePlacesType
				{
					Date = "20230812",
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
					ProcedureCode = "001",
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
						CustomsValue = new CustomsValueType
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
				AttachedDocuments = new Collection<AttachedDocumentType>
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
