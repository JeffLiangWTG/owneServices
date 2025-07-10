using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.GUI.Testing
{
	public class UPSUserControlTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			using (UPSUserControl uPSUserControl = new UPSUserControl())
			{
				AssertNotNull(uPSUserControl);
			}
		}

		public void TestRemoveActionOnGrid()
		{
			using (UPSUserControl uPSUserControl = new UPSUserControl())
			{
				AssertEquals(RemoveAction.NoRemovePossible, uPSUserControl.RelatedUPECusHAWBsGrid.RemoveAction);
			}
		}

		public void TestIsShipperMatchingError_Click()
		{
			UPEJobDeclaration declaration = Factory.New<UPEJobDeclaration>();
			using (UPSUserControlTestClass uPSUserControl = new UPSUserControlTestClass())
			{
				uPSUserControl.JobDeclaration = declaration;
				uPSUserControl.Show();
				Assert("PreCondition: Should be unchecked", !uPSUserControl.ShipperMatchingErrorCheckBox.Checked);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				uPSUserControl.ShipperMatchingError_Clicked(this, EventArgs.Empty);
				AssertEquals("CheckBox should be Unchecked", CheckState.Unchecked, uPSUserControl.ShipperMatchingErrorCheckBox.CheckState);
				Assert("should be false", !declaration.CurrentQueue.P4_CustomFlag3);
				Assert("box should be enabled, i.e. not greyed out", uPSUserControl.ShipperMatchingErrorCheckBox.Enabled);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				uPSUserControl.ShipperMatchingError_Clicked(this, EventArgs.Empty);
				AssertEquals("CheckBox should be checked", CheckState.Checked, uPSUserControl.ShipperMatchingErrorCheckBox.CheckState);
				Assert("should be true", declaration.CurrentQueue.P4_CustomFlag3);
				Assert("CheckBox should be disabled, i.e. greyed out", !uPSUserControl.ShipperMatchingErrorCheckBox.Enabled);
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		void ShipperMatchingErrorCheckBox_Click(object sender, EventArgs e)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		class UPSUserControlTestClass : UPSUserControl
		{
			public new void ShipperMatchingError_Clicked(object sender, EventArgs e)
			{
				base.ShipperMatchingError_Clicked(sender, e);
			}
		}
	}
}
