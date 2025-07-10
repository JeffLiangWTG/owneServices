using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Business
{
	public class FileBusinessObject : NonPersistentBusinessObject
	{
		public DirectoryBusinessObject Directory { get; }
		public ZString FileNameWithExtension { get; }

		public ZString FullPath => Path.Combine(Directory.FullPath, FileNameWithExtension);
		public ZString FileName => Path.GetFileNameWithoutExtension(FileNameWithExtension);
		public ZString Extension => Path.GetExtension(FullPath).TrimStart('.');

		public FileBusinessObject(DirectoryBusinessObject root, string fileName)
		{
			if (!string.IsNullOrEmpty(Path.GetDirectoryName(fileName)))
			{
				throw new ArgumentException("FileName should only be the file name");
			}

			this.Directory = Argument.NotNull(root, nameof(root));
			this.FileNameWithExtension = Argument.NotNullOrEmpty(fileName, nameof(fileName));
		}

		public override bool CanDelete => true;

		public override void Delete()
		{
			try
			{
				File.Delete(FullPath);
				base.Delete();
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				throw new CannotDeleteException(Res.GetString("572c4853-3995-4629-8e6f-7f40577431d1", "Could not delete {0}", FileName));
			}
		}

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			return ResString.GetMultilingualString("2ed81dcb-e97e-43b6-a0c2-8475149f2922", "Deleting is irreversible.");
		}
	}

	public class FileBusinessObjectCollection : NonPersistentBusinessObjectCollection<FileBusinessObject>
	{
		public DirectoryBusinessObject Root { get; }

		public FileBusinessObjectCollection(DirectoryBusinessObject root)
		{
			Root = root;
		}

		public override void Load()
		{
			foreach (var filePath in Root.FileSystem.GetFiles(Root.FullPath))
			{
				Add(new FileBusinessObject(Root, Path.GetFileName(filePath)));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Please use the constructor that accepts a path");
		}

		protected override bool AllowNewCore => false;
	}
}
