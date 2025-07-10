using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.RemoteDesktopServices.Server;
#if !WINZOR
using Enterprise.RemoteDesktopServices.Testing;
#else
using Enterprise.ZArchitecture.GUI.Testing;
#endif
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ResourceStrings.GUI.CustomizableData.Testing
{
	sealed class CustomizableDataTranslationFormWithRemoteDesktopServicesTest : RemoteDesktopServicesTest
	{
		public void TestExportWithRemoteDirectory()
		{
			var tempDir = Temp.GetNewTempSubdirectory();
			try
			{
				using (var testHelper = new CustomizableDataTestHelper())
				using (var form = new CustomizableDataTranslationFormForRemoteTest(
					new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("two"), null), tempDir))
				{
					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					form.ExportForTest();

					var dirFiles = RemoteFileDialog.ListDirectoryFiles(tempDir, ".csv");

					var contentLines =
						("\"Language\",\"Original\",\"Language\",\"Translation\"\r\n"
						+ "\"EN-US,\"zero\",\"{0}\"\r\n"
						+ "\"EN-US,\"one\",\"{0}\"\r\n"
						+ "\"EN-US,\"two\",\"{0}\"\r\n"
						+ "\"EN-US,\"three\",\"{0}\"\r\n"
						+ "\"EN-US,\"four\",\"{0}\"\r\n"
						+ "\"EN-US,\"five\",\"{0}\"\r\n"
						+ "\"EN-US,\"six\",\"{0}\"\r\n"
						+ "\"EN-US,\"seven\",\"{0}\"\r\n"
						+ "\"EN-US,\"eight\",\"{0}\"\r\n"
						+ "\"EN-US,\"nine\",\"{0}\"\r\n"
						+ "\"EN-US,\"ten\",\"{0}\"")
						.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
					var prefix = $"{tempDir}\\Customizable Data Test-";

					foreach (var fileName in dirFiles)
					{
						Assert(fileName.EndsWith(@".csv"));

						var fileContent = Encoding.UTF8.GetString(RemoteFileDialog.OpenFile(fileName));
						var language = fileName.Substring(prefix.Length, 3);
						var lines = fileContent.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

						for (var index = 0; index < lines.Length; index++)
						{
							var line = lines[index];
							Assert(string.Format("{0} should start with {1}", line, string.Format(contentLines[index], language)), line.StartsWith(string.Format(contentLines[index], language)));
						}
					}
				}
			}
			finally
			{
				Directory.Delete(tempDir, recursive: true);
			}
		}

		public void TestImportWithRemoteDirectory()
		{
			UnitTestUserNotification.Instance.ClearMessages();

			var tempDir = Temp.GetNewTempSubdirectory();
			try
			{
				using (var testHelper = new CustomizableDataTestHelper())
				using (var form = new CustomizableDataTranslationFormForRemoteTest(
					new CustomizableDataTranslationPage(testHelper.CustomizableDataResourceStrings, testHelper.GetMultilingualString("two"), null), tempDir))
				{
					var fileContent =
						"\"Language\",\"Original\",\"Language\",\"Translation\"\r\n"
						+ "\"EN-US\",\"zero\",\"RU-RU\",\"0\"\r\n"
						+ "\"EN-US\",\"one\",\"AR-AE\",\"1\"\r\n"
						+ "\"EN-US\",\"two\",\"FR-FR\",\"2\"\r\n"
						+ "\"EN-US\",\"three\",\"ZH-CN\",\"3\"\r\n"
						+ "\"EN-US\",\"bamboozle\",\"ZZZ\",\":-)\"\r\n";
					var filePath = Path.Combine(tempDir, "Test import for Form.csv");
					var result = RemoteFileDialog.SaveFile(filePath, Encoding.UTF8.GetBytes(fileContent));

					Assert("Remote file should be saved", result);

					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					form.ImportForTest();

					AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Import has been successful", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Directory.Delete(tempDir, recursive: true);
			}
		}

		class CustomizableDataTranslationFormForRemoteTest : CustomizableDataTranslationForm
		{
			readonly string directory;
			public CustomizableDataTranslationFormForRemoteTest(CustomizableDataTranslationPage bo, string directory) : base(bo)
			{
				this.directory = directory;
			}

			protected override string GetDirectory(string description)
			{
				isNeedingToUseEnterpriseChannel = true;
				return directory;
			}

			public void ExportForTest()
			{
				OnExport(null, EventArgs.Empty);
			}

			public void ImportForTest()
			{
				OnImport(null, EventArgs.Empty);
			}
		}
	}
}
