using System.Windows.Forms;
using Enterprise.AlwaysOn.Setup;
using Enterprise.AlwaysOn.Setup.GUI;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	public class ListenersControlTest : TestCase
	{
		public void TestFormBashing()
		{
			var addListenerGroupBoxControl = listenersControl.Controls.Find("addListenerGroupBox", false)[0];
			var nameTextBoxControl = addListenerGroupBoxControl.Controls.Find("nameTextBox", false)[0];
			var confirmButtonControl = addListenerGroupBoxControl.Controls.Find("confirmButton", false)[0];

			nameTextBoxControl.Select();
			nameTextBoxControl.Text = "ThisListenerNameIsLongerThan15Characters";

			Assert(!confirmButtonControl.Enabled);

			listenersControl.validateListenerIpsButton_Click();
			Assert(!confirmButtonControl.Enabled);

			var newRow = new DataGridViewRow();
			newRow.CreateCells(listenersControl.ipAddressesDataGridView);
			newRow.CreateCells(listenersControl.ipAddressesDataGridView);
			newRow.Cells[0].Value = "ThisIsNotAValidIP";
			newRow.Cells[1].Value = "ThisIsNotAValidMask";
			listenersControl.ipAddressesDataGridView.Rows.Add(newRow);
			listenersControl.validateListenerIpsButton_Click();
			Assert(!confirmButtonControl.Enabled);
		}

		protected override void SetUp()
		{
			base.SetUp();

			listenersControl = new ListenersControlForTesting();
			testForm = new FormForTesting();
			testForm.Controls.Add(this.listenersControl);
			listenersControl.HookUiControlForm(testForm);
		}

		FormForTesting testForm;
		ListenersControlForTesting listenersControl;
	}

	class ListenersControlForTesting : ListenersControl
	{
		public void MockAvailabilityGroup(IAvailabilityGroup availabilityGroup)
		{
			AvailabilityGroup = availabilityGroup;
		}

		public void MockDbServer(IPrimaryServerInstance primaryServerInstance)
		{
			DbServer = primaryServerInstance;
		}

		public void validateListenerIpsButton_Click()
		{
			base.validateListenerIpsButton_Click(null, null);
		}

		new internal DataGridView ipAddressesDataGridView => base.ipAddressesDataGridView;
	}
}
