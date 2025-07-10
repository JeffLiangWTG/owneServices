using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Business
{
	[Serializable]
	public class SerializableEDocCollection : WrappedBusinessObjectCollection
	{
		public IList<WrappedBusinessObject> ExposedItems => Items;
		protected override WrappedBusinessObject ToWrappedBusinessObject(BusinessObject baseBusinessObject)
		{
			return new SerializableEDoc((StorageDocsBase)baseBusinessObject);
		}

		internal void Add(SerializableEDoc eDoc)
		{
			Items.Add(eDoc);
		}

		public new SerializableEDoc this[int index]
		{
			get { return (SerializableEDoc)Items[index]; }
		}

		public StorageDocsBase[] ToBusinessObjects(StorageMain parent)
		{
			StorageDocsBase[] storageDocsList = new StorageDocsBase[Items.Count];

			for (int i = 0; i < Items.Count; i++)
			{
				storageDocsList[i] = ((SerializableEDoc)Items[i]).ToBusinessObject(parent);
			}

			return storageDocsList;
		}

		public string[] GetContentsAsFiles()
		{
			string[] filenames = new string[Items.Count];

			for (int i = 0; i < Items.Count; i++)
			{
				SerializableEDoc docFromClipboard = Items[i] as SerializableEDoc;
				filenames[i] = docFromClipboard.FileNameWithExtension;
			}

			return filenames;
		}

		public static void DisposeFiles(string[] files)
		{
			foreach (string filename in files)
			{
				try
				{
					File.Delete(filename);
				}
				catch // file in use, etc
				{
				}
			}
		}

		public bool ContainsPK(ZGuid pK)
		{
			for (int i = 0; i < Items.Count; i++)
			{
				if (((SerializableEDoc)Items[i]).PK == pK.ToGuid())
				{
					return true;
				}
			}
			return false;
		}

		public bool ContainsSystemGeneratedDocuments
		{
			get
			{
				foreach (SerializableEDoc element in Items)
				{
					if (element.IsSystemGenerated)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool ContainsOnlyImageFiles
		{
			get
			{
				foreach (SerializableEDoc element in Items)
				{
					if (!SerializableEDocsTools.IsImage(element.DataType))
					{
						return false;
					}
				}
				return true;
			}
		}
#if DEBUG
		public static SerializableEDocCollection New(BusinessObject[] elements)
		{
			var collection = new SerializableEDocCollection();
			collection.AddBaseBusinessObjects(elements);
			return collection;
		}

		public void RemoveForTest(SerializableEDoc eDoc)
		{
			Items.Remove(eDoc);
		}
#endif
	}
}
