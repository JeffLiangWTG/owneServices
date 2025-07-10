using System;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Customs.JP.Shared.GUI.Testing;

sealed class MessageVisualObjectUserControlTest : TestCaseWithFactory
{
	public void TestOnInitialized()
	{
		var provider = GetVisualObjectParentProvider();

		var messageVisualObjectParent = provider.VisualObjectParent;
		var visualObject = messageVisualObjectParent.VisualObjects[0];

		using (var form = new ZForm(provider))
		{
			var control = new MessageVisualObjectUserControl();
			control.SetBindingMember("VisualObjectParent");

			form.Controls.Add(control);
			form.Show();

			var mainSplitContainer = form.FindSingleOrDefault<KSplitContainer>("MainSplitContainer");
			var itemsCurrentNumberCalcEdit = form.FindSingleOrDefault<ZCalcEdit>("ItemsCurrentNumberCalcEdit");
			var objectsCurrentNumberCalcEdit = form.FindSingleOrDefault<ZCalcEdit>("ObjectsCurrentNumberCalcEdit");
			var objectsPreNextPanel = form.FindSingleOrDefault<ZPanel>("ObjectsPreNextPanel");
			var deleteItemButton = form.FindSingleOrDefault<ZButton>("BtnDeleteItem");
			var newItemButton = form.FindSingleOrDefault<ZButton>("BtnNewItem");

			CombineAssertions(() =>
			{
				AssertNotNull("MainSplitContainer", mainSplitContainer);
				AssertNotNull("ItemsCurrentNumberCalcEdit", itemsCurrentNumberCalcEdit);
				AssertNotNull("ObjectsCurrentNumberCalcEdit", objectsCurrentNumberCalcEdit);
				AssertNotNull("ObjectsPreNextPanel", objectsPreNextPanel);
				AssertNotNull("DeleteItemButton", deleteItemButton);
				AssertNotNull("NewItemButton", newItemButton);

				Assert("SubItems - Page index should not be negative.", !itemsCurrentNumberCalcEdit.AllowNegative);
				Assert("Visual Object - Page index should not be negative.", !objectsCurrentNumberCalcEdit.AllowNegative);

				AssertEquals(visualObject.NumberOfResults, (int)itemsCurrentNumberCalcEdit.MaxValue);
				AssertEquals(messageVisualObjectParent.NumberOfResults, (int)objectsCurrentNumberCalcEdit.MaxValue);

				Assert("Should be visible as there are more than 1 VisualObject.", objectsPreNextPanel.Visible);
				Assert("Should be expanded as there is at least one sub items.", !mainSplitContainer.Panel2Collapsed);
			});

			visualObject.AllChildItems.Clear();
			messageVisualObjectParent.VisualObjects.RemoveAndDeleteAll();

			control.SetDataBinding(provider, "VisualObjectParent");

			CombineAssertions(() =>
			{
				Assert("Should be collapsed as there are not any sub items.", mainSplitContainer.Panel2Collapsed);
				Assert("Should be hidden as there are not any VisualObjects.", !objectsPreNextPanel.Visible);
			});
		}
	}

	public void TestEditableItemsGrid()
	{
		using (var control = new MessageVisualObjectUserControl())
		{
			void AssertGrid(string name)
			{
				var grid = control.FindSingle<ZGrid>(name);

				AssertNotNull(name + " - Column No", grid.GetColumnStyle("No"));
				AssertNotNull(name + " - Column ID", grid.GetColumnStyle("ID"));
				AssertNotNull(name + " - Column Name", grid.GetColumnStyle("Name"));
				AssertNotNull(name + " - Column JPName", grid.GetColumnStyle("JPName"));
				AssertNotNull(name + " - Column OriginalValue", grid.GetColumnStyle("OriginalValue"));
				AssertNotNull(name + " - Column Length", grid.GetColumnStyle("Length"));
				AssertNotNull(name + " - Column Instruction", grid.GetColumnStyle("Instruction"));
				AssertNotNull(name + " - Column OverrideValue", grid.GetColumnStyle("OverrideValue"));
			}

			CombineAssertions(() =>
			{
				AssertGrid("HeaderGrid");
				AssertGrid("ItemsGrid");
			});
		}
	}

	public void TestObjectsPreviousButtonAndNextButton()
	{
		var provider = GetVisualObjectParentProvider();
		var messageVisualObjectParent = provider.VisualObjectParent;

		using (var form = new ZForm(provider))
		{
			var control = new MessageVisualObjectUserControl();
			control.SetBindingMember("VisualObjectParent");

			form.Controls.Add(control);
			form.Show();

			var nextButton = form.FindSingle<ZButton>("ObjectsNextButton");
			var previousButton = form.FindSingle<ZButton>("ObjectsPreviousButton");

			AssertNotNull(nextButton.Image);
			AssertNotNull(previousButton.Image);

			AssertEquals("Precondition", 2, messageVisualObjectParent.VisualObjects.Count);

			AssertEquals("CanMoveNext", messageVisualObjectParent.CanMoveNext, nextButton.IsEnabledForBinding);
			AssertEquals("CanMovePrevious", messageVisualObjectParent.CanMovePrevious, previousButton.IsEnabledForBinding);

			nextButton.PerformClick();
			AssertEquals(2, messageVisualObjectParent.CurrentNumber);

			AssertEquals("CanMoveNext", messageVisualObjectParent.CanMoveNext, nextButton.IsEnabledForBinding);
			AssertEquals("CanMovePrevious", messageVisualObjectParent.CanMovePrevious, previousButton.IsEnabledForBinding);

			previousButton.PerformClick();
			AssertEquals(1, messageVisualObjectParent.CurrentNumber);

			AssertEquals("CanMoveNext", messageVisualObjectParent.CanMoveNext, nextButton.IsEnabledForBinding);
			AssertEquals("CanMovePrevious", messageVisualObjectParent.CanMovePrevious, previousButton.IsEnabledForBinding);
		}
	}

	public void TestItemsPreviousButtonAndNextButton()
	{
		var provider = GetVisualObjectParentProvider();
		var messageVisualObjectParent = provider.VisualObjectParent;
		var visualObject = messageVisualObjectParent.VisualObjects[0];

		using (var form = new ZForm(provider))
		{
			var control = new MessageVisualObjectUserControl();
			control.SetBindingMember("VisualObjectParent");

			form.Controls.Add(control);
			form.Show();

			var nextButton = form.FindSingle<ZButton>("ItemsNextButton");
			var previousButton = form.FindSingle<ZButton>("ItemsPreviousButton");

			AssertNotNull(nextButton.Image);
			AssertNotNull(previousButton.Image);

			AssertEquals("Precondition", 2, visualObject.CurrentItems.Count);

			AssertEquals("CanMoveNext", visualObject.CanMoveNext, nextButton.IsEnabledForBinding);
			AssertEquals("CanMovePrevious", visualObject.CanMovePrevious, previousButton.IsEnabledForBinding);

			nextButton.PerformClick();
			AssertEquals(2, visualObject.CurrentNumber);

			AssertEquals("CanMoveNext", visualObject.CanMoveNext, nextButton.IsEnabledForBinding);
			AssertEquals("CanMovePrevious", visualObject.CanMovePrevious, previousButton.IsEnabledForBinding);

			previousButton.PerformClick();
			AssertEquals(1, visualObject.CurrentNumber);

			AssertEquals("CanMoveNext", visualObject.CanMoveNext, nextButton.IsEnabledForBinding);
			AssertEquals("CanMovePrevious", visualObject.CanMovePrevious, previousButton.IsEnabledForBinding);
		}
	}

	public void TestNewItemButtonAndDeleteItemButton()
	{
		var provider = GetVisualObjectParentProvider();
		var messageVisualObjectParent = provider.VisualObjectParent;
		var visualObject = messageVisualObjectParent.VisualObjects[0];

		using (var form = new ZForm(provider))
		{
			var control = new MessageVisualObjectUserControl();
			control.SetBindingMember("VisualObjectParent");

			form.Controls.Add(control);
			form.Show();

			var newItemButton = form.FindSingle<ZButton>("BtnNewItem");
			var deleteItemButton = form.FindSingle<ZButton>("BtnDeleteItem");

			AssertNotNull(newItemButton.Image);
			AssertNotNull(deleteItemButton.Image);

			AssertEquals("Precondition", 2, visualObject.AllChildItems.Count);

			AssertEquals("CanCreateNewItem", visualObject.CanCreateNewItem, newItemButton.IsEnabledForBinding);
			AssertEquals("CanDeleteItem", visualObject.CanDeleteItem, deleteItemButton.IsEnabledForBinding);

			newItemButton.PerformClick();

			AssertEquals("CanCreateNewItem", visualObject.CanCreateNewItem, newItemButton.IsEnabledForBinding);
			AssertEquals("CanDeleteItem", visualObject.CanDeleteItem, deleteItemButton.IsEnabledForBinding);

			deleteItemButton.PerformClick();
			AssertEquals("CanCreateNewItem", visualObject.CanCreateNewItem, newItemButton.IsEnabledForBinding);
			AssertEquals("CanDeleteItem", visualObject.CanDeleteItem, deleteItemButton.IsEnabledForBinding);
		}
	}

	IMessageVisualObjectParentProvider GetVisualObjectParentProvider()
	{
		var messageVisualObjectParent = new MessageVisualObjectParent(Factory);

		messageVisualObjectParent.VisualObjects.AddRange(new[]
		{
			GetMessageVisualObject(),
			GetMessageVisualObject(),
		});

		messageVisualObjectParent.CurrentNumber = 1;

		var providerMock = new Mock<IMessageVisualObjectParentProvider>();
		providerMock.Setup(c => c.VisualObjectParent).Returns(messageVisualObjectParent);

		return providerMock.Object;
	}

	MessageVisualObject GetMessageVisualObject()
	{
		var providerMock = new Mock<IMessageContentProvider>();
		providerMock.Setup(provider => provider.Factory).Returns(Factory);
		providerMock.Setup(provider => provider.GetMessageData()).Returns(Array.Empty<byte>());
		providerMock.Setup(provider => provider.ProcedureCode).Returns("TST");

		var result = new MessageVisualObject(providerMock.Object);

		result.Header.Add(GetEditableFieldBizObject());

		EditableFieldBizObjectCollection GetSubItemCollection()
		{
			var collection = new EditableFieldBizObjectCollection(Factory);
			collection.AddRange(new[]
			{
				GetEditableFieldBizObject(),
				GetEditableFieldBizObject()
			});

			return collection;
		}

		result.AllChildItems.Add(GetSubItemCollection());
		result.AllChildItems.Add(GetSubItemCollection());

		result.CurrentNumber = 1;

		return result;
	}

	EditableFieldBizObject GetEditableFieldBizObject()
	{
		var editableMessageFieldMock = new Mock<IEditableMessageField>();
		editableMessageFieldMock.Setup(field => field.FieldDefinition).Returns(new FieldDefinition());

		return new EditableFieldBizObject(JPProcedureCodeList.Codes.EDA, editableMessageFieldMock.Object);
	}
}
