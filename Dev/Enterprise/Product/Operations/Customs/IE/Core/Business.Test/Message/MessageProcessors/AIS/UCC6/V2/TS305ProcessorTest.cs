using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS305;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS305Processor))]
	sealed class TS305ProcessorTest : TemporaryStorageHeaderMessageProcessorTest<TS305Processor, AISInboundEDIMessage, AISOutboundEDIMessage, TS305Provider>
	{
		protected override void AssertProcessResultCore(TemporaryStorageHeader header, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("AMA_MessageStatus", CustomsWareEntryStatusList.Codes.Invalid, header.AMA_MessageStatus);
			AssertEquals("CustomsStatus", AISEntryStatusList.Codes.Registered, header.CustomsStatus);

			AssertMessageInterpretation(incomingMessage, @"An Amendment Request Rejection (TS305) message has been received for TSD MAN0001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for MAN0001000",
				new[] { "An Amendment Request Rejection (TS305) message has been received for TSD MAN0001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "TS305: TSD Amendment Request Rejection";

		protected override TS305Processor Processor => new TS305Processor(logger, typeof(Ts305));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS305;

		protected override ZString MessageText => Serialize(
			new Ts305()
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
			});
	}
}
