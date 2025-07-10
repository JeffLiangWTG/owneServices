using System.IO;
using CargoWise.Types;
using Enterprise.Client.TNT.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	public class IQDownAirCargoFileImporterTest : AirCargoFileImporterTestCase
	{
		[GuiTest, TestDate(2005, 11, 22, 20, 40, 35)]
		public override void TestImport()
		{
			using (ZForm form = new ZForm())
			{
				IQDownAirCargoFileImporter importer = new IQDownAirCargoFileImporter(form);
				importer.TestFilePath = InvalidFileFormatTest;
				string expectedErrorMessage = "IQDown Source Directory has not been correctly setup in the Registry." + System.Environment.NewLine;
				expectedErrorMessage += "IQDown Processed Directory has not been correctly setup in the Registry." + System.Environment.NewLine;
				AssertImport("", "", importer, expectedErrorMessage);
				AssertEquals("PreCondition: File '" + importer.TestFilePath + "' should exist", true, File.Exists(importer.TestFilePath));
				expectedErrorMessage = @"Error: Invalid Record Delimiter.  Record Delimiter must contain a dot ("".""). ( Error was encouter on Record 01 (MasterBill=08143374214))" + System.Environment.NewLine;
				expectedErrorMessage += @"Error: Invalid file format (First record is not a valid IQDown Flight Record)" + System.Environment.NewLine;
				AssertImport(SourceDirectory, ProcessedDirectory, importer, expectedErrorMessage);
				AssertEquals("File '" + importer.TestFilePath + "' should not exist", false, File.Exists(importer.TestFilePath));
				importer.TestFilePath = NotValidFileTest;
				expectedErrorMessage = "File '" + NotValidFileTest + "' is not an IQDown file";
				AssertImport(SourceDirectory, ProcessedDirectory, importer, expectedErrorMessage);
			}
		}

		void AssertImport(ZString iQDownFileSourceDirectory, ZString iQDownFileProcessedDirectory, IQDownAirCargoFileImporter importer, ZString expectedErrorMessage)
		{
			ZString iQDownFileSourceDirectoryPreviousValue = TNTDataRegistry.Instance.IQDownFileSourceDirectory;
			ZString iQDownFileProcessedDirectoryPreviousValue = TNTDataRegistry.Instance.IQDownFileProcessedDirectory;
			try
			{
				TNTDataRegistry.Instance.IQDownFileSourceDirectory = iQDownFileSourceDirectory;
				TNTDataRegistry.Instance.IQDownFileProcessedDirectory = iQDownFileProcessedDirectory;
				AssertNotNull("Importer should exist", importer);
				UnitTestUserNotification userNotification = Globals.Message as UnitTestUserNotification;
				AssertNotNull("UserNotification should not be null", userNotification);
				userNotification.ClearMessagesAndAnswers();
				AssertEquals("Initial Length (UserNotification.None)", 1, userNotification.PreviousMessages.Length);
				importer.Import();
				var lastMessage = userNotification.LastMessage.Text ?? string.Empty;
				AssertEquals("Expected 1 new error message", 2, userNotification.PreviousMessages.Length);
				AssertEquals("Last Error Message:" + System.Environment.NewLine + lastMessage, expectedErrorMessage, lastMessage);
			}
			finally
			{
				TNTDataRegistry.Instance.IQDownFileSourceDirectory = iQDownFileSourceDirectoryPreviousValue;
				TNTDataRegistry.Instance.IQDownFileProcessedDirectory = iQDownFileProcessedDirectoryPreviousValue;
			}
		}

		TNTTestUtils TestUtils;
		protected override void SetUp()
		{
			base.SetUp();
			TestUtils = new TNTTestUtils();
			invalidFileFormatPath = TestUtils.CopyResourceToFile("SYD.IND.20050811.091011.ok", "Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.INDInterface.Testing.");
			notVaildFilePath = TestUtils.CopyResourceToFile("SYD.EX1.NotIQDownFile.TXT", "Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.INDInterface.Testing.");
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestUtils.Dispose();
		}

		#region InvalidFileFormat
		protected override FileInfo InvalidFileFormat
		{
			get
			{
				if (fInvalidFileFormat == null)
				{
					fInvalidFileFormat = new FileInfo(invalidFileFormatPath);
				}

				return fInvalidFileFormat;
			}
		}
		string invalidFileFormatPath;

		FileInfo fInvalidFileFormat;
		#endregion
		#region NotValidFile
		protected override FileInfo NotValidFile
		{
			get
			{
				if (fNotValidFile == null)
				{
					fNotValidFile = new FileInfo(notVaildFilePath);
				}

				return fNotValidFile;
			}
		}
		string notVaildFilePath;

		FileInfo fNotValidFile;
		#endregion
	}
}
