using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RegistryForm))]
	sealed class RegistryFormTest : ZFormBasherTest
	{
		RegistryFormTreeViewBuilder Builder;

		protected override void SetUp()
		{
			base.SetUp();
			Env.Security.SystemRegistry.IsAllowed = true;
			Builder = new RegistryFormTreeViewBuilder(Factory);
		}

		protected override Form GetFormToBashCore()
		{
			return new RegistryForm();
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "NameOnDbTextBox";
		}

		public void TestRegistryFormAlwaysLoadDefaultPosition()
		{
			int initialLeft;
			int initialTop;
			int initialWidth;
			int initialHeight;

			using (var testForm = new RegistryFormForTest())
			using (new DisposableAction(Globals.IsTest_ForTest.ResetValue))
			{
				Globals.IsTest_ForTest.Value = false;
				testForm.Show();
				initialLeft = testForm.Left;
				initialTop = testForm.Top;
				initialWidth = testForm.Width;
				initialHeight = testForm.Height;
				testForm.Left = 100;
				testForm.Top = 100;
				testForm.Width = 2284;
				testForm.Height = 1179;
				EnterpriseFormLookStrategy.SavePositionAndSize(testForm);
			}

			using (new DisposableAction(Globals.IsTest_ForTest.ResetValue))
			using (var testForm = new RegistryFormForTest())
			{
				Globals.IsTest_ForTest.Value = false;
				testForm.Show();
				CombineAssertions("RegistryForm always load default position", () =>
				{
					AssertEquals("Left", initialLeft, testForm.Left);
					AssertEquals("Top", initialTop, testForm.Top);
					AssertEquals("Width", initialWidth, testForm.Width);
					AssertEquals("Height", initialHeight, testForm.Height);
				});
			}
		}

		public void TestShouldShowMessageIfSystemRegistryEditIsNotAllowed()
		{
			Env.Security.SystemRegistryEdit.IsAllowed = false;

			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();

				var registriesTreeView = testForm.GetRegistriesTreeView();
				var fallbackTreeView = testForm.GetFallbackTreeView();
				registriesTreeView.Nodes.Clear();

				var itemNode = new TreeNode("Item3") { Tag = new RegistryItemTag(WorkflowDataRegistry.Instance.TaskTypes) };
				registriesTreeView.Nodes.Add(itemNode);

				var checkpoint = Env.Security.GetRegistryCheckPoint(WorkflowDataRegistry.Instance.TaskTypes.Name, WorkflowDataRegistry.Instance.TaskTypes.Caption);
				checkpoint.IsAllowed = true;

				registriesTreeView.SelectedNode = itemNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				var securityLabel = testForm.GetSecurityDeniedLabel();
				Assert("SecurityDeniedLabel should be visible", securityLabel.Visible);
				AssertEquals("Should show SystemRegistryEdit is not allowed.", Env.Security.SystemRegistryEdit.ErrorMessageForNotAllowed, securityLabel.Text);
			}
		}

		public void TestPluginControlHeightWhenSecurityDeniedLabelIsVisible()
		{
			Env.Security.SystemRegistryEdit.IsAllowed = false;
			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();

				var registriesTreeView = testForm.GetRegistriesTreeView();
				var fallbackTreeView = testForm.GetFallbackTreeView();
				registriesTreeView.Nodes.Clear();

				var itemNode = new TreeNode("Item3") { Tag = new RegistryItemTag(WorkflowDataRegistry.Instance.TaskTypes) };
				registriesTreeView.Nodes.Add(itemNode);

				var checkpoint = Env.Security.GetRegistryCheckPoint(WorkflowDataRegistry.Instance.TaskTypes.Name, WorkflowDataRegistry.Instance.TaskTypes.Caption);
				checkpoint.IsAllowed = false;

				registriesTreeView.SelectedNode = itemNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				var securityLabel = testForm.GetSecurityDeniedLabel();
				Assert("SecurityDeniedLabel should be visible", securityLabel.Visible);

				var pluginControl = testForm.GetPluginControl();
				var pluginControlLeftBottomLocation = pluginControl.Location.Y + pluginControl.Height;
				Assert("plugin control left bottom location should less than the security label left top location", pluginControlLeftBottomLocation <= securityLabel.Location.Y);
			}
		}

		public void TestOverrideCheckboxEditEnablesSaveButton()
		{
			using (var testForm = new RegistryFormForTest())
			{
				var registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Clear();
				testForm.Show();
				var testNode = new TreeNode();
				var regItem = new LicencedStringRegistryItem(() => false, "TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
				var itemTag = new RegistryItemTag(regItem);
				testNode.Tag = itemTag;
				registriesTreeView.Nodes.Add(testNode);
				registriesTreeView.SelectedNode = testNode;
				var saveButton = testForm.GetSaveButton();

				Assert("Checkbox shouldn't be checked", !testForm.GetOverrideCheckBox().Checked);
				Assert("Save button should be disabled", !saveButton.Enabled);

				testForm.GetOverrideCheckBox().Checked = true;
				Assert("Save button should be enabled", saveButton.Enabled);
			}
		}

		public void TestRadioButtonControlEditEnablesSaveButton()
		{
			using (var testForm = new RegistryFormForTest())
			{
				var registriesTreeView = testForm.GetRegistriesTreeView();
				testForm.Show();
				var testNode = new TreeNode();
				var regItem = new BooleanRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, true);
				regItem.EditorInfo = new BooleanRegistryEditorInfo();
				var itemTag = new RegistryItemTag(regItem);
				testNode.Tag = itemTag;
				registriesTreeView.Nodes.Add(testNode);
				registriesTreeView.SelectedNode = testNode;
				var saveButton = testForm.GetSaveButton();

				testForm.GetOverrideCheckBox().Checked = true;
				testForm.OnSaveResult = DialogResult.Yes;
				saveButton.PerformClick();

				Assert("Save button should be disabled", !saveButton.Enabled);
				AssertType<RadioButtonControl>(testForm.GetPluginControl());

				var controlToInteractWith = (RadioButtonControl)testForm.GetPluginControl();
				var controlBoundBusinessObject = (BusinessObject)controlToInteractWith.CurrentDataItem;
				Assert("Bound business object should not have changes", !controlBoundBusinessObject.HasChanges);

				controlToInteractWith.Value = !controlToInteractWith.Value;
				Assert("Bound business object should have changes", controlBoundBusinessObject.HasChanges);
				Assert("Save button should be enabled", saveButton.Enabled);
			}
		}

		public void TestCalcEditControlEditEnablesSaveButton()
		{
			using (var testForm = new RegistryFormForTest())
			{
				var registriesTreeView = testForm.GetRegistriesTreeView();
				testForm.Show();
				var testNode = new TreeNode();
				var regItem = new IntRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, 100);
				regItem.EditorInfo = new NumericRegistryEditorInfo(0);
				var itemTag = new RegistryItemTag(regItem);
				testNode.Tag = itemTag;
				registriesTreeView.Nodes.Add(testNode);
				registriesTreeView.SelectedNode = testNode;
				var saveButton = testForm.GetSaveButton();

				testForm.GetOverrideCheckBox().Checked = true;
				testForm.OnSaveResult = DialogResult.Yes;
				saveButton.PerformClick();

				Assert("Save button should be disabled", !saveButton.Enabled);
				AssertType<ZUserControl>(testForm.GetPluginControl());

				var pluginControl = (ZUserControl)testForm.GetPluginControl();
				var calcEdit = (ZCalcEdit)pluginControl.Controls[0];
				var controlBoundBusinessObject = (BusinessObject)((pluginControl).CurrentDataItem);
				Assert("Bound business object should not have changes", !controlBoundBusinessObject.HasChanges);

				calcEdit.Text = "10000";
				Assert("Bound business object should have changes", controlBoundBusinessObject.HasChanges);
				Assert("Save button should be enabled", saveButton.Enabled);
			}
		}

		[ExpectNoExceptions]
		public void TestRegistryItemCollectionElementWithChildrenSave()
		{
			var taskTypeRestrictionsCollection = new TaskTypeRestrictionsCollection();
			var taskTypeRestrictions = taskTypeRestrictionsCollection.AddNew();
			taskTypeRestrictions.WorkflowType = "ACI";
			taskTypeRestrictions.TaskType = "UDF";
			taskTypeRestrictions.Active = true;
			taskTypeRestrictions.RestrictionType = "SAM";
			taskTypeRestrictions.NotificationType = "ERR";

			var restrictedTaskTypesCollection = new RestrictedTaskTypesCollection();
			var restrictedTaskTypes = restrictedTaskTypesCollection.AddNew();
			restrictedTaskTypes.WorkflowType = "ACI";
			restrictedTaskTypes.Code = "UDF";

			taskTypeRestrictions.SetTaskTypes(restrictedTaskTypesCollection);

			using (var testForm = new RegistryFormForTest())
			{
				var registriesTreeView = testForm.GetRegistriesTreeView();
				testForm.Show();

				var testNode = new TreeNode();
				var regItem = new TaskAssignmentRestrictionsRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, taskTypeRestrictionsCollection);
				var itemTag = new RegistryItemTag(regItem);
				testNode.Tag = itemTag;
				registriesTreeView.Nodes.Add(testNode);
				registriesTreeView.SelectedNode = testNode;

				testForm.GetOverrideCheckBox().Checked = true;
				testForm.OnSaveResult = DialogResult.Yes;

				var pluginControl = (ZUserControl)testForm.GetPluginControl();
				var dataSource = (TaskTypeRestrictionsCollection)pluginControl.BindingSource.DataSource;
				var dataRow = dataSource[0];
				dataRow.Active = false;

				var saveButton = testForm.GetSaveButton();
				Assert("Save button should be enabled", saveButton.Enabled);
				saveButton.PerformClick();
			}
		}

		public void TestCodeDescriptionBoolEditEnablesSaveButton()
		{
			using (var testForm = new RegistryFormForTest())
			{
				// Arrange
				var registriesTreeView = testForm.GetRegistriesTreeView();
				testForm.Show();
				var testNode = new TreeNode();
				var regItem = new CodeDescriptionBoolRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, new CodeDescriptionBoolRegistryEditorInfo((NoResString)"TestCaption", true));
				var itemTag = new RegistryItemTag(regItem);
				testNode.Tag = itemTag;
				registriesTreeView.Nodes.Add(testNode);
				registriesTreeView.SelectedNode = testNode;
				var saveButton = testForm.GetSaveButton();
				Assert("Save button should be disabled", !saveButton.Enabled);
				// Act & Assert 1
				testForm.GetOverrideCheckBox().Checked = true;
				testForm.OnSaveResult = DialogResult.Yes;
				var pluginControl = (ZUserControl)testForm.GetPluginControl();
				var dataSource = (CodeDescriptionBoolCollection)pluginControl.BindingSource.DataSource;
				var dataRow = dataSource.AddNew();
				dataRow.Code = "ABC";
				dataRow.Bool = true;
				Assert("Save button should be enabled", saveButton.Enabled);
				saveButton.PerformClick();
				Assert("Save button should be disabled", !saveButton.Enabled);
				// Act & Assert 2
				dataRow.Bool = false;
				Assert("Save button should be enabled", saveButton.Enabled);
				saveButton.PerformClick();
				Assert("Save button should be disabled", !saveButton.Enabled);
			}
		}

		[RequiresSTA]
		public void TestCodeDescriptionListEditEnablesSaveButton()
		{
			using (var testForm = new RegistryFormForTest())
			{
				var registriesTreeView = testForm.GetRegistriesTreeView();
				testForm.Show();
				var testNode = new TreeNode();
				var regItem = new CodeDescriptionPairListRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", 3, RegistryStorageFlags.All, new ReadOnlyCodeDescriptionPairList());
				var itemTag = new RegistryItemTag(regItem);
				testNode.Tag = itemTag;
				registriesTreeView.Nodes.Add(testNode);
				registriesTreeView.SelectedNode = testNode;
				var saveButton = testForm.GetSaveButton();
				Assert("Save button should be disabled", !saveButton.Enabled);

				testForm.GetOverrideCheckBox().Checked = true;
				testForm.OnSaveResult = DialogResult.Yes;
				saveButton.PerformClick();
				Assert("Save button should be disabled", !saveButton.Enabled);

				var pluginControl = (CodeDescriptionListEditControlForRegistry)testForm.GetPluginControl();
				KeySender.SendKeyDownToProcessCmdKey(pluginControl, (int)Keys.X);
				Assert("Save button should be enabled", saveButton.Enabled);
				saveButton.PerformClick();
				Assert("Save button should be disabled", !saveButton.Enabled);
			}
		}

		[ExpectNoExceptions]
		public void TestSavingCodeDescriptionPairList_WhenSelectRegistryItemWithAnotherType()
		{
			using (var testForm = new RegistryFormForTest())
			{
				var registriesTreeView = testForm.GetRegistriesTreeView();

				testForm.Show();

				var codeDescriptionPairListNode = new TreeNode("TestCodeDescriptionPairListNode")
				{
					Tag = new RegistryItemTag(new CodeDescriptionPairListRegistryItem(
						name: "TestCodeDescriptionPairListItem",
						category: (NoResString)"Category",
						caption: (NoResString)"TestCodeDescriptionPairListCaption",
						hint: (NoResString)"TestCodeDescriptionPairListHint",
						maxCodeLength: 3,
						editorInfo: new CodeDescriptionPairListEditorInfo(true, false),
						storage: RegistryStorageFlags.All,
						options: RegistryOptions.Default,
						defaultValue: new ReadOnlyCodeDescriptionPairList(),
						useDefaultDefaultValue: false)),
				};

				var textNode = new TreeNode("TestTextNode")
				{
					Tag = new RegistryItemTag(new StringRegistryItem("TestTextItem", (NoResString)"Category", (NoResString)"TestTextCaption", (NoResString)"TestTextHint", RegistryStorageFlags.All)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox),
					}),
				};

				registriesTreeView.Nodes.Add(codeDescriptionPairListNode);
				registriesTreeView.Nodes.Add(textNode);

				registriesTreeView.SelectedNode = codeDescriptionPairListNode;

				var saveButton = testForm.GetSaveButton();
				testForm.GetOverrideCheckBox().Checked = true;
				testForm.OnSaveResult = DialogResult.Yes;

				registriesTreeView.SelectedNode = textNode;

				saveButton.PerformClick();
			}
		}

		public void TestFormOnStartup()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				Assert("Form's HasChanges property should be false on startup", !testForm.HasChangesForTest);
				Assert("HideMenuItem should be checked on startup", testForm.GetHideMenuItem().Checked);
				Assert("ValueLabel should not be visible on form startup", !testForm.GetValueLabel().Visible);
				Assert("OverrideCheckBox should not be visible on form startup", !testForm.GetOverrideCheckBox().Visible);
				Assert("FallbackMessageLabel should not be visible on form startup", !testForm.GetFallbackMessageLabel().Visible);
				Assert("SecurityDeniedLabel should not be visible on form startup", !testForm.GetSecurityDeniedLabel().Visible);
				Assert("FilterChangeLogs should not be checked on form startup", !testForm.GetFilterChangeLogsMenuItem().Checked);
				Control pluginControl = testForm.GetPluginControl();
				Assert("PluginControl should be null on form startup", testForm.GetPluginControl() == null);
				ZPanel pluginPanel = testForm.GetPluginPanel();
				Assert("PluginControl should not be in the PluginPanel on form startup", !pluginPanel.Contains(pluginControl));
				AssertEquals("HintLabel.Text", testForm.GetDefaultHint(), testForm.GetHintLabel().Text);
			}
		}

		[RequiresSTA]
		public void TestLicencedRegistry()
		{
			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();

				var registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Clear();

				var testNode = new TreeNode();
				IRegistryItem regItem = new LicencedStringRegistryItem(() => false, "TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
				RegistryItemTag item = new RegistryItemTag(regItem);
				testNode.Tag = item;
				registriesTreeView.Nodes.Add(testNode);
				registriesTreeView.SelectedNode = testNode;
				Assert("OverrideCheckBox should not be visible", !testForm.GetOverrideCheckBox().Visible);
				Assert("ValueLabel should not be visible", !testForm.GetValueLabel().Visible);
				Assert("FallbackMessageLabel should not be visible", !testForm.GetFallbackMessageLabel().Visible);
				Assert("HintLabel should be visible", testForm.GetHintLabel().Visible);
				AssertEquals("This module is no longer available in CW1. This functionality has been superseded by the eAdaptor web service interface. Please contact your sales rep.", testForm.GetPluginControl().Text);
			}
		}

		[RequiresSTA]
		public void TestFillFallbackTreeView()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView testTree = new TreeView();
				TreeNode testNode = new TreeNode();

				IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
				RegistryItemTag item = new RegistryItemTag(regItem);
				testNode.Tag = item;

				Assert("Precondition: HideMenuItem should be checked", testForm.GetHideMenuItem().Checked);
				Builder.SetHideInactiveFallbacks(true);
				Builder.UpdateFallbackTree(testTree, testNode);
				testForm.FillFallbackTreeViewForTest(testNode);
				AssertEquals("FallbackTreeView Node Count", testTree.GetNodeCount(true), testForm.GetFallbackTreeView().GetNodeCount(true));

				testForm.GetHideMenuItem().PerformClick();
				Builder.SetHideInactiveFallbacks(false);
				Builder.UpdateFallbackTree(testTree, testNode);
				testForm.FillFallbackTreeViewForTest(testNode);
				AssertEquals("FallbackTreeView Node Count", testTree.GetNodeCount(true), testForm.GetFallbackTreeView().GetNodeCount(true));
			}
		}

		public void TestFillFallbackTreeViewIfRegistryNodeIsNull()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				Assert("Precondition: PerformUpdateRegistryItemValueFromPluginControlCalled should be false", !testForm.PerformUpdateRegistryItemValueFromPluginControlCalled);
				testForm.FillFallbackTreeViewForTest(null);
				Assert("PerformUpdateRegistryItemValueFromPluginControlCalled should be false", !testForm.PerformUpdateRegistryItemValueFromPluginControlCalled);
			}
		}

		public void TestLoadsTopLevelNodes()
		{
			// Arrange
			var registryMock = new Mock<IRegistry>(MockBehavior.Strict);
			registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(new[]
			{
				new RegistryCategoryRef(1, "Category 1"),
				new RegistryCategoryRef(2, "Category 2"),
			});

			// Act
			using (var form = new RegistryFormForTest(registryMock.Object))
			{
				// Assert
				var treeView = form.GetRegistriesTreeView();
				AssertContainsExactElementsInExactOrder(new[] { "Category 1", "Category 2" }, treeView.Nodes.Cast<TreeNode>().Select(n => n.Text));
			}
		}

		[RequiresSTA]
		public void TestDoesNotLoadChildNodes()
		{
			// Arrange
			const int cat1Key = 1;
			var registryMock = new Mock<IRegistry>(MockBehavior.Strict);
			registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(new[] { new RegistryCategoryRef(cat1Key, "Cat-1") });
			registryMock.Setup(r => r.GetSortedContent(cat1Key)).Returns(new RegistryCategoryContent(Array.Empty<RegistryCategoryRef>(), Array.Empty<IRegistryItem>()));

			// Act
			using (var form = new RegistryFormForTest(registryMock.Object))
			{
				// Assert
				var treeView = form.GetRegistriesTreeView();
				AssertEquals("Should load top level", 1, treeView.Nodes.Count);

				var node = treeView.Nodes[0];
				if (node.Nodes.Count == 0)
				{
					Fail("Should put a child node so that expand button is visible: " + node.Text);
				}
				else if (node.Nodes.Count > 1)
				{
					Fail("Should not load nested level unless demanded: " + node.Text);
				}
				else
				{
					AssertEquals("Expected loading node", "...", node.Nodes[0].Text);
				}

				AssertNoExceptionThrown(() =>
				{
					registryMock.Verify(r => r.GetSortedContent(cat1Key), Times.Never);
				});
			}
		}

		public void TestLoadsChildNodesOnDemand()
		{
			// Arrange
			const int cat1Key = 1;
			var registryMock = new Mock<IRegistry>(MockBehavior.Strict);
			registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(new[] { new RegistryCategoryRef(cat1Key, "Cat-1") });
			registryMock.Setup(r => r.GetSortedContent(cat1Key)).Returns(new RegistryCategoryContent(new[]
				{
					new RegistryCategoryRef(11, "Sub-Category 11"),
					new RegistryCategoryRef(12, "Sub-Category 12"),
				},
				new IRegistryItem[]
				{
					new StringRegistryItem("TestItem1", null, (NoResString)"ZZ-Item 1", null, RegistryStorageFlags.System),
					new StringRegistryItem("TestItem2", null, (NoResString)"ZZ-Item 2", null, RegistryStorageFlags.System),
				}));

			using (var form = new RegistryFormForTest(registryMock.Object))
			{
				var treeView = form.GetRegistriesTreeView();
				var cat1 = treeView.Nodes[0];

				// Act
				treeView.ExpandNodeIfNeeded(cat1);

				// Assert	
				AssertContainsExactElementsInExactOrder(new[] { "ZZ-Item 1", "ZZ-Item 2", "Sub-Category 11", "Sub-Category 12" }, cat1.Nodes.Cast<TreeNode>().Select(n => n.Text));
				AssertNoExceptionThrown(() =>
				{
					registryMock.Verify(r => r.GetSortedContent(cat1Key), Times.Once);
				});
			}
		}

		[RequiresSTA]
		public void TestDoesNotLoadChildNodesAgain()
		{
			// Arrange
			const int cat1Key = 1;
			var registryMock = new Mock<IRegistry>(MockBehavior.Strict);
			registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(new[] { new RegistryCategoryRef(cat1Key, "Cat-1") });
			registryMock.Setup(r => r.GetSortedContent(cat1Key)).Returns(new RegistryCategoryContent(Array.Empty<RegistryCategoryRef>(), Array.Empty<IRegistryItem>()));

			using (var form = new RegistryFormForTest(registryMock.Object))
			{
				var treeView = form.GetRegistriesTreeView();
				var cat1 = treeView.Nodes[0];

				treeView.ExpandNodeIfNeeded(cat1);
				cat1.Collapse();

				registryMock.Invocations.Clear();

				// Act
				treeView.ExpandNodeIfNeeded(cat1);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					registryMock.Verify(r => r.GetSortedContent(cat1Key), Times.Never);
				});
			}
		}

		[RequiresSTA]
		public void TestDoesNotShowHiddenRegistryItems()
		{
			// Arrange
			var visible = new StringRegistryItem("TestItem1", (NoResString)"Category1", (NoResString)"TestCaption1", (NoResString)"TestHint1", RegistryStorageFlags.System);
			var hidden = new StringRegistryItem("TestItem2", (NoResString)"Category2", (NoResString)"TestCaption2", (NoResString)"TestHint2", RegistryStorageFlags.System, RegistryOptions.IsHidden);

			const int cat1Key = 1;
			var registryMock = new Mock<IRegistry>(MockBehavior.Strict);
			registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(new[] { new RegistryCategoryRef(cat1Key, "Cat-1") });
			registryMock.Setup(r => r.GetSortedContent(cat1Key)).Returns(new RegistryCategoryContent(Array.Empty<RegistryCategoryRef>(), new IRegistryItem[] { visible, hidden }));

			using (var form = new RegistryFormForTest(registryMock.Object))
			{
				var treeView = form.GetRegistriesTreeView();
				var cat1 = treeView.Nodes[0];

				// Act
				treeView.ExpandNodeIfNeeded(cat1);

				// Assert
				AssertEquals(1, cat1.Nodes.Count);
				AssertEquals("TestCaption1", cat1.Nodes[0].Text);
			}
		}

		[RequiresSTA]
		public void TestSearchOnLazyTree()
		{
			//Arrange
			var registryItem = new StringRegistryItem("TestItem1", (NoResString)"Category1", (NoResString)"TestCaption1", (NoResString)"TestHint1", RegistryStorageFlags.System);
			using (var form = new RegistryFormForTest(registryItem))
			{
				var treeView = form.GetRegistriesTreeView();
				var searcher = treeView.TreeViewSearcher;

				// Act
				var searchResult = searcher.Search(treeView, "TestCaption1", StringComparison.Ordinal);

				// Assert
				var node = searchResult.Next();
				AssertNotNull(node);
				AssertEquals("TestCaption1", node.Text);
				AssertEquals("Category1", node.Parent.Text);

				AssertNull(searchResult.Next());
			}
		}

		[RequiresSTA]
		public void TestSearchRetrievesInDepthFirstOrder()
		{
			//Arrange
			var registryMock = new Mock<IRegistry>(MockBehavior.Strict);

			registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(new[] { new RegistryCategoryRef(1, "node 1") });
			registryMock.Setup(r => r.GetSortedContent(It.IsAny<object>())).Returns(new Func<object, RegistryCategoryContent>(key =>
			{
				switch ((int)key)
				{
					case 1: // node 1
						return new RegistryCategoryContent(
							new[] { new RegistryCategoryRef(11, "node 1-1"), new RegistryCategoryRef(12, "node 1-2") },
							Array.Empty<IRegistryItem>());
					case 11: // node 1-1
						return new RegistryCategoryContent(
							Array.Empty<RegistryCategoryRef>(),
							new IRegistryItem[] { new StringRegistryItem("Key111", (NoResString)"", (NoResString)"node 1-1-1", (NoResString)"", RegistryStorageFlags.System) });
					case 12: // node 1-2
						return new RegistryCategoryContent(
							Array.Empty<RegistryCategoryRef>(),
							new IRegistryItem[] { new StringRegistryItem("Key121", (NoResString)"", (NoResString)"node 1-2-1", (NoResString)"", RegistryStorageFlags.System) });
					default:
						throw new ArgumentOutOfRangeException(nameof(key));
				}
			}));

			using (var form = new RegistryFormForTest(registryMock.Object))
			{
				var treeView = form.GetRegistriesTreeView();
				var searcher = treeView.TreeViewSearcher;

				// Act
				var searchResult = searcher.Search(treeView, "node", StringComparison.Ordinal);

				// Assert
				AssertEquals("node 1", searchResult.Next()?.Text);
				AssertEquals("node 1-1", searchResult.Next()?.Text);
				AssertEquals("node 1-1-1", searchResult.Next()?.Text);
				AssertEquals("node 1-2", searchResult.Next()?.Text);
				AssertEquals("node 1-2-1", searchResult.Next()?.Text);
			}
		}

		[RequiresSTA]
		public void TestShowSettingsForFallbackWhenItsRegistryIsSearchedPast()
		{
			using (var form = new RegistryFormForTest())
			{
				form.Show();
				var treeView = form.GetRegistriesTreeView();
				var searcher = treeView.TreeViewSearcher;
				var fallbackTreeView = form.GetFallbackTreeView();
				SetupRegistriesTreeView(treeView, true);
				treeView.SelectedNode = ItemNode1;

				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[1].FirstNode;

				Assert(form.GetValueLabel().Visible);

				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				var searchResult = searcher.Search(treeView, "Item", StringComparison.Ordinal);

				treeView.SelectedNode = searchResult.Next();
				AssertNotNull(treeView.SelectedNode);
				treeView.SelectedNode = searchResult.Next();
				AssertNotNull(treeView.SelectedNode);

				treeView.SelectedNode = searchResult.Next();
				AssertNull(treeView.SelectedNode);

				var firstNode = fallbackTreeView.Nodes[1].FirstNode;
				var lastNode = fallbackTreeView.Nodes[1].LastNode;
				Assert(firstNode != lastNode);

				fallbackTreeView.SelectedNode = firstNode;
				Assert(form.GetValueLabel().Visible);

				fallbackTreeView.SelectedNode = lastNode;
				Assert(form.GetValueLabel().Visible);
			}
		}

		public void TestCheckKeyResolvableToSelectedRegistryNode()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				ArrayList item1Value = new ArrayList();
				item1Value.Add("Item1");
				ArrayList item2Value = new ArrayList();
				item2Value.Add("Item2");
				ArrayList lastSelectedValue = new ArrayList();
				lastSelectedValue.Add("LastSelected");

				TreeView tree = new TreeView();
				TreeNode categoryNode1 = new TreeNode("Category1");
				tree.Nodes.Add(categoryNode1);
				TreeNode categoryNode2 = new TreeNode("Category2");
				tree.Nodes.Add(categoryNode2);
				TreeNode itemNode1 = new TreeNode("Item1");
				categoryNode1.Nodes.Add(itemNode1);
				TreeNode itemNode2 = new TreeNode("Item2");
				categoryNode2.Nodes.Add(itemNode2);

				Hashtable testArray = new Hashtable();
				testArray.Add(itemNode1, item1Value);
				testArray.Add(itemNode2, item2Value);
				testArray.Add(testForm.GetLastSelectedNodeKey(), lastSelectedValue);

				AssertEquals("First fallback item that was added", item1Value, testArray[itemNode1]);
				AssertEquals("Second fallback item that was added", item2Value, testArray[itemNode2]);
				AssertEquals("Last selected fallback item", lastSelectedValue, testArray[testForm.GetLastSelectedNodeKey()]);
			}
		}

		[RequiresSTA]
		public void TestSetSelectedFallback()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, false);
				registriesTreeView.SelectedNode = ItemNode1;

				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[1];
				string selectedFallbackText = fallbackTreeView.Nodes[1].Text;
				string comparator = ((FallbackTreeNode)fallbackTreeView.SelectedNode).Comparator;
				AssertEquals("Fallback selected", selectedFallbackText, fallbackTreeView.SelectedNode.Text);

				registriesTreeView.Select();
				Hashtable selectedFallbacksArray = testForm.GetSelectedFallbacksHash();
				AssertEquals("SelectedFallbacksArray Count", 2, selectedFallbacksArray.Count);

				FallbackTreeNode obtainedNode = (FallbackTreeNode)selectedFallbacksArray[registriesTreeView.SelectedNode];
				AssertEquals("ObtainedNode's Comparator", comparator, obtainedNode.Comparator);
				obtainedNode = (FallbackTreeNode)selectedFallbacksArray[testForm.GetLastSelectedNodeKey()];
				AssertEquals("ObtainedNode's Comparator", comparator, obtainedNode.Comparator);
			}
		}

		public void TestGetSelectedFallback()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeNode testNode1 = new TreeNode("TestCaption1");
				IRegistryItem regItem = new StringRegistryItem("TestItem1", (NoResString)"Category1", (NoResString)"TestCaption1", (NoResString)"TestHint1", RegistryStorageFlags.System);
				RegistryItemTag item = new RegistryItemTag(regItem);
				testNode1.Tag = item;

				TreeNode testNode2 = new TreeNode("TestCaption2");
				regItem = new StringRegistryItem("TestItem2", (NoResString)"Category2", (NoResString)"TestCaption2", (NoResString)"TestHint2", RegistryStorageFlags.Company);
				item = new RegistryItemTag(regItem);
				testNode2.Tag = item;

				TreeNode testNode3 = new TreeNode("TestCaption3");
				regItem = new StringRegistryItem("TestItem3", (NoResString)"Category3", (NoResString)"TestCaption3", (NoResString)"TestHint3", RegistryStorageFlags.All);
				item = new RegistryItemTag(regItem);
				testNode3.Tag = item;

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Add(testNode1);
				registriesTreeView.Nodes.Add(testNode2);
				registriesTreeView.Nodes.Add(testNode3);
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				registriesTreeView.SelectedNode = testNode1;
				Assert("Enterprise/System fallback should be automatically selected if none were previously selected",
				fallbackTreeView.SelectedNode == fallbackTreeView.Nodes[0]);

				registriesTreeView.SelectedNode = testNode2;
				Assert("Company fallback should not be automatically selected if none were previously selected", fallbackTreeView.SelectedNode == null);
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				registriesTreeView.SelectedNode = testNode1;
				Assert("First fallback should be automatically selected", fallbackTreeView.SelectedNode == fallbackTreeView.Nodes[0]);

				registriesTreeView.SelectedNode = testNode2;
				Assert("First fallback should be automatically selected", fallbackTreeView.SelectedNode == fallbackTreeView.Nodes[0]);

				registriesTreeView.SelectedNode = testNode3;
				Assert("Second fallback should be automatically selected", fallbackTreeView.SelectedNode == fallbackTreeView.Nodes[1]);
			}
		}

		public void TestUpdatePluginPanel()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				registriesTreeView.Nodes.Clear();

				IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.CompanyDepartment);
				regItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				RegistryItemTag item = new RegistryItemTag(regItem);
				TreeNode itemNode = new TreeNode("Item");
				itemNode.Tag = item;
				registriesTreeView.Nodes.Add(itemNode);

				registriesTreeView.SelectedNode = itemNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				Assert("ValueLabel should not be visible", !testForm.GetValueLabel().Visible);
				Assert("FallbackMessageLabel should be visible", testForm.GetFallbackMessageLabel().Visible);
				AssertEquals("FallbackMessageLabel.Text", "Please select a valid fallback level.", testForm.GetFallbackMessageLabel().Text);
				Assert("OverrideCheckBox should not be visible", !testForm.GetOverrideCheckBox().Visible);
				Assert("PluginControl should be null", testForm.GetPluginControl() == null);
				Assert("FallbackMessageLabel should be in the PluginPanel", testForm.GetPluginPanel().Contains(testForm.GetFallbackMessageLabel()));
				Assert("PluginControl should not be in the PluginPanel", !testForm.GetPluginPanel().Contains(testForm.GetPluginControl()));
				AssertEquals("Changes Log & Notes tab pages should not be added", 1, testForm.GetRegistryItemTabControl().TabPages.Count);

				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode.FirstNode;

				Assert("ValueLabel should be visible", testForm.GetValueLabel().Visible);
				Assert("OverrideCheckBox should be visible", testForm.GetOverrideCheckBox().Visible);
				Assert("OverrideCheckBox should not be checked if the Registry Item has no value", !testForm.GetOverrideCheckBox().Checked);
				Assert("PluginControl should not be null", testForm.GetPluginControl() != null);
				Assert("PluginControl should be disabled if OverrideCheckBox is not checked", testForm.GetPluginControl().GetReadOnly());
				Assert("PluginControl should be in the PluginPanel", testForm.GetPluginPanel().Contains(testForm.GetPluginControl()));
				AssertEquals("Changes Log & Default Notes tab pages should be added", 3, testForm.GetRegistryItemTabControl().TabPages.Count);
				AssertEquals("Changes Log tab page should be added", typeof(ZStmALogTabPage), testForm.GetRegistryItemTabControl().TabPages[1].GetType());
				AssertEquals("Default notes tab page should be added", typeof(ZTabPage), testForm.GetRegistryItemTabControl().TabPages[2].GetType());

				item = (RegistryItemTag)itemNode.Tag;
				item.HasValue = true;

				// Refreshing the plugin panel
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode.FirstNode;

				Assert("OverrideCheckBox should be checked if the Registry Item has a value", testForm.GetOverrideCheckBox().Checked);
				Assert("PluginControl should be enabled if OverrideCheckBox is checked", !testForm.GetPluginControl().GetReadOnly());

				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode;
				Assert("ValueLabel should not be visible", !testForm.GetValueLabel().Visible);
				Assert("FallbackMessageLabel should be visible", testForm.GetFallbackMessageLabel().Visible);
				AssertEquals("FallbackMessageLabel.Text", "The selected fallback level does not apply to this Registry item.", testForm.GetFallbackMessageLabel().Text);
				Assert("OverrideCheckBox should not be visible", !testForm.GetOverrideCheckBox().Visible);
				Assert("FallbackMessageLabel should be in the PluginPanel", testForm.GetPluginPanel().Contains(testForm.GetFallbackMessageLabel()));
				Assert("PluginControl should not be in the PluginPanel", !testForm.GetPluginPanel().Contains(testForm.GetPluginControl()));
				AssertEquals("Changes Log & Notes tab pages should not be added", 1, testForm.GetRegistryItemTabControl().TabPages.Count);

				IRegistryItem regItem2 = new StringRegistryItem("TestItem2", (NoResString)"Category2", (NoResString)"TestCaption2", (NoResString)"TestHint2", RegistryStorageFlags.All);
				regItem2.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				RegistryItemTag item2 = new RegistryItemTag(regItem2);
				TreeNode itemNode2 = new TreeNode("Item2");
				itemNode2.Tag = item2;
				registriesTreeView.Nodes.Add(itemNode2);

				registriesTreeView.SelectedNode = itemNode2;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[1].FirstNode;
				Assert("FallbackMessageLabel should be visible", testForm.GetFallbackMessageLabel().Visible);
				AssertEquals("FallbackMessageLabel.Text", "Default value obtained from Registry item default.", testForm.GetFallbackMessageLabel().Text);

				IRegistryItem regItem3 = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint",
				RegistryStorageFlags.System, RegistryOptions.IsReadOnly, "DEFAULT");
				regItem3.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				regItem3.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue");
				RegistryItemTag item3 = new RegistryItemTag(regItem3);
				TreeNode itemNode3 = new TreeNode("Item3");
				itemNode3.Tag = item3;
				registriesTreeView.Nodes.Add(itemNode3);

				registriesTreeView.SelectedNode = itemNode3;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				Assert("ValueLabel should be visible", testForm.GetValueLabel().Visible);
				Assert("FallbackMessageLabel should be visible", testForm.GetFallbackMessageLabel().Visible);
				AssertEquals("FallbackMessageLabel.Text", "Default value obtained from System level.", testForm.GetFallbackMessageLabel().Text);
				Assert("OverrideCheckBox should not be visible", !testForm.GetOverrideCheckBox().Visible);
				Assert("FallbackMessageLabel should be in the PluginPanel", testForm.GetPluginPanel().Contains(testForm.GetFallbackMessageLabel()));
				Assert("PluginControl should be in the PluginPanel", testForm.GetPluginPanel().Contains(testForm.GetPluginControl()));
				Assert("PluginControl should be disabled", testForm.GetPluginControl().GetReadOnly());
				AssertEquals("Changes Log & Notes tab pages should be added", 3, testForm.GetRegistryItemTabControl().TabPages.Count);
				AssertEquals("Changes Log tab page should be added", typeof(ZStmALogTabPage), testForm.GetRegistryItemTabControl().TabPages[1].GetType());
				AssertEquals("Notes tab page should be added", typeof(ZStmNoteForRegistryItemTabPage), testForm.GetRegistryItemTabControl().TabPages[2].GetType());
				AssertEquals("Item DefaultValue", "DEFAULT", item3.DefaultValue);
				AssertEquals("Item Current Value", "NewValue", item3.GetValue());
				AssertEquals("PluginControl Text", "NewValue", testForm.GetPluginControl().Controls[0].Text);
			}
		}

		public void TestNewLogCanBeSaved()
		{
			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext("CWPostMaster", EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();

				var registriesTreeView = testForm.GetRegistriesTreeView();
				var fallbackTreeView = testForm.GetFallbackTreeView();
				registriesTreeView.Nodes.Clear();

				var regItem = new StringRegistryItem("TestName3", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, "DEFAULT");
				regItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue");
				var item = new RegistryItemTag(regItem);
				var itemNode = new TreeNode("Item3");
				itemNode.Tag = item;
				registriesTreeView.Nodes.Add(itemNode);

				registriesTreeView.SelectedNode = itemNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				var stmData = testForm.Factory.Load<StmData>(item.GetRegistryItemPK());
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				stmData.Logs.AddNew(Events.EditedARecord, "Add New log");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

				Assert(testForm.HasChangesForTest);

				testForm.OnSaveResult = DialogResult.Yes;
				testForm.GetSaveButton().PerformClick();
				Assert(!testForm.HasChangesForTest);

				var query = new ZQuery(StmALogSchema.SL_Reference, "Add New log");
				var newLog = (new BusinessObjectFactory()).LoadTop1<StmALog>(query);
				AssertNotNull(newLog);
			}
		}

		public void TestSaveLogsPreviousValue()
		{
			AssertSaveLogsPreviousValue(false, 0);
			AssertSaveLogsPreviousValue(true, 1);
		}

		void AssertSaveLogsPreviousValue(bool enhancedLogging, int expectedLogCount)
		{
			using (SystemDataRegistry.Instance.EnableEnhancedLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enhancedLogging))
			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();
				var saveButton = testForm.GetSaveButton();

				SetupTestItem1();
				Item1.HasValue = true;
				Item1.IsChanged = true;
				Item1.NewValue = "NewValue1";

				var oldValue = Item1.GetValue().ToString();
				var newValue = Item1.NewValue.ToString();

				testForm.HasChangesForTest = true;
				var changedItems = testForm.GetChangedItems();
				changedItems.Add(Item1);
				testForm.OnSaveResult = DialogResult.Yes;
				saveButton.PerformClick();

				var stmData = testForm.Factory.Load<StmData>(Item1.GetRegistryItemPK());
				var updateLogCount = stmData.Logs.Find((StmALog log) =>
					log.Event.SE_Code == "EDT" && log.SL_Reference.Contains(oldValue) &&
					log.SL_Reference.Contains(newValue)).Count();
				AssertEquals(updateLogCount, expectedLogCount);
			}
		}

		[RequiresSTA]
		public void TestUpdatePluginPanel_FormReadOnly()
		{
			Env.Security.SystemRegistryEdit.IsAllowed = false;
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				registriesTreeView.Nodes.Clear();

				IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint",
				RegistryStorageFlags.System, RegistryOptions.Default, "DEFAULT");
				regItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue");
				RegistryItemTag item = new RegistryItemTag(regItem);
				TreeNode itemNode = new TreeNode("Item3");
				itemNode.Tag = item;
				registriesTreeView.Nodes.Add(itemNode);

				registriesTreeView.SelectedNode = itemNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				Assert("ValueLabel should be visible", testForm.GetValueLabel().Visible);
				Assert("FallbackMessageLabel should be visible", testForm.GetFallbackMessageLabel().Visible);
				AssertEquals("FallbackMessageLabel.Text", "Default value overridden.", testForm.GetFallbackMessageLabel().Text);
				Assert("OverrideCheckBox should not be visible", !testForm.GetOverrideCheckBox().Visible);
				Assert("FallbackMessageLabel should be in the PluginPanel", testForm.GetPluginPanel().Contains(testForm.GetFallbackMessageLabel()));
				Assert("PluginControl should be in the PluginPanel", testForm.GetPluginPanel().Contains(testForm.GetPluginControl()));
				Assert("PluginControl should be disabled", testForm.GetPluginControl().GetReadOnly());
				AssertEquals("Changes Log & Notes tab pages should be added", 3, testForm.GetRegistryItemTabControl().TabPages.Count);
				AssertEquals("Changes Log tab page should be added", typeof(ZStmALogTabPage), testForm.GetRegistryItemTabControl().TabPages[1].GetType());
				AssertEquals("Notes tab page should be added", typeof(ZStmNoteForRegistryItemTabPage), testForm.GetRegistryItemTabControl().TabPages[2].GetType());
				AssertEquals("Item DefaultValue", "DEFAULT", item.DefaultValue);
				AssertEquals("Item Current Value", "NewValue", item.GetValue());
				AssertEquals("PluginControl Text", "", testForm.GetPluginControl().Text);
				Assert("SecurityDeniedLabel should be visible", testForm.GetSecurityDeniedLabel().Visible);
			}
		}

		public void TestUpdatePluginPanel_SecurityDenied()
		{
			Env.Security.SystemRegistryEdit.IsAllowed = false;
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				var registriesTreeView = testForm.GetRegistriesTreeView();
				var fallbackTreeView = testForm.GetFallbackTreeView();
				registriesTreeView.Nodes.Clear();

				var regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint",
				RegistryStorageFlags.System, RegistryOptions.Default, "DEFAULT");
				regItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue");
				var item = new RegistryItemTag(regItem);
				var itemNode = new TreeNode("Item3");
				itemNode.Tag = item;
				registriesTreeView.Nodes.Add(itemNode);

				var checkpoint = Env.Security.GetRegistryCheckPoint(regItem.Name, regItem.Caption);
				checkpoint.IsAllowed = false;

				registriesTreeView.SelectedNode = itemNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				Assert("ValueLabel should be visible", testForm.GetValueLabel().Visible);
				Assert("SecurityDeniedLabel should be visible", testForm.GetSecurityDeniedLabel().Visible);
				AssertEquals("SecurityDeniedLabel.Text", checkpoint.ErrorMessageForNotAllowed, testForm.GetSecurityDeniedLabel().Text);
				Assert("SecurityDeniedLabel.Height should be 91 or larger: " + testForm.GetSecurityDeniedLabel().Height, testForm.GetSecurityDeniedLabel().Height >= 91);
				Assert("OverrideCheckBox should not be visible", !testForm.GetOverrideCheckBox().Visible);
				Assert("SecurityDenied should be in the PluginPanel", testForm.GetPluginPanel().Contains(testForm.GetSecurityDeniedLabel()));
			}
		}

		public void TestCommitValue()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				GuidRegistryItem guidRegItem = new GuidRegistryItem("", (MultilingualString)null, null, null, RegistryStorageFlags.Branch);
				guidRegItem.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbBranch);
				RegistryItemTag guidItem = new RegistryItemTag(guidRegItem);
				TreeNode guidNode = new TreeNode("");
				guidNode.Tag = guidItem;
				registriesTreeView.Nodes.Add(guidNode);

				registriesTreeView.SelectedNode = guidNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode.FirstNode;
				testForm.GetOverrideCheckBox().Checked = true;
				ZGuidFindBox pluginControl = (ZGuidFindBox)testForm.GetPluginControl();
				pluginControl.Focus();
				pluginControl.CurrentCode = Env.CurrentBranch.Code;

				registriesTreeView.Focus();
				AssertEquals("GuidItem.NewValue should have been updated", Env.CurrentBranch.PK, guidItem.NewValue);
			}
		}

		public void TestHandleRegistryItemChanged()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeViewWithDefaultValue(registriesTreeView, false);

				AssertEquals("ChangedItems.Count", 0, testForm.GetChangedItems().Count);
				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				Assert("Registry Item's IsChanged property should be false on initialisation", !Item1.IsChanged);

				testForm.GetOverrideCheckBox().Checked = true;
				Control pluginControl = testForm.GetPluginControl();
				pluginControl.Focus();
				pluginControl.Text = "HELLO";
				registriesTreeView.Focus();
				AssertEquals("Item.NewValue", "HELLO", pluginControl.Text);
				Assert("Registry Item's IsChanged property should be true if value was changed", Item1.IsChanged);
				AssertEquals("ChangedItems.Count", 1, testForm.GetChangedItems().Count);
				AssertEquals("ChangedItems[0]", Item1, testForm.GetChangedItems()[0]);

				testForm.GetOverrideCheckBox().Checked = false;
				AssertEquals("Default value should be shown", "DEFAULT", testForm.GetPluginControl().Controls[0].Text);
			}
		}

		public void TestHandleRegistryItemChangedNewValueIsNull()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				ImageRegistryItem imageRegItem = new ImageRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, null);
				RegistryItemTag imageItem = new RegistryItemTag(imageRegItem);
				TreeNode imageNode = new TreeNode("");
				imageNode.Tag = imageItem;
				registriesTreeView.Nodes.Add(imageNode);
				imageRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(10, 10));

				AssertEquals("ChangedItems.Count", 0, testForm.GetChangedItems().Count);
				registriesTreeView.SelectedNode = imageNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				Assert("Registry Item's IsChanged property should be false on initialisation", !imageItem.IsChanged);

				Assert("OverrideCheckBox should be checked", testForm.GetOverrideCheckBox().Checked);
				testForm.GetOverrideCheckBox().Checked = false;
				testForm.PluginControlEnteredButLeaveEventNotFiredForTest = true;
				testForm.UpdateRegistryItemValueFromPluginControlForTest();
				AssertEquals("Item.NewValue", null, imageItem.NewValue);
				Assert("Registry Item's IsChanged property should be true if value was changed", imageItem.IsChanged);
				AssertEquals("ChangedItems.Count", 1, testForm.GetChangedItems().Count);
				AssertEquals("ChangedItems[0]", imageItem, testForm.GetChangedItems()[0]);
			}
		}

		public void TestHandleReadOnlyRegistryItemChanged()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeViewWithDefaultValue(registriesTreeView, true);
				RegItem1.Options = RegItem1.Options | RegistryOptions.IsReadOnly;
				RegItem1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue");
				Item1 = new RegistryItemTag(RegItem1);

				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				AssertEquals("Item1's DefaultValue", "DEFAULT", Item1.DefaultValue);
				Assert("OverrideCheckBox should not be visible", !testForm.GetOverrideCheckBox().Visible);

				Control pluginControl = testForm.GetPluginControl();
				AssertEquals("PluginControl's Text", "NewValue", pluginControl.Controls[0].Text);
				pluginControl.Focus();
				registriesTreeView.Focus();
				registriesTreeView.SelectedNode = ItemNode2;
				registriesTreeView.SelectedNode = ItemNode1;
				AssertEquals("PluginControls' Text", "NewValue", testForm.GetPluginControl().Controls[0].Text);
				AssertEquals("Item1's current Value", "NewValue", Item1.GetValue());
			}
		}

		public void TestHideInactiveItemsUpdatesFallbackTree()
		{
			GlbDepartmentCollection departments = new GlbDepartmentCollection(Factory);
			int initialDepartmentCount = departments.Count;
			ZQuery filter = new ZQuery(GlbDepartmentSchema.GE_IsActive, false);
			departments.AdditionalFilter = filter;
			int inactiveCount = departments.Count;

			GlbDepartment inactiveDepartment = departments.AddNew();
			inactiveDepartment.FillWithValidTestData();
			inactiveDepartment.GE_IsActive = false;
			departments.Factory.Save();

			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, false);

				registriesTreeView.SelectedNode = ItemNode1;
				AssertEquals("Department Nodes Count", initialDepartmentCount - inactiveCount, fallbackTreeView.Nodes[0].FirstNode.GetNodeCount(false));

				MenuItem hideMenuItem = testForm.GetHideMenuItem();
				hideMenuItem.PerformClick();
				AssertEquals("Department Nodes Count", initialDepartmentCount + 1, fallbackTreeView.Nodes[0].FirstNode.GetNodeCount(false));
			}
		}

		public void TestHideInactiveItemsUpdatesPluginPanel()
		{
			var inactiveDepartment = Factory.New<GlbDepartment>();
			inactiveDepartment.GE_Desc = "000000000000";
			inactiveDepartment.FillWithValidTestData();
			inactiveDepartment.GE_IsActive = false;
			inactiveDepartment.Factory.Save();

			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, false);
				registriesTreeView.SelectedNode = ItemNode1;

				// Set a value in an active node
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode;
				testForm.GetOverrideCheckBox().Checked = true;
				ZTextBox pluginControl = (ZTextBox)testForm.GetPluginControl().Controls[0];
				pluginControl.Focus();
				pluginControl.Text = "Test1!";
				string firstSelectedNode = fallbackTreeView.SelectedNode.Text;

				// Set a value in an inactive node
				testForm.GetHideMenuItem().PerformClick();
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode;
				AssertEquals("Node Text", "000000000000", fallbackTreeView.SelectedNode.Text);
				testForm.GetOverrideCheckBox().Checked = true;
				pluginControl = (ZTextBox)testForm.GetPluginControl().Controls[0];
				pluginControl.Focus();
				pluginControl.Text = "Test2!";

				// Hide the inactive node, check that the PluginPanel was updated
				testForm.GetHideMenuItem().PerformClick();
				Assert("OverrideCheckBox should be disabled", !testForm.GetOverrideCheckBox().Checked);
				Assert("PluginControl should be ReadOnly", testForm.GetPluginControl().GetReadOnly());
				AssertEquals("PluginControl.Text", "", testForm.GetPluginControl().Text);

				// Check the values of the previous inactive node that was modified
				testForm.GetHideMenuItem().PerformClick();
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode;
				Assert("OverrideCheckBox should be enabled", testForm.GetOverrideCheckBox().Checked);
				Assert("PluginControl should not be ReadOnly", !testForm.GetPluginControl().GetReadOnly());
				AssertEquals("PluginControl.Text", "Test2!", testForm.GetPluginControl().Controls[0].Text);

				// Check the values of the previous active node that was modified
				testForm.GetHideMenuItem().PerformClick();
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode;
				AssertEquals("Node Text", firstSelectedNode, fallbackTreeView.SelectedNode.Text);
				Assert("OverrideCheckBox should be enabled", testForm.GetOverrideCheckBox().Checked);
				Assert("PluginControl should not be ReadOnly", !testForm.GetPluginControl().GetReadOnly());
				AssertEquals("PluginControl.Text", "Test1!", testForm.GetPluginControl().Controls[0].Text);

				// Check that the same node is still selected after clicking on HideMenuItem
				testForm.GetHideMenuItem().PerformClick();
				AssertEquals("Node Text", firstSelectedNode, fallbackTreeView.SelectedNode.Text);
				Assert("OverrideCheckBox should be enabled", testForm.GetOverrideCheckBox().Checked);
				Assert("PluginControl should not be ReadOnly", !testForm.GetPluginControl().GetReadOnly());
				AssertEquals("PluginControl.Text", "Test1!", testForm.GetPluginControl().Controls[0].Text);
			}
		}

		[ExpectNoExceptions]
		public void TestHideInactiveItemsDoesNotCrashWhenPluginValueWasChanged()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, true);

				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				testForm.GetOverrideCheckBox().Checked = true;
				var pluginControl = testForm.GetPluginControl();
				pluginControl.Focus();
				pluginControl.Text = "Test!";

				testForm.GetHideMenuItem().PerformClick();

				registriesTreeView.SelectedNode = ItemNode2;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
			}
		}

		public void TestHints()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				ZLabel hintLabel = testForm.GetHintLabel();

				registriesTreeView.Nodes.Clear();
				TreeNode rootNode = new TreeNode("Root");
				registriesTreeView.Nodes.Add(rootNode);

				SetupTestItem1();
				rootNode.Nodes.Add(ItemNode1);
				registriesTreeView.SelectedNode = ItemNode1;
				AssertEquals("HintLabel.Text", "TestHint", hintLabel.Text);
				registriesTreeView.SelectedNode = rootNode;
				AssertEquals("HintLabel.Text", testForm.GetDefaultHint(), hintLabel.Text);
			}
		}

		[RequiresSTA]
		public void TestSupportLabel_DoesNotAppear()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				// Arrange
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Clear();
				var testNode = new TreeNode();
				IRegistryItem regItem = new StringRegistryItem("TestName", null, null, null, RegistryStorageFlags.System, "");
				RegistryItemTag item = new RegistryItemTag(regItem);
				testNode.Tag = item;
				registriesTreeView.Nodes.Add(testNode);
				// Act
				registriesTreeView.SelectedNode = testNode;
				// Assert
				AssertEquals("No options are set so there should be no support label text", false, testForm.GetSupportLabel().Visible);
			}
		}

		public void TestSupportLabel_VisibleOnlyToSupport()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				// Arrange
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Clear();
				var testNode = new TreeNode();
				IRegistryItem regItem = new StringRegistryItem("TestName", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, "");
				RegistryItemTag item = new RegistryItemTag(regItem);
				testNode.Tag = item;
				registriesTreeView.Nodes.Add(testNode);
				// Act
				registriesTreeView.SelectedNode = testNode;
				// Assert
				AssertEquals("Should contain text", true, testForm.GetSupportLabel().Visible);
				AssertEquals("Should contain text", "Visible only to Support", testForm.GetSupportLabel().Text);
			}
		}

		[RequiresSTA]
		public void TestSupportLabel_EditableOnlyByHostedSupport_SupportUser()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				// Arrange
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Clear();
				var testNode = new TreeNode();
				IRegistryItem regItem = new StringRegistryItem("TestName", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, "");
				RegistryItemTag item = new RegistryItemTag(regItem);
				testNode.Tag = item;
				registriesTreeView.Nodes.Add(testNode);
				// Support User, Hosted Client, and registry option fix  
				using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					EnvProxy.SetHostedLocationForTest("SYD");
					// Act
					registriesTreeView.SelectedNode = testNode;
					// Assert
					AssertEquals("Should contain text", true, testForm.GetSupportLabel().Visible);
					AssertEquals("Should contain text", "This registry is only editable by Support on Hosted Systems", testForm.GetSupportLabel().Text);
				}
			}
		}

		public void TestSupportLabel_EditableOnlyByHostedSupport_NonDevUser()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				// Arrange
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Clear();
				var testNode = new TreeNode();
				IRegistryItem regItem = new StringRegistryItem("TestName", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, "");
				RegistryItemTag item = new RegistryItemTag(regItem);
				testNode.Tag = item;
				registriesTreeView.Nodes.Add(testNode);
				// Non Dev User, Hosted Client, and Registry Option
				using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					EnvProxy.SetHostedLocationForTest("SYD");
					// Act
					registriesTreeView.SelectedNode = testNode;
					// Assert
					AssertEquals("Should contain text", true, testForm.GetSupportLabel().Visible);
					AssertEquals("Should contain text", "This registry is only editable by Support on Hosted Systems", testForm.GetSupportLabel().Text);
				}
			}
		}

		public void TestSupportLabel_ApplicableOnlyTo_eAdaptorNext()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				// Arrange
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Clear();
				var testNode = new TreeNode();
				IRegistryItem regItem = new StringRegistryItem("TestName", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyFor_eAdaptorNext, "");
				RegistryItemTag item = new RegistryItemTag(regItem);
				testNode.Tag = item;
				registriesTreeView.Nodes.Add(testNode);
				// Act
				registriesTreeView.SelectedNode = testNode;
				// Assert
				AssertEquals("Should contain text", true, testForm.GetSupportLabel().Visible);
				AssertEquals("Should contain text", "Applicable only when using eAdaptor Next.", testForm.GetSupportLabel().Text);
			}
		}

		public void TestSupportLabel_ApplicableOnlyTo_eAdaptorNext_WithOtherFlag()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				// Arrange
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Clear();
				var testNode = new TreeNode();
				IRegistryItem regItem = new StringRegistryItem("TestName", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyFor_eAdaptorNext, "");
				RegistryItemTag item = new RegistryItemTag(regItem);
				testNode.Tag = item;
				registriesTreeView.Nodes.Add(testNode);
				// Act
				registriesTreeView.SelectedNode = testNode;
				// Assert
				AssertEquals("Should contain text", true, testForm.GetSupportLabel().Visible);
				AssertEquals("Should contain text", "Applicable only when using eAdaptor Next. Visible only to Support", testForm.GetSupportLabel().Text);
			}
		}

		public void TestOverrideCheckBox()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				ZCheckBox overrideCheckBox = testForm.GetOverrideCheckBox();

				SetupRegistriesTreeView(registriesTreeView, false);
				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				AssertEquals("ChangedItems.Count", 0, testForm.GetChangedItems().Count);
				Assert("OverrideCheckBox should not be checked if the Registry Item has no value", !overrideCheckBox.Checked);
				Assert("The Registry Item's IsChanged property should be false on initialisation", !Item1.IsChanged);
				Assert("PluginControl should be disabled if OverrideCheckBox is not checked", testForm.GetPluginControl().GetReadOnly());
				Assert("The Registry Item wasn't supposed to have a value", !Item1.HasValue);

				overrideCheckBox.Checked = true;
				Assert("The Registry Item's IsChanged property should be true if OverrideCheckBox was changed", Item1.IsChanged);
				Assert("The Registry Item's HasValue property should be true if OverrideCheckBox is checked", Item1.HasValue);
				Assert("PluginControl should be enabled if OverrideCheckBox is checked", testForm.GetPluginControl().Enabled);

				Item1.NewValue = "TEST";
				overrideCheckBox.Checked = false;
				AssertEquals("Item.NewValue", "", Item1.NewValue);
				AssertEquals("ChangedItems.Count", 1, testForm.GetChangedItems().Count);
				AssertEquals("ChangedItems[0]", Item1, testForm.GetChangedItems()[0]);
			}
		}

		[RequiresSTA]
		public void TestOverrideCheckBox_WhenChecked_ShouldRetainInheritatedValue()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeViewWithDefaultValue(registriesTreeView, false);
				registriesTreeView.SelectedNode = ItemNode1;

				// set system override
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				testForm.GetOverrideCheckBox().Checked = true;
				var pluginControl = testForm.GetPluginControl();
				pluginControl.Focus();
				pluginControl.Controls[0].Text = "HELLO";

				// check company default
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[1].Nodes[0];
				AssertEquals(false, testForm.GetOverrideCheckBox().Checked);
				pluginControl = testForm.GetPluginControl();
				AssertEquals("Default value should obtain from system", "HELLO", pluginControl.Controls[0].Text);

				// check company override
				testForm.GetOverrideCheckBox().Checked = true;
				pluginControl = testForm.GetPluginControl();
				AssertEquals("Override value should retain default initially", "HELLO", pluginControl.Controls[0].Text);
			}
		}

		public void TestOverrideCheckBox_GenerateWaringMessage()
		{
			using (RegistryFormForTest form = new RegistryFormForTest())
			{
				form.Show();
				TreeView registriesTreeView = form.GetRegistriesTreeView();
				TreeView fallbackTreeView = form.GetFallbackTreeView();
				ZCheckBox overrideCheckBox = form.GetOverrideCheckBox();
				ZPanel pluginPanel = form.GetPluginPanel();
				CodeDescriptionPairListProvider fontsListProvider = new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList());

				CodePairRegistryItem fontRegItem;
				RegistryItemTag fontRegItemTag;
				TreeNode fontNode;

				//Setup SystemFontRegItem
				SetupTestRegItem("SystemFont", "Font Used for the System");
				SelectNode();

				//Pre-condition
				AssertInitialStatus("Font Used for the System");

				overrideCheckBox.Checked = true;
				AssertWarningMessages();
				overrideCheckBox.Checked = false;
				AssertNoWarningMessages();

				//Setup NavigationMenuFontRegItem
				SetupTestRegItem("NavigationMenuFont", "Font Used for the Navigation Menu");
				SelectNode();

				//Pre-condition
				AssertInitialStatus("Font Used for the Navigation Menu");

				overrideCheckBox.Checked = true;
				AssertWarningMessages();
				overrideCheckBox.Checked = false;
				AssertNoWarningMessages();

				//Setup the NewsAnnouncementFontRegItem
				SetupTestRegItem("NewsAnnouncementFont", "Font Used for the News & Announcements");
				SelectNode();

				//Pre-condition
				AssertInitialStatus("Font Used for the News & Announcements");

				overrideCheckBox.Checked = true;
				AssertWarningMessages();
				overrideCheckBox.Checked = false;
				AssertNoWarningMessages();

				void SetupTestRegItem(string regItemName, string resgItemCaption)
				{
					registriesTreeView.Nodes.Clear();
					fontRegItem = new CodePairRegistryItem((NoResString)regItemName, null, (NoResString)resgItemCaption, null, fontsListProvider, RegistryStorageFlags.System, OFont.DefaultFontName);
					fontRegItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
					fontRegItemTag = new RegistryItemTag(fontRegItem);
					fontNode = new TreeNode(resgItemCaption);
					fontNode.Tag = fontRegItemTag;
					registriesTreeView.Nodes.Add(fontNode);
				}

				void SelectNode()
				{
					registriesTreeView.SelectedNode = fontNode;
					fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				}

				void AssertInitialStatus(string expectedSelectedNodeName)
				{
					AssertEquals(registriesTreeView.SelectedNode.Text, expectedSelectedNodeName);
					Assert("OverrideCheckBox should not be checked if the Registry Item has no value", !overrideCheckBox.Checked);
					Assert("The Registry Item's IsChanged property should be false on initialization", !fontRegItemTag.IsChanged);
					Assert("PluginControl should be disabled if OverrideCheckBox is not checked", form.GetPluginControl().GetReadOnly());
					Assert("The Registry Item wasn't supposed to have a value", !fontRegItemTag.HasValue);
				}

				void AssertNoWarningMessages()
				{
					//No warning message
					Assert("The Registry Item's IsChanged property should be true if OverrideCheckBox was changed", fontRegItemTag.IsChanged);
					Assert("The Registry Item's HasValue property should be false if OverrideCheckBox is not checked", !fontRegItemTag.HasValue);
					Assert("PluginControl should be enabled if OverrideCheckBox is unchecked", !form.GetPluginControl().Enabled);
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
				}

				void AssertWarningMessages()
				{
					Assert("The Registry Item's IsChanged property should be true if OverrideCheckBox was changed", fontRegItemTag.IsChanged);
					Assert("The Registry Item's HasValue property should be true if OverrideCheckBox is checked", fontRegItemTag.HasValue);
					Assert("PluginControl should be enabled if OverrideCheckBox is checked", form.GetPluginControl().Enabled);

					//Assert warning message
					var expectedMessage = string.Format("Warning: Changing {0} might cause text to be displayed incorrectly.\r\nThe font will not go into effect until {1} is closed and relaunched.", fontRegItem.Caption, BrandingFactory.Instance.ProductName);
					AssertEquals("Warning", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		public void TestOverrideCheckBoxEnabledState()
		{
			using (RegistryFormForTest form = new RegistryFormForTest())
			{
				form.Show();
				TreeView registriesTreeView = form.GetRegistriesTreeView();
				TreeView fallbackTreeView = form.GetFallbackTreeView();
				ZCheckBox overrideCheckBox = form.GetOverrideCheckBox();

				SetupRegistriesTreeView(registriesTreeView, true);
				RegItem2.Options = RegistryOptions.MustOverrideDefaultValue;

				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				AssertEquals("Enabled", true, overrideCheckBox.Enabled);
				AssertEquals("Checked", false, overrideCheckBox.Checked);

				registriesTreeView.SelectedNode = ItemNode2;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				AssertEquals("Enabled", false, overrideCheckBox.Enabled);
				AssertEquals("Checked", true, overrideCheckBox.Checked);
			}
		}

		[RequiresSTA]
		public void TestUpdateChangedItems()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				ZCheckBox overrideCheckBox = testForm.GetOverrideCheckBox();

				SetupRegistriesTreeViewWithDefaultValue(registriesTreeView, true);

				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				AssertEquals("ChangedItems.Count", 0, testForm.GetChangedItems().Count);
				testForm.UpdateChangedItemsForTest(true, "TEST");
				AssertEquals("Item.NewValue", "TEST", Item1.NewValue);
				Assert("Item.HasValue should be true", Item1.HasValue);
				Assert("Item.IsChanged should be true", Item1.IsChanged);
				AssertEquals("ChangedItems.Count", 1, testForm.GetChangedItems().Count);
				AssertEquals("ChangedItems[0]", Item1, testForm.GetChangedItems()[0]);

				testForm.UpdateChangedItemsForTest(false, "TEST2");
				AssertEquals("Item.NewValue should be default if HasValue is false", "DEFAULT", Item1.NewValue);
				Assert("Item.HasValue should be false", !Item1.HasValue);
				Assert("Item.IsChanged should be true", Item1.IsChanged);
				AssertEquals("ChangedItems.Count", 1, testForm.GetChangedItems().Count);
				AssertEquals("ChangedItems[0]", Item1, testForm.GetChangedItems()[0]);

				registriesTreeView.SelectedNode = ItemNode2;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				testForm.UpdateChangedItemsForTest(true, "TEST3");
				AssertEquals(2, registriesTreeView.GetNodeCount(false));
				AssertEquals("ChangedItems.Count", 2, testForm.GetChangedItems().Count);
				AssertEquals("ChangedItems[1]", Item2, testForm.GetChangedItems()[1]);
				Assert("SaveButton should be enabled if changes were made", testForm.GetSaveButton().Enabled);
				Assert("HasChanges should be true", testForm.HasChangesForTest);
			}
		}

		public void TestHasChanges()
		{
			using (var testForm = new RegistryFormForTest())
			{
				var registriesTreeView = testForm.GetRegistriesTreeView();
				testForm.Show();
				var testNode = new TreeNode();
				var regItem = new CodeDescriptionBoolRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, new CodeDescriptionBoolRegistryEditorInfo((NoResString)"TestCaption", true));
				var itemTag = new RegistryItemTag(regItem);
				testNode.Tag = itemTag;
				registriesTreeView.Nodes.Add(testNode);
				registriesTreeView.SelectedNode = testNode;
				AssertEquals("Has changes should be false", false, testForm.HasChanges);

				testForm.GetOverrideCheckBox().Checked = true;
				testForm.OnSaveResult = DialogResult.Yes;
				var saveButton = testForm.GetSaveButton();
				var pluginControl = (ZUserControl)testForm.GetPluginControl();
				var dataSource = (CodeDescriptionBoolCollection)pluginControl.BindingSource.DataSource;
				var row1 = dataSource.AddNew();
				row1.Code = "ABC";
				row1.Bool = true;
				AssertEquals("Has changes should be true", true, testForm.HasChanges);

				saveButton.PerformClick();
				AssertEquals("Has changes is now false", false, testForm.HasChanges);

				var row2 = dataSource.AddNew();
				row2.Code = "DFA";
				testForm.OnClosingResult = DialogResult.No;
				AssertEquals("Has changes should be true", true, testForm.HasChanges);

				var row3 = dataSource.AddNew();
				pluginControl.Focus();
				AssertEquals("Has changes should remain true", true, testForm.HasChanges);
			}
		}

		public void TestSuspendAndResumeUpdateOverrideCheckbox()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				ZCheckBox overrideCheckBox = testForm.GetOverrideCheckBox();

				SetupRegistriesTreeView(registriesTreeView, false);
				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				testForm.SuspendUpdateOverrideCheckBoxForTest();
				overrideCheckBox.Checked = true;
				Assert("PluginControl should not have changed if SuspendUpdateOverrideCheckBox was called", testForm.GetPluginControl().GetReadOnly());
				AssertEquals("ChangedItems.Count", 0, testForm.GetChangedItems().Count);

				overrideCheckBox.Checked = false;
				testForm.ResumeUpdateOverrideCheckBoxForTest();
				overrideCheckBox.Checked = true;
				Assert("PluginInControl should have changed if ResumeUpdateOverrideCheckBox was called", testForm.GetPluginControl().Enabled);
				AssertEquals("ChangedItems.Count", 1, testForm.GetChangedItems().Count);
			}
		}

		public void TestSuspendAndResumeSetSelectedFallback()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				registriesTreeView.Nodes.Clear();
				IRegistryItem regItem = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
				regItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				RegistryItemTag item = new RegistryItemTag(regItem);
				TreeNode itemNode = new TreeNode("Item");
				itemNode.Tag = item;
				registriesTreeView.Nodes.Add(itemNode);

				testForm.SuspendSetSelectedFallbackForTest();
				registriesTreeView.SelectedNode = itemNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				AssertEquals("SelectedFallbacksHash.Count", 0, testForm.GetSelectedFallbacksHash().Count);
				Assert("PluginControl should be null - UpdatePluginPanel should not have been called", testForm.GetPluginControl() == null);
				Assert("OverrideCheckBox should not be visible - UpdatePluginPanel should not have been called", !testForm.GetOverrideCheckBox().Visible);

				fallbackTreeView.SelectedNode = null;
				testForm.ResumeSetSelectedFallbackForTest();
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				AssertEquals("SelectedFallbacksHash.Count", 2, testForm.GetSelectedFallbacksHash().Count);
				Assert("PluginControl should not be null - UpdatePluginPanel should have been called", testForm.GetPluginControl() != null);
				Assert("OverrideCheckBox should be visible - UpdatePluginPanel should have been called", testForm.GetOverrideCheckBox().Visible);
			}
		}

		public void TestCanSaveWhenNotShowDescriptionColumnAndLackOfDescriptionValues()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				var regItem = OrganisationsDataRegistry.Instance.ContactSourceTypes;
				regItem.EditorInfo = new CodeDescriptionPairListEditorInfo(true, false);
				Item1 = new RegistryItemTag(regItem);
				ItemNode1 = new TreeNode("Item1");
				ItemNode1.Tag = Item1;

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				registriesTreeView.Nodes.Clear();
				registriesTreeView.Nodes.Add(ItemNode1);

				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				testForm.GetOverrideCheckBox().Focus();
				AssertEquals("False first so that OnClick makes it true", false, testForm.GetOverrideCheckBox().Checked);
				testForm.GetOverrideCheckBox().OnClick(EventArgs.Empty);
				AssertEquals("False first so that OnClick makes it true", true, testForm.GetOverrideCheckBox().Checked);
				testForm.GetSaveButton().PerformClick();
				AssertNotEquals("UnitTestUserNotification.Instance.LastMessage", "The Registry cannot be saved because errors were found in the following items:\r\n\r\n• Organizations/Code Lists/Contact Source List", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("UnitTestUserNotification.Instance.LastMessage", "All changes will be saved. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestErrorMessageWhenSavingWithRegistryItemsInError()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				testForm.DoNotValidateRegistryItem = true;
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, true);

				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				testForm.GetOverrideCheckBox().Checked = true;
				Item1.CurrentFallbackIsInError = true;

				registriesTreeView.SelectedNode = ItemNode2;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				testForm.GetOverrideCheckBox().Checked = true;
				Item2.CurrentFallbackIsInError = true;

				AssertEquals("Precondition: UnitTestUserNotification.Instance.LastMessage should be null", null, UnitTestUserNotification.Instance.LastMessage.Text);
				testForm.GetSaveButton().PerformClick();
				AssertEquals("UnitTestUserNotification.Instance.LastMessage", "The Registry cannot be saved because errors were found in the following items:\r\n\r\n• Category/TestCaption\r\n• Category2/TestCaption2", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestErrorMessageWhenSavingWithNumberRegistryItemsInError()
		{
			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();
				var registriesTreeView = testForm.GetRegistriesTreeView();
				var fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, false);
				var regNum = new IntRegistryItem("TestItemNum", (NoResString)"Category", (NoResString)"TestCaptionNum", (NoResString)"TestHint", RegistryStorageFlags.All, RegistryOptions.Default, 20, 10, 100);
				var itemNum = new RegistryItemTag(regNum);
				var itemNodeNum = new TreeNode("Item");
				itemNodeNum.Tag = itemNum;
				registriesTreeView.Nodes.Add(itemNodeNum);
				registriesTreeView.SelectedNode = itemNodeNum;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				testForm.GetOverrideCheckBox().Checked = true;
				itemNum.NewValue = 5000;
				AssertEquals("Precondition: UnitTestUserNotification.Instance.LastMessage should be null", null, UnitTestUserNotification.Instance.LastMessage.Text);
				testForm.GetSaveButton().PerformClick();

				var errorMessage = "Value must be less than or equal to the maximum (100)";
				AssertEquals(errorMessage, itemNum.LastValidationErrorMessage);
				AssertEndsWith("UnitTestUserNotification.Instance.LastMessage", errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveButtonClickMessage_AndSaveButtonRemainsInactive()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				ZButton saveButton = testForm.GetSaveButton();

				AssertEquals("Precondition: UnitTestUserNotification.Instance.LastMessage should be null", null, UnitTestUserNotification.Instance.LastMessage.Text);
				saveButton.PerformClick();
				//AssertEquals("UnitTestUserNotification.Instance.LastMessage", "No changes have been made to the Registry.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Save button should not be enabled", true, !saveButton.Enabled);

				testForm.HasChangesForTest = true;
				saveButton.PerformClick();
				AssertEquals("UnitTestUserNotification.Instance.LastMessage", "All changes will be saved. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveButtonClickWithChanges()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				ZButton saveButton = testForm.GetSaveButton();

				SetupTestItem1();
				Item1.HasValue = true;
				Item1.IsChanged = true;
				Item1.NewValue = "NewValue1";

				SetupTestItem2();
				Item2.HasValue = true;
				Item2.IsChanged = true;
				Item2.NewValue = "NewValue2";

				testForm.HasChangesForTest = true;
				var changedItems = testForm.GetChangedItems();
				changedItems.Add(Item1);
				changedItems.Add(Item2);

				Env.Registry.RawRegistry.RegistryUserUpdateVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, int.MaxValue);
				int currentRegistryVersion = Env.Registry.RawRegistry.RegistryUserUpdateVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertEquals("CurrentRegistryVersion should be int.MaxValue", int.MaxValue, currentRegistryVersion);

				testForm.OnSaveResult = DialogResult.No;
				saveButton.PerformClick();

				AssertEquals("RegistryUserUpdateVersion should be the same", currentRegistryVersion, Env.Registry.RawRegistry.RegistryUserUpdateVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				AssertEquals("Item1.GetValue()", "NewValue1", Item1.GetValue());
				AssertEquals("Item1.NewValue", "NewValue1", Item1.NewValue);
				Assert("Item1's IsChanged property should be true", Item1.IsChanged);
				AssertEquals("Item2.GetValue()", "NewValue2", Item2.GetValue());
				AssertEquals("Item2.NewValue", "NewValue2", Item2.NewValue);
				Assert("Item2's IsChanged property should be true", Item2.IsChanged);
				Assert("HasChanges for the form should be true", testForm.HasChangesForTest);
				Assert("There should still be changes on the form", testForm.GetChangedItems().Count == 2);

				testForm.OnSaveResult = DialogResult.Yes;
				saveButton.PerformClick();

				AssertEquals("RegistryUserUpdateVersion should be reset to 0", 0, Env.Registry.RawRegistry.RegistryUserUpdateVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				AssertEquals("Item.GetValue()", "NewValue1", Item1.GetValue());
				Assert("Item.NewValue should be null", Item1.NewValue == null);
				Assert("Item1's IsChanged property should be false", !Item1.IsChanged);
				AssertEquals("Item2.GetValue()", "NewValue2", Item2.GetValue());
				Assert("Item2.NewValue should be null", Item2.NewValue == null);
				Assert("Item2's IsChanged property should be false", !Item2.IsChanged);
				Assert("HasChanges for the form should be false", !testForm.HasChangesForTest);
				Assert("There should be no changes on the form", testForm.GetChangedItems().Count == 0);
			}
		}

		public void TestSaveButtonClickWithRegistryValidationException()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				ZButton saveButton = testForm.GetSaveButton();

				SetupTestItem1();
				Item1.HasValue = true;
				Item1.IsChanged = true;
				Item1.NewValue = "NewValue1";

				testForm.HasChangesForTest = true;
				var changedItems = testForm.GetChangedItems();
				changedItems.Add(Item1);

				testForm.OnSaveResult = DialogResult.Yes;
				testForm.isThrowRegistryValidationException = true;
				saveButton.PerformClick();

				AssertEquals("error message show", "RegistryValidationException throws.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestSaveButtonClickShouldCallPreSaveValidationForAllChangedItems()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				ZButton saveButton = testForm.GetSaveButton();

				SetupTestItem1();
				var mockRegDataType1 = new Mock<StringRegistryDataType>() { CallBase = true };
				mockRegDataType1.Protected().Setup("ValidateBeforeRegistryFormSaveCore",
					ItExpr.IsAny<IRegistryItem>(), ItExpr.IsAny<string>(), ItExpr.IsAny<Guid>(),
					ItExpr.IsAny<Guid>(), ItExpr.IsAny<Guid>());
				RegItem1.DataType = mockRegDataType1.Object;

				Item1.HasValue = true;
				Item1.IsChanged = true;
				Item1.NewValue = "NewValue1";

				SetupTestItem2();
				var mockRegDataType2 = new Mock<StringRegistryDataType>() { CallBase = true };
				mockRegDataType2.Protected().Setup("ValidateBeforeRegistryFormSaveCore",
					ItExpr.IsAny<IRegistryItem>(), ItExpr.IsAny<string>(), ItExpr.IsAny<Guid>(),
					ItExpr.IsAny<Guid>(), ItExpr.IsAny<Guid>());
				RegItem2.DataType = mockRegDataType2.Object;
				Item2.HasValue = true;
				Item2.IsChanged = true;
				Item2.NewValue = "NewValue2";

				registriesTreeView.Nodes.Add(ItemNode1);
				registriesTreeView.Nodes.Add(ItemNode2);

				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				testForm.HasChangesForTest = true;
				var changedItems = testForm.GetChangedItems();
				changedItems.Add(Item1);
				changedItems.Add(Item2);

				saveButton.PerformClick();

				mockRegDataType1.VerifyAll();
				mockRegDataType2.VerifyAll();
			}
		}

		public void TestCodePairWithAdditionalEventRegistryItemValidation_IsConnectionAndPortChecked()
		{
			var portRegItem = new RawDataRegistry.PhysicalServerRegistryItem("TestPort", (NoResString)"Test Category", (NoResString)"Test Server Port", (NoResString)"Test Server Port", RegistryDataTypes.IntType, RegistryOptions.Default, 110);
			var secureConnectionTypesListProvider = new CodeDescriptionPairListProvider(() => new SecureConnectionTypes());
			var additionalEventRegistryItem = new CodePairWithAdditionalEventRegistryItem("TestConnection", (NoResString)"Test Category", (NoResString)"Test Connection", (NoResString)"Test Hint", secureConnectionTypesListProvider, false, true, new ComboBoxRegistryEditorInfo(secureConnectionTypesListProvider),
				(registryItem) =>
				{
					return Utilities.IsConnectionAndPortChecked(registryItem, portRegItem);
				}, RegistryStorageFlags.System, RegistryOptions.Default, SecureConnectionTypes.None, false);

			AssertCodePairWithAdditionalEventRegistryItemValidation(portRegItem, additionalEventRegistryItem, 100, SecureConnectionTypes.SSL);
		}

		public void TestCodePairWithAdditionalEventRegistryItemValidation_IsProtocolAndPortChecked_IMAP()
		{
			var portRegItem = new RawDataRegistry.PhysicalServerRegistryItem("TestPort", (NoResString)"Test Category", (NoResString)"Test Server Port", (NoResString)"Test Server Port", RegistryDataTypes.IntType, RegistryOptions.Default, 110);
			var mailRetrievalProtocolsListProvider = new CodeDescriptionPairListProvider(() => new ZArchitecture.Core.Lists.MailRetrievalProtocols());

			var additionalEventRegistryItem = new CodePairWithAdditionalEventRegistryItem("TestProtocol", (NoResString)"Test Category", (NoResString)"Test Protocol", (NoResString)"Test Hint", mailRetrievalProtocolsListProvider, false, true, new ComboBoxRegistryEditorInfo(mailRetrievalProtocolsListProvider),
				(registryItem) =>
				{
					return Utilities.IsProtocolAndPortChecked(registryItem, portRegItem);
				}, RegistryStorageFlags.System, RegistryOptions.Default, ZArchitecture.Core.Lists.MailRetrievalProtocols.POP3, false);

			AssertCodePairWithAdditionalEventRegistryItemValidation(portRegItem, additionalEventRegistryItem, 143, ZArchitecture.Core.Lists.MailRetrievalProtocols.IMAP);
			AssertCodePairWithAdditionalEventRegistryItemValidation(portRegItem, additionalEventRegistryItem, 993, ZArchitecture.Core.Lists.MailRetrievalProtocols.IMAP);
		}

		[RequiresSTA]
		public void TestCodePairWithAdditionalEventRegistryItemValidation_IsProtocolAndPortChecked_POP3()
		{
			var portRegItem = new RawDataRegistry.PhysicalServerRegistryItem("TestPort", (NoResString)"Test Category", (NoResString)"Test Server Port", (NoResString)"Test Server Port", RegistryDataTypes.IntType, RegistryOptions.Default, 143);
			var mailRetrievalProtocolsListProvider = new CodeDescriptionPairListProvider(() => new ZArchitecture.Core.Lists.MailRetrievalProtocols());

			var additionalEventRegistryItem = new CodePairWithAdditionalEventRegistryItem("TestProtocol", (NoResString)"Test Category", (NoResString)"Test Protocol", (NoResString)"Test Hint", mailRetrievalProtocolsListProvider, false, true, new ComboBoxRegistryEditorInfo(mailRetrievalProtocolsListProvider),
				(registryItem) =>
				{
					return Utilities.IsProtocolAndPortChecked(registryItem, portRegItem);
				}, RegistryStorageFlags.System, RegistryOptions.Default, ZArchitecture.Core.Lists.MailRetrievalProtocols.IMAP, false);

			AssertCodePairWithAdditionalEventRegistryItemValidation(portRegItem, additionalEventRegistryItem, 110, ZArchitecture.Core.Lists.MailRetrievalProtocols.POP3);
			AssertCodePairWithAdditionalEventRegistryItemValidation(portRegItem, additionalEventRegistryItem, 995, ZArchitecture.Core.Lists.MailRetrievalProtocols.POP3);
		}

		void AssertCodePairWithAdditionalEventRegistryItemValidation(RawDataRegistry.PhysicalServerRegistryItem portRegItem, CodePairWithAdditionalEventRegistryItem additionalEventRegistryItem, int portRegItemNewValue, object additionalEventRegistryItemNewValue)
		{
			using (var testForm = new RegistryFormForTest())
			{
				var registriesTreeView = testForm.GetRegistriesTreeView();
				testForm.Show();

				var portNode = new TreeNode();
				var portItemTag = new RegistryItemTag(portRegItem);
				portNode.Tag = portItemTag;
				registriesTreeView.Nodes.Add(portNode);

				var additionalEventNode = new TreeNode();
				var additionalEventRegistryItemTag = new RegistryItemTag(additionalEventRegistryItem);
				additionalEventNode.Tag = additionalEventRegistryItemTag;
				registriesTreeView.Nodes.Add(additionalEventNode);

				registriesTreeView.SelectedNode = portNode;
				testForm.UpdateChangedItemsForTest(true, portRegItemNewValue);

				registriesTreeView.SelectedNode = additionalEventNode;
				testForm.UpdateChangedItemsForTest(true, additionalEventRegistryItemNewValue);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				additionalEventRegistryItem.DataType.Validate(additionalEventRegistryItem, additionalEventRegistryItemNewValue, Guid.Empty, Guid.Empty, Guid.Empty);

				AssertNull("There should be no warning message as we have changed the default value", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Test Closing the Form

		enum CloseAction
		{
			CloseFormDirectly,
			CloseButtonClick,
			CloseMenuItemClick
		}

		public void TestOnClosing()
		{
			TestClosingMessage(CloseAction.CloseFormDirectly);
			TestClosingTheFormWithYesAndNoErrors(CloseAction.CloseFormDirectly);
			TestClosingTheFormWithYesAndHasError(CloseAction.CloseFormDirectly);
			TestClosingTheFormWithYesShouldCallPreSaveValidation(CloseAction.CloseFormDirectly);
			TestClosingTheFormWithNo(CloseAction.CloseFormDirectly);
			TestClosingTheFormWithCancel(CloseAction.CloseFormDirectly);
		}

		public void TestCloseButton_Click()
		{
			TestClosingMessage(CloseAction.CloseButtonClick);
			TestClosingTheFormWithYesAndNoErrors(CloseAction.CloseButtonClick);
			TestClosingTheFormWithYesAndHasError(CloseAction.CloseButtonClick);
			TestClosingTheFormWithYesShouldCallPreSaveValidation(CloseAction.CloseButtonClick);
			TestClosingTheFormWithNo(CloseAction.CloseButtonClick);
			TestClosingTheFormWithCancel(CloseAction.CloseButtonClick);
		}

		public void TestCloseMenuItem_Click()
		{
			TestClosingMessage(CloseAction.CloseMenuItemClick);
			TestClosingTheFormWithYesAndNoErrors(CloseAction.CloseMenuItemClick);
			TestClosingTheFormWithYesAndHasError(CloseAction.CloseMenuItemClick);
			TestClosingTheFormWithYesShouldCallPreSaveValidation(CloseAction.CloseMenuItemClick);
			TestClosingTheFormWithNo(CloseAction.CloseMenuItemClick);
			TestClosingTheFormWithCancel(CloseAction.CloseMenuItemClick);
		}

		void TestClosingMessage(CloseAction action)
		{
			using (RegistryFormForTest testForm1 = new RegistryFormForTest())
			{
				testForm1.Show();

				switch (action)
				{
					case CloseAction.CloseFormDirectly:
						testForm1.Close();
						break;
					case CloseAction.CloseButtonClick:
						testForm1.GetCloseButton().PerformClick();
						break;
					case CloseAction.CloseMenuItemClick:
						testForm1.GetCloseMenuItem().PerformClick();
						break;
				}

				AssertEquals("UnitTestUserNotification.Instance.LastMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				SetupTestItem1();
				AssertEquals("Item1.GetValue()", "", (string)Item1.GetValue());
				Assert("Item1.NewValue", Item1.NewValue == null);

				registriesTreeView.Nodes.Add(ItemNode1);
				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				ZCheckBox overrideCheckBox = testForm.GetOverrideCheckBox();
				overrideCheckBox.Checked = true;
				Item1.NewValue = "NewValue";

				switch (action)
				{
					case CloseAction.CloseFormDirectly:
						testForm.Close();
						break;
					case CloseAction.CloseButtonClick:
						testForm.GetCloseButton().PerformClick();
						break;
					case CloseAction.CloseMenuItemClick:
						testForm.GetCloseMenuItem().PerformClick();
						break;
				}

				AssertEquals("UnitTestUserNotification.Instance.LastMessage", "The Registry has been modified.\r\nWould you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void TestClosingTheFormWithYesAndNoErrors(CloseAction action)
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				SetupTestItem1();
				AssertEquals("Item1.GetValue()", "", (string)Item1.GetValue());
				Assert("Item1.NewValue", Item1.NewValue == null);

				registriesTreeView.Nodes.Add(ItemNode1);
				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				ZCheckBox overrideCheckBox = testForm.GetOverrideCheckBox();
				overrideCheckBox.Checked = true;
				Item1.NewValue = "NewValue";

				testForm.OnClosingResult = DialogResult.Yes;

				switch (action)
				{
					case CloseAction.CloseFormDirectly:
						testForm.Close();
						break;
					case CloseAction.CloseButtonClick:
						testForm.GetCloseButton().PerformClick();
						break;
					case CloseAction.CloseMenuItemClick:
						testForm.GetCloseMenuItem().PerformClick();
						break;
				}

				Assert("Form should be closed if user choose Yes", !testForm.Visible);
				AssertEquals("Item1.GetValue()", "NewValue", (string)Item1.GetValue());
				Assert("Item1.NewValue", Item1.NewValue == null);
				Assert("Item1's IsChanged property should be false", !Item1.IsChanged);
			}
		}

		void TestClosingTheFormWithYesAndHasError(CloseAction action)
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				testForm.DoNotValidateRegistryItem = true;
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				SetupTestItem2();
				AssertEquals("Item2.GetValue()", "", (string)Item2.GetValue());
				Assert("Item2NewValue", Item2.NewValue == null);

				registriesTreeView.Nodes.Add(ItemNode2);
				registriesTreeView.SelectedNode = ItemNode2;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				ZCheckBox overrideCheckBox = testForm.GetOverrideCheckBox();
				overrideCheckBox.Checked = true;
				Item2.NewValue = "NewValue";
				Item2.CurrentFallbackIsInError = true;

				testForm.OnClosingResult = DialogResult.Yes;

				switch (action)
				{
					case CloseAction.CloseFormDirectly:
						testForm.Close();
						break;
					case CloseAction.CloseButtonClick:
						testForm.GetCloseButton().PerformClick();
						break;
					case CloseAction.CloseMenuItemClick:
						testForm.GetCloseMenuItem().PerformClick();
						break;
				}

				Assert("Form should not be closed if user choose Yes but some Registry Items have errors", testForm.Visible);
				AssertEquals("Item2.GetValue()", "NewValue", (string)Item2.GetValue());
				AssertEquals("Item2.NewValue", "NewValue", (string)Item2.NewValue);
				Assert("Item2's IsChanged property should be true", Item2.IsChanged);
				Assert("There should still be changes on the form", testForm.GetChangedItems().Count == 1);
			}
		}

		void TestClosingTheFormWithYesShouldCallPreSaveValidation(CloseAction action)
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				var mockRegDataType1 = new Mock<StringRegistryDataType>() { CallBase = true };
				mockRegDataType1.Protected().Setup("ValidateBeforeRegistryFormSaveCore",
					ItExpr.IsAny<IRegistryItem>(), ItExpr.IsAny<string>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<Guid>());
				SetupTestItem1();
				RegItem1.DataType = mockRegDataType1.Object;

				registriesTreeView.Nodes.Add(ItemNode1);
				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				ZCheckBox overrideCheckBox = testForm.GetOverrideCheckBox();
				testForm.UpdateChangedItemsForTest(true, "NewValue");

				testForm.OnClosingResult = DialogResult.Yes;
				switch (action)
				{
					case CloseAction.CloseFormDirectly:
						testForm.Close();
						break;
					case CloseAction.CloseButtonClick:
						testForm.GetCloseButton().PerformClick();
						break;
					case CloseAction.CloseMenuItemClick:
						testForm.GetCloseMenuItem().PerformClick();
						break;
				}

				AssertNoExceptionThrown(mockRegDataType1.VerifyAll);
			}
		}

		void TestClosingTheFormWithNo(CloseAction action)
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				SetupTestItem2();
				AssertEquals("Item2.GetValue()", "", (string)Item2.GetValue());
				Assert("Item2.NewValue", Item2.NewValue == null);

				registriesTreeView.Nodes.Add(ItemNode2);
				registriesTreeView.SelectedNode = ItemNode2;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				ZCheckBox overrideCheckBox = testForm.GetOverrideCheckBox();
				overrideCheckBox.Checked = true;
				Item2.NewValue = "NewValue";

				testForm.OnClosingResult = DialogResult.No;

				switch (action)
				{
					case CloseAction.CloseFormDirectly:
						testForm.Close();
						break;
					case CloseAction.CloseButtonClick:
						testForm.GetCloseButton().PerformClick();
						break;
					case CloseAction.CloseMenuItemClick:
						testForm.GetCloseMenuItem().PerformClick();
						break;
				}

				Assert("Form should be closed without saving if user chooses No", !testForm.Visible);
				AssertEquals("Item2.GetValue()", "", (string)Item2.GetValue());
				Assert("Item2.NewValue", Item2.NewValue == null);
				Assert("Item2's IsChanged property should be false", !Item2.IsChanged);
			}
		}

		void TestClosingTheFormWithCancel(CloseAction action)
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				SetupTestItem2();
				AssertEquals("Item2.GetValue()", "", (string)Item2.GetValue());
				Assert("Item2.NewValue", Item2.NewValue == null);

				registriesTreeView.Nodes.Add(ItemNode2);
				registriesTreeView.SelectedNode = ItemNode2;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				ZCheckBox overrideCheckBox = testForm.GetOverrideCheckBox();
				overrideCheckBox.Checked = true;
				Item2.NewValue = "NewValue";

				testForm.OnClosingResult = DialogResult.Cancel;

				switch (action)
				{
					case CloseAction.CloseFormDirectly:
						testForm.Close();
						break;
					case CloseAction.CloseButtonClick:
						testForm.GetCloseButton().PerformClick();
						break;
					case CloseAction.CloseMenuItemClick:
						testForm.GetCloseMenuItem().PerformClick();
						break;
				}

				Assert("Form should not be closed if user chooses Cancel", testForm.Visible);
				AssertEquals("Item2.GetValue()", "NewValue", (string)Item2.GetValue());
				AssertEquals("Item2.NewValue", "NewValue", (string)Item2.NewValue);
				Assert("Item2's IsChanged property should be true", Item2.IsChanged);
				Assert("There should still be changes on the form", testForm.GetChangedItems().Count == 1);
			}
		}

		#endregion

		[RequiresSTA]
		public void TestUpdateColouredFallbacks()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, true);

				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				testForm.GetOverrideCheckBox().Checked = true;
				AssertEquals("ItemNode.ForeColor", Color.Blue, ItemNode1.ForeColor);
				AssertEquals("FallbackTreeView.Nodes[0].ForeColor", Color.Blue, fallbackTreeView.Nodes[0].ForeColor);

				registriesTreeView.SelectedNode = ItemNode2;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				testForm.GetOverrideCheckBox().Checked = true;

				Item2.HasValue = true;
				Item2.NewValue = "1244";
				Item2.CurrentFallbackIsInError = true;
				testForm.UpdateColouredFallbacksForTest();

				AssertEquals("ItemNode2.ForeColor", Color.Red, ItemNode2.ForeColor);
				AssertEquals("FallbackTreeView.Nodes[0].ForeColor", Color.Red, fallbackTreeView.Nodes[0].ForeColor);
			}
		}

		[RequiresSTA]
		public void TestUnhighlightNodes()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, true);

				ItemNode1.ForeColor = Color.Red;
				ItemNode2.ForeColor = Color.Blue;
				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.Nodes[0].ForeColor = Color.Blue;

				Hashtable colouredNodes = testForm.GetColouredNodes();
				ArrayList value = new ArrayList();
				value.Add(fallbackTreeView.Nodes[0]);
				colouredNodes.Add(ItemNode1, value);
				colouredNodes.Add(ItemNode2, new ArrayList());

				testForm.UnhighlightNodesForTest();
				AssertEquals("ItemNode.ForeColor", registriesTreeView.ForeColor, ItemNode1.ForeColor);
				AssertEquals("ItemNode2.ForeColor", registriesTreeView.ForeColor, ItemNode2.ForeColor);
				AssertEquals("FallbackTreeView.Nodes[0].ForeColor", fallbackTreeView.ForeColor, fallbackTreeView.Nodes[0].ForeColor);
			}
		}

		[RequiresSTA]
		public void TestHighlightFallbackNodes()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				testForm.DoNotValidateRegistryItem = true;

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, true);
				registriesTreeView.SelectedNode = ItemNode1;

				Hashtable colouredNodes = testForm.GetColouredNodes();
				ArrayList value = new ArrayList();
				value.Add(fallbackTreeView.Nodes[0]);
				colouredNodes.Add(ItemNode1, value);

				registriesTreeView.SelectedNode = ItemNode2;
				Item2.HasValue = true;
				Item2.NewValue = "1234";
				Item2.CurrentFallbackIsInError = true;
				value = new ArrayList();
				value.Add(fallbackTreeView.Nodes[0]);
				colouredNodes.Add(ItemNode2, value);

				registriesTreeView.SelectedNode = ItemNode1;
				AssertEquals("FallbackTreeView.Nodes[0].ForeColor", Color.Blue, fallbackTreeView.Nodes[0].ForeColor);
				registriesTreeView.SelectedNode = ItemNode2;
				AssertEquals("FallbackTreeView.Nodes[0].ForeColor", Color.Red, fallbackTreeView.Nodes[0].ForeColor);
			}
		}

		public void TestValidateRegistryItem()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				GuidRegistryItem regItem1 = new GuidRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1", RegistryStorageFlags.All, RegistryOptions.IsValueOptional);
				regItem1.EditorInfo = new ZAddressRegistryEditorInfo();
				StringRegistryItem regItem2 = new StringRegistryItem("Name2", (NoResString)"Category2", (NoResString)"Caption2", (NoResString)"Hint2", new ManifestClientIDDataType(), null, RegistryStorageFlags.All, RegistryOptions.Default, "");

				RegistryItemTag item1 = new RegistryItemTag(regItem1);
				RegistryItemTag item2 = new RegistryItemTag(regItem2);

				TreeNode node1 = new TreeNode("Item1");
				TreeNode node2 = new TreeNode("Item2");

				node1.Tag = item1;
				node2.Tag = item2;

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				registriesTreeView.Nodes.Clear();
				registriesTreeView.Nodes.Add(node1);
				registriesTreeView.Nodes.Add(node2);

				// This tests custom plugin validation
				registriesTreeView.SelectedNode = node1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				testForm.GetOverrideCheckBox().Focus();
				AssertEquals("False first so that OnClick makes it true", false, testForm.GetOverrideCheckBox().Checked);
				testForm.GetOverrideCheckBox().OnClick(EventArgs.Empty);

				Control controlWithErrorProvider = testForm.GetOverrideCheckBox();

				testForm.ValidateRegistryItemForTest(true);
				AssertNoNotifications("ErrorProvider should have been suppressed if SuspendErrorProvider is true.", testForm.Manager);
				testForm.ValidateRegistryItemForTest(false);
				AssertHasError(testForm.Manager.OverrideDefaultInfo, "Please select a valid address from an organization.");

				// This tests datatype validation
				registriesTreeView.SelectedNode = node2;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				testForm.GetOverrideCheckBox().Checked = false;
				AssertEquals("False first so that OnClick makes it true", false, testForm.GetOverrideCheckBox().Checked);
				testForm.GetOverrideCheckBox().OnClick(EventArgs.Empty);
				item2.NewValue = "1312421421a";

				testForm.ValidateRegistryItemForTest(true);
				AssertNoNotifications("ErrorProvider should have been suppressed if SuspendErrorProvider is true.", testForm.Manager);
				testForm.ValidateRegistryItemForTest(false);
				AssertHasError(testForm.Manager.OverrideDefaultInfo, "The Manifest Client ID must have a length of 10.");
			}
		}

		public void TestValidateAllRegistryItems()
		{
			const string invalidValue = "a1234567890";
			const string validValue = "a123456789";

			Test(invalidValue, validValue, (form) => AssertHasError("First item invalid", form.Manager.OverrideDefaultInfo, "The Manifest Client ID must have a length of 10."));
			Test(validValue, invalidValue, (form) => AssertHasError("Second item invalid", form.Manager.OverrideDefaultInfo, "The Manifest Client ID must have a length of 10."));
			Test(validValue, validValue, (form) => AssertNoError("Both items valid", form.Manager.OverrideDefaultInfo, "The Manifest Client ID must have a length of 10."));

			void Test(string item1Value, string item2Value, Action<RegistryFormForTest> assert)
			{
				using (RegistryFormForTest testForm = new RegistryFormForTest())
				{
					testForm.Show();

					StringRegistryItem regItem1 = new StringRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1", new ManifestClientIDDataType(), null, RegistryStorageFlags.All, RegistryOptions.Default, "");
					StringRegistryItem regItem2 = new StringRegistryItem("Name2", (NoResString)"Category2", (NoResString)"Caption2", (NoResString)"Hint2", new ManifestClientIDDataType(), null, RegistryStorageFlags.All, RegistryOptions.Default, "");

					RegistryItemTag item1 = new RegistryItemTag(regItem1);
					RegistryItemTag item2 = new RegistryItemTag(regItem2);

					TreeNode node1 = new TreeNode("Item1");
					TreeNode node2 = new TreeNode("Item2");

					node1.Tag = item1;
					node2.Tag = item2;

					TreeView registriesTreeView = testForm.GetRegistriesTreeView();
					TreeView fallbackTreeView = testForm.GetFallbackTreeView();
					registriesTreeView.Nodes.Clear();
					registriesTreeView.Nodes.Add(node1);
					registriesTreeView.Nodes.Add(node2);

					registriesTreeView.SelectedNode = node1;
					fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
					testForm.GetOverrideCheckBox().Focus();
					AssertEquals("False first so that OnClick makes it true", false, testForm.GetOverrideCheckBox().Checked);
					testForm.GetOverrideCheckBox().OnClick(EventArgs.Empty);
					item1.NewValue = item1Value;

					Control controlWithErrorProvider = testForm.GetOverrideCheckBox();

					registriesTreeView.SelectedNode = node2;
					fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
					testForm.GetOverrideCheckBox().Checked = false;
					AssertEquals("False first so that OnClick makes it true", false, testForm.GetOverrideCheckBox().Checked);
					testForm.GetOverrideCheckBox().OnClick(EventArgs.Empty);
					item2.NewValue = item2Value;

					testForm.ValidateRegistryItemForTest(false, false, true);
					assert(testForm);
				}
			}
		}

		public void TestOverrideDefaultSyncWithOverrideOverrideCheckBox()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				DigitalSignatureRegistryItem regItem1 = new DigitalSignatureRegistryItem("Name1", (NoResString)"Category1", (NoResString)"Caption1", (NoResString)"Hint1", new DigitalSignatureRegistry());
				RegistryItemTag item1 = new RegistryItemTag(regItem1);
				TreeNode node1 = new TreeNode("Item1");
				node1.Tag = item1;

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				registriesTreeView.Nodes.Clear();
				registriesTreeView.Nodes.Add(node1);
				registriesTreeView.SelectedNode = node1;

				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				AssertEquals("False first so that OnClick makes it true", false, testForm.GetOverrideCheckBox().Checked);
				testForm.GetOverrideCheckBox().OnClick(EventArgs.Empty);
				Assert(testForm.GetOverrideCheckBox().Checked);
				AssertEquals("The CurrentOverrideDefault value is true and should be synchronized with the state of the OverrideCheckBox ", testForm.GetOverrideCheckBox().Checked, testForm.CurrentOverrideDefault);

				testForm.GetOverrideCheckBox().OnClick(EventArgs.Empty);
				Assert(!testForm.GetOverrideCheckBox().Checked);
				AssertEquals("The CurrentOverrideDefault value is false and should be synchronized with the state of the OverrideCheckBox ", testForm.GetOverrideCheckBox().Checked, testForm.CurrentOverrideDefault);
			}
		}

		public void TestValidateRegistryItem_ReadOnly()
		{
			Env.Security.SystemRegistryEdit.IsAllowed = false;
			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();

				var regItem = new StringRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption2", (NoResString)"Hint2", new StringRegistryDataTypeForTest(), null, RegistryStorageFlags.All, RegistryOptions.Default, "");
				var item = new RegistryItemTag(regItem);
				var node = new TreeNode("Item") { Tag = item };

				var registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Add(node);
				registriesTreeView.SelectedNode = node;

				testForm.ValidateRegistryItemForTest(false);
				AssertNoErrors(testForm.Manager.OverrideDefaultInfo);
			}
		}

		[RequiresSTA]
		public void TestValidateRegistryItemAccordingToOverriden()
		{
			// Setup
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				var dataType = new Mock<IRegistryDataType>();
				dataType.Setup(m => m.DataType).Returns(typeof(bool));
				dataType.Setup(m => m.IsValidatedOnSetEvenIfEqualDefaultValue).Returns(true);
				var item = new Mock<IRegistryItemInternals>();
				item.Setup(m => m.DataType).Returns(dataType.Object);
				var tag = new RegistryItemTag(item.Object);
				var node = new TreeNode("Item") { Tag = tag };
				var registriesTreeView = testForm.GetRegistriesTreeView();
				var fallbackTreeView = testForm.GetFallbackTreeView();
				registriesTreeView.Nodes.Clear();
				registriesTreeView.Nodes.Add(node);
				registriesTreeView.SelectedNode = node;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				// Act & Assert
				testForm.Manager.OverrideDefault = false;
				AssertExceptionThrown<NullReferenceException>(() => testForm.ValidateRegistryItemForTest(true));
				testForm.Manager.OverrideDefault = true;
				AssertNoExceptionThrown(() => testForm.ValidateRegistryItemForTest(true));
			}
		}

		[ExpectNoExceptions]
		public void TestFallbackTreeViewSelectedNode_NullRef()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				ReleaseTypesRegistryItem item1 = new ReleaseTypesRegistryItem("", null, null, null, RegistryStorageFlags.Branch, new ReleaseTypes());
				item1.EditorInfo = null;
				RegistryItemTag tag1 = new RegistryItemTag(item1);
				TreeNode node1 = new TreeNode("");
				node1.Tag = tag1;
				ReleaseTypesRegistryItem item2 = new ReleaseTypesRegistryItem("", null, null, null, RegistryStorageFlags.Branch, new ReleaseTypes());
				item2.EditorInfo = null;
				RegistryItemTag tag2 = new RegistryItemTag(item2);
				TreeNode node2 = new TreeNode("");
				node2.Tag = tag2;
				registriesTreeView.Nodes.Add(node1);
				registriesTreeView.Nodes.Add(node2);

				registriesTreeView.SelectedNode = node1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode.FirstNode;
				testForm.GetOverrideCheckBox().Checked = true;

				ZCalcEdit edit = testForm.GetPluginControl().Controls.Find("zOriginalsCalcEdit", true)[0] as ZCalcEdit;
				edit.Focus();
				edit.Text = "1";

				//change the select Registry TreeView Node
				registriesTreeView.SelectedNode = node2;
				testForm.GetOverrideCheckBox().Checked = true;
				fallbackTreeView.SelectedNode = null;

				testForm.HasChangesForTest = true;
				testForm.GetSaveButton().PerformClick();

				typeof(RegistryForm).InvokeMember("CommitValue", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, testForm, new object[] { testForm.GetPluginControl() });//Invoked when tabbing out the control
			}
		}

		public void TestSaveShortcutWorks()
		{
			using (RegistryFormForTest form = new RegistryFormForTest())
			{
				var overrideCheckBox = form.GetOverrideCheckBox();
				var registriesTreeView = form.GetRegistriesTreeView();
				var saveButton = form.GetSaveButton();

				form.Show();
				SetupTestItem1();
				registriesTreeView.Nodes.Add(ItemNode1);
				registriesTreeView.SelectedNode = ItemNode1;

				overrideCheckBox.Checked = true;
				Item1.NewValue = "Boo!";
				form.OnSaveResult = DialogResult.Yes;
				Assert(saveButton.Enabled);
				Application.DoEvents();

				KeySender.SendKeyDownToProcessCmdKey(form, (int)(Keys.Control | Keys.S));
				Application.DoEvents();

				Assert(!saveButton.Enabled);
			}
		}

		public void TestCommitValueWillRefreshControlNotifications()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				ReleaseTypesRegistryItem item = new ReleaseTypesRegistryItem("", null, null, null, RegistryStorageFlags.Branch, new ReleaseTypes());
				item.EditorInfo = null;
				RegistryItemTag tag = new RegistryItemTag(item);
				TreeNode node = new TreeNode("");
				node.Tag = tag;
				registriesTreeView.Nodes.Add(node);

				registriesTreeView.SelectedNode = node;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode.FirstNode;
				testForm.GetOverrideCheckBox().Checked = true;

				ZCalcEdit edit = testForm.GetPluginControl().Controls.Find("zOriginalsCalcEdit", true)[0] as ZCalcEdit;
				AssertEquals("There is no notification displaying on control", 0, edit.Extensions.Get<INotificationExtension>().Notifications.Count());
				edit.Focus();
				edit.Text = "256";
				testForm.PluginGroupBox.Focus();
				AssertEquals("There is notification displaying on control after inputting an invalid value", 1, edit.Extensions.Get<INotificationExtension>().Notifications.Count());
				AssertEquals("There is notification displaying on default override checkbox after inputting an invalid value", 1, testForm.Manager.OverrideDefaultInfo.Notifications.Count());

				edit.Focus();
				edit.Text = "2";
				typeof(RegistryForm).InvokeMember("CommitValue", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, testForm, new object[] { testForm.GetPluginControl() });//Invoked when tabbing out the control
				AssertEquals("There is no notification displaying on control after changing to a valid value", 0, edit.Extensions.Get<INotificationExtension>().Notifications.Count());
				AssertEquals("There is no notification displaying on default override checkbox after changing to a valid value", 0, testForm.Manager.OverrideDefaultInfo.Notifications.Count());
			}
		}

		public void TestRefreshDefaultOverrideNotifications()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				ReleaseTypesRegistryItem item = new ReleaseTypesRegistryItem("", null, null, null, RegistryStorageFlags.Branch, new ReleaseTypes());
				item.EditorInfo = null;
				RegistryItemTag tag = new RegistryItemTag(item);
				TreeNode node = new TreeNode("");
				node.Tag = tag;
				registriesTreeView.Nodes.Add(node);

				registriesTreeView.SelectedNode = node;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode.FirstNode;
				testForm.GetOverrideCheckBox().Checked = true;

				ZCalcEdit edit = testForm.GetPluginControl().Controls.Find("zOriginalsCalcEdit", true)[0] as ZCalcEdit;
				ZCalcEdit edit2 = testForm.GetPluginControl().Controls.Find("zCopiesCalcEdit", true)[0] as ZCalcEdit;
				AssertEquals("There is no notification displaying on control", 0, edit.Extensions.Get<INotificationExtension>().Notifications.Count());
				edit.Focus();
				edit.Text = "256";
				edit2.Focus();//Simulates clicking another control
				AssertEquals("There is notification displaying on control after inputting an invalid value", 1, edit.Extensions.Get<INotificationExtension>().Notifications.Count());
				AssertEquals("There is notification displaying on default override checkbox after inputting an invalid value", 1, testForm.Manager.OverrideDefaultInfo.Notifications.Count());

				edit.Focus();
				edit.Text = "2";
				edit2.Focus();
				AssertEquals("There is no notification displaying on control  after changing to a valid value", 0, edit.Extensions.Get<INotificationExtension>().Notifications.Count());
				AssertEquals("There is no notification displaying on default override checkbox after changing to a valid value", 0, testForm.Manager.OverrideDefaultInfo.Notifications.Count());
			}
		}

		[RequiresSTA]
		public void TestRegistryFindMenuItem()
		{
			using (var form = new RegistryFormForTest())
			{
				var registriesTreeView = ((ZTreeView)form.GetRegistriesTreeView());
				Assert("Registry find form should have not been opened", !registriesTreeView.FindFormIsOpen);

				form.GetFindMenuItem().PerformClick();
				Assert("Registry find form should have been opened", registriesTreeView.FindFormIsOpen);
			}
		}

		public void TestRegistryOverridesMenuItemEnabled()
		{
			Env.Security.SystemRegistryEdit.IsAllowed = false;
			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();
				AssertEquals("User without Registry Edit right should have the Find Overrides Menu Item disabled.", Env.Security.SystemRegistryEdit.IsAllowed, testForm.findOverridesMenuItem.Enabled);
				AssertEquals("User without Registry Edit right should have the Import Overrides Menu Item disabled.", Env.Security.SystemRegistryEdit.IsAllowed, testForm.importOverridesMenuItem.Enabled);
			}

			Env.Security.SystemRegistryEdit.IsAllowed = true;
			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();
				AssertEquals("User with Registry Edit right should have the Find Overrides Menu Item enabled.", Env.Security.SystemRegistryEdit.IsAllowed, testForm.findOverridesMenuItem.Enabled);
				AssertEquals("User with Registry Edit right should have the Import Overrides Menu Item enabled.", Env.Security.SystemRegistryEdit.IsAllowed, testForm.importOverridesMenuItem.Enabled);
			}
		}

		[DeveloperOnlyTest]
		public void TestEditMenuItem()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, false);
				registriesTreeView.SelectedNode = ItemNode1;

				// Set a value in an active node
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode;
				testForm.GetOverrideCheckBox().Checked = true;
				var pluginControl = (ZTextBox)testForm.GetPluginControl().Controls[0];
				pluginControl.Focus();
				pluginControl.Text = "YOU'RE TERMINATED.";
				pluginControl.Select(2, 6);

				var menuItem = testForm.GetCopyMenuItem();
				menuItem.PerformClick();
				Application.DoEvents();

				var text = GetClipboardTextWithRetry(menuItem);
				AssertEquals("String is copied.", "U'RE T", text);
				SafeClipboard.Clear();

				menuItem = testForm.GetCutMenuItem();
				menuItem.PerformClick();
				Application.DoEvents();

				text = GetClipboardTextWithRetry(menuItem);
				AssertEquals("String is cut.", "U'RE T", text);
				AssertEquals("YOERMINATED.", pluginControl.Text);

				SafeClipboard.SetText("COOL.");
				pluginControl.Select(0, pluginControl.Text.Length);

				menuItem = testForm.GetPasteMenuItem();
				menuItem.PerformClick();
				Application.DoEvents();
				AssertEquals("String is pasted", "COOL.", pluginControl.Text);

				SafeClipboard.Clear();
			}
		}

		string GetClipboardTextWithRetry(MenuItem menuItem)
		{
			var text = SafeClipboard.GetText();
			var i = 0;

			while (string.IsNullOrEmpty(text) && i < 10)
			{
				Thread.Sleep(100);
				menuItem.PerformClick();
				Application.DoEvents();
				text = SafeClipboard.GetText();
				i++;
			}

			return text;
		}

		[DeveloperOnlyTest]
		public void TestEditMenuItemUsingShortCut()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, false);
				registriesTreeView.SelectedNode = ItemNode1;

				// Set a value in an active node
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode;
				testForm.GetOverrideCheckBox().Checked = true;
				ZTextBox pluginControl = (ZTextBox)testForm.GetPluginControl().Controls[0];

				var result = false;
				for (int i = 0; i < 5; i++)
				{
					pluginControl.Text = "YOU'RE TERMINATED.";

					pluginControl.Focus();
					Application.DoEvents();

					pluginControl.Select(2, 6);
					Application.DoEvents();

					KeySender.PostKeyDown(pluginControl, pluginControl.Handle, Keys.Control | Keys.C);
					Application.DoEvents();

					var clipboardText = SafeClipboard.GetText();
					result |= "U'RE T" == clipboardText;
				}
				Assert("String is copied.", result);
				SafeClipboard.Clear();

				KeySender.PostKeyDown(pluginControl, pluginControl.Handle, Keys.Control | Keys.X);
				Application.DoEvents();
				AssertEquals("String is cut.", "U'RE T", SafeClipboard.GetText());
				AssertEquals("YOERMINATED.", pluginControl.Text);

				SafeClipboard.SetText("COOL.");
				pluginControl.Select(0, pluginControl.Text.Length);
				KeySender.PostKeyDown(pluginControl, pluginControl.Handle, Keys.Control | Keys.P);
				Application.DoEvents();
				AssertEquals("String is pasted", "COOL.", pluginControl.Text);

				SafeClipboard.Clear();
			}
		}

		public void TestHintLabelResize()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				var hintLabel = testForm.GetHintLabel();
				var pluginPanel = testForm.GetPluginPanel();

				const string hint = "This is a Really Long String";
				hintLabel.Text = hint + hint + hint + hint + hint + hint + hint + hint + hint + hint;

				testForm.Width = testForm.Width + 1;
				Application.DoEvents();

				var originalWidth = pluginPanel.Width;

				testForm.WindowState = FormWindowState.Minimized;
				testForm.WindowState = FormWindowState.Normal;

				AssertEquals("PluginPanel.Width", originalWidth, pluginPanel.Width);
			}
		}

		public void TestEmptyHintLabelSize()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();

				registriesTreeView.Nodes.Clear();
				StringRegistryItem regItem = new StringRegistryItem("", null, null, null, RegistryStorageFlags.All);
				RegistryItemTag item = new RegistryItemTag(regItem);
				TreeNode node = new TreeNode("");
				node.Tag = item;
				registriesTreeView.Nodes.Add(node);

				registriesTreeView.SelectedNode = node;
				ZLabel hintLabel = testForm.GetHintLabel();
				AssertEquals("HintLabel.Text", "There is no hint available for this Registry item.", hintLabel.Text);
				Assert("HintLabel.Height should be bigger than 0", hintLabel.Height > 0);
			}
		}

		public void TestResizeFallbackMessageLabel()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, false);

				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				ZLabel fallbackMessageLabel = testForm.GetFallbackMessageLabel();
				ZPanel pluginPanel = testForm.GetPluginPanel();
				Control pluginControl = testForm.GetPluginControl();

				string message = "This is a Really Long String";
				fallbackMessageLabel.Text = message + message + message + message + message + message + message + message + message + message;
				pluginControl.Height = pluginPanel.Height - pluginControl.Top;

				testForm.Width = testForm.Width + 1;
				Size originalSize = pluginControl.Size;

				testForm.WindowState = FormWindowState.Minimized;
				testForm.WindowState = FormWindowState.Normal;

				AssertEquals("PluginControl.Size", originalSize, pluginControl.Size);
			}
		}

		[RequiresSTA]
		public void TestSetupDefaultValueMessage()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				SetupRegistriesTreeView(registriesTreeView, false);
				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.Nodes.Clear();

				FallbackTreeNode fallbackNode = new FallbackTreeNode("Node", Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, true);
				fallbackTreeView.Nodes.Add(fallbackNode);
				fallbackTreeView.SelectedNode = fallbackNode;

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.All, false);
				AssertEquals("Default value obtained from Registry item default.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.All, true);
				AssertEquals("Default value is ambiguous and may not be accurate - please refer to parent fall backs.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.System, false);
				AssertEquals("Default value obtained from System level.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.SystemDepartment, false);
				AssertEquals("Default value obtained from System Department level.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.Company, false);
				AssertEquals("Default value obtained from Company level.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.CompanyDepartment, false);
				AssertEquals("Default value obtained from Company Department level.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.Branch, false);
				AssertEquals("Default value obtained from Branch level.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.BranchDepartment, false);
				AssertEquals("Default value overridden.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment, false);
				AssertEquals("Default value obtained by merging values from System and System Department levels.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company, false);
				AssertEquals("Default value obtained by merging values from System, System Department and Company levels.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment, false);
				AssertEquals("Default value obtained by merging values from System, System Department, Company and Company Department levels.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch, false);
				AssertEquals("Default value obtained by merging values from System, System Department, Company, Company Department and Branch levels.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment, false);
				AssertEquals("Default value obtained by merging values from System, System Department, Company, Company Department, Branch and Branch Department levels.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.Company, false, true);
				AssertEquals("Default value obtained by merging values from Registry item default and Company levels.", testForm.GetFallbackMessageLabel().Text);

				testForm.SetupDefaultValueMessageForTest(RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment, false, true);
				AssertEquals("Default value obtained by merging values from Registry item default, System, System Department, Company, Company Department, Branch and Branch Department levels.", testForm.GetFallbackMessageLabel().Text);
			}
		}

		[RequiresSTA]
		public void TestSetupDefaultValueMessage_RegistryItemLockedDown()
		{
			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext("CWPostMaster", EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				registriesTreeView.Nodes.Clear();

				IRegistryItem regItem = new StringRegistryItem("TestName3", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, "DEFAULT");
				regItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue");
				RegistryItemTag item = new RegistryItemTag(regItem);
				TreeNode itemNode = new TreeNode("Item3");
				itemNode.Tag = item;
				registriesTreeView.Nodes.Add(itemNode);

				registriesTreeView.SelectedNode = itemNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				AssertEquals("Should be locked down", true, regItem.IsLockedDown);
				AssertEquals("FallbackMessageLabel.Text", "Default value overridden.", testForm.GetFallbackMessageLabel().Text);
			}
		}

		public void TestSupportItemsInRegistry_HaveSupportOnlyInDetailsTabpage()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext("CWSupport", EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();

				EnvProxy.SetHostedLocationForTest("SYD");

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Clear();

				IRegistryItem hostedRegItem = new StringRegistryItem("EditableBySupportIfHostedName", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, "DEFAULT");
				hostedRegItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				hostedRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue");
				RegistryItemTag itemSupportIfHosted = new RegistryItemTag(hostedRegItem);
				TreeNode itemNodeSupportIfHosted = new TreeNode("EditableBySupportIfHosted");
				itemNodeSupportIfHosted.Tag = itemSupportIfHosted;
				registriesTreeView.Nodes.Add(itemNodeSupportIfHosted);

				IRegistryItem regItemForSupport = new StringRegistryItem("IsOnlyForSupportName", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, "DEFAULT");
				regItemForSupport.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				regItemForSupport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue");
				RegistryItemTag itemOnlySupport = new RegistryItemTag(regItemForSupport);
				TreeNode itemNodeOnlySupport = new TreeNode("IsOnlyForSupport");
				itemNodeOnlySupport.Tag = itemOnlySupport;
				registriesTreeView.Nodes.Add(itemNodeOnlySupport);

				IRegistryItem regItemForDevelopers = new StringRegistryItem("IsOnlyForDevelopersName", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, "DEFAULT");
				regItemForDevelopers.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				regItemForDevelopers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue");
				RegistryItemTag itemOnlyDevelopers = new RegistryItemTag(regItemForDevelopers);
				TreeNode itemNodeOnlyDevelopers = new TreeNode("IsOnlyForDevelopers");
				itemNodeOnlyDevelopers.Tag = itemOnlyDevelopers;
				registriesTreeView.Nodes.Add(itemNodeOnlyDevelopers);

				IRegistryItem regItemDefault = new StringRegistryItem("DefaultName", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, "DEFAULT");
				regItemDefault.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				regItemDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue");
				RegistryItemTag itemDefault = new RegistryItemTag(regItemDefault);
				TreeNode itemNodeDefault = new TreeNode("Default");
				itemNodeDefault.Tag = itemDefault;
				registriesTreeView.Nodes.Add(itemNodeDefault);

				var supportLabel = testForm.GetSupportLabel();
				registriesTreeView.SelectedNode = itemNodeSupportIfHosted;
				AssertEquals("Support label should be visible if registry item is 'IsOnlyEditableBySupportIfHosted' and CW1 hosted.", true, supportLabel.Visible && supportLabel.Text.Equals(testForm.EditableByHostedSupportOnlyText));

				registriesTreeView.SelectedNode = itemNodeOnlySupport;
				AssertEquals("Support label should be visible if registry item is 'IsOnlyForSupport' and CW1 hosted.", true, supportLabel.Visible && supportLabel.Text.Equals(testForm.VisibleToSupportOnlyText));

				registriesTreeView.SelectedNode = itemNodeOnlyDevelopers;
				AssertEquals("Support label should be visible if registry item is 'IsOnlyForDevelopers' and CW1 hosted.", true, supportLabel.Visible && supportLabel.Text.Equals(testForm.VisibleToDevelopersOnlyText));

				registriesTreeView.SelectedNode = itemNodeDefault;
				AssertEquals("Support label should NOT be visible for default registry item even if CW1 hosted.", false, supportLabel.Visible);

				EnvProxy.SetHostedLocationForTest("NCW");

				registriesTreeView.SelectedNode = itemNodeOnlySupport;
				AssertEquals("Support label should be visible if registry is 'IsOnlyForSupport' and CW1 not hosted.", true, supportLabel.Visible && supportLabel.Text.Equals(testForm.VisibleToSupportOnlyText));

				registriesTreeView.SelectedNode = itemNodeDefault;
				AssertEquals("Support label should NOT be visible for default registry item", false, supportLabel.Visible);
			}
		}

		IEnumerable<TreeNode> GetNodeBranch(TreeNode node)
		{
			yield return node;

			foreach (TreeNode child in node.Nodes)
			{
				foreach (var childChild in GetNodeBranch(child))
				{
					yield return childChild;
				}
			}
		}

		public void TestTreeViewBeforeSelectCallsUpdateRegistryItemValueFromPluginControl()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				testForm.DoNotAddPluginControlLeaveEventHandler = true;
				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, true);

				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				testForm.GetOverrideCheckBox().Checked = true;
				testForm.GetPluginControl().Focus();
				testForm.GetPluginControl().Text = "ABC";

				Assert("UpdateRegistryItemValueFromPluginControl should not be called yet", !testForm.PerformUpdateRegistryItemValueFromPluginControlCalled);
				registriesTreeView.SelectedNode = ItemNode2;

				Assert("UpdateRegistryItemValueFromPluginControl should have been called", testForm.PerformUpdateRegistryItemValueFromPluginControlCalled);
				AssertEquals("PluginControl.Text", "", testForm.GetPluginControl().Text);
				Assert("OverrideCheckBox should not be checked", !testForm.GetOverrideCheckBox().Checked);
				Assert("PluginControl should be ReadOnly", testForm.GetPluginControl().GetReadOnly());

				testForm.PerformUpdateRegistryItemValueFromPluginControlCalled = false;

				registriesTreeView.SelectedNode = ItemNode1;
				Assert("UpdateRegistryItemValueFromPluginControl should not be called if PluginControl was not entered", !testForm.PerformUpdateRegistryItemValueFromPluginControlCalled);

				Assert("OverrideCheckBox should be checked", testForm.GetOverrideCheckBox().Checked);
				Assert("PluginControl should not be ReadOnly", !testForm.GetPluginControl().GetReadOnly());
				AssertEquals("PluginControl.Text", "", testForm.GetPluginControl().Text);
			}
		}

		// ExpectException doesn't work here
		public void TestFallbackTreeView_AfterSelectThrowsException()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();
				SetupRegistriesTreeView(registriesTreeView, true);

				registriesTreeView.SelectedNode = ItemNode1;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode;
				testForm.DoNotPerformUpdateRegistryItemValueFromPluginControl = true;
				testForm.PluginControlEnteredButLeaveEventNotFiredForTest = true;
				Assert("Precondition: Exception should be exception yet", testForm.ExceptionThrown == null);
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.Nodes[1];
				AssertEquals("Exception message", "PluginControl was entered, but its value was not retrieved and updated to the previous registry item before a different fallback was selected.", testForm.ExceptionThrown.Message);
			}
		}

		// ExpectException doesn't work here
		public void TestRegistriesTreeView_AfterSelectThrowsException()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				SetupRegistriesTreeView(registriesTreeView, true);

				registriesTreeView.SelectedNode = ItemNode1;
				testForm.DoNotPerformUpdateRegistryItemValueFromPluginControl = true;
				testForm.PluginControlEnteredButLeaveEventNotFiredForTest = true;
				Assert("Precondition: Exception should be exception yet", testForm.ExceptionThrown == null);
				registriesTreeView.SelectedNode = ItemNode2;
				AssertEquals("Exception message", "PluginControl was entered, but its value was not retrieved and updated to the previous registry item before a different registry item was selected.", testForm.ExceptionThrown.Message);
			}
		}

		public void TestAmpersandDisplaysCorrectly()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				registriesTreeView.Nodes.Clear();
				TreeNode itemNode = new TreeNode("Something & Something");
				registriesTreeView.Nodes.Add(itemNode);

				registriesTreeView.SelectedNode = itemNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				AssertEquals("HintLabel.UseMnemonic", false, testForm.GetHintLabel().UseMnemonic);
				AssertEquals("PluginGroupBox.Text", "Something && Something", testForm.PluginGroupBox.Text);
			}
		}

		public void TestFactoryForRegistryItemEditors()
		{
			using (RegistryFormForTest testForm = new RegistryFormForTest())
			{
				BankAccountBasedOnCurrencyRegistryItem registryItem = new BankAccountBasedOnCurrencyRegistryItem("", null, null, null, RegistryStorageFlags.System);
				RegistryItemTag tag = new RegistryItemTag(registryItem);
				TreeNode node = new TreeNode();
				node.Tag = tag;

				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				TreeView fallbackTreeView = testForm.GetFallbackTreeView();

				registriesTreeView.Nodes.Clear();
				registriesTreeView.Nodes.Add(node);

				registriesTreeView.SelectedNode = node;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				using (BankAccountBasedOnCurrencyControl pluginControl = (BankAccountBasedOnCurrencyControl)testForm.GetPluginControl())
				{
					PropertyInfo propertyInfo = typeof(BankAccountBasedOnCurrencyCollection).GetProperty("CurrentFactory", BindingFlags.NonPublic | BindingFlags.Instance);

					BusinessObjectFactory actualFactory = (BusinessObjectFactory)propertyInfo.GetValue(((IDataBoundControl)pluginControl).DataSource, null);
					Assert("PluginControl's Factory should not be the RegistryFactory.", actualFactory != RegistryFactory.Instance);
					AssertEquals("PluginControl's Factory should be the RegistryForm's Factory.", testForm.Factory, actualFactory);
				}
			}
		}

		[RequiresSTA]
		public void TestFindOverridesShowsTheOverrideLevelsForm()
		{
			using (var form = new RegistryFormForTest())
			{
				form.findOverridesMenuItem.PerformClick();

				AssertEquals(typeof(FindOverridesLevelForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		[RequiresSTA]
		public void TestFindOverridesWithItemsShowsTheDiffForm()
		{
			using (var form = new RegistryFormForTest())
			{
				var items = new[]
				{
					new IntRegistryItem("First", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All),
					new IntRegistryItem("Second", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All),
					new IntRegistryItem("Third", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All),
				};

				var baseLevel = new Mock<IOverrideLevel>();
				baseLevel.Setup(m => m.GetValueOf(It.IsAny<IRegistryItem>())).Returns(0);

				var overrideLevel = new Mock<IOverrideLevel>();
				overrideLevel.Setup(m => m.GetValueOf(It.IsAny<IRegistryItem>())).Returns(1);

				form.diffBusinessObjectForTest = new RegistryComparisonBusinessObject(items, baseLevel.Object, overrideLevel.Object);

				form.findOverridesMenuItem.PerformClick();

				using (var exportFormThatWasHopefullyOpened = Application.OpenForms.OfType<RegistryExportForm>().SingleOrDefault())
				{
					AssertNotNull("Form should have been opened", exportFormThatWasHopefullyOpened);
				}
			}
		}

		[RequiresSTA]
		public void TestFindOverridesClickWhenNoItemsAreFound()
		{
			using (var form = new RegistryFormForTest())
			{
				form.diffBusinessObjectForTest = new RegistryComparisonBusinessObject(Enumerable.Empty<IRegistryItem>(), new DefaultOverrideLevel(), new SystemOverrideLevel());
				form.findOverridesMenuItem.PerformClick();

				var message = (UnitTestUserNotification)Globals.Message;
				AssertEquals(message.LastMessage.Text, "No difference was found between the two levels you selected.");
			}
		}

		public void TestLinkRegistryItems()
		{
			using (RegistryFormForTest form = new RegistryFormForTest())
			{
				form.Show();

				TreeView registriesTreeView = form.GetRegistriesTreeView();
				TreeView fallbackTreeView = form.GetFallbackTreeView();
				ZLabel valueLabel = form.GetValueLabel();
				ZCheckBox overrideCheckBox = form.GetOverrideCheckBox();
				ZPanel pluginPanel = form.GetPluginPanel();

				StringRegistryItem stringItem = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
				LinkRegistryItem linkItem = new LinkRegistryItem((NoResString)"", (NoResString)"Barf", (NoResString)"", ModuleIDs.StmMenuItem);
				TreeNode stringNode = GetRegistryTreeNode(stringItem);
				TreeNode linkNode = GetRegistryTreeNode(linkItem);

				registriesTreeView.Nodes.Clear();
				registriesTreeView.Nodes.Add(stringNode);
				registriesTreeView.Nodes.Add(linkNode);

				AssertNull("LinkButton", form.LinkButton);

				registriesTreeView.SelectedNode = linkNode;

				AssertEquals("ValueLabel.Visible", false, valueLabel.Visible);
				AssertEquals("OverrideCheckBox.Visible", false, overrideCheckBox.Visible);
				AssertNotNull("LinkButton", form.LinkButton);
				AssertEquals("LinkButton.Location", valueLabel.Location, form.LinkButton.Location);
				AssertEquals("PluginPanel.Controls.Contains(LinkButton)", true, pluginPanel.Controls.Contains(form.LinkButton));
				AssertEquals("LinkButton.Text", "Edit Barf", form.LinkButton.Text);
				AssertEquals("LinkButton.ModuleID", ModuleIDs.StmMenuItem, form.LinkButton.ModuleID);
				AssertNull("PluginControl", form.GetPluginControl());

				form.LinkButton.Focus();
				form.UpdateRegistryItemValueFromPluginControlForTest();
				AssertEquals("ChangedItems.Count", 0, form.GetChangedItems().Count);

				registriesTreeView.SelectedNode = stringNode;

				AssertEquals("ValueLabel.Visible", true, valueLabel.Visible);
				AssertEquals("OverrideCheckBox.Visible", true, overrideCheckBox.Visible);
				AssertEquals("PluginPanel.Controls.Contains(LinkButton)", false, pluginPanel.Controls.Contains(form.LinkButton));
				AssertEquals("PluginControl.GetType()", typeof(ZTextBox), form.GetPluginControl().Controls[0].GetType());
			}
		}

		public void TestFormReadOnly()
		{
			Env.Security.SystemRegistryEdit.IsAllowed = false;
			using (RegistryFormForTest form = new RegistryFormForTest())
			{
				Assert(form.IsFormReadOnly);
			}
			Env.Security.SystemRegistryEdit.IsAllowed = true;
			using (RegistryFormForTest form = new RegistryFormForTest())
			{
				Assert(!form.IsFormReadOnly);
			}
		}

#if !WINZOR

		[GuiTest]
		[ExpectNoExceptions]
		public void TestAllRegistryItemsNodesHaveAssociatedEditControl()
		{
			IList<TreeNode> registryNodes;
			using (var form = new RegistryFormForTest(ObjectFactory.Get<IRegistryProvider>()))
			{
				form.Show();

				var registriesTreeView = form.GetRegistriesTreeView();
				registriesTreeView.BeginUpdate();
				registriesTreeView.ExpandAll();
				registriesTreeView.EndUpdate();

				registryNodes = EnumerateAllNodes(registriesTreeView.Nodes).ToList();
			}

			using (var fallbackTreeView = new TreeView())
			{
				var builder = new RegistryFormTreeViewBuilder(Factory);

				foreach (var registryNode in registryNodes)
				{
					if (registryNode.Tag is RegistryItemTag tag)
					{
						if (tag.RegistryItem is LinkRegistryItem)
						{
							// LinkRegistryItems is handled separately by setting up LinkButton control in method RegistryForm.HandleLinkRegistryItem
							continue;
						}

						builder.UpdateFallbackTree(fallbackTreeView, registryNode);

						var fallbackNode = EnumerateAllNodes(fallbackTreeView.Nodes).Cast<FallbackTreeNode>().FirstOrDefault(fn => fn != null && fn.Status == FallbackStatus.Active);
						if (fallbackNode == null)
						{
							// Skip for now - registry item for some reason does not have fallback value, e.g. ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors
							continue;
						}

						var editor = RegistryForm.GetEditor(tag, fallbackNode.GetFallbackLevel(), Factory);
						if (editor == null)
						{
							// In method RegistryForm.HandleValidFallback `editor == null` cases are skipped 
							continue;
						}

						using (var pluginControl = editor.NewWinFormsEditorPane())
						{
							AssertNotNull($"Registry item {tag.Name} should have editor control.", pluginControl);
						}
					}
				}
			}
		}

		IEnumerable<TreeNode> EnumerateAllNodes(TreeNodeCollection nodes)
		{
			if (nodes == null)
			{
				yield break;
			}

			foreach (TreeNode node in nodes)
			{
				yield return node;
				foreach (var childNode in EnumerateAllNodes(node.Nodes))
				{
					yield return childNode;
				}
			}
		}

#endif

		public void TestItemsArePurgedOnSaveOrClear()
		{
			var dictionaryInternals = (IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance;
			using (RegistryFormForTest form = new RegistryFormForTest())
			{
				form.Show();
				SetupTestItem1();
				form.GetRegistriesTreeView().Nodes.Add(ItemNode1);
				form.GetRegistriesTreeView().SelectedNode = ItemNode1;
				form.GetOverrideCheckBox().Checked = true;
				Item1.NewValue = "Boo!";
				form.OnSaveResult = DialogResult.Yes;

				Assert("Some items should be in the dictionary.", dictionaryInternals.Count > 0);
				form.GetSaveButton().PerformClick();
				AssertEquals("Dictionary should be purged.", 0, dictionaryInternals.Count);

				object dummy = SystemDataRegistry.Instance.AllowDocManagerBatchProcessorImports.Value;
				Assert("Some items should be in the dictionary.", dictionaryInternals.Count > 0);

				Item1.NewValue = "Moo!";
				form.HasChangesForTest = true;
				form.OnClosingResult = DialogResult.Cancel;
				form.GetCloseButton().PerformClick();
				Assert("Some items should be in the dictionary.", dictionaryInternals.Count > 0);

				form.OnClosingResult = DialogResult.No;
				form.GetCloseButton().PerformClick();
				AssertEquals("Dictionary should be purged.", 0, dictionaryInternals.Count);
			}
		}

		public void TestTranslateButton()
		{
			using (RegistryFormForTest form = new RegistryFormForTest())
			{
				form.Show();
				SetupTestItem1();
				var regItem2 = new MultilingualStringRegistryItem("TestItem2", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, ResString.GetMultilingualString("x", "Luminous"));
				Item2 = new RegistryItemTag(regItem2);
				ItemNode2 = new TreeNode("Item2");
				ItemNode2.Tag = Item2;

				var translateButton = (ZButton)form.Controls.Find("translateButton", true)[0];

				form.GetRegistriesTreeView().Nodes.Add(ItemNode1);
				form.GetRegistriesTreeView().Nodes.Add(ItemNode2);
				form.GetRegistriesTreeView().SelectedNode = ItemNode1;
				AssertEquals("translateButton.Visible", false, translateButton.Visible);

				form.GetRegistriesTreeView().SelectedNode = ItemNode2;
				AssertEquals("translateButton.Visible", true, translateButton.Visible);

				Form formCreated = null;
				var formCreatedHandler = new EventHandler((sender, args) => { formCreated = sender as Form; });
				try
				{
					ZForm.FormCreated += formCreatedHandler;

					translateButton.PerformClick();
					AssertNotNull(formCreated);
					AssertEquals("CustomizableDataTranslationForm", formCreated.Name);
					formCreated.Dispose();
				}
				finally
				{
					ZForm.FormCreated -= formCreatedHandler;
				}
			}
		}

		public void TestTranslateButtonConsidersIsTranslatable()
		{
			using (RegistryFormForTest form = new RegistryFormForTest())
			{
				form.Show();
				var regItem1 = new CodeDescriptionPairListRegistryItem("Item1", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", 3, RegistryStorageFlags.System, true, new ReadOnlyCodeDescriptionPairList());
				Item1 = new RegistryItemTag(regItem1);
				ItemNode1 = new TreeNode("Item1");
				ItemNode1.Tag = Item1;

				var regItem2 = new CodeDescriptionPairListRegistryItem("Item2", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", 3, RegistryStorageFlags.System, false, new ReadOnlyCodeDescriptionPairList());
				Item2 = new RegistryItemTag(regItem2);
				ItemNode2 = new TreeNode("Item2");
				ItemNode2.Tag = Item2;

				var translateButton = (ZButton)form.Controls.Find("translateButton", true)[0];

				form.GetRegistriesTreeView().Nodes.Add(ItemNode1);
				form.GetRegistriesTreeView().Nodes.Add(ItemNode2);

				form.GetRegistriesTreeView().SelectedNode = ItemNode1;
				AssertEquals("translateButton.Visible", true, translateButton.Visible);

				form.GetRegistriesTreeView().SelectedNode = ItemNode2;
				AssertEquals("translateButton.Visible", false, translateButton.Visible);
			}
		}

		[ExpectNoExceptions] // using mock verication
		public void TestTranslateButtonUsesFallback()
		{
			var item = new MultilingualStringRegistryItem(
				new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All));

			using (RegistryFormForTest form = new RegistryFormForTest(item))
			{
				form.Show();

				var registriesTreeView = form.GetRegistriesTreeView();
				var fallbackTreeView = form.GetFallbackTreeView();
				var registryNode = registriesTreeView.Nodes[0];
				registriesTreeView.ExpandAll();

				MultilingualStringRegistryItem multilingualItem = null;
				do
				{
					registryNode = registryNode.NextVisibleNode;
					var tag = registryNode.Tag as RegistryItemTag;
					if (tag != null)
					{
						multilingualItem = tag.RegistryItem as MultilingualStringRegistryItem;
					}
				}
				while (multilingualItem == null || !multilingualItem.IsTranslatable || !(multilingualItem.Storage.HasFlag(RegistryStorageFlags.System) && multilingualItem.Storage.HasFlag(RegistryStorageFlags.Company)));

				((RegistryItemTag)registryNode.Tag).HasValue = true;
				((RegistryItemTag)registryNode.Tag).NewValue = "system override";
				((RegistryItemTag)registryNode.Tag).IsChanged = true;

				registriesTreeView.SelectedNode = registryNode;
				fallbackTreeView.ExpandAll();
				var fallbackNode = (FallbackTreeNode)fallbackTreeView.Nodes[0];
				do
				{
					fallbackNode = (FallbackTreeNode)fallbackNode.NextVisibleNode;
				}
				while (fallbackNode.Status != FallbackStatus.Active || fallbackNode.GetFallbackLevel().CompanyPK(true) == Guid.Empty);
				fallbackTreeView.SelectedNode = fallbackNode;
				var translateButton = (ZButton)form.Controls.Find("translateButton", true)[0];

				AssertEditTranslations(translateButton, "system override");

				((RegistryItemTag)registryNode.Tag).SaveAllValues();

				AssertEditTranslations(translateButton, "system override");

				((RegistryItemTag)registryNode.Tag).HasValue = true;
				((RegistryItemTag)registryNode.Tag).NewValue = "company override";
				((RegistryItemTag)registryNode.Tag).IsChanged = true;
				AssertEditTranslations(translateButton, "company override");

				((RegistryItemTag)registryNode.Tag).SaveAllValues();
				AssertEditTranslations(translateButton, "company override");
			}
		}

		public void TestTranslateButtonEnable()
		{
			using (var form = new RegistryFormForTest())
			{
				form.Show();
				var regItem1 = new CodeDescriptionPairListRegistryItem("Item1", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", 3, RegistryStorageFlags.CompanyDepartment, true, new ReadOnlyCodeDescriptionPairList());
				Item1 = new RegistryItemTag(regItem1);
				ItemNode1 = new TreeNode("Item1");
				ItemNode1.Tag = Item1;

				var regItem2 = new CodeDescriptionPairListRegistryItem("Item2", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", 3, RegistryStorageFlags.System, false, new ReadOnlyCodeDescriptionPairList());
				Item2 = new RegistryItemTag(regItem2);
				ItemNode2 = new TreeNode("Item2");
				ItemNode2.Tag = Item2;

				var translateButton = (ZButton)form.Controls.Find("translateButton", true)[0];
				var registriesTreeView = form.GetRegistriesTreeView();
				var fallbackTreeView = form.GetFallbackTreeView();

				registriesTreeView.Nodes.Add(ItemNode1);
				registriesTreeView.Nodes.Add(ItemNode2);

				registriesTreeView.SelectedNode = ItemNode1;
				AssertEquals("translateButton.Visible", true, translateButton.Visible);

				fallbackTreeView.SelectedNode = null;
				AssertNull("fallbackTreeView.SelectedNode", fallbackTreeView.SelectedNode);
				AssertEquals("translateButton.Enable", false, translateButton.Enabled);

				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				AssertNotNull("fallbackTreeView.SelectedNode", fallbackTreeView.SelectedNode);
				AssertEquals("SelectedNode is not a Fallback node", FallbackStatus.NotAFallback, ((FallbackTreeNode)fallbackTreeView.SelectedNode).Status);
				AssertEquals("translateButton.Enable", false, translateButton.Enabled);

				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0].FirstNode.FirstNode.FirstNode;
				AssertNotNull("fallbackTreeView.SelectedNode", fallbackTreeView.SelectedNode);
				AssertNotEquals("SelectedNode is a Fallback node", FallbackStatus.NotAFallback, ((FallbackTreeNode)fallbackTreeView.SelectedNode).Status);
				AssertEquals("translateButton.Enable", true, translateButton.Enabled);

				registriesTreeView.SelectedNode = ItemNode2;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];
				AssertEquals("translateButton.Visible", false, translateButton.Visible);
				AssertNotNull("fallbackTreeView.SelectedNode", fallbackTreeView.SelectedNode);
				AssertNotEquals("SelectedNode is a Fallback node", FallbackStatus.NotAFallback, ((FallbackTreeNode)fallbackTreeView.SelectedNode).Status);
				AssertEquals("translateButton.Enable", false, translateButton.Enabled);
			}
		}

		public void TestOverrideCheckBoxCheckedCorrectly()
		{
			using (RegistryFormForTest form = new RegistryFormForTest())
			{
				form.Show();
				TreeView registriesTreeView = form.GetRegistriesTreeView();
				TreeView fallbackTreeView = form.GetFallbackTreeView();
				ZCheckBox overrideCheckBox = form.GetOverrideCheckBox();

				registriesTreeView.Nodes.Clear();

				ColorThemeSelector selector = new ColorThemeSelector();
				IRegistryItem regItem = new ColorThemeRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", selector);
				RegistryItemTag item = new RegistryItemTag(regItem);
				TreeNode itemNode = new TreeNode("Item");
				itemNode.Tag = item;
				registriesTreeView.Nodes.Add(itemNode);

				registriesTreeView.SelectedNode = itemNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				overrideCheckBox.Focus();
				overrideCheckBox.CheckState = CheckState.Checked;

				Assert(overrideCheckBox.CheckState == CheckState.Checked);
			}
		}

		public void TestChangeRegistryItemWhenHintTextTooLong()
		{
			var registry = new Mock<IRegistry>();
			registry.Setup(r => r.GetSortedTopLevelCategories()).Returns(new[] { new RegistryCategoryRef(1, "cat1"), new RegistryCategoryRef(2, "cat2") });

			using (RegistryFormForTest testForm = new RegistryFormForTest(registry.Object))
			{
				testForm.Show();

				string hint = "This is a Really Long String\r\n\r\nThis is a Really Long String\r\n\r\nThis is a Really Long String\r\n\r\n";
				IRegistryItem regItem = new LicencedStringRegistryItem(() => false,
				"TestItem", (NoResString)"Category", (NoResString)"TestCaption",
				(NoResString)(hint + hint + hint + hint + hint + hint + hint + hint + hint + hint + hint + hint + hint + hint + hint),
				RegistryStorageFlags.All);
				RegistryItemTag item = new RegistryItemTag(regItem);

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				var registryNodes = registriesTreeView.Nodes;
				registryNodes[0].Tag = item;

				registriesTreeView.SelectedNode = registryNodes[0];

				Application.DoEvents();

				registriesTreeView.SelectedNode = registryNodes[1];

				var hintLabel = testForm.GetHintLabel();

				AssertEquals("After select another item, the hint should be updated", testForm.GetDefaultHint(), hintLabel.Text);
			}
		}

		public void TestScrollHintUserControlEnabled()
		{
			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();
				Application.DoEvents();

				var hintUserControl = testForm.GetHintUserControl();
				Assert("Auto Scroll must be enabled.", hintUserControl.AutoScroll);
			}
		}

		void AssertEditTranslations(ZButton translateButton, string expectedCaption)
		{
			var mockEditor = new Mock<ICustomizableDataTranslationEditor>(MockBehavior.Strict);
			mockEditor
				.Setup(
					m => m.EditTranslations(
						It.Is<CustomizableDataResourceStrings>(p =>
							p.Source.GetRuntimeCaptions(null, null).Any(c => c.ToString() == expectedCaption)),
						It.Is<ResourceString>(o => o.ToString() == expectedCaption),
						It.IsAny<object>()
					)
				);

			ObjectFactory.Substitute(mockEditor.Object);

			translateButton.PerformClick();
			mockEditor.VerifyAll();

			ObjectFactory.DisposeSubstitutions();
		}

		public void TestPluginBusinessObjectRegistryName()
		{
			using (var testForm = new RegistryFormForTest())
			{
				var registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Clear();
				testForm.Show();
				var testNode = new TreeNode();
				var regItem = new ComplianceReportsSetupRegistryItem("AAA", null, null, null, new ComplianceReportTypeCollection(), RegistryOptions.IsOnlyForSupport);
				var itemTag = new RegistryItemTag(regItem);
				testNode.Tag = itemTag;
				registriesTreeView.Nodes.Add(testNode);
				registriesTreeView.SelectedNode = testNode;

				AssertEquals("AAA", ((testForm.GetPluginControl() as ZUserControl)?.BindingSource.DataSource as RegistryBusinessObjectCollectionTemplate).RegistryName);
			}
		}

		public void TestChangeBranchDepartmentSaveAndClose_RegistryForm()
		{
			using (var testForm = new RegistryFormForTest())
			{
				var registriesTreeView = testForm.GetRegistriesTreeView();
				registriesTreeView.Nodes.Clear();
				testForm.Show();

				var testNode = new TreeNode();
				var regItem = new LicencedStringRegistryItem(() => false, "TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All);
				var itemTag = new RegistryItemTag(regItem);
				testNode.Tag = itemTag;
				registriesTreeView.Nodes.Add(testNode);
				registriesTreeView.SelectedNode = testNode;

				AssertNoExceptionThrown(() => { _ = testForm.FireSaveButton(); });
			}
		}

		#region Notes

		public void TestRegistryItemWithNoteDisplaysNoteOnLoad()
		{
			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();

				var registriesTreeView = testForm.GetRegistriesTreeView();
				var fallbackTreeView = testForm.GetFallbackTreeView();
				registriesTreeView.Nodes.Clear();

				var regItem = new StringRegistryItem("TestName3", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, "DEFAULT");
				regItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
				regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NewValue");
				var item = new RegistryItemTag(regItem);
				var itemNode = new TreeNode("Item3");
				itemNode.Tag = item;
				registriesTreeView.Nodes.Add(itemNode);
				var stmData = testForm.Factory.Load<StmData>(item.GetRegistryItemPK());
				stmData.Notes.AddNew(false, "TestNote", "This is a test note.");

				registriesTreeView.SelectedNode = itemNode;
				fallbackTreeView.SelectedNode = fallbackTreeView.Nodes[0];

				AssertEquals("Changes Log & Notes tab page should be added", 3, testForm.GetRegistryItemTabControl().TabPages.Count);
				AssertEquals("Notes tab page should be added", typeof(ZStmNoteForRegistryItemTabPage), testForm.GetRegistryItemTabControl().TabPages[2].GetType());
			}
		}

		#endregion

		#region TestExportListOfPreservedRegistry

		[RequiresSTA]
		public void TestGetPerservedRegistryItems()
		{
			using (var testForm = new RegistryFormForTest())
			{
				SetupTestItem1();
				RegItem1.Options = RegistryOptions.PreserveTestValue;
				SetupTestItem2();

				var list = testForm.GetPerservedRegistryItemsForTest(new List<IRegistryItem>() { RegItem1, RegItem2 });

				AssertEquals("Only PreserveTestValue", 1, list.Count());
				Assert("Only PreserveTestValue", list.Contains(RegItem1));

				testForm.OldRegistryValues = new List<string>() { RegItem2.Name.ToUpper() };
				list = testForm.GetPerservedRegistryItemsForTest(new List<IRegistryItem>() { RegItem1, RegItem2 });

				AssertEquals(false, RegItem2.HasOption(RegistryOptions.PreserveTestValue));
				AssertEquals("PreserveTestValue + OldRegistryValues", 2, list.Count());
				Assert(list.Contains(RegItem1));
				Assert(list.Contains(RegItem2));

				RegItem2.Options = RegistryOptions.PreserveTestValue | RegistryOptions.IsHidden;
				list = testForm.GetPerservedRegistryItemsForTest(new List<IRegistryItem>() { RegItem1, RegItem2 });

				AssertEquals(true, RegItem2.HasOption(RegistryOptions.PreserveTestValue));
				AssertEquals(true, RegItem2.HasOption(RegistryOptions.IsHidden));
				AssertEquals("Hidden Item Should be remove", 1, list.Count());
				Assert("Hidden Item Should be remove", list.Contains(RegItem1));
			}
		}

		public void TestExportListOfPreservedRegistryItemsMenuItem()
		{
			using (var testForm = new RegistryFormForTest())
			{
				testForm.Show();

				SetupTestItem1();
				RegItem1.Options = RegistryOptions.PreserveTestValue;
				SetupTestItem2();
				RegItem2.Options = RegistryOptions.PreserveTestValue;
				var tempFilePath = Temp.GetTempFileNameWithExtension("csv");

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFilePath;
				try
				{
					testForm.exportListOfPreservedRegistryItemsMenuItem.PerformClick();
					Assert(File.Exists(tempFilePath));
				}
				finally
				{
					DeleteIfExists(tempFilePath);
				}
			}
		}

		#endregion

		#region FilterChangeLogsMenuItem

		public void TestFilterChangeLogsMenuItemNotCheckedAfterCancel()
		{
			// Arrange
			var registryMock = new Mock<IRegistry>(MockBehavior.Strict);
			registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(new[]
			{
				new RegistryCategoryRef(1, "Category 1"),
				new RegistryCategoryRef(2, "Category 2"),
			});
			registryMock.Setup(r => r.GetSortedContent(It.IsAny<object>())).Returns(new RegistryCategoryContent());

			using (var testForm = new RegistryFormForTest(registryMock.Object))
			{
				testForm.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				testForm.GetFilterChangeLogsMenuItem().PerformClick();

				Assert("Filter change logs menu item should not be checked", !testForm.GetFilterChangeLogsMenuItem().Checked);
			}
		}

		public void TestEnableFilterChangeLogsMenuItemUpdatesTreeView()
		{
			// Arrange
			var registryMock = new Mock<IRegistry>(MockBehavior.Strict);
			registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(new[]
			{
				new RegistryCategoryRef(1, "Category 1"),
				new RegistryCategoryRef(2, "Category 2"),
			});
			registryMock.Setup(r => r.GetSortedContent(It.IsAny<object>())).Returns(new RegistryCategoryContent());

			using (var testForm = new RegistryFormForTest(registryMock.Object))
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testForm.GetFilterChangeLogsMenuItem().PerformClick();

				AssertEquals("All registry items should have been hidden", 0, registriesTreeView.Nodes.Count);
			}
		}

		public void TestDisableFilterChangeLogsMenuItemUpdatesTreeView()
		{
			// Arrange
			var registryMock = new Mock<IRegistry>(MockBehavior.Strict);
			registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(new[]
			{
				new RegistryCategoryRef(1, "Category 2"),
				new RegistryCategoryRef(2, "Category 3"),
			});

			using (var testForm = new RegistryFormForTest(registryMock.Object))
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testForm.GetFilterChangeLogsMenuItem().PerformClick();

				AssertEquals("All registry items should have been hidden", 0, registriesTreeView.Nodes.Count);

				testForm.GetFilterChangeLogsMenuItem().PerformClick();

				AssertEquals("All registry items should have been reloaded", 2, registriesTreeView.Nodes.Count);
			}
		}

		public void TestEnalbeFilterControlFiltersForSubCategory()
		{
			const int cat1Key = 1;
			const int subCat11Key = 11;
			const int subCat12Key = 12;
			var registryMock = new Mock<IRegistry>(MockBehavior.Strict);
			registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(new[] { new RegistryCategoryRef(cat1Key, "Cat-1") });
			registryMock.Setup(r => r.GetSortedContent(cat1Key)).Returns(new RegistryCategoryContent(new[]
				{
					new RegistryCategoryRef(11, "Sub-Category 11"),
					new RegistryCategoryRef(12, "Sub-Category 12"),
				},
				new IRegistryItem[]
				{
					new StringRegistryItem("TestItem2", null, (NoResString)"ZZ-Item 2", null, RegistryStorageFlags.System),
				}));
			registryMock.Setup(r => r.GetSortedContent(subCat11Key)).Returns(new RegistryCategoryContent());
			registryMock.Setup(r => r.GetSortedContent(subCat12Key)).Returns(
				new RegistryCategoryContent(Array.Empty<RegistryCategoryRef>(),
					new IRegistryItem[]
					{
						new StringRegistryItem("TestItem1", (NoResString)"Cat-1/Sub-Category 12",
							(NoResString)"ZZ-Item 1", null, RegistryStorageFlags.System)
					}));

			using (var testForm = new RegistryFormForTest(registryMock.Object))
			{
				testForm.Show();

				TreeView registriesTreeView = testForm.GetRegistriesTreeView();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testForm.GetFilterChangeLogsMenuItem().PerformClick();

				AssertEquals("Top level category should be visible", 1, registriesTreeView.Nodes.Count);
				var cat1Node = registriesTreeView.Nodes[0];
				AssertEquals("Only filtered sub category should be visible", 1, cat1Node.Nodes.Count);
				var subCatNode = cat1Node.Nodes[0];
				AssertEquals("Only filtered sub category should be visible", "Sub-Category 12", subCatNode.Text);
				AssertEquals("Only filtered registry item should be visible", 1, subCatNode.Nodes.Count);
				var regItemNode = subCatNode.Nodes[0];
				AssertEquals("Only filtered registry item should be visible", "TestItem1", regItemNode.Name);
				AssertEquals("Only filtered registry item should be visible", "ZZ-Item 1", regItemNode.Text);
			}
		}

		#endregion

		#region Implementation

		void SetupTestItem1()
		{
			SetupTestItem1("");
		}

		void SetupTestItem1(string defaultValue)
		{
			RegItem1 = new StringRegistryItem("TestItem", (NoResString)"Category", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.All, defaultValue);
			RegItem1.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
			Item1 = new RegistryItemTag(RegItem1);
			ItemNode1 = new TreeNode("Item");
			ItemNode1.Tag = Item1;
		}

		void SetupTestItem2()
		{
			RegItem2 = new StringRegistryItem("TestItem2", (NoResString)"Category2", (NoResString)"TestCaption2", (NoResString)"TestHint2", RegistryStorageFlags.All);
			RegItem2.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
			Item2 = new RegistryItemTag(RegItem2);
			ItemNode2 = new TreeNode("Item2");
			ItemNode2.Tag = Item2;
		}

		void SetupRegistriesTreeView(TreeView registriesTreeView, bool addSecondItem)
		{
			SetupRegistriesTreeView(registriesTreeView, addSecondItem, "");
		}

		void SetupRegistriesTreeViewWithDefaultValue(TreeView registriesTreeView, bool addSecondItem)
		{
			SetupRegistriesTreeView(registriesTreeView, addSecondItem, "DEFAULT");
		}

		void SetupRegistriesTreeView(TreeView registriesTreeView, bool addSecondItem, string firstItemDefaultValue)
		{
			registriesTreeView.Nodes.Clear();
			SetupTestItem1(firstItemDefaultValue);
			registriesTreeView.Nodes.Add(ItemNode1);
			if (addSecondItem)
			{
				SetupTestItem2();
				registriesTreeView.Nodes.Add(ItemNode2);
			}
		}

		TreeNode GetRegistryTreeNode(IRegistryItem item)
		{
			RegistryItemTag tag = new RegistryItemTag(item);
			TreeNode node = new TreeNode();
			node.Tag = tag;
			return node;
		}

		StringRegistryItem RegItem1;
		StringRegistryItem RegItem2;
		RegistryItemTag Item1;
		RegistryItemTag Item2;
		TreeNode ItemNode1;
		TreeNode ItemNode2;

		protected override void TearDown()
		{
			EnvProxy.SetHostedLocationForTest(null);
			base.TearDown();
		}

		#endregion

		public class StringRegistryDataTypeForTest : StringRegistryDataType
		{
			protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
				=> throw new RegistryValidationException("Validation fails.");

			protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;
		}

		public class DummyRegistryForm : ZForm, IRegistryForm
		{
			public bool ImportantMethodWasCalled { get; private set; }

			public void UpdateHasChanges() => ImportantMethodWasCalled = true;
		}

		#region RegistryFormForTest

		internal class RegistryFormForTest : RegistryForm
		{
			public RegistryFormForTest()
				: this(CreateEmptyRegistry())
			{
			}

			public RegistryFormForTest(IRegistryItem item)
				: this(CreateSingleItemRegistry(item))
			{
			}

			public RegistryFormForTest(IRegistry registry)
				: this(CreateProvider(registry))
			{ }

			public RegistryFormForTest(IRegistryProvider provider)
				: base(provider)
			{ }

			static IRegistry CreateSingleItemRegistry(IRegistryItem item)
			{
				var category = item.Categories.First().Split('/');
				var registryMock = new Mock<IRegistry>(MockBehavior.Strict);
				object lastKey = category.Length;

				registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(new[] { new RegistryCategoryRef(1, category[0]) });
				registryMock.Setup(r => r.GetSortedContent(It.IsAny<object>())).Returns(new Func<object, RegistryCategoryContent>(key =>
				{
					var categories = category.Skip((int)key).Select((cat, i) => new RegistryCategoryRef(i + 1, cat)).ToArray();
					var items = key.Equals(lastKey) ? new[] { item } : Array.Empty<IRegistryItem>();

					return new RegistryCategoryContent(categories, items);
				}));

				return registryMock.Object;
			}

			static IRegistry CreateEmptyRegistry()
			{
				var registryMock = new Mock<IRegistry>(MockBehavior.Strict);
				registryMock.Setup(r => r.GetSortedTopLevelCategories()).Returns(Array.Empty<RegistryCategoryRef>());
				return registryMock.Object;
			}

			static IRegistryProvider CreateProvider(IRegistry registry)
			{
				var registryFactoryMock = new Mock<IRegistryProvider>(MockBehavior.Strict);
				registryFactoryMock.Setup(f => f.CreateRegistry(It.IsAny<IRegistryItemVisibility>())).Returns(registry);
				return registryFactoryMock.Object;
			}

			public new LinkButton LinkButton
			{
				get { return base.LinkButton; }
			}

			public new BusinessObjectFactory Factory
			{
				get { return base.Factory; }
			}

			public bool PluginControlEnteredButLeaveEventNotFiredForTest
			{
				get { return PluginControlEnteredButLeaveEventNotFired; }
				set { PluginControlEnteredButLeaveEventNotFired = value; }
			}

			public bool HasChangesForTest
			{
				get { return HasChanges; }
				set { HasChanges = value; }
			}

			public MenuItem GetFilterChangeLogsMenuItem()
			{
				return base.filterChangeLogsMenuItem;
			}

			public new ZGroupBox PluginGroupBox
			{
				get { return base.PluginGroupBox; }
			}

			public ZButton GetCloseButton()
			{
				return CloseButton;
			}

			public MenuItem GetCloseMenuItem()
			{
				return CloseMenuItem;
			}

			public KSplitContainer GetTreeViewPanel()
			{
				return treeViewSplitContainer;
			}

			public MenuItem GetFindMenuItem()
			{
				return GetEditMenuItem("&Find");
			}

			public MenuItem GetCutMenuItem()
			{
				return GetEditMenuItem("Cut");
			}

			public MenuItem GetCopyMenuItem()
			{
				return GetEditMenuItem("Copy");
			}

			public MenuItem GetPasteMenuItem()
			{
				return GetEditMenuItem("Paste");
			}

			public MenuItem GetEditMenuItem(string menuItemText)
			{
				foreach (MenuItem item in NewEditMenuItem.MenuItems)
				{
					if (item.Text == menuItemText)
					{
						return item;
					}
				}
				return null;
			}

			public Hashtable GetColouredNodes()
			{
				return ColouredNodes;
			}

			public ZLabel GetValueLabel()
			{
				return ValueLabel;
			}

			public ZLabel GetFallbackMessageLabel()
			{
				return FallbackMessageLabel;
			}

			public ZLabel GetSecurityDeniedLabel()
			{
				return SecurityDeniedLabel;
			}

			public IList<RegistryItemTag> GetChangedItems()
			{
				return ChangedItems;
			}

			public ZButton GetSaveButton()
			{
				return SaveButton;
			}

			public MenuItem GetHideMenuItem()
			{
				return HideMenuItem;
			}

			public string GetDefaultHint()
			{
				return DefaultHint;
			}

			public ZLabel GetHintLabel()
			{
				return HintLabel;
			}

			public ZLabel GetSupportLabel()
			{
				return SupportLabel;
			}

			public ZPanel GetPluginPanel()
			{
				return PluginPanel;
			}

			public ZCheckBox GetOverrideCheckBox()
			{
				return OverrideCheckBox;
			}

			public Control GetPluginControl()
			{
				return PluginControl;
			}

			public ZUserControl GetHintUserControl()
			{
				return HintUserControl;
			}

			public ZTabControl GetRegistryItemTabControl()
			{
				return tabControlRegistryItem;
			}

			public RegistryItemsTreeView GetRegistriesTreeView()
			{
				return RegistriesTreeView;
			}

			public void SetRegistryTreeView(RegistryItemsTreeView registryItemsTreeView)
			{
				RegistriesTreeView = registryItemsTreeView;
			}

			public TreeView GetFallbackTreeView()
			{
				return FallbackTreeView;
			}

			public Hashtable GetSelectedFallbacksHash()
			{
				return SelectedFallbacksHash;
			}

			public TreeNode GetLastSelectedNodeKey()
			{
				return LastSelectedNodeKey;
			}

			public void FillRegistriesTreeViewForTest()
			{
				FillRegistriesTreeView();
			}

			public void FillFallbackTreeViewForTest(TreeNode registryNode)
			{
				FillFallbackTreeView(registryNode);
			}

			public void UpdateChangedItemsForTest(bool hasValue, object newValue)
			{
				UpdateChangedItems(hasValue, newValue);
			}

			public void SuspendSetSelectedFallbackForTest()
			{
				SuspendSetSelectedFallback();
			}

			public void ResumeSetSelectedFallbackForTest()
			{
				ResumeSetSelectedFallback();
			}

			public void SuspendUpdateOverrideCheckBoxForTest()
			{
				SuspendUpdateOverrideCheckBox();
			}

			public void ResumeUpdateOverrideCheckBoxForTest()
			{
				ResumeUpdateOverrideCheckBox();
			}

			public void UpdateColouredFallbacksForTest()
			{
				UpdateColouredFallbacks();
			}

			public void UnhighlightNodesForTest()
			{
				UnhighlightNodes();
			}

			public void SetupDefaultValueMessageForTest(RegistryStorageFlags fallbackLevel, bool isAmbiguous)
			{
				SetupDefaultValueMessage(fallbackLevel, isAmbiguous, false);
			}

			public void SetupDefaultValueMessageForTest(RegistryStorageFlags fallbackLevel, bool isAmbiguous, bool isMergedWithDefaultValue)
			{
				SetupDefaultValueMessage(fallbackLevel, isAmbiguous, isMergedWithDefaultValue);
			}

			public void ValidateRegistryItemForTest(bool suspendErrorProvider, bool isPreSaveValidation = false, bool validateAllRegistryItems = false)
			{
				ValidateRegistryItem(suspendErrorProvider, isPreSaveValidation, validateAllRegistryItems);
			}

			void RegistryForm_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
			{
				HasChanges |= e.ObjectJustWasChanged;
			}

			protected override DialogResult GetSaveDialogResult()
			{
				base.GetSaveDialogResult();
				return OnSaveResult;
			}
			public DialogResult OnSaveResult;

			protected override DialogResult GetCloseDialogResult()
			{
				base.GetCloseDialogResult();
				return OnClosingResult;
			}
			public DialogResult OnClosingResult;

			protected override void AddPluginControlLeaveEventHandler()
			{
				if (!DoNotAddPluginControlLeaveEventHandler)
				{
					base.AddPluginControlLeaveEventHandler();
				}
			}
			public bool DoNotAddPluginControlLeaveEventHandler;

			#region Exceptions

			public ZException ExceptionThrown;

			protected override void FallbackTreeView_AfterSelect(object sender, TreeViewEventArgs e)
			{
				try
				{
					base.FallbackTreeView_AfterSelect(sender, e);
				}
				catch (ZException ex)
				{
					ExceptionThrown = ex;
				}
			}

			protected override void RegistriesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
			{
				try
				{
					base.RegistriesTreeView_AfterSelect(sender, e);
				}
				catch (ZException ex)
				{
					ExceptionThrown = ex;
				}
			}

			protected override void PluginControl_Enter(object sender, EventArgs e)
			{
				try
				{
					base.PluginControl_Enter(sender, e);
				}
				catch (ZException ex)
				{
					ExceptionThrown = ex;
				}
			}

			#endregion

			public void UpdateRegistryItemValueFromPluginControlForTest()
			{
				UpdateRegistryItemValueFromPluginControl();
			}

			public IEnumerable<IRegistryItem> GetPerservedRegistryItemsForTest(IEnumerable<IRegistryItem> registryItems)
			{
				return GetPerservedRegistryItems(registryItems);
			}

			public void SetOldRegistryValuesForTest(IEnumerable<string> values)
			{
				OldRegistryValues = values;
			}

			protected override void PerformUpdateRegistryItemValueFromPluginControl()
			{
				if (!DoNotPerformUpdateRegistryItemValueFromPluginControl)
				{
					base.PerformUpdateRegistryItemValueFromPluginControl();
					PerformUpdateRegistryItemValueFromPluginControlCalled = true;
				}
			}
			public bool DoNotPerformUpdateRegistryItemValueFromPluginControl;
			public bool PerformUpdateRegistryItemValueFromPluginControlCalled;

			protected override void ValidateRegistryItem(bool suspendErrorProvider, bool isPreSaveValidation = false, bool validateAllRegistryItems = false)
			{
				if (!DoNotValidateRegistryItem)
				{
					base.ValidateRegistryItem(suspendErrorProvider, isPreSaveValidation, validateAllRegistryItems);
				}
			}
			public bool DoNotValidateRegistryItem;

			protected override void PerformFillFallbackTreeView(TreeNode registryNode)
			{
				base.PerformFillFallbackTreeView(registryNode);
				performFillFallbackTreeViewCalled = true;
			}
			public bool performFillFallbackTreeViewCalled;

			public new string EditableByHostedSupportOnlyText => base.EditableByHostedSupportOnlyText;
			public new string VisibleToSupportOnlyText => base.VisibleToSupportOnlyText;
			public new string VisibleToDevelopersOnlyText => base.VisibleToDevelopersOnlyText;
		}

		#endregion
	}
}
