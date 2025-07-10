using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Reflection;
using System.Threading;
using AppDomainWrappers.Net;
using CargoWise.ApplicationManager.Common;
using CargoWise.IO;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.Authenticode;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Loader.Common.Testing.Installers
{
	class NGenInstallerTest : TestCase
	{
		[DatCapabilityRequirement("ADMIN")]
		public void TestInstallDoesNotBlockUninstallDoesBlock()
		{
			var currentPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
			var tempDir = Path.Combine(Temp.TempPath, Guid.NewGuid().ToString());
			using (new TempDirectory(tempDir))
			{
				var newInstallPath = Path.Combine(tempDir, "18.0.0.0");
				var newInstallNGenRoot = Path.Combine(newInstallPath, "CargoWise.NGenRoot.dll");
				Directory.CreateDirectory(newInstallPath);
				File.Copy(Path.Combine(currentPath, "CargoWise.NGenRoot.dll"), newInstallNGenRoot);

				var state = new object[] { currentPath, newInstallNGenRoot, NGenInstaller.Action.Install, 300 };

				var installResult = new NGenInstaller(new Installation(new Configuration()), Mock.Of<IAuthenticodeVerificationService>(), currentPath, newInstallNGenRoot, NGenInstaller.Action.Install, 300).Invoke(false, state);

				AssertEquals($"Message: {installResult.Message}", AppManagerResultStatus.Success, installResult.Status);
				try
				{
					AssertNGenInstallerProcessCount(newInstallNGenRoot, 1);
				}
				finally
				{
					var uninstallState = new object[] { currentPath, newInstallNGenRoot, NGenInstaller.Action.Uninstall, 300 };

					var unInstallResult = new NGenInstaller(new Installation(new Configuration()), Mock.Of<IAuthenticodeVerificationService>(), currentPath, newInstallNGenRoot, NGenInstaller.Action.Uninstall).Invoke(false, uninstallState);
					AssertEquals($"Message: {unInstallResult.Message}", AppManagerResultStatus.Success, unInstallResult.Status);
					AssertNGenInstallerProcessCount(newInstallNGenRoot, 0);
				}
			}
		}

		public void TestNeedsToInstall()
		{
			var configuration = new Configuration();
			var services = new MoqMockServiceContainer();
			services.Mocker = new MockRepository(MockBehavior.Default);
			// Ngen won't exist anymore because we will be on .NET 8, so needn't rename this registry item to CargoWise. Will delete obsolete NGen code in WI00884783
			services.RegistryForTest.SetupSequence(m => m.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WiseTech Global\CargoWise One", "nongen", null))
				.Returns((object)null)
				.Returns(1);
			configuration.Services = services;
			var installation = new Installation(configuration);

			AssertEquals("NeedsToInstall", true, new NGenInstaller(installation, "", "", NGenInstaller.Action.Install).NeedsToInstall());
			AssertEquals("NeedsToInstall", true, new NGenInstaller(installation, "", "", NGenInstaller.Action.Uninstall).NeedsToInstall());

			AssertEquals("NeedsToInstall", false, new NGenInstaller(installation, "", "", NGenInstaller.Action.Install).NeedsToInstall());
			AssertEquals("NeedsToInstall", true, new NGenInstaller(installation, "", "", NGenInstaller.Action.Uninstall).NeedsToInstall());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Testing")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTempOrTmpPaths", Justification = "Testing")]
		public void TestUninstallingNonExistingFileReturnsWarning()
		{
			// Arrange
			var appManager = new AppManagerForTesting();
			var currentPath = Environment.CurrentDirectory;
			var tempDir = Path.Combine(Temp.TempPath, Guid.NewGuid().ToString());
			using (new TempDirectory(tempDir))
			{
				var configuration = new MockConfiguration(@"c:\Temp", tempDir);
				configuration.SetAppManagerClient(appManager);

				var fileGuid = "291E55A2-D0E0-405F-BD58-36C037723C58.dll";
				var nonExistingFileName = Path.Combine(currentPath, fileGuid);
				var installResult = new InstallationResultCollection();

				// Act
				new NGenInstaller(new Installation(configuration), currentPath, nonExistingFileName, NGenInstaller.Action.Uninstall, 100)
					.Install(installResult);

				// Assert
				AssertGreaterThan("Warning message expected", installResult.WarningCount, 0);
				AssertContains("Warning message expected",
					"NGenInstaller process exited with error code",
					string.Join(" ", new[] { installResult.GetErrorMessages(), installResult.GetWarningMessages() }));
			}
		}

		#region TestInvokeVerifiesExecutable

		public void TestInvokeVerifiesExecutableWithInvalidPublicKeyAndValidCertificateFail()
		{
			//Arrange
			using var tmpFile = TempFile.New();
			const string expectedMessage = "The assembly does not contain the expected public key token.";
			const AppManagerResultStatus expectedStatus = AppManagerResultStatus.Error;
			var config = CreateTestProcessConfiguration("MockUnsignedProgram.exe", tmpFile.FileName, hasValidCertificate: true);

			//Act
			RunTestProcess(config);

			//Assert
			AssertInstallerOutputFile(tmpFile.FileName, expectedStatus, expectedMessage);
		}

		public void TestInvokeVerifiesExecutableWithInvalidPublicKeyAndInvalidCertificateFail()
		{
			//Arrange
			using var tmpFile = TempFile.New();
			const string expectedMessage = "The assembly does not contain the expected public key token.";
			const AppManagerResultStatus expectedStatus = AppManagerResultStatus.Error;
			var config = CreateTestProcessConfiguration("MockUnsignedProgram.exe", tmpFile.FileName, hasValidCertificate: false);

			//Act
			RunTestProcess(config);

			//Assert
			AssertInstallerOutputFile(tmpFile.FileName, expectedStatus, expectedMessage);
		}

		public void TestInvokeVerifiesExecutableWithValidPublicKeyAndInvalidCertificateIsFail()
		{
			//Arrange
			using var tmpFile = TempFile.New();
			const string expectedMessage = "Authenticode failed";
			const AppManagerResultStatus expectedStatus = AppManagerResultStatus.Error;
			var config = CreateTestProcessConfiguration("MockProgram.exe", tmpFile.FileName, hasValidCertificate: false);

			//Act
			RunTestProcess(config);

			//Assert
			AssertInstallerOutputFile(tmpFile.FileName, expectedStatus, expectedMessage);
		}

		public void TestInvokeVerifiesExecutableWithValidPublicKeyAndValidCertificateIsSuccess()
		{
			//Arrange
			using var tmpFile = TempFile.New();
			const string expectedMessage = "";
			const AppManagerResultStatus expectedStatus = AppManagerResultStatus.Success;
			var config = CreateTestProcessConfiguration("CargoWise.NGenInstaller.exe", tmpFile.FileName, hasValidCertificate: true);

			//Act
			RunTestProcess(config);
			//Assert
			AssertInstallerOutputFile(tmpFile.FileName, expectedStatus, expectedMessage);
		}

		ProcessConfig CreateTestProcessConfiguration(string executableName, string outputPath, bool hasValidCertificate)
		{
			var config = new ProcessConfig
			{
				NamespacePath = "CargoWise.Loader.Common.Testing.Installers",
				ClassName = nameof(NGenInstallerTest),
				MethodName = nameof(InvokeInstallForNGenExecutable),
			};
			config.AssemblyFile = Path.Combine(config.BinFolder, "CargoWise.Loader.Common.Test.dll");
			config.MethodParameters = new[] { config.TempWorkingDirectoryPath, executableName, outputPath, hasValidCertificate.ToString() };
			return config;
		}

		void RunTestProcess(ProcessConfig config)
		{
			var appDomainWrapper = new AppDomainWrapper("MyTestAppDomain");
			appDomainWrapper.RunMethodInProcess48(config);
		}

		static void InvokeInstallForNGenExecutable(string tempDir, string programName, string outputPath, string hasValidCertificate)
		{
			CreateFakeNGenInstallerExecutable(tempDir, programName);
			var targetPath = "any";
			var action = NGenInstaller.Action.Install;
			var waitTimeInMsForTest = 0;
			var state = new object[] { tempDir, targetPath, (int)action, waitTimeInMsForTest };

			var mockVerificationService = new Mock<IAuthenticodeVerificationService>();
			if (hasValidCertificate != true.ToString())
			{
				mockVerificationService.Setup(x => x.VerifyAuthenticodeSignatureIsWhitelisted(It.IsAny<string>())).Throws(new AuthenticodeVerificationException("Authenticode failed"));
			}
			var ngenInstaller = new NGenInstaller(null, mockVerificationService.Object, tempDir, targetPath, action, waitTimeInMsForTest);

			var result = ngenInstaller.Invoke(waitedForMutex: true, state);

			File.WriteAllText(outputPath, $"{result.Status}\n{result.Message}");
		}

		static void CreateFakeNGenInstallerExecutable(string tempDir, string programName)
		{
			var nGenExeForTest = Path.Combine(tempDir, "CargoWise.NGenInstaller.exe");
			var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			File.Copy(Path.Combine(binFolder, programName), nGenExeForTest);
		}

		static void AssertInstallerOutputFile(string outputFile, AppManagerResultStatus expectedStatus, string expectedMessage)
		{
			var resultInfo = File.ReadAllText(outputFile).Split('\n');
			var resultStatus = resultInfo[0].Trim();
			var resultMessage = resultInfo[1].Trim();

			CombineAssertions(() =>
			{
				AssertEquals($"Should have AppManagerResultStatus of '{expectedStatus}' was '{resultStatus}'", expectedStatus.ToString(), resultStatus);
				AssertEquals($"Should have logged message '{expectedMessage}' was '{resultMessage}'", expectedMessage, resultMessage);
			});
		}

		#endregion

		static void AssertNGenInstallerProcessCount(string filePath, int expectedCount)
		{
			var processCountResult = new InstallationResultCollection();
			new NGenInstallerProcessCounter(new Installation(new Configuration()), filePath, expectedCount).Install(processCountResult);
			AssertEquals(processCountResult.GetErrorMessages(), 0, processCountResult.ErrorCount);
		}
	}

	class AppManagerForTesting : MockAppManager
	{
		public MutexRequest LastMutexRequest { get; private set; }

		public override AppManagerResult Invoke(string assemblyPath, string typeName, object state, MutexRequest request)
		{
			if (RetryOnce)
			{
				RetryOnce = false;
				return new AppManagerResult(AppManagerResultStatus.Retry);
			}
			else
			{
				LastMutexRequest = request;

				var workingDirectory = (string)((object[])state)[0];
				var targetPath = (string)((object[])state)[1];
				var action = ((object[])state)[2].ToString();
				var delayTimeInMsForTest = ((object[])state)[3].ToString();
				var appDomainWrapper = new AppDomainWrapper("MockAppManagerClientDomain");
				var config = new ProcessConfig
				{
					NamespacePath = "CargoWise.Loader.Common.Testing.Installers",
					ClassName = nameof(NGenInstallerProxy),
					MethodName = nameof(Invoke)
				};

				config.AssemblyFile = Path.Combine(config.BinFolder, "CargoWise.Loader.Common.Test.dll");
				config.MethodParameters = new string[] { assemblyPath, typeName, workingDirectory, targetPath, action, delayTimeInMsForTest };
				var result = appDomainWrapper.RunMethodInProcess48(config);

				var settings = new JsonSerializerSettings
				{
					Converters = new List<JsonConverter> { new AppManagerResultConverter() }
				};

				var deserializedAppManagerResult = JsonConvert.DeserializeObject<AppManagerResult>(result, settings);
				return deserializedAppManagerResult;
			}
		}

		public class AppManagerResultConverter : JsonConverter<AppManagerResult>
		{
			public override AppManagerResult ReadJson(JsonReader reader, Type objectType, AppManagerResult existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				if (reader.TokenType != JsonToken.StartObject)
				{
					throw new JsonSerializationException("Unexpected token type");
				}

				var message = string.Empty;
				var status = AppManagerResultStatus.TimedOut;

				while (reader.Read())
				{
					if (reader.TokenType == JsonToken.EndObject)
					{
						return new AppManagerResult(status, message);
					}

					if (reader.TokenType == JsonToken.PropertyName && reader.Value is string propertyName)
					{
						_ = reader.Read();

						switch (propertyName)
						{
							case "Message":
								message = serializer.Deserialize<string>(reader);
								break;
							case "Status":
								status = serializer.Deserialize<AppManagerResultStatus>(reader);
								break;
						}
					}
				}

				throw new JsonSerializationException("Unexpected end when deserializing");
			}

			public override void WriteJson(JsonWriter writer, AppManagerResult value, JsonSerializer serializer)
			{
				throw new NotImplementedException();
			}
		}
	}

	[CodeAlive("Called using CargoWise.Loader.Common.Testing.Installers.AppManagerForTesting.Invoke() in a separate process")]
	class NGenInstallerProxy
	{
		public void Invoke(string assemblyPath, string typeName, string workingDirectory, string targetPath, string action, string delayTimeInMsForTest)
		{
			object state = new object[]
			{
				workingDirectory,
				targetPath,
				(NGenInstaller.Action)int.Parse(action),
				int.Parse(delayTimeInMsForTest)
			};

			var invocable = (IAppManagerInvocable)new NGenInstaller(new Installation(new Configuration()), Mock.Of<IAuthenticodeVerificationService>(), null, null, 0);

			var result = invocable.Invoke(waitedForMutex: false, state);

			var serializedResult = JsonConvert.SerializeObject(result);
#pragma warning disable CW1106 // Required to send data back from the AppDomainWrapper to CargoWise.Loader.Common.Testing.MockAppManager.Invoke()
			Console.WriteLine($"{serializedResult}");
#pragma warning restore CW1106
		}
	}

	public class NGenInstallerProcessCounter : AppManagerInvoker, IAppManagerInvocable
	{
		readonly String installPath;
		readonly int expectedCount;

		public NGenInstallerProcessCounter()
			: this(new Installation(new Configuration()), null, 0)
		{
		}

		public NGenInstallerProcessCounter(Installation installation, string installPath, int expectedCount)
			: base(installation)
		{
			this.installPath = installPath;
			this.expectedCount = expectedCount;
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			var result = InvokeAppManager<NGenInstallerProcessCounter>(new object[] { expectedCount, installPath }, null);
			return result;
		}

		public AppManagerResult Invoke(bool waitedForMutex, object state)
		{
			if (state == null || ((object[])state).Length < 2)
			{
				return new AppManagerResult(AppManagerResultStatus.Error, "Invalid arguments");
			}

			var expectedCount = (int)((object[])state)[0];
			var installPath = (string)((object[])state)[1];
			var result = new AppManagerResult(AppManagerResultStatus.Success);

			if (GetMatchingNGenInstallerProcessCount(installPath) != expectedCount)
			{
				result = new AppManagerResult(AppManagerResultStatus.Error, $"process count did not match expected count. Expect process count of {expectedCount}");
			}

			for (int i = 0; ((GetMatchingNGenInstallerProcessCount(installPath) > 0) && (i < 1800)); ++i)
			{
				Thread.Sleep(100);
			}

			return result;
		}

		int GetMatchingNGenInstallerProcessCount(string installPath)
		{
			var matchingProcesses = 0;
			var searcher = new ManagementObjectSearcher("select CommandLine from Win32_Process where Name='CargoWise.NGenInstaller.exe'");
			using (searcher)
			{
				var retObjectCollection = searcher.Get();
				if (retObjectCollection != null)
				{
					foreach (var retObject in retObjectCollection)
					{
						var commandLineObj = retObject["CommandLine"];
						if (commandLineObj != null)
						{
							var commandLine = commandLineObj.ToString();
							if (commandLine.IndexOf(installPath, StringComparison.OrdinalIgnoreCase) != -1)
							{
								++matchingProcesses;
							}
						}
					}
				}
			}

			return matchingProcesses;
		}
	}
}
