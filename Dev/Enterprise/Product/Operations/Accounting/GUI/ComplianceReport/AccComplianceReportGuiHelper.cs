using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using Enterprise.Accounting.Business.ComplianceReport.SAFT;
using Enterprise.Accounting.Business.ComplianceReport.TPAR;
using Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro;
using Enterprise.Accounting.DataTransfer.ComplianceReport.IL.OpenFormat;
using Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT;
using Enterprise.Accounting.DataTransfer.ComplianceReport.TW.VAT;
using Enterprise.Accounting.GUI.ComplianceReport.HMRC;
using Enterprise.Accounting.GUI.ComplianceReport.LiquidazioneIVA;
using Enterprise.Accounting.GUI.ComplianceReport.PTRS;
using Enterprise.Accounting.GUI.ComplianceReport.SAFT;
using Enterprise.Accounting.GUI.ComplianceReport.TPAR;
using Enterprise.Accounting.GUI.ComplianceReport.ZMD;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.ComplianceReport
{
	public static class AccComplianceReportGuiHelper
	{
		public static void HandleFinalize(this AccComplianceReport report)
		{
			if (!Env.Security.FinalizeComplianceReport.IsAllowed)
			{
				Env.Security.FinalizeComplianceReport.ShowError();
			}
			else
			{
				if (report.ACR_IsFinalised)
				{
					Globals.Message.Show(Res.GetString("29f8d76e-2a3f-4e67-bc88-ecad9888efb1", "This Report is already finalized."));
				}
				else if (report.ACR_Status != AccComplianceReport.Status.ReportGenerated)
				{
					Globals.Message.ShowError(Res.GetString("9DE6D536-2DE3-4888-BF7B-2611A78B5E55", "Report can be finalized only when it is in 'Generated' status."));
				}
				else if (!report.IsPreviousReportFinalized())
				{
					Globals.Message.ShowError(Res.GetString("6f052dd1-1c95-439f-be56-8edf3b3638db", "You cannot finalize this report because the previous report is not finalized."));
				}
				else if (report.ReportBaseTablePrefix == AccComplianceDocumentHeaderSchema.Constants.Prefix && ZDate.Today <= report.ACR_DateTo)
				{
					Globals.Message.ShowError(Res.GetString("ac7265cd-e999-49af-ba68-7a4704dc3194", "You cannot finalize this report because current date is not bigger than report's end date."));
				}
				else
				{
					if (DialogResult.OK == Globals.Message.ShowConfirmation(
						getFinaliseMessage(),
						Res.GetString("9a3a2720-87eb-4711-a342-f8a1b0824258", "Report Finalizing"),
						Res.GetString("c87336eb-e937-4911-944b-46f660de3965", "Yes"),
						MessageBoxIcon.Warning))
					{
						using (new CursorSwitcher(Cursors.WaitCursor))
						{
							report.Finalise();
						}
					}
				}
			}

			string getFinaliseMessage()
			{
				if (report.SupportsTPAR)
				{
					return Res.GetString("05849154-55ce-40f8-b337-8a2bdfa99a63", @"Ensure that the TPAR lodgement file has been generated and successfully lodged for this TPAR reporting period. After finalizing:
You can no longer add or adjust any balances for this TPAR reporting period.
You cannot regenerate the report for this TPAR reporting period.
The report data is locked and cannot be unlocked.");
				}
				else
				{
					return Res.GetString("f917b6d1-8e72-4d02-be81-d4627bfe7baa", "You would not be able to change this Report after finalizing. Are you sure you want to proceed?");
				}
			}
		}

		public static void HandleGenerate(this AccComplianceReport report, AccComplianceReportForm form = null)
		{
			var securityRight = Env.Security.GenerateComplianceReport;
			if (!securityRight.IsAllowed)
			{
				securityRight.ShowError();
			}
			else
			{
				var msg = Globals.Message;
				if (report.ACR_IsFinalised)
				{
					msg.ShowError(Res.GetString("8f90e292-04a4-4079-8b78-3b2962d06879", "Report data has been finalized. No change can be made after finalization."));
				}
				else if (report.GeneratedByCRQServiceTask)
				{
					msg.ShowError(Res.GetString("929EE8C8-6A79-4D28-820E-812F4D701A3E", "This report is automatically generated in the background when you add or re-queue the report.\r\nWhen ready, status shows as 'Generated' and a report file is saved to eDocs."));
				}
				else if (report.IsServiceTaskQueuedReport && report.ACR_Status != AccComplianceReport.Status.ReportDataQueued)
				{
					msg.ShowError(Res.GetString("846BB552-4B99-43BB-9909-3C1F35012831", "This report can only be generated when it is in status 'Queued' (QUE).\r\nPlease check the status message for additional information and wait for status 'Queued' (QUE) or re-queue if necessary."));
				}
				else
				{
					using (new CursorSwitcher(Cursors.WaitCursor))
					{
						report.GenerateFromQueue();
					}
					if (form != null)
					{
						switch (report.ACR_Status)
						{
							case AccComplianceReport.Status.ReportGenerated:
								msg.ShowInformation(Res.GetString("F186CC6C-1DD8-4206-A17C-A7E55EC40B89", "The Report was successfully generated.\r\nCompliance Report form will be reloaded to update details."));
								break;
							case AccComplianceReport.Status.ReportDataQueued:
								msg.ShowInformation(Res.GetString("ac092a1e-4f25-4b97-9361-ec1991c5499f", "The Report has been queued and will be generated in the background.\r\nOnce the Report status is set to GEN, re-open the Report's form and see the generation result."));
								break;
						}
					}
				}
			}
		}

		public static void HandleReQueue(this AccComplianceReport report)
		{
			if (!Env.Security.ReQueueComplianceReport.IsAllowed)
			{
				Env.Security.ReQueueComplianceReport.ShowError();
			}
			else
			{
				if (report.SupportsReQueue)
				{
					var messageText = Res.GetString("239f78a7-fcd5-426e-b4c7-38e0a7580072", "Queue will be re-created by a background service task for all relevant transactions in the Report's date range.\r\nPlease click Yes to confirm you want to re-queue the report.");
					if (DialogResult.Yes == Globals.Message.Show(messageText, Res.GetString("d0577bce-fab6-43a9-872c-d333776bae04", "Report Re-Queue"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
					{
						using (new CursorSwitcher(Cursors.WaitCursor))
						{
							report.ReQueue();
						}
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("53629d2f-9a70-4ce0-9a5e-f121b30d9a70", "This Report type does not support re-queue."));
				}
			}
		}

		public static readonly MultilingualString ReQueueMenuItemText = ResString.GetMultilingualString("53fab231-102f-4c91-9b1f-594fefa0beb1", "&Re-Queue");
		public static readonly MultilingualString GenerateMenuItemText = ResString.GetMultilingualString("e869703a-a185-43fc-85c3-55ff6848e2e1", "&Generate");
		public static readonly MultilingualString FinalizeMenuItemText = ResString.GetMultilingualString("bcaf24ee-aa7d-441d-b96c-fca29605b53f", "&Finalize");
		public static readonly MultilingualString GenerateSAFTXmlMenuItemText = ResString.GetMultilingualString("f5cac8a7-3a62-4cc9-a880-9d51ef31c7b9", "Generate &Monthly Transaction XML");
		public static readonly MultilingualString GenerateAnnualSAFTXmlMenuItemText = ResString.GetMultilingualString("3719cd6e-53b0-46b8-8661-f854505a248d", "Generate &Annual SAFT XML");
		public static readonly MultilingualString ExportVATMenuItemText = ResString.GetMultilingualString("af615763-3e0a-4f4d-8f9f-1264cc0ba44a", "Export Compliance Report");
		public static readonly MultilingualString ViewAndSubmitMenuItem = ResString.GetMultilingualString("a5565323-2bf5-4a26-9488-d97ced399b89", "View and Submit");
		public static readonly MultilingualString ImportABNsMenuItem = ResString.GetMultilingualString("7E631CD9-472F-417F-8A6E-3AC67331B109", "Import ABNs");
		public static readonly MultilingualString VatSummaryMenuItem = ResString.GetMultilingualString("67036B99-EB26-464B-B7AA-6977BD9C3B14", "VAT Summary Report");
		public static readonly MultilingualString GenerateEsterometroXmlMenuItemText = ResString.GetMultilingualString("a69ad0ee-fe00-4318-97a2-7b5e1ac73dbb", "Generate &Esterometro XML");
		public static readonly MultilingualString ExportOpenFormatFileLabel = ResString.GetMultilingualString("968ad012-72c6-4733-b011-b55596d68a0c", "Export Open Format Files");
		static readonly MultilingualString GenerateAnnualSAFTXmlDialogTitle = ResString.GetMultilingualString("4fa5f1b9-67fa-43bd-975a-e28387c14112", "Generate Annual SAFT XML");

		static void CollectGenerateFileData(AccComplianceReport report)
		{
			using (var accComplianceReportUsageCollector = ObjectFactory.Get<IAccComplianceReportUsageCollectorFactory>().GetAccComplianceReportUsageCollector(report))
			{
				accComplianceReportUsageCollector.AddChangedStatus(report.ACR_Status, report.ACR_Status);
				accComplianceReportUsageCollector.AddLogonUser(Env.CurrentUser);
				accComplianceReportUsageCollector.AddAction(AccComplianceReportUsageCollectorAction.GeneratingFiles);
				accComplianceReportUsageCollector.AddContext(AccComplianceReportUsageCollectorContext.Cargowise);
			}
		}

		public static void HandleGenerateSAFTXml(this AccComplianceReport report, Form parentForm)
		{
			var generateSaftXmlTitle = Res.GetData("0663f633-8aca-4b2c-9d58-9f45d7214c1b", "Generate Monthly Transaction XML");
			if (!Env.Security.ExportComplianceReport.IsAllowed)
			{
				Env.Security.ExportComplianceReport.ShowError();
			}
			else if (!report.SupportsSAFT)
			{
				Globals.Message.Show(Res.GetString("38650a58-cd1a-4d90-81c3-9438e96990e6", "SAFT XML can be generated only for Report Types 'SAF' and 'SAT'."), generateSaftXmlTitle.Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (!report.SupportsSAFTXmlGeneration)
			{
				Globals.Message.Show(Res.GetString("e6c215e3-f610-4fb6-a216-521a46200e31", "Please, the report must be generated or finalized before generating the SAFT XML"), generateSaftXmlTitle.Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else
			{
				var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(report.Company.GC_RN_NKCountryCode) as IInstanceProvider<IComplianceReportGUIActionProvider>)?.Get();
				var validationErrorMessage = provider.ValidateBeforeGenerateSAFT(new[] { report });

				if (!string.IsNullOrEmpty(validationErrorMessage))
				{
					Globals.Message.Show(validationErrorMessage, generateSaftXmlTitle.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else
				{
					var selector = new ReportModeAndCreditorSelector(new BusinessObjectFactory());
					var result = DialogResult.None;
					if (selector.ShouldPopup)
					{
						result =
#if DEBUG
						Globals.IsTest ? DialogResult.OK :
#endif
						ZFormModaliser.ShowDialogAndDispose(new ReportModeAndCreditorSelectorForm(selector));
					}
					else
					{
						result = DialogResult.OK;
					}

					if (result == DialogResult.OK)
					{
						using (var dialog = GetXmlFileSaveDialog(GetExportFileName(report, report.ACR_DateFrom)))
						{
							result =
#if DEBUG
							Globals.IsTest ? DialogResult.OK :
#endif
							dialog.ShowDialog();

							if (result == DialogResult.OK)
							{
								var errorMessage = Res.GetString("b72b3a9a-977e-490a-95aa-953eff6456de", "There was an error during the SAFT XML generation.");
								var errorCaption = Res.GetString("42dcb13e-d90b-4de8-a0f1-973926c8b481", "Invalid SAFT XML file generation");

								try
								{
									var builder = new SAFTXmlBuilder(selector, report);
									var notifications = new NotificationBuffer();
									var eventAndEdocProgress = new ProgressFormSupportableProxy();

									using (new CursorSwitcher(Cursors.WaitCursor))
									using (var wrapper = new ProgressFormWrapper(parentForm, generateSaftXmlTitle, false, false, builder, eventAndEdocProgress))
									{
										wrapper.ShowProgressForm();
										var exportResult = builder.WriteXmlToStream(() => dialog.OpenFile(), dialog.UnmappedFileName);

										report.AddEventLogAndAttachEDocsAndSave(eventAndEdocProgress, report.SAFTSingleXmlExportedEventLog, exportResult.FileNames);

										if (exportResult.HasValidationError)
										{
											Globals.Message.ShowError(errorMessage, errorCaption);
										}
										else
										{
											Globals.Message.ShowInformation(exportResult.Messages.ToString(), Res.GetString("cdeafeb4-c65e-4651-8e88-c689fe5cc6a9", "Successful SAFT XML file generation"));
										}
									}
								}
								catch (Exception ex) when (!ex.IsCriticalException())
								{
									ErrorReporter.ReportOnce(errorMessage, ex);

									Globals.Message.ShowError(string.Format(CultureInfo.InvariantCulture, @"{0}
{1}", errorMessage, ex.Message), errorCaption);
								}
							}
						}
					}
				}
			}
		}

		static string GetExportFileName(AccComplianceReport report, ZDateTime dataFrom, bool isAnnualReport = false)
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == CountryCodes.Norway)
			{
				var customsRegNo = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.GovBusinessCode, CountryCodes.Norway);

				var builder = new StringBuilder();
				builder.Append((NoResString)"SAF-T Financial_");

				if (!string.IsNullOrEmpty(customsRegNo))
				{
					builder.Append(customsRegNo);
					builder.Append("_");
				}

				builder.Append(dataFrom.ToString("yyyyMMdd", Culture.Invariant));
				builder.Append("_");
				builder.Append(report.ACR_DateTo.ToString("yyyyMMdd", Culture.Invariant));
				builder.Append("_");
				builder.Append(ZDateTime.Now.ToString("yyyyMMddHHmmss", Culture.Invariant));
				return builder.ToString();
			}

			var prefix = isAnnualReport ? (NoResString)"Annual" : "";
			return prefix + "ComplianceReportSAFT-" + ZDateTime.Now.ToString("yyyy-MM-dd_HHmm", Culture.Invariant);
		}

		public static void HandleExportPurchaseAndSalesVATDataFile(this AccComplianceReport report, Form parentForm)
		{
			var writer = new PurchaseAndSalesVATWriter(report);
			var filter = (NoResString)"Text Files (*.txt)|*.txt"; // File extension filter

			HandleExportVATDataFile(report, parentForm, writer, ComplianceReportTypes.PurchaseAndSalesVATForTW, filter);
		}

		public static void HandleExportZeroRatedSalesVATDataFile(this AccComplianceReport report, Form parentForm)
		{
			var writer = new ZeroRatedSalesVATWriter(report);

			HandleExportVATDataFile(report, parentForm, writer, ComplianceReportTypes.ZeroRatedSalesVATForTW);
		}

		static void HandleExportVATDataFile(this AccComplianceReport report, Form parentForm, VATDataFileWriter writer, string defaultExtension, string filter = "")
		{
			if (!Env.Security.ExportComplianceReport.IsAllowed)
			{
				Env.Security.ExportComplianceReport.ShowError();
			}
			else if (!report.SupportsExportVATFile)
			{
				Globals.Message.Show(Res.GetString("083EF32B-99BB-436B-AD53-C41D0FD0A5B2", "Please, the report must be generated or finalized before exporting"), Res.GetString("C9F6D6E4-5339-41F5-A89B-42F7CD3DB9D3", "Export VAT File"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else
			{
				using (var dialog = new ZSaveFileDialog())
				{
					dialog.DefaultExt = defaultExtension;
					if (!string.IsNullOrEmpty(filter))
					{
						dialog.Filter = filter;
					}
					dialog.AddExtension = true;
					dialog.FileName = ComplianceDocumentHelper.GetCompanyProxyVATNumber();
					dialog.InitialDirectory = "\\"; // File extension

					DialogResult result;
#if DEBUG
					if (Globals.IsTest)
					{
						result = DialogResult.OK;
					}
					else
#endif
					{
						result = dialog.ShowDialog();
					}

					if (result == DialogResult.OK)
					{
						using (var stream = dialog.OpenFile())
						{
							using (new CursorSwitcher(Cursors.WaitCursor))
							using (var wrapper = new ProgressFormWrapper(parentForm, new ResourceStringData("252C4BD9-32CD-43CA-B25D-8AE485E1FBDB", (NoResString)"Export VAT File"), false, false, writer)) // Resource string key is given
							{
								wrapper.ShowProgressForm();
								writer.WriteDataToStream(stream);
							}

							Globals.Message.ShowInformation(Res.GetString("2070625B-796E-4FED-8578-B29B20405469", "The VAT file was exported successfully."), Res.GetString("8733BCA7-0E31-4CBD-9D77-CF79CE261C39", "Successful VAT file export"));
						}
					}
				}
			}
		}

		static bool IsThereMissingReportMonthInReportCollection(AccComplianceReport[] reports, ZDateTime startDate, ZDate endDate)
		{
			var months = reports.Select(x => x.ACR_DateFrom.Month + (x.ACR_DateFrom.Year - reports.First().ACR_DateFrom.Year) * 12);
			var missingMonths = Enumerable.Range(months.First(), months.Last() - months.First() + 1).ToList().Except(months).ToList();
			return missingMonths.Any();
		}

		static bool IsReportValidToGenerateAnnualSAFTXml(AccComplianceReport report, AccComplianceReport[] reports, ZDateTime startDate, ZDate endDate)
		{
			var notifications = new NotificationBuffer();
			var result = false;

			if (!Env.Security.ExportComplianceReport.IsAllowed)
			{
				Env.Security.ExportComplianceReport.ShowError();
			}
			else if (!report.SupportsSAFT)
			{
				Globals.Message.Show(Res.GetString("38650a58-cd1a-4d90-81c3-9438e96990e6", "SAFT XML can be generated only for Report Types 'SAF' and 'SAT'."), GenerateAnnualSAFTXmlDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (!report.SupportsSAFTXmlGeneration)
			{
				Globals.Message.Show(Res.GetString("8653ddc4-8211-487d-807e-d4a51ef6f086", "Selected report must be generated or finalized before generating the SAFT XML"), GenerateAnnualSAFTXmlDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (report.ACR_Periodicity != Enterprise.Registry.Business.ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod)
			{
				Globals.Message.Show(Res.GetString("380d3929-0920-4124-a8dc-1263474a780d", "The report must be based on the accounting periods to be able to generate the Annual SAFT XML"), GenerateAnnualSAFTXmlDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (report.ACR_DateFrom > ZDate.Today)
			{
				Globals.Message.ShowError(Res.GetString("8ab7e7b1-d114-4d97-a1cc-6e1dda3e6f5a", @"You are attempting to create a SAFT XML file from a compliance report that is in the future.
SAFT files can only be created for current or past periods."), GenerateAnnualSAFTXmlDialogTitle);
			}
			else if ((report.ACR_DateFrom.Year < ZDate.Today.Year && report.ACR_DateFrom.Month != 12)
				|| (report.ACR_DateFrom.Year == ZDate.Today.Year && (report.ACR_DateFrom > ZDate.Today || report.ACR_DateTo < ZDate.Today))
				|| (IsThereMissingReportMonthInReportCollection(reports, startDate, endDate)))
			{
				Globals.Message.ShowError(Res.GetString("6b27f24c-b06f-4750-9056-55666d82a193", @"You are attempting to create a SAFT XML file from a compliance report that does not contain all relevant dates of the corresponding year.
SAFT XML files must contain all transactions of a fiscal year or all transactions up to the date of creation if the file belongs to the current year.
Please generate all relevant compliance reports of the fiscal year before creating the SAFT XML file."), GenerateAnnualSAFTXmlDialogTitle);
			}
			else
			{
				var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(report.Company.GC_RN_NKCountryCode) as IInstanceProvider<IComplianceReportGUIActionProvider>)?.Get();
				var errorMessage = provider.ValidateBeforeGenerateSAFT(reports);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					Globals.Message.Show(errorMessage, GenerateAnnualSAFTXmlDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else if (reports.OrderBy(x => x.ACR_DateFrom).First().ACR_DateFrom.Month != 1)
				{
					result = DialogResult.OK == Globals.Message.Show(
						Res.GetString("d491346b-ab50-43a6-8168-01d216193e74", @"You are attempting to create a SAFT XML file that does not contain all months of the corresponding fiscal year.
This is permitted only when your Login Company started operations in CargoWise is in any month after January.
Please confirm that this is correct."),
						GenerateAnnualSAFTXmlDialogTitle,
					MessageBoxButtons.OKCancel,
						MessageBoxIcon.Warning);
				}
				else
				{
					result = true;
				}
			}

			return result;
		}

		public static ZGuid[] HandleGenerateAnnualSAFTXml(this AccComplianceReport report, Form parentForm)
		{
			var exportedReportPKs = Array.Empty<ZGuid>();

			var periodCalculator = new AccountingPeriodCalculator(report.Factory);
			var financialYear = new ZInt(periodCalculator.GetPeriodFromDate(report.ACR_DateFrom) / 100);
			var startDate = periodCalculator.GetFirstDayForPeriod(periodCalculator.GetFirstPeriodForYear(financialYear));
			var endDate = report.ACR_DateTo;

			var exportedReports = new BusinessObjectFactory().Load<AccComplianceReport>(report.GetAnnualReportsQuery(startDate, endDate));

			if (IsReportValidToGenerateAnnualSAFTXml(report, exportedReports, startDate, endDate))
			{
				exportedReportPKs = exportedReports.Select(x => x.PK).ToArray();

				var selector = new ReportModeAndCreditorSelector(new BusinessObjectFactory());

				var result = DialogResult.None;
				if (selector.ShouldPopup)
				{
					result =
#if DEBUG
					Globals.IsTest ? DialogResult.OK :
#endif
					ZFormModaliser.ShowDialogAndDispose(new ReportModeAndCreditorSelectorForm(selector));
				}
				else
				{
					result = DialogResult.OK;
				}

				if (result == DialogResult.OK)
				{
					using (var dialog = GetXmlFileSaveDialog(GetExportFileName(report, exportedReports.OrderBy(x => x.ACR_DateFrom).First().ACR_DateFrom, true)))
					{
						result =
#if DEBUG
							Globals.IsTest ? DialogResult.OK :
#endif
							dialog.ShowDialog();

						if (result == DialogResult.OK)
						{
							var builder = new SAFTXmlBuilder(selector, exportedReports);
							var notifications = new NotificationBuffer();
							var eventAndEdocProgress = new ProgressFormSupportableProxy();

							using (new CursorSwitcher(Cursors.WaitCursor))
							using (var wrapper = new ProgressFormWrapper(parentForm, new ResourceStringData("f1d3aee2-0f9d-4100-863f-93e3398f0fff", (NoResString)"Generate Annual SAFT XML"), false, false, builder, eventAndEdocProgress)) // Resource string key is given
							{
								wrapper.ShowProgressForm();
								var exportResult = builder.WriteXmlToStream(() => dialog.OpenFile(), dialog.UnmappedFileName);

								if (exportResult.HasValidationError)
								{
									Globals.Message.ShowError(Res.GetString("c3892666-56f7-4aa0-976b-d5fcf1a6b91f", "There was an error during the Annual SAFT XML generation."), GenerateAnnualSAFTXmlDialogTitle);
								}
								else
								{
									Globals.Message.ShowInformation(exportResult.Messages.ToString(), Res.GetString("cdeafeb4-c65e-4651-8e88-c689fe5cc6a9", "Successful SAFT XML file generation"));
								}
								report.AddEventLogAndAttachEDocsAndSave(eventAndEdocProgress, report.SAFTAnnualXmlExportedEventLog, exportResult.FileNames);
							}
						}
					}
				}
			}
			return exportedReportPKs;
		}

		static string FileExportPathRegistryLabel => AccountingMasterFilesRegistry.Instance.ComplianceReportConfigurationRootFileExportPath.HumanReadableRegistryPath();

		public static void HandleExportOpenFormatFile(this AccComplianceReport report, Form parentForm)
		{
			var fileExportPathRegistry = AccountingMasterFilesRegistry.Instance.ComplianceReportConfigurationRootFileExportPath;

			if (fileExportPathRegistry.Value.IsNullOrEmpty())
			{
				Globals.Message.ShowError(Res.GetString("0EB3F5E8-A7B1-4541-8EBD-6B0129391F01", "File Operation Failed. Details: Operation failed when saving to client. Export path not specified. Please fill the registry settings under: {0}.", FileExportPathRegistryLabel));
				return;
			}

			var writer = new OpenFormatFileWriter(report);

			if (IsReportValidToExportOpenFormatFile(report, writer))
			{
				var eventAndEdocProgress = new ProgressFormSupportableProxy();
				string resultPath;

				using (new CursorSwitcher(Cursors.WaitCursor))
				using (var wrapper = new ProgressFormWrapper(parentForm, new ResourceStringData("be3d90ef-148c-42fc-a58c-f074e80df509", ExportOpenFormatFileLabel), false, false, writer, eventAndEdocProgress))
				{
					if (!TryGetOpenFormatCompliancePath(report, fileExportPathRegistry.Value, out resultPath, out var errorMessage, out var reportGenDateTime))
					{
						Globals.Message.ShowError(errorMessage, ExportOpenFormatFileLabel);
						return;
					}

					try
					{
#if DEBUG
						Directory.CreateDirectory(resultPath);
#endif
						OpenFormatInfo info;
						var bkmvdataFileName = "BKMVDATA.txt";
						var filename = Path.Combine(resultPath, bkmvdataFileName);
						using (var stream = ZSaveFileDialog.OpenFile(filename))
						{
							info = writer.WriteBkmvdataToStream(stream);
							stream.Flush();
						}

						var iniFileName = "INI.txt";
						var iniFilename = Path.Combine(resultPath, iniFileName);
						using (var iniStream = ZSaveFileDialog.OpenFile(iniFilename))
						{
							writer.WriteInidataToStream(iniStream, info, resultPath, reportGenDateTime);
							iniStream.Flush();
						}

						using (Stream stream = new MemoryStream())
						{
							writer.WritePdfFileToStream(stream, info, reportGenDateTime, resultPath);

							report.AddEventLogAndAttachEDocsAndSave(eventAndEdocProgress, Res.GetString("36401ce9-6685-408e-a143-0c0c55ff372c", "Purpose: Open format files exported"), new[] { filename, iniFilename },
								new [] { ("REPORT.pdf", stream) });
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						switch (ex)
						{
							case UnauthorizedAccessException or SecurityException:
								Globals.Message.ShowError(Res.GetString("BD47EE2D-26CC-4720-B392-9B27AFF36E28", "File Operation Failed. Details: Operation failed when saving to client. The path entered in the registry is not accessible. Please check the registry settings under: {0}.", FileExportPathRegistryLabel), ExportOpenFormatFileLabel);
								return;
							case DirectoryNotFoundException or IOException:
								Globals.Message.ShowError(Res.GetString("B297E567-913A-4565-A3F1-1BDAC92D3FB3", "File Operation Failed. Details: Operation failed when saving to client. The path entered in the registry is not valid. Please check the registry settings under: {0}.", FileExportPathRegistryLabel), ExportOpenFormatFileLabel);
								return;
							case NotSupportedException:
								Globals.Message.ShowError(Res.GetString("F2D974B6-942E-4756-94BB-96D297E3E012", "File Operation Failed. Details: Operation failed when generating PDF report. {0}", ex.Message), ExportOpenFormatFileLabel);
								return;
							default:
								throw;
						}
					}
				}

				Globals.Message.ShowInformation(Res.GetString("7d8f5a95-8737-409c-89ab-8ae1bada944a", "Your Open Format files were successfully generated and saved.\r\nYou can find them at {0}.", resultPath), ExportOpenFormatFileLabel);
			}
		}

		static bool TryGetOpenFormatCompliancePath(AccComplianceReport report, string rootPath, out string resultPath, out string errorMessage, out ZDateTime reportGenDateTime)
		{
			var genLog = report.Logs.MostRecentLogByEventTime(Events.StatusUpdated, "TYP=GEN");
			reportGenDateTime = genLog?.SL_EventTime ?? ZDateTime.Empty;
			var compliancePath = CreateOpenFormatCompliancePath(report, reportGenDateTime, rootPath);

			if (!IsValidCompliancePath(compliancePath))
			{
				errorMessage = Res.GetString("0A679449-E2BE-4036-8AE7-B6BFEB84C589", "The path {0} is invalid. Please check the customer's data and the registry: {1}.", compliancePath, FileExportPathRegistryLabel);
				resultPath = null;
				return false;
			}

			if (compliancePath.Length > 50)
			{
#if DEBUG
				if (!IgnoreLongPathOnlyForTest)
				{
					errorMessage = Res.GetString("e17a4502-d00f-4aad-82b8-6a7b2fad84d0", "The path {0} is too long. Please check the registry: {1}.", compliancePath, FileExportPathRegistryLabel);
					resultPath = null;
					return false;
				}
#else
				errorMessage = Res.GetString("e17a4502-d00f-4aad-82b8-6a7b2fad84d0", "The path {0} is too long. Please check the registry: {1}.", compliancePath, FileExportPathRegistryLabel);
				resultPath = null;
				return false;
#endif
			}

			errorMessage = null;
			resultPath = compliancePath;
			return true;
		}

		static string CreateOpenFormatCompliancePath(AccComplianceReport report, ZDateTime reportGenDateTime, string registryPathValue)
		{
			var rootFolder = "OPENFRM";
			var customerVat = report.Company.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, CountryCodes.Israel)?.OK_CustomsRegNo ?? ZString.Empty;
			var genYear = reportGenDateTime.ToString("yy");
			var vatYear = customerVat + "." + genYear;
			var reportGenFormattedDateTime = reportGenDateTime.ToString("MMddHHmm");
			var compliancePath = Path.Combine(registryPathValue, rootFolder, vatYear, reportGenFormattedDateTime);
			return compliancePath;
		}

		static bool IsReportValidToExportOpenFormatFile(AccComplianceReport report, OpenFormatFileWriter writer)
		{
			var result = false;
			var writerError = writer.GetReportDataErrorMessage();

			if (!Env.Security.ExportComplianceReport.IsAllowed)
			{
				Env.Security.ExportComplianceReport.ShowError();
			}
			else if (!report.SupportsExportOpenFormatFile)
			{
				Globals.Message.ShowWarning(Res.GetString("7dc791e5-bb5e-4bba-959e-c271fd1ea255", "Please, the report must be generated or finalized before export."), AccComplianceReportGuiHelper.ExportOpenFormatFileLabel);
			}
			else if (!writerError.IsEmpty)
			{
				Globals.Message.ShowError(writerError, AccComplianceReportGuiHelper.ExportOpenFormatFileLabel);
			}
			else
			{
				result = true;
			}

			return result;
		}

#if DEBUG
		internal static bool IgnoreLongPathOnlyForTest { get; set; }

		public
#endif
		static bool IsValidCompliancePath(string path)
		{
			var regex = new Regex(@"\\OPENFRM\\(\d+).(\d{2})\\(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])(0[0-9]|1[0-9]|2[0-3])([0-5][0-9])");
			return regex.IsMatch(path);
		}

		/// <summary>
		/// Handles the Generate XML action for Esterometro (IT) compliance report.
		/// </summary>
		/// <param name="report">The compliance report to generate from.</param>
		/// <param name="parentForm">The parent form, to use when displaying dialogs.</param>
		/// <param name="showXsdValidationErrors">True displays XSD validation errors (default), false will always show the success message.</param>
		/// <param name="writerCreator">Function to create an alternate EsterometroXmlWriter; should only be used for testing purposes.</param>
		/// <returns>Path to each file saved to disk.</returns>
		public static IEnumerable<string> HandleGenerateEsterometroXml(this AccComplianceReport report, Form parentForm, bool showXsdValidationErrors = false, Func<EsterometroXmlWriter> writerCreator = null)
		{
			var validationTitle = Res.GetString("526e1fb6-871d-4990-88f0-c4b2bb8e8777", "Generate Esterometro XML");
			if (!Env.Security.ExportComplianceReport.IsAllowed)
			{
				Env.Security.ExportComplianceReport.ShowError();
				return Enumerable.Empty<string>();
			}
			if (!report.SupportsEsterometro)
			{
				Globals.Message.Show(Res.GetString("7dd9df54-22c6-431b-988e-71a916fdd919", "Esterometro XML can be generated only for Report Type 'EST' and country/region 'IT'."), validationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return Enumerable.Empty<string>();
			}
			if (!report.SupportsEsterometroXmlGeneration)
			{
				Globals.Message.Show(Res.GetString("79c115ba-ed1e-4248-beda-e23f40e9e06b", "Please, the report must be generated or finalized before generating the Esterometro XML"), validationTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return Enumerable.Empty<string>();
			}

			var writer = writerCreator == null ? new EsterometroXmlWriter(report) : writerCreator();
			using (var mutex = writer.CreateMutexForWriting())
			{
				var errorTitle = Res.GetString("bcf9ec88-ed67-47d1-9925-358bbb3e5d92", "Invalid Esterometro XML file generation");
				if (!mutex.Lock())
				{
					Globals.Message.ShowError(Res.GetString("2084c025-adca-4b2e-b376-16bf8f2e2a18", "Another user is generating an Esterometro XML file. Please wait for a few moments and try again."), errorTitle);
					return Enumerable.Empty<string>();
				}

				using (var dialog = GetXmlFolderPickerDialog(description: Res.GetString("384d19a5-7dad-4f71-9a5d-78677bc957a8", "Location to Save Esterometro XML")))
				{
					var result =
#if DEBUG
						Globals.IsTest ? DialogResult.OK :
#endif
						dialog.ShowDialog();

					if (result != DialogResult.OK)
					{
						return Enumerable.Empty<string>();
					}

					var notifications = new NotificationBuffer();
					var initialisationProgress = new ProgressFormSupportableProxy(2);
					var eventAndEdocProgress = new ProgressFormSupportableProxy();
					var unmappedFolder = dialog.UnmappedSelectedPath;
					var filesWritten = new List<string>();

					using (new CursorSwitcher(Cursors.WaitCursor))
					using (var wrapper = new ProgressFormWrapper(parentForm, new ResourceStringData("17002cb3-bd60-4b24-b176-0dcff8557d4c", (NoResString)"Generate Esterometro XML"), false, false, initialisationProgress, writer, eventAndEdocProgress)) // Resource string key is given
					{
						wrapper.ShowProgressForm();

						initialisationProgress.UpdateProgressStatus(Res.GetString("7c0b60fc-f61b-4fc0-9c01-68c62cfbe3cc", "Calculating Files to be Generated..."));
						var totalFiles = writer.CalculateTotalFilesToBeWritten();

						initialisationProgress.UpdateProgressStatus(Res.GetString("dcb45d9c-de8d-4628-ab8c-3cb78e840964", "Loading Compliance Report Data..."));
						writer.InitializeProgressForm();

						for (int fileNumber = 1; fileNumber <= totalFiles; fileNumber++)
						{
							var xmlFilename = System.IO.Path.Combine(unmappedFolder, writer.GetNextFilename());
							using (var stream = ZSaveFileDialog.OpenFile(xmlFilename))
							{
								writer.WriteXmlToStream(stream, fileNumber);
								if (showXsdValidationErrors)
								{
									writer.ValidateXml(stream, notifications, fileNumber);
								}
								writer.IncrementSequenceNumber();
								stream.Flush();
							}

							filesWritten.Add(xmlFilename);
						}

						report.AddEventLogAndAttachEDocsAndSave(eventAndEdocProgress, FormattableString.Invariant($"Purpose: Esterometro XML Exported"), filesWritten);      // event log reference is not translated
					}

					if (showXsdValidationErrors && notifications.HasErrors)
					{
						Globals.Message.ShowWarning(Res.GetString("0f560546-00ee-4ce8-ab82-2a200d0ae1bd", "The Esterometro XML file was generated, but has validation errors."), errorTitle);
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("0f612a5f-3f37-4cb1-adbf-22418f133403", "The Esterometro XML file was generated successfully."), Res.GetString("6d59a33c-7573-44bc-9c71-e5ba58dc61fc", "Successful Esterometro XML file generation"));
					}

					return filesWritten;
				}
			}
		}

		static ZSaveFileDialog GetXmlFileSaveDialog(string fileName)
		{
			var result = new ZSaveFileDialog();
			result.DefaultExt = (NoResString)"xml"; // File extension
			result.FileName = fileName;
			result.AddExtension = true;
			result.Filter = (NoResString)"XML Files | *.xml"; // File extension filter
			result.InitialDirectory = "\\"; // File extension
			return result;
		}

		static ZFolderBrowserDialog GetXmlFolderPickerDialog(string description = "")
		{
			var result = new ZFolderBrowserDialog();
			result.Description = description ?? "";
			result.RootFolder = System.Environment.SpecialFolder.Desktop;
			return result;
		}

		static void AddEventLogAndAttachEDocsAndSave(this AccComplianceReport report, ProgressFormSupportableProxy progressReporter, ZString eventLogReference, IEnumerable<string> pathsToFilesForEDocs, IEnumerable<(string, Stream)> streamsForEDocs = null)
		{
			var eDocsPaths = pathsToFilesForEDocs ?? Enumerable.Empty<string>();
			progressReporter.SetTotalItemsToComplete(1 + eDocsPaths.Count() + (streamsForEDocs?.Count() ?? 0) + 1);       // 1 step for audit event, N steps for eDocs, 1 step for Factory.Save().

			progressReporter.UpdateProgressStatus(Res.GetString("3d2f25ef-5e0e-452a-bda6-e00e6d666ae9", "Saving audit event..."));
			report.Logs.AddNew(Events.DataExport, eventLogReference);

			var docManagerInfo = report.DocManagerInfo();
			foreach (var path in eDocsPaths)
			{
				var fileNameWithoutPath = System.IO.Path.GetFileName(path);
				progressReporter.UpdateProgressStatus(Res.GetString("0492df3d-dde2-4b3f-ad39-5ca0d3e7d3e1", "Saving eDoc for {0}...", fileNameWithoutPath));
				using (var stream = ZOpenFileDialog.OpenFile(path))
				{
					var fileContent = stream.ToByteArray();
					docManagerInfo.AddFileOrDocument(fileContent, fileNameWithoutPath, RefDocTypes.ComplianceReport);
				}
			}

			if (streamsForEDocs != null)
			{
				foreach (var info in streamsForEDocs)
				{
					progressReporter.UpdateProgressStatus(Res.GetString("0492df3d-dde2-4b3f-ad39-5ca0d3e7d3e1", "Saving eDoc for {0}...", info.Item1));
					var fileContent = info.Item2.ToByteArray();
					docManagerInfo.AddFileOrDocument(fileContent, info.Item1, RefDocTypes.ComplianceReport);
				}
			}

			progressReporter.UpdateProgressStatus(Res.GetString("460d7ed1-1833-4e03-9e3c-951098115346", "Completing XML File Generation..."));
			CollectGenerateFileData(report);
			docManagerInfo.Save();
			report.Logs.Factory.Save();
		}

		static ZQuery GetAnnualReportsQuery(this AccComplianceReport report, ZDateTime startDate, ZDate endDate)
		{
			var result = new ZQuery(AccComplianceReportSchema.ACR_GC_Company, report.ACR_GC_Company);
			result.AddToFilter(AccComplianceReportSchema.ACR_ReportType, report.ACR_ReportType);
			result.AddToFilter(AccComplianceReportSchema.ACR_DateFrom, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);
			result.AddToFilter(AccComplianceReportSchema.ACR_DateTo, SQLComparisonOperator.LessThanOrEqualTo, endDate);
			result.OrderBy = AccComplianceReportSchema.Constants.ACR_DateFrom;
			return result;
		}

		public static void HandleMTDViewAndSubmit(this AccComplianceReport report)
		{
			if (report.SupportsMTD && Env.Security.FinalizeComplianceReport.IsAllowed)
			{
				var cursor = Cursor.Current;
				try
				{
					Cursor.Current = Cursors.WaitCursor;

					var submissionData = MTDSubmissionDataColumnsAdapter.LoadMTDSubmissionData(report) ?? new MTDSubmissionDataColumns(report.Factory, report);

					if (!submissionData.IsGroupMemberSubmission)
					{
						using (submissionData.SuspendSettingHasChangesIncludingChildren())
						{
							var response = report.Validation.CheckMTDReportDateRange();
							submissionData.PeriodKey = response.PeriodKey;
							submissionData.ReturnDueDate = response.IsPeriodOpen ? response.DueOn.Date : response.FulfilledOn.Date;
						}
					}
					ZFormModaliser.ShowDialogAndDispose(new MTDSubmissionForm(submissionData));
				}
				finally
				{
					Cursor.Current = cursor;
				}
			}
			else if (!Env.Security.FinalizeComplianceReport.IsAllowed)
			{
				Env.Security.FinalizeComplianceReport.ShowError();
			}
			else if (!report.ACR_IsFinalised)
			{
				Globals.Message.Show(Res.GetString("69862e0f-bdb4-4240-be4f-fb8cce361570", "This Report isn't finalized and MTD data can not be submitted."));
			}
			else
			{
				Globals.Message.Show(Res.GetString("d934f499-ef9b-42f5-9edd-7515c0e0d1a2", "This Report type does not support MTD."));
			}
		}

		public static void HandleTPARViewAndSubmit(this AccComplianceReport report)
		{
			if (report.SupportsTPAR)
			{
				var factory = new BusinessObjectFactory();
				var query = new ZQuery(AccTaxReturnSchema.ATR_ACR_ComplianceReport, report.PK);
				query.OrderBy = AccTaxReturnSchema.Constants.ATR_Version + OrderByClause.Descending;
				var reportData = factory.LoadTop1<TparReport>(query);
				if (reportData == null)
				{
					reportData = factory.New<TparReport>();
					reportData.ATR_ACR_ComplianceReport = report.PK;
				}
				ZFormModaliser.ShowDialogAndDispose(new TPARForm(reportData));
			}
			else
			{
				Globals.Message.Show(Res.GetString("3f6799f2-d332-40d3-afc6-74249fef1293", "This Report type does not support TPAR."));
			}
		}

		public static void HandlePTRSSmallBusinessViewAndSubmit(this AccComplianceReport report)
		{
			if (report.SupportsPTRSSmallBusiness)
			{
				report.HandleViewAndSubmitTaxReturn<PtrsReport>((x) => new PTRSForm(x));
			}
			else
			{
				Globals.Message.Show(Res.GetString("e703da29-fefa-4e7d-867d-4342175fd21b", "This Report type does not support PTRS Small Business."));
			}
		}

		public static void HandlePTRSAllPaymentsViewAndSubmit(this AccComplianceReport report)
		{
			if (report.SupportsPTRSAllPayments)
			{
				report.HandleViewAndSubmitTaxReturn<PtrsAllPaymentsReport>((x) => new PTRSAllPaymentsForm(x));
			}
			else
			{
				Globals.Message.Show(Res.GetString("125f65cb-1109-4f2c-a3a9-6359c4bd6d41", "This Report type does not support PTRS All Payments."));
			}
		}

		static void HandleViewAndSubmitTaxReturn<T>(this AccComplianceReport report, Func<T, ZChildForm> createForm) where T : AccTaxReturn
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(AccTaxReturnSchema.ATR_ACR_ComplianceReport, report.PK);
			query.OrderBy = AccTaxReturnSchema.Constants.ATR_Version + OrderByClause.Descending;
			var reportData = factory.LoadTop1<T>(query);
			if (reportData == null)
			{
				reportData = factory.New<T>();
				reportData.ATR_ACR_ComplianceReport = report.PK;
			}
			ZFormModaliser.ShowDialogAndDispose(createForm(reportData));
		}

		public static void HandleImportABNs(this AccComplianceReport report)
		{
			if (report.SupportsImportABNs)
			{
				ZFormModaliser.ShowDialogAndDispose(new ImportABNsForm(report));
			}
			else
			{
				Globals.Message.Show(Res.GetString("CBD99301-3D6F-469A-AB5D-243F2DEFC7F6", "This report does not support ABN import."));
			}
		}

		public static void HandleLiquidazioneIVAViewAndSubmit(this AccComplianceReport report)
		{
			if (report.SupportsLiquidazioneIVA)
			{
				var cursor = Cursor.Current;
				try
				{
					Cursor.Current = Cursors.WaitCursor;

					var submissionData = LIQSubmissionDataColumnsAdapter.LoadLIQSubmissionData(report) ?? new LIQSubmissionDataColumns(report.Factory, report);

					ZFormModaliser.ShowDialogAndDispose(new VATSummaryForm(submissionData));
				}
				finally
				{
					Cursor.Current = cursor;
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("651FC52D-FDCE-4EBD-9D9C-0FDA7A18A606", "This Report type does not support VAT Summary."));
			}
		}

		public static void HandleZMDViewAndSubmit(this AccComplianceReport report)
		{
			if (report.SupportsZMGermany)
			{
				ZFormModaliser.ShowDialogAndDispose(new ZMGermanyForm(report));
			}
			else
			{
				Globals.Message.Show(Res.GetString("886C8773-18FE-4195-90E2-59BC793DFD8", "This Report type does not support ZMD."));
			}
		}
	}
}
