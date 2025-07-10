using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WinzorFramework;
using WinzorFramework.JSInterop;

namespace Enterprise.ZArchitecture.GUI.Winzor
{
	public class PasteFileHelper
	{
		protected IFileService FileService { get; }

		public PasteFileHelper(IFileService fileService)
		{
			FileService = fileService;
		}

		public async Task<IDataObject> GetClipboardDataAsync(WinzorPasteEventArgs args)
		{
			var dataObject = new DataObject();

			// Get text from clipboard
			if (!string.IsNullOrEmpty(args.Text))
			{
				dataObject.SetData(DataFormats.Text, args.Text);
			}

			// Get html from clipboard
			if (!string.IsNullOrEmpty(args.Html))
			{
				dataObject.SetData(DataFormats.Html, args.Html);
			}

			// Get files from clipboard
			if (args.Files?.Length > 0 && FileService is not null)
			{
				var maximumLimitSize = SystemDataRegistry.Instance.eDocsMaximumFilesize.Value * FileCommonContent.FromMbToByte;
				var filePaths = await FileService.UploadFilesToServerAsync(args.Files, maximumLimitSize, CancellationToken.None);
				dataObject.SetData(DataFormats.FileDrop, filePaths);
			}

			return dataObject;
		}
	}
}
