using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectXmlHelperTest : TestCaseWithFactory
	{
		public void TestDeserialisation_ShouldNotSetProperties()
		{
			var bizo = Factory.New<DummyBizoWithExplodingProperty>();
			bizo.Property1 = 100;

			AssertEquals(100, bizo.Property1);
			AssertEquals(200, bizo.Property2);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBizo = newFactory.Load<DummyBizoWithExplodingProperty>(bizo.PK);

			AssertEquals(100, loadedBizo.Property1);
			AssertEquals(200, loadedBizo.Property2);
		}

		public void TestCollectionIsRebuiltOnLoad()
		{
			var factory1 = Factory.CreateNewFactory();
			factory1.RefreshEnabled = true;

			var bizo = factory1.NewWithValidTestData<DummyWithChildCollectionBizosOnXmlColumn>();
			var addedItem = bizo.ChildCollection.AddNew();
			addedItem.ChildProperty1 = "ABC123";

			factory1.Save();

			var factory2 = Factory.CreateNewFactory();
			factory2.RefreshEnabled = true;

			var loadedBizo = factory2.Load<DummyWithChildCollectionBizosOnXmlColumn>(bizo.PK);

			AssertEquals(1, loadedBizo.ChildCollection.Cast<DummyChildBizoToBeStoredOnXmlColumn>().Count(c => c.ChildProperty1 == addedItem.ChildProperty1));

			bizo.Property1 = "A";

			factory1.Save();

			AssertEquals(1, loadedBizo.ChildCollection.Cast<DummyChildBizoToBeStoredOnXmlColumn>().Count(c => c.ChildProperty1 == addedItem.ChildProperty1));
			AssertEquals(1, loadedBizo.ChildCollection.Cast<DummyChildBizoToBeStoredOnXmlColumn>().Count());
		}

		public void TestCollectionWithDefaultValuesFindsElements()
		{
			var factory1 = Factory.CreateNewFactory();
			factory1.RefreshEnabled = true;

			var bizo = factory1.NewWithValidTestData<DummyWithChildCollectionBizosOnXmlColumn_CollectionContainsDefaults>();

			AssertEquals(1, bizo.ChildCollection.Count);
			var defaultItem = bizo.ChildCollection[0];

			factory1.Save();
			var factory2 = Factory.CreateNewFactory();
			factory2.RefreshEnabled = true;

			var loadedBizo = factory2.Load<DummyWithChildCollectionBizosOnXmlColumn_CollectionContainsDefaults>(bizo.PK);

			defaultItem.ChildProperty2 = 1;

			factory1.Save();

			AssertEquals(1, loadedBizo.ChildCollection.Count);
			AssertEquals(1, loadedBizo.ChildCollection.Cast<DummyChildBizoToBeStoredOnXmlColumn>().Single(c => c.ChildProperty1 == defaultItem.ChildProperty1).ChildProperty2);
		}

		public void TestSerialise_ShouldNotDeserialiseNonXmlColumns()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			Factory.Save();
			AssertEquals(ZString.Empty, bizo.Z0_Xml);

			var datXml = @"<Z0_Xml>
	<Property1>Shlanky</Property1>
  <NonXmlProperty>27</NonXmlProperty>
</Z0_Xml>";

			bizo.Z0_Xml = datXml;

			Factory.Save();
			AssertMultilineASCIIEquals("", datXml, bizo.Z0_Xml);

			var loadedBizo = Factory.Load<DummyForSingleXmlColumnsTest>(bizo.PK);

			AssertEquals("It's important to note that this is not 27.", ZInt.Zero, loadedBizo.NonXmlProperty);
			AssertEquals("Shlanky", loadedBizo.Property1);
		}

		public void TestSerialise_ShouldNotSerialiseEmptyValues()
		{
			var bizo = Factory.New<DummyForEmptyPropertyValuesTest>();
			Factory.Save();
			AssertEquals(ZString.Empty, bizo.Z0_Xml);

			bizo.Property1 = "Booo";
			Factory.Save();
			AssertMultilineASCIIEquals("",
@"<Z0_Xml>
  <Property1>Booo</Property1>
</Z0_Xml>", bizo.Z0_Xml);

			bizo.Property2 = 123;
			Factory.Save();
			AssertMultilineASCIIEquals("",
@"<Z0_Xml>
  <Property1>Booo</Property1>
  <Property2>123</Property2>
</Z0_Xml>", bizo.Z0_Xml);

			bizo.Property3 = new ZDateTime(2014, 10, 4);
			Factory.Save();
			AssertMultilineASCIIEquals("",
@"<Z0_Xml>
  <Property1>Booo</Property1>
  <Property2>123</Property2>
  <Property3>2014-10-04 00:00:00.000</Property3>
</Z0_Xml>", bizo.Z0_Xml);

			bizo.Property4 = 111;
			Factory.Save();
			AssertMultilineASCIIEquals("",
@"<Z0_Xml>
  <Property1>Booo</Property1>
  <Property2>123</Property2>
  <Property3>2014-10-04 00:00:00.000</Property3>
  <Property4>111</Property4>
</Z0_Xml>", bizo.Z0_Xml);

			bizo.Property5 = ZBool.True;
			Factory.Save();
			AssertMultilineASCIIEquals("",
@"<Z0_Xml>
  <Property5>Y</Property5>
  <Property1>Booo</Property1>
  <Property2>123</Property2>
  <Property3>2014-10-04 00:00:00.000</Property3>
  <Property4>111</Property4>
</Z0_Xml>", bizo.Z0_Xml);

			bizo.ZBlobField = new byte[] { 3, 2, 1 };
			Factory.Save();
			AssertMultilineASCIIEquals("",
@"<Z0_Xml>
  <Property5>Y</Property5>
  <Property1>Booo</Property1>
  <Property2>123</Property2>
  <Property3>2014-10-04 00:00:00.000</Property3>
  <Property4>111</Property4>
  <ZBlobField>3,2,1</ZBlobField>
</Z0_Xml>", bizo.Z0_Xml);
		}

		public void TestSerialise_ShouldNotSerialiseEmptyValues_ExceptWhenEmptyIsDifferentFromColumnDefaultValue()
		{
			var bizo = Factory.New<DummyForEmptyPropertyValuesWithDefaultValuesTest>();
			Factory.Save();
			AssertEquals(ZString.Empty, bizo.Z0_Xml);

			bizo.Property5 = ZBool.False;
			Factory.Save();
			AssertMultilineASCIIEquals("",
@"<Z0_Xml>
  <Property5>N</Property5>
</Z0_Xml>", bizo.Z0_Xml);

			bizo.Property5 = ZBool.True;
			Factory.Save();
			AssertEquals(ZString.Empty, bizo.Z0_Xml);

			bizo.Property6 = "Non-default value";
			Factory.Save();
			AssertMultilineASCIIEquals("",
@"<Z0_Xml>
  <Property6>Non-default value</Property6>
</Z0_Xml>", bizo.Z0_Xml);
		}

		public void TestSerialise_ShouldNotSerialiseEmptyValues_ExceptWhenOverriddenOnAttribute()
		{
			var bizo = Factory.New<DummyForEmptyPropertyValuesWithSerialiseDefaultValuesEnabledTest>();
			Factory.Save();
			AssertMultilineASCIIEquals("",
@"<Z0_Xml>
  <Property5>Y</Property5>
  <Property6>Something...</Property6>
  <Property7></Property7>
</Z0_Xml>", bizo.Z0_Xml);

			bizo.Property5 = ZBool.False;
			Factory.Save();
			AssertMultilineASCIIEquals("",
@"<Z0_Xml>
  <Property5>N</Property5>
  <Property6>Something...</Property6>
  <Property7></Property7>
</Z0_Xml>", bizo.Z0_Xml);

			bizo.Property6 = "Non-default value";
			bizo.Property7 = "Also non-default value";
			Factory.Save();
			AssertMultilineASCIIEquals("",
@"<Z0_Xml>
  <Property5>N</Property5>
  <Property6>Non-default value</Property6>
  <Property7>Also non-default value</Property7>
</Z0_Xml>", bizo.Z0_Xml);
		}

		public void TestConstructNewBizo_ShouldSetXmlColumnDefaultValues()
		{
			var bizo = Factory.New<DummyForEmptyPropertyValuesWithSerialiseDefaultValuesEnabledTest>();

			AssertEquals((byte)15, bizo.Property4);
			AssertEquals(true, bizo.Property5);
			AssertEquals("Something...", bizo.Property6);
		}

		public void TestXmlSerialisableColumns_RecursiveBusinessObject()
		{
			var bizo = Factory.NewWithValidTestData<DummyWithChildBusinessObjectOnXmlColumn>();
			var childBizo = bizo.ChildBusinessObject;
			var grandchildBizo = childBizo.RecursiveChildBusinessObject;
			var grandgrandchildBizo = grandchildBizo.RecursiveChildBusinessObject;

			bizo.Property1 = "Alex the Palex";
			childBizo.ChildProperty1 = "Has one hump";
			grandchildBizo.ChildProperty1 = "So go, Alex, go!";
			grandgrandchildBizo.ChildProperty1 = "That's silly.";

			Factory.Save();

			var loadedBizo = Factory.CreateNewFactory().Load<DummyWithChildBusinessObjectOnXmlColumn>(bizo.PK);
			AssertNotNull(loadedBizo.ChildBusinessObject);
			AssertNotNull(loadedBizo.ChildBusinessObject.RecursiveChildBusinessObject);
			AssertNotNull(loadedBizo.ChildBusinessObject.RecursiveChildBusinessObject.RecursiveChildBusinessObject);

			AssertEquals("Alex the Palex", loadedBizo.Property1);
			AssertEquals("Has one hump", loadedBizo.ChildBusinessObject.ChildProperty1);
			AssertEquals("So go, Alex, go!", loadedBizo.ChildBusinessObject.RecursiveChildBusinessObject.ChildProperty1);
			AssertEquals("That's silly.", loadedBizo.ChildBusinessObject.RecursiveChildBusinessObject.RecursiveChildBusinessObject.ChildProperty1);
		}

		public void TestXmlSerialisableColumns_BusinessObjectCollectionOnBusinessObjectOnBusinessObject()
		{
			var bizo = Factory.NewWithValidTestData<DummyWithChildBusinessObjectOnXmlColumn>();
			var childBizo = bizo.ChildBusinessObject;
			childBizo.ChildProperty1 = "strappleberrypie";
			childBizo.ChildProperty2 = 111;

			var childBizoChildCollectionItem1 = childBizo.Collection.AddNew();
			var childBizoChildCollectionItem2 = childBizo.Collection.AddNew();
			childBizoChildCollectionItem1.ChildProperty1 =
@"Such stack overflow
	wow
				very explode
		pls werk
					wow";

			childBizoChildCollectionItem2.ChildProperty1 = "It werked";

			Factory.Save();

			var loadedBizo = new BusinessObjectFactory().Load<DummyWithChildBusinessObjectOnXmlColumn>(bizo.PK);
			var loadedChildBizo = loadedBizo.ChildBusinessObject;

			AssertEquals("strappleberrypie", loadedChildBizo.ChildProperty1);
			AssertEquals(111, loadedChildBizo.ChildProperty2);
			AssertEquals(2, loadedChildBizo.Collection.Count);
			AssertMultilineASCIIEquals("",
@"Such stack overflow
	wow
				very explode
		pls werk
					wow", loadedChildBizo.Collection[0].ChildProperty1);
			AssertEquals("It werked", loadedChildBizo.Collection[1].ChildProperty1);
		}

		[TestDate(2013, 8, 7)]
		public void TestXmlSerialisableColumns_MultipleColumns()
		{
			var bizo = Factory.NewWithValidTestData<DummyForMultipleXmlColumnsTest>();
			bizo.Property1 = "Blap";
			bizo.Property2 = 2;
			bizo.Property3 = ZDateTime.Today;
			bizo.Property4 = 200;
			bizo.Property5 = 100;
			bizo.NotSerialised = "Should not see me";

			Factory.Save();

			var loadedBizo = new BusinessObjectFactory().Load<DummyForMultipleXmlColumnsTest>(bizo.PK);
			AssertEquals("De-serialising should not set HasChanges", false, loadedBizo.HasChanges);

			AssertEquals("Blap", loadedBizo.Property1);
			AssertEquals(2, loadedBizo.Property2);
			AssertEquals(ZDateTime.Today, loadedBizo.Property3);
			AssertEquals(new ZByte(200), loadedBizo.Property4);
			AssertEquals("Should not serialise Property5 since the XML column name was not specified", ZByte.Zero, loadedBizo.Property5);
			AssertEquals(ZString.Empty, loadedBizo.NotSerialised);

			AssertMultilineASCIIEquals("Z0_Xml contents", "<XmlColumn1><Property1>Blap</Property1><Property2>2</Property2></XmlColumn1>", loadedBizo.Z0_Xml);
			AssertMultilineASCIIEquals("Z0_VarCharMax contents",
@"<Z0_VarCharMax>
  <Property3>2013-08-07 00:00:00.000</Property3>
  <Property4>200</Property4>
</Z0_VarCharMax>", loadedBizo.Z0_VarCharMax);
		}

		[TestDate(2013, 8, 7)]
		public void TestXmlSerialisableColumns_SingleColumns()
		{
			var bizo = Factory.NewWithValidTestData<DummyForSingleXmlColumnsTest>();
			bizo.Property1 = "Blap";
			bizo.Property2 = 2;
			bizo.Property3 = ZDateTime.Today;
			bizo.Property4 = 200;
			bizo.NotSerialised = "Should not see me";
			bizo.ZBlobField = new ZBlob(new byte[] { 1, 2, 3, 4 });

			Factory.Save();

			var loadedBizo = new BusinessObjectFactory().Load<DummyForSingleXmlColumnsTest>(bizo.PK);
			AssertEquals("De-serialising should not set HasChanges", false, loadedBizo.HasChanges);

			AssertEquals("Blap", loadedBizo.Property1);
			AssertEquals(2, loadedBizo.Property2);
			AssertEquals(ZDateTime.Today, loadedBizo.Property3);
			AssertEquals(new ZByte(200), loadedBizo.Property4);
			AssertEquals(ZString.Empty, loadedBizo.NotSerialised);
			AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), loadedBizo.ZBlobField);

			AssertMultilineASCIIEquals("Z0_Xml contents", "<Z0_Xml><Property1>Blap</Property1><Property2>2</Property2><Property3>2013-08-07 00:00:00.000</Property3><Property4>200</Property4><ZBlobField>1,2,3,4</ZBlobField></Z0_Xml>", loadedBizo.Z0_Xml);
			AssertMultilineASCIIEquals("Z0_VarCharMax contents - should be empty since we're only using the XML column", ZString.Empty, loadedBizo.Z0_VarCharMax);
		}

		[TestDate(2013, 8, 7)]
		public void TestXmlSerialisableColumns_CloneShouldSetValuesOnClone()
		{
			var bizo = Factory.NewWithValidTestData<DummyForSingleXmlColumnsTest>();
			bizo.Property1 = "Blap";

			var clone = (DummyForSingleXmlColumnsTest)bizo.Clone();
			AssertEquals("Blap", clone.Property1);
		}

		[TestDate(2013, 8, 7)]
		public void TestXmlSerialisableColumns_OverriddenColumnWithNoAttribute()
		{
			var bizo = Factory.NewWithValidTestData<DummyWithOverriddenColumn>();
			bizo.Property1 = "Blap";
			bizo.Property2 = 2;
			bizo.Property3 = ZDateTime.Today;
			bizo.Property4 = 200;
			bizo.NotSerialised = "Should not see me";
			bizo.ZBlobField = new ZBlob(new byte[] { 1, 2, 3, 4 });

			Factory.Save();

			var loadedBizo = new BusinessObjectFactory().Load<DummyForSingleXmlColumnsTest>(bizo.PK);
			AssertEquals("De-serialising should not set HasChanges", false, loadedBizo.HasChanges);

			AssertEquals("Blap", loadedBizo.Property1);
			AssertEquals(2, loadedBizo.Property2);
			AssertEquals(ZDateTime.Today, loadedBizo.Property3);
			AssertEquals(new ZByte(200), loadedBizo.Property4);
			AssertEquals(ZString.Empty, loadedBizo.NotSerialised);
			AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), loadedBizo.ZBlobField);

			AssertMultilineASCIIEquals("Z0_Xml contents", "<Z0_Xml><Property1>Blap</Property1><Property2>2</Property2><Property3>2013-08-07 00:00:00.000</Property3><Property4>200</Property4><ZBlobField>1,2,3,4</ZBlobField></Z0_Xml>", loadedBizo.Z0_Xml);
			AssertMultilineASCIIEquals("Z0_VarCharMax contents - should be empty since we're only using the XML column", ZString.Empty, loadedBizo.Z0_VarCharMax);
		}

		[TestDate(2013, 8, 7)]
		public void TestXmlSerialisableColumns_ChildObjects()
		{
			var bizo = Factory.NewWithValidTestData<DummyWithChildCollectionBizosOnXmlColumn>();
			bizo.Property1 = "Blap";
			bizo.Property2 = 2;
			bizo.Property3 = ZDateTime.Today;
			bizo.Property4 = 200;
			bizo.NotSerialised = "Should not see me";
			bizo.ZBlobField = new ZBlob(new byte[] { 1, 2, 3, 4 });

			var child1 = bizo.ChildCollection.AddNew();
			child1.ChildProperty1 = "Blah";
			child1.ChildProperty2 = 1;

			var child2 = bizo.ChildCollection.AddNew();
			child2.ChildProperty1 = "Booo";
			child2.ChildProperty2 = 2;

			Factory.Save();

			AssertMultilineASCIIEquals("Z0_Xml contents",
@"<Z0_Xml>
  <ChildCollection>
    <CargoWise.EntityFramework.Testing.DummyChildBizoToBeStoredOnXmlColumn>
      <ChildProperty1>Blah</ChildProperty1>
      <ChildProperty2>1</ChildProperty2>
      <Collection />
      <RecursiveChildBusinessObject>
        <RecursiveChildBusinessObject />
      </RecursiveChildBusinessObject>
    </CargoWise.EntityFramework.Testing.DummyChildBizoToBeStoredOnXmlColumn>
    <CargoWise.EntityFramework.Testing.DummyChildBizoToBeStoredOnXmlColumn>
      <ChildProperty1>Booo</ChildProperty1>
      <ChildProperty2>2</ChildProperty2>
      <Collection />
      <RecursiveChildBusinessObject>
        <RecursiveChildBusinessObject />
      </RecursiveChildBusinessObject>
    </CargoWise.EntityFramework.Testing.DummyChildBizoToBeStoredOnXmlColumn>
  </ChildCollection>
  <Property1>Blap</Property1>
  <Property2>2</Property2>
  <Property3>2013-08-07 00:00:00.000</Property3>
  <Property4>200</Property4>
  <ZBlobField>1,2,3,4</ZBlobField>
</Z0_Xml>", bizo.Z0_Xml);

			var loadedBizo = new BusinessObjectFactory().Load<DummyWithChildCollectionBizosOnXmlColumn>(bizo.PK);
			AssertEquals("De-serialising should not set HasChanges", false, loadedBizo.HasChanges);

			AssertEquals("Blap", loadedBizo.Property1);
			AssertEquals(2, loadedBizo.Property2);
			AssertEquals(ZDateTime.Today, loadedBizo.Property3);
			AssertEquals(new ZByte(200), loadedBizo.Property4);
			AssertEquals(ZString.Empty, loadedBizo.NotSerialised);

			AssertEquals(2, loadedBizo.ChildCollection.Count);
			AssertEquals("Blah", loadedBizo.ChildCollection[0].ChildProperty1);
			AssertEquals(1, loadedBizo.ChildCollection[0].ChildProperty2);

			AssertEquals("Booo", loadedBizo.ChildCollection[1].ChildProperty1);
			AssertEquals(2, loadedBizo.ChildCollection[1].ChildProperty2);
		}

		public void TestCopyXmlColumnProperties()
		{
			var bizo = Factory.New<DummyForSingleXmlColumnsTest>();
			bizo.Property1 = "Blap";
			bizo.Property2 = 2;
			bizo.Property3 = ZDateTime.Today;
			bizo.Property4 = 200;
			bizo.NotSerialised = "Should not see me";
			bizo.ZBlobField = new ZBlob(new byte[] { 1, 2, 3, 4 });

			var copy = Factory.New<DummyForSingleXmlColumnsTest>();
			bizo.CopyXmlColumns(copy);

			AssertEquals("Blap", copy.Property1);
			AssertEquals(2, copy.Property2);
			AssertEquals(ZDateTime.Today, copy.Property3);
			AssertEquals(new ZByte(200), copy.Property4);
			AssertEquals(ZString.Empty, copy.NotSerialised);
			AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), copy.ZBlobField);
		}

		public void TestCopyXmlColumnProperties_WithChildren()
		{
			var bizo = Factory.New<DummyWithChildBusinessObjectOnXmlColumn>();
			bizo.Property1 = "Blap";
			bizo.Property2 = 2;
			bizo.Property3 = ZDateTime.Today;
			bizo.Property4 = 200;
			bizo.NotSerialised = "Should not see me";
			bizo.ZBlobField = new ZBlob(new byte[] { 1, 2, 3, 4 });
			bizo.ChildBusinessObject.ChildProperty1 = "Daniels Trousers";
			bizo.ChildBusinessObject.ChildProperty2 = 23;

			var copy = Factory.New<DummyWithChildBusinessObjectOnXmlColumn>();
			bizo.CopyXmlColumns(copy);

			AssertEquals("Blap", copy.Property1);
			AssertEquals(2, copy.Property2);
			AssertEquals(ZDateTime.Today, copy.Property3);
			AssertEquals(new ZByte(200), copy.Property4);
			AssertEquals(ZString.Empty, copy.NotSerialised);
			AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), copy.ZBlobField);
			AssertEquals("Daniels Trousers", copy.ChildBusinessObject.ChildProperty1);
			AssertEquals(23, copy.ChildBusinessObject.ChildProperty2);
		}

		public void TestCloneCopiesXmlColumnProperties()
		{
			var bizo = Factory.New<DummyWithChildBusinessObjectOnXmlColumn>();
			bizo.Property1 = "Blap";
			bizo.Property2 = 2;
			bizo.Property3 = ZDateTime.Today;
			bizo.Property4 = 200;
			bizo.NotSerialised = "Should not see me";
			bizo.ZBlobField = new ZBlob(new byte[] { 1, 2, 3, 4 });
			bizo.ChildBusinessObject.ChildProperty1 = "Daniels Trousers";
			bizo.ChildBusinessObject.ChildProperty2 = 23;

			var copy = (DummyWithChildBusinessObjectOnXmlColumn)bizo.Clone();

			AssertEquals("Blap", copy.Property1);
			AssertEquals(2, copy.Property2);
			AssertEquals(ZDateTime.Today, copy.Property3);
			AssertEquals(new ZByte(200), copy.Property4);
			AssertEquals(ZString.Empty, copy.NotSerialised);
			AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), copy.ZBlobField);
			AssertEquals("Daniels Trousers", copy.ChildBusinessObject.ChildProperty1);
			AssertEquals(23, copy.ChildBusinessObject.ChildProperty2);
		}

		[TestDate(2013, 8, 7)]
		public void TestXmlSerialisableColumns_ChildObjects_NonXmlColumn()
		{
			var bizo = Factory.NewWithValidTestData<DummyWithChildBizosOnNonSchemaColumnProperty>();
			bizo.Property1 = "Blap";
			bizo.Property2 = 2;
			bizo.Property3 = ZDateTime.Today;
			bizo.Property4 = 200;
			bizo.NotSerialised = "Should not see me";
			bizo.ZBlobField = new ZBlob(new byte[] { 1, 2, 3, 4 });

			var child1 = bizo.ChildCollection.AddNew();
			child1.ChildProperty1 = "Blah";
			child1.ChildProperty2 = 1;

			var child2 = bizo.ChildCollection.AddNew();
			child2.ChildProperty1 = "Booo";
			child2.ChildProperty2 = 2;

			Factory.Save();

			AssertContains("MaiProp contents",
				@"<ChildCollection>
    <CargoWise.EntityFramework.Testing.DummyChildBizoToBeStoredOnXmlColumn>
      <ChildProperty1>Blah</ChildProperty1>
      <ChildProperty2>1</ChildProperty2>
      <Collection />
    </CargoWise.EntityFramework.Testing.DummyChildBizoToBeStoredOnXmlColumn>
    <CargoWise.EntityFramework.Testing.DummyChildBizoToBeStoredOnXmlColumn>
      <ChildProperty1>Booo</ChildProperty1>
      <ChildProperty2>2</ChildProperty2>
      <Collection />
    </CargoWise.EntityFramework.Testing.DummyChildBizoToBeStoredOnXmlColumn>
  </ChildCollection>", bizo.MaiProp);

			AssertContains("MaiProp contents",
				@"<Property1>Blap</Property1>
  <Property2>2</Property2>
  <Property3>2013-08-07 00:00:00.000</Property3>
  <Property4>200</Property4>
  <ZBlobField>1,2,3,4</ZBlobField>", bizo.MaiProp);

			var loadedBizo = new BusinessObjectFactory().Load<DummyWithChildBizosOnNonSchemaColumnProperty>(bizo.PK);
			AssertEquals("De-serialising should not set HasChanges", false, loadedBizo.HasChanges);

			AssertEquals("Blap", loadedBizo.Property1);
			AssertEquals(2, loadedBizo.Property2);
			AssertEquals(ZDateTime.Today, loadedBizo.Property3);
			AssertEquals(new ZByte(200), loadedBizo.Property4);
			AssertEquals(ZString.Empty, loadedBizo.NotSerialised);

			AssertEquals(2, loadedBizo.ChildCollection.Count);
			AssertEquals("Blah", loadedBizo.ChildCollection[0].ChildProperty1);
			AssertEquals(1, loadedBizo.ChildCollection[0].ChildProperty2);

			AssertEquals("Booo", loadedBizo.ChildCollection[1].ChildProperty1);
			AssertEquals(2, loadedBizo.ChildCollection[1].ChildProperty2);
		}

		[TestDate(2013, 8, 7)]
		public void TestXmlSerialisableColumns_ChildObjects_AmbiguousColumn()
		{
			var bizo = Factory.NewWithValidTestData<DummyWithChildBizosOnNonSchemaColumnProperty_AndAmbiguousProperties>();
			bizo.Property1 = "Blap";
			bizo.Property2 = 2;
			bizo.Property3 = ZDateTime.Today;
			bizo.Property4 = 200;
			bizo.NotSerialised = "Should not see me";
			bizo.ZBlobField = new ZBlob(new byte[] { 1, 2, 3, 4 });
			bizo.AmbiguousProperty = 94;

			var child1 = bizo.ChildCollection.AddNew();
			child1.ChildProperty1 = "Blah";
			child1.ChildProperty2 = 1;

			var child2 = bizo.ChildCollection.AddNew();
			child2.ChildProperty1 = "Booo";
			child2.ChildProperty2 = 2;

			Factory.Save();

			AssertMultilineASCIIEquals("MaiProp contents",
@"<MaiProp>
  <AmbiguousProperty>94</AmbiguousProperty>
  <ChildCollection>
    <CargoWise.EntityFramework.Testing.DummyChildBizoToBeStoredOnXmlColumn>
      <ChildProperty1>Blah</ChildProperty1>
      <ChildProperty2>1</ChildProperty2>
      <Collection />
    </CargoWise.EntityFramework.Testing.DummyChildBizoToBeStoredOnXmlColumn>
    <CargoWise.EntityFramework.Testing.DummyChildBizoToBeStoredOnXmlColumn>
      <ChildProperty1>Booo</ChildProperty1>
      <ChildProperty2>2</ChildProperty2>
      <Collection />
    </CargoWise.EntityFramework.Testing.DummyChildBizoToBeStoredOnXmlColumn>
  </ChildCollection>
  <Property1>Blap</Property1>
  <Property2>2</Property2>
  <Property3>2013-08-07 00:00:00.000</Property3>
  <Property4>200</Property4>
  <ZBlobField>1,2,3,4</ZBlobField>
</MaiProp>", bizo.MaiProp);

			var loadedBizo = new BusinessObjectFactory().Load<DummyWithChildBizosOnNonSchemaColumnProperty_AndAmbiguousProperties>(bizo.PK);
			AssertEquals("De-serialising should not set HasChanges", false, loadedBizo.HasChanges);

			AssertEquals("Blap", loadedBizo.Property1);
			AssertEquals(2, loadedBizo.Property2);
			AssertEquals(ZDateTime.Today, loadedBizo.Property3);
			AssertEquals(new ZByte(200), loadedBizo.Property4);
			AssertEquals(ZString.Empty, loadedBizo.NotSerialised);
			AssertEquals(94, loadedBizo.AmbiguousProperty);

			AssertEquals(2, loadedBizo.ChildCollection.Count);
			AssertEquals("Blah", loadedBizo.ChildCollection[0].ChildProperty1);
			AssertEquals(1, loadedBizo.ChildCollection[0].ChildProperty2);

			AssertEquals("Booo", loadedBizo.ChildCollection[1].ChildProperty1);
			AssertEquals(2, loadedBizo.ChildCollection[1].ChildProperty2);
		}

		[TestDate(2013, 8, 7)]
		public void TestXmlSerialisableColumns_ReadOnlyChildCollection()
		{
			var bizo = Factory.NewWithValidTestData<DummyForXmlSerialisationWithReadOnlyCollection>();
			bizo.Property1 = "Blap";
			bizo.Property2 = 2;
			bizo.Property3 = ZDateTime.Today;
			bizo.Property4 = 200;
			bizo.NotSerialised = "Should not see me";
			bizo.ZBlobField = new ZBlob(new byte[] { 1, 2, 3, 4 });

			var child1 = bizo.ReadOnlyCollection[0];
			child1.Property1 = "Blah";

			Factory.Save();

			AssertMultilineASCIIEquals("Z0_Xml contents",
@"<Z0_Xml>
  <ReadOnlyCollection>
    <CargoWise.EntityFramework.Testing.DummyReadOnlyNonPersistentBizo>
      <Property1>Blah</Property1>
    </CargoWise.EntityFramework.Testing.DummyReadOnlyNonPersistentBizo>
  </ReadOnlyCollection>
  <Property1>Blap</Property1>
  <Property2>2</Property2>
  <Property3>2013-08-07 00:00:00.000</Property3>
  <Property4>200</Property4>
  <ZBlobField>1,2,3,4</ZBlobField>
</Z0_Xml>", bizo.Z0_Xml);

			var loadedBizo = new BusinessObjectFactory().Load<DummyForXmlSerialisationWithReadOnlyCollection>(bizo.PK);
			AssertEquals("De-serialising should not set HasChanges", false, loadedBizo.HasChanges);

			AssertEquals("Blap", loadedBizo.Property1);
			AssertEquals(2, loadedBizo.Property2);
			AssertEquals(ZDateTime.Today, loadedBizo.Property3);
			AssertEquals(new ZByte(200), loadedBizo.Property4);
			AssertEquals(ZString.Empty, loadedBizo.NotSerialised);

			AssertEquals(1, loadedBizo.ReadOnlyCollection.Count);
			AssertEquals("Blah", loadedBizo.ReadOnlyCollection[0].Property1);
		}

		public void TestDeserialiseProperty_NotPresentInXml_ShouldPopulateWithDefaultValue()
		{
			var pk = ZGuid.NewZGuid();
			var xml = "<Z0_Xml><Property1>Blah</Property1></Z0_Xml>";

			var sql = string.Format(@"
				INSERT dbo.DummyBizo (
					Z0_PK, Z0_Xml
				) VALUES (
					'{0}', '{1}'
				)
				", pk, xml);

			Db.Connection.ExecuteNonQuery(sql);

			var bizo = Factory.Load<DummyForSingleXmlColumnsTest>(pk);
			AssertEquals("DefaultValue specified on attribute should be set on property", (byte)15, bizo.Property4);
		}

		public void TestPropertyDefaultValueNotZero_ShouldSerialiseZero()
		{
			var bizo1 = Factory.New<DummyBizoWithIntDefaultValue>();
			bizo1.Property1 = 0;

			var bizo2 = Factory.New<DummyBizoWithIntDefaultValue>();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBizo1 = newFactory.Load<DummyBizoWithIntDefaultValue>(bizo1.PK);
			var loadedBizo2 = newFactory.Load<DummyBizoWithIntDefaultValue>(bizo2.PK);

			AssertEquals(0, loadedBizo1.Property1);
			AssertEquals(300, loadedBizo2.Property1);

			AssertEquals("<Z0_Xml><Property1>0</Property1></Z0_Xml>", loadedBizo1.Z0_Xml);
			AssertEquals("", loadedBizo2.Z0_Xml);
		}

		public void TestDeserialise_WithEmptyXmlElement_ShouldParseAsDefaultValue()
		{
			var dummy = Factory.NewWithValidTestData<DummyBizoWithNumericProperties>();
			dummy.Property1 = 69;

			Factory.Save();

			Db.Connection.ExecuteNonQuery($@"
UPDATE dbo.DummyBizo
SET Z0_Xml = @xml
WHERE Z0_PK = @pk",
			cmd =>
			{
				cmd.AddParameterBasedOnDbColumn("@xml", @"<Z0_Xml>
	<Property1 />
	<Property2 />
</Z0_Xml>", DummyBizoSchema.Z0_Xml);
				cmd.AddParameterBasedOnDbColumn("@pk", dummy.PK.ToGuid(), DummyBizoSchema.PK);
			});

			var newFactory = Factory.CreateNewFactory();
			var loadedBizo = newFactory.Load<DummyBizoWithNumericProperties>(dummy.PK);

			CombineAssertions("XML de-serialisation needs to handle empty elements because it's so much easier to write XML transforms that insert empty elements compared with selectively inserting elements to existing XML columns", () =>
			{
				AssertEquals("Property1", 0, loadedBizo.Property1);
				AssertEquals("Property2", 0m, loadedBizo.Property2);
			});
		}

		public void TestStringValuesWithNewLines_ShouldBePreservedUponDeserialisation()
		{
			const string textWithNewLines =
@"New
Lines
Work
";

			var bizo = Factory.New<DummyForSingleXmlColumnsTest>();
			bizo.Property1 = textWithNewLines;
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBizo = newFactory.Load<DummyForSingleXmlColumnsTest>(bizo.PK);

			AssertEquals("Text should be exactly equal, including the types of newlines (e.g. \r\n rather than \n).", textWithNewLines, loadedBizo.Property1);
		}
	}

	#region Dummy bizos

	class DummyBizoWithNumericProperties : DummyBusinessObject
	{
		public DummyBizoWithNumericProperties(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[XmlColumnProperty]
		public ZInt Property1
		{
			get { return GetXmlColumnPropertyValue<ZInt>(Property1Info); }
			set { SetXmlColumnPropertyValue(Property1Info, value); }
		}

		public ZPropertyInfo Property1Info => GetZPropertyInfo(nameof(Property1));

		[XmlColumnProperty]
		public ZDecimal Property2
		{
			get { return GetXmlColumnPropertyValue<ZDecimal>(Property2Info); }
			set { SetXmlColumnPropertyValue(Property2Info, value); }
		}

		public ZPropertyInfo Property2Info => GetZPropertyInfo(nameof(Property2));

		protected internal override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get { yield return new XmlColumnSpecification(DummyBizoSchema.Z0_Xml); }
		}
	}

	class DummyBizoWithExplodingProperty : DummyBusinessObject
	{
		public DummyBizoWithExplodingProperty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			SetXmlColumnPropertyValue(Property2Info, new ZInt(200));
		}

		[XmlColumnProperty]
		public ZInt Property1
		{
			get { return GetXmlColumnPropertyValue<ZInt>(Property1Info); }
			set { SetXmlColumnPropertyValue(Property1Info, value); }
		}

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}

		[XmlColumnProperty]
		public ZInt Property2
		{
			get { return GetXmlColumnPropertyValue<ZInt>(Property2Info); }
			set
			{
				throw new InvalidOperationException("No really, don't set properties in deserialisation");
			}
		}

		public ZPropertyInfo Property2Info
		{
			get { return GetZPropertyInfo(nameof(Property2)); }
		}

		protected internal override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get { yield return new XmlColumnSpecification(DummyBizoSchema.Z0_Xml); }
		}
	}

	class DummyBizoWithIntDefaultValue : DummyBusinessObject
	{
		public DummyBizoWithIntDefaultValue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			Property1 = 300;
		}

		#region Properties

		[XmlColumnProperty(DefaultValue = 300)]
		public virtual ZInt Property1
		{
			get { return GetXmlColumnPropertyValue<ZInt>(Property1Info); }
			set { SetXmlColumnPropertyValue(Property1Info, value); }
		}

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}

		#endregion

		protected internal override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get { yield return new XmlColumnSpecification(DummyBizoSchema.Z0_Xml); }
		}
	}

	class DummyForXmlSerialisationWithReadOnlyCollection : DummyForSingleXmlColumnsTest
	{
		public DummyForXmlSerialisationWithReadOnlyCollection(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable]
		[XmlColumnProperty]
		public DummyReadOnlyCollectionForXmlSerialisation ReadOnlyCollection
		{
			get
			{
				if (collection == null)
				{
					collection = new DummyReadOnlyCollectionForXmlSerialisation(Factory);
					RegisterEditableChildObject(collection);
				}

				return collection;
			}
		}
		DummyReadOnlyCollectionForXmlSerialisation collection;
	}

	class DummyReadOnlyCollectionForXmlSerialisation : NonPersistentBusinessObjectCollection<DummyReadOnlyNonPersistentBizo>
	{
		public DummyReadOnlyCollectionForXmlSerialisation(BusinessObjectFactory factory)
		{
			InitialiseCollectionForDeserialisation();
		}

		protected override void InitialiseCollectionForDeserialisation()
		{
			base.InitialiseCollectionForDeserialisation();
			Add(new DummyReadOnlyNonPersistentBizo());
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Nope!");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override NonPersistentBusinessObject GetItemToDeserialise(XElement element)
		{
			return this[0];
		}
	}

	class DummyReadOnlyNonPersistentBizo : NonPersistentBusinessObject
	{
		[XmlColumnProperty]
		public ZString Property1
		{
			get { return GetXmlColumnPropertyValue<ZString>(Property1Info); }
			set { SetXmlColumnPropertyValue(Property1Info, value); }
		}

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}
	}

	class DummyForEmptyPropertyValuesTest : DummyForSingleXmlColumnsTest
	{
		public DummyForEmptyPropertyValuesTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[XmlColumnProperty]
		public ZBool Property5
		{
			get { return GetXmlColumnPropertyValue<ZBool>(Property5Info); }
			set { SetXmlColumnPropertyValue(Property5Info, value); }
		}

		public ZPropertyInfo Property5Info
		{
			get { return GetZPropertyInfo(nameof(Property5)); }
		}
	}

	class DummyForEmptyPropertyValuesWithDefaultValuesTest : DummyForSingleXmlColumnsTest
	{
		public DummyForEmptyPropertyValuesWithDefaultValuesTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[XmlColumnProperty(DefaultValue = true)]
		public ZBool Property5
		{
			get { return GetXmlColumnPropertyValue<ZBool>(Property5Info); }
			set { SetXmlColumnPropertyValue(Property5Info, value); }
		}

		public ZPropertyInfo Property5Info
		{
			get { return GetZPropertyInfo(nameof(Property5)); }
		}

		[XmlColumnProperty(DefaultValue = "Something...")]
		public ZString Property6
		{
			get { return GetXmlColumnPropertyValue<ZString>(Property6Info); }
			set { SetXmlColumnPropertyValue(Property6Info, value); }
		}

		public ZPropertyInfo Property6Info
		{
			get { return GetZPropertyInfo(nameof(Property6)); }
		}
	}

	class DummyForEmptyPropertyValuesWithSerialiseDefaultValuesEnabledTest : DummyForSingleXmlColumnsTest
	{
		public DummyForEmptyPropertyValuesWithSerialiseDefaultValuesEnabledTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[XmlColumnProperty(DefaultValue = true, SerialiseDefaultValues = true)]
		public ZBool Property5
		{
			get { return GetXmlColumnPropertyValue<ZBool>(Property5Info); }
			set { SetXmlColumnPropertyValue(Property5Info, value); }
		}

		public ZPropertyInfo Property5Info
		{
			get { return GetZPropertyInfo(nameof(Property5)); }
		}

		[XmlColumnProperty(DefaultValue = "Something...", SerialiseDefaultValues = true)]
		public ZString Property6
		{
			get { return GetXmlColumnPropertyValue<ZString>(Property6Info); }
			set { SetXmlColumnPropertyValue(Property6Info, value); }
		}

		public ZPropertyInfo Property6Info
		{
			get { return GetZPropertyInfo(nameof(Property6)); }
		}

		[XmlColumnProperty(SerialiseDefaultValues = true)]
		public ZString Property7
		{
			get { return GetXmlColumnPropertyValue<ZString>(Property7Info); }
			set { SetXmlColumnPropertyValue(Property7Info, value); }
		}

		public ZPropertyInfo Property7Info
		{
			get { return GetZPropertyInfo(nameof(Property7)); }
		}
	}

	class DummyForMultipleXmlColumnsTest : DummyBusinessObject
	{
		public DummyForMultipleXmlColumnsTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[XmlColumnProperty(DummyBusinessObject.Schema.Z0_Xml)]
		public ZString Property1
		{
			get { return GetXmlColumnPropertyValue<ZString>(Property1Info); }
			set { SetXmlColumnPropertyValue(Property1Info, value); }
		}

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}

		[XmlColumnProperty(DummyBusinessObject.Schema.Z0_Xml)]
		public ZInt Property2
		{
			get { return GetXmlColumnPropertyValue<ZInt>(Property2Info); }
			set { SetXmlColumnPropertyValue(Property2Info, value); }
		}

		public ZPropertyInfo Property2Info
		{
			get { return GetZPropertyInfo(nameof(Property2)); }
		}

		[XmlColumnProperty(DummyBusinessObject.Schema.Z0_VarCharMax)]
		public ZDateTime Property3
		{
			get { return GetXmlColumnPropertyValue<ZDateTime>(Property3Info); }
			set { SetXmlColumnPropertyValue(Property3Info, value); }
		}

		public ZPropertyInfo Property3Info
		{
			get { return GetZPropertyInfo(nameof(Property3)); }
		}

		[XmlColumnProperty(DummyBusinessObject.Schema.Z0_VarCharMax)]
		public ZByte Property4
		{
			get { return GetXmlColumnPropertyValue<ZByte>(Property4Info); }
			set { SetXmlColumnPropertyValue(Property4Info, value); }
		}

		public ZPropertyInfo Property4Info
		{
			get { return GetZPropertyInfo(nameof(Property4)); }
		}

		[XmlColumnProperty]
		public ZByte Property5
		{
			get { return GetXmlColumnPropertyValue<ZByte>(Property5Info); }
			set { SetXmlColumnPropertyValue(Property5Info, value); }
		}

		public ZPropertyInfo Property5Info
		{
			get { return GetZPropertyInfo(nameof(Property5)); }
		}

		public ZString NotSerialised { get; set; }

		#endregion

		protected internal override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get
			{
				yield return new XmlColumnSpecification(DummyBizoSchema.Z0_Xml, "XmlColumn1");
				yield return new XmlColumnSpecification(DummyBizoSchema.Z0_VarCharMax);
			}
		}
	}

	class DummyForSingleXmlColumnsTest : DummyBusinessObject
	{
		public DummyForSingleXmlColumnsTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[XmlColumnProperty(DummyBusinessObject.Schema.Z0_Xml)]
		public virtual ZString Property1
		{
			get { return GetXmlColumnPropertyValue<ZString>(Property1Info); }
			set { SetXmlColumnPropertyValue(Property1Info, value); }
		}

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}

		[XmlColumnProperty]
		public virtual ZInt Property2
		{
			get { return GetXmlColumnPropertyValue<ZInt>(Property2Info); }
			set { SetXmlColumnPropertyValue(Property2Info, value); }
		}

		public ZPropertyInfo Property2Info
		{
			get { return GetZPropertyInfo(nameof(Property2)); }
		}

		[XmlColumnProperty]
		public virtual ZDateTime Property3
		{
			get { return GetXmlColumnPropertyValue<ZDateTime>(Property3Info); }
			set { SetXmlColumnPropertyValue(Property3Info, value); }
		}

		public ZPropertyInfo Property3Info
		{
			get { return GetZPropertyInfo(nameof(Property3)); }
		}

		[XmlColumnProperty(DefaultValue = (byte)15)]
		public virtual ZByte Property4
		{
			get { return GetXmlColumnPropertyValue<ZByte>(Property4Info); }
			set { SetXmlColumnPropertyValue(Property4Info, value); }
		}

		public ZPropertyInfo Property4Info
		{
			get { return GetZPropertyInfo(nameof(Property4)); }
		}

		public ZString NotSerialised { get; set; }

		[XmlColumnProperty]
		public virtual ZBlob ZBlobField
		{
			get { return GetXmlColumnPropertyValue<ZBlob>(ZBlobFieldInfo); }
			set { SetXmlColumnPropertyValue(ZBlobFieldInfo, value); }
		}

		public ZPropertyInfo ZBlobFieldInfo
		{
			get { return GetZPropertyInfo(nameof(ZBlobField)); }
		}

		public ZInt NonXmlProperty
		{
			get { return nonXmlProperty; }
			set { SetNonPersistentPropertyValue(NonXmlPropertyInfo, ref nonXmlProperty, value); }
		}
		ZInt nonXmlProperty;

		public ZPropertyInfo NonXmlPropertyInfo
		{
			get { return GetZPropertyInfo(nameof(NonXmlProperty)); }
		}

		#endregion

		protected internal override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get { yield return new XmlColumnSpecification(DummyBizoSchema.Z0_Xml); }
		}
	}

	class DummyWithOverriddenColumn : DummyForSingleXmlColumnsTest
	{
		public DummyWithOverriddenColumn(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}

	class DummyWithChildBusinessObjectOnXmlColumn : DummyForSingleXmlColumnsTest
	{
		public DummyWithChildBusinessObjectOnXmlColumn(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable]
		[XmlColumnProperty(DummyBusinessObject.Schema.Z0_Xml)]
		public virtual DummyChildBizoToBeStoredOnXmlColumn ChildBusinessObject
		{
			get
			{
				if (childBusinessObject == null)
				{
					childBusinessObject = new DummyChildBizoToBeStoredOnXmlColumn();
					RegisterEditableChildObject(childBusinessObject);
				}

				return childBusinessObject;
			}
		}
		DummyChildBizoToBeStoredOnXmlColumn childBusinessObject;
	}

	class DummyWithChildCollectionBizosOnXmlColumn : DummyForSingleXmlColumnsTest
	{
		public DummyWithChildCollectionBizosOnXmlColumn(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable]
		[XmlColumnProperty(DummyBusinessObject.Schema.Z0_Xml)]
		public virtual DummyChildBizoToBeStoredOnXmlColumnCollection ChildCollection
		{
			get
			{
				if (collection == null)
				{
					collection = new DummyChildBizoToBeStoredOnXmlColumnCollection();
					RegisterEditableChildObject(collection);
				}

				return collection;
			}
		}
		DummyChildBizoToBeStoredOnXmlColumnCollection collection;
	}

	class DummyWithChildCollectionBizosOnXmlColumn_CollectionContainsDefaults : DummyForSingleXmlColumnsTest
	{
		public DummyWithChildCollectionBizosOnXmlColumn_CollectionContainsDefaults(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable]
		[XmlColumnProperty(DummyBusinessObject.Schema.Z0_Xml)]
		public virtual DummyChildBizoToBeStoredOnXmlColumnCollection_WithDefaultValues ChildCollection
		{
			get
			{
				if (collection == null)
				{
					collection = new DummyChildBizoToBeStoredOnXmlColumnCollection_WithDefaultValues();
					RegisterEditableChildObject(collection);
				}

				return collection;
			}
		}
		DummyChildBizoToBeStoredOnXmlColumnCollection_WithDefaultValues collection;
	}

	class DummyWithChildBizosOnNonSchemaColumnProperty : DummyWithChildCollectionBizosOnXmlColumn
	{
		public DummyWithChildBizosOnNonSchemaColumnProperty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString MaiProp
		{
			get { return Z0_VarCharMax; }
			set { Z0_VarCharMax = value; }
		}

		[XmlColumnProperty]
		public override DummyChildBizoToBeStoredOnXmlColumnCollection ChildCollection
		{
			get { return base.ChildCollection; }
		}

		[XmlColumnProperty("MaiProp")]
		public override ZString Property1
		{
			get { return base.Property1; }
			set { base.Property1 = value; }
		}

		public ZString AmbiguousProperty { get; set; }

		protected internal override IEnumerable<XmlColumnSpecification> XmlSerialisedColumns
		{
			get { yield return new XmlColumnSpecification(GetType().GetProperty("MaiProp")); }
		}
	}

	class DummyWithChildBizosOnNonSchemaColumnProperty_AndAmbiguousProperties : DummyWithChildBizosOnNonSchemaColumnProperty
	{
		public DummyWithChildBizosOnNonSchemaColumnProperty_AndAmbiguousProperties(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[XmlColumnProperty]
		public new ZInt AmbiguousProperty
		{
			get { return GetXmlColumnPropertyValue<ZInt>(AmbiguousPropertyInfo); }
			set { SetXmlColumnPropertyValue(AmbiguousPropertyInfo, value); }
		}

		public ZPropertyInfo AmbiguousPropertyInfo
		{
			get { return GetZPropertyInfo(nameof(AmbiguousProperty)); }
		}
	}

	class DummyChildBizoToBeStoredOnXmlColumnCollection : NonPersistentBusinessObjectCollection<DummyChildBizoToBeStoredOnXmlColumn>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DummyChildBizoToBeStoredOnXmlColumn();
		}
	}

	class DummyChildBizoToBeStoredOnXmlColumnCollection_WithDefaultValues : NonPersistentBusinessObjectCollection<DummyChildBizoToBeStoredOnXmlColumn>
	{
		public DummyChildBizoToBeStoredOnXmlColumnCollection_WithDefaultValues()
		{
			var itemToAdd = new DummyChildBizoToBeStoredOnXmlColumn()
			{
				ChildProperty1 = "IAMADEFAULT"
			};
			this.Add(itemToAdd);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DummyChildBizoToBeStoredOnXmlColumn();
		}

		protected override NonPersistentBusinessObject GetItemToDeserialise(XElement element)
		{
			var property1 = element.Descendants("ChildProperty1").FirstOrDefault();
			return property1 != null ? this.Cast<DummyChildBizoToBeStoredOnXmlColumn>().FirstOrDefault(c => c.ChildProperty1 == property1.Value) : null;
		}
	}

	class DummyChildBizoToBeStoredOnXmlColumn : NonPersistentBusinessObject
	{
		public DummyChildBizoToBeStoredOnXmlColumn()
		{
		}

		#region Properties

		[XmlColumnProperty]
		public ZString ChildProperty1
		{
			get { return GetXmlColumnPropertyValue<ZString>(ChildProperty1Info); }
			set { SetXmlColumnPropertyValue(ChildProperty1Info, value); }
		}

		public ZPropertyInfo ChildProperty1Info
		{
			get { return GetZPropertyInfo(nameof(ChildProperty1)); }
		}

		[XmlColumnProperty]
		public ZInt ChildProperty2
		{
			get { return GetXmlColumnPropertyValue<ZInt>(ChildProperty2Info); }
			set { SetXmlColumnPropertyValue(ChildProperty2Info, value); }
		}

		public ZPropertyInfo ChildProperty2Info
		{
			get { return GetZPropertyInfo(nameof(ChildProperty2)); }
		}

		#endregion

		#region Related Business Objects

		[XmlColumnProperty]
		public DummyChildBizoToBeStoredOnXmlColumnCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new DummyChildBizoToBeStoredOnXmlColumnCollection();
					RegisterEditableChildObject(collection);
				}

				return collection;
			}
		}

		DummyChildBizoToBeStoredOnXmlColumnCollection collection;

		[ChildEditable]
		[XmlColumnProperty(DummyBusinessObject.Schema.Z0_Xml)]
		public virtual DummyGrandchildBizoToBeStoredOnXmlColumn RecursiveChildBusinessObject
		{
			get
			{
				if (recursiveChildBusinessObject == null)
				{
					recursiveChildBusinessObject = new DummyGrandchildBizoToBeStoredOnXmlColumn();
					RegisterEditableChildObject(recursiveChildBusinessObject);
				}

				return recursiveChildBusinessObject;
			}
		}
		DummyGrandchildBizoToBeStoredOnXmlColumn recursiveChildBusinessObject;

		#endregion
	}

	class DummyGrandchildBizoToBeStoredOnXmlColumn : DummyGreatGrandchildBizoToBeStoredOnXmlColumn
	{
		[ChildEditable]
		[XmlColumnProperty(DummyBusinessObject.Schema.Z0_Xml)]
		public virtual DummyGreatGrandchildBizoToBeStoredOnXmlColumn RecursiveChildBusinessObject
		{
			get
			{
				if (recursiveChildBusinessObject == null)
				{
					recursiveChildBusinessObject = new DummyGreatGrandchildBizoToBeStoredOnXmlColumn();
					RegisterEditableChildObject(recursiveChildBusinessObject);
				}

				return recursiveChildBusinessObject;
			}
		}
		DummyGreatGrandchildBizoToBeStoredOnXmlColumn recursiveChildBusinessObject;
	}

	class DummyGreatGrandchildBizoToBeStoredOnXmlColumn : NonPersistentBusinessObject
	{
		#region Properties

		[XmlColumnProperty]
		public ZString ChildProperty1
		{
			get { return GetXmlColumnPropertyValue<ZString>(ChildProperty1Info); }
			set { SetXmlColumnPropertyValue(ChildProperty1Info, value); }
		}

		public ZPropertyInfo ChildProperty1Info
		{
			get { return GetZPropertyInfo(nameof(ChildProperty1)); }
		}

		#endregion
	}

	#endregion
}
