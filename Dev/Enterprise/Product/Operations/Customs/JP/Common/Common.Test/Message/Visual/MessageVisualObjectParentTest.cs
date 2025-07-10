using System;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(MessageVisualObjectParent))]
sealed class MessageVisualObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestInitializeVisualObjects()
	{
		var parent = new MessageVisualObjectParent(Factory);
		parent.InitializeVisualObjects(null);

		AssertEquals(0, parent.VisualObjects.Count);
		AssertEquals(0, parent.CurrentNumber);

		parent.InitializeVisualObjects(new[]
		{
			GetMessageContentProvider(),
			GetMessageContentProvider()
		});

		AssertEquals(2, parent.VisualObjects.Count);
		AssertEquals(1, parent.CurrentNumber);
	}

	public void TestLazyInitializeVisualObj()
	{
		var parent = new MessageVisualObjectParent(Factory);
		parent.InitializeVisualObjects(new[]
		{
			GetMessageContentProvider(),
			GetMessageContentProvider()
		});

		var visualObject1 = parent.VisualObjects[0];
		var visualObject2 = parent.VisualObjects[1];

		var initializedCount1 = 0;
		var initializedCount2 = 0;

		visualObject1.OnInitialized += (s, e) => { initializedCount1++; };
		visualObject2.OnInitialized += (s, e) =>
		{
			initializedCount2++;
			visualObject2.Header.Add(GetEditableFieldBizObject());
		};

		visualObject1.Header.Add(GetEditableFieldBizObject());

		CombineAssertions(() =>
		{
			Assert(parent.MoveNext());
			AssertEquals("Should initialize the visualObject2 as it's empty.", 1, initializedCount2);

			Assert(parent.MovePrevious());
			AssertEquals("Should don't initialize the visualObject1 as its header has at least one data.", 0, initializedCount1);

			Assert(parent.MoveNext());
			AssertEquals("Should don't initialize the visualObject1.", 0, initializedCount1);
			AssertEquals("Should don't initialize the visualObject2 as its header has at least one data.", 1, initializedCount2);
		});
	}

	public void TestMovePreviousAndMoveNext()
	{
		var parent = new MessageVisualObjectParent(Factory);

		parent.InitializeVisualObjects(new[]
		{
			GetMessageContentProvider(),
			GetMessageContentProvider()
		});

		CombineAssertions(() =>
		{
			AssertEquals("Pre-Condition", 1, parent.CurrentNumber);
			Assert("The current item is the first one.", parent.CanMoveNext);
			Assert("The current item is the first one.", !parent.CanMovePrevious);

			Assert(parent.MoveNext());
			AssertEquals(2, parent.CurrentNumber);

			Assert("The current item is the last one.", !parent.CanMoveNext);
			Assert("The current item is the last one.", parent.CanMovePrevious);

			Assert(parent.MovePrevious());
			AssertEquals(1, parent.CurrentNumber);

			AssertExceptionThrown<NotSupportedException>("Should throw exception as the current index is 1.", "Can't move before the first item.", () => parent.MovePrevious());

			Assert(parent.MoveNext());
			AssertExceptionThrown<NotSupportedException>("Should throw exception as the current index is 2.", "Can't move next when on the last item.", () => parent.MoveNext());
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return new MessageVisualObjectParent(Factory);
	}

	IMessageContentProvider GetMessageContentProvider()
	{
		var providerMock = new Mock<IMessageContentProvider>();
		providerMock.Setup(provider => provider.Factory).Returns(Factory);
		providerMock.Setup(provider => provider.GetMessageData()).Returns(Array.Empty<byte>());
		providerMock.Setup(provider => provider.ProcedureCode).Returns("TST");

		return providerMock.Object;
	}

	EditableFieldBizObject GetEditableFieldBizObject()
	{
		var editableMessageFieldMock = new Mock<IEditableMessageField>();
		editableMessageFieldMock.Setup(field => field.FieldDefinition).Returns(new FieldDefinition());

		return new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, editableMessageFieldMock.Object);
	}
}
