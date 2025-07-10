using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class EDIMessageExporterGridContextMenuItemComponent : GridContextMenuItemComponent<ITEDIMessage>
{
	public EDIMessageExporterGridContextMenuItemComponent(ZGrid grid) : base(grid)
	{
	}

	public EDIMessageExporterGridContextMenuItemComponent(ZGrid grid, MultilingualString caption) : base(grid)
	{
		_caption = caption;
	}

	protected override ZMenuItem GetMenuItem() => new ZMenuItem(_caption);

	protected override void Execute(ZMenuItem menuItem) => new FileSaver(Grid).ShowSaveEdiFileDialog();

	protected override bool IsMenuItemVisible(ZMenuItem menuItem) => Grid.SelectedElements.Length > 0 && !IsUcc6;

	bool IsUcc6 => (Grid.DataSource as JobDeclaration)?.IsUCC6 ?? false;

	readonly MultilingualString _caption = CaptionsProvider.SaveEdiFileCaption;

	#region FileSaver

	class FileSaver
	{
		public FileSaver(ZGrid messagesGrid)
		{
			this.messagesGrid = Argument.NotNull(messagesGrid, nameof(messagesGrid));
		}

		readonly ZGrid messagesGrid;

		public void ShowSaveEdiFileDialog()
		{
			using (var dialog = new ZFolderBrowserDialog())
			{
				dialog.RequireMappablePath = true;
				dialog.Description = CaptionsProvider.SaveEdiFileCaption;

				ShowDialogAndTryToSave(dialog);
			}
		}

		void ShowDialogAndTryToSave(ZFolderBrowserDialog dialog)
		{
			if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
			{
				var filesToExport = GetEDIMessageExportInfoCollectionToExport(dialog.MappedSelectedPath, dialog.UnmappedSelectedPath);
				var shouldExportAndOverwriteExistingFilesIfAny = DetermineIfShouldExportAndOverwriteExistingFilesIfAny(filesToExport);

				if (shouldExportAndOverwriteExistingFilesIfAny)
				{
					var (generatedCount, errorMessage) = TryToSave(filesToExport);
					ShowResult(generatedCount, errorMessage);
				}
			}
		}

		void ShowResult(int generatedCount, ZString errorMessage)
		{
			var message = CaptionsProvider.GetShowResultMessage(generatedCount);
			if (string.IsNullOrEmpty(errorMessage))
			{
				Globals.Message.Show(message);
			}
			else
			{
				var messageToDispaly = PrepareErrorMessage();
				Globals.Message.ShowError(messageToDispaly);
			}

			string PrepareErrorMessage() => message + System.Environment.NewLine + CaptionsProvider.GetErrorResultMessage(errorMessage);
		}

		(int GeneratedCount, ZString ErrorMessage) TryToSave(IEnumerable<EDIMessageExportInfo> filesToExport)
		{
			var generatedCount = 0;
			var errorMessage = ZString.Empty;
			try
			{
				foreach (var file in filesToExport)
				{
					using (var stream = ZSaveFileDialog.OpenFile(file.UnmappedPathIncludingFilename))
					{
						file.Message.Write(stream);
						generatedCount++;
					}
				}
			}
			catch (IOException ex)
			{
				errorMessage = ex.Message;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Exception during saving IT messages in local", ex);
				errorMessage = ex.Message;
			}

			return (generatedCount, errorMessage);
		}

		IEnumerable<EDIMessageExportInfo> GetEDIMessageExportInfoCollectionToExport(ZString mappedPath, ZString unmappedPath)
		{
			foreach (var (message, fileName) in GetSelectedMessagesWithFileName())
			{
				var mappedfilePath = Path.Combine(mappedPath, fileName);
				var unmappedfilePath = Path.Combine(unmappedPath, fileName);
				yield return new EDIMessageExportInfo(mappedfilePath, unmappedfilePath, message);
			}

			IEnumerable<(ITEDIMessage Message, ZString FileName)> GetSelectedMessagesWithFileName() =>
				messagesGrid.SelectedElements
				.OfType<ITEDIMessage>()
				.Select(x => (x, x.GetFileName()))
				.Cast<(ITEDIMessage Message, ZString FileName)>()
				.Where(x => !string.IsNullOrEmpty(x.FileName));
		}

		bool DetermineIfShouldExportAndOverwriteExistingFilesIfAny(IEnumerable<EDIMessageExportInfo> filesToExport)
		{
			if (CheckFilesAlreadyExist())
			{
				return AskUserForConfirmationToOverwriteExistingFiles();
			}
			return true;

			bool CheckFilesAlreadyExist() => filesToExport.Any(file => File.Exists(ZSaveFileDialog.IsRemote ? file.MappedPathIncludingFilename : file.UnmappedPathIncludingFilename));
		}

		ZBool AskUserForConfirmationToOverwriteExistingFiles() => Globals.Message.Show(CaptionsProvider.AskUserForConfirmationQuestion, CaptionsProvider.AskUserForSaveConfirmationCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes;
	}

	#endregion

	class CaptionsProvider
	{
		public static ResourceString SaveEdiFileCaption => ResString.GetMultilingualString("C5BD4167-75AD-49ED-8F8A-D62D2A9C14A8", "Save EDI file");

		public static string GetShowResultMessage(int generatedCount) => Res.GetString("2E9C1F38-E7FC-4AC7-9FA1-4A14CAA3DC22", "{0} message(s) were exported", generatedCount.ToString(Culture.Current));

		public static string GetErrorResultMessage(string errorMessage) => Res.GetString("C20A9C7D-0C12-4AC6-8B42-0907B35B2072", "Some messages exporting failed due to the following error: {0}", errorMessage);

		public static string AskUserForConfirmationQuestion => Res.GetString("1FF68C4C-6403-4017-BEB3-EBCB06981DF7", "The folder already contains one or more files with the same names. If you continue, the files will be overwritten. Do you want to continue?");
		public static string AskUserForSaveConfirmationCaption => Res.GetString("9F099DB9-DD42-44EB-B591-9FCAEEAAAC52", "Confirm Save");
	}

	class EDIMessageExportInfo
	{
		public EDIMessageExportInfo(ZString mappedPathIncludingFilename, ZString unmappedPathIncludingFilename, ITEDIMessage message)
		{
			MappedPathIncludingFilename = Argument.NotNullOrEmpty(mappedPathIncludingFilename, nameof(mappedPathIncludingFilename));
			UnmappedPathIncludingFilename = Argument.NotNullOrEmpty(unmappedPathIncludingFilename, nameof(unmappedPathIncludingFilename));
			Message = Argument.NotNull(message, nameof(message));
		}

		public ZString MappedPathIncludingFilename { get; }
		public ZString UnmappedPathIncludingFilename { get; }
		public ITEDIMessage Message { get; }
	}
}
