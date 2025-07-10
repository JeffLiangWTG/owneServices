using System;
using System.IO;
using CargoWise.Application;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	sealed class FileMapperTest : TestCase
	{
		public void TestOpenWriteCreatesNewDirectoryNotRemote()
		{
			var directoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			var filePath = Path.Combine(directoryPath, "testfile.txt");
			var fileMapper = new FileMapper();

			Assert(!Directory.Exists(directoryPath));

			try
			{
				using (var stream = fileMapper.OpenWrite(filePath)) { }

				Assert(Directory.Exists(directoryPath));
				Assert(File.Exists(filePath));
			}
			finally
			{
				if (Directory.Exists(directoryPath))
				{
					Directory.Delete(directoryPath, true);
				}
			}
		}

		public void TestOpenWriteCreatesNewDirectoryRemote()
		{
			var directoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			var filePath = Path.Combine(directoryPath, "testfile.txt");
			var fileMapper = new FileMapper();
			using (ObjectFactory.Substitute<IMappedClientPath>(new TestMappedClientPath()))
			{
				var mappedPath = ObjectFactory.Get<IMappedClientPath>().GetMappedPath(filePath);
				var mappedDirectory = Path.GetDirectoryName(mappedPath);

				Assert(!Directory.Exists(mappedDirectory));

				try
				{
					InitializationMessageHandler.RemoteInitializationMessage = new RemoteDesktopServices.MessageElements.InitializationMessage(new[] { EnterpriseChannelMessageTypes.SaveFileDialog }, ClientVersion.Version.ToString());
					using (ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest(isRemoteAppSession: true, isWTSSession: true)))
					{
						using (var stream = fileMapper.OpenWrite(filePath))
						{ }
					}

					Assert(Directory.Exists(mappedDirectory));
					Assert(File.Exists(mappedPath));
				}
				finally
				{
					if (Directory.Exists(mappedDirectory))
					{
						Directory.Delete(directoryPath, true);
					}
				}
			}
		}
	}

	sealed class TestMappedClientPath : IMappedClientPath
	{
		public string GetMappedPath(string unmappedPath)
		{
			return Path.Combine(Path.GetDirectoryName(unmappedPath), "mapped", Path.GetFileName(unmappedPath));
		}

		public string GetUnmappedPath(string mappedPath)
		{
			throw new NotImplementedException();
		}
	}
}
