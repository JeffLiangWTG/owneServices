using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Security;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZModuleButtonGridTestForm))]
	sealed class ZModuleButtonGridTest : ZFormBasherTest
	{
		public void TestRowDeleteSecurityRightCheckStrategy()
		{
			using (var grid = new ZModuleButtonGrid())
			{
				AssertEquals(typeof(GridRowDeleteSecurityRightCheckDefaultStrategy), grid.RowDeleteSecurityRightCheckStrategy.GetType());
				grid.InnerGrid.RemoveAction = RemoveAction.Remove;
				AssertEquals(typeof(GridRowDeleteSecurityRightCheckDefaultStrategy), grid.RowDeleteSecurityRightCheckStrategy.GetType());
			}
		}

		public void TestConstructor_StandardAndCustomButtonsAreSpecified_AddButtonsToGridInSameOrder()
		{
			using (ZModuleButtonGridTestForm form = new ZModuleButtonGridWithCustomButtonTestForm(Dummy))
			{
				form.Show();
				Application.DoEvents();

				var grid = (ZDummyModuleButtonGridWithCustomButton)form.Grid;

				var attachButton = grid.AttachButton;
				var detachButton = grid.DetachButton;
				var newButton = grid.NewButton;
				var editButton = grid.EditButton;
				var customButton = grid.CustomButton;

				AssertNotNull(attachButton);
				AssertNotNull(detachButton);
				AssertNotNull(newButton);
				AssertNotNull(editButton);
				AssertNotNull(customButton);

				var expecteButtons = new[] { newButton.Name, editButton.Name, customButton.Name, attachButton.Name, detachButton.Name };
				var toolStrip = (ZToolStrip)form.Controls.Find("toolStrip", true)[0];
				var actualButtons = toolStrip.Items.Cast<ToolStripItem>().Select(x => x.Name).ToArray();
				AssertArrayEqualsByElements(expecteButtons, actualButtons);
			}
		}

		public void TestModuleDecisionProvider_HandlesOkClick()
		{
			var dummy = Dummy;
			Factory.Save();

			using (var form = new ZModuleButtonGridWithCustomButtonTestForm(dummy))
			using (var module = new DummyFilterGridModule())
			using (var popup = new EmbeddedModulePopup(module))
			{
				var grid = (ZDummyModuleButtonGridWithCustomButton)form.Grid;

				var findBox = new Mock<IFindBox>();
				findBox.Setup(box => box.PopupForm).Returns(popup);

				var decisionProvider = grid.GetNewModuleDecisionProvider(findBox.Object);

				decisionProvider.HandleFindBoxOKButton(new[] { dummy });

				AssertNoExceptionThrown(
					message: "We shouldn't touch the code property when there is no guarantee one exists",
					codeToRun: () =>
					{
						findBox.Verify(m => m.Code, Times.Never);
					});
			}
		}

		public void TestReadOnlyButtons()
		{
			Form.DisplayMode = ODisplayMode.ReadOnly;
			Form.Show();
			Assert(!form.Grid.NewButton.Enabled);
			Assert(!form.Grid.EditButton.Enabled);
			AssertEquals("View", form.Grid.EditButton.Text);
			Assert(!form.Grid.AttachButton.Enabled);
			Assert(!form.Grid.DetachButton.Enabled);

			Dummy.Collection.AddNew();
			Assert(form.Grid.EditButton.Enabled);
		}

		public void TestButtonsArentTabStops()
		{
			using (var form = new ZModuleButtonGridTestForm(Dummy))
			{
				var toolStrip = form.Controls.Find("toolStrip", true).First();
				AssertEquals(false, toolStrip.TabStop);
			}
		}

		public void TestActionMenuItems()
		{
			const string ActionsMenuItem = "Actions";
			const string AddedMenuItem1 = "Button Grid Menu Item 1";
			const string AddedMenuItem3 = "Button Grid Menu Item 3";

			using (var form = new ZModuleButtonGridTestForm(Dummy))
			{
				form.Show();
				Application.DoEvents();

				AssertNull(form.Grid.InnerGrid.ContextMenu.MenuItems[ActionsMenuItem]);
				AssertNull(form.Grid.InnerGrid.ContextMenu.MenuItems[AddedMenuItem1]);
				AssertNull(form.Grid.InnerGrid.ContextMenu.MenuItems[AddedMenuItem3]);
			}

			using (new DummyFilterGridModule.ModulePluginProvider(DummyControllerIDs.Dummy1))
			using (var form = new ZModuleButtonGridTestForm(Dummy))
			{
				form.Show();
				Application.DoEvents();

				AssertNull("No need for an actions menu", form.Grid.InnerGrid.ContextMenu.MenuItems[ActionsMenuItem]);
				MenuAssertion.AssertHasMenu("Added 1 menu item", form.Grid.InnerGrid.ContextMenu, AddedMenuItem1);
				AssertNull("menu item 3 was not added", form.Grid.InnerGrid.ContextMenu.MenuItems[AddedMenuItem3]);
			}

			using (new DummyFilterGridModule.ModulePluginProvider(DummyControllerIDs.Dummy1, DummyControllerIDs.Dummy3))
			using (var form = new ZModuleButtonGridTestForm(Dummy))
			{
				form.Show();
				Application.DoEvents();

				MenuAssertion.AssertHasMenu("Added 2 menu items", form.Grid.InnerGrid.ContextMenu, ActionsMenuItem, AddedMenuItem1);
				MenuAssertion.AssertHasMenu("Added 2 menu items", form.Grid.InnerGrid.ContextMenu, ActionsMenuItem, AddedMenuItem3);
				AssertNull("menu item 1 should not be added twice", form.Grid.InnerGrid.ContextMenu.MenuItems[AddedMenuItem1]);
				AssertNull("menu item 3 should not be added twice", form.Grid.InnerGrid.ContextMenu.MenuItems[AddedMenuItem3]);
			}
		}

		public void TestButtonGridEditWhenContinueWithSaveFalse()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			AssertNull("No previous shown form (expected initial state)", Form.Grid.LastShownZForm);

			Form.ExposedContinueWithSave = ContinueWithSave.No;
			Form.Grid.InnerGrid.Select(0);
			Form.Grid.EditButton.PerformClick();
			AssertNull("Should not have shown form and not had an exception", Form.Grid.LastShownZForm);
		}

		public void TestButtonGridNewWhenContinueWithSaveFalse()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			AssertNull("Should not have shown form", Form.Grid.LastShownZForm);
			Form.ExposedContinueWithSave = ContinueWithSave.No;
			Form.Grid.NewButton.PerformClick();

			AssertNull("Should not have shown form and not had an exception", Form.Grid.LastShownZForm);
		}

		public void TestReadOnlyMetaDataProperty()
		{
			using (var grid = new ZModuleButtonGrid())
			{
				var readOnlyProperty = BindableComponentMetaDataPropertyLocator.GetDefaultMetaDataProperty(typeof(ZModuleButtonGrid), MetaDataTypes.ReadOnly);
				AssertEquals("The control ReadOnly meta-data is used", "ReadOnly", readOnlyProperty.Name);
			}
		}

		#region TestEditFormClosed

		public void TestEditFormClosed()
		{
			Form.Grid.EditFormClosed += new EventHandler(Grid_EditFormClosed);
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			Dummy.Factory.Save();

			AssertNull("PreCondition: Should not have shown form", Form.Grid.LastShownZForm);
			Form.Grid.EditButton.PerformClick();

			AssertNotNull("PreCondition: Should have shown form", Form.Grid.LastShownZForm);
			Assert("Edit Form Closed should not have been fired", !IsEditFormClosedFired);

			((Form)Form.Grid.LastShownZForm).Close();
			Assert("Edit Form Closed should have been fired", IsEditFormClosedFired);
		}

		void Grid_EditFormClosed(object sender, EventArgs e)
		{
			IsEditFormClosedFired = true;
		}

		bool IsEditFormClosedFired;

		#endregion

		#region TestButtonGridNewWith_AllowNewWithoutSaving

		public void TestButtonGridNewWith_AllowNewWithoutSaving_ForLegacyCollection()
		{
			var parentBizO = Factory.NewWithValidTestData(typeof(DummyWithCollection)) as DummyWithCollection;
			using (var form = new ZModuleButtonGridTestForm(parentBizO))
			{
				form.Grid.AllowNewWithoutSaving = true;
				form.Show();
				AssertNull("Should not have shown form", form.Grid.LastShownZForm);
				form.Grid.NewButton.PerformClick();

				var newBizObjFormWithDifferentFactory = (ZDummyForm)form.Grid.LastShownZForm;
				AssertNotNull("Should have shown form and not had an exception", newBizObjFormWithDifferentFactory);
				((BusinessObject)newBizObjFormWithDifferentFactory.BusinessEntity).Factory.Save();
				newBizObjFormWithDifferentFactory.FireSaved();

				var childBizOInOtherFormFactory = (DummyBaseBusinessObject)newBizObjFormWithDifferentFactory.BusinessEntity;
				AssertEquals(
					"The child business entity cannot be attached yet as it's master hasn't been saved.",
					"", childBizOInOtherFormFactory.Z0_VarCharMax);

				var childBizOInCurrentFactory = Factory.Load<DummyBaseBusinessObject>(childBizOInOtherFormFactory.PK);
				AssertEquals(
					"There should be a local version of the child business object that has the foreign key set",
					"fk_set", childBizOInCurrentFactory.Z0_VarCharMax);

				form.Grid.LastShownZForm.Dispose();
			}
		}

		public void TestButtonGridNewWith_AllowNewWithoutSaving_ForActiveCollection()
		{
			var parentBizO = Factory.NewWithValidTestData(typeof(DummyWithCollection)) as DummyWithCollection;
			parentBizO.UseActiveCollection = true;
			using (var form = new ZModuleButtonGridTestForm(parentBizO))
			{
				form.Grid.AllowNewWithoutSaving = true;
				form.Show();
				AssertNull("Should not have shown form", form.Grid.LastShownZForm);
				form.Grid.NewButton.PerformClick();

				var newBizObjFormWithDifferentFactory = (ZDummyForm)form.Grid.LastShownZForm;
				AssertNotNull("Should have shown form and not had an exception", newBizObjFormWithDifferentFactory);
				((BusinessObject)newBizObjFormWithDifferentFactory.BusinessEntity).Factory.Save();
				newBizObjFormWithDifferentFactory.FireSaved();

				var childBizOInOtherFormFactory = (DummyBaseBusinessObject)newBizObjFormWithDifferentFactory.BusinessEntity;
				AssertEquals(
					"The child business entity cannot be attached yet as it's master hasn't been saved.",
					"", childBizOInOtherFormFactory.Z0_VarCharMax);

				var childBizOInCurrentFactory = Factory.Load<DummyBaseBusinessObject>(childBizOInOtherFormFactory.PK);
				AssertEquals(
					"There should be a local version of the child business object that has the foreign key set",
					"fk_set", childBizOInCurrentFactory.Z0_VarCharMax);

				form.Grid.LastShownZForm.Dispose();
			}
		}

		class DummyWithCollection : DummyBusinessObject
		{
			public DummyWithCollection(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool UseActiveCollection { get; set; }

			public new IDummyBusinessObjectCollection Collection
			{
				get
				{
					if (fCollection == null)
					{
						if (UseActiveCollection)
						{
							fCollection = new DummyBusinessObjectActiveCollectionWithMockForeignKey(Factory);
						}
						else
						{
							fCollection = new DummyBusinessObjectCollectionWithMockForeignKey(Factory);
						}
					}
					return fCollection;
				}
			}
			IDummyBusinessObjectCollection fCollection;
		}

		interface IDummyBusinessObjectCollection : IBusinessObjectCollection
		{
			new DummyBusinessObject this[int i] { get; }
		}

		class DummyBusinessObjectCollectionWithMockForeignKey : BusinessObjectCollection<DummyBusinessObject>, IDummyBusinessObjectCollection
		{
			public DummyBusinessObjectCollectionWithMockForeignKey(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override void SetCollectionRelationships(BusinessObject child)
			{
				base.SetCollectionRelationships(child);
				((DummyBusinessObject)child).Z0_VarCharMax = "fk_set";
			}

			protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
			{
				base.RemoveCollectionRelationshipsCore(child, forDelete);
				((DummyBusinessObject)child).Z0_VarCharMax = "";
			}
		}

		class DummyBusinessObjectActiveCollectionWithMockForeignKey : ActiveBusinessObjectCollection<DummyBusinessObject>, IDummyBusinessObjectCollection
		{
			public DummyBusinessObjectActiveCollectionWithMockForeignKey(BusinessObjectFactory factory)
				: base(factory, new DummyRelationship(typeof(DummyBusinessObject)))
			{
			}

			class DummyRelationship : CollectionRelationship
			{
				public DummyRelationship(Type elementType)
					: base(elementType)
				{
				}

				protected override bool SupportsAddToRelationshipCore()
				{
					return true;
				}

				protected override void AddToRelationship(BusinessObject businessObject)
				{
					((DummyBusinessObject)businessObject).Z0_VarCharMax = "fk_set";
				}
			}
		}

		#endregion

		public void TestAutoSelectOfFirstElementInGridForEdit()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			AssertNull("Should not have shown form", Form.Grid.LastShownZForm);
			Dummy.Factory.Save();

			Form.Grid.EditButton.PerformClick();
			AssertNotNull("Edit form shown", Form.Grid.LastShownZForm);
			Form.Grid.LastShownZForm.Dispose();

			Dummy.Collection.AddNew();
			Form.Grid.EditButton.PerformClick();
			Assert("Should have told user to select row", UnitTestUserNotification.Instance.LastMessage.WasError);
			if (Form.Grid.LastShownZForm != null)
			{
				Form.Grid.LastShownZForm.Dispose();
			}
		}

		public void TestAutoSelectOfFirstElementInGridForDetach()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			AssertNull("Should not have shown form", Form.Grid.LastShownZForm);
			Dummy.Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Form.Grid.DetachButton.PerformClick();
			AssertEquals("Dummy detached", 0, Dummy.Collection.Count);
			Assert("No meessage shown", UnitTestUserNotification.Instance.LastMessage.WasNone);

			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			AssertEquals("Initial count", 2, Dummy.Collection.Count);
			Form.Grid.DetachButton.PerformClick();
			AssertEquals("No Dummy detached", 2, Dummy.Collection.Count);
			Assert("Should have told user to select row", UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestNoDetatchOfElementBeingEdited()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			AssertNull("Should not have shown form", Form.Grid.LastShownZForm);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			Form.Grid.InnerGrid.Select(0);
			Form.Grid.EditButton.PerformClick();

			try
			{
				AssertNotNull("Should have shown form", Form.Grid.LastShownZForm);

				Form.Grid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Form.Grid.DetachButton.PerformClick();
				Assert("Should have error message", UnitTestUserNotification.Instance.LastMessage.Contains("Sorry"));
				AssertEquals("Not detach as open for edit", 1, Dummy.Collection.Count);
			}
			finally
			{
				Form.Grid.LastShownZForm.Dispose();
			}
		}

		public void TestNoDetatchOfNewUnsavedElement()
		{
			Form.Show();
			var pk = Dummy.Collection.AddNew().PK;
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			AssertNull("Should not have shown form", Form.Grid.LastShownZForm);
			AssertEquals("Not in db yet", false, Dummy.IsInDatabase);

			Form.Grid.InnerGrid.Select(0);

			try
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Form.Grid.DetachButton.PerformClick();
				AssertEquals("Deleted as object is new and not saved", 0, Dummy.Collection.Count);
				AssertNull(Factory.Load<DummyChildBusinessObject>(pk));
			}
			finally
			{
				if (Form.Grid.LastShownZForm != null)
				{
					Form.Grid.LastShownZForm.Dispose();
				}
			}
		}

		public void TestDetachedElementIsNotReAddedOnSave()
		{
			Form.Show();
			Dummy.Factory.Save();

			Form.Grid.NewButton.PerformClick();
			var childForm = (ZForm)Form.Grid.LastShownZForm;
			var childBO = (DummyBusinessObject)childForm.BusinessEntity;
			childForm.FireSaveButton();

			Form.Grid.DetachButton.PerformClick();
			Dummy.Factory.Save();

			childBO.Z0_AnotherDate = ZDateTime.Now;
			childForm.FireSaveButton();

			AssertEquals("childBO should not be in the collection", 0, Dummy.Collection.Count);
			Assert("childBO should be in database", childBO.IsInDatabase);
			Assert("Dummy should be in database", Dummy.IsInDatabase);

			childForm.Dispose();
		}

		public void TestDetachedElementIsNotReAddedOnNewFormSaved()
		{
			Form.Show();
			var initialCount = Dummy.Collection.Count;
			Form.Grid.NewButton.PerformClick();
			AssertNotNull("Should have shown form", Form.Grid.LastShownZForm);

			using (var newForm = (ZForm)Form.Grid.LastShownZForm)
			{
				newForm.FireSaveButton();
				Dummy.Collection.Load();
				AssertEquals("Should be new element added", initialCount + 1, Dummy.Collection.Count);

				Dummy.Collection.RemoveAll();
				newForm.FireSaveButton();
				AssertEquals("New element should not be re-added", 0, Dummy.Collection.Count);
			}
		}

		public void TestDetachMessage()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var grid = new ZModuleButtonGrid())
			{
				UnitTestUserNotification.Instance.AddOKAnswer();
				grid.ConfirmDetach(grid.DetachMessage.Caption);
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("All unsaved changes in detached items will be canceled."));
			}
		}

		public void TestCancelChangesOnDetatchOfChangedElement()
		{
			Form.Show();
			var child = Dummy.Collection.AddNew();
			child.Z0_VarCharMax = "aaa";
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			AssertNull("Should not have shown form", Form.Grid.LastShownZForm);
			Factory.Save();
			child.Z0_VarCharMax = "bbb";
			child.HasChanges = true;
			AssertEquals("Precondition", true, child.HasChanges);

			Form.Grid.InnerGrid.Select(0);

			try
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Form.Grid.DetachButton.PerformClick();
				AssertEquals(false, child.HasChanges);
				AssertEquals("Should be reverted to db value", "aaa", child.Z0_VarCharMax);
				AssertEquals("Detached as changes are cancelled", 0, Dummy.Collection.Count);
			}
			finally
			{
				if (Form.Grid.LastShownZForm != null)
				{
					Form.Grid.LastShownZForm.Dispose();
				}
			}
		}

		public void TestNoGridDeleteOfElementBeingEdited()
		{
			Form.Show();
			Form.Grid.InnerGrid.AllowReadOnlyRowsToBeDeleted = true;

			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			AssertNull("Should not have shown form", Form.Grid.LastShownZForm);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			Form.Grid.InnerGrid.Select(0);
			Form.Grid.EditButton.PerformClick();

			AssertEquals("Should be 1 object in list", 1, Form.Grid.BusinessObjectsOpenForEdit.Count);
			Assert("Should contain the dummy object", Form.Grid.BusinessObjectsOpenForEdit.Contains(Dummy.Collection[0]));

			try
			{
				AssertNotNull("Should have shown form", Form.Grid.LastShownZForm);

				Form.Grid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				KeySender.PostKeyDown(Form.Grid.InnerGrid, Keys.Delete);
				Application.DoEvents();
				Assert("Should have error message", UnitTestUserNotification.Instance.LastMessage.Contains("Sorry"));
				AssertEquals("Not deleted as open for edit", 1, Dummy.Collection.Count);
			}
			finally
			{
				Form.Grid.LastShownZForm.Dispose();
			}
		}

		public void TestGridDeleteSecurityRights()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Factory.Save();
			Form.Grid.ControllerForTest = new DummyControllerWithCustomisableSecurity();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			((DummyControllerWithCustomisableSecurity)Form.Grid.ControllerForTest).SetCheckPointForDelete(new DummyCheckPointWithSecuritySet(false));
			Form.Grid.InnerGrid.SelectAllElements();
			KeySender.PostKeyDown(Form.Grid.InnerGrid, Keys.Delete);
			Application.DoEvents();
			AssertContains("You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Should not be deleted", 3, Dummy.Collection.Count);

			((DummyControllerWithCustomisableSecurity)Form.Grid.ControllerForTest).SetCheckPointForDelete(new DummyCheckPointWithSecuritySet(true));
			Form.Grid.InnerGrid.UnSelectAll();
			Form.Grid.InnerGrid.Select(0);
			KeySender.PostKeyDown(Form.Grid.InnerGrid, Keys.Delete);
			Application.DoEvents();
			AssertEquals("Should be deleted", 2, Dummy.Collection.Count);
		}

		public void TestEditButtonChangesRowReadOnlyInNonReadOnlyGrid()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			AssertNull("Should not have shown form", Form.Grid.LastShownZForm);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			Form.Grid.InnerGrid.Select(0);
			Form.Grid.EditButton.PerformClick();
			AssertNotNull("Should have shown form", Form.Grid.LastShownZForm);
			AssertEquals("Being edited, so should be readonly", true, Dummy.Collection[0].ReadOnly);
			var firstForm = (ZForm)(Form.Grid.LastShownZForm);

			Dummy.Collection.AddNew();
			Form.Grid.InnerGrid.Select(1);
			Factory.Save();
			Form.Grid.EditButton.PerformClick();
			var secondForm = (ZForm)(Form.Grid.LastShownZForm);
			AssertEquals("Being edited, so should be readonly", true, Dummy.Collection[0].ReadOnly);
			AssertEquals("Being edited, so should be readonly", true, Dummy.Collection[1].ReadOnly);

			firstForm.Close();
			AssertEquals("No longer readonly as edit form closed", false, Dummy.Collection[0].ReadOnly);
			AssertEquals("Being edited, so should be readonly", true, Dummy.Collection[1].ReadOnly);

			secondForm.Close();
			AssertEquals("No longer readonly as edit form closed", false, Dummy.Collection[0].ReadOnly);
			AssertEquals("No longer readonly as edit form closed", false, Dummy.Collection[1].ReadOnly);
		}

		public void TestEditButtonLeavesRowReadOnlyInReadOnlyGrid()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			Dummy.Factory.Save();

			Dummy.Collection.SetReadOnlyIncludingChildren(true);

			Form.Grid.InnerGrid.Select(0);
			Form.Grid.EditButton.PerformClick();
			AssertNotNull("Should have shown form", Form.Grid.LastShownZForm);
			AssertEquals("Should still be readonly while being edited", true, Dummy.Collection[0].ReadOnly);
			var firstForm = (ZForm)(Form.Grid.LastShownZForm);

			Dummy.Collection.AddNew();
			Form.Grid.InnerGrid.Select(1);
			Factory.Save();
			Form.Grid.EditButton.PerformClick();
			var secondForm = (ZForm)(Form.Grid.LastShownZForm);
			AssertEquals("Should still be readonly while being edited", true, Dummy.Collection[0].ReadOnly);
			AssertEquals("Should still be readonly while being edited", true, Dummy.Collection[1].ReadOnly);

			firstForm.Close();
			AssertEquals("Should still be readonly after edit form closed", true, Dummy.Collection[0].ReadOnly);
			AssertEquals("Should still be readonly while being edited", true, Dummy.Collection[1].ReadOnly);

			secondForm.Close();
			AssertEquals("Should still be readonly after edit form closed", true, Dummy.Collection[0].ReadOnly);
			AssertEquals("Should still be readonly after edit form closed", true, Dummy.Collection[1].ReadOnly);
		}

		public void TestEditButton()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			AssertNull("Should not have shown form", Form.Grid.LastShownZForm);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			Form.Grid.InnerGrid.Select(0);
			Form.Grid.EditButton.PerformClick();

			try
			{
				AssertNotNull("Should have shown form", Form.Grid.LastShownZForm);
				AssertEquals("Should be in new mode without changes", ODisplayMode.NewSaved, Form.Grid.LastShownZForm.DisplayMode);
				((DummyBusinessObject)((ZForm)(Form.Grid.LastShownZForm)).BusinessEntity).Z0_Description = "HANDOFGOD";
				AssertEquals("Should be in edit mode", ODisplayMode.Edit, Form.Grid.LastShownZForm.DisplayMode);
				((ZForm)(Form.Grid.LastShownZForm)).FireSaveButton();
				AssertEquals("Should be in new mode without changes", ODisplayMode.NewSaved, Form.Grid.LastShownZForm.DisplayMode);

				AssertEquals("Should still be one element", 1, Dummy.Collection.Count);
				AssertEquals("Should be updated", "HANDOFGOD", Dummy.Collection[0].Z0_Description);

				AssertEquals("No changes", false, Dummy.HasChanges);
			}
			finally
			{
				Form.Grid.LastShownZForm.Dispose();
			}
		}
		public void TestRowDoubleClick()
		{
			var dummyBusinessObject = Factory.New<DummyBusinessObject>();

			using (form = new ZModuleButtonGridTestForm(dummyBusinessObject))
			{
				dummyBusinessObject.Collection.AddNew();
				dummyBusinessObject.Factory.Save();

				form.Show();
				form.Grid.InnerGrid.Select(0);

				form.Grid.InnerGrid.PerformMouseDownForTest(0, 2);
				AssertEquals("Should have shown form at NewSaved mode", Form.Grid.LastShownZForm.DisplayMode, ODisplayMode.NewSaved);

				Form.Grid.LastShownZForm?.Dispose();
			}
		}

		public void TestEditButtonWithOverriddenEditObject()
		{
			using (form = new ZModuleButtonGridTestForm(Dummy, true))
			{
				Form.Show();
				Dummy.Collection.AddNew();
				AssertEquals("Added one", 1, Dummy.Collection.Count);
				AssertNull("Should not have shown form", Form.Grid.LastShownZForm);
				Dummy.Collection[0].Z0_Guid = Factory.New<DummyBusinessObject>().PK;
				Dummy.Factory.Save();
				AssertEquals("No changes", false, Dummy.HasChanges);

				Form.Grid.InnerGrid.Select(0);
				Form.Grid.EditButton.PerformClick();

				AssertNotNull("Should have shown form", Form.Grid.LastShownZForm);
				((DummyBaseBusinessObject)((ZForm)(Form.Grid.LastShownZForm)).BusinessEntity).Z0_Description = "HANDOFGOD";
				((ZForm)(Form.Grid.LastShownZForm)).FireSaveButton();

				AssertEquals("Should still be one element", 1, Dummy.Collection.Count);
				AssertEquals("Should be updated", "HANDOFGOD", Dummy.Collection[0].RelatedDummy.Z0_Description);

				AssertEquals("No changes", false, Dummy.HasChanges);
				Form.Grid.LastShownZForm.Dispose();
			}
		}

		public void TestEditButton_NonPersistentBusinessObject()
		{
			var parent = Factory.New<NonPersistentBizOForTestParent>();
			using (var form = new ZModuleButtonGridNonPersistentTestForm(parent))
			{
				form.Show();
				parent.Collection.AddNew();
				AssertEquals("Added one", 1, parent.Collection.Count);
				AssertNull("Should not have shown form", form.Grid.LastShownZForm);

				form.Grid.InnerGrid.Select(0);
				form.Grid.EditButton.PerformClick();

				try
				{
					AssertNotNull("Should have shown form", form.Grid.LastShownZForm);
				}
				finally
				{
					form.Grid.LastShownZForm.Dispose();
				}
			}
		}

		public void TestEditButtonWhenRowHasBeenDeletedByAnotherMachine()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			AssertNull("Should not have shown form", Form.Grid.LastShownZForm);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			Db.Connection.ExecuteScalar("delete from " + Dummy.TableName); // imitate somebody on another machine blowing Dummies away

			Form.Grid.InnerGrid.Select(0);
			Form.Grid.EditButton.PerformClick();

			AssertNull("Should not have shown form and should not have blown up", Form.Grid.LastShownZForm);
		}

		public void TestNewButton()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			AssertNull("Should not have shown form", Form.Grid.LastShownZForm);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			Form.Grid.NewButton.PerformClick();
			AssertNotNull("Should have shown form", Form.Grid.LastShownZForm);
			AssertEquals("Should still be one element", 1, Dummy.Collection.Count);

			((ZForm)Form.Grid.LastShownZForm).FireSaveButton();
			AssertEquals("Should be new element added", 2, Dummy.Collection.Count);
			AssertEquals("Should have changes", true, Dummy.HasChanges);

			Form.Grid.LastShownZForm.Dispose();
		}

		#region TestNewElementIsInitializedBeforeShowingOnForm

		public void TestNewElementIsInitializedBeforeShowingOnForm()
		{
			using (var testForm = new ZModuleButtonGridTestForm(Factory.New<DummyX>()))
			{
				var bound = false;

				var contoller = new DummyControllerX();
				contoller.FormShowing +=
					entity =>
					{
						AssertEquals(InitializedText, ((DummyBaseBusinessObject)entity).Z0_VarCharMax);
						bound = true;
					};

				testForm.Show();
				testForm.Grid.ControllerForTest = contoller;
				testForm.Grid.NewButton.PerformClick();

				Assert("Should have tried to show new form", bound);
			}
		}

		class DummyControllerX : DummyController
		{
			protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
			{
				if (FormShowing != null)
				{
					FormShowing(businessEntity);
				}
				return null;
			}

			public event Action<IBusiness> FormShowing;
		}

		class DummyBizoCollectionX : DummyChildBusinessObjectCollection
		{
			public DummyBizoCollectionX(BusinessObjectFactory factory) : base(factory) { }

			protected override void SetCollectionRelationships(BusinessObject child)
			{
				base.SetCollectionRelationships(child);
				((DummyBaseBusinessObject)child).Z0_VarCharMax = InitializedText;
			}
		}

		class DummyX : DummyBusinessObject
		{
			public DummyX(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override DummyChildBusinessObjectCollection NewCollection()
			{
				return new DummyBizoCollectionX(Factory);
			}
		}

		const string InitializedText = "<<initialized>>";

		#endregion

		public void TestDetachButton()
		{
			Form.Show();

			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			AssertEquals("Added two", 2, Dummy.Collection.Count);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			Form.Grid.InnerGrid.UnSelectAll();
			Form.Grid.DetachButton.PerformClick();
			AssertEquals("Normal message shown", "Please select an item in the grid by first clicking in the left-hand gutter to highlight the whole row.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("still has two", 2, Dummy.Collection.Count);

			Form.Grid.InnerGrid.Select(0);
			Form.Grid.DetachButton.PerformClick();
			AssertEquals("Should be detatched", 1, Dummy.Collection.Count);
			AssertEquals("Should have changes", true, Dummy.HasChanges);

			Dummy.Collection.AddNew();
			AssertEquals("Added two", 2, Dummy.Collection.Count);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);
			Form.Grid.InnerGrid.SelectAllElements();

			Form.Grid.DetachButton.PerformClick();
			AssertEquals("Should be detatched", 0, Dummy.Collection.Count);
			AssertEquals("Should have changes", true, Dummy.HasChanges);
		}

		public void TestDetaching()
		{
			Form.Show();

			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			AssertEquals("Added four", 4, Dummy.Collection.Count);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			form.Grid.InnerGrid.Select(0);
			form.Grid.DetachButton.PerformClick();
			AssertEquals("should now be 3 items in collection", 3, dummy.Collection.Count);

			form.Grid.Detaching += new ModuleButtonGridOperationCancelEventHandler(CancelGridOperation);
			form.Grid.InnerGrid.Select(0);
			form.Grid.DetachButton.PerformClick();
			AssertEquals("should still be 3 items in collection because detach was cancelled", 3, dummy.Collection.Count);
		}

		public void TestDetached()
		{
			Form.Show();

			var childDummy1 = Dummy.Collection.AddNew();
			var childDummy2 = Dummy.Collection.AddNew();
			var childDummy3 = Dummy.Collection.AddNew();
			Dummy.Factory.Save();

			var detachedBizos = new List<BusinessObject>();
			form.Grid.Detached += (s, e) => { detachedBizos.AddRange(e.DetachedBusinessObjects); };

			form.Grid.InnerGrid.Select(0);
			form.Grid.InnerGrid.Select(1);
			form.Grid.DetachButton.PerformClick();

			AssertContainsExactElementsInAnyOrder("Detached objects passed to Detached event handler", new BusinessObject[] { childDummy1, childDummy2 }, detachedBizos);
		}

		public void TestAttaching()
		{
			Form.Show();

			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			AssertEquals("Added four", 4, Dummy.Collection.Count);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			form.Grid.Attaching += new ModuleButtonGridOperationCancelEventHandler(CancelGridOperation);

			form.Grid.InnerGrid.Select(0);
			form.Grid.AttachButton.PerformClick();
			AssertEquals("should not show attach form", true, form.Grid.LastShownAttachPopupForTesting == null);
		}

		public void TestCreatingNew()
		{
			Form.Show();

			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			AssertEquals("Added four", 4, Dummy.Collection.Count);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			form.Grid.CreatingNew += new ModuleButtonGridOperationCancelEventHandler(CancelGridOperation);

			form.Grid.InnerGrid.Select(0);
			form.Grid.NewButton.PerformClick();

			var controller = form.Grid.GetController();
			AssertEquals("should not show attach form", true, controller.LastShownForm == null);
		}

		public void TestEditing()
		{
			Form.Show();

			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			AssertEquals("Added four", 4, Dummy.Collection.Count);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			form.Grid.Editing += new ModuleButtonGridOperationCancelEventHandler(CancelGridOperation);

			form.Grid.InnerGrid.Select(0);
			form.Grid.EditButton.PerformClick();

			var controller = form.Grid.GetController();
			AssertEquals("should not show attach form", true, controller.LastShownForm == null);
		}

		void CancelGridOperation(object sender, ModuleButtonGridOperationCancelEventArgs args)
		{
			args.Cancel = true;
		}

		public void TestAttachButton()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			ZDummyButtonGridModuleDecisionProvider.ConstructorCallCount = 0;
			var grid = form.Grid.InnerGrid;
			grid.BeginEdit(grid.Columns[0].ColumnStyle, 0);
			Form.Grid.AttachButton.PerformClick();
			AssertEquals("There should be no extra edit line in the grid manager after PerformClick", 1, grid.ListManager.List.Count);
			AssertEquals("ShouldHaveCreated the ZDummyButtonGridModuleDecisionProvider by now", 1, ZDummyButtonGridModuleDecisionProvider.ConstructorCallCount);

			try
			{
				Dummy.FilteredCollection.AddNew();
				Dummy.FilteredCollection.Factory.Save();
				Form.Grid.LastShownAttachPopupForTesting.HandleSelection(new BusinessObject[] { Dummy.FilteredCollection[0] });
				AssertEquals("Should be another attached", 2, Dummy.Collection.Count);
				AssertEquals("Existing Element HasChanges", false, Dummy.Collection[0].HasChanges);
				AssertEquals("Added Element HasChanges", false, Dummy.Collection[1].HasChanges);
				AssertEquals("ShouldDisplayNotifications", true, Form.Grid.LastShownAttachPopupForTesting.Module.ModuleDecisionProvider.ShouldDisplayNotifications);
			}
			finally
			{
				Form.Grid.LastShownAttachPopupForTesting.Dispose();
			}
		}

		public void TestAttachButtonIsAlreadyHasAnOwnerFormBeforeSetToNull()
		{
			Form.Show();
			Form.Grid.AttachButton.PerformClick();

			AssertEquals("ZModuleButtonGridTestForm", Form.Grid.LastShownAttachPopupForTesting.Owner?.GetType().Name);
		}

		public void TestAttachButton_MultiSelect()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			Dummy.Factory.Save();
			AssertEquals("No changes", false, Dummy.HasChanges);

			Form.Grid.AttachButton.PerformClick();

			try
			{
				Dummy.FilteredCollection.AddNew();
				Dummy.FilteredCollection.AddNew();
				Dummy.FilteredCollection.Factory.Save();
				Form.Grid.LastShownAttachPopupForTesting.HandleSelection(new BusinessObject[] { Dummy.FilteredCollection[0], Dummy.FilteredCollection[1] });
				AssertEquals("Should be 2 more attached", 3, Dummy.Collection.Count);
				AssertEquals("Existing Element HasChanges", false, Dummy.Collection[0].HasChanges);
				AssertEquals("Added Element (1) HasChanges", false, Dummy.Collection[1].HasChanges);
				AssertEquals("Added Element (2) HasChanges", false, Dummy.Collection[2].HasChanges);
			}
			finally
			{
				Form.Grid.LastShownAttachPopupForTesting.Dispose();
			}
		}

		public void TestOnAttachEvent()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			AssertEquals("Added one", 1, Dummy.Collection.Count);
			Dummy.Factory.Save();

			form.Grid.OnAttach += new EventHandler<ModuleButtonGridOnAttachEventArgs>(AttachEventHandler);
			Form.Grid.AttachButton.PerformClick();

			try
			{
				Dummy.FilteredCollection.AddNew();
				Dummy.FilteredCollection.AddNew();
				Dummy.FilteredCollection.Factory.Save();
				Form.Grid.LastShownAttachPopupForTesting.HandleSelection(new BusinessObject[] { Dummy.FilteredCollection[0], Dummy.FilteredCollection[1] });
				AssertEquals("Should be 2 more attached", 3, Dummy.Collection.Count);
			}
			finally
			{
				Form.Grid.LastShownAttachPopupForTesting.Dispose();
			}
		}

		void AttachEventHandler(object sender, ModuleButtonGridOnAttachEventArgs args)
		{
			AssertEquals("2 objects passed to OnAttachEventHandler", 2, args.AttachedBusinessObjects.Length);
		}

		public void TestBeforeDeatchEvent()
		{
			Form.Show();
			var dummyTask1 = Dummy.Collection.AddNew();
			var dummyTask2 = Dummy.Collection.AddNew();
			Dummy.Factory.Save();
			var toDetachBizos = new List<BusinessObject>();

			form.Grid.BeforeDetach += (s, e) => { toDetachBizos.AddRange(e.ToDetachBusinessObjects); };
			form.Grid.InnerGrid.Select(0);
			form.Grid.InnerGrid.Select(1);
			form.Grid.DetachButton.PerformClick();

			AssertContainsExactElementsInAnyOrder("To detach objects passed to BeforeDetach event handler", new BusinessObject[] { dummyTask1, dummyTask2 }, toDetachBizos);
		}

		public void TestBeforeDetachCancellation()
		{
			Form.Show();
			var dummyTask1 = Dummy.Collection.AddNew();
			var dummyTask2 = Dummy.Collection.AddNew();
			Dummy.Factory.Save();

			form.Grid.BeforeDetach += (s, e) => { e.Cancel = true; };
			form.Grid.InnerGrid.Select(0);
			form.Grid.DetachButton.PerformClick();

			AssertEquals("should still be 2 items in collection because detach was cancelled", 2, dummy.Collection.Count);
		}

		public void TestButtonsReadOnly_WhenCollectionReadOnly()
		{
			Form.Show();
			Dummy.Collection.SetReadOnly(true);
			AssertEquals(false, Form.Grid.NewButton.Enabled);
			AssertEquals(false, Form.Grid.AttachButton.Enabled);
			AssertEquals(false, Form.Grid.DetachButton.Enabled);
		}

		public void TestGetNewModuleDecisionProvider()
		{
			using (var grid = new TestModuleButtonGrid())
			{
				Assert("unexpected null", grid.NewModuleDecisionProvider().GetType() != null);
			}
		}

		public void TestCommittedWhenEditing()
		{
			Form.Show();

			var initialRowCount = ((INeedTable)Dummy.Collection).Table.Rows.Count;

			Form.Grid.InnerGrid.Focus();
			KeySender.PostKeyDown(Form.Grid.InnerGrid.LastFocusedColumn.EditControl, Keys.B);
			Application.DoEvents();

			Form.Grid.InnerGrid.Select(0);
			Form.Grid.EditSelected();

			AssertEquals("Table contains added row", initialRowCount + 1, ((INeedTable)Dummy.Collection).Table.Rows.Count);
		}

		public void TestNeedsSaveToShowEditForm()
		{
			var dummySelected = Dummy.Factory.New<DummyBusinessObject>();

			Form.Show();
			var grid = Form.Grid;

			grid.AlwaysRequiresSaveBeforeEdit = false;

			Dummy.SetIsInDatabase(true);
			Dummy.HasChanges = false;
			dummySelected.SetIsInDatabase(true);
			dummySelected.HasChanges = false;
			AssertEquals("No changes made to form or selected row", false, grid.NeedsSaveToShowEditForm(dummySelected));

			Dummy.SetIsInDatabase(true);
			Dummy.HasChanges = false;
			dummySelected.SetIsInDatabase(true);
			dummySelected.HasChanges = true;
			AssertEquals("Changes on selected row only", true, grid.NeedsSaveToShowEditForm(dummySelected));

			Dummy.SetIsInDatabase(true);
			Dummy.HasChanges = true;
			dummySelected.SetIsInDatabase(true);
			dummySelected.HasChanges = false;
			AssertEquals("Changes on grid DataSource but not selected row", false, grid.NeedsSaveToShowEditForm(dummySelected));

			Dummy.SetIsInDatabase(false);
			Dummy.HasChanges = false;
			dummySelected.SetIsInDatabase(true);
			dummySelected.HasChanges = false;
			AssertEquals("Grid DataSource not in database", true, grid.NeedsSaveToShowEditForm(dummySelected));

			Dummy.SetIsInDatabase(true);
			Dummy.HasChanges = false;
			dummySelected.SetIsInDatabase(false);
			dummySelected.HasChanges = false;
			AssertEquals("Row is not in database, but grid DataSource is", true, grid.NeedsSaveToShowEditForm(dummySelected));
		}

		public void TestAlwaysRequiresSaveBeforeEdit()
		{
			var dummySelected = Dummy.Factory.New<DummyBusinessObject>();
			dummySelected.HasChanges = false;

			Form.Show();
			var grid = Form.Grid;

			Dummy.SetIsInDatabase(true);
			dummySelected.SetIsInDatabase(true);
			Dummy.HasChanges = false;
			dummySelected.HasChanges = false;
			grid.AlwaysRequiresSaveBeforeEdit = false;
			AssertEquals(false, grid.NeedsSaveToShowEditForm(dummySelected));
			grid.AlwaysRequiresSaveBeforeEdit = true;
			AssertEquals(false, grid.NeedsSaveToShowEditForm(dummySelected));

			Dummy.SetIsInDatabase(true);
			dummySelected.SetIsInDatabase(true);
			Dummy.HasChanges = true;
			dummySelected.HasChanges = false;
			grid.AlwaysRequiresSaveBeforeEdit = false;
			AssertEquals(false, grid.NeedsSaveToShowEditForm(dummySelected));
			grid.AlwaysRequiresSaveBeforeEdit = true;
			AssertEquals(true, grid.NeedsSaveToShowEditForm(dummySelected));
		}

		public void TestGridFieldIsPrivate()
		{
			foreach (var field in typeof(ZModuleButtonGrid).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (typeof(ZGrid).IsAssignableFrom(field.FieldType) ||
					typeof(Button).IsAssignableFrom(field.FieldType))
				{
					Assert(
						"Field " + field.Name + " must be private. When protected or public, this bodges the designer when sub-classing",
						!field.IsFamily && !field.IsPublic);
				}
			}
		}

		public void TestEditButtonText()
		{
			using (var grid = new ZModuleButtonGrid())
			{
				AssertEquals("Edit button text default", "Edit", grid.EditButtonText.Caption);
				grid.EditButtonText = Res.GetData("b", "Blah");
				AssertEquals("Edit button text should have changed", "Blah", grid.EditButtonText.Caption);
			}
		}

		public void TestAttachButtonText()
		{
			using (var grid = new ZModuleButtonGrid())
			{
				AssertEquals("Attach button text default", "Attach", grid.AttachButtonText.Caption);
				grid.AttachButtonText = Res.GetData("b", "Blah");
				AssertEquals("Attach button text should have changed", "Blah", grid.AttachButtonText.Caption);
			}
		}

		public void TestDetachButtonText()
		{
			using (var grid = new ZModuleButtonGrid())
			{
				AssertEquals("Detach button text default", "Detach", grid.DetachButtonText.Caption);
				grid.DetachButtonText = Res.GetData("b", "Blah");
				AssertEquals("Detach button text should have changed", "Blah", grid.DetachButtonText.Caption);
			}
		}

		public void TestNewButtonText()
		{
			using (var grid = new ZModuleButtonGrid())
			{
				AssertEquals("New button text default", "New", grid.NewButtonText.Caption);
				grid.NewButtonText = Res.GetData("b", "Blah");
				AssertEquals("New button text should have changed", "Blah", grid.NewButtonText.Caption);
			}
		}

		public void TestShowNewForm_DomainValidationsAddedToController()
		{
			Form.Show();
			Form.Grid.NewButton.PerformClick();
			using (var lastShownForm = (ZDummyForm)Form.Grid.LastShownZForm)
			{
				Assert("Pre-condition", !lastShownForm.BusinessEntity.Factory.HasDomainValidation);
			}

			Dummy.Factory.Validation.MainGroup.RegisterValidationType<DummyBusinessObject, DummyBizoValidation>();
			Form.Grid.NewButton.PerformClick();
			using (var lastShownForm = (ZDummyForm)Form.Grid.LastShownZForm)
			{
				Assert("Domain validations should be added to the controller's domain", lastShownForm.BusinessEntity.Factory.HasDomainValidation);
			}
		}

		[ExpectNoExceptions]
		public void TestShowNewForm_FindBoxListDoNotHaveFactory()
		{
			Dummy.FilteredCollection = new DummyChildBusinessObjectCollection(null);
			Form.Show();
			Form.Grid.NewButton.PerformClick();
			Form.Grid.LastShownZForm.Dispose();
		}

		[ExpectNoExceptions]
		public void TestShowNewForm_NullFindBoxList()
		{
			var dummyChild = Dummy.Collection.AddNew();
			Dummy.Factory.Save();
			Form.Grid.FindBoxList = null;
			Form.Show();
			Form.Grid.NewButton.PerformClick();
			Form.Grid.LastShownZForm.Dispose();
		}

		public void TestShowEditForm_DomainValidationsAddedToController()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			Dummy.Factory.Save();
			Form.Grid.EditButton.PerformClick();
			using (var lastShownForm = (ZDummyForm)Form.Grid.LastShownZForm)
			{
				Assert("Pre-condition", !lastShownForm.BusinessEntity.Factory.HasDomainValidation);
				lastShownForm.Close();
			}

			Dummy.Factory.Validation.MainGroup.RegisterValidationType<DummyBusinessObject, DummyBizoValidation>();
			Form.Grid.EditButton.PerformClick();
			using (var lastShownForm = (ZDummyForm)Form.Grid.LastShownZForm)
			{
				Assert("Domain validations should be added to the controller's domain", lastShownForm.BusinessEntity.Factory.HasDomainValidation);
			}
		}

		public void TestShowEditForm_NextPreviousControl()
		{
			Form.Show();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Factory.Save();

			Form.Grid.InnerGrid.Select(0);
			Form.Grid.EditButton.PerformClick();
			using (var lastShownForm = (ZDummyForm)Form.Grid.LastShownZForm)
			{
				AssertEquals("Control is there", true, lastShownForm.PreviousNextControlForTesting.Visible);
				AssertEquals("Prev button disabled", false, lastShownForm.PreviousNextControlForTesting.PreviousButtonForTesting.IsEnabledForBinding);
				AssertEquals("Next button enabled", true, lastShownForm.PreviousNextControlForTesting.NextButtonForTesting.IsEnabledForBinding);

				lastShownForm.PreviousNextControlForTesting.FireNextButtonForTesting();
				Application.DoEvents();
				AssertEquals("First form closed", true, lastShownForm.IsDisposed);

				AssertNotNull(lastShownForm.PreviousNextControlForTesting.LastOpenedFormForTesting);
				AssertEquals("Control is there on 2nd form", true, ((ZForm)lastShownForm.PreviousNextControlForTesting.LastOpenedFormForTesting).PreviousNextControlForTesting.Visible);
				lastShownForm.PreviousNextControlForTesting.LastOpenedFormForTesting.Dispose();
			}
		}

		[ExpectNoExceptions]
		public void TestShowEditForm_FindBoxListDoNotHaveFactory()
		{
			Dummy.FilteredCollection = new DummyChildBusinessObjectCollection(null);
			Form.Show();
			Dummy.Collection.AddNew();
			Dummy.Factory.Save();
			Form.Grid.EditButton.PerformClick();
			Form.Grid.LastShownZForm.Dispose();
		}

		public void TestShowEditForm_OpenEdit()
		{
			Dummy.FilteredCollection = new DummyChildBusinessObjectCollection(null);
			Dummy.Collection.AddNew();
			Dummy.Factory.Save();
			Form.Show();
			Form.Grid.EditButton.PerformClick();
			using (var lastShownForm = (ZDummyForm)Form.Grid.LastShownZForm)
			{
				AssertEquals("Coillection is editable, so child form is editable", ODisplayMode.NewSaved, lastShownForm.DisplayMode);
			}
		}

		public void TestShowEditForm_OpenView()
		{
			Dummy.FilteredCollection = new DummyChildBusinessObjectCollection(null);
			Dummy.Collection.AddNew();
			Dummy.Factory.Save();
			Dummy.Collection.SetReadOnlyIncludingChildren(true);
			Form.Show();
			Form.Grid.EditButton.PerformClick();
			using (var lastShownForm = (ZDummyForm)Form.Grid.LastShownZForm)
			{
				AssertEquals("Collection is readonly, so child form opens readonly", ODisplayMode.ReadOnly, lastShownForm.DisplayMode);
			}
		}

		public void TestShowEditForm_OpenEditWithOveride()
		{
			Dummy.FilteredCollection = new DummyChildBusinessObjectCollection(null);
			Dummy.Collection.AddNew();
			Dummy.Factory.Save();
			Dummy.Collection.SetReadOnlyIncludingChildren(true);
			using (var form = new ZModuleButtonGridTestFormWithAllowEditGrid(Dummy))
			{
				form.Show();
				form.Grid.EditButton.PerformClick();
				using (var lastShownForm = (ZDummyForm)form.Grid.LastShownZForm)
				{
					AssertEquals("Can edit child record even if collection is readonly", ODisplayMode.NewSaved, lastShownForm.DisplayMode);
				}
			}
		}

		public void TestShowEditForm_AlwaysShowFormInReadOnlyMode()
		{
			Dummy.FilteredCollection = new DummyChildBusinessObjectCollection(null);
			Dummy.Collection.AddNew();
			Dummy.Factory.Save();
			using (var form = new ZModuleButtonGridTestFormAlwaysShowFormInReadOnlyMode(Dummy))
			{
				Assert("Pre-condition: Collection is not readonly", !Dummy.Collection.ReadOnly);
				form.Show();
				form.Grid.EditButton.PerformClick();
				using (ZDummyForm lastShownForm = (ZDummyForm)form.Grid.LastShownZForm)
				{
					AssertEquals("should open form in readonly state", ODisplayMode.ReadOnly, lastShownForm.DisplayMode);
				}
			}
		}

		public void TestShowEditForm_OpenEditWithNullSelected()
		{
			Dummy.FilteredCollection = new DummyChildBusinessObjectCollection(null);
			Dummy.Collection.AddNew();
			Dummy.Factory.Save();
			using (var form = new ZModuleButtonGridWithNullSelectedTestForm(Dummy))
			{
				form.Show();
				form.Grid.EditButton.PerformClick();

				AssertEquals("can't edit message shown", "The item you are trying to edit has been deleted or does not exist.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestShowEditForm_NullFindBoxList()
		{
			var dummyChild = Dummy.Collection.AddNew();
			Dummy.Factory.Save();
			Form.Show();
			Form.Grid.FindBoxList = null;
			Form.Grid.Edit(dummyChild, null);

			Form.Grid.LastShownZForm.Dispose();
		}

		public void TestDesignerCaptionProperties()
		{
			TestDesignerCaptionProperty("EditButtonText");
			TestDesignerCaptionProperty("AttachButtonText");
			TestDesignerCaptionProperty("DetachButtonText");
			TestDesignerCaptionProperty("NewButtonText");
			TestDesignerCaptionProperty("DetachMessage");
			TestDesignerCaptionProperty("NameOfAGridElement");
		}

		void TestDesignerCaptionProperty(string propertyName)
		{
			var type = typeof(ZModuleButtonGrid);
			var textProperty = type.GetProperty(propertyName);
			AssertNotNull(propertyName, textProperty);
			var defaultTextProperty = type.GetProperty("Default" + propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			AssertNotNull("Default" + propertyName, defaultTextProperty);
			var shouldSerializeMethod = type.GetMethod("ShouldSerialize" + propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			AssertNotNull("ShouldSerialize" + propertyName, shouldSerializeMethod);
			var resetMethod = type.GetMethod("Reset" + propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			AssertNotNull("Reset" + propertyName, resetMethod);
			AssertEquals(defaultTextProperty.GetValue(Form.Grid, Array.Empty<object>()), textProperty.GetValue(Form.Grid, Array.Empty<object>()));
			AssertEquals(false, shouldSerializeMethod.Invoke(Form.Grid, Array.Empty<object>()));
			textProperty.SetValue(Form.Grid, new ResourceStringData("XXX", "New Caption"), Array.Empty<object>());
			AssertEquals(true, shouldSerializeMethod.Invoke(Form.Grid, Array.Empty<object>()));
			resetMethod.Invoke(Form.Grid, Array.Empty<object>());
			AssertEquals(defaultTextProperty.GetValue(Form.Grid, Array.Empty<object>()), textProperty.GetValue(Form.Grid, Array.Empty<object>()));
			AssertEquals(false, shouldSerializeMethod.Invoke(Form.Grid, Array.Empty<object>()));
		}

		#region Debug Info

		public void TestGenerateDebugInfoAboutCollection_GridDataSourceFactoryIsNull()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var parent = new NonPersistentParentForTest();
			parent.Collection.AddNew();

			parent.Collection.Factory.Save();

			using (var form = new ZModuleButtonGridTestForm(parent))
			{
				form.Grid.AlwaysRequiresSaveBeforeEdit = true;
				form.Show();
				parent.HasChanges = true;

				CombineAssertions("Precondition", () =>
				{
					AssertNull("DataSource.Factory should be null", form.Grid.DataSource.Factory);
					AssertNullOrEmpty("should have notification", UnitTestUserNotification.Instance.LastMessage.Text);
				});

				form.Grid.EditButton.PerformClick();

				AssertNotEquals
				(
					"Object reference not set to an instance of an object.",
					ExceptionReporterTestListener.Instance.FirstOrDefault()?.InnerException?.Message ?? string.Empty
				);

				AssertEquals
				(
					"WHEN ShowEdit THEN should show selected-record-has-been-deleted message because NonPersistentBusinessObject has NULL factory",
					"The form must be saved before a e.g, Order can be edited or a new e.g, Order can be created. Do you wish to save the form?",
					UnitTestUserNotification.Instance.LastMessage.Text
				);

				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestGenerateDebugInfoAboutBizO_SelectedRecordFactoryIsNull()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var parent = Factory.New<NonPersistentBizOForTestParent>();
			var child = parent.Collection.AddNew();

			using (var form = new ZModuleButtonGridTestForm(parent))
			{
				form.Grid.ColumnStyles.Clear();

				form.Grid.ColumnStyles.Add
				(
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = "Description",
						Caption = "Description"
					}
				);

				form.Grid.AlwaysRequiresSaveBeforeEdit = true;
				form.Show();
				parent.HasChanges = true;

				CombineAssertions("Precondition", () =>
				{
					AssertNull("child.Factory should be null", child.Factory);
					AssertNullOrEmpty("Precondition", UnitTestUserNotification.Instance.LastMessage.Text);
				});

				form.Grid.EditButton.PerformClick();

				AssertNotEquals
				(
					"Object reference not set to an instance of an object.",
					ExceptionReporterTestListener.Instance.FirstOrDefault()?.InnerException?.Message ?? string.Empty
				);

				AssertEquals(
					"WHEN ShowEdit THEN should show selected-record-has-been-deleted message because it is NonPersistentBusinessObject has NULL factory",
					"The selected record has been deleted by another user. It cannot be displayed.",
					UnitTestUserNotification.Instance.LastMessage.Text);

				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestDebugInfoIncludeAllChildrenWithChanges()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var parent = Factory.New<DummyParentBusinessObject>();
			var child = parent.Collection.AddNew();
			child.RegisterEditableChildObject(child.Collection);
			var grandchild = child.Collection.AddNew();
			parent.Factory.Save();

			using (var form = new ZModuleButtonGridTestForm(parent))
			{
				form.Grid.AlwaysRequiresSaveBeforeEdit = true;
				form.Show();
				parent.HasChanges = true;
				parent.Factory.Saved += delegate
				{
					grandchild.Z0_Description += "X";
				};

				form.Grid.EditButton.PerformClick();

				AssertStartsWith("Display which objects have changes", $@"Can't show Edit form for: e.g, Order from button grid. Probably Collection's Master.IsInDatabase is returning False.
Debug info before save : 
Selected is CargoWise.EntityFramework.Testing.DummyBusinessObject
Selected.PK={child.PK}
Selected.IsInDatabase=True
Selected.HasChanges=False
Selected.Factory={Factory._Instance}
SelectedNeedsSaving=False

Collection is CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
DataSource is Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject
DataSource.PK={parent.PK}
DataSource.HasChanges=True
DataSource.Factory={Factory._Instance}
    Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject : PK = {parent.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyParentBusinessObject, HasChanges = true, Factory = {Factory._Instance}
AlwaysRequiresSaveBeforeEdit=True
Collection.MastersAreInDatabase=True
MasterNeedsSaving=True

Debug info after save : 
Selected is CargoWise.EntityFramework.Testing.DummyBusinessObject
Selected.PK={child.PK}
Selected.IsInDatabase=True
Selected.HasChanges=True
Selected.Factory={Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
SelectedNeedsSaving=True

Collection is CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
DataSource is Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject
DataSource.PK={parent.PK}
DataSource.HasChanges=True
DataSource.Factory={Factory._Instance}
    Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject : PK = {parent.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyParentBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
AlwaysRequiresSaveBeforeEdit=True
Collection.MastersAreInDatabase=True
MasterNeedsSaving=True

Debug info after HasErrors() : 
Selected is CargoWise.EntityFramework.Testing.DummyBusinessObject
Selected.PK={child.PK}
Selected.IsInDatabase=True
Selected.HasChanges=True
Selected.Factory={Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
SelectedNeedsSaving=True

Collection is CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
DataSource is Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject
DataSource.PK={parent.PK}
DataSource.HasChanges=True
DataSource.Factory={Factory._Instance}
    Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject : PK = {parent.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyParentBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
AlwaysRequiresSaveBeforeEdit=True
Collection.MastersAreInDatabase=True
MasterNeedsSaving=True", ExceptionReporterTestListener.Instance[0].InnerException.Message);

				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestDebugInfoIncludeAllChildrenWithChanges_CollectionAndBizOFactoryIsNull()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var parent = Factory.New<DummyParentBusinessObject>();
			var child = parent.Collection.AddNew();
			var grandchildCollection = new NonPersistentBizOForTestCollection();
			child.RegisterEditableChildObject(grandchildCollection);
			var grandchild = grandchildCollection.AddNew();
			parent.Factory.Save();

			using (var form = new ZModuleButtonGridTestForm(parent))
			{
				form.Grid.AlwaysRequiresSaveBeforeEdit = true;
				form.Show();
				parent.HasChanges = true;
				parent.Factory.Saved += delegate
				{
					child.HasChanges = true;
					grandchild.HasChanges = true;
				};

				form.Grid.EditButton.PerformClick();
				AssertNotEquals("Object reference not set to an instance of an object.", ExceptionReporterTestListener.Instance[0].InnerException.Message);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestDebugInfoIncludeAllChildrenWithChanges_BizOFactoryIsNull()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var parent = Factory.New<DummyParentBusinessObject>();
			var child = parent.Collection.AddNew();
			var grandchild = new NonPersistentBizOForTest();
			child.RegisterEditableChildObject(grandchild);
			parent.Factory.Save();

			using (var form = new ZModuleButtonGridTestForm(parent))
			{
				form.Grid.AlwaysRequiresSaveBeforeEdit = true;
				form.Show();
				parent.HasChanges = true;
				parent.Factory.Saved += delegate
				{
					child.HasChanges = true;
					grandchild.HasChanges = true;
				};

				form.Grid.EditButton.PerformClick();
				AssertNotEquals("Object reference not set to an instance of an object.", ExceptionReporterTestListener.Instance[0].InnerException.Message);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestDebugInfoIncludeAllChildrenWithChanges_UnhookHasChangesChangedWhenDone()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var parent = Factory.New<DummyParentBusinessObject>();
			var child = parent.Collection.AddNew();
			child.RegisterEditableChildObject(child.Collection);
			var grandchild = child.Collection.AddNew();
			parent.Factory.Save();

			using (var form = new ZModuleButtonGridTestForm(parent))
			{
				form.Grid.AlwaysRequiresSaveBeforeEdit = true;
				form.Show();
				parent.HasChanges = true;
				parent.Factory.Saved += (o, e) => grandchild.Z0_Description += "X";

				var numberOfEventHandlersBeforeSave = NumberOfHasChangesEventHandlers(parent);
				form.Grid.EditButton.PerformClick();

				CombineAssertions(() =>
				{
					var errorMessage = ExceptionReporterTestListener.Instance[0].InnerException.Message;
					ExceptionReporterTestListener.Instance.Clear();

					AssertContains("Include the stack trace where possible", "at CargoWise.EntityFramework.BusinessObject.OnHasChangesChanged(HasChangesChangedEventArgs e)", errorMessage);
					AssertContains("Include all available stack traces - #1", "Stack #1", errorMessage);
					AssertContains("Include changed business object type", "Type: Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject", errorMessage);
					AssertContains("Include all available stack traces - #2", "Stack #2", errorMessage);
					AssertContains("Include changed business object type", "Type: CargoWise.EntityFramework.Testing.DummyBusinessObject", errorMessage);
					AssertEquals("Any additional HasChangesChanged events added during save should be removed", numberOfEventHandlersBeforeSave, NumberOfHasChangesEventHandlers(parent));
				});
			}
		}

		//WI00743739 - logging removed due to performance issues
		//public void TestDebugInfo_JobDocAddress_Changed()
		//{
		//	UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
		//	var parent = Factory.New<DummyParentBusinessObject>();
		//	var child = Factory.New<DummyBusinessObjectForTest>();
		//	parent.Collection.Add(child);
		//	parent.Factory.Save();

		//	using (var form = new ZModuleButtonGridTestForm(parent))
		//	{
		//		form.Grid.AlwaysRequiresSaveBeforeEdit = true;
		//		form.Show();
		//		parent.HasChanges = true;
		//		parent.Factory.Saved += delegate
		//		{
		//			child.HasChanges = true;
		//		};

		//		form.Grid.EditButton.PerformClick();

		//		AssertContains("JobDocAddress form exists error", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertContains("JobDocAddress ConstructorStackTrace: StackTrace", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertContains("JobDocAddress IsDeleting: False", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertContains("JobDocAddress IsInDatabase: True", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertContains($"JobDocAddress E2_ParentID: {ZGuid.BrettsGuid}", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertContains("JobDocAddress E2_ParentTableCode: ParentTableCode", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertContains("JobDocAddress E2_AddressType: AddressType", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertContains("JobDocAddress Collection info: DocAddressCollectionInfo", ExceptionReporterTestListener.Instance[0].InnerException.Message);

		//		Assert(!child.IsDeleted);
		//		ExceptionReporterTestListener.Instance.Clear();
		//	}
		//}

		//WI00743739 - logging removed due to performance issues
		//public void TestDebugInfo_JobDocAddress_Deleted()
		//{
		//	UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
		//	var parent = Factory.New<DummyParentBusinessObject>();
		//	var child = Factory.New<DummyBusinessObjectForTest>();
		//	parent.Collection.Add(child);
		//	parent.Factory.Save();

		//	using (var form = new ZModuleButtonGridTestForm(parent))
		//	{
		//		form.Grid.AlwaysRequiresSaveBeforeEdit = true;
		//		form.Show();
		//		parent.HasChanges = true;
		//		parent.Factory.Saved += delegate
		//		{
		//			child.Delete();
		//		};

		//		form.Grid.EditButton.PerformClick();
		//		AssertContains("JobDocAddress form exists error", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertContains("JobDocAddress ConstructorStackTrace: StackTrace", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertNotContains("JobDocAddress IsDeleting: False", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertNotContains("JobDocAddress IsInDatabase: True", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertNotContains($"JobDocAddress E2_ParentID: {ZGuid.BrettsGuid}", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertNotContains("JobDocAddress E2_ParentTableCode: ParentTableCode", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertNotContains("JobDocAddress E2_AddressType: AddressType", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		AssertNotContains("JobDocAddress Collection info: DocAddressCollectionInfo", ExceptionReporterTestListener.Instance[0].InnerException.Message);
		//		Assert(child.IsDeleted);
		//		ExceptionReporterTestListener.Instance.Clear();
		//	}
		//}

		int NumberOfHasChangesEventHandlers(BusinessObject bizo)
		{
			var eventHandler = (EventHandler<HasChangesChangedEventArgs>)typeof(BusinessObject).GetField(nameof(bizo.HasChangesChanged), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bizo);

			return eventHandler.GetInvocationList().Length;
		}

		public void TestDebugInfoIncludeAllChildrenWithChanges_TheSameKeyExistsInTheSameIssue()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var parent = Factory.New<DummyParentBusinessObject>();
			var child = parent.Collection.AddNew();
			child.RegisterEditableChildObject(child.Collection);
			var grandchild = child.Collection.AddNew();
			parent.Factory.Save();
			var reporterKey1 = "key1";
			var reporterKey2 = "key2";
			var reporterKey3 = "key3";
			using (var form = new ZModuleButtonGridTestForm(parent))
			{
				form.Grid.AlwaysRequiresSaveBeforeEdit = true;
				form.Show();
				parent.HasChanges = true;
				parent.Factory.Saved += (o, e) => grandchild.Z0_Description += "X";

				var numberOfEventHandlersBeforeSave = NumberOfHasChangesEventHandlers(parent);
				form.Grid.EditButton.PerformClick();

				reporterKey1 = UnitTestUserNotification.Instance.ShownErrorKeys[0];
				UnitTestUserNotification.Instance.ClearShownErrorKeys();

				var errorMessage = ExceptionReporterTestListener.Instance[0].InnerException.Message;
				AssertStartsWith("Display which objects have changes", $@"Can't show Edit form for: e.g, Order from button grid. Probably Collection's Master.IsInDatabase is returning False.
Debug info before save : 
Selected is CargoWise.EntityFramework.Testing.DummyBusinessObject
Selected.PK={child.PK}
Selected.IsInDatabase=True
Selected.HasChanges=False
Selected.Factory={Factory._Instance}
SelectedNeedsSaving=False

Collection is CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
DataSource is Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject
DataSource.PK={parent.PK}
DataSource.HasChanges=True
DataSource.Factory={Factory._Instance}
    Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject : PK = {parent.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyParentBusinessObject, HasChanges = true, Factory = {Factory._Instance}
AlwaysRequiresSaveBeforeEdit=True
Collection.MastersAreInDatabase=True
MasterNeedsSaving=True

Debug info after save : 
Selected is CargoWise.EntityFramework.Testing.DummyBusinessObject
Selected.PK={child.PK}
Selected.IsInDatabase=True
Selected.HasChanges=True
Selected.Factory={Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
SelectedNeedsSaving=True

Collection is CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
DataSource is Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject
DataSource.PK={parent.PK}
DataSource.HasChanges=True
DataSource.Factory={Factory._Instance}
    Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject : PK = {parent.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyParentBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
AlwaysRequiresSaveBeforeEdit=True
Collection.MastersAreInDatabase=True
MasterNeedsSaving=True

Debug info after HasErrors() : 
Selected is CargoWise.EntityFramework.Testing.DummyBusinessObject
Selected.PK={child.PK}
Selected.IsInDatabase=True
Selected.HasChanges=True
Selected.Factory={Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
SelectedNeedsSaving=True

Collection is CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
DataSource is Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject
DataSource.PK={parent.PK}
DataSource.HasChanges=True
DataSource.Factory={Factory._Instance}
    Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject : PK = {parent.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyParentBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
AlwaysRequiresSaveBeforeEdit=True
Collection.MastersAreInDatabase=True
MasterNeedsSaving=True", errorMessage);

				ExceptionReporterTestListener.Instance.Clear();
			}
			using (var form = new ZModuleButtonGridTestForm(parent))
			{
				form.Grid.AlwaysRequiresSaveBeforeEdit = true;
				form.Show();
				parent.HasChanges = true;
				parent.Factory.Saved += (o, e) => grandchild.Z0_Description += "Z";

				var numberOfEventHandlersBeforeSave = NumberOfHasChangesEventHandlers(parent);
				form.Grid.EditButton.PerformClick();

				reporterKey2 = UnitTestUserNotification.Instance.ShownErrorKeys[0];
				UnitTestUserNotification.Instance.ClearShownErrorKeys();

				if (ExceptionReporterTestListener.Instance.Count > 0)
				{
					var errorMessage = ExceptionReporterTestListener.Instance[0].InnerException.Message;
					AssertStartsWith("Display which objects have changes", $@"Can't show Edit form for: e.g, Order from button grid. Probably Collection's Master.IsInDatabase is returning False.
Debug info before save : 
Selected is CargoWise.EntityFramework.Testing.DummyBusinessObject
Selected.PK={child.PK}
Selected.IsInDatabase=True
Selected.HasChanges=True
Selected.Factory={Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
SelectedNeedsSaving=True

Collection is CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
DataSource is Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject
DataSource.PK={parent.PK}
DataSource.HasChanges=True
DataSource.Factory={Factory._Instance}
    Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject : PK = {parent.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyParentBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
AlwaysRequiresSaveBeforeEdit=True
Collection.MastersAreInDatabase=True
MasterNeedsSaving=True

Debug info after save : 
Selected is CargoWise.EntityFramework.Testing.DummyBusinessObject
Selected.PK={child.PK}
Selected.IsInDatabase=True
Selected.HasChanges=True
Selected.Factory={Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
SelectedNeedsSaving=True

Collection is CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
DataSource is Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject
DataSource.PK={parent.PK}
DataSource.HasChanges=True
DataSource.Factory={Factory._Instance}
    Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject : PK = {parent.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyParentBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
AlwaysRequiresSaveBeforeEdit=True
Collection.MastersAreInDatabase=True
MasterNeedsSaving=True

Debug info after HasErrors() : 
Selected is CargoWise.EntityFramework.Testing.DummyBusinessObject
Selected.PK={child.PK}
Selected.IsInDatabase=True
Selected.HasChanges=True
Selected.Factory={Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
SelectedNeedsSaving=True

Collection is CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
DataSource is Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject
DataSource.PK={parent.PK}
DataSource.HasChanges=True
DataSource.Factory={Factory._Instance}
    Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject : PK = {parent.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyParentBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
AlwaysRequiresSaveBeforeEdit=True
Collection.MastersAreInDatabase=True
MasterNeedsSaving=True", errorMessage);
				}
				ExceptionReporterTestListener.Instance.Clear();
			}
			using (var form = new ZModuleButtonGridTestForm(parent))
			{
				form.Grid.AlwaysRequiresSaveBeforeEdit = true;
				form.Show();
				parent.HasChanges = true;
				parent.Factory.Saved += (o, e) => grandchild.Z0_Decimal += 1M;

				var numberOfEventHandlersBeforeSave = NumberOfHasChangesEventHandlers(parent);
				form.Grid.EditButton.PerformClick();

				reporterKey3 = UnitTestUserNotification.Instance.ShownErrorKeys[0];
				UnitTestUserNotification.Instance.ClearShownErrorKeys();

				var errorMessage = ExceptionReporterTestListener.Instance[0].InnerException.Message;
				AssertStartsWith("Display which objects have changes", $@"Can't show Edit form for: e.g, Order from button grid. Probably Collection's Master.IsInDatabase is returning False.
Debug info before save : 
Selected is CargoWise.EntityFramework.Testing.DummyBusinessObject
Selected.PK={child.PK}
Selected.IsInDatabase=True
Selected.HasChanges=True
Selected.Factory={Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
SelectedNeedsSaving=True

Collection is CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
DataSource is Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject
DataSource.PK={parent.PK}
DataSource.HasChanges=True
DataSource.Factory={Factory._Instance}
    Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject : PK = {parent.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyParentBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Description HasChanges = true
AlwaysRequiresSaveBeforeEdit=True
Collection.MastersAreInDatabase=True
MasterNeedsSaving=True

Debug info after save : 
Selected is CargoWise.EntityFramework.Testing.DummyBusinessObject
Selected.PK={child.PK}
Selected.IsInDatabase=True
Selected.HasChanges=True
Selected.Factory={Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Decimal HasChanges = true
    Property Z0_Description HasChanges = true
SelectedNeedsSaving=True

Collection is CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
DataSource is Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject
DataSource.PK={parent.PK}
DataSource.HasChanges=True
DataSource.Factory={Factory._Instance}
    Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject : PK = {parent.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyParentBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Decimal HasChanges = true
    Property Z0_Description HasChanges = true
AlwaysRequiresSaveBeforeEdit=True
Collection.MastersAreInDatabase=True
MasterNeedsSaving=True

Debug info after HasErrors() : 
Selected is CargoWise.EntityFramework.Testing.DummyBusinessObject
Selected.PK={child.PK}
Selected.IsInDatabase=True
Selected.HasChanges=True
Selected.Factory={Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Decimal HasChanges = true
    Property Z0_Description HasChanges = true
SelectedNeedsSaving=True

Collection is CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection
DataSource is Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject
DataSource.PK={parent.PK}
DataSource.HasChanges=True
DataSource.Factory={Factory._Instance}
    Enterprise.ZArchitecture.GUI.Testing.ZModuleButtonGridTest+DummyParentBusinessObject : PK = {parent.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyParentBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyBusinessObject : PK = {child.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObjectCollection : HasChanges = true, Factory = {Factory._Instance}
    CargoWise.EntityFramework.Testing.DummyChildBusinessObject : PK = {grandchild.PK}, IsInDatabase = Yes, IsDeleted = False, Type = DummyChildBusinessObject, HasChanges = true, Factory = {Factory._Instance}
    Property Z0_Decimal HasChanges = true
    Property Z0_Description HasChanges = true
AlwaysRequiresSaveBeforeEdit=True
Collection.MastersAreInDatabase=True
MasterNeedsSaving=True", errorMessage);

				ExceptionReporterTestListener.Instance.Clear();
			}

			CombineAssertions(() =>
			{
				AssertEquals("Keys should be generated per-form, and should not explode into a million issues", reporterKey1, reporterKey2);
				AssertEquals("Keys should be generated per-form, and should not explode into a million issues", reporterKey1, reporterKey3);
				AssertEquals("Keys should be generated per-form, and should not explode into a million issuest", reporterKey2, reporterKey3);
			});
		}

		#endregion

		#region TestOnZUserControlBindingToEmptyList

		[ExpectNoExceptions]
		public void TestOnZUserControlBindingToEmptyList()
		{
			var dummies = new DummyWithDummiesCollection(Factory);

			using (var form = new GridTestForm(dummies))
			{
				form.Show();
				AssertEquals(DummyModuleIDs.Dummy2, form.grid.ModuleID);
			}
		}

		class GridTestForm : ZForm
		{
			public GridTestForm(DummyWithDummiesCollection dummies)
				: base(dummies)
			{
				var column = new ZTextBoxColumnStyleInfo();
				column.ColumnName = DummyBusinessObject.Schema.Z0_VarCharMax;

				grid = new ZModuleButtonGrid();
				BindingSource.SetBindingMember(grid, "Dummies");
				grid.Dock = DockStyle.Fill;
				grid.BindToFindBoxList = "FilteredCollection";
				grid.ColumnStyles.Add(column);

				Controls.Add(grid);
			}

			public readonly ZModuleButtonGrid grid;
		}

		class DummyWithDummies : DummyBusinessObject
		{
			public DummyWithDummies(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyWithDummiesCollection Dummies
			{
				get
				{
					if (dummies == null)
					{
						dummies = new DummyWithDummiesCollection(Factory);
						RegisterEditableChildObject(dummies);
					}
					return dummies;
				}
			}
			DummyWithDummiesCollection dummies;
		}

		[ModuleID(ModuleId.Dummy2)]
		class DummyWithDummiesCollection : BusinessObjectCollection<DummyWithDummies>
		{
			public DummyWithDummiesCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion

		#region Test Attach/Detach Security

		public void TestAttachSecurity()
		{
			using (var form = new ZModuleButtonGridTestForm(Factory.New<DummyBusinessObject>()))
			using (var formWithICanAttach = new ZModuleButtonGridTestFormWithICanAttach(Factory.New<DummyBusinessObject>()))
			{
				form.Show();
				formWithICanAttach.Show();

				AssertAttachSecurity(form, true, true, true, true);
				AssertAttachSecurity(formWithICanAttach, true, false, false, false);

				form.Grid.AllowAttachDetachWithoutEditSecurity = true;
				AssertAttachSecurity(form, true, false, true, true);

				formWithICanAttach.Grid.AllowAttachDetachWithoutEditSecurity = true;
				AssertAttachSecurity(formWithICanAttach, true, false, false, false);
			}
		}

		void AssertAttachSecurity(ZModuleButtonGridTestForm form, bool expected1, bool expected2, bool expected3, bool expected4)
		{
			form.Grid.ControllerForTest = new DummyControllerWithCustomisableSecurity();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForEdit(new DummyCheckPointWithSecuritySet(true));
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForView(new DummyCheckPointWithSecuritySet(true));
			form.Grid.AttachButton.PerformClick();
			AssertEquals(expected1, string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForEdit(new DummyCheckPointWithSecuritySet(false));
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForView(new DummyCheckPointWithSecuritySet(true));
			form.Grid.AttachButton.PerformClick();
			AssertEquals(expected2, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForEdit(new DummyCheckPointWithSecuritySet(true));
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForView(new DummyCheckPointWithSecuritySet(false));
			form.Grid.AttachButton.PerformClick();
			AssertEquals(expected3, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForEdit(new DummyCheckPointWithSecuritySet(false));
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForView(new DummyCheckPointWithSecuritySet(false));
			form.Grid.AttachButton.PerformClick();
			AssertEquals(expected4, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
		}

		public void TestDetachSecurity()
		{
			AssertDetachSecurity(Form, true, true, false, true);
			AssertDetachSecurity(FormWithICanAttach, true, false, false, false);

			Form.Grid.AllowAttachDetachWithoutEditSecurity = true;
			AssertDetachSecurity(Form, true, false, false, false);

			FormWithICanAttach.Grid.AllowAttachDetachWithoutEditSecurity = true;
			AssertDetachSecurity(FormWithICanAttach, true, false, false, false);
		}

		void AssertDetachSecurity(ZModuleButtonGridTestForm form, bool expected1, bool expected2, bool expected3, bool expected4)
		{
			form.Show();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Factory.Save();
			form.Grid.ControllerForTest = new DummyControllerWithCustomisableSecurity();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForEdit(new DummyCheckPointWithSecuritySet(true));
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForView(new DummyCheckPointWithSecuritySet(true));
			form.Grid.InnerGrid.Select(0);
			form.Grid.DetachButton.PerformClick();
			AssertEquals(expected1, string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForEdit(new DummyCheckPointWithSecuritySet(false));
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForView(new DummyCheckPointWithSecuritySet(true));
			form.Grid.InnerGrid.Select(0);
			form.Grid.DetachButton.PerformClick();
			AssertEquals(expected2, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForEdit(new DummyCheckPointWithSecuritySet(true));
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForView(new DummyCheckPointWithSecuritySet(false));
			form.Grid.InnerGrid.Select(0);
			form.Grid.DetachButton.PerformClick();
			AssertEquals(expected3, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForEdit(new DummyCheckPointWithSecuritySet(false));
			((DummyControllerWithCustomisableSecurity)form.Grid.ControllerForTest).SetCheckPointForView(new DummyCheckPointWithSecuritySet(false));
			form.Grid.InnerGrid.Select(0);
			form.Grid.DetachButton.PerformClick();
			AssertEquals(expected4, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
		}

		[ExpectNoExceptions]
		public void TestNoModuleExceptionDuringSecurityCheck()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Factory.Save();

			using (var testForm = new ZForm(Dummy))
			using (var testGrid = new TrickedModuleButtonGrid())
			{
				testForm.Controls.Add(testGrid);
				testGrid.BindToFindBoxList = "FilteredCollection";
				testGrid.BindToGridList = "Collection";
				testGrid.Controls.Add(testGrid.InnerGrid);
				Form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testGrid.FireAttachButton();
				Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
			}

			ErrorReporter.Clear();
		}

		class TrickedModuleButtonGrid : ZModuleButtonGrid
		{
			protected override ZController GetNewControllerCore(BusinessObject selected)
			{
				throw new NotImplementedException();
			}

			protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			{
				return new VoidZRecordAttacher(destinationCollection, findBoxList, moduleID);
			}

			public void FireAttachButton()
			{
				AttachButton_Click(this, EventArgs.Empty);
			}
		}

		class VoidZRecordAttacher : ZRecordAttacher
		{
			public VoidZRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
				: base(destinationCollection, findBoxList, moduleID)
			{
			}

			protected override void ShowCore(IZForm formToShowModalTo)
			{
			}
		}

		#endregion

		#region Test Classes

		class TestModuleButtonGrid : ZModuleButtonGrid
		{
			public IModuleDecisionProvider NewModuleDecisionProvider()
			{
				return GetNewModuleDecisionProvider(new DummyFindBox());
			}
		}

		public class NonPersistentParentForTest : NonPersistentBizOForTest
		{
			public string Z0_Description { get; set; }
			public int Z0_Number { get; set; }

			//public NonPersistentBizOForTestCollection Collection
			//{
			//	get
			//	{
			//		if (fCollection == null)
			//		{
			//			fCollection = new NonPersistentBizOForTestCollection();
			//		}
			//		return fCollection;
			//	}
			//}

			//NonPersistentBizOForTestCollection fCollection;

			//public DummyBusinessObjectCollection FilteredCollection
			//{
			//	get
			//	{
			//		if (filteredCollection == null)
			//		{
			//			filteredCollection = new DummyBusinessObjectCollection(Factory);
			//			RegisterEditableChildObject(filteredCollection);
			//		}
			//		return filteredCollection;
			//	}
			//	set { filteredCollection = value; }
			//}
			//DummyBusinessObjectCollection filteredCollection;

			public DummyBusinessObjectCollection Collection
			{
				get
				{
					if (collection == null)
					{
						collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
						RegisterEditableChildObject(collection);
					}
					return collection;
				}
				set { collection = value; }
			}
			DummyBusinessObjectCollection collection;

			public DummyBusinessObjectCollection FilteredCollection
			{
				get
				{
					if (filteredCollection == null)
					{
						filteredCollection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
						RegisterEditableChildObject(filteredCollection);
					}
					return filteredCollection;
				}
				set { filteredCollection = value; }
			}
			DummyBusinessObjectCollection filteredCollection;
		}

		public class NonPersistentBizOForTestParent : DummyBusinessObject
		{
			public NonPersistentBizOForTestParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new NonPersistentBizOForTestCollection Collection
			{
				get
				{
					if (fCollection == null)
					{
						fCollection = new NonPersistentBizOForTestCollection();
					}
					return fCollection;
				}
			}

			NonPersistentBizOForTestCollection fCollection;
		}

		public class NonPersistentBizOForTestCollection : NonPersistentBusinessObjectCollection<NonPersistentBizOForTest>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new NonPersistentBizOForTest();
			}
		}

		public class NonPersistentBizOForTest : NonPersistentBusinessObject
		{
			public ZString Description
			{
				get { return "Hello"; }
			}

			public ZPropertyInfo DescriptionInfo
			{
				get { return GetZPropertyInfo(nameof(Description)); }
			}
		}

		public class DummyParentBusinessObject : DummyBaseBusinessObject
		{
			public DummyParentBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public DummyBusinessObjectCollection Collection
			{
				get
				{
					if (collection == null)
					{
						collection = new DummyBusinessObjectCollection(Factory);
						RegisterEditableChildObject(collection);
					}
					return collection;
				}
				set { collection = value; }
			}
			DummyBusinessObjectCollection collection;

			public DummyBusinessObjectCollection FilteredCollection
			{
				get
				{
					if (filteredCollection == null)
					{
						filteredCollection = new DummyBusinessObjectCollection(Factory);
						RegisterEditableChildObject(filteredCollection);
					}
					return filteredCollection;
				}
				set { filteredCollection = value; }
			}
			DummyBusinessObjectCollection filteredCollection;
		}

		//WI00743739 - logging removed due to performance issues
		//public class DummyBusinessObjectForTest : DummyBusinessObject, Enterprise.Integration.IJobDocAddress
		//{
		//	public DummyBusinessObjectForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		//	{
		//	}

		//	public ZString E2_Address1 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_Address2 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZBool E2_AddressOverride { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZByte E2_AddressSequence { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_AddressType { get => "AddressType"; set => throw new NotImplementedException(); }
		//	public ZString E2_City { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_CompanyName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_Contact { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_Email { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_Fax { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_GovRegNum { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_GovRegNumType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZBool E2_IsResidential { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_Mobile { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZGuid E2_OA_Address { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZGuid E2_ParentID { get => ZGuid.BrettsGuid; set => throw new NotImplementedException(); }
		//	public ZString E2_ParentTableCode { get => "ParentTableCode"; set => throw new NotImplementedException(); }
		//	public ZString E2_Phone { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_Postcode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_RN_NKCountryCode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_ScreeningStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZString E2_ValidationStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZGuid OrganisationPK { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		//	public ZString AddressFull => throw new NotImplementedException();

		//	public ZString E2_AddressMap { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		//	public ZGeography E2_GeoLocation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		//	public string ConstructorStackTrace => "StackTrace";

		//	public string DocAddressCollectionInfo => "DocAddressCollectionInfo";

		//	public bool Equals(Enterprise.Integration.IJobDocAddress other)
		//	{
		//		throw new NotImplementedException();
		//	}
		//}

		#endregion

		#region Implementation

		ZModuleButtonGridTestForm Form
		{
			get { return form ?? (form = new ZModuleButtonGridTestForm(Dummy)); }
		}
		ZModuleButtonGridTestForm form;

		ZModuleButtonGridTestFormWithICanAttach FormWithICanAttach
		{
			get { return form2 ?? (form2 = new ZModuleButtonGridTestFormWithICanAttach(Dummy)); }
		}
		ZModuleButtonGridTestFormWithICanAttach form2;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (form2 != null)
			{
				form2.Dispose();
			}
		}

		public override Type FormToBashType
		{
			get { return typeof(ZModuleButtonGridTestForm); }
		}

		protected override Form GetFormToBashCore()
		{
			return new ZModuleButtonGridTestForm(Dummy);
		}

		public override bool AllowUntranslatableFormTitle()
		{
			return true;
		}

		DummyBusinessObject Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyBusinessObject>()); }
		}
		DummyBusinessObject dummy;

		#endregion

		public void TestSetButtonsReadOnly()
		{
			using (var btnGrid = new ZModuleButtonGrid())
			{
				Assert("DetachButton enabled", btnGrid.DetachButton.Enabled);
				Assert("AttachButton enabled", btnGrid.AttachButton.Enabled);
				Assert("NewButton enabled", btnGrid.NewButton.Enabled);
				Assert("EditButton enabled", btnGrid.EditButton.Enabled);
				AssertEquals("EditButton text", "Edit", btnGrid.EditButton.Text);
				Assert("Grid is not readonly", !btnGrid.InnerGrid.ReadOnly);

				btnGrid.SetButtonsReadOnly(true);
				Assert("DetachButton disabled", !btnGrid.DetachButton.Enabled);
				Assert("AttachButton disabled", !btnGrid.AttachButton.Enabled);
				Assert("NewButton disabled", !btnGrid.NewButton.Enabled);
				Assert("EditButton enabled", !btnGrid.EditButton.Enabled);
				AssertEquals("EditButton text", "View", btnGrid.EditButton.Text);
				Assert("Grid is still not readonly", !btnGrid.InnerGrid.ReadOnly);
			}
		}
	}

	sealed class ZModuleButtonGrid_Test : TestCase
	{
		public void TestSetButtonsReadOnly()
		{
			using (var btnGrid = new ZModuleButtonGrid())
			{
				Assert("DetachButton enabled", btnGrid.DetachButton.Enabled);
				Assert("AttachButton enabled", btnGrid.AttachButton.Enabled);
				Assert("NewButton enabled", btnGrid.NewButton.Enabled);
				Assert("EditButton enabled", btnGrid.EditButton.Enabled);
				AssertEquals("EditButton text", "Edit", btnGrid.EditButton.Text);
				Assert("Grid is not readonly", !btnGrid.InnerGrid.ReadOnly);

				btnGrid.SetButtonsReadOnly(true);
				Assert("DetachButton disabled", !btnGrid.DetachButton.Enabled);
				Assert("AttachButton disabled", !btnGrid.AttachButton.Enabled);
				Assert("NewButton disabled", !btnGrid.NewButton.Enabled);
				Assert("EditButton enabled", !btnGrid.EditButton.Enabled);
				AssertEquals("EditButton text", "View", btnGrid.EditButton.Text);
				Assert("Grid is still not readonly", !btnGrid.InnerGrid.ReadOnly);
			}
		}
	}
}
