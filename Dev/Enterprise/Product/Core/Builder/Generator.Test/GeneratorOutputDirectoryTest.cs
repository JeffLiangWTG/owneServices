using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CargoWise.Shared;
using Enterprise.Builder.Generator;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class GeneratorOutputDirectoryTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckinLog()
		{
			using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut))
			{
				try
				{
					MockSourceControl.Setup();
					string filePath = Path.Combine(MockSourceControl.MockWorkspacePath, "ZArchitecture\\ZArchitecture.sln");
					string filePath2 = Path.Combine(MockSourceControl.MockWorkspacePath, "Clients\\TestClient\\Documents\\TestClientDocuments.xml");

					CopyFile(generatorOutputDirectory, filePath);
					CopyFile(generatorOutputDirectory, filePath2);

					FileIO.MakeFileWriteable(filePath);
					FileIO.MakeFileWriteable(filePath2);

					File.WriteAllText(filePath, "123456");
					File.WriteAllText(filePath2, "qwqwqw");

					generatorOutputDirectory.Save();

					AssertEquals(2, File.ReadAllLines(generatorOutputDirectory.CheckinLog).Length);
				}
				finally
				{
					MockSourceControl.TearDown();
				}
			}
		}

		void CopyFile(GeneratorOutputDirectory generator, string enterprisePath)
		{
			string file = generator.PrepareDestination(enterprisePath);
			File.Copy(enterprisePath, file, true);
			FileIO.MakeFileWriteable(file);
		}

		public void TestCopyFileToDirectory()
		{
			var cwshared = Directory.CreateDirectory(Path.Combine(TempForTest.TempPath, "CWShared"));

			try
			{
				using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut, cwsharedSourceDirectory: cwshared.FullName))
				{
					string fileName1 = "fileIAmLookingFor1.log";
					string fileName2 = "fileIAmLookingFor2.log";
					string fromPath1 = Path.Combine(generatorOutputDirectory.DevSourceDirectory, fileName1);
					string fromPath2 = Path.Combine(generatorOutputDirectory.CWSharedSourceDirectory, fileName2);

					try
					{
						Directory.CreateDirectory(generatorOutputDirectory.DevSourceDirectory);
						Directory.CreateDirectory(generatorOutputDirectory.CWSharedSourceDirectory);
						File.WriteAllText(fromPath1, "F1");
						File.WriteAllText(fromPath2, "F2");
						CopyFile(generatorOutputDirectory, fromPath1);
						CopyFile(generatorOutputDirectory, fromPath2);

						Assert("Dev", File.Exists(Path.Combine(generatorOutputDirectory.DevOutputDirectory, fileName1)));
						Assert("CWShared", File.Exists(Path.Combine(generatorOutputDirectory.CWSharedOutputDirectory, fileName2)));
					}
					finally
					{
						FileIO.DeleteDirectory(new DirectoryInfo(generatorOutputDirectory.DevSourceDirectory));
						FileIO.DeleteDirectory(new DirectoryInfo(generatorOutputDirectory.CWSharedSourceDirectory));
					}
				}
			}
			finally
			{
				try
				{
					cwshared?.Delete(recursive: true);
				}
				catch (DirectoryNotFoundException)
				{
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUndoCheckout()
		{
			using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut))
			{
				string fileName = "fileIAmLookingFor.log";
				string fromPath = Path.Combine(generatorOutputDirectory.DevSourceDirectory, fileName);

				try
				{
					MockSourceControl.Setup();
					SimulateGeneration(fromPath, true, generatorOutputDirectory);
					File.WriteAllText(fromPath, "asdf");
					generatorOutputDirectory.Save();

					string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(fromPath);
					Assert(SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(serverPath));

					generatorOutputDirectory.UndoCheckout();
					Assert(!SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(serverPath));
				}
				finally
				{
					DeleteIfExists(fromPath);
					MockSourceControl.TearDown();
				}
			}
		}

		void SimulateGeneration(string filePath, bool existsInSourceControl, GeneratorOutputDirectory generatorOutputDirectory)
		{
			File.WriteAllText(filePath, String.Empty);

			if (existsInSourceControl)
			{
				string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(filePath);
				File.Copy(filePath, serverPath, true);
				File.SetAttributes(serverPath, FileAttributes.ReadOnly);
			}

			CopyFile(generatorOutputDirectory, filePath);

			string expectedFilePath = filePath.StartsWith(generatorOutputDirectory.CWSharedSourceDirectory)
				? Path.Combine(generatorOutputDirectory.CWSharedOutputDirectory, Path.GetFileName(filePath))
				: Path.Combine(generatorOutputDirectory.DevOutputDirectory, Path.GetFileName(filePath));

			Assert(File.Exists(expectedFilePath));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckoutOrMakeWriteable_checkout()
		{
			using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut))
			{
				string fileToCheckin = Path.Combine(MockSourceControl.MockWorkspacePath, "fileToCheckin.txt");

				try
				{
					MockSourceControl.Setup();
					SimulateGeneration(fileToCheckin, true, generatorOutputDirectory);

					string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(fileToCheckin);
					Assert(!SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(serverPath));

					File.WriteAllText(fileToCheckin, "22222");

					generatorOutputDirectory.Save();
					Assert(SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(serverPath));
				}
				finally
				{
					DeleteIfExists(fileToCheckin);
					MockSourceControl.TearDown();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckoutOrMakeWriteable_checkout_notinsourcecontrol()
		{
			using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut))
			{
				string fileToCheckin = Path.Combine(MockSourceControl.MockWorkspacePath, "fileToCheckin.txt");

				try
				{
					MockSourceControl.Setup();
					SimulateGeneration(fileToCheckin, false, generatorOutputDirectory);

					File.WriteAllText(fileToCheckin, "CHECKIN");
					string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(fileToCheckin);

					generatorOutputDirectory.Save();
					Assert(!SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(serverPath));
				}
				finally
				{
					DeleteIfExists(fileToCheckin);
					MockSourceControl.TearDown();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckoutOrMakeWriteable_writeable_notInSourceControl()
		{
			using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.MakeWriteable))
			{
				string filenotinsourcecontrol = Path.Combine(MockSourceControl.MockWorkspacePath, "file.txt");

				try
				{
					MockSourceControl.Setup();
					SimulateGeneration(filenotinsourcecontrol, false, generatorOutputDirectory);

					string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(filenotinsourcecontrol);

					File.WriteAllText(filenotinsourcecontrol, "CHANGE");
					generatorOutputDirectory.Save();
					Assert((File.GetAttributes(filenotinsourcecontrol) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly);
					Assert(!SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(serverPath));
					Assert(!File.Exists(serverPath));
				}
				finally
				{
					DeleteIfExists(filenotinsourcecontrol);
					MockSourceControl.TearDown();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckoutOrMakeWriteable_writeable()
		{
			using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.MakeWriteable))
			{
				string fileNotInSourceControl = Path.Combine(MockSourceControl.MockWorkspacePath, "file.txt");

				try
				{
					MockSourceControl.Setup();
					SimulateGeneration(fileNotInSourceControl, true, generatorOutputDirectory);

					string serverPath = SourceControlPathsForTest.ConvertLocalPathToServerPath(fileNotInSourceControl);

					generatorOutputDirectory.Save();
					Assert(!SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(serverPath));
					Assert((File.GetAttributes(fileNotInSourceControl) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly);
				}
				finally
				{
					DeleteIfExists(fileNotInSourceControl);
					MockSourceControl.TearDown();
				}
			}
		}

		public void TestDispose_Dev()
		{
			string filePath;
			string outputDirectory;

			using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut))
			{
				outputDirectory = generatorOutputDirectory.DevOutputDirectory;
				filePath = Path.Combine(generatorOutputDirectory.DevOutputDirectory, "someFileForTest.log");
				File.WriteAllText(filePath, String.Empty);
				Assert(File.Exists(filePath));
				Assert(Directory.Exists(generatorOutputDirectory.DevOutputDirectory));
			}

			Assert(!File.Exists(filePath));
			Assert(!Directory.Exists(outputDirectory));
		}

		public void TestDispose_CWShared()
		{
			string filePath;
			string outputDirectory;

			using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut))
			{
				outputDirectory = generatorOutputDirectory.CWSharedOutputDirectory;
				filePath = Path.Combine(generatorOutputDirectory.CWSharedOutputDirectory, "someFileForTest.log");
				File.WriteAllText(filePath, String.Empty);
				Assert(File.Exists(filePath));
				Assert(Directory.Exists(generatorOutputDirectory.CWSharedOutputDirectory));
			}

			Assert(!File.Exists(filePath));
			Assert(!Directory.Exists(outputDirectory));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		[SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "<Pending>")]
		public void TestWrite_Schema()
		{
			using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut))
			{
				string path = Path.Combine(MockSourceControl.MockWorkspacePath, "DataSet.cs");
				DataSet dataSet = new DataSet("zomgDataSet");
				generatorOutputDirectory.WriteToFile(path, dataSet);

				string generatorLocation = Path.Combine(generatorOutputDirectory.DevOutputDirectory, "DataSet.cs");

				Assert(File.Exists(generatorLocation));
				AssertEquals(File.ReadAllText(Path.Combine(BaseSourcePath, "Common", "Tools", "BuildTools", "BuildTools.Test", "Testing", "SchemaSaveResult.xml")), File.ReadAllText(generatorLocation));
			}
		}

		public void TestWrite_string()
		{
			using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut))
			{
				string path = Path.Combine(MockSourceControl.MockWorkspacePath, "string.cs");
				generatorOutputDirectory.WriteToFile(path, "asdf");

				string generatorLocation = Path.Combine(generatorOutputDirectory.DevOutputDirectory, "string.cs");
				Assert(File.Exists(generatorLocation));
				AssertEquals("asdf", File.ReadAllText(generatorLocation));
			}
		}

		public void TestPrepareDestinationCreatesDirectory()
		{
			using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut))
			{
				string result = generatorOutputDirectory.PrepareDestination(Path.Combine(generatorOutputDirectory.DevSourceDirectory, @"mydirectory\autobusinessobject.cs"));
				AssertEquals(Path.Combine(generatorOutputDirectory.DevOutputDirectory, @"mydirectory\autobusinessobject.cs"), result);
				Assert(Directory.Exists(Path.GetDirectoryName(result)));
				Assert(!Directory.Exists(result));
			}
		}

		[ExpectNoExceptions()]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckoutOrMakeWriteable_DestinationDirectoryNotExists()
		{
			using (GeneratorOutputDirectory generatorOutputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.MakeWriteable))
			{
				try
				{
					MockSourceControl.Setup();
					string dummyDirectory = "hello\\world";
					string dummyOutputDirectory = Path.Combine(generatorOutputDirectory.DevOutputDirectory, dummyDirectory);
					string dummySourceDirectory = Path.Combine(generatorOutputDirectory.DevSourceDirectory, dummyDirectory);
					string dummyFile = "DestinationDirectoryNotExists.xml";
					string dummyOutputFile = Path.Combine(dummyOutputDirectory, dummyFile);
					string dummySourceFile = Path.Combine(dummySourceDirectory, dummyFile);
					Directory.CreateDirectory(dummyOutputDirectory);
					File.WriteAllText(dummyOutputFile, "123456");
					Assert(string.Format("PreCondition: Directory {0} should not exists", dummySourceDirectory), !Directory.Exists(dummySourceDirectory));
					generatorOutputDirectory.Save();
					Assert(string.Format("Directory {0} should exists", dummySourceDirectory), Directory.Exists(dummySourceDirectory));
					Assert(string.Format("File {0} should exists", dummySourceFile), File.Exists(dummySourceFile));
				}
				finally
				{
					MockSourceControl.TearDown();
				}
			}
		}

		public void TestWithCWShared()
		{
			using (GeneratorOutputDirectory original = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut))
			{
				using (var replacement = original.WithCWShared("Q:\\ReplacementCWSharedPath"))
				{
					AssertEquals("Q:\\ReplacementCWSharedPath", replacement.CWSharedSourceDirectory);
					AssertEquals(original.DevSourceDirectory, replacement.DevSourceDirectory);
				}
			}
		}
	}
}
