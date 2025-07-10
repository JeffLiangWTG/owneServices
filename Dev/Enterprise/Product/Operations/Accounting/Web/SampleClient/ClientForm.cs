using System;
using System.Data;
using System.IO;
using System.ServiceModel;
using System.Windows.Forms;
using System.Xml.Serialization;
using AccountingWebServiceSampleClient.CreditLimitServiceReference;

namespace AccountingWebServiceSampleClient
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "This is a sample client application, not a part of ediEnterprise")]
	public partial class ClientForm : Form
	{
		public ClientForm()
		{
			InitializeComponent();
		}

		void RequestXMLButton_Click(object sender, EventArgs e)
		{
			try
			{
				ResponseTextBox.Text = GetCreditLimitAndBalanceDetailsRadioButton.Checked ?
					GetCreditLimitAndBalanceDetails() : CheckTransactionPaymentStatus();
			}
			catch (Exception ex) // CriticalExceptionIsHandled Reason = Sample for customer use.
			{
				ResponseTextBox.Text = string.Format("Exception of {0} type was caught\r\nException Message: {1}", ex.GetType(), ex.Message);
			}
		}

		void MethodChanged(object sender, EventArgs e)
		{
			OverdueAgingPeriodNumericUpDown.Enabled = BalanceOverdueAgingOptionComboBox.Enabled = GetCreditLimitAndBalanceDetailsRadioButton.Checked;
			TransactionTypeComboBox.Enabled = TransactionNumberTextBox.Enabled = CheckTransactionPaymentStatusRadioButton.Checked;
			EnabledOrDisableControlForLedgerType();
		}

		string GetCreditLimitAndBalanceDetails()
		{
			string result = "";
			CreditLimitServiceSoapClient serviceClient = GetServiceClient();
			SecuritySOAPHeader securityHeader = new SecuritySOAPHeader() { UserName = UserNameTextBox.Text, Password = PasswordTextBox.Text };
			string balanceOverdueAgingOption = BalanceOverdueAgingOptionComboBox.SelectedValue == null ? string.Empty : (string)BalanceOverdueAgingOptionComboBox.SelectedValue;
			CreditLimitAndBalanceRequest request = new CreditLimitAndBalanceRequest()
			{
				OrgCode = ClientCodeTextBox.Text,
				CompanyCode = CompanyCodeTextBox.Text,
				AccLedger = LedgerTypeComboBox.Text,
				OverdueAgingPeriod = int.Parse(OverdueAgingPeriodNumericUpDown.Value.ToString()),
				BalanceOverdueAgingOption = balanceOverdueAgingOption
			};
			CreditLimitAndBalanceResponse response = serviceClient.GetCreditLimitAndBalanceDetails(securityHeader, request);

			XmlSerializer serializer = new XmlSerializer(typeof(CreditLimitAndBalanceResponse));
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, response);
				result = writer.GetStringBuilder().ToString();
			}
			return result;
		}

		string CheckTransactionPaymentStatus()
		{
			string result = "";
			CreditLimitServiceSoapClient serviceClient = GetServiceClient();
			SecuritySOAPHeader securityHeader = new SecuritySOAPHeader() { UserName = UserNameTextBox.Text, Password = PasswordTextBox.Text };

			TransactionPaymentStatusRequest request = new TransactionPaymentStatusRequest()
			{
				OrgCode = ClientCodeTextBox.Text,
				CompanyCode = CompanyCodeTextBox.Text,
				AccLedger = LedgerTypeComboBox.Text,
				TransactionType = TransactionTypeComboBox.Text,
				TransactionNumber = TransactionNumberTextBox.Text,
				JobTransactionNumber = JobTransactionNumberTextBox.Text,
				InternalReference = InternalReferenceTextBox.Text
			};

			TransactionPaymentStatusResponse response = serviceClient.CheckTransactionPaymentStatus(securityHeader, request);

			XmlSerializer serializer = new XmlSerializer(typeof(TransactionPaymentStatusResponse));
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, response);
				result = writer.GetStringBuilder().ToString();
			}
			return result;
		}

		CreditLimitServiceSoapClient GetServiceClient()
		{
			CreditLimitServiceSoapClient result = null;
			if (urlRadioButton.Checked)
			{
				var url = new Uri(urlTextBox.Text);
				var securityMode = url.Scheme == Uri.UriSchemeHttps ? BasicHttpSecurityMode.Transport
								 : url.Scheme == Uri.UriSchemeHttp ? BasicHttpSecurityMode.None
								 : throw new InvalidOperationException("Unsupported URL for web service. Only HTTP and HTTPS are supported.");
				var binding = new BasicHttpBinding(securityMode);
				var remoteAddress = new EndpointAddress(url);
				result = new CreditLimitServiceSoapClient(binding, remoteAddress);
			}
			else
			{
				result = new CreditLimitServiceSoapClient("CreditLimitServiceSoap");
			}
			return result;
		}

		void EnabledOrDisableControlForLedgerType()
		{
			if (CheckTransactionPaymentStatusRadioButton.Checked)
			{
				JobTransactionNumberTextBox.Enabled = LedgerTypeComboBox.SelectedItem?.ToString() == "AR";
				InternalReferenceTextBox.Enabled = LedgerTypeComboBox.SelectedItem?.ToString() == "AP";
			}
			else
			{
				JobTransactionNumberTextBox.Enabled = false;
				InternalReferenceTextBox.Enabled = false;
			}
		}

		void LedgerTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnabledOrDisableControlForLedgerType();
		}

		void ClientForm_Load(object sender, EventArgs e)
		{
			DataTable table = new DataTable();
			table.Columns.Add("Name", typeof(string));
			table.Columns.Add("Value", typeof(string));

			DataRow row = table.NewRow();
			row["Name"] = "Balance";
			row["Value"] = "BAL";
			table.Rows.Add(row);
			row = table.NewRow();
			row["Name"] = "Revenue";
			row["Value"] = "REV";
			table.Rows.Add(row);
			row = table.NewRow();
			row["Name"] = "Overdue";
			row["Value"] = "OVR";
			table.Rows.Add(row);
			row = table.NewRow();
			row["Name"] = "Aging";
			row["Value"] = "AGE";
			table.Rows.Add(row);

			BalanceOverdueAgingOptionComboBox.DataSource = table;
			BalanceOverdueAgingOptionComboBox.DisplayMember = "Name";
			BalanceOverdueAgingOptionComboBox.ValueMember = "Value";
		}

		void urlRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			urlTextBox.Enabled = urlRadioButton.Checked;
		}
	}
}
