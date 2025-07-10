using System.Collections;
using System.IO;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public struct DirectoryEntry
	{
		public DirectoryEntry(string parentDirectory, string name, bool isFile)
		{
			this.ParentDirectory = parentDirectory;
			this.Name = name;
			this.EntryType = (isFile) ? DirectoryEntryType.File : DirectoryEntryType.Folder;
			//this.IsFile = isFile;
		}

		public readonly string ParentDirectory;
		public readonly string Name;
		//public readonly bool IsFile;
		public readonly DirectoryEntryType EntryType;

		public string FullPath
		{
			get { return Path.Combine(ParentDirectory, Name); }
		}
	}

	public class DirectoryEntryCollection : CollectionBase
	{
		public DirectoryEntry this[int index]
		{
			get { return (DirectoryEntry)List[index]; }
		}

		public int Add(DirectoryEntry element)
		{
			return List.Add(element);
		}

		public void AddRange(DirectoryEntryCollection elements)
		{
			foreach (DirectoryEntry element in elements)
			{
				List.Add(element);
			}
		}
	}

	public enum DirectoryEntryType : int
	{
		Folder = 0,
		File = 1
	}
}
