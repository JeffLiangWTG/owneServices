using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class IDeliveryExtensionsTest : TestCaseWithFactory
	{
		public void TestGetCreatedInterchange_NotEServicesDelivery()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			mode.EK_Destination = "test@emb.com";
			mode.EK_ServerAddressSubject = "EmailAsBodyMode";
			mode.EK_ParentID = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			var context = new DeliveryContext(Factory);
			var emailDelivery = Delivery.GetInstance(mode);
			byte[] buffer;
			emailDelivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(GetStream()));
			Factory.Save();

			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Should send it to correct recepient", true, email.Recipients.Contains("test@emb.com"));
			AssertEquals("Should send it to correct recepients", 1, email.Recipients.Count);
			Assert("Should not be sent for System Communication", !email.Recipients[0].IsForSystemCommunication);
			AssertEquals("Incorrect email subject", "EmailAsBodyMode", email.Subject);
			AssertContains("Should send correct email body", Encoding.UTF8.GetString(buffer), email.Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			AssertNull("No Interchange", emailDelivery.GetCreatedInterchange());

			SubStreamableStream GetStream()
			{
				var random = new Random();
				MemoryStream stream = new MemoryStream();
				System.Threading.Thread.Sleep(10);
				buffer = new byte[random.Next(10, 1024)];
				random.NextBytes(buffer);
				stream.Write(buffer, 0, buffer.Length);
				stream.Flush();
				stream.Position = 0;
				return (SubStreamableStream)stream;
			}
		}

		public void TestGetCreatedInterchange_EServicesDelivery()
		{
			var arInvoice = ObjectCreator.CreateARInvoice<ARInvoice>("AR0001", ObjectCreator.AUD, 1.0M, ObjectCreator.Debtor);
			Factory.Save();

			var pk = arInvoice.PK.ToString();

			var context = EInvoicingTestHelper.GetDeliveryContext(Factory, arInvoice, "APP", "TST", "STT", new Logger());
			var mode = EInvoicingTestHelper.GetEHubMode();
			var eInvoice = EInvoicingTestHelper.GetEInvoice();
			var provider = EInvoicingTestHelper.GetProvider(context, false, mode);
			var delivery = new GEIDeliveryModeDecider(provider).GetDeliveryMode(mode, false);

			using (Factory.AddDisposableService())
			using (var ms = (SubStreamableStream)new MemoryStream())
			{
				new DataObjectSerializer().Serialize(eInvoice, ms);
				delivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(ms, context.ParentInfo));
				Factory.Save();

				var interchange = delivery.GetCreatedInterchange();
				AssertNotNull("Interchange", interchange);
				AssertEquals("TransportType", "HUB", interchange.EI_TransportType);
			}
		}

		TestObjectCreator ObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
