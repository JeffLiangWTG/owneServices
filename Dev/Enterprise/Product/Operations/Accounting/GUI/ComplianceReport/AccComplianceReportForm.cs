using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.GUI.ComplianceReport;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public partial class AccComplianceReportForm : ZForm, IPreviousNextControlOverrideProvider
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public AccComplianceReportForm()
		{
			Initialise();
		}

		public AccComplianceReportForm(AccComplianceReport businessEntity)
			: base(businessEntity)
		{
			Initialise();
		}

		void Initialise()
		{
			SetDefaultReportType();
			SetupGridColumns();
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsControl);
			AutoAddPreviousNextButtons = true;
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			ControllerID = ControllerIDs.AccComplianceReport;
			workflowTabPage.Initialize(ComplianceReport);
			SetupActionMenuItems();
			ComplianceReport.OAuthClientAuthorisation += OAuthAuthorisation;

			RemoveReportLinesCurrentPeriodTabPageIfReportDoesNotShowLines();
			reportTabControl.SelectedTab = reportLinesCurrentPeriodTabPage;
			RemoveReportBooking();
		}

		void SetDefaultReportType()
		{
			if (ComplianceReport?.ACR_ReportType.IsEmpty ?? false)
			{
				var reportTypeCode = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value
					.Cast<ComplianceReportConfiguration>().OrderByDescending(x => x.IsDefaultReportType)
					.Select(x => x.ReportCode).FirstOrDefault();
				ComplianceReport.ACR_ReportType = reportTypeCode;
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			RemoveGLAccountsTabPages();

			if (!ComplianceReport.DoesNotShowReportLines)
			{
				ComplianceReport.ApplyLinesLoadingLimit = ComplianceReport.CheckExeedsMaxReportLinesToLoad();
				if (ComplianceReport.ApplyLinesLoadingLimit)
				{
					ComplianceReport.ConfirmLinesLoadingLimit += ComplianceReport_ConfirmLinesLoadingLimit;
				}
			}

			RemoveReportLinesPreviousPeriodTabPage();

			((AccComplianceReport)BusinessEntity).ACR_PeriodicityInfo.ValueChanged += new EventHandler(periodicityDropEdit_CodeBoxTextChanged);
		}

		void RemoveReportBooking()
		{
			var isUsingGLDTablePrefix = ComplianceReport?.IsUsingGLDTablePrefix ?? false;

			if (!isUsingGLDTablePrefix || !AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.Value)
			{
				this.headerPanel.Controls.Remove(this.ACR_ARB_ReportingBookGuidFindBox);
			}
		}

		void ComplianceReport_ConfirmLinesLoadingLimit(object sender, AccComplianceReport.BoolResponseEventArgs e)
		{
			e.Response = DialogResult.Yes == Globals.Message.Show(Res.GetString("3E891965-A7BA-458F-9B76-A1D627282E5F", @"The report contains more than {0} rows. Click Yes to load the report form with only the first {0} records displayed.
Otherwise, if you wish to display all records, click No. Please be advised that it may take a while to load the full report.

Would you like to preview the report with only first {0} rows displayed?", AccComplianceReport.MaxReportLinesToLoad), Res.GetString("8455DF61-77DC-4A55-830E-0391F6593F7A", "Question"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes);

			ComplianceReport.ConfirmLinesLoadingLimit -= ComplianceReport_ConfirmLinesLoadingLimit;
		}

		void ComplianceReport_WarnAboutLoadingAllLines(object sender, AccComplianceReport.BoolResponseEventArgs e)
		{
			Globals.Message.Show(Res.GetString("5B17B196-6846-44BC-8BB9-9BAF93E42070", @"The report contains more than {0} rows. In order to proceed, all records must be retrieved.
Please be advised that it may take a while to load the data.", AccComplianceReport.MaxReportLinesToLoad)
				, Res.GetString("2DD1699C-68F9-45CD-9DAA-260C3B187740", "Warning")
				, MessageBoxButtons.OK, MessageBoxIcon.Warning, DialogResult.OK);

			e.Response = false;
			ComplianceReport.ConfirmLinesLoadingLimit -= ComplianceReport_WarnAboutLoadingAllLines;
		}

		void RemoveGLAccountsTabPages()
		{
			if (!ComplianceReport.SupportsGLBalanceDetails)
			{
				reportTabControl.TabPages.Remove(GLAccountsOpeningBalanceTabPage);
				reportTabControl.TabPages.Remove(GLAccountsClosingBalanceTabPage);
				reportTabControl.TabPages.Remove(GLAccountsMovementsTabPage);
			}
		}

		void RemoveReportLinesPreviousPeriodTabPage()
		{
			if (!ComplianceReport.HasPreviousPeriodData)
			{
				reportTabControl.TabPages.Remove(reportLinesPreviousPeriodTabPage);
				reportLinesCurrentPeriodTabPage.CaptionResourceString = Res.GetData("6883a612-7126-4ad7-86e0-de72eb13e090", "Report Lines");
			}
		}

		bool RemoveReportLinesCurrentPeriodTabPageIfReportDoesNotShowLines()
		{
			if (!ComplianceReport.DoesNotShowReportLines)
			{
				return false;
			}

			foreach (var control in reportLinesCurrentPeriodTabPage.Controls.OfType<Control>())
			{
				control.Visible = false;
			}

			var noReportLinesLabel = new ZLabel();
			noReportLinesLabel.Name = "LabelNoReportLinesHint";
			noReportLinesLabel.Dock = DockStyle.Fill;
			noReportLinesLabel.Visible = true;
			noReportLinesLabel.IsFontBold = true;
			noReportLinesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			noReportLinesLabel.Text = Res.GetString("430B28AC-6195-4490-8D62-051EDA647D74", "Report data will not be displayed on this screen as it will be generated in the background. Once report status is 'Generated', you can find the report file attached to eDocs.");
			reportLinesCurrentPeriodTabPage.Controls.Add(noReportLinesLabel);

			return true;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateButtonsMenuItemsAndLabels();
			this.Saved += AccComplianceReportForm_Saved;

			if (ComplianceReport.ApplyLinesLoadingLimit)
			{
				ComplianceReport.ConfirmLinesLoadingLimit += ComplianceReport_WarnAboutLoadingAllLines;
			}
		}

		void SetupGridColumns()
		{
			if (ComplianceReport != null)
			{
				reportCurrentPeriodLinesGrid.LayoutKey += ComplianceReport.ACR_ReportType;
				reportTotalsCurrentPeriodGrid.LayoutKey += ComplianceReport.ACR_ReportType;

				var columnNamesToRemove = ComplianceReport.GetColumnNamesToExclude(gui: true).ToArray();

				RemoveColumns(reportTotalsCurrentPeriodGrid, columnNamesToRemove);
				RemoveColumns(reportTotalsPreviousPeriodGrid, columnNamesToRemove);
				RemoveColumns(reportCurrentPeriodLinesGrid, columnNamesToRemove);
				RemoveColumns(reportPreviousPeriodLinesGrid, columnNamesToRemove);

				if (ComplianceReport.IsComplianceDocumentHeaderReport)
				{
					reportTotalsCurrentPeriodGrid.Visible = false;
					reportTotalsPreviousPeriodGrid.Visible = false;

					RemoveColumns(reportCurrentPeriodLinesGrid, columnNamesToRemove);
					RemoveColumns(reportPreviousPeriodLinesGrid, columnNamesToRemove);

					var columnNamesMapping = new Dictionary<string, ResourceStringData>();
					columnNamesMapping.Add(AccComplianceReportLineBase.Schema.TotalExTaxAmount, Res.GetData("24F7A694-406C-4EAC-8CDC-660EAF3A4597", "Amount"));
					columnNamesMapping.Add(AccComplianceReportLineBase.Schema.TotalTaxAmount, Res.GetData("92465163-C3B6-4A7D-82B9-114CEAE96395", "Tax Amount"));
					columnNamesMapping.Add(AccComplianceReportLine.Schema.PostDate, Res.GetData("512565C0-99FB-4FD6-9475-A8CCCA8E05BB", "Document Date"));
					columnNamesMapping.Add(AccComplianceReportLine.Schema.AH_TransactionNum, Res.GetData("94F92F0A-2378-4C2E-93D3-44FF963F90B7", "Document Number"));
					columnNamesMapping.Add(AccComplianceReportLine.Schema.AH_TransactionReference, Res.GetData("09574EBC-B4A7-49FD-B0B2-0A8AF9519284", "Reporting Period"));
					columnNamesMapping.Add(AccComplianceReportLine.Schema.AH_TransactionType, Res.GetData("91DF8B65-FC16-4870-8FFE-4F6AEB21C793", "Compliance Book"));
					columnNamesMapping.Add(AccComplianceReportLine.Schema.OK_CustomsRegNo, Res.GetData("686748B5-11D8-44F0-B34A-AEBF8A9D1DBB", "VAT Registration number"));

					RenameColumns(reportCurrentPeriodLinesGrid, columnNamesMapping);
					RenameColumns(reportPreviousPeriodLinesGrid, columnNamesMapping);
				}
			}
		}

		void RemoveColumns(ZGrid grid, params string[] columnNames)
		{
			grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => columnNames.Contains(x.ColumnName)).ToList().
									ForEach(column => grid.ColumnStyles.Remove(column));
		}

		void RenameColumns(ZGrid grid, Dictionary<string, ResourceStringData> columnNamesMapping)
		{
			grid.ColumnStyles.Cast<ZGridColumnInfo>().ToList().ForEach(column =>
					{
						if (columnNamesMapping.ContainsKey(column.ColumnName))
						{
							column.CaptionResourceString = columnNamesMapping[column.ColumnName];
						}
					}
				);
		}

		void SetupActionMenuItems()
		{
			ReQueueReportMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, AccComplianceReportGuiHelper.ReQueueMenuItemText, ReQueueButton_Click);
			GenerateReportMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, AccComplianceReportGuiHelper.GenerateMenuItemText, GenerateButton_Click);
			FinaliseReportMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, AccComplianceReportGuiHelper.FinalizeMenuItemText, FinaliseButton_Click);
			ReQueueReportMenuItem.Enabled = false;
			GenerateReportMenuItem.Enabled = false;
			FinaliseReportMenuItem.Enabled = false;

			if (ComplianceReport.Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Portugal && ComplianceReport.SupportsSAFTXmlGeneration)
			{
				GenerateXmlMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, AccComplianceReportGuiHelper.GenerateSAFTXmlMenuItemText, GenerateSAFTXmlButton_Click);
				GenerateXmlMenuItem.Enabled = false;
			}
			else if (ComplianceReport.Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Italy && ComplianceReport.SupportsEsterometroXmlGeneration)
			{
				GenerateXmlMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, AccComplianceReportGuiHelper.GenerateEsterometroXmlMenuItemText, GenerateEsterometroXmlButton_Click);
				GenerateXmlMenuItem.Enabled = false;
			}
			else if (ComplianceReport.SupportsExportOpenFormatFile)
			{
				GenerateXmlMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, AccComplianceReportGuiHelper.ExportOpenFormatFileLabel, ExportOpenFormatFileButton_Click);
				GenerateXmlMenuItem.Enabled = false;
			}
			else if ((ComplianceReport.Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom && ComplianceReport.SupportsMTD)
					|| (ComplianceReport.Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia && ComplianceReport.SupportsTPAR))
			{
				ViewAndSubmitMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, AccComplianceReportGuiHelper.ViewAndSubmitMenuItem, ViewAndSubmitButton_Click);
				ViewAndSubmitMenuItem.Enabled = false;
			}
			else if (ComplianceReport.SupportsLiquidazioneIVA)
			{
				VatSummaryMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, AccComplianceReportGuiHelper.VatSummaryMenuItem, vatSummaryButton_Click);
				VatSummaryMenuItem.Enabled = false;
			}
			else if (ComplianceReport.SupportsImportABNs)
			{
				ImportABNsMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, AccComplianceReportGuiHelper.ImportABNsMenuItem, ViewAndSubmitButton_Click);
				ImportABNsMenuItem.Enabled = false;
			}
		}

		MenuItem ReQueueReportMenuItem;
		MenuItem GenerateReportMenuItem;
		MenuItem FinaliseReportMenuItem;
		MenuItem GenerateXmlMenuItem;
		MenuItem ViewAndSubmitMenuItem;
		MenuItem ImportABNsMenuItem;
		MenuItem VatSummaryMenuItem;

		void AccComplianceReportForm_Saved(object sender, EventArgs e)
		{
			UpdateButtonsMenuItemsAndLabels();
		}

		void UpdateButtonsMenuItemsAndLabels()
		{
			bool showButtons = DisplayMode != ODisplayMode.ReadOnly && DisplayMode != ODisplayMode.Delete;
			reQueueButton.Visible = showButtons;
			generateButton.Visible = showButtons;
			finaliseButton.Visible = showButtons;
			generateXmlButton.Visible = false;
			vatSummaryButton.Visible = false;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Italy && ComplianceReport.SupportsEsterometroXmlGeneration)
			{
				this.generateXmlButton.ToolTipCaption = ResString.GetMultilingualString("f6fc8f1b-6217-4881-ab20-1282cfbba454", "Generate Monthly Transaction XML");
				this.generateXmlButton.Click += new EventHandler(this.GenerateEsterometroXmlButton_Click);
				generateXmlButton.Visible = showButtons;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Portugal && ComplianceReport.SupportsSAFTXmlGeneration)
			{
				this.generateXmlButton.ToolTipCaption = ResString.GetMultilingualString("f6fc8f1b-6217-4881-ab20-1282cfbba454", "Generate Monthly Transaction XML");
				this.generateXmlButton.CaptionResourceString = Res.GetData("c6adde8f-cea0-4703-ac3f-96f03215de4f", "Generate XML");
				this.generateXmlButton.Click += new EventHandler(this.GenerateSAFTXmlButton_Click);
				generateXmlButton.Visible = showButtons;
			}
			else if (ComplianceReport.SupportsExportOpenFormatFile)
			{
				this.generateXmlButton.CaptionResourceString = Res.GetData("16abfcf8-3e42-4f05-a81b-d2a5f2b6a0e0", "Export Files");
				this.generateXmlButton.ToolTipCaption = AccComplianceReportGuiHelper.ExportOpenFormatFileLabel;
				this.generateXmlButton.Click += new EventHandler(this.ExportOpenFormatFileButton_Click);
				generateXmlButton.Visible = showButtons;
			}

			var showViewAndSubmit = showButtons && (ComplianceReport.SupportsMTD || ComplianceReport.SupportsTPAR || ComplianceReport.SupportsPTRSSmallBusiness || ComplianceReport.SupportsPTRSAllPayments || ComplianceReport.SupportsZMGermany);
			var showImportANBs = showButtons && ComplianceReport.SupportsImportABNs;

			if (showImportANBs)
			{
				viewAndSubmitButton.CaptionResourceString = Res.GetData("0A63073A-EAFC-4A33-8823-2B7055B041F7", "Import ABNs");
				viewAndSubmitButton.ToolTipCaption = ResString.GetMultilingualString("BBE03387-2A56-49BE-B8B9-4E213C33728B", "Import Reportable Small Business ABNs");
			}
			else if (showViewAndSubmit)
			{
				viewAndSubmitButton.CaptionResourceString = Res.GetData("3f26dad1-8f90-48cf-9c83-d30665c3de75", "View and Submit");
				viewAndSubmitButton.ToolTipCaption = ResString.GetMultilingualString("1CC93442-FC03-4070-927E-7B3D9B29C810", "View and Submit");
			}

			viewAndSubmitButton.Visible = showViewAndSubmit || showImportANBs;

			var showAndEnableVatSummary = showButtons && ComplianceReport.SupportsLiquidazioneIVA;
			vatSummaryButton.Visible = showAndEnableVatSummary;

			if (showButtons)
			{
				viewAndSubmitButton.Enabled = viewAndSubmitButton.Visible;
				SetEnabled(ViewAndSubmitMenuItem, showViewAndSubmit);
				SetEnabled(ImportABNsMenuItem, showImportANBs);

				vatSummaryButton.Enabled = showAndEnableVatSummary;
				SetEnabled(VatSummaryMenuItem, showAndEnableVatSummary);

				bool enablebuttons = ComplianceReport.IsInDatabase && !ComplianceReport.ReadOnly;

				reQueueButton.Enabled = enablebuttons && ComplianceReport.SupportsReQueue;
				SetEnabled(ReQueueReportMenuItem, reQueueButton.Enabled);

				generateButton.Enabled = enablebuttons && !ComplianceReport.GeneratedByCRQServiceTask;
				SetEnabled(GenerateReportMenuItem, generateButton.Enabled);

				finaliseButton.Enabled = enablebuttons;
				SetEnabled(FinaliseReportMenuItem, finaliseButton.Enabled);

				generateXmlButton.Enabled = enablebuttons && generateXmlButton.Visible && (ComplianceReport.SupportsSAFTXmlGeneration || ComplianceReport.SupportsEsterometroXmlGeneration || ComplianceReport.SupportsExportOpenFormatFile);
				SetEnabled(GenerateXmlMenuItem, generateXmlButton.Enabled);
			}

			periodCalcEdit.CaptionResourceString = ComplianceReport.AccountingPeriodCaptionResString;
			periodCalcEdit.UpdateCaption();

			void SetEnabled(MenuItem menuItem, bool enabled)
			{
				if (menuItem != null)
				{
					menuItem.Enabled = enabled;
				}
			}
		}

		protected override ZTabControl TopLevelTabControl
		{
			get { return this.tabControl; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("5d942598-21a9-4e30-8f4f-c575631214cd", "Compliance Report"); }
		}

		protected override void OnClosed(EventArgs e)
		{
			ComplianceReport.OAuthClientAuthorisation -= OAuthAuthorisation;
			base.OnClosed(e);
		}

		void UpdateControlAndRefresh()
		{
			UpdateButtonsMenuItemsAndLabels();
			Refresh();
		}

		protected AccComplianceReport ComplianceReport
		{
			get { return ((AccComplianceReport)BusinessEntity); }
		}

		void ViewAndSubmitButton_Click(object sender, EventArgs e)
		{
			if (ComplianceReport.SupportsMTD)
			{
				ComplianceReport.HandleMTDViewAndSubmit();
			}
			else if (ComplianceReport.SupportsTPAR)
			{
				ComplianceReport.HandleTPARViewAndSubmit();
			}
			else if (ComplianceReport.SupportsImportABNs)
			{
				ComplianceReport.HandleImportABNs();
			}
			else if (ComplianceReport.SupportsPTRSSmallBusiness)
			{
				ComplianceReport.HandlePTRSSmallBusinessViewAndSubmit();
			}
			else if (ComplianceReport.SupportsPTRSAllPayments)
			{
				ComplianceReport.HandlePTRSAllPaymentsViewAndSubmit();
			}
			else if (ComplianceReport.SupportsZMGermany)
			{
				ComplianceReport.HandleZMDViewAndSubmit();
			}
		}

		void GenerateEsterometroXmlButton_Click(object sender, EventArgs e)
		{
			ComplianceReport.HandleGenerateEsterometroXml(this);
		}

		void ExportOpenFormatFileButton_Click(object sender, EventArgs e)
		{
			ComplianceReport.HandleExportOpenFormatFile(this);
		}

		void GenerateSAFTXmlButton_Click(object sender, EventArgs e)
		{
			ComplianceReport.HandleGenerateSAFTXml(this);
		}

		void ReQueueButton_Click(object sender, EventArgs e)
		{
			if (!ShowErrorForHasChanges(Res.GetString("DCDC7157-B211-45F1-9572-C24B5650A5E8", "Please, save Report before re-queuing"), Res.GetString("D3B8008E-DFF7-4806-A6E4-6B91B35B1F3B", "Re-Queue Report")))
			{
				if (ComplianceReport.IsUsingGLDTablePrefix)
				{
					Globals.Message.Show(Res.GetString("F39E239A-C8FA-41E3-827A-7EF7474AD2E7", "The compliance report is generated using accounting journals data source. The queue and re-queue actions are not applicable."),
						Res.GetString("D3B8008E-DFF7-4806-A6E4-6B91B35B1F3B", "Re-Queue Report"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
				}
				else
				{
					ComplianceReport.HandleReQueue();
					UpdateControlAndRefresh();
				}
			}
		}

		void GenerateButton_Click(object sender, EventArgs e)
		{
			if (!ShowErrorForHasChanges(Res.GetString("2b2a477e-8a6f-40b0-92e8-1cfa40af1d69", "Please, save Report before generating"), Res.GetString("b7a73d20-352b-49ed-9f14-0e3f4c34d1d1", "Report Generating")))
			{
				ComplianceReport.HandleGenerate(this);
				if (ComplianceReport.ACR_Status == AccComplianceReport.Status.ReportGenerated)
				{
					ReloadForm();
				}
				else
				{
					UpdateControlAndRefresh();
				}
			}
		}

		void FinaliseButton_Click(object sender, EventArgs e)
		{
			if (!ShowErrorForHasChanges(Res.GetString("68881b69-42ad-45b7-967a-cad95750f35d", "Please, save Report before finalizing"), Res.GetString("1d2cd66c-b115-4546-af31-2ae19183104b", "Report Finalizing")))
			{
				ComplianceReport.HandleFinalize();
				ComplianceReport.RefreshBindingIncludingChildren();
				UpdateControlAndRefresh();
			}
		}

		bool ShowErrorForHasChanges(string message, string caption)
		{
			var isErrorShown = false;
			if (this.BusinessEntityForHasChanges.HasChanges)
			{
				Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				isErrorShown = true;
			}
			return isErrorShown;
		}

		void OAuthAuthorisation(object sender, AccComplianceReport.OAuthClientAuthorisationEventArgs e)
		{
			using (var browserForm = new HMRCAuthorisationForm())
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(browserForm, this) == DialogResult.OK)
				{
					e.AuthorisationCode = browserForm.OAuthCode;
				}
			}
		}

		#region IPreviousNextControlOverrideProvider

		bool IPreviousNextControlOverrideProvider.OverridesGetPreviousNextControl { get { return false; } }
		bool IPreviousNextControlOverrideProvider.OverridesSetPreviousNextControlParentAndPosition { get { return true; } }
		bool IPreviousNextControlOverrideProvider.ShouldDoBaseSetPreviousNextControlParentAndPosition { get { return false; } }

		ZPreviousNextControl IPreviousNextControlOverrideProvider.GetPreviousNextControl(ModuleResultsBusinessObject bizObj, ZController controller)
		{
			throw new NotImplementedException("Shoild not be called as is not overriden");
		}

		void IPreviousNextControlOverrideProvider.SetPreviousNextControlParentAndPosition(ZPreviousNextControl control)
		{
			int bottom = bottomPanel.Height;

			ControlDpiScalingHelper.SetTop(ref control, bottom - control.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
			ControlDpiScalingHelper.SetLeft(ref control, 1, true);
			control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			bottomPanel.Controls.Add(control);
		}

		#endregion

		#region Periodicity Event

		void periodicityDropEdit_CodeBoxTextChanged(object sender, EventArgs e)
		{
			UpdateButtonsMenuItemsAndLabels();
		}

		#endregion

		void vatSummaryButton_Click(object sender, EventArgs e)
		{
			ComplianceReport.HandleLiquidazioneIVAViewAndSubmit();
		}
	}
}
