using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTreeViewTest : TestCaseWithDummy
	{
		public void TestOpenAndCloseFindForm()
		{
			using (var treeView = new ZTreeView())
			{
				treeView.Nodes.Add("First");
				treeView.Nodes.Add("Second");

				using (var form = new ZForm())
				{
					form.Controls.Add(treeView);

					AssertNull("Should not create the form before it is needed", treeView.CurrentFindForm);

					treeView.ShowFindForm();

					AssertNotNull("Should be created", treeView.CurrentFindForm);
					AssertEquals("Should be shown", OpenFindForms.Single(), treeView.CurrentFindForm);
					AssertEquals("Parent should have been set", form, treeView.CurrentFindForm.Owner);

					treeView.CurrentFindForm.Close();

					AssertNoExceptionThrown("Should be able to reopen it", treeView.ShowFindForm);
					AssertEquals("Should be shown", OpenFindForms.Single(), treeView.CurrentFindForm);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		IEnumerable<TreeViewFindForm> OpenFindForms
		{
			get { return Application.OpenForms.OfType<TreeViewFindForm>(); }
		}

		public void TestTreeNodeImages()
		{
			using (Dummy.SuspendValidationTesting())
			{
				using (var testForm = new ZForm())
				using (var testTreeView = new ZTreeView())
				{
					testForm.Controls.Add(testTreeView);
					var testBizOTreeNode = new ZBusinessObjectTreeNode(Dummy, "Dummy Object", 1, 2, 3, 4);
					testTreeView.Nodes.Add(testBizOTreeNode);

					var testBizOTreeNode2 = new ZBusinessObjectTreeNode(Dummy, "Dummy Object", 1, 2, 3, 4);
					testBizOTreeNode.Nodes.Add(testBizOTreeNode2);

					testForm.Show();

					Dummy.Z0_DescriptionInfo.AddWarning("Sample Warning");
					AssertEquals("Warning Image Index should be selected", 4, testBizOTreeNode.ImageIndex);

					Dummy.Z0_DescriptionInfo.AddMessageError("Sample MessageError");
					AssertEquals("Message Error Image Index should be selected", 3, testBizOTreeNode.ImageIndex);

					Dummy.Z0_DescriptionInfo.AddError("Sample Error");
					AssertEquals("Error Image Index should be selected", 2, testBizOTreeNode.ImageIndex);

					Dummy.Z0_DescriptionInfo.ClearAllNotifications();
					AssertEquals("Normal Image Index should be selected", 1, testBizOTreeNode.ImageIndex);
				}

				AssertNoExceptionThrown(() => Dummy.Z0_DescriptionInfo.AddError("Sample Error"));
			}
		}

		public void TestTreeNodeImages_WhenNotificationsChangedSuspended()
		{
			var dummy = (DummyBusinessObject)base.Factory.New(TypeOfDummy);
			using (dummy.SuspendValidationTesting())
			{
				using (var testForm = new ZForm())
				using (var testTreeView = new ZTreeView())
				{
					testForm.Controls.Add(testTreeView);
					var testBizOTreeNode = new ZBusinessObjectTreeNode(dummy, "Dummy Object", 1, 2, 3, 4);
					testTreeView.Nodes.Add(testBizOTreeNode);

					testForm.Show();
					AssertEquals("Warning Image Index should be selected", 1, testBizOTreeNode.ImageIndex);
					AssertEquals(false, testTreeView.IsNotificationsChangedSuspended);

					using (testTreeView.SuspendNotificationsChanged())
					{
						AssertEquals(true, testTreeView.IsNotificationsChangedSuspended);

						dummy.Z0_DescriptionInfo.AddWarning("Sample Warning");
						AssertEquals("Index should not change.", 1, testBizOTreeNode.ImageIndex);

						dummy.Z0_DescriptionInfo.AddMessageError("Sample MessageError");
						AssertEquals("Index should not change.", 1, testBizOTreeNode.ImageIndex);

						dummy.Z0_DescriptionInfo.AddError("Sample Error");
						AssertEquals("Index should not change.", 1, testBizOTreeNode.ImageIndex);

						dummy.Z0_DescriptionInfo.ClearAllNotifications();
						AssertEquals("Index should not change.", 1, testBizOTreeNode.ImageIndex);
					}

					AssertEquals(false, testTreeView.IsNotificationsChangedSuspended);
				}

				AssertNoExceptionThrown(() => dummy.Z0_DescriptionInfo.AddError("Sample Error"));
			}
		}

		public void TestTreeNodeImageIndexesCanBeModified()
		{
			var testTreeView = new ZTreeView();

			using (var testForm = new ZForm())
			using (Dummy.SuspendValidationTesting())
			{
				testForm.Controls.Add(testTreeView);
				var testBizOTreeNode = new ZBusinessObjectTreeNode(Dummy, "Dummy Object", 1, 2, 3, 4);
				testTreeView.Nodes.Add(testBizOTreeNode);
				testForm.Show();

				testBizOTreeNode.NormalImageIndex = 5;
				testBizOTreeNode.ErrorImageIndex = 6;
				testBizOTreeNode.MessageErrorImageIndex = 7;
				testBizOTreeNode.WarningImageIndex = 8;

				Dummy.Z0_DescriptionInfo.AddWarning("Sample Warning");
				AssertEquals("Warning Image Index should be selected", 8, testBizOTreeNode.ImageIndex);

				Dummy.Z0_DescriptionInfo.AddMessageError("Sample MessageError");
				AssertEquals("Warning Image Index should be selected", 7, testBizOTreeNode.ImageIndex);

				Dummy.Z0_DescriptionInfo.AddError("Sample Error");
				AssertEquals("Warning Image Index should be selected", 6, testBizOTreeNode.ImageIndex);

				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				AssertEquals("Warning Image Index should be selected", 5, testBizOTreeNode.ImageIndex);
			}
		}

		public void TestHorizontalScrollDoesntAppearAtWrongTime()
		{
			using (var form = new ZForm())
			using (var treeView = new ZTreeView())
			{
				form.Size = ControlDpiScalingHelper.NewScaledSize(110, 110);
				treeView.Bounds = ControlDpiScalingHelper.NewScaledRectangle(5, 5, 100, 100);
				treeView.Nodes.Add(new TreeNode("splaty"));

				form.Controls.Add(treeView);
				form.Show();
				Application.DoEvents();

				AssertEquals(
					"The client size of the tree view should not be cluttered by a non-functioning horiztonal scroll bar",
					true, treeView.ClientSize.Height > ControlDpiScalingHelper.ScaleToCurrentDpiY(90));
			}
		}

		[DeveloperOnlyTest]
		public void TestCanCopyNodeTextByFormCopyMenuItem()
		{
			SafeClipboard.Clear();

			using (var form = new ZForm())
			using (var treeView = new ZTreeView())
			{
				var rootNodeText = "RootNode";
				treeView.Nodes.Add(new TreeNode(rootNodeText));

				form.Controls.Add(treeView);
				form.Show();
				Application.DoEvents();

				AssertEquals(rootNodeText, treeView.SelectedNode.Text);
				AssertEquals("", SafeClipboard.GetText());

				var copyMenuItem = form.FindMenuItem_ForTest("Copy");
				var action = new Action(() =>
				{
					copyMenuItem.PerformClick();
					Application.DoEvents();
				});
				action.Invoke();

				var text = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
				AssertEquals(rootNodeText, text);
			}
		}

		#region Test Notifications

		public void TestNotifications()
		{
			using (var form = new ZForm())
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var treeView = new ZTreeView())
			using (Dummy.SuspendValidationTesting())
			{
				var treeNode = new ZBusinessObjectTreeNode(Dummy, "Dummy Object", 1, 2, 3, 4);
				treeView.Nodes.Add(treeNode);
				tabPage.Controls.Add(treeView);
				tabControl.TabPages.Add(tabPage);
				form.Controls.Add(tabControl);
				form.Show();
				Application.DoEvents();

				AssertEquals("Warning Image Index should be selected", 1, treeNode.ImageIndex);
				AssertEquals(-1, tabPage.ImageIndex);

				Dummy.Z0_DescriptionInfo.AddWarning("Sample Warning");
				Application.DoEvents();
				AssertEquals("Warning Image Index should be selected", 4, treeNode.ImageIndex);
				AssertEquals(Icons.GetImageIndex(IconTypes.Warning), tabPage.ImageIndex);

				Dummy.Z0_DescriptionInfo.AddMessageError("Sample MessageError");
				Application.DoEvents();
				AssertEquals("Warning Image Index should be selected", 3, treeNode.ImageIndex);
				AssertEquals(Icons.GetImageIndex(IconTypes.MessageError), tabPage.ImageIndex);

				Dummy.Z0_DescriptionInfo.AddError("Sample Error");
				Application.DoEvents();
				AssertEquals("Warning Image Index should be selected", 2, treeNode.ImageIndex);
				AssertEquals(Icons.GetImageIndex(IconTypes.Error), tabPage.ImageIndex);

				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				Application.DoEvents();
				AssertEquals("Warning Image Index should be selected", 1, treeNode.ImageIndex);
				AssertEquals(-1, tabPage.ImageIndex);
			}
		}

		public void TestUpdateNotifications()
		{
			using (var form = new ZForm())
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var treeView = new ZTreeView())
			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_DescriptionInfo.AddError("Sample Error");

				var treeNode = new ZBusinessObjectTreeNode(Dummy, "Dummy Object", 1, 2, 3, 4);
				treeView.Nodes.Add(treeNode);
				tabPage.Controls.Add(treeView);
				tabControl.TabPages.Add(tabPage);
				form.Controls.Add(tabControl);
				form.Show();
				Application.DoEvents();

				AssertEquals("Warning Image Index should be selected", 1, treeNode.ImageIndex);
				AssertEquals(-1, tabPage.ImageIndex);

				treeView.UpdateNotifications();
				Application.DoEvents();

				AssertEquals("Warning Image Index should be selected", 2, treeNode.ImageIndex);
				AssertEquals(Icons.GetImageIndex(IconTypes.Error), tabPage.ImageIndex);
			}
		}

		#endregion
	}
}
