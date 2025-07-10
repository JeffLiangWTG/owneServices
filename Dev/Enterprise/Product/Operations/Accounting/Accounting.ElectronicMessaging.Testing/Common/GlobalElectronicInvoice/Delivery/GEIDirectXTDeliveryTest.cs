using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class GEIDirectXTDeliveryTest : TestCaseWithFactory
	{
		public void TestRecipientIDWithOrgParent()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDEF";

			var transaction = (ITransactionParticipant)Factory;
			using (Factory.AddDisposableService())
			using (transaction.BeginTransactionWithManager())
			{
				var dummyBO = Factory.New<DummyBusinessObject>();
				var context =
					new DeliveryContext(Factory)
					{
						ParentInfo = EntityInfo.New(dummyBO),
						ApplicationCode = ApplicationCodeList.Codes.GlobalElectronicInvoice,
						MessageTypeCode = EInvoiceAPICommandList.Codes.Request,
						MessageSubTypeCode = EInvoiceAPICommandList.Codes.SubmitTransaction,
						Notifications = new Logger()
					};

				var messageBody = """
<GlobalElectronicInvoicing xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
  <Header>
    <ElectronicInvoiceBatchRequest>
      <MessagingSystem>Test Electronic Invoicing System</MessagingSystem>
      <MessageType>REQ</MessageType>
      <BatchNumber>125896</BatchNumber>
    </ElectronicInvoiceBatchRequest>
  </Header>
  <Payload>![CDATA[Test Payload]]</Payload>
</GlobalElectronicInvoicing>
""";
				using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(messageBody)))
				{
					var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
					mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface;
					mode.EK_Destination = "Destination";
					mode.EK_FileFormat = "XML";
					mode.EK_ParentID = org.PK;

					var delivery = new GEIDirectXTDelivery();
					delivery.ErrorNotifier = new EmailNotifier();
					delivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(stream, context.ParentInfo));
					Factory.Save();

					var result = Factory.Load<IXmlEDIInterchange>(new ZQuery()).FirstOrDefault();
					AssertNotNull(result);
					AssertEquals("EI_To", mode.EK_Destination, result.EI_To);
					AssertNullOrEmpty("EI_HeaderText", result.EI_HeaderText);
					AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, result.EI_TransportType);

					var message = result.LoadMessages().FirstOrDefault();
					AssertNotNull(message);
					AssertEquals("EM_MessageType", EInvoiceAPICommandList.Codes.Request, message.EM_MessageType);
					AssertEquals("EM_MessageSubType", EInvoiceAPICommandList.Codes.SubmitTransaction, message.EM_MessageSubType);
					AssertEquals("EM_MessageText", messageBody, message.EM_MessageText);
				}
			}
		}
	}
}
