using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects.Testing;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Moq;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class UXmlDocDataObjectSenderTest : TestCaseWithUXmlSupport
	{
		#region TestSendUniversalXml

		public void TestSendUniversalXml()
		{
			using (DataContextManagersSubstitution())
			{
				var dummy = Factory.New<DummyWithUXmlSupport>();
				var documentData = dummy.LoadOrCreateDocumentData("xxx");

				Factory.Save();

				var dataObject = new DummyWithDocDataObjectValidation();

				Factory.Save();

				var notifications = new DummyNotifications();

				const string documentName = "Test Document";
				const string dataContext = "Test";

				var worksheet = DummyWorksheet.Parse(
$@"#Config:Name=""{documentName}"":DataContext=""{dataContext}"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier""
#End");

				var worksheetTemplate = new StandardTemplate(worksheet);

				var messageInstructions = new StandardMessageInstructions(
					worksheetTemplate,
					new MacroScope(documentData),
					System.Array.Empty<IMacroLibrary>().CreateContext(),
					documentName);

				var data = dataObject.MakeDynamic();

				var document = new Mock<IDocument>();
				var deliveryService = new Mock<IDocumentDeliveryService>();

				document.Setup(d => d.Data).Returns(data);

				deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)));

				var parameters = new UXmlSender.Parameters
				{
					Instructions = messageInstructions,
					Document = document.Object,
					DocumentData = documentData,
					DeliveryService = deliveryService.Object
				};

				var sender = new UXmlSender(parameters);

				sender.Send(notifications);

				deliveryService.Verify(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)), Times.Never);

				AssertEquals(0, notifications.Notifications.Count);

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(1, messages.Length);
			}
		}

		#endregion

		#region TestSendUniversalXml_DataSource

		public void TestSendUniversalXml_DataSource()
		{
			using (DataContextManagersSubstitution())
			{
				var dummy = Factory.New<DummyWithUXmlSupport>();
				var documentData = dummy.LoadOrCreateDocumentData("xxx");

				Factory.Save();

				var dataObject = new DummyDocDataObject();
				dataObject.Text = "001";

				var notifications = new DummyNotifications();

				const string documentName = "Test Document";
				const string dataContext = "Test";

				var worksheet = DummyWorksheet.Parse(
					$@"#Config:Name=""{documentName}"":DataContext=""{dataContext}"":EHubClientID=""Shipping_Instruction"":MessageRecipient=""Carrier""
#End");

				var worksheetTemplate = new StandardTemplate(worksheet);

				var messageInstructions = new StandardMessageInstructions(
					worksheetTemplate,
					new MacroScope(documentData),
					System.Array.Empty<IMacroLibrary>().CreateContext(),
					documentName);

				var data = dataObject.MakeDynamic();

				var document = new Mock<IDocument>();
				var deliveryService = new Mock<IDocumentDeliveryService>();

				document.Setup(d => d.Data).Returns(data);

				deliveryService.Setup(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)));

				var parameters = new UXmlSender.Parameters
				{
					Instructions = messageInstructions,
					Document = document.Object,
					DocumentData = documentData,
					DeliveryService = deliveryService.Object
				};

				var sender = new UXmlSender(parameters);

				sender.Send(notifications);
				deliveryService.Verify(s => s.GetCommunicationSettings(It.IsAny<IBusiness>(), It.Is<INotifications>(n => n == notifications)), Times.Never);

				AssertEquals(0, notifications.Notifications.Count);

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(1, messages.Length);

				var universalShipment = messages.FirstOrDefault().GetEM_MessageTextReader().Parse<Shipment>();
				AssertNotNull(universalShipment);

				AssertEquals("K001", universalShipment.GetMatchingDataSource(DataContextType.DummyBusinessObject)?.Key);
			}
		}

		#endregion
	}
}
