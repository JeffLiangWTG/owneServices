using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Aggregator;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.GeneralLedger.GLJournals;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class GLJournalModule : ZFilterGridModule
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public GLJournalModule()
		{
			ExportMenuItems.Add(ResString.GetMultilingualString("ff3e849b-b736-4ae5-88e3-e88fa11b1a1f", "Import From CSV"), new EventHandler(ImportJournalCSVEventHandler));
			ExportMenuItems.Add(ResString.GetMultilingualString("8aa83d75-cb0a-4b1d-bc45-50c0e59b74e6", "Export GL Transactions To CSV"), new EventHandler(ExportJournalCSVEventHandler));
		}

		public override ModuleIdentifier ID => ModuleIDs.GLJournal;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GeneralLedgerJournals;

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override bool AllowAdvancedDataAutomationWizard => false;

		AccTransactionHeader[] SelectedTransactions => Grid.GetSelectedElements<AccTransactionHeader>();

		SecurityCheckpoint AuditSecurityCheckpoint => Env.Security.GLJournalAuditTransaction;

		SecurityCheckpoint UndoAuditSecurityCheckpoint => Env.Security.GLJournalUndoAuditTransaction;

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("c6b40ecf-d3a5-4828-9293-2aa6d9597307", "&Reverse", "Creates a new reversed item(s) to offset the currently selected item(s) (shortcut Del)");
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, new EventHandler(ImportJournalXMLEventHandler));
		}

		protected void ImportJournalXMLEventHandler(object sender, EventArgs args)
		{
			if (!Env.Security.GeneralLedgerImport.IsAllowed)
			{
				Env.Security.GeneralLedgerImport.ShowError();
			}
			else
			{
				GlJournalXmlDataTransferDirector director = new GlJournalXmlDataTransferDirector(new GLJournalDataAdapter(), true);
				director.PromptUserAndImport(BillingInterfaceName.JournalXMLEventImport);

				if (director.LastImportedJournal != null)
				{
					GLJournalControllerImport journalController = (GLJournalControllerImport)ZControllerFactory.Create(ControllerIDs.GLJournalImport);
					journalController.ShowImportedDataForm(director.LastImportedJournal);
				}
			}
		}

		protected void ExportJournalCSVEventHandler(object sender, EventArgs args)
		{
			new AggregateController().PerformAggregationIfRequired();
			ZFormModaliser.ShowDialogAndDispose(new CsvExportGLTransactionForm(new GLTransactionBusinessObject(new BusinessObjectFactory())));
		}

		protected void ImportJournalCSVEventHandler(object sender, EventArgs args)
		{
			if (!Env.Security.GeneralLedgerImport.IsAllowed)
			{
				Env.Security.GeneralLedgerImport.ShowError();
			}
			else
			{
				using (var dialog = new ZOpenFileDialog())
				{
					dialog.Title = Res.GetString("Accounting|GLJournal|ImportJournalCSVDialog", "Import Journal CSV");
					dialog.CheckFileExists = true;
					if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
					{
						try
						{
							ImportJournalCSV(dialog.OpenFile(), dialog.UnmappedFileName);
						}
						catch (UnauthorizedAccessException ex)
						{
							Globals.Message.ShowError(ex.Message);
						}
						catch (IOException ex)
						{
							Globals.Message.ShowError(ex.Message);
						}
					}
				}
			}
		}

		void ImportJournalCSV(Stream stream, string displayFileName)
		{
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			GLJournalFlatFileDataImporter flatFileDataImporter = new GLJournalFlatFileDataImporter();

			using (var reader = new StreamReader(stream))
			{
				flatFileDataImporter.ImportData(reader, displayFileName, notificationBuffer, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.GLJournalCSVImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, displayFileName));
			}

			if (!notificationBuffer.HasErrors)
			{
				if (flatFileDataImporter.LastImportedJournal != null)
				{
					GLJournalControllerImport journalController = (GLJournalControllerImport)ZControllerFactory.Create(ControllerIDs.GLJournalImport);
					journalController.ShowImportedDataForm(flatFileDataImporter.LastImportedJournal);
				}
			}
			else
			{
				Globals.Message.ShowError(notificationBuffer.AsString);
			}
		}

		#region Implementation

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GLJournal);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GLJournalFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GLJournalCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GLJournalFilterBusinessObject();
		}

		protected MultilingualString PrintMenuItemText = ResString.GetMultilingualString("d9635b2f-3678-42b6-9289-9fb3579bea0c", "&Print");
		protected MultilingualString ForeignCurrencyBalanceAdjustmentMenuItemText = ResString.GetMultilingualString("22ea7b0e-554d-47f2-ad0a-95cb28003071", "Foreign Currency Balance Adjustment");
		protected MultilingualString ReverseAndRedoCurrencyAdjustmentMenuItemText = ResString.GetMultilingualString("a0b79629-bfe3-4e97-906d-bebe8cd37397", "Reverse and Re-do Automated Currency Adjustment");
		protected MultilingualString UploadGLJournalsMenuItemText = ResString.GetMultilingualString("5C183FC1-3626-4DE1-BEF9-902214C1AEEE", "Upload GL Journals");

		protected void HandlePrint(object sender, EventArgs e)
		{
			if (CurrentlySelectedJournal != null)
			{
				AccPrintingUtility utility = new AccPrintingUtility(Factory, Enterprise.Core.Constants.DataContext.GLJournal);
				utility.PrintDocument(CurrentlySelectedJournal, (NoResString)"General Ledger Journal", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			}
			else
			{
				Globals.Message.Show(Res.GetString("10ebd654-ca1d-46e0-ae95-0b54e780dfe2", "Please select a valid Journal to print."));
			}
		}

		void HandlePrintAccountingJournal(object sender, EventArgs e)
		{
			var gridSelections = Grid.GetSelectedElements<TransactionHeader>();
			if (gridSelections != null && gridSelections.Length > 0)
			{
				AccountingJournalPrintHelper.PrintAccountingJournal(gridSelections);
			}
			else
			{
				Globals.Message.Show(Res.GetString("6db4d7b2-2ddc-4f60-b8e6-2bdcf7ddc13e", "Please select Journal(s) to print."));
			}
		}

		void HandleReverseAndRedoCurrencyAdjustment(object sender, EventArgs e)
		{
			if (CheckCanReverseAndRedoCurrencyAdjustment())
			{
				ZControllerFactory.Create(ControllerIDs.GLJournal).ShowDeleteForm(CurrentlySelectedJournal);
			}
		}

		bool CheckCanReverseAndRedoCurrencyAdjustment()
		{
			if (!Env.Security.GLJournalRevserseAndRedoAutomatedCurrencyAdjustment.IsAllowed)
			{
				Env.Security.GLJournalRevserseAndRedoAutomatedCurrencyAdjustment.ShowError();
				return false;
			}

			if (CurrentlySelectedJournal == null)
			{
				Globals.Message.Show(Res.GetString("0b85b391-a3db-40ca-8b05-7b9d1d9e22eb", "Please select a valid Journal."));
				return false;
			}

			var journalNotRelatedMessage = Res.GetString("19b86730-d704-4d50-8d31-92c1d7c677bd", "This journal is not generated by the system in relation to the automated A/R and A/P outstanding balances currency adjustment. Please review selection.");
			if (!CurrentlySelectedJournal.IsPeriodExists)
			{
				Globals.Message.Show(journalNotRelatedMessage);
				return false;
			}

			var period = Factory.Load<AccPeriodManagement>(CurrentlySelectedJournal.PeriodPK);
			var generalLedgerClosedMessage = Res.GetString("e2094e88-6d2c-4ea8-899a-cab74f969aad", "The general ledger has been closed for the accounting period(s) related to the selected automated currency adjustment journal. Please re-open the general ledger first before running this action menu.");
			if (period.AM_IsGeneralLedgerClosed)
			{
				Globals.Message.Show(generalLedgerClosedMessage);
				return false;
			}

			return true;
		}

		void HandleUploadGLJournals(object sender, EventArgs e)
		{
			if (!Env.Security.GLJournalUpload.IsAllowed)
			{
				Env.Security.GLJournalUpload.ShowError();
			}
			else
			{
				var businessEntity = new DataImporterBusinessObject(new BusinessObjectFactory());

				using (var form = new UploadGLJournalsDataImportForm(businessEntity, UploadGLJournalsMenuItemText, BillingInterfaceName.AccountingTransactionsCsvImport))
				{
					form.Importer = new MultiCompaniesGLJournalFlatFileDataImporter();
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}
			}
		}

		protected void CreateForeignCurrencyBalanceAdjustmentForm(object sender, EventArgs e)
		{
			var journal = new BusinessObjectFactory().New<FCBAdjustmentJournal>();
			journal.Lines.AddNew(typeof(FCBAdjustmentJournalLine));

			ZControllerFactory.Create(ControllerIDs.GLJournal).ShowFormForNewEntity(journal);
		}

		protected GLJournal CurrentlySelectedJournal
		{
			get
			{
				return Grid.ListManager.GetCurrent() as GLJournal;
			}
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());

			var foreignCurrencyBalanceAdjustmentMenuItem = new ZMenuItem(ForeignCurrencyBalanceAdjustmentMenuItemText, new EventHandler(CreateForeignCurrencyBalanceAdjustmentForm));

			RegenerateJournalEntriesHelper.AddRegenerateJournalEntriesMenuItemIfAllowed(menuItems, HandleRegenerateJournalEntries);

			menuItems.Add(foreignCurrencyBalanceAdjustmentMenuItem);
			menuItems.Add(new ZMenuItem(AccountingJournalPrintHelper.PrintAccountingJournalText, new EventHandler(HandlePrintAccountingJournal)));
			menuItems.Add(new ZMenuItem(ReverseAndRedoCurrencyAdjustmentMenuItemText, new EventHandler(HandleReverseAndRedoCurrencyAdjustment)));

			menuItems.Add(new ZMenuItem("-"));
			menuItems.Add(new ZMenuItem(AccountingConstants.AuditAndCashActionText.AuditTransactionText, (sender, e) => AccountingAuditHelper.HandleAuditTransaction(this, new AuditAndCashEventArgs(AuditSecurityCheckpoint, SelectedTransactions))));
			menuItems.Add(new ZMenuItem(AccountingConstants.AuditAndCashActionText.UndoAuditTransactionText, (sender, e) => AccountingAuditHelper.HandleUndoAuditTransaction(this, new AuditAndCashEventArgs(UndoAuditSecurityCheckpoint, SelectedTransactions))));

			menuItems.Add(new ZMenuItem(UploadGLJournalsMenuItemText, new EventHandler(HandleUploadGLJournals)));

			if (GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.Value)
			{
				var allowUploadGLJournalsUsingADAW = Env.Security.UploadGLJournalsUsingADAW.IsAllowedWithConstraint();
				var adawMenuItem = new ZMenuItem(ResString.GetMultilingualString("3a70fc46-622f-4441-bca3-a2834b28c7ad", "Upload GL Journals using ADAW"));
				if (allowUploadGLJournalsUsingADAW)
				{
					adawMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("311fd6d3-ffc3-4929-bf22-e09ad217afb3", "Loading...")));
				}
				adawMenuItem.Click += (s, a) =>
				{
					if (!allowUploadGLJournalsUsingADAW)
					{
						Globals.Message.ShowError(Env.Security.UploadGLJournalsUsingADAW.ErrorMessageForNotAllowed);
					}
				};
				adawMenuItem.Popup += (s, a) =>
				{
					if (allowUploadGLJournalsUsingADAW)
					{
						var type = typeof(GLJournalHeaderForADAW);
						IBusinessObjectCollection collection;
						GlowDataWizardIntegration.HandleADAWMenuItemPopup(adawMenuItem, type, m => ShowImportWizard(m, out collection), m => ShowImportMappingWizard(type, m));
					}
				};

				menuItems.Add(adawMenuItem);
			}

			return menuItems.ToArray();
		}

		void HandleRegenerateJournalEntries(object sender, EventArgs e)
		{
			RegenerateJournalEntriesHelper.RegenerateJournalEntries(SelectedBusinessObjects);
		}

		void ShowImportWizard(IDataTransferMapping mapping, out IBusinessObjectCollection collection)
		{
			var headerType = mapping.MappingTables.FirstOrDefault()?.LineDelimiter;
			collection = new GLJournalHeaderForADAWCollection(Factory, headerType ?? string.Empty);

			if (!Env.Security.UploadGLJournalsUsingADAWImportFileUsingMapping.IsAllowedWithConstraint())
			{
				Globals.Message.ShowError(Env.Security.UploadGLJournalsUsingADAWImportFileUsingMapping.ErrorMessageForNotAllowed);
			}
			else
			{
				ShowImportWizardCore(collection, mapping);
			}
		}

#if DEBUG
		public virtual
#endif
		string ShowImportWizardCore(IBusinessObjectCollection collection, IDataTransferMapping mapping)
		{
			var func = new Func<string, INotifications, bool>((fileName, notification) =>
						{
							notification.Add(new NewlineNotification());
							notification.Add(CargoWise.EntityFramework.NotificationType.Information, Res.GetString("2356f9e2-2b29-4b50-95f8-a0bd54e2b2e8", "Start importing data"));
							using (ZOpenFileDialog.ForceLocalFile(ref fileName))
							{
								return new GLJournalForADAWDataImporter(collection).ImportData(fileName, notification, null);
							}
						});

			return GlowDataWizardIntegration.ShowEmbeddedImportWizard(collection, mapping, func);
		}

		void ShowImportMappingWizard(Type type, IDataTransferMapping mapping = null)
		{
			if (!Env.Security.UploadGLJournalsUsingADAWManageMappings.IsAllowedWithConstraint())
			{
				Globals.Message.ShowError(Env.Security.UploadGLJournalsUsingADAWManageMappings.ErrorMessageForNotAllowed);
			}
			else
			{
				var importMappingWizardResult = ShowImportMappingWizardCore(type, mapping);

				if (!string.IsNullOrEmpty(importMappingWizardResult))
				{
					Globals.Message.ShowError(importMappingWizardResult);
				}
			}
		}

#if DEBUG
		public virtual
#endif
		string ShowImportMappingWizardCore(Type typeOfElements, IDataTransferMapping mapping)
		{
			return GlowDataWizardIntegration.ShowImportMappingWizard(typeOfElements, mapping);
		}

		protected override void HandleDeleteClickCore(object sender, EventArgs e)
		{
			var selectedJournals = SelectedBusinessObjects.Cast<GLJournal>().ToArray();

			if (selectedJournals.Length != 1)
			{
				Globals.Message.Show(Res.GetString("02442EA5-E9DA-46E5-ADD9-97B47B30B370", "Please select one journal to reverse."));
			}

			if (CheckCanReverse(CurrentlySelectedJournal))
			{
				base.HandleDeleteClickCore(sender, e);
			}
		}

		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			var glJournal = selectedBusinessObject as GLJournal;
			if (glJournal?.IsLinkedWithDSBJobCloseBatch_Cached ?? false)
			{
				return ShowViewForm(selectedBusinessObject);
			}

			if (CriticalValidationHelpers.CheckGLJournalEntriesNumberHasBeenAssigned(glJournal))
			{
				var errorMsg = Res.GetString("223B7318-878F-4D8F-9CC7-823FEB57E83E", "Editing is not allowed when an unique reference number has been assigned to the journal entries of this GL Journal.\r\nPlease reverse this GL Journal and re-enter.");
				Globals.Message.ShowError(errorMsg);
				return ShowViewForm(selectedBusinessObject);
			}

			return CheckHasFINComplianceReportFromGLJournal(glJournal) ? null : base.ShowEditForm(selectedBusinessObject);
		}

		bool CheckHasFINComplianceReportFromGLJournal(GLJournal glJournal)
		{
			if (glJournal == null)
			{
				return false;
			}

			var finComplianceReportTypes = glJournal.GetComplianceReportTypesByTablePrefixAndStatus(ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.GeneralLedgerData, new[] { AccComplianceReport.Status.ReportFinalised });
			if (!finComplianceReportTypes.Any())
			{
				finComplianceReportTypes = glJournal.GetComplianceReportTypesByTablePrefixAndStatus(ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.AllTransactions, new[] { AccComplianceReport.Status.ReportFinalised });
			}

			if (finComplianceReportTypes.Any())
			{
				var reportCodes = string.Join(", ", finComplianceReportTypes);
				Globals.Message.Show(Res.GetString("D9EB1D33-3403-4054-93DD-DE13DE9640AD",
											"The journal entries of this GL Journal have been included in the finalized compliance report <{0}> and cannot be edited.",
											reportCodes),
									 Res.GetString("36381377-df10-4216-8857-4185b2be5521", "Edit GL Journal"),
									 MessageBoxButtons.OK,
									 MessageBoxIcon.Error);
				return true;
			}
			return false;
		}

		bool CheckCanReverse(GLJournal glJournal)
		{
			if (!Env.Security.ReverseGeneralLedgerJournal.IsAllowed)
			{
				Env.Security.ReverseGeneralLedgerJournal.ShowError();
				return false;
			}

			if (glJournal != null)
			{
				if (glJournal.IsPeriodExists)
				{
					Globals.Message.Show(Res.GetString("5c3bfb67-b561-4d62-ae84-6b4f1b552af8", "This journal is generated by the system in relation to the automated A/R and A/P outstanding balances currency adjustment. Please use \"Reverse and re-do Automated Currency Adjustment\" to reverse."));
					return false;
				}

				var request = glJournal.GetPendingApprovalRequestByTransactionBelongsToGroup();
				if (request != null)
				{
					Globals.Message.Show(Res.GetString("29EC9D14-A3B5-46A0-AF6C-35FD7994B206", "A reversed journal approval request ({0}) is already created, please cancel this approval request before reversing this journal.", request.XP_RequestID));
					return false;
				}

				if (glJournal.IsLinkedWithDSBJobCloseBatch_Cached)
				{
					Globals.Message.Show(Res.GetString("8292E85E-4638-4f01-A71D-BD04539AAFCD", "This journal is generated by the system in relation to the Bulk Disbursement Job Close function. Reversal is not allowed."));
					return false;
				}

				var mutex = new ZGlobalMutex(MutexIDs.GLJournalForm, GLJournal.GetMutexKey(glJournal.PK));
				if (mutex.IsLocked)
				{
					Globals.Message.ShowInformation(glJournal.GetLockMessage(mutex, Res.GetString("cae14351-62e9-472e-a7be-4271ec50a0fb", "reverse")), Res.GetString("20B2D1D1-08AC-4505-A938-B611A9DFBA2B", "Another session is editing the same information"));
					return false;
				}
			}

			return true;
		}

		#endregion
	}
}
