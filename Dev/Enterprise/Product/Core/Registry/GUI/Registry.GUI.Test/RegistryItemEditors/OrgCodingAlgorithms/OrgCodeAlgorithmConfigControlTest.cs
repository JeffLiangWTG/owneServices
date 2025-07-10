using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(OrgCodeAlgorithmConfigControl))]
	sealed class OrgCodeAlgorithmConfigControlTest : Testing.RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new OrgCodeAlgorithm();
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new OrgCodeAlgorithmConfigControl(OrgCodeAlgorithmType.Default);
		}

		public void TestGridsDoNotHaveRemoveAction()
		{
			using (OrgCodeAlgorithmConfigControl control = new OrgCodeAlgorithmConfigControl(OrgCodeAlgorithmType.Default))
			{
				AssertEquals("elementsGrid.RemoveAction", RemoveAction.NoRemovePossible, control.elementsGrid.RemoveAction);
				AssertEquals("orgTypesGrid.RemoveAction", RemoveAction.NoRemovePossible, control.orgTypesGrid.RemoveAction);
			}
		}

		public void TestRegenerate()
		{
			using (MockRegistryForm form = new MockRegistryForm())
			{
				form.Show();
				OrgCodeAlgorithmConfigControl control = new OrgCodeAlgorithmConfigControl(OrgCodeAlgorithmType.Default);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);

				form.HasChanges = true;
				control.regenerateButton.PerformClick();
				AssertEquals("A message saying that the form should be saved first should be shown.", Res.GetString("f28a2daa-e902-481e-8a9e-f42d4bd9c99e", "Please save all changes first before regenerating organization codes."), UnitTestUserNotification.Instance.LastMessage.Text);

				form.HasChanges = false;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				control.regenerateButton.PerformClick();
				AssertEquals("A message asking for confirmation should be shown.", Res.GetString("52027d7a-0f89-4380-92aa-7c4c3ff58d93", "All organization codes that this algorithm applies to will be regenerated, including manually set codes. The process may take a few minutes. Do you wish to continue?"), UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				control.regenerateButton.PerformClick();
				AssertEquals("A message indicating that the update has been completed should be shown.", "Organization codes have been updated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region MockRegistryForm

		class MockRegistryForm : RegistryForm
		{
			public new bool HasChanges
			{
				get { return base.HasChanges; }
				set { base.HasChanges = value; }
			}
		}

		#endregion
	}
}
