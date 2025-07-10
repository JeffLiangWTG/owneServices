using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.DataTransformation;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(VisualizerDocumentData))]
	sealed class VisualizerDocumentDataTest : EnterpriseBusinessObjectTestCase
	{
		public void TestWriteAndLoadXml()
		{
			var parent = Factory.New<DummyBusinessObject>();

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = parent.PK;
			documentData.JDD_ParentTableCode = parent.TablePrefix;
			documentData.JDD_Name = "xxx";

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""Name"">
    <Value>China Shipping</Value>
  </Property>
</Entity>");

			documentData.WriteXml(xml);

			var result = documentData.ReadXml();

			AssertNotNull("xml loaded successfully", result);

			AssertEquals("read xml is the same as written",
				xml.ToString(SaveOptions.DisableFormatting),
				result.ToString(SaveOptions.DisableFormatting));
		}

		public void TestWriteAndLoadXml_NoSelfConcurrency()
		{
			var parent = Factory.New<DummyBusinessObject>();

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = parent.PK;
			documentData.JDD_ParentTableCode = parent.TablePrefix;
			documentData.JDD_Name = "xxx";

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""Name"" />
</Entity>");

			documentData.WriteXml(xml);

			Factory.Save();

			xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""Name"">
    <Value>China Shipping</Value>
  </Property>
</Entity>");

			documentData.WriteXml(xml);

			AssertNoExceptionThrown("No concurrency exception should be thrown", Factory.Save);
		}

		public void TestWriteAndLoadXml_XmlIsTheSame()
		{
			var parent = Factory.New<DummyBusinessObject>();

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = parent.PK;
			documentData.JDD_ParentTableCode = parent.TablePrefix;
			documentData.JDD_Name = "xxx";

			var xml = XDocument.Parse(
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Property Name=""Name"">
    <Value>China Shipping</Value>
  </Property>
</Entity>");

			documentData.WriteXml(xml);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var reloadedDocumentData = otherFactory.Load<VisualizerDocumentData>(documentData.PK);

			AssertMultilineASCIIEquals("written JDD_OverriddenData should be the same as loaded otherwise it would cause self concurrency",
				documentData.JDD_OverriddenData,
				reloadedDocumentData.JDD_OverriddenData);
		}

		public void TestParent_Create()
		{
			var parent = Factory.New<DummyBusinessObject>();

			var documentData = parent.LoadOrCreateDocumentData("xxx");

			AssertEquals("Parent", parent, documentData.Parent);
			AssertEquals("Has no changes", false, documentData.HasChanges);
		}

		public void TestParent_Load()
		{
			var parent = Factory.New<DummyBusinessObject>();

			var documentData = parent.LoadOrCreateDocumentData("xxx");

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var reloadedParent = otherFactory.Load<DummyBusinessObject>(parent.PK);

			var reloadedDocumentData = reloadedParent.LoadOrCreateDocumentData("xxx");

			AssertEquals("prerequisite: document data has been reloaded from database", true, reloadedDocumentData.IsInDatabase);
			AssertEquals("Parent", reloadedParent, reloadedDocumentData.Parent);
			AssertEquals("Has no changes", false, reloadedDocumentData.HasChanges);
		}

		public void TestJDD_ParentID()
		{
			var parent1 = Factory.New<DummyBusinessObject>();

			var documentData = parent1.LoadOrCreateDocumentData("xxx");

			var parent2 = Factory.New<DummyBusinessObject>();
			documentData.JDD_ParentID = parent2.PK;

			AssertEquals("Parent", parent2, documentData.Parent);
		}

		public void TestJDD_ParentTableCode()
		{
			var parent1 = Factory.New<DummyBusinessObject>();

			var documentData = parent1.LoadOrCreateDocumentData("xxx");

			documentData.JDD_ParentTableCode = "JK";

			AssertEquals("Parent", null, documentData.Parent);
		}

		public void TestJobNumber()
		{
			var parent = Factory.New<DummyBusinessObject>();

			var documentData = parent.LoadOrCreateDocumentData("xxx");

			AssertEquals("JobNumber", "", ((IJobNumber)documentData).JobNumber);
		}

		public void TestLoadOrCreate_Create()
		{
			var parent = Factory.New<DummyBusinessObject>();

			var documentData = parent.LoadOrCreateDocumentData("xxx");

			AssertNotNull("document data has been created", documentData);
		}

		public void TestLoadOrCreate_Load()
		{
			var parent = Factory.New<DummyBusinessObject>();

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = parent.PK;
			documentData.JDD_ParentTableCode = parent.TablePrefix;
			documentData.JDD_Name = "xxx";

			AssertEquals("document data has been loaded", documentData, parent.LoadOrCreateDocumentData("xxx"));
		}

		public void TestLoadDocumentData()
		{
			var parent1 = Factory.New<DummyBusinessObject>();
			var parent2 = Factory.New<DummyBusinessObject>();

			var documentData1 = Factory.New<VisualizerDocumentData>();
			documentData1.JDD_ParentID = parent1.PK;
			documentData1.JDD_ParentTableCode = parent1.TablePrefix;
			documentData1.JDD_Name = "aaa";

			var pivot2 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot2.SI_SU = Factory.New<VisualizerMenuItem>().PK;
			pivot2.SI_SO = Factory.New<VisualizerTemplate>().PK;
			pivot2.SI_DocumentTitle = "test doc1";

			var documentData2 = Factory.New<VisualizerDocumentData>();
			documentData2.JDD_ParentID = parent1.PK;
			documentData2.JDD_ParentTableCode = parent1.TablePrefix;
			documentData2.JDD_Name = "bbb";

			var documentData3 = Factory.New<VisualizerDocumentData>();
			documentData3.JDD_ParentID = parent2.PK;
			documentData3.JDD_ParentTableCode = parent2.TablePrefix;
			documentData3.JDD_Name = "ccc";

			AssertContainsExactElementsInAnyOrder("parent 1 document data",
				new[] { documentData1, documentData2 },
				parent1.LoadDocumentData());

			AssertContainsExactElementsInAnyOrder("parent 2 document data",
				new[] { documentData3 },
				parent2.LoadDocumentData());
		}

		[ExpectNoExceptions]
		public void TestLicenceConsumptionOnSave()
		{
			var licenceConsumptionLogCreatorMock = new Mock<ILicenceConsumptionLogCreator>();

			licenceConsumptionLogCreatorMock.Setup(l => l.CreateLog(Env.Licence.FormBuilder));

			using (ObjectFactory.Substitute<ILicenceConsumptionLogCreator>(licenceConsumptionLogCreatorMock.Object))
			{
				var parent = Factory.New<DummyBusinessObject>();

				var documentData = Factory.New<VisualizerDocumentData>();
				documentData.JDD_ParentID = parent.PK;
				documentData.JDD_ParentTableCode = parent.TablePrefix;
				documentData.JDD_Name = "xxx";

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var reloadedDocData = newFactory.Load<VisualizerDocumentData>(documentData.PK);
				reloadedDocData.JDD_OverriddenData = "ABC"; // so it HasChanges for saving

				newFactory.Save();

				licenceConsumptionLogCreatorMock.Verify(l => l.CreateLog(Env.Licence.FormBuilder), Times.Exactly(2));
			}
		}

		public void TestAddDataVersion()
		{
			var parent = Factory.New<DummyBusinessObject>();

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = parent.PK;
			documentData.JDD_ParentTableCode = parent.TablePrefix;
			documentData.JDD_Name = "name";

			const string xml =
@"<Entity Type=""Shipment"">
  <Id Type=""ZGuid"">d016070b-8005-4be5-ab3d-14a305ac6cd2</Id>
  <Property Name=""PortOfLoading"">
    <Value Type=""ZString"">AUSYD</Value>
  </Property>
  <Property Name=""PortOfDischarge"">
    <Value Type=""ZString"">NZAKL</Value>
  </Property>
</Entity>";

			var xmlDoc = XDocument.Parse(xml);

			documentData.WriteXml(xmlDoc);

			var result = documentData.ReadXml();

			AssertNotNull("xml loaded successfully", result);

			AssertMultilineASCIIEquals("reloaded xml",
string.Format(@"<Entity Type=""Shipment"" DataMajorVersion=""{0}"" DataMinorVersion=""{1}"">
  <Id Type=""ZGuid"">d016070b-8005-4be5-ab3d-14a305ac6cd2</Id>
  <Property Name=""PortOfLoading"">
    <Value Type=""ZString"">AUSYD</Value>
  </Property>
  <Property Name=""PortOfDischarge"">
    <Value Type=""ZString"">NZAKL</Value>
  </Property>
</Entity>", VisualizerDocumentDataVersion.DocumentData.Major, VisualizerDocumentDataVersion.DocumentData.Minor),
			xmlDoc.ToString());
		}

		public void TestRunningTransformationDoesNotThrowAnException()
		{
			var parent = Factory.New<DummyBusinessObject>();

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = parent.PK;
			documentData.JDD_ParentTableCode = parent.TablePrefix;
			documentData.JDD_Name = "name";

			const string xml =
@"<Entity Type=""Shipment"">
  <Id Type=""ZGuid"">d016070b-8005-4be5-ab3d-14a305ac6cd2</Id>
  <Property Name=""SubShipmentCollection"">
    <EntityCollection Type=""List[Shipment]"">
      <Items>
        <Entity Type=""Shipment"">
        </Entity>
        <Entity Type=""Shipment"">
          <Id Type=""ZGuid"">d3192ed4-281d-49da-9bc1-dfe93f0f25a0</Id>
          <Property Name=""PackingLineCollection"">
            <EntityCollection Type=""List[PackingLine]"">
              <Items>
                <Entity Type=""PackingLine"">
                  <Id Type=""ZGuid"">8ffc3e91-68a2-4514-ad8d-619bd3efb0b5</Id>
                  <Property Name=""WeightUnit"" >
                    <Value Type=""ZString"">KG</Value>
                  </Property>
                  <Property Name=""VolumeUnit"" />
                </Entity>
              </Items>
            </EntityCollection>
          </Property>
          <Property Name=""ShipmentType"" />
        </Entity>
      </Items>
    </EntityCollection>
  </Property>
  <Property Name=""PortOfLoading"">
    <Value Type=""ZString"">AUSYD</Value>
  </Property>
  <Property Name=""PortOfDischarge"">
    <Value Type=""ZString"">NZAKL</Value>
  </Property>
  <Property Name=""ReleaseType"" State=""Added"">
    <Value Type=""ZString"">AAA</Value>
  </Property>
  <Property Name=""SomeDummyPropertyName"" State=""Added"">
    <Value Type=""Int32""></Value>
  </Property>
</Entity>";

			documentData.WriteXml(XDocument.Parse(xml));

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var reloadedDocumentData = otherFactory.Load<VisualizerDocumentData>(documentData.PK);

			XDocument reloadedXml = null;

			AssertNoExceptionThrown("expect reload data", () =>
			{
				var result = reloadedDocumentData.ReadXml();

				AssertNotNull("xml loaded successfully", result);

				reloadedXml = result;
			});

			AssertNotNull("reloaded xml", reloadedXml);
		}

		public void TestUniqueIndexFailureHandler()
		{
			var parent = Factory.New<DummyBusinessObject>();
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			var documentData1 = factory1.New<VisualizerDocumentData>();
			documentData1.JDD_ParentID = parent.PK;
			documentData1.JDD_ParentTableCode = parent.TablePrefix;
			documentData1.JDD_Name = "SomeData";

			var documentData2 = factory2.New<VisualizerDocumentData>();
			documentData2.JDD_ParentID = parent.PK;
			documentData2.JDD_ParentTableCode = parent.TablePrefix;
			documentData2.JDD_Name = "SomeData";

			factory1.Save();

			try
			{
				factory2.Save();
				Fail("Should have caused save exception due to unique index violation");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
			}

			AssertEquals("Another user has saved changes to this Form while you were working on it. Please close and re-open the Form for the latest changes.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var parent = Factory.New<DummyBusinessObject>();

			var documentData = Factory.NewWithValidTestData<VisualizerDocumentData>();
			documentData.JDD_ParentID = parent.PK;
			documentData.JDD_ParentTableCode = parent.TablePrefix;

			return documentData;
		}

		protected override void SetSpeicalValueWhenGetingNewBusinessObject(BusinessObject result)
		{
			var documentData = (VisualizerDocumentData)result;
			documentData.JDD_ParentTableCode = "JS";
		}

		#endregion
	}
}
