using System;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup
{
	public struct DatabaseFile
	{
		public DatabaseFile(string logicalNameValue, string physicalName, string type)
		{
			if (type != DataFileType && type != LogFileType)
			{
				throw new ArgumentException(Invariant($"Invalid file type [{type}]. Valid values are [{DataFileType}, {LogFileType}]."), nameof(type));
			}

			LogicalName = logicalNameValue;
			PhysicalName = physicalName;
			Type = (type == DataFileType) ? FileType.Data : FileType.Log;
		}

		public string LogicalName { get; }

		public string PhysicalName { get; }

		public FileType Type { get; }

		const string DataFileType = "D";
		const string LogFileType = "L";

		public enum FileType
		{
			Data,
			Log,
		}
	}
}
