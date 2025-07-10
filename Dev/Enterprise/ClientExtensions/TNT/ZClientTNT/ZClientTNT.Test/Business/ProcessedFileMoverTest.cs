using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.TNT
{
	public class ProcessedFileMoverTest : TestCaseWithFactory
	{
		public void TestProcessedFileMover()
		{
			ZString moveToDirectory = "TestMoveToDirectory";
			ProcessedFileMover fileMover = new ProcessedFileMover(moveToDirectory);
			AssertEquals("MoveToDirectory", moveToDirectory, fileMover.MoveToDirectory);
		}

		[TestDate(2005, 11, 22, 20, 40, 35)]
		public void TestMove()
		{
			using (TempFile tempFile = TempFile.New())
			{
				FileInfo fileToMove = new FileInfo(tempFile.Filename);
				ZString moveToDirectory = Env.TempPath + @"TNTProcessedFileMover";
				try
				{
					if (!Directory.Exists(moveToDirectory))
					{
						Directory.CreateDirectory(moveToDirectory);
					}

					ZString processedDirectory = moveToDirectory + @"\20051122\";
					if (Directory.Exists(processedDirectory))
					{
						TempDirectory.DeleteDirectory(processedDirectory);
					}

					AssertEquals("PreCondition: MoveToDirectory '" + moveToDirectory + "' should exist", true, Directory.Exists(moveToDirectory));
					AssertEquals("PreCondition: ProcessedDirectory '" + processedDirectory + "' should not exist", false, Directory.Exists(processedDirectory));
					AssertEquals("PreCondition: File '" + tempFile.Filename + "' should exist", true, File.Exists(tempFile.Filename));
					ProcessedFileMover fileMover = new ProcessedFileMover(moveToDirectory);
					fileMover.Move(fileToMove);
					AssertEquals("File '" + tempFile.Filename + "' should not exist", false, File.Exists(tempFile.Filename));
					AssertEquals("ProcessedDirectory '" + processedDirectory + "' should exist", true, Directory.Exists(processedDirectory));
					ZString processedFile = processedDirectory + Path.GetFileName(tempFile.Filename) + ".20051122.204035";
					AssertEquals("ProcessedFile '" + processedFile + "' should exist", true, File.Exists(processedFile));
				}
				finally
				{
					TempDirectory.DeleteDirectory(moveToDirectory);
				}
			}
		}
	}
}
