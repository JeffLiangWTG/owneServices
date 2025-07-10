using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.IO;
using Enterprise.Client.EDI.Escrow;
using Inedo.AssetDirectories;
using Moq;
using NUnit.Framework;

namespace ZClientEDI.Test.Escrow
{
	class AssetDirectoryClientAdapterTest : TestCase
	{
		public void TestWrongParamsCall()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new AssetDirectoryClientAdapter(null));
			AssertEquals("proGetAssetDirectoryRegistry", result.ParamName);
		}

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestCreateRemoteDirectoryAsync()
		{
			try
			{
				// Arrange
				// Act
				var result = Task.Run(async () =>
				{
					await adapter.CreateRemoteDirectoryAsync(remoteFolderName, CancellationToken.None);
					return await rawClient.ListContentsAsync();
				}).GetAwaiter().GetResult();

				// Assert
				AssertEquals(true, result.Any(item => item.Name == remoteFolderName));
			}
			finally
			{
				Task.Run(async () =>
				{
					await rawClient.DeleteItemAsync(remoteFolderName, true);
				}).GetAwaiter().GetResult();
			}
		}

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestUploadFileAsync()
		{
			try
			{
				// Arrange
				using var tempDir = new TempDirectory(Temp.GetNewTempSubdirectory());

				var localFilePath = Path.Combine(tempDir, "File1.txt");
				var fileContents = "This is a test for a file \n and this is a new 2nd line.";
				File.WriteAllText(localFilePath, fileContents);
				var remoteFilePath = $"{remoteFolderName}/File1.txt";

				// Act
				var result = Task.Run(async () =>
				{
					await adapter.CreateRemoteDirectoryAsync(remoteFolderName, CancellationToken.None);
					await adapter.UploadFileAsync(localFilePath, remoteFilePath, CancellationToken.None);
					return await rawClient.ListContentsAsync(remoteFolderName);
				}).GetAwaiter().GetResult();

				// Assert
				AssertEquals(true, result.Any(item => item.Name == "File1.txt"));
			}
			finally
			{
				Task.Run(async () =>
				{
					await rawClient.DeleteItemAsync(remoteFolderName, true);
				}).GetAwaiter().GetResult();
			}
		}

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestDeleteRemoteDirectoryAsync()
		{
			try
			{
				// Arrange
				Task.Run(async () =>
				{
					await rawClient.CreateDirectoryAsync(remoteFolderName, CancellationToken.None);
				}).GetAwaiter().GetResult();

				// Act
				var result = Task.Run(async () =>
				{
					await adapter.DeleteRemoteDirectoryAsync(remoteFolderName, CancellationToken.None);
					return await rawClient.ListContentsAsync();
				}).GetAwaiter().GetResult();

				// Assert
				AssertEquals(false, result.Any(item => item.Name == remoteFolderName));
			}
			finally
			{
				Task.Run(async () =>
				{
					await rawClient.DeleteItemAsync(remoteFolderName, true);
				}).GetAwaiter().GetResult();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			registryMock = new Mock<IProGetAssetDirectoryRegistry>();
			registryMock.SetupGet(registry => registry.AssetPathUrl).Returns("https://proget.sand.wtg.zone/endpoints/EscrowExport");
			registryMock.SetupGet(registry => registry.ApiKey).Returns("6e797fe767e862d3066fe08d293d867ea159eeb8");
			adapter = new AssetDirectoryClientAdapter(registryMock.Object);
			rawClient = new AssetDirectoryClient(registryMock.Object.AssetPathUrl, apiKey: registryMock.Object.ApiKey);
		}

		Mock<IProGetAssetDirectoryRegistry> registryMock;
		AssetDirectoryClientAdapter adapter;
		AssetDirectoryClient rawClient;
		const string remoteFolderName = "dummyFolder";
	}
}
