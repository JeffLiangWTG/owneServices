using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM099;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM099Provider = Enterprise.Customs.IE.Messaging.UCC5.IM099Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM099MessageInterpreter))]
	class IM099MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM099MessageInterpreter, IM099Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM099;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISUCC5MailboxMessage(Factory, "B00000012", IEXmlObjectSerializer.Serialize(new Im099
			{
				Declaration = new DeclarationType
				{
					Lrn25 = "LRN123456789",
					DateLimitOfResponse = "20230811",
					Remarks = "Remarks001",
					CustomsOffices = new DeclarationTypeCustomsOffices { CustomsOfficeLodgement = "LCO123456" },
				}
			}));
		}

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"IM099: General Notification and Request Information<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>Date Limit of Response</td><td>2023-08-11</td></tr><tr><td>Customs Office Lodgement</td><td>LCO123456</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>";

		protected override IM099Provider GetProvider(TextReader reader) => new IM099Provider(new MailBoxItemProvider<Im099>(reader).Message);
	}
}
