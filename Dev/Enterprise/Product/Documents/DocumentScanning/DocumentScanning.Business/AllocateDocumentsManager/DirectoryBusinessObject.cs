using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	[DebuggerDisplay("Dir({Name})")]
	public class DirectoryBusinessObject : NonPersistentBusinessObject
	{
		public IFileSystem FileSystem { get; }
		public ZString FullPath { get; }
		public ZString Name
		{
			get
			{
				var trimmed = FullPath.TrimEnd('\\');
				var name = Path.GetFileName(trimmed);
				return string.IsNullOrEmpty(name) ? trimmed : (ZString)name;
			}
		}

		public DirectoryBusinessObject(IFileSystem fileSystem, string fullPath)
		{
			Argument.NotNull(fullPath, nameof(fullPath));
			FileSystem = Argument.NotNull(fileSystem, nameof(fileSystem));
			FullPath = fullPath.EndsWith("\\", StringComparison.OrdinalIgnoreCase) ? fullPath : (fullPath + "\\");
		}

		public bool IsAccessable => FileSystem.CanAccessDirectory(FullPath);

		public DirectoryBusinessObjectCollection Directories
		{
			get
			{
				if (directories == null)
				{
					directories = new DirectoryBusinessObjectCollection(this);
					directories.Load();
				}
				return directories;
			}
		}
		DirectoryBusinessObjectCollection directories;

		public FileBusinessObjectCollection Files
		{
			get
			{
				if (files == null)
				{
					files = new FileBusinessObjectCollection(this);
					files.Load();
				}

				return files;
			}
		}
		FileBusinessObjectCollection files;
	}

	public class DirectoryBusinessObjectCollection : NonPersistentBusinessObjectCollection<DirectoryBusinessObject>
	{
		public DirectoryBusinessObject Root { get; }
		IFileSystem FileSystem => Root.FileSystem;

		public DirectoryBusinessObjectCollection(DirectoryBusinessObject root)
		{
			Root = root;
		}

		public DirectoryBusinessObject this[string directoryName]
		{
			get => this.Cast<DirectoryBusinessObject>().FirstOrDefault(dir => dir.Name.EqualsIgnoringCase(directoryName));
		}

		public override void Load()
		{
			if (Root.IsAccessable)
			{
				foreach (var path in FileSystem.GetDirectories(Root.FullPath))
				{
					Add(new DirectoryBusinessObject(FileSystem, path));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Please use the constructor that accepts a path");
		}

		protected override bool AllowNewCore => false;
	}
}
