using System;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class TempFileTest : TestCase
	{
		public void TestTryDeleteFileWithNotFoundShouldNotReportException()
		{
			using (var tempFile = GetNewTempFile())
			{
				var result = TempFile.TryDelete(tempFile.Filename, out var message, false, true);
				Assert("The temp file " + tempFile.Filename + " was deleted.", result);
				AssertNullOrEmpty(message);

				result = TempFile.TryDelete(tempFile.Filename, out message, false, true);
				Assert("The temp file " + tempFile.Filename + " has been deleted.", result);
				AssertNullOrEmpty(message);
			}

			var notExistFileName = Path.Combine(EnvProxy.Instance.TempPath, Guid.NewGuid().ToString(), "test.tif");
			var resultForTotExist = TempFile.TryDelete(notExistFileName, out var messageForTotExist, false, true);

			Assert("The temp file " + notExistFileName + " does not exist.", resultForTotExist);
			AssertNullOrEmpty(messageForTotExist);
		}

		public void TestTryDeleteUnauthorizedAccessFileWithReportException()
		{
			AssertDeleteUnauthorizedAccessFile(true);
		}

		public void TestTryDeleteUnauthorizedAccessFileWithoutReportException()
		{
			AssertDeleteUnauthorizedAccessFile(false);
		}

		void AssertDeleteUnauthorizedAccessFile(bool reportException)
		{
			using (var tempFile = GetNewTempFile())
			{
				using (StreamWriter writer = new StreamWriter(tempFile.Filename))
				{
					writer.WriteLine("Just For Test");
				}

				TempFile.ThrowUnauthorizedAccessExceptionOnDelete = true;
				var result = TempFile.TryDelete(tempFile.Filename, out var message, reportException, true);
				TempFile.ThrowUnauthorizedAccessExceptionOnDelete = false;

				if (reportException)
				{
					Assert("Unable to delete file", !result);
					AssertEquals("Unable to delete file: Filename=" + tempFile.Filename, ErrorReporter.LastMessageReported);
				}
				else
				{
					Assert("Unable to delete file, just ignore it", result);
				}
			}
		}

		public void TestFileIsDeletedOnStreamClose()
		{
			var stream = (FileStream)TempFile.CreateWithDeleteOnClose();
			Assert(File.Exists(stream.Name));
			stream.Dispose();
			Assert(!File.Exists(stream.Name));
		}

		[ExpectNoExceptions]
		public void TestFileDeletedBeforeDisposeDoesNotBlowUp()
		{
			using (TempFile tempFile = GetNewTempFile())
			{
				File.Delete(tempFile.Filename);
				Assert("File no longer exists, and dispose should not cause a problem", !File.Exists(tempFile.Filename));
			}
		}

		public void TestFileCreatedAndDeleted()
		{
			string filename;
			using (TempFile temp = GetNewTempFile())
			{
				filename = temp.Filename;
				Assert(File.Exists(filename));
				File.SetAttributes(filename, FileAttributes.ReadOnly);
			}

			SleepForTestFileCreatedAndDeleted();
			AssertEquals("The temp file " + filename + " was not deleted on Dispose.", false, File.Exists(filename));
		}

		public void TestNewFromFile()
		{
			string testLine = "This is a test";
			using (TempFile file = TempFile.New())
			{
				// create a temp file to copy from
				using (StreamWriter writer = new StreamWriter(file.Filename))
				{
					writer.WriteLine(testLine);
				}

				// test copying the contents of the temp file
				using (TempFile testTempFile = TempFile.NewFromFile(file.Filename))
				{
					using (StreamReader reader = new StreamReader(testTempFile.Filename))
					{
						AssertEquals("Contents should have been copied", testLine, reader.ReadLine());
					}
				}
			}
		}

		public void TestNewFromFile_KeepExtension()
		{
			string testLine = "This is a test";
			using (TempFile file = TempFile.NewWithExtension(".abc"))
			{
				// create a temp file to copy from
				using (StreamWriter writer = new StreamWriter(file.Filename))
				{
					writer.WriteLine(testLine);
				}

				// test copying the contents of the temp file
				using (TempFile testTempFile = TempFile.NewFromFile(file.Filename, true))
				{
					using (StreamReader reader = new StreamReader(testTempFile.Filename))
					{
						AssertEquals("Contents should have been copied", testLine, reader.ReadLine());
					}

					AssertEquals("TestTempFile should have the same extension as the original", Path.GetExtension(file.Filename), Path.GetExtension(testTempFile.Filename));
				}
			}
		}

		[TestRequiresAdministrativePrivileges("required for handles.exe to work properly")]
		public void TestFileDelete()
		{
			using (TempDirectory tempDirectory = new TempDirectory())
			{
				string tempDirectoryWithSpaceInName = Path.Combine(tempDirectory.DirectoryName, "DUMMY FOLDER");
				Directory.CreateDirectory(tempDirectoryWithSpaceInName);
				using (TempFile file = TempFile.NewInDirectory(tempDirectoryWithSpaceInName))
				{
					AssertEquals("Precondition", true, File.Exists(file.Filename));

					string mes = "";
					using (var fs = File.OpenWrite(file.Filename))
					{
						try
						{
							var exeFilename = Path.GetFileNameWithoutExtension(ExeFileNames.CargoWiseWindowsDesktopExe);
							ErrorReporter.Clear();
							Assert(!TempFile.TryDelete(file.Filename, out mes));
							AssertEquals("The process cannot access the file '" + file.Filename + "' because it is being used by another process.", mes);
							var regex = new Regex(string.Format(@"Unable to delete file: Filename={0}", Regex.Escape(file.Filename)), RegexOptions.Singleline);
							AssertMatch(regex, ErrorReporter.LastMessageReported.Replace(System.Environment.NewLine, " "));
						}
						finally
						{
							ErrorReporter.Clear();
						}
					}
					AssertEquals("File should still exist", true, File.Exists(file.Filename));

					File.SetAttributes(file.Filename, FileAttributes.ReadOnly);

					TempFile.TryDelete(file.Filename, out mes);

					AssertEquals("File should have been deleted", false, File.Exists(file.Filename));
					AssertEquals("", mes);
				}
			}
		}

		protected virtual void SleepForTestFileCreatedAndDeleted()
		{
		}

		protected virtual TempFile GetNewTempFile()
		{
			return TempFile.New();
		}
	}
}
