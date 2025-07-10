using System;
using System.Threading.Tasks;
using WinzorFramework;

namespace Enterprise.RemoteDesktopServices.Server
{
	public static class RemoteFileDialog
	{
		public static bool SaveFile(string fileName, byte[] data)
		{
			var cwcs = WinzorDispatcher.Current.CurrentContext.Form.CargoWiseClientServices;
			WinzorDispatcher.Current.CurrentContext.InvokeRenderDispatcher(async Task () =>
			{
				await cwcs.FileService.SaveFileByPathAsync(fileName, data);
			});
			return true;
		}

		public static byte[] OpenFile(string fileName)
		{
			throw new NotImplementedException();
		}

		public static string[] ListDirectoryFiles(string path, string searchPattern)
		{
			throw new NotImplementedException();
		}
	}
}
