using System.IO;
using System.Text;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class Level1FileReader
	{
		#region Constants

		public static class Constants
		{
			public const int FirstHumanReadableChar = 32;
			public const int LastHumanReadableChar = 126;
			public const int CarriageReturn = 13;
			public const int LineFeed = 10;
			public const int EndOfStream = -1;
			public const int Space = 32;
			public const int FileLineLength = 378;
		}

		#endregion

		public Level1FileReader(string fileName)
		{
			this.FileName = fileName;
		}
		readonly string FileName;

		public Level1RecordList Level1RecordList
		{
			get
			{
				if (fLevel1RecordList == null)
				{
					fLevel1RecordList = new Level1RecordList();
					ReadLevel1LinesFromFile();
				}

				return fLevel1RecordList;
			}
		}
		Level1RecordList fLevel1RecordList;

		void ReadLevel1LinesFromFile()
		{
			byte[] buffer = ReadFileBytes();
			StringBuilder lineBuilder = new StringBuilder(Constants.FileLineLength);

			for (int i = 0; i < buffer.Length; i++)
			{
				int currentByte = buffer[i];
				if (currentByte == Constants.CarriageReturn)
				{
					if ((i + 1) < buffer.Length)
					{
						int nextByte = buffer[i + 1];
						if (nextByte == Constants.LineFeed)
						{
							fLevel1RecordList.RecordLines.Add(lineBuilder.ToString());
							lineBuilder.Remove(0, lineBuilder.Length);
							i++;
							continue;
						}
					}
				}

				if (currentByte > Constants.LastHumanReadableChar || currentByte < Constants.FirstHumanReadableChar)
				{
					currentByte = Constants.Space;
				}

				lineBuilder.Append((char)currentByte);
			}

			if (lineBuilder.ToString().Length == Constants.FileLineLength)
			{
				fLevel1RecordList.RecordLines.Add(lineBuilder.ToString());
			}
		}

		byte[] ReadFileBytes()
		{
			byte[] result;
			using (FileStream fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read))
			{
				result = new byte[fileStream.Length];
				BinaryReader binaryReader = new BinaryReader(fileStream);
				result = binaryReader.ReadBytes((int)fileStream.Length);
			}
			return result;
		}
	}
}



