using System.Collections.ObjectModel;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM404;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM404Provider = Enterprise.Customs.IE.Messaging.UCC5.IM404Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM404MessageInterpreter))]
	class IM404MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM404MessageInterpreter, IM404Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM404;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest()
		{
			var data = new Im404
			{
				Declaration = new DeclarationType
				{
					AmendmentAcceptanceDate = "20240301",
					Mrn = "12MRN345CDEFG678R9",
					CustomsOffices = new DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IEDUB100"
					},
					PreferredPaymentMethod48 = "A",
					Remarks = "Remarks001"
				},
				GoodsShipment = new GoodsShipmentType
				{
					GoodsShipmentItem = new Collection<GoodsShipmentTypeItem>
				{
					new GoodsShipmentTypeItem
					{
						GoodsItemNumber16 = "1",
					}
				},
					Taxes = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.Taxes02Type
					{
					}
				}
			};

			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(Factory, "B00001000", IEXmlObjectSerializer.Serialize(data));
		}

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => DefaultInterpretation;

		public const string DefaultInterpretation = @"An Amendment Request Registration (IM404) message has been received from customs for Job B00001000.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
	<tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr>
	<tr><td>Amendment Acceptance Date</td><td>01-Mar-24</td></tr>
	<tr><td>Preferred Payment Method</td><td>A</td></tr>
	<tr><td>Remarks</td><td>Remarks001</td></tr>
</table>";

		protected override IM404Provider GetProvider(TextReader reader) => new IM404Provider(new MailBoxItemProvider<Im404>(reader).Message);
	}
}
