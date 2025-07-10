using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.GUI.DataExport;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.CashBook.DirectDebitBatch
{
	public partial class DirectDebitBatchForm : AccountingZForm, IButtonDeleteTextOverride
	{
		readonly DirectDebitBatchHeader BatchHeader;
		DirectDebitBatchHeader BatchHeaderToGenerateFile;
		ZArchitecture.ZTextBox BatchNoTextBox;
		ZGuidFindBox BankAccountFindBox;
		ZArchitecture.ZGrid DepositBatchLineGrid;
		ZCalcFindBox InvoiceAmountCalcFind;
		ZButton GenerateDDRFileButton;
		System.ComponentModel.IContainer components;
		ZTemplateTabControl MainTabControl;
		ZLogsTabPage zEventTabPage1;
		ZTabPage zTabPage1;
		ZArchitecture.ZLabel CancelledBatchLabel;
		ZButton FindButton;
		ZButton ClearButton;
		ZGroupBox zGroupBox1;
		ZRadioButton AutoDDRRadioButton;
		ZRadioButton NonAutoDDRRadioButton;
		ZRadioButton BothRadioButton;
		ZArchitecture.ZLabel zLabel2;
		ZDateEdit PostDateDateEdit;
		ZDateEdit BatchDateDateEdit;
		ZCalcFindBox zCalcFind1;
		ZArchitecture.ZTextBox BankReferenceNumberTextBox;
		Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;

		public DirectDebitBatchForm(DirectDebitBatchHeader batchHeader)
			: base(batchHeader)
		{
			this.BatchHeader = batchHeader;
			this.BatchHeader.NoRowsSelectedEvent += BatchHeader_NoRowsSelected;

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			OrgEditMenuItemText = ResString.GetMultilingualString("b0f04046-69a4-4619-abbf-737516c7b7bd", "Edit Payment Organization Detail");
			ViewTransactionMenuItemText = ResString.GetMultilingualString("8aa269e7-b612-4aa1-90f8-954f62059685", "View Transaction Detail");
			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			var customiseExportMenuItem = new ZMenuItem();
			customiseExportMenuItem.Click += CustomiseExportMenuItem_Click;
			customiseExportMenuItem.Name = "CustomiseExport";
			customiseExportMenuItem.Caption = ResString.GetMultilingualString("82edd641-68b6-4d3a-93fe-1be3646eda1a", "Customize Export");
			this.ActionsMenuItem.MenuItems.Add(customiseExportMenuItem);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				BatchHeader.NoRowsSelectedEvent -= BatchHeader_NoRowsSelected;
				BatchHeader.AH_RX_NKTransactionCurrencyInfo.ValueChanged -= AH_RX_NKTransactionCurrencyInfo_ValueChanged;
			}
			base.Dispose(disposing);
		}

		ZSaveFileDialog SaveBatchFileDialog;
		MultilingualString OrgEditMenuItemText;
		MultilingualString ViewTransactionMenuItemText;
		MenuItem ViewTransactionMenu;
		MenuItem OrgEditMenuItem;

		#region Base Override

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AutoAddPreviousNextButtons = true;
			CancelledBatchLabel.Visible = (DisplayMode != ODisplayMode.Delete) && BatchHeader.AH_IsCancelled;
			GenerateDDRFileButton.Enabled = ShouldEnableGenerateDDRFileButton;
			SetGridContextMenu();
			if (BatchHeader.AH_ReceiptType == ReceiptTypes.eNettDirectDebit)
			{
				GenerateDDRFileButton.Visible = false;
			}
			AH_RX_NKTransactionCurrencyInfo_ValueChanged(this, null);
			BatchHeader.AH_RX_NKTransactionCurrencyInfo.ValueChanged += AH_RX_NKTransactionCurrencyInfo_ValueChanged;
			this.DepositBatchLineGrid.SetColumnVisible(DisplayMode == ODisplayMode.New, "IncludeInTheBatch");
		}

		void AH_RX_NKTransactionCurrencyInfo_ValueChanged(object sender, EventArgs e)
		{
			InvoiceAmountCalcFind.Visible = BatchHeader.AH_RX_NKTransactionCurrency != BatchHeader.Company.GC_RX_NKLocalCurrency;
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			var newBatchHeader = !BatchHeader.IsInDatabase;
			ContinueWithSave baseResult = base.ValidateAndSave();

			if (baseResult == ContinueWithSave.Yes && DisplayMode != ODisplayMode.Delete)
			{
				if (newBatchHeader)
				{
					PromptForDDRFileGeneration();
				}
				GenerateDDRFileButton.Enabled = ShouldEnableGenerateDDRFileButton;
			}

			return baseResult;
		}

		protected override void SetAutoAddPreviousNextButtons()
		{
			AutoAddPreviousNextButtons = true;
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption = false)
		{
			if (((BusinessObject)businessEntityForValidation).IsInDatabase)
			{
				return new ZErrorMessageBox(businessEntityForValidation, (NoResString)"DDR File", Res.GetString("e1b9e731-764a-490f-8ee4-063ef3cb6364", "generate"), Res.GetString("1e9c3fca-76ff-4ea4-9f25-a9177a69c7f0", "generated"), includeIgnoreOption); // Hard-coded constant
			}
			else
			{
				return base.CreateErrorMessageBox(businessEntityForValidation, includeIgnoreOption);
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion

		#region Implmentation

		protected override bool ShowAuditTab => true;

		void SetGridContextMenu()
		{
			ContextMenu menu = DepositBatchLineGrid.ContextMenu;

			ViewTransactionMenu = new ZMenuItem(ViewTransactionMenuItemText, OnViewTransactionClick);
			menu.MenuItems.Add(ViewTransactionMenu);

			OrgEditMenuItem = new ZMenuItem(OrgEditMenuItemText, OnOrganisationEdit);
			menu.MenuItems.Add(OrgEditMenuItem);

			DepositBatchLineGrid.ContextMenu.Popup += ContextMenu_Popup;
		}

		void SetEditOrgMenuItemReadOnly(TransactionHeader transaction)
		{
			if (transaction != null)
			{
				OrgEditMenuItem.Enabled = (transaction.AH_Ledger != LedgerTypes.CashBook);
				// TODO: Once Direct Payment is Z, then we can show it by double click
				ViewTransactionMenu.Enabled = (transaction.AH_Ledger != LedgerTypes.CashBook);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void RemoveDocumentsMenu()
		{
			foreach (MenuItem menuItem in DepositBatchLineGrid.ContextMenu.MenuItems)
			{
				if (menuItem.Text == "Documents")
				{
					DepositBatchLineGrid.ContextMenu.MenuItems.Remove(menuItem);
					break;
				}
			}
		}

		void OnViewTransactionClick(object sender, EventArgs e)
		{
			ShowSelectedTransaction();
		}

		void OnOrganisationEdit(object sender, EventArgs e)
		{
			if (DepositBatchLineGrid != null && DepositBatchLineGrid.ListManager != null && DepositBatchLineGrid.ListManager.Position >= 0)
			{
				IZForm orgForm = ShowOrganisationFormToEdit();
				if (orgForm != null)
				{
					orgForm.Closed += new EventHandler(OrgForm_Closed);
				}
			}
		}

		void OrgForm_Closed(object sender, EventArgs e)
		{
			BatchHeader.ValidateBeforeFileGeneration();
		}

		protected string GetSaveDialogFilter(string dDRFileFormat)
		{
			string fileExtension = "";
			switch (dDRFileFormat)
			{
				case Constants.DDRFileFormat.ANZ:
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand)
					{
						fileExtension = "MTS";
					}
					else
					{
						fileExtension = "ABA";
					}
					break;
				case Constants.DDRFileFormat.WBC:
				case Constants.DDRFileFormat.CBA:
					fileExtension = "ABA";
					break;
				case Constants.DDRFileFormat.NAB:
				case Constants.DDRFileFormat.ASB:
					fileExtension = "AB";
					break;
				case Constants.DDRFileFormat.BNZ:
					fileExtension = "AFI";
					break;
				case Constants.DDRFileFormat.BCS:
					fileExtension = "BCS";
					break;
				case Constants.DDRFileFormat.BTM:
					fileExtension = "TXT";
					break;
				case Constants.DDRFileFormat.CUS:
					fileExtension = "CSV";
					break;
				default:
					fileExtension = "ABA";
					break;
			}
			string result = String.Format((NoResString)"{0} Files (*.{0})|.{0}", fileExtension); // Used as file filter
			return result;
		}

		void SaveDDRFile(DirectDebitBatchHeader header)
		{
			if (header == null) { throw new ArgumentNullException(nameof(header)); }

			header.ValidateBeforeFileGeneration();
			if (header.HasErrors)
			{
				using (var msgBox = CreateErrorMessageBox(header))
				{
					ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
				}
				return;
			}

			try
			{
				if (header.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.CUS)
				{
					using (var adapter = CreateDirectDebitBatchDataExportAdapter(header.Factory, header))
					{
						var promptUserForFile = !adapter.IsFileNameExpressionUsed;

						var result = GetUnmappedDDRFilePathFromUser(header, promptUserForFile);
						var unmappedFilePath = result.UnmappedFilePath;

						if (result.IsClientCancel)
						{
							return;
						}

						if (string.IsNullOrEmpty(unmappedFilePath) && promptUserForFile)
						{
							ShowFilePathEmptyError();
							return;
						}

						string errorMessage = string.Empty;
						if (adapter.CreateFile(unmappedFilePath, out errorMessage))
						{
							Globals.Message.ShowInformation(Res.GetString("1f997021-c3ca-4585-9c0a-bb71e7945c09", "File successfully created at {0}", adapter.LastExportedFullFileName));
							OpenURL(header.BankAccount.PK);
						}
						else
						{
							Globals.Message.ShowError(errorMessage);
						}
					}
				}
				else
				{
					try
					{
						var result = GetUnmappedDDRFilePathFromUser(header, true);
						var unmappedFilePath = result.UnmappedFilePath;

						if (result.IsClientCancel)
						{
							return;
						}

						if (string.IsNullOrEmpty(unmappedFilePath))
						{
							ShowFilePathEmptyError();
							return;
						}

						if (header.CreateFile(unmappedFilePath, () => ZSaveFileDialog.OpenFile(unmappedFilePath)))
						{
							Globals.Message.ShowInformation(Res.GetString("1f997021-c3ca-4585-9c0a-bb71e7945c09", "File successfully created at {0}", unmappedFilePath));
							OpenURL(header.BankAccount.PK);
						}
					}
					catch (ArgumentException ex)
					{
						Globals.Message.ShowError(ex.Message, Res.GetString("97d7ec7a-9e65-4e29-9214-a9192be0d24e", "Unable to generate DDR File"));
					}
				}
			}
			catch (FormatException e)
			{
				Globals.Message.ShowError(Res.GetString("eeb3b062-1bf8-48b5-912d-d97fa04a7d0c", "Some of the Payee Bank BSB and Payee Bank Account Number are in incorrect format. Please correct them before generating DDR file."), Res.GetString("97d7ec7a-9e65-4e29-9214-a9192be0d24e", "Unable to generate DDR File"));
				ErrorReporter.ReportOnce("While generating Bank DDR File", e);
			}
			catch (UnauthorizedAccessException e)
			{
				ShowIOError(e);
			}
			catch (IOException e)
			{
				ShowIOError(e);
			}

			void ShowFilePathEmptyError()
			{
				Globals.Message.ShowError(Res.GetString("B933B8A8-7E2C-4154-8AB6-545FC944909A", "DDR File cannot be generated as a file path was not provided."), Res.GetString("A77FC402-FFB6-49B8-9101-310F3F182448", "Unable to generate DDR File"));
			}
		}

		protected virtual DirectDebitBatchDataExportAdapter CreateDirectDebitBatchDataExportAdapter(BusinessObjectFactory factory, DirectDebitBatchHeader header)
			=> new DirectDebitBatchDataExportAdapter(factory, header);

		(bool IsClientCancel, string UnmappedFilePath) GetUnmappedDDRFilePathFromUser(DirectDebitBatchHeader header, bool promptUserForFile)
		{
			var isClientCancel = false;
			var unmappedFilePath = "";

			if (!Globals.IsTest && promptUserForFile)
			{
				SaveBatchFileDialog = new ZSaveFileDialog();
				SaveBatchFileDialog.Filter = GetSaveDialogFilter(header.BankAccount.AB_AutoDDRFormat);
				SaveBatchFileDialog.FileName = header.AH_TransactionNum;
				DialogResult selection = SaveBatchFileDialog.ShowDialog();
				if (selection == DialogResult.OK)
				{
					unmappedFilePath = SaveBatchFileDialog.UnmappedFileName;
				}
				else
				{
					isClientCancel = true;
				}
			}
#if DEBUG
			if (Globals.IsTest && promptUserForFile)
			{
				unmappedFilePath = FilePathIsEmpty_ForTestOnly ? string.Empty : Path.Combine(Env.TempPath, "TestDDRFile"
										+ (header.BankAccount.AB_AutoDDRFormat == Constants.DDRFileFormat.CUS ? ".csv" : ".aba")
								   );
			}
#endif
			return (isClientCancel, unmappedFilePath);
		}

		void ShowIOError(Exception ex)
		{
			Globals.Message.ShowError(Res.GetString("3a39b8b5-c7d8-4b80-8469-675c2591c6a7", "Cannot write the file to the disk. Please check with your system administrator./r/n	Details: ") + ex.Message);
		}

#if DEBUG
		protected
#endif
		void OpenURL(ZGuid bankAccountPK)
		{
			DirectDebitFileCreationURL setting = AccountingConfigurationRegistry.Instance.DirectDebitFileCreationURLs.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).Find(bankAccountPK);
			if (setting != null && setting.BankWebsite != ZString.Empty)
			{
				DialogResult result = Globals.Message.Show(
								Res.GetString("e42e3aed-04d6-4d6d-ab0b-c5c8aa990bc9", "Do you want to open the Bank's webpage '{0}' ?", setting.BankWebsite),
								"",
								MessageBoxButtons.YesNo,
								MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
				{
					WebUrlLauncher.Launch(setting.BankWebsite);
				}
			}
		}

		public override string FormVerb
		{
			get
			{
				string verb = base.FormVerb;
				if (DisplayMode == ODisplayMode.Delete)
				{
					verb = Res.GetString("DirectDebitBatchForm|VerbCancel", "Cancel");
				}
				return verb;
			}
		}

		void ShowSelectedTransaction()
		{
			if (SelectedTransaction != null && SelectedTransaction is Payment)
			{
				ZController controller = AccountingControllerCreator.GetNewController(SelectedTransaction);
				controller.ShowViewForm(SelectedTransaction);
			}
		}

		ZOrganisationsForm ShowOrganisationFormToEdit()
		{
			TransactionHeader selectedTrans = SelectedTransaction;
			if (selectedTrans != null)
			{
				ZController orgController = ZControllerFactory.Create(ControllerIDs.Organisation);
				OrgHeader orgsToEdit = SecondFactory.Load(typeof(OrgHeader), selectedTrans.AH_OH) as OrgHeader;
				return orgController.ShowEditForm(orgsToEdit) as ZOrganisationsForm;
			}
			else
			{
				return null;
			}
		}

		string IButtonDeleteTextOverride.DeleteButtonText
		{
			get { return Res.GetString("DirectDebitBatchForm|06BD5C5F-AE56-4178-8A35-A52485DE751C", "&Cancel Batch"); }
		}

		#endregion

		#region Private Properties

		bool ShouldEnableGenerateDDRFileButton => Env.Security.RegenerateDirectDebitFile.IsAllowed && BatchHeader.IsInDatabase && !BatchHeader.AH_IsCancelled;

		TransactionHeader SelectedTransaction
		{
			get
			{
				if (DepositBatchLineGrid != null && DepositBatchLineGrid.ListManager != null)
				{
					return DepositBatchLineGrid.ListManager.GetCurrent() as TransactionHeader;
				}
				else
				{
					return null;
				}
			}
		}

		BusinessObjectFactory fReadOnlyFactory;
		BusinessObjectFactory SecondFactory
		{
			get
			{
				if (fReadOnlyFactory == null)
				{
					fReadOnlyFactory = new BusinessObjectFactory();
				}
				return fReadOnlyFactory;
			}
		}

		#endregion

		#region Public Properties

#if DEBUG
		public bool FilePathIsEmpty_ForTestOnly;
#endif

		#endregion

		#region Event Handler

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			SetEditOrgMenuItemReadOnly(SelectedTransaction);
			RemoveDocumentsMenu();
		}

		void BatchHeader_NoRowsSelected(object sender, EventArgs e)
		{
			Globals.Message.ShowWarning(Res.GetString("36f7a5f6-d36a-43bb-8af3-1ebe828301c2", "No Transaction is selected for DDR"), Res.GetString("26df9b4c-689d-4637-ad87-aab5fc77706e", "DDR Batch"));
			DisplayMode = ODisplayMode.New;
		}

		void GenerateDDRFileButton_Click(object sender, EventArgs e)
		{
			SaveDDRFile(BatchHeader);
		}

		void DepositBatchLineGrid_DoubleClick(object sender, EventArgs e)
		{
			ShowSelectedTransaction();
		}

		void PromptForDDRFileGeneration()
		{
			if (BatchHeader.BankAccount != null && BatchHeader.BankAccount.AB_AllowAutoDDR)
			{
				if (Globals.Message.Show(Res.GetString("7d626c75-6127-439e-99cf-81176e4bc4ce", "Do you want to create DDR File?"), Res.GetString("35348e1e-3053-4416-a369-ce428e594f3f", "DDR Batch Creation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
				{
					BatchHeaderToGenerateFile = SecondFactory.Load(typeof(DirectDebitBatchHeader), BatchHeader.PK) as DirectDebitBatchHeader;
					SaveDDRFile(BatchHeaderToGenerateFile);
				}
			}
		}

		void CustomiseExportMenuItem_Click(object sender, EventArgs e)
		{
			var securityCheckPoint = Env.Security.AllowCustomizeExport;
			if (!securityCheckPoint.IsAllowed)
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
				return;
			}

			DirectDebitBatchHeader header = null;
			try
			{
				header = BusinessEntity as DirectDebitBatchHeader;
				if (!header.IsInDatabase)
				{
					Globals.Message.ShowError(Res.GetString("220ea60c-c56c-4e6a-ab94-ad27a8d8c81e", "Please post the Direct Debit Batch before customizing the export."));
					return;
				}
				if (header.BankAccount.AB_AutoDDRFormat != Constants.DDRFileFormat.CUS)
				{
					Globals.Message.ShowError(Res.GetString("01C221C1-02C0-4824-A59B-7C8CC9FAC6F4", "You must set up your bank account with a DDR File Format of 'CUS - Custom Format' before you can use the DDR Batch Data Export Wizard."));
					return;
				}
				BusinessObjectFactory factory = new BusinessObjectFactory();
				IExportCollectionInfo collectionInfo;
				using (var adapter = new DirectDebitBatchDataExportAdapter(factory, header))
				{
					adapter.Sort(header);
					collectionInfo = adapter.GetMultiTypeCollectionInfo(header);
				}

				using (DataExportWizardForm form = new DataExportWizardForm(collectionInfo, "DDRBatch"))
				{
					ExportWizard exportWizard = form.BusinessEntity as ExportWizard;
					ZQuery query = new ZQuery(StmDataSchema.SD_Owner, header.BankAccount.PK);
					query.AddToFilter(StmDataSchema.SD_Name, "DDRBatchExportSetting");
					StmData data = factory.LoadTop1<StmData>(query);

					if (data != null)
					{
						exportWizard.Setting = data.SD_BinaryValue.ToAscii();
					}

					exportWizard.FileNameExpressionObject = new DataTransfer.DataExport.DataExportDirectDebitBatchHeader(factory, header);

					form.ShowDialog();

					if (form.DialogResult == DialogResult.OK)
					{
						if (data == null)
						{
							data = factory.New<StmData>();
						}
						data.SD_Name = "DDRBatchExportSetting";
						data.SD_Owner = header.BankAccount.PK;
						ZBlob blob = ZBlob.FromAscii(exportWizard.Setting);
						data.SD_BinaryValue = blob;
						factory.Save();
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(Res.GetString("9fcff751-0921-4868-8173-129a3ec1818f", "Error customizing the Direct Debit Batch export of Bank Account: {0}", header != null ? header.BankAccount.AB_Code : ZString.Empty));
			}
		}

		#endregion
	}
}

