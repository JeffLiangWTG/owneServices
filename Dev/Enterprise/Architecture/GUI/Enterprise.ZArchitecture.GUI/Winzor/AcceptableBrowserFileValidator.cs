using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WinzorFramework.JSInterop;

namespace Enterprise.ZArchitecture.GUI;

/// <summary>
/// file validator to validate browse files before uploading to the system. similar with AcceptableFileValidator.
/// </summary>
public static class AcceptableBrowserFileValidator
{
	/// <summary>
	/// quick validation for browser files without uploading to server
	/// </summary>
	/// <param name="files"></param>
	/// <returns></returns>
	public static (BrowserFile[], string) ValidateFiles(BrowserFile[] files)
	{
		var maximumLimitSize = SystemDataRegistry.Instance.eDocsMaximumFilesize.Value * 1024 * 1024;
		var fileTypeValidator = new FileTypeValidation();

		var dangerousFiles = new List<ZString>();
		var largeFiles = new List<ZString>();
		var validFiles = new List<BrowserFile>();

		var messages = new List<ZString>();

		foreach (var file in files)
		{
			var fileSize = file.Size;

			if (fileTypeValidator.IsDangerousFile(file.Name))
			{
				dangerousFiles.Add(file.Name);
			}
			else if (fileSize > maximumLimitSize)
			{
				largeFiles.Add(file.Name);
			}
			else
			{
				validFiles.Add(file);
			}
		}

		if (dangerousFiles.Count > 0 || largeFiles.Count > 0)
		{
			if (dangerousFiles.Count > 0)
			{
				var dangerousFileMessage = Res.GetString("fb0ad34c-3c48-49f7-8c11-abf7b6063180", "The following files were not added because they are potentially dangerous file types:");
				var fileList = string.Join(System.Environment.NewLine, dangerousFiles);
				messages.Add($"{dangerousFileMessage}{System.Environment.NewLine}{fileList}{System.Environment.NewLine}");
			}

			if (largeFiles.Count > 0)
			{
				var limitSizeMessage = StorageDocsHelper.GetMaximumLimitSizeNotifications(SystemDataRegistry.Instance.eDocsMaximumFilesize);
				var fileList = string.Join(System.Environment.NewLine, largeFiles);
				messages.Add($"{limitSizeMessage}:{System.Environment.NewLine}{fileList}{System.Environment.NewLine}");
			}
		}

		return ([.. validFiles], string.Join(System.Environment.NewLine, messages));
	}
}
