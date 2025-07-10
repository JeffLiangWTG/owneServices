using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageMessageSendingObjectParent))]
sealed class TemporaryStorageMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestSendingObjectCollection()
	{
		var sendingObjectCollection = ((TemporaryStorageMessageSendingObjectParent)GetNewBusinessObject()).SendingObjectsCollection;

		CombineAssertions(() =>
		{
			AssertType<TemporaryStorageMessageSendingObjectCollection>(sendingObjectCollection);
			AssertEquals("Number of items in SendingObjectCollection", 1, sendingObjectCollection.Count);
		});
	}

	public void TestMessageSendingObjectProperties()
	{
		var messageProperties = ((TemporaryStorageMessageSendingObjectParent)GetNewBusinessObject()).MessageSendingObjectProperties.ToList();
		CombineAssertions(() =>
		{
			AssertEquals(5, messageProperties.Count);
			AssertMessageProperty(messageProperties, TemporaryStorageMessageSendingObject.Schema.DeclarationType, true, 100);
			AssertMessageProperty(messageProperties, TemporaryStorageMessageSendingObject.BeSchema.IsTestDeclaration, false, 50);
			AssertMessageProperty(messageProperties, TemporaryStorageMessageSendingObject.Schema.MessageType, true, 75, new ResourceStringData("BE.PNTSSendingObjectParentTest|MessageType", "Entry Type"));
			AssertMessageProperty(messageProperties, TemporaryStorageMessageSendingObject.Schema.EntryStatus, true, 100);
			AssertMessageProperty(messageProperties, TemporaryStorageMessageSendingObject.BeSchema.ReferenceNumber, true);
		});
	}

	void AssertMessageProperty(List<MessageSendingObjectProperty> properties, string propertyName, bool expectedMandatory = false, int expectedColumnWidth = 200, ResourceStringData expectedResourceString = null )
	{
		var property = properties.Find(x => x.PropertyName == propertyName);
		AssertNotNull($"Property {propertyName} exist.", property);
		if (property != null)
		{
			AssertEquals($"Property {propertyName} mandatory.", expectedMandatory, property.IsMandatory);
			AssertEquals($"Property {propertyName} columnWidth.", expectedColumnWidth, property.ColumnWidth);
			AssertEquals($"Property {propertyName} caption", expectedResourceString?.Caption, property.ResourceString?.Caption);
		}
	}

	protected override BusinessObject GetNewBusinessObject() => new TemporaryStorageMessageSendingObjectParent(Factory.New<TemporaryStorageHeader>());
}
