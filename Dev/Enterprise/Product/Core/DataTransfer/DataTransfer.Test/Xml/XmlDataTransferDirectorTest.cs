using System;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.GUI.Testing
{
	public class XmlDataTransferDirectorTest : TestCaseWithFactory
	{
		public void TestPromptUserAndImport_PermissionCheck()
		{
			var xmlDataTransferDirector = createInstanceForLicenceCheck();

			for (var i = 0; i <= 1; ++i)
			{
				var permitted = i == 1;
				SetPermission();
				xmlDataTransferDirector.PromptUserAndImport(BillingInterfaceName.Test);
				AssertEquals("Permitted " + permitted, xmlDataTransferDirector.IsPermitted ? "LICENCED" : "NOTLICENCED", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestPromptUserAndImport()
		{
			var mockForm = new Mock<IXmlDataImporterForm>();
			DataImporter importer = null;
			mockForm.SetupSet(f => f.Importer = It.IsAny<DataImporter>()).Callback<DataImporter>(i => importer = i);
			mockForm.Setup(f => f.ShowDialog()).Callback(() =>
			{
				var organisationXml = @"
<?xml version='1.0'?>
<XmlInterchange xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Version='1'>
  <Payload>
    <Organisations>
      <Organisation EDICode='1234' OwnerCode=''>
        <OrganisationDetails>
          <Name>Imported Name</Name>
          <Location>AUSYD</Location>
          <Addresses>
            <Address AddressType='MAIN'>
              <AddressLine1>splaty</AddressLine1>
              <TelephoneNumbers />
              <Sequence>1</Sequence>
            </Address>
          </Addresses>
        </OrganisationDetails>
      </Organisation>
    </Organisations>
  </Payload>\r\n</XmlInterchange>
".Replace("'", "'").Trim();
				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(organisationXml)))
				using (TextReader reader = new StreamReader(stream))
				{
					importer.ImportData(reader, "data.xml", new NotificationBuffer(), SourceInfo.EmptySourceInfo);
				}
			});
			ObjectFactory.Substitute<IXmlDataImporterForm>(mockForm.Object);
			new XmlDataTransferDirector(new OrganisationValueObjectDataAdapter(), false).PromptUserAndImport(BillingInterfaceName.Test);
			var importedOrganisationFilter = new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.Equal, "Imported Name");

			var completelyIndependentFactory = new BusinessObjectFactory();
			var importedOrganisation = completelyIndependentFactory.LoadTop1<OrgHeader>(importedOrganisationFilter);
			AssertNotNull("Organisation should be imported and saved to the database", importedOrganisation);
		}

		public void TestImport_PermissionCheck()
		{
			var xmlDataTransferDirector = createInstanceForLicenceCheck();

			for (var i = 0; i <= 1; ++i)
			{
				var permitted = i == 1;
				SetPermission();
				xmlDataTransferDirector.Import("", null, SourceInfo.EmptySourceInfo);
				AssertEquals("Permitted " + permitted, xmlDataTransferDirector.IsPermitted ? "LICENCED" : "NOTLICENCED", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport()
		{
			var testFile = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "DataTransfer", "DataTransfer.GUI", "Testing", "TestFiles", "OrgHeader.xml");
			new XmlDataTransferDirector(new OrganisationValueObjectDataAdapter(), false).Import(testFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			var importedOrganisationFilter = new ZQuery(OrgHeaderSchema.OH_FullName, "Imported Name");

			var completelyIndependentFactory = new BusinessObjectFactory();
			var importedOrganisation = completelyIndependentFactory.LoadTop1<OrgHeader>(importedOrganisationFilter);
			AssertNotNull("Organisation should be imported and saved to the database", importedOrganisation);
		}

		protected virtual XmlDataTransferDirector createInstanceForLicenceCheck()
		{
			return new XmlDataTransferDirectorForTest();
		}

		void SetPermission()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		sealed class XmlDataTransferDirectorForTest : XmlDataTransferDirector
		{
			public XmlDataTransferDirectorForTest() : base(StmALogValueObjectDataAdapter.New(null, "Test"), true) { }

			protected override void ImportCore(string fileName, INotifications notify, ISourceInfo info)
			{
				Globals.Message.Show("LICENCED");
			}

			protected override void PromptUserAndImportCore(BillingInterfaceName interfaceName)
			{
				Globals.Message.Show("LICENCED");
			}
		}
	}
}
