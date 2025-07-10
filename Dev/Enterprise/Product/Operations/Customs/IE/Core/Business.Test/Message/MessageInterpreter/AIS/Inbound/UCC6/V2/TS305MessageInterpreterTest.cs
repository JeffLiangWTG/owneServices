using System.Collections.ObjectModel;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS305;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS305MessageInterpreter))]
	sealed class TS305MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, TS305MessageInterpreter, TS305Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS305;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			var messageText = IEXmlObjectSerializer.Serialize(GenerateMessage());
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "MAN0001000", messageText);
		}

		Ts305 GenerateMessage()
		{
			return new Ts305()
			{
				Declaration = new DeclarationType06()
				{
					AmendmentRejectionDate = ZDateTime.BrettsBirthday.ToDateTime(),
					AmendmentRejectionReason = "It was unreasonable",
					Mrn = "01MR1234567890ABR9"
				},
				SupervisingCustomsOffice = new SupervisingcustomofficeType()
				{
					ReferenceNumber = "AA123456"
				},
				CustomsOfficeLodgement = new MScoType01()
				{
					ReferenceNumber = "BB123456"
				},
				Declarant = new DeclarantType03()
				{
					Name = "Mr A",
					IdentificationNumber = "AA!",
					Address = new DeclarantAddressType02()
					{
						PoBox = "-",
						Number = "1",
						Street = "One Street",
						StreetAdditionalLine = "-",
						City = "London",
						Postcode = "W1 1AA",
						Country = "UK",
						SubDivision = "-",
					},
					ContactDetails = new ContactDetailsType()
					{
						IdType = "1",
						IdNumber = "12345678",
						Country = "GB"
					},
					Communication = new Collection<CommunicationType>(new[]
					{
						new CommunicationType()
						{
							Type = "T01",
							Identifier = "Identifier"
						}
					})
				},
				FunctionalError = new Collection<MFunctionalErrorType01>(new[]
				{
					new MFunctionalErrorType01
					{
						SequenceNumber = "1",
						Remarks = "Test_Functional_Error",
						ErrorCode = "12",
						ErrorPointer = "POINTER",
						ErrorReason = "THEREASON1",
						OriginalAttributeValue = "ATTR_VALUE",
					},
					new MFunctionalErrorType01
					{
						SequenceNumber = "2",
						Remarks = "Test_Functional_Error_2",
						ErrorCode = "13",
						ErrorPointer = "NPOINTER",
						ErrorReason = "THEREASON2",
						OriginalAttributeValue = "MYVALUEFORTEST",
					}
				})
			};
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"An Amendment Request Rejection (TS305) message has been received for TSD MAN0001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""></table>";

		protected override TS305Provider GetProvider(TextReader reader) => new TS305Provider(new MailBoxItemProvider<Ts305>(reader).Message);
	}
}
