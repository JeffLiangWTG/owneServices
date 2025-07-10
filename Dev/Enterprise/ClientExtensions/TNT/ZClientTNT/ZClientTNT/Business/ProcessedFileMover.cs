using System;
using System.IO;
using CargoWise.Types;

namespace Enterprise.Client.TNT
{
	public class ProcessedFileMover
	{
		internal ProcessedFileMover(ZString moveToDirectory)
		{
			this.MoveToDirectory = ClientSharedComponents.SharedUtil.GetFinalPath(moveToDirectory);
		}

		internal void Move(FileInfo fileToMove)
		{
			if (fileToMove.Exists)
			{
				ZDateTime currentDateTime = ZDateTime.Now;
				ZString currentDateString = currentDateTime.ToString("yyyyMMdd");
				ZString currentTimeString = currentDateTime.ToString("HHmmss");

				DirectoryInfo directory = ProcessedDirectory(currentDateString);

				if (!directory.Exists)
				{
					directory.Create();
				}

				String fileName = fileToMove.Name;
				fileName += "." + currentDateString + "." + currentTimeString;
				FileInfo newFile = new FileInfo(Path.Combine(directory.FullName, fileName));

				try
				{
					fileToMove.MoveTo(newFile.FullName);
				}
				catch (IOException) { }
			}
		}

		DirectoryInfo ProcessedDirectory(string currentDateString)
		{
			return new DirectoryInfo(Path.Combine(MoveToDirectory, currentDateString));
		}

		internal
		readonly ZString MoveToDirectory;
	}
}
