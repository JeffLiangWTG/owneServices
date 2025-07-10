using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using InvoicePaymentSampleClient.InvoicePayServiceRef;

namespace InvoicePaymentSampleClient
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "This is a sample client application, not a part of ediEnterprise")]
	public partial class ClientForm : Form
	{
		public ClientForm()
		{
			InitializeComponent();
			textBoxUrl.Text = System.Configuration.ConfigurationManager.AppSettings.Get("ServiceUrl");
		}

		void RequestXMLButton_Click(object sender, EventArgs e)
		{
			try
			{
				ResponseTextBox.Text = GetInvoicePaymentResponse();
			}
			catch (Exception ex) // CriticalExceptionIsHandled Reason = Sample for customer use.
			{
				ResponseTextBox.Text = string.Format("Exception of {0} type was caught\r\nException Message: {1}", ex.GetType(), ex.Message);
			}
		}

		string GetInvoicePaymentResponse()
		{
			string result = "";
			UpdateInvoicePaymentDetailsServiceSoapClient serviceClient = new UpdateInvoicePaymentDetailsServiceSoapClient("UpdateInvoicePaymentDetailsServiceSoap", textBoxUrl.Text);
			SecuritySOAPHeader securityHeader = new SecuritySOAPHeader() { UserName = UserNameTextBox.Text, Password = PasswordTextBox.Text };
			UpdateInvoicePaymentDetailsRequest request = new UpdateInvoicePaymentDetailsRequest()
			{
				OrgCode = ClientCodeTextBox.Text,
				CompanyCode = CompanyCodeTextBox.Text,
				AccLedger = LedgerTypeComboBox.Text,
				TransactionType = TransactionTypeComboBox.Text,
				AmountPaidInCompanyCurrency = decimal.Parse(PaymentAmontTextBox.Text),
			};
			if (!string.IsNullOrEmpty(TransactionNumberTextBox.Text))
			{
				request.TransactionNumber = TransactionNumberTextBox.Text.Trim();
			}

			if (!string.IsNullOrEmpty(JobTransactionNumberTextBox.Text))
			{
				request.JobTransactionNumber = JobTransactionNumberTextBox.Text.Trim();
			}

			if (!string.IsNullOrEmpty(InternalReferenceTextBox.Text))
			{
				request.InternalReference = InternalReferenceTextBox.Text.Trim();
			}

			if (!string.IsNullOrEmpty(PaymentReferenceTextBox.Text))
			{
				request.PaymentReference = PaymentReferenceTextBox.Text.Trim();
			}

			if (PaymentDateSetCheckBox.Checked)
			{
				request.PaymentDate = PaymentDateTimePicker.Value;
			}

			UpdateInvoicePaymentDetailsResponse response = serviceClient.UpdateInvoicePaymentDetails(securityHeader, request);

			XmlSerializer serializer = new XmlSerializer(typeof(UpdateInvoicePaymentDetailsResponse));
			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, response);
				result = writer.GetStringBuilder().ToString();
			}
			return result;
		}

		void PaymentDateSetCheckBox_CheckStateChanged(object sender, EventArgs e)
		{
			PaymentDateTimePicker.Enabled = PaymentDateSetCheckBox.Checked;
		}

		void LedgerTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			JobTransactionNumberTextBox.Enabled = LedgerTypeComboBox.SelectedItem.ToString() == "AR";
			InternalReferenceTextBox.Enabled = LedgerTypeComboBox.SelectedItem.ToString() == "AP";
		}
	}
}
