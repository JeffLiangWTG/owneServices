using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Winzor;
using WinzorFramework;

namespace Enterprise.DocumentScanning.GUI;

public partial class DocumentsZGrid
{
	protected override bool AllowWinzorPaste => true;

	protected override async Task OnWinzorPasteCoreAsync(WinzorPasteEventArgs args)
	{
		var fileHelper = CreatePasteFileHelper();

		if (!await ShouldHandlePasteAsync(args))
		{
			return;
		}

		var dataObject = await fileHelper.GetClipboardDataAsync(args);

		await InvokeWinzorDispatcherAsync(() =>
		{
			HandleWinzorPaste(dataObject);
		});
	}

	protected async Task<bool> ShouldHandlePasteAsync(WinzorPasteEventArgs args)
	{
		if (!IsInsertAllowed)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				Globals.Message.ShowError(Res.GetString("31F7A101-4B18-434D-B0CD-7F7C13C79B9C", "Manually uploading eDocs is not allowed."));
			});

			return false;
		}

		if (args == null || string.IsNullOrEmpty(args.Text) && string.IsNullOrEmpty(args.Html) && args.Files?.Length == 0) {
			return false;
		}

		if (args.Files?.Length > 0)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				var (validFiles, message) = AcceptableBrowserFileValidator.ValidateFiles(args.Files);
				args.Files = validFiles;
				if (!string.IsNullOrEmpty(message))
				{
					Globals.Message.ShowWarning(message);
				}
			});
		}

		return !string.IsNullOrEmpty(args.Text) || !string.IsNullOrEmpty(args.Html) || args.Files?.Length > 0;
	}

	protected void HandleWinzorPaste(IDataObject dataObject)
	{
		if (!dataObject.GetDataPresent(DataFormats.FileDrop))
		{
			Paste?.Invoke(this, EventArgs.Empty);
			return;
		}

		InsertFromData(dataObject);
	}

	protected virtual PasteFileHelper CreatePasteFileHelper()
	{
		return new PasteFileHelper(FindForm()?.CargoWiseClientServices?.FileService);
	}
}
