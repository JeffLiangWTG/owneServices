using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Client.TNT.AirCargo.Testing
{
	public abstract class AirCargoFileImporterTestCase : TestCaseWithFactory
	{
		public abstract void TestImport();
		protected override void TearDown()
		{
			TempDirectory.DeleteDirectory(SourceDirectory);
			TempDirectory.DeleteDirectory(ProcessedDirectory);
			base.TearDown();
			resourceRetriever.Dispose();
		}

		protected abstract FileInfo InvalidFileFormat { get; }

		protected abstract FileInfo NotValidFile { get; }

		protected EmbeddedResourceRetriever resourceRetriever;

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

		#region InvalidFileFormat
		protected ZString InvalidFileFormatTest
		{
			get
			{
				if (fInvalidFileFormatTest.IsEmpty)
				{
					fInvalidFileFormatTest = Path.Combine(SourceDirectory, InvalidFileFormat.Name);
				}

				if (!File.Exists(fInvalidFileFormatTest))
				{
					File.Copy(InvalidFileFormat.FullName, fInvalidFileFormatTest);
				}

				return fInvalidFileFormatTest;
			}
		}

		ZString fInvalidFileFormatTest;
#endregion
#region NotValidFile
		protected ZString NotValidFileTest
		{
			get
			{
				if (fNotValidFileTest.IsEmpty)
				{
					fNotValidFileTest = Path.Combine(SourceDirectory, NotValidFile.Name);
				}

				if (!File.Exists(fNotValidFileTest))
				{
					File.Copy(NotValidFile.FullName, fNotValidFileTest);
				}

				return fNotValidFileTest;
			}
		}

		ZString fNotValidFileTest;
#endregion
#region SourceDirectory
		protected ZString SourceDirectory
		{
			get
			{
				if (fSourceDirectory.IsEmpty)
				{
					fSourceDirectory = Path.Combine(Env.TempPath, "Source");
				}

				if (!Directory.Exists(fSourceDirectory))
				{
					Directory.CreateDirectory(fSourceDirectory);
				}

				return fSourceDirectory;
			}
		}

		ZString fSourceDirectory;
#endregion
#region ProcessedDirectory
		protected ZString ProcessedDirectory
		{
			get
			{
				if (fProcessedDirectory.IsEmpty)
				{
					fProcessedDirectory = Path.Combine(Env.TempPath, "Processed");
				}

				if (!Directory.Exists(fProcessedDirectory))
				{
					Directory.CreateDirectory(fProcessedDirectory);
				}

				return fProcessedDirectory;
			}
		}

		ZString fProcessedDirectory;
#endregion
	}
}
