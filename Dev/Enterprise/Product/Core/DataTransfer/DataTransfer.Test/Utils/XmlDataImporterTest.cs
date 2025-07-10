using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.DataTransfer.Business.Testing
{
	public class XmlDataImporterTest : TestCaseWithFactory
	{
		public void TestCanImportXmlCollection()
		{
			OrganisationValueObjectDataAdapter adapter = new OrganisationValueObjectDataAdapter();
			XmlDataImporter importer = new XmlDataImporter(new SingleBusinessObjectFactoryProvider(Factory), adapter);

			AssertEquals("Incorrect value returned", true, importer.CanImportXmlCollection(adapter.RootCollectionElementName, ""));

			AssertNotEquals("Precondition:", "Test", adapter.RootCollectionElementName);
			AssertEquals("Incorrect value returned", false, importer.CanImportXmlCollection("Test", ""));
		}

		public void TestImportDataToFactory()
		{
			OrganisationValueObjectDataAdapter adapter = new OrganisationValueObjectDataAdapter();
			XmlDataImporter importer = new XmlDataImporter(new SingleBusinessObjectFactoryProvider(Factory), adapter);
			NotificationBuffer notify = new NotificationBuffer();
			OrgHeader organisation = Factory.New<OrgHeader>();

			organisation.OH_FullName = "Existing Name";
			organisation.OH_Code = "1234";

			using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(OrganisationXmlWithNoError)))
			using (TextReader reader = new StreamReader(stream))
			{
				ITransactionParticipant[] additionalTransactionActions;
				importer.ImportDataToFactory(
					reader,
					"",
					notify,
					SourceInfo.EmptySourceInfo,
					out additionalTransactionActions);
				AssertEquals("Imported Name", organisation.OH_FullName);
			}
		}

		public void TestImportingWithXmlValidationErrorsDoesntSave()
		{
			OrganisationValueObjectDataAdapter adapter = new OrganisationValueObjectDataAdapter();
			XmlDataImporter importer = new XmlDataImporter(adapter);

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(OrganisationXmlWithError)))
			using (var reader = new StreamReader(stream))
			{
				NotificationBuffer notify = new NotificationBuffer();
				bool shouldSave = importer.ImportData(reader, "", notify, SourceInfo.EmptySourceInfo);
				AssertEquals("There should be no validation", false, notify.ContainsNotificationType(ErrorType.XmlSchemaValidation));
				AssertEquals("Should save despite errors in xml", true, shouldSave);
			}

			using (var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(OrganisationXmlWithNoError)))
			using (var reader1 = new StreamReader(stream1))
			{
				NotificationBuffer notify = new NotificationBuffer();
				AssertEquals("No error notification found", false, notify.HasErrors);
				notify.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, "test"));
				bool shouldSave = importer.ImportData(reader1, "", notify, SourceInfo.EmptySourceInfo);
				AssertEquals("Should have Data prevent save errors for the test", true, notify.ContainsNotificationType(ErrorType.DataErrorPreventSave));
				AssertEquals("Should save despite errors in xml", true, shouldSave);
			}
		}

		public void TestImportViaMessageActionWithOnlySaveWhenNoError()
		{
			var mockRepository = new MockRepository(MockBehavior.Strict);
			var adapter = mockRepository.Create<OrganisationValueObjectDataAdapter>();
			var importer = new Mock<XmlDataImporter>(adapter.Object) { CallBase = true };
			var serialiser = mockRepository.Create<XmlValueObjectSerializer>(adapter.Object.ValueObjectType);

			serialiser.Setup(m => m.ImportXmlData(It.IsAny<Stream>(), It.IsAny<IValueObjectDataAdapter>(), It.IsAny<IBusinessObjectCollection>(), It.IsAny<BusinessObjectFactoryProvider>(), It.IsAny<INotifications>()))
				.Callback((Stream reader, IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, BusinessObjectFactoryProvider factoryProvider, INotifications notifications) =>
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, "test"));
				});

			importer.Setup(m => m.GetSerializer())
				.Returns(serialiser.Object);

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(OrganisationXmlWithNoError)))
			using (var reader = new StreamReader(stream))
			{
				var notify = new NotificationBuffer();
				AssertEquals("No error notification found", false, notify.HasErrors);

				ITransactionParticipant[] participants;
				var shouldSave = importer.Object.ImportDataToFactory(reader, "", notify, SourceInfo.EmptySourceInfo, out participants);
				AssertEquals("Should have Data prevent save errors for the test", true, notify.HasErrors);
				AssertEquals("Should have Data prevent save errors for the test", false, notify.ContainsNotificationType(ErrorType.DataErrorPreventSave));
				AssertEquals("Should save despite errors in xml", true, shouldSave);
			}

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(OrganisationXmlWithNoError)))
			using (var reader = new StreamReader(stream))
			{
				var notify = new NotificationBuffer();
				importer.Object.OnlySaveDataWhenNoRecordsHaveErrors = true;
				ITransactionParticipant[] participants;
				var shouldSave = importer.Object.ImportDataToFactory(reader, "", notify, SourceInfo.EmptySourceInfo, out participants);
				AssertEquals("Should have Data prevent save errors for the test", true, notify.HasErrors);
				AssertEquals("Should have Data prevent save errors for the test", false, notify.ContainsNotificationType(ErrorType.DataErrorPreventSave));
				AssertEquals("Should save despite errors in xml", false, shouldSave);
				serialiser.Verify(
					m => m.ImportXmlData(It.IsAny<Stream>(), It.IsAny<IValueObjectDataAdapter>(), It.IsAny<IBusinessObjectCollection>(), It.IsAny<BusinessObjectFactoryProvider>(), It.IsAny<INotifications>()),
					Times.Exactly(2));
				importer.Verify(m => m.GetSerializer(),
					Times.Exactly(2));
				mockRepository.VerifyAll();
			}
		}

		public void TestImportViaGUIActionWithOnlySaveWhenNoError()
		{
			var mockRepository = new MockRepository(MockBehavior.Strict);
			var adapter = mockRepository.Create<OrganisationValueObjectDataAdapter>();
			var importer = new Mock<XmlDataImporter>(adapter.Object) { CallBase = true };
			var serialiser = mockRepository.Create<XmlValueObjectSerializer>(adapter.Object.ValueObjectType);

			serialiser.Setup(m => m.ImportXmlData(It.IsAny<Stream>(), It.IsAny<IValueObjectDataAdapter>(), It.IsAny<IBusinessObjectCollection>(), It.IsAny<BusinessObjectFactoryProvider>(), It.IsAny<INotifications>()))
				.Callback((Stream reader, IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, BusinessObjectFactoryProvider factoryProvider, INotifications notifications) =>
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, "test"));
				});

			importer.Setup(m => m.GetSerializer())
				.Returns(serialiser.Object);

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(OrganisationXmlWithNoError)))
			using (var reader = new StreamReader(stream))
			{
				var notify = new NotificationBuffer();
				AssertEquals("No error notification found", false, notify.HasErrors);
				var shouldSave = importer.Object.ImportData(reader, "", notify, SourceInfo.EmptySourceInfo);
				AssertEquals("Should have Data prevent save errors for the test", true, notify.HasErrors);
				AssertEquals("Should have Data prevent save errors for the test", false, notify.ContainsNotificationType(ErrorType.DataErrorPreventSave));
				AssertEquals("Should save despite errors in xml", true, shouldSave);
			}

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(OrganisationXmlWithNoError)))
			using (var reader = new StreamReader(stream))
			{
				var notify = new NotificationBuffer();
				importer.Object.OnlySaveDataWhenNoRecordsHaveErrors = true;
				var shouldSave = importer.Object.ImportData(reader, "", notify, SourceInfo.EmptySourceInfo);
				AssertEquals("Should have Data prevent save errors for the test", true, notify.HasErrors);
				AssertEquals("Should have Data prevent save errors for the test", false, notify.ContainsNotificationType(ErrorType.DataErrorPreventSave));
				AssertEquals("Should save despite errors in xml", false, shouldSave);
				serialiser.Verify(
					m => m.ImportXmlData(It.IsAny<Stream>(), It.IsAny<IValueObjectDataAdapter>(), It.IsAny<IBusinessObjectCollection>(), It.IsAny<BusinessObjectFactoryProvider>(), It.IsAny<INotifications>()),
					Times.Exactly(2));
				importer.Verify(m => m.GetSerializer(),
					Times.Exactly(2));
				mockRepository.VerifyAll();
			}
		}

		protected string OrganisationXmlWithError = @"
<?xml version='1.0'?>
<XmlInterchange xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Version='1'>
  <Payload>
    <Organisations>
      <Organisation EDICode='1234' OwnerCode=''>
        <OrganisationDetails>
          <Name>Imported Name</Name>		SPLATY
        </OrganisationDetails>
      </Organisation>
    </Organisations>
  </Payload>\r\n</XmlInterchange>
".Replace("'", "'").Trim();

		protected string OrganisationXmlWithNoError = @"
<?xml version='1.0'?>
<XmlInterchange xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Version='1'>
  <Payload>
    <Organisations>
      <Organisation EDICode='1234' OwnerCode=''>
        <OrganisationDetails>
          <Name>Imported Name</Name>
        </OrganisationDetails>
      </Organisation>
    </Organisations>
  </Payload>\r\n</XmlInterchange>
".Replace("'", "'").Trim();
	}
}
