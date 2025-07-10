using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	public class FileMapper : IFileMapper
	{
		public string GetFolderPath(System.Environment.SpecialFolder folder)
		{
			if (ZSaveFileDialog.IsRemote)
			{
				return EnterpriseChannel.Instance.SendMessage<int, string>(EnterpriseChannelMessageTypes.GetFolderPath, (int)folder);
			}
			else
			{
				return System.Environment.GetFolderPath(folder);
			}
		}

		public Stream OpenWrite(string unmappedPath)
		{
			if (ZSaveFileDialog.IsRemote)
			{
				CreateNewDirectoryIfDoesNotExist(ObjectFactory.Get<IMappedClientPath>().GetMappedPath(unmappedPath));
			}
			else if (!(DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode == RemoteConnectingModes.ConnectorOnly && Globals.IsUserInteractive))
			{
				CreateNewDirectoryIfDoesNotExist(unmappedPath);
			}
			return ZSaveFileDialog.OpenFile(unmappedPath);
		}

		public Stream OpenRead(string unmappedPath)
		{
			return ZOpenFileDialog.OpenFile(unmappedPath);
		}

		public bool IsRemote
		{
			get { return ZSaveFileDialog.IsRemote; }
		}

		void CreateNewDirectoryIfDoesNotExist(string fileName)
		{
			var directory = Path.GetDirectoryName(fileName);
			if (!directory.IsNullOrEmpty() && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
		}
	}
}
