using System.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	public class XXXAirCargoFileImporterTest : AirCargoFileImporterTestCase
	{
		[GuiTest, TestDate(2005, 11, 22, 20, 40, 35)]
		public override void TestImport()
		{
			using (ZForm form = new ZForm())
			{
				XXXAirCargoFileImporter importer = new XXXAirCargoFileImporter(form);
				importer.TestFilePath = InvalidFileFormatTest;
				string expectedErrorMessage = "XXX Source Directory has not been correctly setup in the Registry." + System.Environment.NewLine;
				expectedErrorMessage += "XXX Processed Directory has not been correctly setup in the Registry." + System.Environment.NewLine;
				AssertImport("", "", importer, expectedErrorMessage);
				AssertEquals("PreCondition: File '" + importer.TestFilePath + "' should exist", true, File.Exists(importer.TestFilePath));
				expectedErrorMessage = @"Error: Invalid Record Delimiter.  Record Delimiter must contain a dot ("".""). ( Error was encouter on Record 01 (MasterBill=08143374214))" + System.Environment.NewLine;
				expectedErrorMessage += @"Error: Invalid file format (First record is not a valid XXX Flight Record)" + System.Environment.NewLine;
				AssertImport(SourceDirectory, ProcessedDirectory, importer, expectedErrorMessage);
				AssertEquals("File '" + importer.TestFilePath + "' should not exist", false, File.Exists(importer.TestFilePath));
				importer.TestFilePath = NotValidFileTest;
				expectedErrorMessage = "File '" + NotValidFileTest + "' is not an XXX file";
				AssertImport(SourceDirectory, ProcessedDirectory, importer, expectedErrorMessage);
			}
		}

		void AssertImport(ZString xXXFileSourceDirectory, ZString xXXFileProcessedDirectory, XXXAirCargoFileImporter importer, ZString expectedErrorMessage)
		{
			ZString xXXFileSourceDirectoryPreviousValue = TNTDataRegistry.Instance.XXXFileSourceDirectory;
			ZString xXXFileProcessedDirectoryPreviousValue = TNTDataRegistry.Instance.XXXFileProcessedDirectory;
			try
			{
				TNTDataRegistry.Instance.XXXFileSourceDirectory = xXXFileSourceDirectory;
				TNTDataRegistry.Instance.XXXFileProcessedDirectory = xXXFileProcessedDirectory;
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
				TNTDataRegistry.Instance.XXXFileSourceDirectory = xXXFileSourceDirectoryPreviousValue;
				TNTDataRegistry.Instance.XXXFileProcessedDirectory = xXXFileProcessedDirectoryPreviousValue;
			}
		}

		#region InvalidFileFormat
		protected override FileInfo InvalidFileFormat
		{
			get
			{
				if (fInvalidFileFormat == null)
				{
					fInvalidFileFormat = new FileInfo(invalidFilePath);
				}

				return fInvalidFileFormat;
			}
		}

		FileInfo fInvalidFileFormat;
		#endregion
		#region NotValidFile
		protected override FileInfo NotValidFile
		{
			get
			{
				if (fNotValidFile == null)
				{
					fNotValidFile = new FileInfo(notValidFilePath);
				}

				return fNotValidFile;
			}
		}

		FileInfo fNotValidFile;
		#endregion

		string invalidFilePath;
		string notValidFilePath;
		protected override void SetUp()
		{
			base.SetUp();
			invalidFilePath = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.XXXInterface.Testing.SYD.20050811.091011.xxx");
			notValidFilePath = resourceRetriever.SaveResourceToFile("Enterprise.Client.TNT.Testing.DataManipulation.AirCargo.XXXInterface.Testing.SYD.NotXXXFile.ok");
		}
	}
}
