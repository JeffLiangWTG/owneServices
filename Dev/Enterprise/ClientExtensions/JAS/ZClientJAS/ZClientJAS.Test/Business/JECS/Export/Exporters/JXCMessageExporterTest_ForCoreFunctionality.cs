using System;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class JXCMessageExporterTest_ForCoreFunctionality : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestContructor_NullParams()
		{
			new JXCMessageExporterForTest(null, null);
		}

		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", HeaderData, MessageExporter.HeaderData);
			AssertEquals("Should be assigned in the constructor", NotificationBuffer, MessageExporter.NotificationSubscriber);
		}

		public void TestWriteToFile()
		{
			string outputDirName = JASDataRegistry.Instance.JXCOutgoingDirectoryName;
			MessageExporter.WriteToFile();
			AssertEquals("There should be two info notifications indicating that the export was started and successful", 2, NotificationBuffer.Events.Length);
			AssertEquals(string.Format("Start Exporting JXC message for 'MEHMEH' by {0} ({1})", GlbStaff.CurrentUser.GS_LoginName, GlbStaff.CurrentUser.GS_FullName), ((InfoNotification)NotificationBuffer.Events[0]).AdditionalInfo);
			AssertEquals("JXC Message for 'MEHMEH' has been successfully exported to \"" + outputDirName + "\"", ((InfoNotification)NotificationBuffer.Events[1]).AdditionalInfo);
			string targetFilePathName1 = Path.Combine(outputDirName, "jxctestfile1.txt");
			string targetFilePathName2 = Path.Combine(outputDirName, "jxctestfile2.txt");
			Assert("File should exist", File.Exists(targetFilePathName1));
			Assert("File should exist", File.Exists(targetFilePathName2));
			using (StreamReader reader = File.OpenText(targetFilePathName1))
			{
				string content = reader.ReadToEnd();
				AssertEquals(ExpectedFileContent1, content);
			}

			using (StreamReader reader = File.OpenText(targetFilePathName2))
			{
				string content = reader.ReadToEnd();
				AssertEquals(ExpectedFileContent2, content);
			}
		}

		public void TestWriteToFile_OutputDirSpecified()
		{
			string outputDirName = Path.Combine(JASDataRegistry.Instance.JXCOutgoingDirectoryName, "NewOutputDir");
			Directory.CreateDirectory(outputDirName);
			MessageExporter.WriteToFile(outputDirName);
			AssertEquals("There should be two info notifications indicating that the export was started and successful", 2, NotificationBuffer.Events.Length);
			AssertEquals(string.Format("Start Exporting JXC message for 'MEHMEH' by {0} ({1})", GlbStaff.CurrentUser.GS_LoginName, GlbStaff.CurrentUser.GS_FullName), ((InfoNotification)NotificationBuffer.Events[0]).AdditionalInfo);
			AssertEquals("JXC Message for 'MEHMEH' has been successfully exported to \"" + outputDirName + "\"", ((InfoNotification)NotificationBuffer.Events[1]).AdditionalInfo);
			string targetFilePathName1 = Path.Combine(outputDirName, "jxctestfile1.txt");
			string targetFilePathName2 = Path.Combine(outputDirName, "jxctestfile2.txt");
			Assert("File should exist", File.Exists(targetFilePathName1));
			Assert("File should exist", File.Exists(targetFilePathName2));
			using (StreamReader reader = File.OpenText(targetFilePathName1))
			{
				string content = reader.ReadToEnd();
				AssertEquals(ExpectedFileContent1, content);
			}

			using (StreamReader reader = File.OpenText(targetFilePathName2))
			{
				string content = reader.ReadToEnd();
				AssertEquals(ExpectedFileContent2, content);
			}
		}

		public void TestWriteToFile_NoDataToExport()
		{
			string outputDirName = JASDataRegistry.Instance.JXCOutgoingDirectoryName;
			MessageExporter.ContentShouldBeEmpty = true;
			MessageExporter.WriteToFile();
			AssertEquals("There should be one warning notifications indicating that there is nothing to export", 1, NotificationBuffer.Events.Length);
			AssertEquals("No data to be exported", ((WarningNotification)NotificationBuffer.Events[0]).AdditionalInfo);
		}

		public void TestWriteToFile_IOExceptionThrown()
		{
			MessageExporter.ThrowIOExceptionWhenWriteToFileCoreIsCalled = true;
			AssertEquals("Pre-condition", 0, NotificationBuffer.Events.Length);
			MessageExporter.WriteToFile();
			AssertEquals(1, NotificationBuffer.Events.Length);
			ErrorNotification errorNotification = NotificationBuffer.Events[0] as ErrorNotification;
			AssertEquals(ErrorType.IOError, errorNotification.ErrorType);
			AssertEquals("Cannot create and/or write file. SOME MESSAGE", errorNotification.AdditionalInfo);
		}

		public void TestWriteToFile_OutgoingDirectoryIsNotAccessibleAndCannotBeCreated()
		{
			JASDataRegistry.Instance.JXCOutgoingDirectoryName = "AA:\\DirectoryWhichDoesNotExist";
			AssertEquals("Pre-condition", 0, NotificationBuffer.Events.Length);
			MessageExporter.WriteToFile();
			AssertEquals(1, NotificationBuffer.Events.Length);
			ErrorNotification errorNotification = NotificationBuffer.Events[0] as ErrorNotification;
			AssertEquals(ErrorType.Error, errorNotification.ErrorType);
			AssertEquals("Cannot create Export Path \"AA:\\DirectoryWhichDoesNotExist\". Please check if path is valid", errorNotification.AdditionalInfo);
		}

		public void TestWriteToFile_FileAlreadyExists()
		{
			string outputFilePath = Path.Combine(JASDataRegistry.Instance.JXCOutgoingDirectoryName, "jxctestfile1.txt");
			using (File.CreateText(outputFilePath))
			{
			}

			Assert("Sanity check", File.Exists(outputFilePath));
			AssertEquals("Pre-condition", 0, NotificationBuffer.Events.Length);
			MessageExporter.WriteToFile();
			AssertEquals("There should be two info notification indicating that the export was started and successful. The existing file should be overwritten", 2, NotificationBuffer.Events.Length);
			AssertEquals(string.Format("Start Exporting JXC message for 'MEHMEH' by {0} ({1})", GlbStaff.CurrentUser.GS_LoginName, GlbStaff.CurrentUser.GS_FullName), ((InfoNotification)NotificationBuffer.Events[0]).AdditionalInfo);
			AssertEquals("JXC Message for 'MEHMEH' has been successfully exported to \"" + JASDataRegistry.Instance.JXCOutgoingDirectoryName + "\"", ((InfoNotification)NotificationBuffer.Events[1]).AdditionalInfo);
			using (StreamReader reader = File.OpenText(outputFilePath))
			{
				string content = reader.ReadToEnd();
				AssertEquals(ExpectedFileContent1, content);
			}
		}

		public void TestWriteToFile_FileNameIsInvalid()
		{
			AssertNoExceptionThrown(() => MessageExporterWithInvalidFileNames.WriteToFile());
		}

		#region Implementation
		JXCMessageExporterForTest MessageExporter
		{
			get
			{
				if (fMessageExporter == null)
				{
					fMessageExporter = new JXCMessageExporterForTest(HeaderData, NotificationBuffer);
				}

				return fMessageExporter;
			}
		}

		JXCMessageExporterForTest MessageExporterWithInvalidFileNames
		{
			get
			{
				return messageExporterWithInvalidFileNames ?? (messageExporterWithInvalidFileNames = new JXCMessageExporterWithInvalidFileNamesForTest(HeaderData, NotificationBuffer));
			}
		}

		JXCHeaderForTest HeaderData
		{
			get
			{
				if (fHeaderData == null)
				{
					fHeaderData = new JXCHeaderForTest("SO", "DO", "GBLON", "DN", "SN");
				}

				return fHeaderData;
			}
		}

		NotificationBuffer NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new NotificationBuffer();
				}

				return fNotificationBuffer;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			OutgoingDir = JASDataRegistry.Instance.JXCOutgoingDirectoryName;
			JXCTestDirName = Path.Combine(Env.TempPath, "JXCTest");
			JASDataRegistry.Instance.JXCOutgoingDirectoryName = JXCTestDirName;
			Directory.CreateDirectory(JASDataRegistry.Instance.JXCOutgoingDirectoryName);
#if WINZOR // In winzor, we need a JSRuntime instance in order to access JavaScript so that a file can be written
			var form = new System.Windows.Forms.Form();
			form.Show();
#endif
		}

		protected override void TearDown()
		{
			TempDirectory.DeleteDirectory(JXCTestDirName);
			JASDataRegistry.Instance.JXCOutgoingDirectoryName = OutgoingDir;
			base.TearDown();
		}

		#region JXCMessageExporterForTest
		class JXCMessageExporterWithInvalidFileNamesForTest : JXCMessageExporterForTest
		{
			public JXCMessageExporterWithInvalidFileNamesForTest(IJXCExportHeader headerData, INotifications notificationSubscriber) : base(headerData, notificationSubscriber)
			{
			}

			protected override MessageFileNameAndContents[] GetMessageFileNamesAndContents()
			{
				return new MessageFileNameAndContents[] { new MessageFileNameAndContents(String.Format("abc{0}.txt", Path.GetInvalidFileNameChars().First()), new MessageLine[] { Line1, Line4 }) };
			}
		}

		class JXCMessageExporterForTest : JXCMessageExporter
		{
			public JXCMessageExporterForTest(IJXCExportHeader headerData, INotifications notificationSubscriber) : base(headerData, notificationSubscriber)
			{
			}

			protected override MessageFileNameAndContents[] GetMessageFileNamesAndContents()
			{
				MessageFileNameAndContents[] result;
				if (ContentShouldBeEmpty)
				{
					result = Array.Empty<MessageFileNameAndContents>();
				}
				else
				{
					result = new MessageFileNameAndContents[2] { new MessageFileNameAndContents("jxctestfile1.txt", new MessageLine[] { Line1, Line2 }), new MessageFileNameAndContents("jxctestfile2.txt", new MessageLine[] { Line3, Line4 }) };
				}

				return result;
			}

			protected override bool WriteToFileCore(ZString exportPath)
			{
				if (ThrowIOExceptionWhenWriteToFileCoreIsCalled)
				{
					throw new IOException("SOME MESSAGE");
				}
				else
				{
					return base.WriteToFileCore(exportPath);
				}
			}

			public override JXCExportValidationType ExportValidationTypeToUse
			{
				get
				{
					return JXCExportValidationType.None;
				}
			}

			#region Lines
			protected MessageLineForTest Line1
			{
				get
				{
					if (fLine1 == null)
					{
						fLine1 = new MessageLineForTest("LINE1", "L1F1", "L1F2", "L1F3");
					}

					return fLine1;
				}
			}

			protected MessageLineForTest Line2
			{
				get
				{
					if (fLine2 == null)
					{
						fLine2 = new MessageLineForTest("LINE2", "L2F1", "L2F2", "L2F3");
					}

					return fLine2;
				}
			}

			protected MessageLineForTest Line3
			{
				get
				{
					if (fLine3 == null)
					{
						fLine3 = new MessageLineForTest("LINE3", "L3F1", "L3F2", "L3F3");
					}

					return fLine3;
				}
			}

			protected MessageLineForTest Line4
			{
				get
				{
					if (fLine4 == null)
					{
						fLine4 = new MessageLineForTest("LINE4", "L4F1", "L4F2", "L4F3");
					}

					return fLine4;
				}
			}

			MessageLineForTest fLine1;
			MessageLineForTest fLine2;
			MessageLineForTest fLine3;
			MessageLineForTest fLine4;
			#endregion
			public bool ThrowIOExceptionWhenWriteToFileCoreIsCalled;
			public bool ContentShouldBeEmpty;
		}

		#endregion
		#region ExpectedFileContent
		const string ExpectedFileContent1 = "HEAD3100;DO;SO;DN;SN;GBLON\r\n" + "LINE13100;L1F1;L1F2;L1F3\r\n" + "LINE23100;L2F1;L2F2;L2F3\r\n" + "TRLR3100";
		const string ExpectedFileContent2 = "HEAD3100;DO;SO;DN;SN;GBLON\r\n" + "LINE33100;L3F1;L3F2;L3F3\r\n" + "LINE43100;L4F1;L4F2;L4F3\r\n" + "TRLR3100";
		#endregion
		JXCMessageExporterForTest fMessageExporter, messageExporterWithInvalidFileNames;
		NotificationBuffer fNotificationBuffer;
		JXCHeaderForTest fHeaderData;
		string OutgoingDir;
		string JXCTestDirName;
#endregion
	}
}
