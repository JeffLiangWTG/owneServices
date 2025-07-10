using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FileExportTest : TestCaseWithFactory
	{
		public void TestExportType()
		{
			ExportInstructions instructions = new ExportInstructions();
			FileExport export = new FileExport(instructions, Notify);
			AssertEquals("Should ALWAYS be file delivery type", ExportType.File, export.ExportType);
		}

		public void TestDeliverWithoutSpecifiedFilename()
		{
			ExportInstructions instructions = new ExportInstructions();
			AssertEquals("Output file should be empty", ZString.Empty, instructions.OutputFile);
			FileExport export = new FileExport(instructions, Notify);

			using (TempFile file = TempFile.New())
			{
				export.Deliver(file.Filename);
				AssertEquals("Output file should be the same as the filename passed in", file.Filename, instructions.OutputFile);
			}
		}

		public void TestDeliverWithSpecifiedFilename()
		{
			ExportInstructions instructions = new ExportInstructions();
			instructions.SpecifiedFilename = "ABC";
			instructions.FileExtension = FileExtensionType.Txt;

			AssertEquals("Output file should be empty", ZString.Empty, instructions.OutputFile);
			FileExport export = new FileExport(instructions, Notify);

			try
			{
				using (TempFile tempFile = TempFile.New())
				{
					export.Deliver(tempFile.Filename);
					AssertEquals("Output file should be the specified filename and extension", Temp.TempPath + "ABC.txt", instructions.OutputFile);
					AssertEquals("Temp file shouldn't exist anymore", false, File.Exists(tempFile.Filename));
				}
			}
			finally
			{
				DeleteTempFile(instructions.OutputFile);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliverWithSpecifiedFilenameOverwritesExistingIfSupplied()
		{
			ExportInstructions instructions = new ExportInstructions();
			instructions.SpecifiedFilename = "ABC";
			instructions.FileExtension = FileExtensionType.Txt;

			// create a file at the destination specified filename
			ZString sourceFile = Temp.TempPath + "ABC.txt";
			File.Copy(BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\FlatFile\Delivery\TestFiles\File1.txt", sourceFile, true);
			File.SetAttributes(sourceFile, FileAttributes.ReadOnly);

			try
			{
				AssertEquals("Output filename should be empty", ZString.Empty, instructions.OutputFile);
				FileExport export = new LocalOnlyFileExport(instructions, Notify);

				using (TempFile tempFile = TempFile.New())
				{
					export.Deliver(tempFile.Filename);
					AssertEquals("Output file should be specified file", Path.Combine(Temp.TempPath, "ABC.txt"), instructions.OutputFile);
					AssertEquals("File length contents should be empty - we overwrite the existing file (which had contents) with the contents of our temp file (whcih had nothing)", 0, new FileInfo(instructions.OutputFile).Length);
				}
			}
			finally
			{
				DeleteTempFile(instructions.OutputFile);
				DeleteTempFile(sourceFile);
			}
		}

		[ExpectNoExceptions]
		public void TestDeliver_WithReadOnlyFile()
		{
			ExportInstructions instructions = new ExportInstructions();
			instructions.SpecifiedFilename = "ABC";
			instructions.FileExtension = FileExtensionType.Txt;

			FileExport export = new LocalOnlyFileExport(instructions, Notify);

			try
			{
				using (TempFile tempFile = TempFile.New())
				{
					File.SetAttributes(tempFile.Filename, FileAttributes.ReadOnly);
					export.Deliver(tempFile.Filename);
					Assert("should deliver and delete the source file successfully even though it's readonly", !File.Exists(tempFile.Filename));
					Assert("Instructions output file exists", File.Exists(instructions.OutputFile));
				}
			}
			finally
			{
				DeleteTempFile(instructions.OutputFile);
			}
		}

		class LocalOnlyFileExport : FileExport
		{
			public LocalOnlyFileExport(ExportInstructions instructions, INotifications notifications)
				: base(instructions, notifications)
			{
			}

			protected override bool IsRemoteFile
			{
				get { return false; }
			}
		}

		public void TestCanDeliver()
		{
			ExportInstructions instructions = new ExportInstructions();
			FileExport export = new FileExport(instructions, Notify);
			AssertEquals("Can't deliver yet - no specified filename", false, export.CanDeliver);

			instructions.SpecifiedFilename = "ABC";
			AssertEquals("Can deliver now - specified filename provided. File path and extension can use defaults.", true, export.CanDeliver);
		}

		void DeleteTempFile(string filename)
		{
			if (File.Exists(filename))
			{
				File.SetAttributes(filename, FileAttributes.Normal);
				File.Delete(filename);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Notify = new NotificationBuffer();
		}

		NotificationBuffer Notify;
	}
}
