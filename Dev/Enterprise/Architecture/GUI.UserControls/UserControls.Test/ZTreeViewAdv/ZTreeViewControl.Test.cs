using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

namespace Enterprise.ZArchitecture.GUI.Tests
{
	sealed class ZTreeViewControlTest : TestCaseWithFactory
	{
		public void TestDetach()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var model = new DummyTreeModel(Factory, new[] { dummy });
			Factory.Save();

			var childNode = model.RootNodes.Single().AddNewChild(Factory.NewWithValidTestData<DummyBusinessObject>());
			var grandChildNode = childNode.AddNewChild(Factory.NewWithValidTestData<DummyBusinessObject>());
			Factory.Save();

			using (var form = new ZForm(model))
			using (var control = new DummyTreeViewControl())
			{
				control.NameOfATreeElement = Res.GetData("Dummy", "Dummy");
				control.NameOfTreeElementsPlural = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("Dummies", "Dummies");
				form.Controls.Add(control);
				form.Show();

				control.DetachToolStripButton.PerformClick();
				AssertEquals("Detach Dummy", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Please select an item in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				((DummyTreeModelView)control.ModelView).SecurityCheckpointForEditOverride = Env.Security.Organisation;
				var childNodeAdv = control.Tree.FindNodeByTag(childNode);
				control.Tree.SelectedNode = childNodeAdv;
				Env.Security.Organisation.IsAllowed = false;
				control.DetachToolStripButton.PerformClick();
				AssertEquals("Access Denied: Organization", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(Env.Security.Organisation.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.Organisation.IsAllowed = true;
				UnitTestUserNotification.Instance.AddOKAnswer();
				control.DetachToolStripButton.PerformClick();
				AssertEquals("Detach Dummy", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Detach the selected Dummies?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertCollectionNotContains(childNodeAdv, control.Tree.AllNodes);
			}
		}

		#region ReadOnly

		public void TestSetButtonsToReadOnlyIfDisplayModeReadOnly()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var model = new DummyTreeModel(Factory, new[] { dummy });

			using (var form = new ZForm(model))
			using (var control = new DummyTreeViewControl())
			{
				form.Controls.Add(control);
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				AssertEquals(false, control.AttachToolStripButton.Enabled);
				AssertEquals(false, control.EditToolStripButton.Enabled);
				AssertEquals(false, control.NewToolStripButton.Enabled);
				AssertEquals(false, control.DetachToolStripButton.Enabled);
			}
		}

		public void TestSetTreeToReadOnlyIfDisplayModeReadOnly()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var model = new DummyTreeModel(Factory, new[] { dummy });

			using (var form = new ZForm(model))
			using (var control = new DummyTreeViewControl())
			{
				form.Controls.Add(control);
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				AssertEquals(true, control.Tree.Enabled);
				AssertEquals(true, control.Tree.ReadOnly);
			}
		}

		#endregion

		class DummyTreeViewControl : ZTreeViewControl
		{
			protected override IZTreeModelView GetNewTreeModelView()
			{
				return new DummyTreeModelView((DummyTreeModel)CurrentDataItem);
			}
		}
	}
}
