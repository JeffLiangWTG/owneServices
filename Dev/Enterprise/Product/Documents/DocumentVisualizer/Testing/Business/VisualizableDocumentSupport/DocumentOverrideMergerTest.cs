using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.DocDataObjects.Testing;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DocumentOverrideMergerTest : TestCaseWithFactory
	{
		#region TestMerge_RunTransformation

		public void TestMerge_RunTransformation()
		{
			var xml = XDocument.Parse(bookingRequestV1Override);

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_Name = "SeaBookingRequest";
			documentData.JDD_OverriddenData = xml.ToString(SaveOptions.DisableFormatting);
			documentData.JDD_ParentID = Guid.NewGuid();

			Factory.Save();

			var services = new Moq.Mock<IServiceContainer>();
			var document = new Moq.Mock<IDocument>();

			var merger = new DocumentOverrideMerger(services.Object, documentData);
			merger.MergeOverride(document.Object);

			var dataTransformationNote = documentData
				.Notes
				.GetAllNotes()
				.Cast<StmNote>()
				.SingleOrDefault(note => note.ST_Description == PredefinedNoteTypes.Instance.DocumentDataTransformationNote.Description);

			AssertNotNull("data transformation note has been created", dataTransformationNote);
			AssertMultilineASCIIEquals("data transformation note",
@"[Information] Transforming 'SeaBookingRequest' from ver. 1.0 to ver. 3.0
[Information] Running 'Transform the SeaBookingRequest data from V1 to V2.'.
[Information] Finished 'Transform the SeaBookingRequest data from V1 to V2.' in 00:00:00.
Original xml:
<?xml version=""1.0"" encoding=""utf-16""?>
<Entity DataMajorVersion=""1"" DataMinorVersion=""0"">
  <Id>6dc176b3-0a7d-46a5-93ff-ed1e407c4705</Id>
  <Property Name=""BasicFreightCollect"" State=""Added"">
    <Value>Y</Value>
  </Property>
</Entity>
Transformed xml:
<?xml version=""1.0"" encoding=""utf-16""?>
<Entity DataMajorVersion=""2"" DataMinorVersion=""0"">
  <Id>6dc176b3-0a7d-46a5-93ff-ed1e407c4705</Id>
  <Property Name=""PaymentHandlingInstructionCollection"">
    <EntityCollection>
      <Items>
        <Entity State=""Added"">
          <Property Name=""Category"">
            <Entity>
              <Property Name=""Code"" NaturalKey=""true"">
                <Value>FRT</Value>
              </Property>
              <Property Name=""Description"">
                <Value>Freight Charge</Value>
              </Property>
            </Entity>
          </Property>
          <Property Name=""PaymentMethod"">
            <Entity>
              <Property Name=""Code"">
                <Value>CCX</Value>
              </Property>
              <Property Name=""Description"">
                <Value>Collect</Value>
              </Property>
            </Entity>
          </Property>
        </Entity>
      </Items>
    </EntityCollection>
  </Property>
</Entity>",
				dataTransformationNote.ST_NoteText);

			dataTransformationNote.RunPreSaveValidation();
			var validationNotifications = dataTransformationNote.Notifications.ToArray();

			AssertMultilineASCIIEquals("expected valid data transformation note",
				string.Empty,
				string.Join("\r\n", validationNotifications.Select(n => n.Message)));
		}

		const string bookingRequestV1Override =
@"<Entity DataMajorVersion=""1"" DataMinorVersion=""0"">
	<Id>6dc176b3-0a7d-46a5-93ff-ed1e407c4705</Id>
	<Property Name=""BasicFreightCollect"" State=""Added"">
		<Value>Y</Value>
	</Property>
</Entity>";

		#endregion

		#region TestMerge_DoNotCreateNoteWhenTransformationDidNotRun

		public void TestMerge_DoNotCreateNoteWhenTransformationDidNotRun()
		{
			var documentOverride = GetDocumentOverride(2, 0);
			var xml = XDocument.Parse(documentOverride);

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_Name = "data-store-for-test";
			documentData.JDD_OverriddenData = xml.ToString(SaveOptions.DisableFormatting);
			documentData.JDD_ParentID = Guid.NewGuid();

			Factory.Save();

			var services = new Moq.Mock<IServiceContainer>();
			var document = new Moq.Mock<IDocument>();

			var merger = new DocumentOverrideMerger(services.Object, documentData);
			merger.MergeOverride(document.Object);

			var dataTransformationNote = documentData
				.Notes
				.GetAllNotes()
				.Cast<StmNote>()
				.SingleOrDefault(note => note.ST_Description == PredefinedNoteTypes.Instance.DocumentDataTransformationNote.Description);

			AssertNull("data transformation note wasn't created", dataTransformationNote);
		}

		string GetDocumentOverride(int major, int minor) => $@"<Entity DataMajorVersion=""{major}"" DataMinorVersion=""{minor}"">
	<Id>
		4a3db5cc-56e6-462a-8f20-73e12d1c8075
	</Id>
	<Property Name=""ExcessValueDeclaration"">
		<Entity>
			<Id>
				fed22bba-f7e2-4ba5-b651-6b7103cc6f5f
			</Id>
			<Property Name=""Amount"">
				<Value>
					2
				</Value>
			</Property>
		</Entity>
	</Property>
</Entity>";

		#endregion

		#region TestApplyOverride

		public void TestApplyOverride()
		{
			var dummyPK = Guid.NewGuid();
			var xml = XDocument.Parse(string.Format(@"
<Entity>
	<Id>{0}</Id>
	<Property Name=""Text"">
		<Value>2</Value>
	</Property>
</Entity>", dummyPK));

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_Name = "data-store-for-test";
			documentData.JDD_OverriddenData = xml.ToString(SaveOptions.DisableFormatting);
			documentData.JDD_ParentID = Guid.NewGuid();

			Factory.Save();

			var dummy = new DummyDocDataObject(dummyPK);
			var dynamicDummy = dummy.MakeDocDataDynamic();

			var services = new Moq.Mock<IServiceContainer>();
			var document = new Moq.Mock<IDocument>();
			document.SetupGet(d => d.Data).Returns(dynamicDummy);

			var merger = new DocumentOverrideMerger(services.Object, documentData);
			merger.ApplyOverride(document.Object);

			AssertEquals("Apply the property even the dynamic property is not generated", "2", dummy.Text);
		}

		#endregion
	}
}
