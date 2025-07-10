using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DataConverters.Accounting.Cyber2;
using Enterprise.DataConverters.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataConverters.Accounting
{
	public partial class MainForm : ZChildForm
	{
		public MainForm()
			: this(new Converter())
		{
		}

		public MainForm(Converter converter)
		{
			fConverter = converter;
			ControlDpiScalingHelper.SetWidth(this, 800, true);
			TotalAmountTextLabel.Text = Res.GetString("MainForm|TotalAmount", "Total Amount({0}):", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
		}

		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		public static bool IsShown = false;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
#if !WINZOR
			Application.Run(new MainForm(new Converter()));
#endif
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (fConverter != null)
				{
					fConverter.ProgressChanged -= new EventHandler(ImportProgress);
					fConverter.ErrorOccurred -= new EventHandler(Converter_ErrorOccurred);
				}
				if (OpenFileDialog != null)
				{
					OpenFileDialog.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#region IZForm Members

		public override ODisplayMode DisplayMode
		{
			get { return ODisplayMode.Undefined; }
			set { }
		}

		#endregion

		#region Implementation

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			IsShown = true;
			OutPutListTextBox.ReadOnly = true;
			PostDateDateEdit.Text = ZDateTime.Now.ToShortDateString();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			IsShown = false;
		}

		void ProcessDataImport(ZDateTime postDate)
		{
			fConverter.ProgressChanged += new EventHandler(ImportProgress);
			fConverter.ErrorOccurred += new EventHandler(Converter_ErrorOccurred);
			fConverter.ImportCompleted += new EventHandler(Converter_ImportCompleted);

			OutPutListTextBox.Text = "";

			CopyOutputToClipboardButton.Enabled = false;
			CopyOutputToClipboardButton.Visible = false;
			MainStatusBar.Text = Res.GetString("c067b9bb-52b0-4d3e-840c-f308f915a084", "Importing...Please wait...");
			string fileName = PathTextBox.Text.Trim();
			if (string.IsNullOrEmpty(fileName))
			{
				Globals.Message.ShowError(Res.GetString("45199a50-09e2-4bcd-851b-8d2a249278ef", "Please enter the file location"));
			}
			else
			{
				SetButtons(true);
				using (ZOpenFileDialog.ForceLocalFile(ref fileName))
				{
					fConverter.ImportFile(fileName, DebtorsRadioButton.Checked, postDate);
				}
			}
		}

		void ProcessInterbase()
		{
			Logger.UpdateCounters += new EventHandler(Logger_OnUpdateCounters);
			Logger.UpdateLogText += new EventHandler(Logger_OnUpdateLogText);
			try
			{
				ImportData();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				DisplayGeneralErrorMessage(ex.Message);
			}
			Logger.UpdateCounters -= new EventHandler(Logger_OnUpdateCounters);
			Logger.UpdateLogText -= new EventHandler(Logger_OnUpdateLogText);
		}

		protected void ImportData()
		{
			ZString connection = ConnectionTextBox.Text.Trim();
			ZString accountType = (DebtorsRadioButton.Checked) ? "D" : "C";

			InvoiceDataImporter invoiceImporter = new InvoiceDataImporter(Logger, connection, true, accountType);
			invoiceImporter.Import();

			ReceiptDataImporter receiptImporter = new ReceiptDataImporter(Logger, connection, true, accountType);
			receiptImporter.Import();
		}

		void SetButtons(bool isEnabled)
		{
			PathTextBox.Enabled = !isEnabled;
			BrowseFileButton.Enabled = !isEnabled;
			CreditorsRadioButton.Enabled = !isEnabled;
			DebtorsRadioButton.Enabled = !isEnabled;
			StartConversionButton.Enabled = !isEnabled;
			InterbaseCheckBox.Enabled = !isEnabled;
		}

		void DisplayGeneralErrorMessage(string detailedExceptionMessage)
		{
			Globals.Message.ShowError(Res.GetString("ee776400-4996-41f1-a62c-7d4edaa43f55", "Cannot import data! The detailed exception is:") + System.Environment.NewLine + System.Environment.NewLine + detailedExceptionMessage);
		}

		#region Form Events

		internal void StartConversionButton_Click(object sender, EventArgs e)
		{
			if (InterbaseCheckBox.Checked)
			{
				Logger = new ProgressLogger(OleDBConnectionTypes.Interbase);
				ProcessInterbase();
			}
			else
			{
				Logger = new ProgressLogger();
				ZDateTime postDate;
				ZDateTime.TryParseISO8601Date(PostDateDateEdit.Text, out postDate);

				var resultMessage = fConverter.ValidateAccGLHeader(DebtorsRadioButton.Checked);
				if (!resultMessage.IsEmpty)
				{
					Globals.Message.ShowError(resultMessage);
					PostDateDateEdit.Text = "";
				}
				else
				{
					ProcessDataImport(postDate);
				}
			}
		}

		internal void ClipboardCopyButton_Click(object sender, EventArgs e)
		{
			DataObject data = new DataObject();
			data.SetData(DataFormats.Text, OutPutListTextBox.Text);
			if (!SafeClipboard.SetDataObject(data, true))
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
		}

		void ConnectionButton_Click(object sender, EventArgs e)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyImporter importer = new DummyImporter(factory, PathTextBox.Text);

			using (ConnectionForm connect = new ConnectionForm(importer))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(connect) == DialogResult.OK)
				{
					ConnectionTextBox.Text = importer.ConnectionText;
				}
			}
		}

		void InterbaseCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool interbaseOnly = InterbaseCheckBox.Checked;
			ConnectionTextBox.Enabled = interbaseOnly;
			ConnectionButton.Enabled = interbaseOnly;
		}

		void BrowseFileButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog.Filter = "TXT files (*.txt)|*.txt|CSV files (*.csv)|*.csv|All files (*.*)|*.*";
			OpenFileDialog.FilterIndex = 2;
			OpenFileDialog.RestoreDirectory = true;

			if (OpenFileDialog.ShowDialog() == DialogResult.OK)
			{
				PathTextBox.Text = OpenFileDialog.UnmappedFileName;
				PathTextBox.Focus();
				PathTextBox.SelectionStart = PathTextBox.Text.Length;
				PathTextBox.SelectionLength = 0;
			}
		}

		internal void SaveButton_Click(object sender, EventArgs e)
		{
			if (!ValidatorSavePreCondition.Validate(out var errorMessage))
			{
				Globals.Message.ShowError(errorMessage);
				MainStatusBar.Text = errorMessage;
				SaveButton.Enabled = false;

				AppendOutPutListText(errorMessage);
			}
			else
			{
				try
				{
					if (Globals.Message.Show(Res.GetString("78aad542-d516-4cde-ba76-eb94e826607e", "Are you sure you want to save the imported data?"), Res.GetString("e38a2c6b-22c6-4f9f-8a66-9b6f0d47af4f", "Save changes?"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						fConverter.SaveChanges();
						SaveButton.Enabled = false;
						Globals.Message.ShowInformation(Res.GetString("db758105-d012-4ba9-9a0e-9cee82f27943", "Imported data has been saved"), Res.GetString("8b6bd08a-3f6a-4a0f-bc04-4942e4a31e24", "Save finished"));
					}
				}
				catch (CannotSaveAfterCriticalErrorException ex)
				{
					Globals.Message.ShowError(ex.Message, Res.GetString("543eb5e6-4091-4ecc-9b88-ae6ea861bdd5", "Critical Validation Error"));
				}
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			if (fConverter != null)
			{
				fConverter.Stop();
			}

			Close();
		}

		#endregion

		#region AConverter Events

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Baseline issue")]
		void ImportProgress(object sender, EventArgs e)
		{
			if (!IsDisposed)
			{
				ConverterData data = (ConverterData)sender;

				RowsProcessedLabel.Text = data.CurrentRow.ToString();
				RowsProcessedLabel.Refresh();

				RowsErrorLabel.Text = data.Errors.Count.ToString();
				RowsErrorLabel.Refresh();

				TotalAmountLabel.Text = Utilities.Round(data.TotalAmountImported, 2).ToString();
				TotalAmountLabel.Refresh();

				Application.DoEvents();
			}
		}

		void Converter_ErrorOccurred(object sender, EventArgs e)
		{
			ErrorCount++;

			AppendOutPutListText(sender.ToString());
		}

		void Converter_ImportCompleted(object sender, EventArgs e)
		{
			if (ErrorCount == 0)
			{
				SaveButton.Enabled = true;
				this.AcceptButton = SaveButton;

				MainStatusBar.Text = Res.GetString("f039ed70-e5bb-48c5-bb49-2fd056dd50db", "Import Complete. Please check the import output window and save if you're happy with the result.");
				Globals.Message.ShowInformation(MainStatusBar.Text);
				ValidatorSavePreCondition.UpdateInfo(Env.CurrentCompany.LocalCurrency.Code);
			}
			else
			{
				string errorMessage = Res.GetString("5f428bf9-feb3-46fd-b9ac-f20161e560e5", "There were {0} errors during import. Save function is disabled.\r\nPlease fix the errors and run the process again.", ErrorCount);
				Globals.Message.ShowError(errorMessage);
				MainStatusBar.Text = errorMessage;
				SetButtons(false);
				CopyOutputToClipboardButton.Enabled = true;
				CopyOutputToClipboardButton.Visible = true;
			}
			InitializeErrors();
		}

		void InitializeErrors()
		{
			fConverter.ImportCompleted -= new EventHandler(Converter_ImportCompleted);
			fConverter.ErrorOccurred -= new EventHandler(Converter_ErrorOccurred);
			fConverter.ClearErrors();
			ErrorCount = 0;
		}

		int ErrorCount;

		void AppendOutPutListText(string message)
		{
			if (!string.IsNullOrEmpty(OutPutListTextBox.Text))
			{
				OutPutListTextBox.Text += System.Environment.NewLine + message;
			}
			else
			{
				OutPutListTextBox.Text += message;
			}
		}

		SavePreConditionValidator ValidatorSavePreCondition => validatorSavePreCondition ?? (validatorSavePreCondition = new SavePreConditionValidator());
		SavePreConditionValidator validatorSavePreCondition;

		class SavePreConditionValidator
		{
			public SavePreConditionValidator()
			{
				Factory = new Lazy<BusinessObjectFactory>(() => new BusinessObjectFactory());
				isCalledUpdateInfo = false;
			}

			public bool Validate(out string errorMsg)
			{
				if (!isCalledUpdateInfo)
				{
					errorMsg = string.Empty;
					return true;
				}

				var msgBuilder = new ZStringBuilder();
				if (Currency != GetCurrenctCompanyCurrencyInDB())
				{
					msgBuilder.AppendLine(ResString.GetMultilingualString("B90BB8BA-C226-47F6-BCD9-6B1328900D93", "Currency of current company has been updated by other operator. Imported data becomes invalid. Please reopen the form."));
				}

				errorMsg = msgBuilder.ToString();
				return errorMsg == ZString.Empty;
			}

			ZString GetCurrenctCompanyCurrencyInDB()
			{
				var currentCompanyQUery = new ZDBOnlyQuery(typeof(GlbCompany));
				currentCompanyQUery.AddToFilter(ZArchitecture.Schema.GlbCompanySchema.PK, Env.CurrentCompanyPK);
				return Factory.Value.Load<GlbCompany>(currentCompanyQUery)[0].GC_RX_NKLocalCurrency;
			}

			public void UpdateInfo(string currency)
			{
				this.Currency = currency;
				isCalledUpdateInfo = true;
			}

			bool isCalledUpdateInfo;

			ZString Currency;

			readonly Lazy<BusinessObjectFactory> Factory;
		}

		#endregion

		#region Importer Progress Event Handlers

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Baseline issue")]
		void Logger_OnUpdateCounters(object sender, EventArgs e)
		{
			ProgressLogger logger = (ProgressLogger)sender;

			RowsErrorLabel.Text = logger.RecordsInvalid.ToString();
			RowsErrorLabel.Refresh();

			RowsProcessedLabel.Text = logger.RecordsProcessed.ToString();
			RowsProcessedLabel.Refresh();

			Application.DoEvents();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Baseline issue")]
		void Logger_OnUpdateLogText(object sender, EventArgs e)
		{
			AppendOutPutListText(sender.ToString());
			Application.DoEvents();
		}

		#endregion

		#region InternalForTesting
		internal ZTextBox PathTextBoxInternal => PathTextBox;
		internal ZRadioButton DebtorsRadioButtonInternal => DebtorsRadioButton;
		internal ZRadioButton CreditorsRadioButtonInternal => CreditorsRadioButton;
		internal ZButton SaveButtonInternal => SaveButton;
		internal ZButton CopyOutputToClipboardButtonInternal => CopyOutputToClipboardButton;
		internal ZDateEdit PostDateDateEditInternal => PostDateDateEdit;
		internal ZTextBox OutPutListTextBoxInternal => OutPutListTextBox;
		#endregion
		#endregion

		ProgressLogger Logger;
		readonly Converter fConverter;
	}
}
