using System;
using System.Collections;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal abstract class JXCMessageExporterTestCase : TestCaseWithFactory
	{
		protected NotificationBuffer NotificationBuffer
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

		protected JXCMessageExporterForTest ExpectedMessageExporter
		{
			get
			{
				if (fExpectedMessageExporter == null)
				{
					fExpectedMessageExporter = new JXCMessageExporterForTest();
				}

				return fExpectedMessageExporter;
			}
		}

		protected void AssertExportedMessage(JXCMessageExporter messageExporter)
		{
			string[] generatedContents = GetExportedFileContent(messageExporter);
			string[] expectedContents = GetExportedFileContent(ExpectedMessageExporter);
			AssertEquals("The number of files generated is not as expected", expectedContents.Length, generatedContents.Length);
			for (int i = 0; i < expectedContents.Length; i++)
			{
				AssertMultilineEquals("Assert the message structure", expectedContents[i], generatedContents[i], '\r');
			}
		}

		string[] GetExportedFileContent(JXCMessageExporter messageExporter)
		{
			ArrayList list = new ArrayList();
			ZString dirPath = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			try
			{
				messageExporter.WriteToFile(dirPath);
				if (Directory.Exists(dirPath))
				{
					string[] fileNames = Directory.GetFiles(dirPath);
					Array.Sort(fileNames);
					foreach (string fileName in fileNames)
					{
						using (StreamReader reader = new StreamReader(fileName))
						{
							list.Add(reader.ReadToEnd());
						}
					}
				}
			}
			finally
			{
				TempDirectory.DeleteDirectory(dirPath);
			}

			return (string[])list.ToArray(typeof(string));
		}

		NotificationBuffer fNotificationBuffer;
		JXCMessageExporterForTest fExpectedMessageExporter;
		#region JXCMessageForTest
		protected class JXCMessageExporterForTest : JXCMessageExporter
		{
			public JXCMessageExporterForTest() : base(new JXCHeaderForTest(), new NotificationBuffer())
			{
			}

			protected override MessageFileNameAndContents[] GetMessageFileNamesAndContents()
			{
				return ExpectedMessageFileNamesAndContentLines;
			}

			public new JXCHeaderForTest HeaderData
			{
				get
				{
					return (JXCHeaderForTest)base.HeaderData;
				}
			}

			public override JXCExportValidationType ExportValidationTypeToUse
			{
				get
				{
					return JXCExportValidationType.None;
				}
			}

			public MessageFileNameAndContents[] ExpectedMessageFileNamesAndContentLines;
		}
		#endregion
	}
}
