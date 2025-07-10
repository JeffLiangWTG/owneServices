using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Enterprise.FaxRouter.Processor;

namespace Enterprise.FaxRouter.Manager
{
	public partial class FaxManagementUserControl : UserControl
	{
		int APageTracker;

		public FaxManagementUserControl()
		{
			InitializeComponent();
			SetGridLookAndFeel();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public void RefreshGrid()
		{
			PrevButton.Enabled = false;
			FaxManagementDataGrid.BeginInit();
			EventProgress aProgressBar = new EventProgress();
			aProgressBar.ActionMessageLabel.Text = "Loading Fax Manager.... ";
			aProgressBar.Show();
			Application.DoEvents();

			if (TrackingNumberTextBox == null || string.IsNullOrWhiteSpace(TrackingNumberTextBox.Text))
			{
				FaxManagementDataGrid.DataSource = FaxManager.OrderedDataSet(FaxDataModule.GetFaxManagerDataTable());
			}
			else
			{
				String aTrackingNumber = TrackingNumberTextBox.Text.Trim();
				NextButton.Enabled = false;
				FaxManagementDataGrid.DataSource = FaxManager.OrderedDataSet(FaxDataModule.GetFaxManagerDataTable(aTrackingNumber));
			}

			Application.DoEvents();
			aProgressBar.Close();
			FaxManagementDataGrid.EndInit();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void SearchButton_Click(object sender, EventArgs e)
		{
			PrevButton.Enabled = false;
			NextButton.Enabled = false;

			FaxManagementDataGrid.BeginInit();
			if (TrackingNumberTextBox != null && !string.IsNullOrWhiteSpace(TrackingNumberTextBox.Text))
			{
				String aTrackingNumber = TrackingNumberTextBox.Text.Trim();
				FaxManagementDataGrid.DataSource = FaxManager.OrderedDataSet(FaxDataModule.GetFaxManagerDataTable(aTrackingNumber));
			}
			else
			{
				FaxManagementDataGrid.DataSource = null;
			}
			FaxManagementDataGrid.EndInit();
			Application.DoEvents();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void SetGridLookAndFeel()
		{
			DataGridTableStyle faxManagementDataGridTableStyle = new DataGridTableStyle();

			faxManagementDataGridTableStyle.MappingName = "FaxManagementJob";
			faxManagementDataGridTableStyle.AlternatingBackColor = Color.LightSteelBlue;

			DataGridColumnStyle chargeCodeStyle = new DataGridTextBoxColumn();
			chargeCodeStyle.MappingName = "ChargeCode";
			chargeCodeStyle.HeaderText = "Housebill";
			chargeCodeStyle.Width = 120;

			DataGridColumnStyle faxJobIdStyle = new DataGridTextBoxColumn();
			faxJobIdStyle.MappingName = "FaxJobId";
			faxJobIdStyle.HeaderText = "Fax Job Id";
			faxJobIdStyle.Width = 150;

			DataGridColumnStyle receivedDateTimeDateStyle = new DataGridTextBoxColumn();
			receivedDateTimeDateStyle.MappingName = "ReceivedDateTime";
			receivedDateTimeDateStyle.HeaderText = "Rcv Date";
			receivedDateTimeDateStyle.Width = 150;

			DataGridColumnStyle pageCount = new DataGridTextBoxColumn();
			pageCount.MappingName = "PageCount";
			pageCount.HeaderText = "Pages";
			pageCount.Width = 50;

			DataGridColumnStyle faxRecipientId = new DataGridTextBoxColumn();
			faxRecipientId.MappingName = "FaxRecipientId";
			faxRecipientId.HeaderText = "Fax Rcpt Id";
			faxRecipientId.Width = 150;

			DataGridColumnStyle faxSentDateTime = new DataGridTextBoxColumn();
			faxSentDateTime.MappingName = "FaxSentDateTime";
			faxSentDateTime.HeaderText = "Sent Date";
			faxSentDateTime.Width = 140;

			DataGridColumnStyle faxAttention = new DataGridTextBoxColumn();
			faxAttention.MappingName = "FaxAttention";
			faxAttention.HeaderText = "Attn";
			faxAttention.Width = 100;

			DataGridColumnStyle faxNumber = new DataGridTextBoxColumn();
			faxNumber.MappingName = "FaxNumber";
			faxNumber.HeaderText = "Fax No";
			faxNumber.Width = 100;

			DataGridColumnStyle companyName = new DataGridTextBoxColumn();
			companyName.MappingName = "CompanyName";
			companyName.HeaderText = "Company";
			companyName.Width = 100;

			DataGridColumnStyle ackDateTime = new DataGridTextBoxColumn();
			ackDateTime.MappingName = "AckDateTime";
			ackDateTime.HeaderText = "Ack Date";
			ackDateTime.Width = 140;

			DataGridColumnStyle ackSuccess = new DataGridTextBoxColumn();
			ackSuccess.MappingName = "AckSuccess";
			ackSuccess.HeaderText = "Ack Success";
			ackSuccess.Width = 90;

			faxManagementDataGridTableStyle.GridColumnStyles.Add(chargeCodeStyle);
			faxManagementDataGridTableStyle.GridColumnStyles.Add(receivedDateTimeDateStyle);
			faxManagementDataGridTableStyle.GridColumnStyles.Add(faxAttention);
			faxManagementDataGridTableStyle.GridColumnStyles.Add(faxNumber);
			faxManagementDataGridTableStyle.GridColumnStyles.Add(companyName);
			faxManagementDataGridTableStyle.GridColumnStyles.Add(pageCount);
			faxManagementDataGridTableStyle.GridColumnStyles.Add(faxSentDateTime);
			faxManagementDataGridTableStyle.GridColumnStyles.Add(ackDateTime);
			faxManagementDataGridTableStyle.GridColumnStyles.Add(ackSuccess);
			faxManagementDataGridTableStyle.GridColumnStyles.Add(faxJobIdStyle);
			faxManagementDataGridTableStyle.GridColumnStyles.Add(faxRecipientId);

			FaxManagementDataGrid.TableStyles.Add(faxManagementDataGridTableStyle);

			this.FaxManagementDataGrid.DoubleClick += new EventHandler(this.FaxManagementDataGrid_DoubleClick);
		}

		void FaxManagementDataGrid_DoubleClick(object sender, EventArgs e)
		{
			var selectedRows = WinGridUtils.GetSelectedRowsId(FaxManagementDataGrid, "FaxJobId");

			if (selectedRows.Count == 0)
			{
				return;
			}

			var firstFaxJobId = selectedRows[0] as string;
			SaveFaxFile(FaxManager.ExtractPostedFaxFilePath(new Guid(firstFaxJobId)));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:Don't use System.Windows.Forms dialogs", Justification = "Fax Router doesn't run in RDP environment")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void SaveFaxFile(string tempFilePath)
		{
			var saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "TIFF Image|*.tiff";
			saveFileDialog.Title = "Save Fax File";
			saveFileDialog.ShowDialog();
			if (!string.IsNullOrEmpty(saveFileDialog.FileName))
			{
				File.Copy(tempFilePath, saveFileDialog.FileName);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void ShowTop10Button_Click(object sender, EventArgs e)
		{
			PrevButton.Enabled = false;
			NextButton.Enabled = true;

			AMaxDateTimeList = new ArrayList();
			APageTracker = 0;

			FaxManagementDataGrid.BeginInit();

			FaxManagementDataGrid.DataSource = FaxManager.OrderedDataSet(FaxDataModule.GetFaxManagerDataTable());

			FaxManagementDataGrid.EndInit();
			Application.DoEvents();
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			TrackingNumberTextBox.Text = "";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void NextButton_Click(object sender, EventArgs e)
		{
			String aMinDateTime = FaxManager.GetLastReceivedDateTimeOnGrid(FaxManagementDataGrid, WinGridUtils.GridColumnIndex(FaxManagementDataGrid, "FaxManagementJob", "ReceivedDateTime"));
			String aMaxDateTime = FaxManager.GetFirstReceivedDateTimeOnGrid(FaxManagementDataGrid, WinGridUtils.GridColumnIndex(FaxManagementDataGrid, "FaxManagementJob", "ReceivedDateTime"));

			AMaxDateTimeList.Add(aMaxDateTime);
			APageTracker++;

			FaxManagementDataGrid.BeginInit();

			FaxManagementDataGrid.DataSource = FaxManager.OrderedDataSet(FaxDataModule.GetNextFaxManagerDataTable(aMinDateTime));

			FaxManagementDataGrid.EndInit();
			PrevButton.Enabled = true;
			Application.DoEvents();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void PrevButton_Click(object sender, EventArgs e)
		{
			String aMaxDateTime = FaxManager.GetFirstReceivedDateTimeOnGrid(FaxManagementDataGrid, WinGridUtils.GridColumnIndex(FaxManagementDataGrid, "FaxManagementJob", "ReceivedDateTime"));

			FaxManagementDataGrid.BeginInit();

			FaxManagementDataGrid.DataSource = FaxManager.OrderedDataSet(FaxDataModule.GetNextFaxManagerDataTable(aMaxDateTime, AMaxDateTimeList[--APageTracker].ToString()));

			FaxManagementDataGrid.EndInit();
			Application.DoEvents();

			AMaxDateTimeList.RemoveAt(APageTracker);
			if (APageTracker == 0)
			{
				PrevButton.Enabled = false;
			}
		}

		ArrayList AMaxDateTimeList = new ArrayList();
	}
}
