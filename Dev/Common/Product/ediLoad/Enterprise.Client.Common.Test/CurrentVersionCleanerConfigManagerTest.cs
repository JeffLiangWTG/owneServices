using System;
using System.IO;
using System.Xml.Serialization;
using CargoWise.ApplicationManager.Common;
using CargoWise.IO;
using Moq;
using NUnit.Framework;
using static Enterprise.Client.Common.Testing.AppManagerTestHelper;

namespace Enterprise.Client.Common.Testing
{
	class CurrentVersionCleanerConfigManagerTest : TestCase
	{
		public void TestWrongParamsCall()
		{
			AssertExceptionThrown<ArgumentException>(() => currentVersionCleanerConfigManager.LoadConfiguration(null));
			AssertExceptionThrown<ArgumentException>(() => currentVersionCleanerConfigManager.LoadConfiguration(""));
			AssertExceptionThrown<ArgumentException>(() => currentVersionCleanerConfigManager.SaveConfigurationViaAppManager(null, null));
			AssertExceptionThrown<ArgumentException>(() => currentVersionCleanerConfigManager.SaveConfigurationViaAppManager("", null));
			AssertExceptionThrown<ArgumentException>(() => currentVersionCleanerConfigManager.SaveConfigurationViaAppManager("P", null));
		}

		public void TestLoadDefaultValuesWhenNoFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				// Arrange
				var manager = currentVersionCleanerConfigManager;

				// Act
				var config = manager.LoadConfiguration(tempDirectory);

				// Assert
				AssertEquals(TimeSpan.FromDays(1), config.CurrentVersionCleanupIntervalInDays);
				AssertEquals(DateTime.MinValue, config.LastStartTime);
				AssertEquals(TimeSpan.FromDays(14), config.VersionInactiveDurationInDays);
				AssertEquals(Path.Combine(tempDirectory, "CurrentVersion"), config.CurrentVersionFile);
			}
		}

		public void TestLoadValuesFromFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				// Arrange
				var config = new CurrentVersionCleanerConfig
				{
					CurrentVersionFile = "boo",
					CurrentVersionCleanupIntervalInDays = TimeSpan.FromSeconds(42),
					LastStartTime = DateTime.Now,
					VersionInactiveDurationInDays = TimeSpan.FromHours(42),
				};

				// Act
				using (var writer = new FileStream(Path.Combine(tempDirectory, CurrentVersionConfigFileName), FileMode.Create))
				{
					new XmlSerializer(typeof(CurrentVersionCleanerConfig)).Serialize(writer, config);
				}

				// Assert
				var loadedConfig = currentVersionCleanerConfigManager.LoadConfiguration(tempDirectory);
				AssertEquals(config.CurrentVersionFile, loadedConfig.CurrentVersionFile);
				AssertEquals(config.CurrentVersionCleanupIntervalInDays, loadedConfig.CurrentVersionCleanupIntervalInDays);
				AssertEquals(config.LastStartTime, loadedConfig.LastStartTime);
				AssertEquals(config.VersionInactiveDurationInDays, loadedConfig.VersionInactiveDurationInDays);
			}
		}

		public void TestSaveValuesToFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				// Arrange
				var config = new CurrentVersionCleanerConfig
				{
					CurrentVersionFile = "boo",
					CurrentVersionCleanupIntervalInDays = TimeSpan.FromSeconds(42),
					LastStartTime = DateTime.Now,
					VersionInactiveDurationInDays = TimeSpan.FromHours(42),
				};

				// Act
				currentVersionCleanerConfigManager.SaveConfigurationViaAppManager(tempDirectory, config);

				// Assert
				using (var reader = new StreamReader(Path.Combine(tempDirectory, CurrentVersionConfigFileName)))
				{
					var loadedConfig = (CurrentVersionCleanerConfig)new XmlSerializer(typeof(CurrentVersionCleanerConfig)).Deserialize(reader);

					AssertEquals(config.CurrentVersionFile, loadedConfig.CurrentVersionFile);
					AssertEquals(config.CurrentVersionCleanupIntervalInDays, loadedConfig.CurrentVersionCleanupIntervalInDays);
					AssertEquals(config.LastStartTime, loadedConfig.LastStartTime);
					AssertEquals(config.VersionInactiveDurationInDays, loadedConfig.VersionInactiveDurationInDays);
				}
			}
		}

		public void TestSaveValuesOverwritesExistingFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				// Arrange
				var config = new CurrentVersionCleanerConfig
				{
					CurrentVersionFile = "boo",
					CurrentVersionCleanupIntervalInDays = TimeSpan.FromSeconds(42),
					LastStartTime = DateTime.Now,
					VersionInactiveDurationInDays = TimeSpan.FromHours(42),
				};

				var newConfig = new CurrentVersionCleanerConfig
				{
					CurrentVersionFile = "blah",
					CurrentVersionCleanupIntervalInDays = TimeSpan.FromSeconds(9001),
					LastStartTime = DateTime.MinValue,
					VersionInactiveDurationInDays = TimeSpan.FromHours(9001),
				};

				// Act
				using (var writer = new FileStream(Path.Combine(tempDirectory, CurrentVersionConfigFileName), FileMode.Create))
				{
					new XmlSerializer(typeof(CurrentVersionCleanerConfig)).Serialize(writer, config);
				}

				currentVersionCleanerConfigManager.SaveConfigurationViaAppManager(tempDirectory, newConfig);

				// Assert
				using (var reader = new StreamReader(Path.Combine(tempDirectory, CurrentVersionConfigFileName)))
				{
					var loadedConfig = (CurrentVersionCleanerConfig)new XmlSerializer(typeof(CurrentVersionCleanerConfig)).Deserialize(reader);

					AssertEquals(newConfig.CurrentVersionFile, loadedConfig.CurrentVersionFile);
					AssertEquals(newConfig.CurrentVersionCleanupIntervalInDays, loadedConfig.CurrentVersionCleanupIntervalInDays);
					AssertEquals(newConfig.LastStartTime, loadedConfig.LastStartTime);
					AssertEquals(newConfig.VersionInactiveDurationInDays, loadedConfig.VersionInactiveDurationInDays);

					AssertNotEquals(config.CurrentVersionFile, loadedConfig.CurrentVersionFile);
					AssertNotEquals(config.CurrentVersionCleanupIntervalInDays, loadedConfig.CurrentVersionCleanupIntervalInDays);
					AssertNotEquals(config.LastStartTime, loadedConfig.LastStartTime);
					AssertNotEquals(config.VersionInactiveDurationInDays, loadedConfig.VersionInactiveDurationInDays);
				}
			}
		}

		public void TestLoadConfiguration()
		{
			// Arrange
			var manager = currentVersionCleanerConfigManager;

			using (var temp = new TempDirectory())
			{
				// Act
				var config = manager.LoadConfiguration(temp.DirectoryName);

				// Assert
				AssertEquals(TimeSpan.FromDays(14), config.VersionInactiveDurationInDays);
				AssertEquals(TimeSpan.FromDays(1), config.CurrentVersionCleanupIntervalInDays);
				AssertEquals(DateTime.MinValue, config.LastStartTime);
				AssertEquals(DateTime.MinValue, config.NextRuntime);
				AssertEquals(DateTime.MinValue, config.LastSuccessTime);
				AssertEquals(Path.Combine(temp.DirectoryName, "CurrentVersion"), config.CurrentVersionFile);
			}
		}

		public void TestSaveConfiguration()
		{
			using (var temp = new TempDirectory())
			{
				// Arrange
				var utcNow = DateTime.UtcNow;
				var manager = currentVersionCleanerConfigManager;
				var config = manager.LoadConfiguration(temp.DirectoryName);

				config.LastStartTime = utcNow;
				config.NextRuntime = config.LastStartTime + config.CurrentVersionCleanupIntervalInDays;
				config.LastSuccessTime = utcNow.AddMinutes(3);

				// Act
				manager.SaveConfigurationViaAppManager(temp.DirectoryName, config);
				var config1 = manager.LoadConfiguration(temp.DirectoryName);

				// Assert
				Assert(File.Exists(Path.Combine(temp.DirectoryName, CurrentVersionConfigFileName)));
				AssertEquals(config.LastStartTime, config1.LastStartTime);
				AssertEquals(config.NextRuntime, config1.NextRuntime);
				AssertEquals(config.CurrentVersionCleanupIntervalInDays, config1.CurrentVersionCleanupIntervalInDays);
				AssertEquals(config.LastSuccessTime, config1.LastSuccessTime);
			}
		}

		[ExpectNoExceptions]
		public void TestSavingConfigurationIsDoneViaAppManagerWhenNotElevated()
		{
			// Arrange
			var mockAppManager = new Mock<IAppManager>();

			using (var temp = new TempDirectory())
			using (MockAppManager(mockAppManager.Object))
			using (MockIsAdmin(false))
			{
				var config = currentVersionCleanerConfigManager.LoadConfiguration(temp.DirectoryName);

				// Act
				currentVersionCleanerConfigManager.SaveConfigurationViaAppManager(temp.DirectoryName, config);

				// Assert
				mockAppManager.Verify(c => c.Invoke(It.IsAny<string>(), typeof(CurrentVersionCleanerConfigManager).FullName, It.IsAny<object>(), It.IsAny<MutexRequest>()), Times.Once);
			}
		}

		public void TestSavingConfigurationIsDoneDirectlyWhenElevated()
		{
			// Arrange
			var mockAppManager = new Mock<IAppManager>();

			using (var temp = new TempDirectory())
			using (MockAppManager(mockAppManager.Object))
			using (MockIsAdmin(true))
			{
				var config = currentVersionCleanerConfigManager.LoadConfiguration(temp.DirectoryName);

				// Act
				currentVersionCleanerConfigManager.SaveConfigurationViaAppManager(temp.DirectoryName, config);

				// Assert
				mockAppManager.VerifyNeverInvoked();
				Assert(File.Exists(Path.Combine(temp.DirectoryName, CurrentVersionConfigFileName)));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			currentVersionCleanerConfigManager = new CurrentVersionCleanerConfigManager();
		}

		CurrentVersionCleanerConfigManager currentVersionCleanerConfigManager;
		const string CurrentVersionConfigFileName = "CurrentVersionConfig.xml";
	}
}
