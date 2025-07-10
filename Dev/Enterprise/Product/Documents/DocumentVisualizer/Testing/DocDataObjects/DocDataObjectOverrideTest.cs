using System.Linq;
using Enterprise.DocumentVisualizer.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.DocDataObjects.Testing
{
	sealed class DocDataObjectOverrideTest : TestCase
	{
		#region TestPropertyOverride

		public void TestPropertyOverride()
		{
			const string id = "identifier-123";

			var dummy = new DummyDocDataObject(id);

			var dynamicDummy = dummy.MakeDocDataDynamic();
			var dynamicText = dynamicDummy.GetDynamicProperty(nameof(DummyDocDataObject.Text));

			dynamicText.SetValue("user text");

			AssertEquals("HasChanges after setting override", true, dynamicText.HasChanges);
			AssertEquals("IsOverridden after setting override", true, dynamicText.IsOverridden);

			var xml = dynamicDummy.GetOverriddenValuesXml();
			AssertMultilineASCIIEquals("override xml",
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Id>identifier-123</Id>
  <Property Name=""Text"">
    <Value>user text</Value>
  </Property>
</Entity>", xml.ToXmlString());

			var dummy2 = new DummyDocDataObject(id);

			var dynamicDummy2 = dummy2.MakeDocDataDynamic();
			dynamicDummy2.MergeDataFromXml(xml);

			var dynamicText2 = dynamicDummy2.GetDynamicProperty(nameof(DummyDocDataObject.Text));

			AssertEquals("Value after applying override", "user text", dynamicText2.Value);
			AssertEquals("HasChanges after applying override", false, dynamicText2.HasChanges);
			AssertEquals("IsOverridden after applying override", true, dynamicText2.IsOverridden);
		}

		#endregion

		#region TestCollectionElementOverride

		public void TestCollectionElementOverride()
		{
			const string dummyId = "dummy-id";
			const string dummy1Id = "dummy1-id";
			const string dummy2Id = "dummy2-id";

			var dummy = new DummyDocDataObject(dummyId);
			var dummyElement1 = new DummyDocDataObject(dummy1Id);
			var dummyElement2 = new DummyDocDataObject(dummy2Id);

			dummy.Collection = new[]
			{
				dummyElement1,
				dummyElement2
			};

			var dynamicDummy = dummy.MakeDocDataDynamic();
			var dynamicDummyCollection = dynamicDummy.GetDynamicProperty(nameof(DummyDocDataObject.Collection));

			var dynamicElements = ((IDynamicDataCollection)dynamicDummyCollection).ToArray();
			var dynamicElement1 = dynamicElements[0];
			var dynamicElement2 = dynamicElements[1];

			var dynamicElement1Text = dynamicElement1.GetDynamicProperty(nameof(DummyDocDataObject.Text));
			var dynamicElement2Text = dynamicElement2.GetDynamicProperty(nameof(DummyDocDataObject.Text));

			dynamicElement1Text.SetValue("user text");

			AssertEquals("Collection.Element(0).Text Value", "user text", dynamicElement1Text.Value);
			AssertEquals("Collection.Element(1).Text Value", string.Empty, dynamicElement2Text.Value);

			AssertEquals("Collection.Element(0).Text.HasChanges", true, dynamicElement1Text.HasChanges);
			AssertEquals("Collection.Element(0).Text.IsOverridden", true, dynamicElement1Text.IsOverridden);

			AssertEquals("Collection.Element(1).Text.HasChanges", false, dynamicElement2Text.HasChanges);
			AssertEquals("Collection.Element(1).Text.IsOverridden", false, dynamicElement2Text.IsOverridden);

			var xml = dynamicDummy.GetOverriddenValuesXml();
			AssertMultilineASCIIEquals("override xml",
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Id>dummy-id</Id>
  <Property Name=""Collection"">
    <EntityCollection>
      <Items>
        <Entity>
          <Id>dummy1-id</Id>
          <Property Name=""Text"">
            <Value>user text</Value>
          </Property>
        </Entity>
      </Items>
    </EntityCollection>
  </Property>
</Entity>", xml.ToXmlString());

			var dummy2 = new DummyDocDataObject(dummyId);
			var dummyElement1_2 = new DummyDocDataObject(dummy1Id);
			var dummyElement2_2 = new DummyDocDataObject(dummy2Id);

			dummy2.Collection = new[]
			{
				dummyElement1,
				dummyElement2
			};

			var dynamicDummy2 = dummy.MakeDocDataDynamic();
			dynamicDummy2.MergeDataFromXml(xml);

			var dynamicDummyCollection2 = dynamicDummy.GetDynamicProperty(nameof(DummyDocDataObject.Collection));

			var dynamicElements2 = ((IDynamicDataCollection)dynamicDummyCollection2).ToArray();
			var dynamicElement1_2 = dynamicElements2[0];
			var dynamicElement2_2 = dynamicElements2[1];

			var dynamicElement1_2Text = dynamicElement1_2.GetDynamicProperty(nameof(DummyDocDataObject.Text));
			var dynamicElement2_2Text = dynamicElement2_2.GetDynamicProperty(nameof(DummyDocDataObject.Text));

			AssertEquals("Collection.Element(0).Text Value", "user text", dynamicElement1_2Text.Value);
			AssertEquals("Collection.Element(1).Text Value", string.Empty, dynamicElement2_2Text.Value);

			AssertEquals("Collection.Element(0).Text.HasChanges", true, dynamicElement1_2Text.HasChanges);
			AssertEquals("Collection.Element(0).Text.IsOverridden", true, dynamicElement1_2Text.IsOverridden);

			AssertEquals("Collection.Element(1).Text.HasChanges", false, dynamicElement2_2Text.HasChanges);
			AssertEquals("Collection.Element(1).Text.IsOverridden", false, dynamicElement2_2Text.IsOverridden);
		}

		#endregion

		#region TestPropertyOverride_SameObjectIsInCollectionAndIsPropertyValue

		public void TestPropertyOverride_SameObjectIsInCollectionAndIsPropertyValue()
		{
			const string dummyId = "dummy-id";
			const string dummy1Id = "dummy1-id";
			const string dummy2Id = "dummy2-id";
			const string collectionId = "collection-id";

			var dummy = new DummyDocDataObject(dummyId);
			var dummyCollection = new DummyDocDataObjectCollection(collectionId);
			var dummyElement1 = new DummyDocDataObject(dummy1Id);
			var dummyElement2 = new DummyDocDataObject(dummy2Id);

			dummyCollection.Collection = new[]
			{
				dummyElement1,
				dummyElement2
			};

			dummyCollection.Main = dummyElement1;
			dummy.OtherCollection = dummyCollection;

			var dynamicDummy = dummy.MakeDocDataDynamic();
			var dynamicDummyCollection = dynamicDummy.GetDynamicProperty(nameof(DummyDocDataObject.OtherCollection));
			var dynamicMain = dynamicDummyCollection.GetDynamicProperty(nameof(DummyDocDataObjectCollection.Main));
			var dynamicMainText = dynamicMain.GetDynamicProperty(nameof(DummyDocDataObject.Text));

			var dynamicElements = ((IDynamicDataCollection)dynamicDummyCollection).ToArray();
			var dynamicElement1 = dynamicElements[0];
			var dynamicElement2 = dynamicElements[1];

			var dynamicElement1Text = dynamicElement1.GetDynamicProperty(nameof(DummyDocDataObject.Text));
			var dynamicElement2Text = dynamicElement2.GetDynamicProperty(nameof(DummyDocDataObject.Text));

			dynamicMainText.SetValue("user text");

			AssertEquals("DummyDocDataObjectCollection.Main.Text", "user text", dummyCollection.Main.Text);
			AssertEquals("DummyDocDataObjectCollection.Element(0).Text", "user text", dummyCollection.ElementAt(0).Text);
			AssertEquals("DummyDocDataObjectCollection.Element(1).Text", string.Empty, dummyCollection.ElementAt(1).Text);

			AssertEquals("OtherCollection.Main.Text Value", "user text", dynamicMainText.Value);
			AssertEquals("OtherCollection.Element(0).Text Value", "user text", dynamicElement1Text.Value);
			AssertEquals("OtherCollection.Element(1).Text Value", string.Empty, dynamicElement2Text.Value);

			AssertEquals("OtherCollection.Main.Text.HasChanges", true, dynamicMainText.HasChanges);
			AssertEquals("OtherCollection.Main.Text.IsOverridden", true, dynamicMainText.IsOverridden);

			AssertEquals("OtherCollection.Element(0).Text.HasChanges", true, dynamicElement1Text.HasChanges);
			AssertEquals("OtherCollection.Element(0).Text.IsOverridden", true, dynamicElement1Text.IsOverridden);

			AssertEquals("OtherCollection.Element(1).Text.HasChanges", false, dynamicElement2Text.HasChanges);
			AssertEquals("OtherCollection.Element(1).Text.IsOverridden", false, dynamicElement2Text.IsOverridden);

			var xml = dynamicDummy.GetOverriddenValuesXml();
			AssertMultilineASCIIEquals("override xml",
@"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>
<Entity>
  <Id>dummy-id</Id>
  <Property Name=""OtherCollection"">
    <EntityCollection>
      <Id>collection-id</Id>
      <Property Name=""Main"">
        <Entity>
          <Id>dummy1-id</Id>
          <Property Name=""Text"">
            <Value>user text</Value>
          </Property>
        </Entity>
      </Property>
      <Items>
        <Entity>
          <Id>dummy1-id</Id>
          <Property Name=""Text"">
            <Value>user text</Value>
          </Property>
        </Entity>
      </Items>
    </EntityCollection>
  </Property>
</Entity>", xml.ToXmlString());

			var dummy2 = new DummyDocDataObject(dummyId);
			var dummyCollection2 = new DummyDocDataObjectCollection(collectionId);
			var dummyElement1_2 = new DummyDocDataObject(dummy1Id);
			var dummyElement2_2 = new DummyDocDataObject(dummy2Id);

			dummyCollection2.Collection = new[]
			{
				dummyElement1_2,
				dummyElement2_2
			};

			dummyCollection2.Main = dummyElement1_2;
			dummy2.OtherCollection = dummyCollection2;

			var dynamicDummy2 = dummy2.MakeDocDataDynamic();
			dynamicDummy2.MergeDataFromXml(xml);

			var dynamicDummyCollection2 = dynamicDummy2.GetDynamicProperty(nameof(DummyDocDataObject.OtherCollection));
			var dynamicMain2 = dynamicDummyCollection2.GetDynamicProperty(nameof(DummyDocDataObjectCollection.Main));
			var dynamicMainText2 = dynamicMain2.GetDynamicProperty(nameof(DummyDocDataObject.Text));

			var dynamicElements2 = ((IDynamicDataCollection)dynamicDummyCollection2).ToArray();
			var dynamicElement1_2 = dynamicElements2[0];
			var dynamicElement2_2 = dynamicElements2[1];

			var dynamicElement1_2Text = dynamicElement1_2.GetDynamicProperty(nameof(DummyDocDataObject.Text));
			var dynamicElement2_2Text = dynamicElement2_2.GetDynamicProperty(nameof(DummyDocDataObject.Text));

			AssertEquals("DummyDocDataObjectCollection.Main.Text", "user text", dummyCollection2.Main.Text);
			AssertEquals("DummyDocDataObjectCollection.Element(0).Text", "user text", dummyCollection2.ElementAt(0).Text);
			AssertEquals("DummyDocDataObjectCollection.Element(1).Text", string.Empty, dummyCollection2.ElementAt(1).Text);

			AssertEquals("OtherCollection.Main.Text Value", "user text", dynamicMainText2.Value);
			AssertEquals("OtherCollection.Element(0).Text Value", "user text", dynamicElement1_2Text.Value);
			AssertEquals("OtherCollection.Element(1).Text Value", string.Empty, dynamicElement2_2Text.Value);

			AssertEquals("OtherCollection.Main.Text.HasChanges", false, dynamicMainText2.HasChanges);
			AssertEquals("OtherCollection.Main.Text.IsOverridden", true, dynamicMainText2.IsOverridden);

			AssertEquals("OtherCollection.Element(0).Text.HasChanges", false, dynamicElement1_2Text.HasChanges);
			AssertEquals("OtherCollection.Element(0).Text.IsOverridden", true, dynamicElement1_2Text.IsOverridden);

			AssertEquals("OtherCollection.Element(1).Text.HasChanges", false, dynamicElement2_2Text.HasChanges);
			AssertEquals("OtherCollection.Element(1).Text.IsOverridden", false, dynamicElement2_2Text.IsOverridden);
		}

		#endregion
	}
}
