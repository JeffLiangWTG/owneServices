using System;
using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			if (!SerializableEDocsTools.IsImage((string)row[StorageDocsBase.Schema.SC_DataType]))
			{
				return typeof(StorageFile);
			}
			else
			{
				return typeof(StorageDocs);
			}
		}

		public override Type GetTypeForNew()
		{
			return typeof(StorageDocsBase);
		}

		public override Type GetTypeForBinding()
		{
			return typeof(StorageDocsBase);
		}
	}
}
