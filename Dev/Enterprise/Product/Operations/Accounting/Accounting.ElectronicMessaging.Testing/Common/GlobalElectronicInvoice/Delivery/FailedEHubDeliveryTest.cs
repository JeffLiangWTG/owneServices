using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class FailedEHubDeliveryTest : TestCaseWithFactory
	{
		public void TestCreateEHubMessage()
		{
			var transaction = (ITransactionParticipant)Factory;
			using (Factory.AddDisposableService())
			using (var manager = transaction.BeginTransactionWithManager())
			{
				var dummyBO = Factory.New<DummyBusinessObject>();
				var context =
					new DeliveryContext(Factory)
					{
						ParentInfo = EntityInfo.New(dummyBO),
						ApplicationCode = ApplicationCodeList.Codes.GlobalElectronicInvoice,
						MessageTypeCode = "REQ",
						MessageSubTypeCode = "REQ",
						Notifications = new Logger()
					};

				var messageBody = @"<GlobalElectronicInvoicing xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
  <Header>
    <ElectronicInvoiceBatchRequest>
      <MessagingSystem>Test Electronic Invoicing System</MessagingSystem>
      <MessageType>REQ</MessageType>
      <BatchNumber>125896</BatchNumber>
    </ElectronicInvoiceBatchRequest>
  </Header>
  <Payload>![CDATA[Test Payload]]</Payload>
</GlobalElectronicInvoicing>";

				using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(messageBody)))
				{
					var mode = GlbCompany.CurrentCompany.LoadGEICommuncationModes().First();
					var delivery = new FailedGEIEHubDelivery();
					delivery.ErrorNotifier = new EmailNotifier();
					delivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(stream, context.ParentInfo));
					Factory.Save();

					var result = Factory.Load<IXmlEDIInterchange>(new ZQuery()).FirstOrDefault();
					AssertNotNull(result);
					CombineAssertions(delegate
					{
						AssertEquals("EI_From", Env.CurrentCompany.GetLicenceCode(), result.EI_From);
						AssertEquals("EI_To", mode.EK_Destination, result.EI_To);
						AssertEquals("EI_ApplicationCode", ApplicationCodeList.Codes.GlobalElectronicInvoice, result.EI_ApplicationCode);
						AssertEquals("EI_InterchangeType", "REQ", result.EI_InterchangeType);
						AssertEquals("EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, result.EI_ReceiveTransmit);
						AssertEquals("EI_HeaderText", "<EDIDelivery><FileName>SampleGEI</FileName><EmailSubject>Test Subject</EmailSubject></EDIDelivery>", result.EI_HeaderText);
						AssertEquals("EI_MessageText", messageBody, result.EI_BodyText);
						AssertEquals("EI_GB", GlbBranch.CurrentBranch.PK, result.EI_GB);
						AssertEquals("EI_Status", EDIMessageStatusList.Codes.Failed, result.EI_Status);
					});
				}

				manager.CommitTransaction();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var testOrgHeader = Factory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;
			var testAddress = testOrgHeader.Addresses.AddNew();
			testAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			testAddress.OA_Address1 = "Address1";
			testAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);

			ObjectCreator.CreateNewCompany("CM1", orgProxy: ObjectCreator.AALSHI);

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = testOrgHeader.PK;

			var ediCommunication = testOrgHeader.EDICommunicationsModes.AddNew();
			ediCommunication.EK_Module = "GEI";
			ediCommunication.EK_FileFormat = "XML";
			ediCommunication.EK_CommunicationsTransport = "HUB";
			ediCommunication.EK_Destination = "ITTST";
			ediCommunication.EK_CommsDirection = "TRX";
			ediCommunication.EK_Filename = "SampleGEI";
			ediCommunication.EK_ServerAddressSubject = "Test Subject";

			Factory.Save();
		}

		TestObjectCreator ObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
