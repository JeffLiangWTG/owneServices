using System;
using System.Windows.Forms;

namespace Enterprise.DataTransfer.Native.TestClient
{
	/// <summary>
	/// Tool for test Native XML request
	/// </summary>
	public partial class TestClientForm : Form
	{
		public TestClientForm()
		{
			InitializeComponent();

			Text = Core.Constants.ProductName + " Native Schema Tool";

			resultsTextBox.MaxLength = Int32.MaxValue;
			requestTextBox.MaxLength = Int32.MaxValue;
		}

		#region Web Service Panel

		void RetrieveSampleButton_Click(object sender, EventArgs e)
		{
			resultsTextBox.Text = new ServiceClientWrapper(addressTextBox.Text).Retrieve(requestTextBox.Text);
		}

		void UpdateButton_Click(object sender, EventArgs e)
		{
			resultsTextBox.Text = new ServiceClientWrapper(addressTextBox.Text).Update(requestTextBox.Text);
		}

		#endregion
	}
}
