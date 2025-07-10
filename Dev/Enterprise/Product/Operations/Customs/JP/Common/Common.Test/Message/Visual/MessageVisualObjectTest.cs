using System;
using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(MessageVisualObject))]
sealed class MessageVisualObjectTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		return new MessageVisualObject(ContentProvider);
	}

	public void TestCurrentNumber()
	{
		var visualParent = new MessageVisualObject(ContentProvider);
		AssertNoExceptionThrown(() => visualParent.CurrentNumber = 10);
		AssertNoExceptionThrown(() => visualParent.CurrentNumber = -1);

		var sourceField = new EditableMessageField(new FieldDefinition());
		var editableFieldBizObj1 = new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);
		var editableFieldBizObj2 = new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);

		var collection1 = new EditableFieldBizObjectCollection(Factory) { editableFieldBizObj1 };
		var collection2 = new EditableFieldBizObjectCollection(Factory) { editableFieldBizObj2 };

		visualParent.AllChildItems.Add(collection1);
		visualParent.AllChildItems.Add(collection2);

		AssertEquals("Pre-Condition", 2, visualParent.NumberOfResults);
		AssertEquals("Pre-Condition", -1, visualParent.CurrentNumber);

		visualParent.CurrentNumber = 1;
		AssertSame("Should populate the first record", editableFieldBizObj1, visualParent.CurrentItems.Single());
	}

	public void TestMovePreviousAndMoveNext()
	{
		var sourceField = new EditableMessageField(new FieldDefinition());
		var editableFieldBizObj1 = new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);
		var editableFieldBizObj2 = new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);

		var collection1 = new EditableFieldBizObjectCollection(Factory) { editableFieldBizObj1 };
		var collection2 = new EditableFieldBizObjectCollection(Factory) { editableFieldBizObj2 };

		var visualParent = new MessageVisualObject(ContentProvider);
		visualParent.AllChildItems.Add(collection1);
		visualParent.AllChildItems.Add(collection2);

		AssertEquals("Pre-Condition", 0, visualParent.CurrentNumber);

		visualParent.CurrentNumber = 1;

		Assert("The current item is the first one.", visualParent.CanMoveNext);
		Assert("The current item is the first one.", !visualParent.CanMovePrevious);

		visualParent.MoveNext();
		AssertEquals(2, visualParent.CurrentNumber);

		Assert("The current item is the last one.", !visualParent.CanMoveNext);
		Assert("The current item is the last one.", visualParent.CanMovePrevious);

		visualParent.MovePrevious();
		AssertEquals(1, visualParent.CurrentNumber);

		AssertExceptionThrown<NotSupportedException>("Should throw exception as the current index is 1.", "Can't move before the first item.", () => visualParent.MovePrevious());

		visualParent.CurrentNumber = 2;
		AssertExceptionThrown<NotSupportedException>("Should throw exception as the current index is 2.", "Can't move next when on the last item.", () => visualParent.MoveNext());
	}

	public void TestNewItemAndDeleteItem()
	{
		var sourceField = new EditableMessageField(new FieldDefinition());
		var editableFieldBizObj1 = new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);
		var editableFieldBizObj2 = new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, sourceField);

		var collection1 = new EditableFieldBizObjectCollection(Factory) { editableFieldBizObj1 };
		var collection2 = new EditableFieldBizObjectCollection(Factory) { editableFieldBizObj2 };

		var visualParent = new MessageVisualObject(ContentProvider);
		visualParent.AllChildItems.Add(collection1);
		visualParent.AllChildItems.Add(collection2);

		visualParent.Initialize();

		Assert("The current item is the first one.", visualParent.CanMoveNext);
		Assert("The current item is the first one.", !visualParent.CanMovePrevious);
		Assert("The current item is the first one.", visualParent.CanCreateNewItem);
		Assert("The current item is the first one.", !visualParent.CanDeleteItem);

		visualParent.NewEmptyChildItem();

		AssertEquals(3, visualParent.CurrentNumber);
		Assert("The current item is the last one.", visualParent.CanMovePrevious);
		Assert("The current item is the last one.", !visualParent.CanMoveNext);
		Assert("The current item is the last one.", visualParent.CanDeleteItem);

		visualParent.DeleteCurrentChildItem();

		AssertEquals(2, visualParent.CurrentNumber);
		Assert("The current item is the last one.", !visualParent.CanMoveNext);
		Assert("The current item is the last one.", visualParent.CanMovePrevious);
		Assert("readonly items should not been delete.", !visualParent.CanDeleteItem);
	}

	public void TestInitializeAndClean()
	{
		var messageData = ContentProvider.GetMessageData();

		var visualParent = new MessageVisualObject(ContentProvider);
		visualParent.Initialize();

		CombineAssertions(() =>
		{
			AssertEquals(1, visualParent.CurrentNumber);
			Assert("Should populate Header Items.", visualParent.Header.Count > 0);
			Assert("Should populate Sub Items.", visualParent.AllChildItems.Count > 0);
			Assert("Should move to the first item.", visualParent.CurrentItems.Count > 0);

			var expectedHeaderFieldInfos = JPMessageUtils.ConvertMessageToString(ReadTestFile("HeaderFieldInfos.raw"));
			var actualHeaderFieldInfos = string.Join(System.Environment.NewLine, visualParent.Header.Cast<EditableFieldBizObject>().Select(c => $"{c.No} + {c.Name} + {c.JPName} + {c.ID}"));

			var expectedSubItemFieldInfos = JPMessageUtils.ConvertMessageToString(ReadTestFile("SubItemFieldInfos.raw"));
			var actualSubItemFieldInfos = string.Join(System.Environment.NewLine, visualParent.AllChildItems.SelectMany(x => x.Cast<EditableFieldBizObject>().Select(c => $"{c.No} + {c.Name} + {c.JPName} + {c.ID}")));

			AssertEquals("Header - Should export correct Name, JPName and ID.", expectedHeaderFieldInfos, actualHeaderFieldInfos);
			AssertEquals("AllChildItems - Should export correct Name, JPName and ID.", expectedSubItemFieldInfos, actualSubItemFieldInfos);
		});

		visualParent.Clean();

		CombineAssertions(() =>
		{
			AssertEquals(0, visualParent.CurrentNumber);
			AssertEquals("Should clean all Header Items.", 0, visualParent.Header.Count);
			AssertEquals("Should clean all Sub Items.", 0, visualParent.AllChildItems.Count);
			AssertEquals("Should reset the first item.", 0, visualParent.CurrentItems.Count);
		});
	}

	public void TestBuildMessage()
	{
		var messageData = ContentProvider.GetMessageData();

		var visualParent = new MessageVisualObject(ContentProvider);
		visualParent.Initialize();

		AssertArrayEqualsByElements("Should use the last message data if there are not any changes.", messageData, visualParent.BuildMessage());

		var subItem = visualParent.CurrentItems[0];
		subItem.OverrideValue = "X";

		var expectedMessageData = ReadTestFile("ChangedEDAMessage.raw");
		var actualMessageData = visualParent.BuildMessage();

		AssertArrayEqualsByElements("Should export the message data from the override value.", expectedMessageData, actualMessageData);
	}

	byte[] ReadTestFile(string fileName)
	{
		using (var stream = GetType().Assembly.GetManifestResourceStream($"Enterprise.Customs.JP.Common.Testing.Message.Visual.TestFiles.{fileName}"))
		{
			return stream.ToByteArray();
		}
	}

	IMessageContentProvider ContentProvider
	{
		get
		{
			if (contentProvider == null)
			{
				var messageData = ReadTestFile("EDAMessage.raw");

				var mockContentProvider = new Mock<IMessageContentProvider>();
				mockContentProvider.Setup(c => c.Factory).Returns(Factory);
				mockContentProvider.Setup(c => c.ProcedureCode).Returns(JPProcedureCodeList.Codes.EDA);
				mockContentProvider.Setup(c => c.GetMessageData()).Returns(messageData);

				contentProvider = mockContentProvider.Object;
			}

			return contentProvider;
		}
	}
	IMessageContentProvider contentProvider;
}
