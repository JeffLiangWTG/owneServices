using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.IssueManager.Business.Test;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ServiceTask.Testing
{
	public class IssueManagerProcessorServiceTaskTextReadingTest : TestCaseWithXmlDoc
	{
		public void TestParsesXmlWithoutByteOrderMark()
		{
			CopyResourceToDirectory(SampleTestFile, errorLogDirectory);
			TestParsesSingleXmlReportCore();
		}

		public void TestParsesXmlWithByteOrderMark()
		{
			var testFilePath = CopyResourceToDirectory(SampleTestFile, errorLogDirectory);
			PrependByteOrderMark(testFilePath, Encoding.UTF8);
			TestParsesSingleXmlReportCore();
		}

		public void TestFixBrokenXml()
		{
			CopyResourceToDirectory(SampleBrokenXML, errorLogDirectory);
			TestParsesSingleXmlReportCore();
		}

		void TestParsesSingleXmlReportCore()
		{
			// Arrange
			var serviceTask = new IssueManagerProcessorServiceTask();
			serviceTask.ServiceLogger = Mock.Of<ILogger>();

			// Act
			serviceTask.Process(errorLogDirectory, new BusinessObjectFactoryProvider(Factory), deleteFilesAfterProcessed: true);

			// Assert
			var logs = Factory.Load<EdiHelpErrorLog>(new ZQuery());
			AssertEquals(1, logs.Length);

			var log = logs[0];
			AssertNotEquals("Container for all issues with invalid XML", log.HE_ExceptionSource);
			AssertNotEquals("Container for all issues with invalid XML", log.HE_ExceptionMessage);
		}

		public void TestSaveExceptionHandling()
		{
			// Arrange
			var exceptionFactory = MakeFactoryWhoFailsToSave();

			var firstProcessedFile = CopyResourceToDirectory(SmallCallstackTestFile, errorLogDirectory);
			var secondProcessedFile = CopyResourceToDirectory(SampleTestFile, errorLogDirectory);

			var serviceTask = new IssueManagerProcessorServiceTask();

			var logs = new List<(LogType logType, string message)>();
			var logger = new Mock<ILogger>(MockBehavior.Strict);
			logger.Setup(x => x.Log(LogType.Debug, It.IsAny<string>()))
				.Callback<LogType, string>((x, y) => logs.Add((x, y)));
			logger.Setup(x => x.Log(LogType.Error, It.IsAny<string>(), It.IsAny<Exception>()))
				.Callback<LogType, string, Exception>((type, message, ex) => logs.Add((type, $"{message}|{ex.Message}")));

			serviceTask.ServiceLogger = logger.Object;

			// Act
			serviceTask.Process(errorLogDirectory, new BusinessObjectFactoryProvider(exceptionFactory.factory), deleteFilesAfterProcessed: true);

			// Assert
			AssertEquals(1, LoadAllErrorLogs().Length);

			AssertEquals(false, File.Exists(firstProcessedFile));
			AssertEquals(true, File.Exists(Path.ChangeExtension(firstProcessedFile, ".bad")));

			AssertEquals(false, File.Exists(secondProcessedFile));
			AssertEquals(false, File.Exists(Path.ChangeExtension(secondProcessedFile, ".bad")));

			AssertContainsExactElementsInExactOrder(
				new[]
				{
					(LogType.Debug, $"Processing [{Path.GetFileName(firstProcessedFile)}]"),
					(LogType.Error, $"Could not save data retrieved from [{Path.ChangeExtension(firstProcessedFile, ".bad")}]|{exceptionFactory.saveException.Message}"),
					(LogType.Debug, $"Processing [{Path.GetFileName(secondProcessedFile)}]"),
				},
				logs);
		}

		public void TestErrorReportsCanBeProcessedAndSaved()
		{
			// Arrange
			var totalSaveCount = 0;

			var file1 = CopyResourceToDirectory(SampleTestFile, errorLogDirectory);
			var file2 = CopyResourceToDirectory(SmallCallstackTestFile, errorLogDirectory);
			var file3 = CopyResourceToDirectory(SampleGlowWindowsCEFile, errorLogDirectory);

			var serviceTask = new IssueManagerProcessorServiceTask();
			serviceTask.ServiceLogger = Mock.Of<ILogger>();

			var factoryProvider = new BusinessObjectFactoryProvider();
			factoryProvider.Current.Saved += Factory_Saved;
			factoryProvider.CurrentFactoryChanged += FactoryProvider_CurrentFactoryChanged;

			// Act
			serviceTask.Process(errorLogDirectory, factoryProvider, deleteFilesAfterProcessed: true);

			// Assert
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"File or assembly name 'OpenNETCF.Net, Version=2.3.12004.0, Culture=neutral, PublicKeyToken=E60DBEA84BB431B7', or one of its dependencies, was not found.",
					"The maximum length of 'JR_Desc' has been exceeded.\n The maximum length of this property is 1024 characters, but 1106 were entered.",
					"This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.",
				},
				LoadAllErrorLogs());

			AssertEquals(false, File.Exists(file1));
			AssertEquals(false, File.Exists(file2));
			AssertEquals(false, File.Exists(file3));

			void FactoryProvider_CurrentFactoryChanged(object sender, EventArgs e)
			{
				// Currently, creation of first factory won't fire CurrentFactoryChanged event,
				// but it's not sure if we will fire the event later. And we have subscribed to Saved event of first factory,
				// then to avoid double subscriptions to Saved event here, we unsubscribe from it in advance
				factoryProvider.Current.Saved -= Factory_Saved;
				factoryProvider.Current.Saved += Factory_Saved;
			}

			void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				// Assert
				AssertEquals(true, savedSuccessfully);

				switch (++totalSaveCount)
				{
					case 1:
					case 2:
					case 3:
						AssertEquals(totalSaveCount, LoadAllErrorLogs().Length);
						break;
					default:
						Assert("Save count is no more than 3", false);
						break;
				}
			}
		}

		public void TestLargeFileIsRenamedAndNotProcessed()
		{
			// Arrange
			using (SetMaxErrorReportSizeTemporaryValue(1))
			{
				var largeFile = CopyResourceToDirectory(CreateErrorReport(1, "Message"), errorLogDirectory);

				var serviceTask = new IssueManagerProcessorServiceTask();
				var factoryProvider = new BusinessObjectFactoryProvider();

				var logger = new Mock<ILogger>(MockBehavior.Strict);
				logger.Setup(x => x.Log(LogType.Warning, It.IsAny<string>())).Verifiable();
				serviceTask.ServiceLogger = logger.Object;

				// Act
				serviceTask.Process(errorLogDirectory, factoryProvider, deleteFilesAfterProcessed: true);

				// Assert
				AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), LoadAllErrorLogs());
				AssertEquals(false, File.Exists(largeFile));
				AssertEquals(true, File.Exists(Path.ChangeExtension(largeFile, ".big")));
				logger.Verify(x => x.Log(LogType.Warning, $"Found large file [{largeFile}]"), Times.Once());
			}
		}

		public void TestLargeFilesAreRenamedAndNotProcessed()
		{
			// Arrange
			using (SetMaxErrorReportSizeTemporaryValue(1))
			{
				var largeFile1 = CopyResourceToDirectory(CreateErrorReport(1, "MessageA"), errorLogDirectory);
				var largeFile2 = CopyResourceToDirectory(CreateErrorReport(2, "MessageB"), errorLogDirectory);

				var serviceTask = new IssueManagerProcessorServiceTask();
				var factoryProvider = new BusinessObjectFactoryProvider();

				var logger = new Mock<ILogger>(MockBehavior.Strict);
				logger.Setup(x => x.Log(LogType.Warning, It.IsAny<string>())).Verifiable();
				serviceTask.ServiceLogger = logger.Object;

				// Act
				serviceTask.Process(errorLogDirectory, factoryProvider, deleteFilesAfterProcessed: true);

				// Assert
				AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), LoadAllErrorLogs());

				AssertEquals(false, File.Exists(largeFile1));
				AssertEquals(true, File.Exists(Path.ChangeExtension(largeFile1, ".big")));

				AssertEquals(false, File.Exists(largeFile2));
				AssertEquals(true, File.Exists(Path.ChangeExtension(largeFile2, ".big")));

				logger.Verify(x => x.Log(LogType.Warning, $"Found large file [{largeFile1}]"), Times.Once());
				logger.Verify(x => x.Log(LogType.Warning, $"Found large file [{largeFile2}]"), Times.Once());
			}
		}

		public void TestAllRenamedLargeFilesRemain()
		{
			// Arrange
			using (SetMaxErrorReportSizeTemporaryValue(1))
			{
				var renamedLargeFile1 = CopyResourceToDirectory(CreateErrorReport(1, "MessageA"), errorLogDirectory, ".big");
				var renamedLargeFile2 = CopyResourceToDirectory(CreateErrorReport(2, "MessageB"), errorLogDirectory, ".big");

				var serviceTask = new IssueManagerProcessorServiceTask();
				serviceTask.ServiceLogger = Mock.Of<ILogger>();
				var factoryProvider = new BusinessObjectFactoryProvider();

				// Act
				serviceTask.Process(errorLogDirectory, factoryProvider, deleteFilesAfterProcessed: true);

				// Assert
				AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), LoadAllErrorLogs());
				AssertEquals(true, File.Exists(renamedLargeFile1));
				AssertEquals(false, File.Exists(Path.ChangeExtension(renamedLargeFile1, ".xml")));
				AssertEquals(true, File.Exists(renamedLargeFile2));
				AssertEquals(false, File.Exists(Path.ChangeExtension(renamedLargeFile2, ".xml")));
			}
		}

		public void TestRenamedLargeFileIsProcessedIfItIsNoLongerDefinedLarge()
		{
			// Arrange
			using (SetMaxErrorReportSizeTemporaryValue(2))
			{
				var largeFileRenamed = CopyResourceToDirectory(CreateErrorReport(1, "MessageA"), errorLogDirectory, ".big");

				var serviceTask = new IssueManagerProcessorServiceTask();
				serviceTask.ServiceLogger = Mock.Of<ILogger>();
				var factoryProvider = new BusinessObjectFactoryProvider();

				// Act
				serviceTask.Process(errorLogDirectory, factoryProvider, deleteFilesAfterProcessed: true);

				// Assert
				AssertContainsExactElementsInAnyOrder(new[] { "MessageA" }, LoadAllErrorLogs());
				AssertEquals(false, File.Exists(largeFileRenamed));
				AssertEquals(false, File.Exists(Path.ChangeExtension(largeFileRenamed, ".xml")));
			}
		}

		public void TestErrorReportsAreProcessedByFileSizeOrder()
		{
			// Arrange
			using (SetMaxErrorReportSizeTemporaryValue(3))
			{
				var file1 = CopyResourceToDirectory(SampleTestFile, errorLogDirectory);
				var file2 = CopyResourceToDirectory(CreateErrorReport(1, "MessageA"), errorLogDirectory);
				var file3 = CopyResourceToDirectory(CreateErrorReport(2, "MessageB"), errorLogDirectory);

				var serviceTask = new IssueManagerProcessorServiceTask();
				var factoryProvider = new BusinessObjectFactoryProvider();

				var logs = new List<string>();
				var logger = new Mock<ILogger>(MockBehavior.Strict);
				logger.Setup(x => x.Log(LogType.Debug, It.IsAny<string>()))
					.Callback<LogType, string>((_, x) => logs.Add(x));

				serviceTask.ServiceLogger = logger.Object;

				// Act
				serviceTask.Process(errorLogDirectory, factoryProvider, deleteFilesAfterProcessed: true);

				// Assert
				AssertContainsExactElementsInExactOrder(
					new[]
					{
						$"Processing [{Path.GetFileName(file1)}]",
						$"Processing [{Path.GetFileName(file2)}]",
						$"Processing [{Path.GetFileName(file3)}]",
					},
					logs);
			}
		}

		public void TestFactoryIsReCreatedAfterEachSave()
		{
			// Arrange
			var factoryCreatedTotalCount = 0;

			CopyResourceToDirectory(SampleTestFile, errorLogDirectory);
			CopyResourceToDirectory(SmallCallstackTestFile, errorLogDirectory);

			var serviceTask = new IssueManagerProcessorServiceTask();
			serviceTask.ServiceLogger = Mock.Of<ILogger>();

			var factoryProvider = new BusinessObjectFactoryProvider();
			var lastFactoryUsedToSave = factoryProvider.Current;
			lastFactoryUsedToSave.Saved += Factory_Saved;
			factoryProvider.CurrentFactoryChanged += FactoryProvider_CurrentFactoryChanged;

			// Act
			serviceTask.Process(errorLogDirectory, factoryProvider, deleteFilesAfterProcessed: true);

			// Assert
			AssertEquals(2, factoryCreatedTotalCount); // currently, CurrentFactoryChanged event isn't fired for first factory
			AssertNotEquals(lastFactoryUsedToSave, factoryProvider.Current);

			void FactoryProvider_CurrentFactoryChanged(object sender, EventArgs e)
			{
				factoryCreatedTotalCount++;

				factoryProvider.Current.Saved -= Factory_Saved;
				factoryProvider.Current.Saved += Factory_Saved;
			}

			void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				switch (factoryCreatedTotalCount)
				{
					case 0:
						AssertEquals(lastFactoryUsedToSave, factory);
						break;
					case 1:
						AssertNotEquals(lastFactoryUsedToSave, factory);
						lastFactoryUsedToSave = factory;
						break;
					default:
						Assert(false);
						break;
				}
			}
		}

		public void TestFactoryIsReCreatedAfterException()
		{
			// Arrange
			var exceptionFactory = MakeFactoryWhoFailsToSave();
			CopyResourceToDirectory(SampleTestFile, errorLogDirectory);

			var serviceTask = new IssueManagerProcessorServiceTask();
			serviceTask.ServiceLogger = Mock.Of<ILogger>();

			var factoryProvider = new BusinessObjectFactoryProvider(exceptionFactory.factory);

			// Act
			serviceTask.Process(errorLogDirectory, factoryProvider, deleteFilesAfterProcessed: true);

			// Assert
			AssertNotEquals(factoryProvider.Current, exceptionFactory.factory);
		}

		public void TestRefreshEnabledIsSetFalseWhenFactoryIsReCreated()
		{
			// Arrange
			CopyResourceToDirectory(SampleTestFile, errorLogDirectory);
			CopyResourceToDirectory(SmallCallstackTestFile, errorLogDirectory);
			CopyResourceToDirectory(SampleGlowWindowsCEFile, errorLogDirectory);

			var serviceTask = new IssueManagerProcessorServiceTask();
			serviceTask.ServiceLogger = Mock.Of<ILogger>();

			var factoryProvider = new BusinessObjectFactoryProvider();
			factoryProvider.Current.Saved += Factory_Saved;
			factoryProvider.CurrentFactoryChanged += FactoryProvider_CurrentFactoryChanged;

			// Act
			serviceTask.Process(errorLogDirectory, factoryProvider, deleteFilesAfterProcessed: true);

			void FactoryProvider_CurrentFactoryChanged(object sender, EventArgs e)
			{
				// Currently, creation of first factory won't fire CurrentFactoryChanged event,
				// but it's not sure if we will fire the event later. And we have subscribed to Saved event of first factory,
				// then to avoid double subscriptions to Saved event here, we unsubscribe from it in advance
				factoryProvider.Current.Saved -= Factory_Saved;
				factoryProvider.Current.Saved += Factory_Saved;
			}

			void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				// Assert
				AssertEquals("RefreshEnabled should be false", false, factory.RefreshEnabled);
			}
		}

		[ExpectNoExceptions]
		public void TestNoOverflowExceptionWhenMaxErrorReportSizeIsGivenALargeValue()
		{
			// Arrange
			using (SetMaxErrorReportSizeTemporaryValue(int.MaxValue))
			{
				CopyResourceToDirectory(SampleTestFile, errorLogDirectory);

				var serviceTask = new IssueManagerProcessorServiceTask();
				serviceTask.ServiceLogger = Mock.Of<ILogger>();

				var factoryProvider = new BusinessObjectFactoryProvider();

				// Act
				serviceTask.Process(errorLogDirectory, factoryProvider, deleteFilesAfterProcessed: true);
			}
		}

		static (BusinessObjectFactory factory, ZSaveException saveException) MakeFactoryWhoFailsToSave()
		{
			var factory = new BusinessObjectFactory();

			var table = new DataTable("BlahBlah");
			var column = new DataColumn("PK", typeof(Guid));
			table.Columns.Add(column);
			table.PrimaryKey = new[] { column };
			var dataRow = table.NewRow();

			var saveException = new ZSaveException(new ZDataException(new Exception(), dataRow, Db.Connection), factory);
			factory.Saving += x => throw saveException;

			return (factory, saveException);
		}

		static string[] LoadAllErrorLogs()
		{
			return new BusinessObjectFactory()
				.Load<EdiHelpErrorLog>(new ZQuery())
				.Select(x => (string)x.HE_ExceptionMessage)
				.ToArray();
		}

		static IDisposable SetMaxErrorReportSizeTemporaryValue(int value)
		{
			var maxErrorReportSizeDataType = (IntRegistryDataType)EDIDataRegistry.Instance.MaxErrorReportSizeInMb.DataType;
			var originalMaxErrorReportSizeDataTypeMinValue = maxErrorReportSizeDataType.LowerBound;

			maxErrorReportSizeDataType.LowerBound = 1;
			var setTemporaryValueDisposable = EDIDataRegistry.Instance.MaxErrorReportSizeInMb.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

			return new DisposableAction(() =>
			{
				maxErrorReportSizeDataType.LowerBound = originalMaxErrorReportSizeDataTypeMinValue;
				setTemporaryValueDisposable.Dispose();
			});
		}

		string CreateErrorReport(int fileSizeInMb, string exceptionMessage)
		{
			const string ExceptionDetailsMessageTag = "{{{ExceptionDetailsMessage}}}";
			const string MiscellaneousInfoTag = "{{{MiscellaneousInfo}}}";
			var fileSizeInByte = fileSizeInMb * 1024L * 1024L + 512L;

			var templatePath = GetTestFilePath("SampleReport_Template.xml");

			var fileContent = new StringBuilder();
			fileContent.Append(File.ReadAllText(templatePath));

			fileContent.Replace(ExceptionDetailsMessageTag, exceptionMessage);

			var currentLength = fileContent.Length;
			var leftLength = fileSizeInByte - currentLength + MiscellaneousInfoTag.Length;
			fileContent.Replace(MiscellaneousInfoTag, new string('A', (int)leftLength));

			var path = Temp.GetTempFileName(tempDirectoryForCreatedReport);
			File.WriteAllText(path, fileContent.ToString());

			AssertCloseEnough(fileSizeInByte, new FileInfo(path).Length, 500L);
			return path;
		}

		string errorLogDirectory;
		string tempDirectoryForCreatedReport;
		protected override void SetUp()
		{
			base.SetUp();
			errorLogDirectory = Temp.GetNewTempSubdirectory();
			tempDirectoryForCreatedReport = Temp.GetNewTempSubdirectory();
			ClearTables(new[] { HelpErrorLogKeySchema.Constants.TableName, HelpErrorLogOccurrenceSchema.Constants.TableName, HelpErrorLogSchema.Constants.TableName });
		}

		protected override void TearDown()
		{
			TempDirectory.DeleteDirectory(errorLogDirectory);
			TempDirectory.DeleteDirectory(tempDirectoryForCreatedReport);
			base.TearDown();
		}

		static string CopyResourceToDirectory(string resourcePath, string directory, string extension = ".xml")
		{
			var newFileName = $"{Guid.NewGuid()}{extension}";
			var destinationPath = Path.Combine(directory, newFileName);
			File.Copy(resourcePath, destinationPath);
			File.SetAttributes(destinationPath, File.GetAttributes(destinationPath) & ~FileAttributes.ReadOnly);
			return destinationPath;
		}

		void ClearTables(string[] tableNames)
		{
			foreach (var table in tableNames)
			{
				using (var command = Db.Connection.Command(string.Format("DELETE [{0}];", table)))
				{
					command.ExecuteNonQuery();
				}
			}
		}

		void PrependByteOrderMark(string filePath, Encoding encoding)
		{
			var fileData = File.ReadAllBytes(filePath);
			var preamble = encoding.GetPreamble();
			using (var fs = File.OpenWrite(filePath))
			{
				fs.Write(preamble, 0, preamble.Length);
				fs.Write(fileData, 0, fileData.Length);
				fs.SetLength(fs.Position);
			}
		}
	}
}
