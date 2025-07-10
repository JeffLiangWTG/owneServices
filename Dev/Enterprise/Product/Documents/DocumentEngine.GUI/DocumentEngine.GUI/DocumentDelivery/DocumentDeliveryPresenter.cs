using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	class DocumentDeliveryPresenter
	{
		public DocumentDeliveryPresenter(IDocumentDeliveryView view, PrintTask printTask, DeliveryInstructions deliveryInstructions)
		{
			this.view = view;
			this.printTask = printTask;
			this.deliveryInstructions = deliveryInstructions;

			if (ShouldHideLanguageSelectionDropDown())
			{
				view.HideLanguageSelectionDropDown();
			}

			if (ShouldHidePageRangesPanel())
			{
				view.HidePageRangesPanel();
			}

			if (ShouldHideBackgroundDeliveryCheckBox())
			{
				view.HideBackgroundDeliveryCheckBox();
			}

			view.SaveAsButtonClicked += new EventHandler(DoSaveAsButtonClicked);
			view.ViewClosed += new FormClosedEventHandler(DoDeliveryViewClosed);
		}

		readonly IDocumentDeliveryView view;
		readonly PrintTask printTask;
		readonly DeliveryInstructions deliveryInstructions;

		bool? lastSaveAsExport;

		void DoSaveAsButtonClicked(object sender, EventArgs e)
		{
			try
			{
				if (deliveryInstructions.IsDeliveringFormDocument || printTask.ParentMenuCommand is DocumentCommand)
				{
					SaveDocuments();
				}
				else
				{
					SaveReport();
				}
			}
			catch (IOException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			catch (UnauthorizedAccessException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void DoDeliveryViewClosed(object sender, FormClosedEventArgs e)
		{
			printTask?.ResetCachedReports();
		}

		void SaveDocuments()
		{
			var deliverables = deliveryInstructions.DocumentsToBeDelivered.OfType<IDeliverable>().Where(iDeliverable => iDeliverable.IncludedInPrint);

			if (deliverables.Any())
			{
				deliveryInstructions.DocPack.Language = deliveryInstructions.Language;
				var info = view.GetSaveAsFileName();
				if (info != null)
				{
					bool savedSuccessfully;
					var autoSwitchedToXlsxSuccessfully = false;
					try
					{
						savedSuccessfully = SaveDocumentsCore(deliverables, info);
					}
					catch (ExcelLimitationForThisFileFormatException) when (info.Type == SaveAsFileType.Xls)
					{
						ChangeFileFromXlsToXlsx(info);
						savedSuccessfully = SaveDocumentsCore(deliverables, info);
						autoSwitchedToXlsxSuccessfully = true;
					}

					AfterSave(savedSuccessfully, autoSwitchedToXlsxSuccessfully, info.DisplayFileName);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("7635C7E4-A371-4679-AA85-283D02B69CEF", "There is no document included to be saved."));
			}
		}

		bool SaveDocumentsCore(IEnumerable<IDeliverable> deliverables, SaveAsFileInfo info)
		{
			using (var stream = info.FileStream)
			{
				var strategy = deliveryInstructions.IsDeliveringFormDocument
					? (ICreateDeliveryInfoStrategy)ObjectFactory.Get<IFormDeliveryInfoStrategy>()
					: new DefaultCreateDeliveryInfoStrategy();
				var docDeliveryContact = deliveryInstructions.OfficialRecipient ?? new DocDeliveryContact(deliveryInstructions.Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.Print };
				docDeliveryContact.AttachmentType = info.Type.ToString().ToUpper(CultureInfo.InvariantCulture);
				var deliveryInfos = deliverables.Select(deliverable => strategy.CreateDeliveryInfo(deliverable, docDeliveryContact, deliveryInstructions)).WhereNotNull().ToArray();

				if (deliveryInfos.Length == 0)
				{
					return false;
				}
				var deliveryMethod = new DeliveryMethod();
				deliveryInfos.ForEach(deliveryMethod.AddFile);

				if (deliveryMethod.FileCount == 0)
				{
					return false;
				}
				using (var excelInterface = new ExcelInterface())
				using (Res.TemporarilySwitchLanguage(deliveryInstructions.Language))
				using (var mergedStream = deliveryMethod.FileCount > 1 ? deliveryMethod.MergeFilesIntoOneXLS() : deliveryMethod.Infos[0].FileContents)
				{
					SaveStream(excelInterface, mergedStream, info.Type, stream, deliveryInfos.First());
				}
				return true;
			}
		}

		void SaveReport()
		{
			var info = view.GetSaveAsFileName();
			if (info != null)
			{
				using (var stream = info.FileStream)
				{
					if (deliveryInstructions.DocPack.Count > 0 && deliveryInstructions.DocPack[0] is Report report)
					{
						bool savedSuccessfully;
						var autoSwitchedToXlsxSuccessfully = false;

						switch (info.Type)
						{
							case SaveAsFileType.Csv:
							case SaveAsFileType.CsvWithColumnHeadings:
							case SaveAsFileType.Xml:
								if (lastSaveAsExport != null && !lastSaveAsExport.Value)
								{
									deliveryInstructions.ResetCachedReports();
								}

								savedSuccessfully = SaveAsExport(info.Type, stream, report, deliveryInstructions);
								lastSaveAsExport = true;
								break;

							default:
								try
								{
									if (lastSaveAsExport != null && lastSaveAsExport.Value)
									{
										deliveryInstructions.ResetCachedReports();
									}

									savedSuccessfully = SaveAsOther(info.Type, stream, report);
									lastSaveAsExport = false;
								}
								catch (ExcelLimitationForThisFileFormatException) when (info.Type == SaveAsFileType.Xls)
								{
									ChangeFileFromXlsToXlsx(info);
									using (var newStream = info.FileStream)
									{
										savedSuccessfully = SaveAsOther(info.Type, newStream, report);
									}
									if (savedSuccessfully)
									{
										autoSwitchedToXlsxSuccessfully = true;
									}
								}
								break;
						}

						AfterSave(savedSuccessfully, autoSwitchedToXlsxSuccessfully, info.DisplayFileName);
					}
				}
			}
		}

		void ChangeFileFromXlsToXlsx(SaveAsFileInfo saveAsFileInfo)
		{
			saveAsFileInfo.FileStream.Close();
			if (File.Exists(saveAsFileInfo.DisplayFileName))
			{
				File.Delete(saveAsFileInfo.DisplayFileName);
			}

			saveAsFileInfo.DisplayFileName = Path.ChangeExtension(saveAsFileInfo.DisplayFileName, "XLSX");
			saveAsFileInfo.FileStream = new FileStream(saveAsFileInfo.DisplayFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
			saveAsFileInfo.Type = SaveAsFileType.Xlsx;
		}

		void AfterSave(bool savedSuccessfully, bool autoSwitchedToXlsxSuccessfully, string displayFileName)
		{
			if (savedSuccessfully)
			{
				var autoSwitchedMessage = autoSwitchedToXlsxSuccessfully ? ("\r\n" + Res.GetString("58F597F8-2536-4B28-937D-FB84E0B62CB4", "Note: This report exceeded the number of rows (65,535), columns (256) or characters in a formula (1024) supported by Excel 97-2003. The report was saved as XLSX (Excel 2007) instead.")) : string.Empty;
				Globals.Message.Show(Res.GetString("19dc2595-e5d9-4f3f-9b3b-420fd0c0f1a3", "{0} has been successfully saved.{1}", displayFileName, autoSwitchedMessage));
			}
			else
			{
				deliveryInstructions.ResetCachedReports();
			}
		}

		bool SaveAsOther(SaveAsFileType type, Stream stream, Report report)
		{
			bool success = true;

			var strategy = new DefaultCreateDeliveryInfoStrategy();
			var docDeliveryContact = deliveryInstructions.OfficialRecipient;
			docDeliveryContact.AttachmentType = type.ToString().ToUpper(CultureInfo.InvariantCulture);

			var deliveryInfo = strategy.CreateDeliveryInfo(report, docDeliveryContact, deliveryInstructions);
			if (deliveryInfo != null)
			{
				using (var fileContents = deliveryInfo.FileContents)
				using (var excelInterface = new ExcelInterface())
				using (Res.TemporarilySwitchLanguage(deliveryInstructions.Language))
				{
					SaveStream(excelInterface, fileContents, type, stream, deliveryInfo);
					if (!report.ContainsDataRows)
					{
						success = false;
						DisplayEmptyReportMessage(report);
					}
				}
			}
			else
			{
				success = false;
			}

			return success;
		}

		protected virtual void SaveStream(ExcelInterface excelInterface, Stream fileContents, SaveAsFileType type, Stream stream, DeliveryInfo deliveryInfo)
		{
			DocumentSaveHelper.SaveStream(excelInterface, fileContents, type, stream, deliveryInfo);
		}

		bool SaveAsExport(SaveAsFileType type, Stream stream, Report report, DeliveryInstructions deliveryInstructions)
		{
			bool success = true;

			ReportDataExportStrategy strategy;
			if (type == SaveAsFileType.Csv || type == SaveAsFileType.CsvWithColumnHeadings)
			{
				strategy = new CsvCreateDeliveryInfoStrategy() { IncludeColumnHeadings = type == SaveAsFileType.CsvWithColumnHeadings };
			}
			else if (type == SaveAsFileType.Xml)
			{
				strategy = new XmlCreateDeliveryInfoStrategy();
			}
			else
			{
				throw new InvalidOperationException("Unsupported export file type");
			}

			var deliveryInfo = strategy.CreateDeliveryInfo(report, null, deliveryInstructions);

			if (deliveryInfo == null || !deliveryInfo.HasDataForCurrentFileFormat)
			{
				success = false;
			}
			else
			{
				var bytes = deliveryInfo.FileContents.CopyToByteArray();

				if (bytes != null && bytes.Length > 0)
				{
					stream.Write(bytes, 0, bytes.Length);
				}
			}

			if (!report.ContainsDataRows)
			{
				success = false;
				DisplayEmptyReportMessage(report);
			}

			return success;
		}

		void DisplayEmptyReportMessage(Report report)
		{
			var message = Res.GetString("f827e658-1ed0-45de-9da9-f07a108d267e", @"Report '{0}' does not contain any data. The generated file should be discarded.", report.MenuItem != null ? report.MenuItem.SU_MenuName : report.Name);
			Globals.Message.ShowInformation(message, Res.GetString("02570db7-0051-4015-bdf2-41e4d2ba6e25", "No data can be exported"));
		}

		bool ShouldHideLanguageSelectionDropDown()
		{
			if (printTask != null)
			{
				if (printTask.UseStreamMode)
				{
					return !printTask.SupportsLanguageSelectionOverride;
				}

				foreach (var documentPack in printTask.GetDocumentPacks())
				{
					if (documentPack.SupportsLanguageSelection)
					{
						return false;
					}
				}
				return true;
			}
			else
			{
				return !deliveryInstructions.DocPack.SupportsLanguageSelection;
			}
		}

		bool ShouldHidePageRangesPanel()
		{
			if (DocumentsDataRegistry.Instance.EnableSpecificPageRangesPrintingOption.Value)
			{
				if (printTask != null && printTask.GetDocumentPacks().Count() == 1)
				{
					var documentPack = deliveryInstructions.DocPack;
					if (documentPack != null && documentPack.OfType<Report>().Count() == 1)
					{
						var report = documentPack.OfType<Report>().First();
						if (report.Template != null)
						{
							using (var stream = report.Template.GetAsTemplateStream())
							{
								if (stream.Length != 0 && report.CanSpecifyPageRanges)
								{
									return false;
								}
							}
						}
					}
				}
			}
			return true;
		}

		bool ShouldHideBackgroundDeliveryCheckBox()
		{
			var command = deliveryInstructions?.DocPack?.StmMenuCommand;

			if (command is ReportCommand)
			{
				return false;
			}

			var deliverables = deliveryInstructions?.DocumentsToBeDelivered?
					.OfType<IDeliverable>();

			var hasNonUniqueDeliverable = deliverables?
					.GroupBy(d => d.Identifier)
					.Any(g => g.Count() > 1 || string.IsNullOrEmpty(g.Key));
			if (command == null || hasNonUniqueDeliverable == true)
			{
				return true;
			}

			var deliverablesThatHaveInvalidSourceBO = deliveryInstructions?.DocumentsToBeDelivered?.OfType<Report>().Where(r => !r.IsIdentifiablePKValid)?.ToArray();
			if (deliverablesThatHaveInvalidSourceBO?.Length > 0)
			{
				return true;
			}

			if (command is DocumentCommand documentCommand && !documentCommand.IsFromMenu)
			{
				return true;
			}

			return false;
		}
	}
}
