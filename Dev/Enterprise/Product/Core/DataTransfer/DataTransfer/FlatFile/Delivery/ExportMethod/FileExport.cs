using System.IO;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataTransfer.Business
{
	public class FileExport : ExportMethod
	{
		public FileExport(ExportInstructions instructions, INotifications notifications) : base(instructions, notifications)
		{
		}

		public override ExportType ExportType
		{
			get { return ExportType.File; }
		}

		public override void Deliver(ZString savedExportFile)
		{
			string finalOutputFile = savedExportFile;

			if (!Instructions.SpecifiedFilename.IsEmpty)
			{
				if (!IsRemoteFile)
				{
					File.SetAttributes(savedExportFile, FileAttributes.Normal);
					if (File.Exists(Instructions.SpecifiedFilePathWithExtension))
					{
						File.SetAttributes(Instructions.SpecifiedFilePathWithExtension, FileAttributes.Normal);
					}
					File.Copy(savedExportFile, Instructions.SpecifiedFilePathWithExtension, true);
				}
				else
				{
					using (var sourceStream = File.OpenRead(savedExportFile))
					using (var targetStream = ObjectFactory.Get<IUserFileAccess>().OpenFileSave(Instructions.SpecifiedFilePathWithExtension))
					{
						sourceStream.CopyTo(targetStream);
					}
				}
				File.Delete(savedExportFile);
				finalOutputFile = Instructions.SpecifiedFilePathWithExtension;
			}

			Instructions.SetOutputFile(finalOutputFile);
		}

		public override bool CanDeliver
		{
			get { return !Instructions.SpecifiedFilename.IsEmpty; }
		}

		protected virtual bool IsRemoteFile
		{
			get { return true; }
		}
	}
}
