using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	public class DeliveryMethod
	{
		string reportTitle = "";

		internal bool ReportHasNoDataRows;
		internal EmptyReportContingency EmptyReportContingency;

		string CompanySignature => (GlbCompany.CurrentCompany?.GC_Name + "\n" + GlbBranch.CurrentBranch?.GB_BranchName
					+ ((GlbBranch.CurrentBranch?.GB_Address1.IsEmpty ?? true) ? "" : ("\n" + GlbBranch.CurrentBranch.GB_Address1))
					+ ((GlbBranch.CurrentBranch?.GB_Address2.IsEmpty ?? true) ? "" : ("\n" + GlbBranch.CurrentBranch.GB_Address2))
					+ ((GlbBranch.CurrentBranch?.GB_City.IsEmpty ?? true) ? "" : ("\n" + GlbBranch.CurrentBranch.GB_City))
					+ ((GlbBranch.CurrentBranch?.GB_State.IsEmpty ?? true) ? "" : ("\n" + GlbBranch.CurrentBranch.GB_State + ((GlbBranch.CurrentBranch?.GB_PostCode.IsEmpty ?? true) ? "" : ("\n" + GlbBranch.CurrentBranch.GB_PostCode))))
					+ ((GlbBranch.CurrentBranch?.GB_Phone.IsEmpty ?? true) ? "" : ("\n" + Res.GetString("d133fd73-614a-47ca-9bb4-98d362817fc1", "Phone: {0}", GlbBranch.CurrentBranch.GB_Phone)))
					+ ((GlbBranch.CurrentBranch?.GB_Fax.IsEmpty ?? true) ? "" : ("\n" + Res.GetString("0f57abb8-e8ae-4623-95f7-0184cc02f724", "Fax: {0}", GlbBranch.CurrentBranch.GB_Fax))));

		#region Delivery Method Logic

		public static DeliveryMethod FromContact(DocDeliveryContact contact, DeliveryInstructions deliveryInstructions)
		{
			DeliveryMethod deliveryMethod = null;

			switch (deliveryInstructions.Destination)
			{
				case DeliveryInstructionDestination.None:
					deliveryMethod = new DeliveryMethods.DeliveryMethod();
					break;

				case DeliveryInstructionDestination.Disk:
					deliveryMethod = new DeliveryMethods.Disk
					{
						OutputDirectory = deliveryInstructions.OutputDirectory,
						OutputFormatOverride = deliveryInstructions.OutputFormatOverride,
					};
					break;
				case DeliveryInstructionDestination.Memory:
					deliveryMethod = new DeliveryMethods.Memory(deliveryInstructions.OutputForMemoryDeliveryMethod)
					{
						OutputFormatOverride = deliveryInstructions.OutputFormatOverride
					};
					break;
				case DeliveryInstructionDestination.Preview:
				case DeliveryInstructionDestination.DocConfigPreview:
					deliveryMethod = new DeliveryMethods.ExcelPreview(deliveryInstructions.ParentForm);
					break;

				case DeliveryInstructionDestination.Print:
					deliveryMethod = new DeliveryMethods.Printer(deliveryInstructions.PrinterDelivery);
					break;

				case DeliveryInstructionDestination.DocManager:
					deliveryMethod = new DeliveryMethods.DocManager();
					deliveryInstructions.SendToEDocs = true;
					break;

				case DeliveryInstructionDestination.Auto:
					deliveryMethod = AutoDetectDeliveryMethod(contact, deliveryInstructions);
					break;

				case DeliveryInstructionDestination.TakenFromContact:
					{
						if (contact != null)
						{
							switch (contact.DeliveryMethod)
							{
								case Core.Constants.ContactNotifyModes.Email:
								case Core.Constants.ContactNotifyModes.EPrint:
									deliveryMethod = new DeliveryMethods.Email(contact);
									break;

								case Core.Constants.ContactNotifyModes.Fax:
									deliveryMethod = new DeliveryMethods.Fax(contact);
									break;

								case Core.Constants.ContactNotifyModes.Print:
									deliveryMethod = new DeliveryMethods.Printer(deliveryInstructions.PrinterDelivery);
									break;

								case Core.Constants.ContactNotifyModes.EDoc:
									deliveryMethod = new DeliveryMethods.DocManager(contact);
									break;

								case Core.Constants.ContactNotifyModes.Electronic:
									deliveryMethod = new DeliveryMethods.Electronic(contact);
									break;

								case Core.Constants.ContactNotifyModes.Ftp:
									deliveryMethod = new DeliveryMethods.Ftp(contact, deliveryInstructions.ParentGuid);
									break;
							}
						}
						else
						{
							deliveryMethod = new DeliveryMethods.Printer(deliveryInstructions.PrinterDelivery);
						}
						break;
					}
#if DEBUG
				case DeliveryInstructionDestination.DummyDestinationForTesting:
					deliveryMethod = new DummyMethodForTesting();
					break;
#endif

				default:
					throw new InvalidOperationException("Support for DestinationType <" + deliveryInstructions.Destination + "> has not been implemented.");
			}

			deliveryMethod?.SetPropertiesFromDeliveryInstructions(deliveryInstructions);
			if (deliveryMethod != null)
			{
				deliveryMethod.Instructions = deliveryInstructions;
			}
			return deliveryMethod;
		}

		protected virtual void SetPropertiesFromDeliveryInstructions(DeliveryInstructions instructions)
		{
			reportTitle = instructions.DocumentPackTitle;
			SendToEDocs = instructions.SendToEDocs;
			Language = instructions.Language;
		}

		internal bool SendToEDocs { get; private set; }
		internal string Language { get; private set; }
		internal DeliveryInstructions Instructions { get; set; }

		/// <summary>
		/// Adds a file to the list of Excel documents that this DeilveryMethod should deliver.
		/// </summary>
		/// <param name="deliveryInfo">A DeliveryInfo for the Excel file to be delivered.</param>
		public virtual void AddFile(DeliveryInfo deliveryInfo)
		{
			DeliveryInfos.Add(deliveryInfo);
		}

		internal void AddFileAtBeginning(DeliveryInfo deliveryInfo)
		{
			DeliveryInfos.Insert(0, deliveryInfo);
		}

		/// <summary>
		/// Delivers all the Excel templates which have been added to this method.
		/// </summary>
		public void Deliver(INotifications notifications = null)
		{
			bool shouldDeliver = true;

			if (ReportHasNoDataRows && EmptyReportContingency != null)
			{
				switch (EmptyReportContingency.Type.Trim())
				{
					case EmptyReportContingencyList.Codes.SendReport:
					case "":
						shouldDeliver = true;
						break;

					case EmptyReportContingencyList.Codes.SendEmailNotification:
						if (!EmptyReportContingency.EmailAddress.IsEmpty || !EmptyReportContingency.CcEmailAddress.IsEmpty || !EmptyReportContingency.BccEmailAddress.IsEmpty)
						{
							var email = new EmailDef();

							var subjectLine = DeliveryInfos.Count > 0 && !string.IsNullOrEmpty(DeliveryInfos[0].EmailSubjectLine)
								? DeliveryInfos[0].EmailSubjectLine
								: Res.GetString("2DFA0753-0F1E-4004-87E6-9B71314BDC07", "{0} - {1} - {2}", GlbCompany.CurrentCompany.GC_Name, GlbBranch.CurrentBranch.GB_BranchName, reportTitle);
							email.Subject = Res.GetString("0A7A2E02-7BA6-4EC9-8B30-6DA27F28B1BD", "{0} not delivered {1}", subjectLine, ZDateTime.Now);

							var signature = DeliveryInfos.Count > 0 && !string.IsNullOrEmpty(DeliveryInfos[0].EmailSignature) ? DeliveryInfos[0].EmailSignature : CompanySignature;
							email.Body = Res.GetString("0a2f3bc2-5c0e-4491-b949-e703fd16c503", "Report '{0}' has been run at {1}. The resulting document was empty and therefore has not been delivered.\r\n\r\n{2}", reportTitle, ZDateTime.Now, signature);

							if (!EmptyReportContingency.EmailAddress.IsEmpty)
							{
								email.AddRecipientForUserCommunication(EmptyReportContingency.EmailAddress.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), RecipientDef.RecipientTypes.TO);
							}
							if (!EmptyReportContingency.CcEmailAddress.IsEmpty)
							{
								email.AddRecipientForUserCommunication(EmptyReportContingency.CcEmailAddress.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), RecipientDef.RecipientTypes.CC);
							}
							if (!EmptyReportContingency.BccEmailAddress.IsEmpty)
							{
								email.AddRecipientForUserCommunication(EmptyReportContingency.BccEmailAddress.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), RecipientDef.RecipientTypes.BCC);
							}

							Env.OutgoingMailManager.CreateAndSave(email);
						}

						shouldDeliver = false;
						break;

					default:
						shouldDeliver = false;
						break;
				}
			}

			if (shouldDeliver)
			{
				try
				{
					DeliverCore(notifications);
				}
				catch (FlexCelException ex) when (ExcelLimitationForThisFileFormatException.IsSupportedExcelLimitationException(ex, out var _))
				{
					PrintTaskUIProvider.ShowWarning(ErrorCaption, ex.ToString());
				}
				ValidateDeliveryInfos();
			}
		}

		protected virtual void DeliverCore(INotifications notifications = null)
		{
		}

		protected virtual void ValidateDeliveryInfos()
		{
			ValidateDeliveryInfosMerge();
		}

		internal string ErrorCaption => Res.GetString("7703e852-1fa9-4492-a05c-540b467a2f80", "Couldn't merge all templates into one print job");

		void ValidateDeliveryInfosMerge()
		{
			ZStringBuilder deliveryInfosThatFailedToMerge = new ZStringBuilder();

			foreach (DeliveryInfo deliveryInfo in DeliveryInfos)
			{
				if (deliveryInfo.HasMergeIntoPrimaryExcelTemplateFailed && (deliveryInfo.DeliveryFormat == DeliveryInfo.DeliveryFormats.Document || deliveryInfo.DeliveryFormat == DeliveryInfo.DeliveryFormats.Report))
				{
					deliveryInfosThatFailedToMerge.Append("     " + Res.GetString("2d9a0df5-9199-436f-bfd3-09105c1d90b6", "[{0}]", deliveryInfo.Name));
				}
			}

			if (!deliveryInfosThatFailedToMerge.IsEmpty)
			{
				ZStringBuilder warningMessage = new ZStringBuilder();

				warningMessage.Append(
	Res.GetString("92a1238e-5b1c-4bd9-acbe-a8af0a3dfa02", @"The following templates were not able to be merged into one print job.") + @"
");

				warningMessage.Append(deliveryInfosThatFailedToMerge.ToStringWithNewLineBetweenAppends());

				warningMessage.Append(
	@"
" + Res.GetString("ed1d26ce-55db-4187-a5be-75328c484f1d", @"These templates are not shown in the preview, but will be delivered as separate print jobs if you choose to deliver.

To avoid this problem, either:-
   * Don't use embedded OLE objects in your templates -or-
   * Don't use templates with embedded OLE objects as part of a multi-template document pack.

NB: Using the Cover Sheet creates a multi-template document pack behind the scenes."));

				PrintTaskUIProvider.ShowWarning(ErrorCaption, warningMessage.ToStringWithNewLineBetweenAppends());
			}
		}

		public IPrintTaskUIProvider PrintTaskUIProvider
		{
			get { return printTaskUIProvider ?? (printTaskUIProvider = PrintTaskUIProviderFactory.Create()); }
		}

		IPrintTaskUIProvider printTaskUIProvider;

		public int FileCount
		{
			get { return DeliveryInfos.Count; }
		}

		public IReadOnlyList<DeliveryInfo> Infos
		{
			get { return DeliveryInfos; }
		}

		protected List<DeliveryInfo> DeliveryInfos
		{
			get { return deliveryInfos ?? (deliveryInfos = new List<DeliveryInfo>()); }
		}
		List<DeliveryInfo> deliveryInfos;

		internal void ReleaseDeliveryInfos()
		{
			foreach (var deliveryInfo in DeliveryInfos)
			{
				deliveryInfo.ReleaseFileContentsWhenSafe();
			}
		}

#if DEBUG
		internal List<DeliveryInfo> DeliveryInfosForTesting
		{
			get { return DeliveryInfos; }
		}
#endif

		#endregion

		#region Implementation

		internal string EmailSubjectForConsolidateReports = "";
		internal string AttachedFileNameForConsolidateReports = "";

		public Stream MergeFilesIntoOneXLS(string fileFormat = Core.Constants.FileFormats.XLS)
		{
			var tfileFormat = string.Equals(fileFormat, Core.Constants.FileFormats.XLSX, StringComparison.OrdinalIgnoreCase) ? TFileFormats.Xlsx : TFileFormats.Xls;

			var outFile = XlsFileGenerator();
			var result = new MemoryStream();
			outFile.NewFile(1);
			outFile.SetColorPalette(7, Color.FromArgb(-657931));   // Richard Kroon's magic number, it should be a kind of gray for shades
			DeliveryInfo coverSheet = null;
			var insertedDeliveryInfos = new List<DeliveryInfo>();

			var deliveryInfosThatNeedMerge = DeliveryInfos.Where(d =>
				d.DeliveryFormat == DeliveryInfo.DeliveryFormats.Document ||
				d.DeliveryFormat == DeliveryInfo.DeliveryFormats.Report).ToList();

			for (var deliveryInfoNumber = deliveryInfosThatNeedMerge.Count - 1; deliveryInfoNumber >= 0; deliveryInfoNumber--)
			{
				var deliveryInfo = deliveryInfosThatNeedMerge[deliveryInfoNumber];
				if (deliveryInfo.IsCoverSheet)
				{
					coverSheet = deliveryInfo;
				}
				else
				{
					InsertIntoXLS(outFile, deliveryInfoNumber + 1 + " - ", deliveryInfo);
					insertedDeliveryInfos.Add(deliveryInfo);
				}
			}

			if (coverSheet != null)
			{
				InsertIntoXLS(outFile, "", coverSheet);
			}

			if (outFile.SheetCount > 1)
			{
				outFile.ActiveSheet = outFile.SheetCount;
				outFile.SheetVisible = TXlsSheetVisible.VeryHidden;
				outFile.ActiveSheet = 1;
				try
				{
					outFile.Save(result, tfileFormat);
				}
				catch (FlexCelException ex) when (ExcelLimitationForThisFileFormatException.IsSupportedExcelLimitationException(ex, out var limitationType))
				{
					if (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.Value)
					{
						SaveFileAsXlsx(outFile, result);
					}
					else if (Globals.CanShowDialogs)
					{
						var answer = ExcelLimitationsHelper.Messages.ShowTooManyForExcel2003WithFormatSwitchQuestion(limitationType.GetValueOrDefault());
						if (answer == ZDialogResult.Yes)
						{
							SaveFileAsXlsx(outFile, result);
						}
						else
						{
							throw;
						}
					}
					else
					{
						throw;
					}
				}
			}

			return result;

			void SaveFileAsXlsx(XlsFile file, MemoryStream stream)
			{
				stream.SetLength(0);
				stream.Position = 0;
				file.Save(stream, TFileFormats.Xlsx);
			}
		}

		internal Func<XlsFile> XlsFileGenerator
		{
			get { return xlsFileGenerator ?? (() => new XlsFile()); }
			set { xlsFileGenerator = value; }
		}
		Func<XlsFile> xlsFileGenerator;

#if DEBUG
		internal
#endif
		protected Stream MergeFilesIntoOneTIF()
		{
			MemoryStream result;
			DeliveryInfo coverSheet = null;
			var util = ObjectFactory.Get<IDocumentUtilities>();
			using (var resultFile = TempFile.NewWithExtension("tif"))
			{
				for (var deliveryInfoNumber = DeliveryInfos.Count - 1; deliveryInfoNumber >= 0; deliveryInfoNumber--)
				{
					var deliveryInfo = DeliveryInfos[deliveryInfoNumber];
					deliveryInfo.FileContents.Position = 0;
					if (deliveryInfo.DeliveryFormat == DeliveryInfo.DeliveryFormats.Document && deliveryInfo.IsCoverSheet)
					{
						coverSheet = deliveryInfo;
					}
					else if (deliveryInfo.DeliveryFormat == DeliveryInfo.DeliveryFormats.TIFF)
					{
						using (var convertedFile = TempFile.NewWithExtension("tif"))
						using (var fileNameOfPagesToAppend = TempFile.NewWithExtension("tif"))
						{
							FileSaveHelper.SaveMultiPageTifFile(Image.FromStream(deliveryInfo.FileContents), fileNameOfPagesToAppend.Filename);
							util.ConvertFileToTiff(fileNameOfPagesToAppend.Filename, convertedFile.Filename);
							util.AppendMultiPageImageToFile(convertedFile.Filename, resultFile.Filename);
						}
					}
				}
				if (coverSheet != null)
				{
					coverSheet.FileContents.Position = 0;
					var coverSheetBytes = new byte[coverSheet.FileContents.Length];
					coverSheet.FileContents.Read(coverSheetBytes, 0, (int)coverSheet.FileContents.Length);

					coverSheetBytes = DocumentConverter.ConvertFromExcel(coverSheetBytes, OutputFormatType.TIF, ColourDepth.BlackAndWhite);

					using (var convertedFile = TempFile.NewWithExtension("tif"))
					using (var fileNameOfPagesToAppend = TempFile.NewWithExtension("tif"))
					{
						FileSaveHelper.SaveMultiPageTifFile(Image.FromStream(new MemoryStream(coverSheetBytes)), fileNameOfPagesToAppend.Filename);
						util.ConvertFileToTiff(fileNameOfPagesToAppend.Filename, convertedFile.Filename);
						util.AppendMultiPageImageToFile(resultFile.Filename, convertedFile.Filename);
						result = new MemoryStream(util.GetFileAsBytes(convertedFile.Filename));
					}
				}
				else
				{
					result = new MemoryStream(util.GetFileAsBytes(resultFile.Filename));
				}
			}
			return result;
		}

		const int SheetNameMaxLength = 31;//Excel sheet name max length is 31 characters

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		string GetValidSheetName(List<string> sheetNames, string sheetName)
		{
			var newSheetName = sheetName.Substring(0, Math.Min(SheetNameMaxLength, sheetName.Length)).TrimEnd();
			var index = 1;
			while (sheetNames.Contains(newSheetName))
			{
				var suffix = $"({index})";
				newSheetName = newSheetName.Substring(0, SheetNameMaxLength - suffix.Length) + suffix;
				index++;
			}
			sheetNames.Add(newSheetName);
			return newSheetName;
		}

		void InsertIntoXLS(XlsFile outFile, string sheetNamePrefix, DeliveryInfo deliveryInfo)
		{
			var inputFile = XlsFileGenerator();
			deliveryInfo.FileContents.Position = 0;
			inputFile.Open(deliveryInfo.FileContents);
			var outFileSheetNames = new List<string>();

			for (var sheetNumber = inputFile.SheetCount; sheetNumber >= 1; sheetNumber--)
			{
				inputFile.ActiveSheet = sheetNumber;
				var copyingScalingSheet = ShouldCopyScalingSheet(inputFile, outFile);
				if (inputFile.SheetVisible == TXlsSheetVisible.Visible || copyingScalingSheet)
				{
					try
					{
						outFile.InsertAndCopySheets(sheetNumber, 1, 1, inputFile);
					}
					catch (FlexCelXlsAdapterException exception)
					{
						if (exception.ErrorCode == XlsErr.ErrCantCopyPictFmla)
						{
							var sheetsToDelete = inputFile.SheetCount - sheetNumber + 1;
							for (var counter = 0; counter < sheetsToDelete; counter++)
							{
								outFile.ActiveSheet = 1;
								outFile.DeleteSheet(1);
							}
							deliveryInfo.HasMergeIntoPrimaryExcelTemplateFailed = true;
							return;
						}
						throw;
					}

					if (!copyingScalingSheet)
					{
						outFile.SheetName = GetValidSheetName(outFileSheetNames, sheetNamePrefix + inputFile.SheetName);
						var sheetName = deliveryInfo.SheetNames.FirstOrDefault(s => s.StrictName == inputFile.SheetName);
						if (sheetName != null)
						{
							sheetName.StrictName = outFile.SheetName;
							sheetName.EntireName = sheetNamePrefix + sheetName.EntireName;
						}
						else
						{
							deliveryInfo.SheetNames.Add(new SheetName { StrictName = outFile.SheetName, EntireName = outFile.SheetName });
						}
					}
					else
					{
						outFile.SheetName = inputFile.SheetName;
						outFile.SheetVisible = inputFile.SheetVisible;
					}
					for (var row = 1; row <= inputFile.RowCount; row++)
					{
						outFile.SetRowHeight(row, inputFile.GetRowHeight(row));
					}
				}
			}
		}

		bool ShouldCopyScalingSheet(XlsFile inputFile, XlsFile outputFile)
		{
			bool outputFileAlreadyHasScalingSheet = outputFile.GetSheetIndex(Report.FlexCelScaleSheetName, false) > 0;
			bool activeSheetIsScalingSheet = String.Compare(inputFile.SheetName, Report.FlexCelScaleSheetName, true) == 0;

			return activeSheetIsScalingSheet && !outputFileAlreadyHasScalingSheet;
		}

#if DEBUG
		internal
#endif
		protected DeliveryInfo GetFirstXLSDeliveryInfo()
		{
			DeliveryInfo result = null;

			var deliveryInfo = DeliveryInfos.FirstOrDefault(d => d.Instructions != null && d.Instructions.DocPack != null && d.Instructions.DocPack.StmMenuCommand != null);
			if (deliveryInfo != null)
			{
				var menuItem = deliveryInfo.Instructions.DocPack.StmMenuCommand;
				result = GetMatchingDeliveryInfo(menuItem);
			}

			return result ?? DeliveryInfos.FirstOrDefault(i => i.IsSuitableForMerge);
		}

		DeliveryInfo GetMatchingDeliveryInfo(StmMenuItem menuItem)
		{
			var primaryDocumentPK = menuItem.SU_PrimaryDocPackItemId;
			return primaryDocumentPK.IsValid ? DeliveryInfos.FirstOrDefault(i => i.ParentPivotPK == primaryDocumentPK) : null;
		}

#if DEBUG
		internal
#endif
		protected DeliveryInfo GetFirstTIFDeliveryInfo()
		{
			return DeliveryInfos.FirstOrDefault(i => i.DeliveryFormat == DeliveryInfo.DeliveryFormats.TIFF);
		}

		static DeliveryMethod AutoDetectDeliveryMethod(DocDeliveryContact contact, DeliveryInstructions deliveryInstructions)
		{
			DeliveryMethod result = null;

			if (contact == null)
			{
				contact = new DocDeliveryContact(new BusinessObjectFactory());
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;

				if (!deliveryInstructions.DocPackAlreadyPrinted)
				{
					Globals.Message.ShowInformation(Res.GetString("1184e3f8-9e3c-42d6-aff3-d97cac9c202f", "No contact to auto deliver to, sending document to default printer."));
				}

				if (!IsPrinterSpecified(deliveryInstructions))
				{
					deliveryInstructions.ShowPrinterSelectionUI();
				}

				if (deliveryInstructions.PrinterDelivery.PrintQueue != null)
				{
					result = new DeliveryMethods.Printer(deliveryInstructions.PrinterDelivery);
				}
			}
			else
			{
				switch (contact.DeliveryMethod)
				{
					case Core.Constants.ContactNotifyModes.Print:
						result = new DeliveryMethods.Printer(deliveryInstructions.PrinterDelivery);
						break;

					case Core.Constants.ContactNotifyModes.Email:
					case Core.Constants.ContactNotifyModes.EPrint:
						result = new DeliveryMethods.Email(contact);
						break;

					case Core.Constants.ContactNotifyModes.Fax:
						result = new DeliveryMethods.Fax(contact);
						break;

					default:
						throw new InvalidOperationException("Support for this contact delivery method [" + contact.DeliveryMethod + "] has not been implemented.");
				}
			}

			if (result != null)
			{
				result.Instructions = deliveryInstructions;
			}

			return result;
		}

		internal static bool IsPrinterSpecified(DeliveryInstructions instructions)
		{
#if DEBUG
			if (Globals.IsTest && IsPrinterSpecifiedForTesting)
			{
				return true;
			}
#endif

			return instructions.PrinterDelivery.PrintQueue != null;
		}

#if DEBUG
		internal static bool IsPrinterSpecifiedForTesting;
#endif

		#endregion

		public DocManagerInfo ForcedParentDocManagerInfo { get; set; }
	}
}
